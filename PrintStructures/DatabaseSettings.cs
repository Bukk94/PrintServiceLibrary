namespace PrintServiceLibrary
{
    /// <summary>
    /// Data structure for database settings
    /// </summary>
    public class DatabaseSettings
    {
        /// <summary>
        /// Database host name (or IP)
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Database port (default 1521)
        /// </summary>
        public ulong Port { get; set; } = 1521;

        /// <summary>
        /// Database service name (default ee11)
        /// </summary>
        public string ServiceName { get; set; } = "ee11";

        /// <summary>
        /// User to connect to
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// User's password to the database
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Query to run
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Database type
        /// </summary>
        public DatabaseType DatabaseType { get; set; }
    }
}
