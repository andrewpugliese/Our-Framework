using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Data.Common;
using System.Data.Objects;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Practices.EnterpriseLibrary.Data;

using B1.Core;
using B1.DataAccess;
using B1.LoggingManagement;
using B1.ILoggingManagement;
using B1.Configuration;
using B1.CacheManagement;
using B1.TraceViewer;
using B1.Utility.TestConsoleApp;
using B1.SessionManagement;


namespace B1.Utility.DatabaseSetup
{

    /// <summary>
    /// This class implements the User Interface to setup a database to work with the framework.
    /// The UI is divided into Tab Controls with each tab responsible for different functions and tests.
    /// <list type="bullets">
    /// <item>
    /// DbSetup Tab: Provides ability to process Command Files (which contain macros and filename/path of DDL files):
    /// <para><see cref="B1.Utility.DatabaseSetup.DbSetupMgr.btnStart_Click"/></para>
    /// </item>
    /// <item>
    /// TestDataAccessManager Tab: Unit Tests for (Select (with Paging), Insert, Update, Delete, Merge, Compound SQL, Transactions)
    /// <para><see cref="B1.Utility.DatabaseSetup.TestDataAccessMgr"/></para>
    /// </item>
    /// <item>
    /// TestConfig Tab: Unit Tests for Configuration Manager
    /// </item>
    /// <item>
    /// TestLoggingAndTracing Tab: Unit Tests for Logging and Tracing functions
    /// <para><see cref="B1.ILoggingManagement"/></para>
    /// </item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Command files are simple text files (usually named with a .cmd extension) to distinguish them from DDL files
    /// usually named with a .sql extension.
    /// </para>
    /// <para>
    /// The only function of the command file (when used as the InputFile) is to contain the listing of DDL files
    /// to be passed into the command line tool.
    /// </para>
    /// <para>
    /// The DDL files need to be listed in dependancy order (where dependant files appear first).
    /// </para>
    /// </remarks>
    public partial class DbSetupMgr : Form
    {
        bool _bStart = true;
        bool _bPause = true;
        string _setupResults = null;
        string _textEditor = null;
        DataAccessMgr.EnumDbType _dbType;
        UserSignon.UserSessionListEnum _userSessionList = UserSignon.UserSessionListEnum.AllUserSessions;
        AppSession.AppSessionListEnum _appSessionList = AppSession.AppSessionListEnum.AllAppSessions;
        DbSetupWkr _dbSetupWkr = null;
        System.Windows.Forms.Timer _refreshTimer = new System.Windows.Forms.Timer();
        DataAccessMgr _daMgr = null;
        TestDataAccessMgr _testDaMgr = null;
        TestAppSessionControl _testAppCtrl = null;
        enumTraceLevel _configuredTraceLevel = enumTraceLevel.None;
        enumTraceLevel _dynamicTraceLevel = enumTraceLevel.None;
        internal enum TestTypeEnum { Update, Delete, Merge, Insert };
        LoggingMgr _loggingMgr = null;
        bool _startup = true;
        Int32? _currentUserCode = null;
        string _currentUserId = null;
        byte? _sessionControlCode = null;
        AppSession _appSession = null;
        DbSetupParamsCollection _macroParams = null;
        bool _refreshAppSessions = false;
        Dictionary<Int64, UserSession> _userSessions = null;
        bool _dbSetup = false;
        TaskProcessing.TaskProcessEngine _tpe = null;

        /// <summary>
        /// PagingMgr used by GridConrol
        /// </summary>
        PagingMgr _pagingMgr = null;
        /// <summary>
        /// PagingMgr based on primary key
        /// </summary>
        PagingMgr _pagingMgrPrimaryKey = null;
        /// <summary>
        /// PagingMgr based on compound index (AppSequenceName, AppSequenceId)
        /// </summary>
        PagingMgr _pagingMgrCompoundIndex = null;

        /// <summary>
        /// Last paging state for PagingMgr based on self join on compound index (AppSequenceName, AppSequenceId)
        /// </summary>
        string _lastPagingMgrJoinState = null;

        bool _updateStopped = true;
        bool _mergeStopped = true;
        bool _deleteStopped = true;

        /// <summary>
        /// Constructor for DbSetupMgr
        /// </summary>
        public DbSetupMgr()
        {
            InitializeComponent();
            try
            {
                LoadConfigSettings();
                // timer is used to refresh display
                _refreshTimer.Tick += new EventHandler(RefreshHandler);
                _refreshTimer.Interval = 2000;
                _loggingMgr = new LoggingMgr(tbLoggingKey.Text);

                _configuredTraceLevel = _loggingMgr.TraceLevel;
                _dynamicTraceLevel = _configuredTraceLevel;
                cmbTraceLevel.SelectedItem = _configuredTraceLevel.ToString();

                _loggingMgr.WriteToLog(string.Format("{0}; Version: {1}; Startup"
                        , Application.ProductName
                        , Application.ProductVersion)
                    , EventLogEntryType.Information
                    , enumEventPriority.Critical);
                tbLoggingTargets.Text = _loggingMgr.ConfigOptions;
                _loggingMgr.TraceToWindow = true;

                Init();
            }
            catch (Exception Exc)
            {
                MessageBox.Show(Exc.Message + Exc.StackTrace);
            }

        }

        /// <summary>
        /// Returns the DynamicTraceLevel of the setup utility controlled by the UI
        /// </summary>
        public enumTraceLevel DynamicTraceLevel
        {
            get { return _dynamicTraceLevel; }
        }

        void LoadConfigSettings()
        {
            cmbDbType.Items.Add(DataAccessMgr.EnumDbType.SqlServer.ToString());
            cmbDbType.Items.Add(DataAccessMgr.EnumDbType.Oracle.ToString());
            cmbDbType.Items.Add(DataAccessMgr.EnumDbType.Db2.ToString());

            cmbTraceLevel.Items.Add(enumTraceLevel.None.ToString());
            cmbTraceLevel.Items.Add(enumTraceLevel.Level1.ToString());
            cmbTraceLevel.Items.Add(enumTraceLevel.Level2.ToString());
            cmbTraceLevel.Items.Add(enumTraceLevel.Level3.ToString());
            cmbTraceLevel.Items.Add(enumTraceLevel.Level4.ToString());
            cmbTraceLevel.Items.Add(enumTraceLevel.All.ToString());

            _configuredTraceLevel = enumTraceLevel.None;
            cmbTraceLevel.SelectedItem = _configuredTraceLevel.ToString();
            _dynamicTraceLevel = _configuredTraceLevel;

            DbSetupConfiguration dbSetupConfigSection 
                    = AppConfigMgr.GetSection<DbSetupConfiguration>(DbSetupConfiguration.ConfigSectionName);

            tbConnectionKey.Text = tbAppCtrlConnKey.Text = AppConfigMgr.GetValue(Constants.ConnectionKey);
            DbSetupElement dbSetupConfig = dbSetupConfigSection.GetDbSetupConfig(tbConnectionKey.Text);
            tbDbServer.Text = dbSetupConfig.DbServer;
            tbDbName.Text = dbSetupConfig.DbName;

            _dbType = DataAccessMgr.ConvertToDbType(dbSetupConfig.DbType);
            cmbDbType.SelectedItem = _dbType.ToString();

            _textEditor = dbSetupConfig.TextEditor;
            tbUserName.Text = dbSetupConfig.UserName;
            tbUserPwd.Text = dbSetupConfig.UserPassword;
            tbInputFilename.Text = dbSetupConfig.InputFileName;
            tbOutputFilename.Text = dbSetupConfig.OutputFileName;
            tbDDLSourceDir.Text = dbSetupConfig.DDLSourceDirectory;
            _macroParams = dbSetupConfig.Params;
            foreach (DbSetupParamsElement macroParam in dbSetupConfig.Params)
            {
                tbMacroParams.Text += string.Format("Param: {0}; Value: {1}{2}"
                    , macroParam.ParamKey, macroParam.ParamValue, Environment.NewLine);
            }
            tbAppId.Text = tbAppCtrlAppId.Text = AppConfigMgr.GetValue(Constants.ApplicationId);

            ObjectFactoryConfiguration objectFactoryConfig = ObjectFactoryConfiguration.GetSection();
            tbDbProviderDllPath.Text = objectFactoryConfig.GetFactoryObject(tbConnectionKey.Text).AssemblyPath;
            tbDbProvAssembly.Text = objectFactoryConfig.GetFactoryObject(tbConnectionKey.Text).AssemblyName;
            tbDbProvClass.Text = objectFactoryConfig.GetFactoryObject(tbConnectionKey.Text).ObjectClass;

            tbLoggingKey.Text = AppConfigMgr.GetValue(Constants.LoggingKey);

            tbMemFileDirectory.Text = Environment.CurrentDirectory;
            btnChgAppCtrl.Enabled = false;
            tbSignoffWarningMsg.Enabled = false;
            tbRestrictSignonMsg.Enabled = false;
            tbLastModBy.Enabled = false;
            tbLastModDateTime.Enabled = false;
            numUDappCheckinSec.Enabled = false;
            numUDsessionTimeOutSecs.Enabled = false;
            numUDsignonLimit.Enabled = false;
            lblSignonResults.Text = "Enter UserId and Password (leave blank if new user).";
            btnSignoff.Enabled = false;

            btnStopInsert.Enabled = btnStopUpdate.Enabled = btnStopMerge.Enabled = btnStopDelete.Enabled = false;
            if (!string.IsNullOrEmpty(dbSetupConfig.AsSysDba)
                    && _dbType == DataAccessMgr.EnumDbType.Oracle)
                cbAsSysDba.Checked = Convert.ToBoolean(dbSetupConfig.AsSysDba);
            numPageSize.Value = 20;
        }

