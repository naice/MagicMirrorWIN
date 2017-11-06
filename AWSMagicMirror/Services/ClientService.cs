using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MagicMirror.Contracts;
using AWSMagicMirror.Services.Exceptions;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AWSMagicMirror.Services
{
    public class ClientService
    {
        private readonly AccessControlService _accessControlService;
        private const string AMAZON_SKILL_SERVICE_ENDPOINT = "/Service/Amazon/ProcessSkillServiceRequest";

        public ClientService(AccessControlService accessControlService)
        {
            _accessControlService = accessControlService;
        }

        public async Task<ApiResponse> SendRequestAsync(Guid clientId, ApiRequest apiRequest, TimeSpan timeout)
        {
            var request = InternalSendRequestAsync(clientId, apiRequest, timeout);

            if (await Task.WhenAny(request, Task.Delay(timeout)) != request)
            {
                throw new ClientNotReachableException();
            }

            return request.Result;
        }

        private async Task<ApiResponse> InternalSendRequestAsync(Guid clientId, ApiRequest apiRequest, TimeSpan timeout)
        {
            string errorText = "";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = timeout.Add(new TimeSpan(0, 0, 30));
                    string requestUri = _accessControlService.GetClientURLFromClientId(clientId) + AMAZON_SKILL_SERVICE_ENDPOINT;

                    using (var response = await client.PostAsJsonAsync(requestUri: requestUri, value: apiRequest))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return JsonConvert.DeserializeObject<ApiResponse>(
                                await response.Content.ReadAsStringAsync());
                        }
                    }
                }
            }
            catch (TimeoutException) { }
            catch (Exception ex)
            {
                errorText = ex.Message;
                Trace.WriteLine($"InternalSendRequestAsync: Unhandled exeption. {ex.Message}");
            }

            return new ApiResponse() { Error = ApiResponseCode.Error, ErrorText=errorText };
        }
    }
}