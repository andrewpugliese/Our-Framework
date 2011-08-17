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

using B1.DataAccess;

namespace B1.Utility.DatabaseSetupWpf
{
    /// <summary>
    /// Interaction logic for PagingMgrGridControl.xaml
    /// </summary>
    public partial class PagingMgrGridControl : UserControl
    {
        public PagingMgrGridControl()
        {
            InitializeComponent();
        }

        public PagingMgr PagingMgr { get; set; }

        private void FirstButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(PagingMgr.PagingDbCmdEnum.First);
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(PagingMgr.PagingDbCmdEnum.Previous);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(PagingMgr.PagingDbCmdEnum.Next);
        }

        private void LastButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(PagingMgr.PagingDbCmdEnum.Last);
        }

        public void SetPage(PagingMgr.PagingDbCmdEnum pagingOption)
        {
            if (PagingMgr == null)
                throw new ApplicationException("Please initialize the PagingMgr property in the PagingMgrGridControl class");

            PagingGrid.ItemsSource = PagingMgr.GetPage(pagingOption).DefaultView;
        }

    }
}
