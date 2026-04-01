using DotnetMappingBenchmarks.Helpers;
using DotnetMappingBenchmarks.Models;
using System.Text.Json;

namespace DotnetMappingBenchmarks.Services;

public class JsonWriterService(string outputDir)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public async Task WriteResultsAsync(BenchmarkRunResult result)
    {
        var historyPath = Path.Combine(outputDir, "history.json");
        var lastResultPath = Path.Combine(outputDir, "last_result.json");
        var avgResultPath = Path.Combine(outputDir, "avg_results.json");

        var history = await ReadHistoryAsync(historyPath);

        history.Add(result);

        var cutoff = DateTimeOffset.UtcNow.AddMonths(-3);
        history.RemoveAll(r => r.RunAt < cutoff);

        await WriteJsonFileAsync(historyPath, history);
        Console.WriteLine($"Written history.json with {history.Count} entries");

        await WriteJsonFileAsync(lastResultPath, result);
        Console.WriteLine("Written last_result.json");

        var avg = ComputeAverage(history);
        await WriteJsonFileAsync(avgResultPath, avg);
        Console.WriteLine($"Written avg_results.json (averaged over {history.Count} runs)");
    }

    private static async Task<List<BenchmarkRunResult>> ReadHistoryAsync(string path)
    {
        if (!File.Exists(path)) return [];

        try
        {
            await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await JsonSerializer.DeserializeAsync<List<BenchmarkRunResult>>(fs, JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to read history.json, starting fresh. {ex.Message}");
            return [];
        }
    }

    private static async Task WriteJsonFileAsync<T>(string path, T data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var tempPath = path + ".tmp";

        await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        await using (var sw = new StreamWriter(fs)) { await sw.WriteAsync(json); }

        File.Move(tempPath, path, overwrite: true);
    }

    private static BenchmarkRunResult ComputeAverage(List<BenchmarkRunResult> runs)
    {
        var now = TimeZoneHelper.GetCurrentCetTime();

        var libraryNames = runs
            .SelectMany(r => r.Libraries)
            .Select(l => l.Name)
            .Distinct()
            .ToList();

        var libraries = new List<LibraryBenchmarkResult>();

        foreach (var libName in libraryNames)
        {
            var libRuns = runs
                .SelectMany(r => r.Libraries)
                .Where(l => l.Name == libName)
                .ToList();

            var caseNames = libRuns
                .SelectMany(l => l.Cases)
                .Select(c => c.Name)
                .Distinct()
                .ToList();

            var cases = new List<BenchmarkCaseResult>();

            foreach (var caseName in caseNames)
            {
                var caseRuns = libRuns
                    .SelectMany(l => l.Cases)
                    .Where(c => c.Name == caseName)
                    .ToList();

                cases.Add(new BenchmarkCaseResult
                {
                    Name = caseName,
                    MeanUs = Math.Round(caseRuns.Average(c => c.MeanUs), 2),
                    MedianUs = Math.Round(caseRuns.Average(c => c.MedianUs), 2),
                    P95Us = Math.Round(caseRuns.Average(c => c.P95Us), 2),
                    P99Us = Math.Round(caseRuns.Average(c => c.P99Us), 2),
                    StddevUs = Math.Round(caseRuns.Average(c => c.StddevUs), 2),
                    AllocBytes = (long)Math.Round(caseRuns.Average(c => c.AllocBytes))
                });
            }

            libraries.Add(new LibraryBenchmarkResult
            {
                Name = libName,
                Version = libRuns[^1].Version,
                Cases = cases
            });
        }

        return new BenchmarkRunResult
        {
            RunAt = now,
            Libraries = libraries
        };
    }
}
