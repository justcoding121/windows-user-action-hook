using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventHook.Helpers;
using EventHook.Hooks;

namespace EventHook
{
    /// <summary>
    ///     An enum for the type of application event.
    /// </summary>
    public enum ApplicationEvents
    {
        Launched,
        Closed,
        Activated
    }

    /// <summary>
    ///     An object that holds information on application event.
    /// </summary>
    public class WindowData
    {
        public int EventType;
        public IntPtr HWnd;

        public string AppPath { get; set; }
        public string AppName { get; set; }
        public string AppTitle { get; set; }
    }

    /// <summary>
    ///     An event argument object send to user.
    /// </summary>
    public class ApplicationEventArgs : EventArgs
    {
        public WindowData ApplicationData { get; set; }
        public ApplicationEvents Event { get; set; }
    }

    /// <summary>
    ///     A wrapper around shell hook to hook application window change events.
    ///     Uses a producer-consumer pattern to improve performance and to avoid operating system forcing unhook on delayed
    ///     user callbacks.
    /// </summary>
    public class ApplicationWatcher
    {
        private readonly object accesslock = new object();

        private readonly SyncFactory factory;

        private Dictionary<IntPtr, WindowData> activeWindows;
        private AsyncConcurrentQueue<object> appQueue;
        private bool isRunning;

        /// <summary>
        ///     Add window handle to active windows collection
        /// </summary>
        private bool lastEventWasLaunched;

        /// <summary>
        ///     A handle to keep track of last window launched
        /// </summary>
        private IntPtr lastHwndLaunched;

        private DateTime prevTimeApp;
        private CancellationTokenSource taskCancellationTokenSource;

        private WindowHook windowHook;

        internal ApplicationWatcher(SyncFactory factory)
        {
            this.factory = factory;
        }

        public event EventHandler<ApplicationEventArgs> OnApplicationWindowChange;

        /// <summary>
        ///     Start to watch
        /// </summary>
        public void Start()
        {
            lock (accesslock)
            {
                if (!isRunning)
                {
                    activeWindows = new Dictionary<IntPtr, WindowData>();
                    prevTimeApp = DateTime.Now;

                    taskCancellationTokenSource = new CancellationTokenSource();
                    appQueue = new AsyncConcurrentQueue<object>(taskCancellationTokenSource.Token);

                    //This needs to run on UI thread context
                    //So use task factory with the shared UI message pump thread
                    Task.Factory.StartNew(() =>
                        {
                            windowHook = new WindowHook(factory);
                            windowHook.WindowCreated += WindowCreated;
                            windowHook.WindowDestroyed += WindowDestroyed;
                            windowHook.WindowActivated += WindowActivated;
                        },
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        factory.GetTaskScheduler()).Wait();

                    lastEventWasLaunched = false;
                    lastHwndLaunched = IntPtr.Zero;

                    Task.Factory.StartNew(() => AppConsumer());
                    isRunning = true;
                }
            }
        }

        /// <summary>
        ///     Quit watching
        /// </summary>
        public void Stop()
        {
            lock (accesslock)
            {
                if (isRunning)
                {
                    //This needs to run on UI thread context
                    //So use task factory with the shared UI message pump thread
                    Task.Factory.StartNew(() =>
                        {
                            windowHook.WindowCreated -= WindowCreated;
                            windowHook.WindowDestroyed -= WindowDestroyed;
                            windowHook.WindowActivated -= WindowActivated;
                            windowHook.Destroy();
                        },
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        factory.GetTaskScheduler());

                    appQueue.Enqueue(false);
                    isRunning = false;
                    taskCancellationTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        ///     A windows was created on desktop
        /// </summary>
        /// <param name="shellObject"></param>
        /// <param name="hWnd"></param>
        private void WindowCreated(ShellHook shellObject, IntPtr hWnd)
        {
            appQueue.Enqueue(new WindowData { HWnd = hWnd, EventType = 0 });
        }

        /// <summary>
        ///     An existing desktop window was destroyed
        /// </summary>
        /// <param name="shellObject"></param>
        /// <param name="hWnd"></param>
        private void WindowDestroyed(ShellHook shellObject, IntPtr hWnd)
        {
            appQueue.Enqueue(new WindowData { HWnd = hWnd, EventType = 2 });
        }

        /// <summary>
        ///     A windows was brought to foreground
        /// </summary>
        /// <param name="shellObject"></param>
        /// <param name="hWnd"></param>
        private void WindowActivated(ShellHook shellObject, IntPtr hWnd)
        {
            appQueue.Enqueue(new WindowData { HWnd = hWnd, EventType = 1 });
        }

        /// <summary>
        ///     This is used to avoid blocking low level hooks
        ///     Otherwise if user takes long time to return the message
        ///     OS will unsubscribe the hook
        ///     Producer-consumer
        /// </summary>
        /// <returns></returns>
        private async Task AppConsumer()
        {
            while (isRunning)
            {
                //blocking here until a key is added to the queue
                var item = await appQueue.DequeueAsync();
                if (item is bool)
                {
                    break;
                }

                var wnd = (WindowData)item;
                switch (wnd.EventType)
                {
                    case 0:
                        WindowCreated(wnd);
                        break;
                    case 1:
                        WindowActivated(wnd);
                        break;
                    case 2:
                        WindowDestroyed(wnd);
                        break;
                }
            }
        }

        /// <summary>
        ///     A window got created
        /// </summary>
        /// <param name="wnd"></param>
        private void WindowCreated(WindowData wnd)
        {
            activeWindows.Add(wnd.HWnd, wnd);
            ApplicationStatus(wnd, ApplicationEvents.Launched);

            lastEventWasLaunched = true;
            lastHwndLaunched = wnd.HWnd;
        }

        private void WindowActivated(WindowData wnd)
        {
            if (activeWindows.ContainsKey(wnd.HWnd))
            {
                if (!lastEventWasLaunched && lastHwndLaunched != wnd.HWnd)
                {
                    ApplicationStatus(activeWindows[wnd.HWnd], ApplicationEvents.Activated);
                }
            }

            lastEventWasLaunched = false;
        }

        /// <summary>
        ///     Remove handle from active window collection
        /// </summary>
        /// <param name="wnd"></param>
        private void WindowDestroyed(WindowData wnd)
        {
            if (activeWindows.ContainsKey(wnd.HWnd))
            {
                ApplicationStatus(activeWindows[wnd.HWnd], ApplicationEvents.Closed);
                activeWindows.Remove(wnd.HWnd);
            }

            lastEventWasLaunched = false;
        }


        /// <summary>
        ///     invoke user call back
        /// </summary>
        /// <param name="wnd"></param>
        /// <param name="appEvent"></param>
        private void ApplicationStatus(WindowData wnd, ApplicationEvents appEvent)
        {
            var timeStamp = DateTime.Now;

            wnd.AppTitle = appEvent == ApplicationEvents.Closed ? wnd.AppTitle : WindowHelper.GetWindowText(wnd.HWnd);
            wnd.AppPath = appEvent == ApplicationEvents.Closed ? wnd.AppPath : WindowHelper.GetAppPath(wnd.HWnd);
            wnd.AppName = appEvent == ApplicationEvents.Closed
                ? wnd.AppName
                : WindowHelper.GetAppDescription(wnd.AppPath);

            OnApplicationWindowChange?.Invoke(null,
                new ApplicationEventArgs { ApplicationData = wnd, Event = appEvent });
        }
    }
}
