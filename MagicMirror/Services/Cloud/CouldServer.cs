using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace MagicMirror.Services.Cloud
{
    public partial class CloudServer : IDisposable
    {
        private readonly IPEndPoint _endPoint;
        private readonly ConcurrentBag<ICloudRouteHandler> _cloudRouteHandlers;

        public bool IsRunning {
            get {
                return _httpListener?.IsListening ?? false;
            }
        }
        public IPEndPoint IPEndPoint
        {
            get
            {
                return new IPEndPoint(_endPoint.Address, _endPoint.Port);
            }
        }

        private HttpListener _httpListener;

        public CloudServer(int port, ICloudServiceDependencyResolver cloudDependencyResolver, params Assembly[] assemblys)
            : this(GetDefaultEndPoint(port), cloudDependencyResolver, assemblys) { }
        public CloudServer(IPEndPoint endPoint, ICloudServiceDependencyResolver cloudDependencyResolver, params Assembly[] assemblys)
        {
            _endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));


            if (assemblys == null || assemblys.Length == 0)
            {
                _cloudRouteHandlers = new ConcurrentBag<ICloudRouteHandler>();
            }
            else
            {
                _cloudRouteHandlers = new ConcurrentBag<ICloudRouteHandler>(
                    new ICloudRouteHandler[] {
                       new CloudServiceApiRouteHandler(endPoint, cloudDependencyResolver, assemblys),
                    }
                );
            }
        }

        public CloudServer RegisterRouteHandler(ICloudRouteHandler handler)
        {
            _cloudRouteHandlers.Add(handler);

            return this;
        }

        public void Start()
        {
            if (_httpListener == null)
            {
                _httpListener = new HttpListener(_endPoint);
                _httpListener.Request += (sender, e) => ProcessHttpRequestOwnTask(new CloudHttpContext(e.Request, e.Response));
                _httpListener.Start();

                return;
            }

            throw new InvalidOperationException("CloudServer already running.");
        }
        public void Stop()
        {
            if (_httpListener != null)
            {
                _httpListener.Close();
                _httpListener.Dispose();
                _httpListener = null;
            }
        }
        
        private async Task ProcessHttpRequest(CloudHttpContext context)
        {
            foreach (var routeHandler in _cloudRouteHandlers)
            {
                var isHandled = await routeHandler.HandleRouteAsync(context);

                if (isHandled)
                {
                    return;
                }
            }

            context.Response.NotFound();
            context.Response.Close();
        }
        private void ProcessHttpRequestOwnTask(CloudHttpContext context)
        {
            Task.Factory.StartNew(async () => await ProcessHttpRequest(context));
        }

        public void Dispose()
        {
            if (_httpListener != null)
            {
                _httpListener.Close();
                _httpListener.Dispose();
                _httpListener = null;
            }
        }
        
        private static IPEndPoint GetDefaultEndPoint(int port)
        {
            List<IPAddress> ipAddresses = new List<IPAddress>();
            var hostnames = NetworkInformation.GetHostNames();
            foreach (var hn in hostnames)
            {
                if (hn.IPInformation != null && 
                     (hn.IPInformation.NetworkAdapter.IanaInterfaceType == 71 || 
                      hn.IPInformation.NetworkAdapter.IanaInterfaceType == 6))
                {
                    string strIPAddress = hn.DisplayName;
                    
                    if (IPAddress.TryParse(strIPAddress, out IPAddress address))
                        ipAddresses.Add(address);
                }
            }

            if (ipAddresses.Count < 1)
            {
                return new IPEndPoint(IPAddress.Loopback, port);
            }
            else
            {
                return new IPEndPoint(ipAddresses[ipAddresses.Count - 1], port);
            }
        }
    }
}
