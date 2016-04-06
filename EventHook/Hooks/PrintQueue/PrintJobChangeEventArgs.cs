using System;
using System.Printing;
using EventHook.Hooks.Library;

namespace EventHook.Hooks.PrintQueue
{
    internal class PrintJobChangeEventArgs : EventArgs
    {
        private readonly int _jobId;
        private readonly string _jobName;
        private readonly JOBSTATUS _jobStatus;
        private readonly PrintSystemJobInfo _jobInfo;

        internal PrintJobChangeEventArgs(int intJobId, string strJobName, JOBSTATUS jStatus, PrintSystemJobInfo objJobInfo)
        {
            _jobId = intJobId;
            _jobName = strJobName;
            _jobStatus = jStatus;
            _jobInfo = objJobInfo;
        }

        internal int JobId
        {
            get { return _jobId; }
        }

        internal string JobName
        {
            get { return _jobName; }
        }

        internal JOBSTATUS JobStatus
        {
            get { return _jobStatus; }
        }

        internal PrintSystemJobInfo JobInfo
        {
            get { return _jobInfo; }
        }
    }
}