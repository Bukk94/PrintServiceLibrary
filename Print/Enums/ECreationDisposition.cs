namespace PrintServiceLibrary
{
    /// <summary>
    /// Enum with hex values for system file creation used in USB and LPT printing
    /// </summary>
    internal enum ECreationDisposition
    {
        CREATE_NEW = 1,
        CREATE_ALWAYS = 2,
        OPEN_EXISTING = 3,
        OPEN_ALWAYS = 4,
        TRUNCATE_EXISTING = 5
    }
}
