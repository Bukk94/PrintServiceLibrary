namespace PrintServiceLibrary
{
    /// <summary>
    /// Print result info structure. Contains information about printer communication.
    /// </summary>
    public class PrintResult
    {
        /// <summary>
        /// Default parameterless constructor
        /// </summary>
        public PrintResult()
        {
            this.IsSuccessful = false;
            this.Message = string.Empty;
            this.BytesSent = 0;
        }

        /// <summary>
        /// Was print/command successful?
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Return message from the printer
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Total count of the bytes sent
        /// </summary>
        public long BytesSent { get; set; }

        /// <summary>
        /// Unit conversion from bytes to MB
        /// </summary>
        public double MegaBytesSent => this.BytesSent * 0.00000095367432;
    }
}
