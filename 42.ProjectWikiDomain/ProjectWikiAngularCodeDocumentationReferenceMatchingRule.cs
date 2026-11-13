// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;
using ContinuousLearning;

/// <summary>
/// ProjectWiki Angular documentation adapter for frozen-reference calibration.
/// It is intentionally separate from the C# adapter: both subjects live under
/// the generic project-local-wiki domain, so the typed payload and corpus
/// subject guard are the authority that prevents cross-stack evidence mixing.
/// </summary>
public static class ProjectWikiAngularCodeDocumentationReferenceMatchingRule
{
    public const string DomainTag = ProjectWikiCodeDocumentationReferenceMatchingRule.DomainTag;
    public const string SubjectId = "code-documentation-angular";
    public const string RuleId = "project-wiki-angular-code-documentation-exact-evidence-ids";
    public const string RuleVersion =
        "v1.schema-xiigen.project-wiki-code-documentation-reference-evidence.v2.subject-code-documentation-angular.policy-exact-set-ordinal";
    public const string RuleIdentityMismatchErrorCode =
        "PROJECT_WIKI_ANGULAR_REFERENCE_MATCHING_RULE_IDENTITY_MISMATCH";
    public const string CorpusSubjectMismatchErrorCode =
        "PROJECT_WIKI_ANGULAR_REFERENCE_MATCHING_CORPUS_SUBJECT_MISMATCH";

    public static OperationResult<ReferenceMatchingRuleIdentity> CreateIdentity()
        => ReferenceMatchingRuleIdentity.Create(
            DomainTag,
            RuleId,
            RuleVersion,
            "ProjectWiki Angular documentation frozen-reference instrument. It compares exact ordinal evidence-id sets from schema " +
            ProjectWikiReferenceEvidencePayload.Schema +
            " for subject " +
            SubjectId +
            ". Answer prose may differ; duplicate, malformed, or cross-subject payloads fail closed. This is not training, not DPO, and not a paid-reference corpus claim.");

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
            // File extensions and id prefixes are only evidence labels. The
            // subject must come from the typed payload, or identical strings in
            // C#/SQL fixtures could pass an Angular calibration by accident.
            return false;
        }

        var candidateIds = candidate.Value.EvidenceIds;
        var referenceIds = reference.Value.EvidenceIds;
        if (candidateIds.Count != referenceIds.Count)
            return false;

        // The reference instrument validates grounding, not prose copying.
        // Exact ordinal id equality makes added/missing evidence visible while
        // still allowing the natural-language answer text to change.
        return candidateIds.SequenceEqual(referenceIds, StringComparer.Ordinal);
    }

    public static OperationResult<ReferenceMatchingRuleCalibrationResult>
        EvaluateCalibration(ReferenceMatchingCalibrationInput input)
    {
        if (input is null)
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "ProjectWiki Angular reference matching calibration input is required.",
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
                "Calibration input rule identity does not match the ProjectWiki Angular exact evidence-id policy.",
                RuleIdentityMismatchErrorCode);
        }

        if (!StringComparer.Ordinal.Equals(input.Corpus.SubjectId, SubjectId))
        {
            // The generic input has already checked the domain. ProjectWiki
            // needs this subject check before numbers so a C# or SQL corpus
            // cannot be scored through the Angular adapter.
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "Calibration input frozen corpus subject does not match the ProjectWiki Angular exact evidence-id policy.",
                CorpusSubjectMismatchErrorCode);
        }

        return ReferenceMatchingRuleCalibrationEvaluator.Evaluate(
            input,
            CreatePredicate());
    }

    private static bool HasExpectedSubject(ProjectWikiReferenceEvidencePayload payload)
        => StringComparer.Ordinal.Equals(payload.SubjectId, SubjectId);
}
