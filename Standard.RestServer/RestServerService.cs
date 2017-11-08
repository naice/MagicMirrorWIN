using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NETStandard.RestServer
{
    public abstract class RestServerService
    {
        public HttpListenerRequest Request { get; set; }
        public HttpListenerResponse Response { get; set; }

        public RestServerService()
        {

        }
    }
}
