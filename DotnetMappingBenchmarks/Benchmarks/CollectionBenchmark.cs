using AutoMapper;
using BenchmarkDotNet.Attributes;
using DotnetMappingBenchmarks.Models;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetMappingBenchmarks.Benchmarks;

[MemoryDiagnoser]
public class CollectionBenchmark
{
    private List<SimpleSource> _source = null!;
    private IMapper _autoMapper = null!;
    private TypeAdapterConfig _mapsterConfig = null!;
    private MapperlyMapper _mapperlyMapper = null!;

    [GlobalSetup]
    public void Setup()
    {
        _source = BenchmarkData.CreateSimpleSourceList();

        var config = new MapperConfiguration(
            cfg => cfg.CreateMap<SimpleSource, SimpleDestination>(),
            NullLoggerFactory.Instance);
        _autoMapper = config.CreateMapper();

        _mapsterConfig = new TypeAdapterConfig();
        _mapperlyMapper = new MapperlyMapper();

        Nelibur.ObjectMapper.TinyMapper.Bind<SimpleSource, SimpleDestination>();
    }

    [Benchmark(Baseline = true)]
    public List<SimpleDestination> ManualForeach()
    {
        var result = new List<SimpleDestination>(_source.Count);
        foreach (var s in _source)
        {
            result.Add(new SimpleDestination
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
            });
        }
        return result;
    }

    [Benchmark]
    public List<SimpleDestination> ManualLinq() => _source.Select(s => new SimpleDestination
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
    }).ToList();

    [Benchmark]
    public List<SimpleDestination> AutoMapper() => _autoMapper.Map<List<SimpleDestination>>(_source);

    [Benchmark]
    public List<SimpleDestination> Mapster() => _source.Adapt<List<SimpleDestination>>(_mapsterConfig);

    [Benchmark]
    public List<SimpleDestination> TinyMapper()
    {
        var result = new List<SimpleDestination>(_source.Count);
        foreach (var item in _source)
            result.Add(Nelibur.ObjectMapper.TinyMapper.Map<SimpleDestination>(item));
        return result;
    }

    [Benchmark]
    public List<SimpleDestination> AgileMapper() =>
        AgileObjects.AgileMapper.Mapper.Map(_source).ToANew<List<SimpleDestination>>();

    [Benchmark]
    public List<SimpleDestination> Mapperly() => _mapperlyMapper.MapCollection(_source);
}
