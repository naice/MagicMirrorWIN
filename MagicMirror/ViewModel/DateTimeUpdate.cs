using System;
using System.Threading;
using System.Threading.Tasks;
using MagicMirror.Configuration;
using MagicMirror.Factory;

namespace MagicMirror.ViewModel
{
    public class DateTimeUpdate : IUpdateViewModel
    {
        public TimeSpan UpdateTimeout { get; set; } = TimeSpan.FromMinutes(60);
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public SemaphoreSlim UILock { get; set; } = new SemaphoreSlim(1, 1);

        public async Task<object> ProcessData(Configuration.Configuration config)
        {
            await DateTimeFactory.Instance.UpdateTimeAsync();
            return null;
        }

        public void UpdateUI(Configuration.Configuration config, object data)
        {
            // NOOP
        }
    }
}