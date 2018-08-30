using System;
using EventHook.Helpers;

namespace EventHook
{
    /// <summary>
    /// A factory class core to the management of various watchers 
    /// that all shares the same synchronization objects.
    /// Use this class to get instances of differant watchers.
    /// This factory instance should be disposed only after all watchers it have been unsubscribed.
    /// </summary>
    public class EventHookFactory : IDisposable
    {
        private readonly SyncFactory syncFactory = new SyncFactory();

        public void Dispose()
        {
            syncFactory.Dispose();
        }

        /// <summary>
        /// Get an instance of application watcher.
        /// </summary>
        /// <returns></returns>
        public ApplicationWatcher GetApplicationWatcher()
        {
            return new ApplicationWatcher(syncFactory);
        }

        /// <summary>
        /// Get an instance of keystroke watcher.
        /// </summary>
        /// <returns></returns>
        public KeyboardWatcher GetKeyboardWatcher()
        {
            return new KeyboardWatcher(syncFactory);
        }

        /// <summary>
        /// Get an instance of mouse watcher.
        /// </summary>
        /// <returns></returns>
        public MouseWatcher GetMouseWatcher()
        {
            return new MouseWatcher(syncFactory);
        }

        /// <summary>
        /// Get an instance of clipboard watcher.
        /// </summary>
        /// <returns></returns>
        public ClipboardWatcher GetClipboardWatcher()
        {
            return new ClipboardWatcher(syncFactory);
        }

        /// <summary>
        /// Get an instance of print watcher.
        /// </summary>
        /// <returns></returns>
        public PrintWatcher GetPrintWatcher()
        {
            return new PrintWatcher(syncFactory);
        }
    }
}
