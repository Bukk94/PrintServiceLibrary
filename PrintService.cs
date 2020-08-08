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
        private readonly NeodynamicWrapper _neodynamicWrapper;
        private readonly DataBinder _dataBinder;
        private readonly DatabaseQuery _databaseQuery;

        /// <summary>
        /// Default parameterless constructor
        /// </summary>
        public PrintService()
        {
            this._neodynamicWrapper = new NeodynamicWrapper();
            this._dataBinder = new DataBinder();
            this._databaseQuery = new DatabaseQuery();
        }

        /// <summary>
        /// Advanced constructor containing Neodynamic ThermalLabel SDK licenses
        /// </summary>
        /// <param name="NeodynamicLicence">Neodynamic ThermalLabel SDK owner and license</param>
        public PrintService((string owner, string licence) NeodynamicLicence)
        {
            this._neodynamicWrapper = new NeodynamicWrapper(NeodynamicLicence.owner, NeodynamicLicence.licence);
            this._dataBinder = new DataBinder();
            this._databaseQuery = new DatabaseQuery();
        }

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

        #region Print XML template
        /// <summary>
        /// Prints label template via designated printer without any binding
        /// </summary>
        /// <param name="xmlTemplate">XML label template containing label layout</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure containing all connection details</param>
        /// <param name="uploadUsedFonts">Should API upload used fonts to the printer memory?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public PrintResult PrintXmlTemplate(string xml, PrintServiceLibrary.PrinterSettings printerSettings, bool uploadUsedFonts = false)
        {
            return this.PrintXmlTemplate(xml, printerSettings, null, DataSourceType.None, uploadUsedFonts);

        }

        /// <summary>
        /// Prints label template via designated printer, including data binding
        /// </summary>
        /// <param name="xmlTemplate">XML label template containing label layout</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure containing all connection details</param>
        /// <param name="data">Data for data binding</param>
        /// <param name="dataSource">Data source type defined by <see cref="DataSourceType"/></param>
        /// <param name="uploadUsedFonts">Should API upload used fonts to the printer memory?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public PrintResult PrintXmlTemplate(string xml, PrintServiceLibrary.PrinterSettings printerSettings, object data, DataSourceType dataSource = DataSourceType.None, bool uploadUsedFonts = false)
        {
            PrintResult printResult = new PrintResult();

            if (string.IsNullOrEmpty(xml))
            {
                printResult.Message = string.Format("{0} - {1}", Properties.Localization.Error, Properties.Localization.NoXMLTemplate);
                printResult.IsSuccessful = false;
                return printResult;
            }

            if (!Utility.IsXmlValid(xml))
            {
                printResult.Message = string.Format("{0} - {1}", Properties.Localization.Error, Properties.Localization.InvalidXML);
                printResult.IsSuccessful = false;
                return printResult;
            }

            var printerCommands = string.Empty;
            string response = string.Empty;
            object dataSourceValues = null;

            if (uploadUsedFonts)
            {
                xml = this.UploadTemplateFontsToPrinter(printerSettings, xml, PrinterMemoryType.DRAM);
            }
            else
            {
                // Try to lookup all necessary fonts in printer. If one of the fonts is missing, print as graphic
                var templateFonts = this._neodynamicWrapper.SetupTemplateFonts(xml, out string modifiedXml);
                if (templateFonts.Count > 0 && this.CheckIfPrinterContainsFonts(printerSettings, templateFonts, PrinterMemoryType.DRAM))
                {
                    xml = modifiedXml;
                }
            }

            // Binding
            if (this.NeedsBinding(xml))
            {
                if (data == null)
                {
                    printResult.IsSuccessful = false;
                    printResult.Message = string.Format("{0}. {1}", Properties.Localization.BindingFailure, Properties.Localization.DataSourceIsNull);
                    return printResult;
                }

                if (dataSource == DataSourceType.Json && !string.IsNullOrEmpty(data.ToString()))
                {
                    var (dataSetXml, errorMessage) = this._dataBinder.BindJsonDataToDataSet(data.ToString());
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        printResult.Message = errorMessage;
                        printResult.IsSuccessful = false;
                        return printResult;
                    }

                    dataSourceValues = dataSetXml;
                }
                else if (dataSource == DataSourceType.Database)
                {
                    if (!(data is DatabaseSettings dbSettings))
                    {
                        printResult.IsSuccessful = false;
                        printResult.Message = Properties.Localization.MissingDBSettingStructure;
                        return printResult;
                    }

                    var (dataTable, isSuccessful, errorMessage) = this._databaseQuery.ExecuteSQL(dbSettings);
                    if (!isSuccessful)
                    {
                        return new PrintResult
                        {
                            Message = errorMessage,
                            IsSuccessful = false
                        };
                    }

                    dataSourceValues = dataTable;
                }
                else if (dataSource == DataSourceType.Xml && !string.IsNullOrEmpty(data.ToString()))
                {
                    dataSourceValues = this._dataBinder.BindXmlDataToDataSet(data.ToString());
                }
            }

            printerCommands = this.GeneratePrinterCommandsFromXml(xml, printerSettings.ProgrammingLanguage, (int)printerSettings.Dpi, dataSourceValues, printerSettings.UsePrintersRAM);

            switch (printerSettings.CommunicationType)
            {
                case PrintServiceLibrary.CommunicationType.Network:
                case PrintServiceLibrary.CommunicationType.Serial:
                case PrintServiceLibrary.CommunicationType.Parallel:
                case PrintServiceLibrary.CommunicationType.USB:
                    printResult = this.ExecuteCommand(printerCommands, printerSettings);
                    break;
                default:
                    printResult = this._neodynamicWrapper.Print(xml, printerSettings, dataSourceValues);
                    break;
            }

            return printResult;
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

        #region Print Previews
        /// <summary>
        /// Generates ZPL preview based on XML Label Template
        /// </summary>
        public PrintResult PreviewZPLPrint(string xmlTemplate, double dpi)
        {
            return this.PreviewLocalZPLPrint(xmlTemplate, dpi, DataSourceType.None, null);
        }

        public PrintResult PreviewZPLPrint(string xmlTemplate, double dpi, DataSourceType dataSource, object data)
        {
            return this.PreviewLocalZPLPrint(xmlTemplate, dpi, dataSource, data);
        }
        #endregion

        /// <summary>
        /// Binds dynamic values as constants for preview purpose
        /// </summary>
        /// <param name="xml">XML Label template layout</param>
        /// <returns>Returns modified XML label template with binded constants</returns>
        private string BindDynamicValuesAsConstants(string xml)
        {
            string pattern = "DataField=\"(.*?)\"";

            if (Regex.IsMatch(xml, pattern))
            {
                var matchCollection = Regex.Matches(xml, pattern);
                List<string> tags = new List<string>();

                foreach (Match match in matchCollection)
                {
                    tags.Add(match.Groups[1].Value);
                }

                tags.RemoveAll(x => string.IsNullOrEmpty(x));
                xml = this._dataBinder.ReplaceDataFieldsByValuesInXml(xml, tags, tags);
            }

            return xml;
        }

        /// <summary>
        /// Creates ZPL Preview based on XML Label template
        /// </summary>
        /// <param name="xmlTemplate">XML Label template with label layout</param>
        /// <param name="dpi">DPI of the output preview</param>
        /// <param name="dataSource">Data source type defined by <see cref="DataSourceType"/></param>
        /// <param name="data">Data source for data binding fields</param>
        /// <returns>Returns results of the preview, if successfull, ZPL preview string is in Message property</returns>
        private PrintResult PreviewLocalZPLPrint(string xmlTemplate, double dpi, DataSourceType dataSource, object data)
        {
            PrintResult printResult = new PrintResult();

            if (!Utility.IsXmlValid(xmlTemplate))
            {
                printResult.Message = string.Format("{0} - {1}", Properties.Localization.Error, Properties.Localization.InvalidXML);
                printResult.IsSuccessful = false;
                return printResult;
            }

            this._neodynamicWrapper.SetupTemplateFonts(xmlTemplate, out string modifiedXml);

            string zplPrinterCommands = this.GeneratePrinterCommandsFromXml(this.BindDynamicValuesAsConstants(modifiedXml), ProgrammingLanguage.ZPL, (int)dpi, null, false);

            if (this.NeedsBinding(xmlTemplate)) {
                if (dataSource == DataSourceType.Database)
                {
                    // DB Binding
                    if (!(data is DatabaseSettings dbSettings))
                    {
                        printResult.IsSuccessful = false;
                        printResult.Message = Properties.Localization.MissingDBSetting;
                        return printResult;
                    }

                    var (dataTable, isSuccessful, errorMessage) = this._databaseQuery.ExecuteSQL(dbSettings);
                    if (!isSuccessful)
                    {
                        printResult.Message = errorMessage;

                        return printResult;
                    }

                    zplPrinterCommands = this.GeneratePrinterCommandsFromXml(modifiedXml, ProgrammingLanguage.ZPL, (int)dpi, dataTable, false);
                }
                else if (dataSource == DataSourceType.Json && data != null && !string.IsNullOrEmpty(data.ToString()))
                {
                    // Json binding
                    var (dataSetXml, errorMessage) = this._dataBinder.BindJsonDataToDataSet(data.ToString());
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        printResult.Message = errorMessage;

                        return printResult;
                    }

                    zplPrinterCommands = this.GeneratePrinterCommandsFromXml(modifiedXml, ProgrammingLanguage.ZPL, (int)dpi, dataSetXml, false);
                }
                else if (dataSource == DataSourceType.Xml && data != null && !string.IsNullOrEmpty(data.ToString()))
                {
                    var dataSet = this._dataBinder.BindXmlDataToDataSet(data.ToString());
                    zplPrinterCommands = this.GeneratePrinterCommandsFromXml(modifiedXml, ProgrammingLanguage.ZPL, (int)dpi, dataSet, false);
                }
            }

            printResult.Message = zplPrinterCommands;
            printResult.IsSuccessful = true;

            return printResult;
        }

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
                    return this._neodynamicWrapper.ExecuteCommand(printerCommands, printerSettings);
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
        /// Generates printer commands from XML Label template
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="language"></param>
        /// <param name="dpi"></param>
        /// <param name="dataSource"></param>
        /// <param name="usePrintersRAM"></param>
        /// <returns></returns>
        private string GeneratePrinterCommandsFromXml(string xml, PrintServiceLibrary.ProgrammingLanguage language, int dpi, object dataSource, bool usePrintersRAM)
        {
            return this._neodynamicWrapper.GeneratePrinterCommandsFromXml(xml, language, dpi, dataSource, usePrintersRAM);
        }

        /// <summary>
        /// Uploads fonts used in the XML Label template to the printer
        /// </summary>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with printer connection details.</param>
        /// <param name="xmlTemplate">XML Label template containing all label data</param>
        /// <param name="memoryType">Memory type location where to upload the font</param>
        /// <returns>Returns modified XML Label template</returns>
        private string UploadTemplateFontsToPrinter(PrintServiceLibrary.PrinterSettings printerSettings, string xmlTemplate, PrinterMemoryType memoryType)
        {
            string zplCommand = string.Empty;
            var fonts = this._neodynamicWrapper.SetupTemplateFonts(xmlTemplate, out string modifiedXml);
            var printerMemory = this.ListPrinterMemory(printerSettings, PrinterMemoryType.DRAM);

            foreach (var font in fonts)
            {
                if (printerMemory.Contains(string.Format("{0}:{1}.FNT", (char)memoryType, font.ShortName.ToUpper())))
                {
                    continue;
                }

                var path = this.GetSystemFontFileName(font);

                byte[] fontByteArray = File.ReadAllBytes(path);
                var fontData = BitConverter.ToString(fontByteArray);
                fontData = fontData.Replace("-", string.Empty);

                zplCommand += string.Format("~DU{0}:{1}.FNT,{2},{3}", (char)memoryType, font.ShortName, fontByteArray.Length, fontData);
            }

            zplCommand += string.Format("^XA^HW{0}:^XZ", (char)memoryType);
            this.ExecuteCommand(zplCommand, printerSettings, true);

            return modifiedXml;
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
