namespace LlmFromScratch.Tests;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using DPO.Models;
using TranslationDomain;

public sealed class TranslationHandoffPerSideAuthorshipTests
{
    private const string SeedFileName = "language-translate-seed-v1.jsonl";

    [Fact]
    public void BuildSeedCorpusTrainHandoff_AdmitsTrainPairsWithPerSideUserAuthorshipAndBuilderIssuer()
    {
        var corpus = LoadCorpus();
        var handoff = BuildHandoff(corpus);

        Assert.NotEmpty(handoff.TrainingPairs);
        foreach (var record in handoff.TrainingPairs)
        {
            Assert.NotNull(record.Admission);
            Assert.Equal(DpoPairSideAuthorKind.User, record.Admission.ChosenAuthor);
            Assert.Equal(DpoPairSideAuthorKind.User, record.Admission.RejectedAuthor);
            Assert.Equal(
                nameof(TranslationDpoTrainingHandoffBuilder),
                record.Admission.IssuerName);
            Assert.False(record.Admission.HasPaidExternalModelSide);

            Assert.Equal(
                ComputeAdmissionFingerprint(
                    record.TrainerPair.Prompt,
                    record.TrainerPair.Chosen,
                    record.TrainerPair.Rejected),
                record.Admission.ContentFingerprintSha256);
        }
    }

    [Fact]
    public void BuildSeedCorpusTrainHandoff_AdmitsValidatedTrainReadyReplacementWithReplacementFingerprintAndRowAuthorship()
    {
        var corpus = LoadCorpus();
        var sourceRow = corpus.TrainRows[0];
        var trainReadyPairs = corpus.ToTrainReadyPairs(TranslationSeedCorpusSplit.Train);
        var sourcePair = trainReadyPairs[0];
        var replacement = sourcePair with
        {
            Prompt = sourcePair.Prompt + " Keep project terminology literal.",
            ASide = sourcePair.ASide + " Context: admitted artifact replacement.",
            Chosen = "The admitted replacement uses the human-authored wording.",
            Rejected = "The admitted replacement drops the project term. [WRONG: replacement_marker - term was dropped]"
        };
        var replacements = trainReadyPairs
            .Select(pair => pair.Id == replacement.Id ? replacement : pair)
            .ToArray();

        var handoff = BuildHandoff(corpus, replacements);
        var record = Assert.Single(
            handoff.TrainingPairs,
            pair => pair.PairId == replacement.Id);

        Assert.Equal(sourceRow.TeacherProvenance, record.TeacherProvenance);
        Assert.NotNull(record.Admission);
        Assert.Equal(DpoPairSideAuthorKind.User, record.Admission.ChosenAuthor);
        Assert.Equal(DpoPairSideAuthorKind.User, record.Admission.RejectedAuthor);
        Assert.Equal($"{replacement.Prompt}\nInput: {replacement.ASide}", record.TrainerPair.Prompt);
        Assert.Equal(replacement.Chosen, record.TrainerPair.Chosen);
        Assert.Equal(replacement.Rejected, record.TrainerPair.Rejected);
        Assert.Equal(
            ComputeAdmissionFingerprint(
                record.TrainerPair.Prompt,
                record.TrainerPair.Chosen,
                record.TrainerPair.Rejected),
            record.Admission.ContentFingerprintSha256);
    }

    [Fact]
    public void BuildSeedCorpusTrainHandoff_RejectsUnknownExternalClosedContradictoryAndBlankAuthorshipBeforeAdmission()
    {
        var corpus = LoadCorpus();
        var cases = new (string Name, Func<TranslationSeedCorpusRow, TranslationSeedCorpusRow> Mutate)[]
        {
            (
                "unknown source type",
                row => row with
                {
                    TeacherProvenance = row.TeacherProvenance with
                    {
                        SourceType = "unknown"
                    }
                }
            ),
            (
                "external api",
                row => row with
                {
                    TeacherProvenance = row.TeacherProvenance with
                    {
                        ExternalApiUsed = true
                    }
                }
            ),
            (
                "closed teacher",
                row => row with
                {
                    TeacherProvenance = row.TeacherProvenance with
                    {
                        ClosedProprietaryTeacherUsed = true
                    }
                }
            ),
            (
                "contradictory category",
                row => row with
                {
                    SourceCategory = "external_teacher"
                }
            ),
            (
                "blank author",
                row => row with
                {
                    TeacherProvenance = row.TeacherProvenance with
                    {
                        Author = " "
                    }
                }
            )
        };

        foreach (var testCase in cases)
        {
            var invalidCorpus = ReplaceFirstTrainRow(corpus, testCase.Mutate);

            var exception = Assert.Throws<InvalidOperationException>(
                () => BuildHandoff(invalidCorpus));

            Assert.DoesNotContain(
                "pair admission",
                exception.Message,
                StringComparison.OrdinalIgnoreCase);
        }
    }

    private static TranslationDpoTrainingHandoff BuildHandoff(
        TranslationSeedCorpus corpus,
        IReadOnlyList<TranslationTrainReadyPair>? replacementPairs = null)
    {
        var trainBatch = corpus.BuildStructuredBatch(
            "language-translate-seed-v1-train",
            TranslationSeedCorpusSplit.Train);
        return TranslationDpoTrainingHandoffBuilder.BuildSeedCorpusTrainHandoff(
            "language-translate-seed-v1-dpo-handoff",
            corpus,
            trainBatch,
            replacementPairs);
    }

    private static TranslationSeedCorpus ReplaceFirstTrainRow(
        TranslationSeedCorpus corpus,
        Func<TranslationSeedCorpusRow, TranslationSeedCorpusRow> mutate)
    {
        var firstTrainId = corpus.TrainRows[0].SeedId;
        return new TranslationSeedCorpus(corpus.Rows
            .Select(row => row.SeedId == firstTrainId ? mutate(row) : row)
            .ToArray());
    }

    private static TranslationSeedCorpus LoadCorpus()
        => TranslationSeedCorpusLoader.Load(GetRepoSeedDataPath());

    private static string ComputeAdmissionFingerprint(
        string prompt,
        string chosen,
        string rejected)
    {
        using var stream = new MemoryStream();
        var domainBytes = Encoding.ASCII.GetBytes(
            "DPO_TRAINER_PAIR_ADMISSION_CONTENT_SHA256_V1");

        WriteLittleEndianInt32(stream, domainBytes.Length);
        stream.Write(domainBytes);
        WriteFramedUtf8Field(stream, "prompt", prompt);
        WriteFramedUtf8Field(stream, "chosen", chosen);
        WriteFramedUtf8Field(stream, "rejected", rejected);

        return Convert.ToHexString(SHA256.HashData(stream.ToArray()))
            .ToLowerInvariant();
    }

    private static void WriteFramedUtf8Field(
        Stream stream,
        string fieldName,
        string value)
    {
        var fieldNameBytes = Encoding.UTF8.GetBytes(fieldName);
        var valueBytes = Encoding.UTF8.GetBytes(value);

        WriteLittleEndianInt32(stream, fieldNameBytes.Length);
        stream.Write(fieldNameBytes);
        WriteLittleEndianInt64(stream, valueBytes.LongLength);
        stream.Write(valueBytes);
    }

    private static void WriteLittleEndianInt32(Stream stream, int value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        stream.Write(buffer);
    }

    private static void WriteLittleEndianInt64(Stream stream, long value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        stream.Write(buffer);
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
