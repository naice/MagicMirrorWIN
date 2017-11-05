using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Services.Cloud
{
    public abstract class CloudService
    {
        public HttpListenerRequest Request { get; set; }
        public HttpListenerResponse Response { get; set; }

        public CloudService()
        {

        }
    }
}
