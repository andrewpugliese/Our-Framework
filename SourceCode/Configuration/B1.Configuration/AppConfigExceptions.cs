using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Configuration
{
    /// <summary>
    /// An exception representing the case where the config value is not found for a given key.
    /// </summary>
    public class ConfigValueNotFoundException : Exception
    {
        /// <summary>
        /// Get config key name.
        /// </summary>
        public string ConfigKeyName { get; private set; }

        internal ConfigValueNotFoundException(string configKeyName)
        {
            this.ConfigKeyName = configKeyName;
        }

        /// <summary>
        /// Exception message.
        /// </summary>
        public override string Message
        {
            get
            {
                return string.Format("Value not found for the configuration key '{0}'",
                    this.ConfigKeyName);
            }
        }
    }

    /// <summary>
    /// Default instance of the configuration manager is already initiailized.
    /// </summary>
    public class ConfigDefaultInstanceAlreadyInitialized : Exception
    {
        internal ConfigDefaultInstanceAlreadyInitialized()
        {
        }

        /// <summary>
        /// Exception message.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Configuration manager (AppConfigMgr) default instance is already initialized.";
            }
        }
    }

    /// <summary>
    /// Multiple value found for the given config key.
    /// </summary>
    public class ConfigMultipleValueFoundException : Exception
    {
        /// <summary>
        /// Get config key name.
        /// </summary>
        public string ConfigKeyName { get; private set; }

        internal ConfigMultipleValueFoundException(string configKeyName)
        {
            this.ConfigKeyName = configKeyName;
        }

        /// <summary>
        /// Exception message.
        /// </summary>
        public override string Message
        {
            get
            {
                return string.Format("Multiple Value found for the configuration key '{0}'",
                    this.ConfigKeyName);
            }
        }
    }


    /// <summary>
    /// Runtime value function is not found for a given key.
    /// </summary>
    public class ConfigRuntimeValueFunctionNotSetException : Exception
    {
        /// <summary>
        /// Get config key name.
        /// </summary>
        public string ConfigKeyName { get; private set; }

        /// <summary>
        /// Constructor for exception object
        /// </summary>
        /// <param name="configKeyName">Config key that had the exception</param>
        public ConfigRuntimeValueFunctionNotSetException(string configKeyName)
        {
            this.ConfigKeyName = configKeyName;
        }

        /// <summary>
        /// Exception message.
        /// </summary>
        public override string Message
        {
            get
            {
                return string.Format("Runtime Config Value function not found for the configuration key '{0}'",
                    this.ConfigKeyName);
            }
        }
    }

}
