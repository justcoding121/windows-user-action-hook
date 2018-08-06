using System;
using System.Collections;
using System.Printing;
using System.Threading;
using System.Threading.Tasks;
using EventHook.Helpers;
using EventHook.Hooks;
using EventHook.Hooks.Library;

namespace EventHook
{
    /// <summary>
    ///     An object holding key information on a particular print event.
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
    ///     An argument passed along user call backs.
    /// </summary>
    public class PrintEventArgs : EventArgs
    {
        public PrintEventData EventData { get; set; }
    }

    /// <summary>
    ///     A class that wraps around printServer object.
    /// </summary>
    public class PrintWatcher
    {
        private readonly object accesslock = new object();

        private readonly SyncFactory factory;

        private ArrayList printers;
        private PrintServer printServer;

        internal PrintWatcher(SyncFactory factory)
        {
            this.factory = factory;
        }

        private bool isRunning { get; set; }
        public event EventHandler<PrintEventArgs> OnPrintEvent;

        /// <summary>
        ///     Start watching print events
        /// </summary>
        public void Start()
        {
            lock (accesslock)
            {
                if (!isRunning)
                {
                    Task.Factory.StartNew(() =>
                        {
                            isRunning = true;
                            printers = new ArrayList();
                            printServer = new PrintServer();
                            foreach (var pq in printServer.GetPrintQueues())
                            {
                                var pqm = new PrintQueueHook(pq.Name);
                                pqm.OnJobStatusChange += pqm_OnJobStatusChange;
                                pqm.Start();
                                printers.Add(pqm);
                            }
                        },
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        factory.GetTaskScheduler()).Wait();
                }
            }
        }

        /// <summary>
        ///     Stop watching print events
        /// </summary>
        public void Stop()
        {
            lock (accesslock)
            {
                if (isRunning)
                {
                    Task.Factory.StartNew(() =>
                        {
                            if (printers != null)
                            {
                                foreach (PrintQueueHook pqm in printers)
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

                                printers.Clear();
                            }

                            printers = null;
                            isRunning = false;
                        },
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        factory.GetTaskScheduler());
                }
            }
        }

        /// <summary>
        ///     Invoke user callback as soon as a relevent event is fired
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pqm_OnJobStatusChange(object sender, PrintJobChangeEventArgs e)
        {
            if ((e.JobStatus & JOBSTATUS.JOB_STATUS_SPOOLING) == JOBSTATUS.JOB_STATUS_SPOOLING
                && e.JobInfo != null)
            {
                var hWnd = WindowHelper.GetActiveWindowHandle();
                string appTitle = WindowHelper.GetWindowText(hWnd);
                string appName = WindowHelper.GetAppDescription(WindowHelper.GetAppPath(hWnd));

                var printEvent = new PrintEventData
                {
                    JobName = e.JobInfo.JobName,
                    JobSize = e.JobInfo.JobSize,
                    EventDateTime = DateTime.Now,
                    Pages = e.JobInfo.NumberOfPages,
                    PrinterName = ((PrintQueueHook)sender).SpoolerName
                };

                Task.Run(() => OnPrintEvent?.Invoke(null, new PrintEventArgs { EventData = printEvent }));
            }
        }
    }
}
