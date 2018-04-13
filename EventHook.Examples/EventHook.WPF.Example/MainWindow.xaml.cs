using System;
using System.Diagnostics;
using System.Windows;

namespace EventHook.WPF.Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var keyboardWatcher = new KeyboardWatcher();
            keyboardWatcher.Start();
            keyboardWatcher.OnKeyInput += (s, e) =>
            {
                Debug.WriteLine(string.Format("Key {0} event of key {1}", e.KeyData.EventType, e.KeyData.Keyname));
            };

            var mouseWatcher = new MouseWatcher();
            mouseWatcher.Start();
            mouseWatcher.OnMouseInput += (s, e) =>
            {
                Debug.WriteLine(string.Format("Mouse event {0} at point {1},{2}", e.Message.ToString(), e.Point.x, e.Point.y));
            };

            var clipboardWatcher = new ClipboardWatcher();
            clipboardWatcher.Start();
            clipboardWatcher.OnClipboardModified += (s, e) =>
            {
                Debug.WriteLine(string.Format("Clipboard updated with data '{0}' of format {1}", e.Data, e.DataFormat.ToString()));
            };

            var applicationWatcher = new ApplicationWatcher();
            applicationWatcher.Start();
            applicationWatcher.OnApplicationWindowChange += (s, e) =>
            {
                Debug.WriteLine(string.Format("Application window of '{0}' with the title '{1}' was {2}", e.ApplicationData.AppName, e.ApplicationData.AppTitle, e.Event));
            };

            var printWatcher = new PrintWatcher();
            printWatcher.Start();
            printWatcher.OnPrintEvent += (s, e) =>
            {
                Debug.WriteLine(string.Format("Printer '{0}' currently printing {1} pages.", e.EventData.PrinterName, e.EventData.Pages));
            };

        }
    }
}
