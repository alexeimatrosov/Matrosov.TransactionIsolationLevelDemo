using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Laconic.TransactionIsolationLevel.Tests.Extensions
{
    public static class SqlTransactionExtensions
    {
        private const int CommandTimeout = 5;

        public static void InsertMessage(this SqlTransaction transaction, string text)
        {
            transaction.ExecuteNonQuery($"INSERT INTO [dbo].[Messages] ([Text]) VALUES (N'{text}')");
        }

        public static void UpdateMessage(this SqlTransaction transaction, int id, string text)
        {
            transaction.ExecuteNonQuery($"UPDATE [dbo].[Messages] SET [Text] = '{text}' WHERE [Id] = {id}");
        }

        public static Message SelectMessage(this SqlTransaction transaction, int id)
        {
            return transaction.SelectMessages($"SELECT [Id], [Text] FROM [dbo].[Messages] WHERE [Id] = {id}").FirstOrDefault();

        }

        public static Message[] SelectMessages(this SqlTransaction transaction)
        {
            return transaction.SelectMessages("SELECT [Id], [Text] FROM [dbo].[Messages]");
        }

        private static Message[] SelectMessages(this SqlTransaction transaction, string query)
        {
            using (var command = transaction.Connection.CreateCommand())
            {
                command.CommandText = query;
                command.Transaction = transaction;
                command.CommandTimeout = CommandTimeout;
                using (var reader = command.ExecuteReader())
                {
                    return EnumerateMessages(reader).ToArray();
                }
            }
        }

        private static IEnumerable<Message> EnumerateMessages(DbDataReader reader)
        {
            var enumerator = reader.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return new Message
                             {
                                 Id = reader.GetInt32(0),
                                 Text = reader.GetString(1)
                             };
            }
        }

        public static int ExecuteNonQuery(this SqlTransaction transaction, string commandText)
        {
            using (var command = transaction.Connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.Transaction = transaction;
                command.CommandTimeout = CommandTimeout;
                return command.ExecuteNonQuery();
            }
        }
    }
}