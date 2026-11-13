// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using TrainableRouter.Contracts;

public sealed class OrchestrationAcceptanceGateEvaluator
{
    private readonly OrchestrationScenarioPhasePlanner _phasePlanner;

    public OrchestrationAcceptanceGateEvaluator()
        : this(new OrchestrationScenarioPhasePlanner())
    {
    }

    public OrchestrationAcceptanceGateEvaluator(OrchestrationScenarioPhasePlanner phasePlanner)
    {
        _phasePlanner = phasePlanner;
    }

    public async Task<OperationResult<OrchestrationAcceptanceGateResult>> EvaluateAsync(
        ITrainableRoutingFlow flow,
        IReadOnlyList<OrchestrationPromptAcceptanceCase> cases,
        CancellationToken ct = default)
    {
        if (flow is null)
            return OperationResult<OrchestrationAcceptanceGateResult>.Fail("Trainable routing flow is required.", "ROUTING_FLOW_REQUIRED");

        if (cases is null || cases.Count == 0)
            return OperationResult<OrchestrationAcceptanceGateResult>.Fail("At least one acceptance case is required.", "NO_ACCEPTANCE_CASES");

        var results = new List<OrchestrationAcceptanceCaseResult>();
        foreach (var scenario in cases)
        {
            var request = BuildRoutingRequest(scenario);
            var routed = await flow.RouteAsync(request, ct);
            if (!routed.IsSuccess)
                return routed.MapError<OrchestrationAcceptanceGateResult>();

            var planned = _phasePlanner.PlanPhases(scenario.Prompt, scenario.ExpectedSteps);
            if (!planned.IsSuccess)
                return planned.MapError<OrchestrationAcceptanceGateResult>();

            var missing = scenario.ExpectedPhaseTags
                .Where(expected => !planned.Value!.Contains(expected, StringComparer.Ordinal))
                .ToArray();

            results.Add(new OrchestrationAcceptanceCaseResult(
                scenario.ScenarioId,
                routed.Value!.ChosenUnitId,
                routed.Value.ChosenUnitId == OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                routed.Value.Confidence,
                planned.Value!,
                missing,
                missing.Length == 0));
        }

        var routedCorrect = results.Count(result => result.RoutedToExpectedUnit);
        var phaseCorrect = results.Count(result => result.PhaseGatePassed);
        var gate = new OrchestrationAcceptanceGateResult(
            cases.Count,
            routedCorrect,
            phaseCorrect,
            routedCorrect / (double)cases.Count,
            phaseCorrect / (double)cases.Count,
            results.ToArray());

        return OperationResult<OrchestrationAcceptanceGateResult>.Ok(gate);
    }

    private static RoutingRequest BuildRoutingRequest(OrchestrationPromptAcceptanceCase scenario)
        => new(
            $"acceptance-{scenario.ScenarioId}",
            scenario.Prompt,
            string.Join(" ", scenario.ExpectedSteps),
            null,
            "orchestration",
            [
                OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                OrchestrationRoutingCurriculum.TranslationRuEnUnitId,
                OrchestrationRoutingCurriculum.DocumentationDotNetUnitId
            ],
            new Dictionary<string, string>
            {
                ["source"] = "orchestrator-prompts.txt",
                ["scenario_id"] = scenario.ScenarioId
            });
}
