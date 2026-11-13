namespace MatrixAlgorithms;

// MatrixAlgorithmsDispatcher is the single entry point for all matrix algorithm
// operations in this sub-project (Genie DNA rule 6: one dispatcher routes all requests).
// Callers provide an algorithm name and a request dictionary; the dispatcher routes to
// the correct service and returns a plain result dictionary.
// This prevents callers from depending on specific service types directly.
public sealed class MatrixAlgorithmsDispatcher
{
    // The storage and logger shared by all services created by this dispatcher.
    private readonly IMatrixStorage       _storage;
    private readonly IMatrixEventLogger   _logger;

    // A list of all algorithm names this dispatcher understands.
    // Callers can use this to discover available algorithms without hard-coding names.
    public IReadOnlyList<string> AvailableAlgorithms { get; } = [
        "lu",             // LU decomposition with partial pivoting
        "qr",             // QR decomposition using Householder reflections
        "svd",            // Full singular value decomposition
        "truncated-svd",  // Truncated SVD keeping K largest singular values
        "randomized-svd", // Randomized SVD (faster, approximate)
        "lu-solve",       // Solve Ax = b using LU factors
        "qr-least-squares", // Least-squares solve using QR
        "sparse-solve",   // Solve a sparse linear system
        "compress-weight", // Compress a weight matrix using SVD
        "decompress-weight", // Reconstruct from compressed weight matrix
        "analyse-weight", // Spectral decay analysis of a weight matrix
        "quantize",       // Integer 8-bit quantization of a weight matrix
    ];

    // Create the dispatcher by injecting the fabric dependencies.
    public MatrixAlgorithmsDispatcher(IMatrixStorage storage, IMatrixEventLogger logger)
    {
        _storage = storage;
        _logger  = logger;
    }

    // Route a request to the named algorithm and return the result.
    // algorithmName — one of the names in AvailableAlgorithms (case-insensitive)
    // request       — a dictionary with input keys the specific algorithm needs:
    //   "matrix"  (double[]) — the input matrix
    //   "rows"    (int)      — number of rows
    //   "columns" (int)      — number of columns
    //   "targetRank" (int)   — for truncated/randomized SVD and compress-weight
    //   "lMatrix", "uMatrix", "rhs" — for lu-solve
    //   "qMatrix", "rMatrix", "rhs" — for qr-least-squares
    //   "sparseMatrix" (CsrSparseMatrix) — for sparse-solve
    //   "compressed"  (CompressedWeightMatrix) — for decompress-weight
    //   "weights"  (double[]) — for quantize
    // Returns a dictionary with key "result" containing the algorithm output.
    public OperationResult<Dictionary<string, object>> Dispatch(
        string algorithmName, Dictionary<string, object> request)
    {
        // Normalise the name to lower-case so callers need not match exact casing.
        string name = algorithmName.ToLowerInvariant().Trim();

        return name switch
        {
            "lu"              => RunDecomposition(new LuDecompositionService(_storage, _logger),
                                                   request),
            "qr"              => RunDecomposition(new QrDecompositionService(_storage, _logger),
                                                   request),
            "svd"             => RunDecomposition(
                                     new SingularValueDecompositionService(_storage, _logger),
                                     request),
            "truncated-svd"   => RunTruncatedSvd(request),
            "randomized-svd"  => RunRandomizedSvd(request),
            "lu-solve"        => RunLuSolve(request),
            "qr-least-squares"=> RunQrLeastSquares(request),
            "sparse-solve"    => RunSparseSolve(request),
            "compress-weight" => RunCompressWeight(request),
            "decompress-weight" => RunDecompressWeight(request),
            "analyse-weight"  => RunAnalyseWeight(request),
            "quantize"        => RunQuantize(request),
            _                 => OperationResult<Dictionary<string, object>>.Fail(
                                     $"Unknown algorithm '{algorithmName}'. " +
                                     $"Available: {string.Join(", ", AvailableAlgorithms)}.",
                                     "UNKNOWN_ALGORITHM")
        };
    }

