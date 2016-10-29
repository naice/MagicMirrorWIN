using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.YouTube.Model
{
    public class BaseListResult<T> : BaseResult where T : class
    {
        public PageInfoResult PageInfo { get; set; }
        public string RegionCode { get; set; }
        public string NextPageToken { get; set; }

        public List<T> Items { get; set; } 
    }
}
