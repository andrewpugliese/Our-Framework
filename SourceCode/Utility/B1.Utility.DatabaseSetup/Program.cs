using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

using B1.ILoggingManagement;
using B1.LoggingManagement;
using B1.Configuration;

namespace B1.Utility.DatabaseSetup
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new DbSetupMgr());
            }
            catch (Exception Exc)
            {
                HandleException(Exc);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender,
                    UnhandledExceptionEventArgs e)
        {
            HandleException((Exception)e.ExceptionObject);
        }

        private static void HandleException(Exception e)
        {
            try
            {
                LoggingMgr loggingMgr = new LoggingMgr(AppConfigMgr.GetValue(Configuration.Constants.LoggingKey).ToString());
                loggingMgr.WriteToLog(e, enumEventPriority.Critical);
            }
            finally
            {
                MessageBox.Show(e.Message + e.StackTrace);
            }
        }
    }
}
