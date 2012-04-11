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
    /// <para>Provides a WPF control that allows user to page through the result set of a PagingMgr object
    /// into a WPF grid object.</para>
    /// </summary>
    public partial class PagingTableControl : UserControl, IPagingControl
    {
        public delegate void SelectionChangedDelegate(PagingMgr source, DataRow currentRow);
        static string _ErrMsgInvalidPageSize = "Invalid Page Size. Enter a number between 1 and " + Int16.MaxValue.ToString();
        PagingMgr _pagingMgr = null;
        string _title = null;
        short _pageSize = 1;
        SelectionChangedDelegate _selectionChangedHandler = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public PagingTableControl()
        {
            InitializeComponent();
            lblPagingGridMsg.Content = null;
        }

        /// <summary>
        /// Get/Set the source PagingMgr for the control
        /// </summary>
        public PagingMgr Source
        {
            set
            {
                _pagingMgr = value;
                _pageSize = _pagingMgr.PageSize;
                tbPageSize.Text = _pageSize.ToString();
            }
            get { return _pagingMgr; }
        }

        /// <summary>
        /// Set the grid's title
        /// </summary>
        public string Title
        {
            set
            {
                _title = value;
                lblPagingGridTitle.Content = _title;
            }
        }

        /// <summary>
        /// Set the selection changed delegate handler
        /// </summary>
        public SelectionChangedDelegate SelectionChangedHandler
        {
            set
            {
                _selectionChangedHandler = value;
            }

        }

        /// <summary>
        /// Sets the grid to contain the first page of data
        /// </summary>
        /// <returns>Bool indicator of whether there was data loaded into grid</returns>
        public bool First()
        {
            return SetPage(_pagingMgr, PagingMgr.PagingDbCmdEnum.First, _pageSize);
        }

        /// <summary>
        /// Sets the grid to contain the last page of data
        /// </summary>
        /// <returns>Bool indicator of whether there was data loaded into grid</returns>
        public bool Last()
        {
            return SetPage(_pagingMgr, PagingMgr.PagingDbCmdEnum.Last, _pageSize);
        }

        /// <summary>
        /// Sets the grid to contain the previous page of data
        /// </summary>
        /// <returns>Bool indicator of whether there was data loaded into grid</returns>
        public bool Previous()
        {
            return SetPage(_pagingMgr, PagingMgr.PagingDbCmdEnum.Previous, _pageSize);
        }

        /// <summary>
        /// Sets the grid to contain the next page of data
        /// </summary>
        /// <returns>Bool indicator of whether there was data loaded into grid</returns>
        public bool Next()
        {
            return SetPage(_pagingMgr, PagingMgr.PagingDbCmdEnum.Next, _pageSize);
        }

        /// <summary>
        /// Sets the grid to contain the page of data of given direction for the given page size
        /// using the given pagingMgr.
        /// </summary>
        /// <param name="pagingMgr">PagingMgr object instance</param>
        /// <param name="pageDirection">Paging enumeration: First, Last, Next, Previou</param>
        /// <param name="pageSize">Number of records to be loaded in a page</param>
        /// <returns>Bool indicator of whether there was data loaded into grid</returns>
        private bool SetPage(PagingMgr pagingMgr, PagingMgr.PagingDbCmdEnum? pageDirection, short pageSize)
        {
            if (pagingMgr == null)
                throw new ILoggingManagement.ExceptionEvent(ILoggingManagement.enumExceptionEventCodes.NullOrEmptyParameter
                        , "pagingMgr cannot be null");

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

        /// <summary>
        /// Requeries the database and loads the grid with an updated page of data
        /// </summary>
        /// <returns>Bool indicator of whether there was data loaded into grid</returns>
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

        private void dataGridPaging_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.DataGrid dg = (System.Windows.Controls.DataGrid)sender;
            if (dg.HasItems && dg.SelectedIndex >= 0)
                if (_selectionChangedHandler != null)
                    _selectionChangedHandler(_pagingMgr, ((DataRowView)dg.SelectedItem).Row);
        }
    }
}
