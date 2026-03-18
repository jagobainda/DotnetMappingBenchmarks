using DotnetMappingBenchmarks.Models;
using Nelibur.ObjectMapper;

namespace DotnetMappingBenchmarks.Benchmarks;

public class TinyMapperBenchmark : BenchmarkBase
{
    private static bool _bound;
    private static readonly Lock BindLock = new();

    public TinyMapperBenchmark()
    {
        lock (BindLock)
        {
            if (!_bound)
            {
                TinyMapper.Bind<SimpleSource, SimpleDestination>();
                TinyMapper.Bind<NestedDeepSource, NestedDeepDestination>();
                TinyMapper.Bind<NestedInnerSource, NestedInnerDestination>();
                TinyMapper.Bind<NestedSource, NestedDestination>();
                TinyMapper.Bind<NameDiffSource, NameDiffDestination>(config =>
                {
                    config.Bind(s => s.Identifier, d => d.Id);
                    config.Bind(s => s.FirstName, d => d.Name);
                    config.Bind(s => s.LastName, d => d.Surname);
                    config.Bind(s => s.EmailAddress, d => d.Email);
                    config.Bind(s => s.PhoneNumber, d => d.Phone);
                });
                _bound = true;
            }
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
            Name = "TinyMapper",
            Version = GetAssemblyVersion(typeof(TinyMapper)),
            Cases =
            [
                MeasureCase("SimpleFlat", () => TinyMapper.Map<SimpleDestination>(simple)),
                MeasureCase("NestedObject", () => TinyMapper.Map<NestedDestination>(nested)),
                MeasureCase("Collection", () =>
                {
                    var dest = new List<SimpleDestination>(collection.Count);
                    foreach (var item in collection)
                        dest.Add(TinyMapper.Map<SimpleDestination>(item));
                }),
                MeasureCase("NameDifference", () => TinyMapper.Map<NameDiffDestination>(nameDiff))
            ]
        };

        return Task.FromResult(result);
    }
}
