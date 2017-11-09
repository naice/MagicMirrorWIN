using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NETStandard.RestServer
{
    /// <summary>
    /// Base class for Services.
    /// </summary>
    public abstract class RestServerService
    {
        /// <summary>
        /// The current request. Only safe to access while inside a <see cref="RestServerServiceCallAttribute"/> function.
        /// </summary>
        public HttpListenerRequest Request { get; set; }
        /// <summary>
        /// The current response. Only safe to access while inside a <see cref="RestServerServiceCallAttribute"/> function.
        /// </summary>
        public HttpListenerResponse Response { get; set; }
    }
}
