using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Services.Cloud
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class CloudCallAttribute : Attribute
    {
        readonly string route;
        readonly string methods;


        public CloudCallAttribute(string route, string methods = "POST,GET,PUT")
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
