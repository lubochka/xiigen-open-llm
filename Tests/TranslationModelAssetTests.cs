// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TranslationDomain;

public sealed class TranslationModelAssetTests
{
    [Fact]
    public void TranslationModelAsset_ManifestContract_UsesSafeRelativeSchemaFields()
    {
        var manifest = TranslationModelManifestContract.Create(
            accuracyAtExport: 0.87,
            sourceBaseCommit: "test-commit",
            createdAtUtc: DateTimeOffset.Parse("2026-06-02T00:00:00Z"));

        Assert.True(TranslationModelManifestContract.IsValidSharedAssetManifest(manifest));
        Assert.Equal("translation-v1", manifest.ModelId);
        Assert.Equal("TranslationDomain", manifest.Domain);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.Equal("translation-v1-checkpoint.json", manifest.CheckpointFilename);
        Assert.Equal("runner-report.json", manifest.RunnerReportRef);
        Assert.Equal("runner-report.json", manifest.RunnerReportFilename);
        Assert.Equal("training-data/translation/translation-v1/training-manifest.json", manifest.TrainingDataRef);
        Assert.Equal("TranslationMachine", manifest.LoaderType);
        Assert.Equal(1, manifest.SchemaVersion);
        Assert.False(Path.IsPathRooted(manifest.CheckpointFilename));
        Assert.False(Path.IsPathRooted(manifest.RunnerReportRef));
        Assert.False(Path.IsPathRooted(manifest.TrainingDataRef));

        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal("translation-v1", root.GetProperty("model_id").GetString());
        Assert.Equal("TranslationDomain", root.GetProperty("domain").GetString());
        Assert.Equal("translation-v1-checkpoint.json", root.GetProperty("checkpoint_filename").GetString());
        Assert.Equal("runner-report.json", root.GetProperty("runner_report_ref").GetString());
        Assert.Equal("training-data/translation/translation-v1/training-manifest.json", root.GetProperty("training_data_ref").GetString());
        Assert.Equal("TranslationMachine", root.GetProperty("loader_type").GetString());
        Assert.True(root.GetProperty("accuracy_at_export").GetDouble() > 0.0);
        Assert.Equal(1, root.GetProperty("schema_version").GetInt32());
        Assert.DoesNotContain("C:\\", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private-corpora", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TranslationModelAsset_CheckpointContract_PreservesTrainedStateWithoutPrivatePaths()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-asset-test", Guid.NewGuid().ToString("N"));
        var checkpointPath = Path.Combine(root, TranslationModelManifestContract.CheckpointFilename);
        var source = "add null check ValidateToken";
        try
        {
            Directory.CreateDirectory(root);
            var machine = new TranslationMachine();
            var train = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
                BatchId: "translation-v1-checkpoint-contract",
                DomainTag: TranslationMachine.DomainTag,
                Examples:
                [
                    BuildExecutorExample(
                        "asset-executor",
                        source,
                        "IMPERATIVE_V|ADD_OP|NULL_LITERAL|VALIDATE_CHECK|METHOD_REF",
                        "Add a null check to ValidateToken.",
                        "wrong_missing_null_check")
                ],
                SourceIds: ["translation-v1-asset-contract"]));
            Assert.True(train.IsSuccess, train.ErrorMessage);
            Assert.True(train.Value!.RowsTrained > 0);
            Assert.True(train.Value.AccuracyAfter > 0.0);

            var save = await machine.SaveCheckpointAsync(checkpointPath);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            Assert.Equal("translation-v1-checkpoint.json", Path.GetFileName(checkpointPath));

            var json = await File.ReadAllTextAsync(checkpointPath);
            Assert.DoesNotContain("private-corpora", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:\\Users\\", json, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"knn_store\"", json);
            Assert.Contains("\"semantic_tokenizer\"", json);
            Assert.Contains("\"executors\"", json);
            Assert.Contains("\"quality_verifier\"", json);
            Assert.Contains("\"training_history\"", json);

