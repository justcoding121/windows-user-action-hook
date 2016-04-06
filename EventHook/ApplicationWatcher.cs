using System;
using System.Collections.Generic;
using System.Linq;
using EventHook.Helpers;
using Nito.AsyncEx;
using System.Threading.Tasks;
using EventHook.Hooks.Shell;
using EventHook.Hooks.Window;


namespace EventHook
{
    public static class ApplicationWatcher
    {
        private static bool _isRunning;
        private static readonly object Accesslock = new object();

        private static AsyncCollection<object> appQueue;

        private static List<WindowData> _activeWindows;
        private static DateTime _prevTimeApp;

        public static event EventHandler<ApplicationEventArgs> OnApplicationWindowChange;

        public static void Start()
        {
            if (_isRunning) return;

            lock (Accesslock)
                {
                    _activeWindows = new List<WindowData> { };
                    _prevTimeApp = DateTime.Now;

                    appQueue = new AsyncCollection<object>();
                   

                    Task.Factory.StartNew(() => { }).ContinueWith(x =>
                      {
                    WindowHook.WindowCreated += WindowCreated;
                    WindowHook.WindowDestroyed += WindowDestroyed;
                    WindowHook.WindowActivated += WindowActivated;

                      }, SharedMessagePump.GetTaskScheduler());

                    _lastEventWasLaunched = false;
                    _lastHwndLaunched = IntPtr.Zero;

                    Task.Factory.StartNew(() => AppConsumer());
                _isRunning = true;
            }
                }

        public static void Stop()
        {
            if (!_isRunning) return;

            lock (Accesslock)
                {
                WindowHook.WindowCreated -= WindowCreated;
                WindowHook.WindowDestroyed -= WindowDestroyed;
                WindowHook.WindowActivated -= WindowActivated;

                    appQueue.Add(false);
                _isRunning = false;
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
            while (_isRunning)
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
            ApplicationStatus(wnd, ApplicationEvent.Launched);

            _lastEventWasLaunched = true;
            _lastHwndLaunched = wnd.HWnd;

        }

        private static void ApplicationStatus(WindowData wnd, ApplicationEvent appEvent)
        {
            var timeStamp = DateTime.Now;

            wnd.AppTitle = appEvent == ApplicationEvent.Closed ? wnd.AppTitle : WindowHelper.GetWindowText(wnd.HWnd);
            wnd.AppPath = appEvent == ApplicationEvent.Closed ? wnd.AppPath : WindowHelper.GetAppPath(wnd.HWnd);
            wnd.AppName = appEvent == ApplicationEvent.Closed ? wnd.AppName : WindowHelper.GetAppDescription(wnd.AppPath);

            var handler = OnApplicationWindowChange;
            if (handler != null)
            {
                handler(null, new ApplicationEventArgs(wnd, appEvent));
            }
        }

        private static void WindowDestroyed(WindowData wnd)
        {

            if (_activeWindows.Any(x => x.HWnd == wnd.HWnd))
            {
                ApplicationStatus(_activeWindows.First(x => x.HWnd == wnd.HWnd), ApplicationEvent.Closed);
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
                    ApplicationStatus(_activeWindows.First(x => x.HWnd == wnd.HWnd), ApplicationEvent.Activated);
            _lastEventWasLaunched = false;
        }


    }
}
