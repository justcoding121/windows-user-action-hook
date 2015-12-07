using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EventHook.Tests
{
    class Program
    {

        static void Main(string[] args)
        {
            KeyboardWatcher.Start();
            KeyboardWatcher.OnKeyInput += (s, e) =>
            {
                Console.WriteLine(e.KeyData.UnicodeCharacter);
            };

            MouseWatcher.Start();
            MouseWatcher.OnMouseInput += (s, e) =>
            {
                Console.WriteLine(e.Message.ToString());
            };

            ClipboardWatcher.Start();
            ClipboardWatcher.OnClipboardModified += (s, e) =>
                {
                    Console.WriteLine(e.DataFormat.ToString());
                    Console.WriteLine(e.Data);
                };

            ApplicationWatcher.Start();
            ApplicationWatcher.OnApplicationWindowChange += (s, e) =>
            {
                Console.WriteLine(e.ApplicationData.AppTitle);
                Console.WriteLine(e.Event);
            };

            Console.Read();

            KeyboardWatcher.Stop();
            MouseWatcher.Stop();
            ClipboardWatcher.Stop();
            ApplicationWatcher.Stop();
        }



    }
}
