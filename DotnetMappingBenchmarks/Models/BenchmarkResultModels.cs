using System.Text.Json.Serialization;

namespace DotnetMappingBenchmarks.Models;

public class BenchmarkCaseResult
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("mean_us")]
    public double MeanUs { get; set; }

    [JsonPropertyName("median_us")]
    public double MedianUs { get; set; }

    [JsonPropertyName("p95_us")]
    public double P95Us { get; set; }

    [JsonPropertyName("p99_us")]
    public double P99Us { get; set; }

    [JsonPropertyName("stddev_us")]
    public double StddevUs { get; set; }

    [JsonPropertyName("alloc_bytes")]
    public long AllocBytes { get; set; }
}

public class LibraryBenchmarkResult
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("cases")]
    public List<BenchmarkCaseResult> Cases { get; set; } = [];
}

public class BenchmarkRunResult
{
    [JsonPropertyName("run_at")]
    public DateTimeOffset RunAt { get; set; }

    [JsonPropertyName("libraries")]
    public List<LibraryBenchmarkResult> Libraries { get; set; } = [];
}
