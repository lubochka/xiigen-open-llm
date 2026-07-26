// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using DPO.Models;
using TrainingLoop.Optimizers;
using TransformerModel;

namespace DPO;

// Direct Preference Optimisation trainer.
// Loss = -E[log σ(β · (log π(y_w|x) - log π(y_l|x) - log π_ref(y_w|x) + log π_ref(y_l|x)))]
// DNA rule 8: metrics are saved BEFORE the step counter is incremented.
public class DpoTrainer
{
    private const float Beta = 0.1f;  // KL-penalty coefficient
    public const string PolicyCheckpointFilePrefix = "dpo_policy_step_";
    public const string PolicyCheckpointFileExtension = ".bin";

    private readonly LlmModel _policy;
    private readonly LlmModel _reference;
    private readonly AdamW    _optimizer;
    private readonly string   _checkpointDir;
    private readonly float    _learningRate;

    public DpoTrainer(LlmModel policy, LlmModel reference, string checkpointDir, float learningRate = 1e-5f)
    {
        _policy        = policy;
        _reference     = reference;
        _optimizer     = new AdamW(weightDecay: 0.01f);
        _checkpointDir = checkpointDir;
        _learningRate  = learningRate;
        Directory.CreateDirectory(checkpointDir);
    }

    public DpoMetrics Step(DpoBatch batch, int step)
    {
        // Reference log-probs: the model is frozen, no gradient needed
        float refChosenLogP   = LogProbs(_reference, batch.ChosenIds,   batch.ChosenMask);
        float refRejectedLogP = LogProbs(_reference, batch.RejectedIds, batch.RejectedMask);

        // Policy log-probs: the trainable model, backward is needed
        float polChosenLogP   = LogProbs(_policy, batch.ChosenIds,   batch.ChosenMask);
        float polRejectedLogP = LogProbs(_policy, batch.RejectedIds, batch.RejectedMask);

        // DPO margin: how much the policy prefers chosen over rejected relative to reference
        float chosenReward   = Beta * (polChosenLogP   - refChosenLogP);
        float rejectedReward = Beta * (polRejectedLogP - refRejectedLogP);
        float margin         = chosenReward - rejectedReward;

        // DPO loss: -log σ(margin)
        float loss = -MathF.Log(Sigmoid(margin) + 1e-8f);

        // Manual backward: d(-log σ(m))/dm = -(1 - σ(m)) = σ(m) - 1
        float gradMargin = Sigmoid(margin) - 1f;
        BackwardLogProbs(_policy, batch.ChosenIds, batch.ChosenMask,  gradMargin * Beta);
        BackwardLogProbs(_policy, batch.RejectedIds, batch.RejectedMask, -gradMargin * Beta);

        _optimizer.Step(_policy.Parameters(), lr: _learningRate);
        _policy.ZeroGrad();

        var metrics = new DpoMetrics
        {
            Step           = step,
            Loss           = loss,
            ChosenReward   = chosenReward,
            RejectedReward = rejectedReward,
            RewardMargin   = margin,
            Accuracy       = margin > 0 ? 1f : 0f
        };

        // DNA rule 8: save the metrics before returning (we do not increment Step here)
        SaveMetrics(metrics);
        SavePolicyCheckpoint(metrics);
        return metrics;
    }

    public DpoMetrics StepMany(IReadOnlyList<DpoBatch> batches, int step)
    {
        if (batches.Count == 0)
            throw new ArgumentException("At least one DPO batch is required.", nameof(batches));
        if (batches.Count == 1)
            return Step(batches[0], step);

        float lossSum = 0f;
        float chosenRewardSum = 0f;
        float rejectedRewardSum = 0f;
        float marginSum = 0f;
        float accuracySum = 0f;
        var gradientScale = 1f / batches.Count;

        foreach (var batch in batches)
        {
            float refChosenLogP   = LogProbs(_reference, batch.ChosenIds,   batch.ChosenMask);
            float refRejectedLogP = LogProbs(_reference, batch.RejectedIds, batch.RejectedMask);
            float polChosenLogP   = LogProbs(_policy, batch.ChosenIds,   batch.ChosenMask);
            float polRejectedLogP = LogProbs(_policy, batch.RejectedIds, batch.RejectedMask);

            float chosenReward   = Beta * (polChosenLogP   - refChosenLogP);
            float rejectedReward = Beta * (polRejectedLogP - refRejectedLogP);
            float margin         = chosenReward - rejectedReward;
            float loss           = -MathF.Log(Sigmoid(margin) + 1e-8f);
            float gradMargin     = (Sigmoid(margin) - 1f) * gradientScale;

            BackwardLogProbs(_policy, batch.ChosenIds, batch.ChosenMask, gradMargin * Beta);
            BackwardLogProbs(_policy, batch.RejectedIds, batch.RejectedMask, -gradMargin * Beta);

            lossSum += loss;
            chosenRewardSum += chosenReward;
            rejectedRewardSum += rejectedReward;
            marginSum += margin;
            accuracySum += margin > 0 ? 1f : 0f;
        }

        _optimizer.Step(_policy.Parameters(), lr: _learningRate);
        _policy.ZeroGrad();

        var count = batches.Count;
        var metrics = new DpoMetrics
        {
            Step = step,
            Loss = lossSum / count,
            ChosenReward = chosenRewardSum / count,
            RejectedReward = rejectedRewardSum / count,
            RewardMargin = marginSum / count,
            Accuracy = accuracySum / count
        };

        SaveMetrics(metrics);
        SavePolicyCheckpoint(metrics);
        return metrics;
    }

