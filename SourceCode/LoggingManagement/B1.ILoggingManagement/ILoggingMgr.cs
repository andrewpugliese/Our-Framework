using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace B1.ILoggingManagement
{

#pragma warning disable 1591 // disable the xmlComments warning
    public enum enumTraceLevel { None = 0, Level1 = 1, Level2 = 2, Level3 = 3, Level4 = 4, Level5 = 5, All = 100};
    public enum enumEventPriority { Normal = 1, Warning = 2, Critical = 4, Trace = 8, All = 0xf };

    public interface ILoggingMgr
    {
        enumTraceLevel TraceLevel { get; set; }

        Int64 WriteToLog(Exception e);
        Int64 WriteToLog(Exception e, enumEventPriority enumPriority);
        Int64 WriteToLog(string message, EventLogEntryType entryType, enumEventPriority enumPriority);

        void Trace(string traceToken);
        void Trace(string traceToken, enumTraceLevel requiredTraceLevel);
        void Trace(DateTime startTime, string traceToken, enumTraceLevel requiredTraceLevel);
    }

#pragma warning restore 1591 // disable the xmlComments warning
}
