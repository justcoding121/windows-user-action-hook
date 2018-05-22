## Windows User Action Hook

A one stop library for global windows user actions such mouse, keyboard, clipboard, &amp; print events

<a href="https://ci.appveyor.com/project/justcoding121/windows-user-action-hook">![Build Status](https://ci.appveyor.com/api/projects/status/htea647ukrgg4qcl?svg=true)</a>

Kindly report only issues/bugs here . For programming help or questions use [StackOverflow](http://stackoverflow.com/questions/tagged/windows-user-action-hook) with the tag EventHook or Windows-User-Action-Hook.

* [API Documentation](https://justcoding121.github.io/Windows-User-Action-Hook/api/EventHook.html)

## Supported Events

* Keyboard events
* Mouse events
* clipboard events
* application events
* print events

## Usage

Install by [nuget](https://www.nuget.org/packages/EventHook)

    Install-Package EventHook

## Sample Code:

```csharp
using (var eventHookFactory = new EventHookFactory())
{
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

    //waiting here to keep this thread running           
    Console.Read();

    //stop watching
    keyboardWatcher.Stop();
    mouseWatcher.Stop();
    clipboardWatcher.Stop();
    applicationWatcher.Stop();
    printWatcher.Stop();
}
```

![alt tag](https://raw.githubusercontent.com/justcoding121/Windows-User-Action-Hook/stable/EventHook.Examples/EventHook.ConsoleApp.Example/Capture.PNG)
