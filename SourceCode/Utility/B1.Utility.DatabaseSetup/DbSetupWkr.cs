using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;

using B1.Configuration;
using B1.Core;
using B1.DataAccess;
using B1.FileManagement;
using B1.ILoggingManagement;

namespace B1.Utility.DatabaseSetup
{
    /// <summary>
    /// DbSetupWkr (Database Setup Worker)
    /// <para>
    /// This is the main class for the setup utility to perform the back-end specific processing
    /// in order to create a new database or alter an existing database.
    /// </para>
    /// <para>
    /// This class will use the back-end specific command line tools for executing ddl statements:
    /// </para>
    /// <list type="bullet">
    /// <item>
    ///     SqlServer: uses sqlcmd (inside cmd.exe)
    /// </item>
    /// <item>
    ///     Oracle: uses sqlplus (inside cmd.exe)
    /// </item>
    /// <item>
    ///     Db2: uses db2 (under db2cmd environment; which runs inside cmd.exe)
    /// </item>
    ///</list>
    /// <para>
    /// There is a single constructor for this class <see cref="B1.Utility.DatabaseSetup.DbSetupWkr"/>
    /// </para>
    /// <para>
    /// Please see Getting Started for more information. 
    /// </para>
    /// </summary>
    internal class DbSetupWkr
    {
        string _dbServer;
        string _dbName;
        DataAccessMgr.EnumDbType _dbType;
        string _userName;
        string _userPassword;
        bool _asSysDba = false;
        string _inputFileName;
        string _outputFileName;
        string _ddlSourceDir;
        DbSetupParamsCollection _macroParams;
        bool _exit = false;
        bool _pause = false;
        internal delegate void DelegateDbSetupCompleted(String results, bool aborted, TimeSpan timespan);
        internal DelegateDbSetupCompleted _dbSetupCompletedDelegate;
        StringBuilder _results = new StringBuilder();

        /// <summary>
        /// Main constructor for class.
        /// Processing will not begin until Start is called.
        /// </summary>
        /// <param name="dbServer">Database Server Name for SqlServer; TNS ServiceName for Oracle; Blank for DB2</param>
        /// <param name="dbName">Database Name that will be created and altered</param>
        /// <param name="userName">User with privileges to perform the DDL operations</param>
        /// <param name="userPassword">Password for user</param>
        /// <param name="dbType">Enumeration of DbTypes (e.g. Oracle, SqlServer, Db2)</param>
        /// <param name="asSysDba">For Oracle only indicates if user should run as SysDBA</param>
        /// <param name="inputFilename">Filename and path relative to ddlSourceDir to parse;  
        /// File needs to be a utility command file</param>
        /// <param name="outputFilename">Filename and path relative to ddlSourceDir to output results</param>
        /// <param name="ddlSourceDir">Full directory path where the input/output and ddl files can be found</param>
        /// <param name="macroParams">Contains the collection of parameters for macro substitution when parsing a command file</param>
        /// <param name="dbSetupCompleted">Delegate to call when processing is completed.</param>
        internal DbSetupWkr(String dbServer
                , String dbName
                , String userName
                , String userPassword
                , DataAccessMgr.EnumDbType dbType
                , Boolean asSysDba
                , String inputFilename
                , String outputFilename
                , String ddlSourceDir
                , DbSetupParamsCollection macroParams
                , DelegateDbSetupCompleted dbSetupCompleted)
        {
            _dbServer = dbServer;
            _dbName = dbName;
            _dbType = dbType;
            _asSysDba = asSysDba;
            _userName = userName;
            _userPassword = userPassword;
            _ddlSourceDir = ddlSourceDir;
            _inputFileName = ddlSourceDir + "\\" + inputFilename;
            _outputFileName = ddlSourceDir + "\\" + outputFilename;
            _macroParams = macroParams;

            _dbSetupCompletedDelegate = dbSetupCompleted;
        }

