using Microsoft.Data.SqlClient;
using System.Runtime.InteropServices;

namespace SQLMigrationArm64
{
    internal class Program
    {
        private const string TempDbName = "TempDbForArm64Test";

        private static void Main(string[] args)
        {
            Console.WriteLine("LocalSQL access demo on ARM.");

            var connectionString = BuildConnectionString();

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            Console.WriteLine("Connection to LocalDB established successfully.");

            try
            {
                CreateTemporaryDatabase(connection);
                CreateTable(connection);
            }
            finally
            {
                DropTemporaryDatabase(connection);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static string BuildConnectionString()
        {
            var sqlBuilder = new SqlConnectionStringBuilder("Server=(localdb)\\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True;");

            if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                var localDbPipe = LocalDbHelper.GetLocalDbPipe("MSSQLLocalDB");
                sqlBuilder.DataSource = localDbPipe;
                sqlBuilder.TrustServerCertificate = true;
                Console.WriteLine("Using ARM64 adjusted connection string: " + sqlBuilder.ConnectionString);
            }
            else
            {
                Console.WriteLine("Using standard connection string. Its not ARM");
            }

            return sqlBuilder.ConnectionString;
        }

        private static void CreateTemporaryDatabase(SqlConnection connection)
        {
            ExecuteNonQuery(connection, $@"
IF DB_ID('{TempDbName}') IS NOT NULL
    DROP DATABASE [{TempDbName}];
CREATE DATABASE [{TempDbName}];
");
            Console.WriteLine("Temporary database created.");
        }

        private static void CreateTable(SqlConnection connection)
        {
            connection.ChangeDatabase(TempDbName);
            ExecuteNonQuery(connection, "CREATE TABLE TestTable (Id INT PRIMARY KEY, Name NVARCHAR(50));");
            Console.WriteLine("Table created in temporary database.");
        }

        private static void DropTemporaryDatabase(SqlConnection connection)
        {
            try
            {
                connection.ChangeDatabase("master");
                ExecuteNonQuery(connection, $"DROP DATABASE IF EXISTS [{TempDbName}];");
                Console.WriteLine("Temporary database dropped and connection closed.");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Failed to clean up temporary database: {ex.Message}");
            }
        }

        private static void ExecuteNonQuery(SqlConnection connection, string commandText)
        {
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.ExecuteNonQuery();
        }
    }
}
