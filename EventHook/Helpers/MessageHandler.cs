using System.Windows.Forms;

namespace EventHook.Helpers
{
    internal sealed class MessageHandler : NativeWindow
    {
        internal MessageHandler()
        {
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message msg)
        {
            base.WndProc(ref msg);
        }
    }
}