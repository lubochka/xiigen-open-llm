// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DatasetPipeline;
using TensorCore.Core;
using TensorCore.Layers;
using TrainingLoop.Optimizers;
using TrainingLoop.Schedulers;
using TransformerModel;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LlmFromScratch.Tests")]

namespace TrainingLoop;

public enum TrainerRunState
{
    NotStarted,
    Running,
    Completed,
    Cancelled,
    Faulted,
}

// Main pretraining loop:
//   1. Acquire a batch
//   2. Forward -> cross-entropy loss
//   3. Backward -> gradients
//   4. Clip gradients
//   5. Optimizer step
//   6. Clear gradients
//
// The pretraining run is one-shot. Supervised incremental steps keep their own
// scheduler counter and do not consume or advance the pretraining lifecycle.
public class Trainer
{
    private readonly TrainingConfig _cfg;
    private readonly CosineWithWarmup _scheduler;
    private int _supervisedStep;
    private int _completedSteps;
    private int _runState = (int)TrainerRunState.NotStarted;

    public TrainerRunState RunState
        => (TrainerRunState)Volatile.Read(ref _runState);

    public int CompletedSteps
        => Volatile.Read(ref _completedSteps);

    public Trainer(TrainingConfig cfg)
    {
        ArgumentNullException.ThrowIfNull(cfg);
        _cfg = cfg;
        _scheduler = new CosineWithWarmup(
            cfg.WarmupSteps,
            cfg.TotalSteps,
            cfg.MaxLr,
            cfg.MinLr);
    }

    /// <summary>
    /// Runs exactly one weighted, supervised next-token training step over one
    /// token sequence. The caller controls which parameters are trainable via
    /// <see cref="Tensor.RequiresGrad"/>; frozen parameters are never passed to
    /// clipping, the optimizer, or gradient clearing.
    /// </summary>
    public float TrainSupervisedNextTokenStep(
        LlmModel model,
        AdamW optimizer,
        IReadOnlyList<int> tokenIds,
        float weight)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(optimizer);
        ArgumentNullException.ThrowIfNull(tokenIds);

