// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MatrixAlgorithms;

// NumericalUtilities provides the mathematical building blocks that every
// decomposition algorithm in this sub-project needs.
// All methods are pure functions — no side effects, no state, no dependencies.
public static class NumericalUtilities
{
    // ── Matrix operations ────────────────────────────────────────────────────

    // Multiply matrix A (rows_a × cols_a) by matrix B (cols_a × cols_b)
    // and return the result C (rows_a × cols_b).
    // Uses the standard triple-loop algorithm: O(rows_a × cols_a × cols_b) multiplications.
    public static DenseMatrix Multiply(DenseMatrix a, DenseMatrix b)
    {
        // The inner dimensions must match: A must have the same number of columns as B has rows.
        if (a.Columns != b.Rows)
            throw new ArgumentException(
                $"Cannot multiply: A has {a.Columns} columns but B has {b.Rows} rows.");

        var result = new DenseMatrix(a.Rows, b.Columns);

        for (int row = 0; row < a.Rows; row++)
        {
            for (int col = 0; col < b.Columns; col++)
            {
                // Each result entry is the dot product of one row of A with one column of B.
                double sum = 0.0;
                for (int k = 0; k < a.Columns; k++)
                    sum += a[row, k] * b[k, col];
                result[row, col] = sum;
            }
        }

        return result;
    }

    // Return the transpose of a matrix (swap rows and columns).
    // The transpose of an m × n matrix is an n × m matrix where entry [i,j] becomes [j,i].
    public static DenseMatrix Transpose(DenseMatrix m)
    {
        var t = new DenseMatrix(m.Columns, m.Rows);
        for (int r = 0; r < m.Rows; r++)
            for (int c = 0; c < m.Columns; c++)
                t[c, r] = m[r, c];
        return t;
    }

    // Compute the Frobenius norm (норма Фробениуса) of a matrix.
    // The Frobenius norm equals the square root of the sum of squares of all entries.
    // It measures the "total size" of a matrix and is used to quantify reconstruction error.
    public static double FrobeniusNorm(DenseMatrix matrix)
    {
        double sumOfSquares = 0.0;
        var data = matrix.AsArray();
        for (int i = 0; i < data.Length; i++)
            sumOfSquares += data[i] * data[i];

        // Take the square root of the sum of squares to get the actual norm.
        return Math.Sqrt(sumOfSquares);
    }

    // Compute the Frobenius norm of the difference between two flat arrays
    // that represent matrices of the same shape. Used to measure how close
    // a reconstructed matrix is to the original.
    public static double FrobeniusNormDiff(double[] a, double[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException(
                $"Arrays must have the same length: {a.Length} vs {b.Length}.");

        double sumOfSquares = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            double diff = a[i] - b[i];
            sumOfSquares += diff * diff;
        }
        return Math.Sqrt(sumOfSquares);
    }

    // ── Vector operations ────────────────────────────────────────────────────

    // Compute the Euclidean norm (Евклидова норма — length of a vector) of a vector.
    // The Euclidean norm equals the square root of the sum of squares of all entries.
    public static double VectorNorm(double[] vector)
    {
        double sumOfSquares = 0.0;
        for (int i = 0; i < vector.Length; i++)
            sumOfSquares += vector[i] * vector[i];
        return Math.Sqrt(sumOfSquares);
    }

    // Compute the dot product (скалярное произведение) of two vectors.
    // The dot product sums up the products of corresponding entries.
    public static double DotProduct(double[] a, double[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException(
                $"Vectors must have the same length: {a.Length} vs {b.Length}.");

        double sum = 0.0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    // Divide every entry of a vector by its Euclidean norm, producing a unit vector.
    // A unit vector has length 1 and points in the same direction as the original.
    // Modifies the vector in place; if the vector is near-zero, leaves it unchanged.
    public static void NormalizeInPlace(double[] vector)
    {
        double norm = VectorNorm(vector);
        // Guard against division by zero when the vector is effectively the zero vector.
        if (norm < 1e-14) return;
        for (int i = 0; i < vector.Length; i++)
            vector[i] /= norm;
    }

    // Compute the outer product (внешнее произведение) of two vectors.
    // The outer product of u (length m) and v (length n) is an m × n matrix
    // where entry [i,j] = u[i] * v[j].
    // Used in singular value decomposition deflation: A_new = A - sigma * outer(u, v).
    public static DenseMatrix OuterProduct(double[] u, double[] v)
    {
        var result = new DenseMatrix(u.Length, v.Length);
        for (int i = 0; i < u.Length; i++)
            for (int j = 0; j < v.Length; j++)
                result[i, j] = u[i] * v[j];
        return result;
    }

    // Subtract scale × (outer product of u and v) from matrix A, modifying A in place.
    // Used in singular value decomposition deflation to remove the contribution of
    // one singular triplet (left vector, singular value, right vector) from the matrix.
    public static void DeflateInPlace(DenseMatrix a, double scale, double[] u, double[] v)
    {
        for (int i = 0; i < a.Rows; i++)
            for (int j = 0; j < a.Columns; j++)
                a[i, j] -= scale * u[i] * v[j];
    }

    // Subtract a scaled matrix (scale × source) from a target matrix, entry by entry.
    // Used during Gaussian elimination (forward/backward passes in LU decomposition).
    public static void SubtractScaledRow(double[] target, int targetRow,
                                          double[] source, int sourceRow,
                                          double scale, int columns)
    {
        int tStart = targetRow * columns;
        int sStart = sourceRow * columns;
        for (int c = 0; c < columns; c++)
            target[tStart + c] -= scale * source[sStart + c];
    }

    // Generate a random unit vector of the given length using the provided random source.
    // Used to start power iteration in singular value decomposition with a random direction.
    public static double[] RandomUnitVector(int length, Random rng)
    {
        var v = new double[length];
        for (int i = 0; i < length; i++)
            // Draw from a Gaussian distribution (центрированное нормальное распределение) by
            // using the Box-Muller transform: combining two uniform samples into one Gaussian.
            v[i] = rng.NextDouble() - 0.5;
        NormalizeInPlace(v);
        return v;
    }

    // Generate a matrix whose entries are independent standard normal (Gaussian) random values.
    // Used by randomized singular value decomposition to project onto a random subspace.
    public static DenseMatrix RandomGaussianMatrix(int rows, int columns, Random rng)
    {
        var m = new DenseMatrix(rows, columns);
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                // Box-Muller formula produces a Gaussian sample from two uniform samples.
                m[r, c] = rng.NextDouble() - 0.5 + rng.NextDouble() - 0.5;
        return m;
    }

    // Create a square identity matrix (единичная матрица) of the given size.
    // The identity matrix has 1 on the main diagonal and 0 everywhere else.
    // Multiplying any matrix by the identity returns the same matrix unchanged.
    public static DenseMatrix Identity(int size)
    {
        var identity = new DenseMatrix(size, size);
        for (int i = 0; i < size; i++)
            identity[i, i] = 1.0;
        return identity;
    }
}
