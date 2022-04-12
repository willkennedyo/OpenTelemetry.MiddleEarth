using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Gandalf.Controllers
{

    public class GandalfBaseController : ControllerBase
    {
        protected readonly string _sourceName = "Gandalf";
        protected readonly ActivitySource _source;

        protected readonly IHttpContextAccessor _httpContextAccessor;

        public GandalfBaseController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _source = new ActivitySource(_sourceName);
        }
    }
}
