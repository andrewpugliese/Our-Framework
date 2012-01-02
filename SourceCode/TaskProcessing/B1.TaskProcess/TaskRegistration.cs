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
    /// <summary>
    /// Class which handles all the task registrations for the given assembly.
    /// All tasks found in the given assembly which derive from the TaskProcess abstract class
    /// are 'merged' into the database table TaskRegistrations
    /// </summary>
    public class TaskRegistration
    {
        /// <summary>
        /// Constructor accepting the assembly and a usercode performing the registration
        /// </summary>
        /// <param name="daMgr">DataAccess manager object</param>
        /// <param name="assemblyName">Fully qualified assembly name</param>
        /// <param name="assemblyFileName">Fully qualified assembly path and filename</param>
        /// <param name="userCode">Usercode of user performing registration</param>
        /// <returns></returns>
        public static Int32 RegisterAssemblyTasks(DataAccessMgr daMgr
                , string assemblyName
                , string assemblyFileName
                , Int32? userCode)
        {
            // Load the assemlb
            Assembly asm = Assembly.LoadFrom(assemblyFileName);
            DbCommandMgr cmdMgr = new DbCommandMgr(daMgr);
            DbTableDmlMgr dmlMgr = new DbTableDmlMgr(daMgr, DataAccess.Constants.SCHEMA_CORE, Constants.TaskRegistrations);
            dmlMgr.AddColumn(Constants.TaskId, daMgr.BuildParamName(Constants.TaskId), DbTableColumnType.ForInsertOnly);
            dmlMgr.AddColumn(Constants.AssemblyName, daMgr.BuildParamName(Constants.AssemblyName));
            dmlMgr.AddColumn(Constants.TaskDescription, daMgr.BuildParamName(Constants.TaskDescription));
            dmlMgr.AddColumn(Constants.LastRegisteredDate, EnumDateTimeLocale.Default);
            // if usercode was provided add it to last mod key
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
            // build a merge statement
            DbCommand dbCmd = daMgr.BuildMergeDbCommand(dmlMgr);

            int typesFound = 0;
            // set the values for the dbCommand
            foreach (Type t in ObjectFactory.SearchTypes<TaskProcess>(asm))
            {
                // we must create the task object in or to have access to the TaskDesve
                TaskProcess tp = ObjectFactory.Create<TaskProcess>(assemblyFileName, t.FullName, null, null, null, null, null, null);
                dbCmd.Parameters[daMgr.BuildParamName(Constants.TaskId)].Value = t.FullName; 
                dbCmd.Parameters[daMgr.BuildParamName(Constants.AssemblyName)].Value = assemblyName;
                dbCmd.Parameters[daMgr.BuildParamName(Constants.TaskDescription)].Value = tp.TaskDescription();
                if (userCode.HasValue)
                    dbCmd.Parameters[daMgr.BuildParamName(Constants.LastModifiedUserCode)].Value = userCode.Value;
                cmdMgr.AddDbCommand(dbCmd);
                ++typesFound;
            }
            // register the task (update it if exists otherwise insert)
            cmdMgr.ExecuteNonQuery();
            return typesFound;
        }

        /// <summary>
        /// Returns the list of registered tasks as a datatable
        /// </summary>
        /// <param name="daMgr">DataAccess manager object</param>
        /// <returns></returns>
        public static DataTable GetRegisteredTasks(DataAccessMgr daMgr)
        {
            DbCommand dbCmd = daMgr.DbCmdCacheGetOrAdd(Constants.TaskRegistrationList
                    , BuildCmdGetRegisteredTasksList);
            return daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
        }
            
        /// <summary>
        /// Returns the DbCommand to get this list of registered tasks.
        /// </summary>
        /// <param name="daMgr">DataAccess manager object</param>
        /// <returns></returns>
        static DbCommand BuildCmdGetRegisteredTasksList(DataAccessMgr daMgr)
        {
            // get all the columns and all the rows
            DbTableDmlMgr dmlSelectMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.TaskRegistrations);
            // return the DbCommand object
            return daMgr.BuildSelectDbCommand(dmlSelectMgr, null);
        }

    }
}
