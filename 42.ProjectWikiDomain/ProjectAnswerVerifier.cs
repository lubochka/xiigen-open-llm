// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

public sealed class ProjectAnswerVerifier
{
    public const string UnitId = "project-answer-verifier-unit";
    public const string ScenarioId = "project-wiki-answer-verification";

    public ProjectWikiVerificationResult Verify(
        ProjectWikiAnswer answer,
        ProjectEvidenceIndex evidenceIndex,
        string expectedProjectId)
    {
        var warnings = new List<string>();
        var invalidEvidenceIds = answer.EvidenceIds
            .Where(id => !evidenceIndex.ContainsEvidenceId(id))
            .ToArray();
        var boundaryPassed = string.IsNullOrWhiteSpace(expectedProjectId) ||
                             string.Equals(answer.ProjectId, expectedProjectId, StringComparison.Ordinal);

        if (!boundaryPassed)
            warnings.Add("project_boundary_mismatch");

        if (invalidEvidenceIds.Length > 0)
            warnings.Add("invalid_evidence_citation");

        if (answer.EvidenceIds.Length == 0 && string.IsNullOrWhiteSpace(answer.MissingEvidence))
            warnings.Add("missing_evidence_not_declared");

        var advancedAnswer = IsAdvancedCrossStackAnswer(answer.Answer);
        if (advancedAnswer && !HasAllAdvancedSections(answer.Answer))
            warnings.Add("advanced_cross_stack_sections_incomplete");

        if (advancedAnswer && answer.Answer.Contains("missing from retrieved evidence", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(answer.MissingEvidence) &&
            !answer.Answer.Contains("Missing evidence:", StringComparison.OrdinalIgnoreCase))
            warnings.Add("advanced_missing_evidence_not_declared");

        var citationAccuracy = answer.EvidenceIds.Length == 0
            ? (string.IsNullOrWhiteSpace(answer.MissingEvidence) ? 0.0 : 1.0)
            : (answer.EvidenceIds.Length - invalidEvidenceIds.Length) / (double)answer.EvidenceIds.Length;

        var passed = boundaryPassed &&
                     invalidEvidenceIds.Length == 0 &&
                     (answer.EvidenceIds.Length > 0 || !string.IsNullOrWhiteSpace(answer.MissingEvidence)) &&
                     (!advancedAnswer || HasAllAdvancedSections(answer.Answer));

        return new ProjectWikiVerificationResult(
            passed,
            invalidEvidenceIds,
            warnings.ToArray(),
            citationAccuracy,
            boundaryPassed);
    }

    private static bool IsAdvancedCrossStackAnswer(string answerText)
        => answerText.Contains("Cross-stack answer sections:", StringComparison.OrdinalIgnoreCase) ||
           answerText.Contains("Client path:", StringComparison.OrdinalIgnoreCase) ||
           answerText.Contains("Database path:", StringComparison.OrdinalIgnoreCase);

    private static bool HasAllAdvancedSections(string answerText)
        => answerText.Contains("Client path:", StringComparison.OrdinalIgnoreCase) &&
           answerText.Contains("Server path:", StringComparison.OrdinalIgnoreCase) &&
           answerText.Contains("Database path:", StringComparison.OrdinalIgnoreCase) &&
           answerText.Contains("Response and UI consumers:", StringComparison.OrdinalIgnoreCase) &&
           answerText.Contains("Risks and checks:", StringComparison.OrdinalIgnoreCase) &&
           answerText.Contains("Missing evidence:", StringComparison.OrdinalIgnoreCase);
}
