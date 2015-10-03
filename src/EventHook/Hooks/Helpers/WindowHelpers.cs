using System;
using System.Text;
using System.Diagnostics;
using EventHook.Client.Utility.Hooks.Library;

namespace EventHook.Helpers
{
    public  class WindowHelpers
    {
     

        public static IntPtr GetActiveWindowHandle()
        {
            try
            {
                return (IntPtr)User32.GetForegroundWindow();
            }
            catch { }
            return IntPtr.Zero;
        }

        public static string GetAppPath(IntPtr hWnd)
        {
            if (hWnd == null || hWnd == IntPtr.Zero) return null;
            try
            {
                uint pid = 0;
                User32.GetWindowThreadProcessId(hWnd, out pid);
                Process proc = Process.GetProcessById((int)pid);
                return proc.MainModule.FileName.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            try
            {
                int length = User32.GetWindowTextLength(hWnd);
                StringBuilder sb = new StringBuilder(length + 1);
                User32.GetWindowText(hWnd, sb, sb.Capacity);
                return sb.ToString();
            }
            catch (Exception) { return null; };
        }

        public static string GetAppDescription(string _AppPath)
        {
            if (_AppPath != null)
                try { return System.Diagnostics.FileVersionInfo.GetVersionInfo(_AppPath).FileDescription; }
                catch { return null; }
            return null;
        }
    }
}
