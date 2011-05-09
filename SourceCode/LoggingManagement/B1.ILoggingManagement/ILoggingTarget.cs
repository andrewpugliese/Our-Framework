using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;



namespace B1.ILoggingManagement
{
    /// <summary>
    /// Interface used to implement logging targets for LoggingMgr class, e.g., MSMQ, Windows Event Log, DB, etc.
    /// </summary>
    public interface ILoggingTarget
    {
        /// <summary>
        /// Method used for writing message to log target.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="appendText">Append text to end of log. May or may not be relevant depending on target.</param>
        /// <param name="eventId">EventId. May or may not be relevant depending on target.</param>
        /// <param name="eventReference">"Reference identifier for this message. Can be usefull when trying to identify 
        /// something that happend.</param>
        /// <param name="entryType">Category of message.</param>
        /// <param name="enumPriority">Proirity of message./</param>
        void Write(string message, bool appendText, Int32 eventId, Int64 eventReference, EventLogEntryType entryType,
                enumEventPriority enumPriority);

        /// <summary>
        /// Readonly property that returns a collection of message priorities this target wants.
        /// </summary>
        IEnumerable<enumEventPriority> Priorities { get; }
    }
}
