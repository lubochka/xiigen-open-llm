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

public sealed class Learn05FreshLoadRunnerTests
{
    private const string OrchestrationPatternId = "or-learned-pattern:learn-04-local-runtime-token-quality-boundary";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions CompactJsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Learn05FreshLoadsStagedTargetsThroughAvailablePublicBoundaries()
    {
        var repoRoot = FindRepoRoot();
        var artifactDir = Path.Combine(repoRoot, "artifacts", "extension-runtime-token-quality-learnability");
        Directory.CreateDirectory(artifactDir);

        Learn05SourceAssets? sourceAssets = null;
        try
        {
            sourceAssets = await PrepareLearn05SourceAssetsAsync();

            var tracks = new[]
            {
                ProbePromptToTaskAdaptive(sourceAssets.PromptToTaskCheckpointPath),
                await ProbeOrchestrationReasoningMachineAsync(
                    sourceAssets.OrchestrationReasoningCheckpointPath,
                    sourceAssets.OrchestrationReasoningManifestPath),
                ProbeSkillSelectionCorrectionAdaptive(sourceAssets.SkillSelectionCheckpointPath)
            };
            var reportTracks = tracks
                .Select(track => RedactTrackPaths(track, sourceAssets.TempRoot))
                .ToArray();
            var reportPathsRedacted = AreReportPathsRedacted(reportTracks);
            var tempFixtureRemoved = DeleteValidatedTempFixtureRoot(sourceAssets.TempRoot);
            var promptVerdictAccepted = HasExactTrackVerdict(
                tracks,
                "PromptToTaskMachine",
                "PARTIAL_ADAPTIVE_LOADER_ONLY",
                testOnlyProbeSucceeded: false);
            var orchestrationVerdictAccepted = HasExactTrackVerdict(
                tracks,
                "OrchestrationReasoningMachine",
                "PASS_PUBLIC_MACHINE_BOUNDARY",
                testOnlyProbeSucceeded: true)
                && tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").ObservedOutput == OrchestrationPatternId;
            var skillSelectionVerdictAccepted = HasExactTrackVerdict(
                tracks,
                "SkillSelectionMachine",
                "PARTIAL_ADAPTIVE_LOADER_ONLY",
                testOnlyProbeSucceeded: false);
            var expectedTrackVerdictsAccepted = promptVerdictAccepted
                && orchestrationVerdictAccepted
                && skillSelectionVerdictAccepted;
            var allLoadsSucceeded = tracks.All(track => track.LoadSucceeded);
            var noRuntimeInvocationClaimed = tracks.All(track => !track.RuntimeInvocationSucceeded);
            var exactlyOneTestOnlyProbeSucceeded = tracks.Count(track => track.TestOnlyInProcessProbeSucceeded) == 1;
            var noSemanticQualityClaimed = tracks.All(track => !track.SemanticQualityClaimed);
            var noCommonModelMutated = tracks.All(track => !track.CommonModelMutated);
            var realLearn04TargetExercised = false;
            var accepted = expectedTrackVerdictsAccepted
                && allLoadsSucceeded
                && noRuntimeInvocationClaimed
                && exactlyOneTestOnlyProbeSucceeded
                && noSemanticQualityClaimed
                && noCommonModelMutated
                && reportPathsRedacted
                && tempFixtureRemoved
                && !realLearn04TargetExercised;
            var commands = new[]
            {
                @"dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn05FreshLoadRunnerTests --no-restore --logger ""console;verbosity=normal"""
            };

            var report = new Learn05Report(
                Schema: "xiigen.learn_05_fresh_load_report.v1",
                PhaseId: "LEARN-05",
                Verdict: accepted ? "PASS_WITH_LIMITATIONS" : "FAIL",
                GeneratedAtUtc: DateTimeOffset.UtcNow,
                Commands: commands,
                CommandResults: [
                    new Learn05CommandResult(
                        Command: commands[0],
                        ExitCode: null,
                        Result: "IN_PROCESS_EVIDENCE_ONLY",
                        Summary: "This Learn05 artifact is written by the in-process fixture. The external dotnet test exit code and pass/fail status are recorded by the executor TRX/log, not self-authored here.")
                ],
                Tracks: reportTracks,
                WhatThisProves: [
                    "The OrchestrationReasoning test-owned deterministic fixture using the LEARN-04 pattern id/schema fresh-loads through OrchestrationReasoningMachine.LoadCheckpointAsync and produces a Plan response through OrchestrationReasoningMachine.Plan.",
                    "The PromptToTask test-owned deterministic adaptive fixture loads through the public PromptToTaskAdaptiveTrainer.LoadCheckpoint adaptive boundary only.",
                    "The SkillSelection test-owned deterministic correction fixture loads through the public SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint adaptive boundary only.",
                    "The source checkpoint files are generated under a unique test temp root, reported only as <test-temp-root>/... paths, and removed before tracked Learn05 artifacts can record temp_fixture_removed=true."
                ],
                WhatThisDoesNotProve: [
                    "This does not prove semantic quality.",
                    "This does not prove held-out/source-disjoint quality.",
                    "This does not prove ablation/no-memorization controls.",
                    "This does not prove rollback safety.",
                    "This does not promote target checkpoints.",
                    "This does not exercise the real LEARN-04 target checkpoint files; it exercises test-owned deterministic fixtures matching the LEARN-04 pattern id/schema.",
                    "PromptToTask and SkillSelection are not full machine fresh-load proofs because these test-owned deterministic fixtures are adaptive overlay checkpoints without the full machine checkpoint/manifest shape."
                ],
                NextIndependentTrack: "LEARN-06 held-out/ablation runner");

            var trackedOutputs = new[]
            {
                Path.Combine(artifactDir, "fresh-load-report.json"),
                Path.Combine(artifactDir, "fresh-load-report.md"),
                Path.Combine(artifactDir, "learn-05-validation.json"),
                Path.Combine(artifactDir, "parent-review-LEARN-05.json"),
                Path.Combine(artifactDir, "parent-review-LEARN-05.md")
            };
            WriteJson(trackedOutputs[0], report);
            WriteMarkdown(trackedOutputs[1], report);

            var validation = new Learn05Validation(
                Schema: "xiigen.learn_05_validation.v1",
                PhaseId: "LEARN-05",
                Verdict: accepted ? "PASS_WITH_LIMITATIONS" : "FAIL",
                Checks: new Dictionary<string, object>
                {
                    ["accepted"] = accepted,
                    ["fresh_load_report_exists"] = true,
                    ["tracks_count"] = tracks.Length,
                    ["expected_track_verdicts_accepted"] = expectedTrackVerdictsAccepted,
                    ["prompt_to_task_partial_adaptive_loader"] = promptVerdictAccepted,
                    ["orchestration_reasoning_public_machine_boundary_pass"] = orchestrationVerdictAccepted,
                    ["skill_selection_partial_adaptive_loader"] = skillSelectionVerdictAccepted,
                    ["all_loads_succeeded"] = allLoadsSucceeded,
                    ["runtime_invocation_succeeded_claimed"] = !noRuntimeInvocationClaimed,
                    ["test_only_in_process_probe_succeeded"] = exactlyOneTestOnlyProbeSucceeded,
                    ["report_paths_redacted"] = reportPathsRedacted,
                    ["redacted_path_marker"] = "<test-temp-root>/",
                    ["temp_fixture_removed"] = tempFixtureRemoved,
                    ["real_learn_04_target_exercised"] = realLearn04TargetExercised,
                    ["file_parse_alone_claimed"] = false,
                    ["semantic_quality_claimed"] = !noSemanticQualityClaimed,
                    ["learnability_claimed"] = false,
                    ["held_out_or_ablation_claimed"] = false,
                    ["rollback_claimed"] = false,
                    ["promotion_ready_claimed"] = false,
                    ["common_model_mutated"] = !noCommonModelMutated,
                    ["external_ai_used"] = false
                },
                Commands: commands,
                NextPhase: "LEARN-06",
                Limitations: report.WhatThisDoesNotProve);
            WriteJson(trackedOutputs[2], validation);

            var parentFindings = accepted
                ? new[]
                {
                    "OrchestrationReasoning has the required public machine fresh-load boundary proof for a test-owned deterministic fixture using the LEARN-04 pattern id/schema.",
                    "PromptToTask and SkillSelection are accepted only as narrow public adaptive-loader evidence because their test-owned deterministic fixtures are adaptive overlay checkpoints, not full machine checkpoint plus manifest artifacts.",
                    "Accepted is derived from exact track verdicts, all loads, the successful test-only OrchestrationReasoning Plan selection, no runtime invocation claim, no semantic/common mutation, path redaction, and verified temp fixture deletion.",
                    "No semantic quality, held-out/source-disjoint, ablation, rollback, promotion, common-model mutation, runtime, external AI, or real promoted-target claim is made."
                }
                : new[]
                {
                    "Learn05 acceptance predicate failed; inspect learn-05-validation.json checks for the failing component.",
                    "No semantic quality, held-out/source-disjoint, ablation, rollback, promotion, common-model mutation, runtime, external AI, or real promoted-target claim is made."
                };
            var parentReview = new ParentReview(
                Schema: "xiigen.parent_review.learn_05.v1",
                ReviewId: "parent-review-LEARN-05",
                PhaseId: "LEARN-05",
                Verdict: accepted ? "ACCEPTED_FOR_PHASE_COMMIT_WITH_LIMITATIONS" : "REJECTED",
                Accepted: accepted,
                Findings: parentFindings,
                Tracks: reportTracks,
                Commands: commands,
                NextPhase: "LEARN-06 held-out/ablation runner");
            WriteJson(trackedOutputs[3], parentReview);
            WriteParentReviewMarkdown(trackedOutputs[4], parentReview);

            AssertTrackedLearn05OutputsDoNotLeakPaths(trackedOutputs, repoRoot, sourceAssets.TempRoot);
            Assert.True(
                accepted,
                BuildAcceptanceFailureMessage(
                    expectedTrackVerdictsAccepted,
                    allLoadsSucceeded,
                    noRuntimeInvocationClaimed,
                    exactlyOneTestOnlyProbeSucceeded,
                    noSemanticQualityClaimed,
                    noCommonModelMutated,
                    reportPathsRedacted,
                    tempFixtureRemoved,
                    realLearn04TargetExercised));
        }
        finally
        {
            if (sourceAssets is not null && Directory.Exists(sourceAssets.TempRoot))
                DeleteValidatedTempFixtureRoot(sourceAssets.TempRoot);
        }
    }

    [Fact]
    public async Task PrepareLearn05SourceAssetsAsync_WhenSetupThrows_RemovesCreatedTempRoot()
    {
        string? fixtureRoot = null;

        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            PrepareLearn05SourceAssetsAsync(root =>
            {
                fixtureRoot = root;
                throw new InvalidOperationException("learn-05-injected-fixture-failure");
            }));

        Assert.Equal("learn-05-injected-fixture-failure", failure.Message);
        Assert.NotNull(fixtureRoot);
        Assert.False(Directory.Exists(fixtureRoot), $"Injected failure left temp fixture root behind: {fixtureRoot}");
    }

    private static async Task<Learn05SourceAssets> PrepareLearn05SourceAssetsAsync()
        => await PrepareLearn05SourceAssetsAsync(afterFixtureRootCreated: null);

    private static async Task<Learn05SourceAssets> PrepareLearn05SourceAssetsAsync(
        Func<string, Task>? afterFixtureRootCreated)
    {
        var fixtureRoot = CreateValidatedTempFixtureRoot();
        try
        {
            if (afterFixtureRootCreated is not null)
                await afterFixtureRootCreated(fixtureRoot);

            var promptToTaskPath = MaterializePromptToTaskCheckpoint(fixtureRoot);
            var orchestration = await MaterializeOrchestrationReasoningCheckpointAsync(
                fixtureRoot);
            var skillSelectionPath = MaterializeSkillSelectionCheckpoint(fixtureRoot);

            return new Learn05SourceAssets(
                fixtureRoot,
                promptToTaskPath,
                orchestration.CheckpointPath,
                orchestration.ManifestPath,
                skillSelectionPath);
        }
        catch
        {
            DeleteValidatedTempFixtureRoot(fixtureRoot);
            throw;
        }
    }

    private static string MaterializePromptToTaskCheckpoint(string fixtureRoot)
    {
        var userLocalRoot = Path.Combine(fixtureRoot, "prompt-to-task");
        var signal = RequireSuccess(
            new PromptToTaskAdaptiveDpoSignalWriter().Write(
                new PromptToTaskAdaptiveDpoSignalWriteRequest(
                    UserLocalRoot: userLocalRoot,
                    SignalId: "learn-05-prompt-to-task-overlay-signal-001",
                    SourcePromptLedgerId: "learn-05-prompt-to-task-ledger-001",
                    RawPromptSha256: Sha256Text("learn-05 prompt-to-task fresh-load redacted source"),
                    ChosenStructuredTaskRef: "structured-task-ref:learn-05:prompt-to-task:chosen",
                    RejectedStructuredTaskRef: "structured-task-ref:learn-05:prompt-to-task:rejected",
                    FeedbackClass: "learn_05_fresh_load_fixture",
                    ProvenanceRefs: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["fixture_scope"] = "test-owned-temp-fixture",
                        ["raw_prompt_text"] = "redacted",
                        ["phase"] = "LEARN-05"
                    })),
            "write PromptToTask adaptive signal");

        var trained = RequireSuccess(
            new PromptToTaskAdaptiveTrainer().TrainUserLocal(
                new PromptToTaskAdaptiveTrainingRequest(userLocalRoot)),
            "train PromptToTask adaptive checkpoint");
        Assert.NotEmpty(trained.Checkpoint.TaskProfiles);
        Assert.Equal(signal.Signal.SignalId, trained.Checkpoint.TrainedSignalIds.Single());

        return trained.Report.CheckpointPath;
    }

    private static async Task<OrchestrationReasoningCheckpointFiles> MaterializeOrchestrationReasoningCheckpointAsync(
        string fixtureRoot)
    {
        var checkpointDirectory = Path.Combine(fixtureRoot, "orchestration-reasoning");
        Directory.CreateDirectory(checkpointDirectory);
        var sourceRoot = Path.Combine(checkpointDirectory, "source");
        Directory.CreateDirectory(sourceRoot);

        var payloadPath = Path.Combine(sourceRoot, "payload.jsonl");
        var sourceManifestPath = Path.Combine(sourceRoot, "source-manifest.json");
        var ledgerPath = Path.Combine(sourceRoot, "source-admission-ledger.json");
        var evidencePath = Path.Combine(sourceRoot, "source-admission-evidence.json");
        var pairId = "learn-05-orchestration-pair-001";
        var rowId = "learn-05-orchestration-row-001";
        var sourceId = "learn-05-orchestration-source-001";
        var sourceRef = "learn-05-source-ref:orchestration-reasoning";
        var sourceSha = Sha256Text("learn-05 orchestration source material");
        var pattern = CreateLearn05OrchestrationPattern();
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
                ["repo_root_ref"] = "repo://llm_mvp_core-DpoImplementation/test-owned-learn-05-fixture"
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
            "materialize OrchestrationReasoning admitted state");
        var machine = new OrchestrationReasoningMachine();
        RequireSuccess(
            machine.ImportAdmittedState(materialized.AdmittedState),
            "import OrchestrationReasoning admitted state");

        var checkpointPath = Path.Combine(checkpointDirectory, "checkpoint.json");
        var manifestPath = Path.Combine(checkpointDirectory, "manifest.json");
        RequireSuccess(
            await machine.SaveCheckpointAsync(checkpointPath, manifestPath),
            "save OrchestrationReasoning checkpoint");
        RequireSuccess(
            await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath),
            "load OrchestrationReasoning checkpoint");

        return new OrchestrationReasoningCheckpointFiles(
            checkpointPath,
            manifestPath,
            OrchestrationReasoningSourceAdmission.GetProofPath(checkpointPath));
    }

    private static string MaterializeSkillSelectionCheckpoint(string fixtureRoot)
    {
        var userLocalRoot = Path.Combine(fixtureRoot, "skill-selection");
        var signal = RequireSuccess(
            new SkillSelectionCorrectionDpoSignalWriter().WriteUserLocalSignal(
                new SkillSelectionCorrectionDpoSignalWriteRequest(
                    UserLocalRoot: userLocalRoot,
                    CorrectionId: "learn-05-skill-selection-correction-001",
                    ChosenSkillIds: ["planning-session-startup"],
                    RejectedSkillIds: ["status-only"],
                    CorrectionClass: "learn_05_fresh_load_fixture",
                    PromptRef: "prompt-ref:learn-05:skill-selection",
                    OutputRef: "output-ref:learn-05:skill-selection",
                    AiResponseRef: "ai-response-ref:learn-05:skill-selection",
                    SourcePromptLedgerId: "learn-05-skill-selection-ledger-001",
                    ProvenanceRefs: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["fixture_scope"] = "test-owned-temp-fixture",
                        ["raw_prompt_text"] = "redacted",
                        ["phase"] = "LEARN-05"
                    })),
            "write SkillSelection correction adaptive signal");

        var trained = RequireSuccess(
            new SkillSelectionCorrectionAdaptiveTrainer().TrainUserLocalAsync(
                new SkillSelectionCorrectionAdaptiveTrainingRequest(
                    userLocalRoot,
                    signal.SignalPath)),
            "train SkillSelection correction adaptive checkpoint");
        Assert.NotEmpty(trained.Checkpoint.CorrectionProfiles);
        Assert.Equal(signal.Signal.SignalId, trained.Checkpoint.TrainedSignalIds.Single());

        return trained.Report.CheckpointPath;
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
            ["error_code"] = "LEARN-05",
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
                ["near_miss_signals"] = StringArray(["missing_public_machine_boundary"])
            },
            ["universal_reasoning_pattern"] = "learn-05-fresh-load-boundary-fixture",
            ["learned_pattern"] = JsonSerializer.SerializeToNode(pattern, CompactJsonOptions)
        };

    private static OrchestrationReasoningLearnedPattern CreateLearn05OrchestrationPattern()
        => new(
            PatternId: OrchestrationPatternId,
            ScenarioRef: "scenario:learn-05-orchestration-fresh-load",
            RequiredTaskTypes: ["learn_05_fresh_load_task"],
            RequiredConstraintSignals:
            [
                "test_owned_temp_fixture",
                "no_user_local_path_dependency"
            ],
            RequiredRequirementSignals: ["fresh_load_public_boundary_required"],
            RequiredDeliverableSignals: ["fresh_load_report"],
            RequiredEvidenceSignals:
            [
                "checkpoint_sha256",
                "manifest_sha256",
                "source_admission_proof"
            ],
            PhaseFamilies: ["LEARN-05"],
            GateFamilies: ["fresh-load", "no-overclaim"],
            AssetFamilies: ["smart-skills", "orchestration-reasoning"],
            HostCheckSets: ["test-temp-fixture"],
            RiskFlags:
            [
                "no_semantic_quality_claim",
                "no_promotion",
                "no_user_local_dependency"
            ],
            NextGranularizationActions:
            [
                "run_held_out_ablation_next",
                "keep_limitations_explicit"
            ],
            Confidence: 0.92);

    private static Learn05Track ProbePromptToTaskAdaptive(string checkpointPath)
    {
        var load = new PromptToTaskAdaptiveTrainer().LoadCheckpoint(checkpointPath);
        var checkpoint = load.Value;
        return new Learn05Track(
            MachineId: "PromptToTaskMachine",
            Verdict: load.IsSuccess ? "PARTIAL_ADAPTIVE_LOADER_ONLY" : "FAIL",
            TargetCheckpointPath: checkpointPath,
            ManifestPath: null,
            TargetCheckpointSha256: File.Exists(checkpointPath) ? Sha256File(checkpointPath) : null,
            BoundaryUsed: "PromptToTaskAdaptiveTrainer.LoadCheckpoint",
            LoadSucceeded: load.IsSuccess,
            RuntimeInvocationSucceeded: false,
            TestOnlyInProcessProbeSucceeded: false,
            RuntimeInvocationMethod: null,
            ObservedStatus: load.IsSuccess ? "adaptive_checkpoint_loaded" : "adaptive_checkpoint_load_failed",
            ObservedOutput: load.IsSuccess
                ? $"taskProfiles={checkpoint!.TaskProfiles.Count}; trainedSignalIds={checkpoint.TrainedSignalIds.Count}"
                : null,
            SemanticQualityClaimed: checkpoint?.SemanticQualityClaimed ?? false,
            CommonModelMutated: false,
            ErrorCode: load.IsSuccess ? null : load.ErrorCode,
            Limitation: "Narrow public adaptive loader only. The test-owned deterministic fixture is an adaptive overlay checkpoint without a PromptToTaskMachine manifest, so this does not prove PromptToTaskMachine.LoadCheckpointAsync or Translate.");
    }

    private static async Task<Learn05Track> ProbeOrchestrationReasoningMachineAsync(
        string checkpointPath,
        string manifestPath)
    {
        var load = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        if (!load.IsSuccess)
        {
            return new Learn05Track(
                MachineId: "OrchestrationReasoningMachine",
                Verdict: "FAIL",
                TargetCheckpointPath: checkpointPath,
                ManifestPath: manifestPath,
                TargetCheckpointSha256: File.Exists(checkpointPath) ? Sha256File(checkpointPath) : null,
                BoundaryUsed: "OrchestrationReasoningMachine.LoadCheckpointAsync",
                LoadSucceeded: false,
                RuntimeInvocationSucceeded: false,
                TestOnlyInProcessProbeSucceeded: false,
                RuntimeInvocationMethod: null,
                ObservedStatus: "load_failed",
                ObservedOutput: null,
                SemanticQualityClaimed: false,
                CommonModelMutated: false,
                ErrorCode: load.ErrorCode,
                Limitation: load.ErrorMessage);
        }

        var learnedPattern = load.Value!.Checkpoint.LearnedState.LearnedPatterns!
            .Single(pattern => pattern.PatternId == "or-learned-pattern:learn-04-local-runtime-token-quality-boundary");
        var request = BuildMatchingOrchestrationRequest(learnedPattern);
        var response = load.Value.Machine.Plan(request);
        var succeeded = response.IsSuccess
            && response.Value!.ContractStatus == OrchestrationReasoningMachine.LearnedPatternStatus
            && response.Value.ScenarioPlan.SelectedLearnedPatterns.Contains(learnedPattern.PatternId, StringComparer.Ordinal);

        return new Learn05Track(
            MachineId: "OrchestrationReasoningMachine",
            Verdict: succeeded ? "PASS_PUBLIC_MACHINE_BOUNDARY" : "FAIL",
            TargetCheckpointPath: checkpointPath,
            ManifestPath: manifestPath,
            TargetCheckpointSha256: Sha256File(checkpointPath),
            BoundaryUsed: "OrchestrationReasoningMachine.LoadCheckpointAsync + OrchestrationReasoningMachine.Plan",
            LoadSucceeded: load.IsSuccess,
            RuntimeInvocationSucceeded: false,
            TestOnlyInProcessProbeSucceeded: succeeded,
            RuntimeInvocationMethod: "OrchestrationReasoningMachine.Plan",
            ObservedStatus: response.IsSuccess ? response.Value!.ContractStatus : "plan_failed",
            ObservedOutput: response.IsSuccess
                ? string.Join(",", response.Value!.ScenarioPlan.SelectedLearnedPatterns)
                : null,
            SemanticQualityClaimed: load.Value.Checkpoint.SemanticQualityClaimed || load.Value.Manifest.SemanticQualityClaimed,
            CommonModelMutated: false,
            ErrorCode: response.IsSuccess ? null : response.ErrorCode,
            Limitation: "Fresh-load proves loadability and public machine invocation for the test-owned deterministic fixture only. It does not prove semantic quality, held-out/source-disjoint behavior, ablation, rollback, or promotion readiness.");
    }

    private static Learn05Track ProbeSkillSelectionCorrectionAdaptive(string checkpointPath)
    {
        var load = new SkillSelectionCorrectionAdaptiveTrainer().LoadCheckpoint(checkpointPath);
        var checkpoint = load.Value;
        return new Learn05Track(
            MachineId: "SkillSelectionMachine",
            Verdict: load.IsSuccess ? "PARTIAL_ADAPTIVE_LOADER_ONLY" : "FAIL",
            TargetCheckpointPath: checkpointPath,
            ManifestPath: null,
            TargetCheckpointSha256: File.Exists(checkpointPath) ? Sha256File(checkpointPath) : null,
            BoundaryUsed: "SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint",
            LoadSucceeded: load.IsSuccess,
            RuntimeInvocationSucceeded: false,
            TestOnlyInProcessProbeSucceeded: false,
            RuntimeInvocationMethod: null,
            ObservedStatus: load.IsSuccess ? "correction_adaptive_checkpoint_loaded" : "correction_adaptive_checkpoint_load_failed",
            ObservedOutput: load.IsSuccess
                ? $"correctionProfiles={checkpoint!.CorrectionProfiles.Count}; trainedSignalIds={checkpoint.TrainedSignalIds.Count}"
                : null,
            SemanticQualityClaimed: checkpoint?.SemanticQualityClaimed ?? false,
            CommonModelMutated: checkpoint?.CommonCheckpointMutationClaimed ?? false,
            ErrorCode: load.IsSuccess ? null : load.ErrorCode,
            Limitation: "Narrow public correction-adaptive loader only. The test-owned deterministic fixture is not a full SkillSelectionMachine checkpoint/manifest, so this does not prove SkillSelectionMachine.LoadCheckpointAsync or DecideFromTrainedState.");
    }

    private static OrchestrationReasoningRequest BuildMatchingOrchestrationRequest(
        OrchestrationReasoningLearnedPattern pattern)
    {
        var prompt = "redacted LEARN-05 fresh-load probe for local runtime token quality boundary";
        var task = new StructuredTaskContract(
            PromptId: "learn-05-orchestration-fresh-load",
            RawPromptSha256: Sha256Text(prompt),
            UserLanguage: "en",
            TaskType: pattern.RequiredTaskTypes.Single(),
            ExplicitUserRequirements: pattern.RequiredRequirementSignals,
            Constraints: pattern.RequiredConstraintSignals,
            Deliverables: pattern.RequiredDeliverableSignals,
            ForbiddenActions:
            [
                "no_semantic_quality_claim",
                "no_promotion",
                "no_rollback_claim"
            ],
            RequiredHosts: [],
            EvidenceNeeds: pattern.RequiredEvidenceSignals,
            SourceProvenance: new Dictionary<string, string>
            {
                ["boundary"] = "learn-05-public-machine-fresh-load",
                ["raw_prompt_redacted"] = "true"
            },
            SplitId: "learn-05-fresh-load-probe");

        return new OrchestrationReasoningRequest(
            RequestId: "learn-05-orchestration-fresh-load",
            Task: task,
            GuideDerivedStateRefs: [],
            CorpusBundleIds: [],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["fresh_load"] = "true",
                ["semantic_quality_claimed"] = "false"
            });
    }

    private static void WriteMarkdown(string path, Learn05Report report)
    {
        var lines = new List<string>
        {
            "# LEARN-05 fresh-load report",
            "",
            $"Verdict: {report.Verdict}",
            "",
            "## Commands",
            ""
        };
        lines.AddRange(report.Commands.Select(command => $"- `{command}`"));
        lines.Add("");
        lines.Add("## Command results");
        lines.Add("");
        lines.AddRange(report.CommandResults.Select(result => $"- `{result.Command}` -> {result.Result}; {result.Summary}"));
        lines.Add("");
        lines.Add("## Track verdicts");
        lines.Add("");
        foreach (var track in report.Tracks)
        {
            lines.Add($"- {track.MachineId}: {track.Verdict}");
            lines.Add($"  Boundary: `{track.BoundaryUsed}`");
            lines.Add($"  Fixture path: `{track.TargetCheckpointPath}`");
            lines.Add($"  Runtime invocation succeeded: {track.RuntimeInvocationSucceeded.ToString().ToLowerInvariant()}");
            lines.Add($"  Test-only in-process probe succeeded: {track.TestOnlyInProcessProbeSucceeded.ToString().ToLowerInvariant()}");
            lines.Add($"  Observed: {track.ObservedStatus}; {track.ObservedOutput}");
            lines.Add($"  Limitation: {track.Limitation}");
        }

        lines.Add("");
        lines.Add("## What this proves");
        lines.AddRange(report.WhatThisProves.Select(item => $"- {item}"));
        lines.Add("");
        lines.Add("## What this does not prove");
        lines.AddRange(report.WhatThisDoesNotProve.Select(item => $"- {item}"));
        lines.Add("");
        lines.Add("## Next independent track");
        lines.Add("");
        lines.Add($"- {report.NextIndependentTrack}");
        lines.Add("");
        lines.Add("Fixture fresh-load proves only loadability. It requires held-out, ablation, and rollback evidence before readiness.");
        ArtifactTextWriter.WriteText(path, string.Join("\n", lines), finalNewLine: true);
    }

    private static void WriteParentReviewMarkdown(string path, ParentReview review)
    {
        var lines = new List<string>
        {
            "# Parent review LEARN-05",
            "",
            $"Verdict: {review.Verdict}",
            "",
            "## Findings"
        };
        lines.AddRange(review.Findings.Select(item => $"- {item}"));
        lines.Add("");
        lines.Add("## Track verdicts");
        foreach (var track in review.Tracks)
        {
            lines.Add($"- {track.MachineId}: {track.Verdict} via `{track.BoundaryUsed}`");
            lines.Add($"  Fixture path: `{track.TargetCheckpointPath}`");
            if (track.ManifestPath is not null)
                lines.Add($"  Manifest path: `{track.ManifestPath}`");
            lines.Add($"  Runtime invocation succeeded: {track.RuntimeInvocationSucceeded.ToString().ToLowerInvariant()}");
            lines.Add($"  Test-only in-process probe succeeded: {track.TestOnlyInProcessProbeSucceeded.ToString().ToLowerInvariant()}");
        }
        lines.Add("");
        lines.Add("## Next phase");
        lines.Add("");
        lines.Add($"- {review.NextPhase}");
        ArtifactTextWriter.WriteText(path, string.Join("\n", lines), finalNewLine: true);
    }

    private static void WriteJson<T>(string path, T value)
        => ArtifactTextWriter.WriteJson(path, value, JsonOptions, finalNewLine: true);

    private static void WriteJsonNode(string path, JsonNode value)
        => ArtifactTextWriter.WriteText(path, value.ToJsonString(JsonOptions), finalNewLine: true);

    private static JsonArray StringArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
            array.Add(value);
        return array;
    }

    private static T RequireSuccess<T>(
        OperationResult<T> result,
        string action)
    {
        Assert.True(result.IsSuccess, $"{action} failed: {result.ErrorCode} {result.ErrorMessage}");
        return result.Value!;
    }

    private static string CreateValidatedTempFixtureRoot()
    {
        var tempBase = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "xiigen-learn-05-fresh-load"));
        Directory.CreateDirectory(tempBase);
        var fixtureRoot = Path.GetFullPath(Path.Combine(
            tempBase,
            $"run-{Guid.NewGuid():N}"));
        Assert.True(IsSubpathOf(fixtureRoot, tempBase), $"Fixture root escaped temp base: {fixtureRoot}");
        Assert.StartsWith("run-", Path.GetFileName(fixtureRoot), StringComparison.Ordinal);
        Assert.False(Directory.Exists(fixtureRoot), $"Fixture root already exists: {fixtureRoot}");
        Directory.CreateDirectory(fixtureRoot);
        return fixtureRoot;
    }

    private static bool DeleteValidatedTempFixtureRoot(string fixtureRoot)
    {
        var tempBase = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "xiigen-learn-05-fresh-load"));
        var fullRoot = Path.GetFullPath(fixtureRoot);
        Assert.True(IsSubpathOf(fullRoot, tempBase), $"Refusing to delete fixture outside temp base: {fullRoot}");
        Assert.StartsWith("run-", Path.GetFileName(fullRoot), StringComparison.Ordinal);
        if (Directory.Exists(fullRoot))
            Directory.Delete(fullRoot, recursive: true);
        Assert.False(Directory.Exists(fullRoot), $"Fixture temp root was not removed: {fullRoot}");
        return !Directory.Exists(fullRoot);
    }

    private static bool IsSubpathOf(string childPath, string parentPath)
    {
        var parent = Path.GetFullPath(parentPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var child = Path.GetFullPath(childPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return child.StartsWith(parent, StringComparison.OrdinalIgnoreCase);
    }

    private static Learn05Track RedactTrackPaths(
        Learn05Track track,
        string fixtureRoot)
        => track with
        {
            TargetCheckpointPath = RedactTempPath(track.TargetCheckpointPath, fixtureRoot),
            ManifestPath = track.ManifestPath is null
                ? null
                : RedactTempPath(track.ManifestPath, fixtureRoot)
        };

    private static string RedactTempPath(string path, string fixtureRoot)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(fixtureRoot);
        Assert.True(IsSubpathOf(fullPath, fullRoot), $"Refusing to report non-fixture path: {fullPath}");
        return "<test-temp-root>/" + Path.GetRelativePath(fullRoot, fullPath).Replace('\\', '/');
    }

    private static bool HasExactTrackVerdict(
        IReadOnlyList<Learn05Track> tracks,
        string machineId,
        string verdict,
        bool testOnlyProbeSucceeded)
        => tracks.Count(track =>
            track.MachineId == machineId
            && track.Verdict == verdict
            && track.LoadSucceeded
            && !track.RuntimeInvocationSucceeded
            && track.TestOnlyInProcessProbeSucceeded == testOnlyProbeSucceeded) == 1;

    private static bool AreReportPathsRedacted(IReadOnlyList<Learn05Track> reportTracks)
        => reportTracks.All(track =>
            track.TargetCheckpointPath.StartsWith("<test-temp-root>/", StringComparison.Ordinal)
            && (track.ManifestPath is null || track.ManifestPath.StartsWith("<test-temp-root>/", StringComparison.Ordinal)));

    private static string BuildAcceptanceFailureMessage(
        bool expectedTrackVerdictsAccepted,
        bool allLoadsSucceeded,
        bool noRuntimeInvocationClaimed,
        bool exactlyOneTestOnlyProbeSucceeded,
        bool noSemanticQualityClaimed,
        bool noCommonModelMutated,
        bool reportPathsRedacted,
        bool tempFixtureRemoved,
        bool realLearn04TargetExercised)
        => string.Join(
            "; ",
            [
                $"expected_track_verdicts_accepted={expectedTrackVerdictsAccepted}",
                $"all_loads_succeeded={allLoadsSucceeded}",
                $"runtime_invocation_succeeded_claimed={!noRuntimeInvocationClaimed}",
                $"exactly_one_test_only_probe_succeeded={exactlyOneTestOnlyProbeSucceeded}",
                $"semantic_quality_claimed={!noSemanticQualityClaimed}",
                $"common_model_mutated={!noCommonModelMutated}",
                $"report_paths_redacted={reportPathsRedacted}",
                $"temp_fixture_removed={tempFixtureRemoved}",
                $"real_learn_04_target_exercised={realLearn04TargetExercised}"
            ]);

    private static void AssertTrackedLearn05OutputsDoNotLeakPaths(
        IReadOnlyList<string> trackedOutputs,
        string repoRoot,
        string fixtureRoot)
    {
        var tempPath = Path.GetFullPath(Path.GetTempPath());
        var localAppDataTemp = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Temp");
        var forbiddenMarkers = new[]
            {
                "C:" + Path.DirectorySeparatorChar + "Users" + Path.DirectorySeparatorChar + "lubao",
                "C:/Users/" + "lubao",
                NormalizeForScan(tempPath),
                NormalizeForScan(localAppDataTemp),
                NormalizeForScan(Path.GetFullPath(repoRoot)),
                NormalizeForScan(Path.GetFullPath(fixtureRoot))
            }
            .Where(marker => !string.IsNullOrWhiteSpace(marker))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var path in trackedOutputs)
        {
            Assert.True(File.Exists(path), $"Expected tracked Learn05 output: {path}");
            var text = File.ReadAllText(path);
            var normalized = NormalizeForScan(text);
            foreach (var marker in forbiddenMarkers)
            {
                Assert.DoesNotContain(marker, normalized, StringComparison.OrdinalIgnoreCase);
            }

            Assert.Contains("<test-temp-root>/", text, StringComparison.Ordinal);
        }
    }

    private static string NormalizeForScan(string value)
        => value.Replace('\\', '/');

    private static string Sha256File(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

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

    private sealed record Learn05Report(
        string Schema,
        string PhaseId,
        string Verdict,
        DateTimeOffset GeneratedAtUtc,
        IReadOnlyList<string> Commands,
        IReadOnlyList<Learn05CommandResult> CommandResults,
        IReadOnlyList<Learn05Track> Tracks,
        IReadOnlyList<string> WhatThisProves,
        IReadOnlyList<string> WhatThisDoesNotProve,
        string NextIndependentTrack);

    private sealed record Learn05CommandResult(
        string Command,
        int? ExitCode,
        string Result,
        string Summary);

    private sealed record Learn05Track(
        string MachineId,
        string Verdict,
        string TargetCheckpointPath,
        string? ManifestPath,
        string? TargetCheckpointSha256,
        string BoundaryUsed,
        bool LoadSucceeded,
        bool RuntimeInvocationSucceeded,
        bool TestOnlyInProcessProbeSucceeded,
        string? RuntimeInvocationMethod,
        string ObservedStatus,
        string? ObservedOutput,
        bool SemanticQualityClaimed,
        bool CommonModelMutated,
        string? ErrorCode,
        string Limitation);

    private sealed record Learn05Validation(
        string Schema,
        string PhaseId,
        string Verdict,
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
        IReadOnlyList<Learn05Track> Tracks,
        IReadOnlyList<string> Commands,
        string NextPhase);

    private sealed record Learn05SourceAssets(
        string TempRoot,
        string PromptToTaskCheckpointPath,
        string OrchestrationReasoningCheckpointPath,
        string OrchestrationReasoningManifestPath,
        string SkillSelectionCheckpointPath);

    private sealed record OrchestrationReasoningCheckpointFiles(
        string CheckpointPath,
        string ManifestPath,
        string ProofPath);
}
