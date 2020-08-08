using System;
using System.IO.Ports;
using System.Text;

namespace PrintServiceLibrary
{
    /// <summary>
    /// Class that handles serial (COM) connection to the printer
    /// </summary>
    public static class SerialPrint
    {
        /// <summary>
        /// Prints commands via Serial Port (RS-232)
        /// </summary>
        /// <param name="printerCommands">Commands to print (ZPL for example)</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with printer connection details. It must contain at least IP Address and port</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public static PrintResult Print(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings)
        {
            PrintResult printResult = new PrintResult();

            if (string.IsNullOrEmpty(printerCommands))
            {
                printResult.Message = string.Format("{0} - {1}", Properties.Localization.Error, Properties.Localization.NoCommandToExecute);
                printResult.IsSuccessful = false;
                return printResult;
            }

            if (string.IsNullOrEmpty(printerSettings.SerialPortName) || printerSettings.SerialPortBaudRate < 0 || printerSettings.SerialPortDataBits < 0)
            {
                printResult.Message = string.Format("{0} - {1} - {2}", Properties.Localization.Error, Properties.Localization.ErrorConnectToDesignatedPrinter, Properties.Localization.InvalidSerialData);
                printResult.IsSuccessful = false;
                return printResult;
            }

            if (printerSettings.SerialPortStopBits == StopBits.None)
            {
                printResult.Message = string.Format("{0} - {1}", Properties.Localization.Error, Properties.Localization.InvalidStopBits);
                printResult.IsSuccessful = false;
                return printResult;
            }

            // new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One)
            SerialPort serialPort = new SerialPort(printerSettings.SerialPortName, printerSettings.SerialPortBaudRate, printerSettings.SerialPortParity, printerSettings.SerialPortDataBits, printerSettings.SerialPortStopBits)
            {
                Handshake = printerSettings.SerialPortFlowControl
            };

            try
            {
                serialPort.Open();

                byte[] dataToSend = Encoding.UTF8.GetBytes(printerCommands);
                long bytesSent = 0;

                for (int i = 0; i < printerSettings.Copies; i++)
                {
                    serialPort.Write(dataToSend, 0, dataToSend.Length);
                    bytesSent += dataToSend.Length;
                }

                printResult.BytesSent = bytesSent;
            }
            catch (Exception ex)
            {
                printResult.Message = ex.Message;
                return printResult;
            }
            finally
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }

            printResult.IsSuccessful = true;

            return printResult;
        }
    }
}
