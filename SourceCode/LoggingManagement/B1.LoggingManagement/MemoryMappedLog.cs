using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;

namespace B1.LoggingManagement
{
    /// <summary>
    /// 
    /// Base class for memory mapped log file.
    /// 
    /// The memory mapped log is made up of two memory mapped files: an index file and the actual log file.
    /// 
    /// </summary> 
    /// <remarks>
    ///
    /// <para>
    /// Index File
    /// </para>
    /// <para>
    /// The index file is a memory mapped file that is NOT persisted to disk. It is a small file that contains
    /// information about the log file. It also contains status and information for coordinating reads and writes.
    /// </para>
    /// <para>
    /// Index File Format
    /// </para>
    ///  <list type='table'>
    ///  <listheader>
    ///     <term>Offset(s)</term>
    ///     <term>Name</term>
    ///     <term>Discription</term>
    ///  </listheader>
    ///  <item>
    ///     <term>0</term>
    ///     <term>Being Read</term>
    ///     <term>Is the traceviewer application running and reading the log. 
    ///     Used by writer to determine weather to write to log or not.
    ///     0 for not being read, non zero for being read.</term>
    ///  </item>
    ///  <item>
    ///     <term>1 - 4</term>
    ///     <term>Current Write Offset</term>
    ///     <term>The next offset, in the log file, available for writing.</term>
    ///  </item>
    ///  <item>
    ///     <term>5 - 8</term>
    ///     <term>Number of Messages</term>
    ///     <term>The number of messages written to log file.</term>
    ///  </item>
    ///  <item>
    ///     <term>9 - 12</term>
    ///     <term>Next Read Offset</term>
    ///     <term>The next offset, in the log file, to read. Used by reader.</term>
    ///  </item>
    ///  <item>
    ///     <term>13</term>
    ///     <term>Rolled Over </term>
    ///     <term>The log reader has rolled over (0 or non zero). Set by writer. 
    ///           Reset to zero after first read.</term>
    ///  </item>
    ///  <item>
    ///     <term></term>
    ///     <term></term>
    ///     <term></term>
    ///  </item>
    /// </list>
    /// <para>
    /// Log File 
    /// </para>
    /// 
    /// <para>
    /// The log file is a persisted memory mapped file that is created by the reader. It can be thought of as a
    /// doubly-linked list for reading forward and backwards. When being read by a MemoryMappedReader(when _outputForReader is true) 
    /// the format is simply a 4 byte size of message, the message, and this 4 byte size again(for reading in reverse). 
    /// Messages are stored as UTF-8 byte blobs. When NOT being read by a MemoryMappedReader(when _outputForReader is false), 
    /// there is no format. Messages are just simply written byte per byte, one message after another. 
    /// </para>
    /// 
    /// <para>Example of log file containing 2 messages (when _outputForReader is true):</para>
    /// <list type='table'>
    ///  <listheader>
    ///     <term>Offset(s)</term>
    ///     <term>Data (hex)</term>
    ///     <term>Meaning of Data</term>
    ///  </listheader>
    ///  <item>
    ///     <term>0-3</term>
    ///     <term>00000004</term>
    ///     <term>Message size of 4 bytes</term>
    ///  </item>
    ///  <item>
    ///     <term>4-7</term>
    ///     <term>74657374</term>
    ///     <term>Message data: "test"</term>
    ///  </item>
    ///  <item>
    ///     <term>8-11</term>
    ///     <term>00000004</term>
    ///     <term>Message size of 4 bytes</term>
    ///  </item>
    ///  <item>
    ///     <term>12-15</term>
    ///     <term>00000005</term>
    ///     <term>Message size of 5 bytes</term>
    ///  </item>
    ///  <item>
    ///     <term>16-20</term>
    ///     <term>7465737432</term>
    ///     <term>Message data: "test2"</term>
    ///  </item>
    ///  <item>
    ///     <term>21-24</term>
    ///     <term>00000005</term>
    ///     <term>Message Size of 5 bytes</term>
    ///  </item>
    /// </list>
    /// 
    ///</remarks>
    public abstract class MemoryMappedLog : IDisposable
    {
        /// <summary>
        /// Name of memory mapped file and log
        /// </summary>
        public const string TraceLogName = "LoggingManagement.TraceLog";
        /// <summary>
        /// Name of memory mapped file and log
        /// </summary>
        public const string EventLogName = "LoggingManagement.EventLog";

