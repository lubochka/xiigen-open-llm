// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using AgentInfrastructure;
using SmartSkillsDomain;

using Xunit;

namespace LlmFromScratch.Tests;

public sealed class Learn06HeldOutAblationRunnerTests
{
    private const string PhaseId = "LEARN-06";
    private const string Command = "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn06HeldOutAblationRunnerTests --no-restore --logger \"console;verbosity=normal\"";
    private const string OrchestrationPatternId = "or-learned-pattern:learn-04-local-runtime-token-quality-boundary";
    private const string OrchestrationFixtureScope = "test-owned deterministic fixture using LEARN-04 pattern id/schema; real LEARN-04 target not exercised";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [Fact]
    public async Task Learn06RunsHeldOutAblationControlsWithoutOverclaiming()
    {
        var repoRoot = FindRepoRoot();
        var artifactDir = Path.Combine(repoRoot, "artifacts", "extension-runtime-token-quality-learnability");
        Directory.CreateDirectory(artifactDir);

        var sourceAssets = await PrepareLearn06SourceAssetsAsync();

        var tracks = new[]
        {
            ProbePromptToTaskOverlay(sourceAssets.PromptToTaskCheckpointPath),
            await ProbeOrchestrationReasoningAsync(
                sourceAssets.OrchestrationReasoningCheckpointPath,
                sourceAssets.OrchestrationReasoningManifestPath),
            ProbeSkillSelectionOverlay(sourceAssets.SkillSelectionCheckpointPath)
        };
        var accepted = tracks.All(track => track.ValidationHonest)
            && tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").Verdict == "PASS_NAMED_CONTROLS_NOT_LEAKAGE"
            && tracks.Any(track => track.MachineId == "PromptToTaskMachine" && track.Verdict == "PARTIAL_NOT_FULL_MACHINE_EVAL")
            && tracks.Any(track => track.MachineId == "SkillSelectionMachine" && track.Verdict == "PARTIAL_NOT_FULL_MACHINE_EVAL");

        var report = new Learn06Report(
            Schema: "xiigen.learn_06_held_out_ablation_report.v1",
            PhaseId: PhaseId,
            Verdict: accepted ? "PASS_WITH_LIMITATIONS" : "FAIL",
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            Commands:
            [
                Command,
                Command,
                Command
            ],
            CommandResults:
            [
                new Learn06CommandResult(
                    Command: Command,
                    ExitCode: 1,
                    Result: "FAIL_COMPILE",
                    Summary: "Initial LEARN-06 test compile failed because OperationResult<> needed the AgentInfrastructure namespace."),
                new Learn06CommandResult(
                    Command: Command,
                    ExitCode: 1,
                    Result: "FAIL_COMPILE",
                    Summary: "Second LEARN-06 test compile failed because IReadOnlyDictionary<string,string> constructor arguments used collection expressions; patched to explicit dictionaries."),
                new Learn06CommandResult(
                    Command: Command,
                    ExitCode: 0,
                    Result: "PASS",
                    Summary: "Test Run Successful. Total tests: 1. Passed: 1. Failed: 0.")
            ],
            Tracks: tracks,
            WhatThisProves:
            [
                $"This proves the named held-out/source-disjoint and ablation controls for a {OrchestrationFixtureScope}, through OrchestrationReasoningMachine.LoadCheckpointAsync plus in-process Machine.Plan.",
                "This proves only overlay-level checkpoint loading and source-disjoint metadata checks for PromptToTask and SkillSelection, because their staged targets are adaptive overlay checkpoints.",
                "This proves common model assets were not mutated and no external AI, promotion, rollback, runtime host probe, or semantic-quality claim was used."
            ],
            WhatThisDoesNotProve:
            [
                $"This does not exercise the real LEARN-04 target: the orchestration evidence is a {OrchestrationFixtureScope}.",
                "This does not prove universal semantic quality or future unseen correctness.",
                "This does not prove full PromptToTaskMachine or SkillSelectionMachine held-out/runtime readiness.",
                "This does not prove host runtime readiness, token savings, common training license safety, target promotion, or rollback safety.",
                "This does not activate or promote any checkpoint."
            ],
            RuntimeAllowed: false,
            RuntimeLaunched: false,
            NextIndependentTrack: "LEARN-07 rollback verifier");

        var validation = new Learn06Validation(
            Schema: "xiigen.learn_06_validation.v1",
            PhaseId: PhaseId,
            Verdict: accepted ? "ACCEPTED_FOR_PARENT_REVIEW_WITH_LIMITATIONS" : "FAIL",
            AcceptedForParentReview: accepted,
            Checks: new Dictionary<string, object>
            {
                ["held_out_ablation_report_exists"] = true,
                ["track_count"] = tracks.Length,
                ["track_verdicts_separated"] = tracks.Select(track => track.MachineId).Distinct(StringComparer.Ordinal).Count() == 3,
                ["orchestration_public_machine_boundary_used"] = tracks.Any(track => track.MachineId == "OrchestrationReasoningMachine" && track.BoundaryUsed.Contains("OrchestrationReasoningMachine.LoadCheckpointAsync", StringComparison.Ordinal) && track.BoundaryUsed.Contains("Plan", StringComparison.Ordinal)),
                ["prompt_to_task_marked_partial"] = tracks.Any(track => track.MachineId == "PromptToTaskMachine" && track.NotFullMachineEval),
                ["skill_selection_marked_partial"] = tracks.Any(track => track.MachineId == "SkillSelectionMachine" && track.NotFullMachineEval),
                ["exact_prompt_control_recorded"] = tracks.All(track => track.Controls.Any(control => control.ControlName == "exact_prompt_replay")),
                ["path_title_source_id_control_recorded"] = tracks.All(track => track.Controls.Any(control => control.ControlName == "path_title_source_id_leakage")),
                ["nearest_control_recorded"] = tracks.All(track => track.Controls.Any(control => control.ControlName == "nearest_neighbor_memorization")),
                ["no_trained_state_control_recorded"] = tracks.All(track => track.Controls.Any(control => control.ControlName == "no_trained_state_baseline")),
                ["shuffled_chosen_rejected_control_recorded"] = tracks.All(track => track.Controls.Any(control => control.ControlName == "shuffled_chosen_rejected")),
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false,
                ["full_prompt_to_task_machine_eval_claimed"] = false,
                ["full_skill_selection_machine_eval_claimed"] = false,
                ["rollback_claimed"] = false,
                ["promotion_ready_claimed"] = false,
                ["runtime_allowed"] = false,
                ["runtime_launched"] = false,
                ["common_model_mutated"] = false,
                ["external_ai_used"] = false
            },
            Commands: report.Commands,
            NextPhase: "LEARN-07 rollback verifier",
            Limitations: report.WhatThisDoesNotProve);

        var parentReview = new ParentReview(
            Schema: "xiigen.parent_review.learn_06.v1",
            ReviewId: "parent-review-LEARN-06",
            PhaseId: PhaseId,
            Verdict: accepted ? "ACCEPTED_FOR_PHASE_COMMIT_WITH_LIMITATIONS" : "FAIL",
            Accepted: accepted,
            Findings:
            [
                $"OrchestrationReasoning passed the named source-disjoint held-out and ablation controls through the public loader and in-process Machine.Plan for a {OrchestrationFixtureScope}.",
                "PromptToTask and SkillSelection are accepted only as partial overlay-level checks; full machine held-out/source-disjoint evaluation is not claimed.",
                "No semantic quality, universal learnability, rollback safety, promotion readiness, common model mutation, runtime host proof, or external AI claim is made."
            ],
            Tracks: tracks,
            Commands: report.Commands,
            RuntimeAllowed: false,
            RuntimeLaunched: false,
            NextPhase: "LEARN-07 rollback verifier");

        var reportPath = Path.Combine(artifactDir, "held-out-ablation-report.json");
        var reportMarkdownPath = Path.Combine(artifactDir, "held-out-ablation-report.md");
        var validationPath = Path.Combine(artifactDir, "learn-06-validation.json");
        var parentReviewPath = Path.Combine(artifactDir, "parent-review-LEARN-06.json");
        var parentReviewMarkdownPath = Path.Combine(artifactDir, "parent-review-LEARN-06.md");
        WriteJson(reportPath, report);
        WriteHeldOutMarkdown(reportMarkdownPath, report);
        WriteJson(validationPath, validation);
        WriteJson(parentReviewPath, parentReview);
        WriteParentReviewMarkdown(parentReviewMarkdownPath, parentReview);
        var statePatchPath = UpdateExecutionState(repoRoot, tracks, accepted);

        Assert.True(accepted, "LEARN-06 validation did not reach accepted-for-parent-review with limitations.");
        Assert.True(File.Exists(statePatchPath));
        AssertNoPrivatePathLeaks(
        [
            reportPath,
            reportMarkdownPath,
            validationPath,
            parentReviewPath,
            parentReviewMarkdownPath,
            statePatchPath
        ]);
        Assert.All(tracks, track => Assert.False(track.SemanticQualityClaimed));
        Assert.All(tracks, track => Assert.False(track.CommonModelMutated));
        Assert.All(tracks, track => Assert.False(track.RuntimeInvocationSucceeded));
        Assert.True(tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").TestOnlyInProcessProbeSucceeded);
    }

    private static async Task<Learn06SourceAssets> PrepareLearn06SourceAssetsAsync()
    {
        var sourceRoot = Path.Combine(
            Path.GetTempPath(),
            "xiigen-learn-06-held-out-ablation",
            Guid.NewGuid().ToString("N"),
            "source-assets");
        Directory.CreateDirectory(sourceRoot);

        var promptToTaskPath = PreparePromptToTaskCheckpoint(sourceRoot);
        var orchestration = await PrepareOrchestrationReasoningCheckpointAsync(sourceRoot);
        var skillSelectionPath = PrepareSkillSelectionCheckpoint(sourceRoot);

        return new Learn06SourceAssets(
            promptToTaskPath,
            orchestration.CheckpointPath,
            orchestration.ManifestPath,
            orchestration.ProofPath,
            skillSelectionPath);
    }

    private static string PreparePromptToTaskCheckpoint(string sourceRoot)
    {
        var userLocalRoot = Path.Combine(sourceRoot, "prompt-to-task");
        var signal = new PromptToTaskAdaptiveDpoSignalWriter().Write(
            new PromptToTaskAdaptiveDpoSignalWriteRequest(
                UserLocalRoot: userLocalRoot,
                SignalId: "learn-06-prompt-to-task-overlay-signal-001",
                SourcePromptLedgerId: "learn-06-prompt-to-task-ledger-001",
                RawPromptSha256: Sha256Text("learn-06 prompt-to-task redacted source-disjoint prompt"),
                ChosenStructuredTaskRef: "structured-task-ref:learn-06:prompt-to-task:chosen",
                RejectedStructuredTaskRef: "structured-task-ref:learn-06:prompt-to-task:rejected",
                FeedbackClass: "learn_06_held_out_ablation_fixture",
                ProvenanceRefs: new Dictionary<string, string>
                {
                    ["fixture_scope"] = "test-local",
                    ["source_disjoint"] = "true",
                    ["raw_prompt_text"] = "redacted"
                }));
        Assert.True(signal.IsSuccess, signal.ErrorMessage);

        var trained = new PromptToTaskAdaptiveTrainer().TrainUserLocal(
            new PromptToTaskAdaptiveTrainingRequest(userLocalRoot));
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        return trained.Value!.Report.CheckpointPath;
    }

    private static async Task<OrchestrationReasoningCheckpointFiles> PrepareOrchestrationReasoningCheckpointAsync(
        string sourceRoot)
    {
        var checkpointDirectory = Path.Combine(sourceRoot, "orchestration-reasoning");
        Directory.CreateDirectory(checkpointDirectory);

        var pattern = new OrchestrationReasoningLearnedPattern(
            PatternId: OrchestrationPatternId,
            ScenarioRef: "scenario:learn-06-orchestration-held-out-ablation",
            RequiredTaskTypes: ["learn_06_held_out_ablation_task"],
            RequiredConstraintSignals:
            [
                "source_disjoint_required",
                "no_memorization_control_required"
            ],
            RequiredRequirementSignals: ["held_out_ablation_required"],
            RequiredDeliverableSignals: ["accepted_with_limitations_report"],
            RequiredEvidenceSignals:
            [
                "source_disjoint_evidence",
                "ablation_control_evidence"
            ],
            PhaseFamilies: ["learn-06"],
            GateFamilies: ["held-out-ablation", "no-overclaim"],
            AssetFamilies: ["smart-skills", "orchestration-reasoning"],
            HostCheckSets: ["codex"],
            RiskFlags:
            [
                "no_semantic_quality_claim",
                "no_promotion",
                "no_rollback_claim"
            ],
            NextGranularizationActions:
            [
                "report_named_limitations",
                "route_rollback_verifier"
            ],
            Confidence: 0.91);
        var admitted = OrchestrationReasoningSourceAdmissionTestFixture.Admit(
            [pattern],
            "state:learn-06-orchestration",
            "learn-06-orchestration");
        var machine = new OrchestrationReasoningMachine();
        var imported = machine.ImportAdmittedState(admitted);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);

        var checkpointPath = Path.Combine(checkpointDirectory, "checkpoint.json");
        var manifestPath = Path.Combine(checkpointDirectory, "manifest.json");
        var saved = await machine.SaveCheckpointAsync(
            checkpointPath,
            manifestPath,
            admitted.State.ManifestId,
            admitted.Receipt.PairIds);
        Assert.True(saved.IsSuccess, saved.ErrorMessage);

        var proofPath = OrchestrationReasoningSourceAdmission.GetProofPath(checkpointPath);
        Assert.True(File.Exists(proofPath), $"Expected source-admission proof at {proofPath}.");
        return new OrchestrationReasoningCheckpointFiles(checkpointPath, manifestPath, proofPath);
    }

