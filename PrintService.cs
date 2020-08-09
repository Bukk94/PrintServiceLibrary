namespace PrintServiceLibrary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Ports;
    using System.Net;
    using System.Printing;
    using System.Text.RegularExpressions;
    using Microsoft.Win32;

    /// <summary>
    /// Core API class. IPrintService implementation class. 
    /// </summary>
    public class PrintService : IPrintService
    {
        /// <summary>
        /// Sends commands to the printers specified by the printer settings structure
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure containing all connection details</param>
        /// <param name="waitForResponse">Should printing process wait for printer response and return relevant data?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public PrintResult Print(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings, bool waitForResponse = false)
        {
            return this.ExecuteCommand(printerCommands, printerSettings, waitForResponse);
        }

        #region USB printing
        /// <summary>
        /// Sends commands via USB to ZEBRA printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="printerName">Target printer name</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public PrintResult PrintUSB(string printerCommands, string printerName)
        {
            return this.PrintUSB(printerCommands, printerName, 1, false);
        }

        public PrintResult PrintUSB(string printerCommands, string printerName, int copies)
        {
            return this.PrintUSB(printerCommands, printerName, copies, false);
        }

        public PrintResult PrintUSB(string printerCommands, string printerName, int copies, bool waitForResponse = false)
        {
            return UsbPrint.Print(printerCommands, printerName, copies, waitForResponse);
        }

        public PrintResult PrintUSB(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings, bool waitForResponse = false)
        {
            return this.PrintUSB(printerCommands, printerSettings.PrinterName, printerSettings.Copies);
        }

        #endregion

        #region Network Printing
        /// <summary>
        /// Sends commands via Network connection to the printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="ip">Target printer IP address</param>
        /// <param name="portNumber">Target printer port number</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public PrintResult PrintNetwork(string printerCommands, IPAddress ip, int portNumber)
        {
            return this.PrintNetwork(printerCommands, ip, portNumber, 1, false);
        }

        public PrintResult PrintNetwork(string printerCommands, IPAddress ip, int portNumber, int copies, bool waitForResponse = false)
        {
            PrintServiceLibrary.PrinterSettings printerSettings = new PrintServiceLibrary.PrinterSettings(ip, portNumber)
            {
                Copies = copies,
                ProgrammingLanguage = PrintServiceLibrary.ProgrammingLanguage.ZPL
            };

            return this.PrintNetwork(printerCommands, printerSettings, waitForResponse);
        }

        public PrintResult PrintNetwork(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings, bool waitForResponse = false)
        {
            return NetworkPrint.Print(printerCommands, printerSettings, waitForResponse);
        }
        #endregion

        #region Serial Printing
        /// <summary>
        /// Sends commands via Serial (RS-232) connection to the printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="serialPortName">Name of the serial port (COM1 for example)</param>
        /// <param name="serialPortBaudRate">Baud Rate (speed)</param>
        /// <param name="serialPortDataBits">Number of data bits</param>
        /// <param name="serialPortFlowControl">Handshake flow control</param>
        /// <param name="parity">Parity setting</param>
        /// <param name="stopBits">Stop bits</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public PrintResult PrintSerial(string printerCommands, string serialPortName, int serialPortBaudRate, int serialPortDataBits, Handshake serialPortFlowControl, Parity parity, StopBits stopBits)
        {
            return this.PrintSerial(printerCommands, serialPortName, serialPortBaudRate, serialPortDataBits, serialPortFlowControl, parity, stopBits, 1);
        }

        public PrintResult PrintSerial(string printerCommands, string serialPortName, int serialPortBaudRate, int serialPortDataBits, Handshake serialPortFlowControl, Parity parity, StopBits stopBits, int copies)
        {
            PrintServiceLibrary.PrinterSettings printerSettings = new PrintServiceLibrary.PrinterSettings(serialPortName, serialPortBaudRate, stopBits, serialPortDataBits, serialPortFlowControl, parity)
            {
                Copies = copies,
                ProgrammingLanguage = PrintServiceLibrary.ProgrammingLanguage.ZPL
            };

            return this.PrintSerial(printerCommands, printerSettings);
        }

        public PrintResult PrintSerial(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings)
        {
            return SerialPrint.Print(printerCommands, printerSettings);
        }
        #endregion

        #region LPT (Parallel) Print
        /// <summary>
        /// Sends commands via Parallel connection to the printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="parallelPortName">Name of the parallel port (LPT1 for example)</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public PrintResult PrintParallel(string printerCommands, string parallelPortName)
        {
            return LPTPrint.Print(printerCommands, parallelPortName);
        }

        public PrintResult PrintParallel(string printerCommands, string parallelPortName, int copies)
        {
            return LPTPrint.Print(printerCommands, parallelPortName, copies);
        }

        public PrintResult PrintParallel(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings)
        {
            return LPTPrint.Print(printerCommands, printerSettings.ParallelPortName);
        }
        #endregion

        #region Printer Utilities
        /// <summary>
        /// Uploads selected font into printer's memory
        /// </summary>
        /// <param name="fontPath">Font location on the drive</param>
        /// <param name="fontNameInPrinter">Font name in the printer</param>
        /// <param name="memoryType">Memory type defined by <see cref="PrinterMemoryType"/></param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure containing all connection details</param>
        /// <param name="waitForResponse">Should upload process wait for the printer response and return relevant data?</param>
        /// <returns>Returns <see cref="PrintResult"/> with upload details</returns>
        public PrintResult UploadFontToPrinter(string fontPath, string fontNameInPrinter, PrinterMemoryType memoryType, PrintServiceLibrary.PrinterSettings printerSettings, bool waitForResponse = false)
        {
            byte[] fontByteArray = File.ReadAllBytes(fontPath);
            var fontData = BitConverter.ToString(fontByteArray);
            fontData = fontData.Replace("-", string.Empty);

            var zplCommand = string.Format("~DU{0}:{1}.FNT,{2},{3}", (char)memoryType, fontNameInPrinter, fontByteArray.Length, fontData);

            if (waitForResponse)
            {
                zplCommand += string.Format("^XA^HW{0}:^XZ", (char)memoryType);
            }

            return this.ExecuteCommand(zplCommand, printerSettings, waitForResponse);
        }

        /// <summary>
        /// Lists all data in target printer memory
        /// </summary>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure containing all connection details</param>
        /// <param name="memoryType">Memory type defined by <see cref="PrinterMemoryType"/></param>
        /// <returns>Return memory string</returns>
        public string ListPrinterMemory(PrintServiceLibrary.PrinterSettings printerSettings, PrinterMemoryType memoryType)
        {
            var printerResult = this.ExecuteCommand(string.Format("^XA^HW{0}:^XZ", (char)memoryType), printerSettings, true);
            if (printerResult.IsSuccessful)
            {
                return printerResult.Message;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets how much of free memory space is in the target printer memory
        /// </summary>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure containing all connection details</param>
        /// <param name="memoryType">Memory type defined by <see cref="PrinterMemoryType"/></param>
        /// <returns>Return number of free memory or -1 on error</returns>
        public long GetPrinterFreeMemory(PrintServiceLibrary.PrinterSettings printerSettings, PrinterMemoryType memoryType)
        {
            var printerMemory = this.ListPrinterMemory(printerSettings, memoryType);

            if (string.IsNullOrEmpty(printerMemory))
            {
                return -1;
            }

            long freeMemory = -1;
            var pattern = @"-\W.?(.+) bytes free";
            var match = Regex.Match(printerMemory, pattern);
            
            if (Regex.IsMatch(printerMemory, pattern))
            {
                freeMemory = long.Parse(match.Groups[1].Value);
            }

            return freeMemory;
        }

        /// <summary>
        /// Loads all installed printers in the system
        /// </summary>
        /// <returns>Returns list of strings with names of installed printers</returns>
        public List<string> LoadInstalledPrinters()
        {
            LocalPrintServer printServer = new LocalPrintServer();
            PrintQueueCollection printQueuesOnServer = printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local });
            List<string> installedPrinters = new List<string>();
            foreach (PrintQueue printer in printQueuesOnServer)
            {
                installedPrinters.Add(printer.Name);
            }

            return installedPrinters;
        }
        #endregion

        /// <summary>
        /// Executes command in the target printer
        /// </summary>
        /// <param name="printerCommands">Commands to execute</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with connection details</param>
        /// <param name="waitForResponse">Should printing process wait for printer response and return relevant data?</param>
        /// <returns>Returns <see cref="PrintResult"/> with result details</returns>
        private PrintResult ExecuteCommand(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings, bool waitForResponse = false)
        {
            if (string.IsNullOrEmpty(printerCommands))
            {
                return new PrintResult()
                {
                    IsSuccessful = false,
                    Message = string.Format("{0} - {1}", Properties.Localization.Error, Properties.Localization.NoCommandToExecute)
                };
            }

            switch (printerSettings.CommunicationType)
            {
                case PrintServiceLibrary.CommunicationType.Network:
                    return this.PrintNetwork(printerCommands, printerSettings, waitForResponse);
                case PrintServiceLibrary.CommunicationType.Serial:
                    return this.PrintSerial(printerCommands, printerSettings);
                case PrintServiceLibrary.CommunicationType.Parallel:
                    return this.PrintParallel(printerCommands, printerSettings);
                case PrintServiceLibrary.CommunicationType.USB:
                    return this.PrintUSB(printerCommands, printerSettings.PrinterName, printerSettings.Copies, waitForResponse);
                default:
                    throw new NotImplementedException("This type of communication is not supported.");
            }
        }

        /// <summary>
        /// Checks if the target printer contains all necessary fonts
        /// </summary>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with printer connection details.</param>
        /// <param name="fontsToCheck">List of font names to check</param>
        /// <param name="memoryType"><see cref="PrinterMemoryType"/> location to look at</param>
        /// <returns>Returns true if all listed fonts are present in the target printer memory location, false otherwise</returns>
        private bool CheckIfPrinterContainsFonts(PrintServiceLibrary.PrinterSettings printerSettings, List<FontInfo> fontsToCheck, PrinterMemoryType memoryType)
        {
            if (fontsToCheck.Count <= 0)
            {
                return true;
            }

            var printerMemory = this.ListPrinterMemory(printerSettings, memoryType);

            foreach (var font in fontsToCheck)
            {
                if (!printerMemory.Contains(string.Format("{0}:{1}.FNT", (char)memoryType, font.ShortName.ToUpper())))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Does XML Label template needs data binding?
        /// </summary>
        /// <param name="xml">XML Label template to check</param>
        /// <returns>Returns true if data binding is needed, false otherwise</returns>
        private bool NeedsBinding(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return false;
            }

            var pattern = "DataField=\"([^\"]+)\"";

            return Regex.IsMatch(xml, pattern);
        }

        /// <summary>
        /// Gets full system font file path
        /// </summary>
        /// <param name="font"><see cref="FontInfo"/> structure with font info</param>
        /// <returns>Returns path to font file</returns>
        private string GetSystemFontFileName(FontInfo font)
        {
            // Get Windows Fonts folder
            DirectoryInfo dirWindowsFolder = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System));
            string strFontsFolder = Path.Combine(dirWindowsFolder.FullName, "Fonts");
            string fontname = string.Format("{0} (TrueType)", this.GetFontFullName(font, " "));
            RegistryKey fonts = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Fonts", false);
            if (fonts == null)
            {
                fonts = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Fonts", false);
                if (fonts == null)
                {
                    throw new Exception("Can't find font registry database.");
                }
            }

            foreach (string fntkey in fonts.GetValueNames())
            {
                if (fntkey == fontname)
                {
                    return strFontsFolder + "\\" + fonts.GetValue(fntkey).ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets font full name
        /// </summary>
        /// <param name="font"><see cref="FontInfo"/> structure with font info</param>
        /// <param name="separator">String separator to use</param>
        /// <returns>Returns font full name, including bold and italic suffixes</returns>
        private string GetFontFullName(FontInfo font, string separator)
        {
            List<string> fullname = new List<string>();
            fullname.Add(font.Name);

            if (font.Bold)
            {
                fullname.Add("Bold");
            }

            if (font.Italic)
            {
                fullname.Add("Italic");
            }

            return string.Join(separator, fullname);
        }
    }
}
