using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

using B1.ILoggingManagement;

namespace B1.LoggingManagement
{
    #pragma warning disable 1591 // disable the xmlComments warning
    /// <summary>
    /// Class that represents a trace message.
    /// </summary>
    [DataContract]
    public class TraceMessage
    {
        [DataMember]
        public string Time;
        [DataMember]
        public Stack<string> Context;
        [DataMember]
        public string MachineName;
        [DataMember]
        public string ProcessName;
        [DataMember]
        public int ProcessId;
        [DataMember]
        public int ThreadId;
        [DataMember]
        public string Message;
    }
    #pragma warning restore 1591 // disable the xmlComments warning

    /// <summary>
    /// Target for tracing to memory mapped log file. 
    /// </summary>
    /// <remarks>
    /// There is only one trace log per machine. If the trace log is NOT being read, not tracing
    /// occurs. Reading of the trace log can be done with the 
    /// <see cref="B1.LoggingManagement.MemoryMappedLogReader">MemoryMappedLogReader</see> class. There is a 
    /// TraceViewer windows application that provides user interface for viewing the trace log.
    /// </remarks>
    public class TraceLog : ILoggingTarget
    {   
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            Write(message, false, 0, 0, EventLogEntryType.Information, enumEventPriority.Trace);
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
        public void Write(string message, bool appendText, int eventId, long eventReference, 
                System.Diagnostics.EventLogEntryType entryType, enumEventPriority enumPriority)
        {
            if(!MemoryMappedLog.TraceWriter.BeingRead)
                return;

            MemoryMappedLog.TraceWriter.Write(GetTraceBytes(
                    new TraceMessage()
                    {
                        Time = DateTime.Now.ToString("HH:mm:ss.fffffff"),
                        Context = LoggingContext.Context,
                        MachineName = Environment.MachineName,
                        ProcessId = Process.GetCurrentProcess().Id,
                        ProcessName = Process.GetCurrentProcess().ProcessName,
                        ThreadId = Thread.CurrentThread.ManagedThreadId,
                        Message = message
                    }));
        }

        /// <summary>
        /// Priorities of messages this target will log
        /// </summary>
        public IEnumerable<enumEventPriority> Priorities
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetTraceString(TraceMessage message)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TraceMessage));

            using(MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, message);

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte[] GetTraceBytes(TraceMessage message)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TraceMessage));

            using(MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, message);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static TraceMessage GetTraceMessage(string message)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TraceMessage));

            using(MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(message)))
            {
                return (TraceMessage)serializer.ReadObject(ms);
            }
        }

    }
}