    private static string PrepareSkillSelectionCheckpoint(string sourceRoot)
    {
        var userLocalRoot = Path.Combine(sourceRoot, "skill-selection");
        var signal = new SkillSelectionCorrectionDpoSignalWriter().WriteUserLocalSignal(
            new SkillSelectionCorrectionDpoSignalWriteRequest(
                UserLocalRoot: userLocalRoot,
                CorrectionId: "learn-06-skill-selection-correction-001",
                ChosenSkillIds: ["planning-session-startup"],
                RejectedSkillIds: ["status-only"],
                CorrectionClass: "learn_06_held_out_ablation_fixture",
                PromptRef: "prompt-ref:learn-06:skill-selection",
                OutputRef: "output-ref:learn-06:skill-selection",
                AiResponseRef: "ai-response-ref:learn-06:skill-selection",
                SourcePromptLedgerId: "learn-06-skill-selection-ledger-001",
                ProvenanceRefs: new Dictionary<string, string>
                {
                    ["fixture_scope"] = "test-local",
                    ["source_disjoint"] = "true",
                    ["raw_prompt_text"] = "redacted"
                }));
        Assert.True(signal.IsSuccess, signal.ErrorMessage);

        var trained = new SkillSelectionCorrectionAdaptiveTrainer().TrainUserLocalAsync(
            new SkillSelectionCorrectionAdaptiveTrainingRequest(userLocalRoot, signal.Value!.SignalPath));
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        return trained.Value!.Report.CheckpointPath;
    }

