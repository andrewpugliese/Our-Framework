using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Data.Common;

using B1.WebSite.Admin.Models;
using B1.DataAccess;
using B1.SessionManagement;
using B1.DataManagement;
using B1.Core;

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
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        string[] urlParts = returnUrl.Split(new string[] { Constants.UIControlCodeTag }, StringSplitOptions.None);
                        int controlCode = urlParts.Length > 1 ? Convert.ToInt32(urlParts[1]) : 0;
                        if (!results.UserSessionMgr.IsAccessAllowed(controlCode) || true)
                        {
                            string msg = string.Format("Sorry, you are not authorized to access this page: {0}."
                                    , urlParts[0]);
                            System.Web.Routing.RouteValueDictionary dictionary = new System.Web.Routing.RouteValueDictionary();
                            dictionary.Add(Constants.Message, msg);
                            dictionary.Add(Constants.UrlReferrer, model.GoBackUri);
                            return RedirectToAction(Constants.AccessDenied, Constants.Home, dictionary);
                        }
                    }
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction(Constants.Index, Constants.Home);
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
            return RedirectToAction(Constants.Index, Constants.Home);
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
                    return RedirectToAction(Constants.Index, Constants.Home);
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
        // URL: /Account/EditProfile
        // **************************************
 
        [Authorize]
        public ActionResult EditProfile()
        {
            UserSession userSessionMgr = (UserSession)Session[SessionManagement.Constants.UserSessionMgr];
            if (userSessionMgr == null) // we were not signed on yet
            {
                return RedirectToAction(Constants.SignOn, Constants.Account, Constants.Admin);
            }
            if (userSessionMgr.IsAccessAllowed(Constants.UIControlCode_AdminEditProfile_Code))
            {
                DataAccessMgr daMgr = Global.GetDataAccessMgr(this.HttpContext);
                DbCommand dbCmd = UserMaster.GetUserMasterCmd(daMgr);
                dbCmd.Parameters[daMgr.BuildParamName(DataManagement.Constants.UserId)].Value = this.HttpContext.User.Identity.Name;
                EditProfileModel profileData = daMgr.ExecuteCollection<EditProfileModel>(dbCmd, null).First();
                return View(Constants._Page_EditProfile, profileData);
            }
            ViewBag.Status = "Insufficient privileges for the action." + Request.Url;
            string referUrl = Request.QueryString[Constants.UrlReferrer];
            System.Web.Routing.RouteValueDictionary dictionary = new System.Web.Routing.RouteValueDictionary();
            dictionary.Add(Constants.UrlReferrer, referUrl);
            return RedirectToAction(Constants.AccessDenied, Constants.Home, dictionary);
        }

        [HttpPost]
        public ActionResult EditProfile(EditProfileModel editProfile)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(editProfile.ChangedFields))
                {
                    DataAccessMgr daMgr = Global.GetDataAccessMgr(this.HttpContext);

                    DbTableDmlMgr dmlUpdate = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                            , DataManagement.Constants.Table_UserMaster);
                    foreach (string column in dmlUpdate.MainTable.Columns.Keys)
                        if (editProfile.ChangedFields.Contains(column))
                            dmlUpdate.AddColumn(column);

                    dmlUpdate.AddColumn(SessionManagement.Constants.LastModifiedUserCode);
                    dmlUpdate.AddColumn(SessionManagement.Constants.LastModifiedDateTime
                        , Core.EnumDateTimeLocale.Default);

                    dmlUpdate.SetWhereCondition(j => j.Column(DataManagement.Constants.UserId)
                        == j.Parameter(dmlUpdate.MainTable.SchemaName
                            , dmlUpdate.MainTable.TableName
                            , DataManagement.Constants.UserId
                            , daMgr.BuildParamName(DataManagement.Constants.UserId)));

                    DbCommand cmdUpdate = daMgr.BuildUpdateDbCommand(dmlUpdate);
                    UserSession userSessionMgr = (UserSession)Session[SessionManagement.Constants.UserSessionMgr];
                    cmdUpdate.Parameters[daMgr.BuildParamName(DataManagement.Constants.UserId)].Value = userSessionMgr.UserId;
                    cmdUpdate.Parameters[daMgr.BuildParamName(SessionManagement.Constants.LastModifiedUserCode)].Value = userSessionMgr.UserCode;

                    foreach (DbParameter param in cmdUpdate.Parameters)
                        if (param.Value == DBNull.Value)
                            param.Value = GetValueFromModelState(ModelState, param.ParameterName.Substring(1));
                    daMgr.ExecuteNonQuery(cmdUpdate, null, null);
                }
                else ViewBag.NoDataChanged = true;
            }
            return View(editProfile);
        }

        private object GetValueFromModelState(ModelStateDictionary modelStateDictionary, string column)
        {
            if (modelStateDictionary.ContainsKey(column))
                return modelStateDictionary[column].Value.AttemptedValue;
            else return DBNull.Value;
        }

        private object GetValueFromModel(EditProfileModel editProfile, string column)
        {
            string columnLower = column.ToLower();
            if (columnLower == DataManagement.Constants.Email.ToLower())
                return editProfile.EmailAddress;
            throw new ArgumentOutOfRangeException(column, "The given column was not found in the EditProfile data model");
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
                    return RedirectToAction(Constants._Page_ChangePasswordSuccess);
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
