using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace EventHook.Helpers
{
    internal class SharedMessagePump
    {
        private static bool _hasUIThread = false;

        static Lazy<TaskScheduler> _scheduler;
        static Lazy<MessageHandler> _messageHandler;

        static SharedMessagePump()
        {
            _scheduler = new Lazy<TaskScheduler>(() =>
            {
                Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
                if (dispatcher != null)
                {
                    if (SynchronizationContext.Current != null)
                    {
                        _hasUIThread = true;
                        return TaskScheduler.FromCurrentSynchronizationContext();
                    }
                }           
            
                TaskScheduler current = null;

                //if current task scheduler is null, create a message pump 
                //http://stackoverflow.com/questions/2443867/message-pump-in-net-windows-service
                //use async for performance gain!
                new Task(() =>
                {
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        current = TaskScheduler.FromCurrentSynchronizationContext();
                    }

               ), DispatcherPriority.Normal);
                    System.Windows.Threading.Dispatcher.Run();
                }).Start();

                while (current == null)
                {
                    Thread.Sleep(10);
                }

                return current;

            });

            _messageHandler = new Lazy<MessageHandler>(() =>
                {
                    MessageHandler msgHandler = null;

                    new Task((e) =>
                    {
                        msgHandler = new MessageHandler();
                    }, GetTaskScheduler()).Start();

                    while (msgHandler == null)
                    {
                        Thread.Sleep(10);
                    };

                    return msgHandler;
                });
        }

        internal static void Initialize()
        {
            GetTaskScheduler();
            GetHandle();
        }

        internal static TaskScheduler GetTaskScheduler()
        {
            return _scheduler.Value;
        }

        internal static IntPtr GetHandle()
        {
            var handle = IntPtr.Zero;

            if (_hasUIThread)
            {
                try
                {
                    handle = Process.GetCurrentProcess().MainWindowHandle;

                    if (handle != IntPtr.Zero)
                        return handle;
                }
                catch { }
            }

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
