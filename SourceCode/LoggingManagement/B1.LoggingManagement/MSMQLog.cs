using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using B1.ILoggingManagement;

namespace B1.LoggingManagement
{
    /// <summary>
    /// ILoggingTarget implementation for logging to MSMQ
    /// </summary>
    public class MSMQLog : ILoggingTarget
    {
        MsmqTraceListener _msmqLogger;
        ILoggingTarget _backupLog;
        enumEventPriority[] _priorities;
        
        /// <summary>
        /// Initializes an MSMQ Logging Target
        /// </summary>
        /// <param name="logName">Enterprise library log name</param>
        /// <param name="queuePath">MSMQ path e.g., ".\\Private$\\NOC" (backslashes need to be escaped) </param>
        /// <param name="backupLog">Instance of the backup log used in case instantion fails.</param>
        /// <param name="priorities">One or more priorities that this target will log</param>
        public MSMQLog(string logName, string queuePath, ILoggingTarget backupLog, 
            params enumEventPriority [] priorities)
        {
            _priorities = priorities;
            _backupLog = backupLog;

            try
            {
                _msmqLogger = new MsmqTraceListener(logName, queuePath, new TextFormatter(), 
                    MessagePriority.Normal, true, TimeSpan.FromDays(1), TimeSpan.FromDays(1), false, true, false, 
                    MessageQueueTransactionType.None);
            }
            catch(Exception ex)
            {
                string errMsg = string.Format("Could not create MSMQ Logger with queuePath: {0} errorMsg: {1} \r\n" 
                                            , ex
                                            , queuePath);

                _backupLog.Write(errMsg
                            , true
                            , 0
                            , 0
                            , EventLogEntryType.Error
                            , enumEventPriority.Critical);

                throw new ExceptionEvent(enumExceptionEventCodes.EventLogSourceCreateFailed, errMsg, ex);
            }
        }

        #region ILoggingTarget Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="appendText"></param>
        /// <param name="eventId"></param>
        /// <param name="eventReference"></param>
        /// <param name="entryType"></param>
        /// <param name="enumPriority"></param>
        public void Write(string message, bool appendText, int eventId, long eventReference, 
            System.Diagnostics.EventLogEntryType entryType, enumEventPriority enumPriority )
        {
             string referencedMsg = string.Empty;
             
             referencedMsg = string.Format("Event Reference {0}: {1}{2}"
                        , eventReference
                        , Environment.NewLine
                        , message);

            _msmqLogger.Write( referencedMsg );
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

        #endregion
    }
}
