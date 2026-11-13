// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using AgentInfrastructure;

namespace SmartSkillsDomain;

// Domain marker for the OrchestrationReasoning trainable unit contract surface.
public sealed class OrchestrationReasoningDomain;

// Request consumed after PromptToTaskMachine produces a StructuredTaskContract.
public sealed record OrchestrationReasoningRequest(
    string RequestId,
    StructuredTaskContract Task,
    IReadOnlyList<string> GuideDerivedStateRefs,
    IReadOnlyList<string> CorpusBundleIds,
    IReadOnlyDictionary<string, string> AdaptiveContextRefs,
    IReadOnlyDictionary<string, string> RuntimePolicyRefs);

// Scenario plan contract returned by OrchestrationReasoningMachine.
// HostCheckSets contains local host labels to check, not teacher/model names;
// admission keeps that distinction so "claude" can mean the host surface while
// "anthropic/claude-3-5-sonnet" still fails as a forbidden model reference.
public sealed record OrchestrationScenarioPlan(
    string ScenarioId,
    StructuredTaskContract Task,
    IReadOnlyList<string> SelectedLearnedPatterns,
    IReadOnlyList<string> PhaseFamilies,
    IReadOnlyList<string> GateFamilies,
    IReadOnlyList<string> AssetFamilies,
    IReadOnlyList<string> HostCheckSets,
    IReadOnlyList<string> RiskFlags,
    IReadOnlyList<string> NextGranularizationActions);

// Explicit chosen/rejected plan-pattern feedback for later DPO materialization.
public sealed record OrchestrationReasoningPlanPatternDpoRequest(
    string PairId,
    string SourcePromptLedgerId,
    OrchestrationScenarioPlan ChosenPlan,
    OrchestrationScenarioPlan RejectedPlan,
    string Reason,
    IReadOnlyDictionary<string, string> Metadata);

