// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TensorCore.Core;

using System.Numerics;

public static class SimdOps
{
    public static void MatVecRow(
        float[] a,
        int aOffset,
        int aRows,
        int aCols,
        float[] transposedB,
        int transposedBOffset,
        int bCols,
        float[] result,
        int resultOffset)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (transposedB is null) throw new ArgumentNullException(nameof(transposedB));
        if (result is null) throw new ArgumentNullException(nameof(result));

        var vectorSize = Vector<float>.Count;
        for (var row = 0; row < aRows; row++)
        {
            var aRowOffset = aOffset + (row * aCols);
            for (var col = 0; col < bCols; col++)
            {
                var bRowOffset = transposedBOffset + (col * aCols);
                var sum = 0f;
                var k = 0;

                for (; k <= aCols - vectorSize; k += vectorSize)
                    sum += Vector.Dot(
                        new Vector<float>(a, aRowOffset + k),
                        new Vector<float>(transposedB, bRowOffset + k));

                for (; k < aCols; k++)
                    sum += a[aRowOffset + k] * transposedB[bRowOffset + k];

                result[resultOffset + (row * bCols) + col] = sum;
            }
        }
    }

    public static float[] Transpose2D(float[] source, int rows, int cols)
        => Transpose2D(source, 0, rows, cols);

    public static float[] Transpose2D(float[] source, int sourceOffset, int rows, int cols)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        var result = new float[rows * cols];
        Transpose2DInPlace(source, sourceOffset, rows, cols, result, 0);
        return result;
    }

    public static void Transpose2DInPlace(
        float[] source,
        int sourceOffset,
        int rows,
        int cols,
        float[] destination,
        int destinationOffset)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (destination is null) throw new ArgumentNullException(nameof(destination));

        for (var row = 0; row < rows; row++)
        for (var col = 0; col < cols; col++)
            destination[destinationOffset + (col * rows) + row] =
                source[sourceOffset + (row * cols) + col];
    }
}
