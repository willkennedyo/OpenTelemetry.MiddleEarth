using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MiddleEarth.Infrastructure.Services;

public class ComputerVisionService
{
    private readonly ComputerVisionClient _computerVisionClient;
    private readonly ComputerVisionMetricsService _computerVisionMetricsService;
    private readonly MiddleEarth.Infrastructure.ILogger _logger;

    public ComputerVisionService(IConfiguration configuration, MiddleEarth.Infrastructure.ILogger logger, ComputerVisionMetricsService computerVisionMetricsService)
    {
        _computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(configuration.GetValue<string>("AppSettings:CognitiveServicesKey")))
        {
            Endpoint = configuration.GetValue<string>("AppSettings:CognitiveServicesEndpoint")
        };

        _computerVisionMetricsService = computerVisionMetricsService;
        _logger = logger;
    }

    public async Task<string?> GetDescriptionAsync(Stream stream)
    {
        _logger.Information("Getting Information from computerVision");
        stream.Position = 0;

        using var analyzeStream = new MemoryStream();
        await stream.CopyToAsync(analyzeStream);
        analyzeStream.Position = 0;

        _computerVisionMetricsService.PayloadCounter.Add(analyzeStream.Length);
        _computerVisionMetricsService.RequestCounter.Add(1);

        var result = await _computerVisionClient.AnalyzeImageInStreamAsync(analyzeStream, new List<VisualFeatureTypes?> { VisualFeatureTypes.Description });

        var description = result.Description.Captions.FirstOrDefault();
        if (description != null)
        {
            _computerVisionMetricsService.ConfidenceHistogram.Record(description.Confidence, KeyValuePair.Create<string, object?>("confidence-description", description.Text));
            return description.Text;
        }

        _logger.Warning("Any Description fonded from computerVision", extraParams: new { Params = JsonConvert.SerializeObject(result) });
        return null;
    }
}
