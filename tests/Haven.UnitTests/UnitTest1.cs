using System.Reflection;

namespace Haven.UnitTests;

public sealed class ArchitectureTests
{
    [Fact]
    public void Domain_ShouldNotDependOnOuterLayers()
    {
        var references = GetReferencedProjectNames(typeof(Haven.Domain.Class1).Assembly);

        Assert.DoesNotContain("Haven.Application", references);
        Assert.DoesNotContain("Haven.Infrastructure", references);
        Assert.DoesNotContain("Haven.Web", references);
    }

    [Fact]
    public void Application_ShouldDependOnDomain()
    {
        var references = GetReferencedProjectNames(typeof(Haven.Application.Class1).Assembly);

        Assert.Contains("Haven.Domain", references);
        Assert.DoesNotContain("Haven.Infrastructure", references);
        Assert.DoesNotContain("Haven.Web", references);
    }

    private static HashSet<string> GetReferencedProjectNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null && name.StartsWith("Haven.", StringComparison.Ordinal))
            .Select(name => name!)
            .ToHashSet(StringComparer.Ordinal);
}
