using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EventHook.Hooks.Library
{
    internal static class User32
    {
        [DllImport("user32.dll")]
        internal static extern int GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SetClipboardViewer(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ChangeClipboardChain(
            IntPtr hWndRemove, // handle to window to remove
            IntPtr hWndNewNext // handle to next window
            );

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool EnumWindows(EnumWindowsProc numFunc, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetWindow(IntPtr hwnd, int uCmd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool RegisterShellHook(IntPtr hWnd, int flags);

        /// <summary>
        ///     Registers a specified Shell window to receive certain messages for events or notifications that are useful to
        ///     Shell applications. The event messages received are only those sent to the Shell window associated with the
        ///     specified window's desktop. Many of the messages are the same as those that can be received after calling
        ///     the SetWindowsHookEx function and specifying WH_SHELL for the hook type. The difference with
        ///     RegisterShellHookWindow is that the messages are received through the specified window's WindowProc
        ///     and not through a call back procedure.
        /// </summary>
        /// <param name="hWnd">[in] Handle to the window to register for Shell hook messages.</param>
        /// <returns>TRUE if the function succeeds; FALSE if the function fails. </returns>
        /// <remarks>
        ///     As with normal window messages, the second parameter of the window procedure identifies the
        ///     message as a "WM_SHELLHOOKMESSAGE". However, for these Shell hook messages, the
        ///     message value is not a pre-defined constant like other message identifiers (IDs) such as
        ///     WM_COMMAND. The value must be obtained dynamically using a call to
        ///     RegisterWindowMessage(TEXT("SHELLHOOK"));. This precludes handling these messages using
        ///     a traditional switch statement which requires ID values that are known at compile time.
        ///     For handling Shell hook messages, the normal practice is to code an If statement in the default
        ///     section of your switch statement and then handle the message if the value of the message ID
        ///     is the same as the value obtained from the RegisterWindowMessage call.
        ///     for more see MSDN
        /// </remarks>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool RegisterShellHookWindow(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern uint RegisterWindowMessage(string Message);

        [DllImport("user32.dll")]
        internal static extern void SetTaskmanWindow(IntPtr hwnd);

        internal delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);


        /// <summary>
        /// Defines a system-wide hot key.
        /// </summary>
        /// <param name="hWnd">A handle to the window that will receive WM_HOTKEY messages generated by the hot key. 
        /// If this parameter is NULL, WM_HOTKEY messages are posted to the message queue of the calling thread and must be processed in the message loop.</param>
        /// <param name="id">The identifier of the hot key. 
        /// If the hWnd parameter is NULL, then the hot key is associated with the current thread rather than with a particular window. 
        /// If a hot key already exists with the same hWnd and id parameters</param>
        /// <param name="fsModifiers">The keys that must be pressed in combination with the key specified by the uVirtKey parameter in order to generate the WM_HOTKEY message. 
        /// The fsModifiers parameter can be a combination of the following values.</param>
        /// <param name="vk">The virtual-key code of the hot key</param>
        /// <returns>If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterHotKey(IntPtr hWnd, IntPtr id, uint fsModifiers, uint vk);

        /// <summary>
        /// Frees a hot key previously registered by the calling thread.
        /// </summary>
        /// <param name="hWnd">A handle to the window associated with the hot key to be freed. This parameter should be NULL if the hot key is not associated with a window.</param>
        /// <param name="id">The identifier of the hot key to be freed.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, IntPtr id);
    }
}