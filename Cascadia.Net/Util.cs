using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Cascadia.Net
{
    public static class Util
    {
        public static IPAddress GetIpAddressByString(string addressString)
        {
            switch (addressString)
            {
                case "0.0.0.0":
                    return IPAddress.Any;
                case "::":
                    return IPAddress.IPv6Any;
                case "::1":
                    return IPAddress.IPv6Loopback;
                case "127.0.0.1":
                case "localhost":
                    return IPAddress.Loopback;
                default:
                {
                    foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                            ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                        {
                            foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                            {
                                if (ip.Address.ToString() == addressString) return ip.Address;
                            }
                        }
                    }

                    return null;
                }
            }
        }
    }
}
