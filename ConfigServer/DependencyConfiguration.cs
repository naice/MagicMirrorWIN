using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConfigServer
{
    public class DependencyConfiguration
    {
        private static Converter.IJsonConvert _DefaultJsonConverter;
        internal static Converter.IJsonConvert DefaultJsonConverter
        {
            get { return _DefaultJsonConverter ?? throw Ex(); }
            private set { _DefaultJsonConverter = value; }
        }
        private static IConfigurationContract _DefaultConfigurationContract;
        internal static IConfigurationContract DefaultConfigurationContract
        {
            get { return _DefaultConfigurationContract ?? throw Ex(); }
            private set { _DefaultConfigurationContract = value; }
        }
        private static int? _DefaultPort = 8887;
        internal static int DefaultPort
        {
            get { return _DefaultPort ?? throw Ex(); }
            private set { _DefaultPort = value; }
        }
        private static string _DefaultBasePath = @"ConfigServer\INetPub";
        internal static string DefaultBasePath
        {
            get { return _DefaultBasePath ?? throw Ex(); }
            private set { _DefaultBasePath = value; }
        }

        

        /// <summary>
        /// Begin configuration.
        /// </summary>
        public static DependencyConfiguration Begin()
            => new DependencyConfiguration();

        private DependencyConfiguration() { }

        /// <summary>
        /// Set the default <see cref="Converter.IJsonConvert"/> for the framework.
        /// </summary>
        public DependencyConfiguration Set(Converter.IJsonConvert jsonConverter)
        {
            DefaultJsonConverter = jsonConverter ?? throw new ArgumentException(nameof(jsonConverter));
            return this;
        }

        /// <summary>
        /// Set the default <see cref="Model.IStorageIO"/> for the framework.
        /// </summary
        public DependencyConfiguration Set<T>(IConfigurationContract<T> configurationContract)
        {
            DefaultConfigurationContract = configurationContract ?? throw new ArgumentException(nameof(configurationContract));
            return this;
        }

        /// <summary>
        /// Set the default port for the frontend.
        /// </summary>
        public DependencyConfiguration SetPort(int port)
        {
            DefaultPort = port;
            return this;
        }

        /// <summary>
        /// Set the default base path for INetPub folder.
        /// </summary>
        public DependencyConfiguration SetBasePath(string basePath)
        {
            DefaultBasePath = basePath;
            return this;
        }

        private static InvalidConfigurationException Ex([CallerMemberName] string name = null)
        {
            return new InvalidConfigurationException(name);
        }
    }
}
