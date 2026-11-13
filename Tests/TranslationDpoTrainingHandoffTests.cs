namespace LlmFromScratch.Tests;

using System.Text;
using DPO.Models;
using TranslationDomain;

public sealed class TranslationDpoTrainingHandoffTests
{
    private const string SeedFileName = "language-translate-seed-v1.jsonl";

    [Fact]
    public void TranslationDpoTrainingHandoff_MapsTrainRowsToExistingDpoPairsAndExcludesHeldOut()
    {
        var corpus = LoadCorpus();
        var handoff = BuildHandoff(corpus);
        var heldOutIds = corpus.HeldOutRows.Select(row => row.SeedId).ToHashSet(StringComparer.Ordinal);
        var trainIds = corpus.TrainRows.Select(row => row.SeedId).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(corpus.TrainRows.Count, handoff.TrainingPairs.Count);
        Assert.Equal(corpus.HeldOutRows.Count, handoff.HeldOutEvaluationRows.Count);
        Assert.Equal(TranslationMachine.DomainTag, handoff.DomainTag);
        Assert.Equal(TranslationMachine.MachineId, handoff.MachineId);
        Assert.All(handoff.TrainingPairs, pair => Assert.IsType<DpoPair>(pair.TrainerPair));
        Assert.DoesNotContain(handoff.TrainingPairs, pair => heldOutIds.Contains(pair.PairId));
        Assert.DoesNotContain(handoff.HeldOutEvaluationRows, row => trainIds.Contains(row.PairId));
    }

    [Fact]
    public void TranslationDpoTrainingHandoff_PreservesFieldMappingAndLegalProvenance()
    {
        var corpus = LoadCorpus();
        var source = corpus.TrainRows.First();
        var trainReady = TranslationSeedCorpusLoader.ToTrainReadyPair(source);
        var handoff = BuildHandoff(corpus);
        var record = Assert.Single(handoff.TrainingPairs, pair => pair.PairId == source.SeedId);

        Assert.Equal(TranslationUnitIds.ExecutorFor(Enum.Parse<TranslationDirection>(source.Direction)), record.LearningUnitId);
        Assert.Equal($"{TranslationMachine.MachineId}:{TranslationRoutingFlow.FlowId}:{source.Direction}", record.CapabilityId);
        Assert.Equal(source.Direction, record.Direction);
        Assert.Equal(source.SourceLanguage, record.SourceLanguage);
        Assert.Equal(source.TargetLanguage, record.TargetLanguage);
        Assert.Equal(trainReady.Prompt, record.Prompt);
        Assert.Equal(source.InputText, record.Input);
        Assert.Equal(source.ChosenTranslation, record.ChosenResponse);
        Assert.Equal(source.RejectedTranslation, record.RejectedResponse);
        Assert.False(string.IsNullOrWhiteSpace(record.RejectionReason));
        Assert.False(string.IsNullOrWhiteSpace(record.RejectionMarker));
        Assert.Equal("synthetic_self_authored", record.SourceCategory);
        Assert.Equal("project_internal_self_authored", record.License);
        Assert.Equal(source.TeacherProvenance, record.TeacherProvenance);
        Assert.False(record.SemanticQualityClaimed);
        Assert.False(handoff.SemanticQualityClaimed);

        Assert.Contains(source.InputText, record.TrainerPair.Prompt, StringComparison.Ordinal);
        Assert.Equal(trainReady.Chosen, record.TrainerPair.Chosen);
        Assert.Equal(trainReady.Rejected, record.TrainerPair.Rejected);
        Assert.Contains("[WRONG:", record.TrainerPair.Rejected, StringComparison.Ordinal);
        Assert.NotNull(record.Admission);
        Assert.Equal(DpoPairSideAuthorKind.User, record.Admission.ChosenAuthor);
        Assert.Equal(DpoPairSideAuthorKind.User, record.Admission.RejectedAuthor);
        Assert.Equal(
            nameof(TranslationDpoTrainingHandoffBuilder),
            record.Admission.IssuerName);
        Assert.False(record.Admission.HasPaidExternalModelSide);
    }

