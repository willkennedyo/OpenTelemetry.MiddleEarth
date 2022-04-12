using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Gandalf.Controllers
{
    public class BilboBaseController : ControllerBase
    {
        protected readonly string _sourceName = "Bilbo";
        protected readonly ActivitySource _source;

        protected readonly IHttpContextAccessor _httpContextAccessor;

        public BilboBaseController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _source = new ActivitySource(_sourceName);
        }
    }
}
