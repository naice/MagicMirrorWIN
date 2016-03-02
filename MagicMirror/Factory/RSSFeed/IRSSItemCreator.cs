using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace MagicMirror.Factory.RSSFeed
{
    public interface IRSSItemCreator
    {
        string Name { get; }
        Uri Url { get; set; }
        Task<ViewModel.NewsItem> CreateItem(ViewModel.NewsItem result, SyndicationItem item);
    }
}
