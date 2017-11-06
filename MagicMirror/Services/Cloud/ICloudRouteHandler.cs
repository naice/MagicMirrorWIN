using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Services.Cloud
{
    public interface ICloudRouteHandler
    {
        /// <summary>
        /// Handles a route. If a request is not closed must return false! Return true, if request is handled, 
        /// otherwise other handlers would process your context.
        /// </summary>
        Task<bool> HandleRouteAsync(CloudHttpContext context);
    }
}
