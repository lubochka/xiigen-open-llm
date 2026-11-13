// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;

namespace LlmFromScratch.DynamicGraphCompositionLiveLocalServer;

internal static class DynamicGraphCompositionJson
{
    internal static readonly JsonSerializerOptions HttpOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        WriteIndented = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    internal static readonly JsonSerializerOptions CheckpointOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        WriteIndented = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };
}

public sealed record CreateCompositionSessionHttpRequest(
    [property: JsonPropertyName("profile")] DynamicGraphLogicalOrchestratorBootstrapProfile? Profile,
    [property: JsonPropertyName("trained_input")] object? TrainedInput = null);

public sealed record CompositionEvidenceHttpResponse(
    [property: JsonPropertyName("profile_id")] string ProfileId,
    [property: JsonPropertyName("revision")] long Revision,
    [property: JsonPropertyName("source_kind")] string SourceKind,
    [property: JsonPropertyName("node_count")] int NodeCount,
    [property: JsonPropertyName("edge_count")] int EdgeCount,
    [property: JsonPropertyName("initial_revision_advanced")] bool InitialRevisionAdvanced);

public sealed record CompositionSessionHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("created_at_utc")] DateTimeOffset CreatedAtUtc,
    [property: JsonPropertyName("expires_at_utc")] DateTimeOffset ExpiresAtUtc,
    [property: JsonPropertyName("result")] CompositionEvidenceHttpResponse Result,
    [property: JsonPropertyName("boundaries")] CompositionBoundaryHttpResponse Boundaries);

public sealed record CurrentTopologyEvidenceHttpResponse(
    [property: JsonPropertyName("profile_id")] string ProfileId,
    [property: JsonPropertyName("revision")] long Revision,
    [property: JsonPropertyName("signal_id")] string SignalId,
    [property: JsonPropertyName("source_kind")] string SourceKind,
    [property: JsonPropertyName("node_count")] int NodeCount,
    [property: JsonPropertyName("edge_count")] int EdgeCount);

public sealed record CurrentTopologyHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("bootstrap")] CompositionEvidenceHttpResponse Bootstrap,
    [property: JsonPropertyName("current")] CurrentTopologyEvidenceHttpResponse Current,
    [property: JsonPropertyName("revision_advanced_since_bootstrap")]
        bool RevisionAdvancedSinceBootstrap,
    [property: JsonPropertyName("boundaries")] CompositionBoundaryHttpResponse Boundaries);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RunCompositionSessionHttpRequest(
    [property: JsonPropertyName("user_request")] string? UserRequest);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PromptToTaskUntrainedHonestyRunHttpRequest(
    [property: JsonPropertyName("evaluation_id")] string? EvaluationId,
    [property: JsonPropertyName("run_id")] string? RunId,
    [property: JsonPropertyName("prompt_id")] string? PromptId,
    [property: JsonPropertyName("raw_prompt")] string? RawPrompt,
    [property: JsonPropertyName("user_language")] string? UserLanguage);

public sealed record PromptToTaskUntrainedHonestyTargetHttpResponse(
    [property: JsonPropertyName("target_id")] string TargetId,
    [property: JsonPropertyName("level")] string Level,
    [property: JsonPropertyName("mechanism_kind")] string MechanismKind,
    [property: JsonPropertyName("deterministic_final_authority")]
        bool DeterministicFinalAuthority);

public sealed record PromptToTaskUntrainedHonestyProducerHttpResponse(
    [property: JsonPropertyName("unit_id")] string UnitId,
    [property: JsonPropertyName("unit_role")] string UnitRole,
    [property: JsonPropertyName("domain_tag")] string DomainTag,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("registration_order")] long RegistrationOrder);

public sealed record PromptToTaskUntrainedHonestyMeasurementHttpResponse(
    [property: JsonPropertyName("runtime_reported_not_ready")] bool RuntimeReportedNotReady,
    [property: JsonPropertyName("has_no_trained_state_evidence")] bool HasNoTrainedStateEvidence,
    [property: JsonPropertyName("has_no_semantic_classification_evidence")]
        bool HasNoSemanticClassificationEvidence,
    [property: JsonPropertyName("meets_untrained_honesty_gate")] bool MeetsUntrainedHonestyGate);

