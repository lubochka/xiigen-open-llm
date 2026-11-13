// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MatrixAlgorithms;

// RandomizedSvdService computes an approximate rank-K singular value decomposition
// using random projections (случайные проекции) to speed up the computation.
// For a matrix of size m × n, the standard approach costs O(m × n × min(m,n)).
// The randomized approach costs O(m × n × (K + oversample)) where oversample is small,
// which is much faster when K ≪ min(m, n).
//
// Algorithm (following the Facebook PCA randomized_svd approach, BSD-3-Clause):
//   1. Draw a random Gaussian matrix Ω of size n × (K + oversample).
//   2. Form Y = A × Ω — project A onto a random (K + oversample)-dimensional subspace.
//   3. Compute QR(Y) to get an orthonormal basis Q for the range of A.
//   4. Form B = Q^T × A — B is a small (K + oversample) × n matrix.
//   5. Compute exact SVD of B using power iteration deflation on B.
//   6. U = Q × U_B (map back to the original m-dimensional space).
//   The result approximates the true truncated SVD with very high probability.
public sealed class RandomizedSvdService : BaseService, IMatrixDecomposer
{
    // How many extra columns to draw beyond K.
    // More oversample = better accuracy at higher cost. 10 is the standard choice.
    private const int Oversample = 10;

    // Create the service with injected storage and logger (fabric-first rule).
    public RandomizedSvdService(IMatrixStorage storage, IMatrixEventLogger logger)
        : base(storage, logger) { }

    // Compute an approximate rank-K decomposition. TargetRank = min(rows,columns) gives
    // a full decomposition; any smaller value gives a low-rank approximation.
    public OperationResult<DecompositionResult> Decompose(double[] matrix, int rows, int columns)
    {
        // Default: use rank = min(rows, columns) / 2 as a sensible approximation target.
        int defaultRank = Math.Max(1, Math.Min(rows, columns) / 2);
        return DecomposeRank(matrix, rows, columns, defaultRank);
    }

    // Compute an approximate rank-K decomposition with a specific target rank.
    public OperationResult<DecompositionResult> DecomposeRank(
        double[] matrix, int rows, int columns, int targetRank)
    {
        int maxRank = Math.Min(rows, columns);

        if (targetRank <= 0 || targetRank > maxRank)
            return OperationResult<DecompositionResult>.Fail(
                $"Target rank {targetRank} is outside the valid range [1, {maxRank}].", "BAD_RANK");

        if (matrix.Length < rows * columns)
            return OperationResult<DecompositionResult>.Fail(
                $"Matrix array length {matrix.Length} is too short for {rows}×{columns}.", "BAD_INPUT");

        var rng = new Random(42);
        var a   = new DenseMatrix((double[])matrix.Clone(), rows, columns);

        // Effective sketch size is targetRank + Oversample, capped at the maximum rank.
        int sketchSize = Math.Min(targetRank + Oversample, maxRank);

        // ── Step 1: Random projection ─────────────────────────────────────
        // Draw a random Gaussian matrix Ω (columns × sketchSize) and compute Y = A × Ω.
        var omega = NumericalUtilities.RandomGaussianMatrix(columns, sketchSize, rng);
        var y     = NumericalUtilities.Multiply(a, omega); // rows × sketchSize

        // ── Step 2: Orthonormal basis for the range of A ─────────────────
        // QR decompose Y to get Q (rows × sketchSize), an orthonormal basis.
        var qrSvc    = new QrDecompositionService(Storage, EventLogger);
        var qrResult = qrSvc.Decompose(y.AsArray(), y.Rows, y.Columns);
        if (!qrResult.IsSuccess) return qrResult.MapError<DecompositionResult>();

        // Extract the Q factor from the QR decomposition result.
        var qBig = new DenseMatrix(qrResult.Value!.FactorOne,
                                   qrResult.Value.FactorOneRows,
                                   qrResult.Value.FactorOneColumns);

        // ── Step 3: Project A onto the small subspace ─────────────────────
        // B = Q^T × A  — B is sketchSize × columns, a much smaller matrix.
        var qBigT = NumericalUtilities.Transpose(qBig);
        var b     = NumericalUtilities.Multiply(qBigT, a); // sketchSize × columns

        // ── Step 4: Exact SVD of the small matrix B ───────────────────────
        var svdSvc    = new SingularValueDecompositionService(Storage, EventLogger);
        var svdResult = svdSvc.Decompose(b.AsArray(), b.Rows, b.Columns);
        if (!svdResult.IsSuccess) return svdResult.MapError<DecompositionResult>();

        var uB       = svdResult.Value!.FactorOne;   // sketchSize × min(sketchSize, columns)
        var sAll     = svdResult.Value.FactorTwo;    // singular values, descending
        var vAll     = svdResult.Value.FactorThree!; // columns × min(sketchSize, columns)
        int fullRank = svdResult.Value.TargetRank;

        // Keep only the first targetRank components (already sorted descending).
        int keepRank = Math.Min(targetRank, fullRank);

        // ── Step 5: Map U back to the full row space ──────────────────────
        // U_full = Q × U_B[:, :keepRank]  (rows × keepRank)
        var uBSmall = new DenseMatrix(sketchSize, keepRank);
        for (int r = 0; r < sketchSize; r++)
            for (int c = 0; c < keepRank; c++)
                uBSmall[r, c] = uB[r * fullRank + c];

        var uFull = NumericalUtilities.Multiply(qBig, uBSmall); // rows × keepRank

        // ── Pack results ───────────────────────────────────────────────────
        var sKeep = sAll.Take(keepRank).ToArray();
        var vFlat = new double[columns * keepRank];
        for (int r = 0; r < columns; r++)
            for (int c = 0; c < keepRank; c++)
                vFlat[r * keepRank + c] = vAll[r * fullRank + c];

        // Compute reconstruction error: ||A − U × diag(S) × V^T||_F.
        var uCols = new double[keepRank][];
        var vCols = new double[keepRank][];
        for (int k = 0; k < keepRank; k++)
        {
            uCols[k] = new double[rows];
            vCols[k] = new double[columns];
            for (int r = 0; r < rows; r++)    uCols[k][r] = uFull.AsArray()[r * keepRank + k];
            for (int r = 0; r < columns; r++) vCols[k][r] = vFlat[r * keepRank + k];
        }
        var reconstructed = TruncatedSvdService.ReconstructFromFactors(sKeep, uCols, vCols, rows, columns);
        double error = NumericalUtilities.FrobeniusNormDiff(matrix, reconstructed);

        var result = new DecompositionResult
        {
            FactorOne           = uFull.AsArray(),
            FactorOneRows       = rows,
            FactorOneColumns    = keepRank,
            FactorTwo           = sKeep,
            FactorTwoRows       = 1,
            FactorTwoColumns    = keepRank,
            FactorThree         = vFlat,
            FactorThreeRows     = columns,
            FactorThreeColumns  = keepRank,
            AlgorithmName       = $"Randomized SVD rank {keepRank} (oversample {Oversample})",
            ReconstructionError = error,
            TargetRank          = keepRank
        };

        Storage.TrySave("rsvd-last", new Dictionary<string, object>
        {
            ["rows"] = rows, ["columns"] = columns, ["keepRank"] = keepRank, ["error"] = error
        });

        EventLogger.Log(new StandardEvent(
            "matrix.randomized-svd.completed",
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            new Dictionary<string, object> { ["keepRank"] = keepRank, ["error"] = error }));

        return OperationResult<DecompositionResult>.Ok(result);
    }
}
