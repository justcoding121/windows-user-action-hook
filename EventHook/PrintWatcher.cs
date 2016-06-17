using System;
using System.Collections;
using System.Printing;
using EventHook.Hooks;
using EventHook.Helpers;
using EventHook.Hooks.Library;

namespace EventHook
{
    /// <summary>
    /// A object holding key information on a particular print event
    /// </summary>
    public class PrintEventData
    {
        public DateTime EventDateTime { get; set; }
        public string PrinterName { get; set; }
        public string JobName { get; set; }
        public int? Pages { get; set; }
        public int? JobSize { get; set; }
    }

    /// <summary>
    /// An argument passed along user call backs
    /// </summary>
    public class PrintEventArgs : EventArgs
    {
        public PrintEventData EventData { get; set; }
    }

    /// <summary>
    /// A class that wraps around printServer object
    /// </summary>
    public class PrintWatcher
    {
        /*Print history*/
        private static bool _IsRunning;
        private static object _Accesslock = new object();

        private static ArrayList _printers = null;
        private static PrintServer ps = null;

        public static event EventHandler<PrintEventArgs> OnPrintEvent;

        /// <summary>
        /// Start watching print events
        /// </summary>
        public static void Start()
        {
            if (!_IsRunning)
                lock (_Accesslock)
                {
                    _printers = new ArrayList();
                    ps = new PrintServer();
                    foreach (var pq in ps.GetPrintQueues())
                    {

                        var pqm = new PrintQueueHook(pq.Name);
                        pqm.OnJobStatusChange += pqm_OnJobStatusChange;
                        pqm.Start();
                        _printers.Add(pqm);
                    }
                    _IsRunning = true;
                }

        }

        /// <summary>
        /// Stop watching print events
        /// </summary>
        public static void Stop()
        {
            if (_IsRunning)
                lock (_Accesslock)
                {
                    if (_printers != null)
                    {
                        foreach (PrintQueueHook pqm in _printers)
                        {
                            pqm.OnJobStatusChange -= pqm_OnJobStatusChange;

                            try
                            {
                                pqm.Stop();
                            }
                            catch
                            {
                                //ignored intentionally
                                //Not sure why but it throws error
                                //not a bug deal since we a stopping it anyway
                            }
                        }
                        _printers.Clear();
                    }
                    _printers = null;
                    _IsRunning = false;
                }
        }

        /// <summary>
        /// Invoke user callback as soon as a relevent event is fired
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void pqm_OnJobStatusChange(object sender, PrintJobChangeEventArgs e)
        {

            if ((e.JobStatus & JOBSTATUS.JOB_STATUS_SPOOLING) == JOBSTATUS.JOB_STATUS_SPOOLING && e.JobInfo != null)
            {

                var hWnd = WindowHelper.GetActiveWindowHandle();
                var appTitle = WindowHelper.GetWindowText(hWnd);
                var appName = WindowHelper.GetAppDescription(WindowHelper.GetAppPath(hWnd));

                var printEvent = new PrintEventData()
                {

                    JobName = e.JobInfo.JobName,
                    JobSize = e.JobInfo.JobSize,
                    EventDateTime = DateTime.Now,
                    Pages = e.JobInfo.NumberOfPages,
                    PrinterName = ((PrintQueueHook)sender).SpoolerName

                };

                OnPrintEvent?.Invoke(null, new PrintEventArgs() { EventData = printEvent });
            }
        }
    }
}
