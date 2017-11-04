using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Contracts
{
    public class ApiRequest
    {
        public string Action { get; set; }
        public string Parameter { get; set; }
    }
}
