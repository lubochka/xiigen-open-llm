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
        ValidateMatVecArguments(a, transposedB, result);
        MatVecRowCore(
            a,
            aOffset,
            aRows,
            aCols,
            transposedB,
            transposedBOffset,
            bCols,
            result,
            resultOffset,
            CancellationToken.None);
    }

    public static void MatVecRow(
        float[] a,
        int aOffset,
        int aRows,
        int aCols,
        float[] transposedB,
        int transposedBOffset,
        int bCols,
        float[] result,
        int resultOffset,
        CancellationToken cancellationToken)
    {
        ValidateMatVecArguments(a, transposedB, result);
        cancellationToken.ThrowIfCancellationRequested();
        var staged = new float[checked(aRows * bCols)];
        MatVecRowCore(
            a,
            aOffset,
            aRows,
            aCols,
            transposedB,
            transposedBOffset,
            bCols,
            staged,
            0,
            cancellationToken);

        // Publication is the single atomic boundary. There must be no
        // cancellation observation after it starts, otherwise callers could
        // observe an OperationCanceledException with a committed destination.
        cancellationToken.ThrowIfCancellationRequested();
        Array.Copy(staged, 0, result, resultOffset, staged.Length);
    }

    // TensorOps owns an unpublished destination and can therefore preserve
    // cooperative cancellation without paying for a second publication buffer.
    internal static void MatVecRowUnpublished(
        float[] a,
        int aOffset,
        int aRows,
        int aCols,
        float[] transposedB,
        int transposedBOffset,
        int bCols,
        float[] result,
        int resultOffset,
        CancellationToken cancellationToken)
    {
        ValidateMatVecArguments(a, transposedB, result);
        MatVecRowCore(
            a,
            aOffset,
            aRows,
            aCols,
            transposedB,
            transposedBOffset,
            bCols,
            result,
            resultOffset,
            cancellationToken);
    }

    private static void MatVecRowCore(
        float[] a,
        int aOffset,
        int aRows,
        int aCols,
        float[] transposedB,
        int transposedBOffset,
        int bCols,
        float[] result,
        int resultOffset,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var vectorSize = Vector<float>.Count;
        for (var row = 0; row < aRows; row++)
        {
            if (row > 0)
                cancellationToken.ThrowIfCancellationRequested();
            var aRowOffset = aOffset + (row * aCols);
            for (var col = 0; col < bCols; col++)
            {
                var bRowOffset = transposedBOffset + (col * aCols);
                var sum = 0f;
                var k = 0;

                for (; k <= aCols - vectorSize; k += vectorSize)
                {
                    if (k > 0 && (k & 4095) == 0)
                        cancellationToken.ThrowIfCancellationRequested();
                    sum += Vector.Dot(
                        new Vector<float>(a, aRowOffset + k),
                        new Vector<float>(transposedB, bRowOffset + k));
                }

                for (; k < aCols; k++)
                    sum += a[aRowOffset + k] * transposedB[bRowOffset + k];

                result[resultOffset + (row * bCols) + col] = sum;
                if (((col + 1) & 63) == 0)
                {
                    TensorKernelProgress.Report(
                        "SimdOps.MatVecRow",
                        ((long)row * bCols) + col + 1);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }
    }

    // FIX-CS: the same row-by-row dot product, but only for the cells a causal
    // position can actually see. Cells above the diagonal are left untouched at
    // zero; the existing causal mask turns them into negative infinity straight
    // afterwards, so their value was never read. The inner loop below is the
    // inner loop of MatVecRowCore character for character — same vector width,
    // same accumulation order, same scalar tail — so every visible cell comes
    // out bit for bit identical to the full product.
    internal static void CausalMatVecRowUnpublished(
        float[] a,
        int aOffset,
        int aRows,
        int aCols,
        float[] transposedB,
        int transposedBOffset,
        int bCols,
        float[] result,
        int resultOffset,
        CancellationToken cancellationToken)
    {
        ValidateMatVecArguments(a, transposedB, result);
        cancellationToken.ThrowIfCancellationRequested();
        var vectorSize = Vector<float>.Count;
        for (var row = 0; row < aRows; row++)
        {
            if (row > 0)
                cancellationToken.ThrowIfCancellationRequested();
            var aRowOffset = aOffset + (row * aCols);
            // The only difference from the full product: a position attends to
            // itself and to what came before it, never to what comes after.
            var visible = Math.Min(row + 1, bCols);
            for (var col = 0; col < visible; col++)
            {
                var bRowOffset = transposedBOffset + (col * aCols);
                var sum = 0f;
                var k = 0;

                for (; k <= aCols - vectorSize; k += vectorSize)
                {
                    if (k > 0 && (k & 4095) == 0)
                        cancellationToken.ThrowIfCancellationRequested();
                    sum += Vector.Dot(
                        new Vector<float>(a, aRowOffset + k),
                        new Vector<float>(transposedB, bRowOffset + k));
                }

                for (; k < aCols; k++)
                    sum += a[aRowOffset + k] * transposedB[bRowOffset + k];

                result[resultOffset + (row * bCols) + col] = sum;
                if (((col + 1) & 63) == 0)
                {
                    TensorKernelProgress.Report(
                        "SimdOps.CausalMatVecRow",
                        ((long)row * bCols) + col + 1);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }
    }

    private static void ValidateMatVecArguments(
        float[]? a,
        float[]? transposedB,
        float[]? result)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (transposedB is null) throw new ArgumentNullException(nameof(transposedB));
        if (result is null) throw new ArgumentNullException(nameof(result));
    }

    public static float[] Transpose2D(float[] source, int rows, int cols)
        => Transpose2D(source, 0, rows, cols);

    public static float[] Transpose2D(float[] source, int sourceOffset, int rows, int cols)
        => Transpose2D(source, sourceOffset, rows, cols, CancellationToken.None);

    public static float[] Transpose2D(
        float[] source,
        int sourceOffset,
        int rows,
        int cols,
        CancellationToken cancellationToken)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        cancellationToken.ThrowIfCancellationRequested();
        var result = new float[checked(rows * cols)];
        Transpose2DCore(source, sourceOffset, rows, cols, result, 0, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
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
        ValidateTransposeArguments(source, destination);
        Transpose2DCore(
            source,
            sourceOffset,
            rows,
            cols,
            destination,
            destinationOffset,
            CancellationToken.None);
    }

    public static void Transpose2DInPlace(
        float[] source,
        int sourceOffset,
        int rows,
        int cols,
        float[] destination,
        int destinationOffset,
        CancellationToken cancellationToken)
    {
        ValidateTransposeArguments(source, destination);
        cancellationToken.ThrowIfCancellationRequested();
        var staged = new float[checked(rows * cols)];
        Transpose2DCore(source, sourceOffset, rows, cols, staged, 0, cancellationToken);

        // See MatVecRow: cancellation is observed before, never after, the
        // single destination publication.
        cancellationToken.ThrowIfCancellationRequested();
        Array.Copy(staged, 0, destination, destinationOffset, staged.Length);
    }

    // The destination is private to TensorOps and is discarded if cancellation
    // escapes, so partial internal writes are not externally observable.
    internal static void Transpose2DInPlaceUnpublished(
        float[] source,
        int sourceOffset,
        int rows,
        int cols,
        float[] destination,
        int destinationOffset,
        CancellationToken cancellationToken)
    {
        ValidateTransposeArguments(source, destination);
        Transpose2DCore(
            source,
            sourceOffset,
            rows,
            cols,
            destination,
            destinationOffset,
            cancellationToken);
    }

    private static void Transpose2DCore(
        float[] source,
        int sourceOffset,
        int rows,
        int cols,
        float[] destination,
        int destinationOffset,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        for (var row = 0; row < rows; row++)
        {
            if (row > 0)
                cancellationToken.ThrowIfCancellationRequested();
            for (var col = 0; col < cols; col++)
            {
                if (col > 0 && (col & 4095) == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                destination[destinationOffset + (col * rows) + row] =
                    source[sourceOffset + (row * cols) + col];
            }
            if (((row + 1) & 63) == 0)
            {
                TensorKernelProgress.Report(
                    "SimdOps.Transpose2D",
                    (long)(row + 1) * cols);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    private static void ValidateTransposeArguments(float[]? source, float[]? destination)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (destination is null) throw new ArgumentNullException(nameof(destination));
    }
}