public sealed record PromptToTaskUntrainedHonestyBoundaryHttpResponse(
    [property: JsonPropertyName("loopback_only")] bool LoopbackOnly,
    [property: JsonPropertyName("tenant_binding_is_authentication")]
        bool TenantBindingIsAuthentication,
    [property: JsonPropertyName("persistent")] bool Persistent,
    [property: JsonPropertyName("executes_unit")] bool ExecutesUnit,
    [property: JsonPropertyName("measurement_only")] bool MeasurementOnly,
    [property: JsonPropertyName("performs_training")] bool PerformsTraining,
    [property: JsonPropertyName("claims_trained_state")] bool ClaimsTrainedState,
    [property: JsonPropertyName("claims_semantic_quality")] bool ClaimsSemanticQuality,
    [property: JsonPropertyName("changes_authority")] bool ChangesAuthority,
    [property: JsonPropertyName("persists_result")] bool PersistsResult);

public sealed record PromptToTaskUntrainedHonestyHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("evaluation_id")] string EvaluationId,
    [property: JsonPropertyName("target")]
        PromptToTaskUntrainedHonestyTargetHttpResponse Target,
    [property: JsonPropertyName("producer")]
        PromptToTaskUntrainedHonestyProducerHttpResponse Producer,
    [property: JsonPropertyName("measurement")]
        PromptToTaskUntrainedHonestyMeasurementHttpResponse Measurement,
    [property: JsonPropertyName("direct_unit_invocation_count")]
        int DirectUnitInvocationCount,
    [property: JsonPropertyName("boundaries")]
        PromptToTaskUntrainedHonestyBoundaryHttpResponse Boundaries);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AdmitSelectedTargetEvidenceHttpRequest(
    [property: JsonPropertyName("target")] DynamicGraphMeasurementTarget? Target,
    [property: JsonPropertyName("authority_result")]
    [property: JsonConverter(typeof(EdgeAuthorityExecutionResultJsonConverter))]
        EdgeAuthorityExecutionResult? AuthorityResult,
    [property: JsonPropertyName("novelty_request")]
    [property: JsonConverter(
        typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationRequestJsonConverter))]
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? NoveltyRequest);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RunAdmittedSelectedTargetHttpRequest(
    [property: JsonPropertyName("evidence_handle")] string? EvidenceHandle,
    [property: JsonPropertyName("run_id")] string? RunId,
    [property: JsonPropertyName("scenario_id")] string? ScenarioId,
    [property: JsonPropertyName("user_request")] string? UserRequest);

public sealed record SelectedTargetEvidenceAdmissionHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("evidence_handle")] string EvidenceHandle,
    [property: JsonPropertyName("expires_at_utc")] DateTimeOffset ExpiresAtUtc,
    [property: JsonPropertyName("boundaries")] CompositionBoundaryHttpResponse Boundaries);

/// <summary>
/// Reports timing evidence for exactly one completed direct selected-target invocation.
/// A zero routing interval means no separate routing interval was observed, not proof of zero
/// routing wall time. Empty candidate and batch sample banks produce zero counts and null axes.
/// This response makes no lifecycle, training, DPO, promotion, or authority claim.
/// </summary>
public sealed record SelectedTargetRunSpeedHttpResponse(
    [property: JsonPropertyName("evaluated_row_count")] int EvaluatedRowCount,
    [property: JsonPropertyName("inference_elapsed_ms")] long InferenceElapsedMs,
    [property: JsonPropertyName("throughput_rows_per_sec")] double ThroughputRowsPerSec,
    [property: JsonPropertyName("routing_elapsed_ms")] long RoutingElapsedMs,
    [property: JsonPropertyName("candidate_sample_count")] int CandidateSampleCount,
    [property: JsonPropertyName("batch_sample_count")] int BatchSampleCount,
    [property: JsonPropertyName("per_candidate_elapsed_ms")] long? PerCandidateElapsedMs,
    [property: JsonPropertyName("per_batch_elapsed_ms")] long? PerBatchElapsedMs);

public sealed record AdmittedSelectedTargetRunHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("target_id")] string TargetId,
    [property: JsonPropertyName("execution_id")] string ExecutionId,
    [property: JsonPropertyName("evidence_ids")] string[] EvidenceIds,
    [property: JsonPropertyName("direct_target_invocation_count")]
        int DirectTargetInvocationCount,
    [property: JsonPropertyName("run_speed")] SelectedTargetRunSpeedHttpResponse RunSpeed,
    [property: JsonPropertyName("boundaries")] CompositionBoundaryHttpResponse Boundaries);

