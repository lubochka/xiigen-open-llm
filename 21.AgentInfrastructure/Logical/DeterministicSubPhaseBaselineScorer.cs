// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

// This immutable carrier keeps one raw deterministic baseline score and its visible predicate evidence.
internal sealed record SubPhaseBaselineScore(
    LogicalSubPhaseEntry Entry,
    double Score,
    string[] EvidencePredicates);

// This shared scorer preserves the existing multi-router vocabulary, coefficients, and relevance floor.
internal static class DeterministicSubPhaseBaselineScorer
{
    // The token separators mirror the original multi-router source vocabulary splitting.
    private static readonly char[] TokenSeparators = [' ', '\t', '\r', '\n', '-', '_', '.', ',', ':', ';', '/', '\\', '(', ')', '[', ']', '{', '}'];

    // Anchor terms prevent one broad predicate from selecting every sub-phase in a family.
    private static readonly IReadOnlyDictionary<string, string[]> SubPhaseAnchorTerms = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["1.1"] = ["token", "tokens", "component", "typography", "color", "spacing"],
        ["1.2"] = ["layout", "responsive", "density", "page", "structure"],
        ["1.3"] = ["interaction", "navigation", "state", "control", "journey"],
        ["1.4"] = ["accessibility", "usability", "a11y", "wcag"],
        ["2.1"] = ["project", "capability", "business", "documentation", "document", "purpose"],
        ["2.2"] = ["story", "stories", "acceptance", "criteria"],
        ["2.3"] = ["data", "validation", "input", "state", "rule"],
        ["2.4"] = ["edge", "boundary", "exception", "ambiguity", "corner"],
        ["3.1"] = ["architecture", "component", "topology", "project", "codebase", "code", "system"],
        ["3.2"] = ["data", "database", "persistence", "entity", "storage", "model"],
        ["3.3"] = ["api", "endpoint", "contract", "schema", "request", "response", "openapi"],
        ["3.4"] = ["security", "authorization", "authentication", "auth", "jwt", "oauth", "tenant"],
        ["3.5"] = ["observability", "logging", "metrics", "trace", "traces", "alert", "monitoring"],
        ["4.1"] = ["backend", "service", "services", "code", "implement", "implementation", "project"],
        ["4.2"] = ["frontend", "client", "ui", "screen", "state", "figma"],
        ["4.3"] = ["infrastructure", "deployment", "environment", "runtime", "cloud", "aws"],
        ["4.4"] = ["refactor", "refactoring", "quality", "fix", "bug", "issue", "code", "error", "failure", "failed", "log"],
        ["4.5"] = ["provider", "connector", "adapter", "external", "integration"],
        ["5.1"] = ["unit", "test", "tests", "fixture", "verify", "fix", "bug", "issue", "error", "failure", "failed", "log"],
        ["5.2"] = ["integration", "test", "provider", "storage", "boundary"],
        ["5.3"] = ["end", "scenario", "journey", "e2e"],
        ["5.4"] = ["regression", "compatibility", "known", "bug", "issue", "fix", "failure", "failed"],
        ["6.1"] = ["manual", "qa", "acceptance", "exploratory"],
        ["6.2"] = ["release", "risk", "readiness"],
        ["6.3"] = ["security", "compliance", "vulnerability", "control"],
        ["6.4"] = ["performance", "reliability", "latency", "load", "resource"],
        ["7.1"] = ["automation", "harness", "runner", "fixture"],
        ["7.2"] = ["ci", "pipeline", "gate", "build"],
        ["7.3"] = ["monitoring", "feedback", "runtime", "learning", "signal"],
        ["7.4"] = ["coverage", "report", "reporting", "dashboard", "document"],
        ["7.5"] = ["skill", "skills", "learning", "teach", "training", "model", "feedback"]
    };

    // ScoreRelevant snapshots only permitted semantic source fields before any calculation.
    internal static SubPhaseBaselineScore[] ScoreRelevant(
        SubPhaseRouterSourceDocument source,
        IReadOnlyList<LogicalSubPhaseEntry> entries,
        ContextSignals contextSignals)
    {
        // Capture the complete scoring boundary once; paths, ids, status, labels, and segments stay unread.
        var documentTitle = source.DocumentTitle;
        var documentType = source.DocumentType;
        var compiledText = source.CompiledText;
        var compiledPredicateIds = (source.CompiledPredicateIds ?? []).ToArray();

        // Freeze the full caller-provided corpus so hybrid rank and raw scoring observe the same entries.
        var entrySnapshot = entries.ToArray();

        // Normalize predicate and text evidence once from the permitted snapshot.
        var inputPredicates = NormalizePredicates(compiledPredicateIds);
        var inputText = BuildPathFreeScoringText(documentTitle, documentType, compiledText, compiledPredicateIds);
        var inputTokens = Tokenize(inputText);
        var promptTokens = Tokenize(compiledText);
        var hybridScores = BuildHybridScores(inputText, inputPredicates, entrySnapshot);

        // Score the full registry corpus before applying the shared relevance floor.
        var scored = entrySnapshot
            .Select(entry => Score(entry, inputPredicates, inputTokens, promptTokens, inputText, hybridScores, contextSignals))
            .Where(candidate => candidate.Score > 0.0)
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Entry.SubPhase.SubPhaseId, StringComparer.Ordinal)
            .ToArray();

        // Keep only materially relevant branches so weak token echoes do not fan out across the lifecycle.
        if (scored.Length > 0)
        {
            var bestScore = scored[0].Score;
            var relevanceFloor = bestScore >= 0.20 ? Math.Max(0.20, bestScore * 0.80) : bestScore;
            scored = scored.Where(candidate => candidate.Score >= relevanceFloor).ToArray();
        }

        return scored;
    }

    // Tokenize text into stable routing tokens for semantic scoring and path-only warnings.
    internal static HashSet<string> Tokenize(string text)
    {
        return (text ?? string.Empty)
            .Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => token.ToLowerInvariant())
            .Where(token => token.Length > 2)
            .ToHashSet(StringComparer.Ordinal);
    }

    // Score one candidate against input predicates and source tokens.
    private static SubPhaseBaselineScore Score(
        LogicalSubPhaseEntry entry,
        HashSet<string> inputPredicates,
        HashSet<string> inputTokens,
        HashSet<string> promptTokens,
        string inputText,
        IReadOnlyDictionary<string, double> hybridScores,
        ContextSignals contextSignals)
    {
        var candidatePredicates = NormalizePredicates(entry.SubPhase.PredicateIds);
        var predicateMatches = candidatePredicates.Where(inputPredicates.Contains).OrderBy(predicate => predicate, StringComparer.Ordinal).ToArray();
        var predicateScore = candidatePredicates.Count == 0 ? 0.0 : (double)predicateMatches.Length / candidatePredicates.Count;
        var exactSubPhaseMention = ContainsDelimitedSubPhaseId(inputText, entry.SubPhase.SubPhaseId) ? 1.0 : 0.0;
        var anchorTerms = SubPhaseAnchorTerms.TryGetValue(entry.SubPhase.SubPhaseId, out var terms) ? terms : [];
        var anchorMatched = anchorTerms.Length == 0 || anchorTerms.Any(term => promptTokens.Contains(term));
        if (!anchorMatched && exactSubPhaseMention == 0.0)
            return new SubPhaseBaselineScore(entry, 0.0, predicateMatches);

        var candidateTokens = Tokenize($"{entry.SubPhase.SubPhaseId} {entry.SubPhase.Name} {entry.SubPhase.Purpose}");
        var tokenMatches = candidateTokens.Count(inputTokens.Contains);
        var tokenScore = candidateTokens.Count == 0 ? 0.0 : (double)tokenMatches / candidateTokens.Count;
        var hybridScore = hybridScores.TryGetValue(entry.SubPhase.SubPhaseId, out var scoreValue) ? scoreValue : 0.0;
        var contextScore = ScoreContext(entry, contextSignals);
        var score = contextSignals.IsEmpty
            ? ((predicateScore * 0.55) + (exactSubPhaseMention * 0.25) + (tokenScore * 0.20) + (hybridScore * 0.02))
            : ((predicateScore * 0.50) + (exactSubPhaseMention * 0.20) + (tokenScore * 0.15) + (hybridScore * 0.05) + (contextScore * 0.10));

        return new SubPhaseBaselineScore(entry, Math.Max(0.0, score), predicateMatches);
    }

    // Join only the captured semantic document fields and deliberately exclude every provenance field.
    private static string BuildPathFreeScoringText(
        string documentTitle,
        string documentType,
        string compiledText,
        string[] compiledPredicateIds)
        => string.Join(" ", documentTitle, documentType, compiledText, string.Join(" ", compiledPredicateIds));

    private static double ScoreContext(LogicalSubPhaseEntry entry, ContextSignals signals)
    {
        if (signals.IsEmpty)
            return 0.0;

        var score = 0.0;
        if (signals.CompletedSubPhases.Contains(entry.SubPhase.SubPhaseId, StringComparer.Ordinal))
            score -= 0.5;

        if (signals.ActiveKnowledgeEdges.Any(edge => ContainsDelimitedSubPhaseId(edge, entry.SubPhase.SubPhaseId)))
            score += 1.0;

        if (signals.PriorDomains.Any(domain => DomainMatchesSubPhase(domain, entry)))
            score += 0.5;

        return Math.Clamp(score, -0.5, 1.0);
    }

    private static bool ContainsDelimitedSubPhaseId(string source, string subPhaseId)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(subPhaseId))
            return false;

        var searchIndex = 0;
        while (searchIndex <= source.Length - subPhaseId.Length)
        {
            var matchIndex = source.IndexOf(subPhaseId, searchIndex, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
                return false;

            var matchEndIndex = matchIndex + subPhaseId.Length;
            var hasStartBoundary = matchIndex == 0 || !IsSubPhaseIdCharacter(source[matchIndex - 1]);
            var hasEndBoundary = matchEndIndex == source.Length || !IsSubPhaseIdCharacter(source[matchEndIndex]);
            if (hasStartBoundary && hasEndBoundary)
                return true;

            searchIndex = matchIndex + 1;
        }

        return false;
    }

    // Baseline mention grammar treats the codebase's observed caller-owned id characters as continuations:
    // letters, digits, dots, underscores, and hyphens. Other punctuation remains a delimiter, including the
    // colon in the canonical "knowledge:<sub-phase-id>" shape.
    private static bool IsSubPhaseIdCharacter(char value)
        => char.IsLetterOrDigit(value) || value is '.' or '_' or '-';

    private static bool DomainMatchesSubPhase(Domain domain, LogicalSubPhaseEntry entry)
    {
        var labels = entry.SubPhase.DomainIds
            .Concat(entry.SubPhase.PredicateIds)
            .Concat([entry.SubPhase.Name, entry.SubPhase.Purpose])
            .Select(label => label.ToUpperInvariant())
            .ToArray();

        string[] needles = domain switch
        {
            Domain.BUG => ["BUG", "FIX", "ERROR", "FAIL", "TEST", "REGRESSION"],
            Domain.REFACTOR => ["REFACTOR", "QUALITY", "IMPLEMENT", "CODE"],
            Domain.PLAN => ["PLAN", "DOCUMENT", "LEARNING", "AUTOMATION", "WORKFLOW"],
            Domain.ARCH => ["ARCHITECTURE", "SECURITY", "DESIGN", "CONTRACT"],
            Domain.CODE_Q => ["DOCUMENT", "QUESTION", "CODE", "ARCHITECTURE"],
            _ => []
        };

        return needles.Any(needle => labels.Any(label => label.Contains(needle, StringComparison.OrdinalIgnoreCase)));
    }

    private static IReadOnlyDictionary<string, double> BuildHybridScores(
        string inputText,
        HashSet<string> inputPredicates,
        IReadOnlyList<LogicalSubPhaseEntry> entries)
    {
        var candidates = entries
            .Select(entry =>
            {
                var candidatePredicates = NormalizePredicates(entry.SubPhase.PredicateIds);
                var predicateMatches = candidatePredicates.Count(inputPredicates.Contains);
                var knnSeed = candidatePredicates.Count == 0 ? 0.0 : (double)predicateMatches / candidatePredicates.Count;
                var candidateText = string.Join(" ",
                    entry.SubPhase.SubPhaseId,
                    entry.SubPhase.Name,
                    entry.SubPhase.Purpose,
                    string.Join(" ", entry.SubPhase.PredicateIds),
                    string.Join(" ", entry.SubPhase.DomainIds));
                return new AgentAlgorithms.HybridRankCandidate(entry.SubPhase.SubPhaseId, candidateText, knnSeed);
            })
            .ToArray();

        return new AgentAlgorithms.HybridRanker()
            .Rank(inputText, candidates)
            .ToDictionary(result => result.Id, result => result.FusedScore, StringComparer.Ordinal);
    }

    // Normalize predicate ids to stable uppercase tokens.
    private static HashSet<string> NormalizePredicates(string[]? predicateIds)
    {
        return (predicateIds ?? [])
            .Select(predicate => predicate.Trim())
            .Where(predicate => !string.IsNullOrWhiteSpace(predicate))
            .Select(predicate => predicate.ToUpperInvariant())
            .ToHashSet(StringComparer.Ordinal);
    }
}
