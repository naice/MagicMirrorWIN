using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.YouTube.Model
{
    public class ThumbnailsResult
    {
        public ThumbnailResult Default { get; set; }
        public ThumbnailResult Medium { get; set; }
        public ThumbnailResult High { get; set; }

        public IEnumerable<ThumbnailResult> GetAsEnumerable()
        {
            return new ThumbnailResult[]
            {
                Default, Medium, High,
            };
        }
        public ThumbnailResult GetHighDefinition()
        {
            return GetAsEnumerable()
                .Where(A => A != null && A.IsValid())
                .OrderByDescending(A => A.Height * A.Width)
                .FirstOrDefault();
        }
        public ThumbnailResult GetLowDefinition()
        {
            return GetAsEnumerable()
                .Where(A => A != null && A.IsValid())
                .OrderBy(A => A.Height * A.Width)
                .FirstOrDefault();
        }
    }
}
