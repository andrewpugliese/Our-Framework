using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using B1.Core;
using B1.CacheManagement;
using B1.Configuration;

namespace B1.DataAccess
{
    /// <summary>
    /// Data Configuration Manager - This class is used to obtain data out of the database
    /// First the global settings are loaded then
    /// the application settings are merged on top of the global settings. Application level settings overrides the
    /// global level settings.
    /// </summary>
    public class DataConfigMgr
    {
        private DataAccessMgr _daMgr;
        private string _appConfigSetName;
        private string _globalConfigSetName;
        private bool _valueChangeWatcherStarted = false;

        /// <summary>
        /// Function definition for the value change of a config setting.
        /// </summary>
        public delegate bool ValueChangeDelegate(string configKeyName, string oldValue, string newValue);

        /// <summary>
        /// Function definition for the value changes to a list of config settings.
        /// </summary>
        public delegate bool ListChangeDelegate(string configKeyName, IEnumerable<string> oldValue,
                IEnumerable<string> newValue);

        Dictionary<string, IEnumerable<string>> _settings = new Dictionary<string
                , IEnumerable<string>>(StringComparer.CurrentCultureIgnoreCase);
        Dictionary<string, object> _handlers = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Given the DataAccessMgr, it loads up the 
        /// data configuration settings from the database using Default configuration values
        /// for AppConfigSetname, GlobalConfigSetName.
        /// </summary>
        public DataConfigMgr(DataAccessMgr daMgr)
            : this(daMgr
            , AppConfigMgr.GetValueOrDefault(Constants.AppConfigSetName, null)
            , AppConfigMgr.GetValueOrDefault(Constants.GlobalConfigSetName, null))
        {
        }

        /// <summary>
        /// Given the DataAccessMgr, AppConfigSetName and GlobalConfigSetName - it loads up the 
        /// data configuration settings from the database.
        /// </summary>
        public DataConfigMgr(DataAccessMgr daMgr, string appConfigSetName, string globalConfigSetName)
        {
            _daMgr = daMgr;
            LoadDataConfigSettings(appConfigSetName, globalConfigSetName);
        }

        /// <summary>
        /// Load the Application configuration settings from the database. First the global settings are loaded then
        /// the application settings are merged on top of the global settings. Application level settings overrides the
        /// global level settings.
        /// </summary>
        private void LoadDataConfigSettings(string appConfigSetName, string globalConfigSetName)
        {
            // Load the application config settings, if values are provided
            _appConfigSetName = appConfigSetName;
            _globalConfigSetName = globalConfigSetName;
            if (!string.IsNullOrEmpty(_appConfigSetName)
                && !string.IsNullOrEmpty(_globalConfigSetName))
                _settings = GetAppConfigSettings(_globalConfigSetName, _appConfigSetName);
        }


        private static DbCommand CreateAppSettingsDbCommand(DataAccessMgr daMgr)
        {
            DbTableDmlMgr dmlSelect = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                                    , DataAccess.Constants.TABLE_AppConfigSettings);

            dmlSelect.SetWhereCondition((t) =>
                    t.Column(DataAccess.Constants.TABLE_AppConfigSettings, Configuration.Constants.ConfigSetName) ==
                    t.Parameter(DataAccess.Constants.TABLE_AppConfigSettings, Configuration.Constants.ConfigSetName,
                        daMgr.BuildParamName(Configuration.Constants.ConfigSetName)));

            dmlSelect.OrderByColumns.Add(1,
                    new DbQualifiedObject<DbIndexColumnStructure>(DataAccess.Constants.SCHEMA_CORE
                                , DataAccess.Constants.TABLE_AppConfigSettings,
                                daMgr.BuildIndexColumnAscending(Configuration.Constants.ConfigKey)));

            DbCommand dbCmd = daMgr.BuildSelectDbCommand(dmlSelect, null);
            return dbCmd;
        }

