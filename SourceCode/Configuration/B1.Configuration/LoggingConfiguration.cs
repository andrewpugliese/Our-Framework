using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace B1.Configuration
{

    /// <summary>
    /// This class contains the collection of configuration settings for the Logging Elements
    /// </summary>
    public class LoggingCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Returns the element's name
        /// </summary>
        protected override string ElementName
        {
            get { return Constants.LoggingConfig; }
        }

        /// <summary>
        /// Returns the element for the given index
        /// </summary>
        /// <param name="index">Index within the collection</param>
        /// <returns></returns>
        public LoggingElement this[int index]
        {
            get
            {
                return (LoggingElement)BaseGet(index);
            }
        }

        /// <summary>
        /// Returns the element for the given key
        /// </summary>
        /// <param name="keyName">Name of key within the collection</param>
        /// <returns></returns>
        public new LoggingElement this[string keyName]
        {
            get
            {
                return (LoggingElement)BaseGet(keyName);
            }
        }

        /// <summary>
        /// Returns the ConfigurationElementCollectionType for the collection
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        /// <summary>
        /// Returns the newly instantiated LoggingElement
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new LoggingElement();
        }

        /// <summary>
        /// Returns the key  to the given configuration element
        /// </summary>
        /// <param name="element">Configuration Element</param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LoggingElement)element).LoggingKey;
        }
    }

    /// <summary>
    /// The configuration section for logging options
    /// </summary>
    public class LoggingConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Returns name of the configuration section
        /// </summary>
        public const string ConfigSectionName = Constants.LoggingConfigurations;

        /// <summary>
        /// Returns the configuration section
        /// </summary>
        /// <returns></returns>
        public static LoggingConfiguration GetSection()
        {
            return (LoggingConfiguration)ConfigurationManager.GetSection(ConfigSectionName);
        }

        /// <summary>
        /// Returns the collectionn
        /// </summary>
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public LoggingCollection LoggingConfigurations
        {
            get { return (LoggingCollection)base[""]; }
            set { base[""] = value; }
        }


        /// <summary>
        /// Returns the element of the logging configuration for the given object key
        /// </summary>
        /// <param name="objectKey">The key to the element of the collection</param>
        /// <returns></returns>
        public LoggingElement GetLoggingConfig(string objectKey)
        {
            return LoggingConfigurations.Cast<LoggingElement>()
                .FirstOrDefault(attribute => attribute.LoggingKey == objectKey);
        }

    }

    /// <summary>
    /// This class contains the collection of configuration settings for the Logging Target Elements
    /// </summary>
    public class LoggingTargetsCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Returns the element's name
        /// </summary>
        protected override string ElementName
        {
            get { return Constants.LoggingTarget; }
        }

        /// <summary>
        /// Returns the element for the given index
        /// </summary>
        /// <param name="index">Index within the collection</param>
        /// <returns></returns>
        public LoggingTargetElement this[int index]
        {
            get
            {
                return (LoggingTargetElement)BaseGet(index);
            }
        }

        /// <summary>
        /// Returns the element for the given key
        /// </summary>
        /// <param name="keyName">Name of key within the collection</param>
        /// <returns></returns>
        public new LoggingTargetElement this[string keyName]
        {
            get
            {
                return (LoggingTargetElement)BaseGet(keyName);
            }
        }

        /// <summary>
        /// Returns the ConfigurationElementCollectionType for the collection
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

         /// <summary>
         /// Returns the newly instantiated LoggingTargetElement
         /// </summary>
         /// <returns></returns>
         protected override ConfigurationElement CreateNewElement()
        {
            return new LoggingTargetElement();
        }

         /// <summary>
         /// Returns the key  to the given configuration element
         /// </summary>
         /// <param name="element">Configuration Element</param>
         /// <returns></returns>
         protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LoggingTargetElement)element).TargetType;
        }
    }

    /// <summary>
    /// Returns the configuration element
    /// </summary>
    public class LoggingTargetElement : ConfigurationElement
    {
        /// <summary>
        /// Returns the key to the element
        /// </summary>
        [ConfigurationProperty(Constants.TargetType, IsRequired = true)]
        public string TargetType
        {
            get { return this[Constants.TargetType] as string; }
            set { this[Constants.TargetType] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The name of the log)
        /// </summary>
        [ConfigurationProperty(Constants.LogName, IsRequired = true)]
        public string LogName
        {
            get { return this[Constants.LogName] as string; }
            set { this[Constants.LogName] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (Comma seperated list of priorities)
        /// </summary>
        [ConfigurationProperty(Constants.Priorities, IsRequired = true)]
        public string Priorities
        {
            get { return this[Constants.Priorities] as string; }
            set { this[Constants.Priorities] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (Collection of parameters)
        /// </summary>
        [ConfigurationProperty(Constants.Params)]
        public LoggingParamsCollection Params
        {
            get { return (LoggingParamsCollection)this[Constants.Params]; }
            set { this[Constants.Params] = value; }
        }
    }


    /// <summary>
    /// This class contains the collection of configuration settings for the Logging Target Parameter Elements
    /// </summary>
    public class LoggingParamsCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Returns the element's name
        /// </summary>
        protected override string ElementName
        {
            get { return Constants.Param; }
        }

        /// <summary>
        /// Returns the element for the given index
        /// </summary>
        /// <param name="index">Index within the collection</param>
        /// <returns></returns>
        public LoggingParamsElement this[int index]
        {
            get
            {
                return (LoggingParamsElement)BaseGet(index);
            }
        }

        /// <summary>
        /// Returns the element for the given key
        /// </summary>
        /// <param name="keyName">Name of key within the collection</param>
        /// <returns></returns>
        public new LoggingParamsElement this[string keyName]
        {
            get
            {
                return (LoggingParamsElement)BaseGet(keyName);
            }
        }

        /// <summary>
        /// Returns the value of the given parameter
        /// </summary>
        /// <param name="paramKey">The key of the element collection</param>
        /// <returns></returns>
        public object GetParamValue(string paramKey)
        {
            return ((LoggingParamsElement)BaseGet(paramKey)).ParamValue;
        }

        /// <summary>
        /// Returns the ConfigurationElementCollectionType for the collection
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        /// <summary>
        /// Returns the newly instantiated LoggingParamsElement
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new LoggingParamsElement();
        }

        /// <summary>
        /// Returns the key  to the given configuration element
        /// </summary>
        /// <param name="element">Configuration Element</param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LoggingParamsElement)element).ParamKey;
        }
    }

    /// <summary>
    /// Returns the configuration element
    /// </summary>
    public class LoggingParamsElement : ConfigurationElement
    {
        /// <summary>
        /// Returns the key to the element
        /// </summary>
        [ConfigurationProperty(Constants.ParamKey, IsRequired = true)]
        public string ParamKey
        {
            get { return this[Constants.ParamKey] as string; }
            set { this[Constants.ParamKey] = value; }
        }

        /// <summary>
        /// Returns the value of the parameter key
        /// </summary>
        [ConfigurationProperty(Constants.ParamValue, IsRequired = true)]
        public string ParamValue
        {
            get { return this[Constants.ParamValue] as string; }
            set { this[Constants.ParamValue] = value; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LoggingElement : ConfigurationElement
    {
        /// <summary>
        /// Returns the key to the element for logging configuration
        /// </summary>
        [ConfigurationProperty(Constants.LoggingKey, IsRequired = true)]
        public string LoggingKey
        {
            get { return this[Constants.LoggingKey] as string; }
            set { this[Constants.LoggingKey] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The backup log file directory path)
        /// </summary>
        [ConfigurationProperty(Constants.BackupLogFileDirectory, IsRequired = true)]
        public string BackupLogFileDirectory
        {
            get { return this[Constants.BackupLogFileDirectory] as string; }
            set { this[Constants.BackupLogFileDirectory] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The backup log filename)
        /// </summary>
        [ConfigurationProperty(Constants.BackupLogFileName, IsRequired = true)]
        public string BackupLogFileName
        {
            get { return this[Constants.BackupLogFileName] as string; }
            set { this[Constants.BackupLogFileName] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The current trace level)
        /// </summary>
        [ConfigurationProperty(Constants.TraceLevel, IsRequired = true)]
        public string TraceLevel
        {
            get { return this[Constants.TraceLevel] as string; }
            set { this[Constants.TraceLevel] = value; }
        }

        /// <summary>
        /// Returns the collection of logging targets
        /// </summary>
        [ConfigurationProperty(Constants.LoggingTargets)]
        public LoggingTargetsCollection LoggingTargets
        {
            get { return (LoggingTargetsCollection)this[Constants.LoggingTargets]; }
            set { this[Constants.LoggingTargets] = value; }
        }
    }
}
