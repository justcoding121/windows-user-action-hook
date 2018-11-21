using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EventHook.Helpers;
using EventHook.Hooks;

namespace EventHook
{
    /// <summary>
    ///     Type of clipboard content.
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
    ///     An argument send to user.
    /// </summary>
    public class ClipboardEventArgs : EventArgs
    {
        public object Data { get; set; }
        public ClipboardContentTypes DataFormat { get; set; }
    }

    /// <summary>
    ///     Wraps around clipboardHook.
    ///     Uses a producer-consumer pattern to improve performance and to avoid operating system forcing unhook on delayed
    ///     user callbacks.
    /// </summary>
    public class ClipboardWatcher
    {
        private readonly object accesslock = new object();

        private readonly SyncFactory factory;

        private ClipBoardHook clip;
        private AsyncConcurrentQueue<object> clipQueue;
        public bool isRunning;
        private CancellationTokenSource taskCancellationTokenSource;

        internal ClipboardWatcher(SyncFactory factory)
        {
            this.factory = factory;
        }

        public event EventHandler<ClipboardEventArgs> OnClipboardModified;

        /// <summary>
        ///     Start watching
        /// </summary>
        public void Start()
        {
            lock (accesslock)
            {
                if (!isRunning)
                {
                    taskCancellationTokenSource = new CancellationTokenSource();
                    clipQueue = new AsyncConcurrentQueue<object>(taskCancellationTokenSource.Token);

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
        ///     Stop watching
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
        ///     Add event to producer queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClipboardHandler(object sender, EventArgs e)
        {
            clipQueue.Enqueue(sender);
        }

        /// <summary>
        ///     Consume event from producer queue asynchronously
        /// </summary>
        /// <returns></returns>
        private async Task ClipConsumerAsync()
        {
            while (isRunning)
            {
                var item = await clipQueue.DequeueAsync();
                if (item is bool)
                {
                    break;
                }

                ClipboardHandler(item);
            }
        }

        /// <summary>
        ///     Actual handler to invoke user call backs
        /// </summary>
        /// <param name="sender"></param>
        private void ClipboardHandler(object sender)
        {
            IDataObject iData = (DataObject)sender;

            var format = default(ClipboardContentTypes);

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

            if (!validDataType)
            {
                return;
            }

            OnClipboardModified?.Invoke(null, new ClipboardEventArgs { Data = data, DataFormat = format });
        }
    }
}
