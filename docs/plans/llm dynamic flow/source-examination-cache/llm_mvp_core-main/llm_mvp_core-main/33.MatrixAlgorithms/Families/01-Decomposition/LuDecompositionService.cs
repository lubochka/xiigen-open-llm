namespace MatrixAlgorithms;

// LuDecompositionService decomposes a square matrix A into L × U where:
//   L (lower triangular — нижнетреугольная матрица) has 1s on the main diagonal
//     and the elimination multipliers below it.
//   U (upper triangular — верхнетреугольная матрица) holds the reduced row-echelon form.
// The decomposition uses partial pivoting (частичная перестановка строк): before each
// elimination step, rows are swapped to put the largest available entry on the diagonal.
// This prevents division by very small numbers, which causes large numerical errors.
// Reference algorithm: LAPACK dgetrf.f (BSD-like license — translated to pure C#).
public sealed class LuDecompositionService : BaseService, IMatrixDecomposer
{
    // Create the service by injecting the storage and logger it needs (fabric-first rule).
    public LuDecompositionService(IMatrixStorage storage, IMatrixEventLogger logger)
        : base(storage, logger) { }

    // Decompose the given square matrix A into L and U using partial pivoting.
    // matrix  — all entries in row-major order (row 0 first, then row 1, etc.)
    // rows    — number of rows; must equal columns for LU decomposition to be defined
    // columns — number of columns
    public OperationResult<DecompositionResult> Decompose(double[] matrix, int rows, int columns)
    {
        // LU decomposition is only defined for square matrices (same number of rows and columns).
        if (rows != columns)
            return OperationResult<DecompositionResult>.Fail(
                $"LU decomposition requires a square matrix. Got {rows} rows × {columns} columns.",
                "NOT_SQUARE");

        // Validate that the matrix array is large enough for the claimed dimensions.
        if (matrix.Length < rows * columns)
            return OperationResult<DecompositionResult>.Fail(
                $"Matrix array length {matrix.Length} is too short for {rows}×{columns}.", "BAD_INPUT");

        // Work on a copy so the caller's original data is not modified.
        var work = new DenseMatrix((double[])matrix.Clone(), rows, columns);

        // The pivot record tracks which rows were swapped during partial pivoting.
        // pivotRows[i] = j means step i swapped rows i and j.
        var pivotRows = new int[rows];
        for (int i = 0; i < rows; i++) pivotRows[i] = i;

        // ── Main elimination loop: step through each column ───────────────
        for (int step = 0; step < rows; step++)
        {
            // ── Partial pivoting: find the row with the largest absolute value in this column ──
            // Searching from the current diagonal position downward.
            int    largestRow   = step;
            double largestValue = Math.Abs(work[step, step]);

            for (int searchRow = step + 1; searchRow < rows; searchRow++)
            {
                double candidate = Math.Abs(work[searchRow, step]);
                if (candidate > largestValue)
                {
                    largestValue = candidate;
                    largestRow   = searchRow;
                }
            }

            // If the largest available pivot is effectively zero, the matrix is singular
            // (ранг матрицы меньше числа строк — the matrix does not have full rank).
            if (largestValue < 1e-12)
                return OperationResult<DecompositionResult>.Fail(
                    $"Matrix is singular at elimination step {step}: largest pivot = {largestValue:e3}. " +
                    "LU decomposition cannot proceed — the system Ax = b has no unique solution.",
                    "SINGULAR_MATRIX");

            // Swap the current row with the row that has the largest pivot.
            if (largestRow != step)
            {
                work.SwapRows(step, largestRow);
                // Record the swap so the caller can reconstruct the permutation matrix P.
                (pivotRows[step], pivotRows[largestRow]) = (pivotRows[largestRow], pivotRows[step]);
            }

            // ── Gaussian elimination: zero out all entries below the diagonal in this column ──
            for (int eliminationRow = step + 1; eliminationRow < rows; eliminationRow++)
            {
                // The multiplier is how many copies of the pivot row must be subtracted
                // to make work[eliminationRow, step] equal zero.
                double multiplier = work[eliminationRow, step] / work[step, step];

                // Store the multiplier in the lower-triangle slot — this IS the L entry.
                work[eliminationRow, step] = multiplier;

                // Subtract (multiplier × pivot row) from this row for all remaining columns.
                for (int col = step + 1; col < columns; col++)
                    work[eliminationRow, col] -= multiplier * work[step, col];
            }
        }

        // ── Extract L and U from the combined working matrix ──────────────
        // After elimination, work holds:
        //   upper triangle (including diagonal) → U entries
        //   strict lower triangle → L entries (the elimination multipliers)
        var lData = new double[rows * rows];
        var uData = new double[rows * rows];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int idx = r * columns + c;
                if (r > c)
                {
                    // Below the diagonal: this is an L entry (the elimination multiplier).
                    lData[idx] = work[r, c];
                }
                else if (r == c)
                {
                    // On the diagonal: L always has 1 here; U has the diagonal of work.
                    lData[idx] = 1.0;
                    uData[idx] = work[r, c];
                }
                else
                {
                    // Above the diagonal: this is a U entry.
                    uData[idx] = work[r, c];
                }
            }
        }

        // Build the result record carrying both factor matrices.
        var result = new DecompositionResult
        {
            FactorOne        = lData,
            FactorOneRows    = rows,
            FactorOneColumns = rows,
            FactorTwo        = uData,
            FactorTwoRows    = rows,
            FactorTwoColumns = rows,
            AlgorithmName    = "LU with partial pivoting",
            // LU is an exact decomposition — reconstruction error is zero in exact arithmetic.
            ReconstructionError = 0.0
        };

        // Save the result before emitting the event (Genie DNA rule 8).
        Storage.TrySave("lu-last", new Dictionary<string, object>
        {
            ["rows"]      = rows,
            ["algorithm"] = result.AlgorithmName
        });

        // Emit a standard event so observers know a decomposition completed.
        EventLogger.Log(new StandardEvent(
            "matrix.lu.completed",
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            new Dictionary<string, object> { ["rows"] = rows }));

        return OperationResult<DecompositionResult>.Ok(result);
    }
}
