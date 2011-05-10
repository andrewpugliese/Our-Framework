using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.Common;

using B1.DataAccess;
using B1.Data.Models;

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

        public ActionResult AppSessions()
        {
            B1SampleEntities entities = new B1SampleEntities();
            DataAccessMgr daMgr = Global.GetDataAccessMgr(this.HttpContext);

            var query = from a in entities.AppSessions
                        select new { a.AppCode, a.AppId };
            DbCommand dbCmd = daMgr.BuildSelectDbCommand(query, null);
            return PartialView("_AppSessions", daMgr.ExecuteCollection<AppSession>(dbCmd, null));
        }

        public ActionResult UserSessions()
        {
            B1SampleEntities entities = new B1SampleEntities();
            DataAccessMgr daMgr = Global.GetDataAccessMgr(this.HttpContext);

            var query = from a in entities.UserSessions
                        select new { a.AppCode, a.UserId };
            DbCommand dbCmd = daMgr.BuildSelectDbCommand(query, null);
            return PartialView("_UserSessions", daMgr.ExecuteCollection<UserSession>(dbCmd, null));
        }

        public ActionResult Users()
        {
            B1SampleEntities entities = new B1SampleEntities();
            DataAccessMgr daMgr = Global.GetDataAccessMgr(this.HttpContext);

            var query = from a in entities.UserMasters
                        select new { a.UserCode, a.UserId, a.FirstName };
            DbCommand dbCmd = daMgr.BuildSelectDbCommand(query, null);
            return PartialView("_Users", daMgr.ExecuteCollection<UserMaster>(dbCmd, null));
        }
    }
}
