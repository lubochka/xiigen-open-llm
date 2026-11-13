namespace MatrixAlgorithms;

// LuLinearSolver solves the linear system A × x = b using an LU decomposition
// that has already been computed by LuDecompositionService.
// Solving via LU factors is very efficient: two triangular systems instead of one dense one.
// This is a pure-algorithm class (no BaseService) because it has no side effects.
public static class LuLinearSolver
{
    // Solve A × x = b given the LU factors and the right-hand side vector b.
    // lMatrix — the L factor (lower triangular with 1s on the diagonal), flat row-major
    // uMatrix — the U factor (upper triangular), flat row-major
    // size    — the matrix dimension (both L and U are size × size)
    // rhs     — the right-hand side vector b (length = size)
    // Returns the solution vector x such that A × x ≈ b.
    public static OperationResult<double[]> Solve(
        double[] lMatrix, double[] uMatrix, int size, double[] rhs)
    {
        // Validate that the right-hand side has exactly as many entries as the matrix has rows.
        if (rhs.Length != size)
            return OperationResult<double[]>.Fail(
                $"Right-hand side length {rhs.Length} does not match matrix size {size}.", "BAD_INPUT");

        // ── Step 1: Forward substitution — solve L × y = b ───────────────
        // L is unit lower triangular (1s on the diagonal), so we can solve row by row
        // from top to bottom, each time using already-computed y entries on the right.
        var y = new double[size];
        for (int row = 0; row < size; row++)
        {
            // Start with the right-hand side entry for this row.
            double sum = rhs[row];

            // Subtract the contribution of all y entries already computed (columns 0..row-1).
            for (int col = 0; col < row; col++)
                sum -= lMatrix[row * size + col] * y[col];

            // The diagonal of L is 1, so no division needed — y[row] = sum directly.
            y[row] = sum;
        }

        // ── Step 2: Backward substitution — solve U × x = y ─────────────
        // U is upper triangular, so we solve from the bottom row upward,
        // dividing by the diagonal entry at each step.
        var x = new double[size];
        for (int row = size - 1; row >= 0; row--)
        {
            // Start with the y entry for this row.
            double sum = y[row];

            // Subtract the contribution of all x entries already computed (columns row+1..size-1).
            for (int col = row + 1; col < size; col++)
                sum -= uMatrix[row * size + col] * x[col];

            // The diagonal entry of U divides the remaining sum to produce x[row].
            double diagonal = uMatrix[row * size + row];
            if (Math.Abs(diagonal) < 1e-14)
                return OperationResult<double[]>.Fail(
                    $"Backward substitution failed: diagonal entry at row {row} is near zero ({diagonal:e3}). " +
                    "The system is singular or nearly singular.", "SINGULAR_DIAGONAL");

            x[row] = sum / diagonal;
        }

        return OperationResult<double[]>.Ok(x);
    }
}
