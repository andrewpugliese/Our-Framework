using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;

using B1.ILoggingManagement;
using B1.FileManagement;
using B1.Core;

namespace B1.LoggingManagement
{
    /// <summary>
    /// ILoggingTarget implementation for logging to file
    /// </summary>
    public class FileLog : ILoggingTarget
    {
        /// <summary>
        /// Maximum number of attempts to write to a file by the EventLogWriter before failing.
        /// </summary>
        public const int MAX_EventLogWriteAttemptCount = 15;

        private string _logFileName;
        private string _logFileDirectory;
        private string _logFilePrefix;
        private enumEventPriority[] _priorities;

        /// <summary>
        /// Initializes a File Log target with a name of logFileName_MMDDHH
        /// </summary>
        /// <param name="logFileName">Log file name prefix. Actual file name will have _MMDDHH.txt appended to it.</param>
        /// <param name="logFileDirectory">Log file directory</param>
        /// <param name="priorities">One or more priorities that this target will log</param>
        public FileLog(string logFileName, string logFileDirectory, params enumEventPriority[] priorities)
        {
            _logFileName = logFileName;
            _logFileDirectory = logFileDirectory;
            _priorities = priorities;

            if (!String.IsNullOrEmpty(_logFileDirectory) && !Directory.Exists(_logFileDirectory))
                Directory.CreateDirectory(_logFileDirectory);

            _logFilePrefix = string.Format("{0}\\{1}", _logFileDirectory, _logFileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="appendText"></param>
        /// <param name="eventId"></param>
        /// <param name="eventReference"></param>
        /// <param name="entryType"></param>
        /// <param name="enumPriority"></param>
        public void Write(string message, bool appendText, Int32 eventId, Int64 eventReference
                        , EventLogEntryType entryType, enumEventPriority enumPriority)
        {
            DateTime now = DateTime.Now;
            string filename = string.Format("{0}_{1}{2}.txt"
                                            , _logFilePrefix
                    , string.Format("{0:##}{1:##}", now.Month, now.Day)
                    , now.ToString("HH"));

            int writeAttempts = 0;           

            while (writeAttempts < MAX_EventLogWriteAttemptCount)
            {
                try
                {
                    lock(_logFilePrefix)
                    {
                        // just in case there is an OS collision
                        FileMgr.WriteTextToFile(filename
                                    , message + Environment.NewLine
                                    , appendText, true);
                        
                        return;
                    }
                }
                catch
                {
                    ++writeAttempts;   // try a number of times
                }
            }
            if (enumPriority == enumEventPriority.Critical)
                // now we must bail out to caller because we could not write
                throw new ExceptionEvent(enumExceptionEventCodes.EventLogFailoverFileWriteFailed
                        , string.Format("Failed to write exception text to file: {0} after {1} attempts"
                            + " Was attempting to write exception: {2}", filename
                                , writeAttempts
                                , message));
                // otherwise, the message is lost but it was not critical
        }


        /// <summary>
        /// Priorities of messages this target will log
        /// </summary>
        public IEnumerable<enumEventPriority> Priorities
        {
            get
            {
                return _priorities;
            }
        }

    }
}
