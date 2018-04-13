using EventHook.Helpers;
using System;

namespace EventHook
{

    public class EventHookFactory : IDisposable
    {
        private SyncFactory syncFactory = new SyncFactory();

        public ApplicationWatcher GetApplicationWatcher()
        {
            return new ApplicationWatcher(syncFactory);
        }

        public KeyboardWatcher GetKeyboardWatcher()
        {
            return new KeyboardWatcher(syncFactory);
        }

        public MouseWatcher GetMouseWatcher()
        {
            return new MouseWatcher(syncFactory);
        }

        public ClipboardWatcher GetClipboardWatcher()
        {
            return new ClipboardWatcher(syncFactory);
        }

        public PrintWatcher GetPrintWatcher()
        {
            return new PrintWatcher(syncFactory);
        }

        public void Dispose()
        {
            syncFactory.Dispose();
        }
    }
}
