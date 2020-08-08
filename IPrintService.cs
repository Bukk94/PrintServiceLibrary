namespace PrintServiceLibrary
{
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Net;

    /// <summary>
    /// Main communication interface
    /// </summary>
    public interface IPrintService
    {
        /// <summary>
        /// Sends commands to the printers specified by the printer settings structure
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure containing all connection details</param>
        /// <param name="waitForResponse">Should printing process wait for printer response and return relevant data?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult Print(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings, bool waitForResponse = false);

        #region XML Template printing
        /// <summary>
        /// Prints label template via designated printer without any binding
        /// </summary>
        /// <param name="xmlTemplate">XML label template containing label layout</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure containing all connection details</param>
        /// <param name="uploadUsedFonts">Should API upload used fonts to the printer memory?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintXmlTemplate(string xmlTemplate, PrintServiceLibrary.PrinterSettings printerSettings, bool uploadUsedFonts = false);

        /// <summary>
        /// Prints label template via designated printer, including data binding
        /// </summary>
        /// <param name="xmlTemplate">XML label template containing label layout</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure containing all connection details</param>
        /// <param name="data">Data for data binding</param>
        /// <param name="dataSource">Data source type defined by <see cref="DataSourceType"/></param>
        /// <param name="uploadUsedFonts">Should API upload used fonts to the printer memory?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintXmlTemplate(string xmlTemplate, PrintServiceLibrary.PrinterSettings printerSettings, object data, DataSourceType dataSource = DataSourceType.None, bool uploadUsedFonts = false);
        #endregion

        #region Print via USB
        /// <summary>
        /// Sends commands via USB to ZEBRA printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="printerName">Target printer name</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintUSB(string printerCommands, string printerName);

        /// <summary>
        /// Sends commands via USB to ZEBRA printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="printerName">Target printer name</param>
        /// <param name="copies">Number of copies to print</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintUSB(string printerCommands, string printerName, int copies);

        /// <summary>
        /// Sends commands via USB to ZEBRA printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="printerName">Target printer name</param>
        /// <param name="copies">Number of copies to print</param>
        /// <param name="waitForResponse">Should printing process wait for printer response and return relevant data?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintUSB(string printerCommands, string printerName, int copies, bool waitForResponse);

        /// <summary>
        /// Sends commands via USB to ZEBRA printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with printer connection details. It must contain at least PrinterName.</param>
        /// <param name="waitForResponse">Should printing process wait for printer response and return relevant data?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintUSB(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings, bool waitForResponse = false);
        #endregion

        #region Print via Network
        /// <summary>
        /// Sends commands via Network connection to the printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="ip">Target printer IP address</param>
        /// <param name="portNumber">Target printer port number</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintNetwork(string printerCommands, IPAddress ip, int portNumber);

        /// <summary>
        /// Sends commands via Network connection to the printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="ip">Target printer IP address</param>
        /// <param name="portNumber">Target printer port number</param>
        /// <param name="copies">Number of copies to print</param>
        /// <param name="waitForResponse">Should printing process wait for printer response and return relevant data?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintNetwork(string printerCommands, IPAddress ip, int portNumber, int copies, bool waitForResponse = false);

        /// <summary>
        /// Sends commands via Network connection to the printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with printer connection details. It must contain at least IP address and port number.</param>
        /// <param name="waitForResponse">Should printing process wait for printer response and return relevant data?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintNetwork(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings, bool waitForResponse = false);
        #endregion

        #region Print via Serial Port (RS-232)
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
        PrintResult PrintSerial(string printerCommands, string serialPortName, int serialPortBaudRate, int serialPortDataBits, Handshake serialPortFlowControl, Parity parity, StopBits stopBits);

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
        /// <param name="copies">Number of copies to print</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintSerial(string printerCommands, string serialPortName, int serialPortBaudRate, int serialPortDataBits, Handshake serialPortFlowControl, Parity parity, StopBits stopBits, int copies);

        /// <summary>
        /// Sends commands via Serial (RS-232) connection to the printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with printer connection details.</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintSerial(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings);
        #endregion

        #region Print via Parallel Port
        /// <summary>
        /// Sends commands via Parallel connection to the printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="parallelPortName">Name of the parallel port (LPT1 for example)</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintParallel(string printerCommands, string parallelPortName);

        /// <summary>
        /// Sends commands via Parallel connection to the printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="parallelPortName">Name of the parallel port (LPT1 for example)</param>
        /// <param name="copies">Number of copies to print</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintParallel(string printerCommands, string parallelPortName, int copies);

        /// <summary>
        /// Sends commands via Parallel connection to the printer
        /// </summary>
        /// <param name="printerCommands">Printer commands (ZPL for example)</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with printer connection details. Must contain at least Parallel Port Name</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        PrintResult PrintParallel(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings);
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
        PrintResult UploadFontToPrinter(string fontPath, string fontNameInPrinter, PrinterMemoryType memoryType, PrintServiceLibrary.PrinterSettings printerSettings, bool waitForResponse = false);

        /// <summary>
        /// Lists all data in target printer memory
        /// </summary>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure containing all connection details</param>
        /// <param name="memoryType">Memory type defined by <see cref="PrinterMemoryType"/></param>
        /// <returns>Return memory string</returns>
        string ListPrinterMemory(PrintServiceLibrary.PrinterSettings printerSettings, PrinterMemoryType memoryType);

        /// <summary>
        /// Gets how much of free memory space is in the target printer memory
        /// </summary>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure containing all connection details</param>
        /// <param name="memoryType">Memory type defined by <see cref="PrinterMemoryType"/></param>
        /// <returns>Return number of free memory or -1 on error</returns>
        long GetPrinterFreeMemory(PrintServiceLibrary.PrinterSettings printerSettings, PrinterMemoryType memoryType);

        /// <summary>
        /// Get all installed printers on the system
        /// </summary>
        /// <returns>Return list of printer names</returns>
        List<string> LoadInstalledPrinters();
        #endregion

        #region Preview ZPL
        /// <summary>
        /// Creates ZPL preview based on the XML label template without any data binding
        /// </summary>
        /// <param name="xmlTemplate">XML label template containing label layout</param>
        /// <param name="dpi">DPI of the preview</param>
        /// <returns>Returns <see cref="PrintResult"/> with ZPL preview in Message property</returns>
        PrintResult PreviewZPLPrint(string xmlTemplate, double dpi);

        /// <summary>
        /// Creates ZPL preview based on the XML label template with complete data binding
        /// </summary>
        /// <param name="xmlTemplate">XML label template containing label layout</param>
        /// <param name="data">Data for data binding</param>
        /// <param name="dataSource">Data source type defined by <see cref="DataSourceType"/></param>
        /// <returns>Returns <see cref="PrintResult"/> with ZPL preview in Message property</returns>
        PrintResult PreviewZPLPrint(string xmlTemplate, double dpi, DataSourceType dataSource, object data);
        #endregion
    }
}
