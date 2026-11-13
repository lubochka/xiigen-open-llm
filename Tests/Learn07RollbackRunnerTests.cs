// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

using AgentInfrastructure;
using SmartSkillsDomain;

using Xunit;

namespace LlmFromScratch.Tests;

public sealed class Learn07RollbackRunnerTests
{
    private const string PhaseId = "LEARN-07";
    private const string Command = "in-process Learn07RollbackRunnerTests rollback fixture materialization and public load probes";
    private const string TargetOnlyOrchestrationPatternId = "or-learned-pattern:learn-04-local-runtime-token-quality-boundary";
    private const string FixtureTempDirectoryName = "xiigen-learn-07-rollback-fixtures";
    private const string ManifestBoundTempDirectoryName = "xiigen-learn-07-manifest-bound";
    private const string TestTempRootRef = "<test-temp-root>";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions CompactJsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Learn07VerifiesRollbackBoundariesWithoutPromotion()
    {
        var repoRoot = FindRepoRoot();
        var artifactDir = Path.Combine(repoRoot, "artifacts", "extension-runtime-token-quality-learnability");
        Directory.CreateDirectory(artifactDir);

        var fixtureRoot = CreateOwnedTempRoot(FixtureTempDirectoryName);
        try
        {
            var fixture = await MaterializeRollbackFixtureAsync(repoRoot, fixtureRoot);

            var tracks = new[]
            {
                ProbePromptToTaskRollback(
                    fixture.PromptToTaskTargetPath,
                    fixture.PromptToTaskRollbackPath,
                    fixtureRoot),
                await ProbeOrchestrationRollbackAsync(
                    fixture.OrchestrationTargetPath,
                    fixture.OrchestrationTargetManifestPath,
                    fixture.OrchestrationRollbackPath,
                    fixture.OrchestrationRollbackManifestPath,
                    fixtureRoot),
                ProbeSkillSelectionRollback(
                    fixture.SkillSelectionTargetPath,
                    fixture.SkillSelectionRollbackPath,
                    fixtureRoot)
            };

            DeleteOwnedTempRoot(fixtureRoot, FixtureTempDirectoryName);
            var safetyEvidence = new Learn07SafetyEvidence(
                TargetPromotionPerformed: false,
                LocatorUpdated: false,
                OuterFixtureRootDeletedBeforeArtifactWrite: !Directory.Exists(fixtureRoot));
            var accepted = IsStrictlyAccepted(tracks, safetyEvidence);
            IReadOnlyList<string> whatThisProves = accepted
                ?
                [
                    "Rollback proves recovery only. It does not prove the target should be promoted.",
                    "The PromptToTask rollback overlay loads through PromptToTaskAdaptiveTrainer.LoadCheckpoint and target-only trained signal ids/profiles are absent.",
                    "The OrchestrationReasoning rollback checkpoint loads through OrchestrationReasoningMachine.LoadCheckpointAsync plus Plan using the manifest-bound public boundary, and the test-owned LEARN-04 pattern id/schema fixture does not match after rollback.",
                    "The SkillSelection rollback overlay loads through SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint and target-only correction profiles/trained signal ids are absent.",
                    "The orchestration Plan call is recorded only as a test-owned in-process probe, not as a runtime invocation.",
                    "The exact test-owned outer fixture root was deleted and its absence confirmed before these final artifacts were written."
                ]
                :
                [
                    "The strict LEARN-07 acceptance predicate failed; no rollback acceptance claim is made."
                ];
            IReadOnlyList<string> parentFindings = accepted
                ?
                [
                    "OrchestrationReasoning rollback is accepted through the public machine boundary with target-only test-owned LEARN-04 pattern id/schema absent after rollback.",
                    "PromptToTask and SkillSelection rollback are accepted only as overlay-level rollback checks; full machine rollback is not claimed for those tracks.",
                    "No semantic quality, universal learnability, target promotion, locator update, common model mutation, runtime host proof, or external AI claim is made."
                ]
                :
                [
                    "The strict LEARN-07 acceptance predicate failed; parent acceptance and phase-commit readiness are not claimed."
                ];

            var report = new Learn07Report(
                Schema: "xiigen.learn_07_rollback_report.v1",
                PhaseId: PhaseId,
                Verdict: accepted ? "PASS_WITH_LIMITATIONS" : "FAIL",
                GeneratedAtUtc: DateTimeOffset.UtcNow,
                Commands: [Command],
                CommandResults:
                [
                    new Learn07CommandResult(
                        Command: Command,
                        ExitCode: null,
                        Result: "IN_PROCESS_PROBES_COMPLETED",
                        Summary: "This report records in-process Learn07 probes, which do not have a process exit code; external dotnet/TRX status is recorded in the caller's test artifacts.")
                ],
                Tracks: tracks,
                SafetyEvidence: safetyEvidence,
                WhatThisProves: whatThisProves,
                WhatThisDoesNotProve:
                [
                    "This does not prove target quality, target promotion, host runtime readiness, or universal learnability.",
                    "PromptToTask and SkillSelection remain PARTIAL_OVERLAY_ROLLBACK_ONLY because their rollback assets are adaptive overlays, not full machine checkpoint plus manifest artifacts.",
                    "The Orchestration rollback manifest checkpointRef differs from the approval packet alias; the verifier used a temp manifest-bound copy for public LoadCheckpointAsync and did not rewrite the named rollback assets.",
                    "RuntimeInvocationSucceeded is false for every track; runtime_allowed and runtime_launched stay false in the state patch."
                ],
                NextIndependentTrack: "final execution review preparation");

            var validation = new Learn07Validation(
                Schema: "xiigen.learn_07_validation.v1",
                PhaseId: PhaseId,
                Verdict: accepted ? "ACCEPTED_FOR_PARENT_REVIEW_WITH_LIMITATIONS" : "FAIL",
                AcceptedForParentReview: accepted,
                Checks: new Dictionary<string, object>
                {
                    ["rollback_report_exists"] = true,
                    ["track_count"] = tracks.Length,
                    ["track_verdicts_separated"] = tracks.Select(track => track.MachineId).Distinct(StringComparer.Ordinal).Count() == 3,
                    ["prompt_to_task_partial_overlay_only"] = tracks.Any(track => track.MachineId == "PromptToTaskMachine" && track.Verdict == "PARTIAL_OVERLAY_ROLLBACK_ONLY"),
                    ["orchestration_public_machine_boundary_used"] = tracks.Any(track => track.MachineId == "OrchestrationReasoningMachine" && track.BoundaryUsed.Contains("OrchestrationReasoningMachine.LoadCheckpointAsync", StringComparison.Ordinal)),
                    ["skill_selection_partial_overlay_only"] = tracks.Any(track => track.MachineId == "SkillSelectionMachine" && track.Verdict == "PARTIAL_OVERLAY_ROLLBACK_ONLY"),
                    ["target_only_learned_changes_absent_after_rollback"] = tracks.All(track => track.TargetOnlyLearnedChangesAbsentAfterRollback),
                    ["runtime_invocation_succeeded"] = tracks.Any(track => track.RuntimeInvocationSucceeded),
                    ["test_only_in_process_orchestration_probe_succeeded"] = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").TestOnlyInProcessProbeSucceeded,
                    ["test_only_in_process_probe_limited_to_orchestration"] = tracks.Where(track => track.MachineId != "OrchestrationReasoningMachine").All(track => !track.TestOnlyInProcessProbeSucceeded),
                    ["fixture_artifacts_under_test_temp_root"] = TracksUseOnlyStableTestTempRefs(tracks),
                    ["outer_fixture_root_deleted_before_artifact_write"] = safetyEvidence.OuterFixtureRootDeletedBeforeArtifactWrite,
                    ["generated_artifacts_private_path_leak_scan"] = "enforced_by_test",
                    ["file_existence_alone_claimed"] = false,
                    ["semantic_quality_claimed"] = tracks.Any(track => track.SemanticQualityClaimed),
                    ["learnability_claimed"] = false,
                    ["promotion_ready_claimed"] = false,
                    ["target_promotion_or_locator_update"] = safetyEvidence.TargetPromotionPerformed || safetyEvidence.LocatorUpdated,
                    ["common_model_mutated"] = tracks.Any(track => track.CommonModelMutated),
                    ["external_ai_used"] = false
                },
                Commands: report.Commands,
                NextPhase: "final execution review preparation",
                Limitations: report.WhatThisDoesNotProve);

            var parentReview = new ParentReview(
                Schema: "xiigen.parent_review.learn_07.v1",
                ReviewId: "parent-review-LEARN-07",
                PhaseId: PhaseId,
                Verdict: accepted ? "ACCEPTED_FOR_PHASE_COMMIT_WITH_LIMITATIONS" : "FAIL",
                Accepted: accepted,
                Findings: parentFindings,
                Tracks: tracks,
                SafetyEvidence: safetyEvidence,
                Commands: report.Commands,
                NextPhase: "final execution review preparation");

            WriteJson(Path.Combine(artifactDir, "rollback-report.json"), report);
            WriteRollbackMarkdown(Path.Combine(artifactDir, "rollback-report.md"), report);
            WriteJson(Path.Combine(artifactDir, "learn-07-validation.json"), validation);
            WriteJson(Path.Combine(artifactDir, "parent-review-LEARN-07.json"), parentReview);
            WriteParentReviewMarkdown(Path.Combine(artifactDir, "parent-review-LEARN-07.md"), parentReview);
            var statePatchPath = UpdateExecutionState(repoRoot, artifactDir, tracks, safetyEvidence, accepted);

            AssertNoPrivatePathsInGeneratedLearn07Outputs(repoRoot, fixtureRoot, artifactDir, statePatchPath);
            AssertFinalLearn07Artifacts(artifactDir, statePatchPath);
        }
        finally
        {
            DeleteOwnedTempRoot(fixtureRoot, FixtureTempDirectoryName);
        }
    }

