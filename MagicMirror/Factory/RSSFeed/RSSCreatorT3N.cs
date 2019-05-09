using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace MagicMirror.Factory.RSSFeed
{
    public class RSSCreatorT3N : IRSSItemCreator
    {
        const int MAX_NEWS_LEN = 300;

        public string Name { get; } = "t3n";

        public Uri Url { get; set; } = new Uri("http://t3n.de/news/feed/", UriKind.Absolute);

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<ViewModel.NewsItem> CreateItem(ViewModel.NewsItem result, SyndicationItem item)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            result.URIToSource = item.Id;
            return result;
        }
    }
}
