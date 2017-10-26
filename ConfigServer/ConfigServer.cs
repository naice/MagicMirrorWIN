using Restup.Webserver.File;
using Restup.Webserver.Http;
using Restup.Webserver.Rest;
using Restup.WebServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public class DebugLogger : AbstractLogger
        {
            protected override bool IsLogEnabled(LogLevel trace)
            {
                // Ignore level, log everything
                return true;
            }

            protected override void LogMessage(string message, LogLevel loggingLevel, Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{loggingLevel}: {message}");
                System.Diagnostics.Debug.WriteLine($"{ex}");
            }

            protected override void LogMessage(string message, LogLevel loggingLevel, params object[] args)
            {
                System.Diagnostics.Debug.WriteLine($"{loggingLevel}: {(string.Format(message, args))}");
            }
        }
        public class DebugLogFactory : ILogFactory
        {
            private ILogger _debugLogger;

            public DebugLogFactory()
            {
                _debugLogger = new DebugLogger();
            }

            public void Dispose()
            {
                _debugLogger = null;
            }

            public ILogger GetLogger(string name)
            {
                return _debugLogger;
            }

            public ILogger GetLogger<T>()
            {
                return _debugLogger;
            }
        }

        private readonly HttpServer _httpServer;
        ConfigServer()
        {
            Restup.Webserver.LogManager.SetLogFactory(new DebugLogFactory());

            var restRouteHandler = new RestRouteHandler();
            restRouteHandler.RegisterController<ConigController>();

            var fileRouteHandler = new StaticFileRouteHandler(DependencyConfiguration.DefaultBasePath);

            var configuration = new HttpServerConfiguration()
              .ListenOnPort(DependencyConfiguration.DefaultPort)
              .RegisterRoute("api", restRouteHandler)
              .RegisterRoute(fileRouteHandler)
              .EnableCors();

            _httpServer = new HttpServer(configuration);
        }
        #endregion

        public async Task Run()
        {
            await _httpServer.StartServerAsync();
        }

        /// <summary>
        /// No Exception = good.
        /// </summary>
        public static void TestConfigurationObject(object toTest)
        {
            JsonSchemaBuilder.Instance.Build(toTest);
        }
    }
}
