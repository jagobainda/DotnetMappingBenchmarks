Create a .NET 10 Worker Service project called `BenchmarkWorker`. The worker benchmarks .NET object mapping libraries and writes results to JSON files served as static files via Nginx.

**Libraries to benchmark:** AutoMapper, Mapperly, Mapster, TinyMapper. Add any other relevant actively-maintained .NET mapping libraries you know of.

**Benchmark cases** (implement all for every library):
- Simple flat object mapping (10 properties)
- Nested object mapping (2 levels deep)
- Collection mapping (list of 100 items)
- Mapping with field name differences (e.g. `FirstName` → `Name`)

Each benchmark case must do a warmup of 3 iterations before measuring. Then measure 100 iterations and record: mean, median, p95, p99, standard deviation. All times in microseconds.

**Worker behavior:**
- Runs every 5 minutes via a configurable interval in `appsettings.json`
- Output directory for JSON files is configurable in `appsettings.json` (will be an Nginx-served static folder)
- After each full benchmark run, writes or overwrites three JSON files:

`last_result.json`:
```json
{
  "run_at": "2025-01-15T20:30:00+01:00",
  "libraries": [
    {
      "name": "AutoMapper",
      "version": "13.0.1",
      "cases": [
        {
          "name": "SimpleFlat",
          "mean_us": 1.23,
          "median_us": 1.20,
          "p95_us": 1.80,
          "p99_us": 2.10,
          "stddev_us": 0.15
        }
      ]
    }
  ]
}
```

`avg_results.json` — same structure as `last_result.json` but all numeric values are the average across the last 50 runs stored in `history.json`.

`history.json`:
```json
[
  {
    "run_at": "2025-01-15T20:30:00+01:00",
    "libraries": [ ...same structure as last_result libraries array... ]
  }
]
```

On every run, before writing, purge from `history.json` any entries older than 3 months. If `history.json` does not exist yet, create it. Always append the new run, then purge, then recompute `avg_results.json` from whatever remains.

**Project structure:**
- `Benchmarks/` — one file per library, each implementing a common `ILibraryBenchmark` interface with a `RunAsync()` method returning a `LibraryBenchmarkResult`
- `Models/` — all result/history DTOs
- `Services/BenchmarkRunnerService.cs` — orchestrates all benchmarks
- `Services/JsonWriterService.cs` — handles all file I/O and the purge logic
- `Worker.cs` — the hosted service loop

**Also create** `scripts/check_versions.sh`: a bash script that for each library queries the NuGet API (`https://api.nuget.org/v3-flatcontainer/{package}/index.json`), compares the latest version against the version currently referenced in `BenchmarkWorker.csproj`, and if any differ, updates the `.csproj` with `dotnet add package`, then runs `dotnet build` to verify, and finally restarts the `benchmarkworker` systemd service with `systemctl restart benchmarkworker`. The script must log every action with timestamp to `/var/log/benchmarkworker/version_check.log`.

Also create `benchmarkworker.service`: a systemd unit file ready to drop into `/etc/systemd/system/` that runs the worker as a service on Ubuntu Server, with `Restart=always` and the correct `WorkingDirectory`.

All timestamps must be in CET (Europe/Madrid timezone). Use `TimeZoneInfo` to ensure correct offset including DST.

Do not use BenchmarkDotNet. Do not use any database. Use `System.Text.Json` for all serialization. Use file locking when writing JSON files to avoid corruption if the worker is restarted mid-write.

Generate ALL files in full, no placeholders, no "implement this yourself" comments. Every library must be fully configured and ready to run.