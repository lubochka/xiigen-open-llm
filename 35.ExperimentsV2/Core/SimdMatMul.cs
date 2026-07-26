// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Numerics;

namespace ExperimentsV2;

// SimdMatMul implements matrix-vector multiplication using SIMD.
// Processes Vector<float>.Count (8 on AVX, 16 on AVX-512) elements at a time.
// For matrices of size >= 64×64 it gives a 4-8× speedup on modern CPUs.
public static class SimdMatMul
{
    // Multiply a matrix (rows × cols) by a vector (cols) → result (rows).
    // matrix is stored row-major: matrix[row * cols + col].
    public static float[] MatVec(float[] matrix, float[] vector, int rows, int cols)
    {
        var result = new float[rows];
        int vSize  = Vector<float>.Count;

        for (int row = 0; row < rows; row++)
        {
            int   rowOffset = row * cols;
            float sum       = 0f;
            int   col       = 0;

            for (; col <= cols - vSize; col += vSize)
            {
                var mVec = new Vector<float>(matrix, rowOffset + col);
                var vVec = new Vector<float>(vector, col);
                sum += Vector.Dot(mVec, vVec);
            }

            for (; col < cols; col++)
                sum += matrix[rowOffset + col] * vector[col];

            result[row] = sum;
        }
        return result;
    }

    // Plain multiplication without SIMD — for comparison
    public static float[] MatVecPlain(float[] matrix, float[] vector, int rows, int cols)
    {
        var result = new float[rows];
        for (int row = 0; row < rows; row++)
        {
            float sum = 0f;
            for (int col = 0; col < cols; col++)
                sum += matrix[row * cols + col] * vector[col];
            result[row] = sum;
        }
        return result;
    }
}
