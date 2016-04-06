using System;

namespace EventHook.Hooks.Mouse
{
    internal class RawMouseEventArgs : EventArgs
    {
        private readonly MouseMessage _message;
        private readonly POINT _point;

        public RawMouseEventArgs(MouseMessage message, POINT point)
        {
            _message = message;
            _point = point;
        }

        internal MouseMessage Message
        {
            get { return _message; }
        }

        internal POINT Point
        {
            get { return _point; }
        }
    }
}