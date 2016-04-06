using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EventHook.Hooks.Hotkey;

namespace EventHook
{
    public static class HotkeyWatcher
    {
        private static readonly HotkeyListener Listener = new HotkeyListener();
        public static event EventHandler<HotkeyEventArgs> OnHotkeyInput;

        public static void Register(Keys hotkey)
        {
            var info = new HotkeyInfo(hotkey);

            if (info.HotkeyModifiers == Keys.None)
                throw new ArgumentException("Hotkey requires a modifier key.");

            Listener.AddHotkey(info);
        }

        public static void Unregister(Keys hotkey)
        {
            var info = new HotkeyInfo(hotkey);
            Listener.RemoveHotkey(info);
        }

        /// <summary>
        /// Registers all inactive hotkeys.
        /// </summary>
        public static void Start()
        {
            Listener.Start();
        }

        /// <summary>
        /// Unregisters all active hotkeys.
        /// </summary>
        public static void Stop()
        {
            Listener.Stop();
        }
    }
}
