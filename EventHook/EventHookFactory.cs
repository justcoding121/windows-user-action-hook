using EventHook.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventHook
{
    public class EventHookFactory : IDisposable
    {
        private SyncFactory syncFactory = new SyncFactory();

        private ApplicationWatcher applicationWatcher;
        private KeyboardWatcher keyboardWatcher;
        private MouseWatcher mouseWatcher;
        private ClipboardWatcher clipboardWatcher;
        private PrintWatcher printWatcher;

        public ApplicationWatcher GetApplicationWatcher()
        {
            if(applicationWatcher!=null)
            {
                return applicationWatcher;
            }

            applicationWatcher = new ApplicationWatcher(syncFactory);

            return applicationWatcher;
        }

        public KeyboardWatcher GetKeyboardWatcher()
        {
            if (keyboardWatcher != null)
            {
                return keyboardWatcher;
            }

            keyboardWatcher = new KeyboardWatcher(syncFactory);

            return keyboardWatcher;
        }

        public MouseWatcher GetMouseWatcher()
        {
            if (mouseWatcher != null)
            {
                return mouseWatcher;
            }

            mouseWatcher = new MouseWatcher(syncFactory);

            return mouseWatcher;
        }

        public ClipboardWatcher GetClipboardWatcher()
        {
            if (clipboardWatcher != null)
            {
                return clipboardWatcher;
            }

            clipboardWatcher = new ClipboardWatcher(syncFactory);

            return clipboardWatcher;
        }

        public PrintWatcher GetPrintWatcher()
        {
            if (printWatcher != null)
            {
                return printWatcher;
            }

            printWatcher = new PrintWatcher(syncFactory);

            return printWatcher;
        }

        public void Dispose()
        {
            keyboardWatcher?.Stop();
            mouseWatcher?.Stop();
            clipboardWatcher?.Stop();
            applicationWatcher?.Stop();
            printWatcher?.Stop();

            syncFactory.Dispose();
        }
    }
}