    public DpoMetrics EvaluateMany(IReadOnlyList<DpoBatch> batches, int step)
    {
        if (batches.Count == 0)
            throw new ArgumentException("At least one DPO batch is required.", nameof(batches));

        float lossSum = 0f;
        float chosenRewardSum = 0f;
        float rejectedRewardSum = 0f;
        float marginSum = 0f;
        float accuracySum = 0f;

        foreach (var batch in batches)
        {
            float refChosenLogP   = LogProbs(_reference, batch.ChosenIds,   batch.ChosenMask);
            float refRejectedLogP = LogProbs(_reference, batch.RejectedIds, batch.RejectedMask);
            float polChosenLogP   = LogProbs(_policy, batch.ChosenIds,   batch.ChosenMask);
            float polRejectedLogP = LogProbs(_policy, batch.RejectedIds, batch.RejectedMask);

            float chosenReward   = Beta * (polChosenLogP   - refChosenLogP);
            float rejectedReward = Beta * (polRejectedLogP - refRejectedLogP);
            float margin         = chosenReward - rejectedReward;
            float loss           = -MathF.Log(Sigmoid(margin) + 1e-8f);

            lossSum += loss;
            chosenRewardSum += chosenReward;
            rejectedRewardSum += rejectedReward;
            marginSum += margin;
            accuracySum += margin > 0 ? 1f : 0f;
        }

        var count = batches.Count;
        return new DpoMetrics
        {
            Step = step,
            Loss = lossSum / count,
            ChosenReward = chosenRewardSum / count,
            RejectedReward = rejectedRewardSum / count,
            RewardMargin = marginSum / count,
            Accuracy = accuracySum / count
        };
    }

    public DpoMetrics StepVector(IReadOnlyList<DpoVectorBatch> batches, int step)
    {
        if (batches.Count == 0)
            throw new ArgumentException("At least one vector DPO batch is required.", nameof(batches));

        float lossSum = 0f;
        float chosenRewardSum = 0f;
        float rejectedRewardSum = 0f;
        float marginSum = 0f;
        float accuracySum = 0f;
        var totalWeight = Math.Max(1e-6f, batches.Sum(batch => Math.Max(0.01f, batch.Weight)));

        foreach (var vectorBatch in batches)
        {
            var batch = vectorBatch.Batch;
            var vectorWeight = Math.Max(0.01f, vectorBatch.Weight) / totalWeight;
            var vectorSignal = VectorSignal(vectorBatch.TeachingVector);
            var scaledWeight = vectorWeight * vectorSignal;

            float refChosenLogP   = LogProbs(_reference, batch.ChosenIds,   batch.ChosenMask);
            float refRejectedLogP = LogProbs(_reference, batch.RejectedIds, batch.RejectedMask);
            float polChosenLogP   = LogProbs(_policy, batch.ChosenIds,   batch.ChosenMask);
            float polRejectedLogP = LogProbs(_policy, batch.RejectedIds, batch.RejectedMask);

            float chosenReward   = Beta * (polChosenLogP   - refChosenLogP);
            float rejectedReward = Beta * (polRejectedLogP - refRejectedLogP);
            float margin         = chosenReward - rejectedReward;
            float loss           = -MathF.Log(Sigmoid(margin) + 1e-8f);
            float gradMargin     = (Sigmoid(margin) - 1f) * scaledWeight;

            BackwardLogProbs(_policy, batch.ChosenIds, batch.ChosenMask, gradMargin * Beta);
            BackwardLogProbs(_policy, batch.RejectedIds, batch.RejectedMask, -gradMargin * Beta);

            lossSum += loss * vectorWeight;
            chosenRewardSum += chosenReward * vectorWeight;
            rejectedRewardSum += rejectedReward * vectorWeight;
            marginSum += margin * vectorWeight;
            accuracySum += (margin > 0 ? 1f : 0f) * vectorWeight;
        }

        _optimizer.Step(_policy.Parameters(), lr: _learningRate);
        _policy.ZeroGrad();

        var metrics = new DpoMetrics
        {
            Step = step,
            Loss = lossSum,
            ChosenReward = chosenRewardSum,
            RejectedReward = rejectedRewardSum,
            RewardMargin = marginSum,
            Accuracy = accuracySum
        };

        SaveMetrics(metrics);
        SavePolicyCheckpoint(metrics);
        return metrics;
    }

