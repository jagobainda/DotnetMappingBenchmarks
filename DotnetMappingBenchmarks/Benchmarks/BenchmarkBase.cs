using System.Diagnostics;
using System.Reflection;
using DotnetMappingBenchmarks.Models;

namespace DotnetMappingBenchmarks.Benchmarks;

public abstract class BenchmarkBase : ILibraryBenchmark
{
    private const int WarmupIterations = 3;
    private const int MeasureIterations = 100;

    public abstract Task<LibraryBenchmarkResult> RunAsync();

    protected static BenchmarkCaseResult MeasureCase(string name, Action action)
    {
        for (int i = 0; i < WarmupIterations; i++) action();

        var timings = new double[MeasureIterations];
        for (int i = 0; i < MeasureIterations; i++)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            timings[i] = sw.Elapsed.TotalMicroseconds;
        }

        return ComputeStats(name, timings);
    }

    private static BenchmarkCaseResult ComputeStats(string name, double[] timings)
    {
        Array.Sort(timings);
        var mean = timings.Average();
        var median = (timings[49] + timings[50]) / 2.0;
        var p95 = timings[94];
        var p99 = timings[98];
        var variance = timings.Select(t => Math.Pow(t - mean, 2)).Average();
        var stddev = Math.Sqrt(variance);

        return new BenchmarkCaseResult
        {
            Name = name,
            MeanUs = Math.Round(mean, 2),
            MedianUs = Math.Round(median, 2),
            P95Us = Math.Round(p95, 2),
            P99Us = Math.Round(p99, 2),
            StddevUs = Math.Round(stddev, 2)
        };
    }

    protected static SimpleSource CreateSimpleSource() => new()
    {
        Id = 1,
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        Age = 30,
        Address = "123 Main St",
        City = "Springfield",
        Country = "US",
        Salary = 75000.50,
        IsActive = true
    };

    protected static NestedSource CreateNestedSource() => new()
    {
        Id = 1,
        Name = "Parent",
        Inner = new NestedInnerSource
        {
            Code = 42,
            Description = "Inner level",
            Deep = new NestedDeepSource
            {
                Value = "Deep value",
                Number = 99
            }
        }
    };

    protected static NameDiffSource CreateNameDiffSource() => new()
    {
        Identifier = 1,
        FirstName = "Jane",
        LastName = "Smith",
        EmailAddress = "jane.smith@example.com",
        PhoneNumber = "+1234567890"
    };

    protected static List<SimpleSource> CreateSimpleSourceList(int count = 100)
    {
        return [.. Enumerable.Range(1, count).Select(i => new SimpleSource
        {
            Id = i,
            FirstName = $"First{i}",
            LastName = $"Last{i}",
            Email = $"user{i}@example.com",
            Age = 20 + (i % 50),
            Address = $"{i} Main St",
            City = $"City{i}",
            Country = $"Country{i % 10}",
            Salary = 30000 + (i * 100),
            IsActive = i % 2 == 0
        })];
    }

    protected static string GetAssemblyVersion(Type type)
    {
        var assembly = type.Assembly;
        var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (infoVersion is not null)
        {
            var plusIndex = infoVersion.IndexOf('+');
            return plusIndex >= 0 ? infoVersion[..plusIndex] : infoVersion;
        }
        return assembly.GetName().Version?.ToString() ?? "unknown";
    }
}
