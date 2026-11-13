using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartSkillsDomain;

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    Converters = { new JsonStringEnumConverter() }
};
var snakeCaseOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
};
var repoRoot = FindRepoRoot();
var outputPath = Path.Combine(
    repoRoot,
    "artifacts",
    "smart-skills-quality-uplift",
    "q5",
    "skill-selection-candidate-position-executable-probe-report.json");
var heldoutPath = Path.Combine(
    repoRoot,
    "training-data",
    "smart-skills",
    "semantic-quality",
    "skill-selection-heldout-v1",
    "skill-selection-heldout.jsonl");
var checkpointPath = Path.Combine(
    repoRoot,
    "models",
    "skill-selection",
    "skill-selection-v1",
    "skill-selection-v1-checkpoint.json");
var manifestPath = Path.Combine(
    repoRoot,
    "models",
    "skill-selection",
    "skill-selection-v1",
    "manifest.json");
var modelIndexPath = Path.Combine(repoRoot, "models", "index.json");

var report = RunProbe(repoRoot, heldoutPath, checkpointPath, manifestPath, modelIndexPath);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(report, jsonOptions) + Environment.NewLine, Encoding.UTF8);
Console.WriteLine(JsonSerializer.Serialize(report, jsonOptions));
return report.Passed ? 0 : 3;

