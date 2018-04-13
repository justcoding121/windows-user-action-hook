using System;
using EventHook.Hooks;
using EventHook.Helpers;
using System.Threading.Tasks;
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
        private bool isRunning { get; set; }
        private object accesslock = new object();

        private SyncFactory factory;
        private CancellationTokenSource taskCancellationTokenSource;
        private AsyncQueue<object> keyQueue;

        private KeyboardHook keyboardHook;
        public event EventHandler<KeyInputEventArgs> OnKeyInput;

        internal KeyboardWatcher(SyncFactory factory)
        {
            this.factory = factory;
        }
        /// <summary>
        /// Start watching
        /// </summary>
        public void Start()
        {
            lock (accesslock)
            {
                if (!isRunning)
                {
                    taskCancellationTokenSource = new CancellationTokenSource();
                    keyQueue = new AsyncQueue<object>(taskCancellationTokenSource.Token);

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
                    factory.GetTaskScheduler()).Wait();

                    Task.Factory.StartNew(() => ConsumeKeyAsync());

                    isRunning = true;
                }
            }

        }

        /// <summary>
        /// Stop watching
        /// </summary>
        public void Stop()
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
                        factory.GetTaskScheduler()).Wait();
                    }

                    keyQueue.Enqueue(false);
                    isRunning = false;
                    taskCancellationTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// Add key event to the producer queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KListener(object sender, RawKeyEventArgs e)
        {
            keyQueue.Enqueue(new KeyData() { UnicodeCharacter = e.Character, Keyname = e.Key.ToString(), EventType = (KeyEvent)e.EventType });
        }

        /// <summary>
        /// Consume events from the producer queue asynchronously
        /// </summary>
        /// <returns></returns>
        private async Task ConsumeKeyAsync()
        {
            while (isRunning)
            {

                //blocking here until a key is added to the queue
                var item = await keyQueue.DequeueAsync();
                if (item is bool) break;

                KListener_KeyDown((KeyData)item);
            }
        }

        /// <summary>
        /// Invoke user call backs
        /// </summary>
        /// <param name="kd"></param>
        private void KListener_KeyDown(KeyData kd)
        {
            OnKeyInput?.Invoke(null, new KeyInputEventArgs() { KeyData = kd });
        }

    }
}
