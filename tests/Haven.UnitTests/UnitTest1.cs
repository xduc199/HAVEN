using System.Reflection;
using System.Xml.Linq;

namespace Haven.UnitTests;

public sealed class ArchitectureTests
{
    [Fact]
    public void Domain_ShouldNotDependOnOuterLayers()
    {
        var references = GetReferencedProjectNames(
            typeof(Haven.Domain.Class1).Assembly);

        Assert.DoesNotContain("Haven.Application", references);
        Assert.DoesNotContain("Haven.Infrastructure", references);
        Assert.DoesNotContain("Haven.Web", references);
    }

    [Fact]
    public void Application_ShouldDependOnDomain_AndNotOuterLayers()
    {
        var projectPath = FindRepositoryFile(
            Path.Combine(
                "src",
                "Haven.Application",
                "Haven.Application.csproj"));

        var references = XDocument.Load(projectPath)
            .Descendants("ProjectReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include =>
                Path.GetFileNameWithoutExtension(
                    include!.Replace('\\', '/')))
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("Haven.Domain", references);
        Assert.DoesNotContain("Haven.Infrastructure", references);
        Assert.DoesNotContain("Haven.Web", references);
    }

    private static HashSet<string> GetReferencedProjectNames(
        Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name =>
                name is not null &&
                name.StartsWith("Haven.", StringComparison.Ordinal))
            .Select(name => name!)
            .ToHashSet(StringComparer.Ordinal);

    private static string FindRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var solutionPath = Path.Combine(
                directory.FullName, "HAVEN.slnx");

            var targetPath = Path.Combine(
                directory.FullName, relativePath);

            if (File.Exists(solutionPath) && File.Exists(targetPath))
            {
                return targetPath;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"Could not find '{relativePath}' from the test output directory.");
    }
}