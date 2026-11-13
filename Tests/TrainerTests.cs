// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DatasetPipeline;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TrainingLoop.Schedulers;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public class TrainerTests
{
    public static IEnumerable<object[]> InvalidConfigurations()
    {
        yield return ["batch-size"];
        yield return ["sequence-length"];
        yield return ["accumulation"];
        yield return ["total-steps"];
        yield return ["checkpoint-cadence"];
        yield return ["log-cadence"];
        yield return ["negative-warmup"];
        yield return ["oversized-warmup"];
        yield return ["nan-max-lr"];
        yield return ["zero-max-lr"];
        yield return ["negative-min-lr"];
        yield return ["min-above-max"];
        yield return ["clip-norm"];
        yield return ["weight-decay"];
        yield return ["checkpoint-directory"];
        yield return ["model-context"];
    }

    [Theory]
    [MemberData(nameof(InvalidConfigurations))]
    public async Task InvalidPretrainingConfiguration_IsRejectedBeforeOwnershipOrSideEffects(
        string invalidField)
    {
        using var directory = new TrainerTestDirectory();
        var model = CreateModel();
        var config = InvalidConfiguration(
            CreateConfig(directory.Checkpoints),
            invalidField);
        var trainer = new Trainer(config);
        var optimizer = new AdamW(weightDecay: 0f);
        int providerCalls = 0;
        int saverCalls = 0;

        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            trainer.RunPretrainingCoreAsync(
                model,
                optimizer,
                () =>
                {
                    providerCalls++;
                    return ValidBatch();
                },
                _ => saverCalls++));

        Assert.Equal(TrainerRunState.NotStarted, trainer.RunState);
        Assert.Equal(0, trainer.CompletedSteps);
        Assert.Equal(0, providerCalls);
        Assert.Equal(0, saverCalls);
        if (!string.IsNullOrWhiteSpace(config.CheckpointDir))
            Assert.False(Directory.Exists(config.CheckpointDir));
    }

    [Fact]
    public async Task PublicTrainAsync_ConsumesRealDataLoaderAndSavesLoadableCheckpoints()
    {
        using var directory = new TrainerTestDirectory();
        using var dataset = new TemporaryNpyDirectory();
        NpyTestFile.Write(dataset.File("tokens.npy"), 1, 2, 3, 4);
        var model = CreateModel();
        var before = SnapshotTrainableParameters(model);
        var trainer = new Trainer(CreateConfig(
            directory.Checkpoints,
            totalSteps: 1,
            checkpointEvery: 1));
        // Шаг в один токен задан явно: проверка доказывает, что обучатель
        // действительно потребил одну партию, и читает следующую, ожидая
        // продвижения ровно на один токен. После переноса опыта номер 68 шаг по
        // умолчанию равен длине окна, и на осколке из четырёх токенов прижатое к
        // концу последнее окно случайно даёт те же числа — без явного шага
        // проверка перестала бы различать факт потребления партии от нового
        // поведения по умолчанию.
        var loader = new DataLoader(
            dataset.Path,
            batchSize: 1,
            seqLen: 2,
            positionStride: 1);

        await trainer.TrainAsync(
            model,
            new AdamW(weightDecay: 0f),
            loader);

        Assert.Equal(TrainerRunState.Completed, trainer.RunState);
        Assert.Equal(1, trainer.CompletedSteps);
        var (nextInputs, nextTargets) = loader.NextBatch();
        Assert.Equal(new long[] { 2, 3 }, BatchRow(nextInputs, 0));
        Assert.Equal(new long[] { 3, 4 }, BatchRow(nextTargets, 0));

        var periodicPath = Path.Combine(
            directory.Checkpoints,
            "ckpt_000001.bin");
        var finalPath = Path.Combine(
            directory.Checkpoints,
            "model-final.bin");
        Assert.True(File.Exists(periodicPath));
        Assert.True(new FileInfo(periodicPath).Length > 0);
        Assert.True(File.Exists(finalPath));
        Assert.True(new FileInfo(finalPath).Length > 0);
        var after = SnapshotTrainableParameters(model);
        Assert.Contains(
            before.Zip(after),
            pair => !pair.First.SequenceEqual(pair.Second));

        var fresh = CreateModel();
        fresh.LoadCheckpoint(finalPath);
        AssertParameterBitsEqual(after, fresh);
    }

    [Fact]
    public async Task NoTrainableParameters_FailsBeforeOwnershipOrSideEffectsAndIsReusable()
    {
        using var directory = new TrainerTestDirectory();
        var model = CreateModel();
        var parameters = model.Parameters()
            .Select(parameter => parameter.Value)
            .ToArray();
        foreach (var parameter in parameters)
            parameter.RequiresGrad = false;
        var before = parameters
            .Select(parameter => parameter.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray())
            .ToArray();
        var trainer = new Trainer(CreateConfig(directory.Checkpoints));
        int providerCalls = 0;
        int saverCalls = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            trainer.RunPretrainingCoreAsync(
                model,
                new AdamW(weightDecay: 0f),
                () =>
                {
                    providerCalls++;
                    return ValidBatch();
                },
                _ => saverCalls++));

        Assert.Equal(TrainerRunState.NotStarted, trainer.RunState);
        Assert.Equal(0, trainer.CompletedSteps);
        Assert.Equal(0, providerCalls);
        Assert.Equal(0, saverCalls);
        Assert.False(Directory.Exists(directory.Checkpoints));
        for (int index = 0; index < parameters.Length; index++)
        {
            Assert.Equal(
                before[index],
                parameters[index].Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }

        parameters[0].RequiresGrad = true;
        await trainer.RunPretrainingCoreAsync(
            model,
            new AdamW(weightDecay: 0f),
            ValidBatch,
            _ => { });

        Assert.Equal(TrainerRunState.Completed, trainer.RunState);
        Assert.Equal(1, trainer.CompletedSteps);
    }

    [Fact]
    public async Task PreCancelledRun_HasNoSideEffectsAndDoesNotClaimTrainer()
    {
        using var directory = new TrainerTestDirectory();
        var model = CreateModel();
        var trainer = new Trainer(CreateConfig(directory.Checkpoints));
        var optimizer = new AdamW(weightDecay: 0f);
        var before = SnapshotTrainableParameters(model);
        int providerCalls = 0;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            trainer.RunPretrainingCoreAsync(
                model,
                optimizer,
                () =>
                {
                    providerCalls++;
                    return ValidBatch();
                },
                _ => throw new InvalidOperationException("Saver must not run."),
                cancellation.Token));

        Assert.Equal(TrainerRunState.NotStarted, trainer.RunState);
        Assert.Equal(0, trainer.CompletedSteps);
        Assert.Equal(0, providerCalls);
        Assert.False(Directory.Exists(directory.Checkpoints));
        AssertParameterBitsEqual(before, model);

        await trainer.RunPretrainingCoreAsync(
            model,
            optimizer,
            ValidBatch,
            _ => { });

        Assert.Equal(TrainerRunState.Completed, trainer.RunState);
        Assert.Equal(1, trainer.CompletedSteps);
    }

    [Fact(Timeout = 10000)]
    public async Task ConcurrentAndRepeatedRuns_AreRejectedByOneShotOwner()
    {
        using var directory = new TrainerTestDirectory();
        var model = CreateModel();
        var trainer = new Trainer(CreateConfig(directory.Checkpoints));
        var optimizer = new AdamW(weightDecay: 0f);
        using var entered = new ManualResetEventSlim(false);
        using var release = new ManualResetEventSlim(false);
        int competingProviderCalls = 0;

        Task firstRun = Task.Run(() =>
            trainer.RunPretrainingCoreAsync(
                model,
                optimizer,
                () =>
                {
                    entered.Set();
                    release.Wait();
                    return ValidBatch();
                },
                _ => { }));

        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                trainer.RunPretrainingCoreAsync(
                    model,
                    new AdamW(weightDecay: 0f),
                    () =>
                    {
                        competingProviderCalls++;
                        return ValidBatch();
                    },
                    _ => { }));
        }
        finally
        {
            release.Set();
        }

        await firstRun;
        Assert.Equal(0, competingProviderCalls);
        Assert.Equal(TrainerRunState.Completed, trainer.RunState);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            trainer.RunPretrainingCoreAsync(
                model,
                new AdamW(weightDecay: 0f),
                () =>
                {
                    competingProviderCalls++;
                    return ValidBatch();
                },
                _ => { }));

        Assert.Equal(0, competingProviderCalls);
        Assert.Equal(1, trainer.CompletedSteps);
    }

    [Fact]
    public async Task SupervisedSteps_DoNotAdvancePretrainingCounterOrScheduler()
    {
        using var directory = new TrainerTestDirectory();
        var config = CreateConfig(directory.Checkpoints) with
        {
            MaxLr = 0.01f,
            MinLr = 0f,
        };
        var modelAfterSupervised = CreateModel();
        var trainerAfterSupervised = new Trainer(config);
        _ = trainerAfterSupervised.TrainSupervisedNextTokenStep(
            modelAfterSupervised,
            new AdamW(weightDecay: 0f),
            [1, 2, 3],
            1f);
        Assert.Equal(0, trainerAfterSupervised.CompletedSteps);
        Assert.Equal(TrainerRunState.NotStarted, trainerAfterSupervised.RunState);

        var clonedCheckpoint = directory.File("after-supervised.bin");
        modelAfterSupervised.SaveCheckpoint(clonedCheckpoint);
        var controlModel = CreateModel();
        controlModel.LoadCheckpoint(clonedCheckpoint);
        var controlTrainer = new Trainer(config);

        await trainerAfterSupervised.RunPretrainingCoreAsync(
            modelAfterSupervised,
            new AdamW(weightDecay: 0f),
            ValidBatch,
            _ => { });
        await controlTrainer.RunPretrainingCoreAsync(
            controlModel,
            new AdamW(weightDecay: 0f),
            ValidBatch,
            _ => { });

        Assert.Equal(1, trainerAfterSupervised.CompletedSteps);
        Assert.Equal(1, controlTrainer.CompletedSteps);
        AssertParameterBitsEqual(
            SnapshotTrainableParameters(controlModel),
            modelAfterSupervised);
    }

    // Разогрев в дообучении: только самый первый шаг берёт скорость первого шага
    // расписания, остальные шаги остаются продуктовыми. Применённая скорость
    // наблюдается по фактическому сдвигу весов, а не по одному вызову расписания.
    [Fact]
    public void SupervisedWarmup_UsesStepOneRateOnlyOnTheFirstStep()
    {
        using var directory = new TrainerTestDirectory();
        var config = CreateConfig(directory.Checkpoints) with
        {
            WarmupSteps = 4,
            TotalSteps = 8,
            MaxLr = 0.001f,
            MinLr = 0.0001f,
        };
        var schedule = new CosineWithWarmup(
            config.WarmupSteps, config.TotalSteps, config.MaxLr, config.MinLr);
        // Измеренная политика: нулевой шаг получает скорость первого шага, дальше продукт.
        var measuredRates = new[]
        {
            schedule.GetLr(1),
            schedule.GetLr(1),
            schedule.GetLr(2),
            schedule.GetLr(3),
        };
        Assert.Equal(0f, schedule.GetLr(0));
        Assert.Equal(new[] { 0.00025f, 0.00025f, 0.0005f, 0.00075f }, measuredRates);

        var (warmupModel, referenceModel) = TwinModels(directory.File("warmup-twin.bin"));
        var warmupTrainer = new Trainer(config);
        var warmupOptimizer = new AdamW(weightDecay: 0f);
        var referenceOptimizer = new AdamW(weightDecay: 0f);
        var before = SnapshotTrainableParameters(warmupModel);

        for (var step = 0; step < measuredRates.Length; step++)
        {
            // Проверяемое плечо: продуктовый путь с разогревом считает скорость сам.
            warmupTrainer.TrainSupervisedNextTokenStep(
                warmupModel, warmupOptimizer, [1, 2, 3], 1f);
            // Эталон: та же скорость задана явно, разогрева в его расписании нет.
            new Trainer(config with
            {
                WarmupSteps = 0,
                MaxLr = measuredRates[step],
                MinLr = measuredRates[step],
            }).TrainSupervisedNextTokenStep(referenceModel, referenceOptimizer, [1, 2, 3], 1f);
        }

        // Первый урок дошёл до весов: состояние сдвинулось.
        Assert.Contains(
            before.Zip(SnapshotTrainableParameters(warmupModel)),
            pair => !pair.First.SequenceEqual(pair.Second));
        // И продуктовый путь совпал с эталоном измеренной политики число в число.
        AssertParameterBitsEqual(
            SnapshotTrainableParameters(referenceModel),
            warmupModel);
    }

    // Без разогрева поправка нулевого шага не участвует: каждый шаг дообучения
    // берёт ровно ту скорость, которую расписание даёт для своего номера.
    [Fact]
    public void SupervisedWithoutWarmup_KeepsProductRateOnEveryStep()
    {
        using var directory = new TrainerTestDirectory();
        var config = CreateConfig(directory.Checkpoints) with
        {
            WarmupSteps = 0,
            TotalSteps = 8,
            MaxLr = 0.001f,
            MinLr = 0.0001f,
        };
        var schedule = new CosineWithWarmup(
            config.WarmupSteps, config.TotalSteps, config.MaxLr, config.MinLr);
        var productRates = Enumerable.Range(0, 4).Select(schedule.GetLr).ToArray();
        // Нулевой шаг без разогрева положителен и остаётся прежним числом.
        Assert.Equal(config.MaxLr, productRates[0]);

        var (plainModel, referenceModel) = TwinModels(directory.File("no-warmup-twin.bin"));
        var plainTrainer = new Trainer(config);
        var plainOptimizer = new AdamW(weightDecay: 0f);
        var referenceOptimizer = new AdamW(weightDecay: 0f);

        for (var step = 0; step < productRates.Length; step++)
        {
            plainTrainer.TrainSupervisedNextTokenStep(
                plainModel, plainOptimizer, [1, 2, 3], 1f);
            new Trainer(config with
            {
                MaxLr = productRates[step],
                MinLr = productRates[step],
            }).TrainSupervisedNextTokenStep(referenceModel, referenceOptimizer, [1, 2, 3], 1f);
        }

        AssertParameterBitsEqual(
            SnapshotTrainableParameters(referenceModel),
            plainModel);
    }

    // Две модели с полностью одинаковыми начальными числами: вторая загружается
    // из контрольной точки первой, поэтому сравнение плеч честное.
    private static (LlmModel First, LlmModel Second) TwinModels(string checkpoint)
    {
        var first = CreateModel();
        first.SaveCheckpoint(checkpoint);
        var second = CreateModel();
        second.LoadCheckpoint(checkpoint);
        AssertParameterBitsEqual(SnapshotTrainableParameters(first), second);
        return (first, second);
    }

    public static IEnumerable<object[]> InvalidBatches()
    {
        yield return ["input-shape"];
        yield return ["target-shape"];
        yield return ["negative-input"];
        yield return ["negative-target"];
        yield return ["overflow-input"];
        yield return ["overflow-target"];
    }

    [Theory]
    [MemberData(nameof(InvalidBatches))]
    public async Task InvalidBatch_FailsBeforeOptimizerOrCheckpointAndClearsGradients(
        string invalidBatch)
    {
        using var directory = new TrainerTestDirectory();
        var model = CreateModel();
        var trainer = new Trainer(CreateConfig(directory.Checkpoints));
        var before = SnapshotTrainableParameters(model);
        int saverCalls = 0;

        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            trainer.RunPretrainingCoreAsync(
                model,
                new AdamW(weightDecay: 0f),
                () => BuildInvalidBatch(invalidBatch, model.Config.VocabSize),
                _ => saverCalls++));

        Assert.Equal(TrainerRunState.Faulted, trainer.RunState);
        Assert.Equal(0, trainer.CompletedSteps);
        Assert.Equal(0, saverCalls);
        AssertParameterBitsEqual(before, model);
        AssertGradientsCleared(model);
    }

    [Fact]
    public async Task CancellationDuringAccumulation_DoesNotCommitAndClearsGradients()
    {
        using var directory = new TrainerTestDirectory();
        var model = CreateModel();
        var config = CreateConfig(directory.Checkpoints) with
        {
            GradAccumSteps = 2,
        };
        var trainer = new Trainer(config);
        var before = SnapshotTrainableParameters(model);
        using var cancellation = new CancellationTokenSource();
        int providerCalls = 0;
        int saverCalls = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            trainer.RunPretrainingCoreAsync(
                model,
                new AdamW(weightDecay: 0f),
                () =>
                {
                    providerCalls++;
                    if (providerCalls == 2)
                        cancellation.Cancel();
                    return ValidBatch();
                },
                _ => saverCalls++,
                cancellation.Token));

        Assert.Equal(2, providerCalls);
        Assert.Equal(0, saverCalls);
        Assert.Equal(0, trainer.CompletedSteps);
        Assert.Equal(TrainerRunState.Cancelled, trainer.RunState);
        AssertParameterBitsEqual(before, model);
        AssertGradientsCleared(model);

        int retryProviderCalls = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            trainer.RunPretrainingCoreAsync(
                model,
                new AdamW(weightDecay: 0f),
                () =>
                {
                    retryProviderCalls++;
                    return ValidBatch();
                },
                _ => { }));

        Assert.Equal(0, retryProviderCalls);
        Assert.Equal(TrainerRunState.Cancelled, trainer.RunState);
    }

    [Fact]
    public async Task CancellationInsidePeriodicSave_IsObservedAtNextStepBoundary()
    {
        using var directory = new TrainerTestDirectory();
        var model = CreateModel();
        var trainer = new Trainer(CreateConfig(
            directory.Checkpoints,
            totalSteps: 2,
            checkpointEvery: 1));
        using var cancellation = new CancellationTokenSource();
        var saveNames = new List<string>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            trainer.RunPretrainingCoreAsync(
                model,
                new AdamW(weightDecay: 0f),
                ValidBatch,
                path =>
                {
                    saveNames.Add(Path.GetFileName(path));
                    cancellation.Cancel();
                },
                cancellation.Token));

        Assert.Equal(["ckpt_000001.bin"], saveNames);
        Assert.Equal(1, trainer.CompletedSteps);
        Assert.Equal(TrainerRunState.Cancelled, trainer.RunState);
        AssertGradientsCleared(model);
    }

    [Fact]
    public async Task CancellationInsideFinalPeriodicSave_DoesNotInterruptCommittedSaves()
    {
        using var directory = new TrainerTestDirectory();
        var trainer = new Trainer(CreateConfig(
            directory.Checkpoints,
            totalSteps: 1,
            checkpointEvery: 1));
        using var cancellation = new CancellationTokenSource();
        var saveNames = new List<string>();

        await trainer.RunPretrainingCoreAsync(
            CreateModel(),
            new AdamW(weightDecay: 0f),
            ValidBatch,
            path =>
            {
                saveNames.Add(Path.GetFileName(path));
                if (saveNames.Count == 1)
                    cancellation.Cancel();
            },
            cancellation.Token);

        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(
            ["ckpt_000001.bin", "model-final.bin"],
            saveNames);
        Assert.Equal(1, trainer.CompletedSteps);
        Assert.Equal(TrainerRunState.Completed, trainer.RunState);
    }

    [Fact]
    public async Task TwoStepRun_UsesCompletedStepCheckpointNamesAndFinalSave()
    {
        using var directory = new TrainerTestDirectory();
        var trainer = new Trainer(CreateConfig(
            directory.Checkpoints,
            totalSteps: 2,
            checkpointEvery: 1));
        var saveNames = new List<string>();

        await trainer.RunPretrainingCoreAsync(
            CreateModel(),
            new AdamW(weightDecay: 0f),
            ValidBatch,
            path => saveNames.Add(Path.GetFileName(path)));

        Assert.Equal(
            ["ckpt_000001.bin", "ckpt_000002.bin", "model-final.bin"],
            saveNames);
        Assert.Equal(2, trainer.CompletedSteps);
        Assert.Equal(TrainerRunState.Completed, trainer.RunState);
    }

    [Fact]
    public async Task PeriodicSaveFailure_RetainsCommittedCountAndConsumesTrainer()
    {
        using var directory = new TrainerTestDirectory();
        var model = CreateModel();
        var trainer = new Trainer(CreateConfig(
            directory.Checkpoints,
            totalSteps: 2,
            checkpointEvery: 1));
        int saverCalls = 0;
        int retryProviderCalls = 0;

        await Assert.ThrowsAsync<IOException>(() =>
            trainer.RunPretrainingCoreAsync(
                model,
                new AdamW(weightDecay: 0f),
                ValidBatch,
                _ =>
                {
                    saverCalls++;
                    throw new IOException("Injected checkpoint failure.");
                }));

        Assert.Equal(1, saverCalls);
        Assert.Equal(1, trainer.CompletedSteps);
        Assert.Equal(TrainerRunState.Faulted, trainer.RunState);
        AssertGradientsCleared(model);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            trainer.RunPretrainingCoreAsync(
                model,
                new AdamW(weightDecay: 0f),
                () =>
                {
                    retryProviderCalls++;
                    return ValidBatch();
                },
                _ => { }));

        Assert.Equal(0, retryProviderCalls);
        Assert.Equal(TrainerRunState.Faulted, trainer.RunState);
    }

    [Fact]
    public async Task FinalCheckpoint_CanBeLoadedIntoFreshCompatibleModel()
    {
        using var directory = new TrainerTestDirectory();
        var config = CreateConfig(
            directory.Checkpoints,
            totalSteps: 1,
            checkpointEvery: 2);
        var model = CreateModel();
        var trainer = new Trainer(config);

        await trainer.RunPretrainingCoreAsync(
            model,
            new AdamW(weightDecay: 0f),
            ValidBatch,
            model.SaveCheckpoint);

        var finalPath = Path.Combine(directory.Checkpoints, "model-final.bin");
        Assert.True(File.Exists(finalPath));
        Assert.True(new FileInfo(finalPath).Length > 0);

        var fresh = CreateModel();
        fresh.LoadCheckpoint(finalPath);
        AssertParameterBitsEqual(SnapshotTrainableParameters(model), fresh);
    }

    private static TrainingConfig CreateConfig(
        string checkpointDirectory,
        int totalSteps = 1,
        int checkpointEvery = 1)
        => new()
        {
            BatchSize = 1,
            SeqLen = 2,
            MaxLr = 1e-3f,
            MinLr = 1e-3f,
            WarmupSteps = 0,
            TotalSteps = totalSteps,
            GradAccumSteps = 1,
            GradClipNorm = 1f,
            WeightDecay = 0f,
            CheckpointEvery = checkpointEvery,
            CheckpointDir = checkpointDirectory,
            LogEvery = 100,
        };

    private static TrainingConfig InvalidConfiguration(
        TrainingConfig valid,
        string invalidField)
        => invalidField switch
        {
            "batch-size" => valid with { BatchSize = 0 },
            "sequence-length" => valid with { SeqLen = 0 },
            "accumulation" => valid with { GradAccumSteps = 0 },
            "total-steps" => valid with { TotalSteps = 0 },
            "checkpoint-cadence" => valid with { CheckpointEvery = 0 },
            "log-cadence" => valid with { LogEvery = 0 },
            "negative-warmup" => valid with { WarmupSteps = -1 },
            "oversized-warmup" => valid with { WarmupSteps = valid.TotalSteps + 1 },
            "nan-max-lr" => valid with { MaxLr = float.NaN },
            "zero-max-lr" => valid with { MaxLr = 0f },
            "negative-min-lr" => valid with { MinLr = -0.01f },
            "min-above-max" => valid with { MinLr = valid.MaxLr + 0.01f },
            "clip-norm" => valid with { GradClipNorm = 0f },
            "weight-decay" => valid with { WeightDecay = float.NaN },
            "checkpoint-directory" => valid with { CheckpointDir = "   " },
            "model-context" => valid with { SeqLen = CreateModel().Config.ContextLen + 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(invalidField)),
        };

    private static LlmModel CreateModel()
        => new(new ModelConfig
        {
            VocabSize = 32,
            ContextLen = 4,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 2,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
        });

    private static (long[,] inputs, long[,] targets) ValidBatch()
        => (
            new long[,] { { 1, 2 } },
            new long[,] { { 2, 3 } });

    private static long[] BatchRow(long[,] batch, int row)
    {
        var values = new long[batch.GetLength(1)];
        for (int column = 0; column < values.Length; column++)
            values[column] = batch[row, column];
        return values;
    }

    private static (long[,] inputs, long[,] targets) BuildInvalidBatch(
        string invalidBatch,
        int vocabSize)
    {
        var valid = ValidBatch();
        return invalidBatch switch
        {
            "input-shape" => (new long[2, 2], valid.targets),
            "target-shape" => (valid.inputs, new long[1, 1]),
            "negative-input" => (new long[,] { { -1, 2 } }, valid.targets),
            "negative-target" => (valid.inputs, new long[,] { { -1, 3 } }),
            "overflow-input" => (
                new long[,] { { vocabSize, 2 } },
                valid.targets),
            "overflow-target" => (
                valid.inputs,
                new long[,] { { vocabSize, 3 } }),
            _ => throw new ArgumentOutOfRangeException(nameof(invalidBatch)),
        };
    }

    private static IReadOnlyList<int[]> SnapshotTrainableParameters(LlmModel model)
        => model.Parameters()
            .Where(parameter => parameter.Value.RequiresGrad)
            .Select(parameter => parameter.Value.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray())
            .ToArray();

    private static void AssertParameterBitsEqual(
        IReadOnlyList<int[]> expected,
        LlmModel actual)
    {
        var actualBits = SnapshotTrainableParameters(actual);
        Assert.Equal(expected.Count, actualBits.Count);
        for (int index = 0; index < expected.Count; index++)
            Assert.Equal(expected[index], actualBits[index]);
    }

    private static void AssertGradientsCleared(LlmModel model)
    {
        Assert.All(
            model.Parameters().Where(parameter => parameter.Value.RequiresGrad),
            parameter => Assert.True(
                parameter.Grad is null
                || parameter.Grad.Data.All(value => value == 0f)));
    }
}

file sealed class TrainerTestDirectory : IDisposable
{
    public string Root { get; } = Path.Combine(
        Path.GetTempPath(),
        $"trainer-tests-{Guid.NewGuid():N}");

    public string Checkpoints => Path.Combine(Root, "checkpoints");

    public TrainerTestDirectory()
        => Directory.CreateDirectory(Root);

    public string File(string fileName)
        => Path.Combine(Root, fileName);

    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
