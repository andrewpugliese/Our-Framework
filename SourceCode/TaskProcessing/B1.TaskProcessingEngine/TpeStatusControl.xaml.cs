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
    /// This UserControl derived class is used to update the display objects of the Task Processing Engine (TPE)
    /// screens.  It is used in two contexts (Host TPE and Remote Host TPE).
    /// </summary>
    public partial class TpeStatusControl : UserControl
    {
        public delegate void TraceLevelChangeHandler(string newTraceLevel);
        public delegate void MaxTasksChangeHandler(int newMaxTasks);
        public enum TpeHostType { Local = 0, Remote = 1 };
        IProcessControl _parent = null;
        TraceLevelChangeHandler _traceLevelHdlr = null;
        MaxTasksChangeHandler _maxTasksHdlr = null;
        int _maxTasks = Environment.ProcessorCount;
        object _parentContext = null;
        NumericPlusMinus.ClickHandler _maxTasksClickHdlr = null;
        bool _displayOnlyTraceLevelChange = false;
        object _displayTraceLevelChangeMonitor = new object();

        /// <summary>
        /// Default constructor; Fills trace level combo box and disables all contained controls.
        /// </summary>
        public TpeStatusControl()
        {
            InitializeComponent();
            foreach (ILoggingManagement.enumTraceLevel tl in ILoggingManagement.Constants.TraceLevels())
                cmbTraceLevel.Items.Add(tl);
            cmbTraceLevel.SelectedIndex = 0;
            SetDisplayObjects(false);
        }

        /// <summary>
        /// Allows consumer of the control provide an interface to handle the button click events.
        /// <para>It also allows consumer to store a context object that will be returned on the click events.</para>
        /// </summary>
        /// <param name="parentControl">IProcessControl for parent</param>
        /// <param name="parentContext">Optional context to return with every event</param>
        /// <param name="maxTasksHdlr">Optional handler for the max tasks change event</param>
        /// <param name="btnStartContent">Optional caption for the start button</param>
        /// <param name="btnStopContent">Optional caption for the stop button</param>
        /// <param name="btnPauseContent">Optional caption for the pause button</param>
        /// <param name="btnResumeContent">Optional caption for the resume button</param>
        /// <param name="btnStatusContent">Optional caption for the status button</param>
        public void SetContext(IProcessControl parentControl
                , object parentContext = null
                , NumericPlusMinus.ClickHandler maxTasksHdlr = null
                , string btnStartContent = null
                , string btnStopContent = null
                , string btnPauseContent = null
                , string btnResumeContent = null
                , string btnStatusContent = null)
        {
            _parent = parentControl;
            _parentContext = parentContext;
            _maxTasksClickHdlr = maxTasksHdlr;
            // set the context on the tpe control a=hanldr
            tpeControl.SetContext(_parent
                    , _parentContext
                    , btnStartContent
                    , btnStopContent
                    , btnPauseContent
                    , btnResumeContent
                    , btnStatusContent);
        }

        /// <summary>
        /// toggles the enable attribute on the controls based upon given input
        /// </summary>
        /// <param name="enable"></param>
        void SetDisplayObjects(bool enable)
        {
            cmbTraceLevel.IsEnabled = tbConfigId.IsEnabled = tbConnKey.IsEnabled = tbEngineId.IsEnabled 
                = tbHostEndpointAddress.IsEnabled = tbLoggingKey.IsEnabled = npmMaxTasks.IsEnabled 
                = tbTaskAssemblyPath.IsEnabled = tbTPEStatus.IsEnabled = npmMaxTasks.IsEnabled = enable;
        }
        
        /// <summary>
        /// Resets all controls to initial state and disables them
        /// </summary>
        public void Clear()
        {
            _maxTasksHdlr = null;
            _traceLevelHdlr = null;
            tbConnKey.Text = null;
            tbLoggingKey.Text = null;
            tbConfigId.Text = null;
            tbEngineId.Text = null;
            tbTaskAssemblyPath.Text = null;
            tbHostEndpointAddress.Text = null;
            _maxTasks = 1;
            npmMaxTasks.Max = 1;
            npmMaxTasks.Min = 1;
            npmMaxTasks.Value = npmMaxTasks.Min;
            cmbTraceLevel.SelectedIndex = 0;
            SetDisplayObjects(false);
        }

        /// <summary>
        /// Sets the text of the controls to the values given
        /// </summary>
        /// <param name="traceLevelHdlr">Delegate handler for the change tracel level event</param>
        /// <param name="maxTasksHdlr">Delegate handler for the change max tasks event</param>
        /// <param name="connectionKey">Connection key string</param>
        /// <param name="loggingKey">Logging key string</param>
        /// <param name="configId">Configuration Identifier</param>
        /// <param name="engineId">Engine Identifier </param>
        /// <param name="taskAssemblyPath">Path to where task implementation assemblies can be found</param>
        /// <param name="hostEndpointAddress">URL where WCF host TPE can be found</param>
        /// <param name="maxTasks">Maximum number of tasks that can be run in parallel</param>
        /// <param name="traceLevel">Trace level setting to allow messages to be viewed</param>
        public void Display(TraceLevelChangeHandler traceLevelHdlr
            , MaxTasksChangeHandler maxTasksHdlr
            , string connectionKey
            , string loggingKey
            , string configId
            , string engineId
            , string taskAssemblyPath
            , string hostEndpointAddress
            , int maxTasks
            , ILoggingManagement.enumTraceLevel traceLevel)
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
            npmMaxTasks.Initialize("Max Tasks In Parallel", _parentContext, _maxTasksClickHdlr, 1, _maxTasks, 1, 1, 1);
            cmbTraceLevel.SelectedItem = traceLevel;
            SetDisplayObjects(true);
        }

        /// <summary>
        /// Enables/Disables the appropriate buttons to show a process is in a paused state.
        /// </summary>
        public void DisplayPausedState()
        {
            tpeControl.DisplayPausedState();
        }

        /// <summary>
        /// Enables/Disables the appropriate buttons to show a process is in resumed or running state.
        /// </summary>
        public void DisplayResumedState()
        {
            tpeControl.DisplayResumedState();
        }

        /// <summary>
        /// Enables/Disables the appropriate buttons to show a process is in a disconnected state.
        /// </summary>
        public void DisplayDisconnectedState()
        {
            tpeControl.DisplayDisconnectedState();
        }

        /// <summary>
        /// Dispatches a request to set the TraceLevel combo box to show the given trace level as selected
        /// </summary>
        /// <param name="traceLevel">Current trace level enumeration</param>
        public void DisplayTraceLevelState(ILoggingManagement.enumTraceLevel traceLevel)
        {
            lock (_displayTraceLevelChangeMonitor)
            {
                _displayOnlyTraceLevelChange = true;
            }
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send
                                    , new Action<ILoggingManagement.enumTraceLevel>(SetTraceLevel),
                                    traceLevel);
        }

        /// <summary>
        /// Dispatches a request to sets the max tasks control to show the given value.
        /// </summary>
        /// <param name="maxTasks">Current setting for maximum number of tasks.</param>
        public void DisplayMaxTasks(int maxTasks)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send
                                    , new Action<int>(SetMaxTasks),
                                    maxTasks);
        }

        /// <summary>
        /// Changes the max tasks control to show the given value.
        /// </summary>
        /// <param name="maxTasks">Current setting for maximum number of tasks.</param>
        void SetMaxTasks(int maxTasks)
        {
            npmMaxTasks.Value = maxTasks;
        }

        /// <summary>
        /// Changes the trace level combo box to display the given tracel level as selected item
        /// </summary>
        /// <param name="traceLevel">Current trace level enumeration</param>
        void SetTraceLevel(ILoggingManagement.enumTraceLevel traceLevel)
        {
            cmbTraceLevel.SelectedItem = traceLevel;
        }


        /// <summary>
        /// Returns the connection key value
        /// </summary>
        public string ConnectionKey
        {
            get { return tbConnKey.Text; }
        }

        /// <summary>
        /// Returns the logging key value
        /// </summary>
        public string LoggingKey
        {
            get { return tbLoggingKey.Text; }
        }

        /// <summary>
        /// Returns the engine id  value
        /// </summary>
        public string EngineId
        {
            get { return tbEngineId.Text; }
        }

        /// <summary>
        /// Returns the confirguration id value
        /// </summary>
        public string ConfigId
        {
            get { return tbConfigId.Text; }
        }

        /// <summary>
        /// Retuns the task assembly path value
        /// </summary>
        public string TaskAssemblyPath
        {
            get { return tbTaskAssemblyPath.Text; }
        }

        /// <summary>
        /// Returns the host endpoint address (WCF host TPE) value
        /// </summary>
        public string HostEndpointAddress
        {
            get { return tbHostEndpointAddress.Text; }
        }

        /// <summary>
        /// Returns current status value or dispatches request to set the status value
        /// </summary>
        public string Status
        {
            get { return tbTPEStatus.Text; }
            set
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send
                                   , new Action<string>(SetStatus),
                                   value);
            }
        }

        /// <summary>
        /// Sets the status control to the given status string
        /// </summary>
        /// <param name="status">Status string to display</param>
        void SetStatus(string status)
        {
            tbTPEStatus.Text = status;
        }

        /// <summary>
        /// Handles trace level combo box selection chenaged event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        private void cmbTraceLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_displayOnlyTraceLevelChange)
                lock (_displayTraceLevelChangeMonitor)
                {
                    _displayOnlyTraceLevelChange = false;
                }
            if (_traceLevelHdlr != null)
                _traceLevelHdlr(cmbTraceLevel.SelectedValue.ToString());
        }

    }
}
