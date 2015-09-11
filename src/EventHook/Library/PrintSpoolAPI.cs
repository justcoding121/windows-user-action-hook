
using System.Runtime.InteropServices;
using System;
using System.Globalization;

namespace AgentSafeX.Client.Utility.Hooks.Library
{
  


    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        [MarshalAs(UnmanagedType.U2)]
        public short Year;
        [MarshalAs(UnmanagedType.U2)]
        public short Month;
        [MarshalAs(UnmanagedType.U2)]
        public short DayOfWeek;
        [MarshalAs(UnmanagedType.U2)]
        public short Day;
        [MarshalAs(UnmanagedType.U2)]
        public short Hour;
        [MarshalAs(UnmanagedType.U2)]
        public short Minute;
        [MarshalAs(UnmanagedType.U2)]
        public short Second;
        [MarshalAs(UnmanagedType.U2)]
        public short Milliseconds;

        public SYSTEMTIME(DateTime dt)
        {
            dt = dt.ToUniversalTime();  // SetSystemTime expects the SYSTEMTIME in UTC
            Year = (short)dt.Year;
            Month = (short)dt.Month;
            DayOfWeek = (short)dt.DayOfWeek;
            Day = (short)dt.Day;
            Hour = (short)dt.Hour;
            Minute = (short)dt.Minute;
            Second = (short)dt.Second;
            Milliseconds = (short)dt.Millisecond;
        }