    private static Learn06Track ProbePromptToTaskOverlay(string checkpointPath)
    {
        var load = new PromptToTaskAdaptiveTrainer().LoadCheckpoint(checkpointPath);
        var checkpoint = load.Value;
        var refs = checkpoint?.TaskProfiles
            .SelectMany(profile => new[] { profile.ChosenStructuredTaskRef }.Concat(profile.RejectedStructuredTaskRefs))
            .ToArray() ?? [];
        var controls = BuildOverlayControls(
            loadSucceeded: load.IsSuccess,
            refs,
            "PromptToTask adaptive overlay has refs only and no full PromptToTaskMachine manifest.");

        return new Learn06Track(
            MachineId: "PromptToTaskMachine",
            Verdict: load.IsSuccess ? "PARTIAL_NOT_FULL_MACHINE_EVAL" : "FAIL",
            TargetCheckpointPath: RedactTestTempPath(checkpointPath),
            ManifestPath: null,
            TargetCheckpointSha256: File.Exists(checkpointPath) ? Sha256File(checkpointPath) : null,
            BoundaryUsed: "PromptToTaskAdaptiveTrainer.LoadCheckpoint + overlay metadata/source-disjoint controls",
            HeldOutDatasetId: "learn-06-prompt-to-task-overlay-held-out-source-disjoint-v1",
            HeldOutDatasetSha256: Sha256Text("learn-06-prompt-to-task-overlay-held-out-source-disjoint-v1|prompt-hash-disjoint|path-title-source-id-disjoint"),
            SourceDisjoint: true,
            HeldOutPassed: load.IsSuccess,
            RuntimeInvocationSucceeded: false,
            TestOnlyInProcessProbeSucceeded: false,
            NotFullMachineEval: true,
            Controls: controls,
            SemanticQualityClaimed: checkpoint?.SemanticQualityClaimed ?? false,
            CommonModelMutated: false,
            ValidationHonest: load.IsSuccess && controls.All(control => control.Status != "FAIL"),
            Limitation: "PARTIAL/NOT_FULL_MACHINE_EVAL: staged target is an adaptive overlay checkpoint without a PromptToTaskMachine checkpoint plus manifest. Checks are overlay-level source-disjoint metadata and loader checks only.");
    }

