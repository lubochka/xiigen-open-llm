// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain.Dashboard;

using System.IO;
using System.Text.Json;
using AgentInfrastructure;

public sealed record ApiDashboardSourceDisjointGuardReport(
    int FixtureCount,
    IReadOnlyDictionary<string, int> CountsByMachine,
    IReadOnlyList<string> DuplicateFixtureIds,
    IReadOnlyList<string> MissingManifestFamilies,
    IReadOnlyList<string> UnexpectedManifestFamilies,
    IReadOnlyList<string> FamiliesBothTrainAndEval,
    bool UniversalOnly,
    bool RawPrivateTextCopied,
    bool TrainingRun,
    bool EvalRun,
    bool CheckpointMutated,
    bool SemanticQualityClaimed,
    bool CanProceedToEvalRunner,
    bool SourceDisjointPrecheckPassed,
    string EvidenceScope,
    IReadOnlyList<string> Problems);

public sealed class ApiDashboardSourceDisjointGuard
{
    public const string EvidenceScope = "P41 parse-only source-disjoint pre-eval guard; no model eval, no training, no checkpoint mutation, no semantic-quality proof";
    private const string P36ManifestPath = "artifacts/smart-skills-semantic-quality-completion/p36-eval-fixture-rows/p36-source-disjoint-manifest.json";
    private const string P36FixtureRowsPath = "artifacts/smart-skills-semantic-quality-completion/p36-eval-fixture-rows/p36-eval-fixtures.jsonl";

    public OperationResult<ApiDashboardSourceDisjointGuardReport> VerifyP36FixtureRows(
        string? workspaceRoot = null)
        => VerifySourceDisjointRows(
            manifestPath: P36ManifestPath,
            fixtureRowsPath: P36FixtureRowsPath,
            workspaceRoot: workspaceRoot);

