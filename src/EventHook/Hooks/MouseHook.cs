using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace EventHook.Hooks
{
    public static class MouseHook
    {
        private const int WH_MOUSE_LL = 14;

        private static readonly LowLevelMouseProc Proc = HookCallback;
        private static IntPtr _hookId = IntPtr.Zero;
        public static event EventHandler MouseAction = delegate { };


        public static void Start()
        {
            _hookId = SetHook(Proc);
        }

        public static void Stop()
        {
            UnhookWindowsHookEx(_hookId);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            var hook = SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle("user32"), 0);
            if (hook == IntPtr.Zero) throw new Win32Exception();
            return hook;
        }

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            string[] content;
            MSLLHOOKSTRUCT hookStruct;
            if (nCode < 0) return CallNextHookEx(_hookId, nCode, wParam, lParam);
            switch ((MouseMessages) wParam)
            {
                case MouseMessages.WM_LBUTTONDOWN:
                    content = new string[3];
                    hookStruct = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof (MSLLHOOKSTRUCT));
                    content[0] = "LEFT";
                    content[1] = hookStruct.pt.x.ToString();
                    content[2] = hookStruct.pt.y.ToString();
                    MouseAction(content, new EventArgs());
                    break;
                case MouseMessages.WM_RBUTTONDOWN:
                    content = new string[3];
                    hookStruct = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof (MSLLHOOKSTRUCT));
                    content[0] = "RIGHT";
                    content[1] = hookStruct.pt.x.ToString();
                    content[2] = hookStruct.pt.y.ToString();
                    MouseAction(content, new EventArgs());
                    break;

                case MouseMessages.WM_MBUTTONDOWN:
                    content = new string[3];
                    hookStruct = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof (MSLLHOOKSTRUCT));
                    content[0] = "MIDDLE";
                    content[1] = hookStruct.pt.x.ToString();
                    content[2] = hookStruct.pt.y.ToString();
                    MouseAction(content, new EventArgs());
                    break;
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_RBUTTONDOWN = 0x0204,
            WM_MBUTTONDOWN = 0x0207
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public readonly int x;
            public readonly int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public readonly uint mouseData;
            public readonly uint flags;
            public readonly uint time;
            public readonly IntPtr dwExtraInfo;
        }
    }
}