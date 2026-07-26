// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

public sealed class TrainedRouteComposerUnit
{
    public string RenderLearnedFlowPlan(
        OrchestrationTrainingPair learned,
        FlowGroundingResult grounding,
        HumanIntentContract contract,
        string prompt)
    {
        var lines = new List<string>
        {
            "LEARNED_FLOW_GROUNDING_PLAN",
            $"flow_ref={grounding.FlowRef}",
            $"confidence={grounding.Confidence:0.000}",
            $"intent={contract.Kind}",
            $"prompt={NormalizeOneLine(prompt)}",
            $"title={NormalizeOneLine(learned.Chosen.Title)}"
        };

        if (contract.PhaseScope.Count > 0)
            lines.Add($"phase_scope={string.Join(",", contract.PhaseScope)}");
        if (contract.RequiresApprovalGate)
            lines.Add("approval_gate=required");
        if (contract.RequiresSaveState)
            lines.Add("save_state=required");

        lines.Add("mode=learned_flow_grounding");
        lines.Add(SerializeChosen(learned.Chosen));
        return string.Join(Environment.NewLine, lines);
    }

    public string RenderMode2Plan(
        OrchestrationTrainingPair? nearest,
        HumanIntentContract contract,
        string prompt)
    {
        var lines = new List<string>();
        lines.Add("ORCHESTRATION_PLAN");
        lines.Add($"intent={contract.Kind}");
        lines.Add($"prompt={NormalizeOneLine(prompt)}");

        if (contract.AllowedFlowScope.Count > 0)
            lines.Add($"flow_scope={string.Join(",", contract.AllowedFlowScope)}");
        if (contract.PhaseScope.Count > 0)
            lines.Add($"phase_scope={string.Join(",", contract.PhaseScope)}");
        if (contract.RequiresApprovalGate)
            lines.Add("approval_gate=required");
        if (contract.RequiresSaveState)
            lines.Add("save_state=required");

        if (nearest is not null)
            lines.Add($"style_source={nearest.PairId}");

        lines.Add("mode=generalized_non_lookup");
        return string.Join(Environment.NewLine, lines);
    }

    public string RenderHumanExplanation(
        OrchestrationTrainingPair? nearest,
        HumanIntentContract contract,
        string prompt)
    {
        var flowScope = contract.AllowedFlowScope.Count > 0
            ? string.Join(",", contract.AllowedFlowScope)
            : nearest?.FlowRef ?? "general";

        return string.Join(Environment.NewLine, [
            "HUMAN_ORCHESTRATION_EXPLANATION",
            $"flow_scope={flowScope}",
            $"prompt={NormalizeOneLine(prompt)}",
            nearest is null
                ? "explanation=This request asks for a human-readable orchestration explanation."
                : $"explanation=This follows the learned explanation style from {nearest.PairId}.",
            "mode=human_explanation"
        ]);
    }

    public static string SerializeChosen(OrchestrationPlanChosen chosen)
        => string.Join(Environment.NewLine, [
            $"FLOW_REF={chosen.FlowRef}",
            $"TITLE={NormalizeOneLine(chosen.Title)}",
            chosen.PlanText
        ]);

    private static string NormalizeOneLine(string text)
        => string.Join(" ", (text ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
