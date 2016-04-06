using System;

namespace EventHook
{
    public class ClipboardEventArgs : EventArgs
    {
        private readonly object _data;
        private readonly ClipboardContentType _format;

        public ClipboardEventArgs(object data, ClipboardContentType format)
        {
            _data = data;
            _format = format;
        }

        public object Data { get {return _data; } }
        public ClipboardContentType DataFormat { get { return _format; } }
    }
}