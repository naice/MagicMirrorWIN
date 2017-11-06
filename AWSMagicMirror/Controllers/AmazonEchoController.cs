using MagicMirror.Contracts.AmazonEcho;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AWSMagicMirror.Services;
using AWSMagicMirror.Services.Exceptions;
using System.Net.Http.Headers;
using MagicMirror.Contracts;
using Newtonsoft.Json.Linq;

namespace AWSMagicMirror.Controllers
{
    [DefaultExceptionFilter]
    public class AmazonEchoController : ApiController
    {
        private readonly AccessControlService _accessControlService;
        private readonly ClientService _clientService;

        public AmazonEchoController(AccessControlService accessControlService, ClientService clientService)
        {
            _accessControlService = accessControlService;
            _clientService = clientService;
        }

        public async Task<HttpResponseMessage> ExecuteIntent([FromBody] SkillServiceRequest request)
        {
            try
            {
                var clientId = _accessControlService.GetClientUidFromAmazonUserId(request.Session.User.UserId);
                if (clientId == null)
                {
                    throw new UnauthorizedAccessException();
                }

                // clear request user id.
                request.Session.User.UserId = null;
                var apiRequest = new ApiRequest
                {
                    Parameter = JObject.FromObject(request),
                };

                var apiResponse = await _clientService.SendRequestAsync(clientId.Value, apiRequest, TimeSpan.FromSeconds(5));
                if (apiResponse.Error == ApiResponseCode.Success)
                {
                    return CreateJsonResponse(apiResponse.Result.ToObject<SkillServiceResponse>());
                }

                return CreateJsonResponse("Dein Spiegel hat einen Fehler gemeldet, Jens!");
            }
            catch (ClientNotReachableException)
            {
                return CreateJsonResponse("Es tut mir leid, ich konnte deinen Spiegel nicht erreichen. Versuche es später noch einmal.");
            }
        }

        private static HttpResponseMessage CreateJsonResponse(string answer)
        {
            var response = new SkillServiceResponse();
            response.Response.OutputSpeech.Text = answer;

            return CreateJsonResponse(response);
        }
        private static HttpResponseMessage CreateJsonResponse(SkillServiceResponse skillServiceResponse)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var dataString = JsonConvert.SerializeObject(skillServiceResponse, serializerSettings);

            var content = new StringContent(dataString);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            return response;
        }
    }
}
