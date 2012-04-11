using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;

using B1.Cryptography;
using B1.DataAccess;
using B1.SessionManagement;

namespace B1.WebSite.Admin
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        static int _userSessionCount = 0;

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
            Application[DataAccess.Constants.DataAccessMgr] 
                    = new DataAccessMgr(ConfigurationManager.AppSettings[DataAccess.Constants.ConnectionKey]);

            // Create an App session
            StartAppSession();
        }

        protected void Abandon()
        {
        }

        protected void Application_End()
        {
            AppSession appSession = (AppSession)Application[SessionManagement.Constants.AppSession];
            appSession.End();
        }

        protected void Session_OnStart()
        {
            System.Threading.Interlocked.Increment(ref _userSessionCount);
        }

        protected void Session_OnEnd()
        {
            System.Threading.Interlocked.Decrement(ref _userSessionCount);
            if (Session[SessionManagement.Constants.UserSessionMgr] != null)
            {
                UserSession userSessionMgr = (UserSession)Session[SessionManagement.Constants.UserSessionMgr];
                DataAccessMgr daMgr = (DataAccessMgr)Application[DataAccess.Constants.DataAccessMgr];
                UserSignon.Signoff(daMgr, userSessionMgr.SessionCode);
                Session.Remove(SessionManagement.Constants.UserSessionMgr);
             }
        }

        private void StartAppSession()
        {
            // Create an App session for this web site
            AppSession appSession = new AppSession(Global.GetDataAccessMgr(this.Context)
                , ConfigurationManager.AppSettings[SessionManagement.Constants.ApplicationKey]
                , System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Assembly.GetName().Version.ToString()
                , System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Assembly.GetName().Name
                , null);

            // true indicates that reset any session conflict if exist
            appSession.Start("", "Application Startup", false);

            // Save to the Application cache
            Application[SessionManagement.Constants.AppSession] = appSession;
        }

    }

    public static class Global
    {
        public static DataAccessMgr GetDataAccessMgr(HttpContextBase context)
        {
            return (DataAccessMgr)context.Application[DataAccess.Constants.DataAccessMgr];
        }

        public static DataAccessMgr GetDataAccessMgr(HttpContext context)
        {
            return (DataAccessMgr)context.Application[DataAccess.Constants.DataAccessMgr];
        }

        public static AppSession GetAppSession(HttpContextBase context)
        {
            return (AppSession)context.Application[SessionManagement.Constants.AppSession];
        }

        public static AppSession GetAppSession(HttpContext context)
        {
            return (AppSession)context.Application[SessionManagement.Constants.AppSession];
        }

        public static int? GetUserCode(HttpContextBase context)
        {
            return Int32.Parse(GetCookieValueString(context, Constants.CookieContent_UserCode));
        }

        public static int? GetAccessControlGroup(HttpContextBase context)
        {
            return Int32.Parse(GetCookieValueString(context, Constants.CookieContent_AccessControlGroup));
        }

        /// <summary>
        /// Sets user code in cookie. If no cookie exists, a new one will be created.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="UserCode"></param>
        public static void SetUserCode(HttpContextBase context, int userCode)
        {
            SetCookieValueString(context, Constants.CookieContent_UserCode, userCode.ToString());
        }

        /// <summary>
        /// Sets user code and access group in cookie. If no cookie exists, a new one will be created.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="UserCode"></param>
        public static void SetUserCodeAndAccessControl(HttpContextBase context, int userCode, int accessGroupCode)
        {
            SetUserCode(context, userCode);

            SetCookieValueString(context, Constants.CookieContent_AccessControlGroup, accessGroupCode.ToString());
        }


        /// <summary>
        /// Gets the value of the key within the CG cookie. The value in the cookie is expected to be encrypted
        /// </summary>
        /// <param name="context"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static string GetCookieValueString(HttpContextBase context, string keyName)
        {
            if (context.Request.Cookies[Constants.CookieName] == null)
                return null;

            string userCode = context.Request.Cookies[Constants.CookieName][keyName];

            if (userCode != null)
            {
                string[] IVAndCipherText = userCode.Split(',');

                return DecryptData(new SymmetricCipherResults { CipherText = IVAndCipherText[1], IV = IVAndCipherText[0] });
            }

            return null;
        }

        /// <summary>
        /// Sets the value of the key within the CG cookie. The value is encrypted before being set.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="keyName"></param>
        /// <param name="value"></param>
        public static void SetCookieValueString(HttpContextBase context, string keyName, string value)
        {
            HttpCookie cookie = null;

            //See if the response has a cookie.
            if (!context.Response.Cookies.AllKeys.Contains(Constants.CookieName))
            {
                //If the request has a cookie, lets copy it to the response
                if (context.Request.Cookies.AllKeys.Contains(Constants.CookieName))
                {
                    cookie = context.Request.Cookies[Constants.CookieName];
                    context.Response.AppendCookie(cookie);
                }
                else
                    cookie = AddCookie(context);
            }
            else
                cookie = context.Response.Cookies[Constants.CookieName];

            SymmetricCipherResults cipherResults = EncryptData(value);

            cookie[keyName] = string.Format("{0},{1}", cipherResults.IV,
                    cipherResults.CipherText);
        }

        /// <summary>
        /// Adds a temporary cookie to the respose of this context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The newly created cookie</returns>
        public static HttpCookie AddCookie(HttpContextBase context)
        {
            HttpCookie newCookie = new HttpCookie(Constants.CookieName);
            context.Response.AppendCookie(newCookie);

            return newCookie;
        }

        public static SymmetricCipherResults EncryptData(string plainText)
        {
            return SymmetricOperation.EncryptData(plainText, Constants.Cookie_SymmetricAlgorithm,
                    Constants.Cookie_SymmetricKey);
        }

        public static string DecryptData(SymmetricCipherResults cipherResults)
        {
            return SymmetricOperation.DecryptData(cipherResults, Constants.Cookie_SymmetricAlgorithm,
                    Constants.Cookie_SymmetricKey);
        }
    }
}