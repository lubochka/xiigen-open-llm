namespace LlmFromScratch.Tests;

using System.Globalization;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using TransformerModel;
using TransformerModel.Config;
using TensorCore.Core;
using TranslationDomain;

public sealed class TranslationDpoTrainerBridgeTests : IDisposable
{
    private const string SeedFileName = "language-translate-seed-v1.jsonl";
    private readonly string _checkpointDir = Path.Combine(Path.GetTempPath(), $"translation-dpo-bridge-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_checkpointDir))
            Directory.Delete(_checkpointDir, recursive: true);
    }

    [Fact]
    public void BuildTrainDpoBatches_ConvertsTrainPairsToDpoBatchAndExcludesHeldOut()
    {
        var handoff = BuildHandoff();

        var result = TranslationDpoTrainerBridge.BuildTrainDpoBatches(handoff, BuildSettings());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(handoff.TrainingPairs.Count, result.Value!.TrainPairCount);
        Assert.Equal(handoff.HeldOutEvaluationRows.Count, result.Value.HeldOutExcludedCount);
        Assert.Equal(handoff.TrainingPairs.Count, result.Value.Batches.Count);
        Assert.DoesNotContain(result.Value.Records, record =>
            result.Value.HeldOutEvaluationPairIds.Contains(record.PairId, StringComparer.Ordinal));
        Assert.All(result.Value.Records, record =>
        {
            var source = Assert.Single(
                handoff.TrainingPairs,
                candidate => candidate.PairId == record.PairId);
            Assert.IsType<DpoBatch>(record.Batch);
            Assert.Same(source.Admission, record.Batch.Admission);
            Assert.NotNull(record.Batch.Admission);
            Assert.Equal(new[] { 1, BuildSettings().ContextLength }, record.Batch.ChosenIds.Shape.Dimensions);
            Assert.Equal(new[] { 1, BuildSettings().ContextLength }, record.Batch.RejectedIds.Shape.Dimensions);
            Assert.Contains(record.Direction, result.Value.Directions);
            Assert.Contains(record.CapabilityId, result.Value.CapabilityIds);
            Assert.True(record.Batch.ChosenMask.Data.Count(value => value == 1f) >= 4);
            Assert.True(record.Batch.RejectedMask.Data.Count(value => value == 1f) >= 4);
        });
        Assert.Contains("do not pass held-out rows", result.Value.NextEvaluationBoundary, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildTrainDpoBatches_UsesActualPromptChosenRejectedTextNotConstants()
    {
        var handoff = BuildHandoff();
        var baseline = TranslationDpoTrainerBridge.BuildTrainDpoBatches(handoff, BuildSettings()).Value!;
        var first = handoff.TrainingPairs[0];
        var changedHandoff = WithFirstTrainingChosen(
            handoff,
            first.TrainerPair.Chosen + " bridge-unique-token");

        var changed = TranslationDpoTrainerBridge.BuildTrainDpoBatches(changedHandoff, BuildSettings()).Value!;
        var baselineRecord = baseline.Records.Single(record => record.PairId == first.PairId);
        var changedRecord = changed.Records.Single(record => record.PairId == first.PairId);

        Assert.NotEqual(baselineRecord.ChosenTextFingerprint, changedRecord.ChosenTextFingerprint);
        Assert.False(baselineRecord.ChosenTokenIds.SequenceEqual(changedRecord.ChosenTokenIds));
        Assert.Equal(baselineRecord.RejectedTextFingerprint, changedRecord.RejectedTextFingerprint);
        Assert.True(changedRecord.ChosenTokenIds.All(id => id >= 0 && id < BuildSettings().VocabSize));
    }

    [Fact]
    public void BuildTrainDpoBatches_IgnoresUnadmittedInputWhenBuildingTrainingTokens()
    {
        var handoff = BuildHandoff();
        var baseline = TranslationDpoTrainerBridge.BuildTrainDpoBatches(handoff, BuildSettings()).Value!;
        var first = handoff.TrainingPairs[0];
        var inputOnlyMutation = first with
        {
            Input = first.Input + " unadmitted-input-token"
        };

        var changed = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            handoff with { TrainingPairs = [inputOnlyMutation, .. handoff.TrainingPairs.Skip(1)] },
            BuildSettings());

        Assert.True(changed.IsSuccess, changed.ErrorMessage);
        var baselineRecord = baseline.Records.Single(record => record.PairId == first.PairId);
        var changedRecord = changed.Value!.Records.Single(record => record.PairId == first.PairId);

        Assert.Equal(baselineRecord.PromptFingerprint, changedRecord.PromptFingerprint);
        Assert.Equal(baselineRecord.ChosenTextFingerprint, changedRecord.ChosenTextFingerprint);
        Assert.Equal(baselineRecord.RejectedTextFingerprint, changedRecord.RejectedTextFingerprint);
        Assert.Equal(baselineRecord.ChosenTokenIds, changedRecord.ChosenTokenIds);
        Assert.Equal(baselineRecord.RejectedTokenIds, changedRecord.RejectedTokenIds);
    }

    [Fact]
    public void BuildTrainDpoBatches_UsesAdmittedReplacementPromptWithoutStaleSeedInputTokens()
    {
        var corpus = LoadCorpus();
        var sourceRow = corpus.TrainRows[0];
        var trainReadyPairs = corpus.ToTrainReadyPairs(TranslationSeedCorpusSplit.Train);
        var sourcePair = trainReadyPairs[0];
        var replacement = sourcePair with
        {
            Prompt = sourcePair.Prompt + " Keep the admitted artifact wording.",
            ASide = "Admitted replacement source text with unique artifact wording.",
            Chosen = "The admitted replacement keeps the project wording.",
            Rejected = "The stale seed version drops the project wording. [WRONG: stale_seed_guard - project wording dropped]"
        };
        var replacements = trainReadyPairs
            .Select(pair => pair.Id == replacement.Id ? replacement : pair)
            .ToArray();
        var handoff = BuildHandoff(corpus, replacements);
        var baseline = TranslationDpoTrainerBridge.BuildTrainDpoBatches(handoff, BuildSettings()).Value!;
        var record = Assert.Single(handoff.TrainingPairs, pair => pair.PairId == replacement.Id);
        var staleSeedInputMutation = record with
        {
            Input = sourceRow.InputText + " stale-seed-token-that-must-not-train"
        };

        var changed = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            handoff with { TrainingPairs = [staleSeedInputMutation, .. handoff.TrainingPairs.Skip(1)] },
            BuildSettings());

        Assert.True(changed.IsSuccess, changed.ErrorMessage);
        var baselineRecord = baseline.Records.Single(candidate => candidate.PairId == replacement.Id);
        var changedRecord = changed.Value!.Records.Single(candidate => candidate.PairId == replacement.Id);

        Assert.Equal(sourceRow.InputText, record.Input);
        Assert.NotEqual(record.Input, replacement.ASide);
        Assert.Contains(replacement.ASide, baselineRecord.SourcePair.Prompt, StringComparison.Ordinal);
        Assert.Equal(baselineRecord.PromptFingerprint, changedRecord.PromptFingerprint);
        Assert.Equal(baselineRecord.ChosenTokenIds, changedRecord.ChosenTokenIds);
        Assert.Equal(baselineRecord.RejectedTokenIds, changedRecord.RejectedTokenIds);
    }

