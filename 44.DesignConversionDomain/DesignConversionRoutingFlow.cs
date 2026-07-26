// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed record DesignConversionRuntimeFlowInput(
    SubmittedDesignConversionRequest Request,
    IReadOnlyList<RenderQaResultRecord> QaResults,
    IReadOnlyList<DesignConversionQaComparisonArtifact> ComparisonArtifacts,
    IReadOnlyList<string> AiTeacherEvidenceRefs,
    IReadOnlyDictionary<string, string> DeterministicEvidenceRefs);

public sealed record DesignConversionRuntimeFlowResult(
    SubmittedDesignConversionRequest Request,
    DesignConversionCriticUnitOutput CriticOutput,
    DesignConversionImproverUnitOutput ImproverOutput,
    DesignConversionImprovementCandidate Candidate,
    DesignConversionImprovementOutput Output,
    IReadOnlyList<string> UnitExecutionOrder,
    IReadOnlyList<string> EvidenceRefs,
    bool DeterministicLogicClaimedAsLearning);

internal sealed class DesignConversionRuntimeFlowResultCapture
{
    public const string MetadataKey = "runtime_flow_result_capture";

    private readonly object _gate = new();
    private DesignConversionRuntimeFlowResult? _result;
    private CaptureState _state;

    public bool TryCapture(DesignConversionRuntimeFlowResult result)
    {
        lock (_gate)
        {
            if (_state != CaptureState.Empty)
                return false;

            _result = result;
            _state = CaptureState.Captured;
            return true;
        }
    }

    public bool TryTake(out DesignConversionRuntimeFlowResult result)
    {
        lock (_gate)
        {
            if (_state != CaptureState.Captured || _result is null)
            {
                result = null!;
                return false;
            }

            result = _result;
            _result = null;
            _state = CaptureState.Consumed;
            return true;
        }
    }

    private enum CaptureState
    {
        Empty,
        Captured,
        Consumed
    }
}

public sealed class DesignConversionRoutingFlow : ILogicalFlow<DesignConversionRuntimeFlowInput>
{
    public const string FlowId = "design-conversion-routing-flow";
    public const string ScenarioId = "design-conversion-runtime-flow";

    private readonly IDesignConversionCriticUnit _criticUnit;
    private readonly IDesignConversionImproverUnit _improverUnit;

    public DesignConversionRoutingFlow(
        IDesignConversionCriticUnit? criticUnit = null,
        IDesignConversionImproverUnit? improverUnit = null)
    {
        _criticUnit = criticUnit ?? new DesignConversionRuntimeScaffoldCriticUnit();
        _improverUnit = improverUnit ?? new DesignConversionRuntimeScaffoldImproverUnit();
    }

