using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace EventHook.Hooks.Helpers
{
    internal class SharedMessagePump
    {
        static Lazy<TaskScheduler> _scheduler;
        static Lazy<MessageHandler> _messageHandler;

        static SharedMessagePump()
        {
            _scheduler = new Lazy<TaskScheduler>(() =>
            {
                TaskScheduler current = null;
                //check if current is null, else create a message pump and a shared hwnd handle
                //http://stackoverflow.com/questions/2443867/message-pump-in-net-windows-service
                //use async for performance gain!
                var t = new Task(() =>
                {
                   
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        current = TaskScheduler.FromCurrentSynchronizationContext();
                    }

               ), DispatcherPriority.Normal);
                    System.Windows.Threading.Dispatcher.Run();
                });
                t.Start();

                while (current == null) ;

                return current;
            });

            _messageHandler = new Lazy<MessageHandler>(() =>
                {
                    MessageHandler msgHandler = null;
                    var t = new Task((e) => { msgHandler = new MessageHandler(); }, GetTaskScheduler());

                    t.Start();

                    while (msgHandler == null);

                    return msgHandler;
                });
        }

        internal static TaskScheduler GetTaskScheduler()
        {
            return _scheduler.Value;
        }

        internal static IntPtr GetHandle()
        {
            return _messageHandler.Value.Handle;
        }

    }

    internal class MessageHandler : NativeWindow
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
