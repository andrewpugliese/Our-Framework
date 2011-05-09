using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using B1.Core;
using B1.DataAccess;
using B1.CacheManagement;

namespace B1.SessionManagement
{
    /// <summary>
    /// Meta data about a user's signon session
    /// </summary>
    internal struct UserSignonSessionStructure
    {
        /// <summary>
        /// The unique numeric code for the session
        /// </summary>
        public Int64 SessionCode;
        /// <summary>
        /// The unique numeric code for the user
        /// </summary>
        public Int32 UserCode;
        /// <summary>
        /// The unique numeric code for the user's default access group
        /// </summary>
        public Int32 DefaultAccessGroupCode;
        /// <summary>
        /// The unique string identifier for the user
        /// </summary>
        public string UserId;
        /// <summary>
        /// The hashed password for the user
        /// </summary>
        public string PasswordHash;
        /// <summary>
        /// Meta data about the user's environment when they signed on
        /// </summary>
        public UserEnvironmentStructure SignonApp;
    }

    /// <summary>
    /// Meta data about an access control group
    /// </summary>
    internal struct AccessControlGroupStructure
    {
        /// <summary>
        /// Unique numeric code for group
        /// </summary>
        internal Int32 AccessGroupCode;
        /// <summary>
        /// Unique name for group
        /// </summary>
        internal string AccessGroupName;
        /// <summary>
        /// Indicates whether access is denied as a default for this group
        /// </summary>
        internal bool DefaultAccessDenied;
        /// <summary>
        /// Cache of access control rules for the current group
        /// </summary>
        internal CacheMgr<bool> AccessControlRules;
    }

    /// <summary>
    /// Class that represents a user's session.
    /// </summary>
    public class UserSession
    {
        DataAccessMgr _daMgr = null;
        UserSignonSessionStructure _userSignon;
        static CacheMgr<AccessControlGroupStructure> _accessControlCache = new CacheMgr<AccessControlGroupStructure>();
        AccessControlGroupStructure _accessControlData;

        /// <summary>
        /// Constructor to create a UserSession instance for the given user signon meta data
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="userSignon">Meta data about the user's sucessful signon; including session code</param>
        internal UserSession(DataAccessMgr daMgr, UserSignonSessionStructure userSignon)
        {
            _daMgr = daMgr;
            _userSignon.DefaultAccessGroupCode = userSignon.DefaultAccessGroupCode;
            _userSignon.SessionCode = userSignon.SessionCode;
            _userSignon.SignonApp = userSignon.SignonApp;
            _userSignon.UserCode = userSignon.UserCode;
            _userSignon.UserId = userSignon.UserId;
            _userSignon.PasswordHash = userSignon.PasswordHash;
            _accessControlData = LoadAccessControl();
        }

        /// <summary>
        /// Loads the access control rules for the given user's access control group
        /// </summary>
        /// <returns>Access control rules for the given user's access control group</returns>
        AccessControlGroupStructure LoadAccessControl()
        {
            if (!_accessControlCache.Exists(_userSignon.DefaultAccessGroupCode.ToString()))
            {
                AccessControlGroupStructure accessControlGroup = new AccessControlGroupStructure();
                accessControlGroup.AccessGroupCode = _userSignon.DefaultAccessGroupCode;
                CacheMgr<bool> accessControlRules = new CacheMgr<bool>();
                DbCommand dbCmd = _daMgr.DbCmdCacheGetOrAdd(Constants.AccessControlGroupRules, 
                        BuildCmdGetAccessControlRulesList);
                dbCmd.Parameters[_daMgr.BuildParamName(Constants.AccessControlGroupCode)].Value 
                        = _userSignon.DefaultAccessGroupCode;
                DataTable accessRules = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
                foreach (DataRow accessRule in accessRules.Rows)
                {
                    accessControlGroup.DefaultAccessDenied = Convert.ToBoolean(
                            accessRule[Constants.DefaultAccessDenied]);
                    accessControlGroup.AccessGroupName = accessRule[Constants.AccessControlGroupName].ToString();
                    if (accessRule[Constants.UIControlCode] != DBNull.Value)
                    {
                        Int32 uiControlCode = Convert.ToInt32(accessRule[Constants.UIControlCode]);
                        bool accessDenied = Convert.ToBoolean(accessRule[Constants.AccessDenied]);
                        accessControlRules.Set(uiControlCode.ToString(), accessDenied);
                    }
                    accessControlGroup.AccessControlRules = accessControlRules;
                }
                _accessControlCache.Set(_userSignon.DefaultAccessGroupCode.ToString(), accessControlGroup);
            }
            return _accessControlCache.Get(_userSignon.DefaultAccessGroupCode.ToString());
        }


        static DbCommand BuildCmdGetAccessControlRulesList(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.AccessControlGroups);
            string joinTable = dmlSelectMgr.AddJoin(Constants.AccessControlGroupRules
                    , DataAccess.Constants.SCHEMA_CORE
                    , DbTableJoinType.Inner
                    , j => j.Column(Constants.AccessControlGroupCode)
                        == dmlSelectMgr.Column(Constants.AccessControlGroupCode)
                    , Constants.UIControlCode
                    , Constants.AccessDenied);
            dmlSelectMgr.SetWhereCondition(j => j.Column(Constants.AccessControlGroupCode) ==
                        j.Parameter(Constants.AccessControlGroups, Constants.AccessControlGroupCode
                        , daMgr.BuildParamName(Constants.AccessControlGroupCode)));
            return daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
        }

