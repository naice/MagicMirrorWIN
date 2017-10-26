using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.YouTube.Model
{
    public class SnippetResult
    {
        public DateTime PublishedAt { get; set; }
        public string ChannelId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ThumbnailsResult Thumbnails { get; set; }
        public string ChannelTitle { get; set; }
        public string LiveBroadcastContent { get; set; }
    }
}
