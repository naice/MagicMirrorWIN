using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Contracts
{
    public enum ApiResponseCode:int {
        Success = 0,
        Error = 1,
    }

    public class ApiResponse
    {
        public ApiResponseCode Error { get; set; }
        public JObject Result { get; set; }
    }
}