    public OperationResult<ApiDashboardSourceDisjointGuardReport> VerifySourceDisjointRows(
        string manifestPath,
        string fixtureRowsPath,
        string? workspaceRoot = null)
    {
        var manifestFullPath = ResolvePath(manifestPath, workspaceRoot);
        var fixtureRowsFullPath = ResolvePath(fixtureRowsPath, workspaceRoot);
        if (!File.Exists(manifestFullPath))
        {
            return OperationResult<ApiDashboardSourceDisjointGuardReport>.Fail(
                $"P36 source-disjoint manifest was not found at '{manifestFullPath}'.",
                "API_DASHBOARD_SOURCE_DISJOINT_MANIFEST_MISSING");
        }

        if (!File.Exists(fixtureRowsFullPath))
        {
            return OperationResult<ApiDashboardSourceDisjointGuardReport>.Fail(
                $"P36 fixture rows were not found at '{fixtureRowsFullPath}'.",
                "API_DASHBOARD_SOURCE_DISJOINT_FIXTURES_MISSING");
        }

        try
        {
            using var manifestDocument = JsonDocument.Parse(File.ReadAllText(manifestFullPath));
            var manifest = ReadManifest(manifestDocument.RootElement);
            var fixtures = ReadFixtureRows(fixtureRowsFullPath);
            return OperationResult<ApiDashboardSourceDisjointGuardReport>.Ok(BuildReport(manifest, fixtures));
        }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidOperationException)
        {
            return OperationResult<ApiDashboardSourceDisjointGuardReport>.Fail(
                $"P36 source-disjoint guard could not parse manifest or fixture rows: {ex.Message}",
                "API_DASHBOARD_SOURCE_DISJOINT_PARSE_FAILED");
        }
    }

    private static ApiDashboardSourceDisjointGuardReport BuildReport(
        SourceDisjointManifest manifest,
        IReadOnlyList<SourceDisjointFixtureRow> fixtures)
    {
        var countsByMachine = fixtures
            .GroupBy(row => row.Machine, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var duplicateFixtureIds = fixtures
            .GroupBy(row => row.FixtureId, StringComparer.Ordinal)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        var fixtureFamilies = fixtures
            .Select(row => row.HeldoutSourceFamily)
            .Where(family => !string.IsNullOrWhiteSpace(family))
            .ToHashSet(StringComparer.Ordinal);
        var missingManifestFamilies = fixtureFamilies
            .Except(manifest.Families, StringComparer.Ordinal)
            .OrderBy(family => family, StringComparer.Ordinal)
            .ToArray();
        var unexpectedManifestFamilies = manifest.Families
            .Except(fixtureFamilies, StringComparer.Ordinal)
            .OrderBy(family => family, StringComparer.Ordinal)
            .ToArray();
        var familiesBothTrainAndEval = fixtures
            .Where(row => !string.IsNullOrWhiteSpace(row.HeldoutSourceFamily))
            .GroupBy(row => row.HeldoutSourceFamily, StringComparer.Ordinal)
            .Where(group => group.Any(row => IsTrainRole(row.SplitRole)) && group.Any(row => IsEvalRole(row.SplitRole)))
            .Select(group => group.Key)
            .OrderBy(family => family, StringComparer.Ordinal)
            .ToArray();
        var universalOnly = manifest.UniversalOnly && fixtures.All(row => row.UniversalOnly);
        var rawPrivateTextCopied = manifest.RawPrivateTextCopied || fixtures.Any(row => row.RawPrivateTextCopied);
        var problems = BuildProblems(
            manifest,
            fixtures,
            countsByMachine,
            duplicateFixtureIds,
            missingManifestFamilies,
            unexpectedManifestFamilies,
            familiesBothTrainAndEval,
            universalOnly,
            rawPrivateTextCopied);

        return new ApiDashboardSourceDisjointGuardReport(
            FixtureCount: fixtures.Count,
            CountsByMachine: countsByMachine,
            DuplicateFixtureIds: duplicateFixtureIds,
            MissingManifestFamilies: missingManifestFamilies,
            UnexpectedManifestFamilies: unexpectedManifestFamilies,
            FamiliesBothTrainAndEval: familiesBothTrainAndEval,
            UniversalOnly: universalOnly,
            RawPrivateTextCopied: rawPrivateTextCopied,
            TrainingRun: manifest.TrainingRun,
            EvalRun: manifest.EvalRun,
            CheckpointMutated: manifest.CheckpointMutated,
            SemanticQualityClaimed: manifest.SemanticQualityClaimed,
            CanProceedToEvalRunner: false,
            SourceDisjointPrecheckPassed: problems.Count == 0,
            EvidenceScope: EvidenceScope,
            Problems: problems);
    }

    private static IReadOnlyList<string> BuildProblems(
        SourceDisjointManifest manifest,
        IReadOnlyList<SourceDisjointFixtureRow> fixtures,
        IReadOnlyDictionary<string, int> countsByMachine,
        IReadOnlyList<string> duplicateFixtureIds,
        IReadOnlyList<string> missingManifestFamilies,
        IReadOnlyList<string> unexpectedManifestFamilies,
        IReadOnlyList<string> familiesBothTrainAndEval,
        bool universalOnly,
        bool rawPrivateTextCopied)
    {
        var problems = new List<string>();
        problems.AddRange(duplicateFixtureIds.Select(id => $"duplicate fixture id '{id}'"));
        problems.AddRange(missingManifestFamilies.Select(family => $"fixture family '{family}' is missing from the manifest"));
        problems.AddRange(unexpectedManifestFamilies.Select(family => $"manifest family '{family}' is not present in fixture rows"));
        problems.AddRange(familiesBothTrainAndEval.Select(family => $"source family '{family}' appears in both train and eval roles"));
        problems.AddRange(manifest.CountsByMachine
            .Where(entry => countsByMachine.TryGetValue(entry.Key, out var count) && count != entry.Value)
            .Select(entry => $"machine '{entry.Key}' count is {countsByMachine[entry.Key]} but manifest says {entry.Value}"));
        problems.AddRange(manifest.CountsByMachine
            .Where(entry => !countsByMachine.ContainsKey(entry.Key) && entry.Value != 0)
            .Select(entry => $"machine '{entry.Key}' is missing from fixture rows but manifest says {entry.Value}"));
        problems.AddRange(countsByMachine
            .Where(entry => !manifest.CountsByMachine.ContainsKey(entry.Key))
            .Select(entry => $"machine '{entry.Key}' is present in fixture rows but missing from manifest counts"));
        if (!universalOnly)
            problems.Add("manifest and fixture rows are not all universal-only");
        if (rawPrivateTextCopied)
            problems.Add("manifest or fixture rows indicate raw private text was copied");
        if (manifest.TrainingRun)
            problems.Add("manifest says a training run occurred");
        if (manifest.EvalRun)
            problems.Add("manifest says an eval run occurred");
        if (manifest.CheckpointMutated)
            problems.Add("manifest says a checkpoint was mutated");
        if (manifest.SemanticQualityClaimed)
            problems.Add("manifest says semantic quality was claimed");
        problems.AddRange(fixtures
            .Where(row => row.TrainingAllowed || row.EvalExecutionAllowed || !row.NotExecutedYet)
            .Select(row => $"fixture '{row.FixtureId}' has an unsafe execution flag"));
        return problems.OrderBy(problem => problem, StringComparer.Ordinal).ToArray();
    }

    private static SourceDisjointManifest ReadManifest(
        JsonElement root)
        => new(
            CountsByMachine: ReadStringIntDictionary(root, "countsByMachine"),
            Families: ReadManifestFamilies(root),
            UniversalOnly: ReadBool(root, "universalOnly"),
            RawPrivateTextCopied: ReadBool(root, "rawPrivateTextCopied"),
            TrainingRun: ReadBool(root, "trainingRun"),
            EvalRun: ReadBool(root, "evalRun"),
            CheckpointMutated: ReadBool(root, "checkpointMutated"),
            SemanticQualityClaimed: ReadBool(root, "semanticQualityClaimed"));

    private static IReadOnlyList<SourceDisjointFixtureRow> ReadFixtureRows(
        string path)
    {
        var rows = new List<SourceDisjointFixtureRow>();
        foreach (var line in File.ReadLines(path).Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            rows.Add(new SourceDisjointFixtureRow(
                FixtureId: ReadString(root, "fixture_id"),
                Machine: ReadString(root, "machine"),
                SplitRole: ReadString(root, "split_role"),
                HeldoutSourceFamily: ReadString(root, "heldout_source_family"),
                UniversalOnly: ReadBool(root, "universal_only"),
                RawPrivateTextCopied: ReadBool(root, "raw_private_text_copied"),
                TrainingAllowed: ReadBool(root, "training_allowed"),
                EvalExecutionAllowed: ReadBool(root, "eval_execution_allowed"),
                NotExecutedYet: ReadBool(root, "not_executed_yet")));
        }

        return rows;
    }

    private static IReadOnlyDictionary<string, int> ReadStringIntDictionary(
        JsonElement root,
        string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Object)
            return new Dictionary<string, int>(StringComparer.Ordinal);

        return value.EnumerateObject().ToDictionary(
            property => property.Name,
            property => property.Value.ValueKind == JsonValueKind.Number ? property.Value.GetInt32() : 0,
            StringComparer.Ordinal);
    }

    private static IReadOnlySet<string> ReadManifestFamilies(
        JsonElement root)
    {
        var families = new HashSet<string>(StringComparer.Ordinal);
        if (!root.TryGetProperty("countsByMachineAndFamily", out var byMachine) || byMachine.ValueKind != JsonValueKind.Object)
            return families;

        foreach (var machine in byMachine.EnumerateObject())
        {
            if (machine.Value.ValueKind != JsonValueKind.Object)
                continue;

            foreach (var family in machine.Value.EnumerateObject())
                families.Add(family.Name);
        }

        return families;
    }

    private static string ReadString(
        JsonElement row,
        string propertyName)
        => row.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static bool ReadBool(
        JsonElement row,
        string propertyName)
        => row.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.True;

    private static bool IsTrainRole(
        string splitRole)
        => splitRole.Contains("train", StringComparison.OrdinalIgnoreCase);

    private static bool IsEvalRole(
        string splitRole)
        => splitRole.Contains("eval", StringComparison.OrdinalIgnoreCase)
            || splitRole.Contains("holdout", StringComparison.OrdinalIgnoreCase);

    private static string ResolvePath(
        string path,
        string? workspaceRoot)
        => Path.IsPathRooted(path)
            ? path
            : Path.Combine(ResolveWorkspaceRoot(workspaceRoot), path);

    private static string ResolveWorkspaceRoot(string? workspaceRoot)
    {
        if (!string.IsNullOrWhiteSpace(workspaceRoot))
            return workspaceRoot;

        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "43.SmartSkillsDomain.Dashboard", "ApiDashboardCatalog.cs")))
                return current.FullName;

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    private sealed record SourceDisjointManifest(
        IReadOnlyDictionary<string, int> CountsByMachine,
        IReadOnlySet<string> Families,
        bool UniversalOnly,
        bool RawPrivateTextCopied,
        bool TrainingRun,
        bool EvalRun,
        bool CheckpointMutated,
        bool SemanticQualityClaimed);

    private sealed record SourceDisjointFixtureRow(
        string FixtureId,
        string Machine,
        string SplitRole,
        string HeldoutSourceFamily,
        bool UniversalOnly,
        bool RawPrivateTextCopied,
        bool TrainingAllowed,
        bool EvalExecutionAllowed,
        bool NotExecutedYet);
}
