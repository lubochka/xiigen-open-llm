// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;

namespace TransformerModel.Layers;

// RMS normalization: x / sqrt(mean(x^2) + epsilon) * gamma.
public class RmsNorm : Module
{
    private readonly Parameter _gamma;
    private readonly float _eps;

    public RmsNorm(int dim, float eps)
    {
        _eps = eps;
        _gamma = new Parameter(
            new Tensor(Initializers.Ones(dim), new TensorShape(dim)));
        RegisterParameter("gamma", _gamma);
    }

    public Tensor Forward(Tensor x)
        => Forward(x, CancellationToken.None);

    public Tensor Forward(Tensor x, CancellationToken cancellationToken)
    {
        var squared = TensorOps.Pow(x, 2f, cancellationToken);
        var mean = TensorOps.Mean(squared, axis: -1, keepDims: true, cancellationToken);
        var rms = TensorOps.RSqrt(
            TensorOps.AddScalar(mean, _eps, cancellationToken),
            cancellationToken);
        var normalized = TensorOps.MultiplyBroadcast(x, rms, cancellationToken);
        return TensorOps.MultiplyBroadcast(
            normalized,
            _gamma.Value,
            cancellationToken);
    }
}