    [Fact]
    public void TranslationDpoTrainingHandoff_HeldOutRowsRemainEvaluationOnly()
    {
        var corpus = LoadCorpus();
        var handoff = BuildHandoff(corpus);

        foreach (var heldOut in corpus.HeldOutRows)
        {
            var row = Assert.Single(handoff.HeldOutEvaluationRows, candidate => candidate.PairId == heldOut.SeedId);
            Assert.Equal(heldOut.ExpectedLabel, row.ExpectedResponse);
            Assert.Equal(heldOut.RejectedTranslation, row.RejectedResponse);
            Assert.Equal(heldOut.InputText, row.Input);
            Assert.Equal(heldOut.Direction, row.Direction);
            Assert.Equal(TranslationUnitIds.ExecutorFor(Enum.Parse<TranslationDirection>(heldOut.Direction)), row.LearningUnitId);
            Assert.False(row.SemanticQualityClaimed);
            Assert.DoesNotContain(handoff.TrainingPairs, pair => pair.PairId == heldOut.SeedId);
        }

        Assert.Null(typeof(TranslationDpoHeldOutEvaluationRecord).GetProperty("Admission"));
    }

    [Fact]
    public void TranslationDpoTrainingHandoff_RejectsHeldOutBatchBeforeTrainerHandoffCreation()
    {
        var corpus = LoadCorpus();
        var heldOutBatch = corpus.BuildStructuredBatch(
            "language-translate-seed-v1-heldout-not-training",
            TranslationSeedCorpusSplit.HeldOut);

        var exception = Assert.Throws<InvalidOperationException>(
            () => TranslationDpoTrainingHandoffBuilder.BuildSeedCorpusTrainHandoff(
                "language-translate-seed-v1-dpo-handoff",
                corpus,
                heldOutBatch));

        Assert.Contains("rejects held-out examples", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TranslationDpoTrainingHandoff_RejectsIllegalClosedTeacherProvenance()
    {
        var corpus = LoadCorpus();
        var firstTrainId = corpus.TrainRows[0].SeedId;
        var rows = corpus.Rows
            .Select(row => row.SeedId == firstTrainId
                ? row with
                {
                    TeacherProvenance = row.TeacherProvenance with
                    {
                        ClosedProprietaryTeacherUsed = true
                    }
                }
                : row)
            .ToArray();
        var illegalCorpus = new TranslationSeedCorpus(rows);
        var trainBatch = corpus.BuildStructuredBatch(
            "language-translate-seed-v1-train",
            TranslationSeedCorpusSplit.Train);

        var exception = Assert.Throws<InvalidOperationException>(
            () => TranslationDpoTrainingHandoffBuilder.BuildSeedCorpusTrainHandoff(
                "language-translate-seed-v1-dpo-handoff",
                illegalCorpus,
                trainBatch));

        Assert.Contains("closed proprietary teacher", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TranslationDpoTrainingHandoff_ExposesRealTrainerConnectionGapWithoutTrainingClaim()
    {
        var corpus = LoadCorpus();
        var handoff = BuildHandoff(corpus);

        Assert.Equal("DPO.Models.DpoPair", handoff.ExistingDpoPairContract);
        Assert.Equal("DPO.Models.DpoBatch", handoff.ExistingTrainerInputContract);
        Assert.False(handoff.ProducesTensorDpoBatches);
        Assert.Contains("DpoTrainer.StepMany", handoff.NextRealTrainerConnectionGap, StringComparison.Ordinal);
        Assert.Contains("without adding held-out rows", handoff.NextRealTrainerConnectionGap, StringComparison.Ordinal);
    }

    private static TranslationDpoTrainingHandoff BuildHandoff(TranslationSeedCorpus corpus)
    {
        var trainBatch = corpus.BuildStructuredBatch(
            "language-translate-seed-v1-train",
            TranslationSeedCorpusSplit.Train);
        return TranslationDpoTrainingHandoffBuilder.BuildSeedCorpusTrainHandoff(
            "language-translate-seed-v1-dpo-handoff",
            corpus,
            trainBatch);
    }

    private static TranslationSeedCorpus LoadCorpus()
        => TranslationSeedCorpusLoader.Load(GetRepoSeedDataPath());

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
