using EventHook.Hooks.Helpers;
using System;
using System.Windows;
using System.Windows.Interop;

namespace EventHook.Hooks
{
    /// <summary>One window event to many application wide listeners</summary>
    public static class WindowHook
    {
        public static void Start(IntPtr handle)
        {
            var sh = new ShellHook(handle);
            sh.WindowCreated += WindowCreatedEvent;
            sh.WindowDestroyed += WindowDestroyedEvent;
            sh.WindowActivated += WindowActivatedEvent;
        }

        /// <summary>
        ///     A top-level, unowned window has been created. The window exists when the system calls this hook.
        /// </summary>
        public static event GeneralShellHookEventHandler WindowCreated;

        /// <summary>
        ///     A top-level, unowned window is about to be destroyed. The window still exists when the system calls this hook.
        /// </summary>
        public static event GeneralShellHookEventHandler WindowDestroyed;

        /// <summary>
        ///     The activation has changed to a different top-level, unowned window.
        /// </summary>
        public static event GeneralShellHookEventHandler WindowActivated;

        private static void WindowCreatedEvent(ShellHook shellObject, IntPtr hWnd)
        {
            if (WindowCreated != null)
            {
                WindowCreated(shellObject, hWnd);
            }
        }

        private static void WindowDestroyedEvent(ShellHook shellObject, IntPtr hWnd)
        {
            if (WindowDestroyed != null)
            {
                WindowDestroyed(shellObject, hWnd);
            }
        }

        private static void WindowActivatedEvent(ShellHook shellObject, IntPtr hWnd)
        {
            if (WindowActivated != null)
            {
                WindowActivated(shellObject, hWnd);
            }
        }
    }
}