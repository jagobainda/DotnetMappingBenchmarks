using AutoMapper;
using BenchmarkDotNet.Attributes;
using DotnetMappingBenchmarks.Models;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetMappingBenchmarks.Benchmarks;

[MemoryDiagnoser]
public class NameDifferenceBenchmark
{
    private NameDiffSource _source = null!;
    private IMapper _autoMapper = null!;
    private TypeAdapterConfig _mapsterConfig = null!;
    private MapperlyMapper _mapperlyMapper = null!;

    [GlobalSetup]
    public void Setup()
    {
        _source = BenchmarkData.CreateNameDiffSource();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<NameDiffSource, NameDiffDestination>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Identifier))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FirstName))
                .ForMember(d => d.Surname, opt => opt.MapFrom(s => s.LastName))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.EmailAddress))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.PhoneNumber));
        }, NullLoggerFactory.Instance);
        _autoMapper = config.CreateMapper();

        _mapsterConfig = new TypeAdapterConfig();
        _mapsterConfig.NewConfig<NameDiffSource, NameDiffDestination>()
            .Map(d => d.Id, s => s.Identifier)
            .Map(d => d.Name, s => s.FirstName)
            .Map(d => d.Surname, s => s.LastName)
            .Map(d => d.Email, s => s.EmailAddress)
            .Map(d => d.Phone, s => s.PhoneNumber);

        _mapperlyMapper = new MapperlyMapper();

        Nelibur.ObjectMapper.TinyMapper.Bind<NameDiffSource, NameDiffDestination>(cfg =>
        {
            cfg.Bind(s => s.Identifier, d => d.Id);
            cfg.Bind(s => s.FirstName, d => d.Name);
            cfg.Bind(s => s.LastName, d => d.Surname);
            cfg.Bind(s => s.EmailAddress, d => d.Email);
            cfg.Bind(s => s.PhoneNumber, d => d.Phone);
        });

        AgileObjects.AgileMapper.Mapper.WhenMapping
            .From<NameDiffSource>().To<NameDiffDestination>()
            .Map((s, _) => (int)s.Identifier).To(d => d.Id);
        AgileObjects.AgileMapper.Mapper.WhenMapping
            .From<NameDiffSource>().To<NameDiffDestination>()
            .Map((s, _) => s.FirstName).To(d => d.Name);
        AgileObjects.AgileMapper.Mapper.WhenMapping
            .From<NameDiffSource>().To<NameDiffDestination>()
            .Map((s, _) => s.LastName).To(d => d.Surname);
        AgileObjects.AgileMapper.Mapper.WhenMapping
            .From<NameDiffSource>().To<NameDiffDestination>()
            .Map((s, _) => s.EmailAddress).To(d => d.Email);
        AgileObjects.AgileMapper.Mapper.WhenMapping
            .From<NameDiffSource>().To<NameDiffDestination>()
            .Map((s, _) => s.PhoneNumber).To(d => d.Phone);
    }

    [Benchmark(Baseline = true)]
    public NameDiffDestination Manual() => new()
    {
        Id = _source.Identifier,
        Name = _source.FirstName,
        Surname = _source.LastName,
        Email = _source.EmailAddress,
        Phone = _source.PhoneNumber
    };

    [Benchmark]
    public NameDiffDestination ManualLinq() => new()
    {
        Id = _source.Identifier,
        Name = _source.FirstName,
        Surname = _source.LastName,
        Email = _source.EmailAddress,
        Phone = _source.PhoneNumber
    };

    [Benchmark]
    public NameDiffDestination AutoMapper() => _autoMapper.Map<NameDiffDestination>(_source);

    [Benchmark]
    public NameDiffDestination Mapster() => _source.Adapt<NameDiffDestination>(_mapsterConfig);

    [Benchmark]
    public NameDiffDestination TinyMapper() => Nelibur.ObjectMapper.TinyMapper.Map<NameDiffDestination>(_source);

    [Benchmark]
    public NameDiffDestination AgileMapper() =>
        AgileObjects.AgileMapper.Mapper.Map(_source).ToANew<NameDiffDestination>();

    [Benchmark]
    public NameDiffDestination Mapperly() => _mapperlyMapper.MapNameDiff(_source);
}
