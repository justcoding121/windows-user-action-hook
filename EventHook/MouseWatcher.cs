using EventHook.Helpers;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using EventHook.Hooks.Mouse;

namespace EventHook
{
    public static class MouseWatcher
    {
        private static bool _isRunning;
        private static readonly object Accesslock = new object();

        private static AsyncCollection<object> _kQueue;
        private static MouseHook _mh;

        public static event EventHandler<MouseEventArgs> OnMouseInput;

        public static void Start()
        {
            if (_isRunning) return;

            lock (Accesslock)
                {
                    _kQueue = new AsyncCollection<object>();

                    _mh = new MouseHook();
                    _mh.MouseAction += MListener;


                    Task.Factory.StartNew(() => { }).ContinueWith(x =>
                    {
                        _mh.Start();

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
                    if (_mh != null)
                    {
                        _mh.MouseAction -= MListener;
                        _mh.Stop();
                        _mh = null;
                    }
                    _kQueue.Add(false);
                _isRunning = false;
                }
        }


        private static void MListener(object sender, RawMouseEventArgs e)
        {
            _kQueue.Add(e);
        }

        // This is the method to run when the timer is raised. 
        private static async Task ConsumeKeyAsync()
        {
            while (_isRunning)
            {

                //blocking here until a key is added to the queue
                var item = await _kQueue.TakeAsync();
                if (item is bool) break;

                KListener_KeyDown(item as RawMouseEventArgs);
            }
        }

        private static void KListener_KeyDown(RawMouseEventArgs kd)
        {
            var handler = OnMouseInput;
            if (handler != null)
            {
                handler(null, new MouseEventArgs(kd));
            }
        }
    }
}
