using System;

namespace EventHook
{
    public class PrintEventData
    {
        public DateTime EventDateTime { get; set; }
        public string PrinterName { get; set; }
        public string JobName { get; set; }
        public int? Pages { get; set; }
        public int? JobSize { get; set; }
        public string ApplicationTitle { get; set; }
        public string ApplicationName { get; set; }
    }
}