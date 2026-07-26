// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace Inference;

// Next-token sampling strategies: greedy, temperature, top-k, top-p.
// DNA rule 1: all settings are named, no magic numbers.
public static class Sampler
{
    private static readonly Random Rng = new();

    // logits: [vocabSize] float tensor
    public static int Sample(Tensor logits, float temperature = 1.0f, int topK = 0, float topP = 1.0f)
    {
        // Temperature ≤ 0 → greedy (argmax)
        if (temperature <= 0f)
            return TensorOps.Argmax(logits);

        // Apply temperature: divide the logits by the temperature
        var scaled = TensorOps.Scale(logits, 1f / temperature);

        Tensor probs;
        if (topK > 0)
            probs = TopKFilter(scaled, topK);
        else if (topP < 1.0f)
            probs = TopPFilter(scaled, topP);
        else
            probs = TensorOps.Softmax(scaled, dim: -1);

        return MultinomialSample(probs);
    }

    // Top-K filter: keep only the k most probable tokens
    private static Tensor TopKFilter(Tensor logits, int k)
    {
        int n = logits.Shape.TotalElements;
        k = Math.Min(k, n);

        // Find the k-th largest element via a partial sort
        var sorted    = logits.Data.OrderByDescending(v => v).Take(k).ToArray();
        float threshold = sorted[^1]; // the smallest of the top-k

        var filtered = new float[n];
        for (int i = 0; i < n; i++)
            filtered[i] = logits.Data[i] >= threshold ? logits.Data[i] : float.NegativeInfinity;

        return TensorOps.Softmax(new Tensor(filtered, logits.Shape), dim: -1);
    }

    // Top-P (nucleus) filter: take tokens while the cumulative probability < p
    private static Tensor TopPFilter(Tensor logits, float p)
    {
        int n = logits.Shape.TotalElements;

        // Sort in descending order
        var (sortedVals, sortedIdx) = TensorOps.SortDescending(logits);

        // Softmax of the sorted values → cumulative sum
        var sortedProbs = TensorOps.Softmax(sortedVals, dim: -1);
        var cumProbs    = TensorOps.CumSum(sortedProbs, dim: 0);

        // Mask the tokens outside the nucleus
        // Shift the mask right to include the first token that exceeded p
        var filtered = new float[n];
        for (int i = 0; i < n; i++)
        {
            float cumPrev = i == 0 ? 0f : cumProbs.Data[i - 1];
            int   origIdx = (int)sortedIdx.Data[i];
            filtered[origIdx] = cumPrev < p ? logits.Data[origIdx] : float.NegativeInfinity;
        }

        // At least one token must remain (the first is the most probable)
        int topIdx = (int)sortedIdx.Data[0];
        if (float.IsNegativeInfinity(filtered[topIdx]))
            filtered[topIdx] = logits.Data[topIdx];

        return TensorOps.Softmax(new Tensor(filtered, logits.Shape), dim: -1);
    }

    // Multinomial sampling: pick a token proportionally to the probabilities
    private static int MultinomialSample(Tensor probs)
    {
        double r   = Rng.NextDouble();
        double acc = 0;
        for (int i = 0; i < probs.Shape.TotalElements; i++)
        {
            acc += probs.Data[i];
            if (r <= acc) return i;
        }
        return probs.Shape.TotalElements - 1;
    }
}
