// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.RegularExpressions;

public static class HumanIntentBoundaryClassifier
{
    private static readonly Regex FlowSpanRegex = new(
        @"\b(?:flow\s*-?\s*)?(\d{1,2})(?:\s*(?:\*|-\*|-[a-z][a-z0-9_-]*))?\b|\bflow(\d{1,2})\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ForbiddenSpanRegex = new(
        @"\b(\d{1,2})\b\s*(?:not\s+now|not\s+needed|exclude|skip|later|no\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static HumanIntentContract Classify(string prompt)
    {
        var text = prompt ?? string.Empty;
        var normalized = text.ToLowerInvariant();
        var requestedSpans = DetectRequestedFlowSpans(text);
        var forbidden = DetectForbiddenFlowScope(text);
        var allowed = requestedSpans
            .Select(FormatFlowScope)
            .Where(value => value is not null)
            .Cast<string>()
            .Except(forbidden, StringComparer.Ordinal)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var phases = DetectPhaseScope(text);
        var constraints = new List<string>();
        var requiresExplicitApprovalGate = ContainsAny(
            normalized,
            "until i approve",
            "until approved",
            "wait for approval",
            "pending approval",
            "requires approval",
            "require approval",
            "do not continue until",
            "don't continue until");
        if (requiresExplicitApprovalGate)
            constraints.Add("approval_gate_required");

        var kind = IntentKind.ImplementationExecutionRequest;
        if (ContainsAny(normalized, "load planning skills", "load skills", "planning skills", "session init"))
        {
            kind = IntentKind.SkillLoadSessionInit;
        }
        else if (ContainsAny(normalized, "explain", "explanation", "why", "how does", "how works", "объясни"))
        {
            kind = IntentKind.HumanOrchestrationExplanation;
        }
        else if (ContainsAny(normalized, "translate flow", "migration", "migrate", "from python", "to node", "react native"))
        {
            kind = IntentKind.DocumentationMigration;
            constraints.Add("documentation_migration_mode2_only");
        }
        else if (ContainsAny(normalized, "phase only", "proceed", "continue phase", "next phase") ||
                 Regex.IsMatch(normalized, @"\bphase\s+\d"))
        {
            kind = IntentKind.PhaseContinuation;
            constraints.Add("phase_scope_only");
        }
        else if (ContainsAny(normalized, "review architecture", "architecture review", "architectural review"))
        {
            kind = IntentKind.ArchitectureReview;
            constraints.Add("architecture_review_mode2_only");
        }
        else if (ContainsAny(normalized, "testing plan", "test plan", "create tests", "qa plan"))
        {
            kind = IntentKind.TestingPlan;
            constraints.Add("testing_plan_mode2_only");
        }
        else if (requestedSpans.Count > 0)
        {
            kind = IntentKind.KnownFlowPlanning;
        }

        return new HumanIntentContract(
            Kind: kind,
            AllowedFlowScope: allowed,
            ForbiddenFlowScope: forbidden,
            PhaseScope: phases,
            RequiresApprovalGate: requiresExplicitApprovalGate ||
                                  kind is IntentKind.ArchitectureReview or IntentKind.TestingPlan,
            RequiresSaveState: kind is IntentKind.PhaseContinuation or IntentKind.KnownFlowPlanning,
            HumanConstraints: constraints,
            RequestedFlowTextSpans: requestedSpans);
    }

    private static IReadOnlyList<string> DetectRequestedFlowSpans(string text)
        => FlowSpanRegex.Matches(text)
            .Select(match => match.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();

    private static IReadOnlyList<string> DetectForbiddenFlowScope(string text)
        => ForbiddenSpanRegex.Matches(text ?? string.Empty)
            .Select(match => FormatFlowScope(match.Groups[1].Value))
            .Where(value => value is not null)
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static string? FormatFlowScope(string raw)
    {
        var digits = Regex.Match(raw ?? string.Empty, @"\d{1,2}").Value;
        if (!int.TryParse(digits, out var number) || number < 1 || number > 99)
            return null;

        return $"FLOW-{number:D2}";
    }

    private static bool ContainsAny(string text, params string[] needles)
        => needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyList<string> DetectPhaseScope(string prompt)
    {
        var matches = Regex.Matches(prompt ?? string.Empty, @"\b(?:phase\s*)?(\d+(?:\.\d+)?(?:-\d+(?:\.\d+)?)?)\b", RegexOptions.IgnoreCase)
            .Select(match => match.Groups[1].Value)
            .Where(value => value.Contains('.', StringComparison.Ordinal) ||
                            Regex.IsMatch(prompt ?? string.Empty, @"\bphase\b", RegexOptions.IgnoreCase))
            .Distinct(StringComparer.Ordinal)
            .Take(4)
            .ToArray();

        if ((prompt ?? string.Empty).Contains("2.5", StringComparison.Ordinal) && !matches.Contains("2.5", StringComparer.Ordinal))
            return matches.Concat(["2.5"]).Distinct(StringComparer.Ordinal).ToArray();

        return matches;
    }
}
