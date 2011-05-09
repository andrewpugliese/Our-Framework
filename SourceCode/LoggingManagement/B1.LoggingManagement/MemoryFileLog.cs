﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using B1.Core;
using B1.ILoggingManagement;

namespace B1.LoggingManagement
{
    /// <summary>
    /// High performance file logging. Uses a persisted memory mapped file to log to disk.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this class is instantiated, a file of logFileSize will be allocated on the disk. Writing to this file will be 
    /// done by writing to memory. The OS will flush the memory to disk periodically (it happens almost instaneously).
    /// </para>
    /// <para>
    /// The file on disk will have a name of logFileNamePrefix_MM_DD_HH_mm.txt (Month, Day, Hour Minute). If a file with that 
    /// name already exists, a unique name will be generated by appending "_N" where N is a number which makes it unique. 
    /// Once the file has been filled, a new file will be generated by appending "_N" where N is incremented to make the name
    /// unique.
    /// </para>
    /// </remarks>
    public class MemoryFileLog : ILoggingTarget, IDisposable
    {
        private MemoryMappedLogWriter _writer = null;

        private enumEventPriority[] _priorities;

        private string _logName;
        private string _logFileDirectory;
        private string _logFileNamePrefix;
        private int _logFileSize;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logName">Machine unique log name.</param>
        /// <param name="logFileDirectory"></param>
        /// <param name="logFileNamePrefix"></param>
        /// <param name="logFileSize"></param>
        /// <param name="priorities"></param>
        public MemoryFileLog(string logName, string logFileNamePrefix, string logFileDirectory, int logFileSize, 
                params enumEventPriority[] priorities)
        {
            _logName = logName;
            _logFileNamePrefix = logFileNamePrefix;
            _logFileDirectory = logFileDirectory;
            _logFileSize = logFileSize;
            _writer = new MemoryMappedLogWriter(_logName, GenerateLogFileName(), "txt", _logFileDirectory, false, _logFileSize);
            _priorities = priorities;
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
        public void Write(string message, bool appendText, int eventId, long eventReference, System.Diagnostics.EventLogEntryType entryType, enumEventPriority enumPriority)
        {
            _writer.Write(message + Environment.NewLine);
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<enumEventPriority> Priorities
        {
            get { return _priorities; }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>       
        private string GenerateLogFileName()
        {
            DateTime now = DateTime.Now;
            return string.Format("{0}_{1:##}{2:##}{3}{4}", _logFileNamePrefix, now.Month, now.Day
                    , now.ToString("HH"), now.ToString("mm"));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