ProbeReport RunProbe(
    string repoRoot,
    string heldoutPath,
    string checkpointPath,
    string manifestPath,
    string modelIndexPath)
{
    var started = DateTimeOffset.UtcNow;
    var rows = ReadHeldoutRows(heldoutPath);
    var checkpointShaBefore = Sha256File(checkpointPath);
    var manifestShaBefore = Sha256File(manifestPath);
    var modelIndexShaBefore = Sha256File(modelIndexPath);
    var load = LoadCheckpointThroughProductionAdapter(checkpointPath);
    if (load is null)
    {
        return ProbeReport.Failure(
            ReadGitBranch(repoRoot),
            started,
            heldoutPath,
            checkpointPath,
            manifestPath,
            modelIndexPath,
            checkpointShaBefore,
            manifestShaBefore,
            modelIndexShaBefore,
            "SKILL_SELECTION_COMMON_CHECKPOINT_READ_FAILED",
            "SkillSelection checkpoint could not be loaded through the production snake_case adapter.");
    }

    var checkpoint = load.Value.Checkpoint;
    var bestCheckpointPosition = checkpoint.CandidatePositionWeights
        .OrderByDescending(item => item.Value)
        .ThenBy(item => item.Key)
        .Select(item => item.Key)
        .FirstOrDefault();
    var permutationResults = new List<PermutationProbeResult>();
    foreach (var row in rows)
    {
        for (var index = 0; index < row.CandidateOrderPermutations.Count; index++)
        {
            var permutation = row.CandidateOrderPermutations[index];
            var request = new SkillSelectionRequest(
                Prompt: row.RedactedTaskText,
                ProjectRoot: "",
                AvailableSkillIds: permutation,
                HostFacts: row.OrchestrationContext.ToDictionary(
                    item => item.Key,
                    item => JsonElementToStableString(item.Value),
                    StringComparer.Ordinal),
                UserPromptSourceEntryId: row.TaskPromptRef);
            var decisionResult = load.Value.Machine.DecideFromTrainedState(request);
            var decision = decisionResult.Value;
            var expectedRejectedIds = row.ExpectedRejectedSkills.Select(item => item.SkillId).ToArray();
            var actualSelected = decision?.SelectedSkillIds ?? [];
            var actualRejected = decision?.RejectedSkillIds ?? [];
            var firstBaselineSelected = permutation.Count == 0 ? Array.Empty<string>() : [permutation[0]];
            var checkpointBaselineSelected = bestCheckpointPosition >= 0 && bestCheckpointPosition < permutation.Count
                ? [permutation[bestCheckpointPosition]]
                : Array.Empty<string>();
            permutationResults.Add(new PermutationProbeResult(
                RowId: row.RowId,
                PermutationIndex: index,
                CandidateOrder: permutation,
                ExpectedSelectedSkillIds: row.ExpectedSelectedSkillIds,
                ExpectedRejectedSkillIds: expectedRejectedIds,
                ExpectedRejectedReasons: row.ExpectedRejectedSkills
                    .ToDictionary(item => item.SkillId, item => item.Reason, StringComparer.Ordinal),
                ActualSelectedSkillIds: actualSelected,
                ActualRejectedSkillIds: actualRejected,
                ActualEvidenceFeatures: decision?.EvidenceFeatures ?? [],
                Confidence: decision?.Confidence ?? 0.0,
                RequiresSecondPass: decision?.RequiresSecondPass ?? true,
                DecisionText: decision?.DecisionText ?? "",
                ProductionDecisionSucceeded: decisionResult.IsSuccess && decision is not null,
                ExactSelectedMatch: SetEquals(actualSelected, row.ExpectedSelectedSkillIds),
                ExpectedRejectedPresent: expectedRejectedIds.All(expected => actualRejected.Contains(expected, StringComparer.Ordinal)),
                ActualSelectedIsSubsetOfExpected: actualSelected.All(selected => row.ExpectedSelectedSkillIds.Contains(selected, StringComparer.Ordinal)),
                ActualSelectedCount: actualSelected.Count,
                CandidateCount: permutation.Count,
                OverSelectedAll: actualSelected.Count == permutation.Count && permutation.Count > 0,
                UsedCandidatePositionScaffoldEvidence: decision?.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal) ?? false,
                FirstPositionBaselineSelectedSkillIds: firstBaselineSelected,
                FirstPositionBaselineExactMatch: SetEquals(firstBaselineSelected, row.ExpectedSelectedSkillIds),
                CheckpointPositionBaselineSelectedSkillIds: checkpointBaselineSelected,
                CheckpointPositionBaselineExactMatch: SetEquals(checkpointBaselineSelected, row.ExpectedSelectedSkillIds)));
        }
    }

    var stability = permutationResults
        .GroupBy(item => item.RowId, StringComparer.Ordinal)
        .Select(group =>
        {
            var selectedKeys = group
                .Select(item => CanonicalSet(item.ActualSelectedSkillIds))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var rejectedKeys = group
                .Select(item => CanonicalSet(item.ActualRejectedSkillIds))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var evidenceKeys = group
                .Select(item => CanonicalSemanticEvidence(item.ActualEvidenceFeatures))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return new RowStabilityResult(
                RowId: group.Key,
                StableSelectedSet: selectedKeys.Length == 1,
                StableRejectedSet: rejectedKeys.Length == 1,
                StableSemanticEvidence: evidenceKeys.Length == 1,
                SelectedSetVariants: selectedKeys,
                RejectedSetVariants: rejectedKeys,
                SemanticEvidenceVariants: evidenceKeys);
        })
        .ToArray();
    var checkpointShaAfter = Sha256File(checkpointPath);
    var manifestShaAfter = Sha256File(manifestPath);
    var modelIndexShaAfter = Sha256File(modelIndexPath);
    var exactMatches = permutationResults.Count(item => item.ExactSelectedMatch && item.ExpectedRejectedPresent);
    var subsetMatches = permutationResults.Count(item => item.ActualSelectedIsSubsetOfExpected && item.ExpectedRejectedPresent);
    var firstBaselineMatches = permutationResults.Count(item => item.FirstPositionBaselineExactMatch);
    var checkpointBaselineMatches = permutationResults.Count(item => item.CheckpointPositionBaselineExactMatch);
    var firstBaselineFailsOrDegrades = firstBaselineMatches == 0 || firstBaselineMatches < exactMatches;
    var checkpointBaselineFailsOrDegrades = checkpointBaselineMatches == 0 || checkpointBaselineMatches < exactMatches;
    var allRowsStable = stability.All(item => item.StableSelectedSet && item.StableRejectedSet && item.StableSemanticEvidence);
    var noOverSelectAll = permutationResults.All(item => !item.OverSelectedAll);
    var noCandidatePositionFinalAuthority = permutationResults.All(item => !item.UsedCandidatePositionScaffoldEvidence);
    var exactModelMatchesAll = exactMatches == permutationResults.Count;
    var expectedRejectedPresentAll = permutationResults.All(item => item.ExpectedRejectedPresent);
    var checkpointUnchanged = string.Equals(checkpointShaBefore, checkpointShaAfter, StringComparison.Ordinal)
                              && string.Equals(manifestShaBefore, manifestShaAfter, StringComparison.Ordinal)
                              && string.Equals(modelIndexShaBefore, modelIndexShaAfter, StringComparison.Ordinal);
    var passed = rows.Count == 12
                 && permutationResults.Count == 24
                 && exactModelMatchesAll
                 && expectedRejectedPresentAll
                 && allRowsStable
                 && noOverSelectAll
                 && noCandidatePositionFinalAuthority
                 && firstBaselineFailsOrDegrades
                 && checkpointBaselineFailsOrDegrades
                 && checkpointUnchanged;
    var reasonCodes = new List<string>();
    if (rows.Count != 12)
        reasonCodes.Add("Q5_02_HELDOUT_ROW_COUNT_UNEXPECTED");
    if (permutationResults.Count != 24)
        reasonCodes.Add("Q5_02_PERMUTATION_COUNT_UNEXPECTED");
    if (!exactModelMatchesAll)
        reasonCodes.Add("MODEL_SELECTED_SET_DOES_NOT_MATCH_EXPECTED_HELDOUT_SET");
    if (!expectedRejectedPresentAll)
        reasonCodes.Add("MODEL_REJECTED_SET_MISSING_EXPECTED_REJECTIONS");
    if (!allRowsStable)
        reasonCodes.Add("MODEL_DECISION_UNSTABLE_ACROSS_PERMUTATIONS");
    if (!noOverSelectAll)
        reasonCodes.Add("MODEL_OVER_SELECTED_ALL_CANDIDATES");
    if (!noCandidatePositionFinalAuthority)
        reasonCodes.Add("MODEL_USED_CANDIDATE_POSITION_SCAFFOLD_EVIDENCE");
    if (!firstBaselineFailsOrDegrades)
        reasonCodes.Add("FIRST_POSITION_BASELINE_DID_NOT_FAIL_OR_DEGRADE");
    if (!checkpointBaselineFailsOrDegrades)
        reasonCodes.Add("CHECKPOINT_POSITION_BASELINE_DID_NOT_FAIL_OR_DEGRADE");
    if (!checkpointUnchanged)
        reasonCodes.Add("MODEL_OR_INDEX_FILE_MUTATED_DURING_PROBE");

    return new ProbeReport(
        SchemaVersion: 1,
        ReportId: "q5-04-skill-selection-candidate-position-executable-probe",
        WorkOrderId: "Q5-04_CANDIDATE_POSITION_EXECUTABLE_PROBE_REPAIR",
        Branch: ReadGitBranch(repoRoot),
        CreatedAtUtc: started,
        Status: passed ? "PASS" : "FAIL_MODEL_BEHAVIOR",
        Passed: passed,
        SemanticQualityClaimed: false,
        TrainingStarted: false,
        ModelFilesMutated: !checkpointUnchanged,
        ExternalAiUsed: false,
        HostRuntimeClaimed: false,
        DashboardApiWorkRun: false,
        ProbeScope: "Report-only executable probe over Q5-02 held-out rows and candidate-order permutations through the production snake_case common-asset load adapter, SkillSelectionMachine.LoadCheckpoint, and DecideFromTrainedState.",
        EvidenceRefs: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["heldout_jsonl"] = ToRepoRelative(repoRoot, heldoutPath),
            ["checkpoint"] = ToRepoRelative(repoRoot, checkpointPath),
            ["manifest"] = ToRepoRelative(repoRoot, manifestPath),
            ["model_index"] = ToRepoRelative(repoRoot, modelIndexPath),
            ["production_machine"] = "43.SmartSkillsDomain/SkillSelectionMachine.cs",
            ["production_contracts"] = "43.SmartSkillsDomain/SkillSelectionContracts.cs"
        },
        CheckpointReview: new CheckpointProbeReview(
            CheckpointLoaded: true,
            CheckpointSha256Before: checkpointShaBefore,
            CheckpointSha256After: checkpointShaAfter,
            ManifestSha256Before: manifestShaBefore,
            ManifestSha256After: manifestShaAfter,
            ModelIndexSha256Before: modelIndexShaBefore,
            ModelIndexSha256After: modelIndexShaAfter,
            CheckpointUnchanged: checkpointUnchanged,
            TrainedPairCount: checkpoint.TrainedPairIds.Count,
            CandidatePositionWeights: checkpoint.CandidatePositionWeights,
            BestCheckpointCandidatePosition: bestCheckpointPosition,
            RouteMode: checkpoint.RouteMode),
        Summary: new ProbeSummary(
            HeldoutRows: rows.Count,
            TotalPermutations: permutationResults.Count,
            ExactModelMatches: exactMatches,
            ExpectedRejectedPresentCount: permutationResults.Count(item => item.ExpectedRejectedPresent),
            ActualSelectedSubsetOfExpectedCount: subsetMatches,
            FirstPositionBaselineExactMatches: firstBaselineMatches,
            CheckpointPositionBaselineExactMatches: checkpointBaselineMatches,
            FirstPositionBaselineFailsAllPermutations: firstBaselineMatches == 0,
            CheckpointPositionBaselineFailsAllPermutations: checkpointBaselineMatches == 0,
            PermutationStableRows: stability.Count(item => item.StableSelectedSet && item.StableRejectedSet && item.StableSemanticEvidence),
            NoOverSelectAll: noOverSelectAll,
            NoCandidatePositionScaffoldEvidence: noCandidatePositionFinalAuthority),
        RowStability: stability,
        FailuresByPermutation: permutationResults
            .Where(item => !(item.ExactSelectedMatch && item.ExpectedRejectedPresent)
                           || item.OverSelectedAll
                           || item.UsedCandidatePositionScaffoldEvidence)
            .ToArray(),
        PermutationResults: permutationResults,
        Gates: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["load_existing_checkpoint"] = "PASS",
            ["production_decision_path_executed"] = permutationResults.All(item => item.ProductionDecisionSucceeded) ? "PASS" : "FAIL",
            ["all_q5_02_rows_and_permutations_executed"] = rows.Count == 12 && permutationResults.Count == 24 ? "PASS" : "FAIL",
            ["model_selected_expected_exact_set"] = exactModelMatchesAll ? "PASS" : "FAIL",
            ["model_rejected_expected_wrong_skills"] = expectedRejectedPresentAll ? "PASS" : "FAIL",
            ["permutation_stability"] = allRowsStable ? "PASS" : "FAIL",
            ["first_position_baseline_fails_or_degrades"] = firstBaselineFailsOrDegrades ? "PASS" : "FAIL",
            ["checkpoint_position_baseline_fails_or_degrades"] = checkpointBaselineFailsOrDegrades ? "PASS" : "FAIL",
            ["first_position_baseline_fails_all"] = firstBaselineMatches == 0 ? "PASS" : "FAIL",
            ["checkpoint_position_baseline_fails_all"] = checkpointBaselineMatches == 0 ? "PASS" : "FAIL",
            ["no_over_select_all"] = noOverSelectAll ? "PASS" : "FAIL",
            ["semantic_quality_claimed_false"] = "PASS",
            ["no_training_or_model_mutation"] = checkpointUnchanged ? "PASS" : "FAIL"
        },
        Verdict: new ProbeVerdict(
            Result: passed ? "PASS" : "FAIL",
            ReasonCodes: reasonCodes,
            CanAdvanceToQ505: passed,
            ExactNextAction: passed
                ? "Q5-04 review rerun with executable probe evidence, then Q5-05 wrong-skill near-miss review only if parent review accepts Q5-04."
                : "Q5-04 repair/model behavior repair: executable probe now exists, but SkillSelection output does not satisfy Q5-02 expected selected/rejected sets; do not advance to Q5-05."),
        ForbiddenActionsNotRun:
        [
            "branch checkout",
            "branch creation",
            "staging",
            "commit",
            "push",
            "training",
            "model mutation",
            "external AI/tools",
            "host/plugin/runtime work",
            "dashboard/API work",
            "Q6/Q7/Q8 work",
            "broad dotnet test",
            "broad dotnet build"
        ]);
}

