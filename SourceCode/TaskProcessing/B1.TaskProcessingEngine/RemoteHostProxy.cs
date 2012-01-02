using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Text;

using B1.LoggingManagement;

namespace B1.TaskProcessing
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RemoteEngineHost" in both code and config file together.

    /// <summary>
    /// Proxy class which handles WCF requests for a Task Processing Engine (TPE)
    /// The class generates Trace messages about the incoming requests and then
    /// makes the method calls on the TPE and wraps the response in a RemoteHostResponse class.
    /// </summary>
    public class RemoteHostProxy : IRemoteHostTPE
    {
        static TaskProcessingEngine _localTPE = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public RemoteHostProxy()
        {
            // Instantiates TPE using a default internal constructor.
            // This constructor does not create any worker threads nor
            // does it construct an instance of the TPE to process tasks.
            // Instead, it just provides access to the static settings
            // of the host that is receiving the requests
            // so that they can be returned or updated
            if (_localTPE == null)
                _localTPE = new TaskProcessingEngine();
        }

        /// <summary>
        /// Stores client Id after validation by the TPE.
        /// </summary>
        /// <param name="remoteClientId">Unique identifier string of remote client application</param>
        /// <returns>Success or Failure response</returns>
        public RemoteHostResponse Connect(string remoteClientId)
        {
            using (LoggingContext lc = new LoggingContext("Proxy: Task Processing Engine: " + _localTPE.EngineId))
            {
                OperationContext context = OperationContext.Current;
                MessageProperties properties = context.IncomingMessageProperties;
                RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                string clientConnection = string.Format("{0}:{1}:{2} Connected.", endpoint.Address, endpoint.Port, remoteClientId);
                _localTPE.Trace(clientConnection, ILoggingManagement.enumTraceLevel.Level3);
                _localTPE.Connect(remoteClientId);
            }
            return new RemoteHostResponse(true, null);
        }

        /// <summary>
        /// Informs host that client will no longer be monitoring it.
        /// </summary>
        /// <param name="remoteClientId">Unique identifier string of remote client application</param>
        /// <returns>Success or Failure response</returns>
        public RemoteHostResponse Disconnect(string remoteClientId)
        {
            using (LoggingContext lc = new LoggingContext("Proxy: Task Processing Engine: " + _localTPE.EngineId))
            {
                OperationContext context = OperationContext.Current;
                MessageProperties properties = context.IncomingMessageProperties;
                RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                string clientConnection = string.Format("{0}:{1}:{2} Disconnected.", endpoint.Address, endpoint.Port, remoteClientId);
                _localTPE.Trace(clientConnection, ILoggingManagement.enumTraceLevel.Level3);
                _localTPE.Disconnect(remoteClientId);
            }
            return new RemoteHostResponse(true, null);
        }

        /// <summary>
        /// Serializes the configuration settings the host used at startup
        /// </summary>
        /// <returns>Success or Failure response along with serialized string of configSettings</returns>
        public RemoteHostResponseString ConfigSettings()
        {
            string serializedSettings = Core.Functions.Serialize<Dictionary<string,string>>(_localTPE.ConfigSettings());
            return new RemoteHostResponseString(true, null, serializedSettings);
        }

        /// <summary>
        /// Returns the serialized list of client applications currently connected to the host
        /// </summary>
        /// <returns>Success or Failure response along with serialized string of clients that are connected to the host</returns>
        public RemoteHostResponseString RemoteClients()
        {
            string serializedSettings = Core.Functions.Serialize<Dictionary<string, string>>(_localTPE.RemoteClients());
            return new RemoteHostResponseString(true, null, serializedSettings);
        }

        /// <summary>
        /// Requests the current settings the host is using at that moment
        /// </summary>
        /// <returns>Success or Failure response along with serialized string of runtime settings</returns>
        public RemoteHostResponseString DynamicSettings()
        {
            string serializedSettings = Core.Functions.Serialize<Dictionary<string, string>>(_localTPE.DynamicSettings());
            return new RemoteHostResponseString(true, null, serializedSettings);
        }

        /// <summary>
        /// Sets the trace level configuration on the host
        /// </summary>
        /// <param name="clientId">Unique identifier string of remote client application</param>
        /// <param name="traceLevel">The new trace level to use</param>
        /// <returns>Success or Failure response</returns>
        public RemoteHostResponse SetTraceLevel(string remoteClientId, string traceLevel)
        {
            using (LoggingContext lc = new LoggingContext("Proxy: Task Processing Engine: " + _localTPE.EngineId))
            {
                OperationContext context = OperationContext.Current;
                MessageProperties properties = context.IncomingMessageProperties;
                RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                string clientConnection = string.Format("{0}:{1}:{2} Set TraceLevel; {3}"
                        , endpoint.Address, endpoint.Port, remoteClientId, traceLevel);
                _localTPE.Trace(clientConnection, ILoggingManagement.enumTraceLevel.Level3);
                _localTPE.SetTraceLevel(remoteClientId, traceLevel);
            }
            return new RemoteHostResponse(true, null);
        }

        /// <summary>
        /// Changes the maximum number of concurrent tasks that can be processed by the host
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <param name="delta">The change in the number of tasks (e.g. +1, -1, etc)</param>
        /// <returns>Success or Failure response.  On success, the new max is provided in the 
        /// returnValue</returns>
        public RemoteHostResponseInt SetMaxTasks(string remoteClientId, int delta)
        {
            using (LoggingContext lc = new LoggingContext("Proxy: Task Processing Engine: " + _localTPE.EngineId))
            {
                OperationContext context = OperationContext.Current;
                MessageProperties properties = context.IncomingMessageProperties;
                RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                string clientConnection = string.Format("{0}:{1}:{2} SetMaxTasks; delta {3}."
                        , endpoint.Address, endpoint.Port, remoteClientId, delta);
                _localTPE.Trace(clientConnection, ILoggingManagement.enumTraceLevel.Level3);
                int result = _localTPE.SetMaxTasks(remoteClientId, delta);
                return new RemoteHostResponseInt(true, null, result);
            }
        }

        /// <summary>
        /// Causes the host to temporarily suspend processing operations 
        /// </summary>
        /// <param name="clientId">Unique identifier string of remote client application</param>
        /// <returns>Success or Failure response</returns>
        public RemoteHostResponse Pause(string remoteClientId)
        {
            using (LoggingContext lc = new LoggingContext("Proxy: Task Processing Engine: " + _localTPE.EngineId))
            {
                OperationContext context = OperationContext.Current;
                MessageProperties properties = context.IncomingMessageProperties;
                RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                string clientConnection = string.Format("{0}:{1}:{2} Paused.", endpoint.Address, endpoint.Port, remoteClientId);
                _localTPE.Trace(clientConnection, ILoggingManagement.enumTraceLevel.Level3);
                _localTPE.Pause(remoteClientId);
            }
            return new RemoteHostResponse(true, null);
        }

        /// <summary>
        /// Causes the host to resume processing
        /// </summary>
        /// <param name="clientId">Unique identifier string of remote client application</param>
        /// <returns>Success or Failure response</returns>
        public RemoteHostResponse Resume(string remoteClientId)
        {
            using (LoggingContext lc = new LoggingContext("Proxy: Task Processing Engine: " + _localTPE.EngineId))
            {
                OperationContext context = OperationContext.Current;
                MessageProperties properties = context.IncomingMessageProperties;
                RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                string clientConnection = string.Format("{0}:{1}:{2} Resumed.", endpoint.Address, endpoint.Port, remoteClientId);
                _localTPE.Trace(clientConnection, ILoggingManagement.enumTraceLevel.Level3);
                _localTPE.Resume(remoteClientId);
            }
            return new RemoteHostResponse(true, null);
        }

        /// <summary>
        /// Requests a status message from the host
        /// </summary>
        /// <param name="clientId">Unique identifier string of remote client application</param>
        /// <returns>Success or Failure response.  On success, a serialized version of the 
        /// runtime settings is provided in the returnValue</returns>
        public RemoteHostResponseString Status(string remoteClientId)
        {
            string status = _localTPE.Status(remoteClientId);
            return new RemoteHostResponseString(true, null, status);
        }

    }
}
