// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.RegularExpressions;

public sealed class ApiDashboardOverclaimFirewallTests
{
    [Fact]
    public void ApiDashboardOverclaimFirewall_AllowsNegativeWordingExamples()
    {
        Assert.Empty(FindViolations("not semantic quality proof"));
        Assert.Empty(FindViolations("not host runtime proof"));
        Assert.Empty(FindViolations("fresh-load is not complete in dashboard smoke"));
        Assert.Empty(FindViolations("continue-training remains a future gate, not dashboard evidence"));
    }

    [Fact]
    public void ApiDashboardOverclaimFirewall_RejectsPositiveOverclaimExamples()
    {
        Assert.NotEmpty(FindViolations("semantic quality proven by dashboard smoke"));
        Assert.NotEmpty(FindViolations("Claude host runtime proven by dashboard"));
        Assert.NotEmpty(FindViolations("trained model readiness proven by preview"));
        Assert.NotEmpty(FindViolations("checkpoint rewrite complete after smoke test"));
    }

    [Fact]
    public void ApiDashboardOverclaimFirewall_ScansDashboardDocsSourceAndP42Artifacts()
    {
        var root = FindRepositoryRoot();
        var targets = EnumerateScanTargets(root);
        var violations = targets
            .SelectMany(path => FindViolations(File.ReadAllText(path), Path.GetRelativePath(root, path)))
            .ToArray();

        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static IReadOnlyList<string> EnumerateScanTargets(
        string root)
    {
        var targets = new List<string>
        {
            Path.Combine(root, "docs", "projects", "43-smart-skills-domain-dashboard.md"),
            Path.Combine(root, "artifacts", "smart-skills-semantic-quality-completion", "p37-runner-dashboard-wiring-design", "p37-readiness-gap-ledger.jsonl")
        };

        targets.AddRange(Directory
            .EnumerateFiles(Path.Combine(root, "43.SmartSkillsDomain.Dashboard"), "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".xaml", StringComparison.Ordinal)));

        var p42ArtifactDirectory = Path.Combine(root, "artifacts", "smart-skills-semantic-quality-completion", "p42-dashboard-overclaim-firewall");
        if (Directory.Exists(p42ArtifactDirectory))
        {
            targets.AddRange(Directory
                .EnumerateFiles(p42ArtifactDirectory, "*.*", SearchOption.AllDirectories)
                .Where(path => path.EndsWith(".md", StringComparison.Ordinal) || path.EndsWith(".json", StringComparison.Ordinal) || path.EndsWith(".jsonl", StringComparison.Ordinal)));
        }

        return targets.Where(File.Exists).Distinct(StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<string> FindViolations(
        string text,
        string source = "example")
    {
        var violations = new List<string>();
        var lines = Regex.Split(text, "\r?\n");
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (IsAllowedContext(line))
                continue;

            foreach (var pattern in Patterns)
            {
                if (pattern.Regex.IsMatch(line))
                    violations.Add($"{source}:{index + 1}:{pattern.Name}:{line.Trim()}");
            }
        }

        return violations;
    }

    private static bool IsAllowedContext(
        string text)
    {
        var lower = text.ToLowerInvariant();
        return AllowedContextMarkers.Any(lower.Contains);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "43.SmartSkillsDomain.Dashboard", "ApiDashboardCatalog.cs")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root for dashboard overclaim tests.");
    }

    private static readonly IReadOnlyList<string> AllowedContextMarkers =
    [
        "not ",
        "no ",
        "never",
        "cannot",
        "does not",
        "do not",
        "doesn't",
        "false",
        "without",
        "outside",
        "separate",
        "future",
        "missing",
        "blocked",
        "gap",
        "remains",
        "forbidden",
        "reject",
        "rejected",
        "must fail",
        "should fail",
        "violation",
        "negative",
        "non-claim",
        "not claimed",
        "no claim"
    ];

    private static readonly IReadOnlyList<OverclaimPattern> Patterns =
    [
        new("semantic_quality_forward", new(@"\bsemantic[- ]quality\b.{0,80}\b(proven|proved|ready|complete|completed|closed|passes|passed)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)),
        new("semantic_quality_reverse", new(@"\b(proves?|proved|proven|readies|completes?)\b.{0,80}\bsemantic[- ]quality\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)),
        new("trained_model_readiness", new(@"\btrained[- ]model(?:\s+readiness)?\b.{0,80}\b(proven|proved|ready|complete|completed|passes|passed)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)),
        new("host_runtime_forward", new(@"\b(claude|cursor|codex)\b.{0,40}\bhost[- ]runtime\b.{0,80}\b(proven|proved|ready|complete|completed|passes|passed)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)),
        new("host_runtime_reverse", new(@"\b(proves?|proved|proven|readies|completes?)\b.{0,80}\b(claude|cursor|codex)\b.{0,40}\bhost[- ]runtime\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)),
        new("checkpoint_lifecycle", new(@"\b(checkpoint rewrite|fresh-load|fresh load|continue-training|continue training)\b.{0,80}\b(complete|completed|proven|proved|ready|passes|passed)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
    ];

    private sealed record OverclaimPattern(
        string Name,
        Regex Regex);
}
