// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TranslationDomainTools;
using ContinuousLearning;
using ProjectWikiDomain;
using AgentInfrastructure;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public sealed class OrchestrationRepositoryOwnedContinuationOutputTests
{
    [Fact]
    public void ContinuationCheckpointAddress_AcceptsOnlyCanonicalRepositoryModelPath()
    {
        using var fixture = new RepositoryFixture();
        var expectedCheckpointDirectory = Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "models",
            Program.OrchestrationRepositoryOwnedDomainSlug,
            Program.OrchestrationRepositoryOwnedDesignPatternSlug,
            Program.OrchestrationRepositoryOwnedModelIdSlug,
            "v0002",
            "checkpoints",
            "000003");

        var result =
            Program.ResolveRepositoryOwnedOrchestrationContinuationCheckpointAddress(
                fixture.RepositoryRoot,
                expectedCheckpointDirectory,
                "v0002",
                3);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var address = result.Value!;
        Assert.Equal(expectedCheckpointDirectory, address.CheckpointRoot);
        Assert.Equal(
            Path.Combine(expectedCheckpointDirectory, "weights.bin"),
            address.WeightsPath);
        Assert.Equal(
            "training-artifacts/models/programming-task-orchestration/ilearning-orchestrator/orchestration-v1/v0002/checkpoints/000003",
            address.CheckpointRootRelativePath);
        Assert.Equal(
            "training-artifacts/models/programming-task-orchestration/ilearning-orchestrator/orchestration-v1/v0002/checkpoints/000003/weights.bin",
            address.WeightsRelativePath);
    }

    [Fact]
    public void ContinuationCheckpointAddress_RejectsDecoyOutputInsideRepository()
    {
        using var fixture = new RepositoryFixture();
        var decoyOutput = Path.Combine(
            fixture.RepositoryRoot,
            "artifacts",
            "orchestration",
            "continued");

        var result =
            Program.ResolveRepositoryOwnedOrchestrationContinuationCheckpointAddress(
                fixture.RepositoryRoot,
                decoyOutput,
                Program.OrchestrationRepositoryOwnedDefaultModelVersion,
                1);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            Program.OrchestrationRepositoryOwnedOutputInvalidErrorCode,
            result.ErrorCode);
        Assert.False(Directory.Exists(Path.Combine(fixture.RepositoryRoot, "artifacts")));
        Assert.False(Directory.Exists(Path.Combine(fixture.RepositoryRoot, "training-artifacts")));
    }

    [Fact]
    public void ContinuationCheckpointAddress_RejectsOutputOutsideRepository()
    {
        using var fixture = new RepositoryFixture();
        var outsideOutput = Path.Combine(
            Path.GetTempPath(),
            "llm-mvp-outside-orchestration-output",
            Guid.NewGuid().ToString("N"));

        var result =
            Program.ResolveRepositoryOwnedOrchestrationContinuationCheckpointAddress(
                fixture.RepositoryRoot,
                outsideOutput,
                Program.OrchestrationRepositoryOwnedDefaultModelVersion,
                1);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            Program.OrchestrationRepositoryOwnedOutputInvalidErrorCode,
            result.ErrorCode);
        Assert.False(Directory.Exists(outsideOutput));
    }

    [Fact]
    public void ContinuationDpoDataset_ReopensExistingSealedDataset()
    {
        using var fixture = new RepositoryFixture();

        // Смысл проверки: продолжение обучения оркестрации не должно заново
        // придумывать DPO-пары. Оно обязано открыть уже сохранённый sealed-набор
        // из training-data/dpo и только после этого может менять веса модели.
        var dataset = CreateSealedOrchestrationDpoDataset(
            fixture,
            Program.OrchestrationRepositoryOwnedDefaultModelVersion);

        var result = Program.OpenRepositoryOwnedOrchestrationDpoDatasetForContinuation(
            fixture.RepositoryRoot,
            Program.OrchestrationRepositoryOwnedDefaultModelVersion);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(dataset.ManifestSha256, result.Value!.Manifest.ManifestSha256);
        Assert.Single(result.Value.Pairs);
        Assert.Equal(
            Program.OrchestrationRepositoryOwnedDomainSlug,
            result.Value.Manifest.Domain);
        Assert.Equal(
            Program.OrchestrationRepositoryOwnedDesignPatternSlug,
            result.Value.Manifest.DesignPattern);
    }

    [Fact]
    public void ContinuationDpoDataset_RejectsMissingDatasetWithoutCreatingTrainingData()
    {
        using var fixture = new RepositoryFixture();

        var result = Program.OpenRepositoryOwnedOrchestrationDpoDatasetForContinuation(
            fixture.RepositoryRoot,
            Program.OrchestrationRepositoryOwnedDefaultModelVersion);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            Program.OrchestrationRepositoryOwnedDpoDatasetRequiredErrorCode,
            result.ErrorCode);
        Assert.False(Directory.Exists(Path.Combine(fixture.RepositoryRoot, "training-data")));
        Assert.False(Directory.Exists(Path.Combine(fixture.RepositoryRoot, "training-artifacts")));
    }

    [Fact]
    public async Task ContinuationDpoDataset_ReopensMatchingDatasetWithoutTeacherContext()
    {
        using var fixture = new RepositoryFixture();
        var rows = new[] { CreateReferenceRow("mode2-reuse-001") };
        var context = CreateAuthoringContext(() =>
            throw new InvalidOperationException("Setup must not need live Qwen."));
        var prepared =
            OrchestrationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(
                rows,
                context);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var dataset = CreateSealedOrchestrationDpoDataset(
            fixture,
            Program.OrchestrationRepositoryOwnedDefaultModelVersion,
            prepared.Value!.RequestIdentity);

        var result =
            await Program
                .ResolveRepositoryOwnedOrchestrationDpoDatasetForContinuationAsync(
                    fixture.RepositoryRoot,
                    Program.OrchestrationRepositoryOwnedDefaultModelVersion,
                    rows,
                    createCleanMissAuthoringContext: () =>
                        throw new InvalidOperationException(
                            "Existing repository DPO must not construct Qwen."));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            result.Value!.Source);
        Assert.Equal(
            dataset.ManifestSha256,
            result.Value.Dataset.Manifest.ManifestSha256);
        Assert.Single(result.Value.Dataset.Pairs);
    }

    [Fact]
    public async Task ContinuationDpoDataset_CleanMissCreatesSealsAndThenReusesDpoFromDisk()
    {
        using var fixture = new RepositoryFixture();
        var rows = new[] { CreateReferenceRow("mode2-clean-miss-001") };
        var teacher = new CountingTeacherClient(
            CreateOpenTeacherIdentity(),
            """
            {"chosen":"Read the repository state, split the request into safe dependent slices, verify the changed slice, and publish only after the evidence matches.","rejected":"Start multiple unrelated edits before reading the current branch.","rejected_reason":"it ignores dependency order and can corrupt an unfinished code slice"}
            """);
        var factoryCalls = 0;
        var context = CreateAuthoringContext(() =>
        {
            factoryCalls++;
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
        });

        var first =
            await Program
                .ResolveRepositoryOwnedOrchestrationDpoDatasetForContinuationAsync(
                    fixture.RepositoryRoot,
                    Program.OrchestrationRepositoryOwnedDefaultModelVersion,
                    rows,
                    () => OperationResult<
                        OrchestrationRepositoryOwnedDpoDatasetAuthoringContext>
                        .Ok(context));

        Assert.True(
            first.IsSuccess,
            $"DPO clean-miss resolution failed with {first.ErrorCode}: " +
            $"{first.ErrorMessage}; factory calls={factoryCalls}; " +
            $"teacher calls={teacher.InferenceCalls}");
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .NewlyPersistedRepositoryDataset,
            first.Value!.Source);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(1, teacher.InferenceCalls);
        // Проверяем физический файл пары отдельно от содержимого манифеста:
        // так сразу видно, что продолжение обучения получает сохранённые DPO
        // из Git-дерева, а не из памяти теста.
        var pairRelativePath = first.Value.Dataset.Manifest.OrderedPairs[0]
            .RepositoryRelativePairFilePath
            .Replace('/', Path.DirectorySeparatorChar);
        var pairFullPath = Path.Combine(fixture.RepositoryRoot, pairRelativePath);
        Assert.True(File.Exists(pairFullPath), pairFullPath);
        Assert.DoesNotContain(
            rows[0].Chosen.PlanText,
            first.Value.Dataset.Pairs[0].Chosen,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            rows[0].Chosen.PlanText,
            first.Value.Dataset.Pairs[0].Rejected,
            StringComparison.Ordinal);

        var second =
            await Program
                .ResolveRepositoryOwnedOrchestrationDpoDatasetForContinuationAsync(
                    fixture.RepositoryRoot,
                    Program.OrchestrationRepositoryOwnedDefaultModelVersion,
                    rows,
                    createCleanMissAuthoringContext: () =>
                        throw new InvalidOperationException(
                            "A saved orchestration DPO dataset must be reused from disk."));

        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            second.Value!.Source);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(1, teacher.InferenceCalls);
        Assert.Equal(
            first.Value.Dataset.Manifest.ManifestSha256,
            second.Value.Dataset.Manifest.ManifestSha256);
    }

    [Fact]
    public async Task ContinuationDpoDataset_AuthoringCreatesRichPairWithoutPaidReferenceSide()
    {
        using var fixture = new RepositoryFixture();
        var rows = new[] { CreateReferenceRow("mode2-authoring-001") };
        var teacher = new CountingTeacherClient(
            CreateOpenTeacherIdentity(),
            """
            {"chosen":"Read the repository state, split the request into safe dependent slices, verify the changed slice, and publish only after the evidence matches.","rejected":"Start multiple unrelated edits before reading the current branch.","rejected_reason":"it ignores dependency order and can corrupt an unfinished code slice"}
            """);
        var context = CreateAuthoringContext(() =>
            OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher));
        var prepared =
            OrchestrationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(
                rows,
                context);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            fixture.RepositoryRoot,
            Program.OrchestrationRepositoryOwnedDomainSlug,
            Program.OrchestrationRepositoryOwnedDesignPatternSlug,
            Program.OrchestrationRepositoryOwnedDefaultModelVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);

        var authored =
            await OrchestrationRepositoryOwnedDpoDatasetAuthoring
                .AuthorPairsAsync(address.Value!, prepared.Value!, context);

        Assert.True(
            authored.IsSuccess,
            $"{authored.ErrorCode}: {authored.ErrorMessage}");
        var pair = Assert.Single(authored.Value!);
        Assert.DoesNotContain(rows[0].Chosen.PlanText, pair.Chosen);
        Assert.DoesNotContain(rows[0].Chosen.PlanText, pair.Rejected);
        Assert.NotNull(pair.TeacherProvenance);
        Assert.NotNull(pair.ChosenAuthorship);
        Assert.NotNull(pair.RejectedAuthorship);
    }

    [Fact]
    public async Task ContinuationDpoDataset_RejectsExistingDatasetForDifferentSourceRows()
    {
        using var fixture = new RepositoryFixture();
        var oldRows = new[] { CreateReferenceRow("mode2-old-001") };
        var newRows = new[] { CreateReferenceRow("mode2-new-001") };
        var context = CreateAuthoringContext(() =>
            throw new InvalidOperationException("Setup must not need live Qwen."));
        var prepared =
            OrchestrationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(
                oldRows,
                context);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        CreateSealedOrchestrationDpoDataset(
            fixture,
            Program.OrchestrationRepositoryOwnedDefaultModelVersion,
            prepared.Value!.RequestIdentity);

        var result =
            await Program
                .ResolveRepositoryOwnedOrchestrationDpoDatasetForContinuationAsync(
                    fixture.RepositoryRoot,
                    Program.OrchestrationRepositoryOwnedDefaultModelVersion,
                    newRows,
                    createCleanMissAuthoringContext: () =>
                        throw new InvalidOperationException(
                            "A mismatched saved dataset must fail before Qwen."));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer
                .RequestIdentityMismatchErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public async Task ContinuationDpoMaterializer_TrainsFromReopenedTeacherSidesNotPaidReference()
    {
        using var fixture = new RepositoryFixture();
        var rows = new[]
        {
            CreateReferenceRow(
                "mode2-materialize-001",
                paidPlan: "PAID_REFERENCE_PLAN_MUST_NOT_REACH_TRAINING")
        };
        var dataset = await CreateAuthoredSealedOrchestrationDpoResolutionAsync(
            fixture,
            rows);

        var materialized =
            Program.MaterializeRepositoryOwnedOrchestrationDpoPairsForContinuation(
                fixture.RepositoryRoot,
                rows,
                dataset,
                [],
                maxCycles: 1);

        Assert.True(materialized.IsSuccess, materialized.ErrorMessage);
        var trainingPair = Assert.Single(materialized.Value!);
        var reopenedDpoPair = Assert.Single(dataset.Dataset.Pairs);
        var manifestPair = Assert.Single(dataset.Dataset.Manifest.OrderedPairs);
        Assert.Equal(manifestPair.PairId, trainingPair.PairId);
        Assert.NotEqual(rows[0].PairId, trainingPair.PairId);
        Assert.Equal(rows[0].SourceBundleId, trainingPair.SourceBundleId);
        Assert.Equal(
            manifestPair.RepositoryRelativePairFilePath,
            trainingPair.Chosen.SourcePath);
        Assert.Equal(reopenedDpoPair.Chosen, trainingPair.Chosen.PlanText);
        Assert.Equal(reopenedDpoPair.Rejected, trainingPair.RejectedText);
        Assert.DoesNotContain(
            rows[0].Chosen.PlanText,
            trainingPair.Chosen.PlanText,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            rows[0].Chosen.PlanText,
            trainingPair.RejectedText,
            StringComparison.Ordinal);

        var machine = new OrchestrationPlanMachine();
        var train = await machine.TrainOrchestrationBatchAsync(
            new OrchestrationTrainingBatch(
                "test-reopened-orchestration-dpo",
                materialized.Value!));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var exported = Assert.Single(machine.ExportCheckpoint().KnnStore);
        Assert.Equal(reopenedDpoPair.Chosen, exported.Chosen.PlanText);
        Assert.Equal(reopenedDpoPair.Rejected, exported.RejectedText);
        Assert.Equal(rows[0].SourceBundleId, exported.SourceBundleId);
        Assert.DoesNotContain(
            rows[0].Chosen.PlanText,
            exported.Chosen.PlanText,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task ContinueOrchestrationTrainingCommand_TrainsFromReopenedTeacherSidesNotPaidReference()
    {
        using var fixture = new RepositoryFixture();
        var rows = new[]
        {
            CreateReferenceRow(
                "mode2-cli-001",
                paidPlan: "PAID_REFERENCE_PLAN_MUST_NOT_APPEAR_IN_CLI_CHECKPOINT")
        };
        var dataset = await CreateAuthoredSealedOrchestrationDpoResolutionAsync(
            fixture,
            rows);
        var reopenedDpoPair = Assert.Single(dataset.Dataset.Pairs);
        var manifestPair = Assert.Single(dataset.Dataset.Manifest.OrderedPairs);

        var initialModelDirectory = Path.Combine(
            fixture.RepositoryRoot,
            "initial-model");
        Directory.CreateDirectory(initialModelDirectory);
        var initialCheckpointPath = Path.Combine(
            initialModelDirectory,
            "weights.bin");
        // Положительный пример стартует с ранее обученной памяти и проверяет,
        // что она сохранена после применения новой репозиторной DPO-пары.
        var priorRow = CreateReferenceRow(
            "mode2-prior-persisted-001",
            prompt: "already persisted orchestration continuation prompt");
        var initialMachine = new OrchestrationPlanMachine();
        var initialTrain = await initialMachine.TrainOrchestrationBatchAsync(
            new OrchestrationTrainingBatch(
                "test-prior-orchestration-state",
                [priorRow]));
        Assert.True(initialTrain.IsSuccess, initialTrain.ErrorMessage);
        var initialLoop = new OrchestrationLoopCheckpoint(
            "repository-owned-orchestration-loop-test",
            LastCompletedCycle: 4,
            Cursor: 17,
            TrainedPairIds: [priorRow.PairId],
            Mode1PromotedPairIds: [],
            Mode2PromotedPairIds: [priorRow.PairId],
            Mode3PromotedPairIds: []);
        var savedInitial = await initialMachine
            .SaveCheckpointAsync(initialCheckpointPath, initialLoop);
        Assert.True(savedInitial.IsSuccess, savedInitial.ErrorMessage);
        var initialManifestPath = Path.Combine(
            initialModelDirectory,
            "manifest.json");
        await File.WriteAllTextAsync(
            initialManifestPath,
            """
            {"checkpoint_filename":"weights.bin"}
            """,
            Encoding.UTF8);

        var trainingDataDirectory = Path.Combine(
            fixture.RepositoryRoot,
            "training-data",
            "orchestration",
            "orchestration-v1");
        Directory.CreateDirectory(trainingDataDirectory);
        var corpusPath = Path.Combine(
            trainingDataDirectory,
            "orchestration-corpus.jsonl.gz");
        await WriteGzipOrchestrationRowsAsync(corpusPath, rows);
        var trainingManifestPath = Path.Combine(
            trainingDataDirectory,
            "training-manifest.json");
        await File.WriteAllTextAsync(
            trainingManifestPath,
            JsonSerializer.Serialize(
                new Dictionary<string, object?>
                {
                    ["corpus_filename"] = Path.GetFileName(corpusPath),
                    ["corpus_sha256"] = Sha256FileForTest(corpusPath)
                }),
            Encoding.UTF8);

        var outputDirectory =
            CreateRepositoryOwnedContinuationCheckpointDirectory(
                fixture,
                checkpointStep: 1);

        var exitCode = await Program.Main(
        [
            "continue-orchestration-training",
            "--manifest",
            initialManifestPath,
            "--training-manifest",
            trainingManifestPath,
            "--output",
            outputDirectory,
            "--repository-root",
            fixture.RepositoryRoot,
            "--repository-dpo-dataset-version",
            Program.OrchestrationRepositoryOwnedDefaultModelVersion,
            "--repository-model-version",
            Program.OrchestrationRepositoryOwnedDefaultModelVersion,
            "--repository-checkpoint-step",
            "1",
            "--max-cycles",
            "1"
        ]);

        Assert.Equal(0, exitCode);
        var outputCheckpointPath = Path.Combine(outputDirectory, "weights.bin");
        Assert.True(File.Exists(outputCheckpointPath));
        var checkpoint = JsonSerializer.Deserialize<OrchestrationPlanMachineCheckpoint>(
            await File.ReadAllTextAsync(outputCheckpointPath, Encoding.UTF8),
            OrchestrationPlanJsonOptions.Standard);
        Assert.NotNull(checkpoint);
        Assert.Equal(2, checkpoint!.KnnStore.Count);
        var persistedPrior = Assert.Single(
            checkpoint.KnnStore,
            pair => string.Equals(
                pair.PairId,
                priorRow.PairId,
                StringComparison.Ordinal));
        Assert.Equal(priorRow.Chosen.PlanText, persistedPrior.Chosen.PlanText);
        Assert.Equal(priorRow.RejectedText, persistedPrior.RejectedText);
        var trained = Assert.Single(
            checkpoint.KnnStore,
            pair => string.Equals(
                pair.PairId,
                manifestPair.PairId,
                StringComparison.Ordinal));
        Assert.Equal(manifestPair.PairId, trained.PairId);
        Assert.Equal(rows[0].SourceBundleId, trained.SourceBundleId);
        Assert.Equal(reopenedDpoPair.Chosen, trained.Chosen.PlanText);
        Assert.Equal(reopenedDpoPair.Rejected, trained.RejectedText);
        Assert.DoesNotContain(
            rows[0].Chosen.PlanText,
            trained.Chosen.PlanText,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            rows[0].Chosen.PlanText,
            trained.RejectedText,
            StringComparison.Ordinal);
        Assert.Contains(priorRow.PairId, checkpoint.TrainedPairIds);
        Assert.Contains(manifestPair.PairId, checkpoint.TrainedPairIds);
        Assert.Contains(priorRow.PairId, checkpoint.Mode2PromotedPairIds);
        Assert.Contains(manifestPair.PairId, checkpoint.Mode2PromotedPairIds);
        Assert.NotNull(checkpoint.OrchestrationLoop);
        Assert.Equal(
            initialLoop.TrainingBundleRef,
            checkpoint.OrchestrationLoop!.TrainingBundleRef);
        Assert.Equal(
            initialLoop.LastCompletedCycle,
            checkpoint.OrchestrationLoop.LastCompletedCycle);
        Assert.Equal(initialLoop.Cursor, checkpoint.OrchestrationLoop.Cursor);
        Assert.Equal(
            initialLoop.TrainedPairIds,
            checkpoint.OrchestrationLoop.TrainedPairIds);
        Assert.Equal(
            initialLoop.Mode2PromotedPairIds,
            checkpoint.OrchestrationLoop.Mode2PromotedPairIds);

        using var report = JsonDocument.Parse(
            await File.ReadAllTextAsync(
                Path.Combine(outputDirectory, "continue-report.json"),
                Encoding.UTF8));
        var reportRoot = report.RootElement;
        Assert.Equal(
            "sealed_reopened_dpo_pairs",
            reportRoot
                .GetProperty("repository_owned_dpo_training_source")
                .GetString());
        Assert.False(
            reportRoot
                .GetProperty("paid_reference_sides_used_for_training")
                .GetBoolean());
        Assert.True(
            reportRoot
                .GetProperty("bound_reference_input_labels_retained")
                .GetBoolean());
        Assert.Equal(
            "orchestration_knn_grounding_update_not_dpo_gradient",
            reportRoot.GetProperty("training_update_kind").GetString());
        Assert.True(
            reportRoot
                .GetProperty("checkpoint_save_operation_result_verified")
                .GetBoolean());
        Assert.True(
            reportRoot
                .GetProperty("checkpoint_reopened_after_save")
                .GetBoolean());
        Assert.True(
            reportRoot
                .GetProperty("checkpoint_payload_matches_intended_trained_state")
                .GetBoolean());
        Assert.True(
            reportRoot
                .GetProperty("checkpoint_reopened_state_matches_intended_checkpoint")
                .GetBoolean());
    }

    [Fact]
    public async Task ContinueOrchestrationTrainingCommand_FailsWhenRepositoryCheckpointCannotBeSaved()
    {
        using var fixture = new RepositoryFixture();
        var rows = new[]
        {
            CreateReferenceRow("mode2-cli-save-failure-001")
        };
        await CreateAuthoredSealedOrchestrationDpoResolutionAsync(
            fixture,
            rows);

        var initialModelDirectory = Path.Combine(
            fixture.RepositoryRoot,
            "initial-model");
        Directory.CreateDirectory(initialModelDirectory);
        var initialCheckpointPath = Path.Combine(
            initialModelDirectory,
            "weights.bin");
        var savedInitial = await new OrchestrationPlanMachine()
            .SaveCheckpointAsync(initialCheckpointPath);
        Assert.True(savedInitial.IsSuccess, savedInitial.ErrorMessage);
        var initialManifestPath = Path.Combine(
            initialModelDirectory,
            "manifest.json");
        await File.WriteAllTextAsync(
            initialManifestPath,
            """
            {"checkpoint_filename":"weights.bin"}
            """,
            Encoding.UTF8);

        var trainingDataDirectory = Path.Combine(
            fixture.RepositoryRoot,
            "training-data",
            "orchestration",
            "orchestration-v1");
        Directory.CreateDirectory(trainingDataDirectory);
        var corpusPath = Path.Combine(
            trainingDataDirectory,
            "orchestration-corpus.jsonl.gz");
        await WriteGzipOrchestrationRowsAsync(corpusPath, rows);
        var trainingManifestPath = Path.Combine(
            trainingDataDirectory,
            "training-manifest.json");
        await File.WriteAllTextAsync(
            trainingManifestPath,
            JsonSerializer.Serialize(
                new Dictionary<string, object?>
                {
                    ["corpus_filename"] = Path.GetFileName(corpusPath),
                    ["corpus_sha256"] = Sha256FileForTest(corpusPath)
                }),
            Encoding.UTF8);

        var outputDirectory =
            CreateRepositoryOwnedContinuationCheckpointDirectory(
                fixture,
                checkpointStep: 1);
        Directory.CreateDirectory(outputDirectory);

        // Человеческий смысл: проверка заранее занимает служебный путь, куда
        // сохранение сначала пишет новый файл перед заменой основного. Так
        // ошибка записи проявляется без провайдера и без рабочих данных.
        Directory.CreateDirectory(Path.Combine(outputDirectory, "weights.bin.tmp"));

        var previousOut = Console.Out;
        var previousError = Console.Error;
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        Console.SetOut(stdout);
        Console.SetError(stderr);
        int exitCode;
        try
        {
            exitCode = await Program.Main(
            [
                "continue-orchestration-training",
                "--manifest",
                initialManifestPath,
                "--training-manifest",
                trainingManifestPath,
                "--output",
                outputDirectory,
                "--repository-root",
                fixture.RepositoryRoot,
                "--repository-dpo-dataset-version",
                Program.OrchestrationRepositoryOwnedDefaultModelVersion,
                "--repository-model-version",
                Program.OrchestrationRepositoryOwnedDefaultModelVersion,
                "--repository-checkpoint-step",
                "1",
                "--max-cycles",
                "1"
            ]);
        }
        finally
        {
            Console.SetOut(previousOut);
            Console.SetError(previousError);
        }

        Assert.Equal(3, exitCode);
        Assert.Contains("CHECKPOINT_WRITE_FAILED", stderr.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("continue-orchestration-training", stdout.ToString(), StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(outputDirectory, "weights.bin")));
        Assert.False(File.Exists(Path.Combine(outputDirectory, "continue-report.json")));
    }

    [Fact]
    public async Task ContinuationDpoMaterializer_RejectsChangedCurrentSourceRows()
    {
        using var fixture = new RepositoryFixture();
        var sealedRows = new[] { CreateReferenceRow("mode2-current-001") };
        var changedRows = new[] { CreateReferenceRow("mode2-current-002") };
        var dataset = await CreateAuthoredSealedOrchestrationDpoResolutionAsync(
            fixture,
            sealedRows);

        var materialized =
            Program.MaterializeRepositoryOwnedOrchestrationDpoPairsForContinuation(
                fixture.RepositoryRoot,
                changedRows,
                dataset,
                [],
                maxCycles: 1);

        Assert.False(materialized.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer
                .RequestIdentityMismatchErrorCode,
            materialized.ErrorCode);
    }

    [Theory]
    [InlineData("swapped-pairs")]
    [InlineData("missing-pair")]
    [InlineData("duplicate-manifest-pair")]
    public async Task ContinuationDpoMaterializer_RejectsSuppliedDatasetThatDiffersFromSealedReopen(
        string corruption)
    {
        using var fixture = new RepositoryFixture();
        var rows = new[]
        {
            CreateReferenceRow(
                "mode2-association-001",
                prompt: "prompt alpha unique to source row one",
                paidPlan: "PAID_ALPHA"),
            CreateReferenceRow(
                "mode2-association-002",
                prompt: "prompt beta unique to source row two",
                paidPlan: "PAID_BETA")
        };
        var dataset = await CreateAuthoredSealedOrchestrationDpoResolutionAsync(
            fixture,
            rows);
        var supplied = corruption switch
        {
            "swapped-pairs" => dataset with
            {
                Dataset = dataset.Dataset with
                {
                    Pairs = dataset.Dataset.Pairs.Reverse().ToArray()
                }
            },
            "missing-pair" => dataset with
            {
                Dataset = dataset.Dataset with
                {
                    Pairs = dataset.Dataset.Pairs.Take(1).ToArray()
                }
            },
            "duplicate-manifest-pair" => dataset with
            {
                Dataset = dataset.Dataset with
                {
                    Manifest = dataset.Dataset.Manifest with
                    {
                        OrderedPairs =
                        [
                            dataset.Dataset.Manifest.OrderedPairs[0],
                            dataset.Dataset.Manifest.OrderedPairs[0]
                        ]
                    }
                }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(corruption))
        };

        var materialized =
            Program.MaterializeRepositoryOwnedOrchestrationDpoPairsForContinuation(
                fixture.RepositoryRoot,
                rows,
                supplied,
                [],
                maxCycles: 1);

        Assert.False(materialized.IsSuccess);
        Assert.Equal(
            Program.OrchestrationRepositoryOwnedDpoDatasetBindingInvalidErrorCode,
            materialized.ErrorCode);
    }

    [Fact]
    public async Task ContinuationDpoMaterializer_RejectsUnboundSealedGovernanceAssociation()
    {
        using var fixture = new RepositoryFixture();
        var rows = new[] { CreateReferenceRow("mode2-unbound-001") };
        var dataset = await CreateAuthoredSealedOrchestrationDpoResolutionAsync(
            fixture,
            rows,
            pairs =>
            {
                var pair = Assert.Single(pairs);
                var rebound = DpoPairTrainingContentAuthority.Bind(pair with
                {
                    GovernanceDecision = new TrainingDataGovernanceDecision(
                        licenseAccepted: true,
                        isQuarantined: false,
                        ["sha256:" + new string('f', 64)])
                });
                Assert.True(rebound.IsSuccess, rebound.ErrorMessage);
                return [rebound.Value!];
            });

        var materialized =
            Program.MaterializeRepositoryOwnedOrchestrationDpoPairsForContinuation(
                fixture.RepositoryRoot,
                rows,
                dataset,
                [],
                maxCycles: 1);

        Assert.False(materialized.IsSuccess);
        Assert.Equal(
            Program.OrchestrationRepositoryOwnedDpoDatasetBindingInvalidErrorCode,
            materialized.ErrorCode);
    }

    [Fact]
    public async Task ContinuationDpoMaterializer_SkipsHeldOutAndReplayedDpoPairs()
    {
        using var fixture = new RepositoryFixture();
        var rows = new[]
        {
            CreateReferenceRow("mode2-replay-001", prompt: "first train prompt"),
            CreateReferenceRow(
                "mode2-heldout-001",
                prompt: "heldout prompt",
                isHeldOut: true),
            CreateReferenceRow("mode2-replay-002", prompt: "second train prompt")
        };
        var dataset = await CreateAuthoredSealedOrchestrationDpoResolutionAsync(
            fixture,
            rows);
        var allNew =
            Program.MaterializeRepositoryOwnedOrchestrationDpoPairsForContinuation(
                fixture.RepositoryRoot,
                rows,
                dataset,
                [],
                maxCycles: 1);
        Assert.True(allNew.IsSuccess, allNew.ErrorMessage);
        Assert.Equal(2, allNew.Value!.Count);
        Assert.DoesNotContain(
            allNew.Value,
            pair => string.Equals(
                pair.Prompt,
                rows[1].Prompt,
                StringComparison.Ordinal));

        var replayedPairId = allNew.Value[0].PairId;
        var next =
            Program.MaterializeRepositoryOwnedOrchestrationDpoPairsForContinuation(
                fixture.RepositoryRoot,
                rows,
                dataset,
                [replayedPairId],
                maxCycles: 1);

        Assert.True(next.IsSuccess, next.ErrorMessage);
        Assert.Single(next.Value!);
        Assert.DoesNotContain(
            next.Value!,
            pair => string.Equals(
                pair.PairId,
                replayedPairId,
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task ContinuationDpoMaterializer_RejectsOverflowingCycleBound()
    {
        using var fixture = new RepositoryFixture();
        var rows = new[] { CreateReferenceRow("mode2-overflow-001") };
        var dataset = await CreateAuthoredSealedOrchestrationDpoResolutionAsync(
            fixture,
            rows);

        var materialized =
            Program.MaterializeRepositoryOwnedOrchestrationDpoPairsForContinuation(
                fixture.RepositoryRoot,
                rows,
                dataset,
                [],
                maxCycles: int.MaxValue);

        Assert.False(materialized.IsSuccess);
        Assert.Equal(
            Program.OrchestrationRepositoryOwnedDpoDatasetBindingInvalidErrorCode,
            materialized.ErrorCode);
    }

    private static RepositoryOwnedDpoDatasetManifest CreateSealedOrchestrationDpoDataset(
        RepositoryFixture fixture,
        string datasetVersion)
        => CreateSealedOrchestrationDpoDataset(
            fixture,
            datasetVersion,
            RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity(
                [new string('a', 64)],
                CreateOpenTeacherIdentity(),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "programming-task-orchestration-dpo",
                    "v1",
                    new string('c', 64)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "orchestration-qwen-dpo-author",
                    "v1",
                    new string('d', 64))).Value!);

    private static RepositoryOwnedDpoDatasetManifest CreateSealedOrchestrationDpoDataset(
        RepositoryFixture fixture,
        string datasetVersion,
        RepositoryOwnedDpoDatasetRequestIdentity identity)
    {
        // Этот helper строит минимальный настоящий repository-owned DPO-набор:
        // выбранная сторона создана локальной Qwen, отвергнутая сторона — локальной
        // моделью. Платный эталон остаётся только в hash-identity и не попадает в
        // trainable стороны пары.
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            fixture.RepositoryRoot,
            Program.OrchestrationRepositoryOwnedDomainSlug,
            Program.OrchestrationRepositoryOwnedDesignPatternSlug,
            datasetVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var teacher = identity.OpenTeacher;
        var provenance = new TrainingTeacherProvenance(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            teacher.ModelId,
            teacher.ModelVersion,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            "orchestration-dpo-test-request",
            InvocationCount: 1)
        {
            ModelArtifactDigestKind = teacher.ModelArtifactDigestKind,
            ModelArtifactDigest = teacher.ModelArtifactSha256,
        };
        var sideAuthorship = new DpoPairSideAuthorship(
            DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
            provenance);
        var localAuthorship = new DpoPairSideAuthorship(
            DPO.Models.DpoPairSideAuthorKind.LocalModel);
        var governance = new TrainingDataGovernanceDecision(
            licenseAccepted: true,
            isQuarantined: false,
            ["sha256:" + new string('f', 64)]);
        var pair = DpoPairTrainingContentAuthority.Bind(new DpoPair(
            "Break this coding project request into ordered implementation phases.",
            "Plan the dependency order, run the focused checks, and publish the verified slice.",
            "Start editing a random file before reading the project state.",
            Program.OrchestrationRepositoryOwnedDomainSlug,
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = provenance,
            GovernanceDecision = governance,
            ChosenAuthorship = sideAuthorship,
            RejectedAuthorship = localAuthorship,
        });
        Assert.True(pair.IsSuccess, pair.ErrorMessage);
        var manifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address.Value!,
            identity,
            [pair.Value!]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
        return manifest.Value!;
    }

    private static async Task<RepositoryOwnedDpoDatasetResolution>
        CreateAuthoredSealedOrchestrationDpoResolutionAsync(
            RepositoryFixture fixture,
            IReadOnlyList<OrchestrationTrainingPair> rows,
            Func<IReadOnlyList<DpoPair>, IReadOnlyList<DpoPair>>? mutatePairs =
                null)
    {
        // Fake teacher только возвращает заранее заданный JSON для теста.
        // Реальный Qwen/Ollama здесь не запускается и не проверяется.
        var context = CreateAuthoringContext(
            () => OperationResult<ITrainingTeacherInferenceClient>.Ok(
                new CountingTeacherClient(
                    CreateOpenTeacherIdentity(),
                    """
                    {"chosen":"Teacher chosen exact orchestration continuation: reopen the saved DPO pair, keep bound routing labels, run the focused update, and stop after evidence.","rejected":"Teacher rejected exact orchestration continuation: train from the paid reference answer instead of the saved DPO pair.","rejected_reason":"it replaces the open teacher pair with a paid-reference side"}
                    """)),
            rows.Count);
        var prepared =
            OrchestrationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(
                rows,
                context);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            fixture.RepositoryRoot,
            Program.OrchestrationRepositoryOwnedDomainSlug,
            Program.OrchestrationRepositoryOwnedDesignPatternSlug,
            Program.OrchestrationRepositoryOwnedDefaultModelVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var authored =
            await OrchestrationRepositoryOwnedDpoDatasetAuthoring.AuthorPairsAsync(
                address.Value!,
                prepared.Value!,
                context);
        Assert.True(authored.IsSuccess, authored.ErrorMessage);
        var persisted = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address.Value!,
            prepared.Value!.RequestIdentity,
            mutatePairs is null ? authored.Value! : mutatePairs(authored.Value!));
        Assert.True(persisted.IsSuccess, persisted.ErrorMessage);
        var opened = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
            address.Value!,
            prepared.Value.RequestIdentity);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        return new RepositoryOwnedDpoDatasetResolution(
            opened.Value!,
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset);
    }

    private static string CreateRepositoryOwnedContinuationCheckpointDirectory(
        RepositoryFixture fixture,
        int checkpointStep)
        => Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "models",
            Program.OrchestrationRepositoryOwnedDomainSlug,
            Program.OrchestrationRepositoryOwnedDesignPatternSlug,
            Program.OrchestrationRepositoryOwnedModelIdSlug,
            Program.OrchestrationRepositoryOwnedDefaultModelVersion,
            "checkpoints",
            checkpointStep.ToString("D6"));

    private static async Task WriteGzipOrchestrationRowsAsync(
        string path,
        IReadOnlyList<OrchestrationTrainingPair> rows)
    {
        await using var file = File.Create(path);
        await using var gzip = new GZipStream(
            file,
            CompressionLevel.SmallestSize);
        await using var writer = new StreamWriter(gzip, Encoding.UTF8);
        var jsonLineOptions = new JsonSerializerOptions(
            OrchestrationPlanJsonOptions.Standard)
        {
            WriteIndented = false
        };

        foreach (var row in rows)
            await writer.WriteLineAsync(
                JsonSerializer.Serialize(row, jsonLineOptions));
    }

    private static string Sha256FileForTest(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static OrchestrationRepositoryOwnedDpoDatasetAuthoringContext
        CreateAuthoringContext(
            Func<OperationResult<ITrainingTeacherInferenceClient>> createTeacher,
            int maxPairsToAuthor = 1)
        => new(
            createTeacher,
            CreateOpenTeacherIdentity(),
            new RepositoryOwnedDpoGenerationPolicyIdentity(
                "orchestration-local-qwen-dpo-pair-generation",
                "v1",
                new string('c', 64)),
            new RepositoryOwnedDpoCreatorIdentity(
                "orchestration-repository-owned-dpo-dataset-authoring",
                "v1",
                new string('d', 64)),
            MaxPairsToAuthor: maxPairsToAuthor);

    private static RepositoryOwnedDpoOpenTeacherIdentity CreateOpenTeacherIdentity()
        => new(
            OpenWeightTeacherFamily.Qwen.ToString(),
            "qwen-test:9b",
            new string('c', 64),
            LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            new string('d', 64));

    private static OrchestrationTrainingPair CreateReferenceRow(
        string pairId,
        string? prompt = null,
        string? paidPlan = null,
        bool isHeldOut = false)
        => new(
            pairId,
            PairMode.Mode2Generalization,
            prompt ?? "continue a coding project after a user adds storage constraints",
            new OrchestrationPlanChosen(
                "FLOW-02",
                "Dependency-safe continuation",
                "docs/flows/FLOW-02.md",
                paidPlan ??
                "Read the current repository, choose one safe product slice, verify it, then commit and push before continuing."),
            "Skip the repository check and start unrelated implementation.",
            GenerationRouteMode.Mode2Generalization,
            BoundarySource.Classifier,
            ExpectedMlGenerated: true,
            IntentKind.ImplementationExecutionRequest,
            "FLOW-02",
            "orchestration-repository-owned-dpo-test",
            IsHeldOut: isHeldOut);

    private sealed class CountingTeacherClient(
        RepositoryOwnedDpoOpenTeacherIdentity teacher,
        string output)
        : ITrainingTeacherInferenceClient
    {
        public int InferenceCalls { get; private set; }

        public Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(
            string prompt,
            ITrainingTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
            {
                return Task.FromResult(
                    issued.MapError<TrainingTeacherInferenceResult>());
            }

            InferenceCalls++;
            var requestId =
                $"orchestration-dpo-teacher-test-{InferenceCalls:D6}";
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                teacher.ModelId,
                teacher.ModelVersion,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                requestId,
                InvocationCount: InferenceCalls)
            {
                ModelArtifactDigestKind = teacher.ModelArtifactDigestKind,
                ModelArtifactDigest = teacher.ModelArtifactSha256,
            };
            return Task.FromResult(
                OperationResult<TrainingTeacherInferenceResult>.Ok(
                    new TrainingTeacherInferenceResult(
                        output,
                        provenance)
                    {
                        InferenceProfile =
                            LocalQwenOllamaInferenceProfileAuthority
                                .Authoring
                    }));
        }

        public void Dispose()
        {
        }
    }

    private sealed class RepositoryFixture : IDisposable
    {
        public RepositoryFixture()
        {
            RepositoryRoot = Path.Combine(
                Path.GetTempPath(),
                "llm-mvp-orchestration-repo-output",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(RepositoryRoot);
            Directory.CreateDirectory(Path.Combine(RepositoryRoot, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(RepositoryRoot, "11.DPO"));
            File.WriteAllText(Path.Combine(RepositoryRoot, "LlmFromScratch.sln"), string.Empty);
        }

        public string RepositoryRoot { get; }

        public void Dispose()
        {
            if (Directory.Exists(RepositoryRoot))
                Directory.Delete(RepositoryRoot, recursive: true);
        }
    }
}
