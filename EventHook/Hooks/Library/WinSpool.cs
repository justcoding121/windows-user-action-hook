using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace EventHook.Hooks.Library
{
    internal static class WinSpool
    {
        #region DLL Import Functions

        [DllImport("winspool.drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi,
            ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool OpenPrinter(string pPrinterName,
            out IntPtr phPrinter,
            int pDefault);


        [DllImport("winspool.drv", EntryPoint = "ClosePrinter",
            SetLastError = true,
            ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        internal static extern bool ClosePrinter
            (int hPrinter);

        [DllImport("winspool.drv",
            EntryPoint = "FindFirstPrinterChangeNotification",
            SetLastError = true, CharSet = CharSet.Ansi,
            ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr FindFirstPrinterChangeNotification
            ([In] IntPtr hPrinter,
                [In] int fwFlags,
                [In] int fwOptions,
                [In, MarshalAs(UnmanagedType.LPStruct)] PRINTER_NOTIFY_OPTIONS pPrinterNotifyOptions);

        [DllImport("winspool.drv", EntryPoint = "FindNextPrinterChangeNotification",
            SetLastError = true, CharSet = CharSet.Ansi,
            ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        internal static extern bool FindNextPrinterChangeNotification
            ([In] IntPtr hChangeObject,
                [Out] out int pdwChange,
                [In, MarshalAs(UnmanagedType.LPStruct)] PRINTER_NOTIFY_OPTIONS pPrinterNotifyOptions,
                [Out] out IntPtr lppPrinterNotifyInfo
            );

        #endregion

    }
}
