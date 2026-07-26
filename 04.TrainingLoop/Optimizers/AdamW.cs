// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;

namespace TrainingLoop.Optimizers;

// AdamW optimizer — Adaptive Moment estimation with Weight decay.
// m  = exponential moving average of the gradients (direction)
// v  = exponential moving average of the squared gradients (path noise)
// Bias-correction prevents underestimation of the early estimates.
// All coefficients come from the constructor — no magic numbers inside (DNA rule 1).
public class AdamW
{
    private readonly float _beta1;
    private readonly float _beta2;
    private readonly float _weightDecay;
    private readonly float _eps;

    private int _step;

    // State per parameter: the first and second moment (by reference)
    private readonly Dictionary<Parameter, (float[] m, float[] v)> _state = new(
        ReferenceEqualityComparer.Instance
    );

    public AdamW(float lr = 1e-3f, float beta1 = 0.9f, float beta2 = 0.95f,
                 float weightDecay = 0.1f, float eps = 1e-8f)
    {
        _beta1       = beta1;
        _beta2       = beta2;
        _weightDecay = weightDecay;
        _eps         = eps;
    }

    // Step with an explicit lr from the scheduler
    public void Step(IEnumerable<Parameter> parameters, float lr)
    {
        _step++;

        float bc1 = 1f - MathF.Pow(_beta1, _step); // first-moment correction
        float bc2 = 1f - MathF.Pow(_beta2, _step); // second-moment correction

        foreach (var param in parameters)
        {
            if (param.Grad == null) continue;

            int n = param.Value.Data.Length;

            if (!_state.TryGetValue(param, out var state))
            {
                state = (new float[n], new float[n]);
                _state[param] = state;
            }

            var (m, v) = state;
            var w = param.Value.Data;
            var g = param.Grad.Data;

            for (int i = 0; i < n; i++)
            {
                // Update the exponential moving averages
                m[i] = _beta1 * m[i] + (1f - _beta1) * g[i];
                v[i] = _beta2 * v[i] + (1f - _beta2) * g[i] * g[i];

                float mHat = m[i] / bc1;                         // corrected first moment
                float vHat = v[i] / bc2;                         // corrected second moment

                // Decoupled weight decay (this is exactly what sets AdamW apart from Adam)
                w[i] *= 1f - lr * _weightDecay;

                // Step against the gradient
                w[i] -= lr * mHat / (MathF.Sqrt(vHat) + _eps);
            }
        }
    }
}