public sealed record AssistantRunResultHttpResponse(
    [property: JsonPropertyName("scenario_id")] string ScenarioId,
    [property: JsonPropertyName("selected_flow_id")] string SelectedFlowId,
    [property: JsonPropertyName("overall_verdict")] string OverallVerdict,
    [property: JsonPropertyName("final_human_result")] string FinalHumanResult,
    [property: JsonPropertyName("metrics")] IReadOnlyDictionary<string, string> Metrics,
    [property: JsonPropertyName("evidence_ids")] string[] EvidenceIds);

public sealed record TopologyRunEvidenceHttpResponse(
    [property: JsonPropertyName("revision")] long Revision,
    [property: JsonPropertyName("signal_id")] string SignalId,
    [property: JsonPropertyName("source_kind")] string SourceKind);

public sealed record TopologyAssistantRunHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("topology")] TopologyRunEvidenceHttpResponse Topology,
    [property: JsonPropertyName("result")] AssistantRunResultHttpResponse Result,
    [property: JsonPropertyName("boundaries")] CompositionBoundaryHttpResponse Boundaries);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ApplyTopologyMutationHttpRequest(
    [property: JsonPropertyName("expected_revision")] long? ExpectedRevision,
    [property: JsonPropertyName("profile")]
        DynamicGraphLogicalOrchestratorBootstrapProfile? Profile);

public sealed record TopologyMutationAttemptHttpResponse(
    [property: JsonPropertyName("signal_id")] string SignalId,
    [property: JsonPropertyName("source_kind")] string SourceKind,
    [property: JsonPropertyName("mutation_applied")] bool MutationApplied);

public sealed record TopologyMutationCurrentHttpResponse(
    [property: JsonPropertyName("revision")] long Revision,
    [property: JsonPropertyName("signal_id")] string SignalId,
    [property: JsonPropertyName("source_kind")] string SourceKind,
    [property: JsonPropertyName("node_count")] int NodeCount,
    [property: JsonPropertyName("edge_count")] int EdgeCount);

public sealed record TopologyMutationBudgetHttpResponse(
    [property: JsonPropertyName("max_nodes")] int MaxNodes,
    [property: JsonPropertyName("actual_nodes")] int ActualNodes,
    [property: JsonPropertyName("max_edges")] int MaxEdges,
    [property: JsonPropertyName("actual_edges")] int ActualEdges,
    [property: JsonPropertyName("max_depth")] int MaxDepth,
    [property: JsonPropertyName("actual_depth")] int ActualDepth,
    [property: JsonPropertyName("max_parallel_branches")] int MaxParallelBranches,
    [property: JsonPropertyName("actual_parallel_branches")] int ActualParallelBranches,
    [property: JsonPropertyName("within_budget")] bool WithinBudget,
    [property: JsonPropertyName("exceeded_dimensions")] string[] ExceededDimensions);

public sealed record TopologyMutationHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("previous_revision")] long PreviousRevision,
    [property: JsonPropertyName("current_revision")] long CurrentRevision,
    [property: JsonPropertyName("revision_advanced")] bool RevisionAdvanced,
    [property: JsonPropertyName("attempt")] TopologyMutationAttemptHttpResponse Attempt,
    [property: JsonPropertyName("current")] TopologyMutationCurrentHttpResponse Current,
    [property: JsonPropertyName("budget")] TopologyMutationBudgetHttpResponse Budget,
    [property: JsonPropertyName("boundaries")] CompositionBoundaryHttpResponse Boundaries);

public sealed record AssistantRunHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("result")] AssistantRunResultHttpResponse Result,
    [property: JsonPropertyName("boundaries")] CompositionBoundaryHttpResponse Boundaries);

public sealed record CompositionBoundaryHttpResponse(
    [property: JsonPropertyName("loopback_only")] bool LoopbackOnly = true,
    [property: JsonPropertyName("tenant_binding_is_authentication")] bool TenantBindingIsAuthentication = false,
    [property: JsonPropertyName("persistent")] bool Persistent = false,
    [property: JsonPropertyName("executes_assistant")] bool ExecutesAssistant = true,
    [property: JsonPropertyName("applies_graph")] bool AppliesGraph = false,
    [property: JsonPropertyName("performs_training")] bool PerformsTraining = false);

