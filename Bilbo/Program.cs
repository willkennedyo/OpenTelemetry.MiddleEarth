using Microsoft.EntityFrameworkCore;
using MiddleEarth.Infrastructure.DataAccessLayer;
using MiddleEarth.Infrastructure;
using Gandalf.Configuration;
using System.Diagnostics;
using MiddleEarth.Infrastructure.Filters;

const string SourceName = "Bilbo";
const string MeterName = "ComputerVision";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddServices(builder.Configuration)
    .AddOpentelemetry(builder.Configuration, SourceName);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.OperationFilter<ImageExtensionFilter>());
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

Console.WriteLine(Figgle.FiggleFonts.Doom.Render(SourceName));

await EnsureDbAsync(app.Services);


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = string.Empty;
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Bilbo API v1");
});
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Request-Id", Activity.Current?.TraceId.ToString() ?? string.Empty);

    await next();
});

app.Run();


static async Task EnsureDbAsync(IServiceProvider services)
{
    using var db = services.CreateScope().ServiceProvider.GetRequiredService<PhotoDbContext>();
    await db.Database.MigrateAsync();
}