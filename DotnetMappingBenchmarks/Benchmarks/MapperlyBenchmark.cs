using DotnetMappingBenchmarks.Models;
using Riok.Mapperly.Abstractions;

namespace DotnetMappingBenchmarks.Benchmarks;

[Mapper]
public partial class MapperlyMapper
{
    public partial SimpleDestination MapSimple(SimpleSource source);
    public partial NestedDestination MapNested(NestedSource source);
    public partial List<SimpleDestination> MapCollection(List<SimpleSource> source);

    [MapProperty(nameof(NameDiffSource.Identifier), nameof(NameDiffDestination.Id))]
    [MapProperty(nameof(NameDiffSource.FirstName), nameof(NameDiffDestination.Name))]
    [MapProperty(nameof(NameDiffSource.LastName), nameof(NameDiffDestination.Surname))]
    [MapProperty(nameof(NameDiffSource.EmailAddress), nameof(NameDiffDestination.Email))]
    [MapProperty(nameof(NameDiffSource.PhoneNumber), nameof(NameDiffDestination.Phone))]
    public partial NameDiffDestination MapNameDiff(NameDiffSource source);
}

public class MapperlyBenchmark : BenchmarkBase
{
    private readonly MapperlyMapper _mapper = new();

    public override Task<LibraryBenchmarkResult> RunAsync()
    {
        var simple = CreateSimpleSource();
        var nested = CreateNestedSource();
        var collection = CreateSimpleSourceList();
        var nameDiff = CreateNameDiffSource();

        var result = new LibraryBenchmarkResult
        {
            Name = "Mapperly",
            Version = GetAssemblyVersion(typeof(MapperAttribute)),
            Cases =
            [
                MeasureCase("SimpleFlat", () => _mapper.MapSimple(simple)),
                MeasureCase("NestedObject", () => _mapper.MapNested(nested)),
                MeasureCase("Collection", () => _mapper.MapCollection(collection)),
                MeasureCase("NameDifference", () => _mapper.MapNameDiff(nameDiff))
            ]
        };

        return Task.FromResult(result);
    }
}
