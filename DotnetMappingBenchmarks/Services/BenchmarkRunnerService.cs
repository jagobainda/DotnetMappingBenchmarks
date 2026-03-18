using DotnetMappingBenchmarks.Benchmarks;
using DotnetMappingBenchmarks.Helpers;
using DotnetMappingBenchmarks.Models;
using Microsoft.Extensions.Logging;

namespace DotnetMappingBenchmarks.Services;

public class BenchmarkRunnerService(IEnumerable<ILibraryBenchmark> benchmarks, ILogger<BenchmarkRunnerService> logger)
{
    public async Task<BenchmarkRunResult> RunAllBenchmarksAsync()
    {
        var now = TimeZoneHelper.GetCurrentCetTime();

        var result = new BenchmarkRunResult
        {
            RunAt = now,
            Libraries = []
        };

        foreach (var benchmark in benchmarks)
        {
            var typeName = benchmark.GetType().Name;
            logger.LogInformation("Running benchmark: {BenchmarkType}", typeName);

            try
            {
                var libraryResult = await benchmark.RunAsync();
                result.Libraries.Add(libraryResult);
                logger.LogInformation("Completed benchmark: {BenchmarkType} ({CaseCount} cases)", typeName, libraryResult.Cases.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to run benchmark: {BenchmarkType}", typeName);
            }
        }

        return result;
    }
}
