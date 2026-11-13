// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class OrchestrationPlanOrchestrator : ILogicalOrchestrator
{
    public const string FlowId = "orchestration-plan-flow";
    public const string ScenarioId = "orchestration-plan";

    private readonly OrchestrationPlanMachine _machine;

    public OrchestrationPlanOrchestrator(OrchestrationPlanMachine machine)
    {
        _machine = machine;
    }

    public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalOrchestratorResult>.Fail("Orchestration plan cancelled.", "CANCELLED");
        if (request is null)
            return OperationResult<LogicalOrchestratorResult>.Fail("Request is required.", "REQUEST_REQUIRED");

        var generated = await _machine.GeneratePlanAsync(request.UserRequest, cancellationToken);
        if (!generated.IsSuccess)
            return generated.MapError<LogicalOrchestratorResult>();

        var value = generated.Value!;
        var metrics = new Dictionary<string, string>
        {
            ["route_mode"] = value.RouteMode.ToString(),
            ["boundary_source"] = value.BoundarySource.ToString(),
            ["ml_generated"] = value.MlGenerated.ToString(),
            ["intent_kind"] = value.IntentKind.ToString(),
            ["predicted_flow_ref"] = value.FlowGrounding?.FlowRef ?? ""
        };

        var summary = new LogicalFlowSummary(
            "orchestration-plan-summary",
            1,
            0.0,
            value.MlGenerated ? 1.0 : 0.0,
            "ml_generated",
            false,
            true,
            false,
            "complete",
            1.0,
            "route generated",
            "",
            0,
            0);

        var flow = new LogicalFlowResult(
            FlowId,
            ScenarioId,
            "ORCHESTRATION_PLAN_GENERATED",
            value.PlanText,
            summary,
            metrics,
            [],
            value.Warnings.ToArray());

        var result = new LogicalOrchestratorResult(
            request.ScenarioId,
            FlowId,
            "ORCHESTRATION_PLAN_GENERATED",
            value.PlanText,
            request.Flags ?? new LogicalExecutionFlags(),
            flow)
        {
            Warnings = value.Warnings.ToArray()
        };

        return OperationResult<LogicalOrchestratorResult>.Ok(result);
    }
}
