using System;
using System.Collections;
using System.Linq;
using System.Printing;
using EventHook.Helpers;
using EventHook.Hooks.Library;
using EventHook.Hooks.PrintQueue;

namespace EventHook
{
    public static class PrintWatcher
    {
        private static bool _isRunning;
        private static readonly object Accesslock = new object();

        private static ArrayList _printers;
        private static PrintServer _ps;

        public static event EventHandler<PrintEventArgs> OnPrintEvent;

        public static void Start()
        {
            if (_isRunning) return;

            lock (Accesslock)
            {
                _printers = new ArrayList();
                _ps = new PrintServer();
                foreach (var hook in _ps.GetPrintQueues().Select(queue => new PrintQueueHook(queue.Name)))
                {
                    hook.OnJobStatusChange += pqm_OnJobStatusChange;
                    hook.Start();
                    _printers.Add(hook);
                }
                _isRunning = true;
            }
        }

        public static void Stop()
        {
            if (!_isRunning) return;

            lock (Accesslock)
            {
                if (_printers != null)
                {
                    foreach (PrintQueueHook pqm in _printers)
                    {
                        pqm.OnJobStatusChange -= pqm_OnJobStatusChange;
                        pqm.Stop();
                    }
                    _printers.Clear();
                }
                _printers = null;
                _isRunning = false;
            }
        }

        private static void pqm_OnJobStatusChange(object sender, PrintJobChangeEventArgs e)
        {
            if ((e.JobStatus & JOBSTATUS.JOB_STATUS_SPOOLING) != JOBSTATUS.JOB_STATUS_SPOOLING || e.JobInfo == null)
            {
                return;
            }

            var hWnd = WindowHelper.GetActiveWindowHandle();
            var appTitle = WindowHelper.GetWindowText(hWnd);
            var appName = WindowHelper.GetAppDescription(WindowHelper.GetAppPath(hWnd));

            var data = new PrintEventData
            {
                ApplicationTitle = appTitle,
                ApplicationName = appName,
                JobName = e.JobInfo.JobName,
                JobSize = e.JobInfo.JobSize,
                EventDateTime = DateTime.Now,
                Pages = e.JobInfo.NumberOfPages,
                PrinterName = ((PrintQueueHook)sender).SpoolerName
            };

            var handler = OnPrintEvent;
            if (handler != null)
            {
                handler(null, new PrintEventArgs(data));
            }
        }
    }
}
