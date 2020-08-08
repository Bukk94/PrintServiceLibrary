namespace PrintServiceLibrary
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Data structure for USB print
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal class DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)] public string docName;
        [MarshalAs(UnmanagedType.LPStr)] public string outputFile;
        [MarshalAs(UnmanagedType.LPStr)] public string dataType;
    }
}
