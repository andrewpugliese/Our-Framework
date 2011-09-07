using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Dynamic;

using B1.DataAccess;

namespace B1.Web
{
    public static class PagingMgrHelper
    {
        // WebGrid needs enumerable of objects with properties - IEnumerable<DataRow> dont work
        public static IEnumerable<dynamic> DataTableToEnumerable(DataTable dataTable)
        {
            // Anonymous function which converts DataRow to the Expando Object
            Func<DataRow, dynamic> func = (row) =>
                {
                    // ExpandoObject: an object whose members can be dynamically added and removed at run time.
                    //?? Is ExpandoObject is using associative array like JavaScript object?
                    //?? Compiler is converting code for the dynamic object member access to the indexer lookup
                    //?? to the internally maintained dictionary?
                    var obj = (IDictionary<string, object>)new ExpandoObject();

                    // Add the column value to the expando object from the row
                    return row.Table.Columns.Cast<DataColumn>()
                        .Aggregate(obj, (o, col) => { o[col.ColumnName] = row[col.ColumnName]; return o; });
                };

            return dataTable.Rows.Cast<DataRow>().Select<DataRow, dynamic>(func);
        }

        public static string PagingMgrGrid(this HtmlHelper htmlHelper, PagingMgr pagingMgr)
        {
            // Get the pagingState and pagingDirection
            var route = htmlHelper.ViewContext.RouteData;
            var controllerName = route.GetRequiredString("controller");
            var actionName = route.GetRequiredString("action");
            string pagingState = htmlHelper.ViewContext.HttpContext.Request.Params["pagingState"];
            string pagingDirection = htmlHelper.ViewContext.HttpContext.Request.Params["pagingDirection"];
            PagingMgr.PagingDbCmdEnum pagingOption = string.IsNullOrWhiteSpace(pagingDirection) ?
                PagingMgr.PagingDbCmdEnum.First :
                (PagingMgr.PagingDbCmdEnum)Enum.Parse(typeof(PagingMgr.PagingDbCmdEnum), pagingDirection);

            if (!string.IsNullOrWhiteSpace(pagingState))
            {
                // Set the pagingState before getting the intended page
                pagingMgr.RestorePagingState(pagingState);

//                string initHtml = @"
//
//function showPage(action, data) {{
//    $.ajax({{
//        url: action,
//        type: ""get"",
//        format: ""html"",
//        cache: false,
//        data: data,
//        success: function (result) {{
//            // alert(result);
//            $(""#gridContainer"").html(result);
//        }},
//        error: function (xhr, status, error) {{
//
//            //?? CALL the client side error handling function which may be provided by the client
//
//            alert(xhr.responseText);
//            //?? showModalDialog(xhr.statusText, xhr.responseText);
//        }}
//    }});
//}}
//
//";
            }

            // Set the paging option and get the next page
            DataTable pageTable = pagingMgr.GetPage(pagingOption);
            IEnumerable<dynamic> pageData = DataTableToEnumerable(pageTable);

            // Realize the loop
            WebGrid webGrid = new WebGrid(pageData, rowsPerPage: pagingMgr.PageSize);

            // Get the new paging state for next and previous link
            string newPagingState = pagingMgr.GetPagingState();
            string html =
                string.Format("<a href=\"/OurAdminWeb/Grid/TestSequences?pagingState={0}&pagingDirection={1}\">Previous</a>", newPagingState, "prev")
                + " | "
                + string.Format("<a href=\"/OurAdminWeb/Grid/TestSequences?pagingState={0}&pagingDirection={1}\">Next</a><br />", newPagingState, "next");

            return html + webGrid.Table().ToHtmlString();
        }

        public static string ToHtmlString(this PagingMgr pagingMgr, Controller mvcController)
        {
            // Get the pagingState and pagingDirection
            var route = mvcController.RouteData;
            var controllerName = route.GetRequiredString("controller");
            var actionName = route.GetRequiredString("action");
            string pagingState = mvcController.Request.Params["pagingState"];
            string pagingDirection = mvcController.Request.Params["pagingDirection"] ?? "first";

            // Set the pagingState before getting the intended page
            pagingMgr.RestorePagingState(pagingState);

            // Set the paging option and get the next page
            PagingMgr.PagingDbCmdEnum pagingOption = pagingDirection == "first" ? PagingMgr.PagingDbCmdEnum.First
                : pagingDirection == "next" ? PagingMgr.PagingDbCmdEnum.Next
                : pagingDirection == "prev" ? PagingMgr.PagingDbCmdEnum.Previous
                : PagingMgr.PagingDbCmdEnum.Last;
            DataTable pageTable = pagingMgr.GetPage(pagingOption);
            IEnumerable<dynamic> pageData = DataTableToEnumerable(pageTable);

            // Realize the loop
            WebGrid webGrid = new WebGrid(pageData, rowsPerPage: pagingMgr.PageSize);

            // Get the new paging state for next and previous link
            string newPagingState = pagingMgr.GetPagingState();
            string html =
                string.Format("<a href=\"/OurAdminWeb/Grid/TestSequences?pagingState={0}&pagingDirection={1}\">Previous</a>", newPagingState, "prev")
                + " | "
                + string.Format("<a href=\"/OurAdminWeb/Grid/TestSequences?pagingState={0}&pagingDirection={1}\">Next</a><br />", newPagingState, "next");

            return html + webGrid.Table().ToHtmlString();
        }

        public static IHtmlString ToHtmlString<T>(this PagingMgr pagingMgr, PagingMgr.PagingDbCmdEnum pagingDirection)
            where T : new()
        {
            IEnumerable<dynamic> pageData = (IEnumerable<dynamic>)pagingMgr.GetPage<T>(pagingDirection);
            WebGrid webGrid = new WebGrid(pageData);
            return webGrid.Table();
        }


    }
}
