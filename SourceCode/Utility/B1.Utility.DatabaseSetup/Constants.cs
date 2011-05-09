using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Utility.DatabaseSetup
{
    /// <summary>
    /// Global constants to be used by the utility.
    /// </summary>
    public class Constants
    {
#pragma warning disable 1591 // disable the xmlComments warning because the following constants are mostly used as string literals
        public const string ApplicationId = "ApplicationId";
        public const string AppSequenceId = "AppSequenceId";
        public const string AppSequenceName = "AppSequenceName";
        public const string AppSynchTime = "AppSynchTime";
        public const string BreakWithMsg = "BreakWithMsg";
        public const string AppLocalTime = "AppLocalTime";
        public const string CommentBlockEnd = "*/";
        public const string CommentBlockStart = "/*";
        public const string CommentLineStart = "//";
        public const string ConnectionKey = "ConnectionKey";
        public const string Data_Type = "Data_Type";
        public const string DbSequenceId = "DbSequenceId";
        public const string DbServer = "DbServer";
        public const string DbServerTime = "DbServerTime";
        public const string DbName = "DbName";
        public const string DbProviderDllPath = "DbProviderDllPath";
        public const string DbType = "DbType";
        public const string Description = "Description";
        public const string ExtraData = "ExtraData";
        public const string EventLogName = "LOGGING_Target_1_LogName";
        public const string EventLogSource = "LOGGING_TARGET_1_EventLogSource";
        public const string EventLogDirectory = "LOGGING_BackupLogFileDirectory";
        public const string LoggingKey = "LoggingKey";
        public const string Pause = "Pause";
        public const string RunCmdFile = "RunCmdFile";
        public const string Remarks = "Remarks";
        public const string Resume = "Resume";
        public const string ServerOnly = "ServerOnly";
        public const string Start = "Start";
        public const string Stop = "Stop";
        public const string TextEditor = "TextEditor";
        public const string TraceLevel = "TraceLevel";
        public const string Username = "Username";
        public const string Userpassword = "Userpassword";

        public const Int32 UIControl_Signoff = 4;
        public const Int32 UIControl_ChangeSignonControl = 3;
        public const Int32 UIControl_CleanupInactiveAppSessions = 2;
        public const Int32 UIControl_CleanupInactiveUserSessions = 1;
#pragma warning restore 1591 // disable the xmlComments warning
    }
}
