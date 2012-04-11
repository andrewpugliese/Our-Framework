
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

        /// <summary>
        /// Returns the list of trace levels
        /// </summary>
        /// <returns>List of enumTraceLevel items</returns>
        public static List<enumTraceLevel> TraceLevels()
        {
            List<enumTraceLevel> traceLevels = new List<enumTraceLevel>();
            traceLevels.Add(enumTraceLevel.None);
            traceLevels.Add(enumTraceLevel.Level1);
            traceLevels.Add(enumTraceLevel.Level2);
            traceLevels.Add(enumTraceLevel.Level3);
            traceLevels.Add(enumTraceLevel.Level4);
            traceLevels.Add(enumTraceLevel.Level2);
            traceLevels.Add(enumTraceLevel.All);
            return traceLevels;
        }

        /// <summary>
        /// Converts the given string version of trace level to the equivalent enumeration
        /// </summary>
        /// <param name="traceLevel">String version of trace level</param>
        /// <returns>Enumeration type of trace level</returns>
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
