// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MatrixAlgorithms;

// CooSparseMatrix stores a sparse matrix in COO format (Coordinate format —
// координатный формат разреженной матрицы).
// Each non-zero entry is stored as a triple: (row index, column index, value).
// This is the simplest sparse format: easy to build and inspect, but slow to multiply.
// Typical use: building a sparse matrix entry by entry before converting to CSR or CSC.
public sealed class CooSparseMatrix
{
    // The row index of each non-zero entry.
    private readonly List<int>    _rowIndices    = [];
    // The column index of each non-zero entry.
    private readonly List<int>    _columnIndices = [];
    // The value of each non-zero entry, in the same order as the index arrays.
    private readonly List<double> _values        = [];

    // The total number of rows in the full matrix (including rows with only zeros).
    public int Rows    { get; }

    // The total number of columns in the full matrix.
    public int Columns { get; }

    // The count of non-zero entries stored so far.
    public int NonZeroCount => _values.Count;

    // Create an empty sparse matrix with the given dimensions.
    public CooSparseMatrix(int rows, int columns)
    {
        if (rows <= 0 || columns <= 0)
            throw new ArgumentOutOfRangeException(
                $"Matrix dimensions must be positive. Got {rows} rows × {columns} columns.");
        Rows    = rows;
        Columns = columns;
    }

    // Add a non-zero entry at the given position.
    // Adding the same position twice stores two entries (for accumulation patterns like
    // assembling a sparse matrix from element contributions).
    public void Add(int row, int column, double value)
    {
        // Validate that the position is inside the declared matrix dimensions.
        if (row < 0 || row >= Rows || column < 0 || column >= Columns)
            throw new ArgumentOutOfRangeException(
                $"Position ({row},{column}) is outside {Rows}×{Columns} matrix.");

        // Skip entries that are exactly zero — storing them defeats the purpose of sparse format.
        if (value == 0.0) return;

        _rowIndices.Add(row);
        _columnIndices.Add(column);
        _values.Add(value);
    }

    // Read back the value stored at the given position.
    // Returns 0 for positions that have no stored entry (they are implicitly zero).
    public double Get(int row, int column)
    {
        double sum = 0.0;
        for (int i = 0; i < _values.Count; i++)
            if (_rowIndices[i] == row && _columnIndices[i] == column)
                sum += _values[i];
        return sum;
    }

    // Export this matrix as a SparseMatrixData record for storage or conversion.
    // The Format field is set to "COO" so converters know which layout was used.
    public SparseMatrixData ToData()
        => new(
            [.. _rowIndices],
            [.. _columnIndices],
            [.. _values],
            Rows, Columns, "COO");

    // Materialise this sparse matrix as a dense double array (row-major order).
    // Use this for small matrices or for testing; large sparse matrices should never be made dense.
    public double[] ToDenseArray()
    {
        var dense = new double[Rows * Columns];
        for (int i = 0; i < _values.Count; i++)
            dense[_rowIndices[i] * Columns + _columnIndices[i]] += _values[i];
        return dense;
    }

    // Restore a CooSparseMatrix from a SparseMatrixData record.
    public static CooSparseMatrix FromData(SparseMatrixData data)
    {
        var coo = new CooSparseMatrix(data.Rows, data.Columns);
        for (int i = 0; i < data.Values.Length; i++)
            coo.Add(data.RowIndices[i], data.ColumnIndices[i], data.Values[i]);
        return coo;
    }
}
