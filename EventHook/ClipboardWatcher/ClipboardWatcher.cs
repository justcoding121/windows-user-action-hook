using EventHook.Hooks;
using EventHook.Helpers;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace EventHook
{
    public static class ClipboardWatcher
    {
        private static bool _isRunning;
        private static readonly object Accesslock = new object();

        private static ClipBoardHook _clip;
        private static AsyncCollection<object> _clipQueue;

        public static event EventHandler<ClipboardEventArgs> OnClipboardModified;

        public static void Start()
        {
            if (_isRunning) return;

            lock (Accesslock)
            {
                try
                {
                    _clipQueue = new AsyncCollection<object>();


                    Task.Factory.StartNew(() => { }).ContinueWith(x =>
                    {
                        _clip = new ClipBoardHook();
                        _clip.RegisterClipboardViewer();
                        _clip.ClipBoardChanged += ClipboardHandler;

                    }, SharedMessagePump.GetTaskScheduler());

                    Task.Factory.StartNew(() => ClipConsumerAsync());

                    _isRunning = true;

                }
                catch
                {
                    if (_clip != null)
                    {
                        Stop();
                    }

                }
            }
        }

        public static void Stop()
        {
            if (!_isRunning) return;

            lock (Accesslock)
            {
                if (_clip != null)
                {

                    Task.Factory.StartNew(() => { }).ContinueWith(x =>
                    {
                        _clip.ClipBoardChanged -= ClipboardHandler;
                        _clip.UnregisterClipboardViewer();
                        _clip.Dispose();

                    }, SharedMessagePump.GetTaskScheduler());

                }

                _isRunning = false;
                _clipQueue.Add(false);
            }
        }

        private static void ClipboardHandler(object sender, EventArgs e)
        {
            _clipQueue.Add(sender);
        }

        private static async Task ClipConsumerAsync()
        {
            while (_isRunning)
            {
                var item = await _clipQueue.TakeAsync();
                if (item is bool) break;

                ClipboardHandler(item);
            }
        }

        private static void ClipboardHandler(object sender)
        {
            System.Windows.Forms.IDataObject iData = (System.Windows.Forms.DataObject)sender;

            var format = default(ClipboardContentType);

            object data = null;

            var validDataType = false;
            if (iData.GetDataPresent(System.Windows.Forms.DataFormats.Text))
            {
                format = ClipboardContentType.PlainText;
                data = iData.GetData(System.Windows.Forms.DataFormats.Text);
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.Rtf))
            {
                format = ClipboardContentType.RichText;
                data = iData.GetData(System.Windows.Forms.DataFormats.Rtf);
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.CommaSeparatedValue))
            {
                format = ClipboardContentType.Csv;
                data = iData.GetData(System.Windows.Forms.DataFormats.CommaSeparatedValue);
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.Html))
            {
                format = ClipboardContentType.Html;
                data = iData.GetData(System.Windows.Forms.DataFormats.Html);
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.StringFormat))
            {
                format = ClipboardContentType.PlainText;
                data = iData.GetData(System.Windows.Forms.DataFormats.StringFormat);
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.UnicodeText))
            {
                format = ClipboardContentType.UnicodeText;
                data = iData.GetData(System.Windows.Forms.DataFormats.UnicodeText);
                validDataType = true;

            }

            if (!validDataType) return;

            var handler = OnClipboardModified;
            if (handler != null)
            {
                handler(null, new ClipboardEventArgs(data, format));
            }
        }
    }
}
