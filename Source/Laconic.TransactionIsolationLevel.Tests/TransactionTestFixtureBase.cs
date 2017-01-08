using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using Laconic.TransactionIsolationLevel.Tests.Extensions;
using NLog;
using NUnit.Framework;

namespace Laconic.TransactionIsolationLevel.Tests
{
    public class TransactionTestFixtureBase
    {
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["Laconic.TransactionIsolationLevel.Tests"].ConnectionString;

        protected static Logger ReaderLogger = LogManager.GetLogger("Reader");
        protected static Logger WriterLogger = LogManager.GetLogger("Writer");

        [SetUp]
        public void SetUp()
        {
            using (var connection = OpenConnection())
            {
                connection.ExecuteNonQuery("IF OBJECT_ID('dbo.Messages', 'U') IS NOT NULL DROP TABLE [dbo].[Messages]");
                connection.ExecuteNonQuery("CREATE TABLE [dbo].[Messages] ([Id] INT NOT NULL PRIMARY KEY IDENTITY, [Text] NVARCHAR(MAX) NOT NULL)");
                connection.ExecuteNonQuery("INSERT INTO [dbo].[Messages] ([Text]) VALUES (N'Welcome!')");
            }
        }

        protected static SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}