    [Fact]
    public void Learn07OwnedFixtureCleanupIsIdempotentAfterInjectedFailure()
    {
        var fixtureRoot = CreateOwnedTempRoot(FixtureTempDirectoryName);
        try
        {
            var injectFailure = new Action(() =>
            {
                try
                {
                    File.WriteAllText(
                        Path.Combine(fixtureRoot, "failure-injection.txt"),
                        "test-owned failure injection",
                        Encoding.UTF8);
                    throw new InvalidOperationException("learn-07-cleanup-failure-injection");
                }
                finally
                {
                    DeleteOwnedTempRoot(fixtureRoot, FixtureTempDirectoryName);
                }
            });
            var injected = Assert.Throws<InvalidOperationException>(injectFailure);

            Assert.Equal("learn-07-cleanup-failure-injection", injected.Message);
            Assert.False(Directory.Exists(fixtureRoot));
            DeleteOwnedTempRoot(fixtureRoot, FixtureTempDirectoryName);
            Assert.False(Directory.Exists(fixtureRoot));
        }
        finally
        {
            DeleteOwnedTempRoot(fixtureRoot, FixtureTempDirectoryName);
        }
    }

    private static async Task<Learn07FixturePaths> MaterializeRollbackFixtureAsync(
        string repoRoot,
        string fixtureRoot)
    {
        var promptToTask = MaterializePromptToTaskRollbackFixture(fixtureRoot);
        var orchestration = await MaterializeOrchestrationRollbackFixtureAsync(
            repoRoot,
            fixtureRoot);
        var skillSelection = MaterializeSkillSelectionRollbackFixture(fixtureRoot);

        return new Learn07FixturePaths(
            PromptToTaskTargetPath: promptToTask.TargetCheckpointPath,
            PromptToTaskRollbackPath: promptToTask.RollbackCheckpointPath,
            OrchestrationTargetPath: orchestration.TargetCheckpointPath,
            OrchestrationTargetManifestPath: orchestration.TargetManifestPath,
            OrchestrationRollbackPath: orchestration.RollbackCheckpointPath,
            OrchestrationRollbackManifestPath: orchestration.RollbackManifestPath,
            SkillSelectionTargetPath: skillSelection.TargetCheckpointPath,
            SkillSelectionRollbackPath: skillSelection.RollbackCheckpointPath);
    }

    private static OverlayFixturePaths MaterializePromptToTaskRollbackFixture(
        string fixtureRoot)
    {
        var writer = new PromptToTaskAdaptiveDpoSignalWriter();
        var trainer = new PromptToTaskAdaptiveTrainer();
        var targetRoot = Path.Combine(fixtureRoot, "prompt-to-task", "target");
        var rollbackRoot = Path.Combine(fixtureRoot, "prompt-to-task", "rollback");
        RequireSuccess(
            writer.Write(new PromptToTaskAdaptiveDpoSignalWriteRequest(
                UserLocalRoot: targetRoot,
                SignalId: "learn-07-prompt-to-task-target-signal",
                SourcePromptLedgerId: "learn-07-prompt-to-task-source",
                RawPromptSha256: Sha256Text("learn-07 prompt-to-task target prompt"),
                ChosenStructuredTaskRef: "structured-task-ref:learn-07-target",
                RejectedStructuredTaskRef: "structured-task-ref:learn-07-rejected",
                FeedbackClass: "rollback_boundary_target_only",
                ProvenanceRefs: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["fixture"] = "learn-07-rollback",
                    ["scope"] = "repo-local-test-fixture"
                })),
            "write PromptToTask target adaptive signal");
        var target = RequireSuccess(
            trainer.TrainUserLocal(new PromptToTaskAdaptiveTrainingRequest(targetRoot)),
            "train PromptToTask target overlay");
        var rollback = RequireSuccess(
            trainer.TrainUserLocal(new PromptToTaskAdaptiveTrainingRequest(rollbackRoot)),
            "train PromptToTask rollback overlay");

        Assert.NotEmpty(target.Checkpoint.TrainedSignalIds);
        Assert.Empty(rollback.Checkpoint.TrainedSignalIds);
        Assert.Empty(rollback.Checkpoint.TaskProfiles);
        return new OverlayFixturePaths(
            target.Report.CheckpointPath,
            rollback.Report.CheckpointPath);
    }

    private static async Task<OrchestrationFixturePaths> MaterializeOrchestrationRollbackFixtureAsync(
        string repoRoot,
        string fixtureRoot)
    {
        var targetPattern = CreateTargetOnlyOrchestrationPattern();
        var rollbackPattern = new OrchestrationReasoningLearnedPattern(
            PatternId: "or-learned-pattern:learn-07-rollback-baseline-boundary",
            ScenarioRef: "learn-07-rollback-baseline-scenario",
            RequiredTaskTypes: ["rollback-baseline-nonmatch"],
            RequiredConstraintSignals: ["rollback_baseline_constraint"],
            RequiredRequirementSignals: ["rollback_baseline_requirement"],
            RequiredDeliverableSignals: ["rollback_baseline_deliverable"],
            RequiredEvidenceSignals: ["rollback_baseline_evidence"],
            PhaseFamilies: ["LEARN-07"],
            GateFamilies: ["rollback-boundary"],
            AssetFamilies: ["orchestration-reasoning"],
            HostCheckSets: ["repo-local-fixture"],
            RiskFlags: ["no_promotion", "no_semantic_quality_claim"],
            NextGranularizationActions: ["parent_review"],
            Confidence: 0.9);

        var target = await MaterializeOrchestrationCheckpointFixtureAsync(
            repoRoot,
            Path.Combine(fixtureRoot, "orchestration-reasoning", "target"),
            "target",
            targetPattern);
        var rollback = await MaterializeOrchestrationCheckpointFixtureAsync(
            repoRoot,
            Path.Combine(fixtureRoot, "orchestration-reasoning", "rollback"),
            "rollback",
            rollbackPattern);

        return new OrchestrationFixturePaths(
            TargetCheckpointPath: target.CheckpointPath,
            TargetManifestPath: target.ManifestPath,
            RollbackCheckpointPath: rollback.CheckpointPath,
            RollbackManifestPath: rollback.ManifestPath);
    }

    private static async Task<OrchestrationCheckpointFixturePaths>
        MaterializeOrchestrationCheckpointFixtureAsync(
            string repoRoot,
            string root,
            string suffix,
            OrchestrationReasoningLearnedPattern pattern)
    {
        Directory.CreateDirectory(root);
        var sourceRoot = Path.Combine(root, "source");
        Directory.CreateDirectory(sourceRoot);
        var payloadPath = Path.Combine(sourceRoot, "payload.jsonl");
        var sourceManifestPath = Path.Combine(sourceRoot, "source-manifest.json");
        var ledgerPath = Path.Combine(sourceRoot, "source-admission-ledger.json");
        var evidencePath = Path.Combine(sourceRoot, "source-admission-evidence.json");
        var pairId = $"learn-07-{suffix}-pair";
        var rowId = $"learn-07-{suffix}-row";
        var sourceId = $"learn-07-{suffix}-source";
        var sourceRef = $"learn-07-source-ref:{suffix}";
        var sourceSha = Sha256Text($"learn-07-source-material:{suffix}");
        var row = CreateOrchestrationPayloadRow(
            pairId,
            rowId,
            sourceId,
            sourceRef,
            sourceSha,
            pattern);
        ArtifactTextWriter.WriteText(
            payloadPath,
            row.ToJsonString(CompactJsonOptions),
            finalNewLine: true);
        var payloadSha = Sha256File(payloadPath);
        WriteJsonNode(
            sourceManifestPath,
            new JsonObject
            {
                ["schema_version"] = 1,
                ["source_kind"] = OrchestrationReasoningSourceAdmission.ProjectInternalSelfAuthored,
                ["payloads_sha256"] = payloadSha,
                ["source_pairs_sha256"] = Sha256Text($"{pairId}:{sourceSha}"),
                ["row_count"] = 1,
                ["raw_private_text_copied"] = false,
                ["technical_execution_leakage"] = false,
                ["external_ai_used"] = false,
                ["training_started"] = false,
                ["repo_root_ref"] = "repo://llm_mvp_core-DpoImplementation/test-owned-learn-07-rollback-fixture"
            });
        var manifestSha = Sha256File(sourceManifestPath);
        WriteJsonNode(
            ledgerPath,
            new JsonObject
            {
                ["schema_version"] = 1,
                ["entries"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["pair_id"] = pairId,
                        ["row_id"] = rowId,
                        ["source_id"] = sourceId,
                        ["source_ref"] = sourceRef,
                        ["source_sha256"] = sourceSha,
                        ["source_kind"] = OrchestrationReasoningSourceAdmission.ProjectInternalSelfAuthored
                    }
                }
            });
        var ledgerSha = Sha256File(ledgerPath);
        WriteJsonNode(
            evidencePath,
            new JsonObject
            {
                ["schema_version"] = 1,
                ["verified_source_kind"] = OrchestrationReasoningSourceAdmission.ProjectInternalSelfAuthored,
                ["payload_sha256"] = payloadSha,
                ["manifest_sha256"] = manifestSha,
                ["ledger_sha256"] = ledgerSha,
                ["row_ids"] = StringArray([rowId]),
                ["source_ids"] = StringArray([sourceId]),
                ["pair_ids"] = StringArray([pairId]),
                ["source_refs"] = StringArray([sourceRef]),
                ["source_sha256s"] = StringArray([sourceSha])
            });

        var materialized = RequireSuccess(
            await new OrchestrationReasoningTrainer().MaterializeAsync(
                payloadPath,
                sourceManifestPath,
                ledgerPath,
                evidencePath),
            $"materialize OrchestrationReasoning {suffix} admitted state");
        var machine = new OrchestrationReasoningMachine();
        RequireSuccess(
            machine.ImportAdmittedState(materialized.AdmittedState),
            $"import OrchestrationReasoning {suffix} admitted state");
        var checkpointPath = Path.Combine(root, "checkpoint.json");
        var manifestPath = Path.Combine(root, "manifest.json");
        RequireSuccess(
            await machine.SaveCheckpointAsync(checkpointPath, manifestPath),
            $"save OrchestrationReasoning {suffix} checkpoint");
        RequireSuccess(
            await OrchestrationReasoningMachine.LoadCheckpointAsync(
                checkpointPath,
                manifestPath),
            $"load OrchestrationReasoning {suffix} checkpoint");
        return new OrchestrationCheckpointFixturePaths(
            checkpointPath,
            manifestPath);
    }

    private static JsonObject CreateOrchestrationPayloadRow(
        string pairId,
        string rowId,
        string sourceId,
        string sourceRef,
        string sourceSha,
        OrchestrationReasoningLearnedPattern pattern)
        => new()
        {
            ["pair_id"] = pairId,
            ["source_row_id"] = rowId,
            ["source_id"] = sourceId,
            ["approved_pair_ref"] = $"approved-ref:{pairId}",
            ["error_code"] = "LEARN-07",
            ["chosen_payload_ref"] = $"chosen-payload-ref:{pairId}",
            ["rejected_payload_ref"] = $"rejected-payload-ref:{pairId}",
            ["chosen_feature_contract_ref"] = $"chosen-feature-ref:{pairId}",
            ["rejected_feature_contract_ref"] = $"rejected-feature-ref:{pairId}",
            ["trainer_input_contract_ref"] = $"trainer-input-ref:{pairId}",
            ["chosen_ref"] = $"chosen-ref:{pairId}",
            ["rejected_ref"] = $"rejected-ref:{pairId}",
            ["source_ref"] = sourceRef,
            ["source_sha256"] = sourceSha,
            ["model_leg"] = "common",
            ["abstraction_layer"] = "universal_reasoning",
            ["raw_private_text_copied"] = false,
            ["technical_execution_leakage"] = false,
            ["external_ai_used"] = false,
            ["training_started"] = false,
            ["dpo_payload_authored"] = true,
            ["dpo_train_ready"] = false,
            ["excluded_technical_execution_refs"] = new JsonArray(),
            ["chosen_scenario_plan_features"] = new JsonObject
            {
                ["intent_signals"] = StringArray(pattern.RequiredConstraintSignals),
                ["reasoning_steps"] = StringArray(pattern.RequiredRequirementSignals),
                ["evidence_expectations"] = StringArray(pattern.RequiredEvidenceSignals),
                ["decision_guards"] = StringArray(pattern.RequiredDeliverableSignals)
            },
            ["rejected_scenario_plan_features"] = new JsonObject
            {
                ["near_miss_signals"] = StringArray(["near_miss_non_rollback_boundary"])
            },
            ["universal_reasoning_pattern"] = "rollback-boundary-fixture",
            ["learned_pattern"] = JsonSerializer.SerializeToNode(pattern, CompactJsonOptions)
        };

    private static OrchestrationReasoningLearnedPattern CreateTargetOnlyOrchestrationPattern()
        => new(
            PatternId: TargetOnlyOrchestrationPatternId,
            ScenarioRef: "learn-07-target-only-scenario",
            RequiredTaskTypes: ["learning-feedback"],
            RequiredConstraintSignals: ["no_promotion"],
            RequiredRequirementSignals: ["rollback_boundary"],
            RequiredDeliverableSignals: ["parent_review"],
            RequiredEvidenceSignals: ["rollback_evidence"],
            PhaseFamilies: ["LEARN-07"],
            GateFamilies: ["rollback-boundary"],
            AssetFamilies: ["orchestration-reasoning"],
            HostCheckSets: ["repo-local-fixture"],
            RiskFlags: ["no_promotion", "no_semantic_quality_claim"],
            NextGranularizationActions: ["parent_review"],
            Confidence: 0.95);

    private static OverlayFixturePaths MaterializeSkillSelectionRollbackFixture(
        string fixtureRoot)
    {
        var writer = new SkillSelectionCorrectionDpoSignalWriter();
        var trainer = new SkillSelectionCorrectionAdaptiveTrainer();
        var targetRoot = Path.Combine(fixtureRoot, "skill-selection", "target");
        var rollbackRoot = Path.Combine(fixtureRoot, "skill-selection", "rollback");
        var targetWrite = RequireSuccess(
            writer.WriteUserLocalSignal(new SkillSelectionCorrectionDpoSignalWriteRequest(
                UserLocalRoot: targetRoot,
                CorrectionId: "learn-07-target-correction",
                ChosenSkillIds: ["rollback-boundary"],
                RejectedSkillIds: ["promotion-ready"],
                CorrectionClass: "rollback_boundary_target_only",
                PromptRef: "prompt-ref:learn-07",
                OutputRef: "output-ref:learn-07",
                AiResponseRef: "ai-response-ref:learn-07",
                SourcePromptLedgerId: "learn-07-skill-selection-source",
                ProvenanceRefs: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["fixture"] = "learn-07-rollback",
                    ["scope"] = "repo-local-test-fixture"
                })),
            "write SkillSelection target correction signal");
        var target = RequireSuccess(
            trainer.TrainUserLocalAsync(new SkillSelectionCorrectionAdaptiveTrainingRequest(
                targetRoot,
                targetWrite.SignalPath)),
            "train SkillSelection target overlay");
        var rollbackSignalPath = Path.Combine(
            rollbackRoot,
            ".xiigen",
            "smart-skills",
            "corrections",
            "adaptive-dpo-signals.jsonl");
        Directory.CreateDirectory(Path.GetDirectoryName(rollbackSignalPath)!);
        ArtifactTextWriter.WriteText(rollbackSignalPath, string.Empty, finalNewLine: false);
        var rollback = RequireSuccess(
            trainer.TrainUserLocalAsync(new SkillSelectionCorrectionAdaptiveTrainingRequest(
                rollbackRoot,
                rollbackSignalPath)),
            "train SkillSelection rollback overlay");

        Assert.NotEmpty(target.Checkpoint.TrainedSignalIds);
        Assert.Empty(rollback.Checkpoint.TrainedSignalIds);
        Assert.Empty(rollback.Checkpoint.CorrectionProfiles);
        return new OverlayFixturePaths(
            target.Report.CheckpointPath,
            rollback.Report.CheckpointPath);
    }

    private static JsonArray StringArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
            array.Add(value);
        return array;
    }

    private static void WriteJsonNode(string path, JsonNode node)
        => ArtifactTextWriter.WriteText(
            path,
            node.ToJsonString(JsonOptions),
            finalNewLine: true);

    private static T RequireSuccess<T>(
        OperationResult<T> result,
        string label)
    {
        Assert.True(
            result.IsSuccess,
            $"{label} failed: {result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }

    private static Learn07Track ProbePromptToTaskRollback(
        string targetCheckpointPath,
        string rollbackCheckpointPath,
        string fixtureRoot)
    {
        var target = new PromptToTaskAdaptiveTrainer().LoadCheckpoint(targetCheckpointPath);
        var rollback = new PromptToTaskAdaptiveTrainer().LoadCheckpoint(rollbackCheckpointPath);
        var targetSignalIds = target.Value?.TrainedSignalIds ?? [];
        var rollbackSignalIds = rollback.Value?.TrainedSignalIds ?? [];
        var absent = rollback.IsSuccess
            && targetSignalIds.Count > 0
            && !targetSignalIds.Any(id => rollbackSignalIds.Contains(id, StringComparer.Ordinal))
            && (rollback.Value?.TaskProfiles.Count ?? -1) == 0;

        return new Learn07Track(
            MachineId: "PromptToTaskMachine",
            Verdict: absent ? "PARTIAL_OVERLAY_ROLLBACK_ONLY" : "FAIL",
            TargetCheckpointPath: ToStablePathRef(targetCheckpointPath, fixtureRoot),
            RollbackCheckpointPath: ToStablePathRef(rollbackCheckpointPath, fixtureRoot),
            ManifestPath: null,
            TargetCheckpointSha256: File.Exists(targetCheckpointPath) ? Sha256File(targetCheckpointPath) : null,
            RollbackCheckpointSha256: File.Exists(rollbackCheckpointPath) ? Sha256File(rollbackCheckpointPath) : null,
            BoundaryUsed: "PromptToTaskAdaptiveTrainer.LoadCheckpoint rollback overlay",
            RollbackLoaded: rollback.IsSuccess,
            RuntimeInvocationSucceeded: false,
            TestOnlyInProcessProbeSucceeded: false,
            TargetOnlyLearnedChangesAbsentAfterRollback: absent,
            TargetOnlyChangeId: string.Join(",", targetSignalIds),
            RollbackObservedStatus: rollback.IsSuccess ? "rollback_overlay_loaded_without_target_signal_ids" : "rollback_overlay_load_failed",
            RollbackObservedOutput: rollback.IsSuccess
                ? $"rollbackTaskProfiles={rollback.Value!.TaskProfiles.Count}; rollbackTrainedSignalIds={rollbackSignalIds.Count}; targetTrainedSignalIds={targetSignalIds.Count}"
                : rollback.ErrorCode,
            SemanticQualityClaimed: rollback.Value?.SemanticQualityClaimed ?? false,
            CommonModelMutated: false,
            Notes: "PARTIAL_OVERLAY_ROLLBACK_ONLY: PromptToTask target and rollback are adaptive overlay checkpoints without a full PromptToTaskMachine checkpoint plus manifest.");
    }

    private static async Task<Learn07Track> ProbeOrchestrationRollbackAsync(
        string targetCheckpointPath,
        string targetManifestPath,
        string rollbackCheckpointPath,
        string rollbackManifestPath,
        string fixtureRoot)
    {
        var targetLoad = await OrchestrationReasoningMachine.LoadCheckpointAsync(targetCheckpointPath, targetManifestPath);
        if (!targetLoad.IsSuccess)
            return FailedOrchestrationTrack(targetCheckpointPath, rollbackCheckpointPath, rollbackManifestPath, fixtureRoot, targetLoad.ErrorCode, targetLoad.ErrorMessage);

        var learnedPattern = targetLoad.Value!.Checkpoint.LearnedState.LearnedPatterns!
            .Single(pattern => pattern.PatternId == TargetOnlyOrchestrationPatternId);
        var targetRequest = BuildMatchingOrchestrationRequest(learnedPattern);
        var targetResponse = targetLoad.Value.Machine.Plan(targetRequest);
        var targetMatched = IsMatched(targetResponse, learnedPattern);

        var tempDir = CreateOwnedTempRoot(ManifestBoundTempDirectoryName);
        try
        {
            var manifest = JsonSerializer.Deserialize<OrchestrationReasoningCheckpointManifest>(
                File.ReadAllText(rollbackManifestPath, Encoding.UTF8),
                JsonOptions)!;
            var manifestBoundCheckpointPath = Path.Combine(tempDir, manifest.CheckpointRef);
            var manifestBoundManifestPath = Path.Combine(tempDir, manifest.ManifestRef);
            File.Copy(rollbackCheckpointPath, manifestBoundCheckpointPath, overwrite: true);
            File.Copy(rollbackManifestPath, manifestBoundManifestPath, overwrite: true);
            File.Copy(
                OrchestrationReasoningSourceAdmission.GetProofPath(rollbackCheckpointPath),
                OrchestrationReasoningSourceAdmission.GetProofPath(manifestBoundCheckpointPath),
                overwrite: true);

            var rollbackLoad = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                manifestBoundCheckpointPath,
                manifestBoundManifestPath);
            if (!rollbackLoad.IsSuccess)
                return FailedOrchestrationTrack(targetCheckpointPath, rollbackCheckpointPath, rollbackManifestPath, fixtureRoot, rollbackLoad.ErrorCode, rollbackLoad.ErrorMessage);

            var rollbackPatternIds = rollbackLoad.Value!.Checkpoint.LearnedState.LearnedPatterns?
                .Select(pattern => pattern.PatternId)
                .ToHashSet(StringComparer.Ordinal) ?? [];
            var rollbackResponse = rollbackLoad.Value.Machine.Plan(targetRequest);
            var targetOnlyAbsent = targetMatched
                && rollbackResponse.IsSuccess
                && !rollbackPatternIds.Contains(TargetOnlyOrchestrationPatternId)
                && !IsMatched(rollbackResponse, learnedPattern);

            return new Learn07Track(
                MachineId: "OrchestrationReasoningMachine",
                Verdict: targetOnlyAbsent ? "PASS_PUBLIC_MACHINE_BOUNDARY_ROLLBACK" : "FAIL",
                TargetCheckpointPath: ToStablePathRef(targetCheckpointPath, fixtureRoot),
                RollbackCheckpointPath: ToStablePathRef(rollbackCheckpointPath, fixtureRoot),
                ManifestPath: ToStablePathRef(rollbackManifestPath, fixtureRoot),
                TargetCheckpointSha256: Sha256File(targetCheckpointPath),
                RollbackCheckpointSha256: Sha256File(rollbackCheckpointPath),
                BoundaryUsed: "OrchestrationReasoningMachine.LoadCheckpointAsync + OrchestrationReasoningMachine.Plan rollback via manifest-bound temp copy",
                RollbackLoaded: rollbackLoad.IsSuccess,
                RuntimeInvocationSucceeded: false,
                TestOnlyInProcessProbeSucceeded: rollbackResponse.IsSuccess,
                TargetOnlyLearnedChangesAbsentAfterRollback: targetOnlyAbsent,
                TargetOnlyChangeId: TargetOnlyOrchestrationPatternId,
                RollbackObservedStatus: rollbackResponse.IsSuccess ? rollbackResponse.Value!.ContractStatus : "plan_failed",
                RollbackObservedOutput: rollbackResponse.IsSuccess
                    ? $"selected={string.Join(",", rollbackResponse.Value!.ScenarioPlan.SelectedLearnedPatterns)}; targetPatternPresent={rollbackPatternIds.Contains(TargetOnlyOrchestrationPatternId)}; targetMatchedBeforeRollback={targetMatched}"
                    : rollbackResponse.ErrorCode,
                SemanticQualityClaimed: rollbackLoad.Value.Checkpoint.SemanticQualityClaimed || rollbackLoad.Value.Manifest.SemanticQualityClaimed,
                CommonModelMutated: false,
                Notes: $"Rollback manifest checkpointRef is `{manifest.CheckpointRef}` while approval packet checkpoint alias is `{Path.GetFileName(rollbackCheckpointPath)}`; public boundary was exercised using a temp manifest-bound copy without rewriting the named rollback assets.");
        }
        finally
        {
            DeleteOwnedTempRoot(tempDir, ManifestBoundTempDirectoryName);
        }
    }

    private static Learn07Track ProbeSkillSelectionRollback(
        string targetCheckpointPath,
        string rollbackCheckpointPath,
        string fixtureRoot)
    {
        var target = new SkillSelectionCorrectionAdaptiveTrainer().LoadCheckpoint(targetCheckpointPath);
        var rollback = new SkillSelectionCorrectionAdaptiveTrainer().LoadCheckpoint(rollbackCheckpointPath);
        var targetSignalIds = target.Value?.TrainedSignalIds ?? [];
        var rollbackSignalIds = rollback.Value?.TrainedSignalIds ?? [];
        var absent = rollback.IsSuccess
            && targetSignalIds.Count > 0
            && !targetSignalIds.Any(id => rollbackSignalIds.Contains(id, StringComparer.Ordinal))
            && (rollback.Value?.CorrectionProfiles.Count ?? -1) == 0;

        return new Learn07Track(
            MachineId: "SkillSelectionMachine",
            Verdict: absent ? "PARTIAL_OVERLAY_ROLLBACK_ONLY" : "FAIL",
            TargetCheckpointPath: ToStablePathRef(targetCheckpointPath, fixtureRoot),
            RollbackCheckpointPath: ToStablePathRef(rollbackCheckpointPath, fixtureRoot),
            ManifestPath: null,
            TargetCheckpointSha256: File.Exists(targetCheckpointPath) ? Sha256File(targetCheckpointPath) : null,
            RollbackCheckpointSha256: File.Exists(rollbackCheckpointPath) ? Sha256File(rollbackCheckpointPath) : null,
            BoundaryUsed: "SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint rollback overlay",
            RollbackLoaded: rollback.IsSuccess,
            RuntimeInvocationSucceeded: false,
            TestOnlyInProcessProbeSucceeded: false,
            TargetOnlyLearnedChangesAbsentAfterRollback: absent,
            TargetOnlyChangeId: string.Join(",", targetSignalIds),
            RollbackObservedStatus: rollback.IsSuccess ? "rollback_overlay_loaded_without_target_signal_ids" : "rollback_overlay_load_failed",
            RollbackObservedOutput: rollback.IsSuccess
                ? $"rollbackCorrectionProfiles={rollback.Value!.CorrectionProfiles.Count}; rollbackTrainedSignalIds={rollbackSignalIds.Count}; targetTrainedSignalIds={targetSignalIds.Count}"
                : rollback.ErrorCode,
            SemanticQualityClaimed: rollback.Value?.SemanticQualityClaimed ?? false,
            CommonModelMutated: rollback.Value?.CommonCheckpointMutationClaimed ?? false,
            Notes: "PARTIAL_OVERLAY_ROLLBACK_ONLY: SkillSelection target and rollback are correction adaptive overlay checkpoints without a full SkillSelectionMachine checkpoint plus manifest.");
    }

    private static Learn07Track FailedOrchestrationTrack(
        string targetCheckpointPath,
        string rollbackCheckpointPath,
        string manifestPath,
        string fixtureRoot,
        string? errorCode,
        string? errorMessage)
        => new(
            MachineId: "OrchestrationReasoningMachine",
            Verdict: "FAIL",
            TargetCheckpointPath: ToStablePathRef(targetCheckpointPath, fixtureRoot),
            RollbackCheckpointPath: ToStablePathRef(rollbackCheckpointPath, fixtureRoot),
            ManifestPath: ToStablePathRef(manifestPath, fixtureRoot),
            TargetCheckpointSha256: File.Exists(targetCheckpointPath) ? Sha256File(targetCheckpointPath) : null,
            RollbackCheckpointSha256: File.Exists(rollbackCheckpointPath) ? Sha256File(rollbackCheckpointPath) : null,
            BoundaryUsed: "OrchestrationReasoningMachine.LoadCheckpointAsync",
            RollbackLoaded: false,
            RuntimeInvocationSucceeded: false,
            TestOnlyInProcessProbeSucceeded: false,
            TargetOnlyLearnedChangesAbsentAfterRollback: false,
            TargetOnlyChangeId: TargetOnlyOrchestrationPatternId,
            RollbackObservedStatus: "rollback_public_machine_load_failed",
            RollbackObservedOutput: $"{errorCode}: {errorMessage}",
            SemanticQualityClaimed: false,
            CommonModelMutated: false,
            Notes: "Public machine rollback load failed.");

    private static OrchestrationReasoningRequest BuildMatchingOrchestrationRequest(
        OrchestrationReasoningLearnedPattern pattern)
    {
        var prompt = "redacted LEARN-07 rollback probe for local runtime token quality boundary";
        var task = new StructuredTaskContract(
            PromptId: "learn-07-orchestration-rollback",
            RawPromptSha256: Sha256Text(prompt),
            UserLanguage: "en",
            TaskType: pattern.RequiredTaskTypes.Single(),
            ExplicitUserRequirements: pattern.RequiredRequirementSignals,
            Constraints: pattern.RequiredConstraintSignals,
            Deliverables: pattern.RequiredDeliverableSignals,
            ForbiddenActions: ["no_semantic_quality_claim", "no_promotion", "no_locator_update"],
            RequiredHosts: [],
            EvidenceNeeds: pattern.RequiredEvidenceSignals,
            SourceProvenance: new Dictionary<string, string>
            {
                ["boundary"] = "learn-07-public-machine-rollback",
                ["raw_prompt_redacted"] = "true"
            },
            SplitId: "learn-07-rollback-probe");

        return new OrchestrationReasoningRequest(
            RequestId: "learn-07-orchestration-rollback",
            Task: task,
            GuideDerivedStateRefs: [],
            CorpusBundleIds: [],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["rollback"] = "true",
                ["semantic_quality_claimed"] = "false"
            });
    }

    private static bool IsMatched(
        OperationResult<OrchestrationReasoningResponse> response,
        OrchestrationReasoningLearnedPattern pattern)
        => response.IsSuccess
            && response.Value!.ContractStatus == OrchestrationReasoningMachine.LearnedPatternStatus
            && response.Value.ScenarioPlan.SelectedLearnedPatterns.Contains(pattern.PatternId, StringComparer.Ordinal);

    private static bool IsStrictlyAccepted(
        IReadOnlyList<Learn07Track> tracks,
        Learn07SafetyEvidence safetyEvidence)
    {
        if (tracks.Count != 3)
            return false;

        var promptToTaskTracks = tracks
            .Where(track => track.MachineId == "PromptToTaskMachine")
            .ToArray();
        var orchestrationTracks = tracks
            .Where(track => track.MachineId == "OrchestrationReasoningMachine")
            .ToArray();
        var skillSelectionTracks = tracks
            .Where(track => track.MachineId == "SkillSelectionMachine")
            .ToArray();
        if (promptToTaskTracks.Length != 1
            || orchestrationTracks.Length != 1
            || skillSelectionTracks.Length != 1)
        {
            return false;
        }

        var promptToTask = promptToTaskTracks[0];
        var orchestration = orchestrationTracks[0];
        var skillSelection = skillSelectionTracks[0];
        return tracks.All(track => track.RollbackLoaded)
            && tracks.All(track => track.TargetOnlyLearnedChangesAbsentAfterRollback)
            && promptToTask.Verdict == "PARTIAL_OVERLAY_ROLLBACK_ONLY"
            && orchestration.Verdict == "PASS_PUBLIC_MACHINE_BOUNDARY_ROLLBACK"
            && skillSelection.Verdict == "PARTIAL_OVERLAY_ROLLBACK_ONLY"
            && tracks.All(track => !track.RuntimeInvocationSucceeded)
            && orchestration.TestOnlyInProcessProbeSucceeded
            && !promptToTask.TestOnlyInProcessProbeSucceeded
            && !skillSelection.TestOnlyInProcessProbeSucceeded
            && tracks.All(track => !track.SemanticQualityClaimed)
            && tracks.All(track => !track.CommonModelMutated)
            && TracksUseOnlyStableTestTempRefs(tracks)
            && !safetyEvidence.TargetPromotionPerformed
            && !safetyEvidence.LocatorUpdated
            && safetyEvidence.OuterFixtureRootDeletedBeforeArtifactWrite;
    }

    private static bool TracksUseOnlyStableTestTempRefs(IReadOnlyList<Learn07Track> tracks)
        => tracks.All(track => IsStableTestTempRef(track.TargetCheckpointPath)
            && IsStableTestTempRef(track.RollbackCheckpointPath)
            && (track.ManifestPath is null || IsStableTestTempRef(track.ManifestPath)));

    private static void WriteRollbackMarkdown(string path, Learn07Report report)
    {
        var lines = new List<string>
        {
            "# LEARN-07 rollback report",
            "",
            $"Verdict: {report.Verdict}",
            "",
            "## Commands"
        };
        lines.AddRange(report.Commands.Select(command => $"- `{command}`"));
        lines.Add("");
        lines.Add("## Command results");
        lines.AddRange(report.CommandResults.Select(result => $"- `{result.Command}` -> {result.Result}; {result.Summary}"));
        lines.Add("");
        lines.Add("## Track verdicts");
        foreach (var track in report.Tracks)
        {
            lines.Add($"- {track.MachineId}: {track.Verdict}");
            lines.Add($"  Boundary: `{track.BoundaryUsed}`");
            lines.Add($"  Target: `{track.TargetCheckpointPath}`");
            lines.Add($"  Rollback: `{track.RollbackCheckpointPath}`");
            lines.Add($"  Target-only change: `{track.TargetOnlyChangeId}`");
            lines.Add($"  Rollback loaded: {track.RollbackLoaded}; target-only changes absent: {track.TargetOnlyLearnedChangesAbsentAfterRollback}");
            lines.Add($"  Runtime invocation: {track.RuntimeInvocationSucceeded}; test-only in-process probe: {track.TestOnlyInProcessProbeSucceeded}");
            lines.Add($"  Observed: {track.RollbackObservedStatus}; {track.RollbackObservedOutput}");
            lines.Add($"  Limitation: {track.Notes}");
        }

        lines.Add("");
        lines.Add("## Safety evidence");
        lines.Add($"- Target promotion performed: {report.SafetyEvidence.TargetPromotionPerformed}");
        lines.Add($"- Locator updated: {report.SafetyEvidence.LocatorUpdated}");
        lines.Add($"- Outer fixture root deleted before artifact write: {report.SafetyEvidence.OuterFixtureRootDeletedBeforeArtifactWrite}");
        lines.Add("");
        lines.Add("## What this proves");
        lines.AddRange(report.WhatThisProves.Select(item => $"- {item}"));
        lines.Add("");
        lines.Add("## What this does not prove");
        lines.AddRange(report.WhatThisDoesNotProve.Select(item => $"- {item}"));
        lines.Add("");
        lines.Add("## Next independent track");
        lines.Add($"- {report.NextIndependentTrack}");
        lines.Add("");
        lines.Add("Rollback proves recovery only. It does not prove the target should be promoted.");
        ArtifactTextWriter.WriteText(path, string.Join("\n", lines), finalNewLine: true);
    }

    private static void WriteParentReviewMarkdown(string path, ParentReview review)
    {
        var lines = new List<string>
        {
            "# Parent review LEARN-07",
            "",
            $"Verdict: {review.Verdict}",
            "",
            "## Findings"
        };
        lines.AddRange(review.Findings.Select(item => $"- {item}"));
        lines.Add("");
        lines.Add("## Track verdicts");
        lines.AddRange(review.Tracks.Select(track => $"- {track.MachineId}: {track.Verdict} via `{track.BoundaryUsed}`"));
        lines.Add("");
        lines.Add("## Safety evidence");
        lines.Add($"- Target promotion performed: {review.SafetyEvidence.TargetPromotionPerformed}");
        lines.Add($"- Locator updated: {review.SafetyEvidence.LocatorUpdated}");
        lines.Add($"- Outer fixture root deleted before artifact write: {review.SafetyEvidence.OuterFixtureRootDeletedBeforeArtifactWrite}");
        lines.Add("");
        lines.Add("## Next phase");
        lines.Add($"- {review.NextPhase}");
        ArtifactTextWriter.WriteText(path, string.Join("\n", lines), finalNewLine: true);
    }

    private static string UpdateExecutionState(
        string repoRoot,
        string artifactDir,
        IReadOnlyList<Learn07Track> tracks,
        Learn07SafetyEvidence safetyEvidence,
        bool accepted)
    {
        var statePath = Path.Combine(repoRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
        var statePatchRef = ToRepoPathRef(
            ExtensionExecutionStatePatchWriter.DefaultPatchPath(artifactDir, PhaseId),
            repoRoot);
        return ExtensionExecutionStatePatchWriter.WritePatch(
            statePath,
            artifactDir,
            PhaseId,
            new Dictionary<string, object?>
            {
                ["updated_at_local"] = DateTimeOffset.Now.ToString("O"),
                ["current_phase"] = PhaseId,
                ["phase_status"] = accepted
                    ? "ACCEPTED_FOR_PARENT_REVIEW_WITH_LIMITATIONS_PENDING_PHASE_COMMIT"
                    : "FAIL",
                ["phase_scope"] = "Rollback verifier artifacts only; OrchestrationReasoning used public machine LoadCheckpointAsync + Plan; PromptToTask and SkillSelection are partial overlay rollback checks only.",
                ["implementation_authorized_scope"] = "LEARN-07 rollback verifier only; commit/push forbidden by current work order",
                ["eval_allowed"] = true,
                ["eval_started"] = true,
                ["semantic_quality_claimed"] = tracks.Any(track => track.SemanticQualityClaimed),
                ["learnability_claimed"] = false,
                ["runtime_allowed"] = false,
                ["runtime_launched"] = false,
                ["training_allowed"] = false,
                ["training_started"] = false,
                ["checkpoint_mutation_allowed"] = false,
                ["checkpoint_mutation_started"] = false,
                ["external_ai_allowed"] = false,
                ["external_ai_used"] = false,
                ["git_authority_seen"] = "commit/push forbidden by current LEARN-07 work order; pending commit boundary recorded only",
                ["git_commands_run"] = new[] { "git branch --show-current", "git status --short", Command, Command },
                ["git_write_or_ref_commands_run"] = Array.Empty<string>(),
                ["git_directives_emitted"] = Array.Empty<string>(),
                ["operator_input_required"] = false,
                ["continuation_mode"] = "parent_review_then_phase_commit_boundary_before_final_execution_review_preparation",
                ["artifacts"] = new[]
                {
                    "Tests/Learn07RollbackRunnerTests.cs",
                    "artifacts/extension-runtime-token-quality-learnability/rollback-report.json",
                    "artifacts/extension-runtime-token-quality-learnability/rollback-report.md",
                    "artifacts/extension-runtime-token-quality-learnability/learn-07-validation.json",
                    "artifacts/extension-runtime-token-quality-learnability/parent-review-LEARN-07.json",
                    "artifacts/extension-runtime-token-quality-learnability/parent-review-LEARN-07.md",
                    statePatchRef
                },
                ["learn_06_parent_commit"] = new
                {
                    phase_id = "LEARN-06",
                    committed_and_pushed_by_parent = true,
                    commit = "ee08f7ce",
                    note = "Corrected from stale LEARN-06 pending phase commit state per LEARN-07 work order."
                },
                ["next_recommended_phase"] = new
                {
                    phase_id = "FINAL-EXECUTION-REVIEW-PREP",
                    phase_name = "final execution review preparation",
                    reason = "The active plan does not name a concrete phase after LEARN-07 rollback verifier; prepare final execution review without claiming completion."
                },
                ["probe_results"] = new
                {
                    prompt_to_task_verdict = tracks.Single(track => track.MachineId == "PromptToTaskMachine").Verdict,
                    orchestration_reasoning_verdict = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").Verdict,
                    skill_selection_verdict = tracks.Single(track => track.MachineId == "SkillSelectionMachine").Verdict,
                    focused_test_passed = accepted,
                    common_model_mutated = tracks.Any(track => track.CommonModelMutated),
                    runtime_invocation_succeeded = tracks.Any(track => track.RuntimeInvocationSucceeded),
                    test_only_in_process_orchestration_probe_succeeded = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").TestOnlyInProcessProbeSucceeded,
                    outer_fixture_root_deleted_before_artifact_write = safetyEvidence.OuterFixtureRootDeletedBeforeArtifactWrite,
                    target_promotion_performed = safetyEvidence.TargetPromotionPerformed,
                    locator_updated = safetyEvidence.LocatorUpdated,
                    semantic_quality_claimed = tracks.Any(track => track.SemanticQualityClaimed),
                    learnability_claimed = false,
                    rollback_claimed = accepted,
                    promotion_ready_claimed = false
                },
                ["parent_review"] = new
                {
                    review_id = "parent-review-LEARN-07",
                    status = accepted ? "ACCEPTED_FOR_PHASE_COMMIT_WITH_LIMITATIONS" : "FAIL",
                    artifacts = new[]
                    {
                        "artifacts/extension-runtime-token-quality-learnability/parent-review-LEARN-07.md",
                        "artifacts/extension-runtime-token-quality-learnability/parent-review-LEARN-07.json"
                    },
                    note = accepted
                        ? "Accepted LEARN-07 for phase commit with honest limitations: OrchestrationReasoning public machine rollback passed; PromptToTask and SkillSelection are partial overlay rollback only; no semantic quality, learnability, promotion, locator update, common mutation, runtime, or external AI claim."
                        : "LEARN-07 strict acceptance failed; phase-commit readiness is not claimed."
                },
                ["pending_commit_boundary"] = new
                {
                    pending = accepted,
                    phase_id = PhaseId,
                    commit_message = "phase LEARN-07: record rollback verifier artifacts",
                    commit_push_forbidden_by_current_work_order = true,
                    files_to_stage = new[]
                    {
                        "Tests/Learn07RollbackRunnerTests.cs",
                        "artifacts/extension-runtime-token-quality-learnability/rollback-report.json",
                        "artifacts/extension-runtime-token-quality-learnability/rollback-report.md",
                        "artifacts/extension-runtime-token-quality-learnability/learn-07-validation.json",
                        "artifacts/extension-runtime-token-quality-learnability/parent-review-LEARN-07.json",
                        "artifacts/extension-runtime-token-quality-learnability/parent-review-LEARN-07.md",
                        statePatchRef
                    },
                    limitations = new[]
                    {
                        "OrchestrationReasoning rollback proves recovery only",
                        "PromptToTask partial overlay rollback only",
                        "SkillSelection partial overlay rollback only",
                        "no semantic quality claim",
                        "no full PromptToTaskMachine rollback",
                        "no full SkillSelectionMachine rollback",
                        "no target promotion",
                        "no locator update"
                    }
                },
                ["evidence_limitations"] = new
                {
                    fresh_load_loadability_only = true,
                    prompt_to_task_full_machine_fresh_load_claimed = false,
                    skill_selection_full_machine_fresh_load_claimed = false,
                    orchestration_reasoning_public_machine_fresh_load_claimed = true,
                    semantic_quality_claimed = tracks.Any(track => track.SemanticQualityClaimed),
                    learnability_claimed = false,
                    held_out_source_disjoint_eval_claimed = true,
                    ablation_claimed = true,
                    rollback_verification_claimed = accepted,
                    prompt_to_task_full_machine_rollback_claimed = false,
                    skill_selection_full_machine_rollback_claimed = false,
                    prompt_to_task_overlay_rollback_only = true,
                    skill_selection_overlay_rollback_only = true,
                    promotion_readiness_claimed = false,
                    common_model_readiness_claimed = false
                }
            },
            report: null,
            validation: null,
            JsonOptions,
            sourceStateRef: "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
    }

    private static void WriteJson<T>(string path, T value)
        => ArtifactTextWriter.WriteJson(path, value, JsonOptions, finalNewLine: true);

    private static string Sha256File(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string ToStablePathRef(string path, string? fixtureRoot = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        var fullPath = Path.GetFullPath(path);
        if (!string.IsNullOrWhiteSpace(fixtureRoot))
        {
            var fullFixtureRoot = Path.GetFullPath(fixtureRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fixturePrefix = fullFixtureRoot + Path.DirectorySeparatorChar;
            if (fullPath.Equals(fullFixtureRoot, StringComparison.OrdinalIgnoreCase))
                return TestTempRootRef;
            if (fullPath.StartsWith(fixturePrefix, StringComparison.OrdinalIgnoreCase))
                return $"{TestTempRootRef}/{Path.GetRelativePath(fullFixtureRoot, fullPath).Replace(Path.DirectorySeparatorChar, '/')}";
        }

        var repoRoot = Path.GetFullPath(FindRepoRoot())
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var repoPrefix = repoRoot + Path.DirectorySeparatorChar;
        if (fullPath.StartsWith(repoPrefix, StringComparison.OrdinalIgnoreCase))
            return Path.GetRelativePath(repoRoot, fullPath)
                .Replace(Path.DirectorySeparatorChar, '/');

        var tempRoot = Path.GetFullPath(Path.GetTempPath())
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var tempPrefix = tempRoot + Path.DirectorySeparatorChar;
        if (fullPath.StartsWith(tempPrefix, StringComparison.OrdinalIgnoreCase))
            return $"{TestTempRootRef}/{Path.GetFileName(fullPath)}";

        return Path.GetFileName(fullPath);
    }

    private static string ToRepoPathRef(string path, string repoRoot)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRepoRoot = Path.GetFullPath(repoRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var repoPrefix = fullRepoRoot + Path.DirectorySeparatorChar;
        Assert.True(
            fullPath.StartsWith(repoPrefix, StringComparison.OrdinalIgnoreCase),
            $"Expected path under repo root: {path}");
        return Path.GetRelativePath(fullRepoRoot, fullPath)
            .Replace(Path.DirectorySeparatorChar, '/');
    }

    private static bool IsStableTestTempRef(string pathRef)
        => pathRef.StartsWith($"{TestTempRootRef}/", StringComparison.Ordinal);

    private static string CreateOwnedTempRoot(string directoryName)
    {
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            directoryName,
            Guid.NewGuid().ToString("N")));
        ValidateOwnedTempRoot(root, directoryName);
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteOwnedTempRoot(string root, string directoryName)
    {
        ValidateOwnedTempRoot(root, directoryName);
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
        DeleteOwnedTempOwnerRootIfEmpty(directoryName);
    }

    private static void ValidateOwnedTempRoot(string root, string directoryName)
    {
        var fullRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var ownerRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), directoryName))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var ownerPrefix = ownerRoot + Path.DirectorySeparatorChar;
        if (!fullRoot.StartsWith(ownerPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Refusing to use temp root outside owned test directory: {root}");
        if (fullRoot.Equals(ownerRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Refusing to use the temp owner directory itself as a fixture root: {root}");
        var leaf = Path.GetFileName(fullRoot);
        if (leaf.Length != 32 || leaf.Any(character => !Uri.IsHexDigit(character)))
            throw new InvalidOperationException($"Refusing to use a temp fixture root without a 32-hex-character leaf: {root}");
    }

    private static void DeleteOwnedTempOwnerRootIfEmpty(string directoryName)
    {
        var ownerRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), directoryName))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var tempRoot = Path.GetFullPath(Path.GetTempPath())
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!Path.GetFileName(ownerRoot).Equals(directoryName, StringComparison.Ordinal))
            throw new InvalidOperationException($"Refusing to delete an unexpected temp owner root: {ownerRoot}");
        if (!ownerRoot.StartsWith(tempRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Refusing to delete temp owner root outside temp directory: {ownerRoot}");
        if (Directory.Exists(ownerRoot) && !Directory.EnumerateFileSystemEntries(ownerRoot).Any())
            Directory.Delete(ownerRoot, recursive: false);
    }

    private static void AssertNoPrivatePathsInGeneratedLearn07Outputs(
        string repoRoot,
        string fixtureRoot,
        string artifactDir,
        string statePatchPath)
    {
        var generatedPaths = new[]
        {
            Path.Combine(artifactDir, "rollback-report.json"),
            Path.Combine(artifactDir, "rollback-report.md"),
            Path.Combine(artifactDir, "learn-07-validation.json"),
            Path.Combine(artifactDir, "parent-review-LEARN-07.json"),
            Path.Combine(artifactDir, "parent-review-LEARN-07.md"),
            statePatchPath
        };
        var forbiddenFragments = new[]
        {
            Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            Path.GetFullPath(repoRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            Path.GetFullPath(fixtureRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            "C:\\Users",
            "C:/Users",
            "C:\\Projects",
            "C:/Projects",
            "artifacts/extension-runtime-token-quality-learnability/learn-07-rollback-fixtures",
            "artifacts\\extension-runtime-token-quality-learnability\\learn-07-rollback-fixtures"
        };

        foreach (var generatedPath in generatedPaths)
        {
            Assert.True(File.Exists(generatedPath), $"Expected generated Learn07 artifact: {generatedPath}");
            var text = File.ReadAllText(generatedPath, Encoding.UTF8);
            foreach (var forbidden in forbiddenFragments)
            {
                Assert.DoesNotContain(forbidden, text, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(forbidden.Replace('\\', '/'), text, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(forbidden.Replace('/', '\\'), text, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private static void AssertFinalLearn07Artifacts(string artifactDir, string statePatchPath)
    {
        using var reportDocument = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(artifactDir, "rollback-report.json"),
            Encoding.UTF8));
        var report = reportDocument.RootElement;
        Assert.Equal("PASS_WITH_LIMITATIONS", report.GetProperty("verdict").GetString());
        var commandResult = Assert.Single(report.GetProperty("commandResults").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, commandResult.GetProperty("exitCode").ValueKind);
        Assert.Equal("IN_PROCESS_PROBES_COMPLETED", commandResult.GetProperty("result").GetString());

        var tracks = report.GetProperty("tracks").EnumerateArray().ToArray();
        Assert.Equal(3, tracks.Length);
        Assert.All(tracks, track =>
        {
            Assert.True(track.GetProperty("rollbackLoaded").GetBoolean());
            Assert.False(track.GetProperty("runtimeInvocationSucceeded").GetBoolean());
            Assert.True(track.GetProperty("targetOnlyLearnedChangesAbsentAfterRollback").GetBoolean());
            Assert.False(track.GetProperty("semanticQualityClaimed").GetBoolean());
            Assert.False(track.GetProperty("commonModelMutated").GetBoolean());
            Assert.StartsWith(TestTempRootRef + "/", track.GetProperty("targetCheckpointPath").GetString(), StringComparison.Ordinal);
            Assert.StartsWith(TestTempRootRef + "/", track.GetProperty("rollbackCheckpointPath").GetString(), StringComparison.Ordinal);
            var manifestPath = track.GetProperty("manifestPath");
            if (manifestPath.ValueKind != JsonValueKind.Null)
                Assert.StartsWith(TestTempRootRef + "/", manifestPath.GetString(), StringComparison.Ordinal);
        });

        var promptToTask = tracks.Single(track => track.GetProperty("machineId").GetString() == "PromptToTaskMachine");
        var orchestration = tracks.Single(track => track.GetProperty("machineId").GetString() == "OrchestrationReasoningMachine");
        var skillSelection = tracks.Single(track => track.GetProperty("machineId").GetString() == "SkillSelectionMachine");
        Assert.Equal("PARTIAL_OVERLAY_ROLLBACK_ONLY", promptToTask.GetProperty("verdict").GetString());
        Assert.Equal("PASS_PUBLIC_MACHINE_BOUNDARY_ROLLBACK", orchestration.GetProperty("verdict").GetString());
        Assert.Equal("PARTIAL_OVERLAY_ROLLBACK_ONLY", skillSelection.GetProperty("verdict").GetString());
        Assert.False(promptToTask.GetProperty("testOnlyInProcessProbeSucceeded").GetBoolean());
        Assert.True(orchestration.GetProperty("testOnlyInProcessProbeSucceeded").GetBoolean());
        Assert.False(skillSelection.GetProperty("testOnlyInProcessProbeSucceeded").GetBoolean());

        var safetyEvidence = report.GetProperty("safetyEvidence");
        Assert.False(safetyEvidence.GetProperty("targetPromotionPerformed").GetBoolean());
        Assert.False(safetyEvidence.GetProperty("locatorUpdated").GetBoolean());
        Assert.True(safetyEvidence.GetProperty("outerFixtureRootDeletedBeforeArtifactWrite").GetBoolean());

        using var validationDocument = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(artifactDir, "learn-07-validation.json"),
            Encoding.UTF8));
        var validation = validationDocument.RootElement;
        Assert.Equal("ACCEPTED_FOR_PARENT_REVIEW_WITH_LIMITATIONS", validation.GetProperty("verdict").GetString());
        Assert.True(validation.GetProperty("acceptedForParentReview").GetBoolean());
        var checks = validation.GetProperty("checks");
        Assert.False(checks.GetProperty("runtime_invocation_succeeded").GetBoolean());
        Assert.True(checks.GetProperty("test_only_in_process_orchestration_probe_succeeded").GetBoolean());
        Assert.True(checks.GetProperty("test_only_in_process_probe_limited_to_orchestration").GetBoolean());
        Assert.True(checks.GetProperty("fixture_artifacts_under_test_temp_root").GetBoolean());
        Assert.True(checks.GetProperty("outer_fixture_root_deleted_before_artifact_write").GetBoolean());
        Assert.False(checks.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(checks.GetProperty("common_model_mutated").GetBoolean());
        Assert.False(checks.GetProperty("target_promotion_or_locator_update").GetBoolean());

        using var parentReviewDocument = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(artifactDir, "parent-review-LEARN-07.json"),
            Encoding.UTF8));
        var parentReview = parentReviewDocument.RootElement;
        Assert.Equal("ACCEPTED_FOR_PHASE_COMMIT_WITH_LIMITATIONS", parentReview.GetProperty("verdict").GetString());
        Assert.True(parentReview.GetProperty("accepted").GetBoolean());
        Assert.True(parentReview.GetProperty("safetyEvidence").GetProperty("outerFixtureRootDeletedBeforeArtifactWrite").GetBoolean());

        using var statePatchDocument = JsonDocument.Parse(File.ReadAllText(statePatchPath, Encoding.UTF8));
        var patchFields = statePatchDocument.RootElement.GetProperty("patch_fields");
        Assert.Equal("ACCEPTED_FOR_PARENT_REVIEW_WITH_LIMITATIONS_PENDING_PHASE_COMMIT", patchFields.GetProperty("phase_status").GetString());
        Assert.False(patchFields.GetProperty("runtime_allowed").GetBoolean());
        Assert.False(patchFields.GetProperty("runtime_launched").GetBoolean());
        var probeResults = patchFields.GetProperty("probe_results");
        Assert.True(probeResults.GetProperty("focused_test_passed").GetBoolean());
        Assert.False(probeResults.GetProperty("runtime_invocation_succeeded").GetBoolean());
        Assert.True(probeResults.GetProperty("test_only_in_process_orchestration_probe_succeeded").GetBoolean());
        Assert.True(probeResults.GetProperty("outer_fixture_root_deleted_before_artifact_write").GetBoolean());
        Assert.False(probeResults.GetProperty("target_promotion_performed").GetBoolean());
        Assert.False(probeResults.GetProperty("locator_updated").GetBoolean());
        Assert.False(probeResults.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(probeResults.GetProperty("common_model_mutated").GetBoolean());
        Assert.True(probeResults.GetProperty("rollback_claimed").GetBoolean());
        Assert.False(probeResults.GetProperty("promotion_ready_claimed").GetBoolean());
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "AGENTS.md"))
                && Directory.Exists(Path.Combine(current.FullName, "artifacts")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private sealed record Learn07FixturePaths(
        string PromptToTaskTargetPath,
        string PromptToTaskRollbackPath,
        string OrchestrationTargetPath,
        string OrchestrationTargetManifestPath,
        string OrchestrationRollbackPath,
        string OrchestrationRollbackManifestPath,
        string SkillSelectionTargetPath,
        string SkillSelectionRollbackPath);

    private sealed record OverlayFixturePaths(
        string TargetCheckpointPath,
        string RollbackCheckpointPath);

    private sealed record OrchestrationFixturePaths(
        string TargetCheckpointPath,
        string TargetManifestPath,
        string RollbackCheckpointPath,
        string RollbackManifestPath);

    private sealed record OrchestrationCheckpointFixturePaths(
        string CheckpointPath,
        string ManifestPath);

    private sealed record Learn07Report(
        string Schema,
        string PhaseId,
        string Verdict,
        DateTimeOffset GeneratedAtUtc,
        IReadOnlyList<string> Commands,
        IReadOnlyList<Learn07CommandResult> CommandResults,
        IReadOnlyList<Learn07Track> Tracks,
        Learn07SafetyEvidence SafetyEvidence,
        IReadOnlyList<string> WhatThisProves,
        IReadOnlyList<string> WhatThisDoesNotProve,
        string NextIndependentTrack);

    private sealed record Learn07CommandResult(
        string Command,
        int? ExitCode,
        string Result,
        string Summary);

    private sealed record Learn07SafetyEvidence(
        bool TargetPromotionPerformed,
        bool LocatorUpdated,
        bool OuterFixtureRootDeletedBeforeArtifactWrite);

    private sealed record Learn07Track(
        string MachineId,
        string Verdict,
        string TargetCheckpointPath,
        string RollbackCheckpointPath,
        string? ManifestPath,
        string? TargetCheckpointSha256,
        string? RollbackCheckpointSha256,
        string BoundaryUsed,
        bool RollbackLoaded,
        bool RuntimeInvocationSucceeded,
        bool TestOnlyInProcessProbeSucceeded,
        bool TargetOnlyLearnedChangesAbsentAfterRollback,
        string TargetOnlyChangeId,
        string RollbackObservedStatus,
        string? RollbackObservedOutput,
        bool SemanticQualityClaimed,
        bool CommonModelMutated,
        string Notes);

    private sealed record Learn07Validation(
        string Schema,
        string PhaseId,
        string Verdict,
        bool AcceptedForParentReview,
        IReadOnlyDictionary<string, object> Checks,
        IReadOnlyList<string> Commands,
        string NextPhase,
        IReadOnlyList<string> Limitations);

    private sealed record ParentReview(
        string Schema,
        string ReviewId,
        string PhaseId,
        string Verdict,
        bool Accepted,
        IReadOnlyList<string> Findings,
        IReadOnlyList<Learn07Track> Tracks,
        Learn07SafetyEvidence SafetyEvidence,
        IReadOnlyList<string> Commands,
        string NextPhase);
}
