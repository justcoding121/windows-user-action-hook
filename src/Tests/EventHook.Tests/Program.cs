using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EventHook.Tests
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Start();
            System.Windows.Threading.Dispatcher.Run();
        }

        public static void Start()
        {
            //System.Windows.Threading.Dispatcher.CurrentDispatcher.Thread.SetApartmentState(ApartmentState.STA);
            KeyLogger.Start();
            ApplicationWatcher.Start();
            WebWatcher.Start();
            ClipboardWatcher.Start();
            FileWatcher.Start();
            PrintWatcher.Start();
            EmailClientWatcher.Start();          
        }

    }
}
