using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace MagicMirror.Factory.RSSFeed
{
    public class RSSCreatorGolem : IRSSItemCreator
    {
        public string Name { get; } = "Golem";

        public Uri Url { get; set; } = new Uri("http://rss.golem.de/rss.php?feed=RSS2.0", UriKind.Absolute);

        public async Task<ViewModel.NewsItem> CreateItem(ViewModel.NewsItem result, SyndicationItem item)
        {
            result.URIToSource = item.Id;

            return result;
        }
    }
}
