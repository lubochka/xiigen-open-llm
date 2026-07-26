// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TensorCore.Core;

// Tensor shape — describes the size of each dimension.
// For example [8, 512, 64] = 8 samples, 512 positions, 64 numbers per position.
public sealed class TensorShape
{
    public int[] Dimensions { get; }

    // Number of dimensions (0=scalar, 1=vector, 2=matrix, ...)
    public int Rank => Dimensions.Length;

    // Total element count = product of all dimensions
    public int TotalElements { get; }

    // Strides: how far to move in the flat array to advance
    // to the next element along each dimension.
    // Matrix 3×4: strides = [4, 1]; element [i,j] = data[i*4 + j]
    public int[] Strides { get; }

    public TensorShape(params int[] dimensions)
    {
        if (dimensions.Length == 0)
            throw new ArgumentException("Tensor must have at least one dimension");
        if (dimensions.Any(d => d <= 0))
            throw new ArgumentException(
                $"All dimensions must be positive. Got: [{string.Join(", ", dimensions)}]");

        Dimensions    = dimensions.ToArray();
        TotalElements = dimensions.Aggregate(1, (acc, d) => acc * d);

        Strides    = new int[dimensions.Length];
        Strides[^1] = 1;
        for (int i = dimensions.Length - 2; i >= 0; i--)
            Strides[i] = Strides[i + 1] * dimensions[i + 1];
    }

    // Position in the flat array from multidimensional indices.
    // [2, 1] for a 3×4 matrix → 2*4 + 1 = 9
    public int FlatIndex(int[] indices)
    {
        if (indices.Length != Rank)
            throw new ArgumentException(
                $"Index count {indices.Length} does not match rank {Rank}");

        int flatIndex = 0;
        for (int i = 0; i < Rank; i++)
        {
            if (indices[i] < 0 || indices[i] >= Dimensions[i])
                throw new IndexOutOfRangeException(
                    $"Index {indices[i]} is out of bounds for dimension {Dimensions[i]} on axis {i}");
            flatIndex += indices[i] * Strides[i];
        }
        return flatIndex;
    }

    public override string ToString() => $"Shape[{string.Join(", ", Dimensions)}]";
}
