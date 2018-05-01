using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NETStandard.RestServer;

namespace ConfigServer
{
    public class UpdateConfigurationResponse
    {
        public bool Success { get; set; }
        public string UserHint { get; set; }
    }

    public class GetTemplateResponse
    {
        public string JsonSchema { get; set; }
        public string UiSchema { get; set; }
        public object CurrentState { get; set; }
    }

    [RestServerServiceInstance(RestServerServiceInstanceType.SingletonLazy)]
    public class ConfigService : RestServerService
    {
        [RestServerServiceCall("/UpdateConfiguration")]
        public async Task<UpdateConfigurationResponse> UpdateConfiguration(string json)
        {
            try
            {
                var configContract = DependencyConfiguration.DefaultConfigurationContract;
                var jsonConvert = DependencyConfiguration.DefaultJsonConverter;
                var typeToBuild = configContract.ConfigurationType;
                var newConfig = jsonConvert.DeserializeObject(json, typeToBuild);
                var userHint = configContract.ConfigurationValidation(newConfig);

                if (string.IsNullOrEmpty(userHint))
                {
                    await configContract.ConfigurationUpdated(newConfig);
                }

                return new UpdateConfigurationResponse()
                {
                    Success = string.IsNullOrEmpty(userHint),
                    UserHint = userHint,
                };
            }
            catch (Exception ex)
            {

                return new UpdateConfigurationResponse()
                {
                    Success = false,
                    UserHint = ex.Message,
                };
            }
        }

        [RestServerServiceCall("/GetTemplate")]
        public async Task<GetTemplateResponse> GetTemplate()
        {
            Log.I("API: GetTemplate");
            var configContract = DependencyConfiguration.DefaultConfigurationContract;

            var getTemplateResponse = new GetTemplateResponse()
            {
                JsonSchema = configContract.GetConfigurationSchema(),
                UiSchema = configContract.GetConfigurationUiSchema(),
                CurrentState = await configContract.ConfigurationRequest(),
            };
            
            return getTemplateResponse;
        }
    }
}
