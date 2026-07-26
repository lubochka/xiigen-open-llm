// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace Embeddings;

// Mean-pooling + L2 normalization for sentence embeddings.
// The attention mask ensures that padding tokens (zeros) do not skew the mean.
// After normalization, cosine similarity = dot product — faster to compute.
public static class MeanPooling
{
    // Pool the token vectors via a mask-weighted mean.
    // tokenVectors:  [batch, seq, hidden]
    // attentionMask: [batch, seq] — 1 for real tokens, 0 for padding
    // Returns:        [batch, hidden]
    public static Tensor Pool(Tensor tokenVectors, Tensor attentionMask)
    {
        int batch  = tokenVectors.Shape.Dimensions[0];
        int seq    = tokenVectors.Shape.Dimensions[1];
        int hidden = tokenVectors.Shape.Dimensions[2];

        var result = new float[batch * hidden];

        for (int b = 0; b < batch; b++)
        {
            float maskSum = 0f;

            // Weighted sum of the token vectors
            for (int t = 0; t < seq; t++)
            {
                float m = attentionMask.Data[b * seq + t];
                if (m == 0f) continue;

                maskSum += m;
                for (int d = 0; d < hidden; d++)
                    result[b * hidden + d] += tokenVectors.Data[(b * seq + t) * hidden + d] * m;
            }

            // Divide by the number of real tokens (clamp_min to avoid dividing by 0)
            float count = Math.Max(maskSum, 1e-9f);
            for (int d = 0; d < hidden; d++)
                result[b * hidden + d] /= count;
        }

        return new Tensor(result, new TensorShape(batch, hidden));
    }

    // L2-normalize each row to unit length.
    // After normalization, cosine similarity(a, b) = dot(a, b).
    public static Tensor Normalize(Tensor vectors)
    {
        return TensorOps.L2Normalize(vectors, eps: 1e-9f);
    }
}
