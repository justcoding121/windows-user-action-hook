using EventHook.Hooks;
using EventHook.Helpers;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

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
        /*Keyboard*/
        private static bool isRunning { get; set; }
        private static object accesslock = new object();

        private static AsyncCollection<object> mouseQueue;
        private static MouseHook mouseHook;

        public static event EventHandler<MouseEventArgs> OnMouseInput;

        /// <summary>
        /// Start watching mouse events
        /// </summary>
        public static void Start()
        {
            if (!isRunning)
            {
                lock (accesslock)
                {
                    mouseQueue = new AsyncCollection<object>();

                    mouseHook = new MouseHook();
                    mouseHook.MouseAction += MListener;

                    //low level hooks need to be registered in the context of a UI thread
                    Task.Factory.StartNew(() => { }).ContinueWith(x =>
                    {
                        mouseHook.Start();

                    }, SharedMessagePump.GetTaskScheduler());

                    Task.Factory.StartNew(() => ConsumeKeyAsync());

                    isRunning = true;
                }
            }
        }

        /// <summary>
        /// Stop watching mouse events
        /// </summary>
        public static void Stop()
        {
            if (isRunning)
            {
                lock (accesslock)
                {
                    if (mouseHook != null)
                    {
                        mouseHook.MouseAction -= MListener;
                        mouseHook.Stop();
                        mouseHook = null;
                    }
                    mouseQueue.Add(false);
                    isRunning = false;
                }
            }
        }

        /// <summary>
        /// Add mouse event to our producer queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MListener(object sender, RawMouseEventArgs e)
        {
            mouseQueue.Add(e);
        }

        /// <summary>
        /// Consume mouse events in our producer queue asynchronously
        /// </summary>
        /// <returns></returns>
        private static async Task ConsumeKeyAsync()
        {
            while (isRunning)
            {

                //blocking here until a key is added to the queue
                var item = await mouseQueue.TakeAsync();
                if (item is bool) break;

                KListener_KeyDown(item as RawMouseEventArgs);

            }
        }

        /// <summary>
        /// Invoke user callbacks with the argument
        /// </summary>
        /// <param name="kd"></param>
        private static void KListener_KeyDown(RawMouseEventArgs kd)
        {
            OnMouseInput?.Invoke(null, new MouseEventArgs() { Message = kd.Message, Point = kd.Point });
        }
    }
}
