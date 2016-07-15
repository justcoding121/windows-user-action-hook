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
        private static bool isRunning { get; set; }
        private static KeyboardHook keyboardHook;
        private static object accesslock = new object();
        private static AsyncCollection<object> keyQueue;

        public static event EventHandler<KeyInputEventArgs> OnKeyInput;

        /// <summary>
        /// Start watching
        /// </summary>
        public static void Start()
        {
            if (!isRunning)
            {
                lock (accesslock)
                {
                    keyQueue = new AsyncCollection<object>();

                    keyboardHook = new KeyboardHook();
                    keyboardHook.KeyDown += new RawKeyEventHandler(KListener);
                    keyboardHook.KeyUp += new RawKeyEventHandler(KListener);

                    //low level hooks need to run on the context of a UI thread to hook successfully
                    Task.Factory.StartNew(() => { }).ContinueWith(x =>
                    {
                        keyboardHook.Start();

                    }, SharedMessagePump.GetTaskScheduler());

                    Task.Factory.StartNew(() => ConsumeKeyAsync());

                    isRunning = true;
                }
            }
     
        }

        /// <summary>
        /// Stop watching
        /// </summary>
        public static void Stop()
        {
            if (isRunning)
            {
                lock (accesslock)
                {
                    if (keyboardHook != null)
                    {
                        keyboardHook.KeyDown -= new RawKeyEventHandler(KListener);
                        keyboardHook.Stop();
                        keyboardHook = null;
                    }

                    keyQueue.Add(false);
                    isRunning = false;
                }
            }
        }

        /// <summary>
        /// Add key event to the producer queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void KListener(object sender, RawKeyEventArgs e)
        {
            keyQueue.Add(new KeyData() { UnicodeCharacter = e.Character, Keyname = e.Key.ToString(), EventType = (KeyEvent)e.EventType });
        }

       /// <summary>
       /// Consume events from the producer queue asynchronously
       /// </summary>
       /// <returns></returns>
        private static async Task ConsumeKeyAsync()
        {
            while (isRunning)
            {

                //blocking here until a key is added to the queue
                var item = await keyQueue.TakeAsync();
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
