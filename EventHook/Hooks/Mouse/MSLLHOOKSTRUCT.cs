using System;
using System.Runtime.InteropServices;

namespace EventHook.Hooks.Mouse
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MSLLHOOKSTRUCT
    {
        internal POINT pt;
        internal readonly uint mouseData;
        internal readonly uint flags;
        internal readonly uint time;
        internal readonly IntPtr dwExtraInfo;
    }
}