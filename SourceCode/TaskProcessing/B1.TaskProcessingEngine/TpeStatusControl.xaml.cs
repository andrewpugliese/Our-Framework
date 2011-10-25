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
using System.Xaml;

using B1.Wpf.Controls;

namespace B1.TaskProcessing

{
    /// <summary>
    /// Interaction logic for TpeStatusControl.xaml
    /// </summary>
    public partial class TpeStatusControl : UserControl
    {
        public delegate void TraceLevelChangeHandler(string newTraceLevel);
        public delegate void MaxTasksChangeHandler(int newMaxTasks);
        IProcessControl _parent = null;
        TraceLevelChangeHandler _traceLevelHdlr = null;
        MaxTasksChangeHandler _maxTasksHdlr = null;
        int _maxTasks = Environment.ProcessorCount;
        bool _remoteHost = false;

        public TpeStatusControl()
        {
            InitializeComponent();
            cmbTraceLevel.Items.Add(ILoggingManagement.enumTraceLevel.None);
            cmbTraceLevel.Items.Add(ILoggingManagement.enumTraceLevel.Level1);
            cmbTraceLevel.Items.Add(ILoggingManagement.enumTraceLevel.Level2);
            cmbTraceLevel.Items.Add(ILoggingManagement.enumTraceLevel.Level3);
            cmbTraceLevel.Items.Add(ILoggingManagement.enumTraceLevel.Level4);
            cmbTraceLevel.Items.Add(ILoggingManagement.enumTraceLevel.Level5);
            cmbTraceLevel.Items.Add(ILoggingManagement.enumTraceLevel.All);
            cmbTraceLevel.SelectedIndex = 0;
        }


        public void SetContext(IProcessControl clientControl, bool remoteHost = false)
        {
            _parent = clientControl;
            _remoteHost = remoteHost;
            if (_remoteHost)
                tpeControl.SetContext(_parent
                        , _remoteHost
                        , "Connect"
                        , "Disconnect");

        }

        public void Display(TraceLevelChangeHandler traceLevelHdlr
            , MaxTasksChangeHandler maxTasksHdlr
            , string connectionKey
            , string loggingKey
            , string configId
            , string engineId
            , string taskAssemblyPath
            , string hostEndpointAddress
            , int maxTasks)
        {
            _maxTasksHdlr = maxTasksHdlr;
            _traceLevelHdlr = traceLevelHdlr;
            tbConnKey.Text = connectionKey;
            tbLoggingKey.Text = loggingKey;
            tbConfigId.Text = configId;
            tbEngineId.Text = engineId;
            tbTaskAssemblyPath.Text = taskAssemblyPath;
            tbHostEndpointAddress.Text = hostEndpointAddress;
            _maxTasks = maxTasks;
            sbMaxTasks.Maximum = _maxTasks;
            sbMaxTasks.Minimum = 1;
            sbMaxTasks.Value = sbMaxTasks.Minimum;
            tbMaxTasks.Text = Convert.ToInt32(sbMaxTasks.Value).ToString();
        }

        public string ConnectionKey
        {
            get { return tbConnKey.Text; }
        }

        public string LoggingKey
        {
            get { return tbLoggingKey.Text; }
        }

        public string EngineId
        {
            get { return tbEngineId.Text; }
        }

        public string ConfigId
        {
            get { return tbConfigId.Text; }
        }

        public string TaskAssemblyPath
        {
            get { return tbTaskAssemblyPath.Text; }
        }

        public string HostEndpointAddress
        {
            get { return tbHostEndpointAddress.Text; }
        }

        public string Status
        {
            get { return tbTPEStatus.Text; }
            set { tbTPEStatus.Text = value; }
        }

        private void sbMaxTasks_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (tbMaxTasks != null
                && !string.IsNullOrEmpty(tbMaxTasks.Text))
            {
                _maxTasks = e.NewValue < e.OldValue ? ChangeMaxTaskCount(true)
                        : ChangeMaxTaskCount(false);
                tbMaxTasks.Text = _maxTasks.ToString();
                if (_maxTasksHdlr != null)
                    _maxTasksHdlr(_maxTasks);
            }
        }

        private int ChangeMaxTaskCount(bool increase)
        {
            int maxTasks = Convert.ToInt16(tbMaxTasks.Text);
            if (!increase && maxTasks <= 1)
                return 1;
            if (increase && maxTasks >= _maxTasks)
                return _maxTasks;
            return increase ? ++maxTasks : --maxTasks;
        }

        private void cmbTraceLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_traceLevelHdlr != null)
                _traceLevelHdlr(cmbTraceLevel.SelectedValue.ToString());
        }

        private void sbMaxTasks_TouchUp(object sender, TouchEventArgs e)
        {
            tbMaxTasks.Text = ChangeMaxTaskCount(true).ToString();
        }

        private void sbMaxTasks_KeyUp(object sender, KeyEventArgs e)
        {
            tbMaxTasks.Text = ChangeMaxTaskCount(true).ToString();
        }

        private void sbMaxTasks_KeyDown(object sender, KeyEventArgs e)
        {
            tbMaxTasks.Text = ChangeMaxTaskCount(false).ToString();
        }

        private void sbMaxTasks_TouchDown(object sender, TouchEventArgs e)
        {
            tbMaxTasks.Text = ChangeMaxTaskCount(false).ToString();
        }

        private void sbMaxTasks_StylusButtonDown(object sender, StylusButtonEventArgs e)
        {
            tbMaxTasks.Text = ChangeMaxTaskCount(false).ToString();
        }

        private void sbMaxTasks_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            tbMaxTasks.Text = ChangeMaxTaskCount(true).ToString();
        }
    }
}
