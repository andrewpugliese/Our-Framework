using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.Common;

using B1.Core;
using B1.DataAccess;

namespace B1.Configuration
{
    class AppConfigData
    {
        private static DbCommand CreateAppSettingsDbCommand(DataAccessMgr dbMgr)
        {
            DbTableDmlMgr dmlSelect = dbMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                                    , DataAccess.Constants.TABLE_AppConfigSettings);

            dmlSelect.SetWhereCondition((t) =>
                    t.Column(DataAccess.Constants.TABLE_AppConfigSettings, Constants.ConfigSetName) ==
                    t.Parameter(DataAccess.Constants.TABLE_AppConfigSettings, Constants.ConfigSetName, 
                        dbMgr.BuildParamName(Constants.ConfigSetName)));

            dmlSelect.OrderByColumns.Add(1,
                    new DbQualifiedObject<DbIndexColumnStructure>(DataAccess.Constants.SCHEMA_CORE
                                , DataAccess.Constants.TABLE_AppConfigSettings, 
                                dbMgr.BuildIndexColumnAscending(Constants.ConfigKey)));

            DbCommand dbCmd = dbMgr.BuildSelectDbCommand(dmlSelect, 5000);
            return dbCmd;
        }

        private static Dictionary<string, IEnumerable<string>> GetAppConfigSettings(
            DataAccessMgr dbMgr, string sysConfigSetName, string appConfigSetName)
        {
            // Get the AppSettings DbCommand from the cache - create one if NOT exist in the cache
            DbCommand dbCmd = dbMgr.DbCmdCacheGetOrAdd(Constants.AppSettingCacheName, CreateAppSettingsDbCommand);

            // Get the system config set name
            Dictionary<string, IEnumerable<string>> sys =
                    dbMgr.ExecuteReader(dbCmd, null, rdr => CreateAppConfigSettingDictionary(rdr, sysConfigSetName), 
                        dbMgr.BuildParamName(Constants.ConfigSetName), sysConfigSetName);

            // Get the application config set name
            Dictionary<string, IEnumerable<string>> app =
                    dbMgr.ExecuteReader(dbCmd, null, rdr => CreateAppConfigSettingDictionary(rdr, null), 
                        dbMgr.BuildParamName(Constants.ConfigSetName), appConfigSetName);

            return sys.Union(app).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public static Dictionary<string, IEnumerable<string>> CreateAppConfigSettingDictionary(IDataReader rdr, string prepandConfigKeyStr)
        {
            Dictionary<string, IEnumerable<string>> dic = new Dictionary<string, IEnumerable<string>>();
            int configKeyOrdinal = rdr.GetOrdinal(Constants.ConfigKey);
            int configValueOrdinal = rdr.GetOrdinal(Constants.ConfigValue);
            while (rdr.Read())
            {
                string configKey = prepandConfigKeyStr != null
                        ? prepandConfigKeyStr + "." + rdr.GetString(configKeyOrdinal)
                        : rdr.GetString(configKeyOrdinal);
                string configValue = rdr.GetString(configValueOrdinal);
                if (!dic.ContainsKey(configKey)) dic.Add(configKey, new List<string>());
                ((List<string>)dic[configKey]).Add(configValue);
            }
            return dic;
        }
    }
}
