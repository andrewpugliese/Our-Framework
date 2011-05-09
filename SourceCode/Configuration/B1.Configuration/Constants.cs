using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Configuration
{
    /// <summary>
    /// Global constants to be used by the Configuration libary.
    /// </summary>
    public class Constants
    {
#pragma warning disable 1591 // disable the xmlComments warning because the following constants are mostly used as string literals
        public const string AppSettingCacheName = "AppConfigData_AppSettings";
        public const string ConfigSetName = "ConfigSetName";
        public const string ConfigKey = "ConfigKey";
        public const string ConfigValue = "ConfigValue";

        public const string AssemblyName = "AssemblyName";
        public const string AssemblyPath = "AssemblyPath";
        internal const string ObjectFactories = "ObjectFactories";
        internal const string ObjectFactory = "ObjectFactory";
        public const string ObjectKey = "ObjectKey";
        public const string ObjectClass = "ObjectClass";

        internal const string LoggingConfigurations = "LoggingConfigurations";
        internal const string LoggingConfig = "LoggingConfig";
        internal const string LoggingTargets = "LoggingTargets";
        internal const string LoggingTarget = "LoggingTarget";
        public const string LoggingKey = "LoggingKey";
        internal const string LogName = "LogName";
        internal const string Priorities = "Priorities";
        internal const string TargetType = "TargetType";
        internal const string BackupLogFileName = "BackupLogFileName";
        internal const string BackupLogFileDirectory = "BackupLogFileDirectory";
        internal const string TraceLevel = "TraceLevel";

        internal const string DbSetupConfiguration = "DbSetupConfiguration";
        internal const string DbSetupConfigurations = "DbSetupConfigurations";
        internal const string DbSetupConfig = "DbSetupConfig";
        internal const string DbSetupKey = "DbSetupKey";
        internal const string DbServer = "DbServer";
        internal const string DbName = "DbName";
        internal const string DbType = "DbType";
        internal const string UserName = "UserName";
        internal const string UserPassword = "UserPassword";
        internal const string AsSysDba = "AsSysDba";
        internal const string DDLSourceDirectory = "DDLSourceDirectory";
        internal const string TextEditor = "TextEditor";
        internal const string InputFileName = "InputFileName";
        internal const string OutputFileName = "OutputFileName";
        internal const string Param = "Param";
        internal const string Params = "Params";
        internal const string ParamKey = "ParamKey";
        internal const string ParamValue = "ParamValue";
#pragma warning restore 1591 // disable the xmlComments warning
    }
}