        /// <summary>
        /// Begins the asynchronous process of parsing the command file
        /// and processing the embedded DDL files which are referenced in the command file.
        /// Files are processed using following code:
        /// <code>
        /// ProcessStartInfo psi = new ProcessStartInfo();
        /// string processBatchFile = "ddl.bat";
        /// if (_dbType == DbAccessManager.EnumDbType.Db2)
        /// {
        ///     psi.FileName = "db2cmd.exe";
        ///     psi.Arguments = "-c -w -i ddl.bat";
        /// }
        /// else
        /// {
        ///     psi.FileName = "cmd.exe";
        ///     psi.Arguments = "/C ddl.bat";
        /// }
        /// </code>
        /// </summary>
        internal void Start()
        {
            bool aborted = false;
            DateTime batchStart = DateTime.Now;
            TimeSpan totalBatch = new TimeSpan(0, 0, 0);
            try
            {

                if (!File.Exists(_inputFileName))
                    throw new ExceptionEvent(enumExceptionEventCodes.FileNotFound
                            , string.Format("Error; File: {0} could not be found.{1}"
                                    , _inputFileName
                                    , Environment.NewLine));

                if (File.Exists(_outputFileName))
                {
                    File.SetAttributes(_outputFileName, FileAttributes.Normal);
                    File.Delete(_outputFileName);
                }

                ProcessStartInfo psi = new ProcessStartInfo();
                string processBatchFile = "ddl.bat";
                if (_dbType == DataAccessMgr.EnumDbType.Db2)
                {
                    psi.FileName = "db2cmd.exe";
                    psi.Arguments = "-c -w -i ddl.bat";
                }
                else
                {
                   psi.FileName = "cmd.exe";
                   psi.Arguments = "/C ddl.bat";
                }
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process cmdProcess = new Process();
                cmdProcess.StartInfo = psi;
 
                string msg = string.Format("Batch Started: {0}{1}{1}"
                                , batchStart
                                , Environment.NewLine);
                FileMgr.WriteTextToFile(_outputFileName
                        , msg
                        , true
                        , true);
                _results.Append(msg);

                aborted = ProcessFile(cmdProcess, processBatchFile, _inputFileName);

                DateTime batchEnd = DateTime.Now;
                totalBatch = batchEnd - batchStart;
                msg = string.Format("{3}{3}Batch {0} {1}; Total Time: {2} seconds{3}"
                        , aborted ? "Aborted:" : "Completed:"
                        , batchEnd
                        , totalBatch.TotalSeconds
                        , Environment.NewLine);
                FileMgr.WriteTextToFile(_outputFileName
                        , msg
                        , true
                        , true);
                _results.Append(msg);

            }
            catch (Exception Exc)
            {
                _results.Append(Exc.Message + Exc.StackTrace);
                totalBatch = DateTime.Now - batchStart;
                aborted = true;
            }
            finally
            {
                // let caller know the process is completed
                _dbSetupCompletedDelegate(_results.ToString(), aborted, totalBatch);
            }
        }

