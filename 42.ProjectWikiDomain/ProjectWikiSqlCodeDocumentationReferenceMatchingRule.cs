// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;
using ContinuousLearning;

/// <summary>
/// ProjectWiki SQL documentation adapter for frozen-reference calibration.
/// SQL shares the project-local-wiki domain with C# and Angular, so the typed
/// payload subject and corpus subject guard are the authority that keeps equal
/// evidence-id strings from crossing language boundaries.
/// </summary>
public static class ProjectWikiSqlCodeDocumentationReferenceMatchingRule
{
    public const string DomainTag = ProjectWikiCodeDocumentationReferenceMatchingRule.DomainTag;
    public const string SubjectId = "code-documentation-sql";
    public const string RuleId = "project-wiki-sql-code-documentation-exact-evidence-ids";
    public const string RuleVersion =
        "v1.schema-xiigen.project-wiki-code-documentation-reference-evidence.v2.subject-code-documentation-sql.policy-exact-set-ordinal";
    public const string RuleIdentityMismatchErrorCode =
        "PROJECT_WIKI_SQL_REFERENCE_MATCHING_RULE_IDENTITY_MISMATCH";
    public const string CorpusSubjectMismatchErrorCode =
        "PROJECT_WIKI_SQL_REFERENCE_MATCHING_CORPUS_SUBJECT_MISMATCH";

    public static OperationResult<ReferenceMatchingRuleIdentity> CreateIdentity()
        => ReferenceMatchingRuleIdentity.Create(
            DomainTag,
            RuleId,
            RuleVersion,
            "ProjectWiki SQL documentation frozen-reference instrument. It compares exact ordinal evidence-id sets from schema " +
            ProjectWikiReferenceEvidencePayload.Schema +
            " for subject " +
            SubjectId +
            ". Answer prose may differ; duplicate, malformed, or cross-subject payloads fail closed. File extensions, procedure labels, and prose are not SQL authority. This is not training, not DPO, and not a paid-reference corpus claim.");

    public static ReferenceMatchingRulePredicate CreatePredicate()
        => IsMatch;

    public static bool IsMatch(
        string candidateAnswer,
        string referenceAnswer)
    {
        var candidate = ProjectWikiReferenceEvidencePayload.Parse(candidateAnswer);
        var reference = ProjectWikiReferenceEvidencePayload.Parse(referenceAnswer);
        if (!candidate.IsSuccess ||
            candidate.Value is null ||
            !reference.IsSuccess ||
            reference.Value is null)
        {
            return false;
        }

        if (!HasExpectedSubject(candidate.Value) ||
            !HasExpectedSubject(reference.Value))
        {
            // SQL is not inferred from ".sql", "procedure:", or prose. Only the
            // typed subject may choose this adapter, otherwise identical C# or
            // Angular evidence strings could pass through the shared domain.
            return false;
        }

        var candidateIds = candidate.Value.EvidenceIds;
        var referenceIds = reference.Value.EvidenceIds;
        if (candidateIds.Count != referenceIds.Count)
            return false;

        // The instrument measures exact grounding, not text similarity. Natural
        // language wording can change; added or missing evidence ids cannot.
        return candidateIds.SequenceEqual(referenceIds, StringComparer.Ordinal);
    }

    public static OperationResult<ReferenceMatchingRuleCalibrationResult>
        EvaluateCalibration(ReferenceMatchingCalibrationInput input)
    {
        if (input is null)
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "ProjectWiki SQL reference matching calibration input is required.",
                "PROJECT_WIKI_REFERENCE_MATCHING_CALIBRATION_INPUT_REQUIRED");
        }

        var expectedIdentity = CreateIdentity();
        if (!expectedIdentity.IsSuccess || expectedIdentity.Value is null)
            return expectedIdentity.MapError<ReferenceMatchingRuleCalibrationResult>();

        if (!StringComparer.Ordinal.Equals(
                input.RuleIdentity.IdentitySha256,
                expectedIdentity.Value.IdentitySha256))
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "Calibration input rule identity does not match the ProjectWiki SQL exact evidence-id policy.",
                RuleIdentityMismatchErrorCode);
        }

        if (!StringComparer.Ordinal.Equals(input.Corpus.SubjectId, SubjectId))
        {
            // The generic calibration input already binds DomainTag. ProjectWiki
            // still needs this subject check before numbers because C#, Angular,
            // and SQL all live under project-local-wiki.
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "Calibration input frozen corpus subject does not match the ProjectWiki SQL exact evidence-id policy.",
                CorpusSubjectMismatchErrorCode);
        }

        return ReferenceMatchingRuleCalibrationEvaluator.Evaluate(
            input,
            CreatePredicate());
    }

    private static bool HasExpectedSubject(ProjectWikiReferenceEvidencePayload payload)
        => StringComparer.Ordinal.Equals(payload.SubjectId, SubjectId);
}
