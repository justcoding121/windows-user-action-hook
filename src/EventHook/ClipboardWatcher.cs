using EventHook.Hooks;
using EventHook.Hooks.Helpers;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

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
    public class ClipboardWatcher
    {
        /*Clip board monitor*/
        private static ClipBoardHook _clip;
        private static BlockingCollection<object> _clipQueue;
        public static bool ClipRun;
        public static void Start()
        {
            Thread t = null;
            try
            {
                ClipRun = true;
                _clipQueue = new BlockingCollection<object>();
                _clip = new ClipBoardHook();
                _clip.RegisterClipboardViewer();
                _clip.ClipBoardChanged += ClipboardHandler;

                t = new Thread(ClipConsumer)
                {
                    Name = "ClipConsumer",
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest
                };
                t.Start();

            }
            catch
            {
                if (_clip != null)
                {
                    Stop();
                }

                if (t != null)
                    t.Abort();

            }


        }
        public static void Stop()
        {
            if (_clip != null)
            {
                _clip.ClipBoardChanged -= ClipboardHandler;
                _clip.UnregisterClipboardViewer();
                _clip.Dispose();
            }
            ClipRun = false;
            _clipQueue.Add(false);

        }
        private static void ClipboardHandler(object sender, EventArgs e)
        {

            _clipQueue.Add(sender);
        }
        private static void ClipConsumer()
        {
            while (ClipRun)
            {
                var item = _clipQueue.Take();
                if (item is bool) break;

                ClipboardHandler(item);
            }

        }
        private static byte[] ToBinary(object data)
        {
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, data);
                return memoryStream.ToArray();
            }
        }

        private static void ClipboardHandler(object sender)
        {
            var hWnd = WindowHelper.GetActiveWindowHandle();
            var appTitle = WindowHelper.GetWindowText(hWnd);
            var appName = WindowHelper.GetAppDescription(WindowHelper.GetAppPath(hWnd));

            System.Windows.Forms.IDataObject iData = (System.Windows.Forms.DataObject)sender;

            ClipboardContentTypes format = ClipboardContentTypes.PlainText;
            byte[] data = null;

            bool validDataType = false;
            if (iData.GetDataPresent(System.Windows.Forms.DataFormats.Text))
            {
                format = ClipboardContentTypes.PlainText;
                data = ToBinary(iData.GetData(System.Windows.Forms.DataFormats.Text));
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.Rtf))
            {
                format = ClipboardContentTypes.RichText;
                data = ToBinary(iData.GetData(System.Windows.Forms.DataFormats.Rtf));
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.CommaSeparatedValue))
            {
                format = ClipboardContentTypes.Csv;
                data = ToBinary(iData.GetData(System.Windows.Forms.DataFormats.CommaSeparatedValue));
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.Html))
            {
                format = ClipboardContentTypes.Html;
                data = ToBinary(iData.GetData(System.Windows.Forms.DataFormats.Html));
                validDataType = true;

            }

            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.StringFormat))
            {
                format = ClipboardContentTypes.PlainText;
                data = ToBinary(iData.GetData(System.Windows.Forms.DataFormats.StringFormat));
                validDataType = true;

            }
            else if (iData.GetDataPresent(System.Windows.Forms.DataFormats.UnicodeText))
            {
                format = ClipboardContentTypes.UnicodeText;
                data = ToBinary(iData.GetData(System.Windows.Forms.DataFormats.UnicodeText));
                validDataType = true;

            }
            if (!validDataType) return;
            // ReSharper disable once AssignNullToNotNullAttribute
            var url = Path.Combine(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, string.Concat(DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace("/", string.Empty).Replace(":", string.Empty), "_C.dat"));
            File.WriteAllBytes(url, data);

        }
    }
}