        /// <summary>
        /// Returns unique session code for current user's session
        /// </summary>
        public Int64 SessionCode { get { return _userSignon.SessionCode; } }
        /// <summary>
        /// Returns unique numeric identifier for current user
        /// </summary>
        public Int32 UserCode { get { return _userSignon.UserCode; } }
        /// <summary>
        /// Returns unique string identifier for current user
        /// </summary>
        public string UserId { get { return _userSignon.UserId; } }
        /// <summary>
        /// Returns the meta data about the user's environment when they signed on
        /// </summary>
        public UserEnvironmentStructure SignonApp { get { return _userSignon.SignonApp; } }
        /// <summary>
        /// Returns the default access control group's numeric code for the given user
        /// </summary>
        public Int32 DefaultAccessControlGroupCode { get { return _accessControlData.AccessGroupCode; } }
        /// <summary>
        /// Returns the default access control group's name for the given user
        /// </summary>
        public string DefaultAccessControlGroupName { get { return _accessControlData.AccessGroupName; } }

        /// <summary>
        /// Returns whether or not the current user has access to the user interface control identified
        /// by the given code.
        /// </summary>
        /// <param name="uiControlCode">A unique numeric code for a User Interface control (object)
        /// (e.g. button, menu item, etc)</param>
        /// <returns>True of false of whether access is allowed</returns>
        public bool IsAccessAllowed(Int32 uiControlCode)
        {
            return _accessControlData.AccessControlRules.Exists(uiControlCode.ToString()) 
                ? !_accessControlData.AccessControlRules.Get(uiControlCode.ToString())
                : !_accessControlData.DefaultAccessDenied;
        }

        /// <summary>
        /// Determines whether or not the given password hash is the correct value for the current user
        /// </summary>
        /// <param name="passwordHash">A hashed password for the current user</param>
        /// <returns>True when the same; False when they are different</returns>
        public bool VerifyPassword(string passwordHash)
        {
            return passwordHash == _userSignon.PasswordHash;
        }

        public static DbCommand GetUpdateUserSessionDbCommand(DataAccessMgr daMgr)
        {
            return daMgr.DbCmdCacheGetOrAdd("UpdateUserSessionDbCommand",
                        BuildCmdGetUpdateUserSession);
        }

        static DbCommand BuildCmdGetUpdateUserSession(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlUpdateMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.UserSessions);
            dmlUpdateMgr.AddColumn(Constants.SessionDateTime, EnumDateTimeLocale.UTC);
            dmlUpdateMgr.SetWhereCondition(j => j.Column(Constants.SessionCode) ==
                        j.Parameter(Constants.UserSessions, Constants.SessionCode
                        , daMgr.BuildParamName(Constants.SessionCode)));
            return daMgr.BuildUpdateDbCommand(dmlUpdateMgr);
        }

        /// <summary>
        /// Changes the given user's password to the new password and resets any account restrictions 
        /// depending on given parameter.
        /// <para>The method assumes the caller has verified the spelling of the new password.</para>
        /// <para>The method also assumes the caller has verified the existing password if applicable.</para>
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="userId">Unique user identifier</param>
        /// <param name="newPassword">The new password (unhashed) plain text</param>
        /// <param name="resetSignonRestrictions">Indicates whether the accounts restrictions will be reset</param>
        public static void ChangePassword(DataAccessMgr daMgr
            , string userId
            , string newPassword
            , bool resetSignonRestrictions)
        {
            string salt = Cryptography.HashOperation.CreateRandomSalt(Cryptography.HashAlgorithmTypeEnum.SHA512HashAlgorithm);
            string hash = Cryptography.HashOperation.ComputeHash(Cryptography.HashAlgorithmTypeEnum.SHA512HashAlgorithm
                , newPassword
                , salt);
            DbTableDmlMgr dmlUpdate = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , Constants.UserMaster);

            dmlUpdate.AddColumn(Constants.UserPassword
                , daMgr.BuildParamName(Constants.UserPassword));
            dmlUpdate.AddColumn(Constants.PasswordSalt
                , daMgr.BuildParamName(Constants.PasswordSalt));
            dmlUpdate.AddColumn(Constants.FailedSignonAttempts
                , daMgr.BuildParamName(Constants.FailedSignonAttempts));
            dmlUpdate.AddColumn(Constants.ForcePasswordChange
                , daMgr.BuildParamName(Constants.ForcePasswordChange));

            if (resetSignonRestrictions)
                dmlUpdate.AddColumn(Constants.SignonRestricted
                    , daMgr.BuildParamName(Constants.SignonRestricted));

            dmlUpdate.SetWhereCondition((j) => j.Column(Constants.UserId)
                == j.Parameter(dmlUpdate.MainTable.SchemaName
                    , dmlUpdate.MainTable.TableName
                    , Constants.UserId
                    , daMgr.BuildParamName(Constants.UserId)));

            DbCommand cmdUpdate = daMgr.BuildUpdateDbCommand(dmlUpdate);

            cmdUpdate.Parameters[daMgr.BuildParamName(Constants.UserId)].Value
                    = userId;
            cmdUpdate.Parameters[daMgr.BuildParamName(Constants.UserPassword)].Value
                    = hash;
            cmdUpdate.Parameters[daMgr.BuildParamName(Constants.PasswordSalt)].Value
                    = salt;
            if (resetSignonRestrictions)
                cmdUpdate.Parameters[daMgr.BuildParamName(Constants.SignonRestricted)].Value
                    = 0;
            cmdUpdate.Parameters[daMgr.BuildParamName(Constants.FailedSignonAttempts)].Value
                    = 0;
            cmdUpdate.Parameters[daMgr.BuildParamName(Constants.ForcePasswordChange)].Value
                    = 0;
            daMgr.ExecuteNonQuery(cmdUpdate, null, null);
        }
    }
}
