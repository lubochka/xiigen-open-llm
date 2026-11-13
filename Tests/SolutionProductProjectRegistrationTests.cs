namespace LlmFromScratch.Tests;

using System.Text.RegularExpressions;

public sealed class SolutionProductProjectRegistrationTests
{
    private const string CSharpProjectTypeGuid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
    private const string SolutionFolderTypeGuid = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";
    private const string SmartSkillsName = "43.SmartSkillsDomain";
    private const string SmartSkillsProjectPath =
        "43.SmartSkillsDomain\\43.SmartSkillsDomain.csproj";
    private const string DesignConversionName = "44.DesignConversionDomain";
    private const string DesignConversionProjectPath =
        "44.DesignConversionDomain\\44.DesignConversionDomain.csproj";
    private const string DesignConversionFolderGuid =
        "{909EFAD7-EB84-CA7C-482E-6D00AD6154B8}";
    private const string DesignConversionProjectGuid =
        "{9FC390FA-B4E7-4A54-A352-64884D5AF493}";

    private static readonly string[] ExpectedSolutionConfigurations =
    [
        "Debug|Any CPU",
        "Debug|x64",
        "Debug|x86",
        "Release|Any CPU",
        "Release|x64",
        "Release|x86"
    ];

    private static readonly Regex ProjectLinePattern = new(
        "^Project\\(\"(?<type>\\{[0-9A-Fa-f-]+\\})\"\\) = \"(?<name>[^\"]+)\", \"(?<path>[^\"]+)\", \"(?<id>\\{[0-9A-Fa-f-]+\\})\"$",
        RegexOptions.CultureInvariant);

    private static readonly Regex NestedProjectLinePattern = new(
        "^\\s*(?<child>\\{[0-9A-Fa-f-]+\\}) = (?<parent>\\{[0-9A-Fa-f-]+\\})$",
        RegexOptions.CultureInvariant);

    private static readonly Regex ProjectConfigurationLinePattern = new(
        "^\\s*(?<id>\\{[0-9A-Fa-f-]+\\})\\.(?<solution>.+)\\.(?<kind>ActiveCfg|Build\\.0) = (?<project>.+)$",
        RegexOptions.CultureInvariant);