        /// <summary>
        /// The function creates a ddl.bat file which calls the back-end specific
        /// command line tool.
        /// </summary>
        /// <param name="processCmd">OS Process Command Object</param>
        /// <param name="processBatchFile">The batch file that will call the command line tool</param>
        /// <param name="cmdFileName">The command file name to parse</param>
        /// <returns>True indicates success; false indicates an error</returns>
        bool ProcessFile(Process processCmd, string processBatchFile, string cmdFileName)
        {
            bool insideCommentBlock = false;
            using (StreamReader sr = new StreamReader(cmdFileName))
            {
                _results.Length = 0;
                string msg = string.Format("Parsing Command File: {0}{1}"
                        , cmdFileName
                        , Environment.NewLine);
                _results.Append(msg);
                FileMgr.WriteTextToFile(_outputFileName
                        , msg
                        , true
                        , true);

                string line = sr.ReadLine();
                while (!_exit && line != null)
                {
                    while (_pause && !_exit) // have we been paused
                        Thread.Sleep(100);

                    line = line.Trim();
                    if (!insideCommentBlock
                        && !line.StartsWith(Constants.CommentLineStart)
                        && !line.StartsWith(Constants.CommentBlockEnd)
                        && !line.StartsWith(Constants.CommentBlockStart)
                        && line.Length > 0)
                    {
                        // check for the BreakWithMsg keyword
                        if (line.ToLower().StartsWith(Constants.BreakWithMsg.ToLower()))
                        {
                            // display the message and wait for user input
                            if (ContinueAfterPause(line))
                            {
                                line = sr.ReadLine();
                                continue;
                            }
                            else
                            {
                                _exit = true;
                                return true;
                            }
                        }

                        if (line.Contains('{') && line.Contains('}'))
                            line = ReplaceMacros(line);

                        // check for the call keyword to process another command file
                        if (line.ToLower().StartsWith(Constants.RunCmdFile.ToLower()))
                        {
                            bool aborted = ProcessFile(processCmd, processBatchFile, _ddlSourceDir + "\\" 
                                    + line.ToLower().Replace(
                                        Constants.RunCmdFile.ToLower(), "").Trim());
                            // check to see if we were aborted
                            if (aborted)
                                return aborted;
                            line = sr.ReadLine();
                            continue;
                        }

                        bool serverOnly = false;
                        // check for the call keyword to process at server level only
                        if (line.ToLower().StartsWith(Constants.ServerOnly.ToLower()))
                        {
                            serverOnly = true;
                            line = line.ToLower().Replace(Constants.ServerOnly.ToLower(), "").Trim();
                        }

                        string ddlObjectFile = _ddlSourceDir + "\\" + line;
                        if (!File.Exists(ddlObjectFile)) // line is script filename
                            FileMgr.WriteTextToFile(_outputFileName, string.Format("Error; File: {0} could not be found. {1}"
                                    , ddlObjectFile, Environment.NewLine), true, true);
                        else
                        {
                            string cmdOutputFile = "cmdOutput.txt";
                            GenerateDDLBatch(ddlObjectFile, processBatchFile, serverOnly, cmdOutputFile);
                            ProcessDDLBatch(processCmd);
                            string results = null;
                            if (File.Exists(cmdOutputFile))
                            {
                                results = FileMgr.ReadTextFileIntoString(cmdOutputFile);
                                if (!string.IsNullOrEmpty(results))
                                {
                                    _results.Append(results);
                                    FileMgr.WriteTextToFile(_outputFileName, results, true, true);
                                    _results.Append(results);
                                }
                            }
                            else // there was some environment setup issue, because there was no cmdOutputFile found
                            {
                                results = string.Format("{0}Error; there was no output produced by the command.{0}"
                                        + " Possible errors include that the command line utility could not be found.{0}"
                                        + " Or that the listener for the database is not up or configured for tcpip.{0}"
                                        + " Or the input credentials are not correct.{0}"
                                        + " You can try executing the command listed from a dos prompt and viewing the results.{0}"
                                        + " Please refer to the GettingStarted document.{0}"
                                        , Environment.NewLine);
                                FileMgr.WriteTextToFile(_outputFileName, results, true, true);
                                _results.Append(results);
                                return true; // abort the process
                            }
                        }
                    }
                    if (line.StartsWith(Constants.CommentBlockStart))
                        insideCommentBlock = true;
                    if (line.StartsWith(Constants.CommentBlockEnd))
                        insideCommentBlock = false;
                    line = sr.ReadLine();
                }
            }
            return _exit ? true : false;
        }

