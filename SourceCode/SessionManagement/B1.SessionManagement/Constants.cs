using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.SessionManagement
{
    public class Constants
    {
#pragma warning disable 1591 // disable the xmlComments warning
        /// <summary>
        /// Database table used to hold the application session control settings
        /// </summary>
        internal const string SignonControl = "SignonControl";
        /// <summary>
        /// Database table used to hold the access control group definition records
        /// </summary>
        internal const string AccessControlGroups = "AccessControlGroups";
        /// <summary>
        /// Database table used to hold the access control group rules (permission) records
        /// </summary>
        internal const string AccessControlGroupRules = "AccessControlGroupRules";
        /// <summary>
        /// Database table used to hold the application session records
        /// </summary>
        internal const string AppSessions = "AppSessions";
        /// <summary>
        /// Database table used to hold the application master records
        /// </summary>
        internal const string AppMaster = "AppMaster";
        /// <summary>
        /// Database table used to hold the user master records
        /// </summary>
        public const string UserMaster = "UserMaster";
        /// <summary>
        /// Database table used to hold the user session records
        /// </summary>
        internal const string UserSessions = "UserSessions";

        public const string AccessControlGroupCode = "AccessControlGroupCode";
        public const string AccessControlGroupName = "AccessControlGroupName";
        public const string AccessDenied = "AccessDenied";
        public const string AllowMultipleSessions = "AllowMultipleSessions";
        public const string AppId = "AppId";
        public const string AppCode = "AppCode";
        public const string AppMachine = "AppMachine";
        public const string AppName = "AppProduct";
        public const string AppVersion = "AppVersion";
        public const string ControlCode = "ControlCode";
        public const string ConfigSettings = "ConfigSettings";
        public const string DefaultAccessDenied = "DefaultAccessDenied";
        public const string DefaultAccessGroupCode = "DefaultAccessGroupCode";
        public const string EnvironmentSettings = "EnvironmentSettings";
        public const string FailedAttemptLimit = "FailedAttemptLimit";
        public const string ForcePasswordChange = "ForcePasswordChange";
        public const string FailedSignonAttempts = "FailedSignonAttempts";
        public const string FirstName = "FirstName";
        public const string ForceSignOff = "ForceSignOff";
        public const string LastModifiedDateTime = "LastModifiedDateTime";
        public const string LastModifiedUserCode = "LastModifiedUserCode";
        public const string LastName = "LastName";
        public const string LastSignonDateTime = "LastSignonDateTime";
        public const string MachineName = "MachineName";
        public const string MiddleName = "MiddleName";
        public const string MultipleSessionCode = "MultipleSessionCode";
        public const string MultipleSignonAllowed = "MultipleSignonAllowed";
        public const string NamePrefix = "NamePrefix";
        public const string NameSuffix = "NameSuffix";
        public const string PasswordSalt = "PasswordSalt";
        public const string ProcessId = "ProcessId";
        public const string Remarks = "Remarks";
        public const string RemoteAddress = "RemoteAddress";
        public const string RestrictSignon = "RestrictSignon";
        public const string RestrictSignonMsg = "RestrictSignonMsg";
        public const string SessionCode = "SessionCode";
        public const string SessionDateTime = "SessionDateTime";
        public const string SignonDateTime = "SignonDateTime";
        public const string SignonRestricted = "SignonRestricted";
        public const string SignoffWarningMsg = "SignoffWarningMsg";
        public const string StartDateTime = "StartDateTime";
        public const string StatusDateTime = "StatusDateTime";
        public const string StatusSeconds = "StatusSeconds";
        public const string StatusMessage = "StatusMessage";
        public const string TpeEndpointAddress = "TpeEndpointAddress";
        public const string TimeoutSeconds = "TimeoutSeconds";
        public const string UIControlCode = "UIControlCode";
        public const string UserId = "UserId";
        public const string UserPassword = "UserPassword";
        public const string UserCode = "UserCode";
#pragma warning restore 1591 // enable xmlComments warning
    }
}
