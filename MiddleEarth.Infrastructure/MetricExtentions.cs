using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MiddleEarth.Infrastructure
{
    public static class MetricExtentions
    {
        public static IServiceCollection AddOpentelemetry(this IServiceCollection service, IConfiguration configuration, string sourceName)
        {
            service.AddOpenTelemetryTracing(options =>
              options
                 .AddSource(sourceName)
                 .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(sourceName).AddTelemetrySdk())
                 .AddSqlClientInstrumentation(options =>
                 {
                     options.SetDbStatementForText = true;
                     options.RecordException = true;
                 })
                 .AddAspNetCoreInstrumentation(options =>
                 {
                     options.Filter = (req) => !req.Request.Path.ToUriComponent().Contains("index.html", StringComparison.OrdinalIgnoreCase)
                         && !req.Request.Path.ToUriComponent().Contains("swagger", StringComparison.OrdinalIgnoreCase);
                 })
                 .AddHttpClientInstrumentation()
                 .AddOtlpExporter(otlpOptions =>
                 {
                     otlpOptions.Endpoint = new Uri(configuration.GetValue<string>("AppSettings:OtelEndpoint"));
                 })
             );

            service.AddOpenTelemetryMetrics(options =>
                options.AddHttpClientInstrumentation()
                 .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(sourceName).AddTelemetrySdk())
                 .AddMeter("ComputerVision")
                 .AddOtlpExporter(otlpOptions =>
                 {
                     otlpOptions.Endpoint = new Uri(configuration.GetValue<string>("AppSettings:OtelEndpoint"));
                 })
            );
            service.Configure<AspNetCoreInstrumentationOptions>(options =>
            {
                options.RecordException = true;
            });
            return service;

        }
    }
}