using System;

namespace EventHook
{
    public class KeyInputEventArgs : EventArgs
    {
        private readonly KeyData _data;

        public KeyInputEventArgs(KeyData data)
        {
            _data = data;
        }

        public KeyData KeyData { get { return _data; } }
    }
}