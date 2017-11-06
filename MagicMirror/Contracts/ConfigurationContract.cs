using NcodedUniversal.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Contracts
{
    internal class ConfigurationContract : ConfigServer.IConfigurationContract
    {
        public Storage<Configuration.Configuration> ConfigurationStorage = new Storage<Configuration.Configuration>("config.json");
        public Type ConfigurationType => typeof(Configuration.Configuration);

        public async Task<object> ConfigurationRequest()
        {
            return await ConfigurationStorage.Get();
        }
        public async Task ConfigurationUpdated(object newConfigurationObject)
        {
            await ConfigurationStorage.Replace(newConfigurationObject as Configuration.Configuration);
        }
        public string ConfigurationValidation(object newConfigurationObject)
        {
            // TODO: Implement sanity checks
            return null;
        }

        #region Schema
        static string FromRessource(string path)
        {
            var assembly = typeof(ConfigurationContract).GetTypeInfo().Assembly;
            using (var sr = new StreamReader(assembly.GetManifestResourceStream(path)))
            {
                return sr.ReadToEnd();
            }
        }
        static Lazy<string> SCHEMA = new Lazy<string>(() => FromRessource("MagicMirror.Config.ConfigurationSchema.json"));
        static Lazy<string> UI_SCHEMA = new Lazy<string>(() => FromRessource("MagicMirror.Config.ConfigurationUiSchema.json"));
        public string GetConfigurationSchema()
        {
            return SCHEMA.Value;
        }
        public string GetConfigurationUiSchema()
        {
            return UI_SCHEMA.Value;
        }
        #endregion
    }
}
