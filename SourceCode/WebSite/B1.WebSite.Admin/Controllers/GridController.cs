using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

using B1.DataAccess;

namespace B1.WebSite.Admin.Controllers
{
    public class GridController : Controller
    {
        public enum WebEntity
        {
            None = 0,
            User,
            Session
        };

        public ActionResult Index(string showEntity)
        {
            WebEntity entityType;
            if (!Enum.TryParse<WebEntity>(showEntity, out entityType))
            {
                // Unknown entity - return error view
            }

            return View();
        }

        public ActionResult User()
        {
            DataAccessMgr daMgr = new DataAccessMgr(ConfigurationManager.AppSettings["ConnectionKey"]);
            DbTableDmlMgr dml = daMgr.DbCatalogGetTableDmlMgr("B1.UserMaster");
            //?? daMgr.ExecuteDataSet(
            return View();
        }
    }
}
