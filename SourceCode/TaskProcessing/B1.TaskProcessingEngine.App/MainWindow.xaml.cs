using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using B1.Configuration;
using B1.DataAccess;
using B1.ILoggingManagement;
using B1.LoggingManagement;
using B1.Wpf.Controls;
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

            tpq = new PagingMgr(_daMgr, "B1.AppSessions", null, 1, null);
            pagingTableRemoteHosts.Source = tpq;
            pagingTableRemoteHosts.Title = "Task Processing Engine Host Sessions";
            remoteTpeStatus.SetContext(this, true);
            localTpeStatus.SetContext(this, false);

            localTpeStatus.Display(null
                    , null
                    , _connectionKey
                    , _loggingKey
                    , _configId
                    , _engineId
                    , _taskAssemblyPath
                    , _hostEndpointAddress
                    , _maxTasks);
        }

        public void Start(object context)
        {
            // ensure that this tpe host is unique host session base on engineId
            string sessionExists = _appSession.Start(_configSettings, "Application Startup", false);
            while (!string.IsNullOrEmpty(sessionExists))
                if (MessageBox.Show(sessionExists + " Press OK to reset; CANCEL to stop."
                    , "Session Startup Conflict", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    return;
                // otherwise reset session record (remove existing)
                else sessionExists = _appSession.Start(_configSettings, "Application Startup After Session Conflict Reset", true);

            _localTpe = new TaskProcessing.TaskProcessingEngine(_daMgr
                    , _taskAssemblyPath
                    , _engineId
                    , _configId
                    , _appSession.SignonControl
                    , _maxTasks);
            _localTpe.Start(context);
            Status();
        }

        public void Stop(object context)
        {
            if (_localTpe != null)
                _localTpe.Stop(context);
            if (_appSession != null)
                _appSession.End();
            Status(context);
        }

        public void Pause(object context)
        {
            _localTpe.Pause(context);
            Status(context);
        }

        public void Resume(object context)
        {
            _localTpe.Resume(context);
            Status(context);
        }

        public string Status()
        {
            return Status(null);
        }

        public string Status(object context)
        {
            if (_localTpe != null)
               localTpeStatus.Status = _localTpe.Status(context);
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
            Application.Current.MainWindow.Close();
        }

        private void TraceLevelChanged(string newTraceLevel)
        {
            if (_loggingMgr != null)
                _loggingMgr.TraceLevel = ILoggingManagement.Constants.ToTraceLevel(newTraceLevel);
        }

        private void MaxTasksChanged(int newMaxTasks)
        {
            if (_localTpe != null)
                _localTpe.MaxTaskProcesses(_maxTasks - newMaxTasks);
            _maxTasks = newMaxTasks;
        }

        private void tabRemoteHosts_GotFocus(object sender, RoutedEventArgs e)
        {
            pagingTableRemoteHosts.First();
            pagingTableRemoteHosts.Refresh();
        }

    }
}
