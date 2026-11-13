// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using AgentInfrastructure;

namespace SmartSkillsDomain;

// Domain marker for the PromptToTask trainable unit contract surface.
public sealed class PromptToTaskDomain;

// Request submitted to PromptToTaskMachine before any orchestration reasoning starts.
public sealed record PromptToTaskRequest(
    string PromptId,
    string RawPrompt,
    string RawPromptSha256,
    string PromptProvenance,
    string UserLanguage,
    IReadOnlyList<string> ExplicitConstraints,
    IReadOnlyDictionary<string, string> ContextFields);

// Structured task handoff consumed by OrchestrationReasoningMachine.
public sealed record StructuredTaskContract(
    string PromptId,
    string RawPromptSha256,
    string UserLanguage,
    string TaskType,
    IReadOnlyList<string> ExplicitUserRequirements,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<string> Deliverables,
    IReadOnlyList<string> ForbiddenActions,
    IReadOnlyList<string> RequiredHosts,
    IReadOnlyList<string> EvidenceNeeds,
    IReadOnlyDictionary<string, string> SourceProvenance,
    string SplitId);

// Explicit chosen/rejected task-pattern feedback for later DPO materialization.
public sealed record PromptToTaskTaskPatternDpoRequest(
    string PairId,
    string SourcePromptLedgerId,
    StructuredTaskContract ChosenTask,
    StructuredTaskContract RejectedTask,
    string Reason,
    IReadOnlyDictionary<string, string> Metadata);

