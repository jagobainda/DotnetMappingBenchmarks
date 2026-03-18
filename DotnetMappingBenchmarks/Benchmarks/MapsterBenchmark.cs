using DotnetMappingBenchmarks.Models;
using Mapster;

namespace DotnetMappingBenchmarks.Benchmarks;

public class MapsterBenchmark : BenchmarkBase
{
    private readonly TypeAdapterConfig _config;

    public MapsterBenchmark()
    {
        _config = new TypeAdapterConfig();
        _config.NewConfig<NameDiffSource, NameDiffDestination>()
            .Map(d => d.Id, s => s.Identifier)
            .Map(d => d.Name, s => s.FirstName)
            .Map(d => d.Surname, s => s.LastName)
            .Map(d => d.Email, s => s.EmailAddress)
            .Map(d => d.Phone, s => s.PhoneNumber);
    }

    public override Task<LibraryBenchmarkResult> RunAsync()
    {
        var simple = CreateSimpleSource();
        var nested = CreateNestedSource();
        var collection = CreateSimpleSourceList();
        var nameDiff = CreateNameDiffSource();

        var result = new LibraryBenchmarkResult
        {
            Name = "Mapster",
            Version = GetAssemblyVersion(typeof(TypeAdapter)),
            Cases =
            [
                MeasureCase("SimpleFlat", () => simple.Adapt<SimpleDestination>(_config)),
                MeasureCase("NestedObject", () => nested.Adapt<NestedDestination>(_config)),
                MeasureCase("Collection", () => collection.Adapt<List<SimpleDestination>>(_config)),
                MeasureCase("NameDifference", () => nameDiff.Adapt<NameDiffDestination>(_config))
            ]
        };

        return Task.FromResult(result);
    }
}
