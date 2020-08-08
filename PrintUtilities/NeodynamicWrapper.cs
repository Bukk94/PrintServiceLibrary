namespace PrintServiceLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Neodynamic wrapper is a layer between API communication & data and Neodynamic SDK.
    /// This class could re-implement all SDK core functions in the future (ZPL generation for example)
    /// </summary>
    internal class NeodynamicWrapper
    {
        private readonly string SDKLicenseOwner = "";
        private readonly string SDKLicenseKey = "";
        private Neodynamic.SDK.Printing.ThermalLabel _thermalLabel;

        /// <summary>
        /// Default parameterless constructor
        /// </summary>
        public NeodynamicWrapper()
        {
            Neodynamic.SDK.Printing.ThermalLabel.LicenseOwner = this.SDKLicenseOwner;
            Neodynamic.SDK.Printing.ThermalLabel.LicenseKey = this.SDKLicenseKey;
        }

        /// <summary>
        /// Advanced constructor containing Neodynamic ThermalLabel SDK licenses
        /// </summary>
        /// <param name="owner">License owner</param>
        /// <param name="licence">Lincense code</param>
        public NeodynamicWrapper(string owner, string licence)
        {
            Neodynamic.SDK.Printing.ThermalLabel.LicenseOwner = owner;
            this.SDKLicenseOwner = owner;
            Neodynamic.SDK.Printing.ThermalLabel.LicenseKey = licence;
            this.SDKLicenseKey = licence;
        }

        /// <summary>
        /// Generates printer commands based on XML Label template
        /// </summary>
        /// <param name="xml">XML Label template</param>
        /// <param name="programmingLanguage">Programming language in which commands will be generateed</param>
        /// <param name="dpi">DPI of the output label</param>
        /// <param name="dataSource">DataSource for data binding</param>
        /// <param name="usePrintersRAM">Should ZPL generator bind images from memory? If false, bitmap ZPL will be generated</param>
        /// <returns>Returns printer commands representing given label template</returns>
        internal string GeneratePrinterCommandsFromXml(string xml, PrintServiceLibrary.ProgrammingLanguage programmingLanguage, int dpi, object dataSource, bool usePrintersRAM)
        {
            string printerCommands = string.Empty;
            this._thermalLabel = Neodynamic.SDK.Printing.ThermalLabel.CreateFromXmlTemplate(xml);

            if (dataSource != null)
            {
                this.RemoveEmptyBindings(this._thermalLabel, dataSource);
                this._thermalLabel.DataSource = dataSource;
            }

            using (Neodynamic.SDK.Printing.PrintJob pj = new Neodynamic.SDK.Printing.PrintJob())
            {
                pj.Dpi = dpi;
                pj.ProgrammingLanguage = (Neodynamic.SDK.Printing.ProgrammingLanguage)Enum.Parse(typeof(Neodynamic.SDK.Printing.ProgrammingLanguage), programmingLanguage.ToString());
                printerCommands = pj.GetNativePrinterCommands(this._thermalLabel);
            }

            return printerCommands;
        }

        /// <summary>
        /// Setups fonts in XML label template
        /// </summary>
        /// <param name="xml">Input XML Label template</param>
        /// <param name="modifiedXml">Outputs modified XML Label template</param>
        /// <returns>Returns list of used fonts</returns>
        internal List<FontInfo> SetupTemplateFonts(string xml, out string modifiedXml)
        {
            this._thermalLabel = Neodynamic.SDK.Printing.ThermalLabel.CreateFromXmlTemplate(xml);

            List<FontInfo> fonts = new List<FontInfo>();
            List<FontInfo> distinctFonts = new List<FontInfo>();

            // Set font names to objects (user can't do that, yet)
            foreach (var item in this._thermalLabel.Items)
            {
                if (item is Neodynamic.SDK.Printing.TextItem txtItem)
                {
                    FontInfo newFont = new FontInfo()
                    {
                        Name = txtItem.Font.Name,
                        Bold = txtItem.Font.Bold,
                        Italic = txtItem.Font.Italic
                    };

                    fonts.Add(newFont);
                    txtItem.Font.NameAtPrinterStorage = newFont.ShortName + ".FNT";
                }
            }

            foreach (var font in fonts)
            {
                if (!distinctFonts.Any(x => x.Name == font.Name && x.Bold == font.Bold && x.Italic == font.Italic))
                {
                    distinctFonts.Add(font);
                }
            }

            modifiedXml = this._thermalLabel.GetXmlTemplate();

            string settingsPattern = "<Settings.*\\/>";

            if (Regex.IsMatch(xml, settingsPattern))
            {
                if (this._thermalLabel.Items.Count == 0)
                {
                    var header = modifiedXml.Substring(0, modifiedXml.IndexOf("/>") - 1);
                    modifiedXml = string.Format("{0}>\n{1}\n</ThermalLabel>", header, Regex.Match(xml, settingsPattern).Value);
                }
                else
                {
                    var header = modifiedXml.Substring(0, modifiedXml.IndexOf("<Items>") - 1);
                    var itemsAndFooter = modifiedXml.Substring(modifiedXml.IndexOf("<Items>"));
                    modifiedXml = string.Format("{0}{1}\n{2}", header, Regex.Match(xml, settingsPattern).Value, itemsAndFooter);
                }
            }

            return distinctFonts;
        }

        /// <summary>
        /// Executes command in the target printer
        /// </summary>
        /// <param name="data">Data to execute</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with connection details</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        internal PrintResult ExecuteCommand(string data, PrintServiceLibrary.PrinterSettings printerSettings)
        {
            PrintResult printResult = new PrintResult();

            try
            {
                printResult.BytesSent = Encoding.UTF8.GetBytes(data).Length;
                Neodynamic.SDK.Printing.PrintUtils.ExecuteCommand(data, this.ConvertCustomSettingsToNeodynamic(printerSettings));
                printResult.IsSuccessful = true;
            }
            catch (Exception ex)
            {
                printResult.Message = ex.Message;
            }

            return printResult;
        }

        /// <summary>
        /// Prints XML Label template using Neodynamic SDK. 
        /// This method will be used only in rare occasions. Usually API handles printing on its own.
        /// </summary>
        /// <param name="xml">Input XML Label template</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with connection details</param>
        /// <param name="dataSource">Data source for data binding</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        internal PrintResult Print(string xml, PrintServiceLibrary.PrinterSettings printerSettings, object dataSource)
        {
            PrintResult printResult = new PrintResult();
            var neodynamicPrinterSettings = this.ConvertCustomSettingsToNeodynamic(printerSettings);

            using (Neodynamic.SDK.Printing.WindowsPrintJob printJob = new Neodynamic.SDK.Printing.WindowsPrintJob(neodynamicPrinterSettings))
            {
                var printThermalLabel = Neodynamic.SDK.Printing.ThermalLabel.CreateFromXmlTemplate(xml);
                
                printJob.Copies = printerSettings.Copies;
                printJob.PrintOrientation = (Neodynamic.SDK.Printing.PrintOrientation)Enum.Parse(typeof(Neodynamic.SDK.Printing.PrintOrientation), printerSettings.PrintOrientation.ToString());

                if (dataSource != null)
                {
                    this.RemoveEmptyBindings(printThermalLabel, dataSource);
                    printThermalLabel.DataSource = dataSource;
                }

                printJob.ThermalLabel = printThermalLabel;

                try
                {
                    string printerCommands = string.Empty;
                    using (Neodynamic.SDK.Printing.PrintJob pj = new Neodynamic.SDK.Printing.PrintJob())
                    {
                        pj.ProgrammingLanguage = neodynamicPrinterSettings.ProgrammingLanguage;
                        printerCommands = pj.GetNativePrinterCommands(this._thermalLabel);
                    }

                    printResult.BytesSent = Encoding.UTF8.GetBytes(printerCommands).Length;
                    printJob.Print();
                    printResult.IsSuccessful = true;
                }
                catch (Exception ex)
                {
                    printResult.Message = ex.Message;
                }
            }

            return printResult;
        }

        /// <summary>
        /// Removes all fields that contains data binding but data source doesn't provide any actual data for it
        /// </summary>
        /// <param name="thermalLabel">Neodynamic template</param>
        /// <param name="dataSource">Data source with data to bind</param>
        private void RemoveEmptyBindings(Neodynamic.SDK.Printing.ThermalLabel thermalLabel, object dataSource)
        {
            if (dataSource is DataTable dt)
            {
                var dataColumns = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName.ToLower()).ToList();
                foreach (var item in thermalLabel.Items.ToList())
                {
                    if (!string.IsNullOrEmpty(item.DataField) && !dataColumns.Contains(item.DataField.ToLower()))
                    {
                        thermalLabel.Items.Remove(item);
                    }
                }
            }
        }

        /// <summary>
        /// Converts API PrinterSettings to Neodynamic SDK Settings
        /// </summary>
        /// <param name="printerSettings">API PrinterSettings to convert</param>
        /// <returns>Returns Neodynamic SDK PrinterSettings</returns>
        private Neodynamic.SDK.Printing.PrinterSettings ConvertCustomSettingsToNeodynamic(PrintServiceLibrary.PrinterSettings printerSettings)
        {
            var neodynamicPrinterSettings = new Neodynamic.SDK.Printing.PrinterSettings();
            neodynamicPrinterSettings.Dpi = printerSettings.Dpi;
            neodynamicPrinterSettings.PrinterName = printerSettings.PrinterName;
            neodynamicPrinterSettings.ProgrammingLanguage = (Neodynamic.SDK.Printing.ProgrammingLanguage)Enum.Parse(typeof(Neodynamic.SDK.Printing.ProgrammingLanguage), printerSettings.ProgrammingLanguage.ToString());
            neodynamicPrinterSettings.UseDefaultPrinter = printerSettings.UseDefaultPrinter;
            neodynamicPrinterSettings.Communication.CommunicationType = (Neodynamic.SDK.Printing.CommunicationType)Enum.Parse(typeof(Neodynamic.SDK.Printing.CommunicationType), printerSettings.CommunicationType.ToString());
            neodynamicPrinterSettings.Communication.NetworkIPAddress = printerSettings.NetworkIPAddress;
            neodynamicPrinterSettings.Communication.NetworkPort = printerSettings.NetworkPort;
            neodynamicPrinterSettings.Communication.NetworkTimeout = printerSettings.NetworkTimeout;
            neodynamicPrinterSettings.Communication.ParallelPortName = printerSettings.ParallelPortName;
            neodynamicPrinterSettings.Communication.SerialPortBaudRate = printerSettings.SerialPortBaudRate;
            neodynamicPrinterSettings.Communication.SerialPortDataBits = printerSettings.SerialPortDataBits;
            neodynamicPrinterSettings.Communication.SerialPortFlowControl = (Neodynamic.SDK.Printing.SerialPortHandshake)Enum.Parse(typeof(Neodynamic.SDK.Printing.SerialPortHandshake), printerSettings.SerialPortFlowControl.ToString());
            neodynamicPrinterSettings.Communication.SerialPortName = printerSettings.SerialPortName;
            neodynamicPrinterSettings.Communication.SerialPortParity = (Neodynamic.SDK.Printing.SerialPortParity)Enum.Parse(typeof(Neodynamic.SDK.Printing.SerialPortParity), printerSettings.SerialPortParity.ToString());
            neodynamicPrinterSettings.Communication.SerialPortStopBits = (Neodynamic.SDK.Printing.SerialPortStopBits)Enum.Parse(typeof(Neodynamic.SDK.Printing.SerialPortStopBits), printerSettings.SerialPortStopBits.ToString());

            return neodynamicPrinterSettings;
        }
    }
}