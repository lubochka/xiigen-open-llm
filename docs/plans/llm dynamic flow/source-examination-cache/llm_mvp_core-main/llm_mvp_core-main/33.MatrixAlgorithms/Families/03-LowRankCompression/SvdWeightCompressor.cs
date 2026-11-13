namespace MatrixAlgorithms;

// SvdWeightCompressor compresses a neural network weight matrix by keeping only
// the K most important singular directions (the K largest singular values).
// The compressed form stores two smaller matrices: LeftFactor (rows × K) and
// RightFactor (K × columns). Their product approximates the original.
// Storage used: rows×K + K + K×columns  (vs rows×columns uncompressed).
// This implements the IWeightCompressor interface (fabric-first rule).
public sealed class SvdWeightCompressor : BaseService, IWeightCompressor
{
    // The truncated SVD service that does the actual decomposition work.
    private readonly TruncatedSvdService _tsvdService;

    // Create the compressor with injected storage and logger (fabric-first rule).
    public SvdWeightCompressor(IMatrixStorage storage, IMatrixEventLogger logger)
        : base(storage, logger)
    {
        _tsvdService = new TruncatedSvdService(storage, logger);
    }

    // Compress the weight matrix to a rank-targetRank approximation using truncated SVD.
    public OperationResult<CompressedWeightMatrix> Compress(
        double[] weightMatrix, int rows, int columns, int targetRank)
    {
        // Delegate to the truncated SVD service to find the K dominant singular triplets.
        var svdResult = _tsvdService.DecomposeRank(weightMatrix, rows, columns, targetRank);
        if (!svdResult.IsSuccess)
            return OperationResult<CompressedWeightMatrix>.Fail(
                $"Truncated SVD failed: {svdResult.ErrorMessage}", svdResult.ErrorCode);

        var d = svdResult.Value!;

        // FactorOne = U  (rows × targetRank), FactorTwo = S (targetRank values),
        // FactorThree = V (columns × targetRank).
        // We store U, S, and V separately so Decompress can reconstruct A ≈ U × diag(S) × V^T.
        var compressed = new CompressedWeightMatrix(
            d.FactorOne,       // U: rows × targetRank
            d.FactorOneRows,
            d.FactorOneColumns,
            d.FactorTwo,       // S: targetRank singular values
            d.FactorThree!,    // V: columns × targetRank
            d.FactorThreeRows,
            d.FactorThreeColumns,
            "TruncatedSVD",
            d.ReconstructionError);

        Storage.TrySave("svd-compress-last", new Dictionary<string, object>
        {
            ["rows"] = rows, ["columns"] = columns,
            ["targetRank"] = targetRank, ["error"] = d.ReconstructionError
        });

        EventLogger.Log(new StandardEvent(
            "matrix.weight-compress.completed",
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                ["targetRank"] = targetRank, ["error"] = d.ReconstructionError
            }));

        return OperationResult<CompressedWeightMatrix>.Ok(compressed);
    }

    // Reconstruct the approximate full weight matrix from the compressed form.
    // result[i,j] = sum_k  LeftFactor[i,k] × SingularValues[k] × RightFactor[j,k]
    // (which equals U × diag(S) × V^T)
    public OperationResult<double[]> Decompress(CompressedWeightMatrix compressed)
    {
        int rows    = compressed.LeftRows;
        int rank    = compressed.LeftColumns;   // = number of singular values kept
        // V (RightFactor) has shape (originalColumns × rank), stored row-major.
        // RightRows = originalColumns, RightColumns = rank.
        // We need the original column count, which is stored in RightRows.
        int columns = compressed.RightRows;     // = original number of columns

        // Validate consistency of the compressed data.
        if (compressed.SingularValues.Length != rank)
            return OperationResult<double[]>.Fail(
                $"SingularValues length {compressed.SingularValues.Length} must equal rank {rank}.",
                "BAD_COMPRESSED");

        var result = new double[rows * columns];

        // For each singular triplet k: add sigma_k × u_k × v_k^T to the result.
        for (int k = 0; k < rank; k++)
        {
            double sigma = compressed.SingularValues[k];

            // Extract column k of U (length = rows): LeftFactor[row, k] = LeftFactor[row*rank + k]
            for (int r = 0; r < rows; r++)
            {
                double uEntry = compressed.LeftFactor[r * rank + k];
                // For each column of the result, add sigma × u[r] × v[c]
                for (int c = 0; c < columns; c++)
                {
                    double vEntry = compressed.RightFactor[c * rank + k];
                    result[r * columns + c] += sigma * uEntry * vEntry;
                }
            }
        }

        return OperationResult<double[]>.Ok(result);
    }
}
