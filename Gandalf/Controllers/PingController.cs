using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;

namespace Gandalf.Controllers
{
    [Route("/ping")]
    public class PingController : GandalfBaseController
    {

        public PingController(IHttpContextAccessor httpContextAccessor, MiddleEarth.Infrastructure.ILogger logger) : base(httpContextAccessor, logger)
        {
        }

        [HttpGet]
        public string Get()
        {

            using var activity = _source.StartActivity(_sourceName, ActivityKind.Internal)!;
            activity.AddEvent(new ActivityEvent("Ping", tags: new ActivityTagsCollection(new[] { KeyValuePair.Create<string, object?>("Ping", DateTime.Now) })));
            activity.SetTag("otel.status_code", "OK");
            activity.SetTag("otel.status_description", "Ping successfully");

            _logger.Information($"Ping RequestId = {Activity.Current?.TraceId.ToString() ?? string.Empty}");
            return "OK";
        }

        [HttpGet("CallBilbo")]
        public async Task<IActionResult> GetAPingAsync()
        {
            using var activity = _source.StartActivity(_sourceName, ActivityKind.Internal)!;

            var tags = new[] { KeyValuePair.Create<string, object?>("Phrase", "Bilbo, we have a journey to do.") };
            var activities = new ActivityTagsCollection(tags);
            var @event = new ActivityEvent("Calling", tags: activities);

            string result = string.Empty;

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    var uri = "http://bilbo:8000/ping";
                    HttpResponseMessage response = await client.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        return Problem();
                    }
                }
                catch (Exception ex)
                {

                }

            }

                activity.AddEvent(@event);
                activity.SetTag("Words", "You Must Trust Yourself. Trust Your Own Strength.");
                activity.SetTag("otel.status_code", "OK");
                activity.SetTag("otel.status_description", "Ping Bilbo successfully");

            _logger.Information($"Ping RequestId = {Activity.Current?.TraceId.ToString() ?? string.Empty}");

            return Ok($"Bilbo, we have a journey to do.\n{Figgle.FiggleFonts.Doom.Render(result)}");
        }
    }
}
