namespace PrintServiceLibrary
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Printing;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MadWizard.WinUSBNet;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Class that contains USB connection to the printer
    /// </summary>
    public class UsbPrint
    {
        // Windows GUID_DEVINTERFACE_USB_DEVICE class GUID: {a5dcbf10-6530-11d2-901f-00c04fb951ed} - Do NOT change
        private const string GUID_DEVINTERFACE_USB_DEVICE = "{a5dcbf10-6530-11d2-901f-00c04fb951ed}";
        private const string ZebraVendorID = "0A5F";
        private static TimeSpan readTimeOut = TimeSpan.FromMilliseconds(1000);

        /// <summary>
        /// Print data types enum
        /// </summary>
        private enum PrintDataTypes
        {
            RAW,
            XPS_PASS,
            EMF
        }

        #region USB direct print
        /// <summary>
        /// Prints commands via USB port
        /// </summary>
        /// <param name="printerCommands">Commands to print (ZPL for example)</param>
        /// <param name="printerName">Name of the printer to connect to</param>
        /// <param name="copies">Number of copies to print</param>
        /// <param name="waitForResponse">Should printing process wait for printer response and return relevant data?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public static PrintResult Print(string printerCommands, string printerName, int copies, bool waitForResponse = false)
        {
            if (string.IsNullOrEmpty(printerName))
            {
                return new PrintResult { Message = Properties.Localization.PritnerNameEmpty };
            }

            if (string.IsNullOrEmpty(printerCommands))
            {
                return new PrintResult { Message = Properties.Localization.EmptyPrintCommands };
            }

            return ExecuteUsbCommands(printerCommands, printerName, waitForResponse, copies);
        }

        /// <summary>
        /// Executes printing via USB port
        /// </summary>
        /// <param name="printerCommands">Commands to print (ZPL for example)</param>
        /// <param name="printerName">Name of the printer to connect to</param>
        /// <param name="waitForResponse">Should printing process wait for printer response and return relevant data?</param>
        /// <param name="copies">Number of copies to print</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        private static PrintResult ExecuteUsbCommands(string printerCommands, string printerName, bool waitForResponse, int copies)
        {
            var printResult = new PrintResult();
            SafeFileHandle writePort = null;
            SafeFileHandle readPort = null;
            string printerPort = string.Empty;
            RegistryKey deviceKey = Registry.LocalMachine;
            LocalPrintServer printServer = new LocalPrintServer();

            var usbDevices = USBDevice.GetDevices(GUID_DEVINTERFACE_USB_DEVICE).ToList();

            var printQueuesOnLocalServer = printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local }).ToList();
            deviceKey = deviceKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\DeviceClasses\{28d78fad-5a12-11d1-ae5b-0000f803a8c2}");
            if (deviceKey == null)
            {
                printResult.Message = string.Format("{0} - {1}", Properties.Localization.Error, Properties.Localization.AccessRegistryKey);
                return printResult;
            }

            var keyNames = deviceKey.GetSubKeyNames();

            foreach (var usbDevice in usbDevices.Where(x => x.DevicePath.ToUpper().Contains($"VID_{ZebraVendorID}")))
            {
                // Example of PrinterPort:   \\?\usb#vid_0a5f&pid_00ab#jjk011718#{a5dcbf10-6530-11d2-901f-00c04fb951ed}
                printerPort = usbDevice.DevicePath;

                int startSN = usbDevice.DevicePath.ToUpper().IndexOf("&PID_" + usbDevice.PID.ToString("X").PadLeft(4, '0')) + 10;
                int endSN = usbDevice.DevicePath.IndexOf("#", startSN + 1);
                int lengthSN = endSN - startSN;
                string serialNumber = usbDevice.DevicePath.Substring(startSN, lengthSN).ToUpper();
                string printerConnectionPortName = string.Empty;

                try
                {
                    for (int i = 0; i < deviceKey.SubKeyCount; i++)
                    {
                        if (keyNames[i].Contains($"VID_{ZebraVendorID}&PID_" + usbDevice.PID.ToString("X").ToUpper().PadLeft(4, '0') + "#" + serialNumber))
                        {
                            printerConnectionPortName = "USB" + deviceKey.OpenSubKey(keyNames[i] + @"\#\Device Parameters").GetValue("Port Number").ToString().PadLeft(3, '0');
                        }
                    }

                    // Check if correct printer has been found (using ports)
                    if (!IsMatchingPort(printerName, printerConnectionPortName))
                    {
                        printerPort = string.Empty;
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    printResult.IsSuccessful = false;
                    printResult.Message = ex.Message;
                    return printResult;
                }

                writePort = CreateFile(printerPort,
                                       EFileAccess.GENERIC_WRITE,
                                       EFileShare.FILE_SHARE_WRITE,
                                       IntPtr.Zero,
                                       ECreationDisposition.OPEN_ALWAYS,
                                       EFileAttributes.FILE_FLAG_SEQUENTIAL_SCAN | EFileAttributes.FILE_ATTRIBUTE_NORMAL,
                                       IntPtr.Zero);

                readPort = CreateFile(printerPort,
                                      EFileAccess.GENERIC_READ,
                                      EFileShare.FILE_SHARE_READ,
                                      IntPtr.Zero,
                                      ECreationDisposition.OPEN_ALWAYS,
                                      EFileAttributes.FILE_FLAG_SEQUENTIAL_SCAN | EFileAttributes.FILE_ATTRIBUTE_NORMAL,
                                      IntPtr.Zero);

                if (!writePort.IsInvalid || !readPort.IsInvalid)
                {
                    break;
                }
            }

            if (writePort == null || writePort.IsInvalid)
            {
                printResult.Message = string.Format("{0} - {1}", Properties.Localization.WriteError, Properties.Localization.NoUsbPrinterFound);
                writePort?.Dispose();

                return printResult;
            }

            try
            {
                // WRITE
                using (var streamWriter = new StreamWriter(new FileStream(writePort, FileAccess.Write)))
                {
                    for (int i = 0; i < copies; i++)
                    {
                        streamWriter.WriteLine(printerCommands);
                        streamWriter.Flush();
                    }
                }

                printResult.BytesSent = Encoding.ASCII.GetByteCount(printerCommands) * copies;
                printResult.IsSuccessful = true;

                // READ
                if (waitForResponse)
                {
                    if (readPort == null || readPort.IsInvalid)
                    {
                        printResult.Message = string.Format("{0} - {1}", Properties.Localization.ReadError, Properties.Localization.NoUsbPrinterFound);
                        readPort?.Dispose();

                        return printResult;
                    }

                    using (var streamReader = new StreamReader(new FileStream(readPort, FileAccess.Read)))
                    {
                        ReadPrinter(streamReader, printResult);
                    }
                }
            }
            catch (Exception ex)
            {
                printResult.IsSuccessful = false;
                printResult.Message = ex.Message;
            }

            if (!writePort.IsClosed)
            {
                writePort.Close();
            }

            if (!readPort.IsClosed)
            {
                readPort.Close();
            }

            writePort.Dispose();
            readPort.Dispose();

            return printResult;
        }

        /// <summary>
        /// Reads printer response
        /// </summary>
        /// <param name="streamReader">Stream with opened USB port communication</param>
        /// <param name="printResult"><see cref="PrintResult"/> structure to save return data</param>
        private static void ReadPrinter(StreamReader streamReader, PrintResult printResult)
        {
            var cancelSource = new CancellationTokenSource();
            var readTask = Task.Factory.StartNew(
                                             () =>
                                             {
                                                 var readResult = ReadUsbPort(streamReader);
                                                 printResult.IsSuccessful = readResult.IsSuccessful;
                                                 printResult.Message = readResult.Message;
                                                 printResult.BytesSent += readResult.BytesSent;
                                             }, cancelSource.Token);

            if (Task.WaitAny(new[] { readTask }, readTimeOut) < 0)
            { // Timeout
                cancelSource.Cancel();
                printResult.Message = Properties.Localization.ReadRequestTimeOut;
                printResult.IsSuccessful = false;
            }
            else if (readTask.Exception != null)
            { // Exception
                cancelSource.Cancel();
                printResult.Message = string.Format("{0}: {1}", Properties.Localization.ErrorReadPrinterAttempt, readTask.Exception.Message);
                printResult.IsSuccessful = false;
            }
        }

        /// <summary>
        /// Reads printer response on opened USB port. This method will hang when the printer doesn't communicate back, use ReadPrinter method with Cancellation Token
        /// </summary>
        /// <param name="streamReader">Stream with opened USB port communication</param>
        /// <returns>Returns <see cref="PrintResult"/> with response details</returns>
        private static PrintResult ReadUsbPort(StreamReader streamReader)
        {
            PrintResult printResult = new PrintResult();
            string readResult = string.Empty;

            try
            {
                if (streamReader != null && streamReader.BaseStream != null)
                {
                    do
                    {
                        readResult += streamReader.ReadLine();
                        readResult += "\r";
                    }
                    while (!streamReader.EndOfStream);
                }

                printResult.Message = readResult;
                printResult.IsSuccessful = true;
                printResult.BytesSent = Encoding.ASCII.GetByteCount(readResult);
            }
            catch (IOException io)
            {
                streamReader.BaseStream.Flush();
                streamReader.BaseStream.Close();
                streamReader.Close();

                printResult.Message = string.Format("{0}: {1}", Properties.Localization.IOException, io.Message);
            }
            catch (Exception ex)
            {
                streamReader.BaseStream.Flush();
                streamReader.BaseStream.Close();
                streamReader.Close();

                printResult.Message = string.Format("{0}: {1}", Properties.Localization.ErrorOccured, ex.Message);
            }

            return printResult;
        }

        /// <summary>
        /// Checks if entered port name matches the installed printer
        /// </summary>
        /// <param name="printerName">Printer name</param>
        /// <param name="portName">Specific printer port name</param>
        /// <returns>Returns if the printer and port matches</returns>
        private static bool IsMatchingPort(string printerName, string portName)
        {
            LocalPrintServer printServer = new LocalPrintServer();
            var printQueuesOnLocalServer = printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local }).ToList();
            var printer = printQueuesOnLocalServer.FirstOrDefault(x => x.Name == printerName);

            return printer != null && printer.QueuePort.Name == portName;
        }
        #endregion

        #region Direct Print 2
        /// <summary>
        /// When the function is given a printer name and an unmanaged array of bytes, the function sends those bytes to the print queue.
        /// </summary>
        /// <param name="printerName">Printer name</param>
        /// <param name="printBytes">Print bytes</param>
        /// <param name="dataSize">Data size</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public static PrintResult SendBytesToPrinter(string printerName, IntPtr printBytes, Int32 dataSize)
        {
            PrintResult printResult = new PrintResult();
            Int32 writtenDataPointer = 0;
            IntPtr printerPointer = new IntPtr(0);

            DOCINFOA docinfo = new DOCINFOA
            {
                docName = "Label print document",
                dataType = PrintDataTypes.RAW.ToString()
            };

            if (OpenPrinter(printerName.Normalize(), out printerPointer, IntPtr.Zero))
            {
                if (StartDocPrinter(printerPointer, 1, docinfo))
                {
                    if (StartPagePrinter(printerPointer))
                    {
                        // Write bytes
                        printResult.IsSuccessful = WritePrinter(printerPointer, printBytes, dataSize, out writtenDataPointer);
                        printResult.BytesSent = dataSize;
                        EndPagePrinter(printerPointer);
                    }

                    EndDocPrinter(printerPointer);
                }

                ClosePrinter(printerPointer);
            }

            if (!printResult.IsSuccessful)
            {
                printResult.Message = string.Format("{0} {1}", Properties.Localization.ErrorOccuredDuringPrinting, Marshal.GetLastWin32Error());
            }

            return printResult;
        }

        /// <summary>
        /// Sends file to the printer
        /// </summary>
        /// <param name="printerName">Printer name</param>
        /// <param name="fileName">File to send</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public static PrintResult SendFileToPrinter(string printerName, string fileName)
        {
            PrintResult printResult = new PrintResult();

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    Byte[] bytes = new Byte[fs.Length];

                    IntPtr unmanagedBytes = new IntPtr(0);
                    int length = Convert.ToInt32(fs.Length);
                    bytes = br.ReadBytes(length);
                    
                    unmanagedBytes = Marshal.AllocCoTaskMem(length);
                    Marshal.Copy(bytes, 0, unmanagedBytes, length);

                    // Send the unmanaged bytes to the printer
                    printResult = SendBytesToPrinter(printerName, unmanagedBytes, length);
                    
                    Marshal.FreeCoTaskMem(unmanagedBytes);
                }
            }

            return printResult;
        }

        /// <summary>
        /// Sends string to the printer
        /// </summary>
        /// <param name="printerName">Printer name</param>
        /// <param name="stringToPrint">String to send</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public static PrintResult SendStringToPrinter(string printerName, string stringToPrint)
        {
            PrintResult printResult = new PrintResult();
            Int32 dwCount = stringToPrint.Length;

            // Assume that the printer is expecting ANSI text, and then convert the string to ANSI text.
            IntPtr pBytes = Marshal.StringToCoTaskMemAnsi(stringToPrint);

            try
            {
                printResult = SendBytesToPrinter(printerName, pBytes, dwCount);
            }
            catch (Exception ex)
            {
                printResult.Message = ex.ToString();
                printResult.IsSuccessful = false;
            }

            Marshal.FreeCoTaskMem(pBytes);

            return printResult;
        }
        #endregion

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(string lpFileName, EFileAccess dwDesiredAccess, EFileShare dwShareMode, IntPtr lpSecurityAttributes, ECreationDisposition dwCreationDisposition, EFileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetPrinterDriver(IntPtr hPrinter, string pEnvironment, int Level, IntPtr pDriverInfo, int cbBuf, ref int pcbNeeded);
    }
}