        private Dictionary<string, IEnumerable<string>> GetAppConfigSettings(string sysConfigSetName, string appConfigSetName)
        {
            // Get the AppSettings DbCommand from the cache - create one if NOT exist in the cache
            DbCommand dbCmd = _daMgr.DbCmdCacheGetOrAdd(Configuration.Constants.AppSettingCacheName, CreateAppSettingsDbCommand);

            // Get the system config set name
            Dictionary<string, IEnumerable<string>> sys =
                    _daMgr.ExecuteReader(dbCmd, null, rdr => CreateAppConfigSettingDictionary(rdr, sysConfigSetName),
                        _daMgr.BuildParamName(Configuration.Constants.ConfigSetName), sysConfigSetName);

            // Get the application config set name
            Dictionary<string, IEnumerable<string>> app =
                    _daMgr.ExecuteReader(dbCmd, null, rdr => CreateAppConfigSettingDictionary(rdr, null),
                        _daMgr.BuildParamName(Configuration.Constants.ConfigSetName), appConfigSetName);

            return sys.Union(app).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static Dictionary<string, IEnumerable<string>> CreateAppConfigSettingDictionary(
                IDataReader rdr, string prepandConfigKeyStr)
        {
            Dictionary<string, IEnumerable<string>> dic = new Dictionary<string, IEnumerable<string>>();
            int configKeyOrdinal = rdr.GetOrdinal(Configuration.Constants.ConfigKey);
            int configValueOrdinal = rdr.GetOrdinal(Configuration.Constants.ConfigValue);
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

        /// <summary>
        /// Compute config changes.
        /// </summary>
        private void ComputeConfigChange(string callbackKey)
        {
            // Get the latest settings from the database
            Dictionary<string, IEnumerable<string>> newSettings =
                    GetAppConfigSettings(_globalConfigSetName, _appConfigSetName);

            // Check if values of any of the registered key is changed if so call their respective change handler
            // _handlers.ToArray() to make this comparison thread-safe. The value change functions are called in
            // parallel.
            _handlers.ToArray()
                    .AsParallel()
                    .Where(kv =>
                        _settings[kv.Key].Count() != newSettings[kv.Key].Count()
                        || !_settings[kv.Key].SequenceEqual(newSettings[kv.Key]))
                    .AsParallel()
                    .ForAll(kv => RaiseOnChangeEvent(kv.Key, kv.Value, _settings[kv.Key], newSettings[kv.Key]));
        }

        private void RaiseOnChangeEvent(string configKey, object handler, IEnumerable<string> oldValue,
                IEnumerable<string> newValue)
        {
            try
            {
                if (handler is ValueChangeDelegate)
                {
                    ((ValueChangeDelegate)handler)(configKey, oldValue.First(), newValue.First());
                }
                else
                {
                    ((ListChangeDelegate)handler)(configKey, oldValue, newValue);
                }
            }
            catch (Exception ex)
            {
                if (_daMgr.loggingMgr != null)
                    _daMgr.loggingMgr.WriteToLog(ex);
                else throw;
            }
        }

        /// <summary>
        /// Caller can register a callback function for a given key if its value or list of values changes in
        /// the database.
        /// </summary>
        public void RegisterConfigChangeHandler(string configKeyName, ValueChangeDelegate fn)
        {
            _handlers.Add(configKeyName, fn);
            MakeSureConfigChangeWatcherIsRunning();
        }

        /// <summary>
        /// Caller can register a callback function for a given key if its value or list of values changes in
        /// the database.
        /// </summary>
        public void RegisterConfigChangeHandler(string configKeyName, ListChangeDelegate fn)
        {
            _handlers.Add(configKeyName, fn);
            MakeSureConfigChangeWatcherIsRunning();
        }

        private void MakeSureConfigChangeWatcherIsRunning()
        {
            if (_valueChangeWatcherStarted) return;

            lock (this)
            {
                if (_valueChangeWatcherStarted) return;

                // Call every two minutes to compute changes
                RecurringCallbackMgr.Default.Add(this.ToString(), ComputeConfigChange, 120);

                _valueChangeWatcherStarted = true;
            }
        }

    }
}