(SkillSelectionMachine Machine, SkillSelectionCheckpoint Checkpoint)? LoadCheckpointThroughProductionAdapter(string checkpointPath)
{
    try
    {
        var json = File.ReadAllText(checkpointPath, Encoding.UTF8);
        var checkpoint = JsonSerializer.Deserialize<SkillSelectionCheckpoint>(json, snakeCaseOptions);
        if (checkpoint is null)
            return null;
        var machine = new SkillSelectionMachine();
        machine.LoadCheckpoint(checkpoint);
        return (machine, checkpoint);
    }
    catch
    {
        return null;
    }
}

IReadOnlyList<HeldoutRow> ReadHeldoutRows(string path)
{
    var rows = new List<HeldoutRow>();
    foreach (var line in File.ReadLines(path, Encoding.UTF8))
    {
        if (string.IsNullOrWhiteSpace(line))
            continue;
        var row = JsonSerializer.Deserialize<HeldoutRow>(line, jsonOptions)
                  ?? throw new InvalidOperationException($"Could not parse held-out row from {path}.");
        rows.Add(row);
    }

    return rows;
}

bool SetEquals(IReadOnlyList<string> left, IReadOnlyList<string> right)
    => left.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(
        right.OrderBy(item => item, StringComparer.Ordinal),
        StringComparer.Ordinal);

string CanonicalSet(IReadOnlyList<string> values)
    => string.Join("|", values.OrderBy(item => item, StringComparer.Ordinal));

string CanonicalSemanticEvidence(IReadOnlyList<string> values)
    => string.Join(
        "|",
        values
            .Where(item => !item.Contains("_score=", StringComparison.Ordinal)
                           && !item.Contains("source_pair_count=", StringComparison.Ordinal))
            .OrderBy(item => item, StringComparer.Ordinal));

string FindRepoRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json")))
            return directory.FullName;
        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Could not locate repository root with SkillSelection checkpoint.");
}

string ReadGitBranch(string repoRoot)
{
    var headPath = Path.Combine(repoRoot, ".git", "HEAD");
    if (!File.Exists(headPath))
        return "UNKNOWN";
    var head = File.ReadAllText(headPath, Encoding.UTF8).Trim();
    return head.StartsWith("ref: refs/heads/", StringComparison.Ordinal)
        ? head["ref: refs/heads/".Length..]
        : head;
}

string Sha256File(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
}

string ToRepoRelative(string repoRoot, string path)
    => Path.GetRelativePath(repoRoot, Path.GetFullPath(path)).Replace('\\', '/');

string JsonElementToStableString(JsonElement element)
    => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString() ?? "",
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Number => element.GetRawText(),
        JsonValueKind.Null => "",
        _ => element.GetRawText()
    };