        void RefreshHandler(Object myObject, EventArgs myEventArgs)
        {
            RefreshScreen();
        }

        /// <summary>
        /// Update screen with status changes
        /// </summary>
        void RefreshScreen()
        {
            _refreshTimer.Stop();

            if (_daMgr == null && !_dbSetup)
                CreateDbMgr();

            if (_refreshAppSessions
                && _appSession != null
                && !_dbSetup)
            {
                dgvAppSessions.DataSource = AppSession.AppSessions(_daMgr, _appSessionList); ;
                FormatGridColumns(dgvAppSessions);
                dgvAppSessions.Refresh();
                dgvUserSessions.DataSource = UserSignon.UserSessions(_daMgr, _userSessionList);
                FormatGridColumns(dgvUserSessions);
                dgvUserSessions.Refresh();
                dgvUserMaster.DataSource = GetUserMaster();
                FormatGridColumns(dgvUserMaster);
                dgvUserMaster.Refresh();
            }

            if (!string.IsNullOrEmpty(_setupResults))
            {
                tbResults.Text = _setupResults;
                ModifyControls(true);
                Init();
                _dbSetup = false;
            }
            else
            {
                if (_dbSetupWkr != null)
                    tbResults.Text = _dbSetupWkr.Status;
                if (_updateStopped)
                {
                    btnStartUpdate.Enabled = true;
                    btnStopUpdate.Enabled = false;
                }
                if (_mergeStopped)
                {
                    btnStartMerge.Enabled = true;
                    btnStopMerge.Enabled = false;
                }
                if (_deleteStopped)
                {
                    btnStartDelete.Enabled = true;
                    btnStopDelete.Enabled = false;
                }
                _refreshTimer.Start();
            }
        }

        DataTable GetUserMaster()
        {
            DataTable dt = _daMgr.ExecuteDataSet(_daMgr.BuildSelectDbCommand(
                    "select * from " + DataAccess.Constants.SCHEMA_CORE + "." + SessionManagement.Constants.UserMaster
                    , null), null, null).Tables[0];
            if (dt.Rows.Count > 0)
                btnUnrestrictAcnt.Enabled = true;
            return dt;
        }

        void Init()
        {
            if (_bStart)
            {
                btnStart.Text = Constants.Start;
                btnPause.Enabled = false;
            }
            else
            {
                btnStart.Text = Constants.Stop;
                btnPause.Enabled = true;
                if (_bPause)
                    btnPause.Text = Constants.Pause;
                else btnPause.Text = Constants.Resume;
            }
        }

        void Exit()
        {
            _loggingMgr.WriteToLog(string.Format("{0}; Version: {1}; Exiting"
                        , Application.ProductName
                        , Application.ProductVersion), EventLogEntryType.Information
                    , enumEventPriority.Critical);

            ClearDbMgr();

            Application.Exit();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void DbSetupMgr_FormClosing(object sender, EventArgs e)
        {
            Exit();
        }

        void ModifyControls(bool Enabled)
        {
            cmbDbType.Enabled = Enabled;
            tbDbServer.Enabled = Enabled;
            tbDbName.Enabled = Enabled;
            tbUserName.Enabled = Enabled;
            tbUserPwd.Enabled = Enabled;
            tbInputFilename.Enabled = Enabled;
            tbOutputFilename.Enabled = Enabled;
            tbDDLSourceDir.Enabled = Enabled;
            cbAsSysDba.Enabled = Enabled;
        }

        /// <summary>
        /// This method is called when the user clicks on the Start button.
        /// It will prompt the user if they are sure, they want to continue.
        /// <para>At this point, a new thread is created and the Input command file will be parsed.</para>
        /// <para>Processing will continue asynchronously until command file parsing is completed or the user clicks the stop button</para>
        /// <para><see cref="B1.Utility.DatabaseSetup.DbSetupWkr"/></para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_bStart)
            {
                _bPause = true;
                if (MessageBox.Show("Are you sure you want to recreate the database?"
                        , "Database Setup"
                        , MessageBoxButtons.YesNo
                        , MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    try
                    {
                        _dbSetup = true;
                        tbResults.Clear();
                        ClearDbMgr();
                        _dbSetupWkr = new DbSetupWkr(tbDbServer.Text
                                                    , tbDbName.Text
                                                    , tbUserName.Text
                                                    , tbUserPwd.Text
                                                    , _dbType
                                                    , cbAsSysDba.Checked
                                                    , tbInputFilename.Text
                                                    , tbOutputFilename.Text
                                                    , tbDDLSourceDir.Text
                                                    , _macroParams
                                                    , DbSetupCompleted);

                        System.Threading.Thread dbSetupWkrThread = new System.Threading.Thread(_dbSetupWkr.Start);
                        _setupResults = null;
                        ModifyControls(false);
                        dbSetupWkrThread.Start();
                        // Sets the timer interval to 2 seconds.
                        _refreshTimer.Start();
                    }
                    catch (Exception Exc)
                    {
                        MessageBox.Show(Exc.Message + Exc.StackTrace);
                        ModifyControls(true);
                    }
                }
            }
            else
            {
                if (_dbSetupWkr != null)
                {
                    _dbSetupWkr.Exit();
                    _dbSetupWkr = null;
                }
                ModifyControls(true);
            }


            _bStart = !_bStart;
            Init();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (_bPause)
                _refreshTimer.Stop();
            else _refreshTimer.Start();

            _bPause = !_bPause;
            // Sets the timer interval to 2 seconds.
            Init();
        }

        /// <summary>
        /// Called by worker thread when completed parsing command file (or was interrupted).
        /// </summary>
        internal void DbSetupCompleted(String results, bool aborted, TimeSpan timespan)
        {
            _setupResults = results;
            _bStart = !_bStart;
            _bPause = !_bPause;
            MessageBox.Show(string.Format("DbSetup {0}; time elapsed: seconds: {1}, milliseconds: {2}."
                , aborted ? "Aborted" : "Completed"
                , timespan.TotalSeconds
                , timespan.TotalMilliseconds));

        }
        
        private void cmbDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDbType.SelectedItem.ToString() == DataAccessMgr.EnumDbType.Oracle.ToString())
                cbAsSysDba.Visible = true;
            else cbAsSysDba.Visible = false;
            if (!_startup)
                MessageBox.Show("If you are changing the database type, make sure you verify "
                                    + "the other settings (SourceDirectory, InputFile, and credentials.");
            _startup = false;
        }

        private void btnViewInFile_Click(object sender, EventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = _textEditor;
            Process p = new Process();
            psi.Arguments = tbDDLSourceDir.Text + "\\" + tbInputFilename.Text;

            p.StartInfo = psi;
            p.Start();
        }

        private void btnViewOutFile_Click(object sender, EventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = _textEditor;
            Process p = new Process();
            psi.Arguments = tbDDLSourceDir.Text + "\\" +  tbOutputFilename.Text;

            p.StartInfo = psi;
            p.Start();
        }

        private void btnStartInsert_Click(object sender, EventArgs e)
        {
            StartInsert();
        }

        /// <summary>
        /// Returns the DynamicTraceLevel property
        /// </summary>
        /// <returns></returns>
        public object GetTraceLevel()
        {
            return DynamicTraceLevel;
        }

        void ClearDbMgr()
        {
            if (_tpe != null)
            {
                _tpe.Stop();
                _tpe.Dispose();
                _tpe = null;
            }
            if (_daMgr != null)
            {
                if (_testDaMgr != null)
                {
                    _testDaMgr.StopUpdate();
                    _testDaMgr.StopDelete();
                    _testDaMgr.StopInsert();
                    _testDaMgr = null;
                }

                if (_userSessions != null)
                {
                    foreach (Int64 sessionCode in _userSessions.Keys)
                        UserSignon.Signoff(_daMgr, sessionCode);

                    _userSessions.Clear();
                    _userSessions = new Dictionary<long, UserSession>();
                }

                if (_appSession != null)
                    _appSession.End();

                if (_testAppCtrl != null)
                    _testAppCtrl.Stop();

                _daMgr.ClearUniqueIdsCache();

                tbConnectionString.Text = null;
                tbServerTime.Text = null;
                tbServerVersion.Text = null;
                tbProviderVersion.Text = null;
                tbRowCount.Text = null;
                _daMgr = null;
                _pagingMgr = null;
                _pagingMgrCompoundIndex = null;
                _pagingMgrPrimaryKey = null;
            }
        }

