using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;

using B1.DataAccess;
using B1.SessionManagement;

namespace B1.WebSite.Admin
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            // Create a DbAccessManager and store it. This class never contains a live db.
            Application["DataAccessMgr"] = new DataAccessMgr(ConfigurationManager.AppSettings["ConnectionKey"]);

            // Create an App session
            StartAppSession();
        }

        private void StartAppSession()
        {
            // Create an App session for this web site
            AppSession appSession = new AppSession(Global.GetDataAccessMgr(this.Context), "AdminWeb1",
                "1.0", "B1.WebSite.Admin", null);

            // true indicates that reset any session conflict if exist
            appSession.Start("", "Application Startup", false);

            // Save to the Application cache
            Application["AppSession"] = appSession;
        }

        //?? DO the appSession.End() at the end of the application
    }

    public static class Global
    {
        public static DataAccessMgr GetDataAccessMgr(HttpContextBase context)
        {
            return (DataAccessMgr)context.Application["DataAccessMgr"];
        }

        public static DataAccessMgr GetDataAccessMgr(HttpContext context)
        {
            return (DataAccessMgr)context.Application["DataAccessMgr"];
        }

        public static AppSession GetAppSession(HttpContextBase context)
        {
            return (AppSession)context.Application["AppSession"];
        }

        public static AppSession GetAppSession(HttpContext context)
        {
            return (AppSession)context.Application["AppSession"];
        }
    }
}