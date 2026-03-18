using DotnetMappingBenchmarks.Models;

namespace DotnetMappingBenchmarks.Benchmarks;

public interface ILibraryBenchmark
{
    Task<LibraryBenchmarkResult> RunAsync();
}
