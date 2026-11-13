// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using AgentInfrastructure;
using ContinuousLearning;
using TranslationDomain;

namespace LlmFromScratch.Tests;

public sealed class LanguageTranslateReferenceMatchingRuleTests
{
    private static readonly DateTimeOffset CapturedAtUtc =
        new(2026, 9, 8, 8, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FrozenAtUtc =
        new(2026, 9, 8, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Identity_DeterministicallyBindsDomainScorerProfilesAndThresholdPolicy()
    {
        var first = Require(LanguageTranslateReferenceMatchingRule.CreateIdentity());
        var second = Require(LanguageTranslateReferenceMatchingRule.CreateIdentity());

        Assert.Equal(first.IdentitySha256, second.IdentitySha256);
        Assert.Equal(first.CanonicalRepresentation, second.CanonicalRepresentation);
        Assert.Equal(LanguageTranslateHeldOutScorer.DomainTag, first.DomainTag);
        Assert.Equal(
            LanguageTranslateReferenceMatchingRule.RuleId,
            first.RuleId);
        Assert.Contains(
            LanguageTranslateHeldOutScorer.NormalizationProfile,
            first.HumanDescription,
            StringComparison.Ordinal);
        Assert.Contains(
            LanguageTranslateHeldOutScorer.MetricProfile,
            first.HumanDescription,
            StringComparison.Ordinal);
        Assert.Contains(
            LanguageTranslateReferenceMatchingPolicy.Strict.PolicyId,
            first.RuleVersion,
            StringComparison.Ordinal);
        Assert.Contains(
            "threshold-" +
            LanguageTranslateReferenceMatchingPolicy.Strict.MinimumCandidateScoreInvariant,
            first.RuleVersion,
            StringComparison.Ordinal);
        Assert.Contains("not DPO", first.HumanDescription, StringComparison.Ordinal);
    }

    [Fact]
    public void EvaluateCalibration_SelfReferenceKnownPositiveHasZeroFalseMiss()
    {
        var entry = CreateEntry(
            "self-reference-positive",
            "hello world",
            "привет мир");
        var input = CreateCalibrationInput(
            CreateCorpus(entry),
            positives:
            [
                Pair(
                    "positive-self-reference",
                    entry.EntryId,
                    entry.PaidModelReferenceAnswer,
                    entry.PaidModelReferenceAnswer)
            ],
            negatives:
            [
                Pair(
                    "negative-control",
                    entry.EntryId,
                    "до свидания",
                    entry.PaidModelReferenceAnswer)
            ]);

        var result = Require(
            LanguageTranslateReferenceMatchingRule.EvaluateCalibration(input));

        Assert.Equal(1, result.KnownPositiveCount);
        Assert.Equal(0, result.FalseMissCount);
        Assert.Equal(0D, result.FalseMissRate);
    }

    [Fact]
    public void EvaluateCalibration_SameDomainForeignAnswerHasZeroFalseHitUnderStrictPolicy()
    {
        var entry = CreateEntry(
            "foreign-negative",
            "translate: the apple is red",
            "яблоко красное");
        var input = CreateCalibrationInput(
            CreateCorpus(entry),
            positives:
            [
                Pair(
                    "positive-exact",
                    entry.EntryId,
                    entry.PaidModelReferenceAnswer,
                    entry.PaidModelReferenceAnswer)
            ],
            negatives:
            [
                Pair(
                    "negative-same-domain-foreign-answer",
                    entry.EntryId,
                    "поезд опаздывает",
                    entry.PaidModelReferenceAnswer)
            ]);

        var result = Require(
            LanguageTranslateReferenceMatchingRule.EvaluateCalibration(input));

        Assert.Equal(1, result.KnownNegativeCount);
        Assert.Equal(0, result.FalseHitCount);
        Assert.Equal(0D, result.FalseHitRate);
    }

    [Fact]
    public void Predicate_IsSensitiveToCandidateNearThresholdAndPolicyThreshold()
    {
        var boundaryScore = Require(
            LanguageTranslateHeldOutScorer.ScoreOne("abcd", "abce"));
        var relaxed = Require(LanguageTranslateReferenceMatchingPolicy.Create(
            "boundary-open",
            boundaryScore.CandidateScore - 0.001D,
            requireExactMatch: false));
        var strict = Require(LanguageTranslateReferenceMatchingPolicy.Create(
            "boundary-closed",
            boundaryScore.CandidateScore + 0.001D,
            requireExactMatch: false));
        var slightlyCloser = Require(LanguageTranslateReferenceMatchingPolicy.Create(
            "boundary-candidate-change",
            0.39D,
            requireExactMatch: false));

        Assert.True(LanguageTranslateReferenceMatchingRule.IsMatch(
            "abce",
            "abcd",
            relaxed));
        Assert.False(LanguageTranslateReferenceMatchingRule.IsMatch(
            "abce",
            "abcd",
            strict));
        Assert.False(LanguageTranslateReferenceMatchingRule.IsMatch(
            "abce",
            "abcd",
            slightlyCloser));
        Assert.True(LanguageTranslateReferenceMatchingRule.IsMatch(
            "abcde",
            "abcd",
            slightlyCloser));
        Assert.NotEqual(
            Require(LanguageTranslateReferenceMatchingRule.CreateIdentity(relaxed))
                .IdentitySha256,
            Require(LanguageTranslateReferenceMatchingRule.CreateIdentity(strict))
                .IdentitySha256);
    }

    [Fact]
    public void Predicate_InvalidOversizedAndInvalidUnicodeScorerInputsFailClosed()
    {
        var oversizedReference = new string('a', 3_000);
        var oversizedCandidate = new string('b', 3_000);

        Assert.False(LanguageTranslateReferenceMatchingRule.IsMatch(
            oversizedCandidate,
            oversizedReference));
        Assert.False(LanguageTranslateReferenceMatchingRule.IsMatch(
            "\ud800",
            "x"));
        Assert.False(LanguageTranslateReferenceMatchingRule.IsMatch(
            "x",
            "\ud800"));
    }

    [Fact]
    public void CalibrationInput_RejectsForeignEntryHashAndReferenceBeforeAdapterProducesNumbers()
    {
        var entry = CreateEntry(
            "owned-entry",
            "translate: green tree",
            "зелёное дерево");
        var foreignEntry = CreateEntry(
            "foreign-entry",
            "translate: blue sky",
            "синее небо");
        var corpus = CreateCorpus(entry);

        var foreignEntryResult = ReferenceMatchingCalibrationInput.Create(
            corpus,
            Require(LanguageTranslateReferenceMatchingRule.CreateIdentity()),
            knownPositivePairs:
            [
                Pair(
                    "positive-foreign-entry",
                    foreignEntry.EntryId,
                    foreignEntry.PaidModelReferenceAnswer,
                    foreignEntry.PaidModelReferenceAnswer)
            ],
            knownNegativePairs:
            [
                Pair(
                    "negative-control",
                    entry.EntryId,
                    "не тот ответ",
                    entry.PaidModelReferenceAnswer)
            ]);
        var tamperedReferenceResult = ReferenceMatchingCalibrationInput.Create(
            corpus,
            Require(LanguageTranslateReferenceMatchingRule.CreateIdentity()),
            knownPositivePairs:
            [
                Pair(
                    "positive-tampered-reference",
                    entry.EntryId,
                    entry.PaidModelReferenceAnswer,
                    foreignEntry.PaidModelReferenceAnswer)
            ],
            knownNegativePairs:
            [
                Pair(
                    "negative-control",
                    entry.EntryId,
                    "не тот ответ",
                    entry.PaidModelReferenceAnswer)
            ]);

        Assert.False(foreignEntryResult.IsSuccess);
        Assert.Equal(
            "REFERENCE_MATCHING_CALIBRATION_ENTRY_UNKNOWN",
            foreignEntryResult.ErrorCode);
        Assert.False(tamperedReferenceResult.IsSuccess);
        Assert.Equal(
            "REFERENCE_MATCHING_CALIBRATION_REFERENCE_MISMATCH",
            tamperedReferenceResult.ErrorCode);
    }

    private static ReferenceMatchingCalibrationInput CreateCalibrationInput(
        FrozenReferenceCorpus corpus,
        IReadOnlyList<ReferenceMatchingCalibrationPair> positives,
        IReadOnlyList<ReferenceMatchingCalibrationPair> negatives)
        => Require(ReferenceMatchingCalibrationInput.Create(
            corpus,
            Require(LanguageTranslateReferenceMatchingRule.CreateIdentity()),
            positives,
            negatives));

    private static FrozenReferenceCorpus CreateCorpus(
        params FrozenReferenceEntry[] entries)
        => Require(FrozenReferenceCorpus.Build(
            "language-translate-en-ru-reference-fixture",
            "v2026-09-08-unit",
            FrozenAtUtc,
            entries));

    private static FrozenReferenceEntry CreateEntry(
        string seed,
        string ownerInput,
        string referenceAnswer)
    {
        // Unit-only fake paid provenance: this fixture is not a production paid
        // corpus, is not stored under training-data/reference-corpora, and is
        // never used as DPO chosen/rejected training material.
        var referenceHash =
            FrozenReferenceEntry.ComputeNormalizedTextSha256(referenceAnswer);
        var provenance = Require(PaidReferenceProvenance.Create(
            "unit-fake-paid-provider",
            "unit-fake-paid-model",
            "2026-09-08",
            "unit-request-" + seed,
            CapturedAtUtc,
            referenceHash));

        return Require(FrozenReferenceEntry.Create(
            ownerInput,
            referenceAnswer,
            provenance,
            LanguageTranslateHeldOutScorer.DomainTag,
            "en-ru",
            FrozenReferencePartition.HeldOut,
            FrozenReferenceEntry.ComputeNormalizedTextSha256(
                "unit-source:" + seed)));
    }

    private static ReferenceMatchingCalibrationPair Pair(
        string pairId,
        string referenceEntryId,
        string candidate,
        string reference)
        => Require(ReferenceMatchingCalibrationPair.Create(
            pairId,
            referenceEntryId,
            candidate,
            reference));

    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(
            result.IsSuccess,
            $"Expected success but got {result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        return result.Value!;
    }
}
