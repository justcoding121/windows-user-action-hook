
using System;
using System.Windows.Forms;
using AgentSafeX.Client.Utility.Hooks.Library;
using AgentSafeX.Client.Utility.Library;


namespace AgentSafeX.Client.Utility.Hooks
{
    public delegate void GeneralShellHookEventHandler(ShellHook sender, IntPtr hWnd);
  
    public class ShellHook : NativeWindow
    {
        uint WM_ShellHook;

        public ShellHook(IntPtr hWnd)
        {
            CreateParams cp = new CreateParams();

            // Create the actual window
            this.CreateHandle(cp);

            User32.SetTaskmanWindow(hWnd);
         
            if (User32.RegisterShellHookWindow(this.Handle))
            {
                WM_ShellHook = User32.RegisterWindowMessage("SHELLHOOK");

            }

        }

        public void Deregister()
        {
            User32.RegisterShellHook(Handle, 0);
        }

        #region Shell events

        /// <summary>
        /// A top-level, unowned window has been created. The window exists when the system calls this hook.
        /// </summary>
        public event GeneralShellHookEventHandler WindowCreated;
        /// <summary>
        /// A top-level, unowned window is about to be destroyed. The window still exists when the system calls this hook.
        /// </summary>
        public event GeneralShellHookEventHandler WindowDestroyed;
 
        /// <summary>
        /// The activation has changed to a different top-level, unowned window. 
        /// </summary>
        public event GeneralShellHookEventHandler WindowActivated;


        protected unsafe override void WndProc(ref Message m)
        {
            
            if (m.Msg == WM_ShellHook)
            {
                switch ((ShellEvents)m.WParam)
                {
                    
                    case ShellEvents.HSHELL_WINDOWCREATED:
                        if (IsAppWindow(m.LParam))
                        {
                            OnWindowCreated(m.LParam);
                        }

                        break;
                    case ShellEvents.HSHELL_WINDOWDESTROYED:
           
                        if (WindowDestroyed != null)
                        {
                            
                            WindowDestroyed(this, m.LParam);
                        }

                        break;
                
                    case ShellEvents.HSHELL_WINDOWACTIVATED:
                        if (WindowActivated != null)
                        {
                            WindowActivated(this, m.LParam);
                        }
                        break;           
                  
                    default:
                        break;
                }
            }
            base.WndProc(ref m);
        }

        #endregion

        #region Windows enumeration
        public void EnumWindows()
        {
            User32.EnumWindows(EnumWindowsProc, IntPtr.Zero);
        }

        bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam)
        {
            if (IsAppWindow(hWnd))
            {
                OnWindowCreated(hWnd);
            }
            return true;
        }

        bool OnWindowCreated(IntPtr hWnd)
        {

            if (WindowCreated != null)
            {
                WindowCreated(this, hWnd);
            }

            return true;
        }

        bool IsAppWindow(IntPtr hWnd)
        {
            if ((GetWindowLong(hWnd, (int)GWLIndex.GWL_STYLE) & (int)WindowStyle.WS_SYSMENU) == 0) return false;

            if (User32.IsWindowVisible(hWnd))
            {
                if ((GetWindowLong(hWnd, (int)GWLIndex.GWL_EXSTYLE) & (int)WindowStyleEx.WS_EX_TOOLWINDOW) == 0)
                {
                    if (User32.GetParent(hWnd) != null)
                    {
                        IntPtr hwndOwner = User32.GetWindow(hWnd, (int)GetWindowContstants.GW_OWNER);
                        if (hwndOwner != null &&
                        ((GetWindowLong(hwndOwner, (int)GWLIndex.GWL_STYLE) & ((int)WindowStyle.WS_VISIBLE | (int)WindowStyle.WS_CLIPCHILDREN)) != ((int)WindowStyle.WS_VISIBLE | (int)WindowStyle.WS_CLIPCHILDREN)) ||
                        (GetWindowLong(hwndOwner, (int)GWLIndex.GWL_EXSTYLE) & (int)WindowStyleEx.WS_EX_TOOLWINDOW) != 0)
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                    else
                        return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        private int GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return User32.GetWindowLong(hWnd, nIndex);
            }
            return User32.GetWindowLongPtr(hWnd, nIndex);
        }
        #endregion
    }
}
