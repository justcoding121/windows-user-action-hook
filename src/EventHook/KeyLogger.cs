using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EventHook.Hooks;
using EventHook.Hooks.Helpers;

namespace EventHook
{

    public class KeyStrokeEvent
    {
        public DateTime EventDateTime { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationWindowTitle { get; set; }
        public string Location { get; set; }
        public string Keys { get; set; }
    }


    public class KeyLogger
    {

        /*Keyboard*/
        public static bool KeyboardRun { get; set; }
        private static List<IntPtr> _activeWindowsKeys;
        private static DateTime _prevTime;
        private static string _prevWindowTitle;
        private static string _prevWindowPath;
        private static bool _firstKey;
        private static StringBuilder _keys;
        private static KeyboardHook _kh;
        public static object Accesslock { get; set; }

        public static void Start()
        {
            try
            {

                _kQueue = new BlockingCollection<object>();
                _activeWindowsKeys = new List<IntPtr> { };

                _firstKey = true;
                _prevTime = DateTime.Now;

                _keys = new StringBuilder();
                KeyboardRun = true;

                WindowHook.WindowActivated += new GeneralShellHookEventHandler(KeyboardWindowActivated);
                WindowHook.WindowCreated += new GeneralShellHookEventHandler(KeyboardWindowCreated);
                WindowHook.WindowDestroyed += new GeneralShellHookEventHandler(KeyboardWindowDestroyed);

                _kh = new KeyboardHook();
                _kh.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
                _kh.Start();


                var t = new Thread(KeyConsumer)
                {
                    Name = "KeyConsumer",
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest
                };
                t.Start();

            }
            catch
            {
                Stop();

            }

        }
        public static void Stop()
        {


            if (_kh != null)
            {
                _kh.KeyDown -= new RawKeyEventHandler(KListener_KeyDown);
                _kh.Stop();
            }


            _kh = null;

            KeyboardRun = false;
            _kQueue.Add(false);


        }
        private class KeyData
        {
            public string UnicodeCharacter;
            public string Keyname;
            public int EventType;
        }


        private static BlockingCollection<object> _kQueue;

        static void KeyboardWindowActivated(ShellHook shellObject, IntPtr hWnd)
        {
            _kQueue.Add(new WindowData() { HWnd = hWnd, EventType = 1 });
        }
        static void KeyboardWindowCreated(ShellHook shellObject, IntPtr hWnd)
        {
            _kQueue.Add(new WindowData() { HWnd = hWnd, EventType = 0 });
        }
        static private void KeyboardWindowDestroyed(ShellHook shellObject, IntPtr hWnd)
        {
            _kQueue.Add(new WindowData() { HWnd = hWnd, EventType = 2 });
        }
        static private void KListener_KeyDown(object sender, RawKeyEventArgs e)
        {
            _kQueue.Add(new KeyData() { UnicodeCharacter = e.Character, Keyname = e.Key.ToString(), EventType = e.EventType });
        }

        // This is the method to run when the timer is raised. 
        static private void KeyConsumer()
        {
            while (KeyboardRun)
            {

                //blocking here until a key is added to the queue
                var item = _kQueue.Take();
                if (item is bool) break;

                if (item.GetType() == typeof(WindowData))
                {
                    var wnd = (WindowData)item;
                    switch (wnd.EventType)
                    {
                        case 0:
                            KeyboardWindowCreated(wnd);
                            break;
                        case 1:
                            KeyboardWindowActivated(wnd);
                            break;
                        case 2:
                            KeyboardWindowDestroyed(wnd);
                            break;
                    }
                }
                else
                {
                    KListener_KeyDown((KeyData)item);
                }



            }
        }
        static void KeyboardWindowActivated(WindowData wd)
        {
            if (_activeWindowsKeys.Contains(wd.HWnd))
            {
                DateTime curtimeStamp = DateTime.Now;

                if (!_firstKey && _keys.Length > 0 && _prevWindowTitle != null)
                {

                    var record = new KeyStrokeEvent()
                    {
                        ApplicationName = WindowHelper.GetAppDescription(_prevWindowPath),
                        ApplicationWindowTitle = _prevWindowTitle,
                        EventDateTime = DateTime.Now,
                        Keys = _keys.ToString(),
                        Location = _prevWindowPath,
                    };

                }

                _keys.Clear();

                _prevTime = curtimeStamp;
                _prevWindowTitle = WindowHelper.GetWindowText(wd.HWnd);
                _prevWindowPath = WindowHelper.GetAppPath(wd.HWnd);

            }
            _firstKey = false;
        }
        static void KeyboardWindowCreated(WindowData wd)
        {
            _activeWindowsKeys.Add(wd.HWnd);

        }
        private static void KeyboardWindowDestroyed(WindowData wd)
        {

            if (_activeWindowsKeys.Contains(wd.HWnd))
            {
                _activeWindowsKeys.Remove(wd.HWnd);
            }

        }

        private static void KListener_KeyDown(KeyData kd)
        {
            _keys.Append(kd.UnicodeCharacter);

        }


    }
}