// Ref-only trainable pair shape for PromptToTask DPO rows.
public sealed record PromptToTaskTrainingPairContract(
    string TrainingPairId,
    string SourceRowId,
    string ApprovedPairRef,
    string ChosenRef,
    string RejectedRef,
    string ErrorCode,
    string ErrorTaxonomyRef,
    string SourcePromptSha256,
    string SourceManifestRef,
    string SplitId,
    string SplitRole,
    string HeldOutSetId,
    string NearMissStrategyRef,
    bool ContainsRawPrivateText,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

// Checkpoint-state contract for future export/import, fresh-load, and continue-training gates.
public sealed record PromptToTaskCheckpointStateContract(
    string LearnedStateId,
    string CheckpointId,
    string CheckpointSha256,
    string SchemaVersion,
    int SourceRowCount,
    int DpoPairCount,
    string SourceManifestRef,
    string ModelStateRef,
    string TrainingStateRef,
    string ExportManifestRef,
    string FreshLoadProbeRef,
    string ContinueTrainingStateRef,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

// Contract-only signal; this is not semantic inference and not train-ready output.
public sealed record PromptToTaskTaskPatternDpoSignal(
    string PairId,
    string SourcePromptLedgerId,
    string RawPromptSha256,
    StructuredTaskContract ChosenTask,
    StructuredTaskContract RejectedTask,
    string Reason,
    string TargetTrainingAdapter,
    string StorageScope,
    string RouteMode,
    bool SemanticQualityClaimed,
    bool ContainsRawPromptText,
    IReadOnlyDictionary<string, string> Metadata);

// User-local adaptive DPO signal that stores references only, never raw prompt text.
public sealed record PromptToTaskAdaptiveDpoSignal(
    string SignalId,
    string SourcePromptLedgerId,
    string RawPromptSha256,
    string ChosenStructuredTaskRef,
    string RejectedStructuredTaskRef,
    string FeedbackClass,
    string TargetTrainingAdapter,
    string StorageScope,
    bool ContainsRawPromptText,
    bool SemanticQualityClaimed,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

// Write request for a PromptToTask user-local adaptive DPO signal.
public sealed record PromptToTaskAdaptiveDpoSignalWriteRequest(
    string UserLocalRoot,
    string SignalId,
    string SourcePromptLedgerId,
    string RawPromptSha256,
    string ChosenStructuredTaskRef,
    string RejectedStructuredTaskRef,
    string FeedbackClass,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

// Result of persisting one PromptToTask adaptive signal row.
public sealed record PromptToTaskAdaptiveDpoSignalWriteResult(
    string SignalPath,
    PromptToTaskAdaptiveDpoSignal Signal);

// Builds task-pattern DPO signals only from explicit chosen/rejected task contracts.
public sealed class PromptToTaskTaskPatternDpoSignalBuilder
{
    public OperationResult<PromptToTaskTaskPatternDpoSignal> Build(
        PromptToTaskTaskPatternDpoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PairId))
            return OperationResult<PromptToTaskTaskPatternDpoSignal>.Fail(
                "Pair id is required.",
                "PROMPT_TO_TASK_DPO_PAIR_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.SourcePromptLedgerId))
            return OperationResult<PromptToTaskTaskPatternDpoSignal>.Fail(
                "Source prompt ledger id is required.",
                "PROMPT_TO_TASK_DPO_SOURCE_LEDGER_REQUIRED");

        if (request.ChosenTask is null || request.RejectedTask is null)
            return OperationResult<PromptToTaskTaskPatternDpoSignal>.Fail(
                "Chosen and rejected task contracts are required.",
                "PROMPT_TO_TASK_DPO_TASKS_REQUIRED");

        if (!string.Equals(
            request.ChosenTask.RawPromptSha256,
            request.RejectedTask.RawPromptSha256,
            StringComparison.Ordinal))
        {
            return OperationResult<PromptToTaskTaskPatternDpoSignal>.Fail(
                "Chosen and rejected task contracts must reference the same raw prompt hash.",
                "PROMPT_TO_TASK_DPO_PROMPT_HASH_MISMATCH");
        }

        var signal = new PromptToTaskTaskPatternDpoSignal(
            PairId: request.PairId,
            SourcePromptLedgerId: request.SourcePromptLedgerId,
            RawPromptSha256: request.ChosenTask.RawPromptSha256,
            ChosenTask: request.ChosenTask,
            RejectedTask: request.RejectedTask,
            Reason: request.Reason,
            TargetTrainingAdapter: "IPromptToTaskTaskPatternDpoTraining",
            StorageScope: "project-local:.xiigen/prompt-to-task/dpo-signals.jsonl",
            RouteMode: "explicit_task_pattern_feedback_signal",
            SemanticQualityClaimed: false,
            ContainsRawPromptText: false,
            Metadata: request.Metadata);

        return OperationResult<PromptToTaskTaskPatternDpoSignal>.Ok(signal);
    }
}

// Persists PromptToTask adaptive DPO signals into user-local .xiigen state only.
public sealed class PromptToTaskAdaptiveDpoSignalWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public OperationResult<PromptToTaskAdaptiveDpoSignalWriteResult> Write(
        PromptToTaskAdaptiveDpoSignalWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserLocalRoot))
            return OperationResult<PromptToTaskAdaptiveDpoSignalWriteResult>.Fail(
                "User-local root is required.",
                "PROMPT_TO_TASK_ADAPTIVE_ROOT_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.SignalId))
            return OperationResult<PromptToTaskAdaptiveDpoSignalWriteResult>.Fail(
                "Signal id is required.",
                "PROMPT_TO_TASK_ADAPTIVE_SIGNAL_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.SourcePromptLedgerId))
            return OperationResult<PromptToTaskAdaptiveDpoSignalWriteResult>.Fail(
                "Source prompt ledger id is required.",
                "PROMPT_TO_TASK_ADAPTIVE_SOURCE_LEDGER_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.RawPromptSha256))
            return OperationResult<PromptToTaskAdaptiveDpoSignalWriteResult>.Fail(
                "Raw prompt hash is required.",
                "PROMPT_TO_TASK_ADAPTIVE_PROMPT_HASH_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.ChosenStructuredTaskRef)
            || string.IsNullOrWhiteSpace(request.RejectedStructuredTaskRef))
        {
            return OperationResult<PromptToTaskAdaptiveDpoSignalWriteResult>.Fail(
                "Chosen and rejected structured-task refs are required.",
                "PROMPT_TO_TASK_ADAPTIVE_REFS_REQUIRED");
        }

        if (string.Equals(
            request.ChosenStructuredTaskRef,
            request.RejectedStructuredTaskRef,
            StringComparison.Ordinal))
        {
            return OperationResult<PromptToTaskAdaptiveDpoSignalWriteResult>.Fail(
                "Chosen and rejected refs must differ.",
                "PROMPT_TO_TASK_ADAPTIVE_REFS_MUST_DIFFER");
        }

        if (string.IsNullOrWhiteSpace(request.FeedbackClass))
            return OperationResult<PromptToTaskAdaptiveDpoSignalWriteResult>.Fail(
                "Feedback class is required.",
                "PROMPT_TO_TASK_ADAPTIVE_FEEDBACK_CLASS_REQUIRED");

        var signal = new PromptToTaskAdaptiveDpoSignal(
            SignalId: request.SignalId,
            SourcePromptLedgerId: request.SourcePromptLedgerId,
            RawPromptSha256: request.RawPromptSha256,
            ChosenStructuredTaskRef: request.ChosenStructuredTaskRef,
            RejectedStructuredTaskRef: request.RejectedStructuredTaskRef,
            FeedbackClass: request.FeedbackClass,
            TargetTrainingAdapter: "IPromptToTaskAdaptiveDpoTraining",
            StorageScope: "project-local:.xiigen/prompt-to-task/adaptive-dpo-signals.jsonl",
            ContainsRawPromptText: false,
            SemanticQualityClaimed: false,
            ProvenanceRefs: request.ProvenanceRefs);

        var signalPath = Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "prompt-to-task",
            "adaptive-dpo-signals.jsonl");
        Directory.CreateDirectory(Path.GetDirectoryName(signalPath)!);
        File.AppendAllText(signalPath, JsonSerializer.Serialize(signal, JsonOptions) + Environment.NewLine);

        return OperationResult<PromptToTaskAdaptiveDpoSignalWriteResult>.Ok(
            new PromptToTaskAdaptiveDpoSignalWriteResult(signalPath, signal));
    }
}

// User-local adaptive PromptToTask profile learned from redacted DPO signal refs.
public sealed record PromptToTaskAdaptiveTaskProfile(
    string ChosenStructuredTaskRef,
    IReadOnlyList<string> RejectedStructuredTaskRefs,
    IReadOnlyList<string> SourceSignalIds,
    int PositiveCount,
    double Confidence);

// User-local PromptToTask adaptive checkpoint. It is not a common model asset.
public sealed record PromptToTaskAdaptiveCheckpoint(
    int SchemaVersion,
    string CheckpointId,
    IReadOnlyList<PromptToTaskAdaptiveTaskProfile> TaskProfiles,
    IReadOnlyList<string> TrainedSignalIds,
    bool SemanticQualityClaimed,
    DateTimeOffset SavedAtUtc);

// Request to train or no-op-train a PromptToTask user-local adaptive checkpoint.
public sealed record PromptToTaskAdaptiveTrainingRequest(
    string UserLocalRoot,
    string? CheckpointPath = null,
    string? ReportPath = null,
    string? PreviousCheckpointPath = null);

// Report proving whether PromptToTask adaptive training consumed new rows or no-oped.
public sealed record PromptToTaskAdaptiveTrainingReport(
    int SchemaVersion,
    string SignalPath,
    string CheckpointPath,
    string CheckpointSha256,
    int EligibleSignalCount,
    int NewSignalCount,
    int MetricBefore,
    int MetricAfter,
    bool MetricImproved,
    bool NoOp,
    string? NoOpReason,
    IReadOnlyList<string> TrainedSignalIds,
    bool CommonCheckpointMutated,
    bool SemanticQualityClaimed);

public sealed record PromptToTaskAdaptiveTrainingResult(
    PromptToTaskAdaptiveCheckpoint Checkpoint,
    PromptToTaskAdaptiveTrainingReport Report);

// Trains only the user-local PromptToTask adaptive leg from PTT DPO signal refs.
public sealed class PromptToTaskAdaptiveTrainer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public OperationResult<PromptToTaskAdaptiveTrainingResult> TrainUserLocal(
        PromptToTaskAdaptiveTrainingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserLocalRoot))
            return OperationResult<PromptToTaskAdaptiveTrainingResult>.Fail(
                "User-local root is required.",
                "PROMPT_TO_TASK_ADAPTIVE_ROOT_REQUIRED");

        var signalPath = Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "prompt-to-task",
            "adaptive-dpo-signals.jsonl");
        var checkpointPath = request.CheckpointPath ?? Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "prompt-to-task",
            "adaptive-checkpoint.json");
        var reportPath = request.ReportPath ?? Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "prompt-to-task",
            "adaptive-training-report.json");

        var previousCheckpoint = LoadPreviousCheckpoint(request.PreviousCheckpointPath);
        var previouslyTrainedIds = previousCheckpoint?.TrainedSignalIds
            .ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);
        var eligibleSignals = ReadSignals(signalPath)
            .Where(IsEligibleSignal)
            .ToArray();
        var newSignals = eligibleSignals
            .Where(signal => !previouslyTrainedIds.Contains(signal.SignalId))
            .ToArray();

        var trainedIds = previouslyTrainedIds
            .Concat(newSignals.Select(signal => signal.SignalId))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(signalId => signalId, StringComparer.Ordinal)
            .ToArray();

        var profiles = previousCheckpoint?.TaskProfiles
            .ToDictionary(profile => profile.ChosenStructuredTaskRef, StringComparer.Ordinal)
            ?? new Dictionary<string, PromptToTaskAdaptiveTaskProfile>(StringComparer.Ordinal);

        foreach (var group in newSignals.GroupBy(signal => signal.ChosenStructuredTaskRef, StringComparer.Ordinal))
        {
            profiles.TryGetValue(group.Key, out var existing);
            var rejected = (existing?.RejectedStructuredTaskRefs ?? Array.Empty<string>())
                .Concat(group.Select(signal => signal.RejectedStructuredTaskRef))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var sources = (existing?.SourceSignalIds ?? Array.Empty<string>())
                .Concat(group.Select(signal => signal.SignalId))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var positiveCount = (existing?.PositiveCount ?? 0) + group.Count();

            profiles[group.Key] = new PromptToTaskAdaptiveTaskProfile(
                group.Key,
                rejected,
                sources,
                positiveCount,
                Confidence: Math.Min(1.0, positiveCount / 3.0));
        }

        var checkpoint = new PromptToTaskAdaptiveCheckpoint(
            SchemaVersion: 1,
            CheckpointId: $"prompt-to-task-adaptive:{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            TaskProfiles: profiles.Values
                .OrderBy(profile => profile.ChosenStructuredTaskRef, StringComparer.Ordinal)
                .ToArray(),
            TrainedSignalIds: trainedIds,
            SemanticQualityClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);

        Directory.CreateDirectory(Path.GetDirectoryName(checkpointPath)!);
        File.WriteAllText(checkpointPath, JsonSerializer.Serialize(checkpoint, JsonOptions));
        var checkpointHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(checkpointPath)))
            .ToLowerInvariant();

        var noOpReason = newSignals.Length == 0
            ? eligibleSignals.Length == 0
                ? "No eligible PromptToTask adaptive DPO signal rows were found."
                : "All eligible PromptToTask adaptive DPO signal rows were already trained; replay skipped."
            : null;
        var report = new PromptToTaskAdaptiveTrainingReport(
            SchemaVersion: 1,
            SignalPath: signalPath,
            CheckpointPath: checkpointPath,
            CheckpointSha256: checkpointHash,
            EligibleSignalCount: eligibleSignals.Length,
            NewSignalCount: newSignals.Length,
            MetricBefore: previouslyTrainedIds.Count,
            MetricAfter: trainedIds.Length,
            MetricImproved: trainedIds.Length > previouslyTrainedIds.Count,
            NoOp: newSignals.Length == 0,
            NoOpReason: noOpReason,
            TrainedSignalIds: trainedIds,
            CommonCheckpointMutated: false,
            SemanticQualityClaimed: false);

        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        File.WriteAllText(reportPath, JsonSerializer.Serialize(report, JsonOptions));

        return OperationResult<PromptToTaskAdaptiveTrainingResult>.Ok(
            new PromptToTaskAdaptiveTrainingResult(checkpoint, report));
    }

    public OperationResult<PromptToTaskAdaptiveCheckpoint> LoadCheckpoint(string checkpointPath)
    {
        if (!File.Exists(checkpointPath))
            return OperationResult<PromptToTaskAdaptiveCheckpoint>.Fail(
                "PromptToTask adaptive checkpoint was not found.",
                "PROMPT_TO_TASK_ADAPTIVE_CHECKPOINT_NOT_FOUND");

        var checkpoint = JsonSerializer.Deserialize<PromptToTaskAdaptiveCheckpoint>(
            File.ReadAllText(checkpointPath),
            JsonOptions);
        return checkpoint is null
            ? OperationResult<PromptToTaskAdaptiveCheckpoint>.Fail(
                "PromptToTask adaptive checkpoint could not be parsed.",
                "PROMPT_TO_TASK_ADAPTIVE_CHECKPOINT_PARSE_FAILED")
            : OperationResult<PromptToTaskAdaptiveCheckpoint>.Ok(checkpoint);
    }

    private static PromptToTaskAdaptiveCheckpoint? LoadPreviousCheckpoint(string? checkpointPath)
    {
        if (string.IsNullOrWhiteSpace(checkpointPath) || !File.Exists(checkpointPath))
            return null;

        return JsonSerializer.Deserialize<PromptToTaskAdaptiveCheckpoint>(
            File.ReadAllText(checkpointPath),
            JsonOptions);
    }

    private static IReadOnlyList<PromptToTaskAdaptiveDpoSignal> ReadSignals(string signalPath)
    {
        if (!File.Exists(signalPath))
            return Array.Empty<PromptToTaskAdaptiveDpoSignal>();

        return File.ReadLines(signalPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<PromptToTaskAdaptiveDpoSignal>(line, JsonOptions))
            .Where(signal => signal is not null)
            .Cast<PromptToTaskAdaptiveDpoSignal>()
            .ToArray();
    }

    private static bool IsEligibleSignal(PromptToTaskAdaptiveDpoSignal signal)
        => signal.TargetTrainingAdapter == "IPromptToTaskAdaptiveDpoTraining"
            && !signal.ContainsRawPromptText
            && !signal.SemanticQualityClaimed
            && !string.IsNullOrWhiteSpace(signal.SignalId)
            && !string.IsNullOrWhiteSpace(signal.ChosenStructuredTaskRef)
            && !string.IsNullOrWhiteSpace(signal.RejectedStructuredTaskRef);
}

// Response wrapper that keeps PromptToTask output contract-only in this slice.
public sealed record PromptToTaskResponse(
    string ResponseId,
    PromptToTaskRequest Request,
    StructuredTaskContract StructuredTask,
    IReadOnlyList<string> ContractEvidence,
    string ContractStatus);
