// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class ProjectAnswerVerifierDpoGenerator
{
    public Task<OperationResult<ProjectAnswerVerifierDpoBatch>> BuildBatchAsync(
        string batchId = "project-answer-verifier-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<ProjectAnswerVerifierDpoBatch>.Fail("Cancelled.", "CANCELLED"));

        var signal = batchId.Contains("signal", StringComparison.OrdinalIgnoreCase);
        var focused = batchId.Contains("focused", StringComparison.OrdinalIgnoreCase);
        var wrongProjectBoundary = batchId.Contains("wrong-project", StringComparison.OrdinalIgnoreCase);
        var validAnswerBoundary = batchId.Contains("valid-answer", StringComparison.OrdinalIgnoreCase);
        var seeds = wrongProjectBoundary
            ? BuildWrongProjectBoundarySeeds(heldOut)
            : validAnswerBoundary
                ? BuildValidAnswerBoundarySeeds(heldOut)
                : focused
                    ? BuildFocusedSeeds(heldOut)
                    : BuildSeeds(heldOut);
        var pairs = seeds.Select((seed, index) => new ProjectAnswerVerifierDpoPair(
            $"{batchId}-pair-{index + 1:000}",
            seed.Answer,
            seed.EvidenceRecords,
            seed.ExpectedProjectId,
            seed.ExpectedPassed,
            seed.ExpectedWarnings,
            $"verify answer citations for {seed.Answer.ProjectId}",
            BuildChosen(seed, signal),
            BuildRejected(seed, signal),
            seed.RejectedType)).ToArray();
        var wrongAnnotationRate = pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Length;
        var warnings = new List<string>();
        if (pairs.Length != 50)
            warnings.Add("batch_row_count_not_50");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");

        return Task.FromResult(OperationResult<ProjectAnswerVerifierDpoBatch>.Ok(new ProjectAnswerVerifierDpoBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            pairs,
            warnings.ToArray())));
    }

    private static string BuildChosen(VerifierSeed seed, bool signal)
    {
        if (!signal)
            return $"passed: {seed.ExpectedPassed}\nwarnings: {string.Join(", ", seed.ExpectedWarnings)}";

        var warningText = seed.ExpectedWarnings.Length == 0
            ? "WARNINGS_NONE"
            : string.Join(" ", seed.ExpectedWarnings.Select(item => $"WARNING_{TokenSafe(item)}"));
        var citationState = seed.ExpectedWarnings.Contains("invalid_evidence_citation", StringComparer.Ordinal)
            ? "CITATION_INVALID_REJECTED INVALID_EVIDENCE_ID_DETECTED"
            : "CITATION_STATE_ACCEPTABLE";
        var boundaryState = seed.ExpectedWarnings.Contains("project_boundary_mismatch", StringComparer.Ordinal)
            ? "PROJECT_BOUNDARY_MISMATCH_DETECTED PROJECT_BOUNDARY_REJECTED WRONG_PROJECT_EVIDENCE_REJECTED ANSWER_PROJECT_DIFFERS_FROM_EXPECTED_PROJECT CROSS_PROJECT_CITATION_DENIED PASS_FALSE_REQUIRED_FOR_WRONG_PROJECT"
            : "PROJECT_BOUNDARY_OK";
        var abstentionState = seed.Answer.EvidenceIds.Length == 0 && !string.IsNullOrWhiteSpace(seed.Answer.MissingEvidence)
            ? $"PASS_TRUE_DECLARED_ABSTENTION MISSING_EVIDENCE_ABSTENTION_ACCEPTED DECLARED_MISSING_EVIDENCE_REASON_PRESENT MISSING_EVIDENCE_REASON_{TokenSafe(seed.Answer.MissingEvidence)} ABSTAIN_WITHOUT_CITATION_ALLOWED"
            : seed.Answer.EvidenceIds.Length == 0
                ? "MISSING_EVIDENCE_UNDECLARED_REJECTED DECLARED_MISSING_EVIDENCE_REASON_ABSENT UNCITED_ANSWER_MUST_FAIL"
                : "EVIDENCE_PRESENT DECLARED_MISSING_EVIDENCE_REASON_NOT_NEEDED";
        var correctionState = seed.RejectedType switch
        {
            "rejects_valid_answer" => "CORRECTS_FALSE_REJECTION VALID_LOCAL_EVIDENCE_ACCEPTED USEFUL_ANSWER_ALLOWED CITED_ANSWER_MUST_PASS",
            "accepts_wrong_project" => "CORRECTS_WRONG_PROJECT_ACCEPTANCE WRONG_PROJECT_EVIDENCE_DENIED CROSS_PROJECT_CITATION_BLOCKED ANSWER_PROJECT_MUST_MATCH_EXPECTED_PROJECT",
            "accepts_invalid_citation" => "CORRECTS_INVALID_CITATION_ACCEPTANCE INVALID_CITATION_DENIED CITATION_MUST_EXIST_IN_INDEX",
            "accepts_missing_evidence" => "CORRECTS_UNDECLARED_MISSING_EVIDENCE_ACCEPTANCE UNCITED_NON_ABSTENTION_MUST_FAIL",
            _ => "CORRECTS_INCOMPLETE_VERIFIER_RESULT COMPLETE_WARNING_SET_PRESENT"
        };
        return string.Join(" ", [
            "GOOD_PROJECT_ANSWER_VERIFIER_RESULT",
            correctionState,
            $"EXPECTED_PROJECT_{TokenSafe(seed.ExpectedProjectId)}",
            seed.ExpectedPassed ? "PASS_TRUE" : "PASS_FALSE",
            warningText,
            citationState,
            boundaryState,
            abstentionState,
            "RESULT_MATCHES_EVIDENCE_INDEX"
        ]);
    }

    private static string BuildRejected(VerifierSeed seed, bool signal)
    {
        if (signal)
            return BuildSignalRejected(seed);
        return BuildRejected(seed.RejectedType, seed.ExpectedPassed, seed.ExpectedWarnings);
    }

    private static string BuildRejected(string rejectedType, bool passed, string[] warnings)
        => rejectedType switch
        {
            "accepts_invalid_citation" =>
                "passed: true\nwarnings: none\n[WRONG: accepts_invalid_citation allows an answer to cite evidence that is not present in the project wiki index.]",
            "accepts_wrong_project" =>
                "passed: true\nwarnings: none\n[WRONG: accepts_wrong_project ignores project boundary and can answer from another repository.]",
            "rejects_valid_answer" =>
                $"passed: {!passed}\nwarnings: false_failure\n[WRONG: rejects_valid_answer blocks a correctly cited project-local answer and prevents useful local wiki responses.]",
            "accepts_missing_evidence" =>
                "passed: true\nwarnings: none\n[WRONG: accepts_missing_evidence treats an uncited answer as grounded even though no missing-evidence abstention was declared.]",
            _ =>
                $"passed: {passed}\nwarnings: {string.Join(", ", warnings.Take(1))}\n[WRONG: incomplete_verifier_result omits required warning details needed for WikiQueryTrace remediation.]"
        };

    private static string BuildSignalRejected(VerifierSeed seed)
        => seed.RejectedType switch
        {
            "accepts_invalid_citation" =>
                "BAD_PROJECT_ANSWER_VERIFIER_RESULT PASS_TRUE WARNINGS_NONE INVALID_CITATION_ACCEPTED RESULT_CONTRADICTS_EVIDENCE_INDEX\n[WRONG: accepts_invalid_citation allows an answer to cite evidence that is not present in the project wiki index.]",
            "accepts_wrong_project" =>
                $"BAD_PROJECT_ANSWER_VERIFIER_RESULT PASS_TRUE WARNINGS_NONE EXPECTED_PROJECT_{TokenSafe(seed.ExpectedProjectId)} ANSWER_PROJECT_{TokenSafe(seed.Answer.ProjectId)} PROJECT_BOUNDARY_MISMATCH_IGNORED\n[WRONG: accepts_wrong_project ignores project boundary and can answer from another repository.]",
            "rejects_valid_answer" =>
                "BAD_PROJECT_ANSWER_VERIFIER_RESULT PASS_FALSE WARNING_FALSE_FAILURE VALID_LOCAL_EVIDENCE_REJECTED USEFUL_ANSWER_BLOCKED\n[WRONG: rejects_valid_answer blocks a correctly cited project-local answer and prevents useful local wiki responses.]",
            "accepts_missing_evidence" =>
                "BAD_PROJECT_ANSWER_VERIFIER_RESULT PASS_TRUE WARNINGS_NONE UNCITED_ANSWER_ACCEPTED MISSING_EVIDENCE_UNDECLARED\n[WRONG: accepts_missing_evidence treats an uncited answer as grounded even though no missing-evidence abstention was declared.]",
            _ =>
                "BAD_PROJECT_ANSWER_VERIFIER_RESULT PASS_FALSE WARNING_FALSE_MISSING_EVIDENCE_FAILURE MISSING_EVIDENCE_ABSTENTION_REJECTED WARNING_SET_INCOMPLETE TRACE_REMEDIATION_UNUSABLE\n[WRONG: incomplete_verifier_result omits required warning details needed for WikiQueryTrace remediation.]"
        };

    private static VerifierSeed[] BuildSeeds(bool heldOut)
    {
        var entities = heldOut
            ? new[] { "Invoice", "Profile", "Message", "Shipment", "Ticket", "Payment", "Subscription", "Report", "Alert", "Session" }
            : new[] { "Campaign", "Order", "Client", "DietPlan", "MediaAsset", "Task", "Game", "Article", "Booking", "Survey" };
        var rows = new List<VerifierSeed>(50);
        for (var repeat = 0; repeat < 5; repeat++)
        {
            foreach (var entity in entities)
            {
                var evidence = Record("project-a", $"ev:{entity}:{repeat}", $"{entity}Service.Save{entity}");
                var kind = repeat % 5;
                rows.Add(kind switch
                {
                    0 => Seed(
                        Answer("project-a", [evidence.EvidenceId], ""),
                        [evidence],
                        "project-a",
                        true,
                        [],
                        "rejects_valid_answer"),
                    1 => Seed(
                        Answer("project-a", ["missing:evidence"], ""),
                        [evidence],
                        "project-a",
                        false,
                        ["invalid_evidence_citation"],
                        "accepts_invalid_citation"),
                    2 => Seed(
                        Answer("project-b", [evidence.EvidenceId], ""),
                        [evidence],
                        "project-a",
                        false,
                        ["project_boundary_mismatch"],
                        "accepts_wrong_project"),
                    3 => Seed(
                        Answer("project-a", [], ""),
                        [],
                        "project-a",
                        false,
                        ["missing_evidence_not_declared"],
                        "accepts_missing_evidence"),
                    _ => Seed(
                        Answer("project-a", [], "no_matching_project_evidence"),
                        [],
                        "project-a",
                        true,
                        [],
                        "incomplete_verifier_result")
                });
            }
        }

        return rows.Take(50).ToArray();
    }

    private static VerifierSeed[] BuildFocusedSeeds(bool heldOut)
    {
        var entities = heldOut
            ? new[] { "Invoice", "Profile", "Message", "Shipment", "Ticket", "Payment", "Subscription", "Report", "Alert", "Session" }
            : new[] { "Campaign", "Order", "Client", "DietPlan", "MediaAsset", "Task", "Game", "Article", "Booking", "Survey" };
        var rows = new List<VerifierSeed>(50);
        foreach (var repeat in new[] { 0, 1 })
        {
            foreach (var entity in entities)
            {
                var evidence = Record("project-a", $"ev:{entity}:boundary:{repeat}", $"{entity}Service.Save{entity}");
                rows.Add(Seed(
                    Answer("project-b", [evidence.EvidenceId], ""),
                    [evidence],
                    "project-a",
                    false,
                    ["project_boundary_mismatch"],
                    "accepts_wrong_project"));
            }
        }

        foreach (var repeat in new[] { 0, 1 })
        {
            foreach (var entity in entities)
            {
                var evidence = Record("project-a", $"ev:{entity}:abstain:{repeat}", $"{entity}Service.Save{entity}");
                rows.Add(Seed(
                    Answer("project-a", [], "no_matching_project_evidence"),
                    [],
                    "project-a",
                    true,
                    [],
                    "incomplete_verifier_result"));
            }
        }

        foreach (var entity in entities.Take(5))
        {
            var evidence = Record("project-a", $"ev:{entity}:invalid", $"{entity}Service.Save{entity}");
            rows.Add(Seed(
                Answer("project-a", ["missing:evidence"], ""),
                [evidence],
                "project-a",
                false,
                ["invalid_evidence_citation"],
                "accepts_invalid_citation"));
        }

        foreach (var entity in entities.Take(5))
        {
            var evidence = Record("project-a", $"ev:{entity}:valid", $"{entity}Service.Save{entity}");
            rows.Add(Seed(
                Answer("project-a", [evidence.EvidenceId], ""),
                [evidence],
                "project-a",
                true,
                [],
                "rejects_valid_answer"));
        }

        return rows.Take(50).ToArray();
    }

    private static VerifierSeed[] BuildWrongProjectBoundarySeeds(bool heldOut)
    {
        var entities = heldOut
            ? new[] { "Invoice", "Profile", "Message", "Shipment", "Ticket", "Payment", "Subscription", "Report", "Alert", "Session" }
            : new[] { "Campaign", "Order", "Client", "DietPlan", "MediaAsset", "Task", "Game", "Article", "Booking", "Survey" };
        var rows = new List<VerifierSeed>(50);
        for (var repeat = 0; repeat < 5; repeat++)
        {
            foreach (var entity in entities)
            {
                var projectId = repeat % 2 == 0 ? "project-a" : "tenant-alpha";
                var foreignProjectId = repeat % 2 == 0 ? "project-b" : "tenant-beta";
                var evidence = Record(projectId, $"ev:{entity}:boundary:{repeat}", $"{entity}Service.Save{entity}");
                if (repeat % 2 == 0)
                {
                    rows.Add(Seed(
                        Answer(foreignProjectId, [evidence.EvidenceId], ""),
                        [evidence],
                        projectId,
                        false,
                        ["project_boundary_mismatch"],
                        "accepts_wrong_project"));
                }
                else
                {
                    rows.Add(Seed(
                        Answer(projectId, [evidence.EvidenceId], ""),
                        [evidence],
                        projectId,
                        true,
                        [],
                        "rejects_valid_answer"));
                }
            }
        }

        return rows.Take(50).ToArray();
    }

    private static VerifierSeed[] BuildValidAnswerBoundarySeeds(bool heldOut)
    {
        var entities = heldOut
            ? new[] { "Invoice", "Profile", "Message", "Shipment", "Ticket", "Payment", "Subscription", "Report", "Alert", "Session" }
            : new[] { "Campaign", "Order", "Client", "DietPlan", "MediaAsset", "Task", "Game", "Article", "Booking", "Survey" };
        var rows = new List<VerifierSeed>(50);
        for (var repeat = 0; repeat < 5; repeat++)
        {
            foreach (var entity in entities)
            {
                var evidence = Record("project-a", $"ev:{entity}:valid:{repeat}", $"{entity}Service.Save{entity}");
                if (repeat % 2 == 0)
                {
                    rows.Add(Seed(
                        Answer("project-a", [evidence.EvidenceId], ""),
                        [evidence],
                        "project-a",
                        true,
                        [],
                        "rejects_valid_answer"));
                }
                else
                {
                    rows.Add(Seed(
                        Answer("project-a", ["missing:evidence"], ""),
                        [evidence],
                        "project-a",
                        false,
                        ["invalid_evidence_citation"],
                        "accepts_invalid_citation"));
                }
            }
        }

        return rows.Take(50).ToArray();
    }

    private static ProjectWikiAnswer Answer(string projectId, string[] evidenceIds, string missingEvidence)
        => new(projectId, "doc-v1", "answer text", evidenceIds, [], evidenceIds.Length > 0 ? 0.82 : 0.35, missingEvidence, ProjectWikiRetrievalMode.Mode3LexicalEvidence, []);

    private static ProjectWikiEvidenceRecord Record(string projectId, string evidenceId, string symbol)
        => new()
        {
            EvidenceId = evidenceId,
            ProjectId = projectId,
            DocumentationVersion = "doc-v1",
            SymbolId = symbol,
            RelationSymbolId = symbol,
            DocPath = $"documentation/{symbol}.md",
            SourcePath = $"Services/{symbol}.cs",
            SourceLine = 10,
            DocumentationLevel = "D3",
            TargetFramework = "net8.0",
            CSharpLanguageVersion = "CSharp12",
            Meaning = $"{symbol} project-local evidence.",
            Parameters = [],
            Returns = "void",
            CalledFunctions = [],
            CalledBy = [],
            KnownCallees = []
        };

    private static VerifierSeed Seed(
        ProjectWikiAnswer answer,
        ProjectWikiEvidenceRecord[] evidenceRecords,
        string expectedProjectId,
        bool expectedPassed,
        string[] expectedWarnings,
        string rejectedType)
        => new(answer, evidenceRecords, expectedProjectId, expectedPassed, expectedWarnings, rejectedType);

    private sealed record VerifierSeed(
        ProjectWikiAnswer Answer,
        ProjectWikiEvidenceRecord[] EvidenceRecords,
        string ExpectedProjectId,
        bool ExpectedPassed,
        string[] ExpectedWarnings,
        string RejectedType);

    private static string TokenSafe(string value)
    {
        var chars = value.Select(ch => char.IsLetterOrDigit(ch) ? char.ToUpperInvariant(ch) : '_').ToArray();
        return new string(chars).Trim('_');
    }
}