    public static string BuildPolicyCheckpointFileName(int step)
        => $"{PolicyCheckpointFilePrefix}{step:D6}{PolicyCheckpointFileExtension}";

    // The model average log-prob over the non-padded positions
    private static float LogProbs(LlmModel model, Tensor ids, Tensor mask)
    {
        var logits = model.Forward(ids);    // [1, T, vocab]
        int t      = logits.Shape.Dimensions[1];
        int v      = logits.Shape.Dimensions[2];

        float sum  = 0f;
        float cnt  = 0f;

        // Shift by 1: predict ids[t+1] from the context up to t
        for (int i = 0; i < t - 1; i++)
        {
            float maskVal = mask.Data[i + 1];
            if (maskVal == 0f) continue;

            int targetId = (int)ids.Data[i + 1];

            // LogSoftmax of row i
            float maxVal = float.NegativeInfinity;
            for (int j = 0; j < v; j++)
                if (logits.Data[i * v + j] > maxVal)
                    maxVal = logits.Data[i * v + j];

            float logSumExp = 0f;
            for (int j = 0; j < v; j++)
                logSumExp += MathF.Exp(logits.Data[i * v + j] - maxVal);
            logSumExp = MathF.Log(logSumExp);

            float logP = logits.Data[i * v + targetId] - maxVal - logSumExp;
            sum += logP * maskVal;
            cnt += maskVal;
        }

        return cnt > 0f ? sum / cnt : 0f;
    }

    // Simplified backward for the log-probs (accumulate the gradient into logits)
    private static void BackwardLogProbs(LlmModel model, Tensor ids, Tensor mask, float upstream)
    {
        var logits = model.Forward(ids);
        int t      = logits.Shape.Dimensions[1];
        int v      = logits.Shape.Dimensions[2];

        if (logits.Grad == null)
            logits.Grad = TensorOps.ZerosLike(logits);

        float cnt = 0f;
        for (int i = 0; i < t - 1; i++)
            if (mask.Data[i + 1] > 0f) cnt++;
        if (cnt == 0f) return;

        float scale = upstream / cnt;

        for (int i = 0; i < t - 1; i++)
        {
            if (mask.Data[i + 1] == 0f) continue;

            int targetId = (int)ids.Data[i + 1];

            // Softmax of row i
            float maxVal = float.NegativeInfinity;
            for (int j = 0; j < v; j++)
                if (logits.Data[i * v + j] > maxVal) maxVal = logits.Data[i * v + j];

            float expSum = 0f;
            var expVals  = new float[v];
            for (int j = 0; j < v; j++)
            {
                expVals[j] = MathF.Exp(logits.Data[i * v + j] - maxVal);
                expSum    += expVals[j];
            }

            // The local autograd/optimizer path applies the supplied tensor gradient directly
            // through a descent step, so this uses the NLL-style logits gradient orientation.
            for (int j = 0; j < v; j++)
            {
                float sm = expVals[j] / expSum;
                float oh = j == targetId ? 1f : 0f;
                logits.Grad.Data[i * v + j] += (sm - oh) * scale;
            }
        }

        logits.Backward(logits.Grad);
    }

    private static float Sigmoid(float x) => 1f / (1f + MathF.Exp(-x));

    private static float VectorSignal(IReadOnlyList<float> vector)
    {
        if (vector.Count == 0)
            return 1f;

        var activeSlots = vector.Count(value => MathF.Abs(value) > 1e-6f);
        return Math.Clamp(1f + (activeSlots / 60f), 1f, 2f);
    }

    private void SaveMetrics(DpoMetrics m)
    {
        string path = Path.Combine(_checkpointDir, $"dpo_step_{m.Step:D6}.json");
        string tmp  = path + ".tmp";
        File.WriteAllText(tmp, System.Text.Json.JsonSerializer.Serialize(m));
        File.Move(tmp, path, overwrite: true);
    }

    private void SavePolicyCheckpoint(DpoMetrics m)
    {
        string path = Path.Combine(_checkpointDir, BuildPolicyCheckpointFileName(m.Step));
        _policy.SaveCheckpoint(path);
    }
}

public sealed record DpoVectorBatch(DpoBatch Batch, float[] TeachingVector, float Weight = 1f);
