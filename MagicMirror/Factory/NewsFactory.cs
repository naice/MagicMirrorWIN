using MagicMirror.Factory.RSSFeed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace MagicMirror.Factory
{
    public class NewsFactory
    {
        private SyndicationClient _SyndicationClient = new SyndicationClient();

        public async Task<List<ViewModel.NewsItem>> GetNewsAsync(Configuration.Configuration config)
        {
            List<ViewModel.NewsItem> newsItems = new List<ViewModel.NewsItem>();
            foreach (var creator in config.NewsFeeds)
            {
                try
                {
                    var feed = await _SyndicationClient.RetrieveFeedAsync(creator.Url);
                    if (feed != null && feed.Items != null && feed.Items.Count > 0)
                    {
                        foreach (var item in feed.Items)
                        {
                            try
                            {
                                ViewModel.NewsItem rssItem = new ViewModel.NewsItem()
                                {
                                    Source = creator.Name,
                                    Title = item.Title?.Text ?? string.Empty,
                                };
                                string rawContent = item.Content?.Text ?? item.Summary?.Text ?? string.Empty;
                                rssItem.ContentRaw = rawContent ?? rssItem.ContentRaw;
                                rssItem.Created = item.PublishedDate.DateTime;

                                rssItem = await creator.CreateItem(rssItem, item);
                                rssItem.GenerateID();
                                newsItems.Add(rssItem);
                            }
                            catch (Exception ex)
                            {
                                Log.w("Problem with newsfeed item {0}, {1}, {2}", item.Title, creator.Name, creator.Url);
                                Log.e(ex);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.w("Problem with newsfeed {0}, {1}", creator.Name, creator.Url);
                    Log.e(ex);
                }
            }

            newsItems.Sort((A, B) => A.Created.CompareTo(B.Created));
            return newsItems;
        }
    }
}
