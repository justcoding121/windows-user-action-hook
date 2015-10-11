using System;
using System.IO;
using EventHook.Hooks;
using EventHook.Hooks.Helpers;

namespace EventHook
{
    public class WebEvent
    {
        public string Username { get; set; }
        public DateTime EventDateTime { get; set; }
        public string Url { get; set; }
        public string Hostname { get; set; }
    }

    public class WebWatcher
    {

        public static bool WebRun = false;

        private static AutomationHook Bh { get; set; }
        private static IntPtr BHhWnd { get; set; }
        /*Webhistory*/
        public static void Start()
        {

            if (_bhTimer == null)
                _bhTimer = new System.Timers.Timer(2000);

            Bh = new AutomationHook();

            LastUrl = "";

            Url = null;
            _bhRun = false;
            WindowHook.WindowActivated += new GeneralShellHookEventHandler(BH_WindowActivated);

        }

        private static string Url { get; set; }
        private static string _bHappname;
        private static bool _bhRun;
        private static System.Timers.Timer _bhTimer;
        static void BH_WindowActivated(ShellHook shellObject, IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                string path = WindowHelper.GetAppPath(hWnd);
                if (path != null)
                    _bHappname = Path.GetFileName(path);

                Url = Bh.GetUrlFromBrowsersWithIdentifier(_bHappname.ToLower(), hWnd);


                if (Url != null)
                {
                    insert_URL();

                    BHhWnd = hWnd;
                    if (!_bhRun)
                    {
                        _bhTimer.Elapsed += GetUrl;
                        _bhTimer.Start();
                        _bhRun = true;
                    }


                }
                else
                {
                    if (_bhRun)
                    {
                        _bhTimer.Elapsed -= GetUrl;
                        _bhTimer.Stop();
                    }
                    _bhRun = false;
                }

            }
        }
        private static void GetUrl(object sender, System.Timers.ElapsedEventArgs e)
        {
            Url = Bh.GetUrlFromBrowsersWithIdentifier(_bHappname, BHhWnd);
            insert_URL();
        }
        private static String LastUrl { get; set; }
        private static void insert_URL()
        {

            if (Url != null && !string.IsNullOrEmpty(Url.Trim()))
            {
                Url = Url.Trim();
                if (LastUrl != Url)
                {
                    if (Url.ToLower().StartsWith("http://") == false && Url.ToLower().StartsWith("https://") == false)
                        Url = "http://" + Url;

                    var hostName = new Uri(Url).Host;
                    var s = new WebEvent()
                                {

                                    EventDateTime = DateTime.Now,
                                    Hostname = hostName,
                                    Url = Url,
                                    Username = Environment.UserName
                                };

                }

            }

            LastUrl = Url;

        }
        public static void Stop()
        {
            WindowHook.WindowActivated -= BH_WindowActivated;
        }


    }

}
