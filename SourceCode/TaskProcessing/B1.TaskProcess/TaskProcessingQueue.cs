using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;

using B1.DataAccess;
using B1.ILoggingManagement;

namespace B1.TaskProcessing
{
    public class TaskProcessingQueue
    {
        /// <summary>
        /// Enumeration for returning the Task Processing Queue Records
        /// </summary>
#pragma warning disable 1591 // disable the xmlComments warning
        public enum ListEnum { All, NotQueued, Queued, InProcess, Failed, Succeeded };
        public enum StatusCodeEnum { NotQueued = 0, Queued = 32, InProcess = 64, Failed = 128, Succeeded = 255 };
#pragma warning restore 1591 // restore the xmlComments warning

        /// <summary>
        /// Returns the entire data table of application session records in the 
        /// database based on the given enumeration.
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="tpqList">Enumeration indicating what type of session records to return</param>
        /// <returns>DataTable of Task Processing Queue records</returns>
        public static DataTable TaskProcessingQueueList(DataAccessMgr daMgr, ListEnum tpqList)
        {
            DbCommand dbCmd;
            switch (tpqList)
            {
                case ListEnum.Queued:
                    dbCmd = daMgr.DbCmdCacheGetOrAdd("BuildCmdGetTPQListByWaitDateTime"
                            , BuildCmdGetTPQListByWaitDateTime);
                    dbCmd.Parameters[daMgr.BuildParamName(Constants.StatusCode)].Value = Convert.ToByte(tpqList);
                    break;
                case ListEnum.Failed:
                case ListEnum.Succeeded:
                case ListEnum.InProcess:
                case ListEnum.NotQueued:
                    dbCmd = daMgr.DbCmdCacheGetOrAdd("BuildCmdGetTPQListByStatusDateTime"
                            , BuildCmdGetTPQListByStatusDateTime);
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

        public static DataTable TaskDependenciesList(DataAccessMgr daMgr, Int32 taskQueueCode)
        {
            DbCommand dbCmd = daMgr.DbCmdCacheGetOrAdd("BuildCmdGetTaskDependencies"
                      , BuildCmdGetTaskDependencies);
            dbCmd.Parameters[daMgr.BuildParamName(Constants.TaskQueueCode)].Value = taskQueueCode;
            return daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
        }

        static DbCommand BuildCmdGetTaskDependencies(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.TaskDependencies);
            dmlSelectMgr.SetWhereCondition(w => w.Column(Constants.TaskQueueCode) == w.Parameter(Constants.TaskQueueCode));
            dmlSelectMgr.AddOrderByColumnAscending(Constants.TaskQueueCode);
            dmlSelectMgr.AddOrderByColumnAscending(Constants.WaitTaskId);
            return daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
        }

        public static DbCommand GetDeleteDependencyTaskCmd(DataAccessMgr daMgr, DataRow taskQueueItem)
        {
            if (taskQueueItem == null
                || !taskQueueItem.Table.Columns.Contains(TaskProcessing.Constants.TaskQueueCode))
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                        , "DataRow (taskQueueItem) containing TaskProcessingQueue data was empty");

            DbTableDmlMgr dmlMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , TaskProcessing.Constants.TaskDependencies);
            dmlMgr.SetWhereCondition(w => w.Column(TaskProcessing.Constants.TaskQueueCode)
                    == w.Parameter(TaskProcessing.Constants.TaskQueueCode));
            DbCommand dbCmd = daMgr.BuildDeleteDbCommand(dmlMgr);
            dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.TaskQueueCode)].Value
                    = Convert.ToInt32(taskQueueItem[TaskProcessing.Constants.TaskQueueCode]);
            return dbCmd;
        }

        public static DbCommand GetDeleteQueueItemCmd(DataAccessMgr daMgr, DataRow taskQueueItem)
        {
            if (taskQueueItem == null
                || !taskQueueItem.Table.Columns.Contains(TaskProcessing.Constants.TaskQueueCode))
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                        , "DataRow (taskQueueItem) containing TaskProcessingQueue data was empty");

            DbTableDmlMgr dmlMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , TaskProcessing.Constants.TaskProcessingQueue);
            dmlMgr.SetWhereCondition(w => w.Column(TaskProcessing.Constants.TaskQueueCode)
                    == w.Parameter(TaskProcessing.Constants.TaskQueueCode));
            DbCommand dbCmd = daMgr.BuildDeleteDbCommand(dmlMgr);
            dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.TaskQueueCode)].Value
                    = Convert.ToInt32(taskQueueItem[TaskProcessing.Constants.TaskQueueCode]);
            return dbCmd;
        }

        public static DbCommand GetDmlCmd(DataAccessMgr daMgr
                , DataRow taskQueueItem
                , Dictionary<string, object> editedColumns
                , Int32? userCode = null)
        {
            DbCommand dbCmd = null;
            DbTableDmlMgr dmlMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , TaskProcessing.Constants.TaskProcessingQueue);

            foreach (string column in editedColumns.Keys)
                dmlMgr.AddColumn(column);
            if (taskQueueItem == null) // add new item
            {
                dmlMgr.AddColumn(TaskProcessing.Constants.LastModifiedUserCode);
                dmlMgr.AddColumn(TaskProcessing.Constants.LastModifiedDateTime);
                dbCmd = daMgr.BuildInsertDbCommand(dmlMgr);
            }

            else dbCmd = daMgr.BuildChangeDbCommand(dmlMgr, TaskProcessing.Constants.LastModifiedUserCode
                    , TaskProcessing.Constants.LastModifiedDateTime);

            foreach (string column in editedColumns.Keys)
                dbCmd.Parameters[daMgr.BuildParamName(column)].Value
                        = editedColumns[column];

            if (taskQueueItem == null) // add new
            {
                if (userCode.HasValue)
                {
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode)].Value
                        = userCode.Value;
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime)].Value
                        = daMgr.DbSynchTime;
                }
                else
                {
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode)].Value
                        = DBNull.Value;
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime)].Value
                        = DBNull.Value;
                }
            }
            else  // change; where condition params
            {
                dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode)].Value
                    = taskQueueItem[TaskProcessing.Constants.LastModifiedUserCode];
                dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime)].Value
                    = taskQueueItem[TaskProcessing.Constants.LastModifiedDateTime];
                // set portion of the update
                if (userCode.HasValue)
                {
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode, true)].Value
                        = userCode.Value;
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime, true)].Value
                        = daMgr.DbSynchTime;
                }
                else
                {
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode, true)].Value
                        = DBNull.Value;
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime, true)].Value
                        = DBNull.Value;
                }
            }

            return dbCmd;
        }


        public static DbCommand GetDeleteQueueItemCmd(DataAccessMgr daMgr, DataRow taskQueueItem, bool deleteAll)
        {
            if (taskQueueItem == null
                || !taskQueueItem.Table.Columns.Contains(TaskProcessing.Constants.TaskQueueCode))
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                        , "DataRow (taskQueueItem) containing TaskDependency data was empty");

            DbTableDmlMgr dmlMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , TaskProcessing.Constants.TaskDependencies);
            dmlMgr.SetWhereCondition(w => w.Column(TaskProcessing.Constants.TaskQueueCode)
                    == w.Parameter(TaskProcessing.Constants.TaskQueueCode));
            if (!deleteAll)
            {
                System.Linq.Expressions.Expression waitTaskExp =
                    DbPredicate.CreatePredicatePart(w => w.Column(TaskProcessing.Constants.WaitTaskQueueCode)
                            == w.Parameter(TaskProcessing.Constants.WaitTaskQueueCode));
                dmlMgr.AddToWhereCondition(System.Linq.Expressions.ExpressionType.AndAlso, waitTaskExp);
            }
            DbCommand dbCmd = daMgr.BuildDeleteDbCommand(dmlMgr);
            dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.TaskQueueCode)].Value
                    = Convert.ToInt32(taskQueueItem[TaskProcessing.Constants.TaskQueueCode]);
            if (!deleteAll)
            {
                dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.WaitTaskQueueCode)].Value
                        = Convert.ToInt32(taskQueueItem[TaskProcessing.Constants.WaitTaskQueueCode]);
            }
            return dbCmd;
        }


        public static DbCommand GetDependencyDmlCmd(DataAccessMgr daMgr
                , DataRow taskQueueItem
                , Dictionary<string, object> editedColumns
                , Int32? userCode = null)
        {
            DbCommand dbCmd = null;
            DbTableDmlMgr dmlMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , TaskProcessing.Constants.TaskDependencies);

            foreach (string column in editedColumns.Keys)
                dmlMgr.AddColumn(column);
            if (taskQueueItem == null) // add new item
            {
                dmlMgr.AddColumn(TaskProcessing.Constants.LastModifiedUserCode);
                dmlMgr.AddColumn(TaskProcessing.Constants.LastModifiedDateTime);
                dbCmd = daMgr.BuildInsertDbCommand(dmlMgr);
            }

            else dbCmd = daMgr.BuildChangeDbCommand(dmlMgr, TaskProcessing.Constants.LastModifiedUserCode
                    , TaskProcessing.Constants.LastModifiedDateTime);

            foreach (string column in editedColumns.Keys)
                dbCmd.Parameters[daMgr.BuildParamName(column)].Value
                        = editedColumns[column];

            if (taskQueueItem == null) // add new
            {
                if (userCode.HasValue)
                {
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode)].Value
                        = userCode.Value;
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime)].Value
                        = daMgr.DbSynchTime;
                }
                else
                {
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode)].Value
                        = DBNull.Value;

                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime)].Value
                        = DBNull.Value;
                }
            }
            else  // change; where condition params
            {
                dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode)].Value
                    = taskQueueItem[TaskProcessing.Constants.LastModifiedUserCode];
                dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime)].Value
                    = taskQueueItem[TaskProcessing.Constants.LastModifiedDateTime];
                // set portion of the update
                if (userCode.HasValue)
                {
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode, true)].Value
                        = userCode.Value;
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime, true)].Value
                        = daMgr.DbSynchTime;
                }
                else
                {
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode, true)].Value
                        = DBNull.Value;
                    dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime, true)].Value
                        = DBNull.Value;
                }
            }

            return dbCmd;
        }

        /// <summary>
        /// Returns the set of dependency relationships where the given task is the dependent task
        /// </summary>
        /// <param name="daMgr">DbAccessMgr object instance</param>
        /// <param name="waitTaskQueueCode">The taskQueueCode that other tasks may be dependent on</param>
        /// <returns>The DataTable of dependency relationhips</returns>
        public static DataTable GetDependentTasks(DataAccessMgr daMgr
                , Int32 waitTaskQueueCode)
        {
            DbCommand dbCmd = daMgr.DbCmdCacheGetOrAdd("GetDependentTasksCmd", GetDependentTasksCmd);

            dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.WaitTaskQueueCode)].Value
                    = waitTaskQueueCode;

            return daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
        }

        static DbCommand GetDependentTasksCmd(DataAccessMgr daMgr)
        {
            StringBuilder sb = new StringBuilder();
            // we do not have any helper functions to build a recursive query; so we
            // are building this manually.
            sb.AppendFormat("WITH Dependencies ({0}, {1}, Level){2} AS{2} ({2}"
                    , TaskProcessing.Constants.TaskQueueCode
                    , TaskProcessing.Constants.WaitTaskQueueCode
                    , Environment.NewLine);
            sb.AppendFormat("SELECT tpq.{0}, {1}, 1 AS Level{2}"
                    , TaskProcessing.Constants.TaskQueueCode
                    , TaskProcessing.Constants.WaitTaskQueueCode
                    , Environment.NewLine);
            sb.AppendFormat("FROM {0}.{1} tpq{2}"
                    , DataAccess.Constants.SCHEMA_CORE
                    , TaskProcessing.Constants.TaskProcessingQueue
                    , Environment.NewLine);
            sb.AppendFormat("INNER JOIN {0}.{1} td{2}"
                    , DataAccess.Constants.SCHEMA_CORE
                    , TaskProcessing.Constants.TaskDependencies
                    , Environment.NewLine);
            sb.AppendFormat("ON tpq.{0} = td.{0}{1}UNION ALL{1}"
                    , TaskProcessing.Constants.TaskQueueCode
                    , Environment.NewLine);
            sb.AppendFormat("SELECT d.{0}, td.{1}, Level + 1{2}"
                    , TaskProcessing.Constants.TaskQueueCode
                    , TaskProcessing.Constants.WaitTaskQueueCode
                    , Environment.NewLine);
            sb.AppendFormat("FROM Dependencies d{0}"
                    , Environment.NewLine);
            sb.AppendFormat("INNER JOIN {0}.{1} td{2}"
                    , DataAccess.Constants.SCHEMA_CORE
                    , TaskProcessing.Constants.TaskDependencies
                    , Environment.NewLine);
            sb.AppendFormat("ON d.{0} = td.{1}{2})"
                    , TaskProcessing.Constants.WaitTaskQueueCode
                    , TaskProcessing.Constants.TaskQueueCode
                    , Environment.NewLine);
            sb.AppendFormat("SELECT {0}, {1}, Level{2}"
                    , TaskProcessing.Constants.TaskQueueCode
                    , TaskProcessing.Constants.WaitTaskQueueCode
                    , Environment.NewLine);
            sb.AppendFormat("FROM Dependencies d{0}"
                    , Environment.NewLine);
            sb.AppendFormat("WHERE {0} = {1}{2};"
                    , TaskProcessing.Constants.WaitTaskQueueCode
                    , daMgr.BuildBindVariableName(TaskProcessing.Constants.WaitTaskQueueCode)
                    , Environment.NewLine);

            DbCommand dbCmd = daMgr.BuildSelectDbCommand(sb.ToString(), null);
            daMgr.AddNewParameterToCollection(dbCmd.Parameters, TaskProcessing.Constants.WaitTaskQueueCode
                , DbType.Int32
                , null
                , 0
                , ParameterDirection.Input
                , DBNull.Value);
            return dbCmd;
        }

    }
}
