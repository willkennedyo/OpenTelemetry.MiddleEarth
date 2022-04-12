using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Gandalf.Controllers
{

    public class GandalfBaseController : ControllerBase
    {
        protected readonly string _sourceName = "Gandalf";
        protected readonly ActivitySource _source;

        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly MiddleEarth.Infrastructure.ILogger _logger;

        public GandalfBaseController(IHttpContextAccessor httpContextAccessor, MiddleEarth.Infrastructure.ILogger logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _source = new ActivitySource(_sourceName);
            _logger = logger;
            _logger.SetTraceContext(httpContextAccessor);
        }
    }
}
