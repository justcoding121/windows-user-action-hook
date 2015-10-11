using EventHook.Hooks;
using EventHook.Hooks.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Office.Interop.Outlook;

namespace EventHook
{
    public enum EmailTypes
    {
        Inbox = 0,
        Outbox = 1
    }
    public class EmailEvent
    {
        public DateTime EventDateTime { get; set; }
        public List<EmailLogEmailAddress> To { get; set; }
        public string Subject { get; set; }
        public List<EmailLogEmailAddress> Bcc { get; set; }
        public List<EmailLogEmailAddress> Cc { get; set; }
        public string FolderName { get; set; }
        public string From { get; set; }
        public EmailTypes EmailType { get; set; }

    }
    public class EmailLogEmailAddress
    {
        public long Id { get; set; }
        public string EmailAddress { get; set; }

        public virtual EmailEvent EmailLog { get; set; }
    }
  public  class EmailClientWatcher
    {
        private static System.Timers.Timer LoggerTimer { get; set; }
        private static DateTime CheckEmailsFromThisDateTime { get; set; }
        private static bool IsBusy { get; set; }
        static BlockingCollection<object> _appQueue;
        static bool _emailRun;
        static bool _safeToReadEmail;
        public static void Start()
        {
            try
            {
                if (LoggerTimer == null)
                    LoggerTimer = new System.Timers.Timer();

                LoggerTimer.Interval = (1000 * 10);
                LoggerTimer.Elapsed += ReadEmails;
                LoggerTimer.Start();

                _appQueue = new BlockingCollection<object>();

                WindowHook.WindowActivated += WindowActivated;
                _emailRun = true;
                var emailThread = new Thread(AppConsumer)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                };
                emailThread.Start();
            }
            catch { Stop(); }

        }
        public static void Stop()
        {
            _emailRun = false;
            _appQueue.Add(false);

            if (LoggerTimer != null)
                LoggerTimer.Elapsed -= ReadEmails;
            if (LoggerTimer != null) LoggerTimer.Stop();
        }
        static void WindowActivated(ShellHook shellObject, IntPtr hWnd)
        {
            _appQueue.Add(new WindowData() { HWnd = hWnd, EventType = 1 });
        }
        // This is the method to run when the timer is raised. 
        static private void AppConsumer()
        {
            while (_emailRun)
            {

                //blocking here until a key is added to the queue
                var item = _appQueue.Take();
                if (item is bool) break;

                var wnd = (WindowData)item;
                if (wnd.EventType == 1)
                {
                    WindowActivated(wnd);
                }
            }
        }


        static void WindowActivated(WindowData wnd)
        {
            string path = WindowHelper.GetAppPath(wnd.HWnd);
            if (path != null)
            {
                var bHappname = Path.GetFileName(path);
                if (bHappname.ToLower().Trim() == "outlook.exe")
                {
                    _safeToReadEmail = false;
                }
                else if (wnd.HWnd != IntPtr.Zero)
                {
                    _safeToReadEmail = true;
                }
            }

        }

        private static void ReadEmails(object sender, System.Timers.ElapsedEventArgs elapsed)
        {

            if (!IsBusy)
            {
                IsBusy = true;

                Application app = null;
                _NameSpace ns = null;

                try
                {


                    app = Marshal.GetActiveObject("Outlook.Application") as Microsoft.Office.Interop.Outlook.Application;
                    if (app != null) ns = app.GetNamespace("MAPI");
                    if (ns != null)
                    {
                        ns.Logon(string.Empty, string.Empty, Missing.Value, Missing.Value);

                        if (_safeToReadEmail)
                            foreach (Folder f in ns.Folders)
                                EnumerateFolders(f);
                    }
                }
                catch (COMException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    ns = null;
                    app = null;
                }
                IsBusy = false;
            }

        }

        // Uses recursion to enumerate Outlook subfolders.
        private static void EnumerateFolders(Folder folder)
        {
            var childFolders = folder.Folders;
            if (childFolders.Count > 0)
            {
                foreach (Folder childFolder in childFolders)
                {
                    var isvalid = false;
                    var msgType = EmailTypes.Inbox;
                    switch (childFolder.Name.ToLower())
                    {
                        case "inbox":
                        case "received items":
                            isvalid = true;
                            break;
                        case "outbox":
                        case "sent items":
                        case "sent mail":
                            isvalid = true;
                            msgType = EmailTypes.Outbox;
                            break;
                    }


                    if (isvalid && _safeToReadEmail)
                        foreach (var item in from mail in childFolder.Items.Cast<object>().TakeWhile(mail => _safeToReadEmail != false) where (mail as Microsoft.Office.Interop.Outlook.MailItem) != null select (Microsoft.Office.Interop.Outlook.MailItem)mail into item let msgTime = msgType == EmailTypes.Inbox ? item.ReceivedTime : item.CreationTime where msgTime >= CheckEmailsFromThisDateTime select item)
                        {
                            var record = PrepareEmail(item, msgType, childFolder.Name);

                            if (record == null) continue;
                            var url = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, string.Concat(record.EventDateTime.ToString().Replace("/", string.Empty).Replace(":", string.Empty), "_E.msg"));
                            item.SaveAs(url);
                            //using (var Localcontext = new LogDataContext())
                            //{
                            //    if (Localcontext.EmailEvents.Where(x => x.EmailType == record.EmailType && x.EventDateTime == record.EventDateTime).FirstOrDefault() == null)
                            //    {
                            //        Localcontext.EmailEvents.Add(record);
                            //        Localcontext.SaveChanges();
                            //    }
                                           
                            //}
                        }
                    if (_safeToReadEmail == false)
                        break;
                    EnumerateFolders(childFolder);
                }
            }
        }


        private static EmailEvent PrepareEmail(_MailItem item, EmailTypes msgType, string folderName)
        {
            var toAddress = new List<EmailLogEmailAddress>();
            try
            {
                var msgTime = msgType == EmailTypes.Inbox ? item.ReceivedTime : item.CreationTime;


                if (item.To != null)
                    toAddress.AddRange(item.To.Split(';').Select(to => new EmailLogEmailAddress()
                    {
                        EmailAddress = to
                    }));
                var cc = new List<EmailLogEmailAddress>();
                if (item.CC != null)
                    cc.AddRange(item.CC.Split(';').Select(to => new EmailLogEmailAddress()
                    {
                        EmailAddress = to
                    }));
                var bcc = new List<EmailLogEmailAddress>();
                if (item.BCC != null)
                    bcc.AddRange(item.BCC.Split(';').Select(bc => new EmailLogEmailAddress()
                    {
                        EmailAddress = bc
                    }));
                EmailEvent record = new EmailEvent()
                {
                    EmailType = msgType,
                    EventDateTime = msgTime,
                    From = item.SenderEmailAddress,
                    To = toAddress,
                    Subject = item.Subject,
                    Bcc = bcc,
                    Cc = cc,
                    FolderName = folderName
                };
                return record;
            }
            catch
            {
                // ignored
            }
            return null;
        }

    }
}
