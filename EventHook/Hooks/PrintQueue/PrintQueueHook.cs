using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;
using System.Threading;
using EventHook.Hooks.Library;
using Microsoft.Win32.SafeHandles;

namespace EventHook.Hooks.PrintQueue
{
    //http://www.codeproject.com/Articles/51085/Monitor-jobs-in-a-printer-queue-NET

    internal delegate void PrintJobStatusChanged(object sender, PrintJobChangeEventArgs e);

    internal class PrintQueueHook
    {
        internal readonly string SpoolerName;
        internal event PrintJobStatusChanged OnJobStatusChange;

        private const int PRINTER_NOTIFY_OPTIONS_REFRESH = 1;

        private IntPtr _printerHandle = IntPtr.Zero;
        private readonly ManualResetEvent _mrEvent = new ManualResetEvent(false);
        private RegisteredWaitHandle _waitHandle;
        private IntPtr _changeHandle = IntPtr.Zero;
        private readonly PRINTER_NOTIFY_OPTIONS _notifyOptions = new PRINTER_NOTIFY_OPTIONS();
        private readonly Dictionary<int, string> _objJobDict = new Dictionary<int, string>();
        private System.Printing.PrintQueue _spooler;

        internal PrintQueueHook(string strSpoolName)
        {
            // Let us open the printer and get the printer handle.
            SpoolerName = strSpoolName;
        }

        ~PrintQueueHook()
        {
            Stop();
        }

        internal void Start()
        {
            WinSpool.OpenPrinter(SpoolerName, out _printerHandle, 0);
            if (_printerHandle != IntPtr.Zero)
            {
                //We got a valid Printer handle.  Let us register for change notification....
                _changeHandle = WinSpool.FindFirstPrinterChangeNotification(_printerHandle,
                    (int) PRINTER_CHANGES.PRINTER_CHANGE_JOB, 0, _notifyOptions);
                // We have successfully registered for change notification.  Let us capture the handle...
                _mrEvent.SafeWaitHandle = new SafeWaitHandle(_changeHandle, true);

                //Now, let us wait for change notification from the printer queue....
                _waitHandle = ThreadPool.RegisterWaitForSingleObject(_mrEvent, PrinterNotifyWaitCallback, _mrEvent, -1,
                    true);
            }

            _spooler = new System.Printing.PrintQueue(new PrintServer(), SpoolerName);
            foreach (var psi in _spooler.GetPrintJobInfoCollection())
            {
                _objJobDict[psi.JobIdentifier] = psi.Name;
            }
        }

        internal void Stop()
        {
            if (_printerHandle != IntPtr.Zero)
            {
                WinSpool.ClosePrinter((int) _printerHandle);
                _printerHandle = IntPtr.Zero;
            }
        }

        private void PrinterNotifyWaitCallback(object state, bool timedOut)
        {
            if (_printerHandle == IntPtr.Zero) return;

            _notifyOptions.Count = 1;
            var pdwChange = 0;
            IntPtr pNotifyInfo;
            var bResult = WinSpool.FindNextPrinterChangeNotification(_changeHandle, out pdwChange, _notifyOptions,
                out pNotifyInfo);
            //If the Printer Change Notification Call did not give data, exit code
            if ((bResult == false) || (((int) pNotifyInfo) == 0)) return;

            //If the Change Notification was not relgated to job, exit code
            var bJobRelatedChange = ((pdwChange & PRINTER_CHANGES.PRINTER_CHANGE_ADD_JOB) ==
                                     PRINTER_CHANGES.PRINTER_CHANGE_ADD_JOB) ||
                                    ((pdwChange & PRINTER_CHANGES.PRINTER_CHANGE_SET_JOB) ==
                                     PRINTER_CHANGES.PRINTER_CHANGE_SET_JOB) ||
                                    ((pdwChange & PRINTER_CHANGES.PRINTER_CHANGE_DELETE_JOB) ==
                                     PRINTER_CHANGES.PRINTER_CHANGE_DELETE_JOB) ||
                                    ((pdwChange & PRINTER_CHANGES.PRINTER_CHANGE_WRITE_JOB) ==
                                     PRINTER_CHANGES.PRINTER_CHANGE_WRITE_JOB);
            if (!bJobRelatedChange) return;

            //Now, let us initialize and populate the Notify Info data
            var info = (PRINTER_NOTIFY_INFO) Marshal.PtrToStructure(pNotifyInfo, typeof (PRINTER_NOTIFY_INFO));
            var pData = (long) pNotifyInfo + (long) Marshal.OffsetOf(typeof (PRINTER_NOTIFY_INFO), "aData");
            var data = new PRINTER_NOTIFY_INFO_DATA[info.Count];
            for (uint i = 0; i < info.Count; i++)
            {
                data[i] =
                    (PRINTER_NOTIFY_INFO_DATA) Marshal.PtrToStructure((IntPtr) pData, typeof (PRINTER_NOTIFY_INFO_DATA));
                pData += Marshal.SizeOf(typeof (PRINTER_NOTIFY_INFO_DATA));
            }

            for (var i = 0; i < data.Count(); i++)
            {
                if ((data[i].Field == (ushort) PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_STATUS) &&
                    (data[i].Type == (ushort) PRINTERNOTIFICATIONTYPES.JOB_NOTIFY_TYPE)
                    )
                {
                    var jStatus = (JOBSTATUS) Enum.Parse(typeof (JOBSTATUS), data[i].NotifyData.Data.cbBuf.ToString());
                    var intJobId = (int) data[i].Id;
                    string strJobName;
                    PrintSystemJobInfo pji = null;
                    try
                    {
                        _spooler = new System.Printing.PrintQueue(new PrintServer(), SpoolerName);
                        pji = _spooler.GetJob(intJobId);
                        if (!_objJobDict.ContainsKey(intJobId))
                            _objJobDict[intJobId] = pji.Name;
                        strJobName = pji.Name;
                    }
                    catch
                    {
                        //Trace.WriteLine(ex.Message);
                        pji = null;
                        _objJobDict.TryGetValue(intJobId, out strJobName);
                        if (strJobName == null) strJobName = string.Empty;
                    }

                    if (OnJobStatusChange != null)
                    {
                        //Let us raise the event
                        OnJobStatusChange(this, new PrintJobChangeEventArgs(intJobId, strJobName, jStatus, pji));
                    }
                }
            }

            _mrEvent.Reset();
            _waitHandle = ThreadPool.RegisterWaitForSingleObject(_mrEvent, PrinterNotifyWaitCallback, _mrEvent, -1, true);
        }
    }
}
