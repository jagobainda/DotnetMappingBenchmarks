using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using DotnetMappingBenchmarks.Benchmarks;
using DotnetMappingBenchmarks.Services;

var job = Job.Default
    .WithWarmupCount(3)
    .WithIterationCount(5)
    .WithLaunchCount(1);

var config = DefaultConfig.Instance
    .WithOptions(ConfigOptions.DisableOptimizationsValidator)
    .AddJob(job)
    .AddExporter(HtmlExporter.Default)
    .AddExporter(MarkdownExporter.GitHub);

var summaries = new[]
{
    BenchmarkRunner.Run<SimpleFlatBenchmark>(config, args),
    BenchmarkRunner.Run<NestedObjectBenchmark>(config, args),
    BenchmarkRunner.Run<CollectionBenchmark>(config, args),
    BenchmarkRunner.Run<NameDifferenceBenchmark>(config, args)
};

if (summaries.All(s => !s.HasCriticalValidationErrors && s.Reports.Length > 0))
{
    var result = BenchmarkResultTransformer.Transform(summaries);
    var outputDir = ResolveOutputDir();
    Directory.CreateDirectory(outputDir);
    var writer = new JsonWriterService(outputDir);
    await writer.WriteResultsAsync(result);
}

static string ResolveOutputDir()
{
    var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
    if (env.Equals("Development", StringComparison.OrdinalIgnoreCase))
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DotnetMappingBenchmarks");

    return Environment.GetEnvironmentVariable("BENCHMARK_OUTPUT_DIR") ?? "/var/www/cdn/dotnet-mapping-benchmarks/";
}

