
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Loki;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;

namespace MiddleEarth.Infrastructure
{
    public static class SerilogExtension
    {
        public static IServiceCollection AddSerilogApi(this IServiceCollection services, IConfiguration configuration, string name)
        {
            var credentials = new NoAuthCredentials("http://loki:3100"); // Address to local or remote Loki server


            Serilog.ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithCorrelationId()
                .Enrich.WithProperty("Application", name)
                .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles"))
                .Filter.ByExcluding(z => z.MessageTemplate.Text.Contains("Business error"))
                .WriteTo.LokiHttp(new NoAuthCredentials("http://loki:3100"))
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .CreateLogger();

            services.AddSingleton(logger);

            var logEvent = new LogEvent(Guid.NewGuid(), Guid.NewGuid());

            services.Add(new ServiceDescriptor(typeof(ILogger), new SerilogLogger(logger, logEvent)));

            return services;

        }
    }
    public class SerilogLogger : ILogger
    {
        private Serilog.ILogger _logger;
        private IHttpContextAccessor _httpContextAccessor;

        private readonly LogEvent _logEvent;

        private const string _messageTemplate = "{TraceId:l}, {ParentSpanId:l}, {SpanId:l}, {RequestId:l},{Message:l},{Params:l}";

        public SerilogLogger(Serilog.ILogger logger, LogEvent logEvent, IHttpContextAccessor httpContextAccessor = null)
        {
            _logger = logger;
            _logEvent = logEvent;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Register the context in logger
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetLoggerContext<T>()
        {
            _logger = _logger.ForContext<T>();
        }

        /// <summary>
        /// Set trace context in log event to register correlations ids 
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public void SetTraceContext(IHttpContextAccessor httpContextAccessor)
        {
            _logEvent.SetTraceContext(httpContextAccessor);
        }

        #region Debug

        public void DebugWithMessageTemplate(string messageTemplate, params object[] extraParams)
            => _logger.Debug(messageTemplate, new { Params = JsonConvert.SerializeObject(extraParams) });

        /// <summary>
        /// Write a log in debug level
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="callerName">Caller method name</param>
        /// <param name="extraParams">Extra params if necessary</param>
        public void Debug(string message, [CallerMemberName] string callerName = "", params object[] extraParams)
        {
            if (callerName != null)
                message = $"{callerName}:{message}";
            _logger.Debug(_messageTemplate, _logEvent.TraceId, _logEvent.ParentSpanId, _logEvent.SpanId,  _logEvent.RequestId,message, new { Params = JsonConvert.SerializeObject(extraParams) });
        }

        #endregion

        #region Information

        public void InformationWithMessageTemplate(string messageTemplate, params object[] extraParams)
            => _logger.Information(messageTemplate, new { Params = JsonConvert.SerializeObject(extraParams) });

        /// <summary>
        /// Write a log in information level
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="callerName">Caller method name</param>
        /// <param name="extraParams">Extra params if necessary</param>
        public void Information(string message, [CallerMemberName] string callerName = "", params object[] extraParams)
        {
            if (callerName != null)
                message = $"{callerName}:{message}"; 
            _logger.Information(_messageTemplate, _logEvent.TraceId, _logEvent.ParentSpanId, _logEvent.SpanId,  _logEvent.RequestId,message, extraParams);
        }

        #endregion

        #region Warning

        public void WarningWithMessageTemplate(string messageTemplate, Exception exception = null, params object[] extraParams)
            => _logger.Warning(exception, messageTemplate, new { Params = JsonConvert.SerializeObject(extraParams) });

        /// <summary>
        /// Write a log in warning level
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="callerName">Caller method name</param>
        /// <param name="extraParams">Extra params if necessary</param>
        public void Warning(string message, [CallerMemberName] string callerName = "", params object[] extraParams)
            => Warning(message, null, callerName, extraParams);

        /// <summary>
        /// Write a log in warning level
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="exception">Exception message</param>
        /// <param name="callerName">Caller method name</param>
        /// <param name="extraParams">Extra params if necessary</param>
        public void Warning(string message, Exception exception, [CallerMemberName] string callerName = "", params object[] extraParams)
        {
            if (callerName != null)
                message = $"{callerName}:{message}";
            _logger.Warning(exception, _messageTemplate, _logEvent.TraceId, _logEvent.ParentSpanId, _logEvent.SpanId,  _logEvent.RequestId, message, extraParams);
        }

        #endregion

        #region Error

        public void ErrorWithMessageTemplate(string messageTemplate, Exception exception = null, params object[] extraParams)
            => _logger.Error(exception, messageTemplate, new { Params = JsonConvert.SerializeObject(extraParams) });

        /// <summary>
        /// Write a log in error level
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="callerName">Caller method name</param>
        /// <param name="extraParams">Extra params if necessary</param>
        public void Error(string message, [CallerMemberName] string callerName = "", params object[] extraParams)
           => Error(message, null, callerName, extraParams);

        /// <summary>
        /// Write a log in error level
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="exception">Exception message</param>
        /// <param name="callerName">Caller method name</param>
        /// <param name="extraParams">Extra params if necessary</param>
        public void Error(string message, Exception exception, [CallerMemberName] string callerName = "", params object[] extraParams)
        {
            if (callerName != null)
                message = $"{callerName}:{message}";
            _logger.Error(exception, _messageTemplate, _logEvent.TraceId, _logEvent.ParentSpanId, _logEvent.SpanId,  _logEvent.RequestId,message, new { Params = JsonConvert.SerializeObject(extraParams) });
        }

        public void Write(Serilog.Events.LogEvent logEvent)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class LogEvent
    {
        public string TraceId { get; private set; }
        public string RequestId { get; private set; }
        public string SpanId { get; private set; }
        public string ParentSpanId { get; private set; }

        public LogEvent() { }

        public LogEvent(Guid traceId, Guid spanId)
        {
            TraceId = traceId.ToString("N");
            SpanId = spanId.ToString("N").Substring(0, 16);
        }

        public void SetTraceContext(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
                return;

            SetSpanId();
            SetParentSpanId(httpContextAccessor);
            SetRequestId();
        }

        public void SetRequestId()
        {
            TraceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        }

        public void SetSpanId()
        {
            SpanId = Guid.NewGuid().ToString("N").Substring(0, 16);
        }

        public void SetParentSpanId(IHttpContextAccessor httpContextAccessor)
        {
            ParentSpanId = httpContextAccessor.HttpContext.Request.Headers["ParentSpanId"].ToString();
        }

    }
}
