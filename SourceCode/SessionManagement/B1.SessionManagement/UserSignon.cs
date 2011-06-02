using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using B1.Core;
using B1.DataAccess;
using B1.ILoggingManagement;
using B1.Cryptography;

namespace B1.SessionManagement
{
    /// <summary>
    /// Enumeration of the possible return conditions of a user signon operation
    /// <para>Success: Signon was successful</para>
    /// <para>InvaldCredentials: UserId or Password was incorrect</para>
    /// <para>MultipleSignonRestricted: UserId is still actively signed on and the account only allows single signon</para>
    /// <para>PasswordChangeRequired: UserId requires a password change immediately</para>
    /// <para>AccountLocked: UserId account is locked and should not be granted access</para>
    /// <para>SignonsRestricted: All accounts are denied signon</para>
    /// <para>ForcedSignoff: All accounts have been forced off</para>
    /// </summary>
#pragma warning disable 1591 // disable the xmlComments warning
    public enum SignonResultsEnum
    { 
        Success         
        , InvaldCredentials
        , MultipleSignonRestricted
        , PasswordChangeRequired
        , AccountLocked
        , SignonsRestricted
        , ForcedSignoff
    }
#pragma warning restore 1591 // restore the xmlComments warning

    /// <summary>
    /// Meta data about the signon operation
    /// </summary>
    public struct SignonResultsStructure
    {
        /// <summary>
        /// The enumeration result of the operation
        /// </summary>
        public SignonResultsEnum ResultEnum;
        /// <summary>
        /// The message result of the operation
        /// </summary>
        public string ResultMessage;
        /// <summary>
        /// When signon is successful, a valid user session object
        /// </summary>
        public UserSession UserSessionMgr;
    }

    /// <summary>
    /// Meta data about user's environment that is of interest
    /// to gather during signon
    /// </summary>
    public struct UserEnvironmentStructure
    {
        /// <summary>
        /// Unique numeric application code
        /// </summary>
        public Int64 AppCode;
        /// <summary>
        /// Unique string application identifier
        /// </summary>
        public string AppId;
        /// <summary>
        /// Application version string
        /// </summary>
        public string AppVersion;
        /// <summary>
        /// IP address of user's browser
        /// </summary>
        public string RemoteAddress;
    }

    /// <summary>
    /// Static class providing methods for management User Sessions
    /// </summary>
    public static class UserSignon
    {
        /// <summary>
        /// Enumeration for returning the list of user sessions (All, ActiveOnly, InactiveOnly)
        /// </summary>
#pragma warning disable 1591 // disable the xmlComments warning
        public enum UserSessionListEnum { AllUserSessions, ActiveUserSessions, InActiveUserSessions };
#pragma warning restore 1591 // restore the xmlComments warning

        /// <summary>
        /// Returns the entire data table of user session records in the 
        /// database based on the given enumeration.
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="userSessionList">Enumeration indicating what type of session records to return</param>
        /// <returns>DataTable of application session records</returns>
        public static DataTable UserSessions(DataAccessMgr daMgr, UserSessionListEnum userSessionList)
        {
            DbCommand dbCmd;
            if (userSessionList == UserSessionListEnum.AllUserSessions)
                dbCmd = daMgr.DbCmdCacheGetOrAdd(userSessionList.ToString(), BuildCmdGetAllUserSessionsList);
            else if (userSessionList == UserSessionListEnum.ActiveUserSessions)
                dbCmd = daMgr.DbCmdCacheGetOrAdd(userSessionList.ToString(), BuildCmdGetActiveUserSessionsList);
            else dbCmd = daMgr.DbCmdCacheGetOrAdd(userSessionList.ToString(), BuildCmdGetInActiveUserSessionsList);

            return daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
        }

        /// <summary>
        /// Removes all inactive session records from the database
        /// </summary>
        /// <param name="daMgr">DataAcessMgr object</param>
        public static void CleanupInactiveSessions(DataAccessMgr daMgr)
        {
            DbCommand dbCmd = daMgr.DbCmdCacheGetOrAdd("CleanupInactiveUserSessions", BuildCmdDelInActiveUserSessionsList);
            daMgr.ExecuteNonQuery(dbCmd, null, null);
        }

        static DbCommand BuildCmdGetAllUserSessionsList(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.UserSessions);
            return daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
        }