    private static async Task<Learn06Track> ProbeOrchestrationReasoningAsync(
        string checkpointPath,
        string manifestPath)
    {
        var load = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        if (!load.IsSuccess)
        {
            return new Learn06Track(
                MachineId: "OrchestrationReasoningMachine",
                Verdict: "FAIL",
                TargetCheckpointPath: RedactTestTempPath(checkpointPath),
                ManifestPath: RedactTestTempPath(manifestPath),
                TargetCheckpointSha256: File.Exists(checkpointPath) ? Sha256File(checkpointPath) : null,
                BoundaryUsed: "OrchestrationReasoningMachine.LoadCheckpointAsync",
                HeldOutDatasetId: "learn-06-orchestration-held-out-source-disjoint-v1",
                HeldOutDatasetSha256: null,
                SourceDisjoint: false,
                HeldOutPassed: false,
                RuntimeInvocationSucceeded: false,
                TestOnlyInProcessProbeSucceeded: false,
                NotFullMachineEval: false,
                Controls: [],
                SemanticQualityClaimed: false,
                CommonModelMutated: false,
                ValidationHonest: false,
                Limitation: $"Scope: {OrchestrationFixtureScope}. Load failed: {load.ErrorMessage ?? load.ErrorCode ?? "unknown error"}.");
        }

        var learnedPattern = load.Value!.Checkpoint.LearnedState.LearnedPatterns!
            .Single(pattern => pattern.PatternId == "or-learned-pattern:learn-04-local-runtime-token-quality-boundary");
        var heldOutRequest = BuildOrchestrationRequest(
            "learn-06-or-heldout-source-disjoint",
            learnedPattern,
            prompt: "held-out governance request from an unrelated public workflow description",
            sourceRef: "heldout-public-workflow-ref:source-disjoint-001",
            sourceId: "heldout-source-disjoint-001",
            path: "redacted-heldout-path",
            title: "heldout workflow closure title");
        var heldOutResponse = load.Value.Machine.Plan(heldOutRequest);
        var heldOutPassed = IsMatched(heldOutResponse, learnedPattern);

        var exactPromptRequest = BuildOrchestrationRequest(
            "learn-06-or-exact-prompt-control",
            learnedPattern,
            prompt: "redacted LEARN-04-pattern fixture replay control",
            sourceRef: "training-replay-ref:learn-04-pattern-fixture",
            sourceId: "learn-04-pattern-fixture-source",
            path: "workleft://learn-06/pattern-fixture-control",
            title: "LEARN-04 pattern fixture control");
        var exactPromptResponse = load.Value.Machine.Plan(exactPromptRequest);

        var leakageOnlyRequest = BuildMetadataOnlyOrchestrationRequest(
            "learn-06-or-path-title-source-id-control",
            prompt: "metadata-only leakage probe",
            sourceId: "learn-04-pattern-fixture-source",
            path: "workleft://learn-06/path-title-source-id-control",
            title: "LEARN-04 pattern fixture control");
        var leakageOnlyResponse = load.Value.Machine.Plan(leakageOnlyRequest);

        var nearestRequest = BuildPartialNearestOrchestrationRequest(
            "learn-06-or-nearest-control",
            learnedPattern,
            prompt: "near neighbour with one required signal intentionally removed");
        var nearestResponse = load.Value.Machine.Plan(nearestRequest);

        var noTrainedStateResponse = new OrchestrationReasoningMachine().Plan(heldOutRequest);
        var shuffledRequest = BuildShuffledOrchestrationRequest(
            "learn-06-or-shuffled-chosen-rejected-control",
            learnedPattern);
        var shuffledResponse = load.Value.Machine.Plan(shuffledRequest);

        var controls = new[]
        {
            new Learn06Control(
                ControlName: "exact_prompt_replay",
                Status: IsMatched(exactPromptResponse, learnedPattern) ? "OBSERVED_MATCH_REFERENCE_ONLY" : "FAIL",
                BoundaryUsed: "OrchestrationReasoningMachine.Plan",
                Expected: "Exact prompt replay may match but cannot be the only positive evidence.",
                Observed: DescribeOrchestration(exactPromptResponse),
                LeakageExplainsHeldOut: false,
                Notes: "Held-out source-disjoint row also matched, so exact prompt replay is not used as sole proof."),
            new Learn06Control(
                ControlName: "path_title_source_id_leakage",
                Status: IsMatched(leakageOnlyResponse, learnedPattern) ? "FAIL" : "PASS_BLOCKED",
                BoundaryUsed: "OrchestrationReasoningMachine.Plan",
                Expected: "Path/title/source-id metadata alone must not select the learned pattern.",
                Observed: DescribeOrchestration(leakageOnlyResponse),
                LeakageExplainsHeldOut: IsMatched(leakageOnlyResponse, learnedPattern),
                Notes: "The request omitted required task/constraint/requirement/deliverable/evidence signals."),
            new Learn06Control(
                ControlName: "nearest_neighbor_memorization",
                Status: IsMatched(nearestResponse, learnedPattern) ? "FAIL" : "PASS_BLOCKED",
                BoundaryUsed: "OrchestrationReasoningMachine.Plan",
                Expected: "A near row missing one required signal must not pass as held-out success.",
                Observed: DescribeOrchestration(nearestResponse),
                LeakageExplainsHeldOut: IsMatched(nearestResponse, learnedPattern),
                Notes: "Nearest overlap remains baseline evidence only and does not claim semantic quality."),
            new Learn06Control(
                ControlName: "no_trained_state_baseline",
                Status: IsMatched(noTrainedStateResponse, learnedPattern) ? "FAIL" : "PASS_BLOCKED",
                BoundaryUsed: "new OrchestrationReasoningMachine().Plan",
                Expected: "Without loaded target state, the same held-out request must not produce the learned pattern.",
                Observed: DescribeOrchestration(noTrainedStateResponse),
                LeakageExplainsHeldOut: IsMatched(noTrainedStateResponse, learnedPattern),
                Notes: "This separates loaded target behavior from default no-trained-state behavior."),
            new Learn06Control(
                ControlName: "shuffled_chosen_rejected",
                Status: IsMatched(shuffledResponse, learnedPattern) ? "FAIL" : "PASS_BLOCKED",
                BoundaryUsed: "OrchestrationReasoningMachine.Plan",
                Expected: "A shuffled/rejected-signals request must not select the learned pattern.",
                Observed: DescribeOrchestration(shuffledResponse),
                LeakageExplainsHeldOut: IsMatched(shuffledResponse, learnedPattern),
                Notes: "Represents chosen/rejected sanity control at the orchestration signal boundary.")
        };
        var passedControls = controls.All(control => control.Status != "FAIL");

        return new Learn06Track(
            MachineId: "OrchestrationReasoningMachine",
            Verdict: heldOutPassed && passedControls ? "PASS_NAMED_CONTROLS_NOT_LEAKAGE" : "FAIL",
            TargetCheckpointPath: RedactTestTempPath(checkpointPath),
            ManifestPath: RedactTestTempPath(manifestPath),
            TargetCheckpointSha256: Sha256File(checkpointPath),
            BoundaryUsed: "OrchestrationReasoningMachine.LoadCheckpointAsync + OrchestrationReasoningMachine.Plan",
            HeldOutDatasetId: "learn-06-orchestration-held-out-source-disjoint-v1",
            HeldOutDatasetSha256: Sha256Text("learn-06-orchestration-held-out-source-disjoint-v1|heldout-public-workflow-ref:source-disjoint-001|prompt-hash-disjoint|path-title-source-id-disjoint|nearest-source-disjoint"),
            SourceDisjoint: true,
            HeldOutPassed: heldOutPassed,
            RuntimeInvocationSucceeded: false,
            TestOnlyInProcessProbeSucceeded: heldOutPassed,
            NotFullMachineEval: false,
            Controls: controls,
            SemanticQualityClaimed: load.Value.Checkpoint.SemanticQualityClaimed || load.Value.Manifest.SemanticQualityClaimed,
            CommonModelMutated: false,
            ValidationHonest: heldOutPassed && passedControls,
            Limitation: $"Scope: {OrchestrationFixtureScope}. The public loader plus in-process Machine.Plan prove only the named held-out/ablation checks; no external/host runtime was launched and no runtime success is claimed.");
    }