        public DateTime ToDateTime()
        {
            return new DateTime(Year, Month, Day, Hour, Minute, Second, Milliseconds, CultureInfo.CurrentCulture.Calendar, DateTimeKind.Utc).ToLocalTime();
        }
    }

    [Flags]
    public enum JOBSTATUS
    {
        JOB_STATUS_PAUSED = 0x00000001,
        JOB_STATUS_ERROR = 0x00000002,
        JOB_STATUS_DELETING = 0x00000004,
        JOB_STATUS_SPOOLING = 0x00000008,
        JOB_STATUS_PRINTING = 0x00000010,
        JOB_STATUS_OFFLINE = 0x00000020,
        JOB_STATUS_PAPEROUT = 0x00000040,
        JOB_STATUS_PRINTED = 0x00000080,
        JOB_STATUS_DELETED = 0x00000100,
        JOB_STATUS_BLOCKED_DEVQ = 0x00000200,
        JOB_STATUS_USER_INTERVENTION = 0x00000400,
        JOB_STATUS_RESTART = 0x00000800,
        JOB_STATUS_COMPLETE = 0x00001000,
        JOB_STATUS_RETAINED = 0x00002000,
        JOB_STATUS_RENDERING_LOCALLY = 0x00004000,
    }


    public class PRINTER_CHANGES
    {
        public const uint PRINTER_CHANGE_ADD_PRINTER = 1;
        public const uint PRINTER_CHANGE_SET_PRINTER = 2;
        public const uint PRINTER_CHANGE_DELETE_PRINTER = 4;
        public const uint PRINTER_CHANGE_FAILED_CONNECTION_PRINTER = 8;
        public const uint PRINTER_CHANGE_PRINTER = 0xFF;
        public const uint PRINTER_CHANGE_ADD_JOB = 0x100;
        public const uint PRINTER_CHANGE_SET_JOB = 0x200;
        public const uint PRINTER_CHANGE_DELETE_JOB = 0x400;
        public const uint PRINTER_CHANGE_WRITE_JOB = 0x800;
        public const uint PRINTER_CHANGE_JOB = 0xFF00;
        public const uint PRINTER_CHANGE_ADD_FORM = 0x10000;
        public const uint PRINTER_CHANGE_SET_FORM = 0x20000;
        public const uint PRINTER_CHANGE_DELETE_FORM = 0x40000;
        public const uint PRINTER_CHANGE_FORM = 0x70000;
        public const uint PRINTER_CHANGE_ADD_PORT = 0x100000;
        public const uint PRINTER_CHANGE_CONFIGURE_PORT = 0x200000;
        public const uint PRINTER_CHANGE_DELETE_PORT = 0x400000;
        public const uint PRINTER_CHANGE_PORT = 0x700000;
        public const uint PRINTER_CHANGE_ADD_PRINT_PROCESSOR = 0x1000000;
        public const uint PRINTER_CHANGE_DELETE_PRINT_PROCESSOR = 0x4000000;
        public const uint PRINTER_CHANGE_PRINT_PROCESSOR = 0x7000000;
        public const uint PRINTER_CHANGE_ADD_PRINTER_DRIVER = 0x10000000;
        public const uint PRINTER_CHANGE_SET_PRINTER_DRIVER = 0x20000000;
        public const uint PRINTER_CHANGE_DELETE_PRINTER_DRIVER = 0x40000000;
        public const uint PRINTER_CHANGE_PRINTER_DRIVER = 0x70000000;
        public const uint PRINTER_CHANGE_TIMEOUT = 0x80000000;
        public const uint PRINTER_CHANGE_ALL = 0x7777FFFF;
    }

    public enum PRINTERPRINTERNOTIFICATIONTYPES
    {
        PRINTER_NOTIFY_FIELD_SERVER_NAME = 0,
        PRINTER_NOTIFY_FIELD_PRINTER_NAME = 1,
        PRINTER_NOTIFY_FIELD_SHARE_NAME = 2,
        PRINTER_NOTIFY_FIELD_PORT_NAME = 3,
        PRINTER_NOTIFY_FIELD_DRIVER_NAME = 4,
        PRINTER_NOTIFY_FIELD_COMMENT = 5,
        PRINTER_NOTIFY_FIELD_LOCATION = 6,
        PRINTER_NOTIFY_FIELD_DEVMODE = 7,
        PRINTER_NOTIFY_FIELD_SEPFILE = 8,
        PRINTER_NOTIFY_FIELD_PRINT_PROCESSOR = 9,
        PRINTER_NOTIFY_FIELD_PARAMETERS = 10,
        PRINTER_NOTIFY_FIELD_DATATYPE = 11,
        PRINTER_NOTIFY_FIELD_SECURITY_DESCRIPTOR = 12,
        PRINTER_NOTIFY_FIELD_ATTRIBUTES = 13,
        PRINTER_NOTIFY_FIELD_PRIORITY = 14,
        PRINTER_NOTIFY_FIELD_DEFAULT_PRIORITY = 15,
        PRINTER_NOTIFY_FIELD_StartTime = 16,
        PRINTER_NOTIFY_FIELD_UNTIL_TIME = 17,
        PRINTER_NOTIFY_FIELD_STATUS = 18,
        PRINTER_NOTIFY_FIELD_STATUS_STRING = 19,
        PRINTER_NOTIFY_FIELD_CJOBS = 20,
        PRINTER_NOTIFY_FIELD_AVERAGE_PPM = 21,
        PRINTER_NOTIFY_FIELD_TOTAL_PAGES = 22,
        PRINTER_NOTIFY_FIELD_PAGES_PRINTED = 23,
        PRINTER_NOTIFY_FIELD_TOTAL_BYTES = 24,
        PRINTER_NOTIFY_FIELD_BYTES_PRINTED = 25,
    }

    public enum PRINTERJOBNOTIFICATIONTYPES
    {
        JOB_NOTIFY_FIELD_PRINTER_NAME = 0,
        JOB_NOTIFY_FIELD_MACHINE_NAME = 1,
        JOB_NOTIFY_FIELD_PORT_NAME = 2,
        JOB_NOTIFY_FIELD_USER_NAME = 3,
        JOB_NOTIFY_FIELD_NOTIFY_NAME = 4,
        JOB_NOTIFY_FIELD_DATATYPE = 5,
        JOB_NOTIFY_FIELD_PRINT_PROCESSOR = 6,
        JOB_NOTIFY_FIELD_PARAMETERS = 7,
        JOB_NOTIFY_FIELD_DRIVER_NAME = 8,
        JOB_NOTIFY_FIELD_DEVMODE = 9,
        JOB_NOTIFY_FIELD_STATUS = 10,
        JOB_NOTIFY_FIELD_STATUS_STRING = 11,
        JOB_NOTIFY_FIELD_SECURITY_DESCRIPTOR = 12,
        JOB_NOTIFY_FIELD_DOCUMENT = 13,
        JOB_NOTIFY_FIELD_PRIORITY = 14,
        JOB_NOTIFY_FIELD_POSITION = 15,
        JOB_NOTIFY_FIELD_SUBMITTED = 16,
        JOB_NOTIFY_FIELD_StartTime = 17,
        JOB_NOTIFY_FIELD_UNTIL_TIME = 18,
        JOB_NOTIFY_FIELD_TIME = 19,
        JOB_NOTIFY_FIELD_TOTAL_PAGES = 20,
        JOB_NOTIFY_FIELD_PAGES_PRINTED = 21,
        JOB_NOTIFY_FIELD_TOTAL_BYTES = 22,
        JOB_NOTIFY_FIELD_BYTES_PRINTED = 23,
    }

    [StructLayout(LayoutKind.Sequential)]
    public class PRINTER_NOTIFY_OPTIONS
    {
        public int dwVersion = 2;
        public int dwFlags;
        public int Count = 2;
        public IntPtr lpTypes;

        public PRINTER_NOTIFY_OPTIONS()
        {
            int bytesNeeded = (2 + PRINTER_NOTIFY_OPTIONS_TYPE.JOB_FIELDS_COUNT + PRINTER_NOTIFY_OPTIONS_TYPE.PRINTER_FIELDS_COUNT) * 2;
            PRINTER_NOTIFY_OPTIONS_TYPE pJobTypes = new PRINTER_NOTIFY_OPTIONS_TYPE();
            lpTypes = Marshal.AllocHGlobal(bytesNeeded);
            Marshal.StructureToPtr(pJobTypes, lpTypes, true);
        }
    }

    public enum PRINTERNOTIFICATIONTYPES
    {
        PRINTER_NOTIFY_TYPE = 0,
        JOB_NOTIFY_TYPE = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    public class PRINTER_NOTIFY_OPTIONS_TYPE
    {
        public const int JOB_FIELDS_COUNT = 24;
        public const int PRINTER_FIELDS_COUNT = 23;

        public Int16 wJobType;
        public Int16 wJobReserved0;
        public Int32 dwJobReserved1;
        public Int32 dwJobReserved2;
        public Int32 JobFieldCount;
        public IntPtr pJobFields;
        public Int16 wPrinterType;
        public Int16 wPrinterReserved0;
        public Int32 dwPrinterReserved1;
        public Int32 dwPrinterReserved2;
        public Int32 PrinterFieldCount;
        public IntPtr pPrinterFields;

        private void SetupFields()
        {
            if (pJobFields.ToInt32() != 0)
            {
                Marshal.FreeHGlobal(pJobFields);
            }

            if (wJobType == (short)PRINTERNOTIFICATIONTYPES.JOB_NOTIFY_TYPE)
            {
                JobFieldCount = JOB_FIELDS_COUNT;
                pJobFields = Marshal.AllocHGlobal((JOB_FIELDS_COUNT * 2) - 1);

                Marshal.WriteInt16(pJobFields, 0, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_PRINTER_NAME);
                Marshal.WriteInt16(pJobFields, 2, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_MACHINE_NAME);
                Marshal.WriteInt16(pJobFields, 4, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_PORT_NAME);
                Marshal.WriteInt16(pJobFields, 6, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_USER_NAME);
                Marshal.WriteInt16(pJobFields, 8, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_NOTIFY_NAME);
                Marshal.WriteInt16(pJobFields, 10, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_DATATYPE);
                Marshal.WriteInt16(pJobFields, 12, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_PRINT_PROCESSOR);
                Marshal.WriteInt16(pJobFields, 14, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_PARAMETERS);
                Marshal.WriteInt16(pJobFields, 16, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_DRIVER_NAME);
                Marshal.WriteInt16(pJobFields, 18, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_DEVMODE);
                Marshal.WriteInt16(pJobFields, 20, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_STATUS);
                Marshal.WriteInt16(pJobFields, 22, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_STATUS_STRING);
                Marshal.WriteInt16(pJobFields, 24, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_SECURITY_DESCRIPTOR);
                Marshal.WriteInt16(pJobFields, 26, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_DOCUMENT);
                Marshal.WriteInt16(pJobFields, 28, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_PRIORITY);
                Marshal.WriteInt16(pJobFields, 30, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_POSITION);
                Marshal.WriteInt16(pJobFields, 32, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_SUBMITTED);
                Marshal.WriteInt16(pJobFields, 34, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_StartTime);
                Marshal.WriteInt16(pJobFields, 36, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_UNTIL_TIME);
                Marshal.WriteInt16(pJobFields, 38, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_TIME);
                Marshal.WriteInt16(pJobFields, 40, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_TOTAL_PAGES);
                Marshal.WriteInt16(pJobFields, 42, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_PAGES_PRINTED);
                Marshal.WriteInt16(pJobFields, 44, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_TOTAL_BYTES);
                Marshal.WriteInt16(pJobFields, 46, (short)PRINTERJOBNOTIFICATIONTYPES.JOB_NOTIFY_FIELD_BYTES_PRINTED);
            }

            if (pPrinterFields.ToInt32() != 0)
            {
                Marshal.FreeHGlobal(pPrinterFields);
            }

            if (wPrinterType == (short)PRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_TYPE)
            {
                PrinterFieldCount = PRINTER_FIELDS_COUNT;
                pPrinterFields = Marshal.AllocHGlobal((PRINTER_FIELDS_COUNT - 1) * 2);

                Marshal.WriteInt16(pPrinterFields, 0, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_SERVER_NAME);
                Marshal.WriteInt16(pPrinterFields, 2, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_PRINTER_NAME);
                Marshal.WriteInt16(pPrinterFields, 4, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_SHARE_NAME);
                Marshal.WriteInt16(pPrinterFields, 6, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_PORT_NAME);
                Marshal.WriteInt16(pPrinterFields, 8, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_DRIVER_NAME);
                Marshal.WriteInt16(pPrinterFields, 10, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_COMMENT);
                Marshal.WriteInt16(pPrinterFields, 12, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_LOCATION);
                Marshal.WriteInt16(pPrinterFields, 14, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_SEPFILE);
                Marshal.WriteInt16(pPrinterFields, 16, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_PRINT_PROCESSOR);
                Marshal.WriteInt16(pPrinterFields, 18, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_PARAMETERS);
                Marshal.WriteInt16(pPrinterFields, 20, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_DATATYPE);
                Marshal.WriteInt16(pPrinterFields, 22, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_ATTRIBUTES);
                Marshal.WriteInt16(pPrinterFields, 24, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_PRIORITY);
                Marshal.WriteInt16(pPrinterFields, 26, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_DEFAULT_PRIORITY);
                Marshal.WriteInt16(pPrinterFields, 28, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_StartTime);
                Marshal.WriteInt16(pPrinterFields, 30, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_UNTIL_TIME);
                Marshal.WriteInt16(pPrinterFields, 32, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_STATUS_STRING);
                Marshal.WriteInt16(pPrinterFields, 34, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_CJOBS);
                Marshal.WriteInt16(pPrinterFields, 36, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_AVERAGE_PPM);
                Marshal.WriteInt16(pPrinterFields, 38, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_TOTAL_PAGES);
                Marshal.WriteInt16(pPrinterFields, 40, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_PAGES_PRINTED);
                Marshal.WriteInt16(pPrinterFields, 42, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_TOTAL_BYTES);
                Marshal.WriteInt16(pPrinterFields, 44, (short)PRINTERPRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_FIELD_BYTES_PRINTED);
            }
        }

        public PRINTER_NOTIFY_OPTIONS_TYPE()
        {
            wJobType = (short)PRINTERNOTIFICATIONTYPES.JOB_NOTIFY_TYPE;
            wPrinterType = (short)PRINTERNOTIFICATIONTYPES.PRINTER_NOTIFY_TYPE;

            SetupFields();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PRINTER_NOTIFY_INFO
    {
        public uint Version;
        public uint Flags;
        public uint Count;
        public PRINTER_NOTIFY_INFO_DATA_UNION aData;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct PRINTER_NOTIFY_INFO_DATA_DATA
    {
        public uint cbBuf;
        public IntPtr pBuf;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PRINTER_NOTIFY_INFO_DATA_UNION
    {
        [FieldOffset(0)]
        private uint adwData0;
        [FieldOffset(4)]
        private uint adwData1;
        [FieldOffset(0)]
        public PRINTER_NOTIFY_INFO_DATA_DATA Data;
        public uint[] adwData
        {
            get
            {
                return new uint[] { this.adwData0, this.adwData1 };
            }
        }
    }

    // Structure borrowed from http://lifeanEventTimesofadeveloper.blogspot.com/2007/10/unmanaged-structures-padding-and-c-part_18.html.
    [StructLayout(LayoutKind.Sequential)]
    public struct PRINTER_NOTIFY_INFO_DATA
    {
        public ushort Type;
        public ushort Field;
        public uint Reserved;
        public uint Id;
        public PRINTER_NOTIFY_INFO_DATA_UNION NotifyData;
    }

}