sealed record HeldoutRow(
    int SchemaVersion,
    string HeldoutSetId,
    string RowId,
    string MachineTarget,
    string SplitRole,
    string TaskPromptRef,
    string RedactedTaskText,
    IReadOnlyDictionary<string, JsonElement> OrchestrationContext,
    IReadOnlyList<string> ExpectedSelectedSkillIds,
    IReadOnlyList<ExpectedRejectedSkill> ExpectedRejectedSkills,
    IReadOnlyList<string> CandidateSkillIds,
    IReadOnlyList<IReadOnlyList<string>> CandidateOrderPermutations,
    IReadOnlyList<string> WrongSkillCategories,
    IReadOnlyList<string> SourceSkillRefs,
    bool TrainingAllowed,
    bool SemanticQualityClaimed,
    bool RawPrivateTextCopied,
    bool TechnicalExecutionRefsExcluded);

sealed record ExpectedRejectedSkill(
    string SkillId,
    string Reason,
    string ErrorCode,
    string WrongSkillCategory);

sealed record ProbeReport(
    int SchemaVersion,
    string ReportId,
    string WorkOrderId,
    string Branch,
    DateTimeOffset CreatedAtUtc,
    string Status,
    bool Passed,
    bool SemanticQualityClaimed,
    bool TrainingStarted,
    bool ModelFilesMutated,
    bool ExternalAiUsed,
    bool HostRuntimeClaimed,
    bool DashboardApiWorkRun,
    string ProbeScope,
    IReadOnlyDictionary<string, string> EvidenceRefs,
    CheckpointProbeReview? CheckpointReview,
    ProbeSummary? Summary,
    IReadOnlyList<RowStabilityResult> RowStability,
    IReadOnlyList<PermutationProbeResult> FailuresByPermutation,
    IReadOnlyList<PermutationProbeResult> PermutationResults,
    IReadOnlyDictionary<string, string> Gates,
    ProbeVerdict Verdict,
    IReadOnlyList<string> ForbiddenActionsNotRun)
{
    public static ProbeReport Failure(
        string branch,
        DateTimeOffset createdAtUtc,
        string heldoutPath,
        string checkpointPath,
        string manifestPath,
        string modelIndexPath,
        string checkpointSha256,
        string manifestSha256,
        string modelIndexSha256,
        string errorCode,
        string errorMessage)
        => new(
            SchemaVersion: 1,
            ReportId: "q5-04-skill-selection-candidate-position-executable-probe",
            WorkOrderId: "Q5-04_CANDIDATE_POSITION_EXECUTABLE_PROBE_REPAIR",
            Branch: branch,
            CreatedAtUtc: createdAtUtc,
            Status: "FAIL_CHECKPOINT_LOAD",
            Passed: false,
            SemanticQualityClaimed: false,
            TrainingStarted: false,
            ModelFilesMutated: false,
            ExternalAiUsed: false,
            HostRuntimeClaimed: false,
            DashboardApiWorkRun: false,
            ProbeScope: "Report-only executable probe failed before decision execution.",
            EvidenceRefs: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["heldout_jsonl"] = heldoutPath,
                ["checkpoint"] = checkpointPath,
                ["manifest"] = manifestPath,
                ["model_index"] = modelIndexPath
            },
            CheckpointReview: new CheckpointProbeReview(
                CheckpointLoaded: false,
                CheckpointSha256Before: checkpointSha256,
                CheckpointSha256After: checkpointSha256,
                ManifestSha256Before: manifestSha256,
                ManifestSha256After: manifestSha256,
                ModelIndexSha256Before: modelIndexSha256,
                ModelIndexSha256After: modelIndexSha256,
                CheckpointUnchanged: true,
                TrainedPairCount: 0,
                CandidatePositionWeights: new Dictionary<int, int>(),
                BestCheckpointCandidatePosition: 0,
                RouteMode: ""),
            Summary: null,
            RowStability: [],
            FailuresByPermutation: [],
            PermutationResults: [],
            Gates: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["load_existing_checkpoint"] = "FAIL",
                ["semantic_quality_claimed_false"] = "PASS"
            },
            Verdict: new ProbeVerdict(
                Result: "FAIL",
                ReasonCodes: [errorCode],
                CanAdvanceToQ505: false,
                ExactNextAction: $"Q5-04 repair: checkpoint executable probe cannot run until load failure is fixed: {errorMessage}"),
            ForbiddenActionsNotRun:
            [
                "branch checkout",
                "branch creation",
                "staging",
                "commit",
                "push",
                "training",
                "model mutation",
                "external AI/tools",
                "host/plugin/runtime work",
                "dashboard/API work",
                "Q6/Q7/Q8 work",
                "broad dotnet test",
                "broad dotnet build"
            ]);
}

