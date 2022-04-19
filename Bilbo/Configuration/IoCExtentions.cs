using MiddleEarth.Infrastructure.DataAccessLayer;
using MiddleEarth.Infrastructure.Services;

namespace Bilbo.Configuration
{
    public static class IoCExtentions
    {
        public static IServiceCollection AddServices(this IServiceCollection service, IConfiguration configuration)
        {

            var sqlConn = configuration.GetConnectionString("SqlConnection");

            service.AddSqlServer<PhotoDbContext>(sqlConn);
            service.AddScoped<AzureStorageService>();
            service.AddScoped<ComputerVisionService>();
            service.AddSingleton<ComputerVisionMetricsService>();
            service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return service;
        }
    }
}
