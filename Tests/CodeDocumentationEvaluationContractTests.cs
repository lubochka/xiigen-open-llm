// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using ContinuousLearning;

public sealed class CodeDocumentationEvaluationContractTests
{
    private static readonly DateTimeOffset FrozenAtUtc =
        new(2026, 9, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FrozenCorpus_CanonicalizesOppositeInputOrderAndRetainsCanonicalCaseOrder()
    {
        var alpha = FrozenReferenceCorpusTests.CreateEntry(
            "s03-02-alpha",
            FrozenReferencePartition.HeldOut);
        var beta = FrozenReferenceCorpusTests.CreateEntry(
            "s03-02-beta",
            FrozenReferencePartition.HeldOut);
        var first = BuildCorpus(alpha, beta);
        var second = BuildCorpus(beta, alpha);

        Assert.Equal(first.CorpusSha256, second.CorpusSha256);
        Assert.Equal(first.CanonicalRepresentation, second.CanonicalRepresentation);
        Assert.Equal(
            first.EntryNames,
            first.Entries
                .Select(static entry => entry.EntryId)
                .OrderBy(static entryId => entryId, StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public void FrozenCorpus_ChangedContentAndSourceFingerprintChangeIdentity()
    {
        var original = FrozenReferenceCorpusTests.CreateEntry(
            "s03-02-content",
            FrozenReferencePartition.HeldOut,
            referenceAnswer: "original answer");
        var changedContent = FrozenReferenceCorpusTests.CreateEntry(
            "s03-02-content",
            FrozenReferencePartition.HeldOut,
            referenceAnswer: "changed answer");
        var changedSource = FrozenReferenceCorpusTests.CreateEntry(
            "s03-02-content",
            FrozenReferencePartition.HeldOut,
            sourceFingerprint: FrozenReferenceCorpusTests.SourceFingerprint(
                "s03-02-different-source"),
            referenceAnswer: "original answer");
        var changedSet = FrozenReferenceCorpusTests.CreateEntry(
            "s03-02-additional-case",
            FrozenReferencePartition.HeldOut);

        Assert.NotEqual(
            BuildCorpus(original).CorpusSha256,
            BuildCorpus(changedContent).CorpusSha256);
        Assert.NotEqual(
            BuildCorpus(original).CorpusSha256,
            BuildCorpus(changedSource).CorpusSha256);
        Assert.NotEqual(
            BuildCorpus(original).CorpusSha256,
            BuildCorpus(original, changedSet).CorpusSha256);
    }

    [Fact]
    public void FrozenCorpus_SnapshotRemainsStableAfterCallerCollectionMutation()
    {
        var first = FrozenReferenceCorpusTests.CreateEntry(
            "s03-02-snapshot-first",
            FrozenReferencePartition.HeldOut);
        var second = FrozenReferenceCorpusTests.CreateEntry(
            "s03-02-snapshot-second",
            FrozenReferencePartition.HeldOut);
        var entries = new List<FrozenReferenceEntry> { first, second };
        var corpus = FrozenReferenceCorpusTests.Require(FrozenReferenceCorpus.Build(
            "code-documentation-s03-02",
            "v2026-09-15",
            FrozenAtUtc,
            entries));
        var originalHash = corpus.CorpusSha256;
        var originalIds = corpus.EntryNames.ToArray();

        entries.Clear();
        entries.Add(FrozenReferenceCorpusTests.CreateEntry(
            "s03-02-after-capture",
            FrozenReferencePartition.HeldOut));

        Assert.Equal(originalHash, corpus.CorpusSha256);
        Assert.Equal(originalIds, corpus.EntryNames);
        Assert.Equal(2, corpus.Count);
    }

    [Fact]
    public void QualityGateConditions_RejectChangedNoisePolicyIdentity()
    {
        var baseline = CreateCalibrationResult(
            MeasurementNoisePolicyConfig.Canonical.IdentitySha256);
        var changed = CreateCalibrationResult(
            new MeasurementNoisePolicyConfig(
                MeasurementNoisePolicyConfig.Canonical.RepeatCount,
                MeasurementNoisePolicyConfig.Canonical
                    .StandardDeviationMultiplier + 0.25d)
                .IdentitySha256);

        Assert.False(
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .SameExamConditions(baseline, changed));
    }

    [Fact]
    public void CalibrationPlan_RejectsHeldOutOverflowBeforeAnyEvaluationCall()
    {
        var corpus = BuildCorpus(
            FrozenReferenceCorpusTests.CreateEntry(
                "s03-02-overflow-one",
                FrozenReferencePartition.HeldOut),
            FrozenReferenceCorpusTests.CreateEntry(
                "s03-02-overflow-two",
                FrozenReferencePartition.HeldOut));
        var settings = CreateSettings(maxHeldOutCases: 1);
        var configuration = CreateTeacherConfiguration(maxTeacherCalls: 50);

        var plan = CodeDocumentationSavedModelAssessmentCalibrationService
            .PlanCalibration(
                corpus,
                settings,
                configuration,
                MeasurementNoisePolicyConfig.Canonical);

        Assert.False(plan.IsSuccess);
        Assert.Equal(
            CodeDocumentationSavedModelAssessmentService.HeldOutTooLargeErrorCode,
            plan.ErrorCode);
    }

    [Fact]
    public void GenerationIdentity_ChangesForEachGenerationCondition()
    {
        var heldOut = new[]
        {
            FrozenReferenceCorpusTests.CreateEntry(
                "s03-02-generation-case",
                FrozenReferencePartition.HeldOut)
        };
        var baseline = new CodeDocumentationSavedModelAssessmentGenerationSettings(
            1,
            0f,
            1,
            1f,
            17,
            5,
            256);
        var baselineIdentity = CodeDocumentationSavedModelAssessmentService
            .ComputeGenerationSettingsSha256(baseline, heldOut);
        var changedSettings = new[]
        {
            baseline with { MaxNewTokens = 2 },
            baseline with { Temperature = 0.25f },
            baseline with { TopK = 2 },
            baseline with { TopP = 0.75f },
            baseline with { Seed = 18 },
            baseline with { MaxHeldOutCases = 6 },
            baseline with { MaxAnswerChars = 512 },
        };

        Assert.All(
            changedSettings,
            changed => Assert.NotEqual(
                baselineIdentity,
                CodeDocumentationSavedModelAssessmentService
                    .ComputeGenerationSettingsSha256(changed, heldOut)));
    }

    private static FrozenReferenceCorpus BuildCorpus(
        params FrozenReferenceEntry[] entries)
        => FrozenReferenceCorpusTests.Require(FrozenReferenceCorpus.Build(
            "code-documentation-s03-02",
            "v2026-09-15",
            FrozenAtUtc,
            entries));

    private static CodeDocumentationSavedModelAssessmentGenerationSettings
        CreateSettings(int maxHeldOutCases)
        => new(
            MaxNewTokens: 1,
            Temperature: 0f,
            TopK: 1,
            TopP: 1f,
            Seed: 17,
            MaxHeldOutCases: maxHeldOutCases,
            MaxAnswerChars: 256);

    private static CodeDocumentationSavedModelAssessmentCalibrationResult
        CreateCalibrationResult(string noisePolicyIdentity)
    {
        var settings = CreateSettings(maxHeldOutCases: 5);
        var scorer = new HeldOutScorerIdentity(
            "code-documentation-s03-02",
            "evaluator",
            "v1",
            "normalization-v1",
            "metric-v1");
        const string evaluationSetId = "same-evaluation-set";
        var passport = MeasurementNoisePolicy.BuildPassport(
            scorer,
            evaluationSetId,
            scoredRows: 1,
            repeatedScores: [1d, 1d, 1d, 1d, 1d],
            config: MeasurementNoisePolicyConfig.Canonical,
            calibratedAtUtc: DateTimeOffset.UnixEpoch);
        var evaluator = new CodeDocumentationSavedModelAssessmentEvaluatorReceipt(
            "evaluator",
            new string('a', 64),
            new string('b', 64),
            "evaluation-v1",
            new string('c', 64),
            ["request-1"]);
        var representative = new CodeDocumentationSavedModelAssessmentReport(
            true,
            "test",
            "s03-02-calibration",
            "code-documentation",
            "csharp",
            1,
            1,
            1d,
            "v1",
            new string('d', 64),
            new string('e', 64),
            settings,
            new string('f', 64),
            CodeDocumentationSavedModelAssessmentModelPassport
                .AcceptedRepositoryCheckpointSourceKind,
            "models/identity.json",
            new string('1', 64),
            new string('2', 64),
            new string('3', 64),
            new string('4', 64),
            1,
            new string('5', 64),
            1,
            evaluator,
            [new CodeDocumentationSavedModelAssessmentCaseResult(
                "case-1",
                true,
                "ok",
                "answer",
                new string('6', 64),
                6)],
            ["test:evidence"]);
        var calibration = new CodeDocumentationSavedModelAssessmentCalibrationReport(
            true,
            "test",
            "calibration",
            5,
            1,
            5,
            5,
            evaluationSetId,
            noisePolicyIdentity,
            scorer,
            passport,
            [],
            ["test:evidence"]);
        return new CodeDocumentationSavedModelAssessmentCalibrationResult(
            representative,
            calibration);
    }

    private static ContinuousLearningTp1DpoConfiguration
        CreateTeacherConfiguration(int maxTeacherCalls)
    {
        var created = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen-code-doc-quality-test:9b",
            new string('a', 64),
            new string('b', 64),
            maxTeacherCalls,
            "sha256:" + new string('c', 64),
            "sha256:" + new string('d', 64),
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return created.Value!;
    }
}
