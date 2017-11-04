using AWSMagicMirror.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

namespace AWSMagicMirror.Controllers
{
    public class DefaultExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is UnauthorizedAccessException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                return;
            }

            if (context.Exception is ClientNotReachableException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.BadGateway);
                return;
            }

            var httpResponseException = context.Exception as HttpResponseException;
            if (httpResponseException != null)
            {
                Trace.WriteLine($"EXCEPTION ({httpResponseException.Response.StatusCode}): " + context.Exception);
                context.Response = httpResponseException.Response;
            }
            else
            {
                Trace.WriteLine("EXCEPTION:" + context.Exception);
                context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}