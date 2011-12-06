using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;

namespace B1.TaskProcessing
{
    public partial class RemoteHostClient : System.ServiceModel.ClientBase<IRemoteEngineHost>, IRemoteEngineHost
    {
        public RemoteHostClient()
        {
        }

        public RemoteHostClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public RemoteHostClient(string endpointConfigurationName
                , string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public RemoteHostClient(string endpointConfigurationName
                , System.ServiceModel.EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public RemoteHostClient(System.ServiceModel.Channels.Binding binding
                , System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        public void Connect(string clientId)
        {
            base.Channel.Connect(clientId);
        }

        public void Disconnect(string clientId)
        {
            base.Channel.Disconnect(clientId);
        }

        public string ConfigSettings()
        {
            return base.Channel.ConfigSettings();
        }

        public string RemoteClients()
        {
            return base.Channel.RemoteClients();
        }

        public string DynamicSettings()
        {
            return base.Channel.DynamicSettings();
        }

        public int SetMaxTasks(string clientId, int delta)
        {
            return base.Channel.SetMaxTasks(clientId, delta);
        }

        public void SetTraceLevel(string clientId, string traceLevel)
        {
            base.Channel.SetTraceLevel(clientId, traceLevel);
        }

        public void Pause(string clientId)
        {
            base.Channel.Pause(clientId);
        }

        public void Resume(string clientId)
        {
            base.Channel.Resume(clientId);
        }

        public string Status(string clientId)
        {
            return base.Channel.Status(clientId);
        }
    }
}
