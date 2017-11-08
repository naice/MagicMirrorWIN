using System;
using System.Collections.Generic;
using System.Text;

namespace NETStandard.RestServer
{
    public class Log 
    {
        public static ILogger DefaultLogger { get; set; } = new DefaultLoggerImplementation();

        internal static void i(string msg, params object[] args)
        {
            if (DefaultLogger == null) return;

            WriteLine("INFO      " + string.Format(msg, args));
        }
        internal static void e(string msg, params object[] args)
        {
            if (DefaultLogger == null) return;
            
            WriteLine("ERROR     " + string.Format(msg, args));
        }
        internal static void w(string msg, params object[] args)
        {
            if (DefaultLogger == null) return;
            
            WriteLine("WARNING   " + string.Format(msg, args));
        }
        /*
        internal static void ex(string msg, params object[] args)
        {
            if (DefaultLogger == null) return;

            WriteLine("EXCEPTION " + string.Format(msg, args));
        }
        */

        private static void WriteLine(string msg)
        {
            DefaultLogger.WriteLine($"[{DateTime.Now:yyyy-MM-ddTHH:mm:ss.fffffff}] {msg}");
        }

        private class DefaultLoggerImplementation : ILogger
        {
            public void Write(string message)
            {
                System.Diagnostics.Trace.Write(message);
            }

            public void WriteLine(string message)
            {
                System.Diagnostics.Trace.WriteLine(message);
            }
        }
    }
}
