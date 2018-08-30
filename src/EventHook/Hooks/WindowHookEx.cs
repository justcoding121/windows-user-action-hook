namespace EventHook.Hooks
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Track events across all windows
    /// </summary>
    public sealed class WindowHookEx: IDisposable
    {
        readonly WinEventProc proc;
        readonly SortedDictionary<WindowEvent, IntPtr> hooks = new SortedDictionary<WindowEvent, IntPtr>();

        /// <summary>
        /// Must be called from UI thread
        /// </summary>
        public WindowHookEx() {
            this.proc = this.Hook;
        }

        EventHandler<WindowEventArgs> activated;
        /// <summary>
        /// Occurs when a window is about to be activated
        /// </summary>
        public event EventHandler<WindowEventArgs> Activated {
            add => this.EventAdd(ref this.activated, value, WindowEvent.ForegroundChanged);
            remove => this.EventRemove(ref this.activated, value, WindowEvent.ForegroundChanged);
        }

        EventHandler<WindowEventArgs> minimized;
        /// <summary>
        /// Occurs when a window is about to be minimized
        /// </summary>
        public event EventHandler<WindowEventArgs> Minimized {
            add => this.EventAdd(ref this.minimized, value, WindowEvent.Minimized);
            remove => this.EventRemove(ref this.minimized, value, WindowEvent.Minimized);
        }

        EventHandler<WindowEventArgs> unminimized;
        /// <summary>
        /// Occurs when a window is about to be restored from minimized state
        /// </summary>
        public event EventHandler<WindowEventArgs> Unminimized {
            add => this.EventAdd(ref this.unminimized, value, WindowEvent.Unmiminized);
            remove => this.EventRemove(ref this.unminimized, value, WindowEvent.Unmiminized);
        }

        EventHandler<WindowEventArgs> textChanged;
        /// <summary>
        /// Occurs when window's text is changed
        /// </summary>
        public event EventHandler<WindowEventArgs> TextChanged {
            add => this.EventAdd(ref this.textChanged, value, WindowEvent.NameChanged);
            remove => this.EventRemove(ref this.textChanged, value, WindowEvent.NameChanged);
        }

        void EventAdd(ref EventHandler<WindowEventArgs> handler, EventHandler<WindowEventArgs> user,
            WindowEvent @event) {
            lock (this.hooks) {
                handler += user;
                this.EnsureHook(@event);
            }
        }

        void EventRemove(ref EventHandler<WindowEventArgs> handler, EventHandler<WindowEventArgs> user,
            WindowEvent @event) {
            lock (this.hooks) {
                if (handler != null && handler - user == null) {
                    if (!UnhookWinEvent(this.hooks[@event]))
                        throw new Win32Exception();
                    bool existed = this.hooks.Remove(@event);
                    Debug.Assert(existed);
                }

                handler -= user;
            }
        }

        void Hook(IntPtr hookHandle, WindowEvent @event,
            IntPtr hwnd,
            int @object, int child,
            int threadID, int timestampMs) {
            EventHandler<WindowEventArgs> handler;
            switch (@event) {
            case WindowEvent.ForegroundChanged: handler = this.activated; break;
            case WindowEvent.NameChanged: handler = this.textChanged; break;
            case WindowEvent.Minimized: handler = this.minimized; break;
            case WindowEvent.Unmiminized: handler = this.unminimized; break;
            default: Debug.Write($"Unexpected event {@event}"); return;
            }
            handler?.Invoke(this, new WindowEventArgs(hwnd));
        }

        void EnsureHook(WindowEvent @event) {
            if (!this.hooks.TryGetValue(@event, out var hookID)) {
                hookID = this.SetHook(@event);
                this.hooks.Add(@event, hookID);
            }
        }

        IntPtr SetHook(WindowEvent @event) {
            IntPtr hookID = SetWinEventHook(
                hookMin: @event, hookMax: @event,
                moduleHandle: IntPtr.Zero, callback: this.proc,
                processID: 0, threadID: 0,
                flags: HookFlags.OutOfContext);
            if (hookID == IntPtr.Zero)
                throw new Win32Exception();
            return hookID;
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        void ReleaseUnmanagedResources(bool disposing) {
            lock (this.hooks) {
                Debug.Assert(!Environment.HasShutdownStarted && (this.hooks.Count == 0 || disposing), "Somebody forgot to dispose the hook. "
                    + "That will cause heap corruption, because the hook handler "
                    + "will be disposed before hooking is disabled by the code below.");
                foreach (IntPtr hook in this.hooks.Values) {
                    if (!UnhookWinEvent(hook)) {
                        var error = new Win32Exception();
                        // handle is invalid
                        if (error.NativeErrorCode != 0x6)
                            throw error;
                        else
                            Debug.WriteLine("Can't dispose hook: the handle is invalid");
                    }
                }

                this.hooks.Clear();
            }

            GC.KeepAlive(this.proc);
        }

        /// <inheritdoc />
        public void Dispose() {
            this.ReleaseUnmanagedResources(disposing: true);
            this.activated = this.minimized = this.unminimized = this.textChanged = null;
            GC.SuppressFinalize(this);
        }

        ~WindowHookEx() {
            this.ReleaseUnmanagedResources(disposing: false);
        }

        #region Native API
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWinEventHook(WindowEvent hookMin, WindowEvent hookMax,
            IntPtr moduleHandle,
            WinEventProc callback, int processID, int threadID, HookFlags flags);

        [Flags]
        enum HookFlags: int
        {
            OutOfContext = 0,
        }

        enum WindowEvent
        {
            ForegroundChanged = 0x03,
            NameChanged = 0x800C,
            Minimized = 0x0016,
            Unmiminized = 0x0017,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWinEvent(IntPtr hhk);

        delegate void WinEventProc(IntPtr hookHandle, WindowEvent @event,
            IntPtr hwnd,
            int @object, int child,
            int threadID, int timestampMs);
        #endregion
    }

    /// <summary>
    /// The window event arguments.
    /// </summary>
    public class WindowEventArgs
    {
        public WindowEventArgs(IntPtr handle) {
            this.Handle = handle;
        }
        public IntPtr Handle { get; }
    }
}