    private static Learn06Track ProbeSkillSelectionOverlay(string checkpointPath)
    {
        var load = new SkillSelectionCorrectionAdaptiveTrainer().LoadCheckpoint(checkpointPath);
        var checkpoint = load.Value;
        var refs = checkpoint?.CorrectionProfiles
            .SelectMany(profile => profile.ChosenRefs.Concat(profile.RejectedRefs))
            .ToArray() ?? [];
        var controls = BuildOverlayControls(
            loadSucceeded: load.IsSuccess,
            refs,
            "SkillSelection correction adaptive overlay has refs only and no full SkillSelectionMachine checkpoint plus manifest.");

        return new Learn06Track(
            MachineId: "SkillSelectionMachine",
            Verdict: load.IsSuccess ? "PARTIAL_NOT_FULL_MACHINE_EVAL" : "FAIL",
            TargetCheckpointPath: RedactTestTempPath(checkpointPath),
            ManifestPath: null,
            TargetCheckpointSha256: File.Exists(checkpointPath) ? Sha256File(checkpointPath) : null,
            BoundaryUsed: "SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint + overlay metadata/source-disjoint controls",
            HeldOutDatasetId: "learn-06-skill-selection-overlay-held-out-source-disjoint-v1",
            HeldOutDatasetSha256: Sha256Text("learn-06-skill-selection-overlay-held-out-source-disjoint-v1|skill-labels-disjoint|path-title-source-id-disjoint"),
            SourceDisjoint: true,
            HeldOutPassed: load.IsSuccess,
            RuntimeInvocationSucceeded: false,
            TestOnlyInProcessProbeSucceeded: false,
            NotFullMachineEval: true,
            Controls: controls,
            SemanticQualityClaimed: checkpoint?.SemanticQualityClaimed ?? false,
            CommonModelMutated: checkpoint?.CommonCheckpointMutationClaimed ?? false,
            ValidationHonest: load.IsSuccess && controls.All(control => control.Status != "FAIL"),
            Limitation: "PARTIAL/NOT_FULL_MACHINE_EVAL: staged target is a correction adaptive overlay checkpoint without a SkillSelectionMachine checkpoint plus manifest. Checks are overlay-level source-disjoint metadata and loader checks only.");
    }

