using System.Runtime.InteropServices;

namespace EventHook.Hooks.Mouse
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public readonly int x;
        public readonly int y;
    }
}