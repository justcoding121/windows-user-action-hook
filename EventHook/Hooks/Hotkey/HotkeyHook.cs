using System;
using System.Linq;
using System.Text;
using EventHook.Helpers;
using EventHook.Hooks.Library;

namespace EventHook.Hooks.Hotkey
{
    /// <summary>
    /// Represents a single hotkey
    /// </summary>
    internal class HotkeyHook
    {
        private readonly HotkeyHookInfo _info;
        private IntPtr _hookId = IntPtr.Zero;

        internal HotkeyHook(HotkeyInfo info)
        {
            _info = new HotkeyHookInfo(info);
        }

        internal HotkeyHookInfo Info { get { return _info; } }
        internal IntPtr HookId { get { return _hookId; } }

        internal void Start()
        {
            _hookId = SetHook();
        }

        internal void Stop()
        {
            User32.UnregisterHotKey(SharedMessagePump.GetHandle(), _hookId);
        }

        private IntPtr SetHook()
        {
            var hookId = (IntPtr)Kernel32.GlobalAddAtom(Guid.NewGuid().ToString());
            var success = User32.RegisterHotKey(SharedMessagePump.GetHandle(), hookId, _info.HookModifier, _info.HookKey);
            return success ? hookId : IntPtr.Zero;
        }
    }
}
