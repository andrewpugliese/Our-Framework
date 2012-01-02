using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace B1.TaskProcessing
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRemoteEngineHost" in both code and config file together.

    /// <summary>
    /// Response type for a void return type of a WCF interface to a Task Processsing Engine
    /// </summary>
    public struct RemoteHostResponse
    {
        public RemoteHostResponse(bool success = true, string errorMsg = null)
        {
            Success = success;
            ErrorMsg = errorMsg;
        }
        public bool Success;
        public string ErrorMsg;
    }

    /// <summary>
    /// Response type for a string return type of a WCF interface to a Task Processsing Engine
    /// </summary>
    public struct RemoteHostResponseString
    {
        public RemoteHostResponseString(bool success = true
            , string errorMsg = null
            , string returnValue = null)
        {
            Success = success;
            ErrorMsg = errorMsg;
            ReturnValue = returnValue;
        }

        public bool Success;
        public string ErrorMsg;
        public string ReturnValue;
    }

    /// <summary>
    /// Response object for an Int return type of a WCF interface to a Task Processsing Engine
    /// </summary>
    public struct RemoteHostResponseInt
    {
        public RemoteHostResponseInt(bool success = true
            , string errorMsg = null
            , int returnValue = 0)
        {
            Success = success;
            ErrorMsg = errorMsg;
            ReturnValue = returnValue;
        }
        public bool Success;
        public string ErrorMsg;
        public int ReturnValue;
    }

    /// <summary>
    /// Interface supported by a Task Processing Engine Proxy class to facilitated WCF
    /// Every method returns a response type so that it can address any communication
    /// errors.
    /// All methods that will change the state or setting of a TPE host require a string, clietId,
    /// which is a unique identifier for that client application.
    /// </summary>
    [ServiceContract]
    public interface IRemoteHostTPE
    {
        /// <summary>
        /// Establishes communication with remote host TPE to monitor or change settings.
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <returns>Success or Failure response</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        RemoteHostResponse Connect(string clientId);

        /// <summary>
        /// Informs remote host that it will no longer be monitoring it.
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <returns>Success or Failure response</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        RemoteHostResponse Disconnect(string clientId);

        /// <summary>
        /// Causes the remote host to temporarily suspend processing operations 
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <returns>Success or Failure response</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        RemoteHostResponse Pause(string clientId);

        /// <summary>
        /// Causes the remote host to resume processing
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <returns>Success or Failure response</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        RemoteHostResponse Resume(string clientId);

        /// <summary>
        /// Requests a status message from remote host
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <returns>Success or Failure response.  On success, a serialized version of the 
        /// runtime settings is provided in the returnValue</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        RemoteHostResponseString Status(string clientId);

        /// <summary>
        /// Changes the maximum number of concurrent tasks that can be processed
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <param name="delta">The change in the number of tasks (e.g. +1, -1, etc)</param>
        /// <returns>Success or Failure response.  On success, the new max is provided in the 
        /// returnValue</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        RemoteHostResponseInt SetMaxTasks(string clientId, int delta);

        /// <summary>
        /// Sets the trace level configuration on the remote host
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <param name="traceLevel">The new trace level to use</param>
        /// <returns>Success or Failure response</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        RemoteHostResponse SetTraceLevel(string clientId, string traceLevel);

        /// <summary>
        /// Requests the configuration settings the host used at startup
        /// </summary>
        /// <returns>Success or Failure response along with serialized string of configSettings</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        RemoteHostResponseString ConfigSettings();

        /// <summary>
        /// Requests the current settings the host is using at that moment
        /// </summary>
        /// <returns>Success or Failure response along with serialized string of runtime settings</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        RemoteHostResponseString DynamicSettings();

        /// <summary>
        /// Returns the list of client applications currently connected to the remote host
        /// </summary>
        /// <returns>Success or Failure response along with serialized string of clients that are connected to the remote host</returns>
        [OperationContract]
        [FaultContract(typeof(Exception))]
        RemoteHostResponseString RemoteClients();
    }
}