sealed record CheckpointProbeReview(
    bool CheckpointLoaded,
    string CheckpointSha256Before,
    string CheckpointSha256After,
    string ManifestSha256Before,
    string ManifestSha256After,
    string ModelIndexSha256Before,
    string ModelIndexSha256After,
    bool CheckpointUnchanged,
    int TrainedPairCount,
    IReadOnlyDictionary<int, int> CandidatePositionWeights,
    int BestCheckpointCandidatePosition,
    string RouteMode);

sealed record ProbeSummary(
    int HeldoutRows,
    int TotalPermutations,
    int ExactModelMatches,
    int ExpectedRejectedPresentCount,
    int ActualSelectedSubsetOfExpectedCount,
    int FirstPositionBaselineExactMatches,
    int CheckpointPositionBaselineExactMatches,
    bool FirstPositionBaselineFailsAllPermutations,
    bool CheckpointPositionBaselineFailsAllPermutations,
    int PermutationStableRows,
    bool NoOverSelectAll,
    bool NoCandidatePositionScaffoldEvidence);

sealed record RowStabilityResult(
    string RowId,
    bool StableSelectedSet,
    bool StableRejectedSet,
    bool StableSemanticEvidence,
    IReadOnlyList<string> SelectedSetVariants,
    IReadOnlyList<string> RejectedSetVariants,
    IReadOnlyList<string> SemanticEvidenceVariants);

sealed record PermutationProbeResult(
    string RowId,
    int PermutationIndex,
    IReadOnlyList<string> CandidateOrder,
    IReadOnlyList<string> ExpectedSelectedSkillIds,
    IReadOnlyList<string> ExpectedRejectedSkillIds,
    IReadOnlyDictionary<string, string> ExpectedRejectedReasons,
    IReadOnlyList<string> ActualSelectedSkillIds,
    IReadOnlyList<string> ActualRejectedSkillIds,
    IReadOnlyList<string> ActualEvidenceFeatures,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText,
    bool ProductionDecisionSucceeded,
    bool ExactSelectedMatch,
    bool ExpectedRejectedPresent,
    bool ActualSelectedIsSubsetOfExpected,
    int ActualSelectedCount,
    int CandidateCount,
    bool OverSelectedAll,
    bool UsedCandidatePositionScaffoldEvidence,
    IReadOnlyList<string> FirstPositionBaselineSelectedSkillIds,
    bool FirstPositionBaselineExactMatch,
    IReadOnlyList<string> CheckpointPositionBaselineSelectedSkillIds,
    bool CheckpointPositionBaselineExactMatch);

sealed record ProbeVerdict(
    string Result,
    IReadOnlyList<string> ReasonCodes,
    bool CanAdvanceToQ505,
    string ExactNextAction);
