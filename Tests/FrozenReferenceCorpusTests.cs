// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ContinuousLearning;

public sealed class FrozenReferenceCorpusTests
{
    private static readonly DateTimeOffset CapturedAtUtc =
        new(2026, 8, 26, 10, 15, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FrozenAtUtc =
        new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FrozenReferenceEntryContract_ContainsSixRequiredMeaningsAndNoHumanLabels()
    {
        var entry = CreateEntry("source-contract", FrozenReferencePartition.Train);
        var propertyNames = typeof(FrozenReferenceEntry)
            .GetProperties()
            .Select(static property => property.Name)
            .ToArray();

        Assert.False(string.IsNullOrWhiteSpace(entry.EntryId));
        Assert.Contains(nameof(FrozenReferenceEntry.OwnerInput), propertyNames);
        Assert.Contains(nameof(FrozenReferenceEntry.PaidModelReferenceAnswer), propertyNames);
        Assert.Contains(nameof(FrozenReferenceEntry.PaidReferenceProvenance), propertyNames);
        Assert.Contains(nameof(FrozenReferenceEntry.DomainTag), propertyNames);
        Assert.Contains(nameof(FrozenReferenceEntry.SubjectId), propertyNames);
        Assert.Contains(nameof(FrozenReferenceEntry.Partition), propertyNames);
        Assert.Contains(nameof(FrozenReferenceEntry.SourceEntityFingerprint), propertyNames);
        Assert.DoesNotContain(
            propertyNames,
            property => property.Contains("HumanLabel", StringComparison.OrdinalIgnoreCase));
        Assert.NotEqual(
            typeof(TrainingTeacherProvenance),
            entry.PaidReferenceProvenance.GetType());
    }

    [Fact]
    public void PaidReferenceProvenance_ProvesClosedPaidReferenceAndHasNoMutableAuthoritySwitches()
    {
        var provenance = CreateProvenance("immutable-switches", "эталон");
        var switchProperties = typeof(PaidReferenceProvenance)
            .GetProperties()
            .Where(static property =>
                property.Name is nameof(PaidReferenceProvenance.ClosedPaidReference)
                    or nameof(PaidReferenceProvenance.PaidReferenceOutputNeverTrainable)
                    or nameof(PaidReferenceProvenance.PaidReferenceOutputNeverDpoPairSide))
            .ToArray();

        Assert.True(provenance.ClosedPaidReference);
        Assert.True(provenance.PaidReferenceOutputNeverTrainable);
        Assert.True(provenance.PaidReferenceOutputNeverDpoPairSide);
        Assert.Equal(CapturedAtUtc, provenance.CapturedAtUtc);
        Assert.All(switchProperties, property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void FrozenReferenceContracts_DoNotExposePublicCanonicalizationHelpers()
    {
        var canonicalizationType =
            typeof(FrozenReferenceEntry).Assembly.GetType(
                "ContinuousLearning.FrozenReferenceCanonicalization");
        var publicStaticMethods = new[]
            {
                typeof(PaidReferenceProvenance),
                typeof(FrozenReferenceEntry),
                typeof(FrozenReferenceCorpus),
                typeof(ReferenceMatchingRuleIdentity),
                typeof(ReferenceMatchingCalibrationPair),
                typeof(ReferenceMatchingCalibrationInput),
                typeof(ReferenceMatchingRuleCalibrationEvaluator)
            }
            .SelectMany(static type => type.GetMethods(
                BindingFlags.Public | BindingFlags.Static))
            .Select(static method =>
                method.DeclaringType!.Name + "." + method.Name)
            .ToArray();

        Assert.NotNull(canonicalizationType);
        Assert.False(canonicalizationType!.IsPublic);
        Assert.DoesNotContain(
            publicStaticMethods,
            name => name.Contains("Normalize", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Canonical", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Sha256", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FrozenReferenceCorpus_BuildsDeterministicHashAndCanonicalBytesAcrossInputOrder()
    {
        var alpha = CreateEntry("source-alpha", FrozenReferencePartition.Train);
        var beta = CreateEntry("source-beta", FrozenReferencePartition.HeldOut);
        var gamma = CreateEntry("source-gamma", FrozenReferencePartition.Train);

        var first = Require(FrozenReferenceCorpus.Build(
            "translation-en-ru",
            "v2026-08-26",
            FrozenAtUtc,
            new[] { gamma, alpha, beta }));
        var second = Require(FrozenReferenceCorpus.Build(
            "translation-en-ru",
            "v2026-08-26",
            FrozenAtUtc,
            new[] { beta, gamma, alpha }));

        Assert.Equal(first.CorpusSha256, second.CorpusSha256);
        Assert.Equal(first.CanonicalRepresentation, second.CanonicalRepresentation);
        Assert.Equal(first.EntryNames, second.EntryNames);
        Assert.Equal(3, first.Count);
        Assert.Equal("translation", first.DomainTag);
        Assert.Equal("en-ru", first.SubjectId);
        Assert.Equal("v2026-08-26", first.CorpusVersion);
        Assert.Equal(FrozenAtUtc, first.FrozenAtUtc);
        Assert.Equal("exact-source-entity-fingerprint", first.SplitProof.Method);
        Assert.Equal(0, first.SplitProof.CrossPartitionSourceEntityOverlapCount);
    }

    [Fact]
    public void FrozenReferenceCorpus_RejectsSourceEntityPresentInTrainAndHeldOut()
    {
        var sourceFingerprint = SourceFingerprint("same-owner-row");
        var train = CreateEntry(
            "same-owner-row-train",
            FrozenReferencePartition.Train,
            sourceFingerprint);
        var heldOut = CreateEntry(
            "same-owner-row-heldout",
            FrozenReferencePartition.HeldOut,
            sourceFingerprint);

        var result = FrozenReferenceCorpus.Build(
            "translation-en-ru",
            "v2026-08-26",
            FrozenAtUtc,
            new[] { train, heldOut });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "FROZEN_REFERENCE_SOURCE_CROSS_PARTITION_LEAKAGE",
            result.ErrorCode);
    }

    [Fact]
    public void FrozenReferenceCorpus_RejectsDuplicateEntryIds()
    {
        var entry = CreateEntry("source-duplicate", FrozenReferencePartition.Train);

        var result = FrozenReferenceCorpus.Build(
            "translation-en-ru",
            "v2026-08-26",
            FrozenAtUtc,
            new[] { entry, entry });

        Assert.False(result.IsSuccess);
        Assert.Equal("FROZEN_REFERENCE_ENTRY_ID_DUPLICATE", result.ErrorCode);
    }

    [Fact]
    public void FrozenReferenceCorpus_PropagatesCancellationFromEntryEnumerable()
    {
        var cancellation = new OperationCanceledException("corpus snapshot cancelled");

        var thrown = Assert.Throws<OperationCanceledException>(() =>
            FrozenReferenceCorpus.Build(
                "translation-en-ru",
                "v2026-08-26",
                FrozenAtUtc,
                ThrowOnMoveNext<FrozenReferenceEntry?>(cancellation)));

        // Cancellation is orchestration control flow, not a frozen-corpus
        // validation failure that should be hidden inside OperationResult.
        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public void FrozenReferenceCorpus_PropagatesAggregateCancellationGraphFromEntryEnumerable()
    {
        var cancellation = new OperationCanceledException("nested corpus snapshot cancellation");
        var aggregate = new AggregateException(
            new InvalidOperationException("recoverable sibling"),
            cancellation);

        var thrown = Assert.Throws<AggregateException>(() =>
            FrozenReferenceCorpus.Build(
                "translation-en-ru",
                "v2026-08-26",
                FrozenAtUtc,
                ThrowOnMoveNext<FrozenReferenceEntry?>(aggregate)));

        Assert.Same(aggregate, thrown);
        Assert.Contains(thrown.InnerExceptions, exception => ReferenceEquals(exception, cancellation));
    }

    [Fact]
    public void FrozenReferenceCorpus_RejectsMultipleDomainsAndSubjectsWithoutThrowPath()
    {
        var translation = CreateEntry(
            "source-translation",
            FrozenReferencePartition.Train,
            domainTag: "translation",
            subjectId: "en-ru");
        var wiki = CreateEntry(
            "source-wiki",
            FrozenReferencePartition.HeldOut,
            domainTag: "project-wiki",
            subjectId: "en-ru");
        var otherSubject = CreateEntry(
            "source-subject",
            FrozenReferencePartition.HeldOut,
            domainTag: "translation",
            subjectId: "fr-ru");

        var multiDomain = Record.Exception(() => FrozenReferenceCorpus.Build(
            "translation-en-ru",
            "v2026-08-26",
            FrozenAtUtc,
            new[] { translation, wiki }));
        var multiSubject = Record.Exception(() => FrozenReferenceCorpus.Build(
            "translation-en-ru",
            "v2026-08-26",
            FrozenAtUtc,
            new[] { translation, otherSubject }));

        Assert.Null(multiDomain);
        Assert.Null(multiSubject);
        Assert.Equal(
            "FROZEN_REFERENCE_CORPUS_DOMAIN_MISMATCH",
            FrozenReferenceCorpus.Build(
                "translation-en-ru",
                "v2026-08-26",
                FrozenAtUtc,
                new[] { translation, wiki }).ErrorCode);
        Assert.Equal(
            "FROZEN_REFERENCE_CORPUS_SUBJECT_MISMATCH",
            FrozenReferenceCorpus.Build(
                "translation-en-ru",
                "v2026-08-26",
                FrozenAtUtc,
                new[] { translation, otherSubject }).ErrorCode);
    }

    [Fact]
    public void FrozenReferenceCorpus_VersionAndFrozenTimestampAffectIdentity()
    {
        var entry = CreateEntry("source-version", FrozenReferencePartition.Train);
        var first = Require(FrozenReferenceCorpus.Build(
            "translation-en-ru",
            "v2026-08-26",
            FrozenAtUtc,
            new[] { entry }));
        var newVersion = Require(FrozenReferenceCorpus.Build(
            "translation-en-ru",
            "v2026-08-27",
            FrozenAtUtc,
            new[] { entry }));
        var newTime = Require(FrozenReferenceCorpus.Build(
            "translation-en-ru",
            "v2026-08-26",
            FrozenAtUtc.AddHours(1),
            new[] { entry }));

        Assert.NotEqual(first.CorpusSha256, newVersion.CorpusSha256);
        Assert.NotEqual(first.CorpusSha256, newTime.CorpusSha256);
    }

    [Fact]
    public void FrozenReferenceEntry_RejectsNoncanonicalHashes()
    {
        const string referenceAnswer = "эталонный ответ";
        var provenance = CreateProvenance("noncanonical-source", referenceAnswer);

        var result = FrozenReferenceEntry.Create(
            "переведи: alpha",
            referenceAnswer,
            provenance,
            "translation",
            "en-ru",
            FrozenReferencePartition.Train,
            "sha256:" + new string('A', 64));

        Assert.False(result.IsSuccess);
        Assert.Equal("FROZEN_REFERENCE_SOURCE_HASH_NONCANONICAL", result.ErrorCode);
    }

    [Fact]
    public void FrozenReferenceCorpus_ChangesIdentityWhenPaidReferenceAnswerChanges()
    {
        var first = CreateCorpus(referenceAnswer: "один устойчивый эталон");
        var second = CreateCorpus(referenceAnswer: "другой устойчивый эталон");

        Assert.NotEqual(first.Entries[0].EntryId, second.Entries[0].EntryId);
        Assert.NotEqual(first.CorpusSha256, second.CorpusSha256);
    }

    internal static FrozenReferenceCorpus CreateCorpus(
        string referenceAnswer = "устойчивый эталон",
        string corpusVersion = "v2026-08-26")
        => Require(FrozenReferenceCorpus.Build(
            "translation-en-ru",
            corpusVersion,
            FrozenAtUtc,
            new[]
            {
                CreateEntry(
                    "source-corpus",
                    FrozenReferencePartition.Train,
                    referenceAnswer: referenceAnswer)
            }));

    internal static FrozenReferenceEntry CreateEntry(
        string sourceSeed,
        FrozenReferencePartition partition,
        string? sourceFingerprint = null,
        string referenceAnswer = "устойчивый эталон",
        string domainTag = "translation",
        string subjectId = "en-ru")
    {
        var provenance = CreateProvenance(sourceSeed, referenceAnswer);

        return Require(FrozenReferenceEntry.Create(
            "переведи строку " + sourceSeed,
            referenceAnswer,
            provenance,
            domainTag,
            subjectId,
            partition,
            sourceFingerprint ?? SourceFingerprint(sourceSeed)));
    }

    internal static PaidReferenceProvenance CreateProvenance(
        string sourceSeed,
        string referenceAnswer)
    {
        var referenceHash =
            FrozenReferenceEntry.ComputeNormalizedTextSha256(referenceAnswer);
        return Require(PaidReferenceProvenance.Create(
            "paid-provider",
            "paid-model",
            "2026-08-26",
            "request-" + sourceSeed,
            CapturedAtUtc,
            referenceHash));
    }

    internal static string SourceFingerprint(string seed)
        => FrozenReferenceEntry.ComputeNormalizedTextSha256("source-entity:" + seed);

    internal static IEnumerable<T> ThrowOnMoveNext<T>(Exception exception)
        => new ThrowingEnumerable<T>(exception);

    internal static T Require<T>(AgentInfrastructure.OperationResult<T> result)
    {
        Assert.True(
            result.IsSuccess,
            $"Expected success but got {result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    private sealed class ThrowingEnumerable<T> : IEnumerable<T>
    {
        private readonly Exception exception;

        internal ThrowingEnumerable(Exception exception)
            => this.exception = exception;

        public IEnumerator<T> GetEnumerator()
            => new ThrowingEnumerator<T>(exception);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    private sealed class ThrowingEnumerator<T> : IEnumerator<T>
    {
        private readonly Exception exception;

        internal ThrowingEnumerator(Exception exception)
            => this.exception = exception;

        public T Current => default!;

        object? IEnumerator.Current => Current;

        public bool MoveNext()
            => throw exception;

        public void Reset()
            => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }
}

public sealed class ReferenceMatchingRuleCalibrationTests
{
    [Fact]
    public void ReferenceMatchingRuleCalibration_SeparatesFalseMissAndFalseHitMath()
    {
        var entry = FrozenReferenceCorpusTests.CreateEntry(
            "source-calibration",
            FrozenReferencePartition.Train,
            referenceAnswer: "эталон");
        var corpus = CreateCorpus(entry);
        var input = CreateCalibrationInput(
            corpus,
            positives: new[]
            {
                Pair("positive-exact", entry.EntryId, "эталон", "эталон"),
                Pair("positive-miss", entry.EntryId, "эталон с ошибкой", "эталон")
            },
            negatives: new[]
            {
                Pair("negative-clean", entry.EntryId, "чужой ответ", "эталон"),
                Pair("negative-hit", entry.EntryId, "эталон", "эталон")
            });

        var result = FrozenReferenceCorpusTests.Require(
            ReferenceMatchingRuleCalibrationEvaluator.Evaluate(input, ExactMatch));

        Assert.Equal(corpus.CorpusSha256, result.CorpusSha256);
        Assert.Equal(input.RuleIdentity, result.RuleIdentity);
        Assert.Equal(input.CalibrationSetSha256, result.CalibrationSetSha256);
        Assert.Equal(2, result.KnownPositiveCount);
        Assert.Equal(1, result.FalseMissCount);
        Assert.Equal(0.5D, result.FalseMissRate);
        Assert.Equal(new[] { "positive-miss" }, result.FalseMissPairIds);
        Assert.Equal(2, result.KnownNegativeCount);
        Assert.Equal(1, result.FalseHitCount);
        Assert.Equal(0.5D, result.FalseHitRate);
        Assert.Equal(new[] { "negative-hit" }, result.FalseHitPairIds);
    }

    [Fact]
    public void ReferenceMatchingCalibrationPair_CarriesReferenceEntryIdAndBindsToCorpusEntry()
    {
        var entry = FrozenReferenceCorpusTests.CreateEntry(
            "source-entry-binding",
            FrozenReferencePartition.Train,
            referenceAnswer: "эталон");
        var corpus = CreateCorpus(entry);
        var pair = Pair("positive", entry.EntryId, "эталон", "эталон");
        var input = FrozenReferenceCorpusTests.Require(
            ReferenceMatchingCalibrationInput.Create(
                corpus,
                CreateRule(),
                new[] { pair },
                new[] { Pair("negative", entry.EntryId, "чужой", "эталон") }));

        Assert.Equal(entry.EntryId, pair.ReferenceEntryId);
        Assert.Equal(
            entry.PaidReferenceProvenance.ReferenceAnswerSha256,
            pair.ReferenceAnswerSha256);
        Assert.Equal(entry.EntryId, input.KnownPositivePairs[0].ReferenceEntryId);
    }

    [Fact]
    public void ReferenceMatchingRuleCalibration_RejectsUnknownReferenceEntry()
    {
        var entry = FrozenReferenceCorpusTests.CreateEntry(
            "source-known-entry",
            FrozenReferencePartition.Train,
            referenceAnswer: "эталон");
        var corpus = CreateCorpus(entry);
        var unknownEntryId =
            FrozenReferenceEntry.ComputeNormalizedTextSha256("unknown-entry");

        var result = ReferenceMatchingCalibrationInput.Create(
            corpus,
            CreateRule(),
            new[] { Pair("positive", unknownEntryId, "эталон", "эталон") },
            new[] { Pair("negative", entry.EntryId, "чужой", "эталон") });

        Assert.False(result.IsSuccess);
        Assert.Equal("REFERENCE_MATCHING_CALIBRATION_ENTRY_UNKNOWN", result.ErrorCode);
    }

    [Fact]
    public void ReferenceMatchingRuleCalibration_RejectsTamperedReferenceText()
    {
        var entry = FrozenReferenceCorpusTests.CreateEntry(
            "source-tamper-entry",
            FrozenReferencePartition.Train,
            referenceAnswer: "исходный эталон");
        var corpus = CreateCorpus(entry);

        var result = ReferenceMatchingCalibrationInput.Create(
            corpus,
            CreateRule(),
            new[] { Pair("positive", entry.EntryId, "исходный эталон", "подменённый эталон") },
            new[] { Pair("negative", entry.EntryId, "чужой", "исходный эталон") });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "REFERENCE_MATCHING_CALIBRATION_REFERENCE_MISMATCH",
            result.ErrorCode);
    }

    [Fact]
    public void ReferenceMatchingRuleCalibration_IsDeterministicAcrossPairOrder()
    {
        var alpha = FrozenReferenceCorpusTests.CreateEntry(
            "source-alpha-calibration",
            FrozenReferencePartition.Train,
            referenceAnswer: "alpha");
        var beta = FrozenReferenceCorpusTests.CreateEntry(
            "source-beta-calibration",
            FrozenReferencePartition.HeldOut,
            referenceAnswer: "beta");
        var corpus = CreateCorpus(alpha, beta);
        var positiveA = Pair("positive-a", alpha.EntryId, "alpha", "alpha");
        var positiveB = Pair("positive-b", beta.EntryId, "beta", "beta");
        var negativeA = Pair("negative-a", alpha.EntryId, "wrong alpha", "alpha");
        var negativeB = Pair("negative-b", beta.EntryId, "wrong beta", "beta");
        var first = CreateCalibrationInput(
            corpus,
            new[] { positiveB, positiveA },
            new[] { negativeB, negativeA });
        var second = CreateCalibrationInput(
            corpus,
            new[] { positiveA, positiveB },
            new[] { negativeA, negativeB });

        var firstResult = FrozenReferenceCorpusTests.Require(
            ReferenceMatchingRuleCalibrationEvaluator.Evaluate(first, ExactMatch));
        var secondResult = FrozenReferenceCorpusTests.Require(
            ReferenceMatchingRuleCalibrationEvaluator.Evaluate(second, ExactMatch));

        Assert.Equal(first.CanonicalRepresentation, second.CanonicalRepresentation);
        Assert.Equal(first.CalibrationSetSha256, second.CalibrationSetSha256);
        Assert.Equal(firstResult.ResultSha256, secondResult.ResultSha256);
    }

    [Fact]
    public void ReferenceMatchingRuleCalibration_ResultHashBindsKnownPositiveAndNegativePairIds()
    {
        var entry = FrozenReferenceCorpusTests.CreateEntry(
            "source-membership-calibration",
            FrozenReferencePartition.Train,
            referenceAnswer: "answer");
        var corpus = CreateCorpus(entry);
        var first = CreateCalibrationInput(
            corpus,
            new[]
            {
                Pair("positive-alpha", entry.EntryId, "answer", "answer"),
                Pair("positive-beta", entry.EntryId, "answer", "answer"),
            },
            new[]
            {
                Pair("negative-alpha", entry.EntryId, "wrong one", "answer"),
                Pair("negative-beta", entry.EntryId, "wrong two", "answer"),
            });
        var second = CreateCalibrationInput(
            corpus,
            new[]
            {
                Pair("positive-gamma", entry.EntryId, "answer", "answer"),
                Pair("positive-theta", entry.EntryId, "answer", "answer"),
            },
            new[]
            {
                Pair("negative-gamma", entry.EntryId, "wrong one", "answer"),
                Pair("negative-theta", entry.EntryId, "wrong two", "answer"),
            });

        var firstResult = FrozenReferenceCorpusTests.Require(
            ReferenceMatchingRuleCalibrationEvaluator.Evaluate(first, ExactMatch));
        var secondResult = FrozenReferenceCorpusTests.Require(
            ReferenceMatchingRuleCalibrationEvaluator.Evaluate(second, ExactMatch));

        Assert.Equal(first.KnownPositivePairs.Count, second.KnownPositivePairs.Count);
        Assert.Equal(first.KnownNegativePairs.Count, second.KnownNegativePairs.Count);
        Assert.Equal(firstResult.FalseMissCount, secondResult.FalseMissCount);
        Assert.Equal(firstResult.FalseHitCount, secondResult.FalseHitCount);
        Assert.Empty(firstResult.FalseMissPairIds);
        Assert.Empty(firstResult.FalseHitPairIds);
        Assert.Equal(
            ExpectedCalibrationResultSha256V2(first, firstResult),
            firstResult.ResultSha256);
        Assert.Equal(
            ExpectedCalibrationResultSha256V2(second, secondResult),
            secondResult.ResultSha256);
        Assert.NotEqual(firstResult.ResultSha256, secondResult.ResultSha256);
    }

    [Fact]
    public void ReferenceMatchingRuleCalibration_RequiresExplicitPositiveAndNegativePairs()
    {
        var entry = FrozenReferenceCorpusTests.CreateEntry(
            "source-denominator",
            FrozenReferencePartition.Train,
            referenceAnswer: "right");
        var corpus = CreateCorpus(entry);
        var rule = CreateRule();

        var noPositive = ReferenceMatchingCalibrationInput.Create(
            corpus,
            rule,
            Array.Empty<ReferenceMatchingCalibrationPair>(),
            new[] { Pair("negative", entry.EntryId, "wrong", "right") });
        var noNegative = ReferenceMatchingCalibrationInput.Create(
            corpus,
            rule,
            new[] { Pair("positive", entry.EntryId, "right", "right") },
            Array.Empty<ReferenceMatchingCalibrationPair>());

        Assert.False(noPositive.IsSuccess);
        Assert.Equal("REFERENCE_MATCHING_POSITIVE_DENOMINATOR_INVALID", noPositive.ErrorCode);
        Assert.False(noNegative.IsSuccess);
        Assert.Equal("REFERENCE_MATCHING_NEGATIVE_DENOMINATOR_INVALID", noNegative.ErrorCode);
    }

    [Fact]
    public void ReferenceMatchingCalibrationInput_PropagatesCancellationFromCalibrationPairEnumerable()
    {
        var entry = FrozenReferenceCorpusTests.CreateEntry(
            "source-calibration-cancellation",
            FrozenReferencePartition.Train,
            referenceAnswer: "эталон");
        var corpus = CreateCorpus(entry);
        var cancellation = new OperationCanceledException("calibration snapshot cancelled");

        var thrown = Assert.Throws<OperationCanceledException>(() =>
            ReferenceMatchingCalibrationInput.Create(
                corpus,
                CreateRule(),
                FrozenReferenceCorpusTests.ThrowOnMoveNext<ReferenceMatchingCalibrationPair?>(
                    cancellation),
                new[] { Pair("negative", entry.EntryId, "чужой", "эталон") }));

        // Callers must be able to distinguish cancellation from bad calibration
        // data so shutdown does not get reported as a reusable failure result.
        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public void ReferenceMatchingRuleCalibration_PropagatesCancellationFromMatchingPredicate()
    {
        var entry = FrozenReferenceCorpusTests.CreateEntry(
            "source-predicate-cancellation",
            FrozenReferencePartition.Train,
            referenceAnswer: "эталон");
        var input = CreateCalibrationInput(
            CreateCorpus(entry),
            new[] { Pair("positive", entry.EntryId, "эталон", "эталон") },
            new[] { Pair("negative", entry.EntryId, "чужой", "эталон") });
        var cancellation = new OperationCanceledException("predicate cancelled");

        var thrown = Assert.Throws<OperationCanceledException>(() =>
            ReferenceMatchingRuleCalibrationEvaluator.Evaluate(
                input,
                (_, _) => throw cancellation));

        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public void ReferenceMatchingRuleCalibration_ChangesWhenCandidateOrReferencePairChanges()
    {
        var entry = FrozenReferenceCorpusTests.CreateEntry(
            "source-mutation",
            FrozenReferencePartition.Train,
            referenceAnswer: "эталон");
        var corpus = CreateCorpus(entry);
        var baseline = CreateCalibrationInput(
            corpus,
            new[] { Pair("positive", entry.EntryId, "эталон", "эталон") },
            new[] { Pair("negative", entry.EntryId, "чужой ответ", "эталон") });
        var changedCandidate = CreateCalibrationInput(
            corpus,
            new[] { Pair("positive", entry.EntryId, "эталон с ошибкой", "эталон") },
            new[] { Pair("negative", entry.EntryId, "чужой ответ", "эталон") });
        var changedReference = ReferenceMatchingCalibrationInput.Create(
            corpus,
            CreateRule(),
            new[] { Pair("positive", entry.EntryId, "эталон", "другой эталон") },
            new[] { Pair("negative", entry.EntryId, "чужой ответ", "эталон") });

        var baselineResult = FrozenReferenceCorpusTests.Require(
            ReferenceMatchingRuleCalibrationEvaluator.Evaluate(baseline, ExactMatch));
        var changedResult = FrozenReferenceCorpusTests.Require(
            ReferenceMatchingRuleCalibrationEvaluator.Evaluate(changedCandidate, ExactMatch));

        Assert.NotEqual(baseline.CalibrationSetSha256, changedCandidate.CalibrationSetSha256);
        Assert.NotEqual(baselineResult.ResultSha256, changedResult.ResultSha256);
        Assert.Equal(0, baselineResult.FalseMissCount);
        Assert.Equal(1, changedResult.FalseMissCount);
        Assert.False(changedReference.IsSuccess);
        Assert.Equal(
            "REFERENCE_MATCHING_CALIBRATION_REFERENCE_MISMATCH",
            changedReference.ErrorCode);
    }

    private static FrozenReferenceCorpus CreateCorpus(
        params FrozenReferenceEntry[] entries)
        => FrozenReferenceCorpusTests.Require(
            FrozenReferenceCorpus.Build(
                "translation-en-ru",
                "v2026-08-26",
                new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero),
                entries));

    private static ReferenceMatchingCalibrationInput CreateCalibrationInput(
        FrozenReferenceCorpus corpus,
        IReadOnlyList<ReferenceMatchingCalibrationPair> positives,
        IReadOnlyList<ReferenceMatchingCalibrationPair> negatives)
        => FrozenReferenceCorpusTests.Require(
            ReferenceMatchingCalibrationInput.Create(
                corpus,
                CreateRule(),
                positives,
                negatives));

    private static ReferenceMatchingRuleIdentity CreateRule()
        => FrozenReferenceCorpusTests.Require(ReferenceMatchingRuleIdentity.Create(
            "translation",
            "exact-normalized-answer",
            "v1",
            "Сравнивает ответ ученика с эталоном после той же нормализации строк; это калибровочный прибор, а не DPO-судья."));

    private static ReferenceMatchingCalibrationPair Pair(
        string pairId,
        string referenceEntryId,
        string candidate,
        string reference)
        => FrozenReferenceCorpusTests.Require(
            ReferenceMatchingCalibrationPair.Create(
                pairId,
                referenceEntryId,
                candidate,
                reference));

    private static bool ExactMatch(string candidate, string reference)
        => StringComparer.Ordinal.Equals(candidate, reference);

    private static string ExpectedCalibrationResultSha256V2(
        ReferenceMatchingCalibrationInput input,
        ReferenceMatchingRuleCalibrationResult result)
    {
        var canonical = new StringBuilder();
        AppendCanonical(
            canonical,
            "schema",
            "xiigen.reference-matching-calibration-result.v2");
        AppendCanonical(canonical, "corpus_sha256", result.CorpusSha256);
        AppendCanonical(
            canonical,
            "corpus_entry_count",
            result.CorpusEntryCount.ToString(CultureInfo.InvariantCulture));
        AppendCanonical(
            canonical,
            "rule_identity_sha256",
            result.RuleIdentity.IdentitySha256);
        AppendCanonical(
            canonical,
            "calibration_set_sha256",
            result.CalibrationSetSha256);
        AppendCanonical(
            canonical,
            "known_positive_count",
            result.KnownPositiveCount.ToString(CultureInfo.InvariantCulture));
        AppendIds(canonical, "known_positive", PairIds(input.KnownPositivePairs));
        AppendCanonical(
            canonical,
            "false_miss_count",
            result.FalseMissCount.ToString(CultureInfo.InvariantCulture));
        AppendCanonical(
            canonical,
            "false_miss_rate",
            result.FalseMissRate.ToString("R", CultureInfo.InvariantCulture));
        AppendIds(canonical, "false_miss", result.FalseMissPairIds);
        AppendCanonical(
            canonical,
            "known_negative_count",
            result.KnownNegativeCount.ToString(CultureInfo.InvariantCulture));
        AppendIds(canonical, "known_negative", PairIds(input.KnownNegativePairs));
        AppendCanonical(
            canonical,
            "false_hit_count",
            result.FalseHitCount.ToString(CultureInfo.InvariantCulture));
        AppendCanonical(
            canonical,
            "false_hit_rate",
            result.FalseHitRate.ToString("R", CultureInfo.InvariantCulture));
        AppendIds(canonical, "false_hit", result.FalseHitPairIds);
        return Sha256Ref(canonical.ToString());
    }

    private static IReadOnlyList<string> PairIds(
        IReadOnlyList<ReferenceMatchingCalibrationPair> pairs)
        => Array.AsReadOnly(
            pairs.Select(static pair => pair.PairId)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray());

    private static void AppendIds(
        StringBuilder canonical,
        string prefix,
        IReadOnlyList<string> ids)
    {
        AppendCanonical(
            canonical,
            prefix + ".count",
            ids.Count.ToString(CultureInfo.InvariantCulture));
        for (var index = 0; index < ids.Count; index++)
        {
            AppendCanonical(
                canonical,
                prefix + "." + index.ToString(CultureInfo.InvariantCulture),
                ids[index]);
        }
    }

    private static void AppendCanonical(
        StringBuilder canonical,
        string name,
        string? value)
    {
        canonical.Append(name.Length.ToString(CultureInfo.InvariantCulture))
            .Append(':')
            .Append(name)
            .Append('=');
        if (value is null)
        {
            canonical.Append("N;");
            return;
        }

        canonical.Append('V')
            .Append(value.Length.ToString(CultureInfo.InvariantCulture))
            .Append(':')
            .Append(value)
            .Append(';');
    }

    private static string Sha256Ref(string normalizedValue)
        => "sha256:" + Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(normalizedValue)))
            .ToLowerInvariant();
}
