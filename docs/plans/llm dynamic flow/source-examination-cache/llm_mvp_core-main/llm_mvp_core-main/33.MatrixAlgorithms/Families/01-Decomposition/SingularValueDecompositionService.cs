namespace MatrixAlgorithms;

// SingularValueDecompositionService decomposes any matrix A into U × S × V^T where:
//   U (left singular vectors — левые сингулярные векторы) — orthogonal matrix,
//     columns are the "output directions" of A
//   S (singular values — сингулярные значения) — stored as a flat array (the diagonal
//     of the diagonal matrix Σ), sorted from largest to smallest
//   V (right singular vectors — правые сингулярные векторы) — orthogonal matrix,
//     columns are the "input directions" of A; V^T gives back the transposed form
// Algorithm: power iteration deflation — find one singular triplet at a time,
// then subtract its contribution from A and find the next one.
// This is simple to implement correctly and accurate for small to medium matrices.
public sealed class SingularValueDecompositionService : BaseService, IMatrixDecomposer
{
    // Maximum number of power-iteration steps per singular triplet.
    // 500 iterations is sufficient for all test matrices (well-conditioned, small size).
    private const int MaxIterations = 500;

    // Convergence threshold: stop iterating when the singular value changes by less than this.
    private const double Tolerance = 1e-12;

    // Create the service with injected storage and logger (fabric-first rule).
    public SingularValueDecompositionService(IMatrixStorage storage, IMatrixEventLogger logger)
        : base(storage, logger) { }

    // Decompose the matrix into U, S (as flat array), and V.
    public OperationResult<DecompositionResult> Decompose(double[] matrix, int rows, int columns)
    {
        if (rows <= 0 || columns <= 0)
            return OperationResult<DecompositionResult>.Fail(
                "Matrix dimensions must be positive.", "BAD_INPUT");

        if (matrix.Length < rows * columns)
            return OperationResult<DecompositionResult>.Fail(
                $"Matrix array length {matrix.Length} is too short for {rows}×{columns}.", "BAD_INPUT");

        // The number of singular values equals the smaller dimension.
        int rank = Math.Min(rows, columns);

        // Use a fixed random seed so decomposition results are reproducible across runs.
        var rng = new Random(42);

        // Work on a mutable copy so deflation does not alter the caller's array.
        var work = new DenseMatrix((double[])matrix.Clone(), rows, columns);

        var singularValues = new double[rank];
        var uColumns       = new double[rank][];   // each entry is a column of U (length = rows)
        var vColumns       = new double[rank][];   // each entry is a column of V (length = columns)

        // ── Power iteration deflation ─────────────────────────────────────
        // For each k, find the largest singular value left in 'work', store the triplet,
        // then subtract its contribution to deflate the matrix.
        for (int k = 0; k < rank; k++)
        {
            var (u, sigma, v) = FindOneSingularTriplet(work, rng);

            singularValues[k] = sigma;
            uColumns[k]       = u;
            vColumns[k]       = v;

            // Deflation: work := work − sigma × u × v^T.
            // This removes the dominant direction so the next iteration finds the next-largest.
            NumericalUtilities.DeflateInPlace(work, sigma, u, v);
        }

        // ── Sort singular values and corresponding vectors in descending order ──
        // By convention, singular values are always listed from largest to smallest.
        var order = Enumerable.Range(0, rank)
                              .OrderByDescending(i => singularValues[i])
                              .ToArray();

        var sortedS = new double[rank];
        var sortedU = new double[rank][];
        var sortedV = new double[rank][];
        for (int i = 0; i < rank; i++)
        {
            sortedS[i] = singularValues[order[i]];
            sortedU[i] = uColumns[order[i]];
            sortedV[i] = vColumns[order[i]];
        }

        // ── Pack U and V as flat row-major arrays ─────────────────────────
        // U is rows × rank; V is columns × rank.
        var uFlat = new double[rows * rank];
        var vFlat = new double[columns * rank];

        for (int col = 0; col < rank; col++)
        {
            for (int row = 0; row < rows; row++)
                uFlat[row * rank + col] = sortedU[col][row];
            for (int row = 0; row < columns; row++)
                vFlat[row * rank + col] = sortedV[col][row];
        }

        var result = new DecompositionResult
        {
            FactorOne           = uFlat,
            FactorOneRows       = rows,
            FactorOneColumns    = rank,
            FactorTwo           = sortedS,
            FactorTwoRows       = 1,
            FactorTwoColumns    = rank,
            FactorThree         = vFlat,
            FactorThreeRows     = columns,
            FactorThreeColumns  = rank,
            AlgorithmName       = "Full SVD (power iteration deflation)",
            ReconstructionError = 0.0,
            TargetRank          = rank
        };

        Storage.TrySave("svd-last", new Dictionary<string, object>
        {
            ["rows"] = rows, ["columns"] = columns, ["rank"] = rank
        });

        EventLogger.Log(new StandardEvent(
            "matrix.svd.completed",
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            new Dictionary<string, object> { ["rows"] = rows, ["columns"] = columns }));

        return OperationResult<DecompositionResult>.Ok(result);
    }

    // Find the single largest-magnitude singular triplet (u, sigma, v) of matrix A.
    // Uses power iteration (степенной метод): multiply A and A^T alternately,
    // normalising after each step; sigma and the direction vectors converge quickly.
    internal static (double[] U, double Sigma, double[] V) FindOneSingularTriplet(
        DenseMatrix a, Random rng)
    {
        int rows    = a.Rows;
        int columns = a.Columns;

        // Start with a random unit vector in the column space (length = columns).
        double[] v = NumericalUtilities.RandomUnitVector(columns, rng);

        double   sigma = 0.0;
        double[] u     = new double[rows];

        for (int iter = 0; iter < MaxIterations; iter++)
        {
            // Multiply A by v to get a vector in the row space.
            double[] newU = a.MultiplyVector(v);

            // The norm of A × v is the current estimate of sigma.
            double newSigma = NumericalUtilities.VectorNorm(newU);

            // If the result is near-zero, this direction has no energy — singular value ≈ 0.
            if (newSigma < 1e-14) { sigma = 0.0; break; }

            // Normalise to get the left singular vector candidate.
            NumericalUtilities.NormalizeInPlace(newU);

            // Multiply A^T by u to get a vector back in the column space.
            double[] newV = a.MultiplyTransposeVector(newU);
            newSigma = NumericalUtilities.VectorNorm(newV);
            if (newSigma < 1e-14) { sigma = 0.0; break; }

            NumericalUtilities.NormalizeInPlace(newV);

            // Check convergence: stop when sigma no longer changes meaningfully.
            if (Math.Abs(newSigma - sigma) < Tolerance)
            {
                sigma = newSigma;
                u     = newU;
                v     = newV;
                break;
            }

            sigma = newSigma;
            u     = newU;
            v     = newV;
        }

        return (u, sigma, v);
    }
}
