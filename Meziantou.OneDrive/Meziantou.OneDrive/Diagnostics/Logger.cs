using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Meziantou.OneDrive.Diagnostics
{
    public static class Logger
    {
        public static ILogger CurrentLogger { get; set; }

        static Logger()
        {
            CurrentLogger = new EtwLogger();
        }

        [Conditional("TRACE")]
        public static void Log(LogComponent logComponent, LogType type, int indent = 0, [CallerMemberName]string methodName = null, object value = null, IDictionary<string, object> context = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            CurrentLogger?.Log(logComponent, type, indent, methodName, value, context);
        }
    }
}
