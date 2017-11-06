using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Contracts
{
    public class ApiRequest
    {
        public JObject Parameter { get; set; }
    }
}
