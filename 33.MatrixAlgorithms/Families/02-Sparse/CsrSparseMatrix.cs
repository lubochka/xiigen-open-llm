// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MatrixAlgorithms;

// CsrSparseMatrix stores a sparse matrix in CSR format
// (Compressed Sparse Row — the compressed-row format).
// Three arrays describe all non-zero entries:
//   Values     — the non-zero values in row order
//   ColIndices — the column index of each value
//   RowPointers— a length-(rows+1) array where RowPointers[r] is the position in Values
//                where row r begins, and RowPointers[rows] = total number of non-zeros
// CSR is the standard format for sparse matrix-vector products because iterating over
// a row is a single contiguous slice of Values and ColIndices.
public sealed class CsrSparseMatrix
{
    // All non-zero values, in row-major order within each row (columns not necessarily sorted).
    public double[] Values      { get; }

    // Column index corresponding to each value in the Values array.
    public int[]    ColIndices  { get; }

    // RowPointers[r] is the index in Values where row r starts;
    // RowPointers[Rows] equals the total number of non-zero entries.
    public int[]    RowPointers { get; }

    // Total number of rows in the full matrix.
    public int Rows    { get; }

    // Total number of columns in the full matrix.
    public int Columns { get; }

    // Number of stored non-zero entries.
    public int NonZeroCount => Values.Length;

    // Construct directly from the three CSR arrays (used by converters and tests).
    public CsrSparseMatrix(double[] values, int[] colIndices, int[] rowPointers,
                            int rows, int columns)
    {
        Values      = values;
        ColIndices  = colIndices;
        RowPointers = rowPointers;
        Rows        = rows;
        Columns     = columns;
    }

    // Multiply this CSR matrix by a dense column vector (matrix times vector).
    // Returns a result vector of length Rows.
    // This is the most common CSR operation: iterating over the non-zeros of each row
    // in sequence is cache-friendly because Values is laid out row by row.
    public double[] MultiplyVector(double[] vector)
    {
        if (vector.Length != Columns)
            throw new ArgumentException(
                $"Vector length {vector.Length} must equal matrix columns {Columns}.");

        var result = new double[Rows];
        for (int row = 0; row < Rows; row++)
        {
            // The non-zeros for this row occupy Values[RowPointers[row]..RowPointers[row+1]-1].
            double sum = 0.0;
            for (int ptr = RowPointers[row]; ptr < RowPointers[row + 1]; ptr++)
                sum += Values[ptr] * vector[ColIndices[ptr]];
            result[row] = sum;
        }

        return result;
    }

    // Materialise this sparse matrix as a dense double array for small matrices / testing.
    public double[] ToDenseArray()
    {
        var dense = new double[Rows * Columns];
        for (int row = 0; row < Rows; row++)
            for (int ptr = RowPointers[row]; ptr < RowPointers[row + 1]; ptr++)
                dense[row * Columns + ColIndices[ptr]] += Values[ptr];
        return dense;
    }

    // Export as SparseMatrixData. The RowIndices array is rebuilt from RowPointers.
    public SparseMatrixData ToData()
    {
        // Expand RowPointers back to per-entry row indices for the portable COO-style record.
        var rowIndices = new int[Values.Length];
        for (int row = 0; row < Rows; row++)
            for (int ptr = RowPointers[row]; ptr < RowPointers[row + 1]; ptr++)
                rowIndices[ptr] = row;

        return new SparseMatrixData(rowIndices, ColIndices, Values, Rows, Columns, "CSR");
    }
}