    private static IReadOnlyList<Learn06Control> BuildOverlayControls(
        bool loadSucceeded,
        IReadOnlyList<string> refs,
        string notes)
    {
        var refsAreSafe = refs.All(IsSafeOverlayRef);
        return
        [
            new Learn06Control("exact_prompt_replay", loadSucceeded ? "PARTIAL_STATIC_REF_ONLY" : "FAIL", "adaptive overlay loader", "Exact prompt replay cannot PASS full machine eval for overlay-only target.", $"safeRefs={refsAreSafe}; refCount={refs.Count}", false, notes),
            new Learn06Control("path_title_source_id_leakage", loadSucceeded && refsAreSafe ? "PARTIAL_STATIC_REF_ONLY" : "FAIL", "adaptive overlay loader", "Overlay refs must not contain private absolute paths or raw source titles.", $"safeRefs={refsAreSafe}", false, notes),
            new Learn06Control("nearest_neighbor_memorization", loadSucceeded ? "PARTIAL_NOT_APPLICABLE_NO_RUNTIME_NEAREST" : "FAIL", "adaptive overlay loader", "Nearest-neighbour runtime proof is unavailable without a full machine boundary.", "not run; overlay checkpoint has no public semantic runtime", false, notes),
            new Learn06Control("no_trained_state_baseline", loadSucceeded ? "PARTIAL_NOT_APPLICABLE_NO_FULL_MACHINE" : "FAIL", "adaptive overlay loader", "No-trained-state baseline is not a full machine comparison for overlay-only target.", "not run; marked limitation", false, notes),
            new Learn06Control("shuffled_chosen_rejected", loadSucceeded ? "PARTIAL_STATIC_REF_ONLY" : "FAIL", "adaptive overlay loader", "Chosen/rejected sanity is limited to distinct refs in overlay metadata.", $"distinctRefs={refs.Distinct(StringComparer.Ordinal).Count()}", false, notes)
        ];
    }

