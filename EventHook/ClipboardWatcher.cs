using EventHook.Hooks;
using EventHook.Helpers;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EventHook
{
    /// <summary>
    /// Type of clipboard content
    /// </summary>
    public enum ClipboardContentTypes
    {
        PlainText = 0,
        RichText = 1,
        Html = 2,
        Csv = 3,
        UnicodeText = 4
    }

    /// <summary>
    /// An argument send to user
    /// </summary>
    public class ClipboardEventArgs : EventArgs
    {
        public object Data { get; set; }
        public ClipboardContentTypes DataFormat { get; set; }
    }

    /// <summary>
    /// Wraps around clipboardHook
    /// Uses a producer-consumer pattern to improve performance and to avoid operating system forcing unhook on delayed user callbacks
    /// </summary>
    public class ClipboardWatcher
    {
        /*Clip board monitor*/
        public static bool isRunning;
        private static object accesslock = new object();

        private static ClipBoardHook clip;
        private static AsyncCollection<object> clipQueue;

        public static event EventHandler<ClipboardEventArgs> OnClipboardModified;

        /// <summary>
        /// Start watching
        /// </summary>
        public static void Start()
        {
            if (!isRunning)
            {
                lock (accesslock)
                {
                    try
                    {
                        clipQueue = new AsyncCollection<object>();

                        //Low level hooks need to be run in the context of a UI thread
                        Task.Factory.StartNew(() => { }).ContinueWith(x =>
                        {
                            clip = new ClipBoardHook();
                            clip.RegisterClipboardViewer();
                            clip.ClipBoardChanged += ClipboardHandler;

                        }, SharedMessagePump.GetTaskScheduler());

                        Task.Factory.StartNew(() => ClipConsumerAsync());

                        isRunning = true;

                    }
                    catch
                    {
                        if (clip != null)
                        {
                            Stop();
                        }
                    }
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
                    if (clip != null)
                    {
                        Task.Factory.StartNew(() => { }).ContinueWith(x =>
                        {
                            clip.ClipBoardChanged -= ClipboardHandler;
                            clip.UnregisterClipboardViewer();
                            clip.Dispose();

                        }, SharedMessagePump.GetTaskScheduler());
                    }

                    isRunning = false;
                    clipQueue.Add(false);
                }
            }

        }

        /// <summary>
        /// Add event to producer queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ClipboardHandler(object sender, EventArgs e)
        {
            clipQueue.Add(sender);
        }

        /// <summary>
        /// Consume event from producer queue asynchronously
        /// </summary>
        /// <returns></returns>
        private static async Task ClipConsumerAsync()
        {
            while (isRunning)
            {
                var item = await clipQueue.TakeAsync();
                if (item is bool) break;

                ClipboardHandler(item);
            }

        }

        /// <summary>
        /// Actual handler to invoke user call backs
        /// </summary>
        /// <param name="sender"></param>
        private static void ClipboardHandler(object sender)
        {
            IDataObject iData = (DataObject)sender;

            ClipboardContentTypes format = default(ClipboardContentTypes);

            object data = null;

            bool validDataType = false;
            if (iData.GetDataPresent(DataFormats.Text))
            {
                format = ClipboardContentTypes.PlainText;
                data = iData.GetData(DataFormats.Text);
                validDataType = true;

            }
            else if (iData.GetDataPresent(DataFormats.Rtf))
            {
                format = ClipboardContentTypes.RichText;
                data = iData.GetData(DataFormats.Rtf);
                validDataType = true;

            }
            else if (iData.GetDataPresent(DataFormats.CommaSeparatedValue))
            {
                format = ClipboardContentTypes.Csv;
                data = iData.GetData(DataFormats.CommaSeparatedValue);
                validDataType = true;

            }
            else if (iData.GetDataPresent(DataFormats.Html))
            {
                format = ClipboardContentTypes.Html;
                data = iData.GetData(DataFormats.Html);
                validDataType = true;

            }

            else if (iData.GetDataPresent(DataFormats.StringFormat))
            {
                format = ClipboardContentTypes.PlainText;
                data = iData.GetData(DataFormats.StringFormat);
                validDataType = true;

            }
            else if (iData.GetDataPresent(DataFormats.UnicodeText))
            {
                format = ClipboardContentTypes.UnicodeText;
                data = iData.GetData(DataFormats.UnicodeText);
                validDataType = true;

            }

            if (!validDataType) return;

            OnClipboardModified?.Invoke(null, new ClipboardEventArgs() { Data = data, DataFormat = format });

        }
    }
}
