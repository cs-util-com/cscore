using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace com.csutil {

    public class IpLookup {

        public class LocalIpAddress {
            
            private readonly IPAddressInformation _fullIpInfo;
            public readonly NetworkInterface NetworkInfo;
            
            public string Ip => _fullIpInfo.Address.ToString();
            public bool IsIpv4 => _fullIpInfo.Address.AddressFamily == AddressFamily.InterNetwork;
            public bool IsIpv6 => _fullIpInfo.Address.AddressFamily == AddressFamily.InterNetworkV6;
            public string NetworkType => NetworkInfo.NetworkInterfaceType.ToString();
            public string NetworkName => NetworkInfo.Name + " (" + NetworkType + "): " + NetworkInfo.Description;

            public LocalIpAddress(IPAddressInformation fullIpInfo, NetworkInterface networkInfo) {
                _fullIpInfo = fullIpInfo;
                NetworkInfo = networkInfo;
            }
            
            public IPAddressInformation GetFullIpInfo() { return _fullIpInfo; }
            
        }

        /// <summary> From https://forum.unity.com/threads/android-build-cant-get-ip-address.844843/#post-6429857 </summary>
        public static List<LocalIpAddress> GetLocalIPs() {
            var result = new List<LocalIpAddress>();
            foreach (NetworkInterface network in NetworkInterface.GetAllNetworkInterfaces()) {
                if (network.NetworkInterfaceType == NetworkInterfaceType.Loopback || network.OperationalStatus != OperationalStatus.Up) {
                    continue;
                }
                foreach (UnicastIPAddressInformation ip in network.GetIPProperties().UnicastAddresses) {
                    if (ip.Address.ToString() != "127.0.0.1") {
                        var localIpAddress = new LocalIpAddress(ip, network);
                        if (localIpAddress.IsIpv4 || localIpAddress.IsIpv6) {
                            result.Add(localIpAddress);
                        }
                    }
                }
            }
            return result;
        }

    }

}