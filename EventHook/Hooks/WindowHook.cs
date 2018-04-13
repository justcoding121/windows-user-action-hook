using EventHook.Helpers;
using System;

namespace EventHook.Hooks
{
    ///<summary>One window event to many application wide listeners</summary>
    internal class WindowHook
    {
        /// <summary>
        /// A top-level, unowned window has been created. The window exists when the system calls this hook.
        /// </summary>
        internal event GeneralShellHookEventHandler WindowCreated;
        /// <summary>
        /// A top-level, unowned window is about to be destroyed. The window still exists when the system calls this hook.
        /// </summary>
        internal event GeneralShellHookEventHandler WindowDestroyed;

        /// <summary>
        /// The activation has changed to a different top-level, unowned window. 
        /// </summary>
        internal event GeneralShellHookEventHandler WindowActivated;

        private static ShellHook sh;

        internal WindowHook(SyncFactory factory)
        {
            if (sh == null)
            {
                sh = new ShellHook(factory.GetHandle());

                sh.WindowCreated += new GeneralShellHookEventHandler(WindowCreatedEvent);
                sh.WindowDestroyed += new GeneralShellHookEventHandler(WindowDestroyedEvent);
                sh.WindowActivated += new GeneralShellHookEventHandler(WindowActivatedEvent);
            }

        }
        private void WindowCreatedEvent(ShellHook ShellObject, IntPtr hWnd)
        {
            WindowCreated?.Invoke(ShellObject, hWnd);
        }
        private void WindowDestroyedEvent(ShellHook ShellObject, IntPtr hWnd)
        {
            WindowDestroyed?.Invoke(ShellObject, hWnd);
        }
        private void WindowActivatedEvent(ShellHook ShellObject, IntPtr hWnd)
        {
            WindowActivated?.Invoke(ShellObject, hWnd);
        }

        internal void Destroy()
        {
            sh = null;
        }
    }
}