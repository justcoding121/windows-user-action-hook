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

namespace EventHook.WinForms.Example
{
    public partial class MainForm : Form
    {
        KeyboardWatcher keyboardWatcher = new KeyboardWatcher();
        MouseWatcher mouseWatcher = new MouseWatcher();
        ClipboardWatcher clipboardWatcher = new ClipboardWatcher();
        ApplicationWatcher applicationWatcher = new ApplicationWatcher();
        PrintWatcher printWatcher = new PrintWatcher();

        public MainForm()
        {
            InitializeComponent();

            keyboardWatcher.Start();
            keyboardWatcher.OnKeyInput += (s, e) =>
            {
                Debug.WriteLine(string.Format("Key {0} event of key {1}", e.KeyData.EventType, e.KeyData.Keyname));
            };


            mouseWatcher.Start();
            mouseWatcher.OnMouseInput += (s, e) =>
            {
                Debug.WriteLine(string.Format("Mouse event {0} at point {1},{2}", e.Message.ToString(), e.Point.x, e.Point.y));
            };


            clipboardWatcher.Start();
            clipboardWatcher.OnClipboardModified += (s, e) =>
            {
                Debug.WriteLine(string.Format("Clipboard updated with data '{0}' of format {1}", e.Data, e.DataFormat.ToString()));
            };


            applicationWatcher.Start();
            applicationWatcher.OnApplicationWindowChange += (s, e) =>
            {
                Debug.WriteLine(string.Format("Application window of '{0}' with the title '{1}' was {2}", e.ApplicationData.AppName, e.ApplicationData.AppTitle, e.Event));
            };


            //printWatcher.Start();
            //printWatcher.OnPrintEvent += (s, e) =>
            //{
            //    Debug.WriteLine(string.Format("Printer '{0}' currently printing {1} pages.", e.EventData.PrinterName, e.EventData.Pages));
            //};

            keyboardWatcher.Stop();
            mouseWatcher.Stop();
            clipboardWatcher.Stop();
            applicationWatcher.Stop();
            //printWatcher.Stop();

        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
           
        }
    }
}
