// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Globalization;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO.Models;

/// <summary>
/// Result of comparing one already-authored DPO pair against one frozen paid
/// reference through an existing subject matcher. It records only booleans and
/// identities; it deliberately has no caller-supplied score or distance field.
/// </summary>
public sealed record DpoPairReferencePreferenceMatchingResult(
    bool IsAccepted,
    string Status,
    string DomainTag,
    string SubjectId,
    string RuleIdentitySha256,
    string AdmissionContentFingerprintSha256,
    string ReferenceEntryId,
    string ReferenceAnswerSha256,
    bool ChosenMatchesReference,
    bool RejectedMatchesReference,
    string ResultSha256);

/// <summary>
/// Generic deterministic DPO-side preference check. It does not decide whether
/// an answer matches a domain reference itself; it delegates that to the
/// already existing subject matcher and only verifies that the chosen side is
/// closer to the frozen reference than the rejected side.
/// </summary>
public static class DpoPairReferencePreferenceMatchingRule
{
    public const string AcceptedStatus = "accepted_chosen_matches_reference_rejected_does_not";
    public const string DomainMismatchStatus = "domain_mismatch";
    public const string SubjectMismatchStatus = "subject_mismatch";
    public const string RuleIdentityMismatchStatus = "rule_identity_domain_mismatch";
    public const string OwnerInputMismatchStatus = "owner_input_mismatch";
    public const string AdmissionContentMismatchStatus = "admission_content_fingerprint_mismatch";
    public const string PaidExternalModelAuthorStatus = "paid_external_model_side_forbidden";
    public const string AuthorshipUnsupportedStatus = "authorship_unsupported";
    public const string AuthorshipAdmissionMismatchStatus = "authorship_admission_mismatch";
    public const string PaidReferenceCopiedToChosenStatus = "paid_reference_copied_to_chosen_side";
    public const string PaidReferenceCopiedToRejectedStatus = "paid_reference_copied_to_rejected_side";
    public const string BothSidesMatchTieStatus = "both_sides_match_reference_tie";
    public const string NeitherSideMatchesStatus = "neither_side_matches_reference";
    public const string RejectedOnlyMatchesStatus = "rejected_side_matches_reference";
    public const string PredicateFailedErrorCode = "DPO_PAIR_REFERENCE_PREFERENCE_PREDICATE_FAILED";
    public const string InputRequiredErrorCode = "DPO_PAIR_REFERENCE_PREFERENCE_INPUT_REQUIRED";
    public const string ContentInvalidErrorCode = "DPO_PAIR_REFERENCE_PREFERENCE_CONTENT_INVALID";

