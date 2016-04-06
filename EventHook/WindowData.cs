using System;

namespace EventHook
{
    public class WindowData
    {
        public IntPtr HWnd;
        public int EventType;

        public string AppPath { get; set; }
        public string AppName { get; set; }
        public string AppTitle { get; set; }
    }
}