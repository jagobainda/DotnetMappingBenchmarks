using AgileObjects.AgileMapper;
using DotnetMappingBenchmarks.Models;

namespace DotnetMappingBenchmarks.Benchmarks;

public class AgileMapperBenchmark : BenchmarkBase
{
    private static bool _configured;
    private static readonly Lock ConfigLock = new();

    public AgileMapperBenchmark()
    {
        lock (ConfigLock)
        {
            if (_configured)
            {
                return;
            }

            Mapper.WhenMapping
                .From<NameDiffSource>()
                .To<NameDiffDestination>()
                .Map((s, _) => (int)s.Identifier).To(d => d.Id);

            Mapper.WhenMapping
                .From<NameDiffSource>()
                .To<NameDiffDestination>()
                .Map((s, _) => s.FirstName).To(d => d.Name);

            Mapper.WhenMapping
                .From<NameDiffSource>()
                .To<NameDiffDestination>()
                .Map((s, _) => s.LastName).To(d => d.Surname);

            Mapper.WhenMapping
                .From<NameDiffSource>()
                .To<NameDiffDestination>()
                .Map((s, _) => s.EmailAddress).To(d => d.Email);

            Mapper.WhenMapping
                .From<NameDiffSource>()
                .To<NameDiffDestination>()
                .Map((s, _) => s.PhoneNumber).To(d => d.Phone);

            _configured = true;
        }
    }

    public override Task<LibraryBenchmarkResult> RunAsync()
    {
        var simple = CreateSimpleSource();
        var nested = CreateNestedSource();
        var collection = CreateSimpleSourceList();
        var nameDiff = CreateNameDiffSource();

        var result = new LibraryBenchmarkResult
        {
            Name = "AgileMapper",
            Version = GetAssemblyVersion(typeof(Mapper)),
            Cases =
            [
                MeasureCase("SimpleFlat", () => Mapper.Map(simple).ToANew<SimpleDestination>()),
                MeasureCase("NestedObject", () => Mapper.Map(nested).ToANew<NestedDestination>()),
                MeasureCase("Collection", () => Mapper.Map(collection).ToANew<List<SimpleDestination>>()),
                MeasureCase("NameDifference", () => Mapper.Map(nameDiff).ToANew<NameDiffDestination>())
            ]
        };

        return Task.FromResult(result);
    }
}
