using System.Collections.Generic;
using VideoOS.Platform.Transact.Connector.Property;

namespace CascadiaTxnPlugin
{
    public class CascadiaTxnServerInstanceProperties
    {
        public int LocalPort { get; set; }
        public bool Echo { get; set; }
        public bool EnableKeepAlives { get; set; }

        public static CascadiaTxnServerInstanceProperties Parse(IEnumerable<ConnectorProperty> properties)
        {
            var instanceProperties = new CascadiaTxnServerInstanceProperties();
            foreach (var property in properties)
            {
                switch (property.Key)
                {
                    case nameof(Resources.LocalPort):
                    {
                        instanceProperties.LocalPort = ((ConnectorIntegerProperty) property).Value;
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
