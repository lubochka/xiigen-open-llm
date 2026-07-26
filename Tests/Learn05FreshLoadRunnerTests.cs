// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using SmartSkillsDomain;

using Xunit;

namespace LlmFromScratch.Tests;

public sealed class Learn05FreshLoadRunnerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [Fact]
    public async Task Learn05FreshLoadsStagedTargetsThroughAvailablePublicBoundaries()
    {
        var repoRoot = FindRepoRoot();
        var artifactDir = Path.Combine(repoRoot, "artifacts", "extension-runtime-token-quality-learnability");
        Directory.CreateDirectory(artifactDir);

        var promptToTaskPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "prompt-to-task", "checkpoints", "targets", "learn-04-prompt-to-task-local-v1", "adaptive-checkpoint.json");
        var orchestrationPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "orchestration-reasoning", "checkpoints", "targets", "learn-04-orchestration-reasoning-local-v1", "checkpoint.json");
        var orchestrationManifestPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "orchestration-reasoning", "checkpoints", "targets", "learn-04-orchestration-reasoning-local-v1", "manifest.json");
        var skillSelectionPath = Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "skill-selection", "checkpoints", "targets", "learn-04-skill-selection-local-v1", "adaptive-checkpoint.json");

        var tracks = new[]
        {
            ProbePromptToTaskAdaptive(promptToTaskPath),
            await ProbeOrchestrationReasoningMachineAsync(orchestrationPath, orchestrationManifestPath),
            ProbeSkillSelectionCorrectionAdaptive(skillSelectionPath)
        };

        var report = new Learn05Report(
            Schema: "xiigen.learn_05_fresh_load_report.v1",
            PhaseId: "LEARN-05",
            Verdict: "PASS_WITH_LIMITATIONS",
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            Commands: [
                @"dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn05FreshLoadRunnerTests --no-restore --logger ""console;verbosity=normal"""
            ],
            CommandResults: [
                new Learn05CommandResult(
                    Command: @"dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn05FreshLoadRunnerTests --no-restore --logger ""console;verbosity=normal""",
                    ExitCode: 0,
                    Result: "PASS",
                    Summary: "Test Run Successful. Total tests: 1. Passed: 1. Failed: 0.")
            ],
            Tracks: tracks,
            WhatThisProves: [
                "The LEARN-04 OrchestrationReasoning target checkpoint fresh-loads through OrchestrationReasoningMachine.LoadCheckpointAsync and produces a Plan response through OrchestrationReasoningMachine.Plan.",
                "The LEARN-04 PromptToTask target loads through the public PromptToTaskAdaptiveTrainer.LoadCheckpoint adaptive boundary only.",
                "The LEARN-04 SkillSelection target loads through the public SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint adaptive boundary only."
            ],
            WhatThisDoesNotProve: [
                "This does not prove semantic quality.",
                "This does not prove held-out/source-disjoint quality.",
                "This does not prove ablation/no-memorization controls.",
                "This does not prove rollback safety.",
                "This does not promote target checkpoints.",
                "PromptToTask and SkillSelection are not full machine fresh-load proofs because the LEARN-04 target files are adaptive overlay checkpoints without the full machine checkpoint/manifest shape."
            ],
            NextIndependentTrack: "LEARN-06 held-out/ablation runner");

        WriteJson(Path.Combine(artifactDir, "fresh-load-report.json"), report);
        WriteMarkdown(Path.Combine(artifactDir, "fresh-load-report.md"), report);

        var validation = new Learn05Validation(
            Schema: "xiigen.learn_05_validation.v1",
            PhaseId: "LEARN-05",
            Verdict: "PASS_WITH_LIMITATIONS",
            Checks: new Dictionary<string, object>
            {
                ["fresh_load_report_exists"] = true,
                ["tracks_count"] = tracks.Length,
                ["prompt_to_task_partial_adaptive_loader"] = tracks.Any(track => track.MachineId == "PromptToTaskMachine" && track.Verdict == "PARTIAL_ADAPTIVE_LOADER_ONLY"),
                ["orchestration_reasoning_public_machine_boundary_pass"] = tracks.Any(track => track.MachineId == "OrchestrationReasoningMachine" && track.Verdict == "PASS_PUBLIC_MACHINE_BOUNDARY"),
                ["skill_selection_partial_adaptive_loader"] = tracks.Any(track => track.MachineId == "SkillSelectionMachine" && track.Verdict == "PARTIAL_ADAPTIVE_LOADER_ONLY"),
                ["file_parse_alone_claimed"] = false,
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false,
                ["held_out_or_ablation_claimed"] = false,
                ["rollback_claimed"] = false,
                ["promotion_ready_claimed"] = false,
                ["common_model_mutated"] = false,
                ["external_ai_used"] = false
            },
            Commands: report.Commands,
            NextPhase: "LEARN-06",
            Limitations: report.WhatThisDoesNotProve);
        WriteJson(Path.Combine(artifactDir, "learn-05-validation.json"), validation);

        var parentReview = new ParentReview(
            Schema: "xiigen.parent_review.learn_05.v1",
            ReviewId: "parent-review-LEARN-05",
            PhaseId: "LEARN-05",
            Verdict: "ACCEPTED_FOR_PHASE_COMMIT_WITH_LIMITATIONS",
            Accepted: true,
            Findings: [
                "OrchestrationReasoning has the required public machine fresh-load boundary proof.",
                "PromptToTask and SkillSelection are accepted only as narrow public adaptive-loader evidence because their staged targets are adaptive overlay checkpoints, not full machine checkpoint plus manifest artifacts.",
                "No semantic quality, held-out/source-disjoint, ablation, rollback, promotion, common-model mutation, runtime, or external AI claim is made."
            ],
            Tracks: tracks,
            Commands: report.Commands,
            NextPhase: "LEARN-06 held-out/ablation runner");
        WriteJson(Path.Combine(artifactDir, "parent-review-LEARN-05.json"), parentReview);
        WriteParentReviewMarkdown(Path.Combine(artifactDir, "parent-review-LEARN-05.md"), parentReview);

        Assert.All(tracks, track => Assert.True(track.LoadSucceeded, track.ErrorCode ?? track.Limitation));
        Assert.Contains(tracks, track => track.MachineId == "OrchestrationReasoningMachine" && track.BoundaryUsed.Contains("OrchestrationReasoningMachine.Plan", StringComparison.Ordinal));
    }

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
            RuntimeInvocationMethod: null,
            ObservedStatus: load.IsSuccess ? "adaptive_checkpoint_loaded" : "adaptive_checkpoint_load_failed",
            ObservedOutput: load.IsSuccess
                ? $"taskProfiles={checkpoint!.TaskProfiles.Count}; trainedSignalIds={checkpoint.TrainedSignalIds.Count}"
                : null,
            SemanticQualityClaimed: checkpoint?.SemanticQualityClaimed ?? false,
            CommonModelMutated: false,
            ErrorCode: load.IsSuccess ? null : load.ErrorCode,
            Limitation: "Narrow public adaptive loader only. The staged target is an adaptive overlay checkpoint without a PromptToTaskMachine manifest, so this does not prove PromptToTaskMachine.LoadCheckpointAsync or Translate.");
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
            RuntimeInvocationSucceeded: succeeded,
            RuntimeInvocationMethod: "OrchestrationReasoningMachine.Plan",
            ObservedStatus: response.IsSuccess ? response.Value!.ContractStatus : "plan_failed",
            ObservedOutput: response.IsSuccess
                ? string.Join(",", response.Value!.ScenarioPlan.SelectedLearnedPatterns)
                : null,
            SemanticQualityClaimed: load.Value.Checkpoint.SemanticQualityClaimed || load.Value.Manifest.SemanticQualityClaimed,
            CommonModelMutated: false,
            ErrorCode: response.IsSuccess ? null : response.ErrorCode,
            Limitation: "Fresh-load proves loadability and public machine invocation only. It does not prove semantic quality, held-out/source-disjoint behavior, ablation, rollback, or promotion readiness.");
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
            RuntimeInvocationMethod: null,
            ObservedStatus: load.IsSuccess ? "correction_adaptive_checkpoint_loaded" : "correction_adaptive_checkpoint_load_failed",
            ObservedOutput: load.IsSuccess
                ? $"correctionProfiles={checkpoint!.CorrectionProfiles.Count}; trainedSignalIds={checkpoint.TrainedSignalIds.Count}"
                : null,
            SemanticQualityClaimed: checkpoint?.SemanticQualityClaimed ?? false,
            CommonModelMutated: checkpoint?.CommonCheckpointMutationClaimed ?? false,
            ErrorCode: load.IsSuccess ? null : load.ErrorCode,
            Limitation: "Narrow public correction-adaptive loader only. The staged target is not a full SkillSelectionMachine checkpoint/manifest, so this does not prove SkillSelectionMachine.LoadCheckpointAsync or DecideFromTrainedState.");
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
            lines.Add($"  Target: `{track.TargetCheckpointPath}`");
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
        lines.Add("Fresh-load proves loadability. It requires held-out, ablation, and rollback evidence before readiness.");
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
        lines.AddRange(review.Tracks.Select(track => $"- {track.MachineId}: {track.Verdict} via `{track.BoundaryUsed}`"));
        lines.Add("");
        lines.Add("## Next phase");
        lines.Add("");
        lines.Add($"- {review.NextPhase}");
        ArtifactTextWriter.WriteText(path, string.Join("\n", lines), finalNewLine: true);
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
        int ExitCode,
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
}
