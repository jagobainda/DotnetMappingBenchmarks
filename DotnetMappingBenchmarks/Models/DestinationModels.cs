namespace DotnetMappingBenchmarks.Models;

public class SimpleDestination
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Salary { get; set; }
    public bool IsActive { get; set; }
}

public class NestedDestination
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NestedInnerDestination Inner { get; set; } = new();
}

public class NestedInnerDestination
{
    public int Code { get; set; }
    public string Description { get; set; } = string.Empty;
    public NestedDeepDestination Deep { get; set; } = new();
}

public class NestedDeepDestination
{
    public string Value { get; set; } = string.Empty;
    public int Number { get; set; }
}

public class NameDiffDestination
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
