// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace VectorSearch;

public static class Distances
{
    public static float CosineSimilarity(float[] a, float[] b)
    {
        float dot = 0f, na = 0f, nb = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            na  += a[i] * a[i];
            nb  += b[i] * b[i];
        }
        float denom = MathF.Sqrt(na) * MathF.Sqrt(nb);
        return denom < 1e-9f ? 0f : dot / denom;
    }

    public static float EuclideanDistance(float[] a, float[] b)
    {
        float sum = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            float d = a[i] - b[i];
            sum += d * d;
        }
        return MathF.Sqrt(sum);
    }

    public static float InnerProduct(float[] a, float[] b)
    {
        float dot = 0f;
        for (int i = 0; i < a.Length; i++)
            dot += a[i] * b[i];
        return dot;
    }
}
