using Microsoft.AspNetCore.Mvc;
using MiddleEarth.Domain;
using MimeMapping;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace Gandalf.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BilboController : GandalfBaseController
    {
        public BilboController(IHttpContextAccessor httpContextAccessor, MiddleEarth.Infrastructure.ILogger logger) : base(httpContextAccessor, logger)
        {
        }


        [HttpGet]
        public async Task<IEnumerable<Photo>> GetAllAsync()
        {
            try
            {
                _logger.Information($"Getting Photos");
                using var activity = _source.StartActivity(nameof(GetAllAsync), ActivityKind.Internal)!;
                IEnumerable<Photo> result = new List<Photo>();
                HttpStatusCode statuscode = HttpStatusCode.NotFound;
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        var uri = "http://bilbo:8000/Photos";
                        HttpResponseMessage response = await client.GetAsync(uri);
                        if (response.IsSuccessStatusCode && response?.Content != null)
                        {
                            result = await response.Content.ReadFromJsonAsync<IEnumerable<Photo>>();
                            statuscode = response.StatusCode;
                        }
                    }
                    catch (Exception ex)
                    {
                        statuscode = HttpStatusCode.InternalServerError;
                        activity.RecordException(ex);
                        activity.SetStatus(ActivityStatusCode.Error);
                        _logger.Error($"An error occured on Calling BilboApi", ex);
                    }
                    activity.SetStatus(ActivityStatusCode.Ok);
                    activity.AddEvent(new ActivityEvent("Load Photos", tags: new ActivityTagsCollection(new[] { KeyValuePair.Create<string, object?>("Count", result.Count()) })));
                    activity.SetTag("photoNumbers", result.Count());
                    activity.SetTag("otel.status_code", statuscode.ToString());
                    activity.SetTag("otel.status_description", "Load successfully");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occured on Get Photos", ex);
                return null;
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync([FromRoute] Guid id)
        {
            try
            {
                _logger.Information($"Getting Photo by id {id}");
                using var activity = _source.StartActivity(nameof(GetAsync), ActivityKind.Internal)!;
                Stream photo = null;
                string contentType = string.Empty;
                HttpStatusCode statuscode = HttpStatusCode.NotFound;
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        var uri = $"http://bilbo:8000/Photos/{ id }";
                        HttpResponseMessage response = await client.GetAsync(uri);
                        if (response.IsSuccessStatusCode && response?.Content != null)
                        {
                            photo = await response.Content.ReadAsStreamAsync();
                            contentType = response.Content.Headers.ContentType.MediaType;
                            statuscode = response.StatusCode;
                        }
                    }
                    catch (Exception ex)
                    {
                        statuscode = HttpStatusCode.InternalServerError;
                        activity.RecordException(ex);
                        activity.SetStatus(ActivityStatusCode.Error);
                        _logger.Error($"An error occured on Calling BilboApi", ex);
                    }
                    activity.SetStatus(ActivityStatusCode.Ok);
                    activity.AddEvent(new ActivityEvent("Load Photos", tags: new ActivityTagsCollection(new[] { KeyValuePair.Create<string, object?>("photoId", id) })));
                    activity.SetTag("otel.status_code", statuscode.ToString());
                    activity.SetTag("otel.status_description", "Load successfully");
                }

                return photo == null ? NotFound() : File(photo, contentType);
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occured on Get Photo by id {id}", ex);
                return Problem();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromHeader] string documentType, [FromForm] IFormFile file)
        {
            try
            {

                _logger.Information($"Posting new Photo");
                using var activity = _source.StartActivity(nameof(PostAsync), ActivityKind.Internal)!;

                if (file == null)
                {
                    activity.SetStatus(Status.Error);
                    _logger.Warning($"File sent is null");
                    return BadRequest();
                }

                var readStreamActivity = _source.StartActivity(_sourceName, ActivityKind.Consumer, activity.Context)!;
                readStreamActivity.DisplayName = "ReadStream";
                Photo result = null;
                HttpStatusCode statuscode = HttpStatusCode.NotFound;
                using (var client = new HttpClient())
                {
                    try
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            var content = new MultipartFormDataContent();
                            var file_content = new StreamContent(stream);

                            file_content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                            content.Add(file_content, name: "file", fileName: "house.png");
                            content.Add(new StringContent("image/jpeg"), name: "type");

                            var uri = $"http://bilbo:8000/Photos";
                            var response = await client.PostAsync(uri, content);
                            result = await response.Content.ReadFromJsonAsync<Photo>();
                            statuscode = response.StatusCode;
                        }
                    }
                    catch (Exception ex)
                    {
                        statuscode = HttpStatusCode.InternalServerError;
                        activity.RecordException(ex);
                        activity.SetStatus(ActivityStatusCode.Error);
                        _logger.Error($"An error occured on Calling BilboApi", ex);
                    }
                    activity.SetStatus(ActivityStatusCode.Ok);
                    activity.SetTag("otel.status_code", statuscode.ToString());
                    activity.SetTag("otel.status_description", "Load successfully");

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occured on Post Photo", ex);
                return Problem();
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)

        {
            try
            {
                _logger.Information($"Deleting Photo by id {id}");
                using var activity = _source.StartActivity(nameof(DeleteAsync), ActivityKind.Internal)!;
                activity.SetTag("photo-id", id);

                HttpStatusCode statuscode = HttpStatusCode.NotFound;
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        var uri = $"http://bilbo:8000/Photos/{ id }";
                        HttpResponseMessage response = await client.DeleteAsync(uri);
                        if (response.IsSuccessStatusCode && response?.Content != null)
                        {
                            statuscode = response.StatusCode;
                        }
                    }
                    catch (Exception ex)
                    {
                        statuscode = HttpStatusCode.InternalServerError;
                        activity.RecordException(ex);
                        activity.SetStatus(ActivityStatusCode.Error);
                        _logger.Error($"An error occured on Calling BilboApi", ex);
                    }
                    activity.SetStatus(ActivityStatusCode.Ok);
                    activity.AddEvent(new ActivityEvent("Load Photos", tags: new ActivityTagsCollection(new[] { KeyValuePair.Create<string, object?>("photoId", id) })));
                    activity.SetTag("otel.status_code", statuscode.ToString());
                    activity.SetTag("otel.status_description", "Load successfully");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occured on Delete Photo by id {id}", ex);
                return Problem();
            }
        }
    }
}