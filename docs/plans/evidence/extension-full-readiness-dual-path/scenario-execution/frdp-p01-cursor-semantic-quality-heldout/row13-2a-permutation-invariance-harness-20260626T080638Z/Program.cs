using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using SmartSkillsDomain;

var startedAtUtc = DateTimeOffset.UtcNow;
var outputDir = Directory.GetCurrentDirectory();
var checkpointPath = @"C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-1b-evidence-only-adapter-train-export-20260626T073726Z\staged-candidate\skill-selection-checkpoint.json";
var manifestPath = @"C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-1b-evidence-only-adapter-train-export-20260626T073726Z\staged-candidate\skill-selection-manifest.json";
var dpoRowsPath = @"C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-1a-universal-only-dpo-rows-20260626T070919Z\row13-1a-universal-only-dpo-rows.jsonl";
var scenarioResultsPath = @"C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-1c-heldout-rerun-staged-candidate-20260626T074556Z\row13-1c-scenario-results.jsonl";

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

try
{
    var checkpointShaBefore = Sha256File(checkpointPath);
    var manifestShaBefore = Sha256File(manifestPath);
    var loadResult = await SkillSelectionMachine.LoadCheckpointAsync(checkpointPath);
    if (!loadResult.IsSuccess || loadResult.Value is null)
        return await WriteNeedsFixAsync(
            outputDir,
            startedAtUtc,
            checkpointShaBefore,
            manifestShaBefore,
            loadResult.ErrorCode ?? "CHECKPOINT_LOAD_FAILED",
            loadResult.ErrorMessage ?? "SkillSelectionMachine.LoadCheckpointAsync returned no machine.");

    var scenarioRows = ReadJsonl(scenarioResultsPath)
        .Select(ParseScenarioRow)
        .ToArray();
    var dpoRows = ReadJsonl(dpoRowsPath)
        .Select(ParseDpoRow)
        .Where(row => row.Usable)
        .ToArray();

    var exactHeldoutPromptTextAvailable = scenarioRows.Any(row => !string.IsNullOrWhiteSpace(row.PromptText));
    var requiredTargets = scenarioRows
        .Select(row => row.ExpectedSkillIds.SingleOrDefault() ?? "")
        .Where(skillId => !string.IsNullOrWhiteSpace(skillId))
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToArray();
    if (requiredTargets.Length != 4)
        throw new InvalidOperationException($"Expected four target skills from Row13.1c; found {requiredTargets.Length}: {string.Join(",", requiredTargets)}");

    var caseResults = new List<CaseResult>();
    foreach (var scenarioRow in scenarioRows.OrderBy(row => row.RowId, StringComparer.Ordinal))
    {
        var expectedSkill = scenarioRow.ExpectedSkillIds.SingleOrDefault()
            ?? throw new InvalidOperationException($"Scenario row {scenarioRow.RowId} does not contain exactly one expected skill.");
        var sourceRow = dpoRows
            .Where(row => string.Equals(row.TargetSkill, expectedSkill, StringComparison.Ordinal)
                || string.Equals(row.SkillId, expectedSkill, StringComparison.Ordinal))
            .OrderBy(row => row.RowId, StringComparer.Ordinal)
            .FirstOrDefault()
            ?? throw new InvalidOperationException($"No usable Row13.1a proxy prompt row found for target skill {expectedSkill}.");

        var prompt = !string.IsNullOrWhiteSpace(scenarioRow.PromptText)
            ? scenarioRow.PromptText!
            : sourceRow.Prompt;
        if (string.IsNullOrWhiteSpace(prompt))
            throw new InvalidOperationException($"No prompt text available for target skill {expectedSkill}.");

        var permutations = new[]
        {
            new PermutationInput("original", scenarioRow.AvailableSkillIds),
            new PermutationInput("reversed", scenarioRow.AvailableSkillIds.Reverse().ToArray()),
            new PermutationInput("rotated", RotateLeft(scenarioRow.AvailableSkillIds))
        };

        var permutationResults = new List<PermutationResult>();
        foreach (var permutation in permutations)
        {
            var request = new SkillSelectionRequest(
                Prompt: prompt,
                ProjectRoot: "redacted-project-root",
                AvailableSkillIds: permutation.AvailableSkillIds,
                HostFacts: new Dictionary<string, string>(StringComparer.Ordinal),
                UserPromptSourceEntryId: scenarioRow.PromptHash is { Length: > 0 }
                    ? $"row13-2a-proxy-{sourceRow.RowId}-heldout-hash-{scenarioRow.PromptHash}"
                    : $"row13-2a-proxy-{sourceRow.RowId}");
            var decisionResult = loadResult.Value.Machine.DecideFromTrainedState(request);
            if (!decisionResult.IsSuccess || decisionResult.Value is null)
                throw new InvalidOperationException($"Decision failed for {scenarioRow.RowId}/{permutation.Label}: {decisionResult.ErrorCode} {decisionResult.ErrorMessage}");

            var decision = decisionResult.Value;
            var selectedIndexes = decision.SelectedSkillIds
                .Select(skillId => IndexOf(permutation.AvailableSkillIds, skillId))
                .ToArray();
            permutationResults.Add(new PermutationResult(
                RowId: scenarioRow.RowId,
                TargetSkill: expectedSkill,
                Permutation: permutation.Label,
                AvailableSkillIds: permutation.AvailableSkillIds,
                ExpectedSkillId: expectedSkill,
                SelectedSkillIds: decision.SelectedSkillIds.ToArray(),
                RejectedSkillIds: decision.RejectedSkillIds.ToArray(),
                SelectedIndex: selectedIndexes.Length == 1 ? selectedIndexes[0] : null,
                SelectedIndexes: selectedIndexes,
                ExpectedSelected: decision.SelectedSkillIds.Contains(expectedSkill, StringComparer.Ordinal),
                ExactSelectedMatch: SetEquals(decision.SelectedSkillIds, new[] { expectedSkill }),
                SelectedFirstCandidate: decision.SelectedSkillIds.Contains(permutation.AvailableSkillIds.FirstOrDefault() ?? "", StringComparer.Ordinal),
                Confidence: decision.Confidence,
                RequiresSecondPass: decision.RequiresSecondPass,
                EvidenceFeatures: decision.EvidenceFeatures.ToArray(),
                DecisionText: decision.DecisionText));
        }

        var selectedSetVariants = permutationResults
            .Select(result => CanonicalSet(result.SelectedSkillIds))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var selectedSetInvariant = selectedSetVariants.Length == 1;
        var expectedSelectedAcrossPermutations = permutationResults.All(result => result.ExpectedSelected);
        var candidatePositionDependenceDetected = !selectedSetInvariant
            || permutationResults.Any(result => !result.ExpectedSelected && result.SelectedFirstCandidate);

        caseResults.Add(new CaseResult(
            RowId: scenarioRow.RowId,
            TargetSkill: expectedSkill,
            ExpectedSkillId: expectedSkill,
            PromptSourceRowId: sourceRow.RowId,
            PromptSourceKind: exactHeldoutPromptTextAvailable
                ? "row13.1c_exact_prompt"
                : "row13.1a_source_disjoint_training_proxy",
            ExactHeldoutPromptTextAvailable: exactHeldoutPromptTextAvailable,
            SourceDisjointProxyPromptUsed: !exactHeldoutPromptTextAvailable,
            ScenarioAvailableSkillIds: scenarioRow.AvailableSkillIds,
            ProxyPromptAvailableSkillIds: sourceRow.AvailableSkillIds,
            ProxyAndScenarioAvailableSetsMatch: SetEquals(scenarioRow.AvailableSkillIds, sourceRow.AvailableSkillIds),
            Permutations: permutationResults,
            ExpectedSelectedAcrossPermutations: expectedSelectedAcrossPermutations,
            SelectedSetInvariantAcrossPermutations: selectedSetInvariant,
            SelectedSetVariants: selectedSetVariants,
            CandidatePositionDependenceDetected: candidatePositionDependenceDetected));
    }

    var allPermutationResults = caseResults.SelectMany(result => result.Permutations).ToArray();
    var checkpointShaAfter = Sha256File(checkpointPath);
    var manifestShaAfter = Sha256File(manifestPath);
    var noCheckpointMutation = checkpointShaBefore == checkpointShaAfter && manifestShaBefore == manifestShaAfter;
    var overallPermutationInvariant = caseResults.All(result => result.SelectedSetInvariantAcrossPermutations);
    var candidatePositionDependenceDetectedOverall = caseResults.Any(result => result.CandidatePositionDependenceDetected);
    var passCount = allPermutationResults.Count(result => result.ExpectedSelected);
    var exactPassCount = allPermutationResults.Count(result => result.ExactSelectedMatch);
    var status = candidatePositionDependenceDetectedOverall
        ? "ROW13_2A_POSITION_DEPENDENCE_FOUND"
        : passCount == allPermutationResults.Length && noCheckpointMutation
            ? "ROW13_2A_PERMUTATION_PASS"
            : "ROW13_2A_NEEDS_FIX";
    var completedAtUtc = DateTimeOffset.UtcNow;
    var report = new HarnessReport(
        SchemaVersion: "row13.2a.permutation_invariance_report.v1",
        Status: status,
        StartedAtUtc: startedAtUtc,
        CompletedAtUtc: completedAtUtc,
        RuntimeInvocationMethod: "SkillSelectionMachine.LoadCheckpointAsync + SkillSelectionMachine.DecideFromTrainedState",
        Inputs: new InputPaths(checkpointPath, manifestPath, dpoRowsPath, scenarioResultsPath),
        ExactHeldoutPromptTextAvailable: exactHeldoutPromptTextAvailable,
        PromptSourceLimitation: exactHeldoutPromptTextAvailable
            ? "Exact prompt text was present in Row13.1c scenario rows."
            : "Exact held-out prompt text was unavailable in Row13.1c; Row13.2a used one source-disjoint Row13.1a training prompt variant per target skill as a proxy permutation test only.",
        ProxyPermutationTestOnly: !exactHeldoutPromptTextAvailable,
        CandidatePositionProbePhrase: "candidate-position dependence probe over original, reversed, and rotated AvailableSkillIds order",
        CheckpointSha256Before: checkpointShaBefore,
        CheckpointSha256After: checkpointShaAfter,
        ManifestSha256Before: manifestShaBefore,
        ManifestSha256After: manifestShaAfter,
        NoCheckpointMutation: noCheckpointMutation,
        TrainingRun: false,
        CheckpointMutated: !noCheckpointMutation,
        PromotionPerformed: false,
        LocatorUpdatePerformed: false,
        PermutationInvariant: overallPermutationInvariant,
        CandidatePositionDependenceDetected: candidatePositionDependenceDetectedOverall,
        Metrics: new HarnessMetrics(
            Cases: caseResults.Count,
            Permutations: allPermutationResults.Length,
            PassCount: passCount,
            ExactPassCount: exactPassCount,
            InvariantCount: caseResults.Count(result => result.SelectedSetInvariantAcrossPermutations),
            ExpectedSelectedAcrossAllPermutationCount: caseResults.Count(result => result.ExpectedSelectedAcrossPermutations)),
        ReadBackPhrases: new[]
        {
            "original",
            "reversed",
            "rotated",
            "candidate-position",
            "permutationInvariant",
            "no checkpoint mutation"
        },
        Row13_2CanProceed: status == "ROW13_2A_PERMUTATION_PASS",
        EvidenceBoundary: "Proxy permutation/invariance harness only; no product readiness, token savings, full semantic quality, extension readiness, training, mutation, promotion, or locator update claimed.",
        Cases: caseResults);

    await WriteReportsAsync(outputDir, report, allPermutationResults, jsonOptions);
    Console.WriteLine(JsonSerializer.Serialize(report, jsonOptions));
    return status == "ROW13_2A_PERMUTATION_PASS" ? 0 : 2;
}
catch (Exception ex)
{
    var checkpointShaBefore = File.Exists(checkpointPath) ? Sha256File(checkpointPath) : "";
    var manifestShaBefore = File.Exists(manifestPath) ? Sha256File(manifestPath) : "";
    return await WriteNeedsFixAsync(
        outputDir,
        startedAtUtc,
        checkpointShaBefore,
        manifestShaBefore,
        ex.GetType().Name,
        ex.Message);
}

