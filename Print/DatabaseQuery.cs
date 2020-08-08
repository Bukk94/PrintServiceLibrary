using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace PrintServiceLibrary
{
    /// <summary>
    /// Class that handles database connection and query results for Data Binding
    /// </summary>
    internal class DatabaseQuery
    {
        /// <summary>
        /// Verifies <see cref="DatabaseSettings"/> structure if it contains all connection details to create connection string
        /// </summary>
        /// <param name="dbSettings">Object with all connection details</param>
        /// <returns>Returns true or false</returns>
        internal bool VerifyConnectionString(DatabaseSettings dbSettings)
        {
            return !(string.IsNullOrEmpty(dbSettings.Host) || string.IsNullOrEmpty(dbSettings.Password) || string.IsNullOrEmpty(dbSettings.User) || string.IsNullOrEmpty(dbSettings.Password));
        }

        /// <summary>
        /// Creates database connection string based on given settings and credentials
        /// </summary>
        /// <param name="dbSettings">Object with all connection details</param>
        /// <returns>Returns database connection string</returns>
        internal string CreateDatabaseConnectionString(DatabaseSettings dbSettings)
        {
            return CreateDatabaseConnectionString(dbSettings.DatabaseType, dbSettings.Host, dbSettings.Port, dbSettings.ServiceName, dbSettings.User, dbSettings.Password);
        }

        /// <summary>
        /// Creates database connection string based on given settings and credentials
        /// </summary>
        /// <param name="databaseType">Database type based on <see cref="DatabaseType"/>, each database has different string structure</param>
        /// <param name="host">Database host or ip</param>
        /// <param name="port">Database port</param>
        /// <param name="serviceName">Service name (ee11 for example</param>
        /// <param name="user">User to connect as</param>
        /// <param name="password">Password to the database</param>
        /// <returns>Returns database connection string</returns>
        internal string CreateDatabaseConnectionString(DatabaseType databaseType, string host, ulong port, string serviceName, string user, string password)
        {
            switch (databaseType)
            {
                case DatabaseType.Oracle:
                    return  $"Data Source=(DESCRIPTION =" + 
                            $"(ADDRESS = (PROTOCOL = TCP)(HOST = {host})(PORT = {port}))" + 
                            $"(CONNECT_DATA =" + "(SERVER = DEDICATED)" + 
                            $"(SERVICE_NAME = {serviceName})));" + 
                            $"User Id={user};Password={password};";

                case DatabaseType.MSSQL:
                    return $"Data Source={host},{port};User Id={user};Password={password};";

                case DatabaseType.POSTGRE:
                    return $"User ID={user};Password={password};Host={host};Port={port};" +
                           $"Pooling=true;Min Pool Size=0;Max Pool Size=100;Connection Lifetime=0;";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Executes query and all returned values are placed into DataTable
        /// </summary>
        /// <param name="connectionString">Connection string to the database, must correspond to correct database type</param>
        /// <param name="query">Query to execute</param>
        /// <param name="databaseType">Database type based on <see cref="DatabaseType"/>, each database has different string structure</param>
        /// <returns>Returns <see cref="DataTable"/> with query results</returns>
        internal DataTable QueryToDataTable(string connectionString, string query, DatabaseType databaseType)
        {
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }

            switch (databaseType)
            {
                case DatabaseType.POSTGRE:
                    return QueryToDataTable(new Npgsql.NpgsqlConnection(connectionString), query);
                case DatabaseType.MSSQL:
                    return QueryToDataTable(new SqlConnection(connectionString), query);
                case DatabaseType.Oracle:
                    return QueryToDataTable(new OracleConnection(connectionString), query);
            }

            return null;
        }

        /// <summary>
        /// Executes MSSQL query
        /// </summary>
        /// <param name="sqlConnection">MSSQL Database connection</param>
        /// <param name="query">Query to execute</param>
        /// <returns>Returns <see cref="DataTable"/> with query results</returns>
        internal DataTable QueryToDataTable(SqlConnection sqlConnection, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }

            var cmd = new SqlCommand("SET FMTONLY OFF; SET NO_BROWSETABLE OFF; " + query, sqlConnection);
            cmd.CommandTimeout = 600;
            var adapter = new SqlDataAdapter();
            adapter.SelectCommand = cmd;
            var dataTable = new DataTable();

            try
            {
                cmd.Connection.Open();
                adapter.Fill(dataTable);
            }
            finally
            {
                cmd.Connection.Close();
            }

            return dataTable;
        }

        /// <summary>
        /// Executes Oracle query
        /// </summary>
        /// <param name="oracleConnection">Oracle Database connection</param>
        /// <param name="query">Query to execute</param>
        /// <returns>Returns <see cref="DataTable"/> with query results</returns>
        internal DataTable QueryToDataTable(OracleConnection oracleConnection, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }

            var cmd = new OracleCommand(query, oracleConnection);
            cmd.CommandTimeout = 600;
            var adapter = new OracleDataAdapter(); 
            adapter.SelectCommand = cmd;
            var dataTable = new DataTable();

            try
            {
                cmd.Connection.Open();
                adapter.Fill(dataTable);
            }
            finally
            {
                cmd.Connection.Close();
            }

            return dataTable;
        }

        /// <summary>
        /// Executes POSTGRE query
        /// </summary>
        /// <param name="npgsqlConnection">POSTGRE Database connection</param>
        /// <param name="query">Query to execute</param>
        /// <returns>Returns <see cref="DataTable"/> with query results</returns>
        internal DataTable QueryToDataTable(NpgsqlConnection npgsqlConnection, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }

            var cmd = new NpgsqlCommand(query, npgsqlConnection);
            cmd.CommandTimeout = 600;
            var adapter = new NpgsqlDataAdapter();
            adapter.SelectCommand = cmd;
            var dataTable = new DataTable();

            try
            {
                cmd.Connection.Open();
                adapter.Fill(dataTable);
            }
            finally
            {
                cmd.Connection.Close();
            }

            return dataTable;
        }

        /// <summary>
        /// Executes SQL query given by the <see cref="DatabaseSettings"/> data structure
        /// </summary>
        /// <param name="dbSettings"><see cref="DatabaseSettings"/> with all connection and query details</param>
        /// <returns>Returns DataTable with query results, null if the execution fails</returns>
        internal (DataTable dataTable, bool isSuccessful, string errorMessage) ExecuteSQL(DatabaseSettings dbSettings)
        {
            if (string.IsNullOrEmpty(dbSettings.Query))
            {
                return (null, false, string.Format("{0} - {1}", Properties.Localization.Error, Properties.Localization.EmptyQuery));
            }

            if (!this.VerifyConnectionString(dbSettings))
            {
                return (null, false, string.Format("{0} - {1}", Properties.Localization.Error, Properties.Localization.InvalidConnectionString));
            }

            DataTable queryDataTable = null;
            try
            {
                string connectionString = this.CreateDatabaseConnectionString(dbSettings);
                queryDataTable = this.QueryToDataTable(connectionString, dbSettings.Query, dbSettings.DatabaseType);
            }
            catch (Exception ex)
            {
                return (null, false, ex.Message);
            }

            queryDataTable.TableName = "Binding";

            int index = 0;
            var columns = queryDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName.ToLower()).ToList();
            foreach (DataColumn column in queryDataTable.Columns)
            {
                queryDataTable.Columns[index].ColumnName = column.ColumnName.ToLower();
                queryDataTable.AcceptChanges();
                index++;
            }

            return (queryDataTable, true, string.Empty);
        }
    }
}