        void CreateDbMgr()
        {
            string connectionKey = tbConnectionKey.Text;

            _daMgr = new DataAccessMgr(connectionKey, _loggingMgr);
            tbConnectionKey.ReadOnly = true;
            tbAppId.ReadOnly = true;
            tbAppCtrlConnKey.ReadOnly = true;

            tbConnectionString.Text = _daMgr.Database.ConnectionStringWithoutCredentials;
            tbServerVersion.Text = _daMgr.DbServerVersion;
            tbProviderVersion.Text = _daMgr.DbProviderVersion;
            tbServerTime.Text = _daMgr.GetServerTime(EnumDateTimeLocale.UTC)
                    .ToString("ddd, dd MMM yyyy HH:mm:ss.fff") + " GMT";

            AppConfigMgr.SetRuntimeValue(Constants.TraceLevel, GetTraceLevel);

            cmbTraceLevel.Enabled = true;

            // create a pagingMgr to demo paging
            _pagingMgrPrimaryKey = new PagingMgr(_daMgr
                    , DataAccess.Constants.SCHEMA_CORE + "." + DataAccess.Constants.TABLE_TestSequence
                    , DataAccess.Constants.PageSize
                    , Convert.ToInt16(numPageSize.Value));

            
            _pagingMgrCompoundIndex = new PagingMgr(_daMgr
                    , DataAccess.Constants.SCHEMA_CORE + "." + DataAccess.Constants.TABLE_TestSequence
                    , new List<string> { Constants.AppSequenceName, Constants.AppSequenceId }
                    , DataAccess.Constants.PageSize
                    , Convert.ToInt16(numPageSize.Value));         

            if(rbPagingPrimaryKey.Checked)
                _pagingMgr = _pagingMgrPrimaryKey;
            else if(rbPagingCompound.Checked)
                _pagingMgr = _pagingMgrCompoundIndex;
            else if(rbPagingJoin.Checked)
                _pagingMgr = CreatePagingMgrJoin();
            else
                throw new ExceptionEvent(enumExceptionEventCodes.UnknownException,
                        "No paging radio button is selected.");
        }

        void StartAppSession()
        {
            if (_daMgr == null)
                CreateDbMgr();

            if (_daMgr.DatabaseType == DataAccessMgr.EnumDbType.SqlServer) // andrew
            {
                // for future use
                StringBuilder statusMsg = new StringBuilder();
                StringBuilder configSettings = new StringBuilder();
                configSettings.AppendFormat("TraceLevel: {0}{1}", _configuredTraceLevel.ToString(), Environment.NewLine);
                _appSession = new AppSession(_daMgr
                    , tbAppCtrlAppId.Text
                    , Application.ProductVersion
                    , Application.ProductName
                    , Status);
                statusMsg.Append("Application Startup");
                string sessionExists = _appSession.Start(configSettings.ToString(), statusMsg.ToString(), false);
                if (!string.IsNullOrEmpty(sessionExists))
                    if (MessageBox.Show(sessionExists + " Press OK to reset; CANCEL to stop."
                        , "Session Startup Conflict", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                    {
                        Exit();
                        statusMsg.AppendFormat(" after session conflict reset");
                    }
                    else _appSession.Start(configSettings.ToString(), statusMsg.ToString(), true);
                LoadSessionControlRecord();
                _refreshAppSessions = true;
                btnCleanupInactiveAppSessions.Enabled = true;
                btnCleanupInactiveUsers.Enabled = true;
                rbActiveAppSessions.Enabled = rbAllAppSessions.Enabled = rbInActiveAppSessions.Enabled = true;
                _refreshTimer.Interval = _appSession.SignonControlData.StatusSeconds * 1000;
                _refreshTimer.Start();
                RefreshScreen();
            }
            else
            {
                btnChgAppCtrl.Enabled = false;
                tbRestrictSignonMsg.Enabled = false;
                tbSignoffWarningMsg.Enabled = false;
                numUDappCheckinSec.Enabled = false;
                numUDsessionTimeOutSecs.Enabled = false;
                numUDsignonLimit.Enabled = false;
                cbForceSignoff.Enabled = false;
                cbRestrictSignon.Enabled = false;
                tbLastModBy.Enabled = false;
                tbLastModDateTime.Enabled = false;
                tbAppCtrlAppId.Enabled = false;
                MessageBox.Show("Application Session not provided for database type");
            }
            btnStartAppSession.Enabled = false;
        }

        string Status()
        {
            DbCommandMgr cmdMgr = new DbCommandMgr(_daMgr);
            for (int i = 0; i < lbSignedonUsers.Items.Count; i++)
            {
                string[] userSessionItems = lbSignedonUsers.Items[i].ToString().Split(new char[] { ':' });
                Int64 sessionCode = Convert.ToInt64(userSessionItems[2]);
                DbCommand updateSession = _daMgr.CloneDbCommand(UserSession.GetUpdateUserSessionDbCommand(_daMgr));
                updateSession.Parameters[_daMgr.BuildParamName(SessionManagement.Constants.SessionCode)].Value = sessionCode;
                cmdMgr.AddDbCommand(updateSession);
            }
            if (!cmdMgr.IsNoOpDbCommand)
                cmdMgr.ExecuteNonQuery();
            _refreshAppSessions = true; // cause screen refresh
            return lbSignedonUsers.Items.Count.ToString() + " Users signed on.";
        }

        void LoadSessionControlRecord()
        {
            tbRestrictSignonMsg.Enabled = true;
            tbSignoffWarningMsg.Enabled = true;
            tbRestrictSignonMsg.Text = _appSession.SignonControlData.RestrictSignonMsg;
            tbSignoffWarningMsg.Text = _appSession.SignonControlData.SignoffWarningMsg;
            numUDsignonLimit.Value = _appSession.SignonControlData.FailedAttemptLimit;
            _sessionControlCode = _appSession.SignonControlData.SessionControlCode;
            numUDappCheckinSec.Value = _appSession.SignonControlData.StatusSeconds;
            numUDsessionTimeOutSecs.Value = _appSession.SignonControlData.TimeOutSeconds;
            cbForceSignoff.Checked = _appSession.SignonControlData.ForceSignoff;
            cbRestrictSignon.Checked = _appSession.SignonControlData.RestrictSignon;
            tbLastModBy.Text = _appSession.SignonControlData.LastModifiedByUserCode.HasValue 
                    ? _appSession.SignonControlData.LastModifiedByUserCode.Value.ToString() : null;
            tbLastModDateTime.Text = _appSession.SignonControlData.LastModifiedByDateTime.HasValue
                    ? _appSession.SignonControlData.LastModifiedByDateTime.Value.ToString("ddd, dd MMM yyyy HH:mm:ss.fff") 
                    : null;
            btnChgAppCtrl.Enabled = true;
            tbSignoffWarningMsg.Enabled = true;
            tbRestrictSignonMsg.Enabled = true;
            tbLastModBy.Enabled = true;
            tbLastModDateTime.Enabled = true;
            numUDappCheckinSec.Enabled = true;
            numUDsessionTimeOutSecs.Enabled = true;
            numUDsignonLimit.Enabled = true;
        }

        PagingMgr CreatePagingMgrJoin()
        {
            DbTableDmlMgr dmlJoin = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE,
                    DataAccess.Constants.TABLE_TestSequence);
    
            // INNER JOIN B1.TestSequence T2 ON T1.AppSequenceId <> T2.AppSequenceId 
            // AND T1.AppSequenceName = T2.AppSequenceName 
            string t2Alias = dmlJoin.AddJoin(DataAccess.Constants.TABLE_TestSequence, DataAccess.Constants.SCHEMA_CORE, 
                    DbTableJoinType.Inner,
                    t => t.Column(DataAccess.Constants.SCHEMA_CORE,
                        DataAccess.Constants.TABLE_TestSequence, Constants.AppSequenceId) != 
                        t.Column(Constants.AppSequenceId) && 
                        t.Column(DataAccess.Constants.SCHEMA_CORE,
                        DataAccess.Constants.TABLE_TestSequence, Constants.AppSequenceName) == 
                        t.Column(Constants.AppSequenceName));

            
            // where T1.AppSequenceId > 1 AND T1.AppSequenceName > 'A'
            dmlJoin.SetWhereCondition( t => t.Column(Constants.AppSequenceId) > 1 && 
                t.Column(Constants.AppSequenceName) > 'A');

            return new PagingMgr(_daMgr
                    , dmlJoin
                    , new List<string> { Constants.AppSequenceName, Constants.AppSequenceId }
                    , DataAccess.Constants.PageSize
                    , Convert.ToInt16(numPageSize.Value)
                    , _lastPagingMgrJoinState);
        }

        void StartInsert()
        {
            btnStartInsert.Enabled = false;
            btnStopInsert.Enabled = true;

            if (_daMgr == null)
                CreateDbMgr();

            if (_testDaMgr == null)
                _testDaMgr = new TestDataAccessMgr(_daMgr, 5, TestStopped);
            tbResults.Text = _setupResults = string.Empty;
            _testDaMgr.StartInsert();
        }

        private void btnStopInsert_Click(object sender, EventArgs e)
        {
            _testDaMgr.StopInsert();
            btnStartInsert.Enabled = true;
            btnStopInsert.Enabled = false;
        }

        /// <summary>
        /// When the traceLevel combo box is change to a value other than NONE, you will be able to see more or 
        /// less trace messages that are being generated by the Insert or Update operations.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbTraceLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTraceLevel.SelectedItem.ToString() == enumTraceLevel.None.ToString())
                _dynamicTraceLevel = enumTraceLevel.None;
            if (cmbTraceLevel.SelectedItem.ToString() == enumTraceLevel.Level1.ToString())
                _dynamicTraceLevel = enumTraceLevel.Level1;
            if (cmbTraceLevel.SelectedItem.ToString() == enumTraceLevel.Level2.ToString())
                _dynamicTraceLevel = enumTraceLevel.Level2;
            if (cmbTraceLevel.SelectedItem.ToString() == enumTraceLevel.Level3.ToString())
                _dynamicTraceLevel = enumTraceLevel.Level3;
            if (cmbTraceLevel.SelectedItem.ToString() == enumTraceLevel.Level4.ToString())
                _dynamicTraceLevel = enumTraceLevel.Level4;
            if (cmbTraceLevel.SelectedItem.ToString() == enumTraceLevel.All.ToString())
                _dynamicTraceLevel = enumTraceLevel.All;
            if (_daMgr != null)
                _daMgr.loggingMgr.TraceLevel = _dynamicTraceLevel;
        }

