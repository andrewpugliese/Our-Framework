using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceModel;

using B1.Configuration;
using B1.DataAccess;
using B1.ILoggingManagement;
using B1.LoggingManagement;
using B1.Wpf.Controls;
using B1.TaskProcessing;
using B1.TaskProcessingEngine;
using B1.SessionManagement;


namespace B1.TaskProcessingEngine.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , IProcessControl
    {
        // configuration settings variables
        static string _assemblyName
                = System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Assembly.GetName().Name;
        static string _assemblyVersion
                = System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Assembly.GetName().Version.ToString();
        static string _configSettings = null;
        static string _connectionKey = null;
        static string _loggingKey = null;
        static string _configId = null;
        static string _engineId = null;
        static string _taskAssemblyPath = null;
        static string _hostEndpointAddress = null;
        static int _maxTasks = Environment.ProcessorCount;

        // handle to a remote TPE host
        RemoteHostClient _remoteTpe = null;
        // handle to the local host instance of the TPE
        TaskProcessing.TaskProcessingEngine _localTpe = null;

        // components for logging / tracing / and exception handling
        LoggingMgr _loggingMgr = null;
        // component for data access
        DataAccessMgr _daMgr = null;
        // component for managing TPE instances (preventing duplicate instances)
        // and User Signon Control
        AppSession _appSession = null;
        

        // loads configuration parameters and returns a string of all configuration settings
        // that can be used to store in database
        string LoadConfigurationSettings()
        {
            StringBuilder configSettings = new StringBuilder();
            _connectionKey = AppConfigMgr.GetValue(DataAccess.Constants.ConnectionKey);
            configSettings.AppendFormat("{0} : {1}", DataAccess.Constants.ConnectionKey, _connectionKey);
            _loggingKey = AppConfigMgr.GetValue(ILoggingManagement.Constants.LoggingKey);
            configSettings.AppendFormat("{0} : {1}", ILoggingManagement.Constants.LoggingKey, _loggingKey);
            _engineId = AppConfigMgr.GetValueOrDefault(TaskProcessing.Constants.EngineId, null);
            configSettings.AppendFormat("{0} : {1}", TaskProcessing.Constants.EngineId, _engineId);
            _taskAssemblyPath = AppConfigMgr.GetValueOrDefault(TaskProcessing.Constants.TaskAssemblyPath, null);
            configSettings.AppendFormat("{0} : {1}", TaskProcessing.Constants.TaskAssemblyPath, _taskAssemblyPath);
            _hostEndpointAddress = AppConfigMgr.GetValueOrDefault(TaskProcessing.Constants.HostEndpointAddress, null);
            configSettings.AppendFormat("{0} : {1}", TaskProcessing.Constants.HostEndpointAddress, _hostEndpointAddress);
            string maxTasks = AppConfigMgr.GetValueOrDefault(TaskProcessing.Constants.MaxTasksInParallel
                    , _maxTasks.ToString());
            if (!string.IsNullOrEmpty(maxTasks))
            {
                int maxTasksTmp;
                if (int.TryParse(maxTasks, out maxTasksTmp))
                    _maxTasks = maxTasksTmp;
            }

            configSettings.AppendFormat("{0} : {1}", TaskProcessing.Constants.MaxTasksInParallel, maxTasks);
            configSettings.AppendFormat("{0} : {1}", ILoggingManagement.Constants.TraceLevel, enumTraceLevel.None.ToString());
            _configId = AppConfigMgr.GetValueOrDefault(TaskProcessing.Constants.ConfigId, null);
            configSettings.AppendFormat("{0} : {1}", TaskProcessing.Constants.ConfigId, _configId);
            return configSettings.ToString();
        }

        /// <summary>
        /// Main Window for Application
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                _configSettings = LoadConfigurationSettings();
                _loggingMgr = new LoggingMgr(_loggingKey);
                _loggingMgr.TraceToWindow = true;
                _daMgr = new DataAccessMgr(_connectionKey, _loggingMgr);

                // This host requires an app session object
                _appSession = new AppSession(_daMgr
                    , _engineId
                    , _assemblyVersion
                    , _assemblyName
                    , Status);

                // create paging manager for task processing queue
                PagingMgr tpq = new PagingMgr(_daMgr
                        , string.Format("{0}.{1}", DataAccess.Constants.SCHEMA_CORE, TaskProcessing.Constants.TaskProcessingQueue)
                        , null, 1, null);

                // pass paging manager to paging controll
                pagingTableTPQ.Source = tpq;
                pagingTableTPQ.Title = "Task Processing Queue";

                // set the status control for the local TPE
                localTpeStatus.SetContext(this, null, OnPlusMinusClick);
                // pass in the configuration settings
                localTpeStatus.Display(TraceLevelChanged
                        , MaxTasksChanged
                        , _connectionKey
                        , _loggingKey
                        , _configId
                        , _engineId
                        , _taskAssemblyPath
                        , _hostEndpointAddress
                        , _maxTasks
                        , _loggingMgr.TraceLevel);

                // right now we cannot connect to any remote host
                remoteTpeStatus.IsEnabled = false;
            }
            catch(Exception e)
            {
                string errorFileName = "TaskProcessingEngine.Exception.txt";
                string msg = "Will attempt to write exception details to file: " + errorFileName
                    + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine;
                MessageBox.Show(msg, "Fatal Error - Look for file: " + errorFileName);
                FileManagement.FileMgr.WriteTextToFile(errorFileName, msg, false, true);
                Exit();
            }
            

        }

        /// <summary>
        /// Start (or Connect) event handler.
        /// <para>There are 2 instances of the process control control.</para>
        /// <para>When context is null, this function handles the Start event for the local TPE.</para>
        /// <para>When context is not null, this function handles the Connect event for a remote TPE.</para>
        /// </summary>
        /// <param name="context">Indicates to handler method whether it is from the local or remote TPE.</param>
        public void Start(object context)
        {
            // Start Event from local TPE
            if (context == null)
            {
                // ensure that this tpe host is unique host session base on engineId
                // When the Overwrite parameter of appSession.Start is false
                // we will receive an error message when there is an existing record.
                // As long as we receive an error message, the session already exists
                // so user will be presented with dialog box.
                // When user confirms, then we call function with Overwrite = true;
                string sessionExists = _appSession.Start(_configSettings, "Application Startup", false, _hostEndpointAddress);
                while (!string.IsNullOrEmpty(sessionExists))
                    if (MessageBox.Show(sessionExists + " Press OK to reset; CANCEL to stop."
                        , "Session Startup Conflict", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        return;
                    // otherwise reset session record (remove existing)
                    else sessionExists = _appSession.Start(_configSettings
                            , "Application Startup After Session Conflict Reset"
                            , true
                            , _hostEndpointAddress); // host address of this local TPE to be used for other applications to connect to.

                // instantiate this application's local TPE
                _localTpe = new TaskProcessing.TaskProcessingEngine(_daMgr
                        , _taskAssemblyPath
                        , _engineId
                        , _configId
                        , _appSession.SignonControl
                        , _maxTasks
                        , _hostEndpointAddress);
                // start the TPE
                _localTpe.Start();
                // display status
                Status();
            }
            else
            {
                // WCF Connect
                pagingTableRemoteHosts.IsEnabled = false;
                EndpointAddress remoteAddress = new EndpointAddress(string.Format("{0}{1}", _hostEndpointAddress
                        , _engineId));
                WSHttpBinding binding = new WSHttpBinding(SecurityMode.None);
                _remoteTpe = new RemoteHostClient(binding, remoteAddress);
                RemoteHostResponse response =_remoteTpe.Connect(_engineId);
                if (response.Success)
                {
                    RemoteHostResponseString configSettingsResponse = _remoteTpe.ConfigSettings();
                    RemoteHostResponseString dynamicSettingsReponse = _remoteTpe.DynamicSettings();
                    if (configSettingsResponse.Success && dynamicSettingsReponse.Success)
                    {
                        Dictionary<string, string> configSettings = (Dictionary<string, string>)
                                Core.Functions.Deserialize<Dictionary<string, string>>
                                    (configSettingsResponse.ReturnValue);
                        Dictionary<string, string> dynamicSettings = (Dictionary<string, string>)
                                Core.Functions.Deserialize<Dictionary<string, string>>
                                    (dynamicSettingsReponse.ReturnValue);
                        DisplayRemoteTpeConfiguration(configSettings, dynamicSettings);
                    }
                    else remoteTpeStatus.Status = string.Format("{0}{1}{2}", configSettingsResponse.ErrorMsg
                            , Environment.NewLine, dynamicSettingsReponse.ErrorMsg);
                }
                else HostConnectionAborted(response.ErrorMsg);
            }
        }

        /// <summary>
        /// Display the communication error message received when calling the remote host.
        /// <para>Show that the host has been disconnected.</para>
        /// </summary>
        /// <param name="errorMsg">String error message received.</param>
        void HostConnectionAborted(string errorMsg)
        {
            remoteTpeStatus.Clear();
            remoteTpeStatus.Status = errorMsg;
            remoteTpeStatus.DisplayDisconnectedState();
            pagingTableRemoteHosts.IsEnabled = true;
        }

        /// <summary>
        /// Displays the configuration and runtime settings of the remote host
        /// </summary>
        /// <param name="configSettings">Remote host's configuration settings dictionary</param>
        /// <param name="dynamicSettings">Remote host's runtime settings dictionary</param>
        void DisplayRemoteTpeConfiguration(Dictionary<string, string> configSettings
                , Dictionary<string, string> dynamicSettings)
        {
            remoteTpeStatus.Display(RemoteTraceLevelChanged
                    , null
                    , configSettings[DataAccess.Constants.ConnectionKey]
                    , configSettings[ILoggingManagement.Constants.LoggingKey]
                    , configSettings[TaskProcessing.Constants.ConfigId]
                    , configSettings[TaskProcessing.Constants.EngineId]
                    , configSettings[TaskProcessing.Constants.TaskAssemblyPath]
                    , configSettings[TaskProcessing.Constants.HostEndpointAddress]
                    , Convert.ToInt32(dynamicSettings[TaskProcessing.Constants.MaxTasksInParallel])
                    , ILoggingManagement.Constants.ToTraceLevel(dynamicSettings[ILoggingManagement.Constants.TraceLevel]));
            string engineStatus = dynamicSettings[TaskProcessing.Constants.EngineStatus];
            if (engineStatus == TaskProcessing.TaskProcessingEngine.EngineStatusEnum.Paused.ToString())
                remoteTpeStatus.DisplayPausedState();
            remoteTpeStatus.Status = dynamicSettings[TaskProcessing.Constants.StatusMsg];
            remoteTpeStatus.DisplayMaxTasks(Convert.ToInt32(dynamicSettings[TaskProcessing.Constants.MaxTasksInParallel]));
        }

        /// <summary>
        /// Handles the stop event for the local host and the (Disconnect) from the remote host
        /// </summary>
        /// <param name="context">Indicates whether the event came from local or remote host</param>
        public void Stop(object context)
        {
            if (context == null)
            {
                // stop the local engine
                if (_localTpe != null)
                    _localTpe.Stop();
                // remove the session record
                if (_appSession != null)
                    _appSession.End();
                Status(context);
            }
            else
            {
                // Inform the remote host that we are no longer monitoring it.
                remoteTpeStatus.Clear();
                _remoteTpe.Disconnect(_engineId);
                pagingTableRemoteHosts.IsEnabled = true;
            }
        }

        /// <summary>
        /// Updates the display so that it reflects the local TPE in a resumed state.
        /// </summary>
        public void DisplayPausedState()
        {
            localTpeStatus.DisplayPausedState();
        }

        /// <summary>
        /// Updates the display so that it reflects the local TPE in a resumed state.
        /// </summary>
        public void DisplayResumedState()
        {
            localTpeStatus.DisplayResumedState();
        }

        /// <summary>
        /// Pauses processing on the local or remote host
        /// </summary>
        /// <param name="context">Indicates whether the event came from local or remote host</param>
        public void Pause(object context)
        {
            if (context == null && _localTpe != null)
                _localTpe.Pause();
            if (context != null && _remoteTpe != null)
                _remoteTpe.Pause(_engineId);
            Status(context);
        }

        /// <summary>
        /// Resumes processing on the local or remote host
        /// </summary>
        /// <param name="context">Indicates whether the event came from local or remote host</param>
        public void Resume(object context)
        {
            if (context == null && _localTpe != null)
                _localTpe.Resume();
            if (context != null && _remoteTpe != null)
                _remoteTpe.Resume(_engineId);
            Status(context);
        }

        /// <summary>
        /// Returns status of local TPE
        /// </summary>
        /// <returns>Status String</returns>
        public string Status()
        {
            return Status(null);
        }

        /// <summary>
        /// Handles the get status event for the local and remote host.
        /// <para>When remote host, it also retrieves the latest runtime settings of the remote host and displays them.</para>
        /// </summary>
        /// <param name="context">Indicates whether the event came from local or remote host</param>
        /// <returns>The status string</returns>
        public string Status(object context)
        {
            // If it is the local host
            if (context == null && _localTpe != null)
            {
                // get the status
                localTpeStatus.Status = _localTpe.Status();
                // verify that the display settings match watch is actually
                // assigned to the controls.
                if (_daMgr != null && _daMgr.loggingMgr != null)
                    localTpeStatus.DisplayTraceLevelState(_daMgr.loggingMgr.TraceLevel);
                if (_localTpe.EngineStatus == TaskProcessing.TaskProcessingEngine.EngineStatusEnum.Paused)
                    localTpeStatus.DisplayPausedState();
                else if (_localTpe.EngineStatus == TaskProcessing.TaskProcessingEngine.EngineStatusEnum.Running)
                    localTpeStatus.DisplayResumedState();
                else if (_localTpe.EngineStatus == TaskProcessing.TaskProcessingEngine.EngineStatusEnum.Stopped)
                    localTpeStatus.DisplayDisconnectedState();
                localTpeStatus.DisplayMaxTasks(_localTpe.MaxTasks);
            }
                // otherwise, if it was a remote host event
                // request status from the remote host
            else if (context != null && _remoteTpe != null)
            {
                // response lets us know if there was a communication failure
                RemoteHostResponseString response = _remoteTpe.Status(context.ToString());
                if (response.Success)
                {
                    // dynamic settings provide us with the runtime settings of the configuration settings
                    Dictionary<string, string> dynamicSettings
                            = (Dictionary<string, string>)Core.Functions.Deserialize<Dictionary<string, string>>
                                (response.ReturnValue.ToString());
                    string engineStatus = dynamicSettings[TaskProcessing.Constants.EngineStatus];
                    if (engineStatus == TaskProcessing.TaskProcessingEngine.EngineStatusEnum.Paused.ToString())
                        remoteTpeStatus.DisplayPausedState();
                    else remoteTpeStatus.DisplayResumedState();
                    remoteTpeStatus.Status = dynamicSettings[TaskProcessing.Constants.StatusMsg]; ;
                    remoteTpeStatus.DisplayTraceLevelState(ILoggingManagement.Constants.ToTraceLevel(
                            dynamicSettings[ILoggingManagement.Constants.TraceLevel]));
                    remoteTpeStatus.DisplayMaxTasks(Convert.ToInt32(dynamicSettings[TaskProcessing.Constants.MaxTasksInParallel]));
                }
                else HostConnectionAborted(response.ErrorMsg);
            }
            return string.Empty;
        }

        /// <summary>
        /// Handles the button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }

        /// <summary>
        /// Handles all the required operations for graceful exit.
        /// </summary>
        private void Exit()
        {
            Stop(null);
            if (_daMgr != null)
                _daMgr.ClearUniqueIdsCache();
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Close();
        }

        /// <summary>
        /// Handles traceLevel changed event for the local host
        /// </summary>
        /// <param name="newTraceLevel">The new trace level that needs to be assigned</param>
        private void TraceLevelChanged(string newTraceLevel)
        {
            if (_loggingMgr != null)
                _loggingMgr.TraceLevel = ILoggingManagement.Constants.ToTraceLevel(newTraceLevel);
        }

        /// <summary>
        /// Handles traceLevel changed event for the remote host
        /// </summary>
        /// <param name="newTraceLevel">The new trace level that needs to be assigned</param>
        private void RemoteTraceLevelChanged(string newTraceLevel)
        {
            if (_remoteTpe != null)
                _remoteTpe.SetTraceLevel(_engineId, newTraceLevel);
        }

        /// <summary>
        /// Handles the MaxTasksChanged event for the local TPE.
        /// </summary>
        /// <param name="delta"></param>
        private void MaxTasksChanged(int delta)
        {
            if (_localTpe != null)
                _maxTasks = _localTpe.SetMaxTasks(delta);
            _maxTasks += delta;
            if (_maxTasks <= 0)
                _maxTasks = Environment.ProcessorCount * 1;
        }

        /// <summary>
        /// Handles the got focus event for the remote hosts tab.
        /// <para>If the paging control for remote hosts is null, it will be populated</para>
        /// </summary>
        /// <param name="sender">The sender object of the event</param>
        /// <param name="e">The RoutedEventArgs of the event</param>
        private void tabRemoteHosts_GotFocus(object sender, RoutedEventArgs e)
        {
            if (pagingTableRemoteHosts.Source == null)
            {
                pagingTableRemoteHosts.Source = new PagingMgr(_daMgr, string.Format("{0}.{1}"
                        , DataAccess.Constants.SCHEMA_CORE, SessionManagement.Constants.AppSessions)
                        , null, 1, null);
                pagingTableRemoteHosts.Title = "Task Processing Engine Host Sessions";
                pagingTableRemoteHosts.SelectionChangedHandler = RemoteHostSelection;
                pagingTableRemoteHosts.First();
            }
        }

        /// <summary>
        /// Function which handles the response to when a user clicks on the paging control.
        /// <para>It determines if the click was on the paging control of remote TPE hosts.
        /// If it was on remote TPE and the row contains a host with an endpoint address, then it activates
        /// the Connect button.</para>
        /// </summary>
        /// <param name="source">The paging manager source to be compared to pagingMgr of remote hosts</param>
        /// <param name="currentRow">The current row clicked on.</param>
        private void RemoteHostSelection(PagingMgr source, DataRow currentRow)
        {
            // we need to distinguish a click on the local host tab from a 
            // click on the remote hosts tabl.
            // There were two different pagingMgr instances; we compare the hash.
            // do we have data and is it a remoteHost source
            if (currentRow != null
                && source.GetHashCode() == pagingTableRemoteHosts.Source.GetHashCode())
            {
                // do we have an endpoint address to connect to?
                if (currentRow[SessionManagement.Constants.TpeEndpointAddress] != null
                    && currentRow[SessionManagement.Constants.TpeEndpointAddress] != DBNull.Value)
                {
                    remoteTpeStatus.SetContext(this
                            , currentRow[SessionManagement.Constants.TpeEndpointAddress]
                            , OnPlusMinusClick
                            , "Connect"
                            , "Disconnect");
                    remoteTpeStatus.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Handles the plus minus click event for either local or remote TPE.
        /// </summary>
        /// <param name="context">Indicates to handler method whether it is from the local or remote TPE.</param>
        /// <param name="delta">Indicates when the amount of change either positive or negative.</param>
        private void OnPlusMinusClick(object context, int delta)
        {
            // if it is a remote engine
            if (context != null)
            {
                // if we are connected to a remote engine, set its max tasks
                if (_remoteTpe != null)
                    _remoteTpe.SetMaxTasks(_engineId, delta);
            }
                // otherwise it was local TPE; update the max tasks.
            else MaxTasksChanged(delta);
        }

        // if the window was closed, application must exit
        private void wdwDashboard_Closed(object sender, EventArgs e)
        {
            Exit();
        }
    }
}
