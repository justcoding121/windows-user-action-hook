using EventHook.Hooks.Helpers;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace EventHook.Hooks
{
    ///<summary>One window event to many application wide listeners</summary>
    public static class WindowHook
    {
        /// <summary>
        /// A top-level, unowned window has been created. The window exists when the system calls this hook.
        /// </summary>
        public static event GeneralShellHookEventHandler WindowCreated;
        /// <summary>
        /// A top-level, unowned window is about to be destroyed. The window still exists when the system calls this hook.
        /// </summary>
        public static event GeneralShellHookEventHandler WindowDestroyed;

        /// <summary>
        /// The activation has changed to a different top-level, unowned window. 
        /// </summary>
        public static event GeneralShellHookEventHandler WindowActivated;

        private static ShellHook sh;

        static WindowHook()
        {
            if (sh == null)
            {
                sh = new ShellHook(SharedMessagePump.GetHandle());

                sh.WindowCreated += new GeneralShellHookEventHandler(WindowCreatedEvent);
                sh.WindowDestroyed += new GeneralShellHookEventHandler(WindowDestroyedEvent);
                sh.WindowActivated += new GeneralShellHookEventHandler(WindowActivatedEvent);
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