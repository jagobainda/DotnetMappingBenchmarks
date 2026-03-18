using DotnetMappingBenchmarks.Services;

namespace DotnetMappingBenchmarks;

public class Worker(BenchmarkRunnerService runner, JsonWriterService writer, ILogger<Worker> logger, IConfiguration config) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(config.GetValue<int>("BenchmarkSettings:IntervalMinutes", 5));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BenchmarkWorker started. Interval: {Minutes} minutes", _interval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Starting benchmark run...");
                var result = await runner.RunAllBenchmarksAsync();
                await writer.WriteResultsAsync(result);
                logger.LogInformation("Benchmark run completed successfully. Next run in {Minutes} minutes",
                    _interval.TotalMinutes);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Benchmark run failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
