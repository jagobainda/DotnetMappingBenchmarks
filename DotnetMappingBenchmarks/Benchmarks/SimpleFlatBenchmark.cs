using AutoMapper;
using BenchmarkDotNet.Attributes;
using DotnetMappingBenchmarks.Models;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetMappingBenchmarks.Benchmarks;

[MemoryDiagnoser]
public class SimpleFlatBenchmark
{
    private SimpleSource _source = null!;
    private IMapper _autoMapper = null!;
    private TypeAdapterConfig _mapsterConfig = null!;
    private MapperlyMapper _mapperlyMapper = null!;

    [GlobalSetup]
    public void Setup()
    {
        _source = BenchmarkData.CreateSimpleSource();

        var config = new MapperConfiguration(
            cfg => cfg.CreateMap<SimpleSource, SimpleDestination>(),
            NullLoggerFactory.Instance);
        _autoMapper = config.CreateMapper();

        _mapsterConfig = new TypeAdapterConfig();
        _mapperlyMapper = new MapperlyMapper();

        Nelibur.ObjectMapper.TinyMapper.Bind<SimpleSource, SimpleDestination>();
    }

    [Benchmark(Baseline = true)]
    public SimpleDestination Manual() => new()
    {
        Id = _source.Id,
        FirstName = _source.FirstName,
        LastName = _source.LastName,
        Email = _source.Email,
        Age = _source.Age,
        Address = _source.Address,
        City = _source.City,
        Country = _source.Country,
        Salary = _source.Salary,
        IsActive = _source.IsActive
    };

    [Benchmark]
    public SimpleDestination ManualLinq() => new()
    {
        Id = _source.Id,
        FirstName = _source.FirstName,
        LastName = _source.LastName,
        Email = _source.Email,
        Age = _source.Age,
        Address = _source.Address,
        City = _source.City,
        Country = _source.Country,
        Salary = _source.Salary,
        IsActive = _source.IsActive
    };

    [Benchmark]
    public SimpleDestination AutoMapper() => _autoMapper.Map<SimpleDestination>(_source);

    [Benchmark]
    public SimpleDestination Mapster() => _source.Adapt<SimpleDestination>(_mapsterConfig);

    [Benchmark]
    public SimpleDestination TinyMapper() => Nelibur.ObjectMapper.TinyMapper.Map<SimpleDestination>(_source);

    [Benchmark]
    public SimpleDestination AgileMapper() => AgileObjects.AgileMapper.Mapper.Map(_source).ToANew<SimpleDestination>();

    [Benchmark]
    public SimpleDestination Mapperly() => _mapperlyMapper.MapSimple(_source);
}
