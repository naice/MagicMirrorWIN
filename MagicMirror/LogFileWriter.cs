using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MagicMirror
{
    public class LogFileWriter
    {
        public LogFileWriter()
        {
            Log.LogEvent += Log_LogEvent;
        }

        private void Log_LogEvent(object sender, LogEventArgs e)
        {
            System.IO.File.AppendAllText("log.txt", $"[{DateTime.UtcNow:yyyyMMddTHHmmssZ}|{e.Level}|{e.Type}] {e.Message}" + Environment.NewLine);
        }
    }
}