        void TestQuery()
        {
            DbTableDmlMgr dmlSelect = _daMgr.DbCatalogGetTableDmlMgr("B1.TestSequence");

            string dateFunc = _daMgr.GetDbTimeAs(EnumDateTimeLocale.UTC, null);
            dmlSelect.SetWhereCondition(t => t.Column("TestSequence", "DbServerTime") <= t.Function(dateFunc));

            DbCommand dbCmd = _daMgr.BuildSelectDbCommand(dmlSelect, 20);
            DataTable dt = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
        }

        void FormatGridColumns(DataGridView dgv)
        {
            foreach (DataGridViewColumn dgvc in dgv.Columns)
                if (dgvc.ValueType.Name == "DateTime")
                    dgvc.DefaultCellStyle.Format = "ddd, dd MMM yyyy HH:mm:ss.fff";
        }

        private void btnPageFirst_Click(object sender, EventArgs e)
        {
            if (_daMgr == null)
                CreateDbMgr();

            object results = null;
            
            //When using the primary key for paging, each page is a collection of classes instead of
            //a DataTable
            if(rbPagingPrimaryKey.Checked)
                results = _pagingMgr.GetFirstPage<B1.Utility.DatabaseSetup.Models.TestSequence>(Convert.ToInt16(numPageSize.Value));
            else
                results = _pagingMgr.GetFirstPage(Convert.ToInt16(numPageSize.Value));

            if ((results is DataTable && ((DataTable)results).Rows.Count == 0) || 
                (results is IEnumerable<B1.Utility.DatabaseSetup.Models.TestSequence> && 
                    ((IEnumerable<B1.Utility.DatabaseSetup.Models.TestSequence>)results).Count() == 0))
            {
                MessageBox.Show("There was no data found, begin insert to add data");
                 return;
            }
            btnPageNext.Enabled = btnPagePrevious.Enabled = true;
            dgvResults.DataSource = results;
            FormatGridColumns(dgvResults);
            dgvResults.Refresh();
            _daMgr.loggingMgr.WriteToLog("PageFirst Operation Completed."
                                , EventLogEntryType.Information
                                , enumEventPriority.Normal);
        }

        private void btnPageLast_Click(object sender, EventArgs e)
        {
            if (_daMgr == null)
                CreateDbMgr();

            object results = null;
            
            //When using the primary key for paging, each page is a collection of classes instead of
            //a DataTable
            if(rbPagingPrimaryKey.Checked)
                results = _pagingMgr.GetLastPage<B1.Utility.DatabaseSetup.Models.TestSequence>(Convert.ToInt16(numPageSize.Value));
            else
                results = _pagingMgr.GetLastPage(Convert.ToInt16(numPageSize.Value));

            if ((results is DataTable && ((DataTable)results).Rows.Count == 0) || 
                (results is IEnumerable<B1.Utility.DatabaseSetup.Models.TestSequence> && 
                    ((IEnumerable<B1.Utility.DatabaseSetup.Models.TestSequence>)results).Count() == 0))
            {
                MessageBox.Show("There was no data found, begin insert to add data");
                return;
            }
            btnPageNext.Enabled = btnPagePrevious.Enabled = true;
            dgvResults.DataSource = results;
            FormatGridColumns(dgvResults);
            dgvResults.Refresh();
            _daMgr.loggingMgr.WriteToLog("PageLast Operation Completed."
                                , EventLogEntryType.Information
                                , enumEventPriority.Normal);
        }


        private void btnPageNext_Click(object sender, EventArgs e)
        {
            object results = null;

            if (_daMgr == null)
                CreateDbMgr();

            //When using the primary key for paging, each page is a collection of classes instead of
            //a DataTable
            if(rbPagingPrimaryKey.Checked)
                results = _pagingMgr.GetNextPage<B1.Utility.DatabaseSetup.Models.TestSequence>(Convert.ToInt16(numPageSize.Value));
            else
                results = _pagingMgr.GetNextPage(Convert.ToInt16(numPageSize.Value));

            if ((results is DataTable && ((DataTable)results).Rows.Count == 0) || 
                (results is IEnumerable<B1.Utility.DatabaseSetup.Models.TestSequence> && 
                    ((IEnumerable<B1.Utility.DatabaseSetup.Models.TestSequence>)results).Count() == 0))
            {
                MessageBox.Show("End of data.");
                return;
            }

            dgvResults.DataSource = results;
            FormatGridColumns(dgvResults);
            dgvResults.Refresh();
            _daMgr.loggingMgr.WriteToLog("PageNext Operation Completed."
                                , EventLogEntryType.Information
                                , enumEventPriority.Normal);
        }


        private void btnPagePrevious_Click(object sender, EventArgs e)
        {
            object results = null;

            if (_daMgr == null)
                CreateDbMgr();

            //When using the primary key for paging, each page is a collection of classes instead of
            //a DataTable
            if(rbPagingPrimaryKey.Checked)
                results = _pagingMgr.GetPreviousPage<B1.Utility.DatabaseSetup.Models.TestSequence>(Convert.ToInt16(numPageSize.Value));
            else
                results = _pagingMgr.GetPreviousPage(Convert.ToInt16(numPageSize.Value));
            
            if ((results is DataTable && ((DataTable)results).Rows.Count == 0) || 
                (results is IEnumerable<B1.Utility.DatabaseSetup.Models.TestSequence> && 
                    ((IEnumerable<B1.Utility.DatabaseSetup.Models.TestSequence>)results).Count() == 0))
            {
                MessageBox.Show("End of data.");
                return;
            }

            dgvResults.DataSource = results;
            FormatGridColumns(dgvResults);
            dgvResults.Refresh();
            _daMgr.loggingMgr.WriteToLog("PagePrevious Operation Completed."
                                , EventLogEntryType.Information
                                , enumEventPriority.Normal);
        }

        private void btnStartUpdate_Click(object sender, EventArgs e)
        {
            btnStartUpdate.Enabled = false;
            btnStopUpdate.Enabled = true;

            if (_daMgr == null)
                CreateDbMgr();

            if (_testDaMgr == null)
                _testDaMgr = new TestDataAccessMgr(_daMgr, 2, TestStopped);
            _testDaMgr.StartUpdate();
            _updateStopped = false;
            _refreshTimer.Start();
        }

        private void btnStopUpdate_Click(object sender, EventArgs e)
        {
            _testDaMgr.StopUpdate();
        }

        private void btnStartMerge_Click( object sender, EventArgs e )
        {
            btnStartMerge.Enabled = false;
            btnStopMerge.Enabled = true;

            if (_daMgr == null)
                CreateDbMgr();

            if (_testDaMgr == null)
                _testDaMgr = new TestDataAccessMgr( _daMgr, 2, TestStopped );
            _testDaMgr.StartMerge();
            _mergeStopped = false;
            _refreshTimer.Start();
        }

        private void btnStopMerge_Click( object sender, EventArgs e )
        {
            _testDaMgr.StopMerge();
        }

        internal void TestStopped( TestTypeEnum testType )
        {
            if (testType == TestTypeEnum.Update)
                _updateStopped = true;
            else if (testType == TestTypeEnum.Merge)
                _mergeStopped = true;
            else if (testType == TestTypeEnum.Delete)
                _deleteStopped = true;
        }


        private void btnStartDelete_Click(object sender, EventArgs e)
        {
            btnStartDelete.Enabled = false;
            btnStopDelete.Enabled = true;

            if (_daMgr == null)
                CreateDbMgr();

            if (_testDaMgr == null)
                _testDaMgr = new TestDataAccessMgr(_daMgr, 2, TestStopped);

            _testDaMgr.StartDelete();
            _deleteStopped = false;
            _refreshTimer.Start();
        }

