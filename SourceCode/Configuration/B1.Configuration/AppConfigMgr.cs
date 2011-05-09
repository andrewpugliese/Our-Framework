using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.Common;

using B1.Core;
//using B1.DataAccess;
//using B1.CacheManagement;
//using B1.ILoggingManagement;

namespace B1.Configuration
{
    /// <summary>   
    /// Main class for managing the application configuration. If a database access manager
    /// is provided then it loads up the value from database tables otherwise uses the .NET
    /// default configuration functions.
    /// </summary>
    public static class AppConfigMgr
    {

        static Dictionary<string, Func<Object>> _runtimeSettings = new Dictionary<string
                , Func<Object>>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Get all the keys of the AppSettings section.
        /// </summary>
        public static IEnumerable<string> GetKeys()
        {
            return ConfigurationManager.AppSettings.AllKeys;
        }


        /// <summary>
        /// GetValue return one value for a given key. If more than one value is found, it throws an exception
        /// ConfigMultipleValueFoundException.
        /// </summary>
        public static string GetValue(string configKeyName)
        {
            string configValue = GetValueOrDefault(configKeyName, null);
            if (configValue == null) throw new ConfigValueNotFoundException(configKeyName);
            return configValue;
        }

        /// <summary>
        /// Its a helper function which allows to pass a convertor function so that one can parse the value and get
        /// back the type of value one wants.
        /// </summary>
        public static T GetValue<T>(string configKeyName, Func<string, T> convertorFn)
        {
            return convertorFn(GetValue(configKeyName));
        }

        /// <summary>
        /// Its a helper function which converts the value to Int32 and returns Int32 value. This is also an example of
        /// how to take advantage of the generic GetValue function.
        /// </summary>
        public static int GetValueAsInt32(string configKeyName)
        {
            return GetValue(configKeyName, val => Convert.ToInt32(val));
        }

        /// <summary>
        /// GetValueOrDefault do NOT throws an exceptoon, instead returns the provided default value if no value is
        /// found for a given key.
        /// </summary>
        public static string GetValueOrDefault(string configKeyName, string defaultValue)
        {
            return GetValueOrDefault(configKeyName, defaultValue, val => val);
        }

        /// <summary>
        /// Generic GetValueOrDefault allows to convert the value into the desired type.
        /// </summary>
        public static T GetValueOrDefault<T>(string configKeyName, T defaultValue, Func<string, T> convertorFn)
        {
            string configValue = ConfigurationManager.AppSettings[configKeyName];
            return configValue == null ? defaultValue : convertorFn(configValue);
        }

        /// <summary>
        /// Retrieves the configuration section object from the configuration file
        /// and casts it to its specific type
        /// </summary>
        /// <typeparam name="T">The type that defines the configuration section</typeparam>
        /// <param name="sectionName">The configuration section name</param>
        /// <returns></returns>
        public static T GetSection<T>(string sectionName)
        {
            return (T)ConfigurationManager.GetSection(sectionName);
        }

        /// <summary>
        /// Set a function which will be called whenever a v alue will be seeked for a given config key.
        /// </summary>
        public static void SetRuntimeValue(string configKeyName, Func<Object> getValueHandler)
        {
            if (!_runtimeSettings.ContainsKey(configKeyName))
                _runtimeSettings.Add(configKeyName, getValueHandler);
            else _runtimeSettings[configKeyName] = getValueHandler;
        }

        /// <summary>
        /// Call the runtime function for a given key.
        /// </summary>
        public static T GetRuntimeValue<T>(string configKeyName)
        {
            Func<object> configValueFn = null;
            if (_runtimeSettings.TryGetValue(configKeyName, out configValueFn))
                return (T)_runtimeSettings[configKeyName]();
            else
                throw new Configuration.ConfigRuntimeValueFunctionNotSetException(configKeyName);
        }

    }
}
