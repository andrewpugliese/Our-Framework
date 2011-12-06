using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace B1.TaskProcessing
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRemoteEngineHost" in both code and config file together.
    [ServiceContract]
    public interface IRemoteEngineHost
    {
        [OperationContract]
        void Connect(string clientId);
        [OperationContract]
        void Disconnect(string clientId);
        [OperationContract]
        void Pause(string clientId);
        [OperationContract]
        void Resume(string clientId);
        [OperationContract]
        string Status(string clientId);
        [OperationContract]
        int SetMaxTasks(string clientId, int delta);
        [OperationContract]
        void SetTraceLevel(string clientId, string traceLevel);
        [OperationContract]
        string ConfigSettings();
        [OperationContract]
        string DynamicSettings();
        [OperationContract]
        string RemoteClients();
    }
}