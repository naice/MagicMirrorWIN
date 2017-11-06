using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace MagicMirror.Services.Cloud
{
    public partial class CloudServer : IDisposable
    {
        private readonly int _port;
        private readonly Assembly[] _assemblys;
        private readonly List<ExposedCloudService> _exposedCloudServices;
        private readonly ICloudDependencyResolver _cloudDependencyResolver;

        private HttpListener _httpListener;

        public CloudServer(int port, ICloudDependencyResolver cloudDependencyResolver, params Assembly[] assemblys)
        {
            _port = port;
            _exposedCloudServices = new List<ExposedCloudService>();
            _assemblys = assemblys;
            _cloudDependencyResolver = cloudDependencyResolver;

            RegisterExposedServices();
        }

        private void RegisterExposedServices()
        {
            var cloudServiceBaseType = typeof(CloudService);
            foreach (var assembly in _assemblys)
            {
                var test = assembly.GetTypes();


                foreach (var cloudServiceType in assembly.GetTypes()
                    .Where(type=> type != cloudServiceBaseType && type.GetTypeInfo().BaseType == cloudServiceBaseType))
                {
                    var instanceType = CloudServiceInstanceType.Instance;
                    var attrib = cloudServiceType.GetTypeInfo().GetCustomAttribute<CloudServiceInstanceAttribute>();
                    if (attrib != null)
                    {
                        instanceType = attrib.CloudServiceInstanceType;
                    }

                    TryExposeCloudService(cloudServiceType, instanceType);
                }
            }
        }
        private bool IsRouteAlreadyRegistered(string route)
        {
            return GetActionForRoute(route) != null;
        }
        private ExposedCloudAction GetActionForRoute(string route)
        {
            return _exposedCloudServices.SelectMany(service => service.Routes).Where(rroute => string.Compare(rroute.Route, route, true) == 0).FirstOrDefault();
        }
        private void TryExposeCloudService(Type cloudServiceType, CloudServiceInstanceType instanceType)
        {
            ExposedCloudService exposedCloudService = new ExposedCloudService(_cloudDependencyResolver)
            {
                ServiceType = cloudServiceType,
                InstanceType = instanceType,
            };
            
            foreach (var method in cloudServiceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var methodAttribute = method.GetCustomAttribute<CloudCallAttribute>();
                if (methodAttribute == null)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(methodAttribute.Route))
                {
                    throw new ArgumentException($"Invalid route given: {methodAttribute.Route}", nameof(CloudCallAttribute.Route));
                }

                var routeStr = MakeRoute(methodAttribute.Route);

                if (IsRouteAlreadyRegistered(routeStr))
                {
                    throw new ArgumentException($"Route already registered. {routeStr}", nameof(CloudCallAttribute.Route));
                }

                Type inputType = null;
                var parameters = method.GetParameters();
                if (parameters != null && parameters.Length > 1)
                {
                    Log.w($"CloudServer: Method Parameter missmatch. Too many parameters. {routeStr}");
                    continue;
                }
                if (parameters != null && parameters.Length == 1)
                {
                    var parameterInfo = parameters[0];
                    var parameterTypeInfo = parameterInfo.ParameterType.GetTypeInfo();
                    if (!parameterTypeInfo.IsClass)
                    {
                        Log.w($"CloudServer: Method Parameter missmatch. Parameter is no class! {routeStr}");
                        continue;
                    }

                    inputType = parameterInfo.ParameterType;
                }

                var exposedCloudAction = new ExposedCloudAction(exposedCloudService, method)
                {
                    OutputType = method.ReturnType,
                    InputType = inputType,
                    Route = routeStr,
                    Methods = methodAttribute.Methods?.Split(',').Select(str => str.Trim().ToUpper()).ToArray() ?? new string[0]
                };
                exposedCloudService.Routes.Add(exposedCloudAction);

                Log.i($"CloudServer: \"{cloudServiceType.FullName}\" exposed API method \"{exposedCloudAction.Route}\".");
            }

            // We got any routes exposed? 
            if (exposedCloudService.Routes.Count > 0)
            {
                if (exposedCloudService.InstanceType == CloudServiceInstanceType.SingletonStrict)
                {
                    exposedCloudService.GetInstance(null);
                }

                _exposedCloudServices.Add(exposedCloudService);
            }
        }
        private string MakeRoute(string route)
        {
            while (route.StartsWith("/"))
            {
                route = route.Substring(1);
            }
            while(route.EndsWith("/"))
            {
                route = route.Substring(0, route.Length - 1);
            }

            return route;
        }

        public void Start()
        {
            if (_httpListener == null)
            {
                _httpListener = new HttpListener(IPAddress.Parse("192.168.0.110"), _port);
                _httpListener.Request += _httpListener_Request;
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

        private string StripPort(string absPath)
        {
            string portStr = _port.ToString();

            if (absPath.StartsWith(portStr))
            {
                absPath = absPath.Substring(portStr.Length);
            }

            return absPath;
        }

        private async void  _httpListener_Request(object sender, HttpListenerRequestEventArgs e)
        {
            CloudHttpContext context = new CloudHttpContext(e.Request, e.Response);
            string route = MakeRoute(StripPort(context.Request.Url.AbsolutePath));
            var cloudAction = GetActionForRoute(route);

            if (cloudAction == null)
            {
                context.Response.NotFound();
                context.Response.Close();
                return;
            }

            object inputParameter = null;

            try
            {
                if (cloudAction.InputType != null)
                {
                    inputParameter = JsonConvert.DeserializeObject(await context.Request.ReadContentAsStringAsync(), cloudAction.InputType);
                }
            }
            catch (JsonException ex)
            {
                context.Response.ReasonPhrase = "Bad Request - " + ex.Message;
                context.Response.StatusCode = 400;
                context.Response.Close();
                return;
            }

            object result = null;
            try
            {
                result = await cloudAction.Execute(context, inputParameter);
            }
            catch (Exception)
            {
                context.Response.InternalServerError();
                context.Response.Close();
                return;
            }

            if (result != null)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(result);
                    await context.Response.WriteContentAsync(json);
                }
                catch (JsonException)
                {
                    context.Response.InternalServerError();
                    context.Response.Close();
                    return;
                }
            }

            context.Response.Close();
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
    }
}
