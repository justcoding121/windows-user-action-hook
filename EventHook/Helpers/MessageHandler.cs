using System;
using System.Windows.Forms;

namespace EventHook.Helpers
{
    internal class MessageEventArgs : EventArgs
    {
        private Message _message;

        public MessageEventArgs(Message message)
        {
            _message = message;
        }

        public Message Message { get { return _message; } }

        /// <summary>
        /// Suppresses further processing by the subclassed window.
        /// </summary>
        public void Supress()
        {
            _message.Result = IntPtr.Zero;
        }
    }

    internal sealed class MessageHandler : NativeWindow
    {
        internal MessageHandler()
        {
            CreateHandle(new CreateParams());
        }

        public event EventHandler<MessageEventArgs> MessageReceived = delegate { };

        protected override void WndProc(ref Message msg)
        {
            var args = new MessageEventArgs(msg);
            MessageReceived.Invoke(this, args);

            var message = args.Message;
            base.WndProc(ref message);
        }
    }
}