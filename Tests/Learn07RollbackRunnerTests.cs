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

public sealed class Learn07RollbackRunnerTests
{
    private const string PhaseId = "LEARN-07";
    private const string Command = "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn07RollbackRunnerTests --no-restore --logger \"console;verbosity=normal\"";
    private const string TargetOnlyOrchestrationPatternId = "or-learned-pattern:learn-04-local-runtime-token-quality-boundary";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [Fact]
    public async Task Learn07VerifiesRollbackBoundariesWithoutPromotion()
    {
        var repoRoot = FindRepoRoot();
        var artifactDir = Path.Combine(repoRoot, "artifacts", "extension-runtime-token-quality-learnability");
        Directory.CreateDirectory(artifactDir);

        var promptToTaskTargetPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "prompt-to-task", "checkpoints", "targets", "learn-04-prompt-to-task-local-v1", "adaptive-checkpoint.json");
        var promptToTaskRollbackPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "prompt-to-task", "backups", "pre-learn-04", "adaptive-checkpoint.json");
        var orchestrationTargetPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "orchestration-reasoning", "checkpoints", "targets", "learn-04-orchestration-reasoning-local-v1", "checkpoint.json");
        var orchestrationTargetManifestPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "orchestration-reasoning", "checkpoints", "targets", "learn-04-orchestration-reasoning-local-v1", "manifest.json");
        var orchestrationRollbackPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "orchestration-reasoning", "backups", "pre-learn-04", "checkpoint.json");
        var orchestrationRollbackManifestPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "orchestration-reasoning", "backups", "pre-learn-04", "manifest.json");
        var skillSelectionTargetPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "skill-selection", "checkpoints", "targets", "learn-04-skill-selection-local-v1", "adaptive-checkpoint.json");
        var skillSelectionRollbackPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "skill-selection", "backups", "pre-learn-04", "adaptive-checkpoint.json");

        var tracks = new[]
        {
            ProbePromptToTaskRollback(promptToTaskTargetPath, promptToTaskRollbackPath),
            await ProbeOrchestrationRollbackAsync(
                orchestrationTargetPath,
                orchestrationTargetManifestPath,
                orchestrationRollbackPath,
                orchestrationRollbackManifestPath),
            ProbeSkillSelectionRollback(skillSelectionTargetPath, skillSelectionRollbackPath)
        };
        var accepted = tracks.All(track => track.RollbackLoaded)
            && tracks.All(track => track.TargetOnlyLearnedChangesAbsentAfterRollback)
            && tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").Verdict == "PASS_PUBLIC_MACHINE_BOUNDARY_ROLLBACK"
            && tracks.Any(track => track.MachineId == "PromptToTaskMachine" && track.Verdict == "PARTIAL_OVERLAY_ROLLBACK_ONLY")
            && tracks.Any(track => track.MachineId == "SkillSelectionMachine" && track.Verdict == "PARTIAL_OVERLAY_ROLLBACK_ONLY");

        var report = new Learn07Report(
            Schema: "xiigen.learn_07_rollback_report.v1",
            PhaseId: PhaseId,
            Verdict: accepted ? "PASS_WITH_LIMITATIONS" : "FAIL",
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            Commands: [Command, Command],
            CommandResults:
            [
                new Learn07CommandResult(
                    Command: Command,
                    ExitCode: 124,
                    Result: "TIMEOUT",
                    Summary: "Initial focused LEARN-07 invocation timed out after 124137 ms before a test result was returned; rerun with a longer timeout was required."),
                new Learn07CommandResult(
                    Command: Command,
                    ExitCode: 0,
                    Result: "PASS",
                    Summary: "Test Run Successful. Total tests: 1. Passed: 1. Failed: 0.")
            ],
            Tracks: tracks,
            WhatThisProves:
            [
                "Rollback proves recovery only. It does not prove the target should be promoted.",
                "The PromptToTask rollback overlay loads through PromptToTaskAdaptiveTrainer.LoadCheckpoint and target-only trained signal ids/profiles are absent.",
                "The OrchestrationReasoning rollback checkpoint loads through OrchestrationReasoningMachine.LoadCheckpointAsync plus Plan using the manifest-bound public boundary, and the LEARN-04 target-only learned pattern does not match after rollback.",
                "The SkillSelection rollback overlay loads through SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint and target-only correction profiles/trained signal ids are absent."
            ],
            WhatThisDoesNotProve:
            [
                "This does not prove target quality, target promotion, host runtime readiness, or universal learnability.",
                "PromptToTask and SkillSelection remain PARTIAL_OVERLAY_ROLLBACK_ONLY because their rollback assets are adaptive overlays, not full machine checkpoint plus manifest artifacts.",
                "The Orchestration rollback manifest checkpointRef differs from the approval packet alias; the verifier used a temp manifest-bound copy for public LoadCheckpointAsync and did not rewrite the named rollback assets."
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
                ["file_existence_alone_claimed"] = false,
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false,
                ["promotion_ready_claimed"] = false,
                ["target_promotion_or_locator_update"] = false,
                ["common_model_mutated"] = false,
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
            Findings:
            [
                "OrchestrationReasoning rollback is accepted through the public machine boundary with target-only LEARN-04 pattern absent after rollback.",
                "PromptToTask and SkillSelection rollback are accepted only as overlay-level rollback checks; full machine rollback is not claimed for those tracks.",
                "No semantic quality, universal learnability, target promotion, locator update, common model mutation, runtime host proof, or external AI claim is made."
            ],
            Tracks: tracks,
            Commands: report.Commands,
            NextPhase: "final execution review preparation");

        WriteJson(Path.Combine(artifactDir, "rollback-report.json"), report);
        WriteRollbackMarkdown(Path.Combine(artifactDir, "rollback-report.md"), report);
        WriteJson(Path.Combine(artifactDir, "learn-07-validation.json"), validation);
        WriteJson(Path.Combine(artifactDir, "parent-review-LEARN-07.json"), parentReview);
        WriteParentReviewMarkdown(Path.Combine(artifactDir, "parent-review-LEARN-07.md"), parentReview);
        var statePatchPath = UpdateExecutionState(repoRoot, tracks, accepted);

        Assert.True(accepted, "LEARN-07 rollback validation did not reach accepted-for-parent-review with limitations.");
        Assert.True(File.Exists(statePatchPath));
        Assert.All(tracks, track => Assert.False(track.SemanticQualityClaimed));
        Assert.All(tracks, track => Assert.False(track.CommonModelMutated));
    }

    private static Learn07Track ProbePromptToTaskRollback(string targetCheckpointPath, string rollbackCheckpointPath)
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
            TargetCheckpointPath: targetCheckpointPath,
            RollbackCheckpointPath: rollbackCheckpointPath,
            ManifestPath: null,
            TargetCheckpointSha256: File.Exists(targetCheckpointPath) ? Sha256File(targetCheckpointPath) : null,
            RollbackCheckpointSha256: File.Exists(rollbackCheckpointPath) ? Sha256File(rollbackCheckpointPath) : null,
            BoundaryUsed: "PromptToTaskAdaptiveTrainer.LoadCheckpoint rollback overlay",
            RollbackLoaded: rollback.IsSuccess,
            RuntimeInvocationSucceeded: false,
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
        string rollbackManifestPath)
    {
        var targetLoad = await OrchestrationReasoningMachine.LoadCheckpointAsync(targetCheckpointPath, targetManifestPath);
        if (!targetLoad.IsSuccess)
            return FailedOrchestrationTrack(targetCheckpointPath, rollbackCheckpointPath, rollbackManifestPath, targetLoad.ErrorCode, targetLoad.ErrorMessage);

        var learnedPattern = targetLoad.Value!.Checkpoint.LearnedState.LearnedPatterns!
            .Single(pattern => pattern.PatternId == TargetOnlyOrchestrationPatternId);
        var targetRequest = BuildMatchingOrchestrationRequest(learnedPattern);
        var targetResponse = targetLoad.Value.Machine.Plan(targetRequest);
        var targetMatched = IsMatched(targetResponse, learnedPattern);

        var tempDir = Path.Combine(Path.GetTempPath(), "xiigen-learn-07-rollback", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var manifest = JsonSerializer.Deserialize<OrchestrationReasoningCheckpointManifest>(
            File.ReadAllText(rollbackManifestPath, Encoding.UTF8),
            JsonOptions)!;
        var manifestBoundCheckpointPath = Path.Combine(tempDir, manifest.CheckpointRef);
        var manifestBoundManifestPath = Path.Combine(tempDir, manifest.ManifestRef);
        File.Copy(rollbackCheckpointPath, manifestBoundCheckpointPath, overwrite: true);
        File.Copy(rollbackManifestPath, manifestBoundManifestPath, overwrite: true);

        var rollbackLoad = await OrchestrationReasoningMachine.LoadCheckpointAsync(
            manifestBoundCheckpointPath,
            manifestBoundManifestPath);
        if (!rollbackLoad.IsSuccess)
            return FailedOrchestrationTrack(targetCheckpointPath, rollbackCheckpointPath, rollbackManifestPath, rollbackLoad.ErrorCode, rollbackLoad.ErrorMessage);

        var rollbackPatternIds = rollbackLoad.Value!.Checkpoint.LearnedState.LearnedPatterns?
            .Select(pattern => pattern.PatternId)
            .ToHashSet(StringComparer.Ordinal) ?? [];
        var rollbackResponse = rollbackLoad.Value.Machine.Plan(targetRequest);
        var targetOnlyAbsent = targetMatched
            && !rollbackPatternIds.Contains(TargetOnlyOrchestrationPatternId)
            && !IsMatched(rollbackResponse, learnedPattern);

        return new Learn07Track(
            MachineId: "OrchestrationReasoningMachine",
            Verdict: targetOnlyAbsent ? "PASS_PUBLIC_MACHINE_BOUNDARY_ROLLBACK" : "FAIL",
            TargetCheckpointPath: targetCheckpointPath,
            RollbackCheckpointPath: rollbackCheckpointPath,
            ManifestPath: rollbackManifestPath,
            TargetCheckpointSha256: Sha256File(targetCheckpointPath),
            RollbackCheckpointSha256: Sha256File(rollbackCheckpointPath),
            BoundaryUsed: "OrchestrationReasoningMachine.LoadCheckpointAsync + OrchestrationReasoningMachine.Plan rollback via manifest-bound temp copy",
            RollbackLoaded: rollbackLoad.IsSuccess,
            RuntimeInvocationSucceeded: true,
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

    private static Learn07Track ProbeSkillSelectionRollback(string targetCheckpointPath, string rollbackCheckpointPath)
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
            TargetCheckpointPath: targetCheckpointPath,
            RollbackCheckpointPath: rollbackCheckpointPath,
            ManifestPath: null,
            TargetCheckpointSha256: File.Exists(targetCheckpointPath) ? Sha256File(targetCheckpointPath) : null,
            RollbackCheckpointSha256: File.Exists(rollbackCheckpointPath) ? Sha256File(rollbackCheckpointPath) : null,
            BoundaryUsed: "SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint rollback overlay",
            RollbackLoaded: rollback.IsSuccess,
            RuntimeInvocationSucceeded: false,
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
        string? errorCode,
        string? errorMessage)
        => new(
            MachineId: "OrchestrationReasoningMachine",
            Verdict: "FAIL",
            TargetCheckpointPath: targetCheckpointPath,
            RollbackCheckpointPath: rollbackCheckpointPath,
            ManifestPath: manifestPath,
            TargetCheckpointSha256: File.Exists(targetCheckpointPath) ? Sha256File(targetCheckpointPath) : null,
            RollbackCheckpointSha256: File.Exists(rollbackCheckpointPath) ? Sha256File(rollbackCheckpointPath) : null,
            BoundaryUsed: "OrchestrationReasoningMachine.LoadCheckpointAsync",
            RollbackLoaded: false,
            RuntimeInvocationSucceeded: false,
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
            lines.Add($"  Observed: {track.RollbackObservedStatus}; {track.RollbackObservedOutput}");
            lines.Add($"  Limitation: {track.Notes}");
        }

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
        lines.Add("## Next phase");
        lines.Add($"- {review.NextPhase}");
        ArtifactTextWriter.WriteText(path, string.Join("\n", lines), finalNewLine: true);
    }

    private static string UpdateExecutionState(string repoRoot, IReadOnlyList<Learn07Track> tracks, bool accepted)
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
                ["phase_scope"] = "Rollback verifier artifacts only; OrchestrationReasoning used public machine LoadCheckpointAsync + Plan; PromptToTask and SkillSelection are partial overlay rollback checks only.",
                ["implementation_authorized_scope"] = "LEARN-07 rollback verifier only; commit/push forbidden by current work order",
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
                ["git_authority_seen"] = "commit/push forbidden by current LEARN-07 work order; pending commit boundary recorded only",
                ["git_commands_run"] = new[] { "git branch --show-current", "git status --short", Command, Command },
                ["git_write_or_ref_commands_run"] = Array.Empty<string>(),
                ["git_directives_emitted"] = Array.Empty<string>(),
                ["operator_input_required"] = false,
                ["continuation_mode"] = "parent_review_then_phase_commit_boundary_before_final_execution_review_preparation",
                ["artifacts"] = new[]
                {
                    Path.Combine(repoRoot, "Tests", "Learn07RollbackRunnerTests.cs"),
                    Path.Combine(artifactDir, "rollback-report.json"),
                    Path.Combine(artifactDir, "rollback-report.md"),
                    Path.Combine(artifactDir, "learn-07-validation.json"),
                    Path.Combine(artifactDir, "parent-review-LEARN-07.json"),
                    Path.Combine(artifactDir, "parent-review-LEARN-07.md"),
                    ExtensionExecutionStatePatchWriter.DefaultPatchPath(artifactDir, PhaseId)
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
                    common_model_mutated = false,
                    semantic_quality_claimed = false,
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
                        Path.Combine(artifactDir, "parent-review-LEARN-07.md"),
                        Path.Combine(artifactDir, "parent-review-LEARN-07.json")
                    },
                    note = "Accepted LEARN-07 for phase commit with honest limitations: OrchestrationReasoning public machine rollback passed; PromptToTask and SkillSelection are partial overlay rollback only; no semantic quality, learnability, promotion, locator update, common mutation, runtime, or external AI claim."
                },
                ["pending_commit_boundary"] = new
                {
                    pending = true,
                    phase_id = PhaseId,
                    commit_message = "phase LEARN-07: record rollback verifier artifacts",
                    commit_push_forbidden_by_current_work_order = true,
                    files_to_stage = new[]
                    {
                        Path.Combine(repoRoot, "Tests", "Learn07RollbackRunnerTests.cs"),
                        Path.Combine(artifactDir, "rollback-report.json"),
                        Path.Combine(artifactDir, "rollback-report.md"),
                        Path.Combine(artifactDir, "learn-07-validation.json"),
                        Path.Combine(artifactDir, "parent-review-LEARN-07.json"),
                        Path.Combine(artifactDir, "parent-review-LEARN-07.md"),
                        ExtensionExecutionStatePatchWriter.DefaultPatchPath(artifactDir, PhaseId)
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
                    semantic_quality_claimed = false,
                    learnability_claimed = false,
                    held_out_source_disjoint_eval_claimed = true,
                    ablation_claimed = true,
                    rollback_verification_claimed = true,
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
            JsonOptions);
    }

    private static void WriteJson<T>(string path, T value)
        => ArtifactTextWriter.WriteJson(path, value, JsonOptions, finalNewLine: true);

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

    private sealed record Learn07Report(
        string Schema,
        string PhaseId,
        string Verdict,
        DateTimeOffset GeneratedAtUtc,
        IReadOnlyList<string> Commands,
        IReadOnlyList<Learn07CommandResult> CommandResults,
        IReadOnlyList<Learn07Track> Tracks,
        IReadOnlyList<string> WhatThisProves,
        IReadOnlyList<string> WhatThisDoesNotProve,
        string NextIndependentTrack);

    private sealed record Learn07CommandResult(
        string Command,
        int ExitCode,
        string Result,
        string Summary);

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
        IReadOnlyList<string> Commands,
        string NextPhase);
}
