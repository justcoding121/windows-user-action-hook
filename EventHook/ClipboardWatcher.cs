using EventHook.Hooks;
using EventHook.Hooks.Helpers;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace EventHook
{
    public enum ClipboardContentTypes
    {
        PlainText = 0,
        RichText = 1,
        Html = 2,
        Csv = 3,
        UnicodeText = 4
    }

    public class ClipboardEventArgs : EventArgs
    {
        public object Data { get; set; }
        public ClipboardContentTypes DataFormat { get; set; }
    }

    public class ClipboardWatcher
    {
        /*Clip board monitor*/
        public static bool _IsRunning;
        private static object _Accesslock = new object();

        private static ClipBoardHook _clip;
        private static AsyncCollection<object> _clipQueue;

        public static event EventHandler<ClipboardEventArgs> OnClipboardModified;

        public static void Start()
        {
            if (!_IsRunning)
                lock (_Accesslock)
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

                        _IsRunning = true;

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
            if (_IsRunning)
                lock (_Accesslock)
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

                    _IsRunning = false;
                    _clipQueue.Add(false);
                }

        }
        private static void ClipboardHandler(object sender, EventArgs e)
        {
            _clipQueue.Add(sender);
        }

        private static async Task ClipConsumerAsync()
        {
            while (_IsRunning)
            {
                var item = await _clipQueue.TakeAsync();
                if (item is bool) break;

                ClipboardHandler(item);


            }

        }

        private static void ClipboardHandler(object sender)
        {

            System.Windows.Forms.IDataObject iData = (System.Windows.Forms.DataObject)sender;

            ClipboardContentTypes format = default(ClipboardContentTypes);

            object data = null;

            bool validDataType = false;
            if (iData.GetDataPresent(System.Windows.Forms.DataFormats.Text))
            {
                format = ClipboardContentTypes.PlainText;
                data = iData.GetData(System.Windows.Forms.DataFormats.Text);
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.Rtf))
            {
                format = ClipboardContentTypes.RichText;
                data = iData.GetData(System.Windows.Forms.DataFormats.Rtf);
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.CommaSeparatedValue))
            {
                format = ClipboardContentTypes.Csv;
                data = iData.GetData(System.Windows.Forms.DataFormats.CommaSeparatedValue);
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.Html))
            {
                format = ClipboardContentTypes.Html;
                data = iData.GetData(System.Windows.Forms.DataFormats.Html);
                validDataType = true;

            }

            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.StringFormat))
            {
                format = ClipboardContentTypes.PlainText;
                data = iData.GetData(System.Windows.Forms.DataFormats.StringFormat);
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.UnicodeText))
            {
                format = ClipboardContentTypes.UnicodeText;
                data = iData.GetData(System.Windows.Forms.DataFormats.UnicodeText);
                validDataType = true;

            }

            if (!validDataType) return;

            EventHandler<ClipboardEventArgs> handler = OnClipboardModified;
            if (handler != null)
            {
                handler(null, new ClipboardEventArgs() { Data = data, DataFormat = format });
            }

        }
    }
}
