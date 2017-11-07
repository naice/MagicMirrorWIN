using MagicMirror.Contracts;
using MagicMirror.Services.Cloud;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Services.Amazon
{
    [CloudServiceInstance(CloudServiceInstanceType.SingletonLazy)]
    public class AlexaApiController : CloudService
    {
        private readonly ISpeechRecognitionManager _speechRecognitionManager;

        public AlexaApiController(ISpeechRecognitionManager speechRecognitionManager)
        {
            _speechRecognitionManager = speechRecognitionManager;
        }

        [CloudCall("Service/Amazon/ProcessSkillServiceRequest")]
        public async Task<ApiResponse> ProcessSkillServiceRequest(ApiRequest request)
        {
            if (_speechRecognitionManager == null)
            {
                return new ApiResponse()
                {
                    Error = ApiResponseCode.Error,
                    Result = null,
                };
            }

            var result = new Contracts.AmazonEcho.SkillServiceResponse();
            
            result.Response.ShouldEndSession = false;

            var serviceRequest = request.Parameter.ToObject<Contracts.AmazonEcho.SkillServiceRequest>();

            if (serviceRequest.Request.Type == "LaunchRequest")
            {
                // session is launched...
                result.Response.OutputSpeech.Text = "Läuft.";
            }
            else if (serviceRequest.Request.Type == "IntentRequest")
            {
                if (serviceRequest.Request.Intent.Name == "ShowWeatherDetail")
                {
                    await _speechRecognitionManager.OnRecognizedSpeech("WEATHER");
                }
                else if (serviceRequest.Request.Intent.Name == "ShowNewsDetail")
                {
                    await _speechRecognitionManager.OnRecognizedSpeech("NEWS");
                }
                else if (serviceRequest.Request.Intent.Name == "Back" ||
                         serviceRequest.Request.Intent.Name == "AMAZON.CancelIntent")
                {
                    await _speechRecognitionManager.OnRecognizedSpeech("BACK");
                }
                else
                {
                    result.Response.OutputSpeech.Text = "Dieser Intent ist noch nicht implementiert.";
                }
            }

            return new ApiResponse()
            {
                Error = ApiResponseCode.Success,
                Result = JObject.FromObject(result),
            };
        }
    }
}
