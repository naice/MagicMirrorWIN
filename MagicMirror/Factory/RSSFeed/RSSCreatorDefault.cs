using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace MagicMirror.Factory.RSSFeed
{
    public class RSSCreatorDefault : IRSSItemCreator
    {
        const int MAX_NEWS_LEN = 300;

        public string Name { get; } = "default";

        public Uri Url { get; set; } = new Uri("http://t3n.de/news/feed/", UriKind.Absolute);

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<ViewModel.NewsItem> CreateItem(ViewModel.NewsItem result, SyndicationItem item)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (!string.IsNullOrEmpty(result.ContentRaw))
                result.ContentRaw = string.Format(GenericHtml.HTML, result.ContentRaw);
            else if (!string.IsNullOrEmpty(result.Content))
                result.ContentRaw = string.Format(GenericHtml.HTML, result.Content);


            return result;
        }
    }
}
