using AutoMapper;
using BenchmarkDotNet.Attributes;
using DotnetMappingBenchmarks.Models;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetMappingBenchmarks.Benchmarks;

[MemoryDiagnoser]
public class NestedObjectBenchmark
{
    private NestedSource _source = null!;
    private IMapper _autoMapper = null!;
    private TypeAdapterConfig _mapsterConfig = null!;
    private MapperlyMapper _mapperlyMapper = null!;

    [GlobalSetup]
    public void Setup()
    {
        _source = BenchmarkData.CreateNestedSource();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<NestedSource, NestedDestination>();
            cfg.CreateMap<NestedInnerSource, NestedInnerDestination>();
            cfg.CreateMap<NestedDeepSource, NestedDeepDestination>();
        }, NullLoggerFactory.Instance);
        _autoMapper = config.CreateMapper();

        _mapsterConfig = new TypeAdapterConfig();
        _mapperlyMapper = new MapperlyMapper();

        Nelibur.ObjectMapper.TinyMapper.Bind<NestedDeepSource, NestedDeepDestination>();
        Nelibur.ObjectMapper.TinyMapper.Bind<NestedInnerSource, NestedInnerDestination>();
        Nelibur.ObjectMapper.TinyMapper.Bind<NestedSource, NestedDestination>();
    }

    [Benchmark(Baseline = true)]
    public NestedDestination Manual() => new()
    {
        Id = _source.Id,
        Name = _source.Name,
        Inner = new NestedInnerDestination
        {
            Code = _source.Inner.Code,
            Description = _source.Inner.Description,
            Deep = new NestedDeepDestination
            {
                Value = _source.Inner.Deep.Value,
                Number = _source.Inner.Deep.Number
            }
        }
    };

    [Benchmark]
    public NestedDestination ManualLinq() => new()
    {
        Id = _source.Id,
        Name = _source.Name,
        Inner = new NestedInnerDestination
        {
            Code = _source.Inner.Code,
            Description = _source.Inner.Description,
            Deep = new NestedDeepDestination
            {
                Value = _source.Inner.Deep.Value,
                Number = _source.Inner.Deep.Number
            }
        }
    };

    [Benchmark]
    public NestedDestination AutoMapper() => _autoMapper.Map<NestedDestination>(_source);

    [Benchmark]
    public NestedDestination Mapster() => _source.Adapt<NestedDestination>(_mapsterConfig);

    [Benchmark]
    public NestedDestination TinyMapper() => Nelibur.ObjectMapper.TinyMapper.Map<NestedDestination>(_source);

    [Benchmark]
    public NestedDestination AgileMapper() => AgileObjects.AgileMapper.Mapper.Map(_source).ToANew<NestedDestination>();

    [Benchmark]
    public NestedDestination Mapperly() => _mapperlyMapper.MapNested(_source);
}
