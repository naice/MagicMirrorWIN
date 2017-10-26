using System;
using System.Threading.Tasks;

namespace ConfigServer
{
    public interface IConfigurationContract
    {
        /// <summary>
        /// Type of your configuration object.
        /// </summary>
        Type ConfigurationType { get; }

        /// <summary>
        /// Validation function to check you configurations sanity, return string.Empty or null if OK or
        /// return a hitn for the user.
        /// </summary>
        string ConfigurationValidation(object newConfigurationObject);

        /// <summary>
        /// If the configuration is validated this will be called.
        /// </summary>
        Task ConfigurationUpdated(object newConfigurationObject);
        
        /// <summary>
        /// Requests the current configuration object.
        /// </summary>
        Task<object> ConfigurationRequest();

        /// <summary>
        /// Get the config schema.
        /// https://github.com/mozilla-services/react-jsonschema-form
        /// </summary>
        string GetConfigurationSchema();
        /// <summary>
        /// Get the config schema for the UI.
        /// https://github.com/mozilla-services/react-jsonschema-form
        /// </summary>
        string GetConfigurationUiSchema();
    }
}