        /// <summary>
        /// Function substitutes the macro ({name}) with the value found
        /// from the config file.
        /// </summary>
        /// <param name="line">The line of text to search for macros</param>
        /// <returns>The line with the macros replaced</returns>
        string ReplaceMacros(string line)
        {
            while (line.Contains('{'))
            {
                int index = line.IndexOf('{') ;
                string macro = line.Substring(index, line.IndexOf('}') - index + 1);
                string macroVar = line.Substring(index + 1, line.IndexOf('}') - 1 - index );
                string value = _macroParams[macroVar].ParamValue;
                line = line.Replace(macro, value);
            }
            return line;
        }

        /// <summary>
        /// Displays a modal dialog box with the message found in the given line.
        /// Returns the response of the user to continue or not.
        /// </summary>
        /// <param name="line">Line containing message</param>
        /// <returns>True to continue; false to cancel</returns>
        bool ContinueAfterPause(string line)
        {
            if (System.Windows.Forms.MessageBox.Show(string.Format(
                        "{0}.  Press Yes to continue processing; No to end.", line.Replace(Constants.Pause, ""))
                        , "Break With Message", System.Windows.Forms.MessageBoxButtons.YesNo)
                    == System.Windows.Forms.DialogResult.Yes)
                return true;
            else return false;
        }

