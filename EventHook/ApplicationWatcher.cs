using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using EventHook.Hooks;
using EventHook.Helpers;
using Nito.AsyncEx;
using System.Threading.Tasks;
using EventHook.Hooks.Shell;
using EventHook.Hooks.Window;


namespace EventHook
{
    public enum ApplicationEvents
    {
        Launched,
        Closed,
        Activated

    }
    public class WindowData
    {
        public IntPtr HWnd;
        public int EventType;

        public string AppPath { get; set; }
        public string AppName { get; set; }
        public string AppTitle { get; set; }
    }

    public class ApplicationEventArgs : EventArgs
    {
        public WindowData ApplicationData { get; set; }
        public ApplicationEvents Event { get; set; }
    }

    public class ApplicationWatcher
    {
        /*Application history*/
        private static object _Accesslock = new object();
        private static bool _IsRunning;

        private static AsyncCollection<object> appQueue;

        private static List<WindowData> _activeWindows;
        private static DateTime _prevTimeApp;

        public static event EventHandler<ApplicationEventArgs> OnApplicationWindowChange;

        public static void Start()
        {
            if (!_IsRunning)
                lock (_Accesslock)
                {
                    _activeWindows = new List<WindowData> { };
                    _prevTimeApp = DateTime.Now;

                    appQueue = new AsyncCollection<object>();
                   
                    SharedMessagePump.Initialize();
                    Task.Factory.StartNew(() => { }).ContinueWith(x =>
                      {
                          WindowHook.WindowCreated += new GeneralShellHookEventHandler(WindowCreated);
                          WindowHook.WindowDestroyed += new GeneralShellHookEventHandler(WindowDestroyed);
                          WindowHook.WindowActivated += new GeneralShellHookEventHandler(WindowActivated);

                      }, SharedMessagePump.GetTaskScheduler());

                    _lastEventWasLaunched = false;
                    _lastHwndLaunched = IntPtr.Zero;

                    Task.Factory.StartNew(() => AppConsumer());
                    _IsRunning = true;
                }

        }
        public static void Stop()
        {
            if (_IsRunning)
                lock (_Accesslock)
                {
                    WindowHook.WindowCreated -= new GeneralShellHookEventHandler(WindowCreated);
                    WindowHook.WindowDestroyed -= new GeneralShellHookEventHandler(WindowDestroyed);
                    WindowHook.WindowActivated -= new GeneralShellHookEventHandler(WindowActivated);

                    appQueue.Add(false);
                    _IsRunning = false;
                }

        }
        private static void WindowCreated(ShellHook shellObject, IntPtr hWnd)
        {
            appQueue.Add(new WindowData() { HWnd = hWnd, EventType = 0 });
        }
        private static void WindowDestroyed(ShellHook shellObject, IntPtr hWnd)
        {
            appQueue.Add(new WindowData() { HWnd = hWnd, EventType = 2 });
        }
        private static void WindowActivated(ShellHook shellObject, IntPtr hWnd)
        {
            appQueue.Add(new WindowData() { HWnd = hWnd, EventType = 1 });
        }
        // This is the method to run when the timer is raised. 
        private static async Task AppConsumer()
        {
            while (_IsRunning)
            {
                //blocking here until a key is added to the queue
                var item = await appQueue.TakeAsync();
                if (item is bool) break;

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
        private static IntPtr _lastHwndLaunched;
        private static void WindowCreated(WindowData wnd)
        {

            _activeWindows.Add(wnd);
            ApplicationStatus(wnd, ApplicationEvents.Launched);

            _lastEventWasLaunched = true;
            _lastHwndLaunched = wnd.HWnd;

        }

        private static void ApplicationStatus(WindowData wnd, ApplicationEvents appEvent)
        {
            var timeStamp = DateTime.Now;

            wnd.AppTitle = appEvent == ApplicationEvents.Closed ? wnd.AppTitle : WindowHelper.GetWindowText(wnd.HWnd);
            wnd.AppPath = appEvent == ApplicationEvents.Closed ? wnd.AppPath : WindowHelper.GetAppPath(wnd.HWnd);
            wnd.AppName = appEvent == ApplicationEvents.Closed ? wnd.AppName : WindowHelper.GetAppDescription(wnd.AppPath);

            EventHandler<ApplicationEventArgs> handler = OnApplicationWindowChange;
            if (handler != null)
            {
                handler(null, new ApplicationEventArgs() { ApplicationData = wnd, Event = appEvent });
            }
        }

        private static void WindowDestroyed(WindowData wnd)
        {

            if (_activeWindows.Any(x => x.HWnd == wnd.HWnd))
            {
                ApplicationStatus(_activeWindows.First(x => x.HWnd == wnd.HWnd), ApplicationEvents.Closed);
                _activeWindows.RemoveAll(x => x.HWnd == wnd.HWnd);
            }
            _lastEventWasLaunched = false;
        }
        private static bool _lastEventWasLaunched;
        private static void WindowActivated(WindowData wnd)
        {

            if (_activeWindows.Any(x => x.HWnd == wnd.HWnd))
                if ((_lastEventWasLaunched) && _lastHwndLaunched == wnd.HWnd) { }
                else
                    ApplicationStatus(_activeWindows.First(x => x.HWnd == wnd.HWnd), ApplicationEvents.Activated);
            _lastEventWasLaunched = false;
        }


    }
}
