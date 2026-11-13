namespace MatrixAlgorithms;

// TruncatedSvdService computes the best rank-K approximation of a matrix
// by computing only the K largest singular values and their associated direction vectors.
// "Truncated" (усечённое) means we stop after finding K triplets instead of all min(m,n).
// The rank-K approximation A_K = sum_{i=1}^{K} sigma_i × u_i × v_i^T is the best possible
// rank-K matrix in the sense of minimising the Frobenius norm of the difference (A − A_K).
// This property is the Eckart-Young theorem (теорема Экарта-Янга).
public sealed class TruncatedSvdService : BaseService, IMatrixDecomposer
{
    // Create the service with injected storage and logger (fabric-first rule).
    public TruncatedSvdService(IMatrixStorage storage, IMatrixEventLogger logger)
        : base(storage, logger) { }

    // Compute the K-largest singular triplets of the matrix.
    // The target rank K is passed in the matrix array's length relative to rows×columns,
    // but since IMatrixDecomposer does not have a rank parameter, we overload via a helper.
    // This overload computes ALL singular values — use DecomposeRank for a specific rank.
    public OperationResult<DecompositionResult> Decompose(double[] matrix, int rows, int columns)
        => DecomposeRank(matrix, rows, columns, Math.Min(rows, columns));

    // Compute exactly targetRank singular triplets using power iteration deflation.
    // matrix     — flat row-major array of the weight matrix
    // rows       — number of rows
    // columns    — number of columns
    // targetRank — how many singular values to keep (K); must be ≤ min(rows, columns)
    public OperationResult<DecompositionResult> DecomposeRank(
        double[] matrix, int rows, int columns, int targetRank)
    {
        int maxRank = Math.Min(rows, columns);

        // Target rank must be positive and no larger than the true rank of the matrix.
        if (targetRank <= 0 || targetRank > maxRank)
            return OperationResult<DecompositionResult>.Fail(
                $"Target rank {targetRank} is outside the valid range [1, {maxRank}].",
                "BAD_RANK");

        if (matrix.Length < rows * columns)
            return OperationResult<DecompositionResult>.Fail(
                $"Matrix array length {matrix.Length} is too short for {rows}×{columns}.", "BAD_INPUT");

        var rng  = new Random(42);
        var work = new DenseMatrix((double[])matrix.Clone(), rows, columns);

        var singularValues = new double[targetRank];
        var uColumns       = new double[targetRank][];
        var vColumns       = new double[targetRank][];

        // Find exactly targetRank singular triplets with deflation.
        for (int k = 0; k < targetRank; k++)
        {
            var (u, sigma, v) = SingularValueDecompositionService.FindOneSingularTriplet(work, rng);
            singularValues[k] = sigma;
            uColumns[k]       = u;
            vColumns[k]       = v;
            NumericalUtilities.DeflateInPlace(work, sigma, u, v);
        }

        // Sort the K triplets so singular values are in descending order.
        var order = Enumerable.Range(0, targetRank)
                              .OrderByDescending(i => singularValues[i])
                              .ToArray();

        var sortedS = new double[targetRank];
        var sortedU = new double[targetRank][];
        var sortedV = new double[targetRank][];
        for (int i = 0; i < targetRank; i++)
        {
            sortedS[i] = singularValues[order[i]];
            sortedU[i] = uColumns[order[i]];
            sortedV[i] = vColumns[order[i]];
        }

        // Pack U (rows × targetRank) and V (columns × targetRank) as flat arrays.
        var uFlat = new double[rows * targetRank];
        var vFlat = new double[columns * targetRank];
        for (int col = 0; col < targetRank; col++)
        {
            for (int r = 0; r < rows; r++)
                uFlat[r * targetRank + col] = sortedU[col][r];
            for (int r = 0; r < columns; r++)
                vFlat[r * targetRank + col] = sortedV[col][r];
        }

        // Compute the reconstruction error: ||A − A_K||_F where A_K = U * diag(S) * V^T.
        var reconstructed = ReconstructFromFactors(sortedS, sortedU, sortedV, rows, columns);
        double error = NumericalUtilities.FrobeniusNormDiff(matrix, reconstructed);

        var result = new DecompositionResult
        {
            FactorOne          = uFlat,
            FactorOneRows      = rows,
            FactorOneColumns   = targetRank,
            FactorTwo          = sortedS,
            FactorTwoRows      = 1,
            FactorTwoColumns   = targetRank,
            FactorThree        = vFlat,
            FactorThreeRows    = columns,
            FactorThreeColumns = targetRank,
            AlgorithmName      = $"Truncated SVD rank {targetRank}",
            ReconstructionError = error,
            TargetRank         = targetRank
        };

        Storage.TrySave("tsvd-last", new Dictionary<string, object>
        {
            ["rows"] = rows, ["columns"] = columns, ["targetRank"] = targetRank, ["error"] = error
        });

        EventLogger.Log(new StandardEvent(
            "matrix.truncated-svd.completed",
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            new Dictionary<string, object> { ["targetRank"] = targetRank, ["error"] = error }));

        return OperationResult<DecompositionResult>.Ok(result);
    }

    // Reconstruct the approximate matrix A_K = sum_i sigma_i × u_i × v_i^T.
    // Returns a flat row-major array of the reconstructed matrix.
    internal static double[] ReconstructFromFactors(
        double[] singularValues, double[][] uColumns, double[][] vColumns, int rows, int columns)
    {
        var reconstructed = new double[rows * columns];
        int rank = singularValues.Length;

        for (int k = 0; k < rank; k++)
        {
            double sigma = singularValues[k];
            double[] u  = uColumns[k];
            double[] v  = vColumns[k];

            // Add sigma × outer(u, v) to the reconstructed matrix entry by entry.
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    reconstructed[r * columns + c] += sigma * u[r] * v[c];
        }

        return reconstructed;
    }
}
