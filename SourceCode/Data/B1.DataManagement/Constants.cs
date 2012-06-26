using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.DataManagement
{
    public class Constants
    {
        internal const string Schema_UserMaster = DataAccess.Constants.SCHEMA_CORE;
        public const string Table_UserMaster = "UserMaster";
        internal const string CacheKey_GetUserMasterCmd = "_cacheKeyGetUserMasterCmd_";
        public const string Email = "Email";
        public const string UserCode = "UserCode";
        public const string UserId = "UserId";
    }
}
