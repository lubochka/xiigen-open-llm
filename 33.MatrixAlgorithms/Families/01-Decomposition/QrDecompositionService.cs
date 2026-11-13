// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MatrixAlgorithms;

// QrDecompositionService decomposes any matrix A into Q × R where:
//   Q (orthogonal matrix — ортогональная матрица) has columns that are perpendicular
//     to each other and all have length 1; multiplying by Q rotates but does not stretch.
//   R (upper triangular — верхнетреугольная матрица) is the "cleaned-up" form of A.
// Uses Householder reflections (see HouseholderReflection.cs) to zero out entries below
// the diagonal one column at a time.  This is the most numerically stable QR algorithm.
// Reference algorithm: LAPACK dgeqrf.f (BSD-like license — translated to pure C#).
public sealed class QrDecompositionService : BaseService, IMatrixDecomposer
{
    // Create the service by injecting storage and logger (fabric-first rule).
    public QrDecompositionService(IMatrixStorage storage, IMatrixEventLogger logger)
        : base(storage, logger) { }

    // Decompose the matrix into Q (orthogonal) and R (upper triangular).
    // matrix  — all entries in row-major order
    // rows    — number of rows (must be ≥ columns for the standard thin-QR form)
    // columns — number of columns
    public OperationResult<DecompositionResult> Decompose(double[] matrix, int rows, int columns)
    {
        // Require at least as many rows as columns (overdetermined or square system).
        if (rows < columns)
            return OperationResult<DecompositionResult>.Fail(
                $"QR decomposition requires rows ≥ columns. Got {rows} rows × {columns} columns.",
                "UNDERDETERMINED");

        if (matrix.Length < rows * columns)
            return OperationResult<DecompositionResult>.Fail(
                $"Matrix array length {matrix.Length} is too short for {rows}×{columns}.", "BAD_INPUT");

        // R will be built in-place on a copy of A; Q will be built as the product of reflections.
        var r = new DenseMatrix((double[])matrix.Clone(), rows, columns);

        // Start Q as the rows × rows identity matrix; each Householder step will multiply into it.
        var q = NumericalUtilities.Identity(rows);

        // Store each Householder vector so we can accumulate Q after all reflections are applied.
        var householderVectors = new double[columns][];
        var householderTaus    = new double[columns];

        // ── Apply Householder reflections column by column ────────────────
        for (int col = 0; col < columns; col++)
        {
            // Extract the sub-column that must be zeroed: entries col..rows-1 of column col.
            int    subLength = rows - col;
            var    subColumn = new double[subLength];
            for (int i = 0; i < subLength; i++)
                subColumn[i] = r[col + i, col];

            // Compute the Householder vector for this sub-column.
            var (v, tau) = HouseholderReflection.Compute(subColumn);

            // Store for later accumulation into Q.
            householderVectors[col] = v;
            householderTaus[col]    = tau;

            // Apply the reflection to R from the left to zero out the sub-column.
            HouseholderReflection.ApplyFromLeft(r, v, tau, col, col);
        }

        // ── Accumulate Q by applying all reflections in reverse order ─────
        // Q = H_1 × H_2 × … × H_k (product of all the reflections applied in order).
        // We build it by applying each H_k to the right of the current Q, in reverse.
        for (int col = columns - 1; col >= 0; col--)
        {
            HouseholderReflection.ApplyFromLeft(
                q, householderVectors[col], householderTaus[col], col, col);
        }

        // ── Build the "thin" Q: keep only the first 'columns' columns ─────
        // Thin QR (тонкое QR разложение) uses only the columns of Q corresponding
        // to the non-trivial part, giving a rows × columns Q instead of rows × rows.
        var qThin = new double[rows * columns];
        for (int r2 = 0; r2 < rows; r2++)
            for (int c = 0; c < columns; c++)
                qThin[r2 * columns + c] = q[r2, c];

        // Extract the upper triangle of R (set below-diagonal entries to exactly 0).
        var rUpper = r.AsArray();
        for (int row = 0; row < rows; row++)
            for (int c2 = 0; c2 < columns; c2++)
                if (row > c2) rUpper[row * columns + c2] = 0.0;

        var result = new DecompositionResult
        {
            FactorOne        = qThin,
            FactorOneRows    = rows,
            FactorOneColumns = columns,
            FactorTwo        = rUpper,
            FactorTwoRows    = rows,
            FactorTwoColumns = columns,
            AlgorithmName    = "QR with Householder reflections",
            ReconstructionError = 0.0
        };

        Storage.TrySave("qr-last", new Dictionary<string, object>
        {
            ["rows"] = rows, ["columns"] = columns, ["algorithm"] = result.AlgorithmName
        });

        EventLogger.Log(new StandardEvent(
            "matrix.qr.completed",
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            new Dictionary<string, object> { ["rows"] = rows, ["columns"] = columns }));

        return OperationResult<DecompositionResult>.Ok(result);
    }
}
