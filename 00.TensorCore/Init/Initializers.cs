// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TensorCore.Init;

// Weight-initialization algorithms for the neural network.
// Proper initialization prevents vanishing/exploding gradients.
public static class Initializers
{
    // Fixed seed for experiment reproducibility
    private static readonly Random Rng = new(42);

    // Kaiming Uniform (He 2015) — for ReLU networks.
    // Range: [-sqrt(6/fan_in), +sqrt(6/fan_in)]
    public static float[] KaimingUniform(int rows, int cols)
    {
        var data   = new float[rows * cols];
        float bound = MathF.Sqrt(6.0f / cols); // fan_in = cols

        for (int i = 0; i < data.Length; i++)
            data[i] = (float)(Rng.NextDouble() * 2 * bound - bound);

        return data;
    }

    // Normal initialization N(0, std²) via the Box-Muller transform.
    // Used in LoRA for matrix A.
    public static float[] Normal(int rows, int cols, float std = 0.02f)
    {
        var data = new float[rows * cols];

        for (int i = 0; i < data.Length; i += 2)
        {
            // Box-Muller: two uniforms → two standard normals
            double u1    = 1.0 - Rng.NextDouble(); // avoid log(0)
            double u2    = 1.0 - Rng.NextDouble();
            double r     = Math.Sqrt(-2.0 * Math.Log(u1));
            double theta = 2.0 * Math.PI * u2;

            data[i] = (float)(r * Math.Cos(theta) * std);
            if (i + 1 < data.Length)
                data[i + 1] = (float)(r * Math.Sin(theta) * std);
        }

        return data;
    }

    // Zero initialization — used in LoRA for matrix B
    // (the adapter starts as a zero addition to the base model)
    public static float[] Zeros(int rows, int cols) => new float[rows * cols];

    // Ones initialization — used for normalization weights (γ=1, β=0)
    public static float[] Ones(int count)
    {
        var data = new float[count];
        Array.Fill(data, 1.0f);
        return data;
    }
}
