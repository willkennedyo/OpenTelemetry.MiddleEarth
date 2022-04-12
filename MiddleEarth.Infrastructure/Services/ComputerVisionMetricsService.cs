using System.Diagnostics.Metrics;

namespace MiddleEarth.Infrastructure.Services;

public class ComputerVisionMetricsService
{
    private readonly Meter meter;

    public Counter<long> PayloadCounter { get; }

    public Counter<int> RequestCounter { get; }

    public Histogram<double> ConfidenceHistogram { get; }

    public ComputerVisionMetricsService()
    {
        meter = new Meter("ComputerVision");

        PayloadCounter = meter.CreateCounter<long>("PayloadCounter", "bytes");
        RequestCounter = meter.CreateCounter<int>("RequestCounter");
        ConfidenceHistogram = meter.CreateHistogram<double>("ConfidenceHistogram");
    }
}
