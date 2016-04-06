using System;
using EventHook.Helpers;
using EventHook.Hooks.Shell;

namespace EventHook.Hooks.Window
{
    ///<summary>One window event to many application wide listeners</summary>
    internal static class WindowHook
    {
        /// <summary>
        /// A top-level, unowned window has been created. The window exists when the system calls this hook.
        /// </summary>
        internal static event GeneralShellHookEventHandler WindowCreated;
        /// <summary>
        /// A top-level, unowned window is about to be destroyed. The window still exists when the system calls this hook.
        /// </summary>
        internal static event GeneralShellHookEventHandler WindowDestroyed;

        /// <summary>
        /// The activation has changed to a different top-level, unowned window. 
        /// </summary>
        internal static event GeneralShellHookEventHandler WindowActivated;

        private static ShellHook sh;

        static WindowHook()
        {
            if (sh == null)
            {
                sh = new ShellHook(SharedMessagePump.GetHandle());

                sh.WindowCreated += WindowCreatedEvent;
                sh.WindowDestroyed += WindowDestroyedEvent;
                sh.WindowActivated += WindowActivatedEvent;
            }

        }
        private static void WindowCreatedEvent(ShellHook ShellObject, IntPtr hWnd)
        {
            if (WindowCreated != null)
            {
                WindowCreated(ShellObject, hWnd);
            }

        }
        private static void WindowDestroyedEvent(ShellHook ShellObject, IntPtr hWnd)
        {
            if (WindowDestroyed != null)
            {
                WindowDestroyed(ShellObject, hWnd);
            }
        }
        private static void WindowActivatedEvent(ShellHook ShellObject, IntPtr hWnd)
        {
            if (WindowActivated != null)
            {
                WindowActivated(ShellObject, hWnd);
            }
        }
    }
}