async Task<int> WriteNeedsFixAsync(
    string outputDirectory,
    DateTimeOffset started,
    string checkpointShaBefore,
    string manifestShaBefore,
    string reasonCode,
    string reason)
{
    var completedAtUtc = DateTimeOffset.UtcNow;
    var checkpointShaAfter = File.Exists(checkpointPath) ? Sha256File(checkpointPath) : "";
    var manifestShaAfter = File.Exists(manifestPath) ? Sha256File(manifestPath) : "";
    var report = new HarnessReport(
        SchemaVersion: "row13.2a.permutation_invariance_report.v1",
        Status: "ROW13_2A_NEEDS_FIX",
        StartedAtUtc: started,
        CompletedAtUtc: completedAtUtc,
        RuntimeInvocationMethod: "SkillSelectionMachine.LoadCheckpointAsync + SkillSelectionMachine.DecideFromTrainedState",
        Inputs: new InputPaths(checkpointPath, manifestPath, dpoRowsPath, scenarioResultsPath),
        ExactHeldoutPromptTextAvailable: false,
        PromptSourceLimitation: $"NEEDS_FIX: {reasonCode}: {reason}",
        ProxyPermutationTestOnly: true,
        CandidatePositionProbePhrase: "candidate-position dependence probe over original, reversed, and rotated AvailableSkillIds order",
        CheckpointSha256Before: checkpointShaBefore,
        CheckpointSha256After: checkpointShaAfter,
        ManifestSha256Before: manifestShaBefore,
        ManifestSha256After: manifestShaAfter,
        NoCheckpointMutation: checkpointShaBefore == checkpointShaAfter && manifestShaBefore == manifestShaAfter,
        TrainingRun: false,
        CheckpointMutated: checkpointShaBefore != checkpointShaAfter || manifestShaBefore != manifestShaAfter,
        PromotionPerformed: false,
        LocatorUpdatePerformed: false,
        PermutationInvariant: false,
        CandidatePositionDependenceDetected: false,
        Metrics: new HarnessMetrics(0, 0, 0, 0, 0, 0),
        ReadBackPhrases: new[]
        {
            "original",
            "reversed",
            "rotated",
            "candidate-position",
            "permutationInvariant",
            "no checkpoint mutation"
        },
        Row13_2CanProceed: false,
        EvidenceBoundary: "NEEDS_FIX before Row13.2 can proceed; no training, mutation, promotion, or locator update performed.",
        Cases: []);
    await WriteReportsAsync(outputDirectory, report, [], jsonOptions);
    Console.Error.WriteLine(JsonSerializer.Serialize(report, jsonOptions));
    return 3;
}

