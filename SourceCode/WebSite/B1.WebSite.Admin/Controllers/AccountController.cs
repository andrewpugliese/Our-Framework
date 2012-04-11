using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using B1.WebSite.Admin.Models;

using B1.DataAccess;
using B1.SessionManagement;

namespace B1.WebSite.Admin.Controllers
{
    public class AccountController : Controller
    {

        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        // **************************************
        // URL: /Account/SignOn
        // **************************************

        public ActionResult SignOn()
        {
//            Uri referrer = this.Request.UrlReferrer;
  //          ViewBag.UrlReferrer = referrer;
            return View();
        }

        [HttpPost]
        public ActionResult SignOn(SignOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                bool allowMultipleSessions = false;
                AppSession appSession = Global.GetAppSession(this.HttpContext);
                UserEnvironmentStructure ues = new UserEnvironmentStructure();
                ues.AppCode = appSession.AppCode;
                ues.AppId = appSession.AppId;
                ues.AppVersion = appSession.AppVersion;
                SignonResultsStructure results = UserSignon.Signon(Global.GetDataAccessMgr(this.HttpContext)
                        , appSession.SignonControl
                        , model.UserName
                        , model.Password
                        , ues
                        , allowMultipleSessions);

                if (results.ResultEnum == SignonResultsEnum.Success)
                {
                    FormsService.SignIn(model.UserName, model.RememberMe);
                    Session[SessionManagement.Constants.UserSessionMgr] = results.UserSessionMgr;
                    string[] urlParts = returnUrl.Split(new string[] { Constants.UIControlCodeTag }, StringSplitOptions.None);
                    int controlCode = urlParts.Length > 1 ? Convert.ToInt32(urlParts[1]) : 0;
                    if (!results.UserSessionMgr.IsAccessAllowed(controlCode) || true)
                    {
                        string msg = string.Format("Sorry, you are not authorized to access this page: {0}.  Please speak to your administrator."
                                , urlParts[0]);
                        System.Web.Routing.RouteValueDictionary dictionary = new System.Web.Routing.RouteValueDictionary();
                        dictionary.Add("Message", msg);
                        dictionary.Add("UrlReferrer", model.GoBackUri);
                        return RedirectToAction("accessdenied", "home", dictionary);
                    }

                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
#warning "Add other case conditions"
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/SignOff
        // **************************************

        public ActionResult SignOff()
        {
            if (Session[SessionManagement.Constants.UserSessionMgr] != null)
            {
                UserSession userSessionMgr = (UserSession)Session[SessionManagement.Constants.UserSessionMgr];
                DataAccessMgr daMgr = (DataAccessMgr)Global.GetDataAccessMgr(this.HttpContext);
                UserSignon.Signoff(daMgr, userSessionMgr.SessionCode);
                Session.Remove(SessionManagement.Constants.UserSessionMgr);
            }
            FormsService.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Account/SignUp
        // **************************************

        public ActionResult SignUp()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [HttpPost]
        public ActionResult SignUp(SignUpModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsService.SignIn(model.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

    }
}
