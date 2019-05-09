using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicMirror.ViewModel
{
    interface ILazyTimedUpdate
    {
        /// <summary>
        /// Timeout for the update.
        /// </summary>
        TimeSpan UpdateTimeout { get; set; }
        /// <summary>
        /// Timestamp of the last update.
        /// </summary>
        DateTime LastUpdate { get; set; }

        Task<object> Update(Configuration.Configuration config);
    }

    interface IUpdateViewModel : ILazyTimedUpdate
    {
        SemaphoreSlim UILock { get; set; }
        void UpdateUI(Configuration.Configuration config, object data);
    }
}
