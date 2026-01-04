using System.Runtime.InteropServices;

namespace Photobooth.Camera.Canon.Services;

internal static class EdsdkNative
{
    private const string EDSDK = "EDSDK";

    public const int EDS_ERR_OK = 0x00000000;

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsInitializeSDK();

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsTerminateSDK();

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsGetCameraList(out IntPtr outCameraListRef);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsGetChildCount(IntPtr inRef, out int outCount);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsGetChildAtIndex(IntPtr inRef, int inIndex, out IntPtr outRef);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsRelease(IntPtr inRef);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsOpenSession(IntPtr inCameraRef);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsCloseSession(IntPtr inCameraRef);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsGetDeviceInfo(IntPtr inCameraRef, out EdsDeviceInfo outDeviceInfo);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsSendCommand(IntPtr inCameraRef, uint inCommand, int inParam);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsSetPropertyData(IntPtr inRef, uint inPropertyID, int inParam, int inSize, IntPtr inData);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsSetCapacity(IntPtr inCameraRef, EdsCapacity capacity);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsSetObjectEventHandler(IntPtr inRef, uint inEvent, EdsObjectEventHandler inHandler, IntPtr inContext);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int EdsCreateFileStream(string inFileName, uint inCreateDisposition, uint inDesiredAccess, out IntPtr outStream);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsDownload(IntPtr inDirectoryItemRef, uint inReadSize, IntPtr outStream);

    [DllImport(EDSDK, CallingConvention = CallingConvention.StdCall)]
    public static extern int EdsDownloadComplete(IntPtr inDirectoryItemRef);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct EdsDeviceInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szPortName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szDeviceDescription;
        public uint deviceSubType;
        public uint reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EdsCapacity
    {
        public int NumberOfFreeClusters;
        public int BytesPerSector;
        public int Reset;
    }

    public delegate int EdsObjectEventHandler(uint inEvent, IntPtr inRef, IntPtr inContext);

    public const uint kEdsCameraCommand_TakePicture = 0x00000000;
    public const uint kEdsObjectEvent_DirItemCreated = 0x00000208;
    public const uint kEdsObjectEvent_DirItemRequestTransfer = 0x00000209;
    public const uint kEdsPropertyID_SaveTo = 0x0000000b;

    public const int kEdsSaveTo_Host = 2;

    public const uint kEdsFileCreateDisposition_CreateAlways = 0x00000002;
    public const uint kEdsAccess_ReadWrite = 0x00000003;
}
