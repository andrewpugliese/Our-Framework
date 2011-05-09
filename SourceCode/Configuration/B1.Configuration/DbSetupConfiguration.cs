using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace B1.Configuration
{
    /// <summary>
    /// This class contains the collection of configuration settings for the Database Setup Elements
    /// </summary>
    public class DbSetupCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Returns the element's name
        /// </summary>
        protected override string ElementName
        {
            get { return Constants.DbSetupConfig; }
        }

        /// <summary>
        /// Returns the element for the given index
        /// </summary>
        /// <param name="index">Index within the collection</param>
        /// <returns></returns>
        public DbSetupElement this[int index]
        {
            get
            {
                return (DbSetupElement)BaseGet(index);
            }
        }

        /// <summary>
        /// Returns the element for the given key
        /// </summary>
        /// <param name="keyName">Name of key within the collection</param>
        /// <returns></returns>
        public new DbSetupElement this[string keyName]
        {
            get
            {
                return (DbSetupElement)BaseGet(keyName);
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
        /// Returns the newly instantiated DbSetupElement
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new DbSetupElement();
        }

        /// <summary>
        /// Returns the key  to the given configuration element
        /// </summary>
        /// <param name="element">Configuration Element</param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DbSetupElement)element).DbSetupKey;
        }
    }

    /// <summary>
    /// The configuration section for Database Setup options
    /// </summary>
    public class DbSetupConfiguration : ConfigurationSection
    {
        /// <summary>
        /// The name of the configuration section
        /// </summary>
        public const string ConfigSectionName = Constants.DbSetupConfigurations;

        /// <summary>
        /// Returns the configuration section
        /// </summary>
        /// <returns></returns>
        public static DbSetupConfiguration GetSection()
        {
            return (DbSetupConfiguration)ConfigurationManager.GetSection(ConfigSectionName);
        }

        /// <summary>
        /// Returns the collectionn
        /// </summary>
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DbSetupCollection DbSetupConfigurations
        {
            get { return (DbSetupCollection)base[""]; }
            set { base[""] = value; }
        }

        /// <summary>
        /// Returns the element of the database setup configuration for the given object key
        /// </summary>
        /// <param name="objectKey">The key to the element of the collection</param>
        /// <returns></returns>
        public DbSetupElement GetDbSetupConfig(string objectKey)
        {
            return DbSetupConfigurations.Cast<DbSetupElement>()
                .FirstOrDefault(attribute => attribute.DbSetupKey == objectKey);
        }

    }

    /// <summary>
    /// This class contains the collection of configuration settings for the DbSetup Parameter Elements
    /// </summary>
    public class DbSetupParamsCollection : ConfigurationElementCollection
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
        public DbSetupParamsElement this[int index]
        {
            get
            {
                return (DbSetupParamsElement)BaseGet(index);
            }
        }

        /// <summary>
        /// Returns the element for the given key
        /// </summary>
        /// <param name="keyName">Name of key within the collection</param>
        /// <returns></returns>
        public new DbSetupParamsElement this[string keyName]
        {
            get
            {
                return (DbSetupParamsElement)BaseGet(keyName);
            }
        }

        /// <summary>
        /// Returns the value of the given parameter
        /// </summary>
        /// <param name="paramKey">The key of the element collection</param>
        /// <returns></returns>
        public object GetParamValue (string paramKey)
        {
            return ((DbSetupParamsElement)BaseGet(paramKey)).ParamValue;
        }

        /// <summary>
        /// Returns the ConfigurationElementCollectionType for the collection
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        /// <summary>
        /// Returns the newly instantiated DbSetupParamsElement
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new DbSetupParamsElement();
        }

        /// <summary>
        /// Returns the key  to the given configuration element
        /// </summary>
        /// <param name="element">Configuration Element</param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DbSetupParamsElement)element).ParamKey;
        }
    }

    /// <summary>
    /// Returns the configuration element
    /// </summary>
    public class DbSetupParamsElement : ConfigurationElement
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
    /// Returns the configuration element for database setup configuration
    /// </summary>
    public class DbSetupElement : ConfigurationElement
    {
        /// <summary>
        /// Returns the key to the element
        /// </summary>
        [ConfigurationProperty(Constants.DbSetupKey, IsRequired = true)]
        public string DbSetupKey
        {
            get { return this[Constants.DbSetupKey] as string; }
            set { this[Constants.DbSetupKey] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The database server (address, name, ip, or servicename)
        /// </summary>
        [ConfigurationProperty(Constants.DbServer, IsRequired = true)]
        public string DbServer
        {
            get { return this[Constants.DbServer] as string; }
            set { this[Constants.DbServer] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (Database type enumerator: Oracle, SqlServer, DB2)
        /// </summary>
        [ConfigurationProperty(Constants.DbType, IsRequired = true)]
        public string DbType
        {
            get { return this[Constants.DbType] as string; }
            set { this[Constants.DbType] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (Database name will be created or altered)
        /// </summary>
        [ConfigurationProperty(Constants.DbName, IsRequired = true)]
        public string DbName
        {
            get { return this[Constants.DbName] as string; }
            set { this[Constants.DbName] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The username to connect as)
        /// </summary>
        [ConfigurationProperty(Constants.UserName, IsRequired = true)]
        public string UserName
        {
            get { return this[Constants.UserName] as string; }
            set { this[Constants.UserName] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The password of the username)
        /// </summary>
        [ConfigurationProperty(Constants.UserPassword, IsRequired = true)]
        public string UserPassword
        {
            get { return this[Constants.UserPassword] as string; }
            set { this[Constants.UserPassword] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The path and name of the command file)
        /// </summary>
        [ConfigurationProperty(Constants.InputFileName, IsRequired = true)]
        public string InputFileName
        {
            get { return this[Constants.InputFileName] as string; }
            set { this[Constants.InputFileName] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The path and name of the file to store the results)
        /// </summary>
        [ConfigurationProperty(Constants.OutputFileName, IsRequired = true)]
        public string OutputFileName
        {
            get { return this[Constants.OutputFileName] as string; }
            set { this[Constants.OutputFileName] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The path and name of the executable for the editor to use)
        /// </summary>
        [ConfigurationProperty(Constants.TextEditor, IsRequired = true)]
        public string TextEditor
        {
            get { return this[Constants.TextEditor] as string; }
            set { this[Constants.TextEditor] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The path of the directory that contains all the DDL files)
        /// </summary>
        [ConfigurationProperty(Constants.DDLSourceDirectory, IsRequired = true)]
        public string DDLSourceDirectory
        {
            get { return this[Constants.DDLSourceDirectory] as string; }
            set { this[Constants.DDLSourceDirectory] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (True or False for Oracle)
        /// </summary>
        [ConfigurationProperty(Constants.AsSysDba, IsRequired = true)]
        public string AsSysDba
        {
            get { return this[Constants.AsSysDba] as string; }
            set { this[Constants.AsSysDba] = value; }
        }

        /// <summary>
        /// Returns the value of the attribute (The collection of parameters)
        /// </summary>
        [ConfigurationProperty(Constants.Params)]
        public DbSetupParamsCollection Params
        {
            get { return (DbSetupParamsCollection)this[Constants.Params]; }
            set { this[Constants.Params] = value; }
        }
    }
}
