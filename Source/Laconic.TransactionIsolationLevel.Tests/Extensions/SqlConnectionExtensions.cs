using System.Data.SqlClient;

namespace Laconic.TransactionIsolationLevel.Tests.Extensions
{
    public static class SqlConnectionExtensions
    {
        public static int ExecuteNonQuery(this SqlConnection connection, string commandText)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                return command.ExecuteNonQuery();
            }
        }
    }
}