static async Task WriteReportsAsync(
    string outputDirectory,
    HarnessReport report,
    IReadOnlyList<PermutationResult> permutationResults,
    JsonSerializerOptions jsonOptions)
{
    var jsonlOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    Directory.CreateDirectory(outputDirectory);
    await File.WriteAllTextAsync(
        Path.Combine(outputDirectory, "row13-2a-permutation-report.json"),
        JsonSerializer.Serialize(report, jsonOptions) + Environment.NewLine,
        Encoding.UTF8);
    await File.WriteAllLinesAsync(
        Path.Combine(outputDirectory, "row13-2a-permutation-results.jsonl"),
        permutationResults.Select(result => JsonSerializer.Serialize(result, jsonlOptions)),
        Encoding.UTF8);
    await File.WriteAllTextAsync(
        Path.Combine(outputDirectory, "row13-2a-permutation-report.md"),
        BuildMarkdown(report),
        Encoding.UTF8);
}

static string BuildMarkdown(HarnessReport report)
{
    var builder = new StringBuilder();
    builder.AppendLine("# Row13.2a Permutation/Invariance Harness");
    builder.AppendLine();
    builder.AppendLine($"Status: `{report.Status}`");
    builder.AppendLine($"Runtime: `{report.RuntimeInvocationMethod}`");
    builder.AppendLine($"permutationInvariant: `{report.PermutationInvariant.ToString().ToLowerInvariant()}`");
    builder.AppendLine($"candidate-position dependence detected: `{report.CandidatePositionDependenceDetected.ToString().ToLowerInvariant()}`");
    builder.AppendLine($"no checkpoint mutation: `{report.NoCheckpointMutation.ToString().ToLowerInvariant()}`");
    builder.AppendLine($"Row13.2 can proceed: `{report.Row13_2CanProceed.ToString().ToLowerInvariant()}`");
    builder.AppendLine();
    builder.AppendLine(report.PromptSourceLimitation);
    builder.AppendLine();
    builder.AppendLine("| row | expected | original | reversed | rotated | invariant | expected across permutations |");
    builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
    foreach (var item in report.Cases)
    {
        string SelectedFor(string label)
            => string.Join(",", item.Permutations.Single(result => result.Permutation == label).SelectedSkillIds);
        builder.AppendLine($"| {item.RowId} | {item.ExpectedSkillId} | {SelectedFor("original")} | {SelectedFor("reversed")} | {SelectedFor("rotated")} | {item.SelectedSetInvariantAcrossPermutations} | {item.ExpectedSelectedAcrossPermutations} |");
    }

    builder.AppendLine();
    builder.AppendLine("Read-back phrases: original, reversed, rotated, candidate-position, permutationInvariant, no checkpoint mutation.");
    builder.AppendLine(report.EvidenceBoundary);
    return builder.ToString();
}

