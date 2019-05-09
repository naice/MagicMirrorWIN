using System;
using System.Threading;
using System.Threading.Tasks;
using MagicMirror.Configuration;
using MagicMirror.Factory;

namespace MagicMirror.ViewModel
{
    public class DateTimeUpdate : ILazyTimedUpdate
    {
        public TimeSpan UpdateTimeout { get; set; } = TimeSpan.FromMinutes(60);
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;

        public async Task<object> Update(Configuration.Configuration config)
        {
            await DateTimeFactory.Instance.UpdateTimeAsync();
            return null;
        }
    }
}