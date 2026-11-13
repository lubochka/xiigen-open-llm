// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;

namespace TransformerModel.Layers;

// Rotary position embedding for tensors shaped [batch, heads, sequence, dimension].
public class RotaryEmbedding : Module
{
    private readonly int _dim;
    private readonly float _theta;

    public RotaryEmbedding(int dim, float theta)
    {
        _dim = dim;
        _theta = theta;
    }

    public (Tensor cos, Tensor sin) GetFrequencies(int seqLen)
        => GetFrequencies(seqLen, CancellationToken.None);

    public (Tensor cos, Tensor sin) GetFrequencies(
        int seqLen,
        CancellationToken cancellationToken)
        => GetFrequencies(seqLen, positionOffset: 0, cancellationToken);

    // Generates RoPE frequencies for absolute positions [positionOffset, positionOffset + seqLen).
    public (Tensor cos, Tensor sin) GetFrequencies(
        int seqLen,
        int positionOffset,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (seqLen <= 0)
            throw new ArgumentOutOfRangeException(nameof(seqLen), "Sequence length must be positive.");
        if (positionOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(positionOffset), "Position offset must be non-negative.");
        if ((long)positionOffset + seqLen - 1 > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(positionOffset),
                "The final absolute position must fit in Int32.");
        }

        int half = _dim / 2;
        var frequencyData = new float[half];
        for (int index = 0; index < half; index++)
        {
            if ((index & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            frequencyData[index] = 1f / MathF.Pow(_theta, 2f * index / _dim);
        }

        var frequencies = new Tensor(frequencyData, new TensorShape(half));
        var positionData = new float[seqLen];
        for (int index = 0; index < seqLen; index++)
        {
            if ((index & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            positionData[index] = checked(positionOffset + index);
        }
        var positions = new Tensor(positionData, new TensorShape(seqLen));

        var outer = TensorOps.OuterProduct(positions, frequencies, cancellationToken);
        return (
            TensorOps.Cos(outer, cancellationToken),
            TensorOps.Sin(outer, cancellationToken));
    }

    public static Tensor Apply(Tensor x, Tensor cos, Tensor sin)
        => Apply(x, cos, sin, CancellationToken.None);

    public static Tensor Apply(
        Tensor x,
        Tensor cos,
        Tensor sin,
        CancellationToken cancellationToken)
        => TensorOps.ApplyRotaryEmbedding(x, cos, sin, cancellationToken);
}
