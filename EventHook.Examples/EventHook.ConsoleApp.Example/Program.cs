﻿using System;

namespace EventHook.ConsoleApp.Example
{
    class Program
    {

        static void Main(string[] args)
        {
            var eventHookFactory = new EventHookFactory();

            var keyboardWatcher = eventHookFactory.GetKeyboardWatcher();
            keyboardWatcher.Start();
            keyboardWatcher.OnKeyInput += (s, e) =>
            {
                Console.WriteLine(string.Format("Key {0} event of key {1}", e.KeyData.EventType, e.KeyData.Keyname));
            };

            var mouseWatcher = eventHookFactory.GetMouseWatcher();
            mouseWatcher.Start();
            mouseWatcher.OnMouseInput += (s, e) =>
            {
                Console.WriteLine(string.Format("Mouse event {0} at point {1},{2}", e.Message.ToString(), e.Point.x, e.Point.y));
            };

            var clipboardWatcher = eventHookFactory.GetClipboardWatcher();
            clipboardWatcher.Start();
            clipboardWatcher.OnClipboardModified += (s, e) =>
            {
                Console.WriteLine(string.Format("Clipboard updated with data '{0}' of format {1}", e.Data, e.DataFormat.ToString()));
            };


            var applicationWatcher = eventHookFactory.GetApplicationWatcher();
            applicationWatcher.Start();
            applicationWatcher.OnApplicationWindowChange += (s, e) =>
            {
                Console.WriteLine(string.Format("Application window of '{0}' with the title '{1}' was {2}", e.ApplicationData.AppName, e.ApplicationData.AppTitle, e.Event));
            };

            var printWatcher = eventHookFactory.GetPrintWatcher();
            printWatcher.Start();
            printWatcher.OnPrintEvent += (s, e) =>
            {
                Console.WriteLine(string.Format("Printer '{0}' currently printing {1} pages.", e.EventData.PrinterName, e.EventData.Pages));
            };

           
            Console.Read();
            eventHookFactory.Dispose();
        }

    }
}
