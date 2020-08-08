using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PrintServiceLibrary
{
    /// <summary>
    /// Class that handles paralel (LPT) connection to the printer
    /// </summary>
    public static class LPTPrint
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(string lpFileName, EFileAccess dwDesiredAccess, EFileShare dwShareMode, IntPtr lpSecurityAttributes, ECreationDisposition dwCreationDisposition, EFileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

        /// <summary>
        /// Prints commands via LPT parallel port
        /// </summary>
        /// <param name="printerCommands">Commands to pring (ZPL for example)</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with printer connection details. It must contain ParallelPortName which must start with LPT.</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public static PrintResult Print(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings)
        {
            return Print(printerCommands, printerSettings.ParallelPortName, printerSettings.Copies);
        }

        /// <summary>
        /// Prints commands via LPT parallel port
        /// </summary>
        /// <param name="printerCommands">Commands to pring (ZPL for example)</param>
        /// <param name="portName">Parallel port name, must start with LPT</param>
        /// <param name="copies">Number of copies to print</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public static PrintResult Print(string printerCommands, string portName, int copies = 1)
        {
            PrintResult printResult = new PrintResult();
            FileStream lptStream = null;

            if (portName.Length < 3 || portName.ToUpper().Substring(0, 3) != "LPT")
            {
                printResult.IsSuccessful = false;
                printResult.Message = Properties.Localization.InvalidLptPortName;
                return printResult;
            }

            SafeFileHandle lptHandle = CreateFile(portName, EFileAccess.GENERIC_WRITE, EFileShare.FILE_SHARE_NONE, IntPtr.Zero, ECreationDisposition.OPEN_EXISTING, 0, IntPtr.Zero);

            try
            {
                lptStream = new FileStream(lptHandle, FileAccess.ReadWrite);
            }
            catch
            {
                if (lptHandle != null)
                {
                    lptHandle.Dispose();
                }

                printResult.IsSuccessful = false;
                printResult.Message = Properties.Localization.ZebraConnectionFailed;
                return printResult;
            }

            try
            {
                for (int i = 0; i < copies; i++)
                {
                    var buffer = Encoding.ASCII.GetBytes(printerCommands);
                    lptStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                if (lptStream != null)
                {
                    lptStream.Close();
                    lptStream.Dispose();
                }

                if (lptHandle != null)
                {
                    lptHandle.Dispose();
                }

                printResult.IsSuccessful = false;
                printResult.Message = string.Format("{0}. {1}: {2}", Properties.Localization.PrintError, Properties.Localization.ErrorMessage, ex.Message);

                return printResult;
            }

            lptStream.Close();
            lptStream.Dispose();
            lptHandle.Close();
            lptHandle.Dispose();

            printResult.BytesSent = Encoding.ASCII.GetBytes(printerCommands).Length * copies;
            printResult.IsSuccessful = true;

            return printResult;
        }
    }
}
