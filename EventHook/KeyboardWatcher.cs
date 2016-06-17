using System;
using EventHook.Hooks;
using EventHook.Helpers;
using System.Threading.Tasks;
using Nito.AsyncEx;


namespace EventHook
{
    /// <summary>
    /// Key press data
    /// </summary>
    public class KeyInputEventArgs : EventArgs
    {
        public KeyData KeyData { get; set; }
    }

    /// <summary>
    /// Key data
    /// </summary>
    public class KeyData
    {
        public string UnicodeCharacter;
        public string Keyname;
        public KeyEvent EventType;
    }

    /// <summary>
    /// Key press event type
    /// </summary>
    public enum KeyEvent
    {
        down = 0,
        up = 1
    }

    /// <summary>
    /// Wraps low level keyboard hook
    /// Uses a producer-consumer pattern to improve performance and to avoid operating system forcing unhook on delayed user callbacks
    /// </summary>
    public class KeyboardWatcher
    {

        /*Keyboard*/
        private static bool _IsRunning { get; set; }
        private static KeyboardHook _kh;
        private static object _Accesslock = new object();
        private static AsyncCollection<object> _kQueue;

        public static event EventHandler<KeyInputEventArgs> OnKeyInput;

        /// <summary>
        /// Start watching
        /// </summary>
        public static void Start()
        {
            if (!_IsRunning)
                lock (_Accesslock)
                {
                    _kQueue = new AsyncCollection<object>();

                    _kh = new KeyboardHook();
                    _kh.KeyDown += new RawKeyEventHandler(KListener);
                    _kh.KeyUp += new RawKeyEventHandler(KListener);

                    //low level hooks need to run on the context of a UI thread to hook successfully
                    Task.Factory.StartNew(() => { }).ContinueWith(x =>
                    {
                        _kh.Start();
               
                    }, SharedMessagePump.GetTaskScheduler());

                    Task.Factory.StartNew(() => ConsumeKeyAsync());

                    _IsRunning = true;
                }
     
        }

        /// <summary>
        /// Stop watching
        /// </summary>
        public static void Stop()
        {
            if (_IsRunning)
                lock (_Accesslock)
                {
                    if (_kh != null)
                    {
                        _kh.KeyDown -= new RawKeyEventHandler(KListener);
                        _kh.Stop();
                        _kh = null;
                    }

                    _kQueue.Add(false);
                    _IsRunning = false;
                }
        }

        /// <summary>
        /// Add key event to the producer queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void KListener(object sender, RawKeyEventArgs e)
        {
            _kQueue.Add(new KeyData() { UnicodeCharacter = e.Character, Keyname = e.Key.ToString(), EventType = (KeyEvent)e.EventType });
        }

       /// <summary>
       /// Consume events from the producer queue asynchronously
       /// </summary>
       /// <returns></returns>
        private static async Task ConsumeKeyAsync()
        {
            while (_IsRunning)
            {

                //blocking here until a key is added to the queue
                var item = await _kQueue.TakeAsync();
                if (item is bool) break;

                KListener_KeyDown((KeyData)item);
            }
        }

        /// <summary>
        /// Invoke user call backs
        /// </summary>
        /// <param name="kd"></param>
        private static void KListener_KeyDown(KeyData kd)
        {
            OnKeyInput?.Invoke(null, new KeyInputEventArgs() { KeyData = kd });

        }

    }
}
