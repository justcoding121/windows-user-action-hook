using EventHook.Hooks;
using EventHook.Hooks.Helpers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Windows.Automation;

namespace EventHook
{
    public enum FileSystemEventObjectTypes
    {
        File = 0,
        Folder = 1
    }
    public enum FileSystemEvents
    {
        Created = 0,
        Deleted = 1,
        Modified = 2,
        Renamed = 3,
        Visited = 4
    }
    public class FileActivityEvent
    {

        public DateTime EventDateTime { get; set; }
        public FileSystemEventObjectTypes ObjectType { get; set; }
        public FileSystemEvents FileSystemEvent { get; set; }
        public string OldName { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
    }


    public class FileWatcher
    {
        /*File history monitor*/

        public static bool FileRun;
        private static BlockingCollection<object> _fileQueue;
        public static void Start()
        {
            try
            {
                _fileQueue = new BlockingCollection<object>();
                WindowHook.WindowActivated += FileWindowActivated;
                FileRun = true;
                var t = new Thread(FileHistoryConsumer)
                {
                    Name = "FileHistoryConsumer",
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest
                };
                t.Start();
            }
            catch
            {
                Stop();
            }

        }
        public static void Stop()
        {

            FileRun = false;
            _fileQueue.Add(false);

        }
        private class FileHistoryData
        {
            public IntPtr HWnd;
            public int EventType;
        }

        // This is the method to run when the timer is raised. 
        private static void FileHistoryConsumer()
        {

            while (FileRun)
            {

                //blocking here until a key is added to the queue
                var item = _fileQueue.Take();
                if (item is bool) break;


                var wnd = (FileHistoryData)item;

                switch (wnd.EventType)
                {

                    case 1:
                        FileWindowActivated(wnd.HWnd);
                        break;
                }

            }


            if (_watchFile != null & _watchFolder != null)
            {
                _watchFolder.EnableRaisingEvents = false;
                _watchFolder.Dispose();
                _watchFile.EnableRaisingEvents = false;
                _watchFile.Dispose();

                Automation.RemoveAllEventHandlers();
            }
        }
        private static void FileWindowActivated(ShellHook shellObject, IntPtr hWnd)
        {

            _fileQueue.Add(new FileHistoryData() { HWnd = hWnd, EventType = 1 });
        }


        private static void FileWindowActivated(IntPtr hWnd)
        {


            string appname = Path.GetFileName(WindowHelper.GetAppPath(hWnd));
            if (appname != null)
                if (appname.ToLower().Trim() != "explorer.exe")
                {
                    if (_watchFolder != null && _watchFile != null)
                    {
                        _watchFolder.EnableRaisingEvents = false;
                        _watchFile.EnableRaisingEvents = false;
                    }
                }

                else
                {
                    AutomationHook t = new AutomationHook();
                    AutomationElement addressBox = t.GetUrlFromExplorerWithIdentifier("7", hWnd);
                    if (addressBox != null)
                    {
                        StartMonitoringFolder(GetTextFromBox(addressBox, "7"));
                        SubscribePropertyChange(addressBox);
                    }

                }

        }


        private static string GetTextFromBox(AutomationElement elementNode, string windowsVersion)
        {
            if (elementNode == null) return null;

            try
            {
                var value = elementNode.GetCurrentPropertyValue(AutomationElement.NameProperty) as string;


                string s = GetUrl(value, windowsVersion);
                Console.WriteLine(s);
                return s;
            }
            catch { return null; }



        }
        private static string GetUrl(string value, string windowsVersion)
        {

            string url = value;
            switch (windowsVersion)
            {

                case "7":
                    value = value.Replace("Address:", "").TrimStart();
                    switch (value.ToLower())
                    {
                        case "favourites":
                            return Environment.GetFolderPath(Environment.SpecialFolder.Favorites);

                        case "desktop":
                            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        case "downloads":
                            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                        case "recent places":
                            return Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                        case @"libraries\documents":
                            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        case @"libraries\music":
                            return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                        case @"libraries\pictures":
                            return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                        case @"libraries\videos":
                            return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                        default:
                            if (value == Environment.UserName)
                                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                            break;

                    }
                    break;
            }
            return url;
        }

        public static AutomationPropertyChangedEventHandler PropChangeHandler;

        public static void SubscribePropertyChange(AutomationElement element)
        {
            Automation.AddAutomationPropertyChangedEventHandler(element,
                TreeScope.Element,
                PropChangeHandler = new AutomationPropertyChangedEventHandler(OnPropertyChange),
                AutomationElement.NameProperty);

        }


        private static void OnPropertyChange(object src, AutomationPropertyChangedEventArgs e)
        {
            if (Equals(e.Property, AutomationElement.NameProperty))
            {
                StartMonitoringFolder(GetUrl(e.NewValue.ToString(), "7"));
            }
        }


