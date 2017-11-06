using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Services.Cloud
{
    public class CloudHttpContext
    {
        public HttpListenerRequest Request { get; private set; }
        public HttpListenerResponse Response { get; private set; }

        public CloudHttpContext(HttpListenerRequest request, HttpListenerResponse response)
        {
            Request = request;
            Response = response;
        }
    }

}
