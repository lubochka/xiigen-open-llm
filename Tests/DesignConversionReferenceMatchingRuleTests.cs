// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;

namespace LlmFromScratch.Tests;

public sealed class DesignConversionReferenceMatchingRuleTests
{
    private static readonly DateTimeOffset CapturedAtUtc =
        new(2026, 9, 8, 11, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FrozenAtUtc =
        new(2026, 9, 8, 11, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Identity_BindsDesignToCodeLocalPngAndFrozenReferenceLinkPolicy()
    {
        var first = Require(DesignConversionReferenceMatchingRule.CreateIdentity());
        var second = Require(DesignConversionReferenceMatchingRule.CreateIdentity());

        Assert.Equal(first.IdentitySha256, second.IdentitySha256);
        Assert.Equal(
            DesignConversionReferenceMatchingRule.DomainTag,
            first.DomainTag);
        Assert.Equal(
            DesignConversionReferenceMatchingRule.RuleId,
            first.RuleId);
        Assert.Equal(
            "design-to-code",
            DesignConversionReferenceMatchingRule.PatternId);
        Assert.Contains("pattern-design-to-code", first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("bind-img-id-sha", first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("markup-fingerprint", first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("trust-local-png-only", first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("arbitrary JSON is not server-authenticated", first.HumanDescription, StringComparison.Ordinal);
        Assert.Contains("not DPO", first.HumanDescription, StringComparison.Ordinal);
        Assert.DoesNotContain("figma-to-html-css", first.RuleVersion, StringComparison.Ordinal);
        Assert.DoesNotContain("external-receipt", first.RuleVersion, StringComparison.Ordinal);
    }

    [Fact]
    public void IsMatch_UsesCandidateComparisonAgainstFrozenExpectedReferenceNotIndependentSelfScores()
    {
        var reference = Payload(
            DesignConversionReferenceMatchingRule.PaidReferenceRole,
            expectedImageId: "image:checkout-reference",
            expectedSha: Sha('a'),
            actualImageId: "image:paid-output",
            actualSha: Sha('b'),
            expectedMarkupFingerprint: Sha('1'));
        var candidate = Payload(
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            expectedImageId: "image:checkout-reference",
            expectedSha: Sha('a'),
            actualImageId: "image:candidate-output",
            actualSha: Sha('c'),
            expectedMarkupFingerprint: Sha('1'));
        var foreignReference = Payload(
            DesignConversionReferenceMatchingRule.PaidReferenceRole,
            expectedImageId: "image:settings-reference",
            expectedSha: Sha('d'),
            actualImageId: "image:paid-settings-output",
            actualSha: Sha('e'),
            expectedMarkupFingerprint: Sha('1'));

        Assert.True(DesignConversionReferenceMatchingRule.IsMatch(candidate, reference));

        var mismatch = Require(DesignConversionReferenceMatchingRule.EvaluateMatch(
            candidate,
            foreignReference));
        Assert.False(mismatch.IsMatch);
        Assert.Equal(
            DesignConversionReferenceMatchingRule.ExpectedReferenceMismatchStatus,
            mismatch.Status);
        Assert.True(mismatch.CandidateVisualAxis.Passed);
        Assert.True(mismatch.ReferenceVisualAxis.Passed);
        Assert.True(mismatch.CandidateMarkupAxis.Passed);
        Assert.True(mismatch.ReferenceMarkupAxis.Passed);
    }

    [Fact]
    public void IsMatch_CandidateExpectedShaMutationFailsEvenWhenScoresStayAboveThreshold()
    {
        var reference = Payload(
            DesignConversionReferenceMatchingRule.PaidReferenceRole,
            expectedImageId: "image:hero-reference",
            expectedSha: Sha('1'),
            actualImageId: "image:paid-hero-output",
            actualSha: Sha('2'),
            expectedMarkupFingerprint: Sha('a'));
        var sameExpectedIdWrongSha = Payload(
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            expectedImageId: "image:hero-reference",
            expectedSha: Sha('3'),
            actualImageId: "image:candidate-hero-output",
            actualSha: Sha('4'),
            expectedMarkupFingerprint: Sha('a'));

        var result = Require(DesignConversionReferenceMatchingRule.EvaluateMatch(
            sameExpectedIdWrongSha,
            reference));

        Assert.False(result.IsMatch);
        Assert.Equal(
            DesignConversionReferenceMatchingRule.ExpectedReferenceMismatchStatus,
            result.Status);
    }

    [Fact]
    public void IsMatch_CandidateMarkupFingerprintMutationFailsAfterVisualAndScorePass()
    {
        var reference = Payload(
            DesignConversionReferenceMatchingRule.PaidReferenceRole,
            expectedImageId: "image:card-reference",
            expectedSha: Sha('1'),
            actualImageId: "image:paid-card-output",
            actualSha: Sha('2'),
            expectedMarkupFingerprint: Sha('a'));
        var candidateWrongMarkupReference = Payload(
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            expectedImageId: "image:card-reference",
            expectedSha: Sha('1'),
            actualImageId: "image:candidate-card-output",
            actualSha: Sha('3'),
            expectedMarkupFingerprint: Sha('b'));

        var result = Require(DesignConversionReferenceMatchingRule.EvaluateMatch(
            candidateWrongMarkupReference,
            reference));

        Assert.False(result.IsMatch);
        Assert.Equal(
            DesignConversionReferenceMatchingRule.ExpectedMarkupMismatchStatus,
            result.Status);
        Assert.True(result.CandidateVisualAxis.Passed);
        Assert.True(result.ReferenceVisualAxis.Passed);
        Assert.True(result.CandidateMarkupAxis.Passed);
        Assert.True(result.ReferenceMarkupAxis.Passed);
    }

    [Fact]
    public void PayloadCreate_SelfVisualComparisonByIdOrShaFailsClosed()
    {
        var sameImageId = DesignConversionReferenceEvidencePayload.Create(
            DesignConversionReferenceMatchingRule.DomainTag,
            DesignConversionReferenceMatchingRule.SubjectId,
            DesignConversionReferenceMatchingRule.PatternId,
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            VisualAxis(
                expectedImageId: "image:self",
                expectedSha: Sha('1'),
                actualImageId: "image:self",
                actualSha: Sha('2'),
                diffSha: Sha('3')),
            MarkupAxis(Sha('a')));
        var sameImageSha = DesignConversionReferenceEvidencePayload.Create(
            DesignConversionReferenceMatchingRule.DomainTag,
            DesignConversionReferenceMatchingRule.SubjectId,
            DesignConversionReferenceMatchingRule.PatternId,
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            VisualAxis(
                expectedImageId: "image:expected",
                expectedSha: Sha('4'),
                actualImageId: "image:actual",
                actualSha: Sha('4'),
                diffSha: Sha('5')),
            MarkupAxis(Sha('a')));

        Assert.False(sameImageId.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_REFERENCE_VISUAL_SELF_COMPARISON_INVALID", sameImageId.ErrorCode);
        Assert.False(sameImageSha.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_REFERENCE_VISUAL_SELF_HASH_INVALID", sameImageSha.ErrorCode);
    }

    [Fact]
    public void IsMatch_CandidateScoreMutationsFailClosedForVisualAndMarkupAxes()
    {
        var reference = Payload(
            DesignConversionReferenceMatchingRule.PaidReferenceRole,
            expectedImageId: "image:dashboard-reference",
            expectedSha: Sha('5'),
            actualImageId: "image:paid-dashboard-output",
            actualSha: Sha('6'));

        Assert.Equal(
            "visual_score_below_threshold",
            Require(DesignConversionReferenceMatchingRule.EvaluateMatch(
                Payload(
                    DesignConversionReferenceMatchingRule.StudentCandidateRole,
                    expectedImageId: "image:dashboard-reference",
                    expectedSha: Sha('5'),
                    actualImageId: "image:candidate-dashboard-output",
                    actualSha: Sha('7'),
                    visualScore: 0.939D),
                reference)).Status);
        Assert.Equal(
            DesignConversionReferenceMatchingRule.NodeTypeCompositionBelowThresholdStatus,
            Require(DesignConversionReferenceMatchingRule.EvaluateMatch(
                Payload(
                    DesignConversionReferenceMatchingRule.StudentCandidateRole,
                    expectedImageId: "image:dashboard-reference",
                    expectedSha: Sha('5'),
                    actualImageId: "image:candidate-dashboard-output",
                    actualSha: Sha('7'),
                    nodeTypeCompositionScore: 0.899D),
                reference)).Status);
        Assert.Equal(
            DesignConversionReferenceMatchingRule.NestingDepthBelowThresholdStatus,
            Require(DesignConversionReferenceMatchingRule.EvaluateMatch(
                Payload(
                    DesignConversionReferenceMatchingRule.StudentCandidateRole,
                    expectedImageId: "image:dashboard-reference",
                    expectedSha: Sha('5'),
                    actualImageId: "image:candidate-dashboard-output",
                    actualSha: Sha('7'),
                    nestingDepthScore: 0.899D),
                reference)).Status);
        Assert.Equal(
            DesignConversionReferenceMatchingRule.AnchorValuesBelowThresholdStatus,
            Require(DesignConversionReferenceMatchingRule.EvaluateMatch(
                Payload(
                    DesignConversionReferenceMatchingRule.StudentCandidateRole,
                    expectedImageId: "image:dashboard-reference",
                    expectedSha: Sha('5'),
                    actualImageId: "image:candidate-dashboard-output",
                    actualSha: Sha('7'),
                    anchorValuesScore: 0.899D),
                reference)).Status);
    }

    [Fact]
    public void IsMatch_MissingMarkupWrongSubjectForeignPatternAndSwappedRolesFailClosed()
    {
        var reference = Payload(
            DesignConversionReferenceMatchingRule.PaidReferenceRole,
            expectedImageId: "image:login-reference",
            expectedSha: Sha('8'),
            actualImageId: "image:paid-login-output",
            actualSha: Sha('9'));
        var candidate = Payload(
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            expectedImageId: "image:login-reference",
            expectedSha: Sha('8'),
            actualImageId: "image:candidate-login-output",
            actualSha: Sha('a'));

        Assert.Equal(
            DesignConversionReferenceMatchingRule.IncompleteMarkupAxisMissingStatus,
            Require(DesignConversionReferenceMatchingRule.EvaluateMatch(
                Payload(
                    DesignConversionReferenceMatchingRule.StudentCandidateRole,
                    expectedImageId: "image:login-reference",
                    expectedSha: Sha('8'),
                    actualImageId: "image:candidate-login-output",
                    actualSha: Sha('a'),
                    includeMarkupAxis: false),
                reference)).Status);
        Assert.False(DesignConversionReferenceMatchingRule.IsMatch(reference, candidate));
        Assert.False(DesignConversionReferenceMatchingRule.IsMatch(
            Payload(
                DesignConversionReferenceMatchingRule.StudentCandidateRole,
                expectedImageId: "image:login-reference",
                expectedSha: Sha('8'),
                actualImageId: "image:candidate-login-output",
                actualSha: Sha('a'),
                patternId: "figma-to-html-css"),
            reference));
        Assert.False(DesignConversionReferenceMatchingRule.IsMatch(
            Payload(
                DesignConversionReferenceMatchingRule.StudentCandidateRole,
                expectedImageId: "image:login-reference",
                expectedSha: Sha('8'),
                actualImageId: "image:candidate-login-output",
                actualSha: Sha('a'),
                subjectId: "design-tree-to-markup-lookalike"),
            reference));
    }

    [Fact]
    public void IsMatch_WrongSchemaExtraFieldAndMalformedRawJsonFailClosed()
    {
        var reference = Payload(
            DesignConversionReferenceMatchingRule.PaidReferenceRole,
            expectedImageId: "image:raw-reference",
            expectedSha: Sha('1'),
            actualImageId: "image:paid-raw-output",
            actualSha: Sha('2'));
        var candidate = Payload(
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            expectedImageId: "image:raw-reference",
            expectedSha: Sha('1'),
            actualImageId: "image:candidate-raw-output",
            actualSha: Sha('3'));

        Assert.False(DesignConversionReferenceMatchingRule.IsMatch("not json", reference));
        Assert.False(DesignConversionReferenceMatchingRule.IsMatch(
            candidate.Replace(
                DesignConversionReferenceEvidencePayload.Schema,
                "xiigen.design-conversion-reference-evidence.v0",
                StringComparison.Ordinal),
            reference));
        Assert.False(DesignConversionReferenceMatchingRule.IsMatch(
            candidate.Insert(candidate.Length - 1, ",\"extra\":true"),
            reference));
    }

    [Fact]
    public void PayloadCreate_DuplicateEvidenceRefAndInvalidScoresFailClosed()
    {
        var duplicateRef = DesignConversionReferenceEvidencePayload.Create(
            DesignConversionReferenceMatchingRule.DomainTag,
            DesignConversionReferenceMatchingRule.SubjectId,
            DesignConversionReferenceMatchingRule.PatternId,
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            VisualAxis(
                expectedImageId: "image:dup-reference",
                expectedSha: Sha('1'),
                actualImageId: "image:dup-actual",
                actualSha: Sha('2'),
                diffSha: Sha('3'),
                evidenceRefs:
                [
                    "SERVER_OWNED_LOCAL_PNG_COMPARISON_TRUST_CANONICALIZED",
                    "SERVER_OWNED_LOCAL_PNG_COMPARISON_TRUST_CANONICALIZED"
                ]),
            MarkupAxis(Sha('a')));
        var visualNaN = DesignConversionReferenceEvidencePayload.Create(
            DesignConversionReferenceMatchingRule.DomainTag,
            DesignConversionReferenceMatchingRule.SubjectId,
            DesignConversionReferenceMatchingRule.PatternId,
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            VisualAxis(
                expectedImageId: "image:nan-reference",
                expectedSha: Sha('4'),
                actualImageId: "image:nan-actual",
                actualSha: Sha('5'),
                diffSha: Sha('6'),
                score: double.NaN),
            MarkupAxis(Sha('a')));
        var markupInfinity = DesignConversionReferenceEvidencePayload.Create(
            DesignConversionReferenceMatchingRule.DomainTag,
            DesignConversionReferenceMatchingRule.SubjectId,
            DesignConversionReferenceMatchingRule.PatternId,
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            VisualAxis(
                expectedImageId: "image:inf-reference",
                expectedSha: Sha('7'),
                actualImageId: "image:inf-actual",
                actualSha: Sha('8'),
                diffSha: Sha('9')),
            MarkupAxis(Sha('a'), nodeTypeCompositionScore: double.PositiveInfinity));
        var markupOutOfRange = DesignConversionReferenceEvidencePayload.Create(
            DesignConversionReferenceMatchingRule.DomainTag,
            DesignConversionReferenceMatchingRule.SubjectId,
            DesignConversionReferenceMatchingRule.PatternId,
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            VisualAxis(
                expectedImageId: "image:range-reference",
                expectedSha: Sha('a'),
                actualImageId: "image:range-actual",
                actualSha: Sha('b'),
                diffSha: Sha('c')),
            MarkupAxis(Sha('a'), anchorValuesScore: 1.001D));

        Assert.False(duplicateRef.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_REFERENCE_EVIDENCE_REF_DUPLICATE", duplicateRef.ErrorCode);
        Assert.False(visualNaN.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_REFERENCE_VISUAL_SCORE_INVALID", visualNaN.ErrorCode);
        Assert.False(markupInfinity.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_REFERENCE_MARKUP_COMPOSITION_SCORE_INVALID", markupInfinity.ErrorCode);
        Assert.False(markupOutOfRange.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_REFERENCE_MARKUP_ANCHOR_SCORE_INVALID", markupOutOfRange.ErrorCode);
    }

    [Fact]
    public void IsMatch_MetadataOnlyCanonicalMarkerOnlyRejected()
    {
        var reference = Payload(
            DesignConversionReferenceMatchingRule.PaidReferenceRole,
            expectedImageId: "image:meta-reference",
            expectedSha: Sha('1'),
            actualImageId: "image:paid-meta-output",
            actualSha: Sha('2'));
        var copiedMarkerOnlyCandidate = Payload(
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            expectedImageId: "image:meta-reference",
            expectedSha: Sha('1'),
            actualImageId: "image:candidate-meta-output",
            actualSha: Sha('3'),
            evidenceRefs:
            [
                "SERVER_OWNED_LOCAL_PNG_COMPARISON_TRUST_CANONICALIZED",
                ServerOwnedLocalPngComparisonProvider.ProviderKind,
                ServerOwnedLocalPngComparisonProvider.Boundary
            ]);

        var result = Require(DesignConversionReferenceMatchingRule.EvaluateMatch(
            copiedMarkerOnlyCandidate,
            reference));

        Assert.False(result.IsMatch);
        Assert.Equal(DesignConversionReferenceMatchingRule.VisualTrustAbsentStatus, result.Status);
        Assert.True(result.CandidateVisualAxis.IsPresent);
        Assert.False(result.CandidateVisualAxis.IsContractRecognized);
    }

    [Fact]
    public void IsMatch_ExternalReceiptMarkerRejectedByLocalPngOnlyPolicy()
    {
        var reference = Payload(
            DesignConversionReferenceMatchingRule.PaidReferenceRole,
            expectedImageId: "image:external-reference",
            expectedSha: Sha('4'),
            actualImageId: "image:paid-external-output",
            actualSha: Sha('5'));
        var externalCandidate = Payload(
            DesignConversionReferenceMatchingRule.StudentCandidateRole,
            expectedImageId: "image:external-reference",
            expectedSha: Sha('4'),
            actualImageId: "image:candidate-external-output",
            actualSha: Sha('6'),
            trustClassification: DesignConversionReferenceMatchingRule.SupportedExternalVisualAiTrustClassification,
            providerKind: DesignConversionExternalVisualAiComparisonProvider.ProviderKind,
            evidenceRefs: ["SERVER_OWNED_EXTERNAL_VISUAL_AI_RECEIPT_VALIDATED"]);

        var result = Require(DesignConversionReferenceMatchingRule.EvaluateMatch(
            externalCandidate,
            reference));

        Assert.False(result.IsMatch);
        Assert.Equal(DesignConversionReferenceMatchingRule.VisualTrustAbsentStatus, result.Status);
        Assert.False(result.CandidateVisualAxis.IsContractRecognized);
    }

    [Fact]
    public void EvaluateCalibration_GenericEvaluatorKeepsForeignExpectedReferenceFalseHitsAtZero()
    {
        var reference = Payload(
            DesignConversionReferenceMatchingRule.PaidReferenceRole,
            expectedImageId: "image:billing-reference",
            expectedSha: Sha('b'),
            actualImageId: "image:paid-billing-output",
            actualSha: Sha('c'));
        var entry = CreateEntry(
            "billing",
            "Convert the billing dashboard design tree to markup.",
            reference);
        var input = CreateCalibrationInput(
            CreateCorpus(entry),
            positives:
            [
                Pair(
                    "positive-same-frozen-expected",
                    entry.EntryId,
                    Payload(
                        DesignConversionReferenceMatchingRule.StudentCandidateRole,
                        expectedImageId: "image:billing-reference",
                        expectedSha: Sha('b'),
                        actualImageId: "image:candidate-billing-output",
                        actualSha: Sha('d')),
                    entry.PaidModelReferenceAnswer)
            ],
            negatives:
            [
                Pair(
                    "negative-foreign-expected-self-score-decoy",
                    entry.EntryId,
                    Payload(
                        DesignConversionReferenceMatchingRule.StudentCandidateRole,
                        expectedImageId: "image:pricing-reference",
                        expectedSha: Sha('e'),
                        actualImageId: "image:candidate-pricing-output",
                        actualSha: Sha('f')),
                    entry.PaidModelReferenceAnswer)
            ]);

        var result = Require(
            DesignConversionReferenceMatchingRule.EvaluateCalibration(input));

        Assert.Equal(1, result.KnownPositiveCount);
        Assert.Equal(0, result.FalseMissCount);
        Assert.Equal(0D, result.FalseMissRate);
        Assert.Equal(1, result.KnownNegativeCount);
        Assert.Equal(0, result.FalseHitCount);
        Assert.Equal(0D, result.FalseHitRate);
    }

    [Fact]
    public void EvaluateCalibration_WrongIdentityAndWrongCorpusSubjectFailBeforeMetrics()
    {
        var reference = Payload(
            DesignConversionReferenceMatchingRule.PaidReferenceRole,
            expectedImageId: "image:identity-reference",
            expectedSha: Sha('1'),
            actualImageId: "image:paid-identity-output",
            actualSha: Sha('2'));
        var entry = CreateEntry(
            "identity",
            "Convert identity settings.",
            reference);
        var wrongIdentity = Require(ReferenceMatchingRuleIdentity.Create(
            DesignConversionReferenceMatchingRule.DomainTag,
            "design-conversion-foreign-rule",
            "v1",
            "Wrong identity fixture; no metrics should be produced."));
        var wrongIdentityInput = Require(ReferenceMatchingCalibrationInput.Create(
            CreateCorpus(entry),
            wrongIdentity,
            knownPositivePairs:
            [
                Pair(
                    "positive-wrong-identity",
                    entry.EntryId,
                    Payload(
                        DesignConversionReferenceMatchingRule.StudentCandidateRole,
                        expectedImageId: "image:identity-reference",
                        expectedSha: Sha('1'),
                        actualImageId: "image:candidate-identity-output",
                        actualSha: Sha('3')),
                    entry.PaidModelReferenceAnswer)
            ],
            knownNegativePairs:
            [
                Pair(
                    "negative-wrong-identity",
                    entry.EntryId,
                    Payload(
                        DesignConversionReferenceMatchingRule.StudentCandidateRole,
                        expectedImageId: "image:foreign-reference",
                        expectedSha: Sha('4'),
                        actualImageId: "image:candidate-foreign-output",
                        actualSha: Sha('5')),
                    entry.PaidModelReferenceAnswer)
            ]));
        var wrongSubjectEntry = CreateEntry(
            "wrong-subject",
            "Convert a foreign design subject.",
            reference,
            subjectId: "design-tree-to-markup-foreign");
        var wrongSubjectInput = Require(ReferenceMatchingCalibrationInput.Create(
            CreateCorpus(wrongSubjectEntry),
            Require(DesignConversionReferenceMatchingRule.CreateIdentity()),
            knownPositivePairs:
            [
                Pair(
                    "positive-wrong-subject",
                    wrongSubjectEntry.EntryId,
                    Payload(
                        DesignConversionReferenceMatchingRule.StudentCandidateRole,
                        expectedImageId: "image:identity-reference",
                        expectedSha: Sha('1'),
                        actualImageId: "image:candidate-identity-output",
                        actualSha: Sha('3')),
                    wrongSubjectEntry.PaidModelReferenceAnswer)
            ],
            knownNegativePairs:
            [
                Pair(
                    "negative-wrong-subject",
                    wrongSubjectEntry.EntryId,
                    Payload(
                        DesignConversionReferenceMatchingRule.StudentCandidateRole,
                        expectedImageId: "image:foreign-reference",
                        expectedSha: Sha('4'),
                        actualImageId: "image:candidate-foreign-output",
                        actualSha: Sha('5')),
                    wrongSubjectEntry.PaidModelReferenceAnswer)
            ]));

        var wrongIdentityResult =
            DesignConversionReferenceMatchingRule.EvaluateCalibration(wrongIdentityInput);
        var wrongSubjectResult =
            DesignConversionReferenceMatchingRule.EvaluateCalibration(wrongSubjectInput);

        Assert.False(wrongIdentityResult.IsSuccess);
        Assert.Null(wrongIdentityResult.Value);
        Assert.Equal(
            DesignConversionReferenceMatchingRule.RuleIdentityMismatchErrorCode,
            wrongIdentityResult.ErrorCode);
        Assert.False(wrongSubjectResult.IsSuccess);
        Assert.Null(wrongSubjectResult.Value);
        Assert.Equal(
            DesignConversionReferenceMatchingRule.CorpusSubjectMismatchErrorCode,
            wrongSubjectResult.ErrorCode);
    }

    private static ReferenceMatchingCalibrationInput CreateCalibrationInput(
        FrozenReferenceCorpus corpus,
        IReadOnlyList<ReferenceMatchingCalibrationPair> positives,
        IReadOnlyList<ReferenceMatchingCalibrationPair> negatives)
        => Require(ReferenceMatchingCalibrationInput.Create(
            corpus,
            Require(DesignConversionReferenceMatchingRule.CreateIdentity()),
            positives,
            negatives));

    private static FrozenReferenceCorpus CreateCorpus(
        params FrozenReferenceEntry[] entries)
        => Require(FrozenReferenceCorpus.Build(
            "design-conversion-reference-fixture",
            "v2026-09-08-unit",
            FrozenAtUtc,
            entries));

    private static FrozenReferenceEntry CreateEntry(
        string seed,
        string ownerInput,
        string referenceAnswer,
        string subjectId = DesignConversionReferenceMatchingRule.SubjectId)
    {
        // Unit-only fake paid provenance: this fixture exercises frozen-reference
        // matching only; it is not a production paid corpus, DPO row, weight,
        // checkpoint, model call, or training artifact.
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
            DesignConversionReferenceMatchingRule.DomainTag,
            subjectId,
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

    private static string Payload(
        string role,
        string expectedImageId,
        string expectedSha,
        string actualImageId,
        string actualSha,
        string? patternId = null,
        string? subjectId = null,
        string? expectedMarkupFingerprint = null,
        string? trustClassification = null,
        string? providerKind = null,
        IReadOnlyList<string>? evidenceRefs = null,
        bool includeMarkupAxis = true,
        double visualScore = 0.97D,
        double nodeTypeCompositionScore = 0.96D,
        double nestingDepthScore = 0.95D,
        double anchorValuesScore = 0.94D)
    {
        var diffSha = Sha('0');
        return Require(DesignConversionReferenceEvidencePayload.Create(
            DesignConversionReferenceMatchingRule.DomainTag,
            subjectId ?? DesignConversionReferenceMatchingRule.SubjectId,
            patternId ?? DesignConversionReferenceMatchingRule.PatternId,
            role,
            VisualAxis(
                expectedImageId,
                expectedSha,
                actualImageId,
                actualSha,
                diffSha,
                trustClassification,
                providerKind,
                evidenceRefs,
                visualScore),
            includeMarkupAxis
                ? MarkupAxis(
                    expectedMarkupFingerprint ?? Sha('f'),
                    nodeTypeCompositionScore,
                    nestingDepthScore,
                    anchorValuesScore)
                : null)).ToCanonicalString();
    }

    private static DesignConversionReferenceVisualAxisEvidence VisualAxis(
        string expectedImageId,
        string expectedSha,
        string actualImageId,
        string actualSha,
        string diffSha,
        string? trustClassification = null,
        string? providerKind = null,
        IReadOnlyList<string>? evidenceRefs = null,
        double score = 0.97D)
    {
        var comparisonRequestId =
            "comparison:" + actualImageId.Replace("image:", string.Empty, StringComparison.Ordinal);
        return new DesignConversionReferenceVisualAxisEvidence(
            DesignConversionReferenceMatchingRule.SupportedTrustBoundary,
            trustClassification ?? DesignConversionReferenceMatchingRule.SupportedLocalPngTrustClassification,
            providerKind ?? ServerOwnedLocalPngComparisonProvider.ProviderKind,
            comparisonRequestId,
            expectedImageId,
            expectedSha,
            actualImageId,
            actualSha,
            "image:diff-" + actualImageId.Replace("image:", string.Empty, StringComparison.Ordinal),
            diffSha,
            evidenceRefs ?? LocalPngRefs(comparisonRequestId, expectedSha, actualSha, diffSha),
            score);
    }

    private static DesignConversionReferenceMarkupAxisEvidence MarkupAxis(
        string expectedMarkupFingerprint,
        double nodeTypeCompositionScore = 0.96D,
        double nestingDepthScore = 0.95D,
        double anchorValuesScore = 0.94D)
        => new(
            expectedMarkupFingerprint,
            nodeTypeCompositionScore,
            nestingDepthScore,
            anchorValuesScore);

    private static IReadOnlyList<string> LocalPngRefs(
        string comparisonRequestId,
        string expectedSha,
        string actualSha,
        string diffSha)
        =>
        [
            "SERVER_OWNED_LOCAL_PNG_COMPARISON_TRUST_CANONICALIZED",
            ServerOwnedLocalPngComparisonProvider.ProviderKind,
            ServerOwnedLocalPngComparisonProvider.Boundary,
            $"local-png-comparison:{comparisonRequestId}",
            $"expected-sha256:{expectedSha}",
            $"rendered-sha256:{actualSha}",
            $"diff-sha256:{diffSha}"
        ];

    private static string Sha(char hex)
        => "sha256:" + new string(hex, 64);

    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(
            result.IsSuccess,
            $"Expected success but got {result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        return result.Value!;
    }
}
