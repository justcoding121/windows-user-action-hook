using System;
using EventHook.Hooks.Mouse;

namespace EventHook
{
    public class MouseEventArgs : EventArgs
    {
        private readonly MouseMessage _message;
        private readonly POINT _point;

        public MouseEventArgs(MouseMessage message, POINT point)
        {
            _message = message;
            _point = point;
        }

        internal MouseEventArgs(RawMouseEventArgs args)
        {
            _message = args.Message;
            _point = args.Point;
        }

        public MouseMessage Message { get { return _message; } }
        public POINT Point { get { return _point; } }
    }
}