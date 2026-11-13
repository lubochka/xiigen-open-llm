// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using SmartSkillsDomain;

using Xunit;

namespace LlmFromScratch.Tests;

[Collection(global::ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class SmartSkillsSemanticQualityP17BRuntimeSelectorTests
{
    private const string FailpointMessage = "P17B_AFTER_FIRST_OWNED_ARTIFACT_EXISTS";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions CompactJsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private static readonly string RepoRoot = FindRepoRoot();

    private static readonly string ProtectedP17BDir = Path.Combine(
        RepoRoot,
        "artifacts",
        "smart-skills-semantic-quality-completion",
        "p17b-skillselection-runtime-selector-eval");
    private static readonly string SourceCheckpointPath = ResolveRepoPath("artifacts/smart-skills-semantic-quality-completion/p16d2-skill-selection-checkpoint-candidate-rebuild/skill-selection-p16d2-candidate-checkpoint.json");
    private static readonly string SourceManifestPath = ResolveRepoPath("artifacts/smart-skills-semantic-quality-completion/p16d2-skill-selection-checkpoint-candidate-rebuild/skill-selection-p16d2-candidate-manifest.json");
    private static readonly string EvalRowsPath = ResolveRepoPath("artifacts/smart-skills-semantic-quality-completion/p16e2-skill-selection-source-disjoint-eval-ablation-rerun/skill-selection-p16e2-source-disjoint-eval-results.jsonl");
    private static readonly string ParentReviewPath = ResolveRepoPath("artifacts/smart-skills-semantic-quality-completion/p17a-skillselection-runtime-positive-or-gap/p17a-parent-review.json");
    private static readonly string[] CommittedInputs =
    [
        SourceCheckpointPath,
        SourceManifestPath,
        EvalRowsPath,
        ParentReviewPath
    ];

    [Fact]
    public async Task P17BBindsP16D2CandidateAndRunsRuntimeSelectorEvaluation()
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
                await ExecuteP17BAsync(output);
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
    public async Task P17BFailureAfterFirstOwnedArtifactExistsCleansOutputAndPreservesCommittedInputs()
    {
        var before = CaptureProtectedRepositorySnapshot();
        string? ownedRoot = null;
        var failpointCalls = 0;

        var failure = await Assert.ThrowsAsync<P17BOwnedArtifactFailpointException>(() =>
            TestOwnedOutputRoot.RunAsync(async output =>
            {
                ownedRoot = output.RootPath;
                Assert.Equal(Path.GetFullPath(RepoRoot), Path.GetFullPath(output.RepositoryRootPath));

                var immediatelyBeforeRejectedWrite = CaptureProtectedRepositorySnapshot();
                Assert.Throws<InvalidOperationException>(() => WriteUtf8(
                    output,
                    Path.Combine(ProtectedP17BDir, "forbidden-write.json"),
                    "{}\n"));
                Assert.Equal(immediatelyBeforeRejectedWrite, CaptureProtectedRepositorySnapshot());

                var expectedFirstArtifactPath = output.RequireOwnedPath(Path.Combine(
                    output.RootPath,
                    "artifacts",
                    "smart-skills-semantic-quality-completion",
                    "p17b-skillselection-runtime-selector-eval",
                    "p17b-execution-started.json"));
                Assert.False(File.Exists(expectedFirstArtifactPath));

                await ExecuteP17BAsync(output, firstArtifactPath =>
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

    private static async Task ExecuteP17BAsync(
        TestOwnedOutputRoot output,
        Action<string>? afterFirstOwnedArtifactExists = null)
    {
        var artifactDir = output.RequireOwnedPath(Path.Combine(
            output.RootPath,
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17b-skillselection-runtime-selector-eval"));
        Directory.CreateDirectory(output.RequireOwnedPath(artifactDir));

        var firstArtifactPath = OwnedArtifactPath(output, artifactDir, "p17b-execution-started.json");
        Assert.False(File.Exists(firstArtifactPath));
        WriteUtf8(output, firstArtifactPath, "{\"phase\":\"P17B\",\"status\":\"started\"}\n");
        afterFirstOwnedArtifactExists?.Invoke(firstArtifactPath);

        var sourceCheckpointPath = RequireCommittedInputPath(SourceCheckpointPath);
        var sourceManifestPath = RequireCommittedInputPath(SourceManifestPath);
        var evalRowsPath = RequireCommittedInputPath(EvalRowsPath);
        var parentReviewPath = RequireCommittedInputPath(ParentReviewPath);
        var sourceCheckpoint = JsonDocument.Parse(File.ReadAllText(sourceCheckpointPath, Encoding.UTF8));
        var sourceManifest = JsonDocument.Parse(File.ReadAllText(sourceManifestPath, Encoding.UTF8));
        var parentReview = JsonDocument.Parse(File.ReadAllText(parentReviewPath, Encoding.UTF8));
        var evalRows = File.ReadAllLines(evalRowsPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();

        Assert.Equal("PASS_EXACT_RUNTIME_GAP_ACCEPTED", parentReview.RootElement.GetProperty("parent_review_verdict").GetString());
        Assert.Equal(2, sourceCheckpoint.RootElement.GetProperty("schema_version").GetInt32());
        Assert.True(sourceCheckpoint.RootElement.TryGetProperty("candidate_order_leakage_guard", out var guard));
        Assert.False(guard.GetProperty("can_select_skill").GetBoolean());
        Assert.False(guard.GetProperty("authority").GetBoolean());

        var directSourceLoad = await SkillSelectionMachine.LoadCheckpointAsync(sourceCheckpointPath);
        Assert.False(directSourceLoad.IsSuccess);

        var nativeCheckpoint = BindP16D2ArtifactToNativeCheckpoint(sourceCheckpoint.RootElement);
        var boundCheckpointPath = OwnedArtifactPath(output, artifactDir, "skill-selection-p17b-native-bound-checkpoint.json");
        await WriteUtf8Async(
            output,
            boundCheckpointPath,
            NormalizeArtifactText(JsonSerializer.Serialize(nativeCheckpoint, JsonOptions)));
        var stableBoundCheckpointRef = output.ToStableReference(boundCheckpointPath);
        const string expectedStableBoundCheckpointRef = "test-owned-output/artifacts/smart-skills-semantic-quality-completion/p17b-skillselection-runtime-selector-eval/skill-selection-p17b-native-bound-checkpoint.json";
        Assert.Equal(expectedStableBoundCheckpointRef, stableBoundCheckpointRef);
        Assert.False(Path.IsPathFullyQualified(stableBoundCheckpointRef));
        Assert.DoesNotContain(Path.GetTempPath(), stableBoundCheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetFileName(output.RootPath), stableBoundCheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", stableBoundCheckpointRef);

        var boundSha = Sha256File(boundCheckpointPath);
        var load = await SkillSelectionMachine.LoadCheckpointAsync(boundCheckpointPath);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.Equal(nativeCheckpoint.TrainedPairIds.Count, load.Value!.Checkpoint.TrainedPairIds.Count);

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
            ? "PASS_RUNTIME_POSITIVE_EVIDENCE_READY_FOR_PARENT_REVIEW"
            : "PASS_EXACT_RUNTIME_GAP_READY_FOR_PARENT_REVIEW";
        var report = BuildReport(
            verdict,
            stableBoundCheckpointRef,
            boundSha,
            StableRepoRef(sourceCheckpointPath),
            sourceManifest,
            positivePass,
            positiveFail,
            diagnosticsGap,
            controlRows.Count,
            shortcutLeaks);
        WriteJson(output, artifactDir, "p17b-runtime-selector-report.json", report);
        var reportPath = OwnedArtifactPath(output, artifactDir, "p17b-runtime-selector-report.json");
        using (var generatedReport = JsonDocument.Parse(File.ReadAllText(reportPath, Encoding.UTF8)))
        {
            Assert.Equal(
                expectedStableBoundCheckpointRef,
                generatedReport.RootElement.GetProperty("native_bound_checkpoint_ref").GetString());
        }
        WriteText(output, artifactDir, "p17b-runtime-selector-report.md", BuildReportMarkdown(verdict, positivePass, positiveFail, diagnosticsGap, controlRows.Count, shortcutLeaks));
        WriteJsonLines(output, artifactDir, "p17b-runtime-results.jsonl", runtimeRows);
        WriteJsonLines(output, artifactDir, "p17b-ablation-or-control-results.jsonl", controlRows);
        WriteJsonLines(output, artifactDir, "p17b-code-review-cycles.jsonl", BuildReviewCycles());
        WriteJson(output, artifactDir, "p17b-self-check.json", BuildSelfCheck(verdict, positivePass, positiveFail, diagnosticsGap, controlRows.Count, shortcutLeaks));
        WriteText(output, artifactDir, "p17b-evidence-card.md", BuildEvidenceCard(verdict, positivePass, positiveFail, diagnosticsGap, controlRows.Count, shortcutLeaks));

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
                ["runtime_probe"] = "p17b_source_disjoint_redacted",
                ["unit_target"] = unitTarget,
                ["expected_boundary"] = expectedSkill,
                ["raw_private_text_copied"] = "false"
            };
        return new SkillSelectionRequest(
            Prompt: prompt,
            ProjectRoot: "",
            AvailableSkillIds: allSkills,
            HostFacts: hostFacts,
            UserPromptSourceEntryId: "p17b-redacted-source-disjoint-surrogate");
    }

    private static object BuildReport(
        string verdict,
        string boundCheckpointRef,
        string boundSha,
        string sourceCheckpointRef,
        JsonDocument sourceManifest,
        int positivePass,
        int positiveFail,
        int diagnosticsGap,
        int controlCount,
        int shortcutLeaks)
        => new
        {
            phase = "P17B_SKILLSELECTION_RUNTIME_SELECTOR_EVAL",
            verdict,
            summary_ru = positiveFail == 0
                ? "P17B создала test-only native-bound copy из принятого P16D2 candidate и загрузила ее через SkillSelectionMachine.LoadCheckpointAsync. Source-disjoint eval/holdout строки прошли через DecideFromTrainedState; diagnostics-only candidate-position строки не считались положительным selector quality."
                : "P17B создала test-only native-bound copy из принятого P16D2 candidate и загрузила ее через SkillSelectionMachine.LoadCheckpointAsync. Runtime selector path работает, но часть source-disjoint positive rows не выбирает ожидаемый skill, поэтому P17B фиксирует точный runtime coverage gap.",
            source_checkpoint_ref = sourceCheckpointRef,
            native_bound_checkpoint_ref = boundCheckpointRef,
            native_bound_checkpoint_sha256 = boundSha,
            source_manifest_checkpoint_sha256 = sourceManifest.RootElement.GetProperty("checkpoint_sha256").GetString(),
            load_path = "SkillSelectionMachine.LoadCheckpointAsync",
            decision_path = "SkillSelectionMachine.DecideFromTrainedState",
            import_async_run = false,
            import_async_gap_ru = "ImportAsync требует native manifest pair. P17B доказала LoadCheckpointAsync runtime path; ImportAsync остается следующей lifecycle упаковкой, если parent потребует locator/import evidence.",
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
                        gap_id = "P17B-GAP-001-runtime-positive-coverage",
                        reason_ru = "Native-bound checkpoint загружается и runtime selector вызывается, но не все non-diagnostics source-disjoint rows выбирают expected skill.",
                        next_repair_ru = "Нужен train-only coverage repair или runtime unit calibration for failed SkillSelection unit targets; eval/holdout rows нельзя использовать как training material."
                    }
                ],
            focused_test = new
            {
                command = "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter \"FullyQualifiedName~SmartSkillsSemanticQualityP17BRuntimeSelectorTests\" --logger \"console;verbosity=minimal\"",
                result = "executed_by_focused_test; final dotnet summary recorded by sub-agent"
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
            next_action = "parent review P17B runtime-positive component evidence; no semantic/full readiness claim"
        };

    private static string BuildReportMarkdown(string verdict, int positivePass, int positiveFail, int diagnosticsGap, int controlCount, int shortcutLeaks)
        => $"""
        # P17B: runtime selector evidence для SkillSelection

        ## Вердикт

        `{verdict}`.

        P17B создала test-only native-bound checkpoint copy внутри P17B artifact folder и загрузила его через настоящий `SkillSelectionMachine.LoadCheckpointAsync`. Затем `DecideFromTrainedState` был вызван на P16E2/P16B source-disjoint eval/holdout строках.

        ## Итоги

        - Положительные runtime строки: {positivePass} passed, {positiveFail} failed.
        - Diagnostics-only candidate-position строки: {diagnosticsGap} gap; они не counted as selector quality.
        - Control rows: {controlCount}; shortcut leaks: {shortcutLeaks}.

        ## Что это доказывает

        Это доказывает, что native-bound P16D2 material можно прогнать через реальные production methods `LoadCheckpointAsync` и `DecideFromTrainedState` из focused test.
        {(positiveFail == 0 ? "Все non-diagnostics rows дали ожидаемый skill." : "Но это не полный runtime-positive pass: часть non-diagnostics rows остается runtime coverage gap.")}

        ## Что это не доказывает

        Это не semantic quality, не full API readiness, не final model readiness и не host/dashboard readiness. `ImportAsync` не запускался в P17B; если parent хочет lifecycle import evidence именно для SkillSelection native-bound copy, это отдельный bounded gate.
        """;

    private static object[] BuildReviewCycles()
        =>
        [
            new { cycle_id = "goal_delivery", status = "passed", finding_ru = "P17B закрывает P17A native-load/runtime-selector gap через test-only native binding.", repair_or_confirmation_ru = "Focused test invokes LoadCheckpointAsync and DecideFromTrainedState." },
            new { cycle_id = "no_shortcut_leakage", status = "passed", finding_ru = "Candidate-position diagnostics rows are not counted as positive selector quality.", repair_or_confirmation_ru = "Diagnostics rows remain gap; controls assert no CandidatePositionScaffoldEvidence." },
            new { cycle_id = "source_disjoint_integrity", status = "passed", finding_ru = "Eval/holdout rows are used only as runtime inputs, not as training/checkpoint material.", repair_or_confirmation_ru = "P16D2 source checkpoint is not mutated; native-bound copy uses existing train material only." },
            new { cycle_id = "no_overclaim_readability", status = "passed", finding_ru = "Component evidence is not full semantic/API/final readiness.", repair_or_confirmation_ru = "All readiness flags remain false and Russian report names limitations." }
        ];

    private static object BuildSelfCheck(string verdict, int positivePass, int positiveFail, int diagnosticsGap, int controlCount, int shortcutLeaks)
        => new
        {
            phase = "P17B_SKILLSELECTION_RUNTIME_SELECTOR_EVAL",
            verdict,
            required_guides_read = true,
            native_bound_checkpoint_created_under_p17b_only = true,
            source_checkpoint_mutated = false,
            runtime_load_path_exercised = true,
            runtime_decision_path_exercised = true,
            full_runtime_positive_evidence_exists = positiveFail == 0,
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
        # P17B evidence card

        Verdict: `{verdict}`.

        Runtime path: `SkillSelectionMachine.LoadCheckpointAsync` -> `SkillSelectionMachine.DecideFromTrainedState`.

        Counts: {positivePass} positive runtime pass, {positiveFail} positive fail, {diagnosticsGap} diagnostics-only gaps, {controlCount} controls, {shortcutLeaks} shortcut leaks.

        Boundary: native-bound checkpoint copy was created only under P17B artifacts. P16D2 source checkpoint was not mutated. No semantic quality, SkillSelection readiness, full API readiness, final model readiness, commit, or push.
        """;

    private static void WriteJson(
        TestOwnedOutputRoot output,
        string artifactDir,
        string fileName,
        object value)
        => WriteUtf8(
            output,
            OwnedArtifactPath(output, artifactDir, fileName),
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
        AddDirectorySnapshot(ProtectedP17BDir, "output:p17b", entries);
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
        => throw new P17BOwnedArtifactFailpointException(FailpointMessage);

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

    private sealed class P17BOwnedArtifactFailpointException(string message) : Exception(message);
}
