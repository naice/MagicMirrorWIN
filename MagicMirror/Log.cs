using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MagicMirror
{
    public enum LogType : int { All = 0, App = 1, Communication = 2, Mediator = 3, Database = 4 }
    public enum LogLevel : int { All=0, Info=1, Warning=2, Exception=3 }

    public class LogEventArgs : EventArgs
    {
        public LogType Type { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
    }

    static class Log
    {
        public static event EventHandler<LogEventArgs> LogEvent = null;

        const string LN = "****************************************************************************************************";
        const string HD = "*                                           EXCEPTION                                              *";
        const string E = "!E ";
        const string W = "!W ";
        const string I = "!I ";

        private static string makeLines(string s, string prefix = null, string suffix = null)
        {
            StringBuilder sb = new StringBuilder();

            string[] l = Regex.Split(s, Environment.NewLine, RegexOptions.IgnoreCase);

            foreach (var line in l)
            {
                sb.AppendLine((prefix ?? "") + line + (suffix ?? ""));
            }

            return sb.ToString();
        }

        public static void Exception(int logType, Exception ex)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(E + LN);
            sb.AppendLine(E + HD);
            sb.AppendLine(E + LN);
            sb.AppendLine(E + "* MESSAGE: ");
            sb.Append(makeLines(ex.Message, E + "*    "));
            sb.AppendLine(E + LN);
            sb.AppendLine(E + "* STACKTRACE: ");
            sb.Append(makeLines(ex.StackTrace, E + "*"));
            sb.AppendLine(E + LN);

            writeLine((LogType)logType, LogLevel.Exception, sb.ToString());
        }
        public static void Warning(int logType, string msg, params object[] args)
        {
            writeLine((LogType)logType, LogLevel.Warning, W + string.Format(msg, args));
        }
        public static void Information(int logType, string msg, params object[] args)
        {
            writeLine((LogType)logType, LogLevel.Info, I + string.Format(msg, args));
        }

        public static void e(Exception ex)
        {
            Exception((int)LogType.App, ex);
        }
        public static void i(string msg, params object[] args)
        {
            Information((int)LogType.App, msg, args);
        }
        public static void w(string msg, params object[] args)
        {
            Warning((int)LogType.App, msg, args);
        }

        private static void writeLine(LogType type, LogLevel level, string line)
        {
            if (LogEvent != null)
                LogEvent.Invoke(typeof(Log), new LogEventArgs() { Level = level, Message = line, Type = type });

            Debug.WriteLine(line);
        }
    }
}
