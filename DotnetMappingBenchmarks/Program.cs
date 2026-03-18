using DotnetMappingBenchmarks;
using DotnetMappingBenchmarks.Benchmarks;
using DotnetMappingBenchmarks.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<ILibraryBenchmark, AutoMapperBenchmark>();
        services.AddSingleton<ILibraryBenchmark, MapperlyBenchmark>();
        services.AddSingleton<ILibraryBenchmark, MapsterBenchmark>();
        services.AddSingleton<ILibraryBenchmark, TinyMapperBenchmark>();
        services.AddSingleton<ILibraryBenchmark, ManualLinqMapperBenchmark>();
        services.AddSingleton<ILibraryBenchmark, AgileMapperBenchmark>();
        services.AddSingleton<ILibraryBenchmark, ManualLinqMapperBenchmark>();
        services.AddSingleton<BenchmarkRunnerService>();
        services.AddSingleton<JsonWriterService>();
        services.AddHostedService<Worker>();
    });

var host = builder.Build();
host.Run();
