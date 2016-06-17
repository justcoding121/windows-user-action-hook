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
        private static bool _IsRunning { get; set; }
        private static object _Accesslock = new object();

        private static AsyncCollection<object> _kQueue;
        private static MouseHook _mh;

        public static event EventHandler<MouseEventArgs> OnMouseInput;

        /// <summary>
        /// Start watching mouse events
        /// </summary>
        public static void Start()
        {
            if (!_IsRunning)
                lock (_Accesslock)
                {
                    _kQueue = new AsyncCollection<object>();

                    _mh = new MouseHook();
                    _mh.MouseAction += MListener;

                    //low level hooks need to be registered in the context of a UI thread
                    Task.Factory.StartNew(() => { }).ContinueWith(x =>
                    {
                        _mh.Start();

                    }, SharedMessagePump.GetTaskScheduler());

                    Task.Factory.StartNew(() => ConsumeKeyAsync());

                    _IsRunning = true;
                }

        }

        /// <summary>
        /// Stop watching mouse events
        /// </summary>
        public static void Stop()
        {
            if (_IsRunning)
                lock (_Accesslock)
                {
                    if (_mh != null)
                    {
                        _mh.MouseAction -= MListener;
                        _mh.Stop();
                        _mh = null;
                    }
                    _kQueue.Add(false);
                    _IsRunning = false;
                }
        }

        /// <summary>
        /// Add mouse event to our producer queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MListener(object sender, RawMouseEventArgs e)
        {
            _kQueue.Add(e);
        }

        /// <summary>
        /// Consume mouse events in our producer queue asynchronously
        /// </summary>
        /// <returns></returns>
        private static async Task ConsumeKeyAsync()
        {
            while (_IsRunning)
            {

                //blocking here until a key is added to the queue
                var item = await _kQueue.TakeAsync();
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
