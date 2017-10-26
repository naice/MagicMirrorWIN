using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;

namespace ConfigServer
{
    internal class Log
    {
        #region SINGLETON
        private static Log _Instance;
        public static Log Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Log();
                return _Instance;
            }
        }
        Log()
        {
            _GUID = Guid.Parse("8F793392-0B0A-4B60-AE32-24E496DA4D15");
            _channel = new LoggingChannel("ConfigServer", new LoggingChannelOptions(_GUID));
        }
        ~Log()
        {

        }
        #endregion

        private readonly Guid _GUID;
        private readonly LoggingChannel _channel;

        private void Write(string msg, object[] args, LoggingLevel level)
        {
            _channel.LogMessage(string.Format(msg, args), level);
        }
        public static void I(string msg, params object[] args)
        {
            Instance.Write(msg, args, LoggingLevel.Information);
        }
        public static void E(string msg, params object[] args)
        {
            Instance.Write(msg, args, LoggingLevel.Error);
        }
        public static void W(string msg, params object[] args)
        {
            Instance.Write(msg, args, LoggingLevel.Warning);
        }
        public static void FATAL(string msg, params object[] args)
        {
            Instance.Write(msg, args, LoggingLevel.Critical);
        }
    }
}
