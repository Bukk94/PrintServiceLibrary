namespace PrintServiceLibrary
{
    /// <summary>
    /// Enum with hex values for system file creation used in USB and LPT printing
    /// </summary>
    internal enum EFileAccess : uint
    {
        // The following are masks for the predefined standard access types
        DELETE = 0x10000,
        READ_CONTROL = 0x20000,
        WRITE_DAC = 0x40000,
        WRITE_OWNER = 0x80000,
        SYNCHRONIZE = 0x100000,

        STANDARD_RIGHTS_REQUIRED = 0xF0000,
        STANDARD_RIGHTS_READ = READ_CONTROL,
        STANDARD_RIGHTS_WRITE = READ_CONTROL,
        STANDARD_RIGHTS_EXECUTE = READ_CONTROL,
        STANDARD_RIGHTS_ALL = 0x1F0000,
        SPECIFIC_RIGHTS_ALL = 0xFFFF,
        ACCESS_SYSTEM_SECURITY = 0x1000000,
        MAXIMUM_ALLOWED = 0x2000000,

        // These are the generic rights
        GENERIC_READ = 0x80000000,
        GENERIC_WRITE = 0x40000000,
        GENERIC_EXECUTE = 0x20000000,
        GENERIC_ALL = 0x10000000
    }
}