        static DbCommand BuildCmdGetActiveUserSessionsList(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.UserSessions);
            string joinTable = dmlSelectMgr.AddJoin(DataAccess.Constants.SCHEMA_CORE
                    , Constants.SignonControl
                    , DbTableJoinType.Cross, null);
            string dateAdd = daMgr.FormatDateMathSql(EnumDateDiffInterval.Second
                    , dmlSelectMgr.AliasedColumn(joinTable, Constants.StatusSeconds), 
                dmlSelectMgr.Column(Constants.SessionDateTime));
            dmlSelectMgr.SetWhereCondition(j => j.Function(dateAdd) >= 
                    j.Function(daMgr.GetDbTimeAs(EnumDateTimeLocale.UTC, null)));
            return daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
        }

        static DbCommand BuildCmdGetInActiveUserSessionsList(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.UserSessions);
            string joinTable = dmlSelectMgr.AddJoin(DataAccess.Constants.SCHEMA_CORE
                    , Constants.SignonControl    
                    , DbTableJoinType.Cross, null);
            string dateAdd = daMgr.FormatDateMathSql(EnumDateDiffInterval.Second
                    , dmlSelectMgr.AliasedColumn(joinTable, Constants.StatusSeconds),
                    dmlSelectMgr.Column(Constants.SessionDateTime));
            dmlSelectMgr.SetWhereCondition(j => j.Function(dateAdd) <
                j.Function(daMgr.GetDbTimeAs(EnumDateTimeLocale.UTC, null)));
            return daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
        }

        static DbCommand BuildCmdDelInActiveUserSessionsList(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlDeleteMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.UserSessions);
            string joinTable = dmlDeleteMgr.AddJoin(DataAccess.Constants.SCHEMA_CORE
                    , Constants.SignonControl, DbTableJoinType.Cross, null);
            string dateAdd = daMgr.FormatDateMathSql(EnumDateDiffInterval.Second
                    , dmlDeleteMgr.AliasedColumn(joinTable, Constants.StatusSeconds),
                dmlDeleteMgr.Column(Constants.SessionDateTime));
            dmlDeleteMgr.SetWhereCondition(j => j.Function(dateAdd) 
                    < j.Function(daMgr.GetDbTimeAs(EnumDateTimeLocale.UTC, null)));
            return daMgr.BuildDeleteDbCommand(dmlDeleteMgr);
        }

        /// <summary>
        /// Implements the user signon operation
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="signonControl">SignonControl data structure</param>
        /// <param name="userId">Unique user identifier</param>
        /// <param name="userPassword">User's password (NULL for first time initialization)</param>
        /// <param name="userEnv">Meta data about user's environment</param>
        /// <param name="allowMultipleSessions">Indicates whether to allow multiple session for an account that was not setup for multiple sessions</param>
        /// <returns>SignonResult data structure</returns>
        public static SignonResultsStructure Signon(DataAccessMgr daMgr
            , SignonControl signonControl
            , string userId
            , string userPassword
            , UserEnvironmentStructure userEnv
            , bool allowMultipleSessions = false)
        {
            SignonResultsStructure results = new SignonResultsStructure();
            results.ResultEnum = SignonResultsEnum.Success;
            results.ResultMessage = null;

            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , Constants.UserMaster
                    , Constants.UserCode
                    , Constants.UserPassword
                    , Constants.PasswordSalt
                    , Constants.SignonRestricted
                    , Constants.LastSignonDateTime
                    , Constants.FailedSignonAttempts
                    , Constants.ForcePasswordChange
                    , Constants.MultipleSignonAllowed
                    , Constants.DefaultAccessGroupCode);
            dmlSelectMgr.SetWhereCondition((j) => j.Column(Constants.UserId)
                == j.Parameter(dmlSelectMgr.MainTable.SchemaName
                    , dmlSelectMgr.MainTable.TableName
                    , Constants.UserId
                    , daMgr.BuildParamName(Constants.UserId)));
            DbCommand cmdSelectUserMaster = daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
            cmdSelectUserMaster.Parameters[daMgr.BuildParamName(Constants.UserId)].Value = userId;

            DbCommandMgr dbCmdMgr = new DbCommandMgr(daMgr);
            dbCmdMgr.AddDbCommand(cmdSelectUserMaster);
            if (!allowMultipleSessions)
            {
                dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                         , Constants.UserSessions
                         , Constants.SessionCode
                         , Constants.SessionDateTime
                         , Constants.ForceSignOff);
                dmlSelectMgr.SetWhereCondition((j) => j.Column(Constants.UserId)
                    == j.Parameter(dmlSelectMgr.MainTable.SchemaName
                        , dmlSelectMgr.MainTable.TableName
                        , Constants.UserId
                        , daMgr.BuildParamName(Constants.UserId)));
                DbCommand cmdSelectSessions = daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
                cmdSelectSessions.Parameters[daMgr.BuildParamName(Constants.UserId)].Value = userId;
                dbCmdMgr.AddDbCommand(cmdSelectSessions);
            }

            List<string> tableNames = new List<string>();
            tableNames.Add(Constants.UserMaster);

            if (!allowMultipleSessions)
                tableNames.Add(Constants.UserSessions);

            DataSet userSigonData = dbCmdMgr.ExecuteDataSet(tableNames);

            DataTable userMaster = userSigonData.Tables[Constants.UserMaster];
            // see if the userId exists and that the password is correct
            if (userMaster.Rows.Count == 0)
            {
                // userId does not exists, return an invalid credentials message
                results.ResultEnum = SignonResultsEnum.InvaldCredentials;
                results.ResultMessage = "Incorrect UserId or Password, please try again.";
                return results;
            }

            string storedUserPassword = userMaster.Rows[0][Constants.UserPassword].ToString();
            string passwordSalt = userMaster.Rows[0][Constants.PasswordSalt].ToString();
            userPassword = Cryptography.HashOperation.ComputeHash(HashAlgorithmTypeEnum.SHA512HashAlgorithm, userPassword, passwordSalt);
            if (storedUserPassword != userPassword)
            {
                // invalid credentials;  do not indicate whether userId or password is incorrect
                results.ResultEnum = SignonResultsEnum.InvaldCredentials;
                results.ResultMessage = "Incorrect UserId or Password, please try again.";
                Int16 failedAttempts = IncreaseFailedAttemptCount(daMgr, userId);
                // check for failed limit and restrict account
                if (failedAttempts >= signonControl.SignonControlData.FailedAttemptLimit)
                    RestrictSignon(daMgr, userId);
                return results;
            }

            // Since the UserId and Password matched, we found the account,
            // now check for account level restrictions
            bool signonRestricted = Convert.ToBoolean(userMaster.Rows[0][Constants.SignonRestricted]);
            if (signonRestricted)
            {
                // invalid credentials;  do not indicate whether userId or password is incorrect
                results.ResultEnum = SignonResultsEnum.SignonsRestricted;
                results.ResultMessage = "The account is restrcited from signing on.";
                return results;
            }

            bool ForcePasswordChange = Convert.ToBoolean(userMaster.Rows[0][Constants.ForcePasswordChange]);
            if (ForcePasswordChange)
            {
                // invalid credentials;  do not indicate whether userId or password is incorrect
                results.ResultEnum = SignonResultsEnum.PasswordChangeRequired;
                results.ResultMessage = "The account requires a password change before proceeding.";
                return results;
            }

            if (!allowMultipleSessions)
            {
                bool MultipleSignonAllowed = Convert.ToBoolean(userMaster.Rows[0][Constants.MultipleSignonAllowed]);
                DataTable userSessions = userSigonData.Tables[Constants.UserSessions];
                Int16 sessionCount = 0;
                foreach (DataRow userSession in userSessions.Rows)
                {
                    DateTime sessionDateTime = Convert.ToDateTime(userSession[Constants.SessionDateTime]);
                    TimeSpan sessionInterval = daMgr.DbSynchTime - sessionDateTime;
                    if (sessionInterval.TotalSeconds < signonControl.SignonControlData.TimeOutSeconds)
                    {
                        if (!MultipleSignonAllowed)
                        {
                            // if the user cannot have multiple signons, then we must check
                            // for existing (Active) session
                            results.ResultEnum = SignonResultsEnum.MultipleSignonRestricted;
                            results.ResultMessage = "The account can only have a single signon session.  They must signOff the other session first.";
                            return results;
                        }
                        ++sessionCount;
                    }
                }
            }

            // if the userId and password are correct, check signon control (general restrictions)
            if (signonControl.SignonControlData.RestrictSignon)
            {
                results.ResultEnum = SignonResultsEnum.SignonsRestricted;
                results.ResultMessage = signonControl.SignonControlData.RestrictSignonMsg;
                return results;
            }

            if (signonControl.SignonControlData.ForceSignoff)
            {
                results.ResultEnum = SignonResultsEnum.ForcedSignoff;
                results.ResultMessage = signonControl.SignonControlData.SignoffWarningMsg;
                return results;
            }

            // successful signon
            UserSignonSessionStructure uss = new UserSignonSessionStructure();
            uss.UserCode = Convert.ToInt32(userMaster.Rows[0][Constants.UserCode]);
            uss.PasswordHash = userMaster.Rows[0][Constants.UserPassword].ToString();
            uss.DefaultAccessGroupCode = Convert.ToInt32(userMaster.Rows[0][Constants.DefaultAccessGroupCode]);
            uss.SessionCode = AddSession(daMgr, userId, uss.UserCode, userEnv);
            UserSession sessionMgr = new UserSession(daMgr, uss);
            results.ResultMessage = "Welcome.";
            if (userMaster.Rows[0][Constants.LastSignonDateTime] != DBNull.Value)
            {
                DateTime signonDateTime = Convert.ToDateTime(userMaster.Rows[0][Constants.LastSignonDateTime]);
                results.ResultMessage += "  Your last signon was: ." + signonDateTime.ToString();
            }
            results.ResultEnum = SignonResultsEnum.Success;
            results.UserSessionMgr = sessionMgr;
            return results; 
        }

        /// <summary>
        /// Adds a record to the session database table
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="userId">Unique user identifier</param>
        /// <param name="userCode">Unique numeric user identifier</param>
        /// <param name="userEnv">MetaData about the user's environment</param>
        /// <returns>A unique session code</returns>
        static Int64 AddSession(DataAccessMgr daMgr, string userId, Int32 userCode, UserEnvironmentStructure userEnv)
        {
            Int64 sessionCode = daMgr.GetNextSequenceNumber(Constants.SessionCode);
            DbTableDmlMgr dmlInsert = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , Constants.UserSessions);
            dmlInsert.AddColumn(Constants.SessionCode, daMgr.BuildParamName(Constants.SessionCode));
            dmlInsert.AddColumn(Constants.UserCode, daMgr.BuildParamName(Constants.UserCode));
            dmlInsert.AddColumn(Constants.UserId, daMgr.BuildParamName(Constants.UserId));
            dmlInsert.AddColumn(Constants.AppCode, daMgr.BuildParamName(Constants.AppCode));
            dmlInsert.AddColumn(Constants.AppId, daMgr.BuildParamName(Constants.AppId));
            dmlInsert.AddColumn(Constants.AppMachine, daMgr.BuildParamName(Constants.AppMachine));
            dmlInsert.AddColumn(Constants.AppVersion, daMgr.BuildParamName(Constants.AppVersion));
            dmlInsert.AddColumn(Constants.RemoteAddress, daMgr.BuildParamName(Constants.RemoteAddress));

            DbCommand cmdInsert = daMgr.BuildInsertDbCommand(dmlInsert);
            cmdInsert.Parameters[daMgr.BuildParamName(Constants.SessionCode)].Value = sessionCode;
            cmdInsert.Parameters[daMgr.BuildParamName(Constants.UserId)].Value = userId;
            cmdInsert.Parameters[daMgr.BuildParamName(Constants.UserCode)].Value = userCode;
            cmdInsert.Parameters[daMgr.BuildParamName(Constants.AppId)].Value = userEnv.AppId;
            cmdInsert.Parameters[daMgr.BuildParamName(Constants.AppCode)].Value = userEnv.AppCode;
            cmdInsert.Parameters[daMgr.BuildParamName(Constants.AppVersion)].Value = userEnv.AppVersion;
            cmdInsert.Parameters[daMgr.BuildParamName(Constants.RemoteAddress)].Value = userEnv.RemoteAddress;
            cmdInsert.Parameters[daMgr.BuildParamName(Constants.AppMachine)].Value = Environment.MachineName;

            DbTableDmlMgr dmlUpdate = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , Constants.UserMaster);

            dmlUpdate.AddColumn(Constants.FailedSignonAttempts
                , daMgr.BuildParamName(Constants.FailedSignonAttempts));
            dmlUpdate.AddColumn(Constants.LastSignonDateTime
                , EnumDateTimeLocale.Default);

            dmlUpdate.SetWhereCondition((j) => j.Column(Constants.UserId)
                == j.Parameter(dmlUpdate.MainTable.SchemaName
                    , dmlUpdate.MainTable.TableName
                    , Constants.UserId
                    , daMgr.BuildParamName(Constants.UserId)));

            DbCommand cmdUpdate = daMgr.BuildUpdateDbCommand(dmlUpdate);

            cmdUpdate.Parameters[daMgr.BuildParamName(Constants.UserId)].Value
                    = userId;
            cmdUpdate.Parameters[daMgr.BuildParamName(Constants.FailedSignonAttempts)].Value
                    = 0;
            DbCommandMgr cmdMgr = new DbCommandMgr(daMgr);
            cmdMgr.AddDbCommand(cmdInsert);
            cmdMgr.AddDbCommand(cmdUpdate);
            cmdMgr.ExecuteNonQuery();
            return sessionCode;
        }

        /// <summary>
        /// Resets account restriction for the given user identifier
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="userId">Unique user identifier</param>
        public static void ReleaseRestriction(DataAccessMgr daMgr, string userId)
        {
            Restriction(daMgr, userId, true);
        }

        /// <summary>
        /// Sets an account restriction for the given user identifier
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="userId">Unique user identifier</param>
        public static void RestrictSignon(DataAccessMgr daMgr, string userId)
        {
            Restriction(daMgr, userId, false);
        }


        /// <summary>
        /// Sets or resets an account restriction for the given user identifier
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="userId">Unique user identifier</param>
        /// <param name="removeRestriction">Indicates whether to set or reset the restriction</param>
        static void Restriction(DataAccessMgr daMgr, string userId, bool removeRestriction)
        {
            DbTableDmlMgr dmlUpdate = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , Constants.UserMaster);

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
            cmdUpdate.Parameters[daMgr.BuildParamName(Constants.SignonRestricted)].Value
                = removeRestriction ? 0 : 1;
            daMgr.ExecuteNonQuery(cmdUpdate, null, null);
        }

        /// <summary>
        /// Increases the signon failure account for the given user identifier and returns the latest count
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="userId">Unique user identifier</param>
        /// <returns>The icremented count of failed attempts</returns>
        static Int16 IncreaseFailedAttemptCount(DataAccessMgr daMgr, string userId)
        {
            DbTableDmlMgr dmlUpdate = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , Constants.UserMaster);

            DbFunctionStructure addOne = new DbFunctionStructure();
            addOne.FunctionBody = string.Format("{0} + 1", Constants.FailedSignonAttempts); 
            dmlUpdate.AddColumn(Constants.FailedSignonAttempts
                , addOne);

            dmlUpdate.SetWhereCondition((j) => j.Column(Constants.UserId)
                == j.Parameter(dmlUpdate.MainTable.SchemaName
                    , dmlUpdate.MainTable.TableName
                    , Constants.UserId
                    , daMgr.BuildParamName(Constants.UserId)));

            DbCommand cmdUpdate = daMgr.BuildUpdateDbCommand(dmlUpdate);

            cmdUpdate.Parameters[daMgr.BuildParamName(Constants.UserId)].Value
                    = userId;

            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , Constants.UserMaster
                    , Constants.FailedSignonAttempts);
            dmlSelectMgr.SetWhereCondition((j) => j.Column(Constants.UserId)
                == j.Parameter(dmlSelectMgr.MainTable.SchemaName
                    , dmlSelectMgr.MainTable.TableName
                    , Constants.UserId
                    , daMgr.BuildParamName(Constants.UserId)));
            DbCommand cmdSelectUserMaster = daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
            cmdSelectUserMaster.Parameters[daMgr.BuildParamName(Constants.UserId)].Value = userId;
            DbCommandMgr cmdMgr = new DbCommandMgr(daMgr);
            cmdMgr.AddDbCommand(cmdUpdate);
            cmdMgr.AddDbCommand(cmdSelectUserMaster);
            DataTable userMaster = cmdMgr.ExecuteDataTable();
            return Convert.ToInt16(userMaster.Rows[0][Constants.FailedSignonAttempts]);
        }

        /// <summary>
        /// Changes the given user's password to the new password and resets any account restrictions.
        /// <para>The method assumes the caller has verified the spelling of the new password.</para>
        /// <para>The method also assumes the caller has verified the existing password if applicable.</para>
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="userId">Unique user identifier</param>
        /// <param name="newPassword">The new password (unhashed) plain text</param>
        public static void ChangePassword(DataAccessMgr daMgr, string userId, string newPassword)
        {
            UserSession.ChangePassword(daMgr, userId, newPassword, true);
        }

        /// <summary>
        /// Removes the session record for the given session code.
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="sessionCode">Unique session code</param>
        public static void Signoff(DataAccessMgr daMgr, Int64 sessionCode)
        {
            DbTableDmlMgr dmlDeleteMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , Constants.UserSessions);
            dmlDeleteMgr.SetWhereCondition((j) => j.Column(Constants.SessionCode)
                == j.Parameter(dmlDeleteMgr.MainTable.SchemaName
                    , dmlDeleteMgr.MainTable.TableName
                    , Constants.SessionCode
                    , daMgr.BuildParamName(Constants.SessionCode)));
            DbCommand cmdDeleteUserSession = daMgr.BuildDeleteDbCommand(dmlDeleteMgr);
            cmdDeleteUserSession.Parameters[daMgr.BuildParamName(Constants.SessionCode)].Value = sessionCode;
            daMgr.ExecuteNonQuery(cmdDeleteUserSession, null, null);
        }

    }
}
