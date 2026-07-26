// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class DistributedModelOrchestratorFlow : ILogicalFlow<DistributedModelOrchestrationRequest>
{
    public const string FlowId = "distributed-model-orchestrator-flow";
    public const string ScenarioId = "distributed_model_orchestration";

    private readonly DistributedRoutingRuntime _runtime;
    private readonly OrchestrationTaskDecompositionFlow _decompositionFlow;

    public DistributedModelOrchestratorFlow(DistributedRoutingRuntime runtime)
    {
        _runtime = runtime;
        _decompositionFlow = new OrchestrationTaskDecompositionFlow(runtime);
    }

    public async Task<OperationResult<DistributedModelOrchestrationResult>> OrchestrateAsync(
        DistributedModelOrchestrationRequest request,
        CancellationToken ct = default)
    {
        if (request is null)
            return OperationResult<DistributedModelOrchestrationResult>.Fail("Distributed orchestration request is required.", "DISTRIBUTED_ORCHESTRATION_REQUEST_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.Prompt))
            return OperationResult<DistributedModelOrchestrationResult>.Fail("Prompt is required.", "PROMPT_REQUIRED");

        var routingRequest = new RoutingRequest(
            request.RequestId,
            request.Prompt,
            string.Join(" ", request.StepHints),
            request.RequiredDirection,
            request.DomainHint,
            request.CandidateUnitIds,
            request.Context);

        var routed = await DistributedRoutingBootstrap.RouteAndLocateAsync(_runtime, routingRequest, ct);
        if (!routed.IsSuccess)
            return routed.MapError<DistributedModelOrchestrationResult>();

        if (!string.Equals(routed.Value!.Decision.ChosenUnitId, OrchestrationRoutingCurriculum.OrchestrationV309UnitId, StringComparison.Ordinal))
        {
            return OperationResult<DistributedModelOrchestrationResult>.Ok(new DistributedModelOrchestrationResult(
                request.RequestId,
                request.ScenarioId,
                routed.Value.Decision.ChosenUnitId,
                routed.Value.Manifest.DomainTag,
                routed.Value.Manifest.TrainingRunId,
                routed.Value.Manifest.UnitEndpoint,
                [],
                [],
                routed.Value.Decision.Confidence,
                "ROUTED_TO_SPECIALIST_MODEL"));
        }

        var decomposed = await _decompositionFlow.DecomposeAsync(new OrchestrationTaskDecompositionRequest(
            request.RequestId,
            request.ScenarioId,
            request.Prompt,
            request.StepHints), ct);
        if (!decomposed.IsSuccess)
            return decomposed.MapError<DistributedModelOrchestrationResult>();

        return OperationResult<DistributedModelOrchestrationResult>.Ok(new DistributedModelOrchestrationResult(
            request.RequestId,
            request.ScenarioId,
            routed.Value.Decision.ChosenUnitId,
            routed.Value.Manifest.DomainTag,
            routed.Value.Manifest.TrainingRunId,
            routed.Value.Manifest.UnitEndpoint,
            decomposed.Value!.PhaseTags,
            decomposed.Value.Phases,
            routed.Value.Decision.Confidence,
            "ORCHESTRATION_MODEL_SELECTED_AND_DECOMPOSED"));
    }

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<DistributedModelOrchestrationRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail("Logical flow request is required.", "FLOW_REQUEST_REQUIRED");

        var orchestrated = await OrchestrateAsync(request.Input, cancellationToken);
        if (!orchestrated.IsSuccess)
            return orchestrated.MapError<LogicalFlowResult>();

        var value = orchestrated.Value!;
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["orchestrator_design_pattern"] = true.ToString(),
            ["router_flow_id"] = TrainableRoutingFlow.FlowId,
            ["chosen_unit_id"] = value.ChosenUnitId,
            ["domain_tag"] = value.DomainTag,
            ["training_run_id"] = value.TrainingRunId,
            ["remote_endpoint"] = value.RemoteEndpoint,
            ["phase_tags"] = string.Join(";", value.PhaseTags),
            ["phase_count"] = value.Phases.Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["step_count"] = value.Phases.Sum(phase => phase.Steps.Length).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["routing_confidence"] = value.RoutingConfidence.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture)
        };

        var summary = new LogicalFlowSummary(
            request.RunId,
            value.Phases.Length == 0 ? 1 : value.Phases.Length,
            0,
            value.RoutingConfidence,
            "routing_confidence",
            false,
            true,
            false,
            value.Verdict,
            value.RoutingConfidence,
            value.ChosenUnitId,
            $"Distributed model orchestrator selected {value.ChosenUnitId} and produced {value.Phases.Length} phase groups.",
            0,
            0);

        var result = new LogicalFlowResult(
            request.FlowId,
            request.ScenarioId,
            value.Verdict,
            $"chosen={value.ChosenUnitId}; phases={string.Join(",", value.PhaseTags)}",
            summary,
            metrics,
            [$"{value.RequestId}:distributed-orchestrator:{value.ChosenUnitId}"],
            []);

        return OperationResult<LogicalFlowResult>.Ok(result);
    }
}

public sealed record DistributedModelOrchestrationRequest(
    string RequestId,
    string ScenarioId,
    string Prompt,
    string[] StepHints,
    string? RequiredDirection,
    string? DomainHint,
    string[] CandidateUnitIds,
    IReadOnlyDictionary<string, string> Context);

public sealed record DistributedModelOrchestrationResult(
    string RequestId,
    string ScenarioId,
    string ChosenUnitId,
    string DomainTag,
    string TrainingRunId,
    string RemoteEndpoint,
    string[] PhaseTags,
    OrchestrationTaskPhase[] Phases,
    double RoutingConfidence,
    string Verdict);