static IReadOnlyList<JsonElement> ReadJsonl(string path)
{
    var rows = new List<JsonElement>();
    foreach (var line in File.ReadLines(path, Encoding.UTF8))
    {
        if (string.IsNullOrWhiteSpace(line))
            continue;
        using var document = JsonDocument.Parse(line);
        rows.Add(document.RootElement.Clone());
    }

    return rows;
}

static ScenarioRow ParseScenarioRow(JsonElement row)
    => new(
        RowId: GetRequiredString(row, "rowId"),
        PromptHash: GetOptionalString(row, "promptHash") ?? "",
        PromptText: GetOptionalString(row, "Prompt") ?? GetOptionalString(row, "prompt") ?? GetOptionalString(row, "promptText"),
        AvailableSkillIds: GetRequiredStringArray(row, "availableSkillIds"),
        ExpectedSkillIds: GetRequiredStringArray(row, "expectedSkillIds"));

static DpoRow ParseDpoRow(JsonElement row)
    => new(
        RowId: GetRequiredString(row, "rowId"),
        TargetSkill: GetRequiredString(row, "targetSkill"),
        SkillId: GetRequiredString(row, "skillId"),
        Prompt: GetRequiredString(row, "Prompt"),
        AvailableSkillIds: GetRequiredStringArray(row, "availableSkillIds"),
        UniversalOnly: GetOptionalBool(row, "universalOnly") ?? false,
        Usable: GetOptionalBool(row, "usable") ?? false);

