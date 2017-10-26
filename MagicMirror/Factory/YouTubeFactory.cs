using API.YouTube;
using API.YouTube.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Factory
{
    public class YouTubeFactory
    {
        private SearchListResult searchListResult = null;

        public async Task<SearchItemResult> GetFunnyVideo()
        {
            if (searchListResult == null || !searchListResult.Items.Any())
            {
                var requestInfo = new SearchList<SearchListResult>("fail compilation");

                if (searchListResult == null)
                {
                    searchListResult = await Request.Get(requestInfo);
                }
                else
                {
                    requestInfo.PageToken = searchListResult.NextPageToken;
                    searchListResult = await Request.Get(requestInfo);
                }
            }

            if (searchListResult == null || !searchListResult.Items.Any())
                return null;

            var result = searchListResult.Items.First();
            searchListResult.Items.RemoveAt(0);
            return result;
        }
    }
}
