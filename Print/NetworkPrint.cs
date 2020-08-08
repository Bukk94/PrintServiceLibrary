using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PrintServiceLibrary
{
    /// <summary>
    /// Class that handles network (TCP/IP) connection to the printer
    /// </summary>
    public static class NetworkPrint
    {
        /// <summary>
        /// Prints commands via TCP Network connection
        /// </summary>
        /// <param name="printerCommands">Commands to print (ZPL for example)</param>
        /// <param name="printerSettings"><see cref="PrintServiceLibrary.PrinterSettings"/> structure with printer connection details. It must contain at least IP Address and port</param>
        /// <param name="waitForResponse">Should printing process wait for printer response and return relevant data?</param>
        /// <returns>Returns <see cref="PrintResult"/> with printing details</returns>
        public static PrintResult Print(string printerCommands, PrintServiceLibrary.PrinterSettings printerSettings, bool waitForResponse = false)
        {
            PrintResult printResult = new PrintResult();

            if (string.IsNullOrEmpty(printerCommands))
            {
                printResult.Message = string.Format("{0} - {1}", Properties.Localization.Error, Properties.Localization.NoCommandToExecute);
                printResult.IsSuccessful = false;
                return printResult;
            }

            if (printerSettings.NetworkIPAddress == null || printerSettings.NetworkPort <= 0)
            {
                printResult.Message = printResult.Message = string.Format("{0} - {1} - {2}", Properties.Localization.Error, Properties.Localization.ErrorConnectToDesignatedPrinter, Properties.Localization.NoIpAddressOrPort);
                printResult.IsSuccessful = false;
                return printResult;
            }

            IPEndPoint remoteEP = new IPEndPoint(printerSettings.NetworkIPAddress, printerSettings.NetworkPort);
            Socket sender = new Socket(printerSettings.NetworkIPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sender.Connect(remoteEP);

                byte[] dataToSend = Encoding.UTF8.GetBytes(printerCommands);
                long bytesSent = 0;

                for (int i = 0; i < printerSettings.Copies; i++)
                {
                    bytesSent += sender.Send(dataToSend);
                }

                if (waitForResponse)
                {
                    byte[] dataBuffer = new byte[1024];
                    string message = string.Empty;
                    int bytesReceieved;

                    do
                    {
                        bytesReceieved = sender.Receive(dataBuffer);
                        message += Encoding.UTF8.GetString(dataBuffer, 0, bytesReceieved);
                    }
                    while (sender.Available > 0 && bytesReceieved > 0);

                    printResult.Message = message;
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
                if (sender.Connected)
                {
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
            }

            printResult.IsSuccessful = true;

            return printResult;
        }
    }
}
