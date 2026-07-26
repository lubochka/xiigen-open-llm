// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace TensorCore.Layers;

// Layer that randomly zeroes neurons (regularization).
// During training each neuron is zeroed with probability p.
// During inference dropout is disabled (all values pass through).
public class Dropout : Module
{
    private readonly float _p;
    private readonly Random _rng;

    // p — zeroing probability; seed fixes reproducibility
    public Dropout(float p = 0.1f, int? seed = null)
    {
        _p   = p;
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    // training=true — apply dropout; training=false — pass through unchanged
    public Tensor Forward(Tensor input, bool training = true)
    {
        if (!training || _p == 0f)
            return input;

        // Build the mask: 1/(1-p) for surviving neurons, 0 for zeroed ones
        // Scaling by 1/(1-p) preserves the expected value of the activations
        float scale = 1.0f / (1.0f - _p);
        var maskData = new float[input.Shape.TotalElements];
        for (int i = 0; i < maskData.Length; i++)
            maskData[i] = _rng.NextDouble() >= _p ? scale : 0f;

        var mask   = new Tensor(maskData, input.Shape);
        return TensorOps.Multiply(input, mask);
    }
}
