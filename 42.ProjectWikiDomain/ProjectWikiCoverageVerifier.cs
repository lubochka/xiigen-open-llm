// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class ProjectWikiCoverageVerifier
{
    private static readonly string[] RequiredQuestionTypes =
    [
        "meaning",
        "location",
        "parameters",
        "returns",
        "callers",
        "callees",
        "workflow",
        "why",
        "multi_hop"
    ];

    public async Task<OperationResult<ProjectWikiCoverageReport>> VerifyAsync(
        string projectId,
        string wikiDomainRoot,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiCoverageReport>.Fail("Cancelled.", "CANCELLED");

        var index = ProjectEvidenceIndex.LoadFromWikiDomain(wikiDomainRoot);
        if (!index.IsSuccess)
            return index.MapError<ProjectWikiCoverageReport>();

        var questions = new ProjectWikiQuestionSeedGenerator()
            .Generate(index.Value!, maxRecords: int.MaxValue)
            .ToArray();
        var coveredTypes = questions
            .Select(question => question.QuestionType)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(type => type, StringComparer.Ordinal)
            .ToArray();
        var missingTypes = RequiredQuestionTypes
            .Where(type => !coveredTypes.Contains(type, StringComparer.Ordinal))
            .ToArray();
        var orchestrator = ProjectWikiDomainServiceCollectionExtensions.CreateDefault(projectId, wikiDomainRoot);
        if (!orchestrator.IsSuccess)
            return orchestrator.MapError<ProjectWikiCoverageReport>();

        var answered = 0;
        var exact = 0;
        var failures = new List<string>();
        foreach (var question in questions)
        {
            var answer = await orchestrator.Value!.AskAsync(new ProjectWikiQuestionRequest(
                projectId,
                question.Question,
                wikiDomainRoot,
                "",
                TopK: 5,
                AllowSemantic: false), ct);

            if (!answer.IsSuccess)
            {
                failures.Add($"{question.ExpectedEvidenceId}:answer_failed:{answer.ErrorCode}");
                continue;
            }

            answered++;
            if (answer.Value!.EvidenceIds.Contains(question.ExpectedEvidenceId, StringComparer.Ordinal))
                exact++;
            else
                failures.Add($"{question.ExpectedEvidenceId}:expected_evidence_missing_for:{question.QuestionType}");
        }

        var accuracy = questions.Length == 0 ? 0 : exact / (double)questions.Length;
        if (index.Value!.Records.Count == 0)
            failures.Add("no_evidence_records");
        if (questions.Length < index.Value.Records.Count * RequiredQuestionTypes.Length)
            failures.Add("not_all_evidence_records_have_required_question_types");
        failures.AddRange(missingTypes.Select(type => $"missing_question_type:{type}"));

        var passed = index.Value.Records.Count > 0 &&
                     questions.Length >= index.Value.Records.Count * RequiredQuestionTypes.Length &&
                     answered == questions.Length &&
                     exact == questions.Length &&
                     missingTypes.Length == 0;

        return OperationResult<ProjectWikiCoverageReport>.Ok(new ProjectWikiCoverageReport(
            projectId,
            wikiDomainRoot,
            index.Value.Records.Count,
            questions.Length,
            answered,
            exact,
            accuracy,
            coveredTypes,
            missingTypes,
            passed,
            failures.ToArray()));
    }

    public async Task<OperationResult<ProjectWikiCoverageReport>> VerifyGraphAsync(
        ProjectDocumentationKnowledgeGraph graph,
        string wikiDomainRoot,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiCoverageReport>.Fail("Cancelled.", "CANCELLED");

        if (graph is null)
            return OperationResult<ProjectWikiCoverageReport>.Fail("Project documentation graph is required.", "GRAPH_REQUIRED");

        var index = ProjectEvidenceIndex.CreateFromRecords(graph.EvidenceRecords);
        if (!index.IsSuccess)
            return index.MapError<ProjectWikiCoverageReport>();

        var questions = graph.QuestionSeeds.Count > 0
            ? graph.QuestionSeeds.ToArray()
            : new ProjectWikiQuestionSeedGenerator()
                .Generate(index.Value!, maxRecords: int.MaxValue)
                .ToArray();
        var coveredTypes = questions
            .Select(question => question.QuestionType)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(type => type, StringComparer.Ordinal)
            .ToArray();
        var missingTypes = RequiredQuestionTypes
            .Where(type => !coveredTypes.Contains(type, StringComparer.Ordinal))
            .ToArray();
        var orchestrator = ProjectWikiDomainServiceCollectionExtensions.CreateFromGraph(graph, wikiDomainRoot);
        if (!orchestrator.IsSuccess)
            return orchestrator.MapError<ProjectWikiCoverageReport>();

        var answered = 0;
        var exact = 0;
        var failures = new List<string>();
        foreach (var question in questions)
        {
            var answer = await orchestrator.Value!.AskAsync(new ProjectWikiQuestionRequest(
                graph.ProjectId,
                question.Question,
                wikiDomainRoot,
                graph.DocumentationVersion,
                TopK: 5,
                AllowSemantic: false), ct);

            if (!answer.IsSuccess)
            {
                failures.Add($"{question.ExpectedEvidenceId}:answer_failed:{answer.ErrorCode}");
                continue;
            }

            answered++;
            if (answer.Value!.EvidenceIds.Contains(question.ExpectedEvidenceId, StringComparer.Ordinal))
                exact++;
            else
                failures.Add($"{question.ExpectedEvidenceId}:expected_evidence_missing_for:{question.QuestionType}");
        }

        var accuracy = questions.Length == 0 ? 0 : exact / (double)questions.Length;
        if (index.Value!.Records.Count == 0)
            failures.Add("no_evidence_records");
        if (questions.Length < index.Value.Records.Count * RequiredQuestionTypes.Length)
            failures.Add("not_all_evidence_records_have_required_question_types");
        failures.AddRange(missingTypes.Select(type => $"missing_question_type:{type}"));

        var passed = index.Value.Records.Count > 0 &&
                     questions.Length >= index.Value.Records.Count * RequiredQuestionTypes.Length &&
                     answered == questions.Length &&
                     exact == questions.Length &&
                     missingTypes.Length == 0;

        return OperationResult<ProjectWikiCoverageReport>.Ok(new ProjectWikiCoverageReport(
            graph.ProjectId,
            wikiDomainRoot,
            index.Value.Records.Count,
            questions.Length,
            answered,
            exact,
            accuracy,
            coveredTypes,
            missingTypes,
            passed,
            failures.ToArray()));
    }
}
