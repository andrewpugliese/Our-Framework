using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;

namespace B1.TaskProcessing
{
 
    /// <summary>
    /// The class which is responsible for the WCF calls to the remote host Task Processing Engine (TPE)
    /// Since communication can be broken at any call, every method catches a commucation error and returns
    /// a response object class with message.
    /// </summary>
    public partial class RemoteHostClient : System.ServiceModel.ClientBase<IRemoteHostTPE>, IRemoteHostTPE
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RemoteHostClient()
        {
        }

        /// <summary>
        /// Constructor given specific endpoint configuration
        /// </summary>
        /// <param name="endpointConfigurationName">Key to endpoint configuration to lookup in configuration file</param>
        public RemoteHostClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        /// <summary>
        /// Constructor given specific endpoint configuration and remote address string
        /// </summary>
        /// <param name="endpointConfigurationName">Key to endpoint configuration to lookup in configuration file</param>
        /// <param name="remoteAddress">String pointing to remote address</param>
        public RemoteHostClient(string endpointConfigurationName
                , string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        /// <summary>
        /// Constructor given specific endpoint configuration and remote address object
        /// </summary>
        /// <param name="endpointConfigurationName">Key to endpoint configuration to lookup in configuration file</param>
        /// <param name="remoteAddress">Endpoint address object</param>
        public RemoteHostClient(string endpointConfigurationName
                , System.ServiceModel.EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        /// <summary>
        /// Constructor given specific binding and remote address object
        /// </summary>
        /// <param name="binding">Channels Binding Object</param>
        /// <param name="remoteAddress">Endpoint address object</param>
        public RemoteHostClient(System.ServiceModel.Channels.Binding binding
                , System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        /// <summary>
        /// Establishes communication with remote host TPE to monitor or change settings.
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <returns>Success or Failure response</returns>
        public RemoteHostResponse Connect(string clientId)
        {
            try
            {
                return base.Channel.Connect(clientId);
            }
            catch (System.ServiceModel.EndpointNotFoundException enf)
            {
                return new RemoteHostResponse(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(enf));
            }
            catch (Exception exc)
            {
                return new RemoteHostResponse(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(exc));
            }
        }

        /// <summary>
        /// Informs remote host that it will no longer be monitoring it.
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <returns>Success or Failure response</returns>
        public RemoteHostResponse Disconnect(string clientId)
        {
            try
            {
                return base.Channel.Disconnect(clientId);
            }
            catch (System.ServiceModel.EndpointNotFoundException enf)
            {
                return new RemoteHostResponse(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(enf));
            }
            catch (Exception exc)
            {
                return new RemoteHostResponse(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(exc));
            }
        }

        /// <summary>
        /// Requests the configuration settings the host used at startup
        /// </summary>
        /// <returns>Success or Failure response along with serialized string of configSettings</returns>
        public RemoteHostResponseString ConfigSettings()
        {
            try
            {
                return base.Channel.ConfigSettings();
            }
            catch (System.ServiceModel.EndpointNotFoundException enf)
            {
                return new RemoteHostResponseString(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(enf));
            }
            catch (Exception exc)
            {
                return new RemoteHostResponseString(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(exc));
            }
        }

        /// <summary>
        /// Returns the list of client applications currently connected to the remote host
        /// </summary>
        /// <returns>Success or Failure response along with serialized string of clients that are connected to the remote host</returns>
        public RemoteHostResponseString RemoteClients()
        {
            try
            {
                return base.Channel.RemoteClients();
            }
            catch (System.ServiceModel.EndpointNotFoundException enf)
            {
                return new RemoteHostResponseString(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(enf));
            }
            catch (Exception exc)
            {
                return new RemoteHostResponseString(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(exc));
            }
        }

        /// <summary>
        /// Requests the current settings the host is using at that moment
        /// </summary>
        /// <returns>Success or Failure response along with serialized string of runtime settings</returns>
        public RemoteHostResponseString DynamicSettings()
        {
            try
            {
                return base.Channel.DynamicSettings();
            }
            catch (System.ServiceModel.EndpointNotFoundException enf)
            {
                return new RemoteHostResponseString(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(enf));
            }
            catch (Exception exc)
            {
                return new RemoteHostResponseString(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(exc));
            }
        }

        /// <summary>
        /// Changes the maximum number of concurrent tasks that can be processed
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <param name="delta">The change in the number of tasks (e.g. +1, -1, etc)</param>
        /// <returns>Success or Failure response.  On success, the new max is provided in the 
        /// returnValue</returns>
        public RemoteHostResponseInt SetMaxTasks(string clientId, int delta)
        {
            try
            {
                return base.Channel.SetMaxTasks(clientId, delta);
            }
            catch (System.ServiceModel.EndpointNotFoundException enf)
            {
                return new RemoteHostResponseInt(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(enf));
            }
            catch (Exception exc)
            {
                return new RemoteHostResponseInt(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(exc));
            }
        }

        /// <summary>
        /// Sets the trace level configuration on the remote host
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <param name="traceLevel">The new trace level to use</param>
        /// <returns>Success or Failure response</returns>
        public RemoteHostResponse SetTraceLevel(string clientId, string traceLevel)
        {
            try
            {
                return base.Channel.SetTraceLevel(clientId, traceLevel);
            }
            catch (System.ServiceModel.EndpointNotFoundException enf)
            {
                return new RemoteHostResponse(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(enf));
            }
            catch (Exception exc)
            {
                return new RemoteHostResponse(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(exc));
            }
        }

        /// <summary>
        /// Causes the remote host to temporarily suspend processing operations 
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <returns>Success or Failure response</returns>
        public RemoteHostResponse Pause(string clientId)
        {
            try
            {
                return base.Channel.Pause(clientId);
            }
            catch (System.ServiceModel.EndpointNotFoundException enf)
            {
                return new RemoteHostResponse(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(enf));
            }
            catch (Exception exc)
            {
                return new RemoteHostResponse(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(exc));
            }
        }

        /// <summary>
        /// Causes the remote host to resume processing
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <returns>Success or Failure response</returns>
        public RemoteHostResponse Resume(string clientId)
        {
            try
            {
                return base.Channel.Resume(clientId);
            }
            catch (System.ServiceModel.EndpointNotFoundException enf)
            {
                return new RemoteHostResponse(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(enf));
            }
            catch (Exception exc)
            {
                return new RemoteHostResponse(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(exc));
            }
        }

        /// <summary>
        /// Requests a status message from remote host
        /// </summary>
        /// <param name="clientId">Unique identifier string of client application</param>
        /// <returns>Success or Failure response.  On success, a serialized version of the 
        /// runtime settings is provided in the returnValue</returns>
        public RemoteHostResponseString Status(string clientId)
        {
            try
            {
                return base.Channel.Status(clientId);
            }
            catch (System.ServiceModel.EndpointNotFoundException enf)
            {
                 return new RemoteHostResponseString(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(enf));
            }
            catch (Exception exc)
            {
                return new RemoteHostResponseString(false
                        , ILoggingManagement.ExceptionEvent.ConvertExceptionToString(exc));
            }
        }
    }
}
