
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace TSpy.Controller.Utility.Hooks
{
    public static class MouseHook
    {
        public static event EventHandler MouseAction = delegate { };

  
        public static void Start()
        {

            _hookID = SetHook(_proc);


        }
        public static void stop()
        {

            UnhookWindowsHookEx(_hookID);
        }


        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            IntPtr hook = SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle("user32"), 0);
            if (hook == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
            return hook;
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
          int nCode, IntPtr wParam, IntPtr lParam)
        {
            String[] content;
            MSLLHOOKSTRUCT hookStruct;
            if (nCode >= 0)
                switch ((MouseMessages)wParam)
                {

                    case MouseMessages.WM_LBUTTONDOWN:
                        content = new String[3];
                        hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                        content[0] = "LEFT";
                        content[1] = hookStruct.pt.x.ToString();
                        content[2] = hookStruct.pt.y.ToString();
                        MouseAction(content, new EventArgs());
                        break;
                    case MouseMessages.WM_RBUTTONDOWN:
                        content = new String[3];
                        hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                        content[0] = "RIGHT";
                        content[1] = hookStruct.pt.x.ToString();
                        content[2] = hookStruct.pt.y.ToString();
                        MouseAction(content, new EventArgs());
                        break;

                    case MouseMessages.WM_MBUTTONDOWN:
                        content = new String[3];
                        hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                        content[0] = "MIDDLE";
                        content[1] = hookStruct.pt.x.ToString();
                        content[2] = hookStruct.pt.y.ToString();
                        MouseAction(content, new EventArgs());
                        break;

                    default:
                        break;
                }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,

            WM_RBUTTONDOWN = 0x0204,
            WM_MBUTTONDOWN = 0x0207,

        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
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




    }
}