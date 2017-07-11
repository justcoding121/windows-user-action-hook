## Windows User Action Hook

A one stop library for global windows user actions such mouse, keyboard, clipboard, &amp; print events

<a href="https://ci.appveyor.com/project/justcoding121/windows-user-action-hook">![Build Status](https://ci.appveyor.com/api/projects/status/htea647ukrgg4qcl?svg=true)</a>

Kindly report only issues/bugs here . For programming help or questions use [StackOverflow](http://stackoverflow.com/questions/tagged/windows-user-action-hook) with the tag EventHook or Windows-User-Action-Hook.

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
KeyboardWatcher.Start();
KeyboardWatcher.OnKeyInput += (s, e) =>
{
    Console.WriteLine(string.Format("Key {0} event of key {1}", e.KeyData.EventType, e.KeyData.Keyname));
};

MouseWatcher.Start();
MouseWatcher.OnMouseInput += (s, e) =>
{
    Console.WriteLine(string.Format("Mouse event {0} at point {1},{2}", e.Message.ToString(), e.Point.x, e.Point.y));
};

ClipboardWatcher.Start();
ClipboardWatcher.OnClipboardModified += (s, e) =>
{
    Console.WriteLine(string.Format("Clipboard updated with data '{0}' of format {1}", e.Data, e.DataFormat.ToString()));
};

ApplicationWatcher.Start();
ApplicationWatcher.OnApplicationWindowChange += (s, e) =>
{
    Console.WriteLine(string.Format("Application window of '{0}' with the title '{1}' was {2}", e.ApplicationData.AppName, e.ApplicationData.AppTitle, e.Event));
};

PrintWatcher.Start();
PrintWatcher.OnPrintEvent += (s, e) =>
{
    Console.WriteLine(string.Format("Printer '{0}' currently printing {1} pages.", e.EventData.PrinterName, e.EventData.Pages));
};

Console.Read();

KeyboardWatcher.Stop();
MouseWatcher.Stop();
ClipboardWatcher.Stop();
ApplicationWatcher.Stop();
PrintWatcher.Stop(); 
```

![alt tag](https://raw.githubusercontent.com/justcoding121/Windows-User-Action-Hook/stable/EventHook.Examples/EventHook.ConsoleApp.Example/Capture.PNG)
