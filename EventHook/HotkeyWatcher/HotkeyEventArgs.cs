using System;

namespace EventHook
{
    public class HotkeyEventArgs : EventArgs
    {
        private readonly HotkeyInfo _info;

        public HotkeyEventArgs(HotkeyInfo info)
        {
            _info = info;
        }

        public HotkeyInfo HotkeyInfo { get { return _info; } }
    }
}