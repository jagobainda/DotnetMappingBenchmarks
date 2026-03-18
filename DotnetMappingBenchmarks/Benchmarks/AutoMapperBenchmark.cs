using AutoMapper;
using DotnetMappingBenchmarks.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetMappingBenchmarks.Benchmarks;

public class AutoMapperBenchmark : BenchmarkBase
{
    private readonly IMapper _mapper;

    public AutoMapperBenchmark()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SimpleSource, SimpleDestination>();
            cfg.CreateMap<NestedSource, NestedDestination>();
            cfg.CreateMap<NestedInnerSource, NestedInnerDestination>();
            cfg.CreateMap<NestedDeepSource, NestedDeepDestination>();
            cfg.CreateMap<NameDiffSource, NameDiffDestination>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Identifier))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FirstName))
                .ForMember(d => d.Surname, opt => opt.MapFrom(s => s.LastName))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.EmailAddress))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.PhoneNumber));
        }, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    public override Task<LibraryBenchmarkResult> RunAsync()
    {
        var simple = CreateSimpleSource();
        var nested = CreateNestedSource();
        var collection = CreateSimpleSourceList();
        var nameDiff = CreateNameDiffSource();

        var result = new LibraryBenchmarkResult
        {
            Name = "AutoMapper",
            Version = GetAssemblyVersion(typeof(IMapper)),
            Cases =
            [
                MeasureCase("SimpleFlat", () => _mapper.Map<SimpleDestination>(simple)),
                MeasureCase("NestedObject", () => _mapper.Map<NestedDestination>(nested)),
                MeasureCase("Collection", () => _mapper.Map<List<SimpleDestination>>(collection)),
                MeasureCase("NameDifference", () => _mapper.Map<NameDiffDestination>(nameDiff))
            ]
        };

        return Task.FromResult(result);
    }
}
