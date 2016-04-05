using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EventHook.Forms.Tests
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            KeyboardWatcher.Start();
            KeyboardWatcher.OnKeyInput += (s, e) =>
            {
                Debug.WriteLine(string.Format("Key {0} event of key {1}", e.KeyData.EventType, e.KeyData.Keyname));
            };

            MouseWatcher.Start();
            MouseWatcher.OnMouseInput += (s, e) =>
            {
                Debug.WriteLine(string.Format("Mouse event {0} at point {1},{2}", e.Message.ToString(), e.Point.x, e.Point.y));
            };

            ClipboardWatcher.Start();
            ClipboardWatcher.OnClipboardModified += (s, e) =>
            {
                Debug.WriteLine(string.Format("Clipboard updated with data '{0}' of format {1}", e.Data, e.DataFormat.ToString()));
            };

            ApplicationWatcher.Start();
            ApplicationWatcher.OnApplicationWindowChange += (s, e) =>
            {
                Debug.WriteLine(string.Format("Application window of '{0}' with the title '{1}' was {2}", e.ApplicationData.AppName, e.ApplicationData.AppTitle, e.Event));
            };

            PrintWatcher.Start();
            PrintWatcher.OnPrintEvent += (s, e) =>
            {
                Debug.WriteLine(string.Format("Printer '{0}' currently printing {1} pages.", e.EventData.PrinterName, e.EventData.Pages));
            };

        }
    }
}
