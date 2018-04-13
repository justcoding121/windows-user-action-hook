using EventHook.Hooks;
using EventHook.Helpers;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace EventHook
{
    /// <summary>
    /// Event argument to pass data through user callbacks
    /// </summary>
    public class MouseEventArgs : EventArgs
    {
        public MouseMessages Message { get; set; }
        public POINT Point { get; set; }
    }

    /// <summary>
    /// Wraps low level mouse hook
    /// Uses a producer-consumer pattern to improve performance and to avoid operating system forcing unhook on delayed user callbacks
    /// </summary>
    public class MouseWatcher
    {
        private object accesslock = new object();
        private bool isRunning { get; set; }

        private SyncFactory factory;
        private AsyncQueue<object> mouseQueue;
        private CancellationTokenSource taskCancellationTokenSource;

        private MouseHook mouseHook;
        public event EventHandler<MouseEventArgs> OnMouseInput;

        internal MouseWatcher(SyncFactory factory)
        {
            this.factory = factory;
        }
        /// <summary>
        /// Start watching mouse events
        /// </summary>
        public void Start()
        {
            lock (accesslock)
            {
                if (!isRunning)
                {
                    taskCancellationTokenSource = new CancellationTokenSource();
                    mouseQueue = new AsyncQueue<object>(taskCancellationTokenSource.Token);
                    //This needs to run on UI thread context
                    //So use task factory with the shared UI message pump thread
                    Task.Factory.StartNew(() =>
                    {
                        mouseHook = new MouseHook();
                        mouseHook.MouseAction += MListener;
                        mouseHook.Start();
                    },
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    factory.GetTaskScheduler()).Wait();

                    Task.Factory.StartNew(() => ConsumeKeyAsync());

                    isRunning = true;
                }
            }
        }

        /// <summary>
        /// Stop watching mouse events
        /// </summary>
        public void Stop()
        {
            lock (accesslock)
            {
                if (isRunning)
                {
                    if (mouseHook != null)
                    {
                        //This needs to run on UI thread context
                        //So use task factory with the shared UI message pump thread
                        Task.Factory.StartNew(() =>
                        {
                            mouseHook.MouseAction -= MListener;
                            mouseHook.Stop();
                            mouseHook = null;
                        },
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        factory.GetTaskScheduler());
                    }

                    mouseQueue.Enqueue(false);
                    isRunning = false;
                    taskCancellationTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// Add mouse event to our producer queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MListener(object sender, RawMouseEventArgs e)
        {
            mouseQueue.Enqueue(e);
        }

        /// <summary>
        /// Consume mouse events in our producer queue asynchronously
        /// </summary>
        /// <returns></returns>
        private async Task ConsumeKeyAsync()
        {
            while (isRunning)
            {

                //blocking here until a key is added to the queue
                var item = await mouseQueue.DequeueAsync();
                if (item is bool) break;

                KListener_KeyDown(item as RawMouseEventArgs);

            }
        }

        /// <summary>
        /// Invoke user callbacks with the argument
        /// </summary>
        /// <param name="kd"></param>
        private void KListener_KeyDown(RawMouseEventArgs kd)
        {
            OnMouseInput?.Invoke(null, new MouseEventArgs() { Message = kd.Message, Point = kd.Point });
        }
    }
}
