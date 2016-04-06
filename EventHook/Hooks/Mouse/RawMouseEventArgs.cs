using System;
using EventHook.Hooks.Library;

namespace EventHook.Hooks.Mouse
{
    internal class RawMouseEventArgs : EventArgs
    {
        internal Msgs Message { get; set; }
        internal POINT Point { get; set; }
    }
}