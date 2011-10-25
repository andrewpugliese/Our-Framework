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
using System.Data;

using B1.DataAccess;

namespace B1.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for PagingTableControl.xaml
    /// </summary>
    public partial class PagingTableControl : UserControl, IPagingControl
    {
        PagingMgr _pagingMgr = null;
        string _title = null;
        short _pageSize = 1;
        static string _ErrMsgInvalidPageSize = "Invalid Page Size. Enter a number between 1 and " + Int16.MaxValue.ToString();

        public PagingTableControl()
        {
            InitializeComponent();
            lblPagingGridMsg.Content = null;
        }

        public PagingMgr Source
        {
            set
            {
                _pagingMgr = value;
                _pageSize = _pagingMgr.PageSize;
                tbPageSize.Text = _pageSize.ToString();
            }
        }

        public string Title
        {
            set
            {
                _title = value;
                lblPagingGridTitle.Content = _title;
            }
        }

        public DataRow CurrentRow
        {
            get
            {
                return dataGridPaging.Items != null && dataGridPaging.Items.Count > 0
                    ? (DataRow)dataGridPaging.Items[0] : null;
            }

        }

        public bool First()
        {
            return SetPage(_pagingMgr, PagingMgr.PagingDbCmdEnum.First, _pageSize);
        }

        public bool Last()
        {
            return SetPage(_pagingMgr, PagingMgr.PagingDbCmdEnum.Last, _pageSize);
        }

        public bool Previous()
        {
            return SetPage(_pagingMgr, PagingMgr.PagingDbCmdEnum.Previous, _pageSize);
        }

        public bool Next()
        {
            return SetPage(_pagingMgr, PagingMgr.PagingDbCmdEnum.Next, _pageSize);
        }

        private bool SetPage(PagingMgr pagingMgr, PagingMgr.PagingDbCmdEnum? pageDirection, short pageSize)
        {
#warning "Replace with Exception Event"
            if (pagingMgr == null)
                throw new ArgumentNullException();

            DataTable page = pageDirection.HasValue ? pagingMgr.GetPage(pageDirection.Value, pageSize)
                : pagingMgr.RefreshPage(pageSize);
            if (page != null && page.Rows.Count > 0)
            {
                dataGridPaging.ItemsSource = page.DefaultView;
                lblPagingGridMsg.Content = string.Empty;
                return true;
            }
            lblPagingGridMsg.Content = "No more data found.";
            return false;
        }

        public bool Refresh()
        {
            return SetPage(_pagingMgr, null, _pagingMgr.PageSize);
        }

        private void btnPageFirst_Click(object sender, RoutedEventArgs e)
        {
            First();
        }

        private void btnPageLast_Click(object sender, RoutedEventArgs e)
        {
            Last();
        }

        private void btnPagePrevious_Click(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void btnPageNext_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void btnPageRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void tbPageSize_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbPageSize.Text))
            {
                short pageSize = _pageSize;
                if (short.TryParse(tbPageSize.Text, out pageSize))
                    _pageSize = pageSize;
                else
                {
                    lblPagingGridMsg.Content = _ErrMsgInvalidPageSize;
                    tbPageSize.SelectAll();
                }
            }
        }
    }
}
