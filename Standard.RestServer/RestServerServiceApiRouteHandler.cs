using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace NETStandard.RestServer
{
    internal class RestServerServiceRouteHandler : IRestServerRouteHandler
    {
        private readonly IPEndPoint _endPoint;
        private readonly List<ExposedRestServerService> _exposedRestServerServices;
        private readonly IRestServerServiceDependencyResolver _RestServerDependencyResolver;
        private readonly Assembly[] _assemblys;

        public RestServerServiceRouteHandler(IPEndPoint endPoint, IRestServerServiceDependencyResolver RestServerDependencyResolver, params Assembly[] assemblys)
        {
            _exposedRestServerServices = new List<ExposedRestServerService>();
            _assemblys = assemblys;
            _RestServerDependencyResolver = RestServerDependencyResolver;
            _endPoint = endPoint;

            RegisterExposedServices();
        }

        private void RegisterExposedServices()
        {
            var RestServerServiceBaseType = typeof(RestServerService);
            foreach (var assembly in _assemblys)
            {
                var test = assembly.GetTypes();


                foreach (var RestServerServiceType in assembly.GetTypes()
                    .Where(type => type != RestServerServiceBaseType && type.GetTypeInfo().BaseType == RestServerServiceBaseType))
                {
                    var instanceType = RestServerServiceInstanceType.Instance;
                    var attrib = RestServerServiceType.GetTypeInfo().GetCustomAttribute<RestServerServiceInstanceAttribute>();
                    if (attrib != null)
                    {
                        instanceType = attrib.RestServerServiceInstanceType;
                    }

                    TryExposeRestServerService(RestServerServiceType, instanceType);
                }
            }
        }
        private bool IsRouteAlreadyRegistered(string route)
        {
            return GetActionForRoute(route) != null;
        }
        private ExposedRestServerAction GetActionForRoute(string route)
        {
            return _exposedRestServerServices
                .SelectMany(service => service.Routes)
                .Where(rroute => string.Compare(rroute.Route, route, true) == 0)
                .FirstOrDefault();
        }
        private void TryExposeRestServerService(Type RestServerServiceType, RestServerServiceInstanceType instanceType)
        {
            ExposedRestServerService exposedRestServerService = new ExposedRestServerService(_RestServerDependencyResolver)
            {
                ServiceType = RestServerServiceType,
                InstanceType = instanceType,
            };

            foreach (var method in RestServerServiceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var methodAttribute = method.GetCustomAttribute<RestServerCallAttribute>();
                if (methodAttribute == null)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(methodAttribute.Route))
                {
                    throw new ArgumentException($"Invalid route given: {methodAttribute.Route}", nameof(RestServerCallAttribute.Route));
                }

                var routeStr = MakeRoute(methodAttribute.Route);

                if (IsRouteAlreadyRegistered(routeStr))
                {
                    throw new ArgumentException($"Route already registered. {routeStr}", nameof(RestServerCallAttribute.Route));
                }

                Type inputType = null;
                var parameters = method.GetParameters();
                if (parameters != null && parameters.Length > 1)
                {
                    Log.w($"{nameof(RestServerServiceRouteHandler)}: Method Parameter missmatch. Too many parameters. {routeStr}");
                    continue;
                }
                if (parameters != null && parameters.Length == 1)
                {
                    var parameterInfo = parameters[0];
                    var parameterTypeInfo = parameterInfo.ParameterType.GetTypeInfo();
                    if (!parameterTypeInfo.IsClass)
                    {
                        Log.w($"{nameof(RestServerServiceRouteHandler)}: Method Parameter missmatch. Parameter is no class! {routeStr}");
                        continue;
                    }

                    inputType = parameterInfo.ParameterType;
                }

                var exposedRestServerAction = new ExposedRestServerAction(exposedRestServerService, method)
                {
                    OutputType = method.ReturnType,
                    InputType = inputType,
                    Route = routeStr,
                    Methods = methodAttribute.Methods?.Split(',').Select(str => str.Trim().ToUpper()).ToArray() ?? new string[0]
                };
                exposedRestServerService.Routes.Add(exposedRestServerAction);

                Log.i($"{nameof(RestServerServiceRouteHandler)}: \"{RestServerServiceType.FullName}\" exposed API method \"{exposedRestServerAction.Route}\".");
            }

            // We got any routes exposed? 
            if (exposedRestServerService.Routes.Count > 0)
            {
                if (exposedRestServerService.InstanceType == RestServerServiceInstanceType.SingletonStrict)
                {
                    exposedRestServerService.GetInstance(null);
                }

                _exposedRestServerServices.Add(exposedRestServerService);
            }
        }
        private static string MakeRoute(string route)
        {
            while (route.StartsWith("/"))
            {
                route = route.Substring(1);
            }
            while (route.EndsWith("/"))
            {
                route = route.Substring(0, route.Length - 1);
            }

            return route;
        }
        private string StripPort(string absPath)
        {
            string portStr = _endPoint.Port.ToString();

            if (absPath.StartsWith(portStr))
            {
                absPath = absPath.Substring(portStr.Length);
            }

            return absPath;
        }

        public async Task<bool> HandleRouteAsync(RestServerHttpContext context)
        {
            string route = MakeRoute(StripPort(context.Request.Url.AbsolutePath));
            var RestServerAction = GetActionForRoute(route);
            if (RestServerAction == null)
            {
                return false;
            }

            object inputParameter = null;

            try
            {
                if (RestServerAction.InputType != null)
                {
                    inputParameter = JsonConvert.DeserializeObject(await context.Request.ReadContentAsStringAsync(), RestServerAction.InputType);
                }
            }
            catch (JsonException ex)
            {
                context.Response.ReasonPhrase = "Bad Request - " + ex.Message;
                context.Response.StatusCode = 400;
                context.Response.Close();
                return true;
            }

            object result = null;
            try
            {
                result = await RestServerAction.Execute(context, inputParameter);
            }
            catch (Exception)
            {
                context.Response.InternalServerError();
                context.Response.Close();
                return true;
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
                    return true;
                }
            }

            return true;
        }
    }
}