        private void btnStopDelete_Click(object sender, EventArgs e)
        {
            _testDaMgr.StopDelete();
        }


        private void btnAbort_Click( object sender, EventArgs e )
        {
            if (MessageBox.Show("Are you sure you want to simulate an application crash?"
                    , "Application Crash"
                    , MessageBoxButtons.YesNo
                    , MessageBoxIcon.Exclamation) == DialogResult.Yes)
                throw new ExceptionEvent(enumExceptionEventCodes.UnknownException
                        , "Example of an unhandled exception resulting in application crash.");
        }

        private void btnRowcount_Click(object sender, EventArgs e)
        {
            if (_daMgr == null)
                CreateDbMgr();
            tbRowCount.Text = _daMgr.ExecuteRowCount(DataAccess.Constants.SCHEMA_CORE 
                    + "." + DataAccess.Constants.TABLE_TestSequence).ToString();
        }

        private void btnSetRuntimeValue_Click(object sender, EventArgs e)
        {
            //if (_daMgr == null)
            //    CreateDbMgr();
            AppConfigMgr.SetRuntimeValue("RuntimeConfig", () => txtSetRuntimeValue.Text);
            btnSetRuntimeValue.Enabled = false;
        }

        private void btnGetRuntimeValue_Click(object sender, EventArgs e)
        {
            //if (_daMgr == null)
            //    CreateDbMgr(); 
            labelGetRuntimeValue.Text = AppConfigMgr.GetRuntimeValue<string>("RuntimeConfig");
        }

        private void btnTranTest_Click_1(object sender, EventArgs e)
        {
            if (_daMgr == null)
                CreateDbMgr();
            if (!cbExtDbTran.Checked)
                InternalTransactionTest();
            else ExternalTransactionTest();
        }

        private void InternalTransactionTest()
        {
            DbCommandMgr dbCmdMgr = new DbCommandMgr(_daMgr);
            DbTableDmlMgr dmlInsert = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , DataAccess.Constants.TABLE_TestSequence);
            DbFunctionStructure autogenerate = new DbFunctionStructure();
            if (_daMgr.DatabaseType == DataAccessMgr.EnumDbType.SqlServer
                || _daMgr.DatabaseType == DataAccessMgr.EnumDbType.Db2)
                autogenerate.AutoGenerate = true; // identity column
            else
            {// oracle sequence
                autogenerate.AutoGenerate = false;
                autogenerate.FunctionBody = DataAccess.Constants.SCHEMA_CORE + ".DbSequenceId_Seq.nextVal"; 
            }

            dmlInsert.AddColumn(Constants.AppSequenceId, _daMgr.BuildParamName(Constants.AppSequenceId));
            dmlInsert.AddColumn(Constants.AppSequenceName, _daMgr.BuildParamName(Constants.AppSequenceName));
            dmlInsert.AddColumn(Constants.AppLocalTime, _daMgr.BuildParamName(Constants.AppLocalTime));
            dmlInsert.AddColumn(Constants.AppSynchTime, _daMgr.BuildParamName(Constants.AppSynchTime));
            dmlInsert.AddColumn(Constants.Remarks, _daMgr.BuildParamName(Constants.Remarks));
            dmlInsert.AddColumn(Constants.DbSequenceId, autogenerate);// will default to ddl function
            dmlInsert.AddColumn(Constants.DbServerTime, EnumDateTimeLocale.Default); // will default to ddl function
            DbCommand dbCmd = _daMgr.BuildInsertDbCommand(dmlInsert);

