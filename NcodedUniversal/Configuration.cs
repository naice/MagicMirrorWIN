using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NcodedUniversal
{
    /// <summary>
    /// Fluent configuration for NcodedXMobile-Framework dependencys
    /// </summary>
    public class Configuration
    {
        private static Converter.IJsonConvert _DefaultJsonConverter;
        internal static Converter.IJsonConvert DefaultJsonConverter
        {
            get { return _DefaultJsonConverter ?? throw Ex(); }
            private set { _DefaultJsonConverter = value; }
        }
        private static Storage.IStorageIO _DefaultStorageIO;
        internal static Storage.IStorageIO DefaultStorageIO
        {
            get { return _DefaultStorageIO ?? throw Ex(); }
            private set { _DefaultStorageIO = value; }
        }

        /// <summary>
        /// Begin configuration.
        /// </summary>
        public static Configuration Begin()
            => new Configuration();

        private Configuration() { }

        /// <summary>
        /// Set the default <see cref="Converter.IJsonConvert"/> for the framework.
        /// </summary>
        public Configuration Set(Converter.IJsonConvert jsonConverter)
        {
            DefaultJsonConverter = jsonConverter ?? throw new ArgumentException(nameof(jsonConverter));
            return this;
        }

        /// <summary>
        /// Set the default <see cref="Model.IStorageIO"/> for the framework.
        /// </summary
        public Configuration Set(Storage.IStorageIO storageIO)
        {
            DefaultStorageIO = storageIO ?? throw new ArgumentException(nameof(storageIO));
            return this;
        }
        private static InvalidConfigurationException Ex([CallerMemberName] string name = null)
        {
            return new InvalidConfigurationException(name);
        }
    }
}
