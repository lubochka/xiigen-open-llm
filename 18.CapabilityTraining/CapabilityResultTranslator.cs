// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

// This translator converts a learned capability routing result into developer-readable text.
public static class CapabilityResultTranslator
{
    public static string Translate(string knnOutput, LogicalSubPhase subPhase, string originalPrompt)
    {
        var promptLine = string.IsNullOrWhiteSpace(originalPrompt)
            ? string.Empty
            : $"For your request: \"{originalPrompt.Trim()}\"";
        var subPhaseLine = $"Sub-phase {subPhase.SubPhaseId} ({subPhase.Name})";
        var scopeLine = string.IsNullOrWhiteSpace(subPhase.Purpose)
            ? string.Empty
            : $"Scope: {subPhase.Purpose}";
        var focusLine = subPhase.PredicateIds.Length == 0
            ? string.Empty
            : $"Focus areas: {string.Join(", ", subPhase.PredicateIds)}";

        return string.Join(Environment.NewLine, new[] { promptLine, subPhaseLine, scopeLine, focusLine }
            .Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    public static string TranslateFromKnnOutput(string knnOutput, string originalPrompt)
    {
        if (string.IsNullOrWhiteSpace(knnOutput))
            return string.IsNullOrWhiteSpace(originalPrompt)
                ? "Capability identified."
                : $"Capability identified for: \"{originalPrompt.Trim()}\"";

        return string.IsNullOrWhiteSpace(originalPrompt)
            ? knnOutput
            : $"For your request: \"{originalPrompt.Trim()}\"{Environment.NewLine}{knnOutput}";
    }
}
