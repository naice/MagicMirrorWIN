using NETStandard.RestServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace ConfigServer
{
    public class ConfigServer
    {
        #region SINGLETON
        private static ConfigServer _Instance;

        public static ConfigServer Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ConfigServer();
                return _Instance;
            }
        }

        private readonly RestServer _httpServer;
        ConfigServer()
        {
            _httpServer = new RestServer(
                new IPEndPoint(IPAddress.Parse(GetLocalIp()), DependencyConfiguration.DefaultPort),
                null, 
                System.Reflection.Assembly.GetExecutingAssembly());
            _httpServer.RegisterRouteHandler(new RestServerServiceFileRouteHandler(DependencyConfiguration.DefaultBasePath));
        }
        private static string GetLocalIp()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null) return null;
            var hostname =
                NetworkInformation.GetHostNames()
                    .FirstOrDefault(
                        hn => hn.IPInformation?.NetworkAdapter != null && 
                            hn.Type == Windows.Networking.HostNameType.Ipv4 &&
                            hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);

            // the ip address
            return hostname?.CanonicalName;
        }
        #endregion

        public void Run()
        {
            _httpServer.Start();
        }
    }
}
