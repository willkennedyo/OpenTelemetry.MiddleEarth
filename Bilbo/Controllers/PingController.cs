using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Gandalf.Controllers
{
    [Route("/ping")]
    public class PingController: BilboBaseController
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public PingController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        [HttpGet]
        public string Get()
        {

            using var activity = _source.StartActivity(_sourceName, ActivityKind.Internal)!;
            activity.AddEvent(new ActivityEvent("Ping", tags: new ActivityTagsCollection(new[] { KeyValuePair.Create<string, object?>("Ping", DateTime.Now) })));
            activity.SetTag("otel.status_code", "OK");
            activity.SetTag("otel.status_description", "Ping successfully");

            return "OK";
        }
    }
}