    [Fact]
    public void Solution_RegistersSmartSkillsDomainExactlyOnceInsideMatchingFolder()
    {
        var solution = ReadSolution();
        var project = Assert.Single(solution.Projects, candidate =>
            string.Equals(candidate.Path, SmartSkillsProjectPath, StringComparison.Ordinal));
        var folder = Assert.Single(solution.Projects, candidate =>
            string.Equals(candidate.TypeGuid, SolutionFolderTypeGuid, StringComparison.OrdinalIgnoreCase)
            && string.Equals(candidate.Name, SmartSkillsName, StringComparison.Ordinal)
            && string.Equals(candidate.Path, SmartSkillsName, StringComparison.Ordinal));

        Assert.Equal(SmartSkillsName, project.Name);
        Assert.Equal(
            CSharpProjectTypeGuid,
            project.TypeGuid,
            StringComparer.OrdinalIgnoreCase);
        Assert.False(string.Equals(
            project.ProjectGuid,
            folder.ProjectGuid,
            StringComparison.OrdinalIgnoreCase));

        var projectGuids = solution.Projects
            .Select(candidate => candidate.ProjectGuid)
            .ToArray();
        Assert.Equal(
            projectGuids.Length,
            projectGuids.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        var nesting = Assert.Single(solution.NestedProjects, candidate =>
            string.Equals(
                candidate.ChildProjectGuid,
                project.ProjectGuid,
                StringComparison.OrdinalIgnoreCase));
        Assert.Equal(
            folder.ProjectGuid,
            nesting.ParentProjectGuid,
            StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Solution_RegistersSmartSkillsDomainForEveryBuildConfiguration()
    {
        var solution = ReadSolution();
        var project = Assert.Single(solution.Projects, candidate =>
            string.Equals(candidate.Path, SmartSkillsProjectPath, StringComparison.Ordinal));

        Assert.Equal(
            ExpectedSolutionConfigurations.OrderBy(value => value, StringComparer.Ordinal),
            solution.SolutionConfigurations.OrderBy(value => value, StringComparer.Ordinal));
        AssertEveryBuildConfiguration(solution, project.ProjectGuid);
    }

    [Fact]
    public void Solution_PreservesDesignConversionDomainRegistration()
    {
        var solution = ReadSolution();
        var project = Assert.Single(solution.Projects, candidate =>
            string.Equals(candidate.Path, DesignConversionProjectPath, StringComparison.Ordinal));
        var folder = Assert.Single(solution.Projects, candidate =>
            string.Equals(candidate.TypeGuid, SolutionFolderTypeGuid, StringComparison.OrdinalIgnoreCase)
            && string.Equals(candidate.Name, DesignConversionName, StringComparison.Ordinal)
            && string.Equals(candidate.Path, DesignConversionName, StringComparison.Ordinal));

        Assert.Equal(DesignConversionName, project.Name);
        Assert.Equal(
            CSharpProjectTypeGuid,
            project.TypeGuid,
            StringComparer.OrdinalIgnoreCase);
        Assert.Equal(
            DesignConversionProjectGuid,
            project.ProjectGuid,
            StringComparer.OrdinalIgnoreCase);
        Assert.Equal(
            DesignConversionFolderGuid,
            folder.ProjectGuid,
            StringComparer.OrdinalIgnoreCase);

        var nesting = Assert.Single(solution.NestedProjects, candidate =>
            string.Equals(
                candidate.ChildProjectGuid,
                project.ProjectGuid,
                StringComparison.OrdinalIgnoreCase));
        Assert.Equal(
            folder.ProjectGuid,
            nesting.ParentProjectGuid,
            StringComparer.OrdinalIgnoreCase);
        AssertEveryBuildConfiguration(solution, project.ProjectGuid);
    }

    private static void AssertEveryBuildConfiguration(
        SolutionSnapshot solution,
        string projectGuid)
    {
        var projectConfigurations = solution.ProjectConfigurations
            .Where(candidate => string.Equals(
                candidate.ProjectGuid,
                projectGuid,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        Assert.Equal(ExpectedSolutionConfigurations.Length * 2, projectConfigurations.Length);

        foreach (var configuration in ExpectedSolutionConfigurations)
        {
            var expectedProjectConfiguration = configuration.StartsWith(
                "Debug|",
                StringComparison.Ordinal)
                ? "Debug|Any CPU"
                : "Release|Any CPU";
            var active = Assert.Single(projectConfigurations, candidate =>
                string.Equals(
                    candidate.SolutionConfiguration,
                    configuration,
                    StringComparison.Ordinal)
                && string.Equals(candidate.MappingKind, "ActiveCfg", StringComparison.Ordinal));
            var build = Assert.Single(projectConfigurations, candidate =>
                string.Equals(
                    candidate.SolutionConfiguration,
                    configuration,
                    StringComparison.Ordinal)
                && string.Equals(candidate.MappingKind, "Build.0", StringComparison.Ordinal));

            Assert.Equal(expectedProjectConfiguration, active.MappedProjectConfiguration);
            Assert.Equal(expectedProjectConfiguration, build.MappedProjectConfiguration);
        }
    }

    private static SolutionSnapshot ReadSolution()
    {
        var lines = File.ReadAllLines(Path.Combine(GetRepoRoot(), "LlmFromScratch.sln"));
        var projects = lines
            .Select(line => ProjectLinePattern.Match(line))
            .Where(match => match.Success)
            .Select(match => new SolutionProject(
                match.Groups["type"].Value,
                match.Groups["name"].Value,
                match.Groups["path"].Value,
                match.Groups["id"].Value))
            .ToArray();
        var nestedProjects = ReadGlobalSection(lines, "NestedProjects")
            .Select(line => NestedProjectLinePattern.Match(line))
            .Where(match => match.Success)
            .Select(match => new NestedProject(
                match.Groups["child"].Value,
                match.Groups["parent"].Value))
            .ToArray();
        var projectConfigurations = ReadGlobalSection(lines, "ProjectConfigurationPlatforms")
            .Select(line => ProjectConfigurationLinePattern.Match(line))
            .Where(match => match.Success)
            .Select(match => new ProjectConfiguration(
                match.Groups["id"].Value,
                match.Groups["solution"].Value,
                match.Groups["kind"].Value,
                match.Groups["project"].Value))
            .ToArray();
        var solutionConfigurations = ReadGlobalSection(lines, "SolutionConfigurationPlatforms")
            .Select(line => line.Trim().Split(" = ", StringSplitOptions.None))
            .Where(parts => parts.Length == 2 && string.Equals(
                parts[0],
                parts[1],
                StringComparison.Ordinal))
            .Select(parts => parts[0])
            .ToArray();

        return new SolutionSnapshot(
            projects,
            nestedProjects,
            projectConfigurations,
            solutionConfigurations);
    }

    private static IEnumerable<string> ReadGlobalSection(
        IReadOnlyList<string> lines,
        string sectionName)
    {
        var start = Enumerable.Range(0, lines.Count)
            .Single(index => lines[index].Trim().StartsWith(
                $"GlobalSection({sectionName}) = ",
                StringComparison.Ordinal));

        for (var index = start + 1; index < lines.Count; index++)
        {
            if (string.Equals(lines[index].Trim(), "EndGlobalSection", StringComparison.Ordinal))
                yield break;
            yield return lines[index];
        }

        throw new InvalidDataException($"Solution section {sectionName} is not terminated.");
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root containing LlmFromScratch.sln was not found.");
    }

    private sealed record SolutionSnapshot(
        IReadOnlyList<SolutionProject> Projects,
        IReadOnlyList<NestedProject> NestedProjects,
        IReadOnlyList<ProjectConfiguration> ProjectConfigurations,
        IReadOnlyList<string> SolutionConfigurations);

    private sealed record SolutionProject(
        string TypeGuid,
        string Name,
        string Path,
        string ProjectGuid);

    private sealed record NestedProject(
        string ChildProjectGuid,
        string ParentProjectGuid);

    private sealed record ProjectConfiguration(
        string ProjectGuid,
        string SolutionConfiguration,
        string MappingKind,
        string MappedProjectConfiguration);
}
