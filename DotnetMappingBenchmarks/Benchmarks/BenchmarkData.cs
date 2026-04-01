using DotnetMappingBenchmarks.Models;

namespace DotnetMappingBenchmarks.Benchmarks;

public static class BenchmarkData
{
    public static SimpleSource CreateSimpleSource() => new()
    {
        Id = 1,
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        Age = 30,
        Address = "123 Main St",
        City = "Springfield",
        Country = "US",
        Salary = 75000.50,
        IsActive = true
    };

    public static NestedSource CreateNestedSource() => new()
    {
        Id = 1,
        Name = "Parent",
        Inner = new NestedInnerSource
        {
            Code = 42,
            Description = "Inner level",
            Deep = new NestedDeepSource
            {
                Value = "Deep value",
                Number = 99
            }
        }
    };

    public static NameDiffSource CreateNameDiffSource() => new()
    {
        Identifier = 1,
        FirstName = "Jane",
        LastName = "Smith",
        EmailAddress = "jane.smith@example.com",
        PhoneNumber = "+1234567890"
    };

    public static List<SimpleSource> CreateSimpleSourceList(int count = 100) =>
        [.. Enumerable.Range(1, count).Select(i => new SimpleSource
        {
            Id = i,
            FirstName = $"First{i}",
            LastName = $"Last{i}",
            Email = $"user{i}@example.com",
            Age = 20 + (i % 50),
            Address = $"{i} Main St",
            City = $"City{i}",
            Country = $"Country{i % 10}",
            Salary = 30000 + (i * 100),
            IsActive = i % 2 == 0
        })];
}
