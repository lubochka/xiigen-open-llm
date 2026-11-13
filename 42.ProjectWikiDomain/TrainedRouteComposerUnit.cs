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

        // FIX-P118: this is the whole repair. Before it the generalized route ended on the
        // line above, so a learned plan text never reached the answer at all: the body
        // carried only the lesson number written by style_source. Experiment 118 measured
        // that limit line by line - 0 of 2 answers carried a single word of the lesson plan,
        // in both of its arms.
        // The body is appended only when the request matches the chosen lesson request
        // exactly, and it is written by the same product serializer the learned route
        // already uses, so the two routes cannot drift apart in how a plan body looks.
        if (nearest is not null && IsExactPromptMatch(prompt, nearest.Prompt))
            lines.Add(SerializeChosen(nearest.Chosen));

        return string.Join(Environment.NewLine, lines);
    }

    // FIX-P118: the condition of the repair above, kept deliberately narrower than the
    // nearest-lesson search. That search ranks lessons by the share of shared words
    // (OrchestrationKnnStore.Jaccard), where a request carrying the same words in a
    // different order already scores a full 1.0. This check is not a similarity measure and
    // not a new threshold: the two requests are compared as text after whitespace collapsing
    // and lowercasing, so a match is either exact or absent, and a reordered request receives
    // no plan body.
    // A null request or a null lesson request cannot throw here, because NormalizeOneLine
    // maps null to the empty string.
    public static bool IsExactPromptMatch(string prompt, string lessonPrompt)
        => string.Equals(
            NormalizeOneLine(prompt).ToLowerInvariant(),
            NormalizeOneLine(lessonPrompt).ToLowerInvariant(),
            StringComparison.Ordinal);

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
