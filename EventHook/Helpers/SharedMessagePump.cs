using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EventHook.Helpers
{
    internal static class SharedMessagePump
    {
        private static bool _hasUiThread;

        private static readonly Lazy<TaskScheduler> Scheduler;
        internal static readonly Lazy<MessageHandler> MessageHandler;

        static SharedMessagePump()
        {
            Scheduler = new Lazy<TaskScheduler>(() =>
            {
                var dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
                if (dispatcher != null)
                {
                    if (SynchronizationContext.Current != null)
                    {
                        _hasUiThread = true;
                        return TaskScheduler.FromCurrentSynchronizationContext();
                    }
                }

                TaskScheduler current = null;

                //if current task scheduler is null, create a message pump 
                //http://stackoverflow.com/questions/2443867/message-pump-in-net-windows-service
                Task.Factory.StartNew(() =>
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        current = TaskScheduler.FromCurrentSynchronizationContext();
                    }), DispatcherPriority.Normal);
                    Dispatcher.Run();
                }).Wait();

                return current;
            });

            MessageHandler = new Lazy<MessageHandler>(() =>
            {
                MessageHandler msgHandler = null;

                Task.Factory.StartNew(e =>
                {
                    msgHandler = new MessageHandler();
                }, GetTaskScheduler()).Wait();

                return msgHandler;
            });

            Initialize();
        }

        private static void Initialize()
        {
            GetTaskScheduler();
            GetHandle();
        }

        internal static TaskScheduler GetTaskScheduler()
        {
            return Scheduler.Value;
        }

        internal static IntPtr GetHandle()
        {
            if (!_hasUiThread)
            {
                return MessageHandler.Value.Handle;
            }

            try
            {
                var handle = Process.GetCurrentProcess().MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    return handle;
                }
            }
            catch
            {
                // ignored
            }

            return MessageHandler.Value.Handle;
        }
    }
}
