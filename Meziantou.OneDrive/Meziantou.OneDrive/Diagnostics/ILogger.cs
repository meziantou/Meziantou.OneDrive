using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Meziantou.OneDrive.Diagnostics
{
    public interface ILogger
    {
        void Log(LogComponent logComponent, LogType type, int indent = 0, [CallerMemberName]string methodName = null, object value = null, IDictionary<string, object> context = null);
    }
}