    public static OperationResult<DpoPairReferencePreferenceMatchingResult> Evaluate(
        DpoPair pair,
        DpoTrainerPairAdmission admission,
        string subjectId,
        FrozenReferenceEntry referenceEntry,
        ReferenceMatchingRuleIdentity subjectMatcherIdentity,
        ReferenceMatchingRulePredicate subjectMatcher)
    {
        if (pair is null ||
            admission is null ||
            referenceEntry is null ||
            subjectMatcherIdentity is null ||
            subjectMatcher is null)
        {
            return Failure(
                "DPO pair, admission, frozen reference entry, matcher identity, and matcher predicate are required.",
                InputRequiredErrorCode);
        }

        if (referenceEntry.PaidReferenceProvenance is null) // This legacy normalized pair matcher requires its known-model reference contract.
            return Failure("Native capture reference requires its separate assessment path.", InputRequiredErrorCode); // Refuse before a legacy identity dereference; no training authority is added.

        var admissionContentFingerprintSha256 = admission.ContentFingerprintSha256;
        var normalizedDomain = FrozenReferenceCanonicalization.NormalizeIdentity(pair.Domain);
        var normalizedSubject = FrozenReferenceCanonicalization.NormalizeIdentity(subjectId);
        var normalizedPrompt = FrozenReferenceCanonicalization.NormalizeContent(pair.Prompt);
        var normalizedChosen = FrozenReferenceCanonicalization.NormalizeContent(pair.Chosen);
        var normalizedRejected = FrozenReferenceCanonicalization.NormalizeContent(pair.Rejected);
        if (!normalizedDomain.IsSuccess || normalizedDomain.Value is null)
            return normalizedDomain.MapError<DpoPairReferencePreferenceMatchingResult>();
        if (!normalizedSubject.IsSuccess || normalizedSubject.Value is null)
            return normalizedSubject.MapError<DpoPairReferencePreferenceMatchingResult>();
        if (!normalizedPrompt.IsSuccess || normalizedPrompt.Value is null)
            return ContentFailure();
        if (!normalizedChosen.IsSuccess || normalizedChosen.Value is null)
            return ContentFailure();
        if (!normalizedRejected.IsSuccess || normalizedRejected.Value is null)
            return ContentFailure();

        if (!StringComparer.Ordinal.Equals(subjectMatcherIdentity.DomainTag, normalizedDomain.Value))
        {
            return OkRejected(
                RuleIdentityMismatchStatus,
                normalizedDomain.Value,
                normalizedSubject.Value,
                subjectMatcherIdentity,
                referenceEntry,
                admissionContentFingerprintSha256,
                chosenMatchesReference: false,
                rejectedMatchesReference: false);
        }

        if (!StringComparer.Ordinal.Equals(referenceEntry.DomainTag, normalizedDomain.Value))
        {
            return OkRejected(
                DomainMismatchStatus,
                normalizedDomain.Value,
                normalizedSubject.Value,
                subjectMatcherIdentity,
                referenceEntry,
                admissionContentFingerprintSha256,
                chosenMatchesReference: false,
                rejectedMatchesReference: false);
        }

        if (!StringComparer.Ordinal.Equals(referenceEntry.SubjectId, normalizedSubject.Value))
        {
            return OkRejected(
                SubjectMismatchStatus,
                normalizedDomain.Value,
                normalizedSubject.Value,
                subjectMatcherIdentity,
                referenceEntry,
                admissionContentFingerprintSha256,
                chosenMatchesReference: false,
                rejectedMatchesReference: false);
        }

        if (!AdmissionMatchesPair(admission, pair))
        {
            return OkRejected(
                AdmissionContentMismatchStatus,
                normalizedDomain.Value,
                normalizedSubject.Value,
                subjectMatcherIdentity,
                referenceEntry,
                admissionContentFingerprintSha256,
                chosenMatchesReference: false,
                rejectedMatchesReference: false);
        }

        if (admission.HasPaidExternalModelSide ||
            admission.ChosenAuthor is DpoPairSideAuthorKind.PaidExternalModel ||
            admission.RejectedAuthor is DpoPairSideAuthorKind.PaidExternalModel)
        {
            return OkRejected(
                PaidExternalModelAuthorStatus,
                normalizedDomain.Value,
                normalizedSubject.Value,
                subjectMatcherIdentity,
                referenceEntry,
                admissionContentFingerprintSha256,
                chosenMatchesReference: false,
                rejectedMatchesReference: false);
        }

        if (!HasAllowedPairAuthorship(pair))
        {
            return OkRejected(
                AuthorshipUnsupportedStatus,
                normalizedDomain.Value,
                normalizedSubject.Value,
                subjectMatcherIdentity,
                referenceEntry,
                admissionContentFingerprintSha256,
                chosenMatchesReference: false,
                rejectedMatchesReference: false);
        }

        if (pair.ChosenAuthorship!.AuthorKind != admission.ChosenAuthor ||
            pair.RejectedAuthorship!.AuthorKind != admission.RejectedAuthor)
        {
            return OkRejected(
                AuthorshipAdmissionMismatchStatus,
                normalizedDomain.Value,
                normalizedSubject.Value,
                subjectMatcherIdentity,
                referenceEntry,
                admissionContentFingerprintSha256,
                chosenMatchesReference: false,
                rejectedMatchesReference: false);
        }

        if (!StringComparer.Ordinal.Equals(normalizedPrompt.Value, referenceEntry.OwnerInput))
        {
            return OkRejected(
                OwnerInputMismatchStatus,
                normalizedDomain.Value,
                normalizedSubject.Value,
                subjectMatcherIdentity,
                referenceEntry,
                admissionContentFingerprintSha256,
                chosenMatchesReference: false,
                rejectedMatchesReference: false);
        }

        var chosenHash = FrozenReferenceCanonicalization.Sha256Ref(normalizedChosen.Value);
        var rejectedHash = FrozenReferenceCanonicalization.Sha256Ref(normalizedRejected.Value);
        var referenceHash = referenceEntry.PaidReferenceProvenance.ReferenceAnswerSha256;
        if (StringComparer.Ordinal.Equals(chosenHash, referenceHash))
        {
            return OkRejected(
                PaidReferenceCopiedToChosenStatus,
                normalizedDomain.Value,
                normalizedSubject.Value,
                subjectMatcherIdentity,
                referenceEntry,
                admissionContentFingerprintSha256,
                chosenMatchesReference: false,
                rejectedMatchesReference: false);
        }

        if (StringComparer.Ordinal.Equals(rejectedHash, referenceHash))
        {
            return OkRejected(
                PaidReferenceCopiedToRejectedStatus,
                normalizedDomain.Value,
                normalizedSubject.Value,
                subjectMatcherIdentity,
                referenceEntry,
                admissionContentFingerprintSha256,
                chosenMatchesReference: false,
                rejectedMatchesReference: false);
        }

        bool chosenMatchesReference;
        bool rejectedMatchesReference;
        try
        {
            chosenMatchesReference = subjectMatcher(
                normalizedChosen.Value,
                referenceEntry.PaidModelReferenceAnswer);
            rejectedMatchesReference = subjectMatcher(
                normalizedRejected.Value,
                referenceEntry.PaidModelReferenceAnswer);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure(
                "Subject reference matching predicate failed while evaluating the DPO pair preference.",
                PredicateFailedErrorCode);
        }

        var status = ResolveStatus(
            chosenMatchesReference,
            rejectedMatchesReference);
        return OkRejected(
            status,
            normalizedDomain.Value,
            normalizedSubject.Value,
            subjectMatcherIdentity,
            referenceEntry,
            admissionContentFingerprintSha256,
            chosenMatchesReference,
            rejectedMatchesReference);
    }

