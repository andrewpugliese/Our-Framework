using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.Common;

using B1.DataAccess;
using B1.Data.Models;

using B1.Web;

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
            if (entityType == WebEntity.User)
                @ViewBag.Status = "Accesd Denied.";
            return View();
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
            DataAccessMgr daMgr = Global.GetDataAccessMgr(this.HttpContext);
            B1SampleEntities entities = new B1SampleEntities();
            var query = from a in entities.UserMasters
                        select new { a.UserCode, a.UserId, a.FirstName };
            DbCommand dbCmd = daMgr.BuildSelectDbCommand(query, null);
            return PartialView("_Users", daMgr.ExecuteCollection<UserMaster>(dbCmd, null));
        }

        //?? public ActionResult AppSessions()
        public string AppSessions()
        {
            B1SampleEntities entities = new B1SampleEntities();
            DataAccessMgr daMgr = Global.GetDataAccessMgr(this.HttpContext);

            var query = from a in entities.AppSessions
                        orderby new { a.AppCode, a.MultipleSessionCode }
                        select new { a.AppCode, a.AppId, a.MultipleSessionCode };                
            PagingMgr testSequenceMgr = new PagingMgr(daMgr, query, DataAccess.Constants.PageSize, 20);
            return testSequenceMgr.ToHtmlString<AppSession>(PagingMgr.PagingDbCmdEnum.First).ToHtmlString();
            //DbCommand dbCmd = daMgr.BuildSelectDbCommand(query, null);
            //return PartialView("_AppSessions", daMgr.ExecuteCollection<AppSession>(dbCmd, null));
        }

        public string TestSequences()
        {
            DataAccessMgr daMgr = Global.GetDataAccessMgr(this.HttpContext);
            B1SampleEntities entities = new B1SampleEntities();
            var query = from a in entities.TestSequences
                        orderby new { a.AppSequenceName, a.AppSequenceId }
                        select new { a.AppSequenceId, a.AppSequenceName, a.DbSequenceId };
            PagingMgr testSequenceMgr = new PagingMgr(daMgr, query, DataAccess.Constants.PageSize, 20);
            return testSequenceMgr.ToHtmlString(this);
        }

        public ActionResult UserEditForm(int userCode)
        {
            B1SampleEntities entities = new B1SampleEntities();
            DataAccessMgr daMgr = Global.GetDataAccessMgr(this.HttpContext);

            var query = from a in entities.UserMasters
                        where a.UserCode == userCode
                        select new { a.UserCode, a.UserId, a.FirstName };
            DbCommand dbCmd = daMgr.BuildSelectDbCommand(query, null);
            return PartialView(Constants._Page_UserEdit, daMgr.ExecuteCollection<UserMaster>(dbCmd, null).First());
        }

        public ActionResult TestSequences2()
        {
            DataAccessMgr daMgr = Global.GetDataAccessMgr(this.HttpContext);
            B1SampleEntities entities = new B1SampleEntities();
            var query = from a in entities.TestSequences
                        orderby new { a.AppSequenceName, a.AppSequenceId }
                        select new { a.AppSequenceId, a.AppSequenceName, a.DbSequenceId };
            PagingMgr testSequenceMgr = new PagingMgr(daMgr, query, DataAccess.Constants.PageSize, 10);
            return PartialView(Constants._Page_PagingMgrView, testSequenceMgr);
        }

        public ActionResult Users2()
        {
            DataAccessMgr daMgr = Global.GetDataAccessMgr(this.HttpContext);
            B1SampleEntities entities = new B1SampleEntities();
            var query = from a in entities.UserMasters
                        select new { a.UserCode, a.UserId, a.FirstName, "Edit" };
            PagingMgr testSequenceMgr = new PagingMgr(daMgr, query, DataAccess.Constants.PageSize, 5);
            return PartialView(Constants._Page_PagingMgrView, testSequenceMgr);
        }

    }
}
