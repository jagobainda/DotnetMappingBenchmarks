using System.Reflection;
using AutoMapper;
using BenchmarkDotNet.Reports;
using DotnetMappingBenchmarks.Helpers;
using DotnetMappingBenchmarks.Models;
using Mapster;

namespace DotnetMappingBenchmarks.Services;

public static class BenchmarkResultTransformer
{
    private const double NsToUs = 1_000.0;

    public static BenchmarkRunResult Transform(Summary[] summaries)
    {
        var libraries = new Dictionary<string, LibraryBenchmarkResult>(StringComparer.Ordinal);

        foreach (var summary in summaries)
        {
            var scenarioName = GetScenarioName(summary);

            foreach (var report in summary.Reports)
            {
                if (!report.Success || report.ResultStatistics is null)
                    continue;

                var methodName = report.BenchmarkCase.Descriptor.WorkloadMethod.Name;
                var libraryName = GetLibraryDisplayName(methodName);

                if (!libraries.TryGetValue(libraryName, out var libResult))
                {
                    libResult = new LibraryBenchmarkResult
                    {
                        Name = libraryName,
                        Version = GetLibraryVersion(methodName)
                    };
                    libraries[libraryName] = libResult;
                }

                var stats = report.ResultStatistics;
                libResult.Cases.Add(new BenchmarkCaseResult
                {
                    Name = scenarioName,
                    MeanUs = Math.Round(stats.Mean / NsToUs, 2),
                    MedianUs = Math.Round(stats.Percentiles.P50 / NsToUs, 2),
                    P95Us = Math.Round(stats.Percentiles.P95 / NsToUs, 2),
                    P99Us = Math.Round(stats.Percentiles.Percentile(99) / NsToUs, 2),
                    StddevUs = Math.Round(stats.StandardDeviation / NsToUs, 2),
                    AllocBytes = report.GcStats.GetBytesAllocatedPerOperation(report.BenchmarkCase) ?? 0
                });
            }
        }

        return new BenchmarkRunResult
        {
            RunAt = TimeZoneHelper.GetCurrentCetTime(),
            Libraries = [.. libraries.Values]
        };
    }

    private static string GetScenarioName(Summary summary)
    {
        var typeName = summary.BenchmarksCases[0].Descriptor.Type.Name;
        const string suffix = "Benchmark";
        return typeName.EndsWith(suffix) ? typeName[..^suffix.Length] : typeName;
    }

    private static string GetLibraryDisplayName(string methodName) => methodName switch
    {
        "Manual" or "ManualForeach" => "Manual (foreach)",
        "ManualLinq" => "Manual (LINQ)",
        _ => methodName
    };

    private static string GetLibraryVersion(string methodName) => methodName switch
    {
        "AutoMapper" => GetAssemblyVersion(typeof(IMapper)),
        "Mapster" => GetAssemblyVersion(typeof(TypeAdapterConfig)),
        "TinyMapper" => GetAssemblyVersion(typeof(Nelibur.ObjectMapper.TinyMapper)),
        "AgileMapper" => GetAssemblyVersion(typeof(AgileObjects.AgileMapper.Mapper)),
        "Mapperly" => GetAssemblyVersion(typeof(Riok.Mapperly.Abstractions.MapperAttribute)),
        _ => $".NET {Environment.Version}"
    };

    private static string GetAssemblyVersion(Type type)
    {
        var assembly = type.Assembly;
        var infoVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (infoVersion is not null)
        {
            var plusIndex = infoVersion.IndexOf('+');
            return plusIndex >= 0 ? infoVersion[..plusIndex] : infoVersion;
        }

        return assembly.GetName().Version?.ToString() ?? "unknown";
    }
}
