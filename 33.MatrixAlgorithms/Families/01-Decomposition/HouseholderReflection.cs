// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MatrixAlgorithms;

// HouseholderReflection computes the reflection that zeroes out all entries
// of a column vector below position 0 in a single matrix-vector operation.
// A Householder reflection (отражение Хаусхолдера) is defined by:
//   H = I − tau × v × v^T
// where v is the reflection vector and tau = 2 / (v^T × v).
// Applying H to a column of a matrix zeros out all entries below the first one.
// This is the numerically stable building block of QR decomposition.
// Reference: LAPACK dlarfg.f (BSD-like license — translated to pure C#).
public static class HouseholderReflection
{
    // Compute the Householder vector and scaling factor for the given column vector.
    // column — the entries to be processed; entry 0 will become ±norm(column);
    //          entries 1..length-1 will become zero after applying H.
    // Returns (v, tau) where H = I − tau × v × v^T zeros out below column[0].
    public static (double[] Vector, double Tau) Compute(double[] column)
    {
        int length = column.Length;

        // Make a working copy so the caller's array is not modified.
        var v = (double[])column.Clone();

        // Compute the 2-norm (length) of the tail entries that must be zeroed out.
        // "Tail" means entries 1 through length-1 (everything below position 0).
        double tailNormSquared = 0.0;
        for (int i = 1; i < length; i++)
            tailNormSquared += v[i] * v[i];
        double tailNorm = Math.Sqrt(tailNormSquared);

        // If the tail is already zero, no reflection is needed — return identity transform.
        if (tailNorm < 1e-15)
            return (v, 0.0);

        // Compute the full 2-norm of the column (including entry 0).
        double columnNorm = Math.Sqrt(v[0] * v[0] + tailNormSquared);

        // Choose the sign of the shift to avoid catastrophic cancellation
        // (always ADD rather than subtract two nearly equal numbers).
        double sign = v[0] >= 0.0 ? 1.0 : -1.0;

        // Shift entry 0 by the column norm to create the Householder vector.
        // After this shift, v defines a hyperplane through the origin that reflects
        // the original column to align with the first coordinate axis.
        v[0] += sign * columnNorm;

        // Compute tau = 2 / (v^T × v).
        // After the shift, v^T × v = v[0]^2 + tail^2 = (v[0] + sign*norm)^2 + tail^2.
        double vTv = v[0] * v[0] + tailNormSquared;
        double tau = vTv < 1e-30 ? 0.0 : 2.0 / vTv;

        return (v, tau);
    }

    // Apply the Householder reflection H = I − tau × v × v^T to the columns of matrix A
    // starting at column startCol (columns 0..startCol-1 are left unchanged).
    // This zeros out entries below position startRow in column startCol of A.
    // Modifies A in place to avoid allocating a new matrix.
    public static void ApplyFromLeft(DenseMatrix a, double[] v, double tau,
                                      int startRow, int startCol)
    {
        // If tau is zero, the reflection is the identity — nothing to do.
        if (Math.Abs(tau) < 1e-15) return;

        int rows    = a.Rows;
        int cols    = a.Columns;
        int vLength = v.Length; // = rows - startRow

        // Compute w = tau × A^T × v (a row vector of length cols - startCol).
        // w[j] = tau × sum_i v[i] × A[startRow + i, startCol + j]
        var w = new double[cols - startCol];
        for (int j = 0; j < cols - startCol; j++)
        {
            double sum = 0.0;
            for (int i = 0; i < vLength; i++)
                sum += v[i] * a[startRow + i, startCol + j];
            w[j] = tau * sum;
        }

        // Update A: A[startRow+i, startCol+j] -= v[i] × w[j]
        // This is the rank-1 update A := A - v × w^T.
        for (int i = 0; i < vLength; i++)
            for (int j = 0; j < cols - startCol; j++)
                a[startRow + i, startCol + j] -= v[i] * w[j];
    }

    // Apply the Householder reflection from the RIGHT: A := A × H.
    // This zeros out entries to the right of position startCol in row startRow of A.
    public static void ApplyFromRight(DenseMatrix a, double[] v, double tau,
                                       int startRow, int startCol)
    {
        if (Math.Abs(tau) < 1e-15) return;

        int rows    = a.Rows;
        int vLength = v.Length; // = cols - startCol

        // Compute w = tau × A × v (a column vector of length rows - startRow).
        var w = new double[rows - startRow];
        for (int i = 0; i < rows - startRow; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < vLength; j++)
                sum += a[startRow + i, startCol + j] * v[j];
            w[i] = tau * sum;
        }

        // Update A: A[startRow+i, startCol+j] -= w[i] × v[j]
        for (int i = 0; i < rows - startRow; i++)
            for (int j = 0; j < vLength; j++)
                a[startRow + i, startCol + j] -= w[i] * v[j];
    }
}
