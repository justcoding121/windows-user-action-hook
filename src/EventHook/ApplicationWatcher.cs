using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using EventHook.Hooks;
using EventHook.Hooks.Helpers;


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
    public class ApplicationWatcher
    {
        /*Application history*/
        static BlockingCollection<object> _appQueue;
        static public bool AppRun;
        static private List<WindowData> _activeWindows;
        static private DateTime _prevTimeApp;


        public static void Start()
        {
            try
            {
                _activeWindows = new List<WindowData> { };
                _prevTimeApp = DateTime.Now;

                _appQueue = new BlockingCollection<object>();

                WindowHook.WindowCreated += new GeneralShellHookEventHandler(WindowCreated);
                WindowHook.WindowDestroyed += new GeneralShellHookEventHandler(WindowDestroyed);
                WindowHook.WindowActivated += new GeneralShellHookEventHandler(WindowActivated);

                _lastEventWasLaunched = false;
                _lastHwndLaunched = IntPtr.Zero;

                AppRun = true;

                var appThread = new Thread(AppConsumer)
                {
                    Name = "ApplicationConsumer",
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                };

                appThread.Start();

            }
            catch
            {
                Stop();
            }


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
        static private void AppConsumer()
        {
            while (AppRun)
            {
                //blocking here until a key is added to the queue
                var item = _appQueue.Take();
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

        private static void ApplicationStatus(WindowData wnd, ApplicationEvents Event)
        {

            var timeStamp = DateTime.Now;

            wnd.AppTitle = Event == ApplicationEvents.Closed ? wnd.AppTitle : WindowHelper.GetWindowText(wnd.HWnd);
            wnd.AppPath = Event == ApplicationEvents.Closed ? wnd.AppPath : WindowHelper.GetAppPath(wnd.HWnd);
            wnd.AppName = Event == ApplicationEvents.Closed ? wnd.AppName : WindowHelper.GetAppDescription(wnd.AppPath);

            Console.WriteLine((wnd.AppTitle));
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
