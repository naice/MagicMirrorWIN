using API.YouTube.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.YouTube.Model
{
    public class SearchList<T> : IRequest<T>
    {
        public string Part { get; set; } = "snippet";
        public string Order { get; set; } = "viewCount";
        public string Type { get; set; } = "video";
        public string VideoDefinition { get; set; } = "high";
        public bool VideoEmbeddable { get; set; } = true;
        public int MaxResults { get; set; } = 50;
        public string Q { get; set; }
        public string Key { get; set; }
        public string PageToken { get; set; }


        public SearchList(string query)
        {
            Q = query;
        }

        public string GetRequestUrl()
        {
            if (string.IsNullOrEmpty(Key))
                Key = Constants.APIKey;

            return Constants.APIURL + "search?" + Provider.Properties.GetQuery(this);
        }


    }
}
