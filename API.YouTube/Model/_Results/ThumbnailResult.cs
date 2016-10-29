using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.YouTube.Model
{
    public class ThumbnailResult
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Url) && Width > 0 && Height > 0;
        }
    }
}
