namespace DotnetMappingBenchmarks.Models;

public class SimpleSource
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

public class NestedSource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NestedInnerSource Inner { get; set; } = new();
}

public class NestedInnerSource
{
    public int Code { get; set; }
    public string Description { get; set; } = string.Empty;
    public NestedDeepSource Deep { get; set; } = new();
}

public class NestedDeepSource
{
    public string Value { get; set; } = string.Empty;
    public int Number { get; set; }
}

public class NameDiffSource
{
    public int Identifier { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
