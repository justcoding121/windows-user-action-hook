using System;
using EventHook.Helpers;
using System.Threading.Tasks;
using EventHook.Hooks.Keyboard;
using Nito.AsyncEx;

namespace EventHook
{
    public static class KeyboardWatcher
    {
        private static bool _isRunning;
        private static KeyboardHook _kh;
        private static readonly object Accesslock = new object();
        private static AsyncCollection<object> _kQueue;

        public static event EventHandler<KeyInputEventArgs> OnKeyInput;

        public static void Start()
        {
            if (_isRunning) return;

            lock (Accesslock)
                {
                    _kQueue = new AsyncCollection<object>();

                    _kh = new KeyboardHook();
                _kh.KeyDown += KListener;
                _kh.KeyUp += KListener;


                    Task.Factory.StartNew(() => { }).ContinueWith(x =>
                    {
                        _kh.Start();
               
                    }, SharedMessagePump.GetTaskScheduler());

                    Task.Factory.StartNew(() => ConsumeKeyAsync());

                _isRunning = true;
            }
                }
     
        public static void Stop()
        {
            if (!_isRunning) return;

            lock (Accesslock)
                {
                    if (_kh != null)
                    {
                    _kh.KeyDown -= KListener;
                        _kh.Stop();
                        _kh = null;
                    }

                    _kQueue.Add(false);
                _isRunning = false;
                }
        }

        private static void KListener(object sender, RawKeyEventArgs e)
        {
            _kQueue.Add(new KeyData { UnicodeCharacter = e.Character, Keyname = e.Key.ToString(), KeyState = (KeyState)e.KeyState });
        }

        // This is the method to run when the timer is raised. 
        private static async Task ConsumeKeyAsync()
        {
            while (_isRunning)
            {
                //blocking here until a key is added to the queue
                var item = await _kQueue.TakeAsync();
                if (item is bool) break;

                KListener_KeyDown((KeyData)item);
            }
        }

        private static void KListener_KeyDown(KeyData kd)
        {
            var handler = OnKeyInput;
            if (handler != null)
            {
                handler(null, new KeyInputEventArgs(kd));
            }
        }
    }
}