    private static bool AdmissionMatchesPair(
        DpoTrainerPairAdmission admission,
        DpoPair pair)
    {
        var rebound = new DpoTrainerPairAdmission(
            admission.ChosenAuthor,
            admission.RejectedAuthor,
            pair.Prompt,
            pair.Chosen,
            pair.Rejected,
            admission.IssuerName);
        return StringComparer.Ordinal.Equals(
            rebound.ContentFingerprintSha256,
            admission.ContentFingerprintSha256);
    }

    private static bool IsAllowedAuthor(DpoPairSideAuthorKind author)
        => author is DpoPairSideAuthorKind.User
            or DpoPairSideAuthorKind.LocalModel
            or DpoPairSideAuthorKind.OpenTeacher
            or DpoPairSideAuthorKind.DeterministicGenerator;

    private static bool HasAllowedPairAuthorship(DpoPair pair)
        => pair.ChosenAuthorship is not null &&
           pair.RejectedAuthorship is not null &&
           IsAllowedAuthor(pair.ChosenAuthorship.AuthorKind) &&
           IsAllowedAuthor(pair.RejectedAuthorship.AuthorKind);

    private static string ResolveStatus(
        bool chosenMatchesReference,
        bool rejectedMatchesReference)
        => (chosenMatchesReference, rejectedMatchesReference) switch
        {
            (true, false) => AcceptedStatus,
            (true, true) => BothSidesMatchTieStatus,
            (false, true) => RejectedOnlyMatchesStatus,
            _ => NeitherSideMatchesStatus
        };

    private static OperationResult<DpoPairReferencePreferenceMatchingResult> OkRejected(
        string status,
        string domainTag,
        string subjectId,
        ReferenceMatchingRuleIdentity ruleIdentity,
        FrozenReferenceEntry referenceEntry,
        string admissionContentFingerprintSha256,
        bool chosenMatchesReference,
        bool rejectedMatchesReference)
    {
        var accepted = StringComparer.Ordinal.Equals(status, AcceptedStatus);
        var resultSha256 = BuildResultSha256(
            accepted,
            status,
            domainTag,
            subjectId,
            ruleIdentity,
            referenceEntry,
            admissionContentFingerprintSha256,
            chosenMatchesReference,
            rejectedMatchesReference);
        return OperationResult<DpoPairReferencePreferenceMatchingResult>.Ok(
            new DpoPairReferencePreferenceMatchingResult(
                accepted,
                status,
                domainTag,
                subjectId,
                ruleIdentity.IdentitySha256,
                admissionContentFingerprintSha256,
                referenceEntry.EntryId,
                referenceEntry.PaidReferenceProvenance.ReferenceAnswerSha256,
                chosenMatchesReference,
                rejectedMatchesReference,
                resultSha256));
    }

    private static string BuildResultSha256(
        bool accepted,
        string status,
        string domainTag,
        string subjectId,
        ReferenceMatchingRuleIdentity ruleIdentity,
        FrozenReferenceEntry referenceEntry,
        string admissionContentFingerprintSha256,
        bool chosenMatchesReference,
        bool rejectedMatchesReference)
    {
        var canonical = new StringBuilder();
        FrozenReferenceCanonicalization.Append(canonical, "schema", "xiigen.dpo-pair-reference-preference-result.v1");
        FrozenReferenceCanonicalization.Append(canonical, "accepted", accepted ? "true" : "false");
        FrozenReferenceCanonicalization.Append(canonical, "status", status);
        FrozenReferenceCanonicalization.Append(canonical, "domain_tag", domainTag);
        FrozenReferenceCanonicalization.Append(canonical, "subject_id", subjectId);
        FrozenReferenceCanonicalization.Append(canonical, "rule_identity_sha256", ruleIdentity.IdentitySha256);
        FrozenReferenceCanonicalization.Append(canonical, "admission_content_fingerprint_sha256", admissionContentFingerprintSha256);
        FrozenReferenceCanonicalization.Append(canonical, "reference_entry_id", referenceEntry.EntryId);
        FrozenReferenceCanonicalization.Append(canonical, "reference_answer_sha256", referenceEntry.PaidReferenceProvenance.ReferenceAnswerSha256);
        FrozenReferenceCanonicalization.Append(canonical, "chosen_matches_reference", chosenMatchesReference ? "true" : "false");
        FrozenReferenceCanonicalization.Append(canonical, "rejected_matches_reference", rejectedMatchesReference ? "true" : "false");
        return FrozenReferenceCanonicalization.Sha256Ref(canonical.ToString());
    }

    private static OperationResult<DpoPairReferencePreferenceMatchingResult> ContentFailure()
        => Failure(
            "DPO pair content is unsafe or cannot be normalized for frozen-reference preference matching.",
            ContentInvalidErrorCode);

    private static OperationResult<DpoPairReferencePreferenceMatchingResult> Failure(
        string message,
        string code)
        => OperationResult<DpoPairReferencePreferenceMatchingResult>.Fail(
            message,
            code);
}
