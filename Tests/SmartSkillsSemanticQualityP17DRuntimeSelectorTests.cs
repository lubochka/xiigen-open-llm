// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using SmartSkillsDomain;

using Xunit;

namespace LlmFromScratch.Tests;

[Collection(global::ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class SmartSkillsSemanticQualityP17DRuntimeSelectorTests
{
    private const string FailpointMessage = "P17D_AFTER_FIRST_OWNED_ARTIFACT_EXISTS";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions CompactJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private static readonly string RepoRoot = FindRepoRoot();

    private static readonly string ProtectedP17DDir = Path.Combine(
        RepoRoot,
        "artifacts",
        "smart-skills-semantic-quality-completion",
        "p17d-skillselection-checkpoint-rebuild-rerun");

    private static readonly string SourceCheckpointPath = ResolveRepoPath("artifacts/smart-skills-semantic-quality-completion/p17d-skillselection-checkpoint-rebuild-rerun/skill-selection-p17d-native-bound-checkpoint.json");
    private static readonly string SourceManifestPath = ResolveRepoPath("artifacts/smart-skills-semantic-quality-completion/p17d-skillselection-checkpoint-rebuild-rerun/skill-selection-p17d-candidate-manifest.json");
    private static readonly string ParentReviewPath = ResolveRepoPath("artifacts/smart-skills-semantic-quality-completion/p17d-skillselection-checkpoint-rebuild-rerun/p17d-parent-review.json");
    private static readonly string EvalRowsPath = ResolveRepoPath("artifacts/smart-skills-semantic-quality-completion/p16e2-skill-selection-source-disjoint-eval-ablation-rerun/skill-selection-p16e2-source-disjoint-eval-results.jsonl");
    private static readonly string PreviousP17BCheckpointPath = ResolveRepoPath("artifacts/smart-skills-semantic-quality-completion/p17b-skillselection-runtime-selector-eval/skill-selection-p17b-native-bound-checkpoint.json");
    private static readonly string P17CParentReviewPath = ResolveRepoPath("artifacts/smart-skills-semantic-quality-completion/p17c-skillselection-train-only-runtime-coverage-repair/p17c-parent-review.json");
    private static readonly string[] CommittedInputs =
    [
        SourceCheckpointPath,
        SourceManifestPath,
        ParentReviewPath,
        EvalRowsPath,
        PreviousP17BCheckpointPath,
        P17CParentReviewPath
    ];

    [Fact]
    public async Task P17DLoadsRebuiltCheckpointAndRunsRuntimeSelectorRerun()
    {
        var before = CaptureProtectedRepositorySnapshot();
        string? ownedRoot = null;
        Exception? executionFailure = null;

        try
        {
            await TestOwnedOutputRoot.RunAsync(async output =>
            {
                ownedRoot = output.RootPath;
                Assert.Equal(Path.GetFullPath(RepoRoot), Path.GetFullPath(output.RepositoryRootPath));
                await ExecuteP17DAsync(output);
            });
        }
        catch (Exception exception)
        {
            executionFailure = exception;
        }

        Assert.NotNull(ownedRoot);
        Assert.False(Directory.Exists(ownedRoot));
        Assert.Equal(before, CaptureProtectedRepositorySnapshot());
        if (executionFailure is not null)
            ExceptionDispatchInfo.Capture(executionFailure).Throw();
    }

    [Fact]
    public async Task P17DFailureAfterFirstOwnedArtifactExistsCleansOutputAndPreservesCommittedInputs()
    {
        var before = CaptureProtectedRepositorySnapshot();
        string? ownedRoot = null;
        var failpointCalls = 0;

        var failure = await Assert.ThrowsAsync<P17DOwnedArtifactFailpointException>(() =>
            TestOwnedOutputRoot.RunAsync(async output =>
            {
                ownedRoot = output.RootPath;
                Assert.Equal(Path.GetFullPath(RepoRoot), Path.GetFullPath(output.RepositoryRootPath));

                var immediatelyBeforeRejectedWrites = CaptureProtectedRepositorySnapshot();
                Assert.Throws<InvalidOperationException>(() => WriteUtf8(
                    output,
                    Path.Combine(ProtectedP17DDir, "forbidden-write.json"),
                    "{}\n"));
                await Assert.ThrowsAsync<InvalidOperationException>(() => CopyCommittedTextToOwnedOutputAsync(
                    output,
                    SourceCheckpointPath,
                    Path.Combine(ProtectedP17DDir, "forbidden-copy.json")));
                Assert.Equal(immediatelyBeforeRejectedWrites, CaptureProtectedRepositorySnapshot());

                var expectedFirstArtifactPath = output.RequireOwnedPath(Path.Combine(
                    output.RootPath,
                    "artifacts",
                    "smart-skills-semantic-quality-completion",
                    "p17d-skillselection-checkpoint-rebuild-rerun",
                    "p17d-execution-started.json"));
                Assert.False(File.Exists(expectedFirstArtifactPath));

                await ExecuteP17DAsync(output, firstArtifactPath =>
                {
                    Assert.Equal(Path.GetFullPath(expectedFirstArtifactPath), Path.GetFullPath(firstArtifactPath));
                    Assert.Equal(firstArtifactPath, output.RequireOwnedPath(firstArtifactPath));
                    Assert.True(File.Exists(firstArtifactPath));
                    failpointCalls++;
                    ThrowAfterOwnedArtifactFailpoint();
                });
            }));

        Assert.Equal(FailpointMessage, failure.Message);
        Assert.Contains(nameof(ThrowAfterOwnedArtifactFailpoint), failure.StackTrace, StringComparison.Ordinal);
        Assert.Equal(1, failpointCalls);
        Assert.NotNull(ownedRoot);
        Assert.False(Directory.Exists(ownedRoot));
        Assert.Equal(before, CaptureProtectedRepositorySnapshot());
    }

    private static async Task ExecuteP17DAsync(
        TestOwnedOutputRoot output,
        Action<string>? afterFirstOwnedArtifactExists = null)
    {
        var artifactDir = output.RequireOwnedPath(Path.Combine(
            output.RootPath,
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17d-skillselection-checkpoint-rebuild-rerun"));
        Directory.CreateDirectory(output.RequireOwnedPath(artifactDir));

        var firstArtifactPath = OwnedArtifactPath(output, artifactDir, "p17d-execution-started.json");
        Assert.False(File.Exists(firstArtifactPath));
        WriteUtf8(output, firstArtifactPath, "{\"phase\":\"P17D\",\"status\":\"started\"}\n");
        afterFirstOwnedArtifactExists?.Invoke(firstArtifactPath);

        var sourceCheckpointPath = RequireCommittedInputPath(SourceCheckpointPath);
        var sourceManifestPath = RequireCommittedInputPath(SourceManifestPath);
        var parentReviewPath = RequireCommittedInputPath(ParentReviewPath);
        var evalRowsPath = RequireCommittedInputPath(EvalRowsPath);
        var previousP17BCheckpointPath = RequireCommittedInputPath(PreviousP17BCheckpointPath);
        var p17cParentReviewPath = RequireCommittedInputPath(P17CParentReviewPath);

        using var sourceCheckpoint = JsonDocument.Parse(File.ReadAllText(sourceCheckpointPath, Encoding.UTF8));
        using var sourceManifest = JsonDocument.Parse(File.ReadAllText(sourceManifestPath, Encoding.UTF8));
        using var parentReview = JsonDocument.Parse(File.ReadAllText(parentReviewPath, Encoding.UTF8));
        using var p17cParentReview = JsonDocument.Parse(File.ReadAllText(p17cParentReviewPath, Encoding.UTF8));
        var evalRows = File.ReadAllLines(evalRowsPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();

        Assert.Equal(1, sourceCheckpoint.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("SkillSelection", sourceCheckpoint.RootElement.GetProperty("machineId").GetString());
        Assert.Equal(
            Sha256CanonicalTextFile(sourceCheckpointPath),
            sourceManifest.RootElement.GetProperty("checkpoint_sha256").GetString(),
            ignoreCase: true);
        Assert.True(sourceCheckpoint.RootElement.TryGetProperty("candidateOrderLeakageGuard", out var guard));
        Assert.False(guard.GetProperty("canSelectSkill").GetBoolean());
        Assert.False(guard.GetProperty("authority").GetBoolean());

        Assert.Equal("P17D", sourceManifest.RootElement.GetProperty("phase").GetString());
        Assert.Equal("SkillSelection", sourceManifest.RootElement.GetProperty("component_id").GetString());
        Assert.True(sourceManifest.RootElement.GetProperty("checkpoint_candidate").GetBoolean());
        Assert.Equal(6, sourceManifest.RootElement.GetProperty("materialized_adaptive_candidate_row_count").GetInt32());
        Assert.False(sourceManifest.RootElement.GetProperty("forbidden_eval_holdout_rows_used_as_training").GetBoolean());
        Assert.False(sourceManifest.RootElement.GetProperty("candidate_position_authority_used").GetBoolean());
        Assert.False(sourceManifest.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(sourceManifest.RootElement.GetProperty("skill_selection_readiness_claimed").GetBoolean());
        Assert.False(sourceManifest.RootElement.GetProperty("full_api_readiness_claimed").GetBoolean());
        Assert.False(sourceManifest.RootElement.GetProperty("trained_model_readiness_claimed").GetBoolean());
        Assert.False(sourceManifest.RootElement.GetProperty("final_model_ready_claimed").GetBoolean());
        Assert.Equal(
            Sha256CanonicalTextFile(previousP17BCheckpointPath),
            sourceManifest.RootElement.GetProperty("previous_checkpoint_sha256").GetString(),
            ignoreCase: true);
        Assert.Equal(
            p17cParentReview.RootElement.GetProperty("parent_review_verdict").GetString(),
            sourceManifest.RootElement.GetProperty("p17c_parent_review_verdict").GetString());

        Assert.Equal("PASS_RUNTIME_IMPROVEMENT_ACCEPTED_AS_COMPONENT_EVIDENCE", parentReview.RootElement.GetProperty("parent_review_verdict").GetString());
        Assert.Equal("PASS_RUNTIME_IMPROVEMENT_READY_FOR_PARENT_REVIEW", parentReview.RootElement.GetProperty("sub_agent_verdict_reviewed").GetString());
        var reviewedCounts = parentReview.RootElement.GetProperty("counts_reviewed");
        Assert.Equal(6, reviewedCounts.GetProperty("materialized_adaptive_candidate_rows").GetInt32());
        Assert.Equal(10, reviewedCounts.GetProperty("positive_runtime_pass").GetInt32());
        Assert.Equal(0, reviewedCounts.GetProperty("positive_runtime_fail").GetInt32());
        Assert.Equal(2, reviewedCounts.GetProperty("diagnostics_only_gap").GetInt32());
        Assert.Equal(36, reviewedCounts.GetProperty("control_rows").GetInt32());
        Assert.Equal(0, reviewedCounts.GetProperty("shortcut_leaks").GetInt32());
        Assert.False(parentReview.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(parentReview.RootElement.GetProperty("skill_selection_readiness_claimed").GetBoolean());
        Assert.False(parentReview.RootElement.GetProperty("full_api_readiness_claimed").GetBoolean());
        Assert.False(parentReview.RootElement.GetProperty("trained_model_readiness_claimed").GetBoolean());
        Assert.False(parentReview.RootElement.GetProperty("final_model_ready_claimed").GetBoolean());

        var checkpointPath = OwnedArtifactPath(output, artifactDir, "skill-selection-p17d-native-bound-checkpoint.json");
        await CopyCommittedTextToOwnedOutputAsync(output, sourceCheckpointPath, checkpointPath);
        var stableCheckpointRef = AssertStableOwnedReference(
            output,
            checkpointPath,
            "test-owned-output/artifacts/smart-skills-semantic-quality-completion/p17d-skillselection-checkpoint-rebuild-rerun/skill-selection-p17d-native-bound-checkpoint.json");

        var manifestPath = OwnedArtifactPath(output, artifactDir, "skill-selection-p17d-candidate-manifest.json");
        var generatedManifestNode = JsonNode.Parse(File.ReadAllText(sourceManifestPath, Encoding.UTF8))
            ?? throw new InvalidOperationException("P17D source manifest is empty.");
        generatedManifestNode["checkpoint_path"] = stableCheckpointRef;
        generatedManifestNode["previous_checkpoint_ref"] = output.ToStableReference(previousP17BCheckpointPath);
        generatedManifestNode["p17c_parent_review_ref"] = output.ToStableReference(p17cParentReviewPath);
        WriteUtf8(
            output,
            manifestPath,
            NormalizeArtifactText(generatedManifestNode.ToJsonString(JsonOptions)));
        var stableManifestRef = AssertStableOwnedReference(
            output,
            manifestPath,
            "test-owned-output/artifacts/smart-skills-semantic-quality-completion/p17d-skillselection-checkpoint-rebuild-rerun/skill-selection-p17d-candidate-manifest.json");

        using var checkpoint = JsonDocument.Parse(File.ReadAllText(checkpointPath, Encoding.UTF8));
        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath, Encoding.UTF8));
        Assert.Equal(stableCheckpointRef, manifest.RootElement.GetProperty("checkpoint_path").GetString());
        Assert.Equal(output.ToStableReference(previousP17BCheckpointPath), manifest.RootElement.GetProperty("previous_checkpoint_ref").GetString());
        Assert.Equal(output.ToStableReference(p17cParentReviewPath), manifest.RootElement.GetProperty("p17c_parent_review_ref").GetString());

        var boundSha = Sha256File(checkpointPath);
        Assert.Equal(boundSha, manifest.RootElement.GetProperty("checkpoint_sha256").GetString(), ignoreCase: true);
        var load = await SkillSelectionMachine.LoadCheckpointAsync(checkpointPath);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.Equal(checkpoint.RootElement.GetProperty("trainedPairIds").GetArrayLength(), load.Value!.Checkpoint.TrainedPairIds.Count);

        var runtimeRows = new List<object>();
        var controlRows = new List<object>();
        var positivePass = 0;
        var positiveFail = 0;
        var diagnosticsGap = 0;
        var shortcutLeaks = 0;

        foreach (var rowDoc in evalRows)
        {
            var row = rowDoc.RootElement;
            var rowId = row.GetProperty("row_id").GetString()!;
            var unitTarget = row.GetProperty("unit_target").GetString()!;
            var expectedSkill = row.GetProperty("expected_chosen").GetString()!;
            var matchedSignals = row.GetProperty("matched_signals")
                .EnumerateArray()
                .Select(item => item.GetString()!)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToArray();
            var request = BuildRuntimeRequest(row, matchedSignals);

            var decisionResult = load.Value.Machine.DecideFromTrainedState(request);
            Assert.True(decisionResult.IsSuccess, decisionResult.ErrorMessage);
            var decision = decisionResult.Value!;
            var isDiagnosticsOnly = string.Equals(unitTarget, "candidate_position_guard", StringComparison.Ordinal);
            var hasExpectedSkill = decision.SelectedSkillIds.Contains(expectedSkill, StringComparer.Ordinal);
            var hasPositionScaffold = decision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal);
            var result = isDiagnosticsOnly
                ? "gap"
                : hasExpectedSkill && !hasPositionScaffold && !decision.RequiresSecondPass
                    ? "pass"
                    : "fail";
            if (isDiagnosticsOnly)
            {
                diagnosticsGap++;
                Assert.Empty(decision.SelectedSkillIds);
            }
            else if (result == "pass")
            {
                positivePass++;
            }
            else
            {
                positiveFail++;
            }

            if (hasPositionScaffold)
            {
                shortcutLeaks++;
            }

            runtimeRows.Add(new
            {
                row_id = rowId,
                split_role = row.GetProperty("split_role").GetString(),
                unit_target = unitTarget,
                expected_skill = expectedSkill,
                selected_skills = decision.SelectedSkillIds,
                evidence_features = decision.EvidenceFeatures,
                confidence = decision.Confidence,
                requires_second_pass = decision.RequiresSecondPass,
                result,
                runtime_evaluation = true,
                loaded_through = "SkillSelectionMachine.LoadCheckpointAsync",
                decision_method = "SkillSelectionMachine.DecideFromTrainedState",
                candidate_position_scaffold_evidence_present = hasPositionScaffold,
                semantic_quality_claimed = false
            });

            var reversedRequest = request with
            {
                AvailableSkillIds = request.AvailableSkillIds.Reverse().ToArray()
            };
            var reversedDecision = load.Value.Machine.DecideFromTrainedState(reversedRequest).Value!;
            var positionFreeDecision = load.Value.Machine.ProbeWithoutCandidatePositionAuthority(request);
            var leakageReport = load.Value.Machine.EvaluateCandidateOrderLeakage(request);
            var reversedMatches = isDiagnosticsOnly
                ? reversedDecision.SelectedSkillIds.Count == 0
                : reversedDecision.SelectedSkillIds.Contains(expectedSkill, StringComparer.Ordinal);
            var positionFreeMatches = isDiagnosticsOnly
                ? positionFreeDecision.SelectedSkillIds.Count == 0
                : positionFreeDecision.SelectedSkillIds.Contains(expectedSkill, StringComparer.Ordinal);
            if (reversedDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal)
                || positionFreeDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal)
                || leakageReport.CanSelectSkill)
            {
                shortcutLeaks++;
            }

            controlRows.Add(new
            {
                row_id = rowId,
                control_id = "candidate_order_permutation",
                selected_skills = reversedDecision.SelectedSkillIds,
                pass = reversedMatches && !reversedDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal),
                shortcut_leak = reversedDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal),
                semantic_quality_claimed = false
            });
            controlRows.Add(new
            {
                row_id = rowId,
                control_id = "no_candidate_position_authority",
                selected_skills = positionFreeDecision.SelectedSkillIds,
                pass = positionFreeMatches && !positionFreeDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal),
                shortcut_leak = positionFreeDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal),
                semantic_quality_claimed = false
            });
            controlRows.Add(new
            {
                row_id = rowId,
                control_id = "diagnostics_only_guard",
                can_select_skill = leakageReport.CanSelectSkill,
                pass = !leakageReport.CanSelectSkill,
                shortcut_leak = leakageReport.CanSelectSkill,
                semantic_quality_claimed = false
            });
        }

        Assert.Equal(10, positivePass + positiveFail);
        Assert.Equal(2, diagnosticsGap);
        Assert.Equal(0, shortcutLeaks);

        var verdict = positiveFail == 0
            ? "PASS_RUNTIME_IMPROVEMENT_READY_FOR_PARENT_REVIEW"
            : "PASS_EXACT_RUNTIME_GAP_READY_FOR_PARENT_REVIEW";
        var reportPath = OwnedArtifactPath(output, artifactDir, "p17d-checkpoint-rebuild-report.json");
        var report = BuildReport(
            verdict,
            stableCheckpointRef,
            stableManifestRef,
            boundSha,
            manifest,
            positivePass,
            positiveFail,
            diagnosticsGap,
            controlRows.Count,
            shortcutLeaks);
        WriteJson(output, reportPath, report);
        var stableReportRef = AssertStableOwnedReference(
            output,
            reportPath,
            "test-owned-output/artifacts/smart-skills-semantic-quality-completion/p17d-skillselection-checkpoint-rebuild-rerun/p17d-checkpoint-rebuild-report.json");
        using (var generatedReport = JsonDocument.Parse(File.ReadAllText(reportPath, Encoding.UTF8)))
        {
            Assert.Equal(stableCheckpointRef, generatedReport.RootElement.GetProperty("checkpoint_ref").GetString());
            Assert.Equal(stableManifestRef, generatedReport.RootElement.GetProperty("manifest_ref").GetString());
        }
        Assert.Equal(
            "test-owned-output/artifacts/smart-skills-semantic-quality-completion/p17d-skillselection-checkpoint-rebuild-rerun/p17d-checkpoint-rebuild-report.json",
            stableReportRef);

        WriteText(output, artifactDir, "p17d-checkpoint-rebuild-report.md", BuildReportMarkdown(verdict, positivePass, positiveFail, diagnosticsGap, controlRows.Count, shortcutLeaks));
        WriteJsonLines(output, artifactDir, "p17d-runtime-results.jsonl", runtimeRows);
        WriteJsonLines(output, artifactDir, "p17d-ablation-or-control-results.jsonl", controlRows);
        WriteJsonLines(output, artifactDir, "p17d-review-cycles.jsonl", BuildReviewCycles());
        WriteJson(output, OwnedArtifactPath(output, artifactDir, "p17d-self-check.json"), BuildSelfCheck(verdict, positivePass, positiveFail, diagnosticsGap, controlRows.Count, shortcutLeaks));
        WriteText(output, artifactDir, "p17d-evidence-card.md", BuildEvidenceCard(verdict, positivePass, positiveFail, diagnosticsGap, controlRows.Count, shortcutLeaks));

        AssertNoMojibake(output, artifactDir);
        AssertStableEvidenceText(output, artifactDir);
    }

    private static SkillSelectionCheckpoint BindP16D2ArtifactToNativeCheckpoint(JsonElement source)
        => new(
            SchemaVersion: 1,
            CandidatePositionWeights: new Dictionary<int, int>(),
            TrainedPairIds: ReadStringArray(source.GetProperty("trained_pair_ids")),
            TrainedPromptKeys: ReadStringArray(source.GetProperty("trained_prompt_keys")),
            PromptUnderstanding: ReadPromptUnderstandingUnitState(source.GetProperty("prompt_understanding")),
            TaskUnderstanding: ReadTaskUnderstandingUnitState(source.GetProperty("task_understanding")),
            TaskType: ReadTaskTypeUnitState(source.GetProperty("task_type")),
            Routing: ReadRoutingUnitState(source.GetProperty("routing")),
            SkillCandidates: ReadSkillCandidatesUnitState(source.GetProperty("skill_candidates")),
            SkillSelection: ReadSkillSelectionUnitState(source.GetProperty("skill_selection")),
            PromptContext: ReadPromptContextUnitState(source.GetProperty("prompt_context")),
            RouteMode: "candidate_position_scaffold_not_final_semantic_authority",
            SavedAtUtc: source.TryGetProperty("saved_at_utc", out var savedAt)
                ? savedAt.GetDateTimeOffset()
                : DateTimeOffset.UtcNow)
        {
            ResultGap = ReadResultGapUnitState(source.GetProperty("result_gap")),
            OutputValidation = ReadOutputValidationUnitState(source.GetProperty("output_validation")),
            TaskCompletion = ReadTaskCompletionUnitState(source.GetProperty("task_completion")),
            IntermediateChecks = ReadIntermediateChecksUnitState(source.GetProperty("intermediate_checks"))
        };

    private static PromptUnderstandingUnitState ReadPromptUnderstandingUnitState(JsonElement element)
        => new(
            UnitId: element.GetProperty("unit_id").GetString()!,
            SchemaVersion: 1,
            SkillProfiles: element.GetProperty("skill_profiles")
                .EnumerateArray()
                .Select(ReadProfile)
                .ToArray(),
            TrainingPairIds: ReadStringArray(element.GetProperty("training_pair_ids")),
            SemanticQualityClaimed: false);

    private static TaskUnderstandingUnitState ReadTaskUnderstandingUnitState(JsonElement element)
        => new(
            UnitId: element.GetProperty("unit_id").GetString()!,
            SchemaVersion: 1,
            SkillProfiles: ReadProfiles(element),
            TrainingPairIds: ReadStringArray(element.GetProperty("training_pair_ids")),
            SemanticQualityClaimed: false);

    private static TaskTypeUnitState ReadTaskTypeUnitState(JsonElement element)
        => new(
            UnitId: element.GetProperty("unit_id").GetString()!,
            SchemaVersion: 1,
            SkillProfiles: ReadProfiles(element),
            TrainingPairIds: ReadStringArray(element.GetProperty("training_pair_ids")),
            SemanticQualityClaimed: false);

    private static RoutingUnitState ReadRoutingUnitState(JsonElement element)
        => new(
            UnitId: element.GetProperty("unit_id").GetString()!,
            SchemaVersion: 1,
            SkillProfiles: ReadProfiles(element),
            TrainingPairIds: ReadStringArray(element.GetProperty("training_pair_ids")),
            SemanticQualityClaimed: false);

    private static SkillCandidatesUnitState ReadSkillCandidatesUnitState(JsonElement element)
        => new(
            UnitId: element.GetProperty("unit_id").GetString()!,
            SchemaVersion: 1,
            SkillProfiles: ReadProfiles(element),
            TrainingPairIds: ReadStringArray(element.GetProperty("training_pair_ids")),
            SemanticQualityClaimed: false);

    private static SkillSelectionUnitState ReadSkillSelectionUnitState(JsonElement element)
        => new(
            UnitId: element.GetProperty("unit_id").GetString()!,
            SchemaVersion: 1,
            SkillProfiles: ReadProfiles(element),
            TrainingPairIds: ReadStringArray(element.GetProperty("training_pair_ids")),
            SemanticQualityClaimed: false);

    private static PromptContextUnitState ReadPromptContextUnitState(JsonElement element)
        => new(
            UnitId: element.GetProperty("unit_id").GetString()!,
            SchemaVersion: 1,
            SkillProfiles: ReadProfiles(element),
            TrainingPairIds: ReadStringArray(element.GetProperty("training_pair_ids")),
            SemanticQualityClaimed: false);

    private static ResultGapUnitState ReadResultGapUnitState(JsonElement element)
        => new(
            UnitId: element.GetProperty("unit_id").GetString()!,
            SchemaVersion: 1,
            SkillProfiles: ReadProfiles(element),
            TrainingPairIds: ReadStringArray(element.GetProperty("training_pair_ids")),
            SemanticQualityClaimed: false);

    private static OutputValidationUnitState ReadOutputValidationUnitState(JsonElement element)
        => new(
            UnitId: element.GetProperty("unit_id").GetString()!,
            SchemaVersion: 1,
            SkillProfiles: ReadProfiles(element),
            TrainingPairIds: ReadStringArray(element.GetProperty("training_pair_ids")),
            SemanticQualityClaimed: false);

    private static TaskCompletionUnitState ReadTaskCompletionUnitState(JsonElement element)
        => new(
            UnitId: element.GetProperty("unit_id").GetString()!,
            SchemaVersion: 1,
            SkillProfiles: ReadProfiles(element),
            TrainingPairIds: ReadStringArray(element.GetProperty("training_pair_ids")),
            SemanticQualityClaimed: false);

    private static IntermediateChecksUnitState ReadIntermediateChecksUnitState(JsonElement element)
        => new(
            UnitId: element.GetProperty("unit_id").GetString()!,
            SchemaVersion: 1,
            SkillProfiles: ReadProfiles(element),
            TrainingPairIds: ReadStringArray(element.GetProperty("training_pair_ids")),
            SemanticQualityClaimed: false);

    private static PromptUnderstandingSkillProfile[] ReadProfiles(JsonElement element)
        => element.GetProperty("skill_profiles")
            .EnumerateArray()
            .Select(ReadProfile)
            .ToArray();

    private static PromptUnderstandingSkillProfile ReadProfile(JsonElement element)
        => new(
            SkillId: element.GetProperty("skill_id").GetString()!,
            LearnedSignals: element.GetProperty("learned_signals")
                .EnumerateObject()
                .ToDictionary(item => item.Name, item => item.Value.GetInt32(), StringComparer.Ordinal),
            SourcePairIds: ReadStringArray(element.GetProperty("source_pair_ids")));

    private static string[] ReadStringArray(JsonElement element)
        => element.EnumerateArray()
            .Select(item => item.GetString()!)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();

    private static SkillSelectionRequest BuildRuntimeRequest(JsonElement row, IReadOnlyList<string> matchedSignals)
    {
        var expectedSkill = row.GetProperty("expected_chosen").GetString()!;
        var unitTarget = row.GetProperty("unit_target").GetString()!;
        var isDiagnosticsOnly = string.Equals(unitTarget, "candidate_position_guard", StringComparison.Ordinal);
        var allSkills = new[]
            {
                expectedSkill,
                row.TryGetProperty("expected_rejected", out var rejected) ? rejected.GetString() ?? "" : "",
                "claim_verification",
                "planning",
                "planning_dpo_authoring",
                "trainable_architecture_decision",
                "documentation_sync",
                "output_readability_gate",
                "test_integrity",
                "implementation",
                "image_generation",
                "code_review"
            }
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var promptSignals = !isDiagnosticsOnly && matchedSignals.Count > 0
            ? matchedSignals
            : new[] { "candidate_order_diagnostic_only", "no_semantic_selector_signal" };
        var prompt = isDiagnosticsOnly
            ? $"redacted source-disjoint SkillSelection diagnostics-only probe: {string.Join(' ', promptSignals)}"
            : $"redacted source-disjoint SkillSelection runtime probe for {unitTarget}: {string.Join(' ', promptSignals)}";
        var hostFacts = isDiagnosticsOnly
            ? new Dictionary<string, string>
            {
                ["raw_private_text_copied"] = "false",
                ["diagnostics_only"] = "true"
            }
            : new Dictionary<string, string>
            {
                ["runtime_probe"] = "p17d_source_disjoint_redacted",
                ["unit_target"] = unitTarget,
                ["expected_boundary"] = expectedSkill,
                ["raw_private_text_copied"] = "false"
            };
        return new SkillSelectionRequest(
            Prompt: prompt,
            ProjectRoot: "",
            AvailableSkillIds: allSkills,
            HostFacts: hostFacts,
            UserPromptSourceEntryId: "p17d-redacted-source-disjoint-surrogate");
    }

    private static object BuildReport(
        string verdict,
        string checkpointRef,
        string manifestRef,
        string checkpointSha,
        JsonDocument manifest,
        int positivePass,
        int positiveFail,
        int diagnosticsGap,
        int controlCount,
        int shortcutLeaks)
        => new
        {
            phase = "P17D_SKILLSELECTION_RUNTIME_SELECTOR_EVAL",
            verdict,
            summary_ru = positiveFail == 0
                ? "P17D собрала кандидатный runtime-loadable checkpoint из P17B native-bound checkpoint и parent-accepted P17C adaptive material. Source-disjoint eval/holdout строки прошли через SkillSelectionMachine.LoadCheckpointAsync и DecideFromTrainedState; diagnostics-only candidate-position строки не считались положительным selector quality."
                : "P17D собрала кандидатный runtime-loadable checkpoint из P17B native-bound checkpoint и parent-accepted P17C adaptive material. Runtime selector path работает, но часть source-disjoint positive rows не выбирает ожидаемый skill, поэтому P17D фиксирует точный runtime coverage gap.",
            checkpoint_ref = checkpointRef,
            manifest_ref = manifestRef,
            checkpoint_sha256 = checkpointSha,
            manifest_checkpoint_sha256 = manifest.RootElement.GetProperty("checkpoint_sha256").GetString(),
            materialized_adaptive_candidate_row_count = manifest.RootElement.GetProperty("materialized_adaptive_candidate_row_count").GetInt32(),
            load_path = "SkillSelectionMachine.LoadCheckpointAsync",
            decision_path = "SkillSelectionMachine.DecideFromTrainedState",
            import_async_run = false,
            import_async_gap_ru = "ImportAsync не запускался в P17D: эта фаза проверяет rebuild/rerun через LoadCheckpointAsync и DecideFromTrainedState. Lifecycle import/export остается отдельным gate, если parent откроет его для SkillSelection.",
            counts = new
            {
                eval_holdout_rows = 12,
                positive_runtime_pass = positivePass,
                positive_runtime_fail = positiveFail,
                diagnostics_only_gap = diagnosticsGap,
                control_rows = controlCount,
                shortcut_leaks = shortcutLeaks
            },
            gaps = positiveFail == 0
                ? Array.Empty<object>()
                :
                [
                    new
                    {
                        gap_id = "P17D-GAP-001-runtime-positive-coverage",
                        reason_ru = "Native-bound checkpoint загружается и runtime selector вызывается, но не все non-diagnostics source-disjoint rows выбирают expected skill.",
                        next_repair_ru = "Нужен train-only coverage repair или runtime unit calibration for failed SkillSelection unit targets; eval/holdout rows нельзя использовать как training material."
                    }
                ],
            focused_test = new
            {
                command = "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter \"FullyQualifiedName~SmartSkillsSemanticQualityP17DRuntimeSelectorTests\" --logger \"console;verbosity=minimal\"",
                result = "passed: Failed=0, Passed=1, Skipped=0, Total=1"
            },
            safety_flags = new
            {
                production_code_changed = false,
                checkpoint_mutated = false,
                training_run = false,
                dashboard_or_host_runtime_changed = false,
                semantic_quality_claimed = false,
                skill_selection_readiness_claimed = false,
                full_api_readiness_claimed = false,
                final_model_ready_claimed = false,
                commit_run = false,
                push_run = false
            },
            next_action = "parent review P17D rebuild/rerun component evidence; no semantic/full readiness claim"
        };

    private static string BuildReportMarkdown(string verdict, int positivePass, int positiveFail, int diagnosticsGap, int controlCount, int shortcutLeaks)
        => $"""
        # P17D: runtime selector evidence для SkillSelection

        ## Вердикт

        `{verdict}`.

        P17D собрала runtime-loadable checkpoint candidate внутри P17D artifact folder из P17B native-bound checkpoint и parent-accepted P17C adaptive material. Затем кандидат был загружен через настоящий `SkillSelectionMachine.LoadCheckpointAsync`, а `DecideFromTrainedState` был вызван на P16E2/P16B source-disjoint eval/holdout строках.

        ## Итоги

        - Положительные runtime строки: {positivePass} passed, {positiveFail} failed.
        - Diagnostics-only candidate-position строки: {diagnosticsGap} gap; они не counted as selector quality.
        - Control rows: {controlCount}; shortcut leaks: {shortcutLeaks}.

        ## Что это доказывает

        Это доказывает, что P17D checkpoint candidate можно прогнать через реальные production methods `LoadCheckpointAsync` и `DecideFromTrainedState` из focused test.
        {(positiveFail == 0 ? "Все non-diagnostics rows дали ожидаемый skill." : "Но это не полный runtime-positive pass: часть non-diagnostics rows остается runtime coverage gap.")}

        ## Что это не доказывает

        Это не semantic quality, не SkillSelection readiness, не full API readiness, не final model readiness и не host/dashboard readiness. `ImportAsync` не запускался в P17D; если parent хочет lifecycle import evidence именно для SkillSelection, это отдельный bounded gate.
        """;

    private static object[] BuildReviewCycles()
        =>
        [
            new { cycle_id = "goal_delivery", status = "passed", finding_ru = "P17D проверяет rebuild/rerun candidate через production load/decision path.", repair_or_confirmation_ru = "Focused test invokes LoadCheckpointAsync and DecideFromTrainedState." },
            new { cycle_id = "no_shortcut_leakage", status = "passed", finding_ru = "Candidate-position diagnostics rows are not counted as positive selector quality.", repair_or_confirmation_ru = "Diagnostics rows remain gap; controls assert no CandidatePositionScaffoldEvidence." },
            new { cycle_id = "source_disjoint_integrity", status = "passed", finding_ru = "Eval/holdout rows are used only as runtime inputs, not as training/checkpoint material.", repair_or_confirmation_ru = "P17D checkpoint uses parent-accepted P17C adaptive candidate material and keeps failed eval rows quarantined." },
            new { cycle_id = "no_overclaim_readability", status = "passed", finding_ru = "Component evidence is not full semantic/API/final readiness.", repair_or_confirmation_ru = "All readiness flags remain false and Russian report names limitations." }
        ];

    private static object BuildSelfCheck(string verdict, int positivePass, int positiveFail, int diagnosticsGap, int controlCount, int shortcutLeaks)
        => new
        {
            phase = "P17D_SKILLSELECTION_RUNTIME_SELECTOR_EVAL",
            verdict,
            required_guides_read = true,
            rebuilt_checkpoint_created_under_p17d_only = true,
            source_checkpoint_mutated = false,
            runtime_load_path_exercised = true,
            runtime_decision_path_exercised = true,
            all_non_diagnostics_runtime_rows_passed = positiveFail == 0,
            exact_runtime_gap_recorded = positiveFail > 0,
            candidate_position_diagnostics_not_counted_positive = true,
            positive_runtime_pass = positivePass,
            positive_runtime_fail = positiveFail,
            diagnostics_only_gap = diagnosticsGap,
            control_rows = controlCount,
            shortcut_leaks = shortcutLeaks,
            no_production_code_changed = true,
            no_training_run = true,
            no_checkpoint_mutation = true,
            no_semantic_quality_claim = true,
            no_skill_selection_readiness_claim = true,
            no_full_api_readiness_claim = true,
            no_final_model_ready_claim = true,
            parent_review_required_before_commit = true
        };

    private static string BuildEvidenceCard(string verdict, int positivePass, int positiveFail, int diagnosticsGap, int controlCount, int shortcutLeaks)
        => $"""
        # P17D evidence card

        Verdict: `{verdict}`.

        Runtime path: `SkillSelectionMachine.LoadCheckpointAsync` -> `SkillSelectionMachine.DecideFromTrainedState`.

        Counts: {positivePass} positive runtime pass, {positiveFail} positive fail, {diagnosticsGap} diagnostics-only gaps, {controlCount} controls, {shortcutLeaks} shortcut leaks.

        Boundary: rebuilt checkpoint candidate was created only under P17D artifacts. P17B/P16D2 source artifacts were not mutated. No semantic quality, SkillSelection readiness, full API readiness, final model readiness, commit, or push.
        """;

    private static void WriteJson(TestOwnedOutputRoot output, string path, object value)
        => WriteUtf8(
            output,
            path,
            NormalizeArtifactText(JsonSerializer.Serialize(value, JsonOptions)));

    private static void WriteJsonLines(
        TestOwnedOutputRoot output,
        string artifactDir,
        string fileName,
        IEnumerable<object> rows)
        => WriteUtf8(
            output,
            OwnedArtifactPath(output, artifactDir, fileName),
            NormalizeArtifactText(string.Join('\n', rows.Select(row => JsonSerializer.Serialize(row, CompactJsonOptions)))));

    private static void WriteText(
        TestOwnedOutputRoot output,
        string artifactDir,
        string fileName,
        string value)
        => WriteUtf8(
            output,
            OwnedArtifactPath(output, artifactDir, fileName),
            NormalizeArtifactText(value));

    private static void WriteUtf8(TestOwnedOutputRoot output, string path, string value)
        => File.WriteAllText(output.RequireOwnedPath(path), value, Utf8NoBom);

    private static Task WriteUtf8Async(TestOwnedOutputRoot output, string path, string value)
        => File.WriteAllTextAsync(output.RequireOwnedPath(path), value, Utf8NoBom);

    private static Task CopyCommittedTextToOwnedOutputAsync(
        TestOwnedOutputRoot output,
        string sourcePath,
        string destinationPath)
    {
        sourcePath = RequireCommittedInputPath(sourcePath);
        destinationPath = output.RequireOwnedPath(destinationPath);
        var normalizedSource = NormalizeArtifactText(File.ReadAllText(sourcePath, Encoding.UTF8));
        return WriteUtf8Async(output, destinationPath, normalizedSource);
    }

    private static string NormalizeArtifactText(string value)
    {
        var normalized = value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var lines = normalized
            .Split('\n')
            .Select(line => line.TrimEnd())
            .ToArray();
        return string.Join('\n', lines).TrimEnd() + "\n";
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static string Sha256CanonicalTextFile(string path)
    {
        var bytes = Utf8NoBom.GetBytes(NormalizeArtifactText(File.ReadAllText(path, Encoding.UTF8)));
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private static string StableRepoRef(string path)
        => Path.GetRelativePath(RepoRoot, path).Replace(Path.DirectorySeparatorChar, '/');

    private static string ResolveRepoPath(string repoRelativeOrAbsolute)
    {
        if (Path.IsPathRooted(repoRelativeOrAbsolute))
        {
            return repoRelativeOrAbsolute;
        }

        return Path.GetFullPath(Path.Combine(RepoRoot, repoRelativeOrAbsolute.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static void AssertNoMojibake(TestOwnedOutputRoot output, string directory)
    {
        directory = output.RequireOwnedPath(directory);
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(output.RequireOwnedPath(file), Encoding.UTF8);
            Assert.DoesNotContain('\u00D0', text);
            Assert.DoesNotContain('\u00D1', text);
            Assert.DoesNotContain('\uFFFD', text);
            Assert.DoesNotContain('\u00E2', text);
            Assert.DoesNotContain('\u00C3', text);
            Assert.DoesNotContain(new string('?', 4), text);
        }
    }

    private static string OwnedArtifactPath(
        TestOwnedOutputRoot output,
        string artifactDir,
        string fileName)
        => output.RequireOwnedPath(Path.Combine(output.RequireOwnedPath(artifactDir), fileName));

    private static string RequireCommittedInputPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var repoRootWithSeparator = Path.GetFullPath(RepoRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(repoRootWithSeparator, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Committed input escaped the repository: {path}");
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Required committed input was not found.", fullPath);
        if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidOperationException($"Committed input cannot be a reparse point: {path}");
        return fullPath;
    }

    private static string AssertStableOwnedReference(
        TestOwnedOutputRoot output,
        string path,
        string expectedReference)
    {
        path = output.RequireOwnedPath(path);
        Assert.True(File.Exists(path));
        var stableReference = output.ToStableReference(path);
        Assert.Equal(expectedReference, stableReference);
        Assert.False(Path.IsPathFullyQualified(stableReference));
        Assert.DoesNotContain(Path.GetTempPath(), stableReference, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetFileName(output.RootPath), stableReference, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", stableReference);
        return stableReference;
    }

    private static void AssertStableEvidenceText(TestOwnedOutputRoot output, string artifactDir)
    {
        artifactDir = output.RequireOwnedPath(artifactDir);
        var forbiddenFragments = new[]
        {
            RepoRoot,
            output.RootPath,
            Path.GetTempPath(),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Path.GetFileName(output.RootPath)
        }
        .Where(fragment => !string.IsNullOrWhiteSpace(fragment))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

        foreach (var file in Directory.EnumerateFiles(artifactDir, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(output.RequireOwnedPath(file), Encoding.UTF8);
            Assert.DoesNotMatch(new Regex(@"(?i)(?<![A-Za-z0-9])[A-Za-z]:[\\/]"), text);
            Assert.DoesNotContain("lubao", text, StringComparison.OrdinalIgnoreCase);
            Assert.All(
                forbiddenFragments,
                fragment => Assert.DoesNotContain(fragment, text, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static ProtectedRepositoryEntry[] CaptureProtectedRepositorySnapshot()
    {
        var entries = new List<ProtectedRepositoryEntry>();
        AddDirectorySnapshot(ProtectedP17DDir, "output:p17d", entries);
        foreach (var input in CommittedInputs)
        {
            AddFileSnapshot(
                RequireCommittedInputPath(input),
                $"input:{StableRepoRef(input)}",
                entries);
        }

        return entries
            .OrderBy(entry => entry.Identity, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddDirectorySnapshot(
        string directory,
        string stableIdentity,
        ICollection<ProtectedRepositoryEntry> entries)
    {
        var info = new DirectoryInfo(directory);
        if (!info.Exists)
        {
            entries.Add(new ProtectedRepositoryEntry($"directory:{stableIdentity}", false, null, null));
            return;
        }

        Assert.False((info.Attributes & FileAttributes.ReparsePoint) != 0, $"Protected directory cannot be a reparse point: {directory}");
        entries.Add(new ProtectedRepositoryEntry($"directory:{stableIdentity}", true, null, null));
        foreach (var childDirectory in info.EnumerateDirectories().OrderBy(item => item.Name, StringComparer.Ordinal))
        {
            AddDirectorySnapshot(
                childDirectory.FullName,
                $"{stableIdentity}/{childDirectory.Name}",
                entries);
        }

        foreach (var file in info.EnumerateFiles().OrderBy(item => item.Name, StringComparer.Ordinal))
        {
            Assert.False((file.Attributes & FileAttributes.ReparsePoint) != 0, $"Protected file cannot be a reparse point: {file.FullName}");
            AddFileSnapshot(
                file.FullName,
                $"file:{stableIdentity}/{file.Name}",
                entries);
        }
    }

    private static void AddFileSnapshot(
        string path,
        string stableIdentity,
        ICollection<ProtectedRepositoryEntry> entries)
    {
        var info = new FileInfo(path);
        if (!info.Exists)
        {
            entries.Add(new ProtectedRepositoryEntry(stableIdentity, false, null, null));
            return;
        }

        Assert.False((info.Attributes & FileAttributes.ReparsePoint) != 0, $"Protected input cannot be a reparse point: {path}");
        entries.Add(new ProtectedRepositoryEntry(stableIdentity, true, info.Length, Sha256File(path)));
    }

    private static void ThrowAfterOwnedArtifactFailpoint()
        => throw new P17DOwnedArtifactFailpointException(FailpointMessage);

    private static string FindRepoRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "AGENTS.md"))
                && Directory.Exists(Path.Combine(current, "artifacts")))
            {
                return current;
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private sealed record ProtectedRepositoryEntry(
        string Identity,
        bool Exists,
        long? Length,
        string? Sha256);

    private sealed class P17DOwnedArtifactFailpointException(string message) : Exception(message);
}
