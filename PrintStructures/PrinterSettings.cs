namespace PrintServiceLibrary
{
    using System.IO;
    using System.IO.Ports;
    using System.Net;

    /// <summary>
    /// Structure containing all printer connection details.
    /// </summary>
    public class PrinterSettings
    {
        /// <summary>
        /// Default parameterless constructor
        /// </summary>
        public PrinterSettings()
        {
            this.DefaultInit();

            this.PrinterName = string.Empty;
            this.SerialPortName = "COM1";
            this.ParallelPortName = "LPT1";

            this.SerialPortBaudRate = 9600;
            this.SerialPortStopBits = StopBits.One;
            this.SerialPortDataBits = 8;
            this.SerialPortFlowControl = Handshake.XOnXOff;
            this.SerialPortParity = Parity.None;

            this.NetworkIPAddress = IPAddress.None;
            this.NetworkPort = 0;
            this.NetworkTimeout = 1500;

            this.CommunicationType = CommunicationType.USB;
        }

        /// <summary>
        /// Constructor for USB printing
        /// </summary>
        /// <param name="printerName">Name of the USB printer</param>
        public PrinterSettings(string printerName)
        {
            this.DefaultInit();

            this.PrinterName = printerName;
            this.CommunicationType = CommunicationType.USB;
        }

        /// <summary>
        /// Constructor for network (TCP/IP) printing
        /// </summary>
        /// <param name="ip">IP of target printer</param>
        /// <param name="port">port of target printer (default 9100)</param>
        /// <param name="timeout">Connection timeout (default 1500ms)</param>
        public PrinterSettings(IPAddress ip, int port = 9100, int timeout = 1500)
        {
            this.DefaultInit();

            this.NetworkIPAddress = ip;
            this.NetworkPort = port;
            this.NetworkTimeout = timeout;
            this.CommunicationType = CommunicationType.Network;
        }

        /// <summary>
        /// Constructor for LPT printing
        /// </summary>
        /// <param name="parallelPort">Name of the LPT port</param>
        /// <param name="copies">Number of copies to print</param>
        public PrinterSettings(string parallelPort, int copies)
        {
            this.DefaultInit();

            this.ParallelPortName = parallelPort;
            this.Copies = copies;
            this.CommunicationType = CommunicationType.Parallel;
        }

        /// <summary>
        /// Constructor for serial (COM) printing
        /// </summary>
        /// <param name="serialPortName">Name of the COM port</param>
        /// <param name="baudRate">Number of baud rate</param>
        /// <param name="stopBits">Stop bits</param>
        /// <param name="dataBits">Data bits</param>
        /// <param name="flowControl">Flow control</param>
        /// <param name="portParity">Port parity</param>
        public PrinterSettings(string serialPortName, int baudRate, StopBits stopBits, int dataBits, Handshake flowControl, Parity portParity)
        {
            this.DefaultInit();

            this.SerialPortName = serialPortName;

            this.SerialPortBaudRate = baudRate;
            this.SerialPortStopBits = stopBits;
            this.SerialPortDataBits = dataBits;
            this.SerialPortFlowControl = flowControl;
            this.SerialPortParity = portParity;

            this.CommunicationType = CommunicationType.Serial;
        }

        /// <summary>
        /// Number of copies to print
        /// </summary>
        public int Copies { get; set; }

        /// <summary>
        /// Use system default printer?
        /// </summary>
        public bool UseDefaultPrinter { get; set; }

        /// <summary>
        /// Use printers RAM for caching?
        /// </summary>
        public bool UsePrintersRAM { get; set; }

        /// <summary>
        /// Target programming language
        /// </summary>
        public ProgrammingLanguage ProgrammingLanguage { get; set; }

        /// <summary>
        /// Print orientation
        /// </summary>
        public PrintOrientation PrintOrientation { get; set; }

        /// <summary>
        /// Print DPI
        /// </summary>
        public double Dpi { get; set; }

        /// <summary>
        /// Print name for USB printing
        /// </summary>
        public string PrinterName { get; set; }

        /// <summary>
        /// Communication type
        /// </summary>
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// Name of the serial port for COM printing
        /// </summary>
        public string SerialPortName { get; set; }

        /// <summary>
        /// Baud rate for COM printing
        /// </summary>
        public int SerialPortBaudRate { get; set; }

        /// <summary>
        /// Port parity for COM printing
        /// </summary>
        public Parity SerialPortParity { get; set; }

        /// <summary>
        /// Stop bits for COM printing
        /// </summary>
        public StopBits SerialPortStopBits { get; set; }

        /// <summary>
        /// Data bits for COM printing
        /// </summary>
        public int SerialPortDataBits { get; set; }

        /// <summary>
        /// Handshake (port flow control) for COM printing
        /// </summary>
        public Handshake SerialPortFlowControl { get; set; }

        /// <summary>
        /// Parallel port name for LPT printing 
        /// </summary>
        public string ParallelPortName { get; set; }

        /// <summary>
        /// IP Address for network printing
        /// </summary>
        public IPAddress NetworkIPAddress { get; set; }

        /// <summary>
        /// Network port for network printing
        /// </summary>
        public int NetworkPort { get; set; }

        /// <summary>
        /// Network timeout for network printing
        /// </summary>
        public int NetworkTimeout { get; set; }

        /// <summary>
        /// Default initialization
        /// </summary>
        private void DefaultInit()
        {
            this.Dpi = 203.0;
            this.Copies = 1;
            this.PrintOrientation = PrintOrientation.Portrait;
            this.ProgrammingLanguage = ProgrammingLanguage.ZPL;
            this.UsePrintersRAM = false;
        }
    }
}
