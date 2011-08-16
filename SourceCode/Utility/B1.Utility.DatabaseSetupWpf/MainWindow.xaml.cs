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
using System.Configuration;

using B1.DataAccess;
using B1.Data.Models;

namespace B1.Utility.DatabaseSetupWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TestSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            DataAccessMgr daMgr = new DataAccessMgr(ConfigurationManager.AppSettings["ConnectionKey"]);
            B1SampleEntities entities = new B1SampleEntities();
            var query = from a in entities.TestSequences
                        orderby new { a.AppSequenceName, a.AppSequenceId }
                        select new { a.AppSequenceId, a.AppSequenceName, a.DbSequenceId };
            MainGridControl.PagingMgr = new PagingMgr(daMgr, query, DataAccess.Constants.PageSize, 10);
            MainGridControl.SetPage(PagingMgr.PagingDbCmdEnum.First);
        }
    }
}
