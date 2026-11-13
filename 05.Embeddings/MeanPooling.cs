// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace Embeddings;

// Mean-pooling + L2-нормализация для sentence embeddings.
// Маска внимания гарантирует что padding-токены (нули) не искажают среднее.
// После нормализации cosine similarity = dot product — быстрее вычислять.
public static class MeanPooling
{
    // Pool токен-векторов через взвешенное среднее по маске.
    // tokenVectors:  [batch, seq, hidden]
    // attentionMask: [batch, seq] — 1 для реальных токенов, 0 для padding
    // Возвращает:    [batch, hidden]
    public static Tensor Pool(Tensor tokenVectors, Tensor attentionMask)
    {
        int batch  = tokenVectors.Shape.Dimensions[0];
        int seq    = tokenVectors.Shape.Dimensions[1];
        int hidden = tokenVectors.Shape.Dimensions[2];

        var result = new float[batch * hidden];

        for (int b = 0; b < batch; b++)
        {
            float maskSum = 0f;

            // Взвешенная сумма токен-векторов
            for (int t = 0; t < seq; t++)
            {
                float m = attentionMask.Data[b * seq + t];
                if (m == 0f) continue;

                maskSum += m;
                for (int d = 0; d < hidden; d++)
                    result[b * hidden + d] += tokenVectors.Data[(b * seq + t) * hidden + d] * m;
            }

            // Делим на количество реальных токенов (clamp_min чтобы не делить на 0)
            float count = Math.Max(maskSum, 1e-9f);
            for (int d = 0; d < hidden; d++)
                result[b * hidden + d] /= count;
        }

        return new Tensor(result, new TensorShape(batch, hidden));
    }

    // L2-нормализация каждой строки до единичной длины.
    // После нормализации cosine similarity(a, b) = dot(a, b).
    public static Tensor Normalize(Tensor vectors)
    {
        return TensorOps.L2Normalize(vectors, eps: 1e-9f);
    }
}
