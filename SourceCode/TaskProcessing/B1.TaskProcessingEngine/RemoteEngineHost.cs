using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Text;

namespace B1.TaskProcessing
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RemoteEngineHost" in both code and config file together.

    public class RemoteTaskProcessEngine : IRemoteEngineHost
    {
        static TaskProcessingEngine _localTPE = null;
        static Dictionary<string, string> _configSettings = new Dictionary<string, string>();
        static Dictionary<string, DateTime> _clientConnections = new Dictionary<string, DateTime>();
        RemoteEndpointMessageProperty _clientEndpoint = null;
        string _clientId = null;

        public void Connect(string appId)
        {
            _clientId = string.Format("{0}:{1}:{2}", _clientEndpoint.Address, _clientEndpoint.Port, appId);
            if (!_clientConnections.ContainsKey(_clientId))
                _clientConnections.Add(_clientId, DateTime.UtcNow);
        }

        public void Disconnect(string appId)
        {
            OperationContext context = OperationContext.Current;
            MessageProperties properties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            string key = string.Format("{0}:{1}:{2}", endpoint.Address, endpoint.Port, appId);
            if (_clientConnections.ContainsKey(key))
                _clientConnections.Remove(key);
        }

        public RemoteTaskProcessEngine()
        {
            if (_localTPE == null)
                _localTPE = new TaskProcessingEngine();
            OperationContext context = OperationContext.Current;
            MessageProperties properties = context.IncomingMessageProperties;
            _clientEndpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
        }

        public Dictionary<string, string> ConfigSettings()
        {
            return _localTPE.ConfigSettings();
        }

        public int MaxTasksSet(string appId, int delta)
        {
            return _localTPE.MaxTasksSet(appId, delta);
        }

        public int MaxTasksGet()
        {
            return _localTPE.MaxTasksGet();
        }

        public void Pause(string appId)
        {
            _localTPE.Pause(_clientId);
        }

        public void Resume(string appId)
        {
            _localTPE.Resume(_clientId);
        }

        public string Status()
        {
            return _localTPE.Status(null);
        }

    }
}
