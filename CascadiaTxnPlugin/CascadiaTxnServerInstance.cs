using Cascadia.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using VideoOS.Platform.Transact.Connector;
using VideoOS.Platform.Transact.Connector.Property;

namespace CascadiaTxnPlugin
{
    public class CascadiaTxnServerInstance : ConnectorInstance
    {
        private static readonly ConcurrentDictionary<Guid, int> PortRegistry = new ConcurrentDictionary<Guid, int>();
        private readonly Guid _instanceId;
        private ITransactionDataReceiver _txnReceiver;
        private CascadiaTxnServerInstanceProperties _properties;
        private TcpServer _tcpServer;

        public CascadiaTxnServerInstance()
        {
            _instanceId = Guid.NewGuid();
        }

        public override void Init(ITransactionDataReceiver transactionDataReceiver, IEnumerable<ConnectorProperty> properties)
        {
            Util.Log(
                false, 
                $"{GetType().FullName}.{System.Reflection.MethodBase.GetCurrentMethod().Name} [{_instanceId}]", 
                $"Initializing {Resources.CascadiaTransactionServer} connector instance");
            _txnReceiver = transactionDataReceiver;
            UpdateProperties(properties);
        }

        public override void UpdateProperties(IEnumerable<ConnectorProperty> properties)
        {
            _properties = CascadiaTxnServerInstanceProperties.Parse(properties);
            PortRegistry.AddOrUpdate(
                _instanceId, 
                guid => _properties.LocalPort, 
                (guid, i) => _properties.LocalPort);
            StartInstance();
        }

        public override ConnectorPropertyValidationResult ValidateProperties(IEnumerable<ConnectorProperty> properties)
        {
            var newPort =
                properties.Single(p => p.Key == nameof(Resources.LocalPort)) as ConnectorIntegerProperty;
            if (newPort == null) 
                return ConnectorPropertyValidationResult.CreateInvalidResult(nameof(Resources.LocalPort), $"Property not found: {Resources.LocalPort}");
            var portInUse = PortRegistry.Where(pair => pair.Key != _instanceId && pair.Value == newPort.Value).ToList();
            if (portInUse.Count != 0)
            {
                Util.Log(
                    true, 
                    $"{GetType().FullName}.{System.Reflection.MethodBase.GetCurrentMethod().Name} [{_instanceId}]", 
                    $"Port {newPort} in use by instance {portInUse.First().Key}");
                return ConnectorPropertyValidationResult.CreateInvalidResult(
                    nameof(Resources.LocalPort), 
                    string.Format(Resources.LocalPortInUse, newPort));
            }
            return ConnectorPropertyValidationResult.ValidResult;
        }

        private void StartInstance()
        {
            if (_tcpServer != null) Close();
            Util.Log(
                false, 
                $"{GetType().FullName}.{System.Reflection.MethodBase.GetCurrentMethod().Name} [{_instanceId}]", 
                $"Starting {Resources.CascadiaTransactionServer} listener on port {_properties.LocalPort}");
            _tcpServer = new TcpServer(new TcpServerOptions()
            {
                Echo = _properties.Echo, 
                EnableKeepAlive = _properties.EnableKeepAlives, 
                LocalEndpoint = new IPEndPoint(IPAddress.Any, _properties.LocalPort), 
                MaxConnections = 1
            });
            _tcpServer.BytesReceived += (sender, bytes) => _txnReceiver.WriteRawData(bytes);
            _tcpServer.ErrorMessage += (sender, exception) =>
                Util.Log(true, "Cascadia.Net.TcpServer", exception.ToString());
            _tcpServer.InfoMessage += (sender, s) => Util.Log(false, "Cascadia.Net.TcpServer", s);
            _tcpServer.Start();
        }

        public override void Close()
        {
            Util.Log(
                false, 
                $"{GetType().FullName}.{System.Reflection.MethodBase.GetCurrentMethod().Name} [{_instanceId}]", 
                $"Closing {Resources.CascadiaTransactionServer} connector instance");
            if (!PortRegistry.TryRemove(_instanceId, out var port))
            {
                Util.Log(
                    true, 
                    $"{GetType().FullName}.{System.Reflection.MethodBase.GetCurrentMethod().Name} [{_instanceId}]", 
                    $"An error occurred while unregistering instance. Port {_properties.LocalPort} may not be available until an Event Server restart.");
            }
            _tcpServer?.Stop();
        }
    }
}
