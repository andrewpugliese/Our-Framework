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
        IProcessControl _clientControl = null;
        object _clientContext = null;

        public ProcessControlBar()
        {
            InitializeComponent();
            InitializeButtonState();
        }

        public void SetContext(IProcessControl clientControl
                , object clientContext = null
                , string btnStartContent = null
                , string btnStopContent = null
                , string btnPauseContent = null
                , string btnResumeContent = null
                , string btnStatusContent = null)
        {
            _clientControl = clientControl;
            _clientContext = clientContext;
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
            _clientControl.Start(_clientContext);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            InitializeButtonState();
            _clientControl.Stop(_clientContext);
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            btnResume.IsEnabled = true;
            btnPause.IsEnabled = !btnResume.IsEnabled;
            _clientControl.Pause(_clientContext);
        }

        private void btnResume_Click(object sender, RoutedEventArgs e)
        {
            btnResume.IsEnabled = false;
            btnPause.IsEnabled = !btnResume.IsEnabled;
            _clientControl.Resume(_clientContext);
        }

        private void btnStatus_Click(object sender, RoutedEventArgs e)
        {
            _clientControl.Status(_clientContext);
        }

    }
}