 #pragma warning disable 1591 // disable the xmlComments warning
        public const int MaxMessageSizeBytes = 1024 * 1024;       
        public const int RolloverMarker = -1;

        // Index block sizes
        protected const int Size_IsBeingRead = 1;
        protected const int Size_CurrentWriteOffset = sizeof(int);
        protected const int Size_NumMessages = sizeof(int);
        protected const int Size_NextReadOffset = sizeof(int);
        protected const int Size_RolledOver = 1;
        protected const int Size_Header = Size_IsBeingRead + Size_CurrentWriteOffset + Size_NumMessages + Size_NextReadOffset
                + Size_RolledOver;

        // index block positions
        protected const int FilePos_IsBeingRead = 0;
        protected const int FilePos_CurrentWriteOffset = FilePos_IsBeingRead + Size_IsBeingRead;
        protected const int FilePos_NumMessages = FilePos_CurrentWriteOffset + Size_CurrentWriteOffset;
        protected const int FilePos_NextReadOffset = FilePos_NumMessages + Size_NumMessages;
        protected const int FilePos_RolledOver = FilePos_NextReadOffset + Size_NextReadOffset;

        protected MemoryMappedFile _indexFile;
        protected MemoryMappedFile _logFile;
        protected MemoryMappedViewAccessor _headerAccessor = null;
        protected Mutex _lock;

        protected long _startingFileSizeBytes = MaxMessageSizeBytes * 10;
        protected long _logFileSizeBytes = MaxMessageSizeBytes * 10;
        protected long _logFileIncreaseSize = MaxMessageSizeBytes * 10;
        protected string _logFilePath;


        public string _indexName;
        public string _logName;
        