    // ── Private routing helpers ───────────────────────────────────────────

    // Run any IMatrixDecomposer using the "matrix", "rows", "columns" keys from the request.
    private static OperationResult<Dictionary<string, object>> RunDecomposition(
        IMatrixDecomposer svc, Dictionary<string, object> req)
    {
        if (!TryGet<double[]>(req, "matrix", out var matrix) ||
            !TryGet<int>(req, "rows",    out var rows) ||
            !TryGet<int>(req, "columns", out var columns))
            return MissingKey();

        var r = svc.Decompose(matrix!, rows, columns);
        return r.IsSuccess
            ? OperationResult<Dictionary<string, object>>.Ok(
                new() { ["result"] = r.Value!, ["algorithm"] = r.Value!.AlgorithmName })
            : r.MapError<Dictionary<string, object>>();
    }

    private OperationResult<Dictionary<string, object>> RunTruncatedSvd(
        Dictionary<string, object> req)
    {
        if (!TryGet<double[]>(req, "matrix",     out var matrix) ||
            !TryGet<int>(req, "rows",             out var rows) ||
            !TryGet<int>(req, "columns",          out var columns) ||
            !TryGet<int>(req, "targetRank",       out var targetRank))
            return MissingKey();

        var svc = new TruncatedSvdService(_storage, _logger);
        var r   = svc.DecomposeRank(matrix!, rows, columns, targetRank);
        return r.IsSuccess
            ? OperationResult<Dictionary<string, object>>.Ok(
                new() { ["result"] = r.Value!, ["error"] = r.Value!.ReconstructionError })
            : r.MapError<Dictionary<string, object>>();
    }

    private OperationResult<Dictionary<string, object>> RunRandomizedSvd(
        Dictionary<string, object> req)
    {
        if (!TryGet<double[]>(req, "matrix",   out var matrix) ||
            !TryGet<int>(req, "rows",           out var rows) ||
            !TryGet<int>(req, "columns",        out var columns) ||
            !TryGet<int>(req, "targetRank",     out var targetRank))
            return MissingKey();

        var svc = new RandomizedSvdService(_storage, _logger);
        var r   = svc.DecomposeRank(matrix!, rows, columns, targetRank);
        return r.IsSuccess
            ? OperationResult<Dictionary<string, object>>.Ok(
                new() { ["result"] = r.Value!, ["error"] = r.Value!.ReconstructionError })
            : r.MapError<Dictionary<string, object>>();
    }

    private static OperationResult<Dictionary<string, object>> RunLuSolve(
        Dictionary<string, object> req)
    {
        if (!TryGet<double[]>(req, "lMatrix", out var l) ||
            !TryGet<double[]>(req, "uMatrix", out var u) ||
            !TryGet<int>(req, "size",         out var size) ||
            !TryGet<double[]>(req, "rhs",     out var rhs))
            return MissingKey();

        var r = LuLinearSolver.Solve(l!, u!, size, rhs!);
        return r.IsSuccess
            ? OperationResult<Dictionary<string, object>>.Ok(new() { ["result"] = r.Value! })
            : r.MapError<Dictionary<string, object>>();
    }

    private static OperationResult<Dictionary<string, object>> RunQrLeastSquares(
        Dictionary<string, object> req)
    {
        if (!TryGet<double[]>(req, "qMatrix", out var q) ||
            !TryGet<double[]>(req, "rMatrix", out var r) ||
            !TryGet<int>(req, "rows",         out var rows) ||
            !TryGet<int>(req, "columns",      out var cols) ||
            !TryGet<double[]>(req, "rhs",     out var rhs))
            return MissingKey();

        var res = QrLeastSquaresSolver.Solve(q!, r!, rows, cols, rhs!);
        return res.IsSuccess
            ? OperationResult<Dictionary<string, object>>.Ok(new() { ["result"] = res.Value! })
            : res.MapError<Dictionary<string, object>>();
    }

    private OperationResult<Dictionary<string, object>> RunSparseSolve(
        Dictionary<string, object> req)
    {
        if (!TryGet<CsrSparseMatrix>(req, "sparseMatrix", out var sparse) ||
            !TryGet<double[]>(req, "rhs", out var rhs))
            return MissingKey();

        var svc = new SparseLinearSolver(_storage, _logger);
        var r   = svc.Solve(sparse!, rhs!);
        return r.IsSuccess
            ? OperationResult<Dictionary<string, object>>.Ok(new() { ["result"] = r.Value! })
            : r.MapError<Dictionary<string, object>>();
    }

    private OperationResult<Dictionary<string, object>> RunCompressWeight(
        Dictionary<string, object> req)
    {
        if (!TryGet<double[]>(req, "matrix",   out var matrix) ||
            !TryGet<int>(req, "rows",           out var rows) ||
            !TryGet<int>(req, "columns",        out var columns) ||
            !TryGet<int>(req, "targetRank",     out var targetRank))
            return MissingKey();

        var svc = new SvdWeightCompressor(_storage, _logger);
        var r   = svc.Compress(matrix!, rows, columns, targetRank);
        return r.IsSuccess
            ? OperationResult<Dictionary<string, object>>.Ok(
                new() { ["result"] = r.Value!, ["error"] = r.Value!.ReconstructionError })
            : r.MapError<Dictionary<string, object>>();
    }

    private OperationResult<Dictionary<string, object>> RunDecompressWeight(
        Dictionary<string, object> req)
    {
        if (!TryGet<CompressedWeightMatrix>(req, "compressed", out var compressed))
            return MissingKey();

        var svc = new SvdWeightCompressor(_storage, _logger);
        var r   = svc.Decompress(compressed!);
        return r.IsSuccess
            ? OperationResult<Dictionary<string, object>>.Ok(new() { ["result"] = r.Value! })
            : r.MapError<Dictionary<string, object>>();
    }

    private OperationResult<Dictionary<string, object>> RunAnalyseWeight(
        Dictionary<string, object> req)
    {
        if (!TryGet<double[]>(req, "matrix",  out var matrix) ||
            !TryGet<int>(req, "rows",          out var rows) ||
            !TryGet<int>(req, "columns",       out var columns))
            return MissingKey();

        var svc = new WeightMatrixAnalyzer(_storage, _logger);
        var r   = svc.Analyse(matrix!, rows, columns);
        return r.IsSuccess
            ? OperationResult<Dictionary<string, object>>.Ok(
                new() { ["result"] = r.Value!, ["estimatedRank"] = r.Value!.EstimatedRank })
            : r.MapError<Dictionary<string, object>>();
    }

    private static OperationResult<Dictionary<string, object>> RunQuantize(
        Dictionary<string, object> req)
    {
        if (!TryGet<double[]>(req, "weights", out var weights) ||
            !TryGet<int>(req, "rows",          out var rows) ||
            !TryGet<int>(req, "columns",       out var cols))
            return MissingKey();

        var result = Int8Quantizer.Quantize(weights!, rows, cols);
        return OperationResult<Dictionary<string, object>>.Ok(
            new() { ["result"] = result, ["maxError"] = result.MaxAbsoluteError });
    }

    // ── Utility helpers ───────────────────────────────────────────────────

    // Try to read a typed value from the request dictionary.
    // Returns false and sets value to default if the key is missing or the type is wrong.
    private static bool TryGet<T>(Dictionary<string, object> req, string key, out T? value)
    {
        if (req.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    // Produce a standard failure result for a missing required request key.
    private static OperationResult<Dictionary<string, object>> MissingKey()
        => OperationResult<Dictionary<string, object>>.Fail(
            "One or more required request keys are missing or have the wrong type.",
            "MISSING_KEY");
}
