//adapted from
//https://gist.github.com/Ciantic/471698
namespace EventHook.Hooks.Keyboard
{
    internal class KeyboardHook
    {
        private KeyboardListener _listener;
        internal event RawKeyEventHandler KeyDown = delegate { };
        internal event RawKeyEventHandler KeyUp = delegate { };

        internal void Start()
        {
            _listener = new KeyboardListener();
            _listener.KeyDown += KListener_KeyDown;
            _listener.KeyUp += KListener_KeyUp;
        }

        internal void Stop()
        {
            if (_listener != null)
            {
                _listener.KeyDown -= KListener_KeyDown;
                _listener.KeyUp -= KListener_KeyUp;
                _listener.Dispose();
            }
        }


        private void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            KeyDown(sender, args);
        }

        private void KListener_KeyUp(object sender, RawKeyEventArgs args)
        {
            KeyUp(sender, args);
        }
    }

    /// <summary>
    ///     Raw keyevent handler.
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="args">raw keyevent arguments</param>
    internal delegate void RawKeyEventHandler(object sender, RawKeyEventArgs args);

}
