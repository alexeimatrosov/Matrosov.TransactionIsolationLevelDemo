using System;
using System.Data;
using System.Threading;
using Laconic.TransactionIsolationLevel.Tests.Extensions;
using NUnit.Framework;

namespace Laconic.TransactionIsolationLevel.Tests
{
    [TestFixture]
    public class DirtyReadTests : TransactionTestFixtureBase
    {
        [TestCase(IsolationLevel.ReadUncommitted)]
        [TestCase(IsolationLevel.ReadCommitted)]
        [TestCase(IsolationLevel.RepeatableRead)]
        [TestCase(IsolationLevel.Serializable)]
        public void DirtyRead_WithIsolationLevel(IsolationLevel isolationLevel)
        {
            var watchThread = new Thread(Watch);
            watchThread.Start(isolationLevel);

            var duckThread = new Thread(Duck);
            duckThread.Start();

            WatchTurn.Set();

            watchThread.Join();
            duckThread.Join();
        }

        private void Watch(object o)
        {
            var isolationLevel = (IsolationLevel) o;

            WatchTurn.WaitOne();

            try
            {
                using (var connection = OpenConnection())
                {
                    using (var transaction = connection.BeginTransaction(isolationLevel))
                    {
                        WatchLogger.Info($"Transaction started. Isolation level = {isolationLevel}.");
                        WatchLogger.Info(transaction.SelectMessage(1));

                        WatchYieldAndWait();

                        WatchLogger.Info(transaction.SelectMessage(1));

                        WatchYieldAndWait();

                        WatchLogger.Info(transaction.SelectMessage(1));

                        transaction.Commit();
                        WatchLogger.Info("Transaction commited.");
                    }
                }
            }
            catch (Exception ex)
            {
                WatchLogger.Error(ex);
            }
        }

        private void Duck()
        {
            DuckTurn.WaitOne();

            try
            {
                using (var connection = OpenConnection())
                {
                    using (var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        DuckLogger.Info($"Transaction started. Isolation level = {IsolationLevel.ReadUncommitted}.");

                        DuckLogger.Info("Updating {Id = 1, Text = \"Quack!\"}.");
                        transaction.UpdateMessage(1, "Quack!");

                        DuckYieldAndWait();

                        transaction.Rollback();
                        DuckLogger.Info("Transaction rolled back.");
                    }
                }

                WatchTurn.Set();
            }
            catch (Exception ex)
            {
                DuckLogger.Error(ex);
            }
        }
    }
}
