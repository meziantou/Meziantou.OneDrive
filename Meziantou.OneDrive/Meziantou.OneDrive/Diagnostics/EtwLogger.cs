using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Meziantou.OneDrive.Diagnostics
{
    public class EtwLogger : ILogger, IDisposable
    {
        public static Guid ProviderGuid = new Guid("17A37928-B4F6-45E6-AC29-D03550A95097");
        private EventProvider _provider = new EventProvider(ProviderGuid);

        public virtual void Log(LogComponent logComponent, LogType type, int indent = 0, [CallerMemberName]string methodName = null, object value = null, IDictionary<string, object> context = null)
        {
            if (_provider == null)
                return;

            StringBuilder sb = new StringBuilder();
            string sindent = indent > 0 ? new string(' ', indent) : null;
            sb.Append("[" + logComponent + "]");
            if (!string.IsNullOrWhiteSpace(methodName))
            {
                methodName = "." + methodName;
            }

            sb.Append(sindent + "[" + Thread.CurrentThread.ManagedThreadId + "][" + type + "]" + methodName + ": " + value);
            sb.AppendLine();

            _provider.WriteMessageEvent(sb.ToString(), (byte)type, 0);
            //_provider.WriteMessageEvent(sb.ToString(), (byte)type, (long)component);
        }

        public void Dispose()
        {
            if (_provider != null)
            {
                _provider.Dispose();
                _provider = null;
            }
        }
    }
}
