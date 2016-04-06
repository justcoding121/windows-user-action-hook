using System;

namespace EventHook
{
    public class ApplicationEventArgs : EventArgs
    {
        private readonly WindowData _data;
        private readonly ApplicationEvent _eventType;

        public ApplicationEventArgs(WindowData data, ApplicationEvent eventType)
        {
            _data = data;
            _eventType = eventType;
        }

        public WindowData ApplicationData { get { return _data; } }
        public ApplicationEvent Event { get { return _eventType; } }
    }
}