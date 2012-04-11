using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using B1.WebSite.Admin.Models;

namespace B1.WebSite.Admin.Controllers
{
    public class BaseController : Controller
    {
        //
        // GET: /Base/
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var siteMenuManager = new SiteMenuManager();
            ViewBag.SiteLinks = siteMenuManager.GetSitemMenuItems().ToList();
            base.OnActionExecuting(filterContext);
        }

    }
}
