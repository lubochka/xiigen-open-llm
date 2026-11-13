namespace MatrixAlgorithms;

// SparseMatrixMultiplier multiplies two sparse matrices stored in CSR format.
// Sparse matrix multiplication (разреженное матричное умножение) produces a result
// matrix that may itself be sparse if both inputs have few non-zeros per row.
// The algorithm loops over rows of A, and for each non-zero in that row (a_ij),
// adds a_ij × row j of B to the result row i.
public static class SparseMatrixMultiplier
{
    // Multiply CSR matrix A by CSR matrix B and return the result as a new CSR matrix.
    // A must have the same number of columns as B has rows.
    // Returns an OperationResult because the dimensions might not be compatible.
    public static OperationResult<CsrSparseMatrix> Multiply(CsrSparseMatrix a, CsrSparseMatrix b)
    {
        // The inner dimensions must match: A's columns must equal B's rows.
        if (a.Columns != b.Rows)
            return OperationResult<CsrSparseMatrix>.Fail(
                $"Cannot multiply: A has {a.Columns} columns but B has {b.Rows} rows.",
                "DIMENSION_MISMATCH");

        int resultRows    = a.Rows;
        int resultColumns = b.Columns;

        // Accumulate the result in a temporary dictionary of dictionaries.
        // result[i][j] will hold the accumulated value at position (i, j) of the product.
        // Using a dictionary avoids allocating a full dense rows × columns array.
        var resultEntries = new Dictionary<int, double>[resultRows];
        for (int i = 0; i < resultRows; i++)
            resultEntries[i] = new Dictionary<int, double>();

        // For each row i of A:
        for (int rowA = 0; rowA < a.Rows; rowA++)
        {
            // For each non-zero entry (rowA, colA, a_ij) in A:
            for (int ptrA = a.RowPointers[rowA]; ptrA < a.RowPointers[rowA + 1]; ptrA++)
            {
                int    colA  = a.ColIndices[ptrA];  // j in a_ij
                double valueA = a.Values[ptrA];     // a_ij

                // For each non-zero entry (colA, colB, b_jk) in row j of B:
                for (int ptrB = b.RowPointers[colA]; ptrB < b.RowPointers[colA + 1]; ptrB++)
                {
                    int    colB   = b.ColIndices[ptrB]; // k in b_jk
                    double valueB = b.Values[ptrB];     // b_jk

                    // result[i][k] += a_ij × b_jk
                    ref double slot = ref System.Runtime.InteropServices.CollectionsMarshal
                        .GetValueRefOrAddDefault(resultEntries[rowA], colB, out _);
                    slot += valueA * valueB;
                }
            }
        }

        // Convert the accumulated dictionary-of-dictionaries into CSR format.
        var values     = new List<double>();
        var colIndices = new List<int>();
        var rowPointers = new int[resultRows + 1];

        for (int row = 0; row < resultRows; row++)
        {
            rowPointers[row] = values.Count;
            // Sort by column index so the CSR row is in column-ascending order.
            foreach (var (col, val) in resultEntries[row].OrderBy(kv => kv.Key))
            {
                // Skip entries that accumulated to zero (due to cancellation).
                if (Math.Abs(val) < 1e-15) continue;
                values.Add(val);
                colIndices.Add(col);
            }
        }

        rowPointers[resultRows] = values.Count;

        return OperationResult<CsrSparseMatrix>.Ok(
            new CsrSparseMatrix(
                [.. values],
                [.. colIndices],
                rowPointers,
                resultRows,
                resultColumns));
    }
}