// Ref-only trainable pair shape for OrchestrationReasoning guide-state DPO rows.
public sealed record OrchestrationReasoningTrainingPairContract(
    string TrainingPairId,
    string SourceRowId,
    string ApprovedPairRef,
    string ChosenScenarioRef,
    string RejectedScenarioRef,
    string ErrorCode,
    string ErrorTaxonomyRef,
    string SourceGuideSha256,
    string SourceManifestRef,
    string SplitId,
    string SplitRole,
    string HeldOutSetId,
    string NearMissStrategyRef,
    bool ContainsRawPrivateText,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

// Checkpoint-state contract for future OrchestrationReasoning guide-state lifecycle gates.
public sealed record OrchestrationReasoningCheckpointStateContract(
    string LearnedGuideStateId,
    string CheckpointId,
    string CheckpointSha256,
    string SchemaVersion,
    int SourceRowCount,
    int DpoPairCount,
    string SourceManifestRef,
    string GuideStateRef,
    string ModelStateRef,
    string TrainingStateRef,
    string ExportManifestRef,
    string FreshLoadProbeRef,
    string ContinueTrainingStateRef,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

// Contract-only signal; this is not orchestration inference and not train-ready output.
public sealed record OrchestrationReasoningPlanPatternDpoSignal(
    string PairId,
    string SourcePromptLedgerId,
    string RawPromptSha256,
    OrchestrationScenarioPlan ChosenPlan,
    OrchestrationScenarioPlan RejectedPlan,
    string Reason,
    string TargetTrainingAdapter,
    string StorageScope,
    string RouteMode,
    bool SemanticQualityClaimed,
    bool ContainsRawPromptText,
    IReadOnlyDictionary<string, string> Metadata);

// User-local adaptive DPO signal that stores scenario-plan references only.
public sealed record OrchestrationReasoningAdaptiveDpoSignal(
    string SignalId,
    string SourcePromptLedgerId,
    string RawPromptSha256,
    string ChosenRef,
    string RejectedRef,
    string FeedbackClass,
    string TargetTrainingAdapter,
    string StorageScope,
    bool ContainsRawPromptText,
    bool ContainsRawAnalysisText,
    bool SemanticQualityClaimed,
    IReadOnlyDictionary<string, string> RedactedPromptRefs,
    IReadOnlyDictionary<string, string> RedactedAnalysisRefs,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

// Write request for OrchestrationReasoning user-local adaptive feedback.
public sealed record OrchestrationReasoningAdaptiveDpoSignalWriteRequest(
    string UserLocalRoot,
    string SignalId,
    string SourcePromptLedgerId,
    string RawPromptSha256,
    string ChosenScenarioPlanRef,
    string RejectedScenarioPlanRef,
    string FeedbackClass,
    IReadOnlyDictionary<string, string> RedactedPromptRefs,
    IReadOnlyDictionary<string, string> RedactedAnalysisRefs,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

// Result of persisting one OrchestrationReasoning adaptive signal row.
public sealed record OrchestrationReasoningAdaptiveDpoSignalWriteResult(
    string SignalPath,
    OrchestrationReasoningAdaptiveDpoSignal Signal);

// User-local adaptive OrchestrationReasoning profile learned from redacted DPO signal refs.
public sealed record OrchestrationReasoningAdaptiveScenarioPlanProfile(
    string ChosenRef,
    IReadOnlyList<string> RejectedRefs,
    IReadOnlyList<string> SourceSignalIds,
    int PositiveCount,
    double Confidence);

// User-local OrchestrationReasoning adaptive checkpoint. It is not a common model asset.
public sealed record OrchestrationReasoningAdaptiveCheckpoint(
    int SchemaVersion,
    string CheckpointId,
    IReadOnlyList<OrchestrationReasoningAdaptiveScenarioPlanProfile> ScenarioPlanProfiles,
    IReadOnlyList<string> TrainedSignalIds,
    bool SemanticQualityClaimed,
    DateTimeOffset SavedAtUtc);

// Request to train or no-op-train an OrchestrationReasoning user-local adaptive checkpoint.
public sealed record OrchestrationReasoningAdaptiveTrainingRequest(
    string UserLocalRoot,
    string? CheckpointPath = null,
    string? ReportPath = null,
    string? PreviousCheckpointPath = null,
    string? SourceAdmissionManifestPath = null,
    string? SourceAdmissionLedgerPath = null,
    string? SourceAdmissionEvidencePath = null);

// Report proving whether OrchestrationReasoning adaptive training consumed new rows or no-oped.
public sealed record OrchestrationReasoningAdaptiveTrainingReport(
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

public sealed record OrchestrationReasoningAdaptiveTrainingResult(
    OrchestrationReasoningAdaptiveCheckpoint Checkpoint,
    OrchestrationReasoningAdaptiveTrainingReport Report);

// Builds plan-pattern DPO signals only from explicit chosen/rejected scenario plans.
public sealed class OrchestrationReasoningPlanPatternDpoSignalBuilder
{
    public OperationResult<OrchestrationReasoningPlanPatternDpoSignal> Build(
        OrchestrationReasoningPlanPatternDpoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PairId))
            return OperationResult<OrchestrationReasoningPlanPatternDpoSignal>.Fail(
                "Pair id is required.",
                "ORCHESTRATION_REASONING_DPO_PAIR_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.SourcePromptLedgerId))
            return OperationResult<OrchestrationReasoningPlanPatternDpoSignal>.Fail(
                "Source prompt ledger id is required.",
                "ORCHESTRATION_REASONING_DPO_SOURCE_LEDGER_REQUIRED");

        if (request.ChosenPlan is null || request.RejectedPlan is null)
            return OperationResult<OrchestrationReasoningPlanPatternDpoSignal>.Fail(
                "Chosen and rejected scenario plans are required.",
                "ORCHESTRATION_REASONING_DPO_PLANS_REQUIRED");

        if (!string.Equals(
            request.ChosenPlan.Task.RawPromptSha256,
            request.RejectedPlan.Task.RawPromptSha256,
            StringComparison.Ordinal))
        {
            return OperationResult<OrchestrationReasoningPlanPatternDpoSignal>.Fail(
                "Chosen and rejected scenario plans must reference the same raw prompt hash.",
                "ORCHESTRATION_REASONING_DPO_PROMPT_HASH_MISMATCH");
        }

        var signal = new OrchestrationReasoningPlanPatternDpoSignal(
            PairId: request.PairId,
            SourcePromptLedgerId: request.SourcePromptLedgerId,
            RawPromptSha256: request.ChosenPlan.Task.RawPromptSha256,
            ChosenPlan: request.ChosenPlan,
            RejectedPlan: request.RejectedPlan,
            Reason: request.Reason,
            TargetTrainingAdapter: "IOrchestrationReasoningPlanPatternDpoTraining",
            StorageScope: "project-local:.xiigen/orchestration-reasoning/dpo-signals.jsonl",
            RouteMode: "explicit_plan_pattern_feedback_signal",
            SemanticQualityClaimed: false,
            ContainsRawPromptText: false,
            Metadata: request.Metadata);

        return OperationResult<OrchestrationReasoningPlanPatternDpoSignal>.Ok(signal);
    }
}

// Persists OrchestrationReasoning adaptive DPO signals into user-local .xiigen state only.
public sealed class OrchestrationReasoningAdaptiveDpoSignalWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public OperationResult<OrchestrationReasoningAdaptiveDpoSignalWriteResult> Write(
        OrchestrationReasoningAdaptiveDpoSignalWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserLocalRoot))
            return OperationResult<OrchestrationReasoningAdaptiveDpoSignalWriteResult>.Fail(
                "User-local root is required.",
                "ORCHESTRATION_REASONING_ADAPTIVE_ROOT_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.SignalId))
            return OperationResult<OrchestrationReasoningAdaptiveDpoSignalWriteResult>.Fail(
                "Signal id is required.",
                "ORCHESTRATION_REASONING_ADAPTIVE_SIGNAL_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.SourcePromptLedgerId))
            return OperationResult<OrchestrationReasoningAdaptiveDpoSignalWriteResult>.Fail(
                "Source prompt ledger id is required.",
                "ORCHESTRATION_REASONING_ADAPTIVE_SOURCE_LEDGER_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.RawPromptSha256))
            return OperationResult<OrchestrationReasoningAdaptiveDpoSignalWriteResult>.Fail(
                "Raw prompt hash is required.",
                "ORCHESTRATION_REASONING_ADAPTIVE_PROMPT_HASH_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.ChosenScenarioPlanRef)
            || string.IsNullOrWhiteSpace(request.RejectedScenarioPlanRef))
        {
            return OperationResult<OrchestrationReasoningAdaptiveDpoSignalWriteResult>.Fail(
                "Chosen and rejected scenario-plan refs are required.",
                "ORCHESTRATION_REASONING_ADAPTIVE_REFS_REQUIRED");
        }

        if (string.Equals(
            request.ChosenScenarioPlanRef,
            request.RejectedScenarioPlanRef,
            StringComparison.Ordinal))
        {
            return OperationResult<OrchestrationReasoningAdaptiveDpoSignalWriteResult>.Fail(
                "Chosen and rejected refs must differ.",
                "ORCHESTRATION_REASONING_ADAPTIVE_REFS_MUST_DIFFER");
        }

        if (string.IsNullOrWhiteSpace(request.FeedbackClass))
            return OperationResult<OrchestrationReasoningAdaptiveDpoSignalWriteResult>.Fail(
                "Feedback class is required.",
                "ORCHESTRATION_REASONING_ADAPTIVE_FEEDBACK_CLASS_REQUIRED");

        var signal = new OrchestrationReasoningAdaptiveDpoSignal(
            SignalId: request.SignalId,
            SourcePromptLedgerId: request.SourcePromptLedgerId,
            RawPromptSha256: request.RawPromptSha256,
            ChosenRef: request.ChosenScenarioPlanRef,
            RejectedRef: request.RejectedScenarioPlanRef,
            FeedbackClass: request.FeedbackClass,
            TargetTrainingAdapter: "IOrchestrationReasoningAdaptiveDpoTraining",
            StorageScope: "user-local:.xiigen/orchestration-reasoning/adaptive-dpo-signals.jsonl",
            ContainsRawPromptText: false,
            ContainsRawAnalysisText: false,
            SemanticQualityClaimed: false,
            RedactedPromptRefs: request.RedactedPromptRefs,
            RedactedAnalysisRefs: request.RedactedAnalysisRefs,
            ProvenanceRefs: request.ProvenanceRefs);

        var signalPath = Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "orchestration-reasoning",
            "adaptive-dpo-signals.jsonl");
        Directory.CreateDirectory(Path.GetDirectoryName(signalPath)!);
        File.AppendAllText(signalPath, JsonSerializer.Serialize(signal, JsonOptions) + Environment.NewLine);

        return OperationResult<OrchestrationReasoningAdaptiveDpoSignalWriteResult>.Ok(
            new OrchestrationReasoningAdaptiveDpoSignalWriteResult(signalPath, signal));
    }
}

// Trains only the user-local OrchestrationReasoning adaptive leg from DPO signal refs.
public sealed class OrchestrationReasoningAdaptiveTrainer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public OperationResult<OrchestrationReasoningAdaptiveTrainingResult> TrainUserLocal(
        OrchestrationReasoningAdaptiveTrainingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserLocalRoot))
            return OperationResult<OrchestrationReasoningAdaptiveTrainingResult>.Fail(
                "User-local root is required.",
                "ORCHESTRATION_REASONING_ADAPTIVE_ROOT_REQUIRED");

        var signalPath = Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "orchestration-reasoning",
            "adaptive-dpo-signals.jsonl");
        var checkpointPath = request.CheckpointPath ?? Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "orchestration-reasoning",
            "adaptive-checkpoint.json");
        var reportPath = request.ReportPath ?? Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "orchestration-reasoning",
            "adaptive-training-report.json");

        var admissionManifestPath = request.SourceAdmissionManifestPath ?? Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "orchestration-reasoning",
            "adaptive-source-admission-manifest.json");
        var admissionEvidencePath = request.SourceAdmissionEvidencePath ?? Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "orchestration-reasoning",
            "adaptive-source-admission-evidence.json");
        var admission = OrchestrationReasoningSourceAdmission.AdmitAsync(
                signalPath,
                admissionManifestPath,
                request.SourceAdmissionLedgerPath,
                admissionEvidencePath)
            .GetAwaiter()
            .GetResult();
        if (!admission.IsSuccess)
            return admission.MapError<OrchestrationReasoningAdaptiveTrainingResult>();
        if (!string.Equals(
                admission.Value!.Decision.AllowedSourceKind,
                OrchestrationReasoningSourceAdmission.ILearningDynamicGraph,
                StringComparison.Ordinal))
        {
            return OperationResult<OrchestrationReasoningAdaptiveTrainingResult>.Fail(
                "Adaptive training requires verified ilearning_dynamic_graph evidence.",
                OrchestrationReasoningSourceAdmission.AdmissionRejectedCode);
        }

        IReadOnlyList<OrchestrationReasoningAdaptiveDpoSignal> boundSignals;
        try
        {
            boundSignals = ReadSignals(signalPath);
        }
        catch (JsonException ex)
        {
            return OperationResult<OrchestrationReasoningAdaptiveTrainingResult>.Fail(
                ex.Message,
                OrchestrationReasoningSourceAdmission.AdmissionRejectedCode);
        }
        if (boundSignals.Any(signal => !IsEligibleSignal(signal))
            || !boundSignals.Select(signal => signal.SignalId)
                .ToHashSet(StringComparer.Ordinal)
                .SetEquals(admission.Value.Decision.PairIds))
        {
            return OperationResult<OrchestrationReasoningAdaptiveTrainingResult>.Fail(
                "Every adaptive signal must be eligible and exactly bound by dynamic-graph evidence.",
                OrchestrationReasoningSourceAdmission.AdmissionRejectedCode);
        }

        var previousCheckpointResult = LoadPreviousCheckpoint(request.PreviousCheckpointPath);
        if (!previousCheckpointResult.IsSuccess)
            return previousCheckpointResult.MapError<OrchestrationReasoningAdaptiveTrainingResult>();
        var previousCheckpoint = previousCheckpointResult.Value;
        var previouslyTrainedIds = previousCheckpoint?.TrainedSignalIds
            .ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);
        var eligibleSignals = boundSignals.ToArray();
        var newSignals = eligibleSignals
            .Where(signal => !previouslyTrainedIds.Contains(signal.SignalId))
            .ToArray();

        var trainedIds = previouslyTrainedIds
            .Concat(newSignals.Select(signal => signal.SignalId))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(signalId => signalId, StringComparer.Ordinal)
            .ToArray();

        var profiles = previousCheckpoint?.ScenarioPlanProfiles
            .ToDictionary(profile => profile.ChosenRef, StringComparer.Ordinal)
            ?? new Dictionary<string, OrchestrationReasoningAdaptiveScenarioPlanProfile>(StringComparer.Ordinal);

        foreach (var group in newSignals.GroupBy(signal => signal.ChosenRef, StringComparer.Ordinal))
        {
            profiles.TryGetValue(group.Key, out var existing);
            var rejected = (existing?.RejectedRefs ?? Array.Empty<string>())
                .Concat(group.Select(signal => signal.RejectedRef))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var sources = (existing?.SourceSignalIds ?? Array.Empty<string>())
                .Concat(group.Select(signal => signal.SignalId))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var positiveCount = (existing?.PositiveCount ?? 0) + group.Count();

            profiles[group.Key] = new OrchestrationReasoningAdaptiveScenarioPlanProfile(
                group.Key,
                rejected,
                sources,
                positiveCount,
                Confidence: Math.Min(1.0, positiveCount / 3.0));
        }

        var checkpoint = new OrchestrationReasoningAdaptiveCheckpoint(
            SchemaVersion: 1,
            CheckpointId: $"orchestration-reasoning-adaptive:{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            ScenarioPlanProfiles: profiles.Values
                .OrderBy(profile => profile.ChosenRef, StringComparer.Ordinal)
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
                ? "No eligible OrchestrationReasoning adaptive DPO signal rows were found."
                : "All eligible OrchestrationReasoning adaptive DPO signal rows were already trained; replay skipped."
            : null;
        var report = new OrchestrationReasoningAdaptiveTrainingReport(
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

        return OperationResult<OrchestrationReasoningAdaptiveTrainingResult>.Ok(
            new OrchestrationReasoningAdaptiveTrainingResult(checkpoint, report));
    }

    public OperationResult<OrchestrationReasoningAdaptiveCheckpoint> LoadCheckpoint(string checkpointPath)
    {
        if (!File.Exists(checkpointPath))
            return OperationResult<OrchestrationReasoningAdaptiveCheckpoint>.Fail(
                "OrchestrationReasoning adaptive checkpoint was not found.",
                "ORCHESTRATION_REASONING_ADAPTIVE_CHECKPOINT_NOT_FOUND");

        try
        {
            var json = File.ReadAllText(checkpointPath);
            using var document = JsonDocument.Parse(json);
            if (OrchestrationReasoningSourceAdmission.ContainsCommonCheckpointMarkers(
                    document.RootElement))
            {
                return OperationResult<OrchestrationReasoningAdaptiveCheckpoint>.Fail(
                    "Common checkpoint material was placed on an adaptive checkpoint path.",
                    OrchestrationReasoningSourceAdmission.ProvenanceRebuildRequiredCode);
            }
            if (!document.RootElement.TryGetProperty("scenarioPlanProfiles", out _)
                || !document.RootElement.TryGetProperty("trainedSignalIds", out _))
            {
                return OperationResult<OrchestrationReasoningAdaptiveCheckpoint>.Fail(
                    "Adaptive checkpoint envelope kind is invalid.",
                    "ORCHESTRATION_REASONING_ADAPTIVE_CHECKPOINT_KIND_INVALID");
            }

            var checkpoint = JsonSerializer.Deserialize<OrchestrationReasoningAdaptiveCheckpoint>(
                json,
                JsonOptions);
            return ValidateAdaptiveCheckpoint(checkpoint);
        }
        catch (JsonException ex)
        {
            return OperationResult<OrchestrationReasoningAdaptiveCheckpoint>.Fail(
                ex.Message,
                "ORCHESTRATION_REASONING_ADAPTIVE_CHECKPOINT_PARSE_FAILED");
        }
    }

    private static OperationResult<OrchestrationReasoningAdaptiveCheckpoint?> LoadPreviousCheckpoint(
        string? checkpointPath)
    {
        if (string.IsNullOrWhiteSpace(checkpointPath) || !File.Exists(checkpointPath))
            return OperationResult<OrchestrationReasoningAdaptiveCheckpoint?>.Ok(null);

        var loaded = new OrchestrationReasoningAdaptiveTrainer().LoadCheckpoint(checkpointPath);
        return loaded.IsSuccess
            ? OperationResult<OrchestrationReasoningAdaptiveCheckpoint?>.Ok(loaded.Value)
            : OperationResult<OrchestrationReasoningAdaptiveCheckpoint?>.Fail(
                loaded.ErrorMessage,
                loaded.ErrorCode);
    }

    private static OperationResult<OrchestrationReasoningAdaptiveCheckpoint> ValidateAdaptiveCheckpoint(
        OrchestrationReasoningAdaptiveCheckpoint? checkpoint)
    {
        if (checkpoint is null)
            return OperationResult<OrchestrationReasoningAdaptiveCheckpoint>.Fail(
                "OrchestrationReasoning adaptive checkpoint could not be parsed.",
                "ORCHESTRATION_REASONING_ADAPTIVE_CHECKPOINT_PARSE_FAILED");
        if (checkpoint.SchemaVersion != 1)
            return OperationResult<OrchestrationReasoningAdaptiveCheckpoint>.Fail(
                "Adaptive checkpoint schema must be v1.",
                "ORCHESTRATION_REASONING_ADAPTIVE_CHECKPOINT_SCHEMA_INVALID");
        if (string.IsNullOrWhiteSpace(checkpoint.CheckpointId)
            || checkpoint.ScenarioPlanProfiles is null
            || checkpoint.TrainedSignalIds is null
            || checkpoint.ScenarioPlanProfiles.Any(profile =>
                profile is null
                || profile.RejectedRefs is null
                || profile.SourceSignalIds is null))
        {
            return OperationResult<OrchestrationReasoningAdaptiveCheckpoint>.Fail(
                "Adaptive checkpoint requires a valid id and non-null collections.",
                "ORCHESTRATION_REASONING_ADAPTIVE_CHECKPOINT_INVALID");
        }

        return OperationResult<OrchestrationReasoningAdaptiveCheckpoint>.Ok(checkpoint);
    }

    private static IReadOnlyList<OrchestrationReasoningAdaptiveDpoSignal> ReadSignals(string signalPath)
    {
        if (!File.Exists(signalPath))
            return Array.Empty<OrchestrationReasoningAdaptiveDpoSignal>();

        return File.ReadLines(signalPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<OrchestrationReasoningAdaptiveDpoSignal>(line, JsonOptions))
            .Where(signal => signal is not null)
            .Cast<OrchestrationReasoningAdaptiveDpoSignal>()
            .ToArray();
    }

    private static bool IsEligibleSignal(OrchestrationReasoningAdaptiveDpoSignal signal)
        => signal.TargetTrainingAdapter == "IOrchestrationReasoningAdaptiveDpoTraining"
            && !signal.ContainsRawPromptText
            && !signal.ContainsRawAnalysisText
            && !signal.SemanticQualityClaimed
            && !string.IsNullOrWhiteSpace(signal.SignalId)
            && !string.IsNullOrWhiteSpace(signal.ChosenRef)
            && !string.IsNullOrWhiteSpace(signal.RejectedRef);
}

// Response wrapper that keeps orchestration reasoning output contract-only here.
public sealed record OrchestrationReasoningResponse(
    string ResponseId,
    OrchestrationReasoningRequest Request,
    OrchestrationScenarioPlan ScenarioPlan,
    IReadOnlyList<string> ContractEvidence,
    string ContractStatus);
