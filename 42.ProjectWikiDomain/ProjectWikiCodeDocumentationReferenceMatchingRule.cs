// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;
using ContinuousLearning;

/// <summary>
/// ProjectWiki C# documentation adapter for frozen-reference calibration.
/// It measures exact evidence grounding: the candidate and paid reference must
/// cite the same set of evidence ids, regardless of prose wording or id order.
/// This is not a teacher, not a DPO side selector, and not a text-similarity
/// metric.
/// </summary>
public static class ProjectWikiCodeDocumentationReferenceMatchingRule
{
    public const string DomainTag = "project-local-wiki";
    public const string SubjectId = "code-documentation-csharp";
    public const string RuleId = "project-wiki-code-documentation-exact-evidence-ids";
    public const string RuleVersion =
        "v1.schema-xiigen.project-wiki-code-documentation-reference-evidence.v2.subject-code-documentation-csharp.policy-exact-set-ordinal";

    public static OperationResult<ReferenceMatchingRuleIdentity> CreateIdentity()
        => ReferenceMatchingRuleIdentity.Create(
            DomainTag,
            RuleId,
            RuleVersion,
            "ProjectWiki C# documentation frozen-reference instrument. It compares exact ordinal evidence-id sets from schema " +
            ProjectWikiReferenceEvidencePayload.Schema +
            " for subject " +
            SubjectId +
            ". Answer prose may differ; duplicate or malformed evidence ids fail closed. This is not training, not DPO, and not a paid-reference corpus claim.");

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
            // Subject is part of the typed payload so an Angular or SQL answer
            // with the same evidence-id strings cannot be accepted as C#.
            return false;
        }

        var candidateIds = candidate.Value.EvidenceIds;
        var referenceIds = reference.Value.EvidenceIds;
        if (candidateIds.Count != referenceIds.Count)
            return false;

        // ProjectWiki answer text can legitimately vary. The instrument is exact
        // only about the grounding set, using ordinal comparison so future
        // Angular/SQL subjects cannot blur C# evidence ids into locale rules.
        return candidateIds.SequenceEqual(referenceIds, StringComparer.Ordinal);
    }

    public static OperationResult<ReferenceMatchingRuleCalibrationResult>
        EvaluateCalibration(ReferenceMatchingCalibrationInput input)
    {
        if (input is null)
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "ProjectWiki reference matching calibration input is required.",
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
                "Calibration input rule identity does not match the ProjectWiki C# exact evidence-id policy.",
                "PROJECT_WIKI_REFERENCE_MATCHING_RULE_IDENTITY_MISMATCH");
        }

        if (!StringComparer.Ordinal.Equals(input.Corpus.SubjectId, SubjectId))
        {
            // Generic calibration input is domain-scoped. The ProjectWiki
            // adapter adds this subject guard before metrics so C#, Angular,
            // and SQL corpora cannot share one project-local-wiki lane.
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "Calibration input frozen corpus subject does not match the ProjectWiki C# exact evidence-id policy.",
                "PROJECT_WIKI_REFERENCE_MATCHING_CORPUS_SUBJECT_MISMATCH");
        }

        return ReferenceMatchingRuleCalibrationEvaluator.Evaluate(
            input,
            CreatePredicate());
    }

    private static bool HasExpectedSubject(ProjectWikiReferenceEvidencePayload payload)
        => StringComparer.Ordinal.Equals(payload.SubjectId, SubjectId);
}