        /// <summary>
        /// Function creates the ddl.bat file with the appropriate command line tool and parameters.
        /// </summary>
        /// <param name="ddlFile">The input file to pass into the command line tool</param>
        /// <param name="ddlBatchFile">The name of the batch file to create</param>
        /// <param name="serverOnly">Boolean indicating whether or not to connect only to the server (not the database);
        /// For example on a create database command.</param>
        /// <param name="cmdOutputFile">The output file name.</param>
        private void GenerateDDLBatch(string ddlFile, string ddlBatchFile, bool serverOnly, string cmdOutputFile)
        {
            string msg = string.Format("Executing Script File: {0}{1}"
                    , ddlFile
                    , Environment.NewLine);
            _results.Append(msg);

            FileMgr.WriteTextToFile(_outputFileName
                    , msg
                    , true
                    , true);

            // Generate the BatchFile contents
            StringBuilder cmdScript = new StringBuilder();
            string cmd = null;
            if (_dbType == DataAccessMgr.EnumDbType.SqlServer)
            {
                cmd = string.Format("sqlcmd -S {0} {1} {2} -i \"{3}\" -o {4}"
                        , _dbServer
                        , (string.IsNullOrEmpty(_userName)
                            || string.IsNullOrEmpty(_userPassword))
                            ? "" : string.Format("-U{0} -P{1}", _userName, _userPassword)
                        , string.IsNullOrEmpty(_dbName) || serverOnly
                            ? "" : string.Format("-d {0} ", _dbName)
                        , ddlFile
                        , cmdOutputFile);
                // record to results file indented under filename
                FileMgr.WriteTextToFile(_outputFileName
                        , string.Format("    {0}{1}", cmd, Environment.NewLine), true, true);
            }
            else if (_dbType == DataAccessMgr.EnumDbType.Oracle)
            {
                // for Oracle we need to issue a spool command for the output
                // so we will be wrapping the actual script file with another file
                // which will issue the spool and exit commands

                string cmdWrapperFile = "cmdWrapper.sql";
                StringBuilder wrapper = new StringBuilder();
                wrapper.AppendFormat("spool {0}{1}", cmdOutputFile, Environment.NewLine);
                wrapper.AppendFormat("whenever sqlerror exit sql.sqlcode{0}", Environment.NewLine);
                wrapper.AppendFormat("start \"{0}\"{1}", ddlFile, Environment.NewLine);
                wrapper.AppendFormat("exit{0}", Environment.NewLine);

                FileMgr.WriteTextToFile(cmdWrapperFile, wrapper.ToString(), false, true);
                cmd = string.Format("sqlplus {0}@{1}{2} @{3}"
                        , !string.IsNullOrEmpty(_userName)
                            && !string.IsNullOrEmpty(_userPassword)
                                ? string.Format("{0}/{1}"
                                    , _userName
                                    , _userPassword) : ""
                        , string.IsNullOrEmpty(_dbServer) ? "" : _dbServer
                        , _asSysDba ? " as SysDBA" : ""
                        , cmdWrapperFile);
                // record to results file indented under filename
                FileMgr.WriteTextToFile(_outputFileName
                        , string.Format("    {0}{1}", cmd, Environment.NewLine), true, true);
            }
            else 
            {
                // Db2 does not have a single line option for setting server, db, username, and password
                // so these need to be seperate command lines
                StringBuilder wrapper = new StringBuilder();
                // if there is a server defined, then connect to it
                if (!string.IsNullOrEmpty(_dbServer))
                {
                    // generate the server connect cmd
                    cmd = string.Format("db2 connect to {0} {1}"
                            , _dbServer
                            , !string.IsNullOrEmpty(_userName)  // if there is a user/pwd
                                && !string.IsNullOrEmpty(_userPassword)
                                    ? string.Format("user {0} using {1}"
                                        , _userName
                                        , _userPassword) : Environment.NewLine);
                    // record to results file indented under filename
                    FileMgr.WriteTextToFile(_outputFileName
                            , string.Format("    {0}{1}", cmd, Environment.NewLine), true, true);
                    wrapper.AppendFormat("{0}{1}", cmd, Environment.NewLine);
                }

                // if we need to connect to a database
                if (!serverOnly)
                {
                    cmd = string.Format("db2 connect to {0} {1}"
                            , _dbName
                            , !string.IsNullOrEmpty(_userName)
                                && !string.IsNullOrEmpty(_userPassword)
                                    ? string.Format("user {0} using {1}"
                                        , _userName
                                        , _userPassword) : Environment.NewLine);
                    // record to results file indented under filename
                    FileMgr.WriteTextToFile(_outputFileName
                            , string.Format("    {0}{1}", cmd, Environment.NewLine), true, true);
                    wrapper.AppendFormat("{0}{1}", cmd, Environment.NewLine);
                }

                // format the actual command line command
                cmd = string.Format("db2 -f{0} -z{1}"
                        , ddlFile
                        , cmdOutputFile);
                // record to results file indented under filename
                FileMgr.WriteTextToFile(_outputFileName
                        , string.Format("    {0}{1}", cmd, Environment.NewLine), true, true);
                wrapper.AppendFormat("{0}{1}", cmd, Environment.NewLine);
                
                cmd = "db2 terminate";
                // record to results file indented under filename
                FileMgr.WriteTextToFile(_outputFileName
                        , string.Format("    {0}{1}", cmd, Environment.NewLine), true, true);

                cmd = wrapper.ToString() + cmd;
            }

            // remove existing cmdOutputFile and add command
            cmdScript.AppendFormat("if exist {1} del {1}{0}{2}{0}type {1} >> {3}"
                    , Environment.NewLine
                    , cmdOutputFile
                    , cmd
                    , _outputFileName);

            FileMgr.WriteTextToFile(ddlBatchFile, cmdScript.ToString(), false, true);
        }

        /// <summary>
        /// Starts the OS process:
        /// <code>
        /// osCmdProcess.Start();
        /// osCmdProcess.WaitForExit();
        /// osCmdProcess.Close();
        /// </code>
        /// </summary>
        /// <param name="osCmdProcess">The OS Process command</param>
        private void ProcessDDLBatch(Process osCmdProcess)
        {
            osCmdProcess.Start();
            osCmdProcess.WaitForExit();
            osCmdProcess.Close();
        }

        internal string Status
        {
            get
            {
                return _results.ToString();
            }
        }

        internal void Pause()
        {
            _pause = true;
            _results.AppendFormat("Paused.{0}", Environment.NewLine);
        }

        internal void Resume()
        {
            _pause = false;
            _results.AppendFormat("Resumed.{0}", Environment.NewLine);
        }

        internal void Exit()
        {
            _exit = true;
        }
    }

}
