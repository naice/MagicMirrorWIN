using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Factory
{
    public class DateTimeFactory
    {
        #region SINGLETON
        private static DateTimeFactory _Instance;
        public static DateTimeFactory Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new DateTimeFactory();
                return _Instance;
            }
        }
        DateTimeFactory()
        {

        }
        #endregion

        TimeSpan _offset = new TimeSpan(0, 0, 0);
        public TimeSpan CurrentOffset
        {
            get { return _offset; }
            private set { _offset = value; }
        }

        public DateTime Now
        {
            get
            {
                // CurrentOffset -= TimeSpan.FromMinutes(5); Testing only ;-)

                return DateTime.Now - CurrentOffset;
            }
        }

        void UpdateOffset(DateTime currentCorrectUtcTime)
        {
            CurrentOffset = DateTime.UtcNow - currentCorrectUtcTime;
        }

        public async void UpdateTime()
        {
            await UpdateTimeAsync();
        }

        public async Task UpdateTimeAsync()
        {
            Ntp.NtpClient client = new Ntp.NtpClient();

            try
            {
                var currentCorrectUtcTime = await client.GetNetworkTimeAsync();

                UpdateOffset(currentCorrectUtcTime);
            }
            catch (Exception ex)
            {
                Sentry.SentrySdk.CaptureException(ex);
                Log.e(ex);
            }
        }
    }
}
