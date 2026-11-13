namespace LlmFromScratch.Tests;

using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using TranslationDomain;

public sealed class TranslationSeedCorpusTests
{
    private const string SeedFileName = "language-translate-seed-v1.jsonl";

    [Fact]
    public void SeedCorpusLanguageTranslateV1_LoaderReadsAllRowsAndPreservesSplitCounts()
    {
        var corpus = LoadCorpus();
        var directions = corpus.Rows
            .Select(row => row.Direction)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(16, corpus.Rows.Count);
        Assert.Equal(12, corpus.TrainRows.Count);
        Assert.Equal(4, corpus.HeldOutRows.Count);
        Assert.Contains("EN_HE", directions);
        Assert.Contains("HE_EN", directions);
        Assert.Contains("EN_ES", directions);
        Assert.Contains("ES_EN", directions);
    }

    [Fact]
    public void SeedCorpusLanguageTranslateV1_RowsHaveRequiredDpoPairSchema()
    {
        var corpus = LoadCorpus();
        var ids = new HashSet<string>(StringComparer.Ordinal);

        foreach (var row in corpus.Rows)
        {
            Assert.StartsWith("SeedCorpus-LanguageTranslate-v1-", row.SeedId, StringComparison.Ordinal);
            Assert.True(ids.Add(row.SeedId), $"Duplicate seed_id: {row.SeedId}");
            Assert.False(string.IsNullOrWhiteSpace(row.SourceLanguage), row.SeedId);
            Assert.False(string.IsNullOrWhiteSpace(row.TargetLanguage), row.SeedId);
            Assert.False(string.IsNullOrWhiteSpace(row.InputText), row.SeedId);
            Assert.False(string.IsNullOrWhiteSpace(row.ChosenTranslation), row.SeedId);
            Assert.False(string.IsNullOrWhiteSpace(row.RejectedTranslation), row.SeedId);
            Assert.True(
                !string.IsNullOrWhiteSpace(row.RejectedReason) || !string.IsNullOrWhiteSpace(row.WrongMarker),
                $"{row.SeedId} must include rejected_reason or wrong_marker.");
        }
    }

    [Fact]
    public void SeedCorpusLanguageTranslateV1_UsesSelfAuthoredInternalProvenanceWithoutTeacherClaims()
    {
        var corpus = LoadCorpus();

        foreach (var row in corpus.Rows)
        {
            Assert.Equal("synthetic_self_authored", row.SourceCategory);
            Assert.Equal("project_internal_self_authored", row.License);
            Assert.False(row.SemanticQualityClaimed);
            Assert.Equal("synthetic_self_authored", row.TeacherProvenance.SourceType);
            Assert.Equal("xiigen_internal_seed_author", row.TeacherProvenance.Author);
            Assert.Equal("none", row.TeacherProvenance.ModelProvider);
            Assert.Equal("none", row.TeacherProvenance.ModelName);
            Assert.False(row.TeacherProvenance.ExternalApiUsed);
            Assert.False(row.TeacherProvenance.ClosedProprietaryTeacherUsed);
        }
    }

    [Fact]
    public void SeedCorpusLanguageTranslateV1_TrainRowsProjectToDpoPairsWithProvenance()
    {
        var corpus = LoadCorpus();
        var trainPairs = corpus.ToDpoPairs(TranslationSeedCorpusSplit.Train);

        Assert.Equal(12, trainPairs.Count);
        foreach (var pair in trainPairs)
        {
            var source = Assert.Single(corpus.TrainRows, row => row.SeedId == pair.PairId);
            Assert.Equal(source.InputText, pair.Input);
            Assert.Equal(source.ChosenTranslation, pair.Chosen);
            Assert.Equal(source.RejectedTranslation, pair.Rejected);
            Assert.False(string.IsNullOrWhiteSpace(pair.RejectedReason));
            Assert.False(string.IsNullOrWhiteSpace(pair.WrongMarker));
            Assert.Equal(source.Direction, pair.Direction);
            Assert.Equal("synthetic_self_authored", pair.SourceCategory);
            Assert.Equal("project_internal_self_authored", pair.License);
            Assert.False(pair.SemanticQualityClaimed);
            Assert.False(pair.TeacherProvenance.ClosedProprietaryTeacherUsed);
        }
    }

    [Fact]
    public void SeedCorpusLanguageTranslateV1_BuildsStructuredTrainBatchWithoutHeldOutLeakage()
    {
        var corpus = LoadCorpus();
        var batch = corpus.BuildStructuredBatch(
            "language-translate-seed-v1-train",
            TranslationSeedCorpusSplit.Train);
        var heldOutIds = corpus.HeldOutRows
            .Select(row => row.SeedId)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(TranslationMachine.DomainTag, batch.DomainTag);
        Assert.Equal(36, batch.Examples.Count);
        Assert.Equal(12, batch.SourceIds.Count);
        Assert.All(batch.Examples, example => Assert.False(example.HeldOut));
        Assert.DoesNotContain(batch.Examples, example => heldOutIds.Contains(example.SourceId));
        Assert.Contains(batch.Examples, example => example.UnitId == TranslationUnitIds.ExecutorFor(TranslationDirection.EN_HE));
        Assert.Contains(batch.Examples, example => example.UnitId == TranslationUnitIds.ExecutorFor(TranslationDirection.HE_EN));
        Assert.Contains(batch.Examples, example => example.UnitId == TranslationUnitIds.ExecutorFor(TranslationDirection.EN_ES));
        Assert.Contains(batch.Examples, example => example.UnitId == TranslationUnitIds.ExecutorFor(TranslationDirection.ES_EN));
        Assert.All(batch.Examples, example => Assert.Contains("[WRONG:", example.RejectedOutputJson));
    }

