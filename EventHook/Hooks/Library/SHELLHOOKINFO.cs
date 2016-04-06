using System;

namespace EventHook.Hooks.Library
{
    internal struct SHELLHOOKINFO
    {
        internal IntPtr Hwnd;
        internal RECT Rc;
    }
}