using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using B1.SessionManagement;
using B1.WebSite.Admin;
using B1.DataManagement;

namespace B1.WebSite.Admin.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        [Authorize]
        public ActionResult Index()
        {
            UserSession userSessionMgr = (UserSession)Session[SessionManagement.Constants.UserSessionMgr];
            if (userSessionMgr == null) // we were not signed on yet
            {
                return RedirectToAction(Constants.SignOn, Constants.Account, Constants.Admin);
            }
            if (userSessionMgr.IsAccessAllowed(Constants.UIControlCode_AdminUser_Code))
            {
                return RedirectToAction(Constants.Users, Constants.Admin);
            }
            ViewBag.Status = "Insufficient privileges for the action." + Request.Url;
            string referUrl = Request.QueryString[Constants.UrlReferrer];
            System.Web.Routing.RouteValueDictionary dictionary = new System.Web.Routing.RouteValueDictionary();
            dictionary.Add(Constants.UrlReferrer, referUrl);
            return RedirectToAction(Constants.AccessDenied, Constants.Home, dictionary);
        }

        [Authorize]
        public ActionResult Users()
        {
            UserSession userSessionMgr = (UserSession)Session[SessionManagement.Constants.UserSessionMgr];
            if (userSessionMgr == null) // we were not signed on yet
            {
                return RedirectToAction(Constants.SignOn, Constants.Account, Constants.Admin);
            }
            if (userSessionMgr.IsAccessAllowed(Constants.UIControlCode_AdminUser_Code))
            {
                ViewBag.Status = "Please browse the list of users or edit one.";
                return View();
            }
            ViewBag.Status = "Insufficient privileges for the action." + Request.Url;
            string referUrl = Request.QueryString[Constants.UrlReferrer];
            System.Web.Routing.RouteValueDictionary dictionary = new System.Web.Routing.RouteValueDictionary();
            dictionary.Add(Constants.UrlReferrer, referUrl);
            return RedirectToAction(Constants.AccessDenied, Constants.Home, dictionary);
        }

        public ActionResult GoBack()
        {
            string referUrl = Request.QueryString[Constants.UrlReferrer];
            if (!string.IsNullOrEmpty(referUrl))
                return Redirect(referUrl);
            return RedirectToAction(Constants.Index, Constants.Home);
        }
    }
}