    [Fact]
    public void BuildTrainDpoBatches_StripsReservedDiagnosticsBeforeTokenizationAndKeepsRawAuditText()
    {
        var handoff = BuildHandoff();
        var naturalRejected = "The demo ends at nine while keeping [ordinary bracket note] visible.";
        var sameNaturalFirstDiagnostics =
            $"{naturalRejected} [PHRASE: демо начинается => demo starts] [WRONG: diagnostic_a - first audit reason]";
        var sameNaturalSecondDiagnostics =
            $"{naturalRejected} [PHRASE: другой текст => other text] [WRONG: diagnostic_b - second audit reason]";
        var differentNaturalSameDiagnostics =
            "The demo starts at nine while keeping [ordinary bracket note] visible. [WRONG: diagnostic_a - first audit reason]";

        var firstDiagnostics = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingRejected(handoff, sameNaturalFirstDiagnostics),
            BuildSettings()).Value!.Records[0];
        var secondDiagnostics = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingRejected(handoff, sameNaturalSecondDiagnostics),
            BuildSettings()).Value!.Records[0];
        var differentNatural = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingRejected(handoff, differentNaturalSameDiagnostics),
            BuildSettings()).Value!.Records[0];

        Assert.Equal(sameNaturalFirstDiagnostics, firstDiagnostics.SourcePair.Rejected);
        Assert.Equal(sameNaturalSecondDiagnostics, secondDiagnostics.SourcePair.Rejected);
        Assert.Equal(firstDiagnostics.RejectedTextFingerprint, secondDiagnostics.RejectedTextFingerprint);
        Assert.Equal(firstDiagnostics.RejectedTokenIds, secondDiagnostics.RejectedTokenIds);
        Assert.NotEqual(firstDiagnostics.RejectedTextFingerprint, differentNatural.RejectedTextFingerprint);
        Assert.NotEqual(firstDiagnostics.RejectedTokenIds, differentNatural.RejectedTokenIds);
    }

    [Fact]
    public void BuildTrainDpoBatches_StripsAgreementDiagnosticsBeforeTokenizationAndKeepsRawAuditText()
    {
        var handoff = BuildHandoff();
        var naturalRejected = "The demo ends at nine while keeping the sentence content visible.";
        var firstAgreement =
            $"{naturalRejected} [AGREEMENT: демо => demo] [AGREEMENT: начинается => starts]";
        var secondAgreement =
            $"{naturalRejected} [agreement: встреча => meeting] [AGREEMENT: завтра => tomorrow]";
        var invalidAgreementA =
            $"{naturalRejected} [AGREEMENT: this has no arrow A]";
        var invalidAgreementB =
            $"{naturalRejected} [AGREEMENT: this has no arrow B]";

        var firstRecord = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingRejected(handoff, firstAgreement),
            BuildSettings()).Value!.Records[0];
        var secondRecord = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingRejected(handoff, secondAgreement),
            BuildSettings()).Value!.Records[0];
        var invalidAgreementFirst = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingRejected(handoff, invalidAgreementA),
            BuildSettings()).Value!.Records[0];
        var invalidAgreementSecond = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingRejected(handoff, invalidAgreementB),
            BuildSettings()).Value!.Records[0];

        Assert.Equal(firstAgreement, firstRecord.SourcePair.Rejected);
        Assert.Equal(secondAgreement, secondRecord.SourcePair.Rejected);
        Assert.Equal(firstRecord.RejectedTextFingerprint, secondRecord.RejectedTextFingerprint);
        Assert.Equal(firstRecord.RejectedTokenIds, secondRecord.RejectedTokenIds);
        Assert.NotEqual(invalidAgreementFirst.RejectedTextFingerprint, invalidAgreementSecond.RejectedTextFingerprint);
    }

    [Fact]
    public void BuildTrainDpoBatches_StripsChosenDiagnosticsAndPreservesOrdinaryBracketText()
    {
        var handoff = BuildHandoff();
        var naturalChosen = "The demo starts at nine with [ordinary bracket note] visible and [WRONG: unclosed text";
        var firstDiagnostic =
            $"{naturalChosen} [WRONG: chosen_a - first audit reason] [PHRASE: начало => start] [AGREEMENT: демо => demo]";
        var secondDiagnostic =
            $"{naturalChosen} [wrong: chosen_b - second audit reason] [phrase: другой => other] [agreement: встреча => meeting]";
        var invalidPhraseA =
            $"{naturalChosen} [PHRASE: this has no arrow A]";
        var invalidPhraseB =
            $"{naturalChosen} [PHRASE: this has no arrow B]";

        var firstRecord = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingChosen(handoff, firstDiagnostic),
            BuildSettings()).Value!.Records[0];
        var secondRecord = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingChosen(handoff, secondDiagnostic),
            BuildSettings()).Value!.Records[0];
        var invalidPhraseFirst = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingChosen(handoff, invalidPhraseA),
            BuildSettings()).Value!.Records[0];
        var invalidPhraseSecond = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingChosen(handoff, invalidPhraseB),
            BuildSettings()).Value!.Records[0];

        Assert.Equal(firstDiagnostic, firstRecord.SourcePair.Chosen);
        Assert.Equal(secondDiagnostic, secondRecord.SourcePair.Chosen);
        Assert.Equal(firstRecord.ChosenTextFingerprint, secondRecord.ChosenTextFingerprint);
        Assert.Equal(firstRecord.ChosenTokenIds, secondRecord.ChosenTokenIds);
        Assert.NotEqual(invalidPhraseFirst.ChosenTextFingerprint, invalidPhraseSecond.ChosenTextFingerprint);
    }

    [Fact]
    public void BuildHeldOutEvaluationDpoBatches_StripsReservedDiagnosticsBeforeEvaluationAndKeepsRawAuditText()
    {
        var handoff = BuildHandoff();
        var heldOut = handoff.HeldOutEvaluationRows[0];
        var naturalRejected = "The client rejected the large pilot today.";
        var firstDiagnostics = heldOut with
        {
            RejectedResponse = $"{naturalRejected} [AGREEMENT: client => client]",
            RejectionMarker = "heldout_diagnostic_a",
            RejectionReason = "first held-out audit reason"
        };
        var secondDiagnostics = heldOut with
        {
            RejectedResponse = $"{naturalRejected} [AGREEMENT: pilot => pilot]",
            RejectionMarker = "heldout_diagnostic_b",
            RejectionReason = "second held-out audit reason"
        };

        var firstRecord = TranslationDpoTrainerBridge.BuildHeldOutEvaluationDpoBatches(
            handoff with { HeldOutEvaluationRows = [firstDiagnostics, .. handoff.HeldOutEvaluationRows.Skip(1)] },
            BuildSettings()).Value!.Records[0];
        var secondRecord = TranslationDpoTrainerBridge.BuildHeldOutEvaluationDpoBatches(
            handoff with { HeldOutEvaluationRows = [secondDiagnostics, .. handoff.HeldOutEvaluationRows.Skip(1)] },
            BuildSettings()).Value!.Records[0];

        Assert.Contains(naturalRejected, firstRecord.SourcePair.Rejected, StringComparison.Ordinal);
        Assert.Contains("[AGREEMENT: client => client]", firstRecord.SourcePair.Rejected, StringComparison.Ordinal);
        Assert.Contains("[AGREEMENT: pilot => pilot]", secondRecord.SourcePair.Rejected, StringComparison.Ordinal);
        Assert.Contains("[WRONG: heldout_diagnostic_a", firstRecord.SourcePair.Rejected, StringComparison.Ordinal);
        Assert.Contains("[WRONG: heldout_diagnostic_b", secondRecord.SourcePair.Rejected, StringComparison.Ordinal);
        Assert.Equal(firstRecord.RejectedTextFingerprint, secondRecord.RejectedTextFingerprint);
        Assert.Equal(firstRecord.RejectedTokenIds, secondRecord.RejectedTokenIds);
    }

    [Fact]
    public void BuildTrainDpoBatches_RejectsDiagnosticsOnlyRejectedTextBeforeTraining()
    {
        var handoff = BuildHandoff();
        var diagnosticsOnly =
            "[PHRASE: демо начинается => demo starts]\n[WRONG: diagnostic_only - no rejected translation remains]";

        var result = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingRejected(handoff, diagnosticsOnly),
            BuildSettings());

        Assert.False(result.IsSuccess);
        Assert.Equal("TRAIN_REJECTED_TEXT_EMPTY_AFTER_DIAGNOSTIC_STRIP", result.ErrorCode);
    }

    [Fact]
    public void BuildHeldOutEvaluationDpoBatches_RejectsDiagnosticsOnlyRejectedTextBeforeEvaluation()
    {
        var handoff = BuildHandoff();
        var heldOut = handoff.HeldOutEvaluationRows[0] with
        {
            RejectedResponse = "[PHRASE: демо начинается => demo starts]",
            RejectionMarker = "diagnostic_only",
            RejectionReason = "no rejected translation remains"
        };

        var result = TranslationDpoTrainerBridge.BuildHeldOutEvaluationDpoBatches(
            handoff with { HeldOutEvaluationRows = [heldOut, .. handoff.HeldOutEvaluationRows.Skip(1)] },
            BuildSettings());

        Assert.False(result.IsSuccess);
        Assert.Equal("HELDOUT_REJECTED_TEXT_EMPTY_AFTER_DIAGNOSTIC_STRIP", result.ErrorCode);
    }

    [Fact]
    public void BuildTrainDpoBatches_RejectsDiagnosticsOnlyChosenTextBeforeTraining()
    {
        var handoff = BuildHandoff();
        var diagnosticsOnly =
            "[WRONG: chosen_diagnostic_only - no accepted translation remains]\n[PHRASE: демо начинается => demo starts]";

        var result = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            WithFirstTrainingChosen(handoff, diagnosticsOnly),
            BuildSettings());

        Assert.False(result.IsSuccess);
        Assert.Equal("TRAIN_CHOSEN_TEXT_EMPTY_AFTER_DIAGNOSTIC_STRIP", result.ErrorCode);
    }

    [Fact]
    public void BuildHeldOutEvaluationDpoBatches_ConvertsHeldOutRowsToEvaluationBatchesAndExcludesTrain()
    {
        var handoff = BuildHandoff();
        var trainIds = handoff.TrainingPairs.Select(row => row.PairId).ToHashSet(StringComparer.Ordinal);

        var result = TranslationDpoTrainerBridge.BuildHeldOutEvaluationDpoBatches(handoff, BuildSettings());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(handoff.HeldOutEvaluationRows.Count, result.Value!.HeldOutEvaluationCount);
        Assert.Equal(handoff.TrainingPairs.Count, result.Value.TrainExcludedCount);
        Assert.Equal(handoff.HeldOutEvaluationRows.Count, result.Value.Batches.Count);
        Assert.Equal(
            handoff.TrainingPairs.Select(row => row.PairId).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            result.Value.ExcludedTrainPairIds);
        Assert.DoesNotContain(result.Value.Records, record => trainIds.Contains(record.PairId));
        Assert.All(result.Value.Records, record =>
        {
            var row = Assert.Single(handoff.HeldOutEvaluationRows, candidate => candidate.PairId == record.PairId);
            Assert.IsType<DpoBatch>(record.Batch);
            Assert.Null(record.Batch.Admission);
            Assert.Equal(row.ExpectedResponse, record.SourcePair.Chosen);
            Assert.Contains(row.RejectedResponse, record.SourcePair.Rejected, StringComparison.Ordinal);
            Assert.Contains("[WRONG:", record.SourcePair.Rejected, StringComparison.Ordinal);
            Assert.Equal(new[] { 1, BuildSettings().ContextLength }, record.Batch.ChosenIds.Shape.Dimensions);
            Assert.Equal(new[] { 1, BuildSettings().ContextLength }, record.Batch.RejectedIds.Shape.Dimensions);
            Assert.Contains(record.Direction, result.Value.Directions);
            Assert.Contains(record.CapabilityId, result.Value.CapabilityIds);
            Assert.True(record.Batch.ChosenMask.Data.Count(value => value == 1f) >= 4);
            Assert.True(record.Batch.RejectedMask.Data.Count(value => value == 1f) >= 4);
        });
        Assert.Contains("EvaluateMany", result.Value.EvaluationBoundary, StringComparison.Ordinal);
        Assert.Contains("train rows stay excluded", result.Value.EvaluationBoundary, StringComparison.Ordinal);
    }

    [Fact]
    public void TrainWithStepMany_CallsRealTrainerAndReturnsMetricsBoundary()
    {
        var handoff = BuildHandoff();
        using var admitted = BuildAdmittedTrainer(handoff);
        var trainer = admitted.Trainer;

#pragma warning disable CS0618
        var result = TranslationDpoTrainerBridge.TrainWithStepMany(
            handoff,
            admitted.MutationSnapshot,
            trainer,
            step: 3);
#pragma warning restore CS0618

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(TranslationDpoTrainerBridge.TrainerCallPath, result.Value!.TrainerCallPath);
        Assert.Equal(3, result.Value.Step);
        Assert.Equal(3, result.Value.TrainerMetrics.Step);
        Assert.Equal(handoff.TrainingPairs.Count, result.Value.TrainPairCount);
        Assert.Equal(handoff.HeldOutEvaluationRows.Count, result.Value.HeldOutExcludedCount);
        Assert.False(float.IsNaN(result.Value.TrainerMetrics.Loss));
        Assert.False(float.IsInfinity(result.Value.TrainerMetrics.Loss));
        Assert.True(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000003.json")));
        Assert.True(File.Exists(Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(3))));
    }

    [Fact]
    public void EvaluateHeldOutWithEvaluateMany_CallsRealTrainerAndReturnsMetricsWithoutSemanticQualityClaim()
    {
        var handoff = BuildHandoff();
        var trainer = new DpoTrainer(
            new LlmModel(BuildTinyConfig()),
            new LlmModel(BuildTinyConfig()),
            _checkpointDir);

        var result = TranslationDpoTrainerBridge.EvaluateHeldOutWithEvaluateMany(
            handoff,
            trainer,
            step: 9,
            settings: BuildSettings());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(TranslationDpoTrainerBridge.EvaluationCallPath, result.Value!.TrainerCallPath);
        Assert.Equal(9, result.Value.Step);
        Assert.Equal(9, result.Value.EvaluationMetrics.Step);
        Assert.Equal(handoff.HeldOutEvaluationRows.Count, result.Value.HeldOutEvaluationCount);
        Assert.Equal(handoff.TrainingPairs.Count, result.Value.TrainExcludedCount);
        Assert.False(result.Value.SemanticQualityClaimed);
        Assert.Contains("do not claim semantic quality readiness", result.Value.SemanticQualityBoundary, StringComparison.Ordinal);
        Assert.False(float.IsNaN(result.Value.EvaluationMetrics.Loss));
        Assert.False(float.IsInfinity(result.Value.EvaluationMetrics.Loss));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000009.json")));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(9))));
    }

    [Fact]
    public void TrainThenEvaluate_DoesNotPassHeldOutRowsToStepManyOrTrainRowsToEvaluation()
    {
        var handoff = BuildHandoff();
        using var admitted = BuildAdmittedTrainer(handoff);
        var trainer = admitted.Trainer;

#pragma warning disable CS0618
        var train = TranslationDpoTrainerBridge.TrainWithStepMany(
            handoff,
            admitted.MutationSnapshot,
            trainer,
            step: 3);
#pragma warning restore CS0618
        var eval = TranslationDpoTrainerBridge.EvaluateHeldOutWithEvaluateMany(
            handoff,
            trainer,
            step: 4,
            settings: BuildSettings());

        Assert.True(train.IsSuccess, train.ErrorMessage);
        Assert.True(eval.IsSuccess, eval.ErrorMessage);
        Assert.Empty(train.Value!.TrainedPairIds.Intersect(eval.Value!.HeldOutEvaluationPairIds, StringComparer.Ordinal));
        Assert.Empty(eval.Value.HeldOutEvaluationPairIds.Intersect(eval.Value.ExcludedTrainPairIds, StringComparer.Ordinal));
        Assert.True(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000003.json")));
        Assert.True(File.Exists(Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(3))));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000004.json")));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(4))));
    }

    [Fact]
    public void TrainWithStepMany_DoesNotPassHeldOutRowsToTrainer()
    {
        var handoff = BuildHandoff();
        var batchResult = TranslationDpoTrainerBridge.BuildTrainDpoBatches(handoff, BuildSettings()).Value!;
        var heldOutIds = handoff.HeldOutEvaluationRows.Select(row => row.PairId).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(12, batchResult.TrainPairCount);
        Assert.Equal(4, batchResult.HeldOutExcludedCount);
        Assert.DoesNotContain(batchResult.Records, record => heldOutIds.Contains(record.PairId));
        Assert.All(handoff.HeldOutEvaluationRows, row =>
            Assert.Contains(row.PairId, batchResult.HeldOutEvaluationPairIds));
    }

    [Fact]
    public void BuildTrainDpoBatches_RejectsPairMutationAfterAdmissionBeforeTokenization()
    {
        var handoff = BuildHandoff();
        var first = handoff.TrainingPairs[0];
        var mutatedFirst = first with
        {
            TrainerPair = first.TrainerPair with
            {
                Rejected = first.TrainerPair.Rejected + " admission-mutation"
            },
            RejectedResponse = first.RejectedResponse + " admission-mutation"
        };

        var result = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            handoff with { TrainingPairs = [mutatedFirst, .. handoff.TrainingPairs.Skip(1)] },
            BuildSettings());

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_PAIR_ADMISSION_CONTENT_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public void BuildTrainDpoBatches_RejectsMissingWrongIssuerAndWrongSideAuthors()
    {
        var handoff = BuildHandoff();
        var first = handoff.TrainingPairs[0];
        var cases = new[]
        {
            (
                Record: first with { Admission = null },
                ErrorCode: DpoTrainerPairAdmission.MissingErrorCode
            ),
            (
                Record: first with
                {
                    Admission = DpoTrainerPairCustoms.CreateAdmitted(
                        first.TrainerPair.Prompt,
                        first.TrainerPair.Chosen,
                        first.TrainerPair.Rejected,
                        DpoPairSideAuthorKind.User,
                        DpoPairSideAuthorKind.User,
                        "unexpected-translation-issuer").Admission
                },
                ErrorCode: "DPO_PAIR_ADMISSION_ISSUER_MISMATCH"
            ),
            (
                Record: first with
                {
                    Admission = DpoTrainerPairCustoms.CreateAdmitted(
                        first.TrainerPair.Prompt,
                        first.TrainerPair.Chosen,
                        first.TrainerPair.Rejected,
                        DpoPairSideAuthorKind.LocalModel,
                        DpoPairSideAuthorKind.User,
                        nameof(TranslationDpoTrainingHandoffBuilder)).Admission
                },
                ErrorCode: "DPO_PAIR_ADMISSION_TRANSLATION_AUTHOR_MISMATCH"
            )
        };

        foreach (var testCase in cases)
        {
            var result = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
                handoff with { TrainingPairs = [testCase.Record, .. handoff.TrainingPairs.Skip(1)] },
                BuildSettings());

            Assert.False(result.IsSuccess);
            Assert.Equal(testCase.ErrorCode, result.ErrorCode);
        }
    }

    [Fact]
    public void TextFeaturizerBuildRecord_RejectsDirectTrainingRecordWithIllegalProvenance()
    {
        var handoff = BuildHandoff();
        var first = handoff.TrainingPairs[0];
        var featurizer = new TranslationDpoTextFeaturizer(BuildSettings());
        var cases = new (TranslationDpoTrainingHandoffRecord Record, string ErrorCode)[]
        {
            (
                first with { SourceCategory = "external_teacher" },
                "ILLEGAL_SOURCE_OR_LICENSE"
            ),
            (
                first with
                {
                    TeacherProvenance = first.TeacherProvenance with
                    {
                        SourceType = "external_teacher"
                    }
                },
                "ILLEGAL_TEACHER_PROVENANCE"
            ),
            (
                first with
                {
                    TeacherProvenance = first.TeacherProvenance with
                    {
                        ExternalApiUsed = true
                    }
                },
                "ILLEGAL_TEACHER_PROVENANCE"
            ),
            (
                first with
                {
                    TeacherProvenance = first.TeacherProvenance with
                    {
                        ClosedProprietaryTeacherUsed = true
                    }
                },
                "ILLEGAL_TEACHER_PROVENANCE"
            )
        };

        foreach (var testCase in cases)
        {
            var result = featurizer.BuildRecord(testCase.Record);

            Assert.False(result.IsSuccess);
            Assert.Equal(testCase.ErrorCode, result.ErrorCode);
        }
    }

    [Fact]
    public void BuildMutationBatchSnapshot_BindsEveryAdmittedRowToExactFinalTensors()
    {
        var handoff = BuildHandoff();

        var result = TranslationDpoTrainerBridge.BuildMutationBatchSnapshot(
            handoff,
            BuildSettings());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var snapshot = result.Value!;
        Assert.Equal(snapshot.BatchBuild.Batches.Count, snapshot.TrainingContent.Rows.Count);
        for (var ordinal = 0; ordinal < snapshot.BatchBuild.Batches.Count; ordinal++)
        {
            var batch = snapshot.BatchBuild.Batches[ordinal];
            var record = snapshot.BatchBuild.Records[ordinal];
            var admitted = snapshot.TrainingContent.Rows[ordinal];
            Assert.Same(batch, record.Batch);
            Assert.Equal(SerializeTensorBits("chosen_ids", batch.ChosenIds), admitted.Prompt);
            Assert.Equal(SerializeTensorBits("chosen_mask", batch.ChosenMask), admitted.Chosen);
            Assert.Equal(
                string.Concat(
                    SerializeTensorBits("rejected_ids", batch.RejectedIds),
                    "\n",
                    SerializeTensorBits("rejected_mask", batch.RejectedMask)),
                admitted.Rejected);
            Assert.StartsWith(
                "translation-dpo-final-token-tensors-v2\n",
                admitted.Context,
                StringComparison.Ordinal);
            Assert.Contains($"ordinal={ordinal}", admitted.Context, StringComparison.Ordinal);
            Assert.Contains(record.PairId, admitted.Context, StringComparison.Ordinal);
            Assert.Contains(record.LearningUnitId, admitted.Context, StringComparison.Ordinal);
            Assert.Contains(record.CapabilityId, admitted.Context, StringComparison.Ordinal);
            Assert.Contains(record.Direction, admitted.Context, StringComparison.Ordinal);
            Assert.Contains(record.SourceLanguage, admitted.Context, StringComparison.Ordinal);
            Assert.Contains(record.TargetLanguage, admitted.Context, StringComparison.Ordinal);
            Assert.Contains("vocab_size=512", admitted.Context, StringComparison.Ordinal);
            Assert.Contains("context_len=64", admitted.Context, StringComparison.Ordinal);
            Assert.DoesNotContain("project-wiki", admitted.Context, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void BuildMutationBatchSnapshot_DigestChangesWithTokenizationOrFinalTensorInput()
    {
        var handoff = BuildHandoff();
        var baseline = TranslationDpoTrainerBridge.BuildMutationBatchSnapshot(
            handoff,
            BuildSettings()).Value!;
        var changedSettings = TranslationDpoTrainerBridge.BuildMutationBatchSnapshot(
            handoff,
            new TranslationDpoTokenizationSettings(512, 63)).Value!;
        var changedChosen = string.Concat(
            "tensor-change ",
            handoff.TrainingPairs[0].ChosenResponse);
        var changedFirst = handoff.TrainingPairs[0] with
        {
            ChosenResponse = changedChosen,
            TrainerPair = handoff.TrainingPairs[0].TrainerPair with
            {
                Chosen = changedChosen
            }
        };
        var changedHandoff = handoff with
        {
            TrainingPairs = [ReadmitTrainingRecord(changedFirst), .. handoff.TrainingPairs.Skip(1)]
        };
        var changedInput = TranslationDpoTrainerBridge.BuildMutationBatchSnapshot(
            changedHandoff,
            BuildSettings()).Value!;

        Assert.NotEqual(baseline.TrainingContent.Sha256, changedSettings.TrainingContent.Sha256);
        Assert.NotEqual(baseline.TrainingContent.Sha256, changedInput.TrainingContent.Sha256);
        Assert.NotEqual(
            baseline.TrainingContent.Rows[0].Prompt,
            changedInput.TrainingContent.Rows[0].Prompt);
    }

    [Fact]
    public void TrainWithStepMany_RevokedAfterTrainerConstructionMapsAdmissionAndWritesNoArtifacts()
    {
        var handoff = BuildHandoff();
        using var admitted = BuildAdmittedTrainer(handoff);
        admitted.RevokeLease();

#pragma warning disable CS0618
        var result = TranslationDpoTrainerBridge.TrainWithStepMany(
            handoff,
            admitted.MutationSnapshot,
            admitted.Trainer,
            step: 17);
#pragma warning restore CS0618

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000017.json")));
        Assert.False(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(17))));
    }

    [Fact]
    public void TrainWithStepMany_ChangedFrozenTensorFailsIntegrityBeforeMutation()
    {
        var handoff = BuildHandoff();
        using var admitted = BuildAdmittedTrainer(handoff);
        admitted.MutationSnapshot.BatchBuild.Batches[0].ChosenIds.Data[0] += 1f;

#pragma warning disable CS0618
        var result = TranslationDpoTrainerBridge.TrainWithStepMany(
            handoff,
            admitted.MutationSnapshot,
            admitted.Trainer,
            step: 18);
#pragma warning restore CS0618

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_TRAINING_SNAPSHOT_INTEGRITY_FAILED", result.ErrorCode);
        Assert.False(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000018.json")));
        Assert.False(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(18))));
    }

    [Fact]
    public void BuildTrainDpoBatches_RejectsEmptyHandoffBeforeTraining()
    {
        var empty = BuildHandoff() with { TrainingPairs = [] };

        var result = TranslationDpoTrainerBridge.BuildTrainDpoBatches(empty, BuildSettings());

        Assert.False(result.IsSuccess);
        Assert.Equal("HANDOFF_EMPTY", result.ErrorCode);
    }

    [Fact]
    public void BuildTrainDpoBatches_RejectsHeldOutLeakageBeforeTraining()
    {
        var handoff = BuildHandoff();
        var leakedHeldOut = handoff.HeldOutEvaluationRows[0];
        var leakedTrainRecord = handoff.TrainingPairs[0] with
        {
            PairId = leakedHeldOut.PairId,
            Input = leakedHeldOut.Input
        };
        var leakedHandoff = handoff with
        {
            TrainingPairs = [leakedTrainRecord, .. handoff.TrainingPairs.Skip(1)]
        };

        var result = TranslationDpoTrainerBridge.BuildTrainDpoBatches(leakedHandoff, BuildSettings());

        Assert.False(result.IsSuccess);
        Assert.Equal("HELDOUT_LEAKAGE", result.ErrorCode);
    }

    [Fact]
    public void BuildHeldOutEvaluationDpoBatches_RejectsTrainRowsSuppliedAsHeldOut()
    {
        var handoff = BuildHandoff();
        var trainAsHeldOut = BuildHeldOutFromTrain(handoff.TrainingPairs[0]);
        var leakedHandoff = handoff with { HeldOutEvaluationRows = [trainAsHeldOut] };

        var result = TranslationDpoTrainerBridge.BuildHeldOutEvaluationDpoBatches(leakedHandoff, BuildSettings());

        Assert.False(result.IsSuccess);
        Assert.Equal("HELDOUT_LEAKAGE", result.ErrorCode);
    }

    [Fact]
    public void BuildTrainDpoBatches_RejectsClosedTeacherAndSemanticQualityClaims()
    {
        var handoff = BuildHandoff();
        var closedTeacher = handoff.TrainingPairs[0] with
        {
            TeacherProvenance = handoff.TrainingPairs[0].TeacherProvenance with
            {
                ClosedProprietaryTeacherUsed = true
            }
        };
        var semanticClaim = handoff.TrainingPairs[0] with { SemanticQualityClaimed = true };

        var closedResult = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            handoff with { TrainingPairs = [closedTeacher, .. handoff.TrainingPairs.Skip(1)] },
            BuildSettings());
        var semanticResult = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            handoff with { TrainingPairs = [semanticClaim, .. handoff.TrainingPairs.Skip(1)] },
            BuildSettings());

        Assert.False(closedResult.IsSuccess);
        Assert.Equal("ILLEGAL_TEACHER_PROVENANCE", closedResult.ErrorCode);
        Assert.False(semanticResult.IsSuccess);
        Assert.Equal("SEMANTIC_QUALITY_CLAIMED", semanticResult.ErrorCode);
    }

    [Fact]
    public void BuildHeldOutEvaluationDpoBatches_RejectsIllegalProvenanceSemanticClaimAndEmptyText()
    {
        var handoff = BuildHandoff();
        var heldOut = handoff.HeldOutEvaluationRows[0];
        var cases = new[]
        {
            (
                Row: heldOut with { SourceCategory = "external_teacher" },
                ErrorCode: "ILLEGAL_SOURCE_OR_LICENSE"
            ),
            (
                Row: heldOut with
                {
                    TeacherProvenance = heldOut.TeacherProvenance with
                    {
                        SourceType = "external_teacher"
                    }
                },
                ErrorCode: "ILLEGAL_TEACHER_PROVENANCE"
            ),
            (
                Row: heldOut with
                {
                    TeacherProvenance = heldOut.TeacherProvenance with
                    {
                        ClosedProprietaryTeacherUsed = true
                    }
                },
                ErrorCode: "ILLEGAL_TEACHER_PROVENANCE"
            ),
            (
                Row: heldOut with { SemanticQualityClaimed = true },
                ErrorCode: "SEMANTIC_QUALITY_CLAIMED"
            ),
            (
                Row: heldOut with { ExpectedResponse = "" },
                ErrorCode: "HELDOUT_RECORD_TEXT_REQUIRED"
            ),
            (
                Row: heldOut with { RejectedResponse = "" },
                ErrorCode: "HELDOUT_RECORD_TEXT_REQUIRED"
            )
        };

        foreach (var testCase in cases)
        {
            var result = TranslationDpoTrainerBridge.BuildHeldOutEvaluationDpoBatches(
                handoff with { HeldOutEvaluationRows = [testCase.Row, .. handoff.HeldOutEvaluationRows.Skip(1)] },
                BuildSettings());

            Assert.False(result.IsSuccess);
            Assert.Equal(testCase.ErrorCode, result.ErrorCode);
        }
    }

    [Fact]
    public void BuildTrainDpoBatches_RejectsIllegalSourceAndEmptyText()
    {
        var handoff = BuildHandoff();
        var illegalSource = handoff.TrainingPairs[0] with { SourceCategory = "external_teacher" };
        var emptyChosen = handoff.TrainingPairs[0] with
        {
            TrainerPair = handoff.TrainingPairs[0].TrainerPair with { Chosen = "" }
        };

        var illegalResult = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            handoff with { TrainingPairs = [illegalSource, .. handoff.TrainingPairs.Skip(1)] },
            BuildSettings());
        var emptyTextResult = TranslationDpoTrainerBridge.BuildTrainDpoBatches(
            handoff with { TrainingPairs = [emptyChosen, .. handoff.TrainingPairs.Skip(1)] },
            BuildSettings());

        Assert.False(illegalResult.IsSuccess);
        Assert.Equal("ILLEGAL_SOURCE_OR_LICENSE", illegalResult.ErrorCode);
        Assert.False(emptyTextResult.IsSuccess);
        Assert.Equal("TRAIN_RECORD_TEXT_REQUIRED", emptyTextResult.ErrorCode);
    }

    private static TranslationDpoTrainingHandoff BuildHandoff(
        TranslationSeedCorpus? corpus = null,
        IReadOnlyList<TranslationTrainReadyPair>? validatedTrainingDataArtifactPairs = null)
    {
        var sourceCorpus = corpus ?? LoadCorpus();
        var trainBatch = sourceCorpus.BuildStructuredBatch(
            "language-translate-seed-v1-train",
            TranslationSeedCorpusSplit.Train);
        return TranslationDpoTrainingHandoffBuilder.BuildSeedCorpusTrainHandoff(
            "language-translate-seed-v1-dpo-handoff",
            sourceCorpus,
            trainBatch,
            validatedTrainingDataArtifactPairs);
    }

    private static TranslationSeedCorpus LoadCorpus()
        => TranslationSeedCorpusLoader.Load(GetRepoSeedDataPath());

    private static TranslationDpoTrainingHandoff WithFirstTrainingRejected(
        TranslationDpoTrainingHandoff handoff,
        string rejected)
    {
        var first = handoff.TrainingPairs[0];
        var changedFirst = first with
        {
            TrainerPair = first.TrainerPair with { Rejected = rejected },
            RejectedResponse = rejected
        };
        return handoff with { TrainingPairs = [ReadmitTrainingRecord(changedFirst), .. handoff.TrainingPairs.Skip(1)] };
    }

    private static TranslationDpoTrainingHandoff WithFirstTrainingChosen(
        TranslationDpoTrainingHandoff handoff,
        string chosen)
    {
        var first = handoff.TrainingPairs[0];
        var changedFirst = first with
        {
            TrainerPair = first.TrainerPair with { Chosen = chosen },
            ChosenResponse = chosen
        };
        return handoff with { TrainingPairs = [ReadmitTrainingRecord(changedFirst), .. handoff.TrainingPairs.Skip(1)] };
    }

    private static TranslationDpoTrainingHandoffRecord ReadmitTrainingRecord(
        TranslationDpoTrainingHandoffRecord record)
    {
        var admitted = DpoTrainerPairCustoms.CreateAdmitted(
            record.TrainerPair.Prompt,
            record.TrainerPair.Chosen,
            record.TrainerPair.Rejected,
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.User,
            nameof(TranslationDpoTrainingHandoffBuilder));

        // Тестовые мутанты, которые меняют raw pair ради проверки tokenization,
        // должны получить новый паспорт; иначе это уже проверка защиты от mutation.
        return record with
        {
            TrainerPair = admitted.Pair,
            Admission = admitted.Admission
        };
    }

    private static TranslationDpoTokenizationSettings BuildSettings()
        => new(VocabSize: 512, ContextLength: 64);

    private static TranslationDpoHeldOutEvaluationRecord BuildHeldOutFromTrain(
        TranslationDpoTrainingHandoffRecord record)
        => new(
            PairId: record.PairId,
            LearningUnitId: record.LearningUnitId,
            CapabilityId: record.CapabilityId,
            Direction: record.Direction,
            SourceLanguage: record.SourceLanguage,
            TargetLanguage: record.TargetLanguage,
            Prompt: record.Prompt,
            Input: record.Input,
            ExpectedResponse: record.ChosenResponse,
            RejectedResponse: record.RejectedResponse,
            RejectionReason: record.RejectionReason,
            RejectionMarker: record.RejectionMarker,
            SourceCategory: record.SourceCategory,
            License: record.License,
            TeacherProvenance: record.TeacherProvenance,
            SemanticQualityClaimed: record.SemanticQualityClaimed);

    private static ModelConfig BuildTinyConfig()
        => new()
        {
            VocabSize = BuildSettings().VocabSize,
            ContextLen = BuildSettings().ContextLength,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 32
        };

    private AdmittedTrainerFixture BuildAdmittedTrainer(
        TranslationDpoTrainingHandoff handoff)
    {
        var mutationSnapshot = TranslationDpoTrainerBridge
            .BuildMutationBatchSnapshot(handoff, BuildSettings()).Value!;
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            new string('c', 64),
            16,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32)
                .Select(static value => (byte)value)
                .ToArray()).Value!;
        var authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(configuration, static () => true);
        var lease = ((IServerOwnedDpoTrainingLeaseBroker)authority)
            .AcquireTranslationTrainingLease(
                mutationSnapshot.TrainingContent,
                "translation-bridge-test")
            .Value!;
        var capability = lease.IssueDpoTrainerMutationAdmissionCapability()
            .Value!;
        return new AdmittedTrainerFixture(
            new DpoTrainer(
                new LlmModel(BuildTinyConfig()),
                new LlmModel(BuildTinyConfig()),
                _checkpointDir,
                capability),
            lease,
            mutationSnapshot);
    }

    private sealed class AdmittedTrainerFixture(
        DpoTrainer trainer,
        ServerOwnedLegalTrainingTeacherAdmissionLease lease,
        TranslationDpoMutationBatchSnapshot mutationSnapshot) : IDisposable
    {
        internal DpoTrainer Trainer { get; } = trainer;

        internal TranslationDpoMutationBatchSnapshot MutationSnapshot { get; } =
            mutationSnapshot;

        internal void RevokeLease() => lease.Dispose();

        public void Dispose() => lease.Dispose();
    }

    private static string SerializeTensorBits(string name, Tensor tensor)
    {
        var builder = new System.Text.StringBuilder(
            name.Length + 64 + tensor.Data.Length * 8);
        builder.Append(name);
        builder.Append(";f32;shape=");
        for (var index = 0; index < tensor.Shape.Dimensions.Length; index++)
        {
            if (index > 0)
                builder.Append(',');
            builder.Append(tensor.Shape.Dimensions[index]
                .ToString(CultureInfo.InvariantCulture));
        }
        builder.Append(";bits=");
        foreach (var value in tensor.Data)
        {
            builder.Append(BitConverter.SingleToInt32Bits(value)
                .ToString("x8", CultureInfo.InvariantCulture));
        }
        return builder.ToString();
    }

    private static string GetRepoSeedDataPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "27.TranslationDomain",
                "SeedData",
                SeedFileName);
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "27.TranslationDomain", "SeedData", SeedFileName);
    }
}
