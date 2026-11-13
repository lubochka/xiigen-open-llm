// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MatrixAlgorithms;

// CscSparseMatrix stores a sparse matrix in CSC format
// (Compressed Sparse Column — формат сжатых столбцов).
// It is the column-oriented counterpart of CSR (Compressed Sparse Row).
// Three arrays describe all non-zero entries:
//   Values     — the non-zero values in column order
//   RowIndices — the row index of each value
//   ColPointers— a length-(columns+1) array where ColPointers[c] is the position in Values
//                where column c begins, and ColPointers[Columns] = total non-zeros
// CSC is efficient for column-oriented operations: extracting a column, computing A × B^T,
// or solving triangular systems with column-by-column pivoting.
public sealed class CscSparseMatrix
{
    // All non-zero values, in column-major order within each column.
    public double[] Values      { get; }

    // Row index corresponding to each value in the Values array.
    public int[]    RowIndices  { get; }

    // ColPointers[c] is the index in Values where column c starts;
    // ColPointers[Columns] equals the total number of non-zero entries.
    public int[]    ColPointers { get; }

    // Total number of rows in the full matrix.
    public int Rows    { get; }

    // Total number of columns in the full matrix.
    public int Columns { get; }

    // Number of stored non-zero entries.
    public int NonZeroCount => Values.Length;

    // Construct directly from the three CSC arrays.
    public CscSparseMatrix(double[] values, int[] rowIndices, int[] colPointers,
                            int rows, int columns)
    {
        Values      = values;
        RowIndices  = rowIndices;
        ColPointers = colPointers;
        Rows        = rows;
        Columns     = columns;
    }

    // Multiply the transpose of this matrix (A^T) by a dense column vector.
    // A^T × v is naturally efficient in CSC format because iterating over a column of A
    // corresponds to iterating over a row of A^T.
    public double[] MultiplyTransposeVector(double[] vector)
    {
        if (vector.Length != Rows)
            throw new ArgumentException(
                $"Vector length {vector.Length} must equal matrix rows {Rows}.");

        var result = new double[Columns];
        for (int col = 0; col < Columns; col++)
        {
            double sum = 0.0;
            for (int ptr = ColPointers[col]; ptr < ColPointers[col + 1]; ptr++)
                sum += Values[ptr] * vector[RowIndices[ptr]];
            result[col] = sum;
        }

        return result;
    }

    // Materialise this sparse matrix as a dense double array for small matrices / testing.
    public double[] ToDenseArray()
    {
        var dense = new double[Rows * Columns];
        for (int col = 0; col < Columns; col++)
            for (int ptr = ColPointers[col]; ptr < ColPointers[col + 1]; ptr++)
                dense[RowIndices[ptr] * Columns + col] += Values[ptr];
        return dense;
    }

    // Export as SparseMatrixData. ColIndices is rebuilt from ColPointers.
    public SparseMatrixData ToData()
    {
        var colIndices = new int[Values.Length];
        for (int col = 0; col < Columns; col++)
            for (int ptr = ColPointers[col]; ptr < ColPointers[col + 1]; ptr++)
                colIndices[ptr] = col;

        return new SparseMatrixData(RowIndices, colIndices, Values, Rows, Columns, "CSC");
    }
}
