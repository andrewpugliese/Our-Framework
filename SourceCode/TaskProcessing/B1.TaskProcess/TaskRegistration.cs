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
                , bool includeVersionInTaskId
                , bool includeAssemblyPath)
        {
            Assembly asm = Assembly.LoadFrom(assemblyFileName);
            DbCommandMgr cmdMgr = new DbCommandMgr(daMgr);
            DbTableDmlMgr dmlMgr = new DbTableDmlMgr(Constants.TaskRegistrations, DataAccess.Constants.SCHEMA_CORE);
            dmlMgr.AddColumn(Constants.TaskId, daMgr.BuildParamName(Constants.TaskId), DbTableColumnType.ForInsertOnly);
            dmlMgr.AddColumn(Constants.AssemblyName, daMgr.BuildParamName(Constants.AssemblyName));
            dmlMgr.AddColumn(Constants.AssemblyVersion, daMgr.BuildParamName(Constants.AssemblyVersion));
            dmlMgr.AddColumn(Constants.TaskDescription, daMgr.BuildParamName(Constants.TaskDescription));
            dmlMgr.AddColumn(Constants.LastRegisteredDate, EnumDateTimeLocale.Default);
            if (includeAssemblyPath)
                dmlMgr.AddColumn(Constants.AssemblyFile, daMgr.BuildParamName(Constants.AssemblyFile));
            dmlMgr.AddColumn(Constants.ClassName, daMgr.BuildParamName(Constants.ClassName));
            dmlMgr.SetWhereCondition((j) =>
                    j.Column(Constants.TaskId) ==
                    j.Parameter(Constants.TaskRegistrations, Constants.TaskId,
                        daMgr.BuildParamName(Constants.TaskId)));
            DbCommand dbCmd = daMgr.BuildMergeDbCommand(dmlMgr);

            int typesFound = 0; ;
            string version = asm.GetName().Version.ToString();
            foreach (Type t in ObjectFactory.SearchTypes<TaskProcess>(asm))
            {
                TaskProcess tp = ObjectFactory.Create<TaskProcess>(assemblyFileName, t.FullName, null, null, null, null);
                dbCmd.Parameters[daMgr.BuildParamName(Constants.TaskId)].Value = t.FullName 
                        + (includeVersionInTaskId ? "." + version : "");
                dbCmd.Parameters[daMgr.BuildParamName(Constants.ClassName)].Value = t.FullName; 
                dbCmd.Parameters[daMgr.BuildParamName(Constants.AssemblyFile)].Value = assemblyFileName;
                dbCmd.Parameters[daMgr.BuildParamName(Constants.AssemblyName)].Value = assemblyName;
                dbCmd.Parameters[daMgr.BuildParamName(Constants.AssemblyVersion)].Value = version;
                dbCmd.Parameters[daMgr.BuildParamName(Constants.TaskDescription)].Value = tp.TaskDescription();
                cmdMgr.AddDbCommand(dbCmd);
                ++typesFound;
            }
            cmdMgr.ExecuteNonQuery();
            return typesFound;
        }
    }
}
