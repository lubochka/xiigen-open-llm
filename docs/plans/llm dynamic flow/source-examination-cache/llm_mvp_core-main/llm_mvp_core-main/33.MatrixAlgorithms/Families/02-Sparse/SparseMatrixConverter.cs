namespace MatrixAlgorithms;

// SparseMatrixConverter transforms sparse matrices between the three storage formats:
// COO (Coordinate), CSR (Compressed Sparse Row), and CSC (Compressed Sparse Column).
// All conversions are pure functions that return new objects without modifying inputs.
public static class SparseMatrixConverter
{
    // Convert a COO (Coordinate format) sparse matrix to CSR (Compressed Sparse Row).
    // COO is easy to build; CSR is efficient for row-by-row operations and matrix-vector multiply.
    public static CsrSparseMatrix CooToCsr(CooSparseMatrix coo)
    {
        var data = coo.ToData();
        return DataToCsr(data);
    }

    // Convert a COO sparse matrix to CSC (Compressed Sparse Column).
    public static CscSparseMatrix CooToCsc(CooSparseMatrix coo)
    {
        var data = coo.ToData();
        return DataToCsc(data);
    }

    // Convert a CSR matrix to CSC by transposing the index structure.
    public static CscSparseMatrix CsrToCsc(CsrSparseMatrix csr)
    {
        var data = csr.ToData();
        return DataToCsc(data);
    }

    // Convert a CSC matrix to CSR by transposing the index structure.
    public static CsrSparseMatrix CscToCsr(CscSparseMatrix csc)
    {
        var data = csc.ToData();
        return DataToCsr(data);
    }

    // Build a CSR matrix from a SparseMatrixData record (which uses COO-style row/col arrays).
    // This groups entries by row and builds the RowPointers array.
    public static CsrSparseMatrix DataToCsr(SparseMatrixData data)
    {
        int nnz  = data.Values.Length;
        int rows = data.Rows;

        // Count how many non-zeros each row has.
        var rowCount = new int[rows];
        for (int i = 0; i < nnz; i++)
            rowCount[data.RowIndices[i]]++;

        // Build RowPointers as the prefix sum of rowCount.
        // RowPointers[r] = sum of non-zeros in rows 0..r-1.
        var rowPointers = new int[rows + 1];
        for (int r = 0; r < rows; r++)
            rowPointers[r + 1] = rowPointers[r] + rowCount[r];

        // Fill Values and ColIndices in row order.
        var values     = new double[nnz];
        var colIndices = new int[nnz];
        var fillPos    = (int[])rowPointers.Clone(); // next available position per row

        for (int i = 0; i < nnz; i++)
        {
            int row     = data.RowIndices[i];
            int pos     = fillPos[row]++;
            values[pos]     = data.Values[i];
            colIndices[pos] = data.ColumnIndices[i];
        }

        return new CsrSparseMatrix(values, colIndices, rowPointers, rows, data.Columns);
    }

    // Build a CSC matrix from a SparseMatrixData record.
    private static CscSparseMatrix DataToCsc(SparseMatrixData data)
    {
        int nnz     = data.Values.Length;
        int columns = data.Columns;

        // Count how many non-zeros each column has.
        var colCount = new int[columns];
        for (int i = 0; i < nnz; i++)
            colCount[data.ColumnIndices[i]]++;

        // Build ColPointers as the prefix sum of colCount.
        var colPointers = new int[columns + 1];
        for (int c = 0; c < columns; c++)
            colPointers[c + 1] = colPointers[c] + colCount[c];

        // Fill Values and RowIndices in column order.
        var values     = new double[nnz];
        var rowIndices = new int[nnz];
        var fillPos    = (int[])colPointers.Clone();

        for (int i = 0; i < nnz; i++)
        {
            int col     = data.ColumnIndices[i];
            int pos     = fillPos[col]++;
            values[pos]     = data.Values[i];
            rowIndices[pos] = data.RowIndices[i];
        }

        return new CscSparseMatrix(values, rowIndices, colPointers, data.Rows, columns);
    }
}
