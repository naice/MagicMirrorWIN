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
    public class DataReceived
    {
        public int ID { get; set; }
        public string PropName { get; set; }
    }

    [RestController(InstanceCreationType.Singleton)]
    public class ConigController
    {
        [UriFormat("/UpdateConfiguration")]
        public IGetResponse GetWithSimpleParameters()
        {
            //return new GetResponse(
            //  GetResponse.ResponseStatus.OK,
            //  new DataReceived() { ID = id, PropName = propName });
            return new GetResponse(GetResponse.ResponseStatus.OK, new DataReceived() { ID = 1, PropName = "TEST" });
        }
        [UriFormat("/GetTemplate")]
        public async Task<IGetResponse> GetTemplateJSON()
        {
            string templateJSON = "{\"title\":\"Todo\",\"type\":\"object\",\"required\":[\"title\"],\"properties\":{\"title\":{\"type\":\"string\",\"title\":\"Title\",\"default\":\"A new task\"},\"done\":{\"type\":\"boolean\",\"title\":\"Done ? \",\"default\":false}}}";

            JsonSchemaBuilder.Instance.Build(await DependencyConfiguration.DefaultConfigurationContract.ConfigurationRequest());
            //return new GetResponse(
            //  GetResponse.ResponseStatus.OK,
            //  new DataReceived() { ID = id, PropName = propName });
            return new GetResponse(GetResponse.ResponseStatus.OK, templateJSON);
        }
    }
}
