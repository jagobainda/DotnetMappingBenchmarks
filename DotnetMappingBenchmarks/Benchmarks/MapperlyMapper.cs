using DotnetMappingBenchmarks.Models;
using Riok.Mapperly.Abstractions;

namespace DotnetMappingBenchmarks.Benchmarks;

[Mapper]
public partial class MapperlyMapper
{
    public partial SimpleDestination MapSimple(SimpleSource source);
    public partial NestedDestination MapNested(NestedSource source);
    public partial List<SimpleDestination> MapCollection(List<SimpleSource> source);

    [MapProperty(nameof(NameDiffSource.Identifier), nameof(NameDiffDestination.Id))]
    [MapProperty(nameof(NameDiffSource.FirstName), nameof(NameDiffDestination.Name))]
    [MapProperty(nameof(NameDiffSource.LastName), nameof(NameDiffDestination.Surname))]
    [MapProperty(nameof(NameDiffSource.EmailAddress), nameof(NameDiffDestination.Email))]
    [MapProperty(nameof(NameDiffSource.PhoneNumber), nameof(NameDiffDestination.Phone))]
    public partial NameDiffDestination MapNameDiff(NameDiffSource source);
}
