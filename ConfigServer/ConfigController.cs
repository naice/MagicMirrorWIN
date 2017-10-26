using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    [RestController(InstanceCreationType.Singleton)]
    public class ConigController
    {
        [UriFormat("/UpdateConfiguration")]
        public async Task<IGetResponse> UpdateConfiguration(string json)
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

            return new GetResponse(GetResponse.ResponseStatus.OK,
                new UpdateConfigurationResponse()
                {
                    Success = string.IsNullOrEmpty(userHint),
                    UserHint = userHint,
                }
            );
        }

        [UriFormat("/GetTemplate")]
        public async Task<IGetResponse> GetTemplate()
        {
            Log.I("API: GetTemplate");
            var configContract = DependencyConfiguration.DefaultConfigurationContract;

            return new GetResponse(GetResponse.ResponseStatus.OK,
                new GetTemplateResponse()
                {
                    JsonSchema = configContract.GetConfigurationSchema(),
                    UiSchema = configContract.GetConfigurationUiSchema(),
                    CurrentState = await configContract.ConfigurationRequest(),
                }
            );
        }
    }
}
