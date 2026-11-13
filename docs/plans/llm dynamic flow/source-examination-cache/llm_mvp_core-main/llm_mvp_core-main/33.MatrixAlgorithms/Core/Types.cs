namespace MatrixAlgorithms;

// ─── Decomposition result types ─────────────────────────────────────────────

// DecompositionResult holds the output matrices produced by any decomposition algorithm.
// The interpretation of FactorOne, FactorTwo, and FactorThree depends on which
// algorithm produced this result — the AlgorithmName field identifies it.
//
// For LU decomposition:
//   FactorOne = L (lower triangular — нижнетреугольная матрица)
//   FactorTwo = U (upper triangular — верхнетреугольная матрица)
//   FactorThree = null
//
// For QR decomposition:
//   FactorOne = Q (orthogonal matrix — ортогональная матрица, columns are perpendicular unit vectors)
//   FactorTwo = R (upper triangular — верхнетреугольная матрица)
//   FactorThree = null
//
// For singular value decomposition (SVD):
//   FactorOne = U (left singular vectors — левые сингулярные векторы)
//   FactorTwo = S (singular values stored as a flat array — сингулярные значения)
//   FactorThree = V (right singular vectors — правые сингулярные векторы, NOT transposed)
public sealed record DecompositionResult
{
    // First factor matrix stored as a flat row-major double array.
    public double[] FactorOne        { get; init; } = [];
    // Number of rows in the first factor matrix.
    public int      FactorOneRows    { get; init; }
    // Number of columns in the first factor matrix.
    public int      FactorOneColumns { get; init; }

    // Second factor matrix stored as a flat row-major double array.
    public double[] FactorTwo        { get; init; } = [];
    // Number of rows in the second factor matrix (1 for singular value arrays).
    public int      FactorTwoRows    { get; init; }
    // Number of columns in the second factor matrix (rank for singular value arrays).
    public int      FactorTwoColumns { get; init; }

    // Third factor matrix, only present for singular value decomposition (V matrix).
    // Null for LU and QR decompositions.
    public double[]? FactorThree        { get; init; }
    // Number of rows in the third factor matrix; 0 when FactorThree is null.
    public int       FactorThreeRows    { get; init; }
    // Number of columns in the third factor matrix; 0 when FactorThree is null.
    public int       FactorThreeColumns { get; init; }

    // The name of the algorithm that produced this decomposition, e.g. "LU with partial pivoting".
    public string AlgorithmName      { get; init; } = string.Empty;

    // Frobenius norm of the difference between the reconstructed matrix and the original.
    // Zero for exact decompositions (LU, QR, full SVD); small positive for approximations.
    public double ReconstructionError { get; init; }

    // The number of singular values kept in a truncated or randomized SVD.
    // Zero for exact decompositions.
    public int TargetRank { get; init; }
}

// ─── Sparse matrix data types ────────────────────────────────────────────────

// SparseMatrixData is the portable representation of a sparse matrix
// (разреженная матрица — матрица, где большинство элементов равны нулю).
// The Format string tells converters which format the three arrays use.
public sealed record SparseMatrixData(
    // Row indices of the non-zero entries (for COO and CSR-adjacent formats).
    int[]    RowIndices,
    // Column indices of the non-zero entries.
    int[]    ColumnIndices,
    // Non-zero values corresponding to the index arrays above.
    double[] Values,
    // Total number of rows in the full matrix.
    int      Rows,
    // Total number of columns in the full matrix.
    int      Columns,
    // Format identifier: "COO", "CSR", or "CSC".
    string   Format);

// ─── Weight compression types ────────────────────────────────────────────────

// CompressedWeightMatrix stores a neural network weight matrix in low-rank form.
// Instead of storing all rows × columns values, we store:
//   LeftFactor  (rows × rank) times diag(SingularValues) times RightFactor^T (rank × cols)
// This uses rows*rank + rank + rank*cols values instead of rows*cols — much smaller
// when rank is small compared to both rows and columns.
public sealed record CompressedWeightMatrix(
    // Left factor matrix U, stored flat row-major, dimensions LeftRows × LeftColumns.
    double[] LeftFactor,
    // Number of rows in LeftFactor (equals the original matrix row count).
    int      LeftRows,
    // Number of columns in LeftFactor (equals the compression rank).
    int      LeftColumns,
    // The rank dominant singular values (сингулярные значения — measure of importance of each direction).
    double[] SingularValues,
    // Right factor matrix V, stored flat row-major, dimensions RightRows × RightColumns.
    // To reconstruct: result = LeftFactor * diag(SingularValues) * RightFactor^T.
    double[] RightFactor,
    // Number of rows in RightFactor (equals the compression rank).
    int      RightRows,
    // Number of columns in RightFactor (equals the original matrix column count).
    int      RightColumns,
    // The method used to produce this compressed form, e.g. "TruncatedSVD".
    string   CompressionMethod,
    // Frobenius norm of the reconstruction error; 0 when there is no approximation.
    double   ReconstructionError);

// QuantizedWeightMatrix stores a weight matrix as integer 8-bit values
// (integers in the range -128 to 127) together with a scale factor and zero-point
// offset that allow converting back to floating-point.
// This format uses one byte per weight instead of eight (for double), saving 8×.
public sealed record QuantizedWeightMatrix(
    // Each weight as a signed 8-bit integer (signed byte, range -128 to 127).
    sbyte[]  QuantizedValues,
    // The multiplication factor to convert each quantized integer back to a float.
    // original ≈ (quantized - ZeroPoint) * Scale.
    double   Scale,
    // The integer that represents the floating-point zero; applied before scaling.
    int      ZeroPoint,
    // Number of rows in the original matrix (needed to reshape the flat array).
    int      Rows,
    // Number of columns in the original matrix.
    int      Columns,
    // Maximum absolute error between original and reconstructed values.
    double   MaxAbsoluteError);

// SpectralDecayReport holds the analysis of how fast the singular values
// of a weight matrix decrease. Fast decay means a low-rank approximation is accurate;
// slow decay means the matrix needs many rank components to be well approximated.
public sealed record SpectralDecayReport(
    // All computed singular values sorted from largest to smallest.
    double[] SingularValues,
    // The ratio of the largest singular value to the smallest non-zero singular value.
    // A large ratio means the matrix has a wide range of "importance" across its directions.
    double   ConditionNumber,
    // The estimated numerical rank: the count of singular values above the tolerance threshold.
    int      EstimatedRank,
    // The fraction of the total matrix energy (sum of squares of all singular values)
    // captured by the top EstimatedRank singular values.
    double   EnergyFractionCaptured,
    // The tolerance used to decide whether a singular value is "significant" (not just noise).
    double   SignificanceTolerance);