    [Fact]
    public async Task SeedCorpusLanguageTranslateV1_StructuredTrainBatchIsAcceptedByTranslationMachine()
    {
        var corpus = LoadCorpus();
        var trainBatch = corpus.BuildStructuredBatch(
            "language-translate-seed-v1-machine-train",
            TranslationSeedCorpusSplit.Train);

        var machine = new TranslationMachine();
        var result = await machine.TrainStructuredBatchAsync(trainBatch);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(36, result.Value!.RowsTrained);
        Assert.Equal(0.0, result.Value.RewardMargin, precision: 5);
        Assert.Contains(TranslationUnitIds.SemanticTokenizer, result.Value.PromotedUnitIds);
        Assert.Contains(TranslationUnitIds.QualityVerifier, result.Value.PromotedUnitIds);
        Assert.Contains(TranslationUnitIds.ExecutorFor(TranslationDirection.EN_ES), result.Value.PromotedUnitIds);
        Assert.Contains(TranslationUnitIds.ExecutorFor(TranslationDirection.ES_EN), result.Value.PromotedUnitIds);
    }

    [Fact]
    public void SeedCorpusLanguageTranslateV1_HeldOutRowsPreserveExpectedLabelsAndBatchFlag()
    {
        var corpus = LoadCorpus();
        var heldOutBatch = corpus.BuildStructuredBatch(
            "language-translate-seed-v1-heldout",
            TranslationSeedCorpusSplit.HeldOut);

        Assert.Equal(12, heldOutBatch.Examples.Count);
        Assert.All(heldOutBatch.Examples, example => Assert.True(example.HeldOut));
        foreach (var row in corpus.HeldOutRows)
        {
            Assert.False(string.IsNullOrWhiteSpace(row.ExpectedLabel), row.SeedId);
            Assert.Equal(NormalizeExpectedAnswer(row.ChosenTranslation), NormalizeExpectedAnswer(row.ExpectedLabel!));
            Assert.Equal(1.0, CharacterAccuracy(row.ChosenTranslation, row.ExpectedLabel!), precision: 5);
        }
    }

    [Fact]
    public void SeedCorpusLanguageTranslateV1_InvalidMissingRequiredFieldIsRejected()
    {
        var firstLine = File.ReadLines(GetRepoSeedDataPath(), Encoding.UTF8)
            .First(line => !string.IsNullOrWhiteSpace(line));
        var obj = JsonNode.Parse(firstLine)!.AsObject();
        Assert.True(obj.Remove("chosen_translation"));

        var exception = Assert.Throws<InvalidOperationException>(
            () => TranslationSeedCorpusLoader.ParseJsonl(obj.ToJsonString(), "broken-seed"));

        Assert.Contains("missing required field 'chosen_translation'", exception.Message);
    }

    [Fact]
    public void SeedCorpusLanguageTranslateV1_DoesNotLeakTrainIdsOrInputsIntoHeldOut()
    {
        var corpus = LoadCorpus();
        var trainIds = corpus.TrainRows.Select(row => row.SeedId).ToHashSet(StringComparer.Ordinal);
        var trainInputs = corpus.TrainRows
            .Select(row => NormalizeExpectedAnswer(row.InputText))
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain(corpus.HeldOutRows, row => trainIds.Contains(row.SeedId));
        Assert.DoesNotContain(corpus.HeldOutRows, row => trainInputs.Contains(NormalizeExpectedAnswer(row.InputText)));
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

    private static string NormalizeExpectedAnswer(string value)
    {
        var normalized = (value ?? string.Empty)
            .Normalize(NormalizationForm.FormC)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Trim();
        return Regex.Replace(normalized, "[\t\n\f\r ]+", " ");
    }

    private static double CharacterAccuracy(string actual, string expected)
    {
        var actualNormalized = NormalizeExpectedAnswer(actual);
        var expectedNormalized = NormalizeExpectedAnswer(expected);
        if (actualNormalized.Length == 0 || expectedNormalized.Length == 0)
            return actualNormalized.Length == expectedNormalized.Length ? 1.0 : 0.0;

        var limit = Math.Min(actualNormalized.Length, expectedNormalized.Length);
        var matches = 0;
        for (var index = 0; index < limit; index++)
            if (actualNormalized[index] == expectedNormalized[index])
                matches++;

        return matches / (double)Math.Max(actualNormalized.Length, expectedNormalized.Length);
    }
}
