using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Services.Cloud
{
    public class CloudServer : IDisposable
    {
        private readonly int _port;
        private readonly Assembly[] _assemblys;
        private readonly List<ExposedCloudService> _exposedCloudServices;
        private readonly ICloudDependencyResolver _cloudDependencyResolver;

        private HttpListener _httpListener;

        private class ExposedCloudService
        {
            public Type ServiceType { get; set; }
            public CloudServiceInstanceType InstanceType { get; set; } = CloudServiceInstanceType.Instance;
            public CloudService SingletonInstance { get; set; }
            public List<ExposedCloudAction> Routes { get; set; } = new List<ExposedCloudAction>();

            private readonly ICloudDependencyResolver _cloudDependencyResolver;

            public ExposedCloudService(ICloudDependencyResolver cloudDependencyResolver)
            {
                _cloudDependencyResolver = cloudDependencyResolver;
            }

            public object GetInstance()
            {
                if (InstanceType == CloudServiceInstanceType.Instance)
                {
                    return InternalCreateInstance();
                }

                if (InstanceType == CloudServiceInstanceType.SingletonStrict ||
                    InstanceType == CloudServiceInstanceType.SingletonLazy)
                {
                    return SingletonInstance = SingletonInstance ?? InternalCreateInstance();
                }

                throw new NotImplementedException($"{nameof(ExposedCloudService)}: InstanceType not implemented. {InstanceType}");
            }

            private CloudService InternalCreateInstance()
            {
                var constuctors = ServiceType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

                foreach (var constr in constuctors.OrderByDescending(constructor => constructor.GetParameters().Length))
                {
                    var parameters = constr.GetParameters();
                    if (parameters == null || parameters.Length == 0)
                    {
                        // default constructor
                        return constr.Invoke(new object[0]) as CloudService;
                    }

                    if (_cloudDependencyResolver != null)
                    {
                        // resolve dependencys
                        var dependencys = _cloudDependencyResolver.GetDependecys(parameters.Select(param => param.ParameterType).ToArray());
                        if (dependencys == null || parameters.Length != dependencys.Length)
                        {
                            // no dependecys found.
                            continue;
                        }
                        var cloudService = constr.Invoke(dependencys) as CloudService;
                        if (cloudService != null)
                        {
                            return cloudService;
                        }
                    }
                }

                throw new InvalidOperationException($"{nameof(ExposedCloudService)}: Could not find a matching constructor for {ServiceType.FullName}.");
            }
        }
        private class ExposedCloudAction
        {
            public Type InputType { get; set; }
            public Type OutputType { get; set; }
            public string Route { get; set; }
            public string[] Methods { get; set; }
            public ExposedCloudService CloudService { get { return _cloudService; } }

            private readonly ExposedCloudService _cloudService;
            private readonly MethodInfo _methodInfo;

            public ExposedCloudAction(ExposedCloudService cloudService, MethodInfo methodInfo)
            {
                _cloudService = cloudService;
                _methodInfo = methodInfo;
            }

            public async Task<object> Execute(object param)
            {
                // VOID
                if (OutputType == null && InputType == null)
                {
                    ExecuteVoid();
                    return null;
                }
                if (OutputType == null)
                {
                    ExecuteVoid(param);
                    return null;
                }
                if (OutputType == typeof(Task) && InputType == null)
                {
                    await ExecuteVoidAsync();
                    return null;
                }
                if (OutputType == typeof(Task))
                {
                    await ExecuteVoidAsync(param);
                    return null;
                }

                // RESULT
                if (IsGenericTaskType(OutputType) && InputType == null)
                {
                    return await ExecuteAsync();
                }
                if (IsGenericTaskType(OutputType))
                {
                    return await ExecuteAsync(param);
                }
                if (InputType == null)
                {
                    return ExecuteInternal();
                }

                return ExecuteInternal(param);
            }

            private bool IsGenericTaskType(Type type)
            {
                var typeInfo = type.GetTypeInfo();
                return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Task<>);
            }

            private object ExecuteInternal(object param)
            {
                return _methodInfo.Invoke(_cloudService.GetInstance(), new object[] { param });
            }
            private object ExecuteInternal()
            {
                return _methodInfo.Invoke(_cloudService.GetInstance(), new object[0]);
            }
            private async Task<object> ExecuteAsync(object param)
            {
                return await (dynamic)_methodInfo.Invoke(_cloudService.GetInstance(), new object[] { param });
            }
            private async Task<object> ExecuteAsync()
            {
                return await (dynamic)_methodInfo.Invoke(_cloudService.GetInstance(), new object[0]);
            }
            private async Task ExecuteVoidAsync(object param)
            {
                await (Task) _methodInfo.Invoke(_cloudService.GetInstance(), new object[] { param });
            }
            private async Task ExecuteVoidAsync()
            {
                await (Task) _methodInfo.Invoke(_cloudService.GetInstance(), new object[0]);
            }
            private void ExecuteVoid(object param)
            {
                _methodInfo.Invoke(_cloudService.GetInstance(), new object[] { param });
            }
            private void ExecuteVoid()
            {
                _methodInfo.Invoke(_cloudService.GetInstance(), new object[0]);
            }
        }

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

            if (exposedCloudService.Routes.Count > 0)
            {
                _exposedCloudServices.Add(exposedCloudService);
            }
        }
        private string MakeRoute(string route)
        {
            if (route.StartsWith("/"))
            {
                route = route.Substring(1);
            }
            if (route.EndsWith("/"))
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
            string route = MakeRoute(StripPort(e.Request.Url.AbsolutePath));
            var cloudAction = GetActionForRoute(route);

            if (cloudAction == null)
            {
                e.Response.NotFound();
                e.Response.Close();
                return;
            }

            object inputParameter = null;

            try
            {
                if (cloudAction.InputType != null)
                {
                    inputParameter = JsonConvert.DeserializeObject(await e.Request.ReadContentAsStringAsync(), cloudAction.InputType);
                }
            }
            catch (JsonException ex)
            {
                e.Response.ReasonPhrase = "Bad Request - " + ex.Message;
                e.Response.StatusCode = 400;
                e.Response.Close();
                return;
            }

            object result = null;
            try
            {
                result = await cloudAction.Execute(inputParameter);
            }
            catch (Exception)
            {
                e.Response.InternalServerError();
                e.Response.Close();
                return;
            }

            if (result != null)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(result);
                    await e.Response.WriteContentAsync(json);
                }
                catch (JsonException)
                {
                    e.Response.InternalServerError();
                    e.Response.Close();
                    return;
                }
            }

            e.Response.Close();
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
