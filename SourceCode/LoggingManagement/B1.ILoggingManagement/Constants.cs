
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.ILoggingManagement
{
    /// <summary>
    /// Contains all string literals, numeric constants, etc for the ILoggingManagement interface
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Returns string version of LoggingKey
        /// </summary>
        public const string LoggingKey = "LoggingKey";

        /// <summary>
        /// Returns the string version of TraceLevel
        /// </summary>
        public const string TraceLevel = "TraceLevel";

        public static List<string> TraceLevels()
        {
            List<string> traceLevels = new List<string>();
            traceLevels.Add(enumTraceLevel.None.ToString());
            traceLevels.Add(enumTraceLevel.Level1.ToString());
            traceLevels.Add(enumTraceLevel.Level2.ToString());
            traceLevels.Add(enumTraceLevel.Level3.ToString());
            traceLevels.Add(enumTraceLevel.Level4.ToString());
            traceLevels.Add(enumTraceLevel.Level2.ToString());
            traceLevels.Add(enumTraceLevel.All.ToString());
            return traceLevels;
        }

        public static enumTraceLevel ToTraceLevel(string traceLevel)
        {
            if (traceLevel.ToLower() == enumTraceLevel.None.ToString().ToLower())
                return enumTraceLevel.None;
            if (traceLevel.ToLower() == enumTraceLevel.Level1.ToString().ToLower())
                return enumTraceLevel.Level1;
            if (traceLevel.ToLower() == enumTraceLevel.Level2.ToString().ToLower())
                return enumTraceLevel.Level2;
            if (traceLevel.ToLower() == enumTraceLevel.Level3.ToString().ToLower())
                return enumTraceLevel.Level3;
            if (traceLevel.ToLower() == enumTraceLevel.Level4.ToString().ToLower())
                return enumTraceLevel.Level4;
            if (traceLevel.ToLower() == enumTraceLevel.Level5.ToString().ToLower())
                return enumTraceLevel.Level5;
            if (traceLevel.ToLower() == enumTraceLevel.All.ToString().ToLower())
                return enumTraceLevel.All;
            else throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                , string.Format("Unknown traceLevel: {0}", traceLevel));
        }
    }
}
