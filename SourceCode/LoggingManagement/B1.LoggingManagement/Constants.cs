using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.LoggingManagement
{
    class Constants
    {
        internal const string EventLogSource = "EventLogSource";
        internal const string MSMQPath = "MSMQPath";
        internal const string LogFileDirectory = "LogFileDirectory";
        internal const string LogFileNamePrefix = "LogFileNamePrefix";
        internal const string LogFileSize = "LogFileSize";

        //Actual windows event log has max message size of 32766 bytes(not string length). We take away 1000 bytes for 
        //extra header information we might add such as event reference and continuation msg.
        internal const int MaxMessageSize = 31766;
    }
}