        if (tokenIds.Count < 2 || tokenIds.Count > model.Config.ContextLen + 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tokenIds),
                $"Token sequence length must be between 2 and {model.Config.ContextLen + 1}.");
        }

        if (!float.IsFinite(weight) || weight <= 0f)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be finite and positive.");
        if (!float.IsFinite(_cfg.GradClipNorm) || _cfg.GradClipNorm <= 0f)
        {
            throw new InvalidOperationException(
                "Gradient clipping norm must be finite and positive.");
        }

        for (var index = 0; index < tokenIds.Count; index++)
        {
            if (tokenIds[index] < 0 || tokenIds[index] >= model.Config.VocabSize)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tokenIds),
                    $"Token id at index {index} is outside the model vocabulary.");
            }
        }

        var sequenceLength = tokenIds.Count - 1;
        var inputs = new int[sequenceLength];
        var targets = new int[sequenceLength];
        for (var index = 0; index < sequenceLength; index++)
        {
            inputs[index] = tokenIds[index];
            targets[index] = tokenIds[index + 1];
        }

        var trainableParameters = model.Parameters()
            .Where(parameter => parameter.Value.RequiresGrad)
            .ToArray();
        if (trainableParameters.Length == 0)
            throw new InvalidOperationException("No trainable parameters are available.");

        // Warmup makes the schedule return exactly zero on the very first
        // supervised step, and a zero rate is a refusal below. Only that one
        // step reads the rate of step 1 instead; every later step keeps its own
        // product rate, so the schedule as a whole is not shifted.
        var scheduleStep = _supervisedStep == 0 && _cfg.WarmupSteps > 0
            ? 1
            : _supervisedStep;
        // The rate is read before any model call: an unusable rate must cost
        // zero forward passes, zero backward passes and zero optimizer steps.
        var learningRate = _scheduler.GetLr(scheduleStep);
        if (!float.IsFinite(learningRate) || learningRate <= 0f)
        {
            // The gradient-clearing contract of the old refusal is preserved,
            // but no weight, moment, counter or random state has moved yet.
            ClearGradients(trainableParameters);
            throw new InvalidOperationException("Training learning rate must be finite and positive.");
        }

        var logits = model.Forward(ToTensor(inputs, batch: 1, sequenceLength), training: true);
        var loss = CrossEntropyLoss(logits, targets) * weight;
        if (!float.IsFinite(loss))
        {
            ClearGradients(trainableParameters);
            throw new InvalidOperationException("Supervised next-token loss is not finite.");
        }

        BackwardLoss(logits, targets, weight / sequenceLength);
        TensorOps.ClipGradNorm(trainableParameters, _cfg.GradClipNorm);

        optimizer.Step(trainableParameters, learningRate);
        if (trainableParameters.Any(
                parameter => parameter.Value.Data.Any(value => !float.IsFinite(value))))
        {
            ClearGradients(trainableParameters);
            throw new InvalidOperationException(
                "Supervised next-token training produced a non-finite parameter.");
        }

        ClearGradients(trainableParameters);
        _supervisedStep++;

        return loss;
    }

    public async Task TrainAsync(
        LlmModel model,
        AdamW optimizer,
        DataLoader loader,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(optimizer);
        ArgumentNullException.ThrowIfNull(loader);

        await RunPretrainingCoreAsync(
            model,
            optimizer,
            loader.NextBatch,
            model.SaveCheckpoint,
            ct);
    }

    internal async Task RunPretrainingCoreAsync(
        LlmModel model,
        AdamW optimizer,
        Func<(long[,] inputs, long[,] targets)> batchProvider,
        Action<string> checkpointSaver,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(optimizer);
        ArgumentNullException.ThrowIfNull(batchProvider);
        ArgumentNullException.ThrowIfNull(checkpointSaver);

        ValidatePretrainingConfiguration(model);
        var trainableParameters = model.Parameters()
            .Where(parameter => parameter.Value.RequiresGrad)
            .ToArray();
        if (trainableParameters.Length == 0)
            throw new InvalidOperationException("No trainable parameters are available.");

        ct.ThrowIfCancellationRequested();
        if (Interlocked.CompareExchange(
                ref _runState,
                (int)TrainerRunState.Running,
                (int)TrainerRunState.NotStarted)
            != (int)TrainerRunState.NotStarted)
        {
            throw new InvalidOperationException(
                "Pretraining can be started only once per Trainer instance.");
        }

        try
        {
            Directory.CreateDirectory(_cfg.CheckpointDir);

            while (CompletedSteps < _cfg.TotalSteps)
            {
                ct.ThrowIfCancellationRequested();
                float totalLoss = 0f;

                for (int accumulation = 0;
                     accumulation < _cfg.GradAccumSteps;
                     accumulation++)
                {
                    ct.ThrowIfCancellationRequested();
                    var (inputIds2d, targetIds2d) = batchProvider();
                    ct.ThrowIfCancellationRequested();
                    ValidateBatch(model, inputIds2d, targetIds2d);

                    int[] flatInput = Flatten2D(inputIds2d);
                    int[] flatTarget = Flatten2D(targetIds2d);
                    var inputTensor = ToTensor(
                        flatInput,
                        _cfg.BatchSize,
                        _cfg.SeqLen);

                    var logits = model.Forward(inputTensor, training: true);
                    float loss =
                        CrossEntropyLoss(logits, flatTarget) / _cfg.GradAccumSteps;
                    if (!float.IsFinite(loss))
                        throw new InvalidOperationException("Pretraining loss is not finite.");
                    totalLoss += loss;

                    BackwardLoss(
                        logits,
                        flatTarget,
                        1f / (_cfg.GradAccumSteps * _cfg.BatchSize * _cfg.SeqLen));
                }

                ct.ThrowIfCancellationRequested();

                TensorOps.ClipGradNorm(trainableParameters, _cfg.GradClipNorm);
                int previousCompletedSteps = CompletedSteps;
                float learningRate = _scheduler.GetLr(previousCompletedSteps);
                if (!float.IsFinite(learningRate) || learningRate < 0f)
                {
                    throw new InvalidOperationException(
                        "Pretraining learning rate must be finite and non-negative.");
                }

                ct.ThrowIfCancellationRequested();

                // Optimizer start is the commit boundary. Cancellation is next
                // observed only after the complete step transaction.
                optimizer.Step(trainableParameters, learningRate);
                int completedStep = checked(previousCompletedSteps + 1);
                Volatile.Write(ref _completedSteps, completedStep);
                ClearGradients(trainableParameters);

                if (completedStep % _cfg.LogEvery == 0)
                {
                    Console.WriteLine(
                        $"step={completedStep} loss={totalLoss:F4} lr={learningRate:E3}");
                }

                if (completedStep % _cfg.CheckpointEvery == 0)
                {
                    checkpointSaver(Path.Combine(
                        _cfg.CheckpointDir,
                        $"ckpt_{completedStep:D6}.bin"));
                }
            }

            checkpointSaver(Path.Combine(_cfg.CheckpointDir, "model-final.bin"));
            Volatile.Write(ref _runState, (int)TrainerRunState.Completed);
            await Task.CompletedTask;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            ClearGradients(trainableParameters);
            Volatile.Write(ref _runState, (int)TrainerRunState.Cancelled);
            throw;
        }
        catch
        {
            ClearGradients(trainableParameters);
            Volatile.Write(ref _runState, (int)TrainerRunState.Faulted);
            throw;
        }
    }

    private void ValidatePretrainingConfiguration(LlmModel model)
    {
        if (_cfg.BatchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(_cfg.BatchSize));
        if (_cfg.SeqLen <= 0)
            throw new ArgumentOutOfRangeException(nameof(_cfg.SeqLen));
        if (_cfg.GradAccumSteps <= 0)
            throw new ArgumentOutOfRangeException(nameof(_cfg.GradAccumSteps));
        if (_cfg.TotalSteps <= 0)
            throw new ArgumentOutOfRangeException(nameof(_cfg.TotalSteps));
        if (_cfg.CheckpointEvery <= 0)
            throw new ArgumentOutOfRangeException(nameof(_cfg.CheckpointEvery));
        if (_cfg.LogEvery <= 0)
            throw new ArgumentOutOfRangeException(nameof(_cfg.LogEvery));
        if (_cfg.WarmupSteps < 0 || _cfg.WarmupSteps > _cfg.TotalSteps)
            throw new ArgumentOutOfRangeException(nameof(_cfg.WarmupSteps));
        if (!float.IsFinite(_cfg.MaxLr) || _cfg.MaxLr <= 0f)
            throw new ArgumentOutOfRangeException(nameof(_cfg.MaxLr));
        if (!float.IsFinite(_cfg.MinLr)
            || _cfg.MinLr < 0f
            || _cfg.MinLr > _cfg.MaxLr)
        {
            throw new ArgumentOutOfRangeException(nameof(_cfg.MinLr));
        }
        if (!float.IsFinite(_cfg.GradClipNorm) || _cfg.GradClipNorm <= 0f)
            throw new ArgumentOutOfRangeException(nameof(_cfg.GradClipNorm));
        if (!float.IsFinite(_cfg.WeightDecay) || _cfg.WeightDecay < 0f)
            throw new ArgumentOutOfRangeException(nameof(_cfg.WeightDecay));
        if (string.IsNullOrWhiteSpace(_cfg.CheckpointDir))
        {
            throw new ArgumentException(
                "Checkpoint directory cannot be empty.",
                nameof(_cfg.CheckpointDir));
        }
        if (_cfg.SeqLen > model.Config.ContextLen)
        {
            throw new ArgumentOutOfRangeException(
                nameof(_cfg.SeqLen),
                "Training sequence length exceeds the model context length.");
        }
    }

    private void ValidateBatch(
        LlmModel model,
        long[,] inputs,
        long[,] targets)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(targets);
        ValidateBatchShape(inputs, nameof(inputs));
        ValidateBatchShape(targets, nameof(targets));
        ValidateTokenRange(inputs, nameof(inputs), model.Config.VocabSize);
        ValidateTokenRange(targets, nameof(targets), model.Config.VocabSize);
    }

    private void ValidateBatchShape(long[,] batch, string parameterName)
    {
        if (batch.GetLength(0) != _cfg.BatchSize
            || batch.GetLength(1) != _cfg.SeqLen)
        {
            throw new ArgumentException(
                $"Training batch must have shape [{_cfg.BatchSize}, {_cfg.SeqLen}].",
                parameterName);
        }
    }

    private static void ValidateTokenRange(
        long[,] batch,
        string parameterName,
        int vocabSize)
    {
        foreach (long token in batch)
        {
            if (token < 0 || token >= vocabSize)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    $"Training token id {token} is outside the model vocabulary.");
            }
        }
    }

    private static void ClearGradients(IEnumerable<Parameter> parameters)
    {
        foreach (var parameter in parameters)
            parameter.ZeroGrad();
    }

    private static float CrossEntropyLoss(Tensor logits, int[] targets)
    {
        int batch = logits.Shape.Dimensions[0];
        int sequence = logits.Shape.Dimensions[1];
        int vocabulary = logits.Shape.Dimensions[2];

        var flatLogits = TensorOps.Reshape(
            logits,
            batch * sequence,
            vocabulary);
        return TensorOps.CrossEntropy(flatLogits, targets);
    }

    private static void BackwardLoss(Tensor logits, int[] targets, float scale)
    {
        int batch = logits.Shape.Dimensions[0];
        int sequence = logits.Shape.Dimensions[1];
        int vocabulary = logits.Shape.Dimensions[2];

        if (logits.Grad == null)
            logits.Grad = TensorOps.ZerosLike(logits);

        // Один рабочий массив на весь вызов: он хранит показатели степени текущей
        // строки, чтобы второй раз их не вычислять. Для каждой следующей строки
        // все его ячейки перезаписываются ниже, поэтому значения прошлой строки
        // в градиент попасть не могут.
        var exponentials = new float[vocabulary];

        for (int position = 0; position < batch * sequence; position++)
        {
            float maxValue = float.NegativeInfinity;
            for (int token = 0; token < vocabulary; token++)
            {
                float value = logits.Data[position * vocabulary + token];
                if (value > maxValue)
                    maxValue = value;
            }

            float exponentialSum = 0f;
            for (int token = 0; token < vocabulary; token++)
            {
                // Показатель степени этого элемента считается ровно один раз.
                float exponential = MathF.Exp(
                    logits.Data[position * vocabulary + token] - maxValue);
                // Это же значение запоминается для петли градиента ниже; ячейка
                // текущей строки перезаписывается, остатка прошлой строки нет.
                exponentials[token] = exponential;
                // И оно же сразу идёт в знаменатель, то есть в сумму строки.
                exponentialSum += exponential;
            }

            for (int token = 0; token < vocabulary; token++)
            {
                // Доля элемента берётся из уже посчитанного значения, повторного
                // вычисления показателя степени здесь больше нет.
                float softmax = exponentials[token] / exponentialSum;
                float oneHot = token == targets[position] ? 1f : 0f;
                logits.Grad.Data[position * vocabulary + token] +=
                    (softmax - oneHot) * scale;
            }
        }

        logits.Backward(logits.Grad);
    }

    private static Tensor ToTensor(int[] ids, int batch, int sequenceLength)
    {
        var data = ids.Select(id => (float)id).ToArray();
        return new Tensor(data, new TensorShape(batch, sequenceLength));
    }

    private static int[] Flatten2D(long[,] values)
    {
        int batch = values.GetLength(0);
        int sequence = values.GetLength(1);
        var result = new int[batch * sequence];
        for (int row = 0; row < batch; row++)
        {
            for (int column = 0; column < sequence; column++)
                result[row * sequence + column] = (int)values[row, column];
        }

        return result;
    }
}