    public async Task<OperationResult<DesignConversionRuntimeFlowResult>> RunAsync(
        DesignConversionRuntimeFlowInput input,
        string runId,
        LogicalExecutionFlags? flags = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionRuntimeFlowResult>.Fail(
                "DesignConversion routing flow cancelled.",
                "CANCELLED");

        if (input is null || input.Request is null)
            return OperationResult<DesignConversionRuntimeFlowResult>.Fail(
                "DesignConversion runtime flow input is required.",
                "FLOW_INPUT_REQUIRED");

        var executionFlags = flags ?? new LogicalExecutionFlags();
        var executionMetadata = metadata ?? new Dictionary<string, object>();
        var unitOrder = new List<string>();

        var criticInput = new DesignConversionCriticUnitInput(
            Request: input.Request,
            QaResults: input.QaResults,
            ComparisonArtifacts: input.ComparisonArtifacts,
            AiTeacherEvidenceRefs: input.AiTeacherEvidenceRefs,
            DeterministicEvidenceRefs: input.DeterministicEvidenceRefs);

        var criticResult = await _criticUnit.ExecuteAsync(new LogicalUnitRequest<DesignConversionCriticUnitInput>(
            RunId: runId,
            UnitId: DesignConversionRuntimeScaffoldCriticUnit.UnitId,
            UnitRole: DesignConversionRuntimeScaffoldCriticUnit.Role,
            DomainTag: DesignConversionRuntimeScaffoldCriticUnit.DomainTag,
            Input: criticInput,
            Flags: executionFlags,
            Metadata: executionMetadata), cancellationToken);
        if (!criticResult.IsSuccess)
            return criticResult.MapError<DesignConversionRuntimeFlowResult>();

        unitOrder.Add(criticResult.Value!.UnitId);
        var criticOutput = criticResult.Value.Output;
        var candidate = CreateCandidate(input.Request, runId, criticOutput);

        var improverInput = new DesignConversionImproverUnitInput(
            Request: input.Request,
            CriticVerdict: criticOutput.Verdict,
            Candidate: candidate,
            QaEvidenceRefs: criticOutput.QaEvidenceRefs,
            AiTeacherEvidenceRefs: criticOutput.AiTeacherEvidenceRefs);

        var improverResult = await _improverUnit.ExecuteAsync(new LogicalUnitRequest<DesignConversionImproverUnitInput>(
            RunId: runId,
            UnitId: DesignConversionRuntimeScaffoldImproverUnit.UnitId,
            UnitRole: DesignConversionRuntimeScaffoldImproverUnit.Role,
            DomainTag: DesignConversionRuntimeScaffoldImproverUnit.DomainTag,
            Input: improverInput,
            Flags: executionFlags,
            Metadata: executionMetadata), cancellationToken);
        if (!improverResult.IsSuccess)
            return improverResult.MapError<DesignConversionRuntimeFlowResult>();

        unitOrder.Add(improverResult.Value!.UnitId);
        var improverOutput = improverResult.Value.Output;
        var evidenceRefs = criticResult.Value.EvidenceIds
            .Concat(improverResult.Value.EvidenceIds)
            .Append("DESIGN_CONVERSION_ROUTING_FLOW_CRITIC_THEN_IMPROVER")
            .Append("DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE")
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return OperationResult<DesignConversionRuntimeFlowResult>.Ok(new DesignConversionRuntimeFlowResult(
            Request: input.Request,
            CriticOutput: criticOutput,
            ImproverOutput: improverOutput,
            Candidate: improverOutput.Candidate,
            Output: improverOutput.Output,
            UnitExecutionOrder: unitOrder,
            EvidenceRefs: evidenceRefs,
            DeterministicLogicClaimedAsLearning:
                criticOutput.DeterministicLogicClaimedAsLearning ||
                improverOutput.DeterministicLogicClaimedAsLearning));
    }

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<DesignConversionRuntimeFlowInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail(
                "DesignConversion routing flow cancelled.",
                "CANCELLED");

        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail(
                "Logical flow request is required.",
                "REQUEST_REQUIRED");

        if (!string.Equals(request.FlowId, FlowId, StringComparison.Ordinal))
            return OperationResult<LogicalFlowResult>.Fail(
                $"Unsupported flow id '{request.FlowId}'. Expected '{FlowId}'.",
                "UNSUPPORTED_FLOW_ID");

        var domainResult = await RunAsync(
            request.Input,
            request.RunId,
            request.Flags,
            request.Metadata,
            cancellationToken);
        if (!domainResult.IsSuccess)
            return domainResult.MapError<LogicalFlowResult>();

        var value = domainResult.Value!;
        if (request.Metadata.TryGetValue(DesignConversionRuntimeFlowResultCapture.MetadataKey, out var captureValue))
        {
            if (captureValue is not DesignConversionRuntimeFlowResultCapture capture)
                return OperationResult<LogicalFlowResult>.Fail(
                    "DesignConversion runtime flow result capture is invalid.",
                    "RUNTIME_FLOW_RESULT_CAPTURE_INVALID");
            if (!capture.TryCapture(value))
                return OperationResult<LogicalFlowResult>.Fail(
                    "DesignConversion runtime flow result capture is already used.",
                    "RUNTIME_FLOW_RESULT_CAPTURE_ALREADY_USED");
        }

