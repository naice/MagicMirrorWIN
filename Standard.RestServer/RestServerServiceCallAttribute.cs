using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETStandard.RestServer
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class RestServerServiceCallAttribute : Attribute
    {
        readonly string route;
        readonly string methods;


        public RestServerServiceCallAttribute(string route, string methods = "POST,GET,PUT")
        {
            this.route = route;
            this.methods = methods;
        }

        public string Route
        {
            get { return route; }
        }
        public string Methods
        {
            get { return methods; }
        }
    }
}
