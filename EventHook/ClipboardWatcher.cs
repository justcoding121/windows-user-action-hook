using EventHook.Hooks;
using EventHook.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

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
        private object accesslock = new object();
        public bool isRunning;

        private SyncFactory factory;
        private AsyncQueue<object> clipQueue;
        private CancellationTokenSource taskCancellationTokenSource;

        private ClipBoardHook clip;
        public event EventHandler<ClipboardEventArgs> OnClipboardModified;

        internal ClipboardWatcher(SyncFactory factory)
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
                    clipQueue = new AsyncQueue<object>(taskCancellationTokenSource.Token);

                    //This needs to run on UI thread context
                    //So use task factory with the shared UI message pump thread
                    Task.Factory.StartNew(() =>
                    {
                        clip = new ClipBoardHook();
                        clip.RegisterClipboardViewer();
                        clip.ClipBoardChanged += ClipboardHandler;
                    },
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    factory.GetTaskScheduler()).Wait();

                    Task.Factory.StartNew(() => ClipConsumerAsync());

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
                    if (clip != null)
                    {
                        //This needs to run on UI thread context
                        //So use task factory with the shared UI message pump thread
                        Task.Factory.StartNew(() =>
                        {
                            clip.ClipBoardChanged -= ClipboardHandler;
                            clip.UnregisterClipboardViewer();
                            clip.Dispose();
                        },
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        factory.GetTaskScheduler());
                    }

                    isRunning = false;
                    clipQueue.Enqueue(false);
                    taskCancellationTokenSource.Cancel();
                }
            }

        }

        /// <summary>
        /// Add event to producer queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClipboardHandler(object sender, EventArgs e)
        {
            clipQueue.Enqueue(sender);
        }

        /// <summary>
        /// Consume event from producer queue asynchronously
        /// </summary>
        /// <returns></returns>
        private async Task ClipConsumerAsync()
        {
            while (isRunning)
            {
                var item = await clipQueue.DequeueAsync();
                if (item is bool) break;

                ClipboardHandler(item);
            }

        }

        /// <summary>
        /// Actual handler to invoke user call backs
        /// </summary>
        /// <param name="sender"></param>
        private void ClipboardHandler(object sender)
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