static string[] GetRequiredStringArray(JsonElement row, string propertyName)
{
    if (!row.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
        throw new InvalidOperationException($"Missing required array property {propertyName}.");
    return value.EnumerateArray().Select(item => item.GetString() ?? "").Where(item => item.Length > 0).ToArray();
}

static string GetRequiredString(JsonElement row, string propertyName)
    => GetOptionalString(row, propertyName)
        ?? throw new InvalidOperationException($"Missing required string property {propertyName}.");

static string? GetOptionalString(JsonElement row, string propertyName)
    => row.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static bool? GetOptionalBool(JsonElement row, string propertyName)
    => row.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

static string[] RotateLeft(IReadOnlyList<string> values)
    => values.Count <= 1
        ? values.ToArray()
        : values.Skip(1).Concat(values.Take(1)).ToArray();

static bool SetEquals(IEnumerable<string> left, IEnumerable<string> right)
    => new HashSet<string>(left, StringComparer.Ordinal).SetEquals(right);

static int IndexOf(IReadOnlyList<string> values, string value)
{
    for (var index = 0; index < values.Count; index++)
    {
        if (string.Equals(values[index], value, StringComparison.Ordinal))
            return index;
    }

    return -1;
}

static string CanonicalSet(IEnumerable<string> values)
    => string.Join("|", values.Order(StringComparer.Ordinal));

static string Sha256File(string path)
{
    using var stream = File.OpenRead(path);
    var hash = SHA256.HashData(stream);
    return Convert.ToHexString(hash).ToLowerInvariant();
}

internal sealed record InputPaths(
    string CheckpointPath,
    string ManifestPath,
    string DpoRowsPath,
    string ScenarioResultsPath);

internal sealed record ScenarioRow(
    string RowId,
    string PromptHash,
    string? PromptText,
    IReadOnlyList<string> AvailableSkillIds,
    IReadOnlyList<string> ExpectedSkillIds);

internal sealed record DpoRow(
    string RowId,
    string TargetSkill,
    string SkillId,
    string Prompt,
    IReadOnlyList<string> AvailableSkillIds,
    bool UniversalOnly,
    bool Usable);

internal sealed record PermutationInput(
    string Label,
    IReadOnlyList<string> AvailableSkillIds);

internal sealed record PermutationResult(
    string RowId,
    string TargetSkill,
    string Permutation,
    IReadOnlyList<string> AvailableSkillIds,
    string ExpectedSkillId,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    int? SelectedIndex,
    IReadOnlyList<int> SelectedIndexes,
    bool ExpectedSelected,
    bool ExactSelectedMatch,
    bool SelectedFirstCandidate,
    double Confidence,
    bool RequiresSecondPass,
    IReadOnlyList<string> EvidenceFeatures,
    string DecisionText);

internal sealed record CaseResult(
    string RowId,
    string TargetSkill,
    string ExpectedSkillId,
    string PromptSourceRowId,
    string PromptSourceKind,
    bool ExactHeldoutPromptTextAvailable,
    bool SourceDisjointProxyPromptUsed,
    IReadOnlyList<string> ScenarioAvailableSkillIds,
    IReadOnlyList<string> ProxyPromptAvailableSkillIds,
    bool ProxyAndScenarioAvailableSetsMatch,
    IReadOnlyList<PermutationResult> Permutations,
    bool ExpectedSelectedAcrossPermutations,
    bool SelectedSetInvariantAcrossPermutations,
    IReadOnlyList<string> SelectedSetVariants,
    bool CandidatePositionDependenceDetected);

internal sealed record HarnessMetrics(
    int Cases,
    int Permutations,
    int PassCount,
    int ExactPassCount,
    int InvariantCount,
    int ExpectedSelectedAcrossAllPermutationCount);

internal sealed record HarnessReport(
    string SchemaVersion,
    string Status,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    string RuntimeInvocationMethod,
    InputPaths Inputs,
    bool ExactHeldoutPromptTextAvailable,
    string PromptSourceLimitation,
    bool ProxyPermutationTestOnly,
    string CandidatePositionProbePhrase,
    string CheckpointSha256Before,
    string CheckpointSha256After,
    string ManifestSha256Before,
    string ManifestSha256After,
    bool NoCheckpointMutation,
    bool TrainingRun,
    bool CheckpointMutated,
    bool PromotionPerformed,
    bool LocatorUpdatePerformed,
    bool PermutationInvariant,
    bool CandidatePositionDependenceDetected,
    HarnessMetrics Metrics,
    IReadOnlyList<string> ReadBackPhrases,
    bool Row13_2CanProceed,
    string EvidenceBoundary,
    IReadOnlyList<CaseResult> Cases);
