using EventHook.Client.Utility.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace EventHook.Common
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
        private static Window f;
        private static ShellHook sh;
        static WindowHook()
        {
            if (f == null)
            {
                f = new Window();
                f.Visibility = Visibility.Hidden;
                f.WindowState = WindowState.Minimized;
                f.Show();
                f.Hide();

                sh = new ShellHook(new WindowInteropHelper(f).Handle);
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
