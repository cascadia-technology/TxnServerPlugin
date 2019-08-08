using System;
using System.Collections.Generic;
using VideoOS.Platform.Transact.Connector;
using VideoOS.Platform.Transact.Connector.Property;

namespace CascadiaTxnPlugin
{
    public class CascadiaTxnServerDefinition : ConnectorDefinition
    {
        public override Guid Id { get; } = new Guid(Resources.PluginId);

        public override string Name { get; } = Resources.ProductName;

        public override string DisplayName => Name;

        private static readonly Version ConnectorVersion = new Version(1, 0);
        public override string VersionText => ConnectorVersion.ToString();

        public override string Manufacturer { get; } = Resources.Manufacturer;

        public override void Init()
        {
            Util.Log(
                false, 
                $"{GetType().FullName}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", 
                $"Initialized {Name} connector definition");
        }

        public override ConnectorInstance CreateConnectorInstance()
        {
            Util.Log(
                false, 
                $"{GetType().FullName}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", 
                $"Creating a new {Name} connector instance");
            return new CascadiaTxnServerInstance();
        }

        public override void Close()
        {
            Util.Log(
                false, 
                $"{GetType().FullName}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", 
                $"Closing {Name} connector definition");
        }

        public override IEnumerable<ConnectorPropertyDefinition> GetPropertyDefinitions()
        {
            return new List<ConnectorPropertyDefinition>
            {
                new ConnectorStringPropertyDefinition(nameof(Resources.Interface), Resources.Interface, "0.0.0.0", Resources.InterfaceToolTip),
                new ConnectorIntegerPropertyDefinition(nameof(Resources.LocalPort), Resources.LocalPort, 5123,
                    Resources.LocalPortToolTip) {MinValue = 1, MaxValue = 65535},
                new ConnectorStringPropertyDefinition(nameof(Resources.AllowedExternalAddresses), Resources.AllowedExternalAddresses, string.Empty, Resources.AllowedExternalAddressesToolTip),
                new ConnectorBooleanPropertyDefinition(nameof(Resources.Echo), Resources.Echo, false,
                    Resources.EchoToolTip),
                new ConnectorBooleanPropertyDefinition(nameof(Resources.EnableKeepAlives), Resources.EnableKeepAlives,
                    true, Resources.EnableKeepAlivesToolTip),
            };
        }
    }
}