            var checkpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                json,
                TranslationCheckpointJsonOptions.Standard);
            Assert.NotNull(checkpoint);
            Assert.True(checkpoint!.HasStructuredTraining);
            Assert.True(checkpoint.KnnStore.Entries.Count > 0);
            Assert.NotEmpty(checkpoint.Executors);
            Assert.Contains(checkpoint.Executors, executor => executor.Direction == TranslationDirection.RU_EN);
            Assert.NotEmpty(checkpoint.TrainingHistory);

            var loaded = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.Null(loaded.Value!.TatoebaLoop);

            var translated = await RunTranslationAsync(loaded.Value.Machine, source);
            Assert.DoesNotContain("[RU_EN:BOOTSTRAP]", translated);
            Assert.Equal("Add a null check to ValidateToken.", translated);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TranslationModelAsset_TrainingDataContract_UsesHashedSafeCorpusSnapshot()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-training-data-test", Guid.NewGuid().ToString("N"));
        var corpusPath = Path.Combine(root, TranslationTrainingDataContract.CorpusFilename);
        try
        {
            Directory.CreateDirectory(root);
            var rows = new[]
            {
                new
                {
                    pair_id = "translation-v1-safe-001",
                    source = "add null check",
                    target = "Add a null check.",
                    direction = "RU_EN"
                },
                new
                {
                    pair_id = "translation-v1-safe-002",
                    source = "launch worker thread",
                    target = "Launch worker thread.",
                    direction = "RU_EN"
                }
            };

            await using (var file = File.Create(corpusPath))
            await using (var gzip = new GZipStream(file, CompressionLevel.SmallestSize))
            await using (var writer = new StreamWriter(gzip, Encoding.UTF8))
            {
                foreach (var row in rows)
                    await writer.WriteLineAsync(JsonSerializer.Serialize(row, JsonOptions));
            }

            var corpusSha = Sha256File(corpusPath);
            var manifest = TranslationTrainingDataContract.CreateManifest(
                corpusSha,
                rows.Length,
                DateTimeOffset.Parse("2026-06-02T00:00:00Z"));

            Assert.True(TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(manifest));
            Assert.Equal("translation-v1", manifest.LinkedModelVersion);
            Assert.Equal("corpus.jsonl.gz", manifest.CorpusFilename);
            Assert.Equal("corpus.jsonl.gz", manifest.CorpusRef);
            Assert.Equal(corpusSha, manifest.CorpusSha256);
            Assert.Equal(rows.Length, manifest.TrainingPairsCount);
            Assert.Equal("gzip", manifest.Compression);
            Assert.Equal("application/x-jsonlines", manifest.ContentType);
            Assert.False(Path.IsPathRooted(manifest.CorpusFilename));
            Assert.False(Path.IsPathRooted(manifest.CorpusRef));

            await using (var file = File.OpenRead(corpusPath))
            await using (var gzip = new GZipStream(file, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzip, Encoding.UTF8))
            {
                var lines = new List<string>();
                while (await reader.ReadLineAsync() is { } line)
                    lines.Add(line);

                Assert.Equal(rows.Length, lines.Count);
                Assert.All(lines, line => Assert.DoesNotContain("C:\\", line, StringComparison.OrdinalIgnoreCase));
                Assert.All(lines, line => Assert.DoesNotContain("private-corpora", line, StringComparison.OrdinalIgnoreCase));
            }

            var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            using var document = JsonDocument.Parse(manifestJson);
            var rootElement = document.RootElement;
            Assert.Equal(1, rootElement.GetProperty("schema_version").GetInt32());
            Assert.Equal("translation-v1", rootElement.GetProperty("linked_model_version").GetString());
            Assert.Equal("corpus.jsonl.gz", rootElement.GetProperty("corpus_filename").GetString());
            Assert.Equal("corpus.jsonl.gz", rootElement.GetProperty("corpus_ref").GetString());
            Assert.Equal(corpusSha, rootElement.GetProperty("corpus_sha256").GetString());
            Assert.True(rootElement.GetProperty("training_pairs_count").GetInt32() > 0);
            Assert.DoesNotContain("C:\\", manifestJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private-corpora", manifestJson, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static TranslationLearningExample BuildExecutorExample(
        string id,
        string sourceText,
        string signature,
        string translatedText,
        string rejectedType)
        => new(
            ExampleId: id,
            UnitId: TranslationUnitIds.ExecutorRuEn,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new ExecutorInput(sourceText, "ru", "technical"), JsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(new ExecutorOutput(
                translatedText,
                0.95,
                null,
                null), JsonOptions),
            RejectedOutputJson: $"{{\"text\":\"missing required token\",\"wrong\":\"[WRONG: {rejectedType} - checkpoint contract negative.]\"}}",
            RejectedType: rejectedType,
            SemanticSignature: signature,
            SourceId: "translation-v1-asset-contract",
            HeldOut: false);

    private static async Task<string> RunTranslationAsync(
        TranslationMachine machine,
        string sourceText,
        string sourceLanguage = "ru",
        string targetLanguage = "en")
    {
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);
        var result = await orchestrator.Value!.RunAsync(new AgentInfrastructure.Logical.LogicalOrchestratorRequest(
            RunId: $"asset-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: sourceText,
            Flags: new AgentInfrastructure.Logical.LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = sourceLanguage,
                ["target_language"] = targetLanguage,
                ["topic_domain"] = "technical"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!.FinalHumanResult;
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
}
