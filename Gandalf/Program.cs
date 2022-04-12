using System.Diagnostics;
using Gandalf.Configuration;
using MiddleEarth.Infrastructure;
using MiddleEarth.Infrastructure.Filters;
using Serilog;

const string SourceName = "Gandalf";
const string MeterName = "ComputerVision";

try
{

    var builder = WebApplication.CreateBuilder(args);

    string appName = AppDomain.CurrentDomain.FriendlyName;

    builder.Services.AddSerilogApi(builder.Configuration, SourceName);
    builder.Host.UseSerilog();
       
    builder.Services
        .AddServices(builder.Configuration)
        .AddOpentelemetry(builder.Configuration, SourceName);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options => options.OperationFilter<ImageExtensionFilter>());
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    var app = builder.Build();

    Console.WriteLine(Figgle.FiggleFonts.Doom.Render(SourceName));

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gandalf API v1");
    });

    app.UseRouting();


    app.UseAuthorization();

    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("Request-Id", Activity.Current?.TraceId.ToString() ?? string.Empty);

        await next();
    });

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}