namespace MatrixAlgorithms;

// QuantizationPlusLowRankCompressor combines two compression strategies:
//   1. Low-rank approximation via truncated SVD captures the dominant structure.
//   2. Integer 8-bit quantization compresses the residual (the remaining error).
// This two-stage approach achieves better accuracy than quantization alone
// because the dominant structure is stored in higher precision (float64),
// while the residual is small enough that quantization error is negligible.
// Strategy: A ≈ A_lr + Dequantize(Q8(A − A_lr))
//   where A_lr is the rank-K approximation and A − A_lr is the residual.
public sealed class QuantizationPlusLowRankCompressor : BaseService
{
    // The SVD compressor that produces the low-rank approximation.
    private readonly SvdWeightCompressor _svdCompressor;

    // Create the compressor by injecting storage and logger (fabric-first rule).
    public QuantizationPlusLowRankCompressor(IMatrixStorage storage, IMatrixEventLogger logger)
        : base(storage, logger)
    {
        _svdCompressor = new SvdWeightCompressor(storage, logger);
    }

    // Compress A using low-rank approximation for the dominant part
    // and 8-bit quantization for the residual.
    // Returns a tuple of (compressed low-rank part, quantized residual).
    public OperationResult<(CompressedWeightMatrix LowRankPart, QuantizedWeightMatrix Residual)>
        Compress(double[] weightMatrix, int rows, int columns, int targetRank)
    {
        if (weightMatrix.Length < rows * columns)
            return OperationResult<(CompressedWeightMatrix, QuantizedWeightMatrix)>.Fail(
                "Weight matrix array is too short.", "BAD_INPUT");

        // ── Step 1: Compute the low-rank approximation A_lr ──────────────
        var compressResult = _svdCompressor.Compress(weightMatrix, rows, columns, targetRank);
        if (!compressResult.IsSuccess)
            return OperationResult<(CompressedWeightMatrix, QuantizedWeightMatrix)>.Fail(
                $"Low-rank compression failed: {compressResult.ErrorMessage}",
                compressResult.ErrorCode);

        var lowRankPart = compressResult.Value!;

        // ── Step 2: Reconstruct A_lr and compute the residual ─────────────
        var decompressResult = _svdCompressor.Decompress(lowRankPart);
        if (!decompressResult.IsSuccess)
            return OperationResult<(CompressedWeightMatrix, QuantizedWeightMatrix)>.Fail(
                "Failed to reconstruct low-rank approximation.", "DECOMPRESS_FAILED");

        double[] approx   = decompressResult.Value!;
        int      count    = rows * columns;
        var      residual = new double[count];

        // residual[i] = original[i] − approximated[i]
        for (int i = 0; i < count; i++)
            residual[i] = weightMatrix[i] - approx[i];

        // ── Step 3: Quantise the residual to 8 bits ───────────────────────
        var quantizedResidual = Int8Quantizer.Quantize(residual, rows, columns);

        // Save and emit event (DNA rules 8 and 9).
        double finalError = quantizedResidual.MaxAbsoluteError;
        Storage.TrySave("qplus-last", new Dictionary<string, object>
        {
            ["rows"] = rows, ["columns"] = columns,
            ["targetRank"] = targetRank, ["finalError"] = finalError
        });

        EventLogger.Log(new StandardEvent(
            "matrix.qplus-compress.completed",
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                ["targetRank"] = targetRank, ["finalError"] = finalError
            }));

        return OperationResult<(CompressedWeightMatrix, QuantizedWeightMatrix)>
            .Ok((lowRankPart, quantizedResidual));
    }

    // Reconstruct the approximate full weight matrix from the two-stage compressed form.
    // result ≈ Decompress(LowRankPart) + Dequantize(Residual)
    public OperationResult<double[]> Decompress(
        CompressedWeightMatrix lowRankPart, QuantizedWeightMatrix residual)
    {
        // Reconstruct the low-rank approximation.
        var lowRankResult = _svdCompressor.Decompress(lowRankPart);
        if (!lowRankResult.IsSuccess)
            return lowRankResult;

        double[] lowRank = lowRankResult.Value!;

        // Dequantise the residual.
        double[] residualFloat = Int8Quantizer.Dequantize(residual);

        // Add the two parts together entry by entry.
        int count  = lowRank.Length;
        var result = new double[count];
        for (int i = 0; i < count; i++)
            result[i] = lowRank[i] + residualFloat[i];

        return OperationResult<double[]>.Ok(result);
    }
}
