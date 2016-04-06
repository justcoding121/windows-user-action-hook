using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using EventHook.Hooks.Library;

namespace EventHook.Hooks.Mouse
{
    /// <summary>
    /// <see cref="http://stackoverflow.com/questions/11607133/global-mouse-event-handler"/>
    /// </summary>
    internal class MouseHook
    {
        private const int WH_MOUSE_LL = 14;

        private readonly User32.LowLevelMouseProc _proc;
        private static IntPtr _hookId = IntPtr.Zero;
        internal event EventHandler<RawMouseEventArgs> MouseAction = delegate { };

        internal MouseHook()
        {
            _proc = HookCallback;
        }
        internal void Start()
        {
            _hookId = SetHook(_proc);
        }

        internal void Stop()
        {
            User32.UnhookWindowsHookEx(_hookId);
        }

        private static IntPtr SetHook(User32.LowLevelMouseProc proc)
        {
            var hook = User32.SetWindowsHookEx(WH_MOUSE_LL, proc, Kernel32.GetModuleHandle("user32"), 0);
            if (hook == IntPtr.Zero) throw new Win32Exception();
            return hook;
        }

        private IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);

            var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

            MouseAction(null, new RawMouseEventArgs((MouseMessage)wParam, hookStruct.pt));

            return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
}
