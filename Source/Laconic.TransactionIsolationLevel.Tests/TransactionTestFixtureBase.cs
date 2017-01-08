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
        private static readonly TimeSpan TurnTimeOut = TimeSpan.FromSeconds(5);
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["Laconic.TransactionIsolationLevel.Tests"].ConnectionString;

        protected static Logger WatchLogger = LogManager.GetLogger("Watch");
        protected static Logger DuckLogger = LogManager.GetLogger("Duck");

        protected AutoResetEvent WatchTurn;
        protected AutoResetEvent DuckTurn;

        [SetUp]
        public void SetUp()
        {
            WatchTurn = new AutoResetEvent(false);
            DuckTurn = new AutoResetEvent(false);

            using (var connection = OpenConnection())
            {
                connection.ExecuteNonQuery("IF OBJECT_ID('dbo.Messages', 'U') IS NOT NULL DROP TABLE [dbo].[Messages]");
                connection.ExecuteNonQuery("CREATE TABLE [dbo].[Messages] ([Id] INT NOT NULL PRIMARY KEY IDENTITY, [Text] NVARCHAR(MAX) NOT NULL)");
                connection.ExecuteNonQuery("INSERT INTO [dbo].[Messages] ([Text]) VALUES (N'Welcome!')");
            }
        }

        [TearDown]
        public void TearDown()
        {
            WatchTurn.Dispose();
            DuckTurn.Dispose();
        }

        protected void WatchYieldAndWait()
        {
            YieldAndWait(DuckTurn, WatchTurn);
        }

        protected void DuckYieldAndWait()
        {
            YieldAndWait(WatchTurn, DuckTurn);
        }

        private static void YieldAndWait(AutoResetEvent yieldTo, AutoResetEvent waitFor)
        {
            yieldTo.Set();
            var hasTurn = waitFor.WaitOne(TurnTimeOut);
            if (hasTurn == false)
            {
                throw new Exception("Waiting for next turn timed out.");
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