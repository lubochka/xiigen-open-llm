// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ContinuousLearning;

public sealed class RepositoryOwnedReferenceMatchingCalibrationManifestTests
{
    private static readonly DateTimeOffset CapturedAtUtc =
        new(2026, 9, 8, 8, 15, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FrozenAtUtc =
        new(2026, 9, 8, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void PersistAndOpen_RoundTripsSeparateFalseMissAndFalseHitWithoutRawAnswers()
    {
        using var fixture = new RepositoryOwnedCalibrationFixture();
        var testCase = CreatePersistedCalibrationCase(fixture, "v0001");

        var manifest = Require(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer
                .PersistAndSeal(
                    testCase.CalibrationAddress,
                    testCase.Input,
                    testCase.Result));

        Assert.True(File.Exists(testCase.CalibrationAddress.ManifestPath));
        Assert.Equal(
            testCase.CalibrationAddress.CalibrationRootRelativePath,
            manifest.RepositoryRelativeCalibrationDirectory);
        Assert.Equal(
            testCase.CalibrationAddress.ManifestRelativePath,
            manifest.RepositoryRelativeManifestPath);
        Assert.Equal(
            testCase.CorpusAddress.ManifestRelativePath,
            manifest.RepositoryRelativeLinkedCorpusManifestPath);
        Assert.Equal(testCase.Corpus.CorpusSha256, manifest.CorpusSha256);
        Assert.Equal(testCase.Corpus.Count, manifest.CorpusEntryCount);
        Assert.Equal(testCase.RuleIdentity.DomainTag, manifest.RuleDomain);
        Assert.Equal(testCase.RuleIdentity.RuleId, manifest.RuleId);
        Assert.Equal(testCase.RuleIdentity.RuleVersion, manifest.RuleVersion);
        Assert.Equal(
            testCase.RuleIdentity.HumanDescription,
            manifest.RuleHumanDescription);
        Assert.Equal(
            testCase.RuleIdentity.IdentitySha256,
            manifest.RuleIdentitySha256);
        Assert.Equal(
            testCase.Input.CalibrationSetSha256,
            manifest.CalibrationSetSha256);
        Assert.Equal(testCase.Result.ResultSha256, manifest.ResultSha256);
        Assert.Equal(2, manifest.KnownPositiveCount);
        Assert.Equal(
            new[] { "positive-match", "positive-miss" },
            manifest.KnownPositivePairIds);
        Assert.Equal(2, manifest.KnownNegativeCount);
        Assert.Equal(
            new[] { "negative-hit", "negative-ok" },
            manifest.KnownNegativePairIds);
        Assert.Equal(1, manifest.FalseMissCount);
        Assert.Equal(0.5D, manifest.FalseMissRate);
        Assert.Equal(new[] { "positive-miss" }, manifest.FalseMissPairIds);
        Assert.Equal(1, manifest.FalseHitCount);
        Assert.Equal(0.5D, manifest.FalseHitRate);
        Assert.Equal(new[] { "negative-hit" }, manifest.FalseHitPairIds);

        var rawManifest = File.ReadAllText(testCase.CalibrationAddress.ManifestPath);
        Assert.DoesNotContain("raw-reference-answer-one", rawManifest);
        Assert.DoesNotContain("raw-reference-answer-two", rawManifest);
        Assert.DoesNotContain("raw-candidate-miss", rawManifest);
        Assert.DoesNotContain("raw-candidate-negative-ok", rawManifest);

        var opened = Require(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer.OpenSealed(
                testCase.CalibrationAddress));

        Assert.Equal(manifest.ManifestSha256, opened.Manifest.ManifestSha256);
        Assert.Equal(manifest.ResultSha256, opened.Manifest.ResultSha256);
        Assert.Equal(
            new[] { "positive-miss" },
            opened.Manifest.FalseMissPairIds);
        Assert.Equal(
            new[] { "negative-hit" },
            opened.Manifest.FalseHitPairIds);
        Assert.Equal(
            testCase.Corpus.CorpusSha256,
            opened.LinkedCorpus.Corpus.CorpusSha256);
    }

    [Fact]
    public void PersistAndSeal_SameAddressIsIdempotentButChangedResultConflicts()
    {
        using var fixture = new RepositoryOwnedCalibrationFixture();
        var testCase = CreatePersistedCalibrationCase(fixture, "v0002");

        var first = Require(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer.Seal(
                testCase.CalibrationAddress,
                testCase.Input,
                testCase.Result));
        var second = Require(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer.Seal(
                testCase.CalibrationAddress,
                testCase.Input,
                testCase.Result));
        var changedValidResult = Require(
            ReferenceMatchingRuleCalibrationEvaluator.Evaluate(
                testCase.Input,
                static (_, _) => true));

        var conflict =
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer
                .PersistAndSeal(
                    testCase.CalibrationAddress,
                    testCase.Input,
                    changedValidResult);
        var reopened = Require(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer.Open(
                testCase.CalibrationAddress));

        Assert.Equal(first.ManifestSha256, second.ManifestSha256);
        Assert.False(conflict.IsSuccess);
        Assert.Equal(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer
                .ConflictErrorCode,
            conflict.ErrorCode);
        Assert.Equal(first.ManifestSha256, reopened.Manifest.ManifestSha256);
        Assert.Equal(testCase.Result.ResultSha256, reopened.Manifest.ResultSha256);
        Assert.NotEqual(
            changedValidResult.ResultSha256,
            reopened.Manifest.ResultSha256);
    }

    [Fact]
    public void EvaluatePersistAndSeal_ComputesAddressAndStoresRepositoryManifest()
    {
        using var fixture = new RepositoryOwnedCalibrationFixture();
        var testCase = CreatePersistedCalibrationCase(fixture, "v0006");

        var manifest = Require(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer
                .EvaluatePersistAndSeal(
                    fixture.RepositoryRoot,
                    "en-ru",
                    testCase.Input,
                    ExactReferenceMatch));
        var reopened = Require(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer
                .OpenSealed(testCase.CalibrationAddress));

        Assert.True(File.Exists(testCase.CalibrationAddress.ManifestPath));
        Assert.Equal(
            testCase.CalibrationAddress.ManifestRelativePath,
            manifest.RepositoryRelativeManifestPath);
        Assert.Equal(testCase.Result.ResultSha256, manifest.ResultSha256);
        Assert.Equal(manifest.ManifestSha256, reopened.Manifest.ManifestSha256);
        Assert.Equal(
            new[] { "positive-miss" },
            reopened.Manifest.FalseMissPairIds);
        Assert.Equal(
            new[] { "negative-hit" },
            reopened.Manifest.FalseHitPairIds);
    }

    [Fact]
    public void EvaluatePersistAndSeal_RejectsWrongSubjectWithoutParallelManifest()
    {
        using var fixture = new RepositoryOwnedCalibrationFixture();
        var testCase = CreatePersistedCalibrationCase(fixture, "v0007");
        var predicateCalls = 0;
        var wrongAddress = Require(RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceMatchingCalibrationAddress(
                fixture.RepositoryRoot,
                "translation",
                "wrong-subject",
                testCase.Corpus.CorpusVersion,
                testCase.RuleIdentity.RuleId,
                testCase.RuleIdentity.IdentitySha256,
                testCase.Input.CalibrationSetSha256));

        var result = RepositoryOwnedReferenceMatchingCalibrationManifestSealer
            .EvaluatePersistAndSeal(
                fixture.RepositoryRoot,
                "wrong-subject",
                testCase.Input,
                (candidate, reference) =>
                {
                    predicateCalls++;
                    return ExactReferenceMatch(candidate, reference);
                });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer
                .InvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(0, predicateCalls);
        Assert.False(File.Exists(wrongAddress.ManifestPath));
        Assert.False(Directory.Exists(wrongAddress.CalibrationRoot));
    }

    [Fact]
    public void EvaluatePersistAndSeal_RejectsMissingLinkedCorpusBeforePredicate()
    {
        using var fixture = new RepositoryOwnedCalibrationFixture();
        var testCase = CreatePersistedCalibrationCase(fixture, "v0009");
        var predicateCalls = 0;
        File.Delete(testCase.CorpusAddress.ManifestPath);

        var result = RepositoryOwnedReferenceMatchingCalibrationManifestSealer
            .EvaluatePersistAndSeal(
                fixture.RepositoryRoot,
                "en-ru",
                testCase.Input,
                (candidate, reference) =>
                {
                    predicateCalls++;
                    return ExactReferenceMatch(candidate, reference);
                });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer
                .InvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(0, predicateCalls);
        Assert.False(File.Exists(testCase.CalibrationAddress.ManifestPath));
        Assert.False(Directory.Exists(testCase.CalibrationAddress.CalibrationRoot));
    }

    [Fact]
    public void EvaluatePersistAndSeal_StopsBeforeWriteWhenPredicateFails()
    {
        using var fixture = new RepositoryOwnedCalibrationFixture();
        var testCase = CreatePersistedCalibrationCase(fixture, "v0008");

        var result = RepositoryOwnedReferenceMatchingCalibrationManifestSealer
            .EvaluatePersistAndSeal(
                fixture.RepositoryRoot,
                "en-ru",
                testCase.Input,
                static (_, _) => throw new InvalidOperationException(
                    "predicate failed before manifest write"));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "REFERENCE_MATCHING_RULE_PREDICATE_FAILED",
            result.ErrorCode);
        Assert.False(File.Exists(testCase.CalibrationAddress.ManifestPath));
    }

    [Theory]
    [InlineData(CalibrationManifestTamperKind.ManifestSelfHash)]
    [InlineData(CalibrationManifestTamperKind.LinkedCorpusManifestPathEscape)]
    [InlineData(CalibrationManifestTamperKind.CorpusHash)]
    [InlineData(CalibrationManifestTamperKind.CorpusEntryCount)]
    [InlineData(CalibrationManifestTamperKind.RuleIdentity)]
    [InlineData(CalibrationManifestTamperKind.RuleVersion)]
    [InlineData(CalibrationManifestTamperKind.RuleDescriptionTrailingWhitespace)]
    [InlineData(CalibrationManifestTamperKind.KnownPositiveCount)]
    [InlineData(CalibrationManifestTamperKind.KnownPositivePairIdsSubstitution)]
    [InlineData(CalibrationManifestTamperKind.KnownNegativeCount)]
    [InlineData(CalibrationManifestTamperKind.KnownNegativePairIdsSubstitution)]
    [InlineData(CalibrationManifestTamperKind.KnownPairCrossSetIntersection)]
    [InlineData(CalibrationManifestTamperKind.FalseMissRate)]
    [InlineData(CalibrationManifestTamperKind.FalseMissIdList)]
    [InlineData(CalibrationManifestTamperKind.FalseHitCount)]
    [InlineData(CalibrationManifestTamperKind.FalseHitRate)]
    [InlineData(CalibrationManifestTamperKind.FalseHitIdList)]
    [InlineData(CalibrationManifestTamperKind.ResultHash)]
    public void OpenSealed_FailsClosedWhenManifestIsTampered(
        CalibrationManifestTamperKind tamperKind)
    {
        using var fixture = new RepositoryOwnedCalibrationFixture();
        var testCase = CreatePersistedCalibrationCase(fixture, "v0003");
        var manifest = Require(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer
                .PersistAndSeal(
                    testCase.CalibrationAddress,
                    testCase.Input,
                    testCase.Result));

        ApplyManifestTamper(testCase.CalibrationAddress, manifest, tamperKind);

        var opened =
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer.OpenSealed(
                testCase.CalibrationAddress);

        Assert.False(opened.IsSuccess);
        Assert.Equal(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer
                .InvalidErrorCode,
            opened.ErrorCode);
    }

    [Fact]
    public void OpenSealed_FailsClosedWhenCalibrationAddressEscapesExpectedManifestPath()
    {
        using var fixture = new RepositoryOwnedCalibrationFixture();
        var testCase = CreatePersistedCalibrationCase(fixture, "v0005");
        Require(RepositoryOwnedReferenceMatchingCalibrationManifestSealer
            .PersistAndSeal(
                testCase.CalibrationAddress,
                testCase.Input,
                testCase.Result));
        var escapedAddress = FabricateReferenceMatchingCalibrationAddress(
            testCase.CalibrationAddress,
            manifestPath: testCase.CorpusAddress.ManifestPath,
            manifestRelativePath: testCase.CorpusAddress.ManifestRelativePath);

        var opened =
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer.OpenSealed(
                escapedAddress);

        Assert.False(opened.IsSuccess);
        Assert.Equal(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer
                .InvalidErrorCode,
            opened.ErrorCode);
    }

    [Theory]
    [InlineData(LinkedCorpusTamperKind.MissingManifest)]
    [InlineData(LinkedCorpusTamperKind.PayloadByte)]
    public void OpenSealed_FailsClosedWhenLinkedSealedCorpusIsMissingOrTampered(
        LinkedCorpusTamperKind tamperKind)
    {
        using var fixture = new RepositoryOwnedCalibrationFixture();
        var testCase = CreatePersistedCalibrationCase(fixture, "v0004");
        Require(RepositoryOwnedReferenceMatchingCalibrationManifestSealer
            .PersistAndSeal(
                testCase.CalibrationAddress,
                testCase.Input,
                testCase.Result));

        ApplyLinkedCorpusTamper(testCase.CorpusAddress, tamperKind);

        var opened =
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer.OpenSealed(
                testCase.CalibrationAddress);

        Assert.False(opened.IsSuccess);
        Assert.Equal(
            RepositoryOwnedReferenceMatchingCalibrationManifestSealer
                .InvalidErrorCode,
            opened.ErrorCode);
    }

    private static CalibrationCase CreatePersistedCalibrationCase(
        RepositoryOwnedCalibrationFixture fixture,
        string corpusVersion)
    {
        var corpus = CreateCorpus(corpusVersion);
        var corpusAddress = Require(RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceCorpusAddress(
                fixture.RepositoryRoot,
                "translation",
                "en-ru",
                corpusVersion));
        Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(
            corpusAddress,
            corpus));

        var ruleIdentity = Require(ReferenceMatchingRuleIdentity.Create(
            "translation",
            "exact-reference-match",
            "v0001",
            "Exact string reference matching calibration rule."));
        var positivePairs = new[]
        {
            Pair(
                "positive-match",
                corpus.Entries[0],
                corpus.Entries[0].PaidModelReferenceAnswer),
            Pair(
                "positive-miss",
                corpus.Entries[1],
                "raw-candidate-miss"),
        };
        var negativePairs = new[]
        {
            Pair(
                "negative-ok",
                corpus.Entries[0],
                "raw-candidate-negative-ok"),
            Pair(
                "negative-hit",
                corpus.Entries[1],
                corpus.Entries[1].PaidModelReferenceAnswer),
        };
        var input = Require(ReferenceMatchingCalibrationInput.Create(
            corpus,
            ruleIdentity,
            positivePairs,
            negativePairs));
        var result = Require(ReferenceMatchingRuleCalibrationEvaluator.Evaluate(
            input,
            ExactReferenceMatch));
        var calibrationAddress = Require(RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceMatchingCalibrationAddress(
                fixture.RepositoryRoot,
                "translation",
                "en-ru",
                corpusVersion,
                ruleIdentity.RuleId,
                ruleIdentity.IdentitySha256,
                input.CalibrationSetSha256));

        return new CalibrationCase(
            corpusAddress,
            calibrationAddress,
            corpus,
            ruleIdentity,
            input,
            result);
    }

    private static FrozenReferenceCorpus CreateCorpus(string corpusVersion)
        => Require(FrozenReferenceCorpus.Build(
            "translation-en-ru",
            corpusVersion,
            FrozenAtUtc,
            new[]
            {
                CreateEntry(
                    "source-one-" + corpusVersion,
                    FrozenReferencePartition.Train,
                    "raw-reference-answer-one"),
                CreateEntry(
                    "source-two-" + corpusVersion,
                    FrozenReferencePartition.HeldOut,
                    "raw-reference-answer-two"),
            }));

    private static FrozenReferenceEntry CreateEntry(
        string sourceSeed,
        FrozenReferencePartition partition,
        string referenceAnswer)
    {
        var provenance = Require(PaidReferenceProvenance.Create(
            "paid-provider",
            "paid-model",
            "2026-09-08",
            "request-" + sourceSeed,
            CapturedAtUtc,
            FrozenReferenceEntry.ComputeNormalizedTextSha256(referenceAnswer)));
        return Require(FrozenReferenceEntry.Create(
            "owner input " + sourceSeed,
            referenceAnswer,
            provenance,
            "translation",
            "en-ru",
            partition,
            FrozenReferenceEntry.ComputeNormalizedTextSha256(
                "source-entity:" + sourceSeed)));
    }

    private static ReferenceMatchingCalibrationPair Pair(
        string pairId,
        FrozenReferenceEntry entry,
        string candidateAnswer)
        => Require(ReferenceMatchingCalibrationPair.Create(
            pairId,
            entry.EntryId,
            candidateAnswer,
            entry.PaidModelReferenceAnswer));

    private static bool ExactReferenceMatch(string candidate, string reference)
        => string.Equals(candidate, reference, StringComparison.Ordinal);

    private static void ApplyManifestTamper(
        RepositoryOwnedReferenceMatchingCalibrationAddress address,
        RepositoryOwnedReferenceMatchingCalibrationManifest manifest,
        CalibrationManifestTamperKind tamperKind)
    {
        switch (tamperKind)
        {
            case CalibrationManifestTamperKind.ManifestSelfHash:
                ReplaceJsonString(
                    address.ManifestPath,
                    "manifestSha256",
                    new string('0', 64));
                return;
            case CalibrationManifestTamperKind.LinkedCorpusManifestPathEscape:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonStringInText(
                        text,
                        "repositoryRelativeLinkedCorpusManifestPath",
                        "../escape/manifest.json"));
                return;
            case CalibrationManifestTamperKind.CorpusHash:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonStringInText(
                        text,
                        "corpusSha256",
                        FrozenReferenceEntry.ComputeNormalizedTextSha256(
                            "tampered-corpus")));
                return;
            case CalibrationManifestTamperKind.CorpusEntryCount:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonNumberInText(
                        text,
                        "corpusEntryCount",
                        "3"));
                return;
            case CalibrationManifestTamperKind.RuleIdentity:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonStringInText(
                        text,
                        "ruleIdentitySha256",
                        FrozenReferenceEntry.ComputeNormalizedTextSha256(
                            "tampered-rule")));
                return;
            case CalibrationManifestTamperKind.RuleVersion:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonStringInText(
                        text,
                        "ruleVersion",
                        "v9999"));
                return;
            case CalibrationManifestTamperKind.RuleDescriptionTrailingWhitespace:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonStringInText(
                        text,
                        "ruleHumanDescription",
                        manifest.RuleHumanDescription + " "));
                return;
            case CalibrationManifestTamperKind.KnownPositiveCount:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonNumberInText(
                        text,
                        "knownPositiveCount",
                        "3"));
                return;
            case CalibrationManifestTamperKind.KnownPositivePairIdsSubstitution:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceStringInArrayInText(
                        text,
                        "knownPositivePairIds",
                        "positive-match",
                        "positive-made-up"));
                return;
            case CalibrationManifestTamperKind.KnownNegativeCount:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonNumberInText(
                        text,
                        "knownNegativeCount",
                        "3"));
                return;
            case CalibrationManifestTamperKind.KnownNegativePairIdsSubstitution:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceStringInArrayInText(
                        text,
                        "knownNegativePairIds",
                        "negative-ok",
                        "negative-oz"));
                return;
            case CalibrationManifestTamperKind.KnownPairCrossSetIntersection:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceStringInArrayInText(
                        text,
                        "knownNegativePairIds",
                        "negative-ok",
                        "positive-match"));
                return;
            case CalibrationManifestTamperKind.FalseMissRate:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonNumberInText(
                        text,
                        "falseMissRate",
                        "0.25"));
                return;
            case CalibrationManifestTamperKind.FalseMissIdList:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceStringInArrayInText(
                        text,
                        "falseMissPairIds",
                        "positive-miss",
                        "negative-ok"));
                return;
            case CalibrationManifestTamperKind.FalseHitCount:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonNumberInText(
                        text,
                        "falseHitCount",
                        "2"));
                return;
            case CalibrationManifestTamperKind.FalseHitRate:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonNumberInText(
                        text,
                        "falseHitRate",
                        "0.25"));
                return;
            case CalibrationManifestTamperKind.FalseHitIdList:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceStringInArrayInText(
                        text,
                        "falseHitPairIds",
                        "negative-hit",
                        "positive-match"));
                return;
            case CalibrationManifestTamperKind.ResultHash:
                RewriteManifestWithSelfHash(
                    address,
                    manifest,
                    text => ReplaceJsonStringInText(
                        text,
                        "resultSha256",
                        FrozenReferenceEntry.ComputeNormalizedTextSha256(
                            "tampered-result")));
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(tamperKind));
        }
    }

    private static void ApplyLinkedCorpusTamper(
        RepositoryOwnedReferenceCorpusAddress corpusAddress,
        LinkedCorpusTamperKind tamperKind)
    {
        switch (tamperKind)
        {
            case LinkedCorpusTamperKind.MissingManifest:
                File.Delete(corpusAddress.ManifestPath);
                return;
            case LinkedCorpusTamperKind.PayloadByte:
                var payloadPath = Path.Combine(
                    corpusAddress.CorpusRoot,
                    RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
                var payload = File.ReadAllBytes(payloadPath);
                payload[^2] = payload[^2] == (byte)'0' ? (byte)'1' : (byte)'0';
                File.WriteAllBytes(payloadPath, payload);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(tamperKind));
        }
    }

    private static RepositoryOwnedReferenceMatchingCalibrationAddress
        FabricateReferenceMatchingCalibrationAddress(
            RepositoryOwnedReferenceMatchingCalibrationAddress source,
            string? repositoryRoot = null,
            string? referenceCorpusArtifactRoot = null,
            string? domainSlug = null,
            string? subjectSlug = null,
            string? corpusVersion = null,
            string? ruleIdSlug = null,
            string? ruleIdentitySha256 = null,
            string? ruleIdentitySha256Segment = null,
            string? calibrationSetSha256 = null,
            string? calibrationSetSha256Segment = null,
            string? calibrationRoot = null,
            string? manifestPath = null,
            string? calibrationRootRelativePath = null,
            string? manifestRelativePath = null)
        => new(
            repositoryRoot ?? source.RepositoryRoot,
            referenceCorpusArtifactRoot ?? source.ReferenceCorpusArtifactRoot,
            domainSlug ?? source.DomainSlug,
            subjectSlug ?? source.SubjectSlug,
            corpusVersion ?? source.CorpusVersion,
            ruleIdSlug ?? source.RuleIdSlug,
            ruleIdentitySha256 ?? source.RuleIdentitySha256,
            ruleIdentitySha256Segment ?? source.RuleIdentitySha256Segment,
            calibrationSetSha256 ?? source.CalibrationSetSha256,
            calibrationSetSha256Segment ?? source.CalibrationSetSha256Segment,
            calibrationRoot ?? source.CalibrationRoot,
            manifestPath ?? source.ManifestPath,
            calibrationRootRelativePath ?? source.CalibrationRootRelativePath,
            manifestRelativePath ?? source.ManifestRelativePath);

    private static void RewriteManifestWithSelfHash(
        RepositoryOwnedReferenceMatchingCalibrationAddress address,
        RepositoryOwnedReferenceMatchingCalibrationManifest manifest,
        Func<string, string> rewrite)
    {
        var rewritten = rewrite(File.ReadAllText(address.ManifestPath));
        var unsigned = rewritten.Replace(
            manifest.ManifestSha256,
            string.Empty,
            StringComparison.Ordinal);
        var selfHash = Sha256(Encoding.UTF8.GetBytes(unsigned));
        File.WriteAllText(
            address.ManifestPath,
            unsigned.Replace(
                "\"manifestSha256\":\"\"",
                $"\"manifestSha256\":\"{selfHash}\"",
                StringComparison.Ordinal),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void ReplaceJsonString(
        string path,
        string propertyName,
        string replacement)
        => File.WriteAllText(
            path,
            ReplaceJsonStringInText(
                File.ReadAllText(path),
                propertyName,
                replacement),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

    private static string ReplaceJsonStringInText(
        string text,
        string propertyName,
        string replacement)
    {
        var marker = "\"" + propertyName + "\":\"";
        var start = text.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0, "manifest string property not found: " + propertyName);
        start += marker.Length;
        var end = text.IndexOf('"', start);
        Assert.True(end > start, "manifest string property end not found: " + propertyName);
        return text[..start] + replacement + text[end..];
    }

    private static string ReplaceJsonNumberInText(
        string text,
        string propertyName,
        string replacement)
    {
        var marker = "\"" + propertyName + "\":";
        var start = text.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0, "manifest number property not found: " + propertyName);
        start += marker.Length;
        var end = start;
        while (end < text.Length
               && (char.IsDigit(text[end])
                   || text[end] is '.' or '-' or '+' or 'e' or 'E'))
        {
            end++;
        }

        Assert.True(end > start, "manifest number property end not found: " + propertyName);
        return text[..start] + replacement + text[end..];
    }

    private static string ReplaceStringInArrayInText(
        string text,
        string propertyName,
        string oldValue,
        string newValue)
    {
        var arrayMarker = "\"" + propertyName + "\":[";
        var arrayStart = text.IndexOf(arrayMarker, StringComparison.Ordinal);
        Assert.True(arrayStart >= 0, "manifest array property not found: " + propertyName);
        var arrayEnd = text.IndexOf(']', arrayStart);
        Assert.True(arrayEnd > arrayStart, "manifest array end not found: " + propertyName);
        var oldMarker = "\"" + oldValue + "\"";
        var valueStart = text.IndexOf(oldMarker, arrayStart, arrayEnd - arrayStart);
        Assert.True(valueStart >= 0, "manifest array value not found: " + oldValue);
        return text[..valueStart] + "\"" + newValue + "\"" +
               text[(valueStart + oldMarker.Length)..];
    }

    private static string Sha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static T Require<T>(AgentInfrastructure.OperationResult<T> result)
    {
        Assert.True(
            result.IsSuccess,
            $"Expected success but got {result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    public enum CalibrationManifestTamperKind
    {
        ManifestSelfHash,
        LinkedCorpusManifestPathEscape,
        CorpusHash,
        CorpusEntryCount,
        RuleIdentity,
        RuleVersion,
        RuleDescriptionTrailingWhitespace,
        KnownPositiveCount,
        KnownPositivePairIdsSubstitution,
        KnownNegativeCount,
        KnownNegativePairIdsSubstitution,
        KnownPairCrossSetIntersection,
        FalseMissRate,
        FalseMissIdList,
        FalseHitCount,
        FalseHitRate,
        FalseHitIdList,
        ResultHash,
    }

    public enum LinkedCorpusTamperKind
    {
        MissingManifest,
        PayloadByte,
    }

    private sealed record CalibrationCase(
        RepositoryOwnedReferenceCorpusAddress CorpusAddress,
        RepositoryOwnedReferenceMatchingCalibrationAddress CalibrationAddress,
        FrozenReferenceCorpus Corpus,
        ReferenceMatchingRuleIdentity RuleIdentity,
        ReferenceMatchingCalibrationInput Input,
        ReferenceMatchingRuleCalibrationResult Result);

    private sealed class RepositoryOwnedCalibrationFixture : IDisposable
    {
        internal RepositoryOwnedCalibrationFixture()
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                $"repo-owned-reference-matching-calibration-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Root);
            RepositoryRoot = Path.Combine(Root, "repo");
            Directory.CreateDirectory(RepositoryRoot);
            File.WriteAllText(
                Path.Combine(RepositoryRoot, "LlmFromScratch.sln"),
                "fixture solution sentinel");
            Directory.CreateDirectory(Path.Combine(
                RepositoryRoot,
                "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(RepositoryRoot, "11.DPO"));
        }

        internal string Root { get; }
        internal string RepositoryRoot { get; }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
