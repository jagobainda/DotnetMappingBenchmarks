using DotnetMappingBenchmarks.Models;

namespace DotnetMappingBenchmarks.Benchmarks;

public class ManualMapperBenchmark : BenchmarkBase
{
    public override Task<LibraryBenchmarkResult> RunAsync()
    {
        var simple = CreateSimpleSource();
        var nested = CreateNestedSource();
        var collection = CreateSimpleSourceList();
        var nameDiff = CreateNameDiffSource();

        var result = new LibraryBenchmarkResult
        {
            Name = "Manual (foreach)",
            Version = $".NET {Environment.Version}",
            Cases =
            [
                MeasureCase("SimpleFlat", () => MapSimple(simple)),
                MeasureCase("NestedObject", () => MapNested(nested)),
                MeasureCase("Collection", () => MapCollection(collection)),
                MeasureCase("NameDifference", () => MapNameDiff(nameDiff))
            ]
        };

        return Task.FromResult(result);
    }

    private static SimpleDestination MapSimple(SimpleSource s) => new()
    {
        Id = s.Id,
        FirstName = s.FirstName,
        LastName = s.LastName,
        Email = s.Email,
        Age = s.Age,
        Address = s.Address,
        City = s.City,
        Country = s.Country,
        Salary = s.Salary,
        IsActive = s.IsActive
    };

    private static NestedDestination MapNested(NestedSource s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Inner = new NestedInnerDestination
        {
            Code = s.Inner.Code,
            Description = s.Inner.Description,
            Deep = new NestedDeepDestination
            {
                Value = s.Inner.Deep.Value,
                Number = s.Inner.Deep.Number
            }
        }
    };

    private static List<SimpleDestination> MapCollection(List<SimpleSource> sources)
    {
        var result = new List<SimpleDestination>(sources.Count);
        foreach (var s in sources) result.Add(MapSimple(s));
        return result;
    }

    private static NameDiffDestination MapNameDiff(NameDiffSource s) => new()
    {
        Id = s.Identifier,
        Name = s.FirstName,
        Surname = s.LastName,
        Email = s.EmailAddress,
        Phone = s.PhoneNumber
    };
}