using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiddleEarth.Infrastructure
{
    public interface ILogger
    {
        void SetLoggerContext<T>();

        void SetTraceContext(IHttpContextAccessor httpContextAccessor);

        void DebugWithMessageTemplate(string messageTemplate, params object[] extraParams);

        void Debug(string message, [CallerMemberName] string callerName = "", params object[] extraParams);

        void InformationWithMessageTemplate(string messageTemplate, params object[] extraParams);

        void Information(string message, [CallerMemberName] string callerName = "", params object[] extraParams);

        void WarningWithMessageTemplate(string messageTemplate, Exception exception = null, params object[] extraParams);

        void Warning(string message, Exception exception, [CallerMemberName] string callerName = "", params object[] extraParams);

        void Warning(string message, [CallerMemberName] string callerName = "", params object[] extraParams);

        void ErrorWithMessageTemplate(string messageTemplate, Exception exception = null, params object[] extraParams);

        void Error(string message, [CallerMemberName] string callerName = "", params object[] extraParams);

        void Error(string message, Exception exception, [CallerMemberName] string callerName = "", params object[] extraParams);
    }
}
