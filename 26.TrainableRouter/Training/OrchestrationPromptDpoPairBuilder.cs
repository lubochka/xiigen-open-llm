// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;

public sealed class OrchestrationPromptDpoPairBuilder
{
    public OperationResult<OrchestrationPromptDpoPair[]> BuildPromptFilePairs(
        IReadOnlyList<OrchestrationPromptAcceptanceCase> cases)
    {
        if (cases is null || cases.Count == 0)
            return OperationResult<OrchestrationPromptDpoPair[]>.Fail("At least one orchestration prompt case is required.", "NO_ORCHESTRATION_PROMPT_CASES");

        var pairs = cases
            .Select((scenario, index) => BuildPair(index + 1, scenario))
            .ToArray();

        return OperationResult<OrchestrationPromptDpoPair[]>.Ok(pairs);
    }

    private static OrchestrationPromptDpoPair BuildPair(
        int index,
        OrchestrationPromptAcceptanceCase scenario)
    {
        var phases = string.Join(" -> ", scenario.ExpectedPhaseTags);
        var steps = string.Join(" | ", scenario.ExpectedSteps);
        var chosen = $"Route to {OrchestrationRoutingCurriculum.OrchestrationV309UnitId}, load the V309 remote manifest, and decompose the request into phases: {phases}. Required steps: {steps}.";
        var rejectedUnit = ChooseRejectedUnit(scenario);
        var rejected = $"Route to {rejectedUnit} or start direct implementation without phase decomposition.\n[WRONG: this scenario requires orchestration routing plus ordered phase decomposition. A specialist or direct implementation route cannot preserve the required phase chain '{phases}', so downstream work loses dependency order, feedback loops, and validation gates.]";

        return new OrchestrationPromptDpoPair(
            $"orch-prompt-file-dpo-{index:000}",
            scenario.ScenarioId,
            scenario.Prompt,
            chosen,
            rejected,
            OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
            rejectedUnit,
            scenario.ExpectedPhaseTags);
    }

    private static string ChooseRejectedUnit(OrchestrationPromptAcceptanceCase scenario)
    {
        var text = $"{scenario.Prompt} {string.Join(" ", scenario.ExpectedSteps)}".ToLowerInvariant();
        if (text.Contains("documentation", StringComparison.OrdinalIgnoreCase) || text.Contains("document", StringComparison.OrdinalIgnoreCase))
            return OrchestrationRoutingCurriculum.DocumentationDotNetUnitId;

        if (text.Contains("language", StringComparison.OrdinalIgnoreCase) || text.Contains("translate", StringComparison.OrdinalIgnoreCase))
            return OrchestrationRoutingCurriculum.TranslationRuEnUnitId;

        return OrchestrationRoutingCurriculum.DocumentationDotNetUnitId;
    }
}

public sealed record OrchestrationPromptDpoPair(
    string PairId,
    string ScenarioId,
    string ASide,
    string Chosen,
    string Rejected,
    string ChosenUnitId,
    string RejectedUnitId,
    string[] ExpectedPhaseTags);
