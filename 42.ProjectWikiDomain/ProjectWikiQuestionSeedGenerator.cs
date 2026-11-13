// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

public sealed class ProjectWikiQuestionSeedGenerator
{
    public IReadOnlyList<ProjectWikiQuestionSeed> Generate(
        ProjectEvidenceIndex evidenceIndex,
        int maxRecords = 100)
    {
        if (evidenceIndex is null)
            return [];

        var duplicateSymbols = evidenceIndex.Records
            .GroupBy(DisplaySymbol, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return evidenceIndex.Records
            .Take(Math.Max(0, maxRecords))
            .SelectMany(evidence => BuildQuestions(evidence, duplicateSymbols))
            .ToArray();
    }

    private static ProjectWikiQuestionSeed[] BuildQuestions(
        ProjectWikiEvidenceRecord evidence,
        IReadOnlySet<string> duplicateSymbols)
    {
        var symbol = DisplayQuestionSymbol(evidence, duplicateSymbols);
        var projectId = DisplayProjectId(evidence);

        return
        [
            Seed("meaning", $"What does {symbol} do in project {projectId}?", evidence),
            Seed("location", $"Where is {symbol} located in the codebase?", evidence),
            Seed("parameters", $"What parameters or inputs does {symbol} accept?", evidence),
            Seed("returns", $"What does {symbol} return to its caller?", evidence),
            Seed("callers", $"Who calls {symbol}, or is there no recorded caller?", evidence),
            Seed("callees", $"Which lower-level functions does {symbol} call or delegate to?", evidence),
            Seed("workflow", $"How does {symbol} participate in the surrounding project workflow?", evidence),
            Seed("why", $"Why does the documentation for {symbol} need caller and callee evidence?", evidence),
            Seed("multi_hop", $"Explain the caller and callee chain around {symbol}.", evidence)
        ];
    }

    private static ProjectWikiQuestionSeed Seed(
        string type,
        string question,
        ProjectWikiEvidenceRecord evidence)
        => new(
            type,
            question,
            evidence.EvidenceId,
            DisplayProjectId(evidence),
            evidence.DocumentationLevel,
            evidence.SourcePath);

    private static string DisplayQuestionSymbol(
        ProjectWikiEvidenceRecord evidence,
        IReadOnlySet<string> duplicateSymbols)
    {
        var symbol = DisplaySymbol(evidence);
        if (!duplicateSymbols.Contains(symbol))
            return symbol;

        return $"{symbol} at {evidence.SourcePath}:{evidence.SourceLine}";
    }

    private static string DisplaySymbol(ProjectWikiEvidenceRecord evidence)
    {
        if (!string.IsNullOrWhiteSpace(evidence.SymbolId))
            return evidence.SymbolId;

        if (!string.IsNullOrWhiteSpace(evidence.RelationSymbolId))
            return evidence.RelationSymbolId;

        var parts = evidence.EvidenceId.Split("::", StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[^1] : evidence.EvidenceId;
    }

    private static string DisplayProjectId(ProjectWikiEvidenceRecord evidence)
    {
        if (!string.IsNullOrWhiteSpace(evidence.ProjectId))
            return evidence.ProjectId;

        var parts = evidence.EvidenceId.Split("::", StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "project-local-wiki";
    }
}
