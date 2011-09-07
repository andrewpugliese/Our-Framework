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
using System.Threading.Tasks;

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
        public string Context;
        [DataMember]
        public int ContextLevel;
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
    /// </summary>FF
    /// <remarks>
    /// There is only one trace log per machine. If the trace log is NOT being read, not tracing
    /// occurs. Reading of the trace log can be done with the 
    /// <see cref="B1.LoggingManagement.MemoryMappedLogReader">MemoryMappedLogReader</see> class. There is a 
    /// TraceViewer windows application that provides user interface for viewing the trace log.
    /// </remarks>
    public class TraceLog : ILoggingTarget
    {   
        private static Lazy<ConcurrentQueue<TraceMessage>> _traceQueue = new Lazy<ConcurrentQueue<TraceMessage>>(
                LazyThreadSafetyMode.ExecutionAndPublication);

        private static Lazy<Task> _traceWriterThread = new Lazy<Task>(
                new Func<Task>( () => 
                    {
                        Task thread = new Task(QueueThread, TaskCreationOptions.LongRunning);
                        thread.Start();
                        return thread;
                    }), LazyThreadSafetyMode.ExecutionAndPublication);

        private static bool _shutdown = false;

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
            if (!MemoryMappedLog.TraceWriter.BeingRead)
                return;

            while (_traceWriterThread.Value.Status != TaskStatus.Running)
                Thread.Sleep(200);

            _traceQueue.Value.Enqueue(new TraceMessage()
                    {
                        Time = DateTime.Now.ToString("HH:mm:ss.fffffff"),
                        Context = LoggingContext.ContextString,
                        ContextLevel = LoggingContext.Level,
                        MachineName = Environment.MachineName,
                        ProcessId = Process.GetCurrentProcess().Id,
                        ProcessName = Process.GetCurrentProcess().ProcessName,
                        ThreadId = Thread.CurrentThread.ManagedThreadId,
                        Message = message
                    });
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

        private static void QueueThread()
        {
            while(!_shutdown)
            {
                if(!MemoryMappedLog.TraceWriter.BeingRead)
                {
                    Thread.Sleep(500);
                    continue;
                }
                
                TraceMessage msg;

                while(_traceQueue.Value.TryDequeue(out msg))
                {
                    if(_shutdown)
                        return;

                    if(!MemoryMappedLog.TraceWriter.BeingRead)
                        continue;
                                         
                    MemoryMappedLog.TraceWriter.Write(GetTraceBytes(msg));

                }

                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~TraceLog()
        {
            _shutdown = true;
        }

    }
}
