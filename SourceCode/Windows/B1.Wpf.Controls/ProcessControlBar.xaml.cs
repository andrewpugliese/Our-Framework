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

namespace B1.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for ProcessControlBar.xaml
    /// </summary>
    public partial class ProcessControlBar : UserControl
    {
        IProcessControl _parentControl = null;
        object _parentContext = null;

        public ProcessControlBar()
        {
            InitializeComponent();
            InitializeButtonState();
        }

        public bool Enabled
        {
            get
            {
                return this.IsEnabled;
            }
            set
            {
                if (this.IsEnabled != value)
                    this.IsEnabled = value;
            }
        }

        public void DisplayPausedState()
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send
                                 , new Action<bool>(UpdateButtons),
                                 true);
        }

        public void DisplayResumedState()
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send
                                 , new Action<bool>(UpdateButtons),
                                 false);
        }

        void UpdateButtons(bool isPaused)
        {
            if (isPaused)
            {
                btnResume.IsEnabled = true;
                btnPause.IsEnabled = !btnResume.IsEnabled;
            }
            else
            {
                btnResume.IsEnabled = false;
                btnPause.IsEnabled = !btnResume.IsEnabled;
            }
        }

        public void SetContext(IProcessControl parentControl
                , object parentContext = null
                , string btnStartContent = null
                , string btnStopContent = null
                , string btnPauseContent = null
                , string btnResumeContent = null
                , string btnStatusContent = null)
        {
            _parentControl = parentControl;
            _parentContext = parentContext;
            if (!string.IsNullOrEmpty(btnStartContent))
                btnStart.Content = btnStartContent;
            if (!string.IsNullOrEmpty(btnStopContent))
                btnStop.Content = btnStopContent;
            if (!string.IsNullOrEmpty(btnPauseContent))
                btnPause.Content = btnPauseContent;
            if (!string.IsNullOrEmpty(btnResumeContent))
                btnResume.Content = btnResumeContent;
            if (!string.IsNullOrEmpty(btnStatusContent))
                btnStatus.Content = btnStatusContent;
        }

        void InitializeButtonState()
        {
            btnStart.IsEnabled = true;
            btnPause.IsEnabled = btnStop.IsEnabled = btnStatus.IsEnabled = btnResume.IsEnabled = !btnStart.IsEnabled;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = btnResume.IsEnabled = false;
            btnPause.IsEnabled = btnStop.IsEnabled = btnStatus.IsEnabled = !btnStart.IsEnabled;
            _parentControl.Start(_parentContext);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            InitializeButtonState();
            _parentControl.Stop(_parentContext);
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            DisplayPausedState();
            _parentControl.Pause(_parentContext);
        }

        private void btnResume_Click(object sender, RoutedEventArgs e)
        {
            DisplayResumedState();
            _parentControl.Resume(_parentContext);
        }

        private void btnStatus_Click(object sender, RoutedEventArgs e)
        {
            _parentControl.Status(_parentContext);
        }

    }
}
