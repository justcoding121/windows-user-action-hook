using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Windows.Threading;
using EventHook.Helpers;
using EventHook.Hooks.Library;

namespace EventHook.Hooks.Hotkey
{
    internal class HotkeyListener
    {
        private readonly ConcurrentDictionary<HotkeyHook, bool> _hooks = 
            new ConcurrentDictionary<HotkeyHook, bool>();

        internal HotkeyListener()
        {
            SharedMessagePump.MessageHandler.Value.MessageReceived += Handler_MessageReceived;
        }

        internal event EventHandler<HotkeyEventArgs> OnHotkey;

        private void Handler_MessageReceived(object sender, MessageEventArgs e)
        {
            var handler = OnHotkey;
            if (handler == null || e.Message.Msg != (int)WinMessage.WM_HOTKEY)
            {
                return;
            }

            var hook = _hooks
                .Where(kvp => kvp.Key.HookId == e.Message.WParam)
                .Select(kvp => kvp.Key)
                .SingleOrDefault();
            if (hook == null)
            {
                return;
            }

            e.Supress();
            var args = new HotkeyEventArgs(hook.Info);
            handler.Invoke(sender, args);
        }

        public void AddHotkey(HotkeyInfo info)
        {
            _hooks.GetOrAdd(new HotkeyHook(info), false);
        }

        public void RemoveHotkey(HotkeyInfo info)
        {
            var hook = _hooks
                .Where(kvp => kvp.Key.Info.Equals(info))
                .Select(kvp => kvp.Key)
                .SingleOrDefault();

            if (hook != null)
            {
                hook.Stop();

                bool value;
                _hooks.TryRemove(hook, out value);
            }
        }

        public void Start()
        {
            foreach (var hook in _hooks.Where(kvp => !kvp.Value).Select(kvp => kvp.Key))
            {
                hook.Start();
                _hooks[hook] = true;
            }
        }

        public void Stop()
        {
            foreach (var hook in _hooks.Where(kvp => kvp.Value).Select(kvp => kvp.Key))
            {
                hook.Stop();
                _hooks[hook] = false;
            }
        }
    }
}
