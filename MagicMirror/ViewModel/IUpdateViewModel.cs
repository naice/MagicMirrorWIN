using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicMirror.ViewModel
{
    interface IUpdateViewModel
    {
        TimeSpan UpdateTimeout { get; set; }
        DateTime LastUpdate { get; set; }
        SemaphoreSlim UILock { get; set; }

        Task<object> ProcessData(Configuration.Configuration config);

        void UpdateUI(Configuration.Configuration config, object data);
    }
}
