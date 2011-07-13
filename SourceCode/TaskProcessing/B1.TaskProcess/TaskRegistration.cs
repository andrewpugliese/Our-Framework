using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.Common;

using B1.Core;
using B1.DataAccess;

namespace B1.TaskProcessing
{
    public class TaskRegistration
    {
        public static Int32 RegisterAssemblyTasks(DataAccessMgr daMgr
                , string assemblyName
                , string assemblyFileName
                , Int32? userCode)
        {
            Assembly asm = Assembly.LoadFrom(assemblyFileName);
            DbCommandMgr cmdMgr = new DbCommandMgr(daMgr);
            DbTableDmlMgr dmlMgr = new DbTableDmlMgr(daMgr, DataAccess.Constants.SCHEMA_CORE, Constants.TaskRegistrations);
            dmlMgr.AddColumn(Constants.TaskId, daMgr.BuildParamName(Constants.TaskId), DbTableColumnType.ForInsertOnly);
            dmlMgr.AddColumn(Constants.AssemblyName, daMgr.BuildParamName(Constants.AssemblyName));
            dmlMgr.AddColumn(Constants.TaskDescription, daMgr.BuildParamName(Constants.TaskDescription));
            dmlMgr.AddColumn(Constants.LastRegisteredDate, EnumDateTimeLocale.Default);
            if (userCode.HasValue)
            {
                dmlMgr.AddColumn(Constants.LastModifiedUserCode, daMgr.BuildParamName(Constants.LastModifiedUserCode));
                dmlMgr.AddColumn(Constants.LastModifiedDateTime, EnumDateTimeLocale.Default);
            }
            dmlMgr.SetWhereCondition((j) =>
                    j.Column(Constants.TaskId) ==
                        j.Parameter(Constants.TaskRegistrations
                        , Constants.TaskId
                        , daMgr.BuildParamName(Constants.TaskId)));
            DbCommand dbCmd = daMgr.BuildMergeDbCommand(dmlMgr);

            int typesFound = 0; ;
            foreach (Type t in ObjectFactory.SearchTypes<TaskProcess>(asm))
            {
                TaskProcess tp = ObjectFactory.Create<TaskProcess>(assemblyFileName, t.FullName, null, null, null, null);
                dbCmd.Parameters[daMgr.BuildParamName(Constants.TaskId)].Value = t.FullName; 
                dbCmd.Parameters[daMgr.BuildParamName(Constants.AssemblyName)].Value = assemblyName;
                dbCmd.Parameters[daMgr.BuildParamName(Constants.TaskDescription)].Value = tp.TaskDescription();
                if (userCode.HasValue)
                    dbCmd.Parameters[daMgr.BuildParamName(Constants.LastModifiedUserCode)].Value = userCode.Value;
                cmdMgr.AddDbCommand(dbCmd);
                ++typesFound;
            }
            cmdMgr.ExecuteNonQuery();
            return typesFound;
        }

        public static DataTable GetRegisteredTasks(DataAccessMgr daMgr)
        {
            DbCommand dbCmd = daMgr.DbCmdCacheGetOrAdd(Constants.TaskRegistrationList
                    , BuildCmdGetRegisteredTasksList);
            return daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
        }
            
        static DbCommand BuildCmdGetRegisteredTasksList(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.TaskRegistrations);
            return daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
        }

    }
}