        static FileSystemWatcher _watchFolder, _watchFile;
        private static void StartMonitoringFolder(string path)
        {
            if (path == null) return;
            path = path.Replace("Address:", "").TrimStart();

            if (Directory.Exists(path))

                if (_watchFolder == null && _watchFile == null)
                {
                    _watchFolder = new FileSystemWatcher
                    {
                        Path = path,
                        NotifyFilter = System.IO.NotifyFilters.DirectoryName | System.IO.NotifyFilters.Attributes
                    };
                    _watchFolder.Changed += new FileSystemEventHandler(EventRaised);
                    _watchFolder.Created += new FileSystemEventHandler(EventRaised);
                    _watchFolder.Deleted += new FileSystemEventHandler(EventRaised);
                    _watchFolder.Renamed += new System.IO.RenamedEventHandler(EventRenameRaised);

                    _watchFile = new FileSystemWatcher
                    {
                        Path = path,
                        NotifyFilter = System.IO.NotifyFilters.FileName | System.IO.NotifyFilters.Attributes
                    };
                    _watchFile.Changed += new FileSystemEventHandler(EventRaised);
                    _watchFile.Created += new FileSystemEventHandler(EventRaised);
                    _watchFile.Deleted += new FileSystemEventHandler(EventRaised);
                    _watchFile.Renamed += new System.IO.RenamedEventHandler(EventRenameRaised);

                    try
                    {
                        _watchFolder.EnableRaisingEvents = true;
                        _watchFile.EnableRaisingEvents = true;
                    }
                    catch { }

                }
                else
                {
                    if (_watchFolder != null)
                    {
                        _watchFolder.EnableRaisingEvents = false;
                        _watchFolder.Path = path;
                        _watchFile.EnableRaisingEvents = false;
                        _watchFile.Path = path;

                        try
                        {
                            _watchFolder.EnableRaisingEvents = true;
                            _watchFile.EnableRaisingEvents = true;
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

            SaveFileActivity(path);
        }



        private static void EventRaised(object sender, System.IO.FileSystemEventArgs e)
        {
            FileSystemEventObjectTypes type;
            if (sender == _watchFile)
                type = FileSystemEventObjectTypes.File;
            else
                type = FileSystemEventObjectTypes.Folder;

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    FileRecord(FileSystemEvents.Modified, e.FullPath, e.Name, type);
                    break;
                case WatcherChangeTypes.Created:
                    FileRecord(FileSystemEvents.Created, e.FullPath, e.Name, type);

                    break;
                case WatcherChangeTypes.Deleted:
                    FileRecord(FileSystemEvents.Deleted, e.FullPath, e.Name, type);
                    break;

                default:
                    break;
            }
        }
        private static void FileRecord(FileSystemEvents action, string path, string name, FileSystemEventObjectTypes objType)
        {


            var hWnd = WindowHelper.GetActiveWindowHandle();
            var appTitle = WindowHelper.GetWindowText(hWnd);
            var appName = WindowHelper.GetAppDescription(WindowHelper.GetAppPath(hWnd));

            FileActivityEvent record = new FileActivityEvent()
            {

                EventDateTime = DateTime.Now,
                FileSystemEvent = action,
                ObjectType = objType,
                Location = path,
                Name = name

            };



        }

        public static void EventRenameRaised(object sender, System.IO.RenamedEventArgs e)
        {

            FileSystemEventObjectTypes fType;

            fType = sender != _watchFile ? FileSystemEventObjectTypes.Folder : FileSystemEventObjectTypes.File;

            var hWnd = WindowHelper.GetActiveWindowHandle();
            var appTitle = WindowHelper.GetWindowText(hWnd);
            var appName = WindowHelper.GetAppDescription(WindowHelper.GetAppPath(hWnd));

         

                var record = new FileActivityEvent()
                {

                    EventDateTime = DateTime.Now,
                    FileSystemEvent = FileSystemEvents.Renamed,
                    Location = e.FullPath,
                    Name = e.Name,
                    OldName = e.OldName,
                    ObjectType = fType

                };


            


        }

        private static void SaveFileActivity(string path)
        {
            var hWnd = WindowHelper.GetActiveWindowHandle();
            var appTitle = WindowHelper.GetWindowText(hWnd);
            var appName = WindowHelper.GetAppDescription(WindowHelper.GetAppPath(hWnd));

            var record = new FileActivityEvent()
            {
                EventDateTime = DateTime.Now,
                FileSystemEvent = FileSystemEvents.Visited,
                Location = path,
                ObjectType = FileSystemEventObjectTypes.Folder

            };

        }
    }
}
