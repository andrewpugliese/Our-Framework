using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using B1.DataAccess;

namespace B1.TaskProcessingEngine
{
    public class TaskProcessingQueue
    {
        /// <summary>
        /// Enumeration for returning the Task Processing Queue Records
        /// </summary>
#pragma warning disable 1591 // disable the xmlComments warning
        public enum TaskProcessingQueueListEnum { All, NotQueued, Queued, InProcess, Failed, Succeeded };
#pragma warning restore 1591 // restore the xmlComments warning

        /// <summary>
        /// Returns the entire data table of application session records in the 
        /// database based on the given enumeration.
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="tpqList">Enumeration indicating what type of session records to return</param>
        /// <returns>DataTable of Task Processing Queue records</returns>
        public static DataTable TaskProcessingQueueList(DataAccessMgr daMgr, TaskProcessingQueueListEnum tpqList)
        {
            DbCommand dbCmd;
            switch (tpqList)
            {
                case TaskProcessingQueueListEnum.Queued:
                    dbCmd = daMgr.DbCmdCacheGetOrAdd("BuildCmdGetTPQListByWaitDateTime", BuildCmdGetTPQListByWaitDateTime);
                    dbCmd.Parameters[daMgr.BuildParamName(Constants.StatusCode)].Value = Convert.ToByte(tpqList);
                    break;
                case TaskProcessingQueueListEnum.Failed:
                case TaskProcessingQueueListEnum.Succeeded:
                case TaskProcessingQueueListEnum.InProcess:
                case TaskProcessingQueueListEnum.NotQueued:
                    dbCmd = daMgr.DbCmdCacheGetOrAdd("BuildCmdGetTPQListByStatusDateTime", BuildCmdGetTPQListByStatusDateTime);
                    dbCmd.Parameters[daMgr.BuildParamName(Constants.StatusCode)].Value = Convert.ToByte(tpqList);
                    break;
                default:
                    dbCmd = daMgr.DbCmdCacheGetOrAdd("BuildCmdGetTPQListByTaskId", BuildCmdGetTPQListByTaskId);
                    break;
            }

            return daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
        }

        static DbCommand BuildCmdGetTPQListByTaskId(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.TaskProcessingQueue);
            string joinTable = dmlSelectMgr.AddJoin(DataAccess.Constants.SCHEMA_CORE
                    , Constants.TaskStatusCodes
                    , DbTableJoinType.Inner
                    , j => j.AliasedColumn(Constants.StatusCode)
                        == j.JoinAliasedColumn(Constants.StatusCode)
                    , Constants.StatusName);
            dmlSelectMgr.AddOrderByColumnAscending(Constants.TaskId);
            return daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
        }

        static DbCommand BuildCmdGetTPQListByWaitDateTime(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.TaskProcessingQueue);
            string joinTable = dmlSelectMgr.AddJoin(DataAccess.Constants.SCHEMA_CORE
                    , Constants.TaskStatusCodes
                    , DbTableJoinType.Inner
                    , j => j.AliasedColumn(Constants.StatusCode)
                        == j.JoinAliasedColumn(Constants.StatusCode)
                    , Constants.StatusName);
            dmlSelectMgr.SetWhereCondition(w => w.AliasedColumn(Constants.StatusCode)
                    == w.Parameter(Constants.StatusCode));
            dmlSelectMgr.AddOrderByColumnAscending(Constants.StatusCode);
            dmlSelectMgr.AddOrderByColumnAscending(Constants.WaitForDateTime);
            dmlSelectMgr.AddOrderByColumnAscending(Constants.TaskQueueCode);
            return daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
        }

        static DbCommand BuildCmdGetTPQListByStatusDateTime(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.TaskProcessingQueue);
            string joinTable = dmlSelectMgr.AddJoin(DataAccess.Constants.SCHEMA_CORE
                    , Constants.TaskStatusCodes
                    , DbTableJoinType.Inner
                    , j => j.AliasedColumn(Constants.StatusCode)
                        == j.JoinAliasedColumn(Constants.StatusCode)
                    , Constants.StatusName);
            dmlSelectMgr.SetWhereCondition(w => w.AliasedColumn(Constants.StatusCode)
                    == w.Parameter(Constants.StatusCode));
            dmlSelectMgr.AddOrderByColumnAscending(Constants.StatusCode);
            dmlSelectMgr.AddOrderByColumnDescending(Constants.StatusDateTime);
            dmlSelectMgr.AddOrderByColumnAscending(Constants.TaskQueueCode);
            return daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
        }

    }
}
