using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.ViewModel
{
    /// <summary>
    /// helper class :-)
    /// </summary>
    public static class UI
    {
        public static async void EnsureOn(Windows.UI.Core.DispatchedHandler callback)
        {
            await EnsureOnAsync(callback);
        }
        public static async Task EnsureOnAsync(Windows.UI.Core.DispatchedHandler callback)
        {
            if (App.Dispatcher.HasThreadAccess)
            {
                callback.Invoke();
            }
            else
            {
                await App.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, callback);
            }
        }
    }
}