        var summary = new LogicalFlowSummary(
            SummaryId: $"{request.RunId}-design-conversion-runtime-summary",
            Tier: 0,
            BaselinePrimary: 0.0,
            EnhancedPrimary: value.CriticOutput.Verdict.Score,
            PrimaryMetricName: "critic_input_evidence_score",
            LowerIsBetter: false,
            Passed: value.CriticOutput.Verdict.Passed,
            ShouldScaleUp: false,
            NextAction: value.CriticOutput.Verdict.Passed
                ? "return_candidate"
                : "route_candidate_to_trained_improver_or_render_qa_when_available",
            ScalingSignal: 0.0,
            ScalingSignalNote: "Runtime scaffold only; no DPO, checkpoint, or learned visual QA run.",
            Notes: "DesignConversion flow calls Critic then Improver over Slice 1 contracts and treats QA data as input evidence.",
            BaselineTimeMs: 0.0,
            EnhancedTimeMs: 0.0);

        var result = new LogicalFlowResult(
            FlowId: FlowId,
            ScenarioId: request.ScenarioId,
            OverallVerdict: value.CriticOutput.Verdict.Passed
                ? "DESIGN_CONVERSION_INPUT_EVIDENCE_PASSED"
                : "DESIGN_CONVERSION_INPUT_EVIDENCE_NEEDS_IMPROVEMENT",
            FinalHumanResult: value.Output.OutputId,
            Summary: summary,
            Metrics: new Dictionary<string, string>
            {
                ["critic_unit_id"] = value.UnitExecutionOrder.ElementAtOrDefault(0) ?? "",
                ["improver_unit_id"] = value.UnitExecutionOrder.ElementAtOrDefault(1) ?? "",
                ["unit_order"] = string.Join(">", value.UnitExecutionOrder),
                ["critic_verdict_id"] = value.CriticOutput.Verdict.VerdictId,
                ["critic_passed"] = value.CriticOutput.Verdict.Passed.ToString(),
                ["critic_score"] = value.CriticOutput.Verdict.Score.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture),
                ["candidate_id"] = value.Candidate.CandidateId,
                ["output_id"] = value.Output.OutputId,
                ["deterministic_logic_claimed_as_learning"] = value.DeterministicLogicClaimedAsLearning.ToString(),
                ["qa_data_role"] = "input_evidence_only"
            },
            EvidenceIds: value.EvidenceRefs.ToArray(),
            Warnings:
            [
                "DESIGN_CONVERSION_RUNTIME_SCAFFOLD_NOT_LEARNED_OUTPUT",
                DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
            ]);

        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    private static DesignConversionImprovementCandidate CreateCandidate(
        SubmittedDesignConversionRequest request,
        string runId,
        DesignConversionCriticUnitOutput criticOutput)
        => new(
            CandidateId: $"{request.RequestId}:{runId}:runtime-scaffold-candidate",
            RequestId: request.RequestId,
            Html: request.Html,
            Css: request.Css,
            ChangeReasons:
            [
                "Runtime scaffold candidate created after critic verdict; no learned visual fix is claimed."
            ],
            EvidenceRefs: criticOutput.QaEvidenceRefs
                .Append($"critic-output:{criticOutput.OutputId}")
                .Append($"critic-verdict:{criticOutput.Verdict.VerdictId}")
                .Append("DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE")
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Metadata: new Dictionary<string, string>
            {
                ["critic_verdict_id"] = criticOutput.Verdict.VerdictId,
                ["critic_passed"] = criticOutput.Verdict.Passed.ToString(),
                ["runtime_scaffold_candidate"] = bool.TrueString,
                ["learned_improvement_claimed"] = bool.FalseString
            });
}
