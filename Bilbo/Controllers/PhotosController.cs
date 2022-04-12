using MiddleEarth.Infrastructure.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiddleEarth.Domain;
using MiddleEarth.Infrastructure.Services;
using MimeMapping;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Text.Json;
using MiddleEarth.Infrastructure.Extentions;

namespace Gandalf.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PhotosController : BilboBaseController
    {
        private PhotoDbContext _photoDbContext;
        private AzureStorageService _azureStorageService;
        private ComputerVisionService _computerVisionService;

        public PhotosController(IHttpContextAccessor httpContextAccessor, PhotoDbContext photoDbContext, ComputerVisionService computerVisionService, AzureStorageService azureStorageService, MiddleEarth.Infrastructure.ILogger looger) : base(httpContextAccessor, looger)
        {
            _photoDbContext = photoDbContext;
            _computerVisionService = computerVisionService;
            _azureStorageService = azureStorageService;
        }

        [HttpGet]
        public async Task<IEnumerable<Photo>> GetAllAsync()
        {
            try
            {
                _logger.Information($"Getting Photos");
                using var activity = _source.StartActivity(nameof(GetAllAsync), ActivityKind.Internal)!;

                var photos = await _photoDbContext.Photos.OrderBy(p => p.OriginalFileName).ToListAsync();

                activity.AddEvent(new ActivityEvent("Load Photos", tags: new ActivityTagsCollection(new[] { KeyValuePair.Create<string, object?>("Count", photos.Count) })));
                activity.SetTag("photoNumbers", photos.Count);
                activity.SetTag("otel.status_code", "OK");
                activity.SetTag("otel.status_description", "Load successfully");

                return photos;
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
                using var getActivity = _source.StartActivity(nameof(GetAsync), ActivityKind.Internal)!;

                var dbActivity = _source.StartActivity(nameof(GetAsync), ActivityKind.Consumer, getActivity.Context)!;

                var photo = await _photoDbContext.Photos.FindAsync(id);
                if (photo is null)
                {
                    dbActivity.SetStatus(Status.Error);
                    _logger.Warning($"Any photo founded on db { id }");
                    return NotFound();
                }

                dbActivity.SetTag("mimetype", MimeUtility.GetMimeMapping(photo.OriginalFileName));
                dbActivity.Stop();

                var streamActivity = _source.StartActivity(nameof(GetAsync), ActivityKind.Consumer, getActivity.Context)!;
                var stream = await _azureStorageService.ReadAsync(photo.Path);
                if (stream is null)
                {
                    streamActivity.SetStatus(Status.Error);
                    _logger.Warning($"File not founded for { id }");
                    return NotFound();
                }

                streamActivity.SetTag("stream.dimension", stream.Length);
                streamActivity.Stop();

                return File(stream, MimeUtility.GetMimeMapping(photo.OriginalFileName));
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occured on Get Photo by id {id}", ex);
                return Problem();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromForm] IFormFile file)
        {
            try
            {

                _logger.Information($"Posting new Photo");
                using var postActivity = _source.StartActivity(nameof(PostAsync), ActivityKind.Internal)!;

                if (file == null)
                {
                    postActivity.SetStatus(Status.Error);
                    _logger.Warning($"File sent is null");
                    return BadRequest();
                }

                var readStreamActivity = _source.StartActivity(nameof(PostAsync), ActivityKind.Consumer, postActivity.Context)!;
                readStreamActivity.DisplayName = "ReadStream";


                if (file is null || !file.IsImage())
                {
                    readStreamActivity.SetStatus(Status.Error);
                    _logger.Warning($"The file extention isn't compatible");
                    return BadRequest();
                }

                using var stream = file.OpenReadStream();

                var id = Guid.NewGuid();
                var newFileName = $"{id}{Path.GetExtension(file.FileName)}".ToLowerInvariant();

                readStreamActivity.Stop();

                var computerVisionActivity = _source.StartActivity(nameof(PostAsync), ActivityKind.Consumer, postActivity.Context)!;
                computerVisionActivity.DisplayName = "Computer Vision";

                var description = await _computerVisionService.GetDescriptionAsync(stream);

                computerVisionActivity.SetTag("description", description);
                computerVisionActivity.Stop();

                var storageActivity = _source.StartActivity(nameof(PostAsync), ActivityKind.Consumer, postActivity.Context)!;
                storageActivity.DisplayName = "Storage Activity";

                await _azureStorageService.SaveAsync(newFileName, stream);
                storageActivity.Stop();

                var dbActivity = _source.StartActivity(nameof(PostAsync), ActivityKind.Consumer, postActivity.Context)!;
                dbActivity.DisplayName = "Entity Framework Core Activity";

                var photo = new Photo
                {
                    Id = id,
                    OriginalFileName = file.FileName,
                    Path = newFileName,
                    Description = description,
                    UploadDate = DateTime.UtcNow
                };

                dbActivity.SetTag("entity", JsonSerializer.Serialize(photo));

                _photoDbContext.Photos.Add(photo);
                await _photoDbContext.SaveChangesAsync();

                dbActivity.Stop();

                return Ok(photo);
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
                using var deleteActivity = _source.StartActivity(nameof(DeleteAsync), ActivityKind.Internal)!;
                deleteActivity.SetTag("photo-id", id);
                var photo = await _photoDbContext.Photos.FindAsync(id);
                if (photo is null)
                {
                    deleteActivity.SetStatus(Status.Error);
                    _logger.Warning($"Any photo founded: Id {id}");
                    return NotFound();
                }

                var storageActivity = _source.StartActivity(_sourceName, ActivityKind.Consumer, deleteActivity.Context)!;
                storageActivity.DisplayName = "Storage Activity";

                await _azureStorageService.DeleteAsync(photo.Path);
                storageActivity.Stop();

                var dbActivity = _source.StartActivity(_sourceName, ActivityKind.Consumer, deleteActivity.Context)!;
                dbActivity.DisplayName = "Entity Framework Core Activity";
                dbActivity.SetTag("photo", photo);

                _photoDbContext.Photos.Remove(photo);
                await _photoDbContext.SaveChangesAsync();

                dbActivity.Stop();

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