        protected const string WriterMutexName = "TraceAndEventLogWriterMutex";
#pragma warning restore 1591 // restore the xmlComments warning
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logName">Unique log name. If 2 or more processes use this file name</param>
        /// <param name="logFileName">Used to create memory mapped file name and associated persisted file name.</param>
        /// <param name="logFileDirectory"></param>
        protected MemoryMappedLog(string logName, string logFileName, string logFileDirectory)
        {
           _logFilePath = logFileDirectory + "\\" + logFileName;
           _logName = logName;
            _indexName = _logName + "_Index";

           _lock = new Mutex(false, _logName + "_Mutex");
                        
           _indexFile = MemoryMappedFile.CreateOrOpen(_indexName.Replace('\\', ':'), Size_Header);

           _headerAccessor = _indexFile.CreateViewAccessor(0, Size_Header);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logName"></param>
        protected MemoryMappedLog(string logName)
        {
           _logName = logName + ".log";
           _indexName = logName + "_Index";

           _lock = new Mutex(false, WriterMutexName);

           _logFilePath = Environment.GetEnvironmentVariable("TEMP") + "\\" + _logName;
                        
           _indexFile = MemoryMappedFile.CreateOrOpen(_indexName, Size_Header);

           _headerAccessor = _indexFile.CreateViewAccessor(0, Size_Header);
        }
        
        /// <summary>
        /// Is there a reader currently reading this log
        /// </summary>
        public virtual bool BeingRead 
        {
            get
            {       
                return _headerAccessor.ReadByte(FilePos_IsBeingRead) > 0;
            }
            protected set
            {
                _headerAccessor.Write(FilePos_IsBeingRead, value ? (byte)1 : (byte)0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected bool _outputForReader = true;

        /// <summary>
        /// The next place to start writing
        /// </summary>
        protected int CurrentWriteOffset
        {
            get
            {
                return _headerAccessor.ReadInt32(FilePos_CurrentWriteOffset);
            }
            set
            {
                _headerAccessor.Write(FilePos_CurrentWriteOffset, value);
            }
        }

        /// <summary>
        /// Has there been a rollover since the last read. A read, resets this.
        /// </summary>
        protected bool RolledOver
        {
            get
            {
                return _headerAccessor.ReadByte(FilePos_RolledOver) > 0;
            }
            set
            {
                _headerAccessor.Write(FilePos_RolledOver, value ? (byte)1 : (byte)0);
            }
        }

        /// <summary>
        /// Number of messages written to log. A rollover resets this to zero.
        /// </summary>
        public int NumMessages
        {
            get
            {
                return _headerAccessor.ReadInt32(FilePos_NumMessages);
            }
            protected set
            {
                _headerAccessor.Write(FilePos_NumMessages, value);
            }
        }

        /// <summary>
        /// Next place to read. This location is moved by readers and writers.
        /// </summary>
        public int NextReadOffset
        {
            get
            {
                return _headerAccessor.ReadInt32(FilePos_NextReadOffset);
            }
            protected set
            {
                _headerAccessor.Write(FilePos_NextReadOffset, value);
            }
        }

        private readonly static object _eventReaderLock = new object();
        private readonly static object _eventWriterLock = new object();
        private readonly static object _traceReaderLock = new object();
        private readonly static object _traceWriterLock = new object();

        private static Lazy<MemoryMappedLogReader> _eventReader = new Lazy<MemoryMappedLogReader>(
                () => new MemoryMappedLogReader(EventLogName, MemoryMappedLog.MaxMessageSizeBytes * 500),
                LazyThreadSafetyMode.ExecutionAndPublication);

        private static Lazy<MemoryMappedLogWriter> _eventWriter = new Lazy<MemoryMappedLogWriter>(
                () => new MemoryMappedLogWriter(EventLogName),
                LazyThreadSafetyMode.ExecutionAndPublication);

        private static Lazy<MemoryMappedLogReader> _traceReader = new Lazy<MemoryMappedLogReader>(
                () => new MemoryMappedLogReader(TraceLogName, MemoryMappedLog.MaxMessageSizeBytes * 500),
                LazyThreadSafetyMode.ExecutionAndPublication);

        private static Lazy<MemoryMappedLogWriter> _traceWriter = new Lazy<MemoryMappedLogWriter>(
                () => new MemoryMappedLogWriter(TraceLogName),
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets MemoryMappedLogReader for reading normal log messages
        /// </summary>
        public static MemoryMappedLogReader EventReader
        {
            get 
            {
                return _eventReader.Value;
            }
        }

        /// <summary>
        /// Gets MemoryMappedLogReader for writing normal log messages
        /// </summary>
        public static MemoryMappedLogWriter EventWriter
        {
            get 
            {
                return _eventWriter.Value;
            }
        }

        /// <summary>
        /// Single instance of MemoryMappedLogReader class for reading trace log. Lazily initialized the first time
        /// this property is used.
        /// </summary>
        public static MemoryMappedLogReader TraceReader
        {
            get 
            {
                return _traceReader.Value;
            }
        }

        /// <summary>
        /// Single instance of MemoryMappedLogReader class for writing to trace log. Lazily initialized the first time
        /// this property is used.
        /// </summary>
        public static MemoryMappedLogWriter TraceWriter
        {
            get 
            {
                return _traceWriter.Value;
            }
        }
        
        /// <summary>
        /// Next offset for writing.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentWriteOffset()
        {
            _lock.WaitOne();
            try
            {
                return CurrentWriteOffset;
            }
            finally
            {
                _lock.ReleaseMutex();
            }
        }
                
        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            if(_logFile != null)
                _logFile.Dispose();

            if(_indexFile != null)
                _indexFile.Dispose();
        }
    }

    /// <summary>
    /// <para>
    /// Class used for writing to memory mapped log file. 
    /// </para>
    /// </summary>
    /// <remarks>
    /// The _outputForReader member determines both the format of the log file and when writing takes place. If _outputForReader is true,
    /// writing will not happen until a MemoryMappedLogReader is created with the same log name. Also, the format of the log file 
    /// will be changed to support the reading by the MemoryMappedLogReader. 
    /// See: <see cref="B1.LoggingManagement.MemoryMappedLog">MemoryMappedLog</see> class.
    /// 
    /// </remarks>
    public class MemoryMappedLogWriter : MemoryMappedLog
    {
        private string _logFileDirectory;
        private string _logFileNamePrefix;
        private string _logFileNameExtension;

        /// <summary>
        /// Creates an instance of MemoryMappedLogWriter for consumption by MemoryMappedLogReader. Writing will not happen
        /// until a MemoryMappedLogReader is created with the same log name.
        /// </summary>
        /// <param name="logName">Name of log. Used to create memory mapped file</param>
        public MemoryMappedLogWriter(string logName) : base(logName)
        {
        }

        /// <summary>
        /// Creates an instance of MemoryMappedLogWriter. If outputForReader is true, writing will not happen
        /// until a MemoryMappedLogReader is created with the same log name. 
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="logFileNamePrefix"></param>
        /// <param name="logFileNameExtension"></param>
        /// <param name="logFileDirectory"></param>
        /// <param name="outputForReader"></param>
        /// <param name="logFileSizeBytes"></param>
        public MemoryMappedLogWriter(string logName, string logFileNamePrefix, string logFileNameExtension, 
                string logFileDirectory, bool outputForReader, int logFileSizeBytes) 
                : base(logName, GenerateUniqueLogFileName(logFileNamePrefix, logFileNameExtension, logFileDirectory) 
                    , logFileDirectory)
        {   
            _outputForReader = outputForReader;
            _logFileSizeBytes = logFileSizeBytes;
            _logFileDirectory = logFileDirectory;
            _logFileNamePrefix = logFileNamePrefix;
            _logFileNameExtension = logFileNameExtension;

            if(!outputForReader)
            {
                try
                {
                    _lock.WaitOne();
                    //This will be used when supporting multiple processes using same log file
                    //_logFile = MemoryMappedFile.OpenExisting(_logName, MemoryMappedFileRights.Write);
                    ChangeToNextLogFile();
                }
                //This will be used when supporting multiple processes using same log file
                //catch(FileNotFoundException)
                //{
                //    ChangeToNextLogFile();
                //}
                finally
                {
                    _lock.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Writes message to log at CurrentWriteOffset. Increments CurrentWriteOffset when done.
        /// </summary>
        /// <param name="message">Message to write. String is stored as UTF-8 byte blob.</param>
        public void Write(string message)
        {
            if(_outputForReader && BeingRead == false)
                return;

            byte[] bytes = Encoding.UTF8.GetBytes(message);

            Write(bytes);
        }

        /// <summary>
        /// Writes message to log at CurrentWriteOffset. Increments CurrentWriteOffset when done.
        /// </summary>
        /// <param name="bytes">Message to write</param>
        public void Write(byte[] bytes)
        {   
            if(_outputForReader && BeingRead == false)
                return;

            int offset = 0;

            try
            {
                _lock.WaitOne();
                
                if(_logFile == null)
                    _logFile = MemoryMappedFile.OpenExisting(_logName);
            }
            catch(AbandonedMutexException) // Reader crashed
            {
                _lock.ReleaseMutex();
                return;
            }
            catch(Exception)
            {
                _lock.ReleaseMutex();
                throw;
            }

            try
            {
                // If not outputting for reader(tracing) and no more room in log file.
                if(!_outputForReader && CurrentWriteOffset + bytes.Length >= _logFileSizeBytes)
                    ChangeToNextLogFile();

                offset = CurrentWriteOffset;

                int sizeBytesCount = _outputForReader ? (sizeof(int) * 2) : 0;

                if(_outputForReader && offset + bytes.Length + sizeBytesCount >= _logFileSizeBytes)
                {
                        using(MemoryMappedViewAccessor newBlockAccessor = _logFile.CreateViewAccessor(
                            offset, sizeof(int)))
                        {
                            RolledOver = true;
                            newBlockAccessor.Write(0, RolloverMarker);
                            NumMessages = 0;
                            NextReadOffset = 0;
                        }
                 
                    offset = 0;
                }

                using(MemoryMappedViewAccessor newBlockAccessor = _logFile.CreateViewAccessor(
                        offset, bytes.Length + sizeBytesCount))
                {
                    newBlockAccessor.WriteArray(_outputForReader ? sizeof(int) : 0, bytes, 0, bytes.Length); // write block

                    if(_outputForReader)
                    {
                        newBlockAccessor.Write(bytes.Length + sizeof(int), bytes.Length); // size of this block at end
                        newBlockAccessor.Write((int)0, bytes.Length); // write size of this block at beginning
                    };
                }

                CurrentWriteOffset = offset + bytes.Length + sizeBytesCount;

                NumMessages++;
            }
            finally
            {
                _lock.ReleaseMutex();
            }

            return;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ChangeToNextLogFile()
        {
            try
            {
                _lock.WaitOne();

                _logFilePath = _logFileDirectory + "\\" + GenerateUniqueLogFileName(_logFileNamePrefix,
                        _logFileNameExtension, _logFileDirectory);

                if(_logFile != null)
                    _logFile.Dispose();

                _logFile = CreateLogFile(_logFilePath);
                
            }
            finally
            {
                _lock.ReleaseMutex();
            }
        }

        /// <summary>
        /// Will generate a filename based on logFileNamePrefix and date. If file already exists, will append
        /// "_N" to file where N is a number to make it unique within directory.
        /// </summary>
        /// <param name="logFileNamePrefix"></param>
        /// <param name="logFileNameExtesion"></param>
        /// <param name="logFileDirectory"></param>
        /// <returns></returns>
        public static string GenerateUniqueLogFileName(string logFileNamePrefix, string logFileNameExtesion, 
                string logFileDirectory)
        {
            DirectoryInfo dir = new DirectoryInfo(logFileDirectory);

            int n = 1;
            string uniqueFileName = logFileNamePrefix;

            while(dir.EnumerateFiles(uniqueFileName + "*").Count() > 0)
            {
                uniqueFileName = logFileNamePrefix + "_" + n.ToString();
                n++;
            }

            return uniqueFileName + "." + logFileNameExtesion;
        }

        /// <summary>
        /// 
        /// </summary>
        protected MemoryMappedFile CreateLogFile(string logFilePath)
        {
            using(FileStream fstream = new FileStream(_logFilePath, FileMode.CreateNew, FileAccess.ReadWrite, 
                    FileShare.ReadWrite))
            {
                CurrentWriteOffset = 0;

                return MemoryMappedFile.CreateFromFile(fstream, _logName, _logFileSizeBytes,
                        MemoryMappedFileAccess.ReadWrite, null, 
                        HandleInheritability.Inheritable, false);                                   
            }
        }

        /// <summary>
        /// Disposes of the object and removes file if empty.
        /// </summary>
        public override void Dispose()
        {
            int currentWriteOffset = CurrentWriteOffset;

            base.Dispose();

            if(currentWriteOffset == 0)
                File.Delete(_logFilePath);
        }
    }

    /// <summary>
    /// Class used for reading from a memory mapped log.
    /// </summary>
    public class MemoryMappedLogReader : MemoryMappedLog
    {       
        private byte[] _messageBuffer = new byte[MaxMessageSizeBytes];
        
        /// <summary>
        /// Creates an instance of MemoryMappedLogReader
        /// </summary>
        /// <param name="logName">Name of log. Used to create memory mapped file</param>
        /// <param name="logSize">Size of log in bytes.</param>
        public MemoryMappedLogReader(string logName, long logSize) : base(logName)
        {
            _logFileSizeBytes = logSize;
            InitializeMemoryMappedLogReader();
        }

        /// <summary>
        /// Read messages moving forward in log file.
        /// </summary>
        /// <param name="numMessages">Number of messages to try to read.</param>
        /// <param name="startingOffset">Optional: Offset to begin reading</param>
        /// <returns>List of messages. Count may be less than or equal to numMessages</returns>
        public List<string> ReadNextMessages(int numMessages, int? startingOffset = null)
        {
            List<string> messages = new List<string>();

            string message = ReadNextMessage(startingOffset);
            while(message != null && messages.Count < numMessages)
            {
                messages.Add(message);
                if(messages.Count < numMessages)
                    message = ReadNextMessage();
            }

            return messages;
        }

        /// <summary>
        /// Read next message moving forward in log file.
        /// </summary>
        /// <param name="startingOffset">Optional: Offset to begin reading</param>
        /// <returns>Next message or null if no next message.</returns>
        public string ReadNextMessage(int? startingOffset = null)
        {   
            MemoryMappedViewAccessor currentMessageAccessor = null;

            try
            {
                _lock.WaitOne();

                // Reset Rollover
                if(RolledOver)
                {
                    RolledOver = false;
                    NextReadOffset = 0;
                }
                else if(startingOffset != null)
                    NextReadOffset = startingOffset.Value;
                
                if(NextReadOffset >= CurrentWriteOffset) //we are at end
                    return null;

                currentMessageAccessor = _logFile.CreateViewAccessor(NextReadOffset, 
                        Math.Min(_logFileSizeBytes - NextReadOffset - 1, 
                        MaxMessageSizeBytes + sizeof(int)));

                int messageSize = currentMessageAccessor.ReadInt32(0);

                if(messageSize == RolloverMarker)
                {
                    currentMessageAccessor.Dispose();

                    NextReadOffset = 0;

                    currentMessageAccessor =_logFile.CreateViewAccessor(NextReadOffset, 
                        MaxMessageSizeBytes + sizeof(int));

                    messageSize = currentMessageAccessor.ReadInt32(0);
                }
                else if(messageSize == 0)
                    return null;

                currentMessageAccessor.ReadArray(sizeof(int), _messageBuffer, 0, messageSize);

                NextReadOffset += messageSize + (sizeof(int) * 2);

                return Encoding.UTF8.GetString(_messageBuffer, 0, messageSize);
            }
            finally
            {
                _lock.ReleaseMutex();
                if(currentMessageAccessor != null)
                    currentMessageAccessor.Dispose();
            }
        }

        /// <summary>
        /// Read messages moving backwords in log file.
        /// </summary>
        /// <param name="numMessages">Number of messages to try to read.</param>
        /// <param name="startingOffset">Optional: Offset to begin reading</param>
        /// <returns>List of messages. Count may be less than or equal to numMessages</returns>
        public List<string> ReadPreviousMessages(int numMessages, int? startingOffset = null)
        {
            List<string> messages = new List<string>();

            string message = ReadPreviousMessage(startingOffset);
            while(message != null && messages.Count < numMessages)
            {
                messages.Insert(0, message);
                if(messages.Count < numMessages)
                    message = ReadPreviousMessage();
            }

            return messages;
        }

        /// <summary>
        /// Read next message moving backword in log file.
        /// </summary>
        /// <param name="startingOffset">Optional: Offset to begin reading</param>
        /// <returns>Previous message or null if no previous message.</returns>
        public string ReadPreviousMessage(int? startingOffset = null)
        {   
            MemoryMappedViewAccessor currentMessageAccessor = null;

            try
            {
                _lock.WaitOne();

                if(startingOffset != null)
                    NextReadOffset = startingOffset.Value;

                if(NextReadOffset == 0 || NextReadOffset > CurrentWriteOffset)
                    return null;

                int messageSize = 0;

                using(currentMessageAccessor = _logFile.CreateViewAccessor(NextReadOffset - sizeof(int), 
                        sizeof(int)))
                {
                    // Read previous message size
                    messageSize = currentMessageAccessor.ReadInt32(0);
                }

                using(currentMessageAccessor = _logFile.CreateViewAccessor(NextReadOffset - sizeof(int) 
                        - messageSize, messageSize))
                {
                    currentMessageAccessor.ReadArray(0, _messageBuffer, 0, messageSize);
                }

                NextReadOffset -=  messageSize + (sizeof(int) * 2);

                return Encoding.UTF8.GetString(_messageBuffer, 0, messageSize);
            }
            finally
            {
                _lock.ReleaseMutex();
                if(currentMessageAccessor != null)
                    currentMessageAccessor.Dispose();
            }
        }

        private void InitializeMemoryMappedLogReader()
        {    
            _lock.WaitOne();
            try
            {
                NextReadOffset = 0;
                CurrentWriteOffset = 0;
                NumMessages = 0;
                BeingRead = true;
                bool fileInUse = false;

                try
                {
                    
                    if(File.Exists(_logFilePath))
                        File.Delete(_logFilePath);
                }
                catch(UnauthorizedAccessException)
                {
                    // Cannot recreate or resize this file
                    fileInUse = true;
                }

                if(!fileInUse)
                {
                    _logFile = MemoryMappedFile.CreateFromFile(_logFilePath, FileMode.OpenOrCreate, _logName,
                        _logFileSizeBytes);
                }
                else
                    _logFile = MemoryMappedFile.OpenExisting(_logName);
                

                using(MemoryMappedViewAccessor currentMessageAccessor = _logFile.CreateViewAccessor(0, 
                    sizeof(int)))
                {
                    currentMessageAccessor.Write(0, (int)0); //write length of zero at first four bytes
                }
            }
            finally
            {
                _lock.ReleaseMutex();
            }

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(
                    (o, e) => BeingRead = false );

            AppDomain.CurrentDomain.DomainUnload += new EventHandler(
                    (o, e) => BeingRead = false );
        }
    }

}
