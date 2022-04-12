using MiddleEarth.Infrastructure.DataAccessLayer;
using MiddleEarth.Infrastructure.Services;

namespace Gandalf.Configuration
{
    public static class IoCExtentions
    {
        public static IServiceCollection AddServices(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return service;
        }
    }
}