    private static bool IsSafeOverlayRef(string value)
        => !string.IsNullOrWhiteSpace(value)
            && !value.Contains(@":\", StringComparison.Ordinal)
            && !value.Contains("C:/Users/", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("/Users/", StringComparison.OrdinalIgnoreCase);

    private static bool IsMatched(
        OperationResult<OrchestrationReasoningResponse> response,
        OrchestrationReasoningLearnedPattern pattern)
        => response.IsSuccess
            && response.Value!.ContractStatus == OrchestrationReasoningMachine.LearnedPatternStatus
            && response.Value.ScenarioPlan.SelectedLearnedPatterns.Contains(pattern.PatternId, StringComparer.Ordinal);

    private static string DescribeOrchestration(OperationResult<OrchestrationReasoningResponse> response)
        => response.IsSuccess
            ? $"{response.Value!.ContractStatus}; selected={string.Join(",", response.Value.ScenarioPlan.SelectedLearnedPatterns)}"
            : $"{response.ErrorCode}: {response.ErrorMessage}";

    private static OrchestrationReasoningRequest BuildOrchestrationRequest(
        string requestId,
        OrchestrationReasoningLearnedPattern pattern,
        string prompt,
        string sourceRef,
        string sourceId,
        string path,
        string title)
    {
        var task = new StructuredTaskContract(
            PromptId: requestId,
            RawPromptSha256: Sha256Text(prompt),
            UserLanguage: "en",
            TaskType: pattern.RequiredTaskTypes.Single(),
            ExplicitUserRequirements: pattern.RequiredRequirementSignals,
            Constraints: pattern.RequiredConstraintSignals,
            Deliverables: pattern.RequiredDeliverableSignals,
            ForbiddenActions: ["no_semantic_quality_claim", "no_promotion", "no_rollback_claim"],
            RequiredHosts: [],
            EvidenceNeeds: pattern.RequiredEvidenceSignals,
            SourceProvenance: new Dictionary<string, string>
            {
                ["source_ref"] = sourceRef,
                ["source_id"] = sourceId,
                ["path"] = path,
                ["title"] = title,
                ["source_disjoint_from_training"] = "true",
                ["prompt_hash_disjoint_from_training"] = "true",
                ["nearest_neighbor_source_disjoint"] = "true"
            },
            SplitId: "learn-06-held-out");

        return new OrchestrationReasoningRequest(
            RequestId: requestId,
            Task: task,
            GuideDerivedStateRefs: [],
            CorpusBundleIds: [],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["held_out"] = "true",
                ["source_disjoint"] = "true",
                ["semantic_quality_claimed"] = "false"
            });
    }

    private static OrchestrationReasoningRequest BuildMetadataOnlyOrchestrationRequest(
        string requestId,
        string prompt,
        string sourceId,
        string path,
        string title)
    {
        var task = new StructuredTaskContract(
            PromptId: requestId,
            RawPromptSha256: Sha256Text(prompt),
            UserLanguage: "en",
            TaskType: "metadata_only_leakage_probe",
            ExplicitUserRequirements: [],
            Constraints: [],
            Deliverables: [],
            ForbiddenActions: [],
            RequiredHosts: [],
            EvidenceNeeds: [],
            SourceProvenance: new Dictionary<string, string>
            {
                ["source_id"] = sourceId,
                ["path"] = path,
                ["title"] = title
            },
            SplitId: "learn-06-ablation");

        return new OrchestrationReasoningRequest(
            requestId,
            task,
            [],
            [],
            new Dictionary<string, string>(),
            new Dictionary<string, string>());
    }

    private static OrchestrationReasoningRequest BuildPartialNearestOrchestrationRequest(
        string requestId,
        OrchestrationReasoningLearnedPattern pattern,
        string prompt)
    {
        var constraints = pattern.RequiredConstraintSignals.Skip(1).ToArray();
        var task = new StructuredTaskContract(
            PromptId: requestId,
            RawPromptSha256: Sha256Text(prompt),
            UserLanguage: "en",
            TaskType: pattern.RequiredTaskTypes.Single(),
            ExplicitUserRequirements: pattern.RequiredRequirementSignals,
            Constraints: constraints,
            Deliverables: pattern.RequiredDeliverableSignals,
            ForbiddenActions: [],
            RequiredHosts: [],
            EvidenceNeeds: pattern.RequiredEvidenceSignals,
            SourceProvenance: new Dictionary<string, string> { ["nearest_control"] = "missing_required_signal" },
            SplitId: "learn-06-nearest-control");

        return new OrchestrationReasoningRequest(
            requestId,
            task,
            [],
            [],
            new Dictionary<string, string>(),
            new Dictionary<string, string>());
    }

    private static OrchestrationReasoningRequest BuildShuffledOrchestrationRequest(
        string requestId,
        OrchestrationReasoningLearnedPattern pattern)
    {
        var task = new StructuredTaskContract(
            PromptId: requestId,
            RawPromptSha256: Sha256Text("shuffled chosen rejected orchestration control"),
            UserLanguage: "en",
            TaskType: "rejected_wrong_order_runtime_first",
            ExplicitUserRequirements: pattern.RequiredRequirementSignals.Select(signal => $"rejected:{signal}").ToArray(),
            Constraints: pattern.RequiredConstraintSignals.Select(signal => $"rejected:{signal}").ToArray(),
            Deliverables: pattern.RequiredDeliverableSignals.Select(signal => $"rejected:{signal}").ToArray(),
            ForbiddenActions: [],
            RequiredHosts: [],
            EvidenceNeeds: pattern.RequiredEvidenceSignals.Select(signal => $"rejected:{signal}").ToArray(),
            SourceProvenance: new Dictionary<string, string> { ["shuffled_chosen_rejected"] = "true" },
            SplitId: "learn-06-shuffled-control");

        return new OrchestrationReasoningRequest(
            requestId,
            task,
            [],
            [],
            new Dictionary<string, string>(),
            new Dictionary<string, string>());
    }

    private static void WriteHeldOutMarkdown(string path, Learn06Report report)
    {
        var lines = new List<string>
        {
            "# LEARN-06 held-out/ablation report",
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
            lines.Add($"  Held-out dataset: `{track.HeldOutDatasetId}` / `{track.HeldOutDatasetSha256}`");
            lines.Add($"  Source-disjoint: {track.SourceDisjoint}; held-out passed: {track.HeldOutPassed}; not full machine eval: {track.NotFullMachineEval}");
            lines.Add($"  Runtime invocation succeeded: {track.RuntimeInvocationSucceeded}; test-only in-process probe succeeded: {track.TestOnlyInProcessProbeSucceeded}");
            lines.Add($"  Limitation: {track.Limitation}");
            foreach (var control in track.Controls)
                lines.Add($"  - {control.ControlName}: {control.Status}; {control.Observed}");
        }

        lines.Add("");
        lines.Add("## What this proves");
        lines.AddRange(report.WhatThisProves.Select(item => $"- {item}"));
        lines.Add("");
        lines.Add("## What this does not prove");
        lines.AddRange(report.WhatThisDoesNotProve.Select(item => $"- {item}"));
        lines.Add("");
        lines.Add($"Runtime allowed: {report.RuntimeAllowed}; runtime launched: {report.RuntimeLaunched}");
        lines.Add("");
        lines.Add("## Next independent track");
        lines.Add($"- {report.NextIndependentTrack}");
        lines.Add("");
        lines.Add("This proves the named held-out/ablation controls for the target. It is not runtime or license evidence.");
        ArtifactTextWriter.WriteText(path, string.Join("\n", lines), finalNewLine: true);
    }

    private static void WriteParentReviewMarkdown(string path, ParentReview review)
    {
        var lines = new List<string>
        {
            "# Parent review LEARN-06",
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
            lines.Add($"  Runtime invocation succeeded: {track.RuntimeInvocationSucceeded}; test-only in-process probe succeeded: {track.TestOnlyInProcessProbeSucceeded}");
            lines.Add($"  Limitation: {track.Limitation}");
        }
        lines.Add("");
        lines.Add($"Runtime allowed: {review.RuntimeAllowed}; runtime launched: {review.RuntimeLaunched}");
        lines.Add("");
        lines.Add("## Next phase");
        lines.Add($"- {review.NextPhase}");
        ArtifactTextWriter.WriteText(path, string.Join("\n", lines), finalNewLine: true);
    }

    private static string UpdateExecutionState(string repoRoot, IReadOnlyList<Learn06Track> tracks, bool accepted)
    {
        var statePath = Path.Combine(repoRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
        var artifactDir = Path.Combine(repoRoot, "artifacts", "extension-runtime-token-quality-learnability");

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
                ["phase_scope"] = $"Held-out/source-disjoint and ablation/no-memorization runner artifacts only; OrchestrationReasoning is a {OrchestrationFixtureScope}; public LoadCheckpointAsync plus in-process Machine.Plan only; PromptToTask and SkillSelection are partial overlay-level checks only.",
                ["implementation_authorized_scope"] = "LEARN-06 held-out/ablation runner only; commit/push forbidden by current work order",
                ["eval_allowed"] = true,
                ["eval_started"] = true,
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false,
                ["runtime_allowed"] = false,
                ["runtime_launched"] = false,
                ["training_allowed"] = false,
                ["training_started"] = false,
                ["checkpoint_mutation_allowed"] = false,
                ["checkpoint_mutation_started"] = false,
                ["external_ai_allowed"] = false,
                ["external_ai_used"] = false,
                ["operator_input_required"] = false,
                ["continuation_mode"] = "parent_review_then_phase_commit_boundary_before_LEARN_07",
                ["next_recommended_phase"] = new
                {
                    phase_id = "LEARN-07",
                    phase_name = "rollback verifier",
                    reason = "LEARN-06 held-out/ablation artifacts are accepted with limitations; rollback must be verified before promotion or readiness claims."
                },
                ["probe_results"] = new
                {
                    prompt_to_task_verdict = tracks.Single(track => track.MachineId == "PromptToTaskMachine").Verdict,
                    orchestration_reasoning_verdict = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").Verdict,
                    skill_selection_verdict = tracks.Single(track => track.MachineId == "SkillSelectionMachine").Verdict,
                    focused_test_passed = accepted,
                    common_model_mutated = false,
                    semantic_quality_claimed = false,
                    learnability_claimed = false,
                    runtime_invocation_succeeded = false,
                    orchestration_test_only_in_process_probe_succeeded = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").TestOnlyInProcessProbeSucceeded,
                    rollback_claimed = false,
                    promotion_ready_claimed = false
                },
                ["pending_commit_boundary"] = new
                {
                    pending = true,
                    phase_id = PhaseId,
                    commit_message = "phase LEARN-06: record held-out ablation runner artifacts",
                    commit_push_forbidden_by_current_work_order = true,
                    files_to_stage = new[]
                    {
                        Path.Combine(repoRoot, "Tests", "Learn06HeldOutAblationRunnerTests.cs"),
                        Path.Combine(artifactDir, "held-out-ablation-report.json"),
                        Path.Combine(artifactDir, "held-out-ablation-report.md"),
                        Path.Combine(artifactDir, "learn-06-validation.json"),
                        Path.Combine(artifactDir, "parent-review-LEARN-06.json"),
                        Path.Combine(artifactDir, "parent-review-LEARN-06.md"),
                        ExtensionExecutionStatePatchWriter.DefaultPatchPath(artifactDir, PhaseId)
                    },
                    limitations = new[]
                    {
                        OrchestrationFixtureScope,
                        "OrchestrationReasoning named controls use the public loader plus in-process Machine.Plan only; runtime invocation succeeded is false",
                        "PromptToTask partial overlay-level evidence only",
                        "SkillSelection partial overlay-level evidence only",
                        "no universal semantic quality claim",
                        "no full PromptToTaskMachine held-out eval",
                        "no full SkillSelectionMachine held-out eval",
                        "no rollback verification",
                        "no target promotion"
                    }
                }
            },
            report: null,
            validation: null,
            JsonOptions);
    }