public sealed record SafeErrorHttpResponse(
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message);

public sealed record ReadinessHttpResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("reason_code")] string? ReasonCode = null,
    [property: JsonPropertyName("loopback_only")] bool LoopbackOnly = true,
    [property: JsonPropertyName("tenant_binding_is_authentication")] bool TenantBindingIsAuthentication = false,
    [property: JsonPropertyName("persistent")] bool Persistent = false,
    [property: JsonPropertyName("executes_assistant")] bool ExecutesAssistant = true,
    [property: JsonPropertyName("applies_graph")] bool AppliesGraph = false,
    [property: JsonPropertyName("performs_training")] bool PerformsTraining = false);

public sealed class DynamicGraphEdgeAuthorityNoveltyEvaluationRequestJsonConverter
    : JsonConverter<DynamicGraphEdgeAuthorityNoveltyEvaluationRequest>
{
    public override DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var wire = JsonSerializer.Deserialize<NoveltyRequestWire>(ref reader, options);
        if (wire?.PairedSelection is null)
            throw new JsonException("A paired selection is required.");

        var pairedWire = wire.PairedSelection;
        if (string.IsNullOrWhiteSpace(pairedWire.EvaluationId)
            || string.IsNullOrWhiteSpace(pairedWire.EdgeId)
            || string.IsNullOrWhiteSpace(pairedWire.AuthorityUnitId)
            || pairedWire.SplitProof is null
            || pairedWire.Rows is null
            || pairedWire.LearnedCorrectCount is null
            || pairedWire.LearnedSelectionAccuracy is null
            || pairedWire.BaselineCorrectCount is null
            || pairedWire.BaselineSelectionAccuracy is null
            || pairedWire.AccuracyDrop is null
            || pairedWire.NormalizedAccuracyDrop is null
            || pairedWire.MeetsQ11NumericalGate is null
            || pairedWire.RowsEvaluated != pairedWire.Rows.Count)
        {
            throw new JsonException("The paired selection is invalid.");
        }

        var imported = DynamicGraphEdgeAuthorityAblationEvaluationRunner.ImportValidated(
            new DynamicGraphEdgeAuthorityAblationEvaluationImport(
                pairedWire.EvaluationId,
                pairedWire.EdgeId,
                pairedWire.AuthorityUnitId,
                pairedWire.ScorerId,
                pairedWire.ScorerVersion,
                pairedWire.ThresholdSetId,
                pairedWire.NormalizationId,
                pairedWire.SplitProof,
                pairedWire.Rows,
                pairedWire.LearnedCorrectCount.Value,
                pairedWire.LearnedSelectionAccuracy.Value,
                pairedWire.BaselineCorrectCount.Value,
                pairedWire.BaselineSelectionAccuracy.Value,
                pairedWire.AccuracyDrop.Value,
                pairedWire.NormalizedAccuracyDrop.Value,
                pairedWire.MeetsQ11NumericalGate.Value));
        if (!imported.IsSuccess || imported.Value is null)
            throw new JsonException("The paired selection is invalid.");

        return new DynamicGraphEdgeAuthorityNoveltyEvaluationRequest(
            imported.Value,
            wire.TrainingBindings,
            wire.RequestedRowIds,
            wire.NoveltyRows,
            wire.CallerAttestedFingerprintNormalizationId,
            wire.CallerAttestedTokenizationId);
    }

    public override void Write(
        Utf8JsonWriter writer,
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("paired_selection");
        JsonSerializer.Serialize(
            writer,
            FromPairedSelection(
                value.PairedSelection
                ?? throw new JsonException("The paired selection is required.")),
            options);
        writer.WritePropertyName("training_bindings");
        JsonSerializer.Serialize(writer, value.TrainingBindings, options);
        writer.WritePropertyName("requested_row_ids");
        JsonSerializer.Serialize(writer, value.RequestedRowIds, options);
        writer.WritePropertyName("novelty_rows");
        JsonSerializer.Serialize(writer, value.NoveltyRows, options);
        writer.WriteString(
            "caller_attested_fingerprint_normalization_id",
            value.CallerAttestedFingerprintNormalizationId);
        writer.WriteString(
            "caller_attested_tokenization_id",
            value.CallerAttestedTokenizationId);
        writer.WriteEndObject();
    }

    private static PairedSelectionWire FromPairedSelection(
        DynamicGraphEdgeAuthorityAblationEvaluationResult result)
        => new(
            result.EvaluationId,
            result.EdgeId,
            result.AuthorityUnitId,
            result.ScorerId,
            result.ScorerVersion,
            result.ThresholdSetId,
            result.NormalizationId,
            result.SplitProof,
            result.Rows,
            result.RowsEvaluated,
            result.LearnedCorrectCount,
            result.LearnedSelectionAccuracy,
            result.BaselineCorrectCount,
            result.BaselineSelectionAccuracy,
            result.AccuracyDrop,
            result.NormalizedAccuracyDrop,
            result.MeetsQ11NumericalGate);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    private sealed record NoveltyRequestWire(
        [property: JsonPropertyName("paired_selection")]
            PairedSelectionWire? PairedSelection,
        [property: JsonPropertyName("training_bindings")]
            IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>? TrainingBindings,
        [property: JsonPropertyName("requested_row_ids")]
            IReadOnlyList<string>? RequestedRowIds,
        [property: JsonPropertyName("novelty_rows")]
            IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowInput>? NoveltyRows,
        [property: JsonPropertyName("caller_attested_fingerprint_normalization_id")]
            string? CallerAttestedFingerprintNormalizationId,
        [property: JsonPropertyName("caller_attested_tokenization_id")]
            string? CallerAttestedTokenizationId);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    private sealed record PairedSelectionWire(
        [property: JsonPropertyName("evaluation_id")] string? EvaluationId,
        [property: JsonPropertyName("edge_id")] string? EdgeId,
        [property: JsonPropertyName("authority_unit_id")] string? AuthorityUnitId,
        [property: JsonPropertyName("scorer_id")] string? ScorerId,
        [property: JsonPropertyName("scorer_version")] string? ScorerVersion,
        [property: JsonPropertyName("threshold_set_id")] string? ThresholdSetId,
        [property: JsonPropertyName("normalization_id")] string? NormalizationId,
        [property: JsonPropertyName("split_proof")] DynamicGraphMeasurementSplitProof? SplitProof,
        [property: JsonPropertyName("rows")]
            IReadOnlyList<DynamicGraphEdgeAuthorityAblationRowEvidence>? Rows,
        [property: JsonPropertyName("rows_evaluated")] int? RowsEvaluated,
        [property: JsonPropertyName("learned_correct_count")] int? LearnedCorrectCount,
        [property: JsonPropertyName("learned_selection_accuracy")]
            double? LearnedSelectionAccuracy,
        [property: JsonPropertyName("baseline_correct_count")] int? BaselineCorrectCount,
        [property: JsonPropertyName("baseline_selection_accuracy")]
            double? BaselineSelectionAccuracy,
        [property: JsonPropertyName("accuracy_drop")] double? AccuracyDrop,
        [property: JsonPropertyName("normalized_accuracy_drop")]
            double? NormalizedAccuracyDrop,
        [property: JsonPropertyName("meets_q11_numerical_gate")]
            bool? MeetsQ11NumericalGate);
}

