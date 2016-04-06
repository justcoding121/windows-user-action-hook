using System;

namespace EventHook
{
    public class PrintEventArgs : EventArgs
    {
        private readonly PrintEventData _data;

        public PrintEventArgs(PrintEventData data)
        {
            _data = data;
        }

        public PrintEventData EventData { get { return _data; } }
    }
}