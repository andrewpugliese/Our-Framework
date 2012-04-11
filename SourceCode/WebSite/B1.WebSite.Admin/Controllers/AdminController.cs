using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using B1.SessionManagement;
using B1.WebSite.Admin;

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
                return RedirectToAction(Constants.Signon, Constants.Account, Constants.Admin);
            }
            if (userSessionMgr.IsAccessAllowed(5) && false)
            {
                ViewBag.Status = "Please select from administration menu items";
                return View();
            }
            ViewBag.Status = "Insufficient privileges for the action." + Request.Url;
            string referUrl = Request.QueryString["UrlReferrer"];
            Uri vbReferURL = ViewBag.UrlReferrer;
            Uri ReferURL = Request.UrlReferrer;
            System.Web.Routing.RouteValueDictionary dictionary = new System.Web.Routing.RouteValueDictionary();
            dictionary.Add("UrlReferrer", referUrl);
            return RedirectToAction("accessdenied","home", dictionary);
        }

        public ActionResult GoBack()
        {
            string referUrl = Request.QueryString["UrlReferrer"];
            if (!string.IsNullOrEmpty(referUrl))
                return Redirect(referUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
