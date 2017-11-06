using MagicMirror.Services.Cloud;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Services.Amazon
{
    [CloudServiceInstance(CloudServiceInstanceType.SingletonStrict)]
    public class AlexaApiController : CloudService
    {
        [CloudCall("Service/Amazon/ProcessSkillServiceRequest")]
        public async Task<Contracts.ApiResponse> ProcessSkillServiceRequest(Contracts.ApiRequest request)
        {
            var serviceRequest = request.Parameter.ToObject<Contracts.AmazonEcho.SkillServiceRequest>();


            var result = new Contracts.AmazonEcho.SkillServiceResponse();
            result.Response.OutputSpeech.Text = "Spieglein, Spieglein an der Wand!";
            result.Response.ShouldEndSession = false;

            return new Contracts.ApiResponse()
            {
                Error = Contracts.ApiResponseCode.Success,
                Result = JObject.FromObject(result),
            };
        }
    }
}