            Int64 appSequenceId = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);

            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceId)].Value = appSequenceId;
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceName)].Value = GenerateRandomName();
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppLocalTime)].Value = DateTime.UtcNow;
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSynchTime)].Value = _daMgr.DbSynchTime;

            dbCmd.Parameters[_daMgr.BuildParamName(Constants.Remarks)].Value = string.Format(
                    "{0}:{1};{2}"
                    , Environment.MachineName
                    , System.Threading.Thread.CurrentThread.ManagedThreadId
                    , "Transaction1 Statement 1");

            dbCmdMgr.TransactionBeginBlock();
            appSequenceId = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);

            dbCmdMgr.AddDbCommand(dbCmd);
            dbCmd = _daMgr.CloneDbCommand(dbCmd);
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceId)].Value = appSequenceId;
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceName)].Value = GenerateRandomName();
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppLocalTime)].Value = DateTime.UtcNow;
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSynchTime)].Value = _daMgr.DbSynchTime;

            dbCmd.Parameters[_daMgr.BuildParamName(Constants.Remarks)].Value = string.Format(
                    "{0}:{1};{2}"
                    , Environment.MachineName
                    , System.Threading.Thread.CurrentThread.ManagedThreadId
                    , "Transaction1 Statement 2");
            dbCmdMgr.AddDbCommand(dbCmd);

            dbCmdMgr.TransactionEndBlock();

            // start another transaction
            dbCmdMgr.TransactionBeginBlock();
            dbCmd = _daMgr.CloneDbCommand(dbCmd);


            appSequenceId = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceId)].Value = appSequenceId;
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceName)].Value = GenerateRandomName();
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppLocalTime)].Value = DateTime.UtcNow;
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSynchTime)].Value = _daMgr.DbSynchTime;

            dbCmd.Parameters[_daMgr.BuildParamName(Constants.Remarks)].Value = string.Format(
                    "{0}:{1};{2}"
                    , Environment.MachineName
                    , System.Threading.Thread.CurrentThread.ManagedThreadId
                    , "Transaction2 Statement 1");
            dbCmdMgr.AddDbCommand(dbCmd);

            dbCmd = _daMgr.CloneDbCommand(dbCmd);
            if (rbTranTestPass.Checked)
                appSequenceId = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);
            // otherwise appSequenceId will be a dupe
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceId)].Value = appSequenceId;
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceName)].Value = GenerateRandomName();
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppLocalTime)].Value = DateTime.UtcNow;
            dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSynchTime)].Value = _daMgr.DbSynchTime;

            dbCmd.Parameters[_daMgr.BuildParamName(Constants.Remarks)].Value = string.Format(
                    "{0}:{1};{2}"
                    , Environment.MachineName
                    , System.Threading.Thread.CurrentThread.ManagedThreadId
                    , "InnerTransaction2 Statement 2" + (rbTranTestPass.Checked ? "" : " with dupe Id"));
            dbCmdMgr.AddDbCommand(dbCmd);

            // end inner transaction block
            dbCmdMgr.TransactionEndBlock();

            try
            {
                _loggingMgr.WriteToLog(dbCmdMgr.DbCommandBlockDebugText, EventLogEntryType.Information, enumEventPriority.Normal);
                dbCmdMgr.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                _loggingMgr.WriteToLog(exc);
            }
        }

        private void ExternalTransactionTest()
        {
            Int64 appSequenceId = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);
            using (DbConnection dbConn = _daMgr.Database.CreateConnection())
            {
                dbConn.Open();
                DbTransaction dbTrans = dbConn.BeginTransaction();
                DbTableDmlMgr dmlInsert = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , DataAccess.Constants.TABLE_TestSequence);
                DbFunctionStructure autogenerate = new DbFunctionStructure();
                if (_daMgr.DatabaseType == DataAccessMgr.EnumDbType.SqlServer
                    || _daMgr.DatabaseType == DataAccessMgr.EnumDbType.Db2)
                    autogenerate.AutoGenerate = true; // identity column
                else
                {// oracle sequence
                    autogenerate.AutoGenerate = false;
                    autogenerate.FunctionBody = DataAccess.Constants.SCHEMA_CORE + ".DbSequenceId_Seq.nextVal"; 
                }

                dmlInsert.AddColumn(Constants.AppSequenceId, _daMgr.BuildParamName(Constants.AppSequenceId));
                dmlInsert.AddColumn(Constants.AppSequenceName, _daMgr.BuildParamName(Constants.AppSequenceName));
                dmlInsert.AddColumn(Constants.AppLocalTime, _daMgr.BuildParamName(Constants.AppLocalTime));
                dmlInsert.AddColumn(Constants.AppSynchTime, _daMgr.BuildParamName(Constants.AppSynchTime));
                dmlInsert.AddColumn(Constants.Remarks, _daMgr.BuildParamName(Constants.Remarks));
                dmlInsert.AddColumn(Constants.DbSequenceId, autogenerate);// will default to ddl function
                dmlInsert.AddColumn(Constants.DbServerTime, EnumDateTimeLocale.Default); // will default to ddl function
                DbCommand dbCmd = _daMgr.BuildInsertDbCommand(dmlInsert);

                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceId)].Value = appSequenceId;
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceName)].Value = GenerateRandomName();
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppLocalTime)].Value = DateTime.UtcNow;
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSynchTime)].Value = _daMgr.DbSynchTime;

                dbCmd.Parameters[_daMgr.BuildParamName(Constants.Remarks)].Value = string.Format(
                        "{0}:{1};{2}"
                        , Environment.MachineName
                        , System.Threading.Thread.CurrentThread.ManagedThreadId
                        , "Transaction1 Statement 1");
                dbCmd.Transaction = dbTrans;
                try
                {
                    _daMgr.ExecuteNonQuery(dbCmd, dbTrans, null);
                }
                catch (Exception e)
                {
                    _loggingMgr.WriteToLog(e);
                }
                appSequenceId = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);

                dbCmd = _daMgr.CloneDbCommand(dbCmd);
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceId)].Value = appSequenceId;
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceName)].Value = GenerateRandomName();
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppLocalTime)].Value = DateTime.UtcNow;
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSynchTime)].Value = _daMgr.DbSynchTime;

                dbCmd.Parameters[_daMgr.BuildParamName(Constants.Remarks)].Value = string.Format(
                        "{0}:{1};{2}"
                        , Environment.MachineName
                        , System.Threading.Thread.CurrentThread.ManagedThreadId
                        , "Transaction1 Statement 2");
                dbCmd.Transaction = dbTrans;
                try
                {
                    _daMgr.ExecuteNonQuery(dbCmd, dbTrans, null);
                    dbTrans.Commit();
                }
                catch (Exception e)
                {
                    _loggingMgr.WriteToLog(e);
                    dbTrans.Rollback();
                }
            }

            using (DbConnection dbConn = _daMgr.Database.CreateConnection())
            {
                dbConn.Open();
                DbTransaction dbTrans = dbConn.BeginTransaction();
                DbTableDmlMgr dmlInsert = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , DataAccess.Constants.TABLE_TestSequence);
                DbFunctionStructure autogenerate = new DbFunctionStructure();
                if (_daMgr.DatabaseType == DataAccessMgr.EnumDbType.SqlServer
                     || _daMgr.DatabaseType == DataAccessMgr.EnumDbType.Db2)
                    autogenerate.AutoGenerate = true; // identity column
                else
                {// oracle sequence
                    autogenerate.AutoGenerate = false;
                    autogenerate.FunctionBody = DataAccess.Constants.SCHEMA_CORE + ".DbSequenceId_Seq.nextVal"; 
                }

                dmlInsert.AddColumn(Constants.AppSequenceId, _daMgr.BuildParamName(Constants.AppSequenceId));
                dmlInsert.AddColumn(Constants.AppSequenceName, _daMgr.BuildParamName(Constants.AppSequenceName));
                dmlInsert.AddColumn(Constants.AppLocalTime, _daMgr.BuildParamName(Constants.AppLocalTime));
                dmlInsert.AddColumn(Constants.AppSynchTime, _daMgr.BuildParamName(Constants.AppSynchTime));
                dmlInsert.AddColumn(Constants.Remarks, _daMgr.BuildParamName(Constants.Remarks));
                dmlInsert.AddColumn(Constants.DbSequenceId, autogenerate);// will default to ddl function
                dmlInsert.AddColumn(Constants.DbServerTime, EnumDateTimeLocale.Default); // will default to ddl function
                DbCommand dbCmd = _daMgr.BuildInsertDbCommand(dmlInsert);

                appSequenceId = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);

                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceId)].Value = appSequenceId;
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceName)].Value = GenerateRandomName();
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppLocalTime)].Value = DateTime.UtcNow;
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSynchTime)].Value = _daMgr.DbSynchTime;

                dbCmd.Parameters[_daMgr.BuildParamName(Constants.Remarks)].Value = string.Format(
                        "{0}:{1};{2}"
                        , Environment.MachineName
                        , System.Threading.Thread.CurrentThread.ManagedThreadId
                        , "Transaction1 Statement 1");
                dbCmd.Transaction = dbTrans;
                try
                {
                    _daMgr.ExecuteNonQuery(dbCmd, dbTrans, null);
                }
                catch (Exception e)
                {
                    _loggingMgr.WriteToLog(e);
                }

                if (rbTranTestPass.Checked)
                    appSequenceId = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);

                dbCmd = _daMgr.CloneDbCommand(dbCmd);
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceId)].Value = appSequenceId;
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceName)].Value = GenerateRandomName();
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppLocalTime)].Value = DateTime.UtcNow;
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSynchTime)].Value = _daMgr.DbSynchTime;

                dbCmd.Parameters[_daMgr.BuildParamName(Constants.Remarks)].Value = string.Format(
                        "{0}:{1};{2}"
                        , Environment.MachineName
                        , System.Threading.Thread.CurrentThread.ManagedThreadId
                        , "Transaction1 Statement 2");
                dbCmd.Transaction = dbTrans;
                try
                {
                    _daMgr.ExecuteNonQuery(dbCmd, dbTrans, null);
                    dbTrans.Commit();
                }
                catch (Exception e)
                {
                    _loggingMgr.WriteToLog(e);
                    dbTrans.Rollback();
                }
            }
        }

        internal static string GenerateRandomName()
        {
            Random r = new Random();
            return "".PadRight(r.Next(6, 12), (char)r.Next(65, 90)) + " " + 
                    "".PadRight(r.Next(6, 18), (char)r.Next(65, 90));
        }


        private void btnConfigTest_Click(object sender, EventArgs e)
        {
#warning"NS TO FIX TEST"
            MessageBox.Show("Function temp disabled; NS to FIX.");
            //if (_daMgr == null)
            //    CreateDbMgr();
            //AppConfigMgr.InitializeDefault(_daMgr, "eContracting", "ALL");
        }

        private void btnWatch_Click(object sender, EventArgs e)
        {
#warning"NS TO FIX TEST"
            MessageBox.Show("Function temp disabled; NS to FIX.");
            //if (_daMgr == null)
            //    CreateDbMgr();
            //_daMgr.dataConfigMgr.RegisterConfigChangeHandler("ExperianTimeout",
            //        (key, oldValue, newValue) =>
            //        {
            //            labelConfigChangeValue.BeginInvoke((Action)delegate() { labelConfigChangeValue.Text = newValue; });
            //            return true;
            //        });
            //btnWatch.Enabled = false;
        }

        private void rbPagingPrimaryKey_CheckedChanged(object sender, EventArgs e)
        {
            if(rbPagingPrimaryKey.Checked)
                _pagingMgr = _pagingMgrPrimaryKey;
             
        }

        private void rbPagingCompound_CheckedChanged(object sender, EventArgs e)
        {
            if(rbPagingCompound.Checked)
                _pagingMgr = _pagingMgrCompoundIndex;
        }

        private void rbPagingJoin_CheckedChanged(object sender, EventArgs e)
        {
            if(rbPagingJoin.Checked)
                _pagingMgr = CreatePagingMgrJoin();

            // Save state if no longer using paging manager with joined tables.
            if(!rbPagingJoin.Checked)
                _lastPagingMgrJoinState = _pagingMgr.GetPagingState();
        }

        private void btnStartAppSession_Click(object sender, EventArgs e)
        {
            StartAppSession();
        }


        private void btnChgAppCtrl_Click(object sender, EventArgs e)
        {
            if (!_currentUserCode.HasValue)
            {
                MessageBox.Show("You must first be signed on");
                return;
            }
            if (lbSignedonUsers.SelectedItem != null)
            {
                string[] userSessionItems = lbSignedonUsers.SelectedItem.ToString().Split(new char[] { ':' });
                Int64 sessionCode = Convert.ToInt64(userSessionItems[2]);
                if (_userSessions[sessionCode].IsAccessAllowed(Constants.UIControl_CleanupInactiveAppSessions))
                {
                    if (_appSession != null)
                    {
                        SignonControlStructure newSignonControlData = new SignonControlStructure();
                        newSignonControlData.SessionControlCode = _appSession.SignonControlData.SessionControlCode;
                        newSignonControlData.RestrictSignon = cbRestrictSignon.Checked;
                        newSignonControlData.ForceSignoff = cbForceSignoff.Checked;
                        newSignonControlData.FailedAttemptLimit = Convert.ToByte(numUDsignonLimit.Value);
                        newSignonControlData.RestrictSignonMsg = tbRestrictSignonMsg.Text;
                        newSignonControlData.SignoffWarningMsg = tbSignoffWarningMsg.Text;
                        newSignonControlData.StatusSeconds = Convert.ToInt16(numUDappCheckinSec.Value);
                        newSignonControlData.TimeOutSeconds = Convert.ToInt16(numUDsessionTimeOutSecs.Value);
                        newSignonControlData.LastModifiedByDateTime = _daMgr.DbSynchTime;
                        if (_currentUserCode.HasValue)
                            newSignonControlData.LastModifiedByUserCode = _currentUserCode.Value;

                        Int32? lastModifiedByUser = null;
                        DateTime? lastModifiedByDate = null;
                        if (!string.IsNullOrEmpty(tbLastModBy.Text))
                            lastModifiedByUser = Convert.ToInt32(tbLastModBy.Text);
                        if (!string.IsNullOrEmpty(tbLastModDateTime.Text))
                            lastModifiedByDate = Convert.ToDateTime(tbLastModDateTime.Text.Replace("UTC", ""));//.ToUniversalTime();
                        bool recordUpdated = _appSession.ChangeSignonControl(lastModifiedByUser
                                , lastModifiedByDate, newSignonControlData);
                        if (recordUpdated)
                        {
                            MessageBox.Show("Record Updated Successfully.");
                        }
                        else
                        {
                            MessageBox.Show("Record NOT updated; It may have been updated by another user. Try Again.");
                        }
                        LoadSessionControlRecord();
                    }
                }
                else
                {
                    MessageBox.Show("Access Denied.");
                    return;
                }
            }
            else
            {
                MessageBox.Show("You must first Select a user account.");
                return;
            }
            
        }

        private void btnStartTraceViewer_Click(object sender, EventArgs e)
        {
            Process.Start(Environment.CurrentDirectory + "\\B1.TraceViewer.Exe");
        }

        private void btnSelectMemFileFolder_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowser.ShowDialog();

            if(result == System.Windows.Forms.DialogResult.OK)
            {
                tbMemFileDirectory.Text = folderBrowser.SelectedPath;
            }
        }

        private void btnStartMemFileLogTest_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(tbMemFileNamePrefix.Text) || string.IsNullOrWhiteSpace(tbMemFileDirectory.Text))
            {
                MessageBox.Show("Log name prefix and directory must be provided.", "Missing file information.", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }
            ThreadPool.SetMinThreads(50, 50);

            btnStartMemFileLogTest.Enabled = false;

            try
            {
                Random r = new Random();
                MemoryFileLog log = new MemoryFileLog(tbMemFileNamePrefix.Text, tbMemFileNamePrefix.Text,
                        tbMemFileDirectory.Text, (int)numMemFileSize.Value, enumEventPriority.All);
           
                Action logAction = new Action( () =>
                    {
                        for(int i = 0; i < this.numMemFileMessages.Value ; i++)
                        {

                            log.Write(string.Format("Thread {0} , Number {1}, {2}", Thread.CurrentThread.ManagedThreadId.ToString(),
                                    r.Next(), "".PadRight(r.Next(20, 60), (char)r.Next(65, 90)))
                                    , true, 1, 1, EventLogEntryType.Information, enumEventPriority.Normal);
                            Thread.Sleep(1);
                        }
                    });

                Action[] logActionArray = new Action[(int)numMemFileThreads.Value];
                for(int i = 0; i < logActionArray.Length; i++)
                    logActionArray[i] = logAction;

                ThreadPool.SetMinThreads(50, 50);

                Task.Factory.StartNew(
                    new Action( () =>
                        { 
                            Parallel.Invoke(logActionArray);
                            log.Dispose();
                            BeginInvoke( new Action(() => btnStartMemFileLogTest.Enabled = true));
                        }));
                return;
            }
            catch(DirectoryNotFoundException)
            {
                MessageBox.Show("Invalid drive or directory.", "Directory not found.", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                btnStartMemFileLogTest.Enabled = true;
            }
        }

        private void btnLaunchTracingTest_Click(object sender, EventArgs e)
        {
            Process.Start(Environment.CurrentDirectory + "\\B1.Utility.TestConsoleApp.exe", "-t");
        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void tbAppCtrlServerVer_TextChanged(object sender, EventArgs e)
        {

        }

        private void tbAppCtrlProvVersion_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSignon_Click(object sender, EventArgs e)
        {
            Signon();
        }

        void Signon(bool allowMultipleSessions = false)
        {
            if (_appSession == null)
                StartAppSession();

            if (string.IsNullOrEmpty(tbUserId.Text))
            {
                MessageBox.Show("Cannot have empty UserId");
                tbUserId.Focus();
                return;
            }

            UserEnvironmentStructure ues = new UserEnvironmentStructure();
            ues.AppCode = _appSession.AppCode;
            ues.AppId = _appSession.AppId;
            ues.AppVersion = _appSession.AppVersion;
            for (int i = 0; i < nudSignonSessions.Value; i++)
            {
                SignonResultsStructure results = UserSignon.Signon(_daMgr
                        , _appSession.SignonControl
                        , tbUserId.Text
                        , tbUserSignonPwd.Text
                        , ues
                        , allowMultipleSessions);
                lblSignonResults.Text = results.ResultMessage;
                if (results.ResultEnum == SignonResultsEnum.Success)
                {
                    int index = lbSignedonUsers.Items.Add(string.Format("{0} : {1} : {2}"
                        , tbUserId.Text
                        , results.UserSessionMgr.UserCode
                        , results.UserSessionMgr.SessionCode));
                    lbSignedonUsers.SelectedIndex = index;
                    btnSignoff.Enabled = true;
                    if (_userSessions == null)
                        _userSessions = new Dictionary<long, UserSession>();
                    _userSessions.Add(results.UserSessionMgr.SessionCode, results.UserSessionMgr);
                }
                else if (results.ResultEnum == SignonResultsEnum.PasswordChangeRequired)
                {
                    frmPasswordMaintenance pwdMain = new frmPasswordMaintenance(null);
                    DialogResult dr = pwdMain.ShowDialog();
                    if (dr == System.Windows.Forms.DialogResult.OK)
                    {
                        UserSignon.ChangePassword(_daMgr, tbUserId.Text, pwdMain.NewPassword);
                        MessageBox.Show("Password Changed.  Please signin.");
                        tbUserId.Text = tbUserSignonPwd.Text = null;
                        tbUserId.Focus();
                        return;
                    }
                }
                if (nudSignonSessions.Value == 1 || results.ResultEnum != SignonResultsEnum.Success)
                {
                    if (results.ResultEnum == SignonResultsEnum.MultipleSignonRestricted)
                    {
                        DialogResult dr = MessageBox.Show(results.ResultMessage + Environment.NewLine
                                + "Click Ok to confirm new session and continue; click Cancel otherwise."
                                , "Existing Active Session Found", MessageBoxButtons.OKCancel);
                        if (dr == System.Windows.Forms.DialogResult.OK)
                        {
                            Signon(true); // override limit; allow multiple sessions
                        }
                    }
                    else MessageBox.Show(results.ResultMessage);
                    tbUserId.Text = tbUserSignonPwd.Text = null;
                }
                lblSignonResults.Text = "Enter UserId and Password (leave blank if new user).";

                if (results.ResultEnum != SignonResultsEnum.Success)
                    break;
                rbActiveUsers.Enabled = rbAllUsers.Enabled = rbInactiveUsers.Enabled = true;
            }
            RefreshScreen();
        }

        private void btnSignoff_Click(object sender, EventArgs e)
        {
            if (lbSignedonUsers.Items.Count == 0)
            {
                MessageBox.Show("No users are signed on.");
                btnSignoff.Enabled = false;
                return;
            }
            if (lbSignedonUsers.SelectedItem != null)
            {
                string[] userSessionItems = lbSignedonUsers.SelectedItem.ToString().Split(new char[] { ':' });
                Int64 sessionCode = Convert.ToInt64(userSessionItems[2]);
                if (_userSessions[sessionCode].IsAccessAllowed(Constants.UIControl_Signoff))
                {
                    UserSignon.Signoff(_daMgr, sessionCode);
                    _userSessions.Remove(sessionCode);
                    lbSignedonUsers.Items.Remove(lbSignedonUsers.SelectedItem);
                }
                else
                {
                    MessageBox.Show("Access Denied.");
                    return;
                }
            }
            else
            {
                MessageBox.Show("You must first Select a user session");
                return;
            }
        }

        private void btnUnrestrictAcnt_Click(object sender, EventArgs e)
        {
            if (dgvUserMaster.Rows.Count == 0)
            {
                MessageBox.Show("There are no user accounts.");
                btnSignoff.Enabled = false;
                return;
            }
            if (dgvUserMaster.SelectedRows.Count == 1)
            {
                string userId = dgvUserMaster.SelectedRows[0].Cells[SessionManagement.Constants.UserId].Value.ToString();
                UserSignon.ReleaseRestriction(_daMgr, userId);
                MessageBox.Show("Restriction released.");
            }
            else
            {
                MessageBox.Show("You must first Select a user account.");
                return;
            }

        }

        private void lbSignedonUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbSignedonUsers.SelectedItem != null
                || lbSignedonUsers.Items.Count > 0)
            {
                if (lbSignedonUsers.SelectedItem == null)
                    lbSignedonUsers.SelectedIndex = 0;
                string[] userSessionItems = lbSignedonUsers.SelectedItem.ToString().Split(new char[] { ':' });
                _currentUserCode = Convert.ToInt32(userSessionItems[1]);
                _currentUserId = userSessionItems[0];
                tbCurrentUser.Text = _currentUserId;
            }
        }

        private void rbAllUsers_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSession == null)
                StartAppSession();
            if (rbAllUsers.Checked)
            {
                _userSessionList = UserSignon.UserSessionListEnum.AllUserSessions;
                RefreshScreen();
            }
        }

        private void rbActiveUsers_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSession == null)
                StartAppSession();
            if (rbActiveUsers.Checked)
            {
                _userSessionList = UserSignon.UserSessionListEnum.ActiveUserSessions;
                RefreshScreen();
            }
        }

        private void rbInactiveUsers_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSession == null)
                StartAppSession();
            if (rbInactiveUsers.Checked)
            {
                _userSessionList = UserSignon.UserSessionListEnum.InActiveUserSessions;
                RefreshScreen();
            }
        }

        private void btnCleanupInactive_Click(object sender, EventArgs e)
        {
            if (!_currentUserCode.HasValue)
            {
                MessageBox.Show("You must first be signed on");
                return;
            }
            if (lbSignedonUsers.SelectedItem != null)
            {
                string[] userSessionItems = lbSignedonUsers.SelectedItem.ToString().Split(new char[] { ':' });
                Int64 sessionCode = Convert.ToInt64(userSessionItems[2]);
                if (_userSessions[sessionCode].IsAccessAllowed(Constants.UIControl_CleanupInactiveAppSessions))
                {
                    UserSignon.CleanupInactiveSessions(_daMgr);
                    RefreshScreen();
                }
                else
                {
                    MessageBox.Show("Access Denied.");
                    return;
                }
            }
            else
            {
                MessageBox.Show("You must first Select a user account.");
                return;
            } 
        }

        private void rbAllAppSessions_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSession == null)
                StartAppSession();
            if (rbAllAppSessions.Checked)
            {
                _appSessionList = AppSession.AppSessionListEnum.AllAppSessions;
                _refreshAppSessions = true;
                RefreshScreen();
            }
        }

        private void rbActiveAppSessions_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSession == null)
                StartAppSession();
            if (rbActiveUsers.Checked)
            {
                _appSessionList = AppSession.AppSessionListEnum.ActiveAppSessions;
                _refreshAppSessions = true;
                RefreshScreen();
            }
        }

        private void rbInActiveAppSessions_CheckedChanged(object sender, EventArgs e)
        {
            if (_appSession == null)
                StartAppSession();
            if (rbInActiveAppSessions.Checked)
            {
                _appSessionList = AppSession.AppSessionListEnum.InActiveAppSessions;
                _refreshAppSessions = true;
                RefreshScreen();
            }
        }

        private void btnCleanupInactiveAppSessions_Click(object sender, EventArgs e)
        {
            if (!_currentUserCode.HasValue)
            {
                MessageBox.Show("You must first be signed on");
                return;
            }
            if (lbSignedonUsers.SelectedItem != null)
            {
                string[] userSessionItems = lbSignedonUsers.SelectedItem.ToString().Split(new char[] { ':' });
                Int64 sessionCode = Convert.ToInt64(userSessionItems[2]);
                if (_userSessions[sessionCode].IsAccessAllowed(Constants.UIControl_CleanupInactiveAppSessions))
                {
                    AppSession.CleanupInactiveSessions(_daMgr);
                    _refreshAppSessions = true;
                    RefreshScreen();
                }
                else
                {
                    MessageBox.Show("Access Denied.");
                    return;
                }
            }
            else
            {
                MessageBox.Show("You must first Select a user account.");
                return;
            }
        }

        private void btnGenPwdHash_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(tbUserSignonPwd.Text))
                sb.AppendFormat("You must enter a password.");
            else
            {
                string salt = Cryptography.HashOperation.CreateRandomSalt(Cryptography.HashAlgorithmTypeEnum.SHA512HashAlgorithm);
                string hash = Cryptography.HashOperation.ComputeHash(Cryptography.HashAlgorithmTypeEnum.SHA512HashAlgorithm
                    , tbUserSignonPwd.Text
                    , salt);
                sb.AppendFormat("Pwd: {0}{1}Hash: {2}{1}Salt: {3}{1}"
                    , tbUserSignonPwd.Text
                    , Environment.NewLine
                    , hash
                    , salt);
                new GenerateHash(tbUserSignonPwd.Text, hash, salt).ShowDialog();
            }
        }

        private void btnStartTPE_Click(object sender, EventArgs e)
        {
            btnStopTPE.Enabled = btnPauseTPE.Enabled = true;
            btnStartTPE.Enabled = false;
            if (_daMgr == null)
                CreateDbMgr();
            _tpe = new TaskProcessing.TaskProcessEngine(_daMgr);
            _tpe.Start();
        }

        private void btnStopTPE_Click(object sender, EventArgs e)
        {
            btnStopTPE.Enabled = btnPauseTPE.Enabled = btnResumeTPE.Enabled = false;
            btnStartTPE.Enabled = true;
            _tpe.Stop();
        }

        private void btnPauseTPE_Click(object sender, EventArgs e)
        {
            btnPauseTPE.Enabled = false;
            btnResumeTPE.Enabled = true;
            _tpe.Pause();
        }

        private void btnResumeTPE_Click(object sender, EventArgs e)
        {
            btnPauseTPE.Enabled = true;
            btnResumeTPE.Enabled = false;
            _tpe.Resume();
        }

        private void btnTaskRegister_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.DefaultExt = ".dll";
            DialogResult dr = ofd.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                if (_daMgr == null)
                    CreateDbMgr();
                TaskProcessing.TaskRegistration.RegisterAssemblyTasks(_daMgr, ofd.SafeFileName, ofd.FileName, true, true);
            }
        }

        private void btnTestEFUpdate_Click(object sender, EventArgs e)
        {
            if(_daMgr == null)
                CreateDbMgr();

            Models.SampleDbEntities entities = new Models.SampleDbEntities();

            int seqParam = 0;

            //Select top 10 entities ordered by appsequenceid
            DbCommand cmdSelect = _daMgr.BuildSelectDbCommand(
                    from a in entities.TestSequences where a.AppSequenceId > seqParam orderby a.AppSequenceId select a, 10);

            var sequences = _daMgr.ExecuteContext<Models.TestSequence>(cmdSelect, null, entities);

            DbCommand dbCmd = null;

            // the overloads collection is used for columns that require a database operation and are not known to the EF
            // for example (getdate()).  So we show example with column DbServerTime
            Dictionary<string, object> overloads = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            overloads.Add(Constants.DbServerTime, _daMgr.GetDbTimeAs(EnumDateTimeLocale.UTC, null));
            foreach (Models.TestSequence seq in sequences)
            {
                seq.Remarks = "Updated By EF Test Update; localTime: " + DateTime.Now.ToString("HH:mm:ss:fff");
                seq.AppLocalTime = DateTime.Now;
                seq.AppSynchTime = _daMgr.DbSynchTime;

                // First time in, dbCmd will be null so a new command will be created. 
                // Subsequent calls will use the first DbCommand.
                // Also, each call to the db will update 
                Tuple<ObjectContext, DbCommand> results = _daMgr.UpdateEntity(entities, seq, overloads, null, dbCmd);
                dbCmd = results.Item2;
            }

        }

        private void btnTestMultiContextSingleLINQ_Click(object sender, EventArgs e)
        {
            if (_daMgr == null)
                CreateDbMgr();

            TestDataAccessMgr aa = new TestDataAccessMgr(_daMgr, 1, null);
            aa.testEntities();

            //?? TestDataAccessMgr.TestDbMultiContext(_daMgr);
        }

        private void btnTestEFInsert_Click(object sender, EventArgs e)
        {
            if(_daMgr == null)
                CreateDbMgr();

            Models.SampleDbEntities entities = new Models.SampleDbEntities();

            DbCommand dbCmd = null;

            // the overloads collection is used for columns that require a database operation and are not known to the EF
            // for example (getdate()).  So we show example with column DbServerTime
            Dictionary<string, object> overloads = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            overloads.Add(Constants.DbServerTime, _daMgr.GetDbTimeAs(EnumDateTimeLocale.UTC, null));

            Int64 appSequenceId = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);

            DbFunctionStructure autogenerate = new DbFunctionStructure();
            if (_daMgr.DatabaseType == DataAccessMgr.EnumDbType.SqlServer
                || _daMgr.DatabaseType == DataAccessMgr.EnumDbType.Db2)
                autogenerate.AutoGenerate = true; // identity column
            else
            { // oracle sequence
                autogenerate.AutoGenerate = false;
                autogenerate.FunctionBody = DataAccess.Constants.SCHEMA_CORE + ".DbSequenceId_Seq.nextVal";
            }

            overloads.Add(Constants.DbSequenceId, autogenerate);

            Models.TestSequence seq = new Models.TestSequence()
            {
                AppSequenceId = appSequenceId,
                AppSequenceName = "EF Insert",
                AppLocalTime = DateTime.Now,
                AppSynchTime = _daMgr.DbSynchTime,
                Remarks = "Added by EF Test Insert"
            };

       
            _daMgr.InsertEntity(entities, seq, overloads, null);

            
            overloads.Clear();
            if (_daMgr.DatabaseType == DataAccessMgr.EnumDbType.Oracle)
            {
                autogenerate.FunctionBody = DataAccess.Constants.SCHEMA_CORE + ".TESTDBSEQUENCEID_SEQ.nextVal";
                overloads.Add(Constants.DbSequenceId, autogenerate);
            }

            _daMgr.InsertEntity(entities, new Models.TestDbSequenceId() { Remarks = "Added by EF Test Insert" },
                    overloads, null);      
            
        }

    }
}
