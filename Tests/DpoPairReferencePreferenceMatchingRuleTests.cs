// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using AgentInfrastructure;
using ContinuousLearning;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;
using DpoTrainerPairAdmission = DPO.Models.DpoTrainerPairAdmission;

namespace LlmFromScratch.Tests;

public sealed class DpoPairReferencePreferenceMatchingRuleTests
{
    private const string DomainTag = "unit-reference-preference-domain";
    private const string SubjectId = "unit-reference-preference-subject";
    private const string OwnerInput = "Improve this deterministic product slice.";
    private const string PaidReferenceAnswer = "paid frozen reference answer";
    private const string ChosenMatchAnswer = "chosen answer accepted by subject matcher";
    private const string RejectedMissAnswer = "rejected answer outside subject matcher";

    private static readonly DateTimeOffset CapturedAtUtc =
        new(2026, 9, 8, 14, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FrozenAtUtc =
        new(2026, 9, 8, 14, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Evaluate_AcceptsOnlyChosenMatchAgainstFrozenReference()
    {
        var reference = CreateReferenceEntry();
        var pair = CreatePair();
        var admission = CreateAdmission(pair);

        var result = Require(Evaluate(pair, reference, admission: admission));

        Assert.True(result.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.AcceptedStatus,
            result.Status);
        Assert.Equal(
            admission.ContentFingerprintSha256,
            result.AdmissionContentFingerprintSha256);
        Assert.True(result.ChosenMatchesReference);
        Assert.False(result.RejectedMatchesReference);
        Assert.Equal(reference.EntryId, result.ReferenceEntryId);
        Assert.Equal(
            reference.PaidReferenceProvenance.ReferenceAnswerSha256,
            result.ReferenceAnswerSha256);
        Assert.DoesNotContain(PaidReferenceAnswer, result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Evaluate_ChosenRejectedSwapFailsAsRejectedOnlyMatch()
    {
        var reference = CreateReferenceEntry();
        var pair = CreatePair(
            chosen: RejectedMissAnswer,
            rejected: ChosenMatchAnswer);

        var result = Require(Evaluate(pair, reference));

        Assert.False(result.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.RejectedOnlyMatchesStatus,
            result.Status);
        Assert.False(result.ChosenMatchesReference);
        Assert.True(result.RejectedMatchesReference);
    }

    [Fact]
    public void Evaluate_BothMatchTieFailsClosed()
    {
        var result = Require(Evaluate(
            CreatePair(chosen: ChosenMatchAnswer, rejected: ChosenMatchAnswer),
            CreateReferenceEntry()));

        Assert.False(result.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.BothSidesMatchTieStatus,
            result.Status);
        Assert.True(result.ChosenMatchesReference);
        Assert.True(result.RejectedMatchesReference);
    }

    [Fact]
    public void Evaluate_NeitherSideMatchesFailsClosed()
    {
        var result = Require(Evaluate(
            CreatePair(chosen: "unmatched chosen", rejected: "unmatched rejected"),
            CreateReferenceEntry()));

        Assert.False(result.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.NeitherSideMatchesStatus,
            result.Status);
        Assert.False(result.ChosenMatchesReference);
        Assert.False(result.RejectedMatchesReference);
    }

    [Fact]
    public void Evaluate_PaidReferenceCopiedIntoEitherSideFailsBeforePredicate()
    {
        var reference = CreateReferenceEntry();

        var copiedChosen = Require(Evaluate(
            CreatePair(chosen: PaidReferenceAnswer, rejected: RejectedMissAnswer),
            reference,
            predicate: (_, _) => throw new InvalidOperationException("predicate must not run for copied chosen")));
        var copiedRejected = Require(Evaluate(
            CreatePair(chosen: ChosenMatchAnswer, rejected: PaidReferenceAnswer),
            reference,
            predicate: (_, _) => throw new InvalidOperationException("predicate must not run for copied rejected")));

        Assert.False(copiedChosen.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.PaidReferenceCopiedToChosenStatus,
            copiedChosen.Status);
        Assert.False(copiedRejected.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.PaidReferenceCopiedToRejectedStatus,
            copiedRejected.Status);
    }

    [Fact]
    public void Evaluate_PaidExternalModelAuthorshipOnEitherSideFailsClosed()
    {
        var reference = CreateReferenceEntry();
        var pair = CreatePair();

        var paidChosen = Require(Evaluate(
            pair,
            reference,
            admission: CreateAdmission(
                pair,
                DpoPairSideAuthorKind.PaidExternalModel,
                DpoPairSideAuthorKind.User)));
        var paidRejected = Require(Evaluate(
            pair,
            reference,
            admission: CreateAdmission(
                pair,
                DpoPairSideAuthorKind.LocalModel,
                DpoPairSideAuthorKind.PaidExternalModel)));

        Assert.False(paidChosen.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.PaidExternalModelAuthorStatus,
            paidChosen.Status);
        Assert.False(paidRejected.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.PaidExternalModelAuthorStatus,
            paidRejected.Status);
    }

    [Fact]
    public void Evaluate_PromptOwnerInputMismatchFailsBeforePredicate()
    {
        var result = Require(Evaluate(
            CreatePair(prompt: "Different owner input."),
            CreateReferenceEntry(),
            predicate: (_, _) => throw new InvalidOperationException("predicate must not run before owner binding")));

        Assert.False(result.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.OwnerInputMismatchStatus,
            result.Status);
    }

    [Fact]
    public void Evaluate_DomainSubjectAndRuleIdentityMismatchFailClosedBeforeMetrics()
    {
        var pair = CreatePair();
        var wrongDomainReference = CreateReferenceEntry(domainTag: "unit-reference-preference-domain-other");
        var domainMismatch = Require(Evaluate(pair, wrongDomainReference));
        var subjectMismatch = Require(Evaluate(
            pair,
            CreateReferenceEntry(),
            subjectId: "unit-reference-preference-subject-other"));
        var ruleIdentityMismatch = Require(Evaluate(
            pair,
            CreateReferenceEntry(),
            identity: CreateIdentity(domainTag: "unit-reference-preference-domain-other")));

        Assert.False(domainMismatch.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.DomainMismatchStatus,
            domainMismatch.Status);
        Assert.False(subjectMismatch.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.SubjectMismatchStatus,
            subjectMismatch.Status);
        Assert.False(ruleIdentityMismatch.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.RuleIdentityMismatchStatus,
            ruleIdentityMismatch.Status);
    }

    [Fact]
    public void Evaluate_AdmissionContentMismatchFailsBeforePredicate()
    {
        var pair = CreatePair();
        var mismatchedAdmission = new DpoTrainerPairAdmission(
            DpoPairSideAuthorKind.LocalModel,
            DpoPairSideAuthorKind.User,
            pair.Prompt,
            pair.Chosen,
            "different rejected bytes",
            "unit-test");

        var result = Require(Evaluate(
            pair,
            CreateReferenceEntry(),
            admission: mismatchedAdmission,
            predicate: (_, _) => throw new InvalidOperationException("predicate must not run before admission binding")));

        Assert.False(result.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.AdmissionContentMismatchStatus,
            result.Status);
    }

    [Fact]
    public void Evaluate_MissingChosenOrRejectedAuthorshipFailsBeforePredicate()
    {
        var reference = CreateReferenceEntry();
        var missingChosenAuthorship = CreatePair(includeChosenAuthorship: false);
        var missingRejectedAuthorship = CreatePair(includeRejectedAuthorship: false);
        var predicateCalls = 0;
        ReferenceMatchingRulePredicate predicate = (_, _) =>
        {
            predicateCalls++;
            throw new InvalidOperationException("predicate must not run before authorship binding");
        };

        var chosenResult = Require(Evaluate(
            missingChosenAuthorship,
            reference,
            admission: CreateAdmission(missingChosenAuthorship),
            predicate: predicate));
        var rejectedResult = Require(Evaluate(
            missingRejectedAuthorship,
            reference,
            admission: CreateAdmission(missingRejectedAuthorship),
            predicate: predicate));

        Assert.False(chosenResult.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.AuthorshipUnsupportedStatus,
            chosenResult.Status);
        Assert.False(rejectedResult.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.AuthorshipUnsupportedStatus,
            rejectedResult.Status);
        Assert.Equal(0, predicateCalls);
    }

    [Fact]
    public void Evaluate_AdmissionAuthorKindMismatchFailsBeforePredicate()
    {
        var pair = CreatePair();
        var expectedAdmission = CreateAdmission(pair);
        var mismatchedAdmission = CreateAdmission(
            pair,
            DpoPairSideAuthorKind.DeterministicGenerator,
            DpoPairSideAuthorKind.User);
        var predicateCalls = 0;
        ReferenceMatchingRulePredicate predicate = (_, _) =>
        {
            predicateCalls++;
            throw new InvalidOperationException("predicate must not run before authorship admission binding");
        };

        var result = Require(Evaluate(
            pair,
            CreateReferenceEntry(),
            admission: mismatchedAdmission,
            predicate: predicate));

        Assert.Equal(
            expectedAdmission.ContentFingerprintSha256,
            mismatchedAdmission.ContentFingerprintSha256);
        Assert.False(result.IsAccepted);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.AuthorshipAdmissionMismatchStatus,
            result.Status);
        Assert.Equal(
            mismatchedAdmission.ContentFingerprintSha256,
            result.AdmissionContentFingerprintSha256);
        Assert.Equal(0, predicateCalls);
    }

    [Fact]
    public void Evaluate_PredicateFailureFailsSafelyAndCancellationPropagatesLikeGenericPolicy()
    {
        var failure = DpoPairReferencePreferenceMatchingRule.Evaluate(
            CreatePair(),
            CreateAdmission(CreatePair()),
            SubjectId,
            CreateReferenceEntry(),
            CreateIdentity(),
            (_, _) => throw new InvalidOperationException("subject matcher failed"));

        Assert.False(failure.IsSuccess);
        Assert.Equal(
            DpoPairReferencePreferenceMatchingRule.PredicateFailedErrorCode,
            failure.ErrorCode);
        Assert.Throws<OperationCanceledException>(() =>
            DpoPairReferencePreferenceMatchingRule.Evaluate(
                CreatePair(),
                CreateAdmission(CreatePair()),
                SubjectId,
                CreateReferenceEntry(),
                CreateIdentity(),
                (_, _) => throw new OperationCanceledException()));
    }

    [Fact]
    public void Evaluate_ResultHashBindsExactAdmissionContentFingerprint()
    {
        var reference = CreateReferenceEntry();
        var firstPair = CreatePair(
            chosen: "first accepted chosen answer",
            rejected: "first rejected miss");
        var secondPair = CreatePair(
            chosen: "second accepted chosen answer",
            rejected: "second rejected miss");
        var firstAdmission = CreateAdmission(firstPair);
        var secondAdmission = CreateAdmission(secondPair);
        var acceptedChosenAnswers = new HashSet<string>(StringComparer.Ordinal)
        {
            firstPair.Chosen,
            secondPair.Chosen
        };
        ReferenceMatchingRulePredicate predicate = (candidateAnswer, referenceAnswer) =>
            StringComparer.Ordinal.Equals(referenceAnswer, PaidReferenceAnswer) &&
            acceptedChosenAnswers.Contains(candidateAnswer);

        var first = Require(Evaluate(
            firstPair,
            reference,
            admission: firstAdmission,
            predicate: predicate));
        var second = Require(Evaluate(
            secondPair,
            reference,
            admission: secondAdmission,
            predicate: predicate));

        Assert.True(first.IsAccepted);
        Assert.True(second.IsAccepted);
        Assert.Equal(
            firstAdmission.ContentFingerprintSha256,
            first.AdmissionContentFingerprintSha256);
        Assert.Equal(
            secondAdmission.ContentFingerprintSha256,
            second.AdmissionContentFingerprintSha256);
        Assert.NotEqual(
            first.AdmissionContentFingerprintSha256,
            second.AdmissionContentFingerprintSha256);
        Assert.NotEqual(first.ResultSha256, second.ResultSha256);
        Assert.DoesNotContain(firstPair.Prompt, first.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(firstPair.Chosen, first.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(firstPair.Rejected, first.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(PaidReferenceAnswer, first.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Evaluate_ResultIsDeterministicAndHasNoCallerScoreOrDistanceSurface()
    {
        var pair = CreatePair();
        var admission = CreateAdmission(pair);
        var reference = CreateReferenceEntry();

        var first = Require(Evaluate(pair, reference, admission: admission));
        var second = Require(Evaluate(pair, reference, admission: admission));
        var resultPropertyNames =
            typeof(DpoPairReferencePreferenceMatchingResult)
                .GetProperties()
                .Select(static property => property.Name)
                .ToArray();

        Assert.Equal(first, second);
        Assert.Equal(first.ResultSha256, second.ResultSha256);
        Assert.Equal(
            admission.ContentFingerprintSha256,
            first.AdmissionContentFingerprintSha256);
        Assert.DoesNotContain(
            resultPropertyNames,
            static name => name.Contains("Score", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            resultPropertyNames,
            static name => name.Contains("Distance", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            resultPropertyNames,
            static name => name.Contains("PaidModelReferenceAnswer", StringComparison.Ordinal));
        Assert.DoesNotContain(PaidReferenceAnswer, first.ToString(), StringComparison.Ordinal);
    }

    private static OperationResult<DpoPairReferencePreferenceMatchingResult> Evaluate(
        DpoPair pair,
        FrozenReferenceEntry reference,
        string subjectId = SubjectId,
        ReferenceMatchingRuleIdentity? identity = null,
        ReferenceMatchingRulePredicate? predicate = null,
        DpoTrainerPairAdmission? admission = null)
        => DpoPairReferencePreferenceMatchingRule.Evaluate(
            pair,
            admission ?? CreateAdmission(pair),
            subjectId,
            reference,
            identity ?? CreateIdentity(),
            predicate ?? SubjectMatcher);

    private static bool SubjectMatcher(string candidateAnswer, string referenceAnswer)
        => StringComparer.Ordinal.Equals(referenceAnswer, PaidReferenceAnswer) &&
           StringComparer.Ordinal.Equals(candidateAnswer, ChosenMatchAnswer);

    private static DpoPair CreatePair(
        string prompt = OwnerInput,
        string chosen = ChosenMatchAnswer,
        string rejected = RejectedMissAnswer,
        string domainTag = DomainTag,
        DpoPairSideAuthorKind chosenAuthor = DpoPairSideAuthorKind.LocalModel,
        DpoPairSideAuthorKind rejectedAuthor = DpoPairSideAuthorKind.User,
        bool includeChosenAuthorship = true,
        bool includeRejectedAuthorship = true)
        => new(prompt, chosen, rejected, domainTag, CapturedAtUtc)
        {
            ChosenAuthorship = includeChosenAuthorship
                ? new DpoPairSideAuthorship(chosenAuthor)
                : null,
            RejectedAuthorship = includeRejectedAuthorship
                ? new DpoPairSideAuthorship(rejectedAuthor)
                : null
        };

    private static DpoTrainerPairAdmission CreateAdmission(
        DpoPair pair,
        DpoPairSideAuthorKind chosenAuthor = DpoPairSideAuthorKind.LocalModel,
        DpoPairSideAuthorKind rejectedAuthor = DpoPairSideAuthorKind.User)
        => new(
            chosenAuthor,
            rejectedAuthor,
            pair.Prompt,
            pair.Chosen,
            pair.Rejected,
            "unit-test");

    private static FrozenReferenceEntry CreateReferenceEntry(
        string domainTag = DomainTag,
        string subjectId = SubjectId)
    {
        // Unit-only paid provenance fixture: no real paid corpus, DPO row,
        // teacher call, model call, checkpoint, weight, or training artifact is
        // created by this test.
        var referenceHash =
            FrozenReferenceEntry.ComputeNormalizedTextSha256(PaidReferenceAnswer);
        var provenance = Require(PaidReferenceProvenance.Create(
            "unit-fake-paid-provider",
            "unit-fake-paid-model",
            "2026-09-08",
            "unit-request-reference-preference",
            FrozenAtUtc,
            referenceHash));

        return Require(FrozenReferenceEntry.Create(
            OwnerInput,
            PaidReferenceAnswer,
            provenance,
            domainTag,
            subjectId,
            FrozenReferencePartition.HeldOut,
            FrozenReferenceEntry.ComputeNormalizedTextSha256("unit-source-reference-preference")));
    }

    private static ReferenceMatchingRuleIdentity CreateIdentity(
        string domainTag = DomainTag)
        => Require(ReferenceMatchingRuleIdentity.Create(
            domainTag,
            "unit-reference-preference-subject-matcher",
            "v1",
            "Unit subject matcher identity for DPO pair reference preference tests."));

    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(
            result.IsSuccess,
            $"Expected success but got {result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        return result.Value!;
    }
}
