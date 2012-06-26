using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Common;

using B1.DataAccess;

namespace B1.DataManagement
{
    public class UserMaster
    {
        public static DbCommand GetUserMasterCmd(DataAccessMgr daMgr)
        {
            return daMgr.DbCmdCacheGetOrAdd(Constants.CacheKey_GetUserMasterCmd, BuildGetUserMasterCmd);
        }

        static DbCommand BuildGetUserMasterCmd(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlMgr = new DbTableDmlMgr(daMgr, Constants.Schema_UserMaster, Constants.Table_UserMaster);
            dmlMgr.SetWhereCondition(w => ((w.Parameter(Constants.UserId) == null )
                                        || w.Column(Constants.UserId) == w.Parameter(Constants.UserId))
                                    && ((w.Parameter(Constants.UserCode) == null )
                                        || w.Column(Constants.UserCode) == w.Parameter(Constants.UserCode)));
            DbCommand dbCmd = daMgr.BuildSelectDbCommand(dmlMgr, null);
            return dbCmd;
        }
    }
}
