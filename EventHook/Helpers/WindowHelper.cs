using System;
using System.Diagnostics;
using System.Text;
using EventHook.Hooks.Library;

namespace EventHook.Helpers
{
    /// <summary>
    /// A helper class to get window names/handles etc
    /// </summary>
    internal class WindowHelper
    {
        /// <summary>
        /// Get the handle of current acitive window on screen if any
        /// </summary>
        /// <returns></returns>
        internal static IntPtr GetActiveWindowHandle()
        {
            try
            {
                return (IntPtr) User32.GetForegroundWindow();
            }
            catch (Exception)
            {
                // ignored
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// The the application exe path of this window
        /// </summary>
        /// <param name="hWnd">window handle</param>
        /// <returns></returns>
        internal static string GetAppPath(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return null;
            try
            {
                uint pid;
                User32.GetWindowThreadProcessId(hWnd, out pid);
                var proc = Process.GetProcessById((int) pid);
                return proc.MainModule.FileName;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Get the title text of this window
        /// </summary>
        /// <param name="hWnd">widow handle</param>
        /// <returns></returns>
        internal static string GetWindowText(IntPtr hWnd)
        {
            try
            {
                var length = User32.GetWindowTextLength(hWnd);
                var sb = new StringBuilder(length + 1);
                User32.GetWindowText(hWnd, sb, sb.Capacity);
                return sb.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get the application description file attribute from path of an executable file
        /// </summary>
        /// <param name="appPath"></param>
        /// <returns></returns>
        internal static string GetAppDescription(string appPath)
        {
            if (appPath == null) return null;
            try
            {
                return FileVersionInfo.GetVersionInfo(appPath).FileDescription;
            }
            catch
            {
                return null;
            }
        }
    }
}