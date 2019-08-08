using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VideoOS.Platform.Transact.Connector.Property;

namespace CascadiaTxnPlugin
{
    public class CascadiaTxnServerInstanceProperties
    {
        public IPAddress LocalIp { get; set; }
        public int LocalPort { get; set; }
        public bool Echo { get; set; }
        public bool EnableKeepAlives { get; set; }
        public List<string> AllowedExternalAddresses { get; set; } = new List<string>();

        public static CascadiaTxnServerInstanceProperties Parse(IEnumerable<ConnectorProperty> properties)
        {
            var instanceProperties = new CascadiaTxnServerInstanceProperties();
            foreach (var property in properties)
            {
                switch (property.Key)
                {
                    case nameof(Resources.Interface):
                    {
                        instanceProperties.LocalIp =
                            Cascadia.Net.Util.GetIpAddressByString(((ConnectorStringProperty) property).Value);
                        break;
                    }

                    case nameof(Resources.LocalPort):
                    {
                        instanceProperties.LocalPort = ((ConnectorIntegerProperty) property).Value;
                        break;
                    }

                    case nameof(Resources.AllowedExternalAddresses):
                    {
                        instanceProperties.AllowedExternalAddresses =
                            ((ConnectorStringProperty) property).Value.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();
                        break;
                    }

                    case nameof(Resources.Echo):
                    {
                        instanceProperties.Echo = ((ConnectorBooleanProperty) property).Value;
                        break;
                    }

                    case nameof(Resources.EnableKeepAlives):
                    {
                        instanceProperties.EnableKeepAlives = ((ConnectorBooleanProperty) property).Value;
                        break;
                    }
                }
            }

            return instanceProperties;
        }

    }
}
