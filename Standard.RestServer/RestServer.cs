using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace NETStandard.RestServer
{
    public partial class RestServer : IDisposable
    {
        private readonly IPEndPoint _endPoint;
        private readonly ConcurrentBag<IRestServerRouteHandler> _RestServerRouteHandlers;

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

        private System.Net.Http.HttpListener _httpListener;

        public RestServer(int port, IRestServerServiceDependencyResolver RestServerDependencyResolver, params Assembly[] assemblys)
            : this(GetDefaultEndPoint(port), RestServerDependencyResolver, assemblys) { }
        public RestServer(IPEndPoint endPoint, IRestServerServiceDependencyResolver RestServerDependencyResolver, params Assembly[] assemblys)
        {
            _endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            
            if (assemblys == null || assemblys.Length == 0)
            {
                _RestServerRouteHandlers = new ConcurrentBag<IRestServerRouteHandler>();
            }
            else
            {
                _RestServerRouteHandlers = new ConcurrentBag<IRestServerRouteHandler>(
                    new IRestServerRouteHandler[] {
                       new RestServerServiceRouteHandler(endPoint, RestServerDependencyResolver, assemblys),
                    }
                );
            }
        }

        public RestServer RegisterRouteHandler(IRestServerRouteHandler handler)
        {
            _RestServerRouteHandlers.Add(handler);

            return this;
        }

        public void Start()
        {
            if (_httpListener == null)
            {
                _httpListener = new System.Net.Http.HttpListener(_endPoint);
                _httpListener.Request += (sender, e) => ProcessHttpRequestOwnTask(new RestServerHttpContext(e.Request, e.Response));
                _httpListener.Start();

                return;
            }

            throw new InvalidOperationException("RestServerServer already running.");
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
        
        private async Task ProcessHttpRequest(RestServerHttpContext context)
        {
            foreach (var routeHandler in _RestServerRouteHandlers)
            {
                var isHandled = await routeHandler.HandleRouteAsync(context);

                if (isHandled && !context.Response.IsClosed)
                {
                    context.Response.Close();
                    return;
                }
            }

            context.Response.NotFound();
            context.Response.Close();
        }
        private void ProcessHttpRequestOwnTask(RestServerHttpContext context)
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
            throw new NotImplementedException();

            /*
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
            */
        }
    }
}
