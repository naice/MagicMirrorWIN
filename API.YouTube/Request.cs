using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace API.YouTube
{
    public static class Request
    {
        private const string CONTENTTYPE_JSON = "application/json";

        public static async Task<T> Get<T>(Contracts.IRequest<T> request) where T : class
        {
            var response = await Http(request.GetRequestUrl(), HttpMethod.Get);

            if (response == null || response.StatusCode != HttpStatusCode.Ok)
            {
                return null;
            }

            string rawText = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(rawText))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(rawText);
            }
            catch 
            {
                return null;
            }
        }

        internal static async Task<HttpResponseMessage> Http(string url, HttpMethod method, string json = "", CancellationTokenSource cancellationTokenSource = null)
        {
            try
            {
                Uri uri = new Uri(url, UriKind.Absolute);

                using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
                {
                    // HERE WE COULD ALLOW UNTRUSTED CERT.
                    filter.AutomaticDecompression = true;
                    using (HttpClient client = new HttpClient(filter))
                    {
                        using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, uri))
                        {
                            //requestMessage.Headers["Accept-Encoding"] = "gzip";
                            requestMessage.Headers.Accept.Add(
                                new HttpMediaTypeWithQualityHeaderValue("application/json"));
                            requestMessage.Headers.AcceptEncoding.Add(
                                new HttpContentCodingWithQualityHeaderValue("gzip"));
                            
                            using (HttpStringContent jsonContent = new HttpStringContent(json, Windows.Storage.Streams.UnicodeEncoding.Utf8, CONTENTTYPE_JSON))
                            {
                                if (!string.IsNullOrEmpty(json))
                                {
                                    requestMessage.Content = jsonContent;
                                }

                                if (cancellationTokenSource == null)
                                    cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));

                                return await client.SendRequestAsync(requestMessage)
                                    .AsTask(cancellationTokenSource.Token)
                                    .ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012851)
                {
                    // SELF SIGNED CERT
                    // EXPIRED CERT
                }
            }

            return null;
        }
    }
}
