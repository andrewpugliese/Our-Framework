using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using B1.LoggingManagement;
using B1.ILoggingManagement;

namespace B1.Utility.TestConsoleApp
{
    public class LoggingTest
    {
        private static Random _random = new Random();
        public static void RunTest()
        {
            // Uncomment the MSMQLog line out if you have MSMQ setup with a private queue called NOC
            // MemFileLogTest();

            FileLog backupFileLog = new FileLog("TestBackupLog", "", enumEventPriority.All );

            LoggingMgr lm = new LoggingMgr( 
                    new List<ILoggingTarget>() 
                    {
                        new WindowsEventLog( "Application", "LoggingTest", backupFileLog, enumEventPriority.Critical ),
                        new FileLog( "ApplicationLogFile", Environment.CurrentDirectory, enumEventPriority.All ),
                        //new FastFileLog( "FastLogFile_" + Process.GetCurrentProcess().Id.ToString(), 
                        new MemoryFileLog( "LoggingTestLog", "LogFile",
                                Environment.CurrentDirectory, 10000, enumEventPriority.All)
                        //new MSMQLog( "MSMQLog", ".\\Private$\\NOC", backupFileLog, enumEventPriority.Critical, 
                        //        enumEventPriority.Normal) 
                    }
                    , backupFileLog , enumTraceLevel.All );

            //This should log in the Windows Event Log and the file log at c:\applicationLogFile_*.txt
            lm.WriteToLog("Test Log 1: Critical", System.Diagnostics.EventLogEntryType.Information, 
                enumEventPriority.Critical);

            //This should log only in the file log at c:\applicationLogFile_*.txt
            lm.WriteToLog("Test Log 2: Normal", System.Diagnostics.EventLogEntryType.Information, 
                enumEventPriority.Normal);

            // Should only go in FileLog at c:\applicationLogFile_*.txt
            lm.Trace("Trace Token");

            // You can also construct the LoggingMgr with the following constructor. This will give you a default of
            //  windows event logging with a backup text file
            LoggingMgr lmDefault = new LoggingMgr("Application", "LoggingTest", "c:", enumTraceLevel.All );

            lmDefault.WriteToLog("Test Default Log", EventLogEntryType.Information, enumEventPriority.Normal);        
        }

        public static void TestTrace()
		{
            FileLog backupFileLog = new FileLog("TestBackupLog", "", enumEventPriority.All );

            LoggingMgr lm = new LoggingMgr( 
                    new List<ILoggingTarget>() 
                    {
                        new WindowsEventLog( "Application", "LoggingTest", backupFileLog, enumEventPriority.Critical ),
                    }, backupFileLog , enumTraceLevel.All );

            TraceLog log = new TraceLog();
            lm.TraceToWindow = true;

			int n = 0;
            while(true)
            {
                using(new LoggingContext("Running Trace Test"))
                {
                    lm.Trace(@"Number: " + n.ToString());
                    n++;

                    LoggingTest.TraceDeep1(lm);

                    LoggingTest.TraceLargeMessage(lm);
            
                    System.Threading.Thread.Sleep(1);
                }
            }
		}

        protected static void TraceDeep1(LoggingMgr lm)
        {
            using(new LoggingContext("TraceDeep1"))
            {
                System.Threading.Thread.Sleep(10);
                lm.Trace("Call To TraceDeep1");

                TraceDeep2(lm);
            }
        }

        protected static void TraceDeep2(LoggingMgr lm)
        {
            using(new LoggingContext("TraceDeep2"))
            {
                System.Threading.Thread.Sleep(10);
                lm.Trace("Call To TraceDeep2");
                TraceDeep3(lm);
            }
        }

        protected static void TraceDeep3(LoggingMgr lm)
        {
            using(new LoggingContext("TraceDeep3"))
            {
                System.Threading.Thread.Sleep(10);
                lm.Trace("Call To TraceDeep3.");
            }
        }
		
        protected static void TraceLargeMessage(LoggingMgr lm)
        {
            
            lm.Trace("RANDOM MESSAGE: ".PadRight(_random.Next(50, 200), (char)_random.Next(65, 90)));
        }

        public static void MemFileLogTest()
        {
            Random r = new Random();
            MemoryFileLog log = new MemoryFileLog( "MemFileLogTest", "LogFile",
                    Environment.CurrentDirectory, 10000, enumEventPriority.All);

            Action logAction = new Action( () =>
                {
                    for(int i = 0; i < 100; i++)
                    {
                        log.Write(string.Format("Thread {0} , Number {1}, Some text", Thread.CurrentThread.ManagedThreadId.ToString(),
                            r.Next()), true, 1, 1, EventLogEntryType.Information, enumEventPriority.Normal);
                        Thread.Sleep(1);
                    }
                });

            ThreadPool.SetMinThreads(50, 50);
            Parallel.Invoke(logAction, logAction, logAction, logAction, logAction, logAction, logAction, logAction, logAction, logAction);

            return;
        }
    }
}
