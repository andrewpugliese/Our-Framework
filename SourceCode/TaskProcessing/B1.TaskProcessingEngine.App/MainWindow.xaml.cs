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

        RemoteHostClient _remoteTpe = null;
        TaskProcessing.TaskProcessingEngine _localTpe = null;
        LoggingMgr _loggingMgr = null;
        DataAccessMgr _daMgr = null;
        AppSession _appSession = null;
        
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

        public MainWindow()
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

            PagingMgr tpq = new PagingMgr(_daMgr, "B1.TaskProcessingQueue", null, 1, null);

            pagingTableTPQ.Source = tpq;
            pagingTableTPQ.Title = "Task Processing Queue";

            localTpeStatus.SetContext(this, null);
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

            remoteTpeStatus.IsEnabled = false;
        }

        public void Start(object context)
        {
#warning"identify remote instance in context"
            if (context == null)
            {
                // ensure that this tpe host is unique host session base on engineId
                string sessionExists = _appSession.Start(_configSettings, "Application Startup", false, _hostEndpointAddress);
                while (!string.IsNullOrEmpty(sessionExists))
                    if (MessageBox.Show(sessionExists + " Press OK to reset; CANCEL to stop."
                        , "Session Startup Conflict", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        return;
                    // otherwise reset session record (remove existing)
                    else sessionExists = _appSession.Start(_configSettings
                            , "Application Startup After Session Conflict Reset"
                            , true);

                _localTpe = new TaskProcessing.TaskProcessingEngine(_daMgr
                        , _taskAssemblyPath
                        , _engineId
                        , _configId
                        , _appSession.SignonControl
                        , _maxTasks
                        , _hostEndpointAddress);
                _localTpe.Start();
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
                _remoteTpe.Connect(_engineId);
                Dictionary<string, string> configSettings = (Dictionary<string,string>)Core.Functions.Deserialize<Dictionary<string,string>>
                            (_remoteTpe.ConfigSettings());
                Dictionary<string, string> dynamicSettings = (Dictionary<string,string>)Core.Functions.Deserialize<Dictionary<string,string>>
                            (_remoteTpe.DynamicSettings());
                DisplayRemoteTpeConfiguration(configSettings, dynamicSettings);
            }
        }

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

        public void Stop(object context)
        {
            if (context == null)
            {
                if (_localTpe != null)
                    _localTpe.Stop();
                if (_appSession != null)
                    _appSession.End();
                Status(context);
            }
            else
            {
                // WCF DisConnect
                remoteTpeStatus.Clear();
                _remoteTpe.Disconnect(_engineId);
                pagingTableRemoteHosts.IsEnabled = true;
            }
        }

        public void DisplayPausedState()
        {
            localTpeStatus.DisplayPausedState();
        }

        public void DisplayResumedState()
        {
            localTpeStatus.DisplayResumedState();
        }

        public void Pause(object context)
        {
            if (context == null && _localTpe != null)
                _localTpe.Pause();
            if (context != null && _remoteTpe != null)
                _remoteTpe.Pause(_engineId);
            Status(context);
        }

        public void Resume(object context)
        {
            if (context == null && _localTpe != null)
                _localTpe.Resume();
            if (context != null && _remoteTpe != null)
                _remoteTpe.Resume(_engineId);
            Status(context);
        }

        public string Status()
        {
            return Status(null);
        }

        public string Status(object context)
        {
            if (context == null && _localTpe != null)
            {
                localTpeStatus.Status = _localTpe.Status();
                if (_daMgr != null && _daMgr.loggingMgr != null)
                    localTpeStatus.DisplayTraceLevelState(_daMgr.loggingMgr.TraceLevel);
                if (_localTpe.EngineStatus == TaskProcessing.TaskProcessingEngine.EngineStatusEnum.Paused)
                    localTpeStatus.DisplayPausedState();
                else localTpeStatus.DisplayResumedState();
                localTpeStatus.DisplayMaxTasks(_localTpe.MaxTasks);
            }
            if (context != null && _remoteTpe != null)
            {
                Dictionary<string, string> dynamicSettings 
                        = (Dictionary<string, string>)Core.Functions.Deserialize<Dictionary<string, string>>
                            (_remoteTpe.Status(context.ToString()));
                string engineStatus = dynamicSettings[TaskProcessing.Constants.EngineStatus];
                if (engineStatus == TaskProcessing.TaskProcessingEngine.EngineStatusEnum.Paused.ToString())
                    remoteTpeStatus.DisplayPausedState();
                else remoteTpeStatus.DisplayResumedState();
                string status = dynamicSettings[TaskProcessing.Constants.StatusMsg];
                remoteTpeStatus.Status = status;
                remoteTpeStatus.DisplayTraceLevelState(ILoggingManagement.Constants.ToTraceLevel(
                        dynamicSettings[ILoggingManagement.Constants.TraceLevel]));
                remoteTpeStatus.DisplayMaxTasks(Convert.ToInt32(dynamicSettings[TaskProcessing.Constants.MaxTasksInParallel]));
            }
            return string.Empty;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }

        private void Exit()
        {
            Stop(null);
            if (_daMgr != null)
                _daMgr.ClearUniqueIdsCache();
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Close();
        }

        private void TraceLevelChanged(string newTraceLevel)
        {
            if (_loggingMgr != null)
                _loggingMgr.TraceLevel = ILoggingManagement.Constants.ToTraceLevel(newTraceLevel);
        }

        private void RemoteTraceLevelChanged(string newTraceLevel)
        {
            if (_remoteTpe != null)
                _remoteTpe.SetTraceLevel(_engineId, newTraceLevel);
        }

        private void MaxTasksChanged(int delta)
        {
            if (_localTpe != null)
                _maxTasks = _localTpe.SetMaxTasks(delta);
            _maxTasks += delta;
            if (_maxTasks <= 0)
                _maxTasks = Environment.ProcessorCount * 1;
        }

        private void tabRemoteHosts_GotFocus(object sender, RoutedEventArgs e)
        {
            if (pagingTableRemoteHosts.Source == null)
            {
                pagingTableRemoteHosts.Source = new PagingMgr(_daMgr, "B1.AppSessions", null, 1, null); ;
                pagingTableRemoteHosts.Title = "Task Processing Engine Host Sessions";
                pagingTableRemoteHosts.SelectionChangedHandler = RemoteHostSelection;
                pagingTableRemoteHosts.First();
            }
        }

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

        private void OnPlusMinusClick(object context, int delta)
        {
            if (context != null)
            {
                if (_remoteTpe != null)
                    _remoteTpe.SetMaxTasks(_engineId, delta);
            }
            else MaxTasksChanged(delta);
        }

        private void wdwDashboard_Closed(object sender, EventArgs e)
        {
            Exit();
        }
    }
}