public sealed class EdgeAuthorityExecutionResultJsonConverter
    : JsonConverter<EdgeAuthorityExecutionResult>
{
    public override EdgeAuthorityExecutionResult? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var wire = JsonSerializer.Deserialize<AuthorityExecutionResultWire>(
            ref reader,
            options);
        if (wire?.NormalizedInput is null
            || wire.ProposedCandidates is null
            || wire.Decision is null
            || wire.AuthorityEvidence is null
            || wire.CandidateOrderLeakageReport is null
            || wire.IdentityLeakageReport is null
            || wire.UnitConfidence is null
            || wire.EvidenceIds is null
            || wire.Warnings is null)
        {
            throw new JsonException("The edge-authority execution result is invalid.");
        }

        return new EdgeAuthorityExecutionResult(
            wire.NormalizedInput,
            wire.ProposedCandidates,
            ToDecision(wire.Decision),
            wire.AuthorityEvidence,
            ToOrderReport(wire.CandidateOrderLeakageReport),
            ToIdentityReport(wire.IdentityLeakageReport),
            wire.UnitConfidence.Value,
            wire.EvidenceIds,
            wire.Warnings);
    }

    public override void Write(
        Utf8JsonWriter writer,
        EdgeAuthorityExecutionResult value,
        JsonSerializerOptions options)
    {
        var wire = new AuthorityExecutionResultWire(
            value.NormalizedInput,
            value.ProposedCandidates.ToArray(),
            FromDecision(value.Decision),
            value.AuthorityEvidence,
            FromOrderReport(value.CandidateOrderLeakageReport),
            FromIdentityReport(value.IdentityLeakageReport),
            value.UnitConfidence,
            value.EvidenceIds.ToArray(),
            value.Warnings.ToArray());
        JsonSerializer.Serialize(writer, wire, options);
    }

    private static EdgeAuthorityDecision ToDecision(DecisionWire? wire)
    {
        if (wire?.OrderedTargets is null
            || wire.FanOutCount is null
            || wire.KnowledgeEdge is null
            || wire.AuthoritySource is null
            || wire.AblationBaselineRank is null
            || wire.OrderedTargets.Any(target =>
                target is null
                || target.TargetSubPhaseId is null
                || target.Confidence is null))
        {
            throw new JsonException("The edge-authority decision is invalid.");
        }

        return new EdgeAuthorityDecision(
            wire.OrderedTargets
                .Select(target =>
                    (target.TargetSubPhaseId!, target.Confidence!.Value))
                .ToArray(),
            wire.FanOutCount.Value,
            wire.KnowledgeEdge.Value,
            wire.AuthoritySource,
            wire.AblationBaselineRank);
    }

    private static DecisionWire FromDecision(EdgeAuthorityDecision decision)
        => new(
            decision.OrderedTargets
                .Select(target => new TargetConfidenceWire(
                    target.TargetSubPhaseId,
                    target.Confidence))
                .ToArray(),
            decision.FanOutCount,
            decision.KnowledgeEdge,
            decision.AuthoritySource,
            decision.AblationBaselineRank.ToArray());

    private static DynamicGraphEdgeCandidateOrderLeakageReport ToOrderReport(
        OrderReportWire? wire)
    {
        if (wire?.EdgeId is null
            || wire.ResolvedAuthorityUnitId is null
            || wire.IsOrderInvariant is null
            || wire.Probes is null
            || wire.ExcludedLeakageSignals is null
            || wire.Probes.Any(probe =>
                probe is null
                || probe.ProbeId is null
                || probe.CandidateOrder is null
                || probe.Decision is null))
        {
            throw new JsonException("The candidate-order report is invalid.");
        }

        return new DynamicGraphEdgeCandidateOrderLeakageReport(
            wire.EdgeId,
            wire.ResolvedAuthorityUnitId,
            wire.IsOrderInvariant.Value,
            wire.Probes
                .Select(probe => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    probe.ProbeId!,
                    probe.CandidateOrder!,
                    ToDecision(probe.Decision)))
                .ToArray(),
            wire.ExcludedLeakageSignals);
    }

    private static OrderReportWire FromOrderReport(
        DynamicGraphEdgeCandidateOrderLeakageReport report)
        => new(
            report.EdgeId,
            report.ResolvedAuthorityUnitId,
            report.IsOrderInvariant,
            report.Probes
                .Select(probe => new OrderProbeWire(
                    probe.ProbeId,
                    probe.CandidateOrder.ToArray(),
                    FromDecision(probe.Decision)))
                .ToArray(),
            report.ExcludedLeakageSignals.ToArray());

    private static DynamicGraphEdgeIdentityLeakageReport ToIdentityReport(
        IdentityReportWire? wire)
    {
        if (wire?.EdgeId is null
            || wire.ResolvedAuthorityUnitId is null
            || wire.IsIdentityInvariant is null
            || wire.Probes is null
            || wire.ExcludedLeakageSignals is null
            || wire.Probes.Any(probe =>
                probe is null
                || probe.ProbeId is null
                || probe.ControlledSignal is null
                || probe.ControlValue is null
                || probe.Decision is null))
        {
            throw new JsonException("The identity-leakage report is invalid.");
        }

        return new DynamicGraphEdgeIdentityLeakageReport(
            wire.EdgeId,
            wire.ResolvedAuthorityUnitId,
            wire.IsIdentityInvariant.Value,
            wire.Probes
                .Select(probe => new DynamicGraphEdgeIdentityControlProbe(
                    probe.ProbeId!,
                    probe.ControlledSignal!,
                    probe.ControlValue!,
                    ToDecision(probe.Decision)))
                .ToArray(),
            wire.ExcludedLeakageSignals);
    }

    private static IdentityReportWire FromIdentityReport(
        DynamicGraphEdgeIdentityLeakageReport report)
        => new(
            report.EdgeId,
            report.ResolvedAuthorityUnitId,
            report.IsIdentityInvariant,
            report.Probes
                .Select(probe => new IdentityProbeWire(
                    probe.ProbeId,
                    probe.ControlledSignal,
                    probe.ControlValue,
                    FromDecision(probe.Decision)))
                .ToArray(),
            report.ExcludedLeakageSignals.ToArray());

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    private sealed record AuthorityExecutionResultWire(
        [property: JsonPropertyName("normalized_input")] EdgeAuthorityInput? NormalizedInput,
        [property: JsonPropertyName("proposed_candidates")]
            IReadOnlyList<EdgeCandidate>? ProposedCandidates,
        [property: JsonPropertyName("decision")] DecisionWire? Decision,
        [property: JsonPropertyName("authority_evidence")]
            DynamicGraphEdgeAuthorityEvidence? AuthorityEvidence,
        [property: JsonPropertyName("candidate_order_leakage_report")]
            OrderReportWire? CandidateOrderLeakageReport,
        [property: JsonPropertyName("identity_leakage_report")]
            IdentityReportWire? IdentityLeakageReport,
        [property: JsonPropertyName("unit_confidence")] double? UnitConfidence,
        [property: JsonPropertyName("evidence_ids")] IReadOnlyList<string>? EvidenceIds,
        [property: JsonPropertyName("warnings")] IReadOnlyList<string>? Warnings);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    private sealed record DecisionWire(
        [property: JsonPropertyName("ordered_targets")]
            IReadOnlyList<TargetConfidenceWire>? OrderedTargets,
        [property: JsonPropertyName("fan_out_count")] int? FanOutCount,
        [property: JsonPropertyName("knowledge_edge")] bool? KnowledgeEdge,
        [property: JsonPropertyName("authority_source")] string? AuthoritySource,
        [property: JsonPropertyName("ablation_baseline_rank")]
            IReadOnlyList<string>? AblationBaselineRank);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    private sealed record TargetConfidenceWire(
        [property: JsonPropertyName("target_sub_phase_id")] string? TargetSubPhaseId,
        [property: JsonPropertyName("confidence")] double? Confidence);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    private sealed record OrderReportWire(
        [property: JsonPropertyName("edge_id")] string? EdgeId,
        [property: JsonPropertyName("resolved_authority_unit_id")]
            string? ResolvedAuthorityUnitId,
        [property: JsonPropertyName("is_order_invariant")] bool? IsOrderInvariant,
        [property: JsonPropertyName("probes")] IReadOnlyList<OrderProbeWire>? Probes,
        [property: JsonPropertyName("excluded_leakage_signals")]
            IReadOnlyList<string>? ExcludedLeakageSignals);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    private sealed record OrderProbeWire(
        [property: JsonPropertyName("probe_id")] string? ProbeId,
        [property: JsonPropertyName("candidate_order")]
            IReadOnlyList<EdgeCandidate>? CandidateOrder,
        [property: JsonPropertyName("decision")] DecisionWire? Decision);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    private sealed record IdentityReportWire(
        [property: JsonPropertyName("edge_id")] string? EdgeId,
        [property: JsonPropertyName("resolved_authority_unit_id")]
            string? ResolvedAuthorityUnitId,
        [property: JsonPropertyName("is_identity_invariant")] bool? IsIdentityInvariant,
        [property: JsonPropertyName("probes")] IReadOnlyList<IdentityProbeWire>? Probes,
        [property: JsonPropertyName("excluded_leakage_signals")]
            IReadOnlyList<string>? ExcludedLeakageSignals);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    private sealed record IdentityProbeWire(
        [property: JsonPropertyName("probe_id")] string? ProbeId,
        [property: JsonPropertyName("controlled_signal")] string? ControlledSignal,
        [property: JsonPropertyName("control_value")] string? ControlValue,
        [property: JsonPropertyName("decision")] DecisionWire? Decision);
}
