namespace PrintServiceLibrary
{
    /// <summary>
    /// Enum with hex values for system file creation used in USB and LPT printing
    /// </summary>
    internal enum EFileShare
    {
        FILE_SHARE_NONE = 0x0,
        FILE_SHARE_READ = 0x1,
        FILE_SHARE_WRITE = 0x2,
        FILE_SHARE_DELETE = 0x4
    }
}
