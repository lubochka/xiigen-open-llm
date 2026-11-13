// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TranslationDomain;

public sealed class TranslationModelAssetTests
{
    [Fact]
    public async Task TranslationModelAsset_ManifestContract_UsesSafeRelativeSchemaFields()
    {
        var artifactRoot = Path.Combine(Path.GetTempPath(), "xiigen-translation-model-manifest-contract-test", Guid.NewGuid().ToString("N"));
        try
        {
            var artifact = await CreateValidatedTrainingDataArtifactAsync(artifactRoot);
            var manifest = TranslationModelManifestContract.Create(
                accuracyAtExport: 0.87,
                sourceBaseCommit: "test-commit",
                createdAtUtc: DateTimeOffset.Parse("2026-06-02T00:00:00Z"),
                trainingDataEvidence: TranslationModelManifestContract.CreateTrainingDataEvidence(artifact));

            Assert.True(TranslationModelManifestContract.IsValidSharedAssetManifest(manifest));
            Assert.Equal("translation-v1", manifest.ModelId);
            Assert.Equal("TranslationDomain", manifest.Domain);
            Assert.Equal("1.0.0", manifest.Version);
            Assert.Equal("translation-v1-checkpoint.json", manifest.CheckpointFilename);
            Assert.Equal("runner-report.json", manifest.RunnerReportRef);
            Assert.Equal("runner-report.json", manifest.RunnerReportFilename);
            Assert.Equal("training-data/translation/translation-v1/training-manifest.json", manifest.TrainingDataRef);
            Assert.Equal("training-data/translation/translation-v1/training-manifest.json", manifest.TrainingDataEvidence.ManifestPath);
            Assert.Equal("training-data/translation/translation-v1/corpus.jsonl.gz", manifest.TrainingDataEvidence.CorpusPath);
            Assert.Equal(artifact.CorpusSha256, manifest.TrainingDataEvidence.CorpusSha256);
            Assert.Equal(artifact.CorpusRowsCount, manifest.TrainingDataEvidence.CorpusRowsCount);
            Assert.True(manifest.TrainingDataEvidence.CommonTrainReady);
            Assert.True(manifest.TrainingDataEvidence.DpoTrainReady);
            Assert.True(manifest.TrainingDataEvidence.LegalCommonDpoManifestValidated);
            Assert.Equal("ACCEPT", manifest.TrainingDataEvidence.LegalVerdict);
            Assert.Equal("TranslationMachine", manifest.LoaderType);
            Assert.Equal(1, manifest.SchemaVersion);
            Assert.False(Path.IsPathRooted(manifest.CheckpointFilename));
            Assert.False(Path.IsPathRooted(manifest.RunnerReportRef));
            Assert.False(Path.IsPathRooted(manifest.TrainingDataRef));
            Assert.False(Path.IsPathRooted(manifest.TrainingDataEvidence.ManifestPath));
            Assert.False(Path.IsPathRooted(manifest.TrainingDataEvidence.CorpusPath));

            var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            Assert.Equal("translation-v1", root.GetProperty("model_id").GetString());
            Assert.Equal("TranslationDomain", root.GetProperty("domain").GetString());
            Assert.Equal("translation-v1-checkpoint.json", root.GetProperty("checkpoint_filename").GetString());
            Assert.Equal("runner-report.json", root.GetProperty("runner_report_ref").GetString());
            Assert.Equal("training-data/translation/translation-v1/training-manifest.json", root.GetProperty("training_data_ref").GetString());
            var evidence = root.GetProperty("training_data_evidence");
            Assert.Equal("training-data/translation/translation-v1/training-manifest.json", evidence.GetProperty("manifest_path").GetString());
            Assert.Equal("training-data/translation/translation-v1/corpus.jsonl.gz", evidence.GetProperty("corpus_path").GetString());
            Assert.Equal(artifact.CorpusSha256, evidence.GetProperty("corpus_sha256").GetString());
            Assert.Equal(artifact.CorpusRowsCount, evidence.GetProperty("corpus_rows_count").GetInt32());
            Assert.True(evidence.GetProperty("common_train_ready").GetBoolean());
            Assert.True(evidence.GetProperty("dpo_train_ready").GetBoolean());
            Assert.True(evidence.GetProperty("legal_common_dpo_manifest_validated").GetBoolean());
            Assert.Equal("ACCEPT", evidence.GetProperty("legal_verdict").GetString());
            Assert.Equal("TranslationMachine", root.GetProperty("loader_type").GetString());
            Assert.True(root.GetProperty("accuracy_at_export").GetDouble() > 0.0);
            Assert.Equal(1, root.GetProperty("schema_version").GetInt32());
            Assert.DoesNotContain("C:\\", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private-corpora", json, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(artifactRoot))
                Directory.Delete(artifactRoot, recursive: true);
        }
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
            Assert.Equal("synthetic_self_authored", manifest.SourceCategory);
            Assert.Equal("translation_seed_dpo_pairs", manifest.DataClass);
            Assert.Equal("CommonGlobal", manifest.StorageScope);
            Assert.Equal("common_allowed", manifest.PrivacyScope);
            Assert.True(manifest.CommonTrainReady);
            Assert.True(manifest.DpoTrainReady);
            Assert.Equal("seed-corpus/translation/language-translate-seed-v1", manifest.ProvenanceRef);
            Assert.Equal("train_only_source_disjoint_from_held_out", manifest.SplitPolicy);
            Assert.Equal("language-translate-seed-v1-train", manifest.SourceDisjointGroup);
            Assert.Equal("project_internal_self_authored", manifest.LicenseOrProjectInternalRef);
            Assert.Equal("ACCEPT", manifest.LegalVerdict);
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
            Assert.Equal("synthetic_self_authored", rootElement.GetProperty("source_category").GetString());
            Assert.Equal("translation_seed_dpo_pairs", rootElement.GetProperty("data_class").GetString());
            Assert.Equal("CommonGlobal", rootElement.GetProperty("storage_scope").GetString());
            Assert.Equal("common_allowed", rootElement.GetProperty("privacy_scope").GetString());
            Assert.True(rootElement.GetProperty("common_train_ready").GetBoolean());
            Assert.True(rootElement.GetProperty("dpo_train_ready").GetBoolean());
            Assert.Equal("seed-corpus/translation/language-translate-seed-v1", rootElement.GetProperty("provenance_ref").GetString());
            Assert.Equal("train_only_source_disjoint_from_held_out", rootElement.GetProperty("split_policy").GetString());
            Assert.Equal("language-translate-seed-v1-train", rootElement.GetProperty("source_disjoint_group").GetString());
            Assert.Equal("project_internal_self_authored", rootElement.GetProperty("license_or_project_internal_ref").GetString());
            Assert.Equal("ACCEPT", rootElement.GetProperty("legal_verdict").GetString());
            Assert.DoesNotContain("C:\\", manifestJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private-corpora", manifestJson, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TranslationModelAsset_TrainingDataContract_RejectsMissingLegalReadinessFields()
    {
        var manifest = TranslationTrainingDataContract.CreateManifest(
            new string('a', 64),
            trainingPairsCount: 2,
            DateTimeOffset.Parse("2026-06-02T00:00:00Z"));

        Assert.False(TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(
            manifest with { CommonTrainReady = false }));
        Assert.False(TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(
            manifest with { DpoTrainReady = false }));
        Assert.False(TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(
            manifest with { LegalVerdict = "PENDING" }));
        Assert.False(TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(
            manifest with { LicenseOrProjectInternalRef = "unknown" }));
        Assert.False(TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(
            manifest with { StorageScope = "PersonalClient" }));
        Assert.False(TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(
            manifest with { PrivacyScope = "private" }));
        Assert.False(TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(
            manifest with { ProvenanceRef = "../private-corpora/provenance.json" }));
    }

    [Fact]
    public async Task TranslationModelAsset_TrainingDataArtifactStore_ExportsRuntimeManifestWithLegalFields()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-training-artifact-test", Guid.NewGuid().ToString("N"));
        try
        {
            var rows = BuildTrainReadyRows();
            var export = await TranslationTrainingDataArtifactStore.ExportTrainReadyPairsAsync(
                root,
                rows,
                DateTimeOffset.Parse("2026-06-02T00:00:00Z"));

            Assert.True(export.IsSuccess, export.ErrorMessage);
            Assert.Equal(TranslationTrainingDataContract.ManifestRepoPath, export.Value!.ManifestRepoPath);
            Assert.EndsWith(
                Path.Combine("training-data", "translation", "translation-v1", "training-manifest.json"),
                export.Value.ManifestPath,
                StringComparison.OrdinalIgnoreCase);
            Assert.Equal(rows.Length, export.Value.TrainingPairsCount);
            Assert.Equal(rows.Length, export.Value.Manifest.TrainingPairsCount);
            Assert.True(File.Exists(export.Value.ManifestPath));
            Assert.True(File.Exists(export.Value.CorpusPath));

            var validation = await TranslationTrainingDataArtifactStore.LoadAndValidateArtifactAsync(root);
            Assert.True(validation.IsSuccess, validation.ErrorMessage);
            Assert.Equal(rows.Length, validation.Value!.CorpusRowsCount);
            Assert.Equal(export.Value.CorpusSha256, validation.Value.CorpusSha256);
            Assert.True(TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(validation.Value.Manifest));

            var manifestJson = await File.ReadAllTextAsync(export.Value.ManifestPath);
            using var document = JsonDocument.Parse(manifestJson);
            var manifestRoot = document.RootElement;
            Assert.Equal("synthetic_self_authored", manifestRoot.GetProperty("source_category").GetString());
            Assert.Equal("translation_seed_dpo_pairs", manifestRoot.GetProperty("data_class").GetString());
            Assert.Equal("CommonGlobal", manifestRoot.GetProperty("storage_scope").GetString());
            Assert.Equal("common_allowed", manifestRoot.GetProperty("privacy_scope").GetString());
            Assert.True(manifestRoot.GetProperty("common_train_ready").GetBoolean());
            Assert.True(manifestRoot.GetProperty("dpo_train_ready").GetBoolean());
            Assert.Equal("seed-corpus/translation/language-translate-seed-v1", manifestRoot.GetProperty("provenance_ref").GetString());
            Assert.Equal("train_only_source_disjoint_from_held_out", manifestRoot.GetProperty("split_policy").GetString());
            Assert.Equal("language-translate-seed-v1-train", manifestRoot.GetProperty("source_disjoint_group").GetString());
            Assert.Equal("project_internal_self_authored", manifestRoot.GetProperty("license_or_project_internal_ref").GetString());
            Assert.Equal("ACCEPT", manifestRoot.GetProperty("legal_verdict").GetString());
            Assert.DoesNotContain("C:\\", manifestJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private-corpora", manifestJson, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TranslationModelAsset_ExportStore_WritesValidatedTrainingDataArtifactEvidence()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-model-export-test", Guid.NewGuid().ToString("N"));
        try
        {
            var artifactRoot = Path.Combine(root, "artifact-root");
            var artifact = await CreateValidatedTrainingDataArtifactAsync(artifactRoot);
            var checkpointPath = Path.Combine(root, "inputs", "checkpoint.json");
            var reportPath = Path.Combine(root, "inputs", "runner-report.json");
            await WriteModelAssetInputsAsync(checkpointPath, reportPath);

            var outputDir = Path.Combine(root, "asset");
            var export = await TranslationModelAssetStore.ExportAsync(new TranslationModelAssetExportOptions(
                CheckpointPath: checkpointPath,
                RunnerReportPath: reportPath,
                OutputDirectory: outputDir,
                TrainingDataArtifactRoot: artifactRoot,
                AccuracyAtExport: 0.81,
                SourceBaseCommit: "test-commit",
                CreatedAtUtc: DateTimeOffset.Parse("2026-06-02T00:00:00Z")));

            Assert.True(export.IsSuccess, export.ErrorMessage);
            Assert.True(File.Exists(export.Value!.ManifestPath));
            Assert.True(File.Exists(export.Value.CheckpointPath));
            Assert.True(File.Exists(export.Value.RunnerReportPath));
            Assert.True(TranslationModelManifestContract.IsValidSharedAssetManifest(export.Value.Manifest));
            Assert.Equal(artifact.CorpusSha256, export.Value.Manifest.TrainingDataEvidence.CorpusSha256);
            Assert.Equal(artifact.CorpusRowsCount, export.Value.Manifest.TrainingDataEvidence.CorpusRowsCount);
            Assert.Equal(artifact.Manifest.TrainingPairsCount, export.Value.Manifest.TrainingDataEvidence.ManifestTrainingPairsCount);
            Assert.True(export.Value.Manifest.TrainingDataEvidence.CommonTrainReady);
            Assert.True(export.Value.Manifest.TrainingDataEvidence.DpoTrainReady);
            Assert.True(export.Value.Manifest.TrainingDataEvidence.LegalCommonDpoManifestValidated);
            Assert.Equal("ACCEPT", export.Value.Manifest.TrainingDataEvidence.LegalVerdict);

            var manifestJson = await File.ReadAllTextAsync(export.Value.ManifestPath);
            using var document = JsonDocument.Parse(manifestJson);
            var evidence = document.RootElement.GetProperty("training_data_evidence");
            Assert.Equal("training-data/translation/translation-v1/training-manifest.json", evidence.GetProperty("manifest_path").GetString());
            Assert.Equal("training-data/translation/translation-v1/corpus.jsonl.gz", evidence.GetProperty("corpus_path").GetString());
            Assert.Equal(artifact.CorpusSha256, evidence.GetProperty("corpus_sha256").GetString());
            Assert.Equal(artifact.CorpusRowsCount, evidence.GetProperty("corpus_rows_count").GetInt32());
            Assert.True(evidence.GetProperty("legal_common_dpo_manifest_validated").GetBoolean());
            Assert.DoesNotContain("C:\\", manifestJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private-corpora", manifestJson, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("corpus_sha256")]
    [InlineData("training_pairs_count")]
    [InlineData("common_train_ready")]
    [InlineData("dpo_train_ready")]
    [InlineData("legal_verdict")]
    [InlineData("missing_common_train_ready")]
    public async Task TranslationModelAsset_ExportStore_RejectsStaleOrInvalidTrainingDataArtifactBeforeWritingAsset(string mutation)
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-model-export-negative-test", Guid.NewGuid().ToString("N"));
        try
        {
            var artifactRoot = Path.Combine(root, "artifact-root");
            await CreateValidatedTrainingDataArtifactAsync(artifactRoot);
            MutateTrainingDataManifest(artifactRoot, node =>
            {
                switch (mutation)
                {
                    case "corpus_sha256":
                        node["corpus_sha256"] = new string('b', 64);
                        break;
                    case "training_pairs_count":
                        node["training_pairs_count"] = 999;
                        break;
                    case "common_train_ready":
                        node["common_train_ready"] = false;
                        break;
                    case "dpo_train_ready":
                        node["dpo_train_ready"] = false;
                        break;
                    case "legal_verdict":
                        node["legal_verdict"] = "PENDING";
                        break;
                    case "missing_common_train_ready":
                        Assert.True(node.Remove("common_train_ready"));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null);
                }
            });

            var checkpointPath = Path.Combine(root, "inputs", "checkpoint.json");
            var reportPath = Path.Combine(root, "inputs", "runner-report.json");
            await WriteModelAssetInputsAsync(checkpointPath, reportPath);

            var outputDir = Path.Combine(root, "asset");
            var export = await TranslationModelAssetStore.ExportAsync(new TranslationModelAssetExportOptions(
                CheckpointPath: checkpointPath,
                RunnerReportPath: reportPath,
                OutputDirectory: outputDir,
                TrainingDataArtifactRoot: artifactRoot,
                AccuracyAtExport: 0.81,
                SourceBaseCommit: "test-commit",
                CreatedAtUtc: DateTimeOffset.Parse("2026-06-02T00:00:00Z")));

            Assert.False(export.IsSuccess);
            Assert.False(File.Exists(Path.Combine(outputDir, "manifest.json")));
            Assert.False(File.Exists(Path.Combine(outputDir, TranslationModelManifestContract.CheckpointFilename)));
            Assert.False(File.Exists(Path.Combine(outputDir, TranslationModelManifestContract.RunnerReportRef)));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TranslationModelAsset_PublishValidation_RejectsStaleTrainingDataEvidence()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-model-publish-negative-test", Guid.NewGuid().ToString("N"));
        try
        {
            var artifactRoot = Path.Combine(root, "artifact-root");
            await CreateValidatedTrainingDataArtifactAsync(artifactRoot, BuildTrainReadyRows());
            var checkpointPath = Path.Combine(root, "inputs", "checkpoint.json");
            var reportPath = Path.Combine(root, "inputs", "runner-report.json");
            await WriteModelAssetInputsAsync(checkpointPath, reportPath);
            var export = await TranslationModelAssetStore.ExportAsync(new TranslationModelAssetExportOptions(
                CheckpointPath: checkpointPath,
                RunnerReportPath: reportPath,
                OutputDirectory: Path.Combine(root, "asset"),
                TrainingDataArtifactRoot: artifactRoot,
                AccuracyAtExport: 0.81,
                SourceBaseCommit: "test-commit",
                CreatedAtUtc: DateTimeOffset.Parse("2026-06-02T00:00:00Z")));
            Assert.True(export.IsSuccess, export.ErrorMessage);

            await CreateValidatedTrainingDataArtifactAsync(artifactRoot, BuildTrainReadyRows().Take(1).ToArray());

            var publishValidation = await TranslationModelAssetStore.PublishAsync(
                root,
                export.Value!.ManifestPath,
                artifactRoot,
                DateTimeOffset.Parse("2026-06-02T00:30:00Z"));

            Assert.False(publishValidation.IsSuccess);
            Assert.Equal("TRANSLATION_MODEL_TRAINING_DATA_EVIDENCE_STALE", publishValidation.ErrorCode);
            Assert.False(File.Exists(Path.Combine(root, "models", "index.json")));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TranslationModelAsset_PublishStore_WritesRegistryOnlyAfterTrainingDataEvidenceValidation()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-model-publish-test", Guid.NewGuid().ToString("N"));
        try
        {
            var artifactRoot = Path.Combine(root, "artifact-root");
            var artifact = await CreateValidatedTrainingDataArtifactAsync(artifactRoot);
            var checkpointPath = Path.Combine(root, "inputs", "checkpoint.json");
            var reportPath = Path.Combine(root, "inputs", "runner-report.json");
            await WriteModelAssetInputsAsync(checkpointPath, reportPath);
            var export = await TranslationModelAssetStore.ExportAsync(new TranslationModelAssetExportOptions(
                CheckpointPath: checkpointPath,
                RunnerReportPath: reportPath,
                OutputDirectory: Path.Combine(root, "asset"),
                TrainingDataArtifactRoot: artifactRoot,
                AccuracyAtExport: 0.81,
                SourceBaseCommit: "test-commit",
                CreatedAtUtc: DateTimeOffset.Parse("2026-06-02T00:00:00Z")));
            Assert.True(export.IsSuccess, export.ErrorMessage);

            var publish = await TranslationModelAssetStore.PublishAsync(
                root,
                export.Value!.ManifestPath,
                artifactRoot,
                DateTimeOffset.Parse("2026-06-02T00:30:00Z"));

            Assert.True(publish.IsSuccess, publish.ErrorMessage);
            Assert.True(File.Exists(publish.Value!.RegistryPath));
            Assert.Equal(artifact.CorpusSha256, publish.Value.Manifest.TrainingDataEvidence.CorpusSha256);
            Assert.True(publish.Value.Manifest.TrainingDataEvidence.LegalCommonDpoManifestValidated);

            using var indexJson = JsonDocument.Parse(await File.ReadAllTextAsync(publish.Value.RegistryPath));
            var entry = Assert.Single(indexJson.RootElement.GetProperty("models").EnumerateArray());
            Assert.Equal("translation-v1", entry.GetProperty("model_id").GetString());
            Assert.Equal("TranslationDomain", entry.GetProperty("domain").GetString());
            Assert.Equal("asset/manifest.json", entry.GetProperty("manifest_path").GetString());
            Assert.Equal("asset/translation-v1-checkpoint.json", entry.GetProperty("checkpoint_path").GetString());
            Assert.Equal("1.0.0", entry.GetProperty("version").GetString());
            Assert.Equal(0.81, entry.GetProperty("accuracy").GetDouble(), precision: 3);
            Assert.Equal(Sha256File(export.Value.ManifestPath), entry.GetProperty("manifest_sha256").GetString());
            Assert.Equal(Sha256File(export.Value.CheckpointPath), entry.GetProperty("checkpoint_sha256").GetString());
            Assert.Equal(Sha256File(export.Value.CheckpointPath), entry.GetProperty("manifest_checkpoint_sha256").GetString());
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TranslationModelAsset_PublishStore_PreservesUnrelatedRegistryRowsAndRootMetadata()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-model-publish-lossless-test", Guid.NewGuid().ToString("N"));
        try
        {
            var artifactRoot = Path.Combine(root, "artifact-root");
            await CreateValidatedTrainingDataArtifactAsync(artifactRoot);
            var checkpointPath = Path.Combine(root, "inputs", "checkpoint.json");
            var reportPath = Path.Combine(root, "inputs", "runner-report.json");
            await WriteModelAssetInputsAsync(checkpointPath, reportPath);
            var export = await TranslationModelAssetStore.ExportAsync(new TranslationModelAssetExportOptions(
                CheckpointPath: checkpointPath,
                RunnerReportPath: reportPath,
                OutputDirectory: Path.Combine(root, "asset"),
                TrainingDataArtifactRoot: artifactRoot,
                AccuracyAtExport: 0.91,
                SourceBaseCommit: "test-commit",
                CreatedAtUtc: DateTimeOffset.Parse("2026-06-02T00:00:00Z")));
            Assert.True(export.IsSuccess, export.ErrorMessage);

            var indexPath = Path.Combine(root, "models", "index.json");
            Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
            var orchestration = await WriteBoundRegistryFilesAsync(
                root,
                "models/orchestration/orchestration-v1/manifest.json",
                "models/orchestration/orchestration-v1/orchestration-v1-checkpoint.json",
                "{\"model\":\"orchestration\"}\n",
                "{\"checkpoint\":\"orchestration\"}\n");
            var staleTranslation = await WriteBoundRegistryFilesAsync(
                root,
                "stale/manifest.json",
                "stale/translation-v1-checkpoint.json",
                "{\"model\":\"old-translation\"}\n",
                "{\"checkpoint\":\"old-translation\"}\n");
            var skillSelection = await WriteBoundRegistryFilesAsync(
                root,
                "models/skill-selection/skill-selection-v1/manifest.json",
                "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
                "{\"model\":\"skill-selection\"}\n",
                "{\"checkpoint\":\"skill-selection\"}\n");

            var originalIndex = $$"""
                {
                  "registry_schema": "root-v1",
                  "models": [
                    {
                      "model_id": "orchestration-v1",
                      "domain": "OrchestrationPlanDomain",
                      "manifest_path": "models/orchestration/orchestration-v1/manifest.json",
                      "checkpoint_path": "models/orchestration/orchestration-v1/orchestration-v1-checkpoint.json",
                      "version": "1.0.0",
                      "accuracy": 0.9,
                      "checkpoint_sha256": "{{orchestration.CheckpointSha256}}",
                      "manifest_sha256": "{{orchestration.ManifestSha256}}",
                      "manifest_checkpoint_sha256": "{{orchestration.CheckpointSha256}}",
                      "orchestration_preserved": {
                        "marker": "keep-orchestration-row"
                      },
                      "published_at_utc": "2026-06-02T18:28:26.0101827+00:00"
                    },
                    {
                      "model_id": "translation-v1",
                      "domain": "TranslationDomain",
                      "manifest_path": "stale/manifest.json",
                      "checkpoint_path": "stale/translation-v1-checkpoint.json",
                      "version": "0.9.0",
                      "accuracy": 0.1,
                      "checkpoint_sha256": "{{staleTranslation.CheckpointSha256}}",
                      "manifest_sha256": "{{staleTranslation.ManifestSha256}}",
                      "manifest_checkpoint_sha256": "{{staleTranslation.CheckpointSha256}}",
                      "translation_additive_metadata": {
                        "marker": "keep-translation-additive"
                      },
                      "published_at_utc": "2026-01-01T00:00:00Z"
                    },
                    {
                      "model_id": "skill-selection-v1",
                      "domain": "SkillSelectionDomain",
                      "manifest_path": "models/skill-selection/skill-selection-v1/manifest.json",
                      "checkpoint_path": "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
                      "version": "skill-selection-v1",
                      "checkpoint_sha256": "{{skillSelection.CheckpointSha256}}",
                      "manifest_sha256": "{{skillSelection.ManifestSha256}}",
                      "manifest_checkpoint_sha256": "{{skillSelection.CheckpointSha256}}",
                      "skill_unknown": [
                        "left",
                        "right"
                      ],
                      "published_at_utc": "2026-06-10T22:40:38.4720594+00:00"
                    }
                  ],
                  "root_tail": {
                    "marker": "keep-root-tail"
                  }
                }
                """.Replace("\r\n", "\n", StringComparison.Ordinal);
            await File.WriteAllTextAsync(indexPath, originalIndex, Utf8NoBom);

            var originalRoot = JsonNode.Parse(originalIndex)!.AsObject();
            var originalModels = originalRoot["models"]!.AsArray();
            var originalOrchestrationJson = originalModels[0]!.ToJsonString();
            var originalSkillSelectionJson = originalModels[2]!.ToJsonString();
            var originalTranslationPropertyOrder = originalModels[1]!
                .AsObject()
                .Select(property => property.Key)
                .ToArray();

            var publish = await TranslationModelAssetStore.PublishAsync(
                root,
                export.Value!.ManifestPath,
                artifactRoot,
                DateTimeOffset.Parse("2026-06-02T00:30:00Z"));

            Assert.True(publish.IsSuccess, publish.ErrorMessage);
            var updatedText = await File.ReadAllTextAsync(indexPath, Utf8NoBom);
            var updatedRoot = JsonNode.Parse(updatedText)!.AsObject();
            Assert.Equal(["registry_schema", "models", "root_tail"], updatedRoot.Select(property => property.Key).ToArray());
            Assert.Equal("root-v1", updatedRoot["registry_schema"]!.GetValue<string>());
            Assert.Equal("keep-root-tail", updatedRoot["root_tail"]!["marker"]!.GetValue<string>());

            var updatedModels = updatedRoot["models"]!.AsArray();
            Assert.Equal(
                ["orchestration-v1", "translation-v1", "skill-selection-v1"],
                updatedModels.Select(model => model!["model_id"]!.GetValue<string>()).ToArray());
            Assert.Equal(originalOrchestrationJson, updatedModels[0]!.ToJsonString());
            Assert.Equal(originalSkillSelectionJson, updatedModels[2]!.ToJsonString());

            var translationEntry = updatedModels[1]!.AsObject();
            Assert.Equal(originalTranslationPropertyOrder, translationEntry.Select(property => property.Key).ToArray());
            Assert.Equal("TranslationDomain", translationEntry["domain"]!.GetValue<string>());
            Assert.Equal("asset/manifest.json", translationEntry["manifest_path"]!.GetValue<string>());
            Assert.Equal("asset/translation-v1-checkpoint.json", translationEntry["checkpoint_path"]!.GetValue<string>());
            Assert.Equal("1.0.0", translationEntry["version"]!.GetValue<string>());
            Assert.Equal(0.91, translationEntry["accuracy"]!.GetValue<double>(), precision: 3);
            Assert.Equal(
                DateTimeOffset.Parse("2026-06-02T00:30:00Z"),
                DateTimeOffset.Parse(translationEntry["published_at_utc"]!.GetValue<string>()));
            Assert.Equal(
                "keep-translation-additive",
                translationEntry["translation_additive_metadata"]!["marker"]!.GetValue<string>());

            var expectedManifestSha = Sha256File(export.Value.ManifestPath);
            var expectedCheckpointSha = Sha256File(export.Value.CheckpointPath);
            Assert.Equal(expectedManifestSha, translationEntry["manifest_sha256"]!.GetValue<string>());
            Assert.Equal(expectedCheckpointSha, translationEntry["checkpoint_sha256"]!.GetValue<string>());
            Assert.Equal(expectedCheckpointSha, translationEntry["manifest_checkpoint_sha256"]!.GetValue<string>());
            Assert.DoesNotContain(expectedManifestSha.ToUpperInvariant(), updatedText, StringComparison.Ordinal);
            Assert.DoesNotContain(expectedCheckpointSha.ToUpperInvariant(), updatedText, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TranslationModelAsset_PublishStore_RejectsCorruptUnrelatedRegistryRowBeforeMutation()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-model-publish-corrupt-unrelated-test", Guid.NewGuid().ToString("N"));
        try
        {
            var artifactRoot = Path.Combine(root, "artifact-root");
            await CreateValidatedTrainingDataArtifactAsync(artifactRoot);
            var checkpointPath = Path.Combine(root, "inputs", "checkpoint.json");
            var reportPath = Path.Combine(root, "inputs", "runner-report.json");
            await WriteModelAssetInputsAsync(checkpointPath, reportPath);
            var export = await TranslationModelAssetStore.ExportAsync(new TranslationModelAssetExportOptions(
                CheckpointPath: checkpointPath,
                RunnerReportPath: reportPath,
                OutputDirectory: Path.Combine(root, "asset"),
                TrainingDataArtifactRoot: artifactRoot,
                AccuracyAtExport: 0.92,
                SourceBaseCommit: "test-commit",
                CreatedAtUtc: DateTimeOffset.Parse("2026-06-02T00:00:00Z")));
            Assert.True(export.IsSuccess, export.ErrorMessage);

            var indexPath = Path.Combine(root, "models", "index.json");
            Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
            var originalIndex = """
                {
                  "registry_schema": "root-v1",
                  "models": [
                    {
                      "model_id": "orchestration-v1",
                      "domain": "OrchestrationPlanDomain",
                      "manifest_path": "models/orchestration/orchestration-v1/manifest.json",
                      "version": "1.0.0",
                      "published_at_utc": "2026-06-02T18:28:26.0101827+00:00"
                    }
                  ],
                  "root_tail": {
                    "marker": "keep-root-tail"
                  }
                }
                """.Replace("\r\n", "\n", StringComparison.Ordinal);
            await File.WriteAllTextAsync(indexPath, originalIndex, Utf8NoBom);
            var beforeBytes = await File.ReadAllBytesAsync(indexPath);

            var publish = await TranslationModelAssetStore.PublishAsync(
                root,
                export.Value!.ManifestPath,
                artifactRoot,
                DateTimeOffset.Parse("2026-06-02T00:30:00Z"));

            Assert.False(publish.IsSuccess);
            Assert.Equal("TRANSLATION_MODEL_INDEX_EXISTING_ROW_INTEGRITY_FAILED", publish.ErrorCode);
            Assert.Contains("checkpoint_path", publish.ErrorMessage, StringComparison.Ordinal);
            Assert.Null(publish.Value);
            Assert.Equal(beforeBytes, await File.ReadAllBytesAsync(indexPath));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("missing-models", "TRANSLATION_MODEL_INDEX_INVALID")]
    [InlineData("models-not-array", "TRANSLATION_MODEL_INDEX_INVALID")]
    [InlineData("model-entry-not-object", "TRANSLATION_MODEL_INDEX_INVALID")]
    [InlineData("missing-model-id", "TRANSLATION_MODEL_INDEX_INVALID")]
    [InlineData("blank-model-id", "TRANSLATION_MODEL_INDEX_INVALID")]
    [InlineData("non-string-model-id", "TRANSLATION_MODEL_INDEX_INVALID")]
    [InlineData("duplicate-translation", "TRANSLATION_MODEL_INDEX_DUPLICATE_MODEL_ID")]
    [InlineData("duplicate-unrelated", "TRANSLATION_MODEL_INDEX_DUPLICATE_MODEL_ID")]
    public async Task TranslationModelAsset_PublishStore_RejectsMalformedOrDuplicateRegistryWithoutOverwrite(
        string mutation,
        string expectedErrorCode)
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-model-publish-invalid-index-test", Guid.NewGuid().ToString("N"));
        try
        {
            var artifactRoot = Path.Combine(root, "artifact-root");
            await CreateValidatedTrainingDataArtifactAsync(artifactRoot);
            var checkpointPath = Path.Combine(root, "inputs", "checkpoint.json");
            var reportPath = Path.Combine(root, "inputs", "runner-report.json");
            await WriteModelAssetInputsAsync(checkpointPath, reportPath);
            var export = await TranslationModelAssetStore.ExportAsync(new TranslationModelAssetExportOptions(
                CheckpointPath: checkpointPath,
                RunnerReportPath: reportPath,
                OutputDirectory: Path.Combine(root, "asset"),
                TrainingDataArtifactRoot: artifactRoot,
                AccuracyAtExport: 0.87,
                SourceBaseCommit: "test-commit",
                CreatedAtUtc: DateTimeOffset.Parse("2026-06-02T00:00:00Z")));
            Assert.True(export.IsSuccess, export.ErrorMessage);

            var indexPath = Path.Combine(root, "models", "index.json");
            Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
            var originalIndex = mutation switch
            {
                "missing-models" => """
                    {
                      "registry_schema": "root-v1"
                    }
                    """,
                "models-not-array" => """
                    {
                      "models": {
                        "not": "array"
                      }
                    }
                    """,
                "model-entry-not-object" => """
                    {
                      "models": [
                        "translation-v1"
                      ]
                    }
                    """,
                "missing-model-id" => """
                    {
                      "models": [
                        {
                          "domain": "TranslationDomain"
                        }
                      ]
                    }
                    """,
                "blank-model-id" => """
                    {
                      "models": [
                        {
                          "model_id": "   ",
                          "domain": "TranslationDomain"
                        }
                      ]
                    }
                    """,
                "non-string-model-id" => """
                    {
                      "models": [
                        {
                          "model_id": 123,
                          "domain": "TranslationDomain"
                        }
                      ]
                    }
                    """,
                "duplicate-translation" => """
                    {
                      "models": [
                        {
                          "model_id": "translation-v1",
                          "domain": "TranslationDomain"
                        },
                        {
                          "model_id": "translation-v1",
                          "domain": "TranslationDomain"
                        }
                      ]
                    }
                    """,
                "duplicate-unrelated" => """
                    {
                      "models": [
                        {
                          "model_id": "orchestration-v1",
                          "domain": "OrchestrationPlanDomain"
                        },
                        {
                          "model_id": "orchestration-v1",
                          "domain": "OrchestrationPlanDomain"
                        },
                        {
                          "model_id": "translation-v1",
                          "domain": "TranslationDomain"
                        }
                      ]
                    }
                    """,
                _ => throw new InvalidOperationException($"Unknown mutation {mutation}.")
            };
            originalIndex = originalIndex.Replace("\r\n", "\n", StringComparison.Ordinal);
            await File.WriteAllTextAsync(indexPath, originalIndex, Utf8NoBom);
            var beforeBytes = await File.ReadAllBytesAsync(indexPath);

            var publish = await TranslationModelAssetStore.PublishAsync(
                root,
                export.Value!.ManifestPath,
                artifactRoot,
                DateTimeOffset.Parse("2026-06-02T00:30:00Z"));

            Assert.False(publish.IsSuccess);
            Assert.Equal(expectedErrorCode, publish.ErrorCode);
            Assert.Null(publish.Value);
            Assert.Equal(beforeBytes, await File.ReadAllBytesAsync(indexPath));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TranslationModelAsset_PublishStore_RejectsMissingCheckpointBeforeRegistryMutation()
    {
        var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-model-publish-missing-checkpoint-test", Guid.NewGuid().ToString("N"));
        try
        {
            var artifactRoot = Path.Combine(root, "artifact-root");
            await CreateValidatedTrainingDataArtifactAsync(artifactRoot);
            var checkpointPath = Path.Combine(root, "inputs", "checkpoint.json");
            var reportPath = Path.Combine(root, "inputs", "runner-report.json");
            await WriteModelAssetInputsAsync(checkpointPath, reportPath);
            var export = await TranslationModelAssetStore.ExportAsync(new TranslationModelAssetExportOptions(
                CheckpointPath: checkpointPath,
                RunnerReportPath: reportPath,
                OutputDirectory: Path.Combine(root, "asset"),
                TrainingDataArtifactRoot: artifactRoot,
                AccuracyAtExport: 0.87,
                SourceBaseCommit: "test-commit",
                CreatedAtUtc: DateTimeOffset.Parse("2026-06-02T00:00:00Z")));
            Assert.True(export.IsSuccess, export.ErrorMessage);

            var indexPath = Path.Combine(root, "models", "index.json");
            Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
            var originalIndex = """
                {
                  "registry_schema": "root-v1",
                  "models": [
                    {
                      "model_id": "orchestration-v1",
                      "domain": "OrchestrationPlanDomain",
                      "manifest_path": "models/orchestration/orchestration-v1/manifest.json",
                      "published_at_utc": "2026-06-02T18:28:26.0101827+00:00"
                    }
                  ],
                  "root_tail": {
                    "marker": "keep-root-tail"
                  }
                }
                """.Replace("\r\n", "\n", StringComparison.Ordinal);
            await File.WriteAllTextAsync(indexPath, originalIndex, Utf8NoBom);
            var beforeBytes = await File.ReadAllBytesAsync(indexPath);

            File.Delete(export.Value!.CheckpointPath);
            var publish = await TranslationModelAssetStore.PublishAsync(
                root,
                export.Value.ManifestPath,
                artifactRoot,
                DateTimeOffset.Parse("2026-06-02T00:30:00Z"));

            Assert.False(publish.IsSuccess);
            Assert.Equal("TRANSLATION_MODEL_CHECKPOINT_NOT_FOUND", publish.ErrorCode);
            Assert.Null(publish.Value);
            Assert.Equal(beforeBytes, await File.ReadAllBytesAsync(indexPath));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TranslationModelAsset_CommittedRegistryTranslationRow_BindsPhysicalManifestAndCheckpoint()
    {
        var repoRoot = GetRepoRoot();
        var indexPath = Path.Combine(repoRoot, "models", "index.json");
        var indexText = File.ReadAllText(indexPath, Utf8NoBom);
        var indexRoot = JsonNode.Parse(indexText)!.AsObject();
        var models = indexRoot["models"]!.AsArray();
        var translationEntry = Assert.Single(
            models.Select(model => model!.AsObject()),
            model => string.Equals(
                model["model_id"]!.GetValue<string>(),
                TranslationModelManifestContract.ModelId,
                StringComparison.Ordinal));

        Assert.Equal("models/translation/translation-v1/manifest.json", translationEntry["manifest_path"]!.GetValue<string>());
        Assert.Equal("models/translation/translation-v1/translation-v1-checkpoint.json", translationEntry["checkpoint_path"]!.GetValue<string>());

        var manifestPath = Path.Combine(
            repoRoot,
            translationEntry["manifest_path"]!.GetValue<string>().Replace('/', Path.DirectorySeparatorChar));
        var checkpointPath = Path.Combine(
            repoRoot,
            translationEntry["checkpoint_path"]!.GetValue<string>().Replace('/', Path.DirectorySeparatorChar));
        var expectedManifestSha = Sha256File(manifestPath);
        var expectedCheckpointSha = Sha256File(checkpointPath);

        Assert.Equal(expectedManifestSha, translationEntry["manifest_sha256"]!.GetValue<string>());
        Assert.Equal(expectedCheckpointSha, translationEntry["checkpoint_sha256"]!.GetValue<string>());
        Assert.Equal(expectedCheckpointSha, translationEntry["manifest_checkpoint_sha256"]!.GetValue<string>());
        Assert.DoesNotContain(expectedManifestSha.ToUpperInvariant(), indexText, StringComparison.Ordinal);
        Assert.DoesNotContain(expectedCheckpointSha.ToUpperInvariant(), indexText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TranslationModelAsset_RepoLocalTrainingDataArtifact_LoadsUnderSharedLegalCommonDpoContract()
    {
        var repoRoot = GetRepoRoot();
        var validation = await TranslationTrainingDataArtifactStore.LoadValidatedTrainReadyPairsAsync(repoRoot);

        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        Assert.Equal(TranslationTrainingDataContract.ManifestRepoPath, validation.Value!.ManifestRepoPath);
        Assert.EndsWith(
            Path.Combine("training-data", "translation", "translation-v1", "training-manifest.json"),
            validation.Value.ManifestPath,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal(12, validation.Value.CorpusRowsCount);
        Assert.Equal(12, validation.Value.Manifest.TrainingPairsCount);
        Assert.True(TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(validation.Value.Manifest));
        Assert.True(validation.Value.Manifest.CommonTrainReady);
        Assert.True(validation.Value.Manifest.DpoTrainReady);
        Assert.Equal("ACCEPT", validation.Value.Manifest.LegalVerdict);

        var seedCorpus = TranslationSeedCorpusLoader.Load(Path.Combine(
            repoRoot,
            "27.TranslationDomain",
            "SeedData",
            "language-translate-seed-v1.jsonl"));
        var expectedIds = seedCorpus.ToTrainReadyPairs(TranslationSeedCorpusSplit.Train)
            .Select(pair => pair.Id)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var actualIds = validation.Value.TrainingPairs
            .Select(pair => pair.Id)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(expectedIds, actualIds);
        Assert.All(validation.Value.TrainingPairs, pair => Assert.False(string.IsNullOrWhiteSpace(pair.ASide)));
        Assert.All(validation.Value.TrainingPairs, pair => Assert.Equal("train_ready", pair.ValidationOutcome));
        Assert.DoesNotContain(validation.Value.TrainingPairs, pair => (pair.ValidationErrors?.Count ?? 0) > 0);
    }

    [Fact]
    public async Task TranslationModelAsset_TrainingDataArtifactStore_RejectsManifestsMissingLegalCommonDpoFields()
    {
        var missingFields = new[]
        {
            "source_category",
            "data_class",
            "storage_scope",
            "privacy_scope",
            "common_train_ready",
            "dpo_train_ready",
            "provenance_ref",
            "split_policy",
            "source_disjoint_group",
            "license_or_project_internal_ref",
            "legal_verdict"
        };

        foreach (var missingField in missingFields)
        {
            var root = Path.Combine(Path.GetTempPath(), "xiigen-translation-training-artifact-negative-test", Guid.NewGuid().ToString("N"));
            try
            {
                var export = await TranslationTrainingDataArtifactStore.ExportTrainReadyPairsAsync(
                    root,
                    BuildTrainReadyRows(),
                    DateTimeOffset.Parse("2026-06-02T00:00:00Z"));
                Assert.True(export.IsSuccess, export.ErrorMessage);

                var manifestJson = await File.ReadAllTextAsync(export.Value!.ManifestPath);
                var node = JsonNode.Parse(manifestJson)!.AsObject();
                Assert.True(node.Remove(missingField), $"Expected generated manifest to contain {missingField} before negative mutation.");
                await File.WriteAllTextAsync(
                    export.Value.ManifestPath,
                    node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n",
                    Encoding.UTF8);

                var validation = await TranslationTrainingDataArtifactStore.LoadAndValidateArtifactAsync(root);
                Assert.False(validation.IsSuccess);
                Assert.Equal("TRAINING_DATA_MANIFEST_FIELD_MISSING", validation.ErrorCode);
                Assert.Contains(missingField, validation.ErrorMessage, StringComparison.Ordinal);
            }
            finally
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
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

    private static async Task<TranslationTrainingDataArtifactValidationResult> CreateValidatedTrainingDataArtifactAsync(
        string artifactRoot,
        IReadOnlyList<TranslationTrainReadyPair>? rows = null)
    {
        var export = await TranslationTrainingDataArtifactStore.ExportTrainReadyPairsAsync(
            artifactRoot,
            rows ?? BuildTrainReadyRows(),
            DateTimeOffset.Parse("2026-06-02T00:00:00Z"));
        Assert.True(export.IsSuccess, export.ErrorMessage);

        var validation = await TranslationTrainingDataArtifactStore.LoadAndValidateArtifactAsync(artifactRoot);
        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        return validation.Value!;
    }

    private static async Task WriteModelAssetInputsAsync(string checkpointPath, string reportPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(checkpointPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        await File.WriteAllTextAsync(checkpointPath, "{\"checkpoint\":true}\n", Utf8NoBom);
        await File.WriteAllTextAsync(reportPath, "{\"accuracy_at_export\":0.81}\n", Utf8NoBom);
    }

    private static async Task<(string ManifestSha256, string CheckpointSha256)> WriteBoundRegistryFilesAsync(
        string repositoryRoot,
        string manifestRef,
        string checkpointRef,
        string manifestText,
        string checkpointText)
    {
        var manifestPath = Path.Combine(repositoryRoot, manifestRef.Replace('/', Path.DirectorySeparatorChar));
        var checkpointPath = Path.Combine(repositoryRoot, checkpointRef.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(checkpointPath)!);
        await File.WriteAllTextAsync(manifestPath, manifestText, Utf8NoBom);
        await File.WriteAllTextAsync(checkpointPath, checkpointText, Utf8NoBom);
        return (Sha256File(manifestPath), Sha256File(checkpointPath));
    }

    private static void MutateTrainingDataManifest(string artifactRoot, Action<JsonObject> mutate)
    {
        var manifestPath = Path.Combine(
            artifactRoot,
            TranslationTrainingDataContract.ManifestRepoPath.Replace('/', Path.DirectorySeparatorChar));
        var manifestJson = File.ReadAllText(manifestPath, Utf8NoBom);
        var node = JsonNode.Parse(manifestJson)!.AsObject();
        mutate(node);
        File.WriteAllText(
            manifestPath,
            node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n",
            Utf8NoBom);
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root with LlmFromScratch.sln was not found.");
    }

    private static TranslationTrainReadyPair[] BuildTrainReadyRows()
        =>
        [
            new TranslationTrainReadyPair(
                Id: "translation-v1-runtime-001",
                Source: "seed-corpus/translation/language-translate-seed-v1/runtime-001",
                Direction: "RU_EN",
                PairFamily: "language-translate-seed-v1",
                Prompt: "Translate from ru to en.",
                ASide: "добавь проверку null",
                Chosen: "Add a null check.",
                Rejected: "Skip the null check. [WRONG: missing_null_check - Drops the required validation.]",
                ValidationOutcome: "train_ready",
                ValidationErrors: []),
            new TranslationTrainReadyPair(
                Id: "translation-v1-runtime-002",
                Source: "seed-corpus/translation/language-translate-seed-v1/runtime-002",
                Direction: "RU_EN",
                PairFamily: "language-translate-seed-v1",
                Prompt: "Translate from ru to en.",
                ASide: "запусти рабочий поток",
                Chosen: "Launch the worker thread.",
                Rejected: "Stop the worker thread. [WRONG: action_reversal - Reverses the action.]",
                ValidationOutcome: "train_ready",
                ValidationErrors: [])
        ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
}
