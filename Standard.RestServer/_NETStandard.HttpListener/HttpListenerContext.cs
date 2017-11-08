

namespace System.Net.Http
{
    public class HttpListenerContext
    {
        public HttpListenerContext(HttpListenerRequest request, HttpListenerResponse response)
        {
            Request = request;
            Response = response;
        }

        
        public HttpListenerRequest Request { get; }

        
        public HttpListenerResponse Response { get; }
    }
}