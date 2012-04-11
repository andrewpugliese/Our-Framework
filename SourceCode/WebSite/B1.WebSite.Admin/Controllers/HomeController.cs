using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace B1.WebSite.Admin.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            DateTime now = DateTime.Now;
            ViewBag.DateTime = string.Format("{0} {1}", now.ToLongDateString(), now.ToShortTimeString());
            string greeting = "Good Morning";
            if (now.Hour > 12 && now.Hour < 17)
                greeting = "Good Afternoon";
            else if (now.Hour > 17)
                greeting = "Good Evening";
            ViewBag.Message = greeting + ". Welcome to our home page!";

            return base.View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}
