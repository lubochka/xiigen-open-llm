namespace MatrixAlgorithms;

// QrLeastSquaresSolver uses the QR decomposition to find the best-fit solution
// to an overdetermined linear system (more equations than unknowns).
// An overdetermined system (переопределённая система) has no exact solution, so instead
// we find the x that minimises ||A × x − b||² — the sum of squared errors.
// This is the classic "method of least squares" (метод наименьших квадратов).
// Example: fitting a straight line y = m×x + c through 10 measured data points.
public static class QrLeastSquaresSolver
{
    // Find the least-squares solution to A × x ≈ b.
    // qMatrix — the Q factor (rows × columns), flat row-major
    // rMatrix — the R factor (rows × columns), flat row-major
    // rows, columns — dimensions of both Q and R
    // rhs     — the right-hand side vector b (length = rows)
    // Returns the vector x (length = columns) that minimises ||A×x − b||².
    public static OperationResult<double[]> Solve(
        double[] qMatrix, double[] rMatrix, int rows, int columns, double[] rhs)
    {
        if (rhs.Length != rows)
            return OperationResult<double[]>.Fail(
                $"Right-hand side length {rhs.Length} must equal rows {rows}.", "BAD_INPUT");

        // ── Step 1: Compute Q^T × b ──────────────────────────────────────
        // The least-squares solution satisfies R × x = Q^T × b.
        // We compute the right-hand side of this reduced system first.
        var qtb = new double[columns];
        for (int col = 0; col < columns; col++)
        {
            double dotProduct = 0.0;
            // Each entry of Q^T × b is the dot product of column col of Q with b.
            for (int row = 0; row < rows; row++)
                dotProduct += qMatrix[row * columns + col] * rhs[row];
            qtb[col] = dotProduct;
        }

        // ── Step 2: Backward substitution to solve R × x = Q^T × b ──────
        // R is upper triangular (верхнетреугольная), so we solve from the last row upward.
        var x = new double[columns];
        for (int row = columns - 1; row >= 0; row--)
        {
            double sum = qtb[row];

            // Subtract contributions of already-computed x entries (to the right).
            for (int col = row + 1; col < columns; col++)
                sum -= rMatrix[row * columns + col] * x[col];

            double diagonal = rMatrix[row * columns + row];
            if (Math.Abs(diagonal) < 1e-14)
                return OperationResult<double[]>.Fail(
                    $"R is singular at row {row}: diagonal = {diagonal:e3}. " +
                    "The columns of A are linearly dependent — no unique least-squares solution exists.",
                    "SINGULAR_R");

            x[row] = sum / diagonal;
        }

        return OperationResult<double[]>.Ok(x);
    }
}
