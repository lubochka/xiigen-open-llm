// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel;
using DatasetPipeline;
using TrainingLoop.Optimizers;
using TrainingLoop.Schedulers;

namespace TrainingLoop;

// Main training loop:
//   1. Take a batch
//   2. Forward → cross-entropy loss
//   3. Backward → gradients
//   4. Clip gradients
//   5. Optimizer step
//   6. Zero the gradients
// Gradient accumulation simulates a large batch under limited memory.
// DNA rule 8: the checkpoint is saved BEFORE the step counter is incremented.
public class Trainer
{
    private readonly TrainingConfig    _cfg;
    private readonly CosineWithWarmup  _scheduler;
    private int _globalStep;

    public Trainer(TrainingConfig cfg)
    {
        _cfg       = cfg;
        _scheduler = new CosineWithWarmup(cfg.WarmupSteps, cfg.TotalSteps, cfg.MaxLr, cfg.MinLr);
    }

    public async Task TrainAsync(LlmModel model, AdamW optimizer, DataLoader loader,
        CancellationToken ct = default)
    {
        Directory.CreateDirectory(_cfg.CheckpointDir);

        while (_globalStep < _cfg.TotalSteps)
        {
            ct.ThrowIfCancellationRequested();
            float totalLoss = 0f;

            // Accumulate gradients over several mini-batches
            for (int acc = 0; acc < _cfg.GradAccumSteps; acc++)
            {
                var (inputIds2d, targetIds2d) = loader.NextBatch();

                // Convert long[,] into a Tensor [B, T]
                int[] flatInput  = Flatten2D(inputIds2d);
                int[] flatTarget = Flatten2D(targetIds2d);
                var inputTensor  = ToTensor(flatInput, _cfg.BatchSize, _cfg.SeqLen);

                var logits = model.Forward(inputTensor);  // [B, T, V]
                float loss = CrossEntropyLoss(logits, flatTarget) / _cfg.GradAccumSteps;
                totalLoss += loss;

                // Start backward from the scalar loss
                BackwardLoss(logits, flatTarget, 1f / (_cfg.GradAccumSteps * _cfg.BatchSize * _cfg.SeqLen));
            }

            // Clip gradients to prevent explosion
            TensorOps.ClipGradNorm(model.Parameters(), _cfg.GradClipNorm);

            float lr = _scheduler.GetLr(_globalStep);
            optimizer.Step(model.Parameters(), lr);
            model.ZeroGrad();

            if (_globalStep % _cfg.LogEvery == 0)
                Console.WriteLine($"step={_globalStep} loss={totalLoss:F4} lr={lr:E3}");

            // DNA rule 8: save BEFORE incrementing the counter
            if (_globalStep > 0 && _globalStep % _cfg.CheckpointEvery == 0)
                model.SaveCheckpoint(Path.Combine(_cfg.CheckpointDir, $"ckpt_{_globalStep:D6}.bin"));

            _globalStep++;
        }

        await Task.CompletedTask;
    }

    // Compute cross-entropy loss without autograd (for monitoring)
    private static float CrossEntropyLoss(Tensor logits, int[] targets)
    {
        int b = logits.Shape.Dimensions[0];
        int t = logits.Shape.Dimensions[1];
        int v = logits.Shape.Dimensions[2];

        // Flatten [B,T,V] → [B*T, V]
        var flatLogits = TensorOps.Reshape(logits, b * t, v);
        return TensorOps.CrossEntropy(flatLogits, targets);
    }

    // Manual backward through the cross-entropy gradient (dL/dlogits = softmax(logits) - one_hot)
    private static void BackwardLoss(Tensor logits, int[] targets, float scale)
    {
        int b = logits.Shape.Dimensions[0];
        int t = logits.Shape.Dimensions[1];
        int v = logits.Shape.Dimensions[2];

        if (logits.Grad == null)
            logits.Grad = TensorOps.ZerosLike(logits);

        // For each position: grad = (softmax - one_hot) * scale
        for (int pos = 0; pos < b * t; pos++)
        {
            // Compute the softmax of the row
            float maxVal = float.NegativeInfinity;
            for (int j = 0; j < v; j++)
            {
                float val = logits.Data[pos * v + j];
                if (val > maxVal) maxVal = val;
            }

            float expSum = 0f;
            for (int j = 0; j < v; j++)
                expSum += MathF.Exp(logits.Data[pos * v + j] - maxVal);

            for (int j = 0; j < v; j++)
            {
                float sm = MathF.Exp(logits.Data[pos * v + j] - maxVal) / expSum;
                float oneHot = j == targets[pos] ? 1f : 0f;
                logits.Grad.Data[pos * v + j] += (sm - oneHot) * scale;
            }
        }

        logits.Backward(logits.Grad);
    }

    // Convert a flat int[] into a Tensor [B, T]
    private static Tensor ToTensor(int[] ids, int batch, int seq)
    {
        var data = ids.Select(i => (float)i).ToArray();
        return new Tensor(data, new TensorShape(batch, seq));
    }

    // Flatten long[B,T] → int[]
    private static int[] Flatten2D(long[,] arr)
    {
        int b = arr.GetLength(0);
        int t = arr.GetLength(1);
        var result = new int[b * t];
        for (int i = 0; i < b; i++)
        for (int j = 0; j < t; j++)
            result[i * t + j] = (int)arr[i, j];
        return result;
    }
}
