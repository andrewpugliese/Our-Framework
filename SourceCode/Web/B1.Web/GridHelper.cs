using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Web;
using System.Web.Helpers;

using B1.DataAccess;

namespace B1.Web
{
    public static class PagingMgrHelper
    {
        //?? PagingMgrView
        public static IHtmlString View(this PagingMgr pagingMgr, PagingMgr.PagingDbCmdEnum pagingDirection)
        {
            DataTable pageTable = pagingMgr.GetPage(pagingDirection);

            IEnumerable<dynamic> data = new List<dynamic>()
            {
                new {a = "hello"},
                new {a = "hello2"}
            };

            WebGrid webGrid = new WebGrid(data, new List<string>() { "a" });//??pageTable.Rows.Cast<DataRow>());
            return webGrid.Table();
        }
    }
}
