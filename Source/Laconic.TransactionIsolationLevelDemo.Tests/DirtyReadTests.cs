using System;
using System.Data;
using System.Threading;
using Laconic.TransactionIsolationLevelDemo.Tests.Extensions;
using NUnit.Framework;

namespace Laconic.TransactionIsolationLevelDemo.Tests
{
    [TestFixture]
    public class DirtyReadTests : TransactionTestFixtureBase
    {
        [TestCase(IsolationLevel.ReadUncommitted)]
        [TestCase(IsolationLevel.ReadCommitted)]
        [TestCase(IsolationLevel.RepeatableRead)]
        [TestCase(IsolationLevel.Serializable)]
        public void DirtyRead_IsolationLevel(IsolationLevel isolationLevel)
        {
            var readerThread = new Thread(Reader);
            readerThread.Start(isolationLevel);

            var writerThread = new Thread(Writer);
            writerThread.Start();

            readerThread.Join();
            writerThread.Join();
        }

        private static void Reader(object o)
        {
            var isolationLevel = (IsolationLevel) o;

            try
            {
                using (var connection = OpenConnection())
                {
                    using (var transaction = connection.BeginTransaction(isolationLevel))
                    {
                        ReaderLogger.Info($"Transaction begun. Isolation level = {isolationLevel}.");

                        Thread.Sleep(500);

                        ReaderLogger.Info(transaction.SelectMessage(1));

                        transaction.Commit();
                        ReaderLogger.Info("Transaction committed.");
                    }
                }
            }
            catch (Exception ex)
            {
                ReaderLogger.Error(ex);
            }
        }

        private static void Writer()
        {
            Thread.Sleep(250);

            try
            {
                using (var connection = OpenConnection())
                {
                    using (var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        WriterLogger.Info($"Transaction begun. Isolation level = {IsolationLevel.ReadUncommitted}.");
                        WriterLogger.Info("Updating {Id = 1, Text = \"Quack!\"}.");
                        transaction.UpdateMessage(1, "Quack!");

                        Thread.Sleep(500);

                        transaction.Commit();
                        WriterLogger.Info("Transaction committed.");
                    }
                }
            }
            catch (Exception ex)
            {
                WriterLogger.Error(ex);
            }
        }
    }
}
