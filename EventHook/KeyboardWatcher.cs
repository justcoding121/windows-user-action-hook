using System;
using EventHook.Hooks;
using EventHook.Helpers;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Threading;

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
        private  bool isRunning { get; set; }
        private  KeyboardHook keyboardHook;
        private  object accesslock = new object();
        private  AsyncCollection<object> keyQueue;
        public  event EventHandler<KeyInputEventArgs> OnKeyInput;

        /// <summary>
        /// Start watching
        /// </summary>
        public  void Start()
        {
            lock (accesslock)
            {
                if (!isRunning)
                {
                    keyQueue = new AsyncCollection<object>();
                    //This needs to run on UI thread context
                    //So use task factory with the shared UI message pump thread
                    Task.Factory.StartNew(() =>
                    {
                        keyboardHook = new KeyboardHook();
                        keyboardHook.KeyDown += new RawKeyEventHandler(KListener);
                        keyboardHook.KeyUp += new RawKeyEventHandler(KListener);
                        keyboardHook.Start();
                    },
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    SyncFactory.GetTaskScheduler());

                    Task.Factory.StartNew(() => ConsumeKeyAsync());

                    isRunning = true;
                }
            }

        }

        /// <summary>
        /// Stop watching
        /// </summary>
        public  void Stop()
        {
            lock (accesslock)
            {
                if (isRunning)
                {
                    if (keyboardHook != null)
                    {
                        //This needs to run on UI thread context
                        //So use task factory with the shared UI message pump thread
                        Task.Factory.StartNew(() =>
                        {
                            keyboardHook.KeyDown -= new RawKeyEventHandler(KListener);
                            keyboardHook.Stop();
                            keyboardHook = null;
                        },
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        SyncFactory.GetTaskScheduler());
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
        private  void KListener(object sender, RawKeyEventArgs e)
        {
            keyQueue.Add(new KeyData() { UnicodeCharacter = e.Character, Keyname = e.Key.ToString(), EventType = (KeyEvent)e.EventType });
        }

        /// <summary>
        /// Consume events from the producer queue asynchronously
        /// </summary>
        /// <returns></returns>
        private  async Task ConsumeKeyAsync()
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
        private  void KListener_KeyDown(KeyData kd)
        {
            OnKeyInput?.Invoke(null, new KeyInputEventArgs() { KeyData = kd });

        }

    }
}
