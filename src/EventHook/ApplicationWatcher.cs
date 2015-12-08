using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using EventHook.Hooks;
using EventHook.Hooks.Helpers;
using Nito.AsyncEx;
using System.Threading.Tasks;


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
        static AsyncCollection<object> _appQueue;
        static public bool AppRun;
        static private List<WindowData> _activeWindows;
        static private DateTime _prevTimeApp;

        public static event EventHandler<ApplicationEventArgs> OnApplicationWindowChange;

        public static void Start()
        {

            _activeWindows = new List<WindowData> { };
            _prevTimeApp = DateTime.Now;

            _appQueue = new AsyncCollection<object>();

            var handler = SharedMessagePump.GetHandle();
         
            Task.Factory.StartNew(() => { }).ContinueWith(x =>
              {
                  WindowHook.WindowCreated += new GeneralShellHookEventHandler(WindowCreated);
                  WindowHook.WindowDestroyed += new GeneralShellHookEventHandler(WindowDestroyed);
                  WindowHook.WindowActivated += new GeneralShellHookEventHandler(WindowActivated);

              }, SharedMessagePump.GetTaskScheduler());

            _lastEventWasLaunched = false;
            _lastHwndLaunched = IntPtr.Zero;

            Task.Factory.StartNew(() => AppConsumer());
            AppRun = true;

        }
        public static void Stop()
        {
            WindowHook.WindowCreated -= new GeneralShellHookEventHandler(WindowCreated);
            WindowHook.WindowDestroyed -= new GeneralShellHookEventHandler(WindowDestroyed);
            WindowHook.WindowActivated -= new GeneralShellHookEventHandler(WindowActivated);

            _appQueue.Add(false);
            AppRun = false;

        }
        static void WindowCreated(ShellHook shellObject, IntPtr hWnd)
        {
            _appQueue.Add(new WindowData() { HWnd = hWnd, EventType = 0 });
        }
        static void WindowDestroyed(ShellHook shellObject, IntPtr hWnd)
        {
            _appQueue.Add(new WindowData() { HWnd = hWnd, EventType = 2 });
        }
        static void WindowActivated(ShellHook shellObject, IntPtr hWnd)
        {
            _appQueue.Add(new WindowData() { HWnd = hWnd, EventType = 1 });
        }
        // This is the method to run when the timer is raised. 
        static private async Task AppConsumer()
        {
            while (AppRun)
            {
                //blocking here until a key is added to the queue
                var item = await _appQueue.TakeAsync();
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
        static IntPtr _lastHwndLaunched;
        static void WindowCreated(WindowData wnd)
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

        static void WindowDestroyed(WindowData wnd)
        {

            if (_activeWindows.Any(x => x.HWnd == wnd.HWnd))
            {
                ApplicationStatus(_activeWindows.First(x => x.HWnd == wnd.HWnd), ApplicationEvents.Closed);
                _activeWindows.RemoveAll(x => x.HWnd == wnd.HWnd);
            }
            _lastEventWasLaunched = false;
        }
        static bool _lastEventWasLaunched;
        static void WindowActivated(WindowData wnd)
        {

            if (_activeWindows.Any(x => x.HWnd == wnd.HWnd))
                if ((_lastEventWasLaunched) && _lastHwndLaunched == wnd.HWnd) { }
                else
                    ApplicationStatus(_activeWindows.First(x => x.HWnd == wnd.HWnd), ApplicationEvents.Activated);
            _lastEventWasLaunched = false;
        }


    }
}
