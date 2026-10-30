using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;

using Xunit;

namespace QuestMaster.Architecture.Tests;

// NFR-25 and ADR-0001: QuestMaster.Rules is the pure domain, with no ASP.NET Core, EF Core or I/O.
// Time and randomness are not forbidden yet: ADR-0003 decides where the dice source lives.
public class RulesPurityTests
{
    private const string RulesAssemblyName = "QuestMaster.Rules";

    private static readonly string[] ForbiddenNamespaces =
    [
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
        "System.Data",
        "System.IO",
        "System.Net",
    ];

    private static readonly string[] ForbiddenTypes =
    [
        "System.Console",
    ];

    // The compiled assembly lists every external type its code uses, so this catches actual usage.
    [Fact]
    public void Rules_code_uses_no_types_from_forbidden_namespaces()
    {
        var rulesAssemblyPath = Path.Combine(AppContext.BaseDirectory, RulesAssemblyName + ".dll");

        var forbiddenTypesInUse = ReadReferencedTypeNames(rulesAssemblyPath)
            .Where(IsForbidden)
            .Distinct()
            .Order()
            .ToList();

    Assert.Empty(forbiddenTypesInUse);
    }

    // The compiler drops a reference that no code uses, so the first test cannot see it.
    // This test catches the declared intent in the project file instead.
    [Fact]
    public void Rules_project_declares_no_references()
    {
        var projectFile = XDocument.Load(FindRulesProjectFile());
        string[] referenceKinds = ["ProjectReference", "PackageReference", "FrameworkReference"];

        var declaredReferences = projectFile.Descendants()
            .Where(element => referenceKinds.Contains(element.Name.LocalName))
            .Select(element => $"{element.Name.LocalName} {element.Attribute("Include")?.Value}")
            .ToList();

        Assert.Empty(declaredReferences);
    }

    private static List<string> ReadReferencedTypeNames(string assemblyPath)
    {
        using var stream = File.OpenRead(assemblyPath);
        using var peReader = new PEReader(stream);
        var metadata = peReader.GetMetadataReader();

        return metadata.TypeReferences
            .Select(handle => metadata.GetTypeReference(handle))
            .Select(typeReference => metadata.GetString(typeReference.Namespace) + "." + metadata.GetString(typeReference.Name))
            .ToList();
    }

    private static bool IsForbidden(string fullTypeName)
    {
        var isInForbiddenNamespace = ForbiddenNamespaces.Any(forbidden => fullTypeName.StartsWith(forbidden + ".", StringComparison.Ordinal));
        var isForbiddenType = ForbiddenTypes.Contains(fullTypeName);
        return isInForbiddenNamespace || isForbiddenType;
    }

    // Tests run from tests/<project>/bin/<configuration>/<framework>, so walk up to the folder that holds the solution file.
    private static string FindRulesProjectFile()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "QuestMaster.slnx")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException("Could not find QuestMaster.slnx above " + AppContext.BaseDirectory);
        }

        return Path.Combine(directory.FullName, "src", RulesAssemblyName, RulesAssemblyName + ".csproj");
    }
}