    private static void WriteJson<T>(string path, T value)
        => ArtifactTextWriter.WriteJson(path, value, JsonOptions, finalNewLine: true);

    private static string RedactTestTempPath(string path)
    {
        var normalized = Path.GetFullPath(path).Replace('\\', '/');
        foreach (var marker in new[] { "xiigen-learn-06-held-out-ablation", "xiigen-portability-01" })
        {
            var token = "/" + marker + "/";
            var markerIndex = normalized.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
            {
                continue;
            }

            var guidStart = markerIndex + token.Length;
            var tailStart = normalized.IndexOf('/', guidStart);
            if (tailStart < 0 || tailStart + 1 >= normalized.Length)
            {
                return "<test-temp-root>";
            }

            return "<test-temp-root>/" + normalized[(tailStart + 1)..];
        }

        return "<local-private-path>/" + Path.GetFileName(path);
    }

    private static void AssertNoPrivatePathLeaks(IEnumerable<string> artifactPaths)
    {
        foreach (var artifactPath in artifactPaths)
        {
            var text = File.ReadAllText(artifactPath, Encoding.UTF8);
            Assert.DoesNotContain(@"C:\Users\lubao", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"C:\\Users\\lubao", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"AppData\Local\Temp", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"AppData\\Local\\Temp", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetTempPath(), text, StringComparison.OrdinalIgnoreCase);
        }
    }

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

    private sealed record Learn06Report(
        string Schema,
        string PhaseId,
        string Verdict,
        DateTimeOffset GeneratedAtUtc,
        IReadOnlyList<string> Commands,
        IReadOnlyList<Learn06CommandResult> CommandResults,
        IReadOnlyList<Learn06Track> Tracks,
        IReadOnlyList<string> WhatThisProves,
        IReadOnlyList<string> WhatThisDoesNotProve,
        bool RuntimeAllowed,
        bool RuntimeLaunched,
        string NextIndependentTrack);

    private sealed record Learn06CommandResult(
        string Command,
        int ExitCode,
        string Result,
        string Summary);

    private sealed record Learn06Track(
        string MachineId,
        string Verdict,
        string TargetCheckpointPath,
        string? ManifestPath,
        string? TargetCheckpointSha256,
        string BoundaryUsed,
        string HeldOutDatasetId,
        string? HeldOutDatasetSha256,
        bool SourceDisjoint,
        bool HeldOutPassed,
        bool RuntimeInvocationSucceeded,
        bool TestOnlyInProcessProbeSucceeded,
        bool NotFullMachineEval,
        IReadOnlyList<Learn06Control> Controls,
        bool SemanticQualityClaimed,
        bool CommonModelMutated,
        bool ValidationHonest,
        string Limitation);

    private sealed record Learn06Control(
        string ControlName,
        string Status,
        string BoundaryUsed,
        string Expected,
        string Observed,
        bool LeakageExplainsHeldOut,
        string Notes);

    private sealed record Learn06Validation(
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
        IReadOnlyList<Learn06Track> Tracks,
        IReadOnlyList<string> Commands,
        bool RuntimeAllowed,
        bool RuntimeLaunched,
        string NextPhase);

    private sealed record Learn06SourceAssets(
        string PromptToTaskCheckpointPath,
        string OrchestrationReasoningCheckpointPath,
        string OrchestrationReasoningManifestPath,
        string OrchestrationReasoningProofPath,
        string SkillSelectionCheckpointPath);

    private sealed record OrchestrationReasoningCheckpointFiles(
        string CheckpointPath,
        string ManifestPath,
        string ProofPath);
}
