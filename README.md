# .NET Mapping Benchmarks

Comparative benchmarks of major object mapping libraries for .NET, executed with [BenchmarkDotNet](https://benchmarkdotnet.org/) on **.NET 10**. Results persist as static JSON files and can be served directly via Nginx.

## Benchmarked Libraries

| Library                                                | Version | Mapping Type                                    |
| ------------------------------------------------------ | ------- | ----------------------------------------------- |
| **Manual (foreach)**                                   | .NET 10 | Baseline — field-to-field assignment            |
| **Manual (LINQ)**                                      | .NET 10 | Variant using LINQ projection                   |
| [AutoMapper](https://automapper.org/)                  | 16.1.1  | Convention + runtime reflection                 |
| [Mapperly](https://mapperly.riok.app/)                 | 5.0.0   | Source generator — compile-time code generation |
| [Mapster](https://github.com/MapsterMapper/Mapster)    | 10.0.7  | IL emission at runtime                          |
| [TinyMapper](https://github.com/TinyMapper/TinyMapper) | 3.0.3   | IL emission with explicit binding               |
| [AgileMapper](https://agileobjects.co.uk/agile-mapper) | 1.8.1   | Convention + runtime reflection                 |

---

## Benchmark Scenarios

Each library is measured across four distinct scenarios, representative of real-world usage patterns:

### 1. `SimpleFlatBenchmark` — Flat Object (10 Properties)

Mapping a single object with 10 primitive properties (`int`, `string`, `double`, `bool`). Properties have identical names in source and destination.

```
SimpleSource → SimpleDestination
Id, FirstName, LastName, Email, Age, Address, City, Country, Salary, IsActive
```

### 2. `NestedObjectBenchmark` — Nested Objects (2 Levels Deep)

Mapping an object graph with two levels of nesting. Tests each library's ability to traverse and recreate the hierarchy.

```
NestedSource
  └── NestedInnerSource
        └── NestedDeepSource
                ↓
NestedDestination
  └── NestedInnerDestination
        └── NestedDeepDestination
```

### 3. `CollectionBenchmark` — Collection of 100 Items

Mapping a `List<SimpleSource>` of 100 items to `List<SimpleDestination>`. Measures the cost of iterating and mapping in a loop; baseline uses explicit `foreach`.

### 4. `NameDifferenceBenchmark` — Differing Property Names

Mapping where property names differ between source and destination. Requires explicit configuration in each library.

```
NameDiffSource          →  NameDiffDestination
  Identifier            →    Id
  FirstName             →    Name
  LastName              →    Surname
  EmailAddress          →    Email
  PhoneNumber           →    Phone
```

---

## Collected Metrics

BenchmarkDotNet is configured with `[MemoryDiagnoser]` on all benchmarks. For each method, the following is recorded:

| Metric        | Description                                |
| ------------- | ------------------------------------------ |
| `mean_us`     | Arithmetic mean across all iterations (µs) |
| `median_us`   | 50th percentile (µs)                       |
| `p95_us`      | 95th percentile (µs)                       |
| `p99_us`      | 99th percentile (µs)                       |
| `stddev_us`   | Standard deviation (µs)                    |
| `alloc_bytes` | Bytes allocated on the heap per operation  |

The exact BenchmarkDotNet configuration is:

```csharp
var job = Job.Default
    .WithWarmupCount(3)    // 3 warmup iterations
    .WithIterationCount(5) // 5 measurement iterations
    .WithLaunchCount(1);   // 1 benchmarking process
```

> Results are also exported as **HTML** and **GitHub Markdown** under `BenchmarkDotNet.Artifacts/results/`.

---

## Project Architecture

```
DotnetMappingBenchmarks/
├── Benchmarks/
│   ├── BenchmarkData.cs          # Static test data reused across benchmarks
│   ├── MapperlyMapper.cs         # Mapperly mapper (partial class, source generator)
│   ├── SimpleFlatBenchmark.cs    # Scenario 1
│   ├── NestedObjectBenchmark.cs  # Scenario 2
│   ├── CollectionBenchmark.cs    # Scenario 3
│   └── NameDifferenceBenchmark.cs# Scenario 4
├── Models/
│   ├── SourceModels.cs           # SimpleSource, NestedSource, NameDiffSource
│   ├── DestinationModels.cs      # SimpleDestination, NestedDestination, NameDiffDestination
│   └── BenchmarkResultModels.cs  # Result DTOs serialized to JSON
├── Services/
│   ├── BenchmarkResultTransformer.cs # Converts BDN Summary[] to BenchmarkRunResult
│   └── JsonWriterService.cs      # Writes and maintains output JSON files
├── Helpers/
│   └── TimeZoneHelper.cs         # Timestamps in CET/Europe:Madrid with DST support
├── Program.cs                    # Entry point, configures BDN and orchestrates writing
└── appsettings.json              # Environment configuration
scripts/
├── run_benchmarks.sh             # Runs the published binary in production
└── check_versions.sh             # Checks and auto-updates NuGet versions
```

---

## Complete Flow Walkthrough

```
┌─────────────────────────────────────────────────────────┐
│  Program.cs                                             │
│                                                         │
│  1. Configures BenchmarkDotNet (job + exporters)        │
│  2. Runs the 4 benchmarks sequentially                  │
│     · SimpleFlatBenchmark                               │
│     · NestedObjectBenchmark                             │
│     · CollectionBenchmark                               │
│     · NameDifferenceBenchmark                           │
│  3. If all succeed:                                     │
│     · BenchmarkResultTransformer.Transform()            │
│       → groups by library, converts ns→µs               │
│     · JsonWriterService.WriteResultsAsync()             │
│       → updates history.json, last_result.json,         │
│          avg_results.json                               │
└─────────────────────────────────────────────────────────┘
```

### Result Transformation

`BenchmarkResultTransformer` receives the array of BenchmarkDotNet `Summary` objects and transforms them into the domain model:

- Converts nanoseconds → microseconds (÷ 1000)
- Groups cases by library (e.g., all "AutoMapper" entries from the 4 scenarios under a single library record)
- Resolves each library's version from the `AssemblyInformationalVersionAttribute` at runtime
- Normalizes names: `ManualForeach` → `Manual (foreach)`, `ManualLinq` → `Manual (LINQ)`

### Result Persistence

`JsonWriterService` manages three JSON files:

| File               | Content                                        |
| ------------------ | ---------------------------------------------- |
| `last_result.json` | Result from the most recent execution          |
| `history.json`     | Array of all executions from the last 3 months |
| `avg_results.json` | Averages of all executions in the history      |

Writes are **atomic**: data is written to a temporary `.tmp` file and then renamed, preventing corruption if interrupted.

Entries in `history.json` older than 3 months are automatically purged on each run, before `avg_results.json` is recalculated.

---

## Output JSON Format

```json
{
    "run_at": "2025-04-01T20:30:00+02:00",
    "libraries": [
        {
            "name": "Mapperly",
            "version": "5.0.0",
            "cases": [
                {
                    "name": "SimpleFlat",
                    "mean_us": 0.05,
                    "median_us": 0.05,
                    "p95_us": 0.06,
                    "p99_us": 0.07,
                    "stddev_us": 0.01,
                    "alloc_bytes": 0
                },
                { "name": "NestedObject", "...": "..." },
                { "name": "Collection", "...": "..." },
                { "name": "NameDifference", "...": "..." }
            ]
        },
        { "name": "AutoMapper", "...": "..." }
    ]
}
```

Timestamps use **CET/CEST (Europe/Madrid)** timezone with correct offset for DST, managed by `TimeZoneHelper` on both Windows (`Romance Standard Time`) and Linux (`Europe/Madrid`).

---

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Run in **Release** mode (BenchmarkDotNet requires it)

---

## Running the Benchmarks

### Locally (Development)

```bash
# Set environment so JSON writes to Documents
$env:DOTNET_ENVIRONMENT = "Development"  # PowerShell
# export DOTNET_ENVIRONMENT=Development  # bash

dotnet run -c Release --project DotnetMappingBenchmarks/DotnetMappingBenchmarks.csproj
```

> BenchmarkDotNet requires Release compilation. In Debug mode it emits a warning and may refuse to run.

### In Production (Linux)

```bash
# Publish
dotnet publish DotnetMappingBenchmarks/DotnetMappingBenchmarks.csproj \
  -c Release -o /opt/DotnetMappingBenchmarks/publish

# Run manually
bash scripts/run_benchmarks.sh
```

The `run_benchmarks.sh` script sets `DOTNET_ENVIRONMENT=Production` and `TZ=Europe/Madrid`, and redirects stdout/stderr to `/var/log/benchmarkworker/benchmarkworker.log`.

---

## Version Maintenance

The `scripts/check_versions.sh` script automates dependency updates:

1. Queries the [NuGet Flat Container API](https://api.nuget.org/v3-flatcontainer/{package}/index.json) for each package
2. Compares against the version in the `.csproj`
3. If different, runs `dotnet add package` with the latest version
4. Cleans `bin/obj`, restores, and publishes with `dotnet publish`
5. Triggers a new benchmark run after the update

All steps are logged with CET timestamps to `/var/log/benchmarkworker/version_check.log`.

```bash
bash scripts/check_versions.sh
```

---

## Why Mapperly is Different

Mapperly uses **source generators**: mapping code is generated at **compile time**, not at runtime. This has important implications for the benchmarks:

- **No initialization cost**: no reflection, no IL emission, no JIT compilation of expressions on first use
- **Zero allocations** on simple mappings: the compiler can optimize away intermediate allocations
- **Generated code is equivalent to manual code**: on simple scenarios, Mapperly and Manual should be statistically indistinguishable

The mapper is declared as a `partial class` with the `[Mapper]` attribute, and Riok.Mapperly generates the implementation:

```csharp
[Mapper]
public partial class MapperlyMapper
{
    public partial SimpleDestination MapSimple(SimpleSource source);
    public partial NestedDestination MapNested(NestedSource source);
    public partial List<SimpleDestination> MapCollection(List<SimpleSource> source);

    [MapProperty(nameof(NameDiffSource.Identifier), nameof(NameDiffDestination.Id))]
    [MapProperty(nameof(NameDiffSource.FirstName),  nameof(NameDiffDestination.Name))]
    // ...
    public partial NameDiffDestination MapNameDiff(NameDiffSource source);
}
```

---

## Model Structure

### Flat Object

```
SimpleSource / SimpleDestination
  Id (int) · FirstName · LastName · Email · Age · Address · City · Country · Salary (double) · IsActive (bool)
```

### Nested Objects

```
NestedSource              NestedDestination
  Id                        Id
  Name                      Name
  Inner (NestedInnerSource)  Inner (NestedInnerDestination)
    Code                       Code
    Description                Description
    Deep (NestedDeepSource)    Deep (NestedDeepDestination)
      Value                      Value
      Number                     Number
```

### Differing Names

```
NameDiffSource   →   NameDiffDestination
  Identifier     →     Id
  FirstName      →     Name
  LastName       →     Surname
  EmailAddress   →     Email
  PhoneNumber    →     Phone
```

---

## BenchmarkDotNet Exports

In addition to the domain JSON files, BenchmarkDotNet generates its own artifacts in `BenchmarkDotNet.Artifacts/results/`:

- `*-report.html` — Interactive visual report
- `*-report-github.md` — Markdown table ready to paste on GitHub
- `*-report-full.json` — Complete data in BDN format

---

## License

MIT
