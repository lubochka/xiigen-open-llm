# Matrix Algorithms — Sub-Projects 28 through 30
## Source document: matrix decomposition and sparse matrix families reference
## Solution: LlmFromScratch.sln — Implementation Plan v1.0

**Project:** llm-mvp-core  
**Language:** C# / .NET 8  
**External packages:** none — all algorithms implemented from scratch following reference Fortran/C sources  
**Document purpose:** Step-by-step implementation plan for three matrix algorithm families as independent sub-projects within the same solution  
**This document does not touch XIIGen work.**

---

## What this document adds to the solution

Three sub-projects covering the complete landscape of matrix algorithms used in machine learning and scientific computing:

| Number | Family name | Algorithms inside |
|---|---|---|
| 28 | Matrix decomposition (exact and approximate) | LU, QR, full Singular Value Decomposition, truncated Singular Value Decomposition, randomized Singular Value Decomposition |
| 29 | Sparse matrix formats and operations | Compressed Sparse Row, Coordinate format, Compressed Sparse Column, sparse matrix multiplication, sparse linear solvers |
| 30 | Low-rank neural network weight compression | Weight matrix decomposition via truncated Singular Value Decomposition, quantization combined with low-rank approximation, sparse attention matrices |

**Why these matter to the existing solution:**
- Sub-project 00 (TensorCore) implements basic tensor operations — sub-project 28 adds the decomposition layer on top
- Sub-project 03 (Transformer Model) has large weight matrices — sub-project 30 can compress them by 4–16× with small accuracy loss
- Sub-project 10 (LoRA) adds small trainable adapter matrices — sub-project 30 is the complementary operation: compressing large existing weight matrices
- Sub-project 07 (Graph Engine) builds large adjacency matrices — sub-project 29 stores them 10–100× more efficiently

**Dependency on sub-project 00 (TensorCore):** sub-projects 28 and 30 use the `Tensor` class from TensorCore for all matrix storage and basic operations. Sub-project 29 is independent — it defines its own sparse matrix types.

---

## Part 1 — Coding Rules (Genie DNA) — All Nine Apply

All nine coding rules from the main implementation plan (v2.0) apply to all three sub-projects. Key reminders for numerical code:

- Coding rule 1: matrix data is stored as `double[]` flat arrays or the existing `Tensor` type — no typed matrix classes with business logic inside
- Coding rule 3: every decomposition method returns `OperationResult<T>` — a singular matrix (cannot be decomposed) returns a failure result, never throws
- Coding rule 4: every service class (`LuDecompositionService`, `SparseMatrixService`, etc.) extends `BaseService`
- Coding rule 7: decompositions that write to disk (large weight matrix compressions) accept an `operationId` — same matrix submitted twice returns cached result

---

## Part 2 — Global Abbreviation Table for Matrix Algorithm Domain

| Short form | Full name | Plain-language explanation |
|---|---|---|
| Singular Value Decomposition (SVD) | Singular Value Decomposition (сингулярное разложение матрицы) | every matrix can be written as three matrices multiplied together: left directions × importance values × right directions; the importance values (singular values) tell you which directions matter most |
| LU decomposition | LU decomposition (разложение матрицы на нижнетреугольную и верхнетреугольную) | writes any square matrix as two triangular matrices multiplied together: L (Lower — нижнетреугольная, всё выше главной диагонали = 0) and U (Upper — верхнетреугольная, всё ниже = 0); makes solving linear systems very fast |
| QR decomposition | QR decomposition (разложение на ортогональную и верхнетреугольную матрицы) | writes any matrix as Q (orthogonal — столбцы взаимно перпендикулярны и имеют длину 1) times R (upper triangular — верхнетреугольная); used in least-squares problems and for numerically stable systems |
| Rank (matrix rank) | Rank of a matrix (ранг матрицы) | the number of truly independent rows (or columns) in a matrix; a rank-5 matrix out of 1000 rows means most rows are just combinations of 5 "basis" rows |
| Low-rank approximation | Low-rank matrix approximation (приближение матрицы малого ранга) | replace a large matrix with two or three smaller matrices whose product is close to the original; used to compress neural network weight matrices |
| Truncated Singular Value Decomposition | Truncated Singular Value Decomposition (усечённое сингулярное разложение) | instead of computing all singular values and directions, compute only the K largest ones; the result is the best possible rank-K approximation of the original matrix |
| Randomized Singular Value Decomposition | Randomized Singular Value Decomposition (случайное сингулярное разложение) | project the matrix onto a random low-dimensional subspace first, then apply exact decomposition to the small result; 10–100× faster than exact decomposition with nearly identical accuracy |
| Singular value | Singular value (сингулярное значение) | a non-negative number that measures how "important" one direction in the matrix is; large singular value = that direction carries most of the information; small = that direction is mostly noise |
| Left singular vectors | Left singular vectors (левые сингулярные векторы) | the matrix U in the decomposition A = U × S × Vᵀ; each column is a direction in the row space of the original matrix |
| Right singular vectors | Right singular vectors (правые сингулярные векторы) | the matrix V in the decomposition A = U × S × Vᵀ; each column is a direction in the column space of the original matrix |
| Frobenius norm | Frobenius norm (норма Фробениуса) | a measure of how large a matrix is: the square root of the sum of squares of all its entries; used to measure how much accuracy is lost in an approximation |
| Pivot | Pivot element (опорный элемент в разложении) | during Gaussian elimination (Гауссово исключение — шаговое приведение матрицы к треугольному виду), the pivot is the element chosen on the diagonal; choosing the largest element (partial pivoting) makes the algorithm numerically stable |
| Partial pivoting | Partial pivoting (частичная перестановка строк) | during LU decomposition, swap rows to put the largest available element on the diagonal before each elimination step; prevents division by very small numbers that would introduce large numerical errors |
| Orthogonal matrix | Orthogonal matrix (ортогональная матрица) | a square matrix whose columns are all perpendicular to each other and all have length 1; multiplying by an orthogonal matrix rotates or reflects vectors without changing their length |
| Householder reflection | Householder reflection (отражение Хаусхолдера) | a way to zero out all entries below the diagonal in one column of a matrix in a single step; the building block of numerically stable QR decomposition |
| Givens rotation | Givens rotation (вращение Гивенса) | zeros out one specific entry of a matrix by rotating two rows; slower than Householder for dense matrices but useful for sparse ones |
| Sparse matrix | Sparse matrix (разреженная матрица) | a matrix where most entries are zero; storing only the non-zero entries and their positions uses far less memory than storing all entries |
| CSR format | Compressed Sparse Row format (формат сжатых строк) | a way to store a sparse matrix: three arrays — the non-zero values, the column index of each value, and the position in the first two arrays where each row starts |
| COO format | Coordinate format (координатный формат разреженной матрицы) | store each non-zero entry as a triple (row, column, value); the simplest sparse format, easy to build but slow to multiply |
| CSC format | Compressed Sparse Column format (формат сжатых столбцов) | same as Compressed Sparse Row but organized by columns instead of rows; faster for column-oriented operations |
| Fill-in | Fill-in in sparse factorization (заполнение в разреженном разложении) | when computing LU or QR of a sparse matrix, previously-zero entries may become non-zero; good ordering of rows/columns minimizes fill-in |
| Approximate Message Passing (AMP) | — | not used in this plan |
| LoRA | Low-Rank Adaptation (адаптация матриц низкого ранга для дообучения) | adds small trainable matrices A×B on top of frozen weight matrices (see sub-project 10) |
| Quantization | Weight quantization (квантизация весов нейросети) | reducing the number of bits used to store each weight; 8-bit integers instead of 32-bit floats = 4× less memory, small accuracy loss |
| Mixed-precision | Mixed-precision computation (вычисления смешанной точности) | storing weights in low precision (8-bit or 4-bit) but computing in higher precision (16-bit or 32-bit) |
| Spectral decay | Spectral decay of a weight matrix (спектральное затухание матрицы весов) | how quickly the singular values of a weight matrix decrease; fast decay means a low-rank approximation will be accurate; slow decay means the matrix is genuinely high-rank |

---

## Part 3 — Shared Fabric Interfaces

All three sub-projects share one project root and one set of fabric interfaces:

```
28.MatrixAlgorithms/
├── Fabric/
│   ├── IMatrixDecomposer.cs        — decomposes a matrix into factors
│   ├── ISparseMatrixStorage.cs     — stores and retrieves sparse matrix data
│   ├── IWeightCompressor.cs        — compresses neural network weight matrices
│   └── InMemoryMatrixStorage.cs    — concrete implementation for tests
├── Core/
│   ├── DenseMatrix.cs              — thin wrapper over double[] with row/col indexing
│   ├── DecompositionResult.cs      — holds the factor matrices from any decomposition
│   └── SparseMatrixEntry.cs        — one non-zero entry: row, column, value
├── Families/
│   ├── 01-Decomposition/           — sub-project 28
│   ├── 02-Sparse/                  — sub-project 29
│   └── 03-LowRankCompression/      — sub-project 30
└── Tests/
```

```csharp
// IMatrixDecomposer is the single interface that every decomposition algorithm implements.
// Whether LU, QR, or Singular Value Decomposition — they all receive a matrix and return factors.
public interface IMatrixDecomposer
{
    // Decompose the input matrix into its factor matrices
    // matrix — the matrix to decompose, stored as a flat row-major double array
    //   (row-major means the entries of row 0 come first, then row 1, etc.)
    // rows — the number of rows in the matrix
    // columns — the number of columns in the matrix
    // Returns the factor matrices specific to each algorithm
    //   (for LU: lower and upper triangular matrices; for Singular Value Decomposition:
    //    left singular vectors, singular values, right singular vectors)
    Task<OperationResult<DecompositionResult>> DecomposeAsync(
        double[] matrix,
        int rows,
        int columns);
}

// ISparseMatrixStorage saves and loads sparse matrices without materializing the zeros.
// Any service that needs to store a graph adjacency matrix or a sparse weight matrix uses this.
public interface ISparseMatrixStorage
{
    // Save a sparse matrix in the specified format under the given name
    Task<OperationResult<bool>> SaveAsync(string name, SparseMatrixData data);

    // Load a previously saved sparse matrix by name
    Task<OperationResult<SparseMatrixData>> LoadAsync(string name);
}

// IWeightCompressor compresses a large neural network weight matrix
// into a low-rank approximation that uses less memory while preserving most accuracy.
public interface IWeightCompressor
{
    // Compress a weight matrix by keeping only the K most important directions
    // weightMatrix — the full weight matrix as a flat double array
    // rows, columns — dimensions of the weight matrix
    // targetRank — how many singular values to keep (K); smaller = more compression, less accuracy
    // Returns the compressed representation (two or three smaller matrices)
    Task<OperationResult<CompressedWeightMatrix>> CompressAsync(
        double[] weightMatrix,
        int rows,
        int columns,
        int targetRank);

    // Reconstruct the approximate full matrix from the compressed representation
    // The result is not identical to the original but is close (low reconstruction error)
    Task<OperationResult<double[]>> DecompressAsync(CompressedWeightMatrix compressed);
}

// DecompositionResult holds the output matrices from any matrix decomposition.
// Each decomposition algorithm stores its specific factors here.
public record DecompositionResult
{
    // First factor matrix (L in LU, Q in QR, U in Singular Value Decomposition)
    // stored as a flat row-major double array
    public double[] FactorOne { get; init; } = Array.Empty<double>();
    public int FactorOneRows { get; init; }
    public int FactorOneColumns { get; init; }

    // Second factor matrix (U in LU, R in QR, diagonal S in Singular Value Decomposition)
    public double[] FactorTwo { get; init; } = Array.Empty<double>();
    public int FactorTwoRows { get; init; }
    public int FactorTwoColumns { get; init; }

    // Third factor matrix (only present in Singular Value Decomposition: Vᵀ)
    public double[]? FactorThree { get; init; }
    public int FactorThreeRows { get; init; }
    public int FactorThreeColumns { get; init; }

    // The name of the algorithm that produced this decomposition
    public string AlgorithmName { get; init; } = string.Empty;

    // For approximate decompositions: the Frobenius norm of the reconstruction error
    // (square root of the sum of squared differences between original and reconstructed matrix)
    // Zero for exact decompositions; small for good approximations
    public double ReconstructionError { get; init; }
}
```

---

## Sub-Project 28 — Matrix Decomposition (Exact and Approximate)

**What this sub-project implements:** the five decomposition algorithms from the source document. Three are exact (LU, QR, full Singular Value Decomposition) and two are approximate (truncated Singular Value Decomposition, randomized Singular Value Decomposition). All five are building blocks used throughout machine learning, scientific computing, and signal processing.

**Reference sources:**
- LU and QR: `https://github.com/Reference-LAPACK/lapack` — `dgetrf.f` (LU) and `dgeqrf.f` (QR) — BSD-like license
- Full Singular Value Decomposition: `https://github.com/Reference-LAPACK/lapack` — `dgesvd.f` and `dgesdd.f`
- Randomized Singular Value Decomposition: `https://github.com/scikit-learn/scikit-learn` — `sklearn/utils/extmath.py`, function `randomized_svd()` — BSD 3-Clause
- Fast Randomized Singular Value Decomposition: `https://github.com/facebook/fbpca` — `fbpca.py` — BSD 3-Clause

**Dependencies:** sub-project 00 (TensorCore) for the `Tensor` type and basic matrix multiply. If TensorCore is not yet complete, use `double[]` arrays directly with manual index arithmetic.

**Folder:** `28.MatrixAlgorithms/Families/01-Decomposition/`

---

### Phase 0 — Dense matrix helper and numerical utilities (1 day)

Before any decomposition algorithm, define the shared numerical helpers.

What to write: `DenseMatrix.cs`, `NumericalUtilities.cs`

```csharp
// DenseMatrix is a thin wrapper around a flat double array that adds named row/column indexing.
// It does not contain any algorithms — just storage and index arithmetic.
// "Dense" (плотная матрица) means every entry is stored, including zeros.
// This contrasts with sparse matrices (sub-project 29) where zeros are omitted.
public class DenseMatrix
{
    // The flat array of all matrix entries in row-major order
    // Row-major means: row 0 entries come first, then row 1, then row 2, and so on
    // To find entry (row, col): access data[row * Columns + col]
    private readonly double[] data;

    // How many rows the matrix has
    public int Rows { get; }

    // How many columns the matrix has
    public int Columns { get; }

    // Create a matrix filled with zeros
    public DenseMatrix(int rows, int columns)
    {
        // Allocate space for every entry, initialized to 0 by .NET's default
        data = new double[rows * columns];
        Rows = rows;
        Columns = columns;
    }

    // Create a matrix from an existing flat array (does not copy — shares the array)
    public DenseMatrix(double[] existingData, int rows, int columns)
    {
        data = existingData;
        Rows = rows;
        Columns = columns;
    }

    // Get or set the entry at the given row and column
    // Converts 2D (row, col) coordinates to the 1D flat array index
    public double this[int row, int column]
    {
        get => data[row * Columns + column];
        set => data[row * Columns + column] = value;
    }

    // Return the flat array for algorithms that operate directly on the storage
    public double[] AsArray() => data;

    // Create a copy of a specific row as a new 1D array
    public double[] GetRow(int rowIndex)
    {
        var row = new double[Columns];
        Array.Copy(data, rowIndex * Columns, row, 0, Columns);
        return row;
    }

    // Swap two complete rows in-place (used by LU decomposition with partial pivoting)
    public void SwapRows(int rowA, int rowB)
    {
        if (rowA == rowB) return;
        for (int col = 0; col < Columns; col++)
        {
            // Hold the value from row A temporarily while we overwrite it
            double temp = this[rowA, col];
            this[rowA, col] = this[rowB, col];
            this[rowB, col] = temp;
        }
    }
}
```

`NumericalUtilities` provides matrix multiply and the Frobenius norm (норма Фробениуса — корень из суммы квадратов всех элементов — мера "размера" матрицы):

```csharp
// NumericalUtilities provides the basic matrix operations needed by all decompositions.
// These are self-contained mathematical functions with no dependencies.
public static class NumericalUtilities
{
    // Multiply two dense matrices A (rows_a × cols_a) and B (cols_a × cols_b)
    // and return the result C (rows_a × cols_b).
    // Uses the standard triple-loop algorithm: O(rows_a × cols_a × cols_b) operations.
    public static DenseMatrix Multiply(DenseMatrix a, DenseMatrix b)
    {
        // The number of columns in A must equal the number of rows in B
        // for the multiplication to be defined
        if (a.Columns != b.Rows)
            throw new ArgumentException(
                $"Cannot multiply: A has {a.Columns} columns but B has {b.Rows} rows.");

        var result = new DenseMatrix(a.Rows, b.Columns);

        for (int row = 0; row < a.Rows; row++)
        {
            for (int col = 0; col < b.Columns; col++)
            {
                // Each entry of the result is the dot product of a row of A with a column of B
                double sum = 0;
                for (int k = 0; k < a.Columns; k++)
                {
                    sum += a[row, k] * b[k, col];
                }
                result[row, col] = sum;
            }
        }

        return result;
    }

    // Compute the Frobenius norm (норма Фробениуса) of a matrix
    // = square root of the sum of all entries squared
    // Measures the "total size" of the matrix; used to measure reconstruction error
    public static double FrobeniusNorm(DenseMatrix matrix)
    {
        double sumOfSquares = 0;
        for (int row = 0; row < matrix.Rows; row++)
        {
            for (int col = 0; col < matrix.Columns; col++)
            {
                double entry = matrix[row, col];
                sumOfSquares += entry * entry;
            }
        }

        // Take the square root to get the norm from the sum of squares
        return Math.Sqrt(sumOfSquares);
    }
}
```

Phase exit: `dotnet build` exits 0.

---

### Phase 1 — LU decomposition with partial pivoting (2 days)

**What LU decomposition does in plain language:** writes any square matrix A as the product of two triangular matrices — L (lower triangular — нижнетреугольная: everything above the main diagonal is zero) times U (upper triangular — верхнетреугольная: everything below is zero). This makes solving a system Ax = b very fast: first solve Ly = b (forward substitution — прямая подстановка: fill in the solution row by row from top), then solve Ux = y (backward substitution — обратная подстановка: fill in from bottom up).

**Partial pivoting** (частичная перестановка строк): before each elimination step, swap rows to put the largest available entry on the diagonal. This prevents dividing by very small numbers, which would cause large numerical errors.

**Reference:** LAPACK `dgetrf.f` — BSD-like license. The Fortran code is readable as pseudocode.

What to write: `LuDecompositionService.cs`

```csharp
// LuDecompositionService decomposes a square matrix A into L × U (with row permutation).
// L is lower triangular (нижнетреугольная), U is upper triangular (верхнетреугольная).
// The decomposition enables fast solution of linear systems and matrix inversion.
// Reference: LAPACK dgetrf.f (BSD-like license)
public class LuDecompositionService : BaseService, IMatrixDecomposer
{
    public async Task<OperationResult<DecompositionResult>> DecomposeAsync(
        double[] matrix, int rows, int columns)
    {
        // LU decomposition is only defined for square matrices
        if (rows != columns)
        {
            return OperationResult<DecompositionResult>.Fail(
                $"LU decomposition requires a square matrix (same number of rows and columns). " +
                $"Got {rows} rows and {columns} columns.",
                "NOT_SQUARE");
        }

        // Work on a copy so we do not modify the caller's data
        var workingMatrix = new DenseMatrix((double[])matrix.Clone(), rows, columns);

        // The pivot array records which rows were swapped during partial pivoting
        // pivot[i] = j means row i was swapped with row j at step i
        var pivotRows = new int[rows];
        for (int i = 0; i < rows; i++) pivotRows[i] = i;

        for (int step = 0; step < rows; step++)
        {
            // --- Partial pivoting: find the row with the largest entry in this column ---
            // Searching for the largest entry (in absolute value) below and including the diagonal
            int largestRow = step;
            double largestValue = Math.Abs(workingMatrix[step, step]);

            for (int searchRow = step + 1; searchRow < rows; searchRow++)
            {
                double candidate = Math.Abs(workingMatrix[searchRow, step]);
                if (candidate > largestValue)
                {
                    largestValue = candidate;
                    largestRow = searchRow;
                }
            }

            // If the largest value is effectively zero, the matrix is singular
            // (no unique solution to Ax = b exists for this matrix)
            if (largestValue < 1e-12)
            {
                return OperationResult<DecompositionResult>.Fail(
                    $"Matrix is singular (rank-deficient) at elimination step {step}: " +
                    $"the largest available pivot is {largestValue:e3}, effectively zero. " +
                    "LU decomposition cannot proceed — the system Ax = b has no unique solution.",
                    "SINGULAR_MATRIX");
            }

            // Swap the current row with the row that has the largest pivot
            if (largestRow != step)
            {
                workingMatrix.SwapRows(step, largestRow);

                // Record the swap in the pivot array so the decompressor can undo it
                (pivotRows[step], pivotRows[largestRow]) = (pivotRows[largestRow], pivotRows[step]);
            }

            // --- Gaussian elimination: zero out all entries below the pivot ---
            // For each row below the diagonal in this column:
            //   multiply the multiplier (factor row / pivot) times the pivot row and subtract
            for (int eliminationRow = step + 1; eliminationRow < rows; eliminationRow++)
            {
                // The multiplier is how many times the pivot row must be subtracted
                // to make the entry at (eliminationRow, step) equal zero
                double multiplier = workingMatrix[eliminationRow, step] / workingMatrix[step, step];

                // Store the multiplier in the lower triangle — this IS the L matrix
                workingMatrix[eliminationRow, step] = multiplier;

                // Subtract (multiplier × pivot row) from this row for all columns to the right
                for (int col = step + 1; col < columns; col++)
                {
                    workingMatrix[eliminationRow, col] -=
                        multiplier * workingMatrix[step, col];
                }
            }
        }

        // Extract L and U from the combined working matrix
        // The upper triangle (including diagonal) is U; the strict lower triangle is L
        var lMatrix = new DenseMatrix(rows, rows);
        var uMatrix = new DenseMatrix(rows, rows);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (row > col)
                {
                    // Below the diagonal: this is an L entry (multiplier stored during elimination)
                    lMatrix[row, col] = workingMatrix[row, col];
                }
                else if (row == col)
                {
                    // On the diagonal: L has 1, U has the working matrix entry
                    lMatrix[row, col] = 1.0;
                    uMatrix[row, col] = workingMatrix[row, col];
                }
                else
                {
                    // Above the diagonal: this is a U entry
                    uMatrix[row, col] = workingMatrix[row, col];
                }
            }
        }

        var result = new DecompositionResult
        {
            FactorOne = lMatrix.AsArray(),
            FactorOneRows = rows, FactorOneColumns = rows,
            FactorTwo = uMatrix.AsArray(),
            FactorTwoRows = rows, FactorTwoColumns = rows,
            AlgorithmName = "LU with partial pivoting",
            ReconstructionError = 0 // LU is exact
        };

        return OperationResult<DecompositionResult>.Ok(result);
    }
}
```

Also write `LuLinearSolver.cs` — uses the LU factors to solve Ax = b via forward and backward substitution.

Gate B check: decompose the 3×3 matrix `[[2,1,1],[4,3,3],[8,7,9]]` and verify L × U = P × A (where P is the permutation from partial pivoting). Compare L and U entry by entry against the LAPACK reference output.

Tests:
- Verify L × U = original matrix (within 1e-10 numerical tolerance)
- Verify that solving Ax = b gives the correct x (verify by substituting back: A × x must equal b)
- Verify singular matrix (all-zeros row) returns failure result, not exception
- Verify 1×1 matrix decomposes correctly

**Estimated: 2 days**

---

### Phase 2 — QR decomposition using Householder reflections (2.5 days)

**What QR decomposition does:** writes any matrix A as Q (orthogonal — ортогональная: columns are perpendicular to each other and have length 1) times R (upper triangular). Q can be thought of as a rotation/reflection; R is the "cleaned up" upper triangular form. Used in: solving least-squares problems (метод наименьших квадратов — finding the best-fit line or curve through data), computing eigenvalues, and numerically stable linear system solving.

**Householder reflection** (отражение Хаусхолдера): a way to zero out all entries below the diagonal in one column in a single step. It reflects the column vector through a hyperplane chosen so that the reflected vector points along the first coordinate axis. This is the most numerically stable way to build QR.

**Reference:** LAPACK `dgeqrf.f` — BSD-like license.

What to write: `QrDecompositionService.cs`, `HouseholderReflection.cs`

```csharp
// HouseholderReflection computes the reflection matrix H that zeros out
// all entries of a column vector below the first entry.
// H = I - 2 × v × vᵀ where v is the reflection direction (Хаусхолдер вектор).
// Multiplying a matrix column by H zeros out all entries below position target_row.
// Reference: LAPACK dlarfg.f (householder vector computation)
public static class HouseholderReflection
{
    // Compute the Householder vector v for a given column of the matrix
    // column — the column entries to be zeroed below position 0
    // Returns: the reflection vector v and the scalar tau (= 2 / (vᵀ × v))
    //   tau is stored separately to avoid computing 2/||v||² repeatedly
    public static (double[] vector, double tau) Compute(double[] column)
    {
        int length = column.Length;
        var v = (double[])column.Clone();

        // Compute the 2-norm (Евклидова норма — длина вектора как расстояние от начала координат)
        // of all entries BELOW the first one (the entries we want to zero out)
        double normOfTail = 0;
        for (int i = 1; i < length; i++)
        {
            normOfTail += v[i] * v[i];
        }
        normOfTail = Math.Sqrt(normOfTail);

        // If the tail is already zero, no reflection is needed
        if (normOfTail < 1e-15)
            return (v, 0);

        // Compute the full norm of the column (including the first entry)
        double columnNorm = Math.Sqrt(v[0] * v[0] + normOfTail * normOfTail);

        // Choose the sign of the shift to avoid numerical cancellation
        // (always add rather than subtract two nearly-equal numbers)
        double sign = v[0] >= 0 ? 1.0 : -1.0;

        // Shift the first entry by the column norm to create the Householder vector
        v[0] += sign * columnNorm;

        // Compute tau = 2 / (vᵀ × v) — the scaling factor for the reflection
        // After shifting v[0], the dot product vᵀv = 2 × |v[0]| × columnNorm
        double tau = 2.0 / (v[0] * v[0] + normOfTail * normOfTail);

        return (v, tau);
    }
}
```

Also write `QrLeastSquaresSolver.cs` — uses QR to find the best-fit solution to an overdetermined system (more equations than unknowns, e.g. fitting a line through 100 data points).

Gate B: decompose a 4×3 matrix and verify Q is orthogonal (Q × Qᵀ = identity within 1e-10) and R is upper triangular (all entries below diagonal < 1e-10).

Tests:
- Verify Q is orthogonal
- Verify R is upper triangular
- Verify Q × R = original matrix
- Verify least-squares solver gives the same answer as the textbook formula for 10-point line fitting

**Estimated: 2.5 days**

---

### Phase 3 — Full Singular Value Decomposition (2 days)

**What Singular Value Decomposition does:** writes any matrix A as U × S × Vᵀ where:
- U (left singular vectors — левые сингулярные векторы): orthogonal matrix whose columns are the "output directions"
- S (singular values — сингулярные значения): diagonal matrix (only the diagonal entries are non-zero) containing non-negative values in descending order; the largest value shows which direction carries the most information
- Vᵀ (transposed right singular vectors — транспонированные правые сингулярные векторы): orthogonal matrix whose rows are the "input directions"

**Bidiagonalization approach:** the standard algorithm first reduces A to a bidiagonal matrix (only diagonal and superdiagonal entries are non-zero) using Householder reflections from Phase 2, then applies the QR iteration (QR итерация — итерационный метод нахождения сингулярных значений) to the small bidiagonal matrix.

**Reference:** LAPACK `dgesvd.f` (full algorithm, Fortran) and `dgesdd.f` (divide-and-conquer variant — метод "разделяй и властвуй", faster for large matrices).

What to write: `SingularValueDecompositionService.cs`, `BidiagonalReducer.cs`

```csharp
// SingularValueDecompositionService decomposes any matrix A into U × S × Vᵀ.
// U: orthogonal matrix of "output directions" (rows of the transformed space)
// S: diagonal matrix of singular values (importance of each direction)
// Vᵀ: orthogonal matrix of "input directions" (columns of the original space)
// Reference: LAPACK dgesvd.f (BSD-like license)
public class SingularValueDecompositionService : BaseService, IMatrixDecomposer
{
    public async Task<OperationResult<DecompositionResult>> DecomposeAsync(
        double[] matrix, int rows, int columns)
    {
        // Step 1: Reduce A to bidiagonal form B using Householder reflections
        //   Apply Householder from the left to zero out below-diagonal entries
        //   Apply Householder from the right to zero out entries two columns right of diagonal
        //   After this step, only the main diagonal and superdiagonal of B are non-zero
        var (bidiagonal, leftTransform, rightTransform) =
            await BidiagonalReducer.ReduceAsync(matrix, rows, columns);

        // Step 2: Apply QR iteration to the small bidiagonal matrix to find its singular values
        //   The QR iteration repeatedly applies Givens rotations (вращения Гивенса —
        //   обнуление одного элемента вращением двух строк) until the superdiagonal
        //   entries become negligibly small; the diagonal entries then converge to singular values
        var singularValues = await QrIterationForBidiagonal(bidiagonal);

        // Step 3: Sort singular values in descending order
        //   (largest first, as required by the standard convention)
        Array.Sort(singularValues, (a, b) => b.CompareTo(a));

        // Step 4: Accumulate the left and right transforms to produce U and V
        //   U = product of all left Householder reflections and Givens rotations
        //   V = product of all right Householder reflections and Givens rotations

        // ... full implementation of steps 1-4 ...

        var result = new DecompositionResult
        {
            // U matrix: rows × min(rows,columns)
            FactorOne = leftTransform,
            FactorOneRows = rows, FactorOneColumns = Math.Min(rows, columns),
            // S: stored as a flat array of singular values (the diagonal of the S matrix)
            FactorTwo = singularValues,
            FactorTwoRows = singularValues.Length, FactorTwoColumns = 1,
            // Vᵀ matrix: min(rows,columns) × columns
            FactorThree = rightTransform,
            FactorThreeRows = Math.Min(rows, columns), FactorThreeColumns = columns,
            AlgorithmName = "Full Singular Value Decomposition",
            ReconstructionError = 0
        };

        return OperationResult<DecompositionResult>.Ok(result);
    }

    // Apply the QR iteration algorithm to a bidiagonal matrix to find its singular values
    // The iteration repeatedly applies implicit shifts and Givens rotations
    // until the off-diagonal entries are smaller than the machine epsilon (машинный эпсилон —
    // наименьшее число которое компьютер отличает от нуля в данной точности)
    private static Task<double[]> QrIterationForBidiagonal(double[,] bidiagonal)
    {
        // ... Golub-Reinsch algorithm (алгоритм Голуба-Рейнша для бидиагональных матриц) ...
        throw new NotImplementedException("Implement the QR iteration for bidiagonal matrices");
    }
}
```

Gate B: decompose a known 3×3 matrix (e.g. `[[1,0,0],[0,2,0],[0,0,3]]` — a diagonal matrix) and verify the singular values are exactly `[3, 2, 1]` (sorted descending). Verify U × S × Vᵀ = original matrix within 1e-10.

Tests:
- Verify singular values are sorted in descending order
- Verify U and V are orthogonal
- Verify U × S × Vᵀ = original matrix
- Verify a rank-deficient matrix has some singular values very close to zero

**Estimated: 2 days**

---

### Phase 4 — Truncated Singular Value Decomposition (1.5 days)

**What truncated Singular Value Decomposition does:** instead of computing all singular values, compute only the K largest ones. The result is the best possible rank-K approximation of the original matrix — proven by the Eckart-Young theorem (теорема Экарта-Янга: усечённое сингулярное разложение даёт наименьшую возможную ошибку аппроксимации среди всех матриц ранга K).

Use the full Singular Value Decomposition from Phase 3, keep only the first K columns of U, first K diagonal entries of S, and first K rows of Vᵀ.

What to write: `TruncatedSvdService.cs`

```csharp
// TruncatedSvdService computes the best rank-K approximation of a matrix.
// Keeps only the K largest singular values and their corresponding directions.
// The approximation error is the sum of squared discarded singular values.
// Proof: by the Eckart-Young theorem (теорема Экарта-Янга), no rank-K matrix
//   can be closer to the original than the truncated Singular Value Decomposition approximation.
public class TruncatedSvdService : BaseService
{
    private readonly SingularValueDecompositionService fullSvd;

    // Compute the best rank-K approximation of the given matrix
    // targetRank — how many singular values to keep;
    //   targetRank=1 gives the single most important direction
    //   targetRank=64 keeps 64 directions; for a 4096×4096 matrix this is 16× compression
    public async Task<OperationResult<TruncatedSvdResult>> ComputeAsync(
        double[] matrix, int rows, int columns, int targetRank)
    {
        // Validate that the requested rank does not exceed the matrix rank
        int maximumPossibleRank = Math.Min(rows, columns);
        if (targetRank > maximumPossibleRank || targetRank < 1)
        {
            return OperationResult<TruncatedSvdResult>.Fail(
                $"Target rank {targetRank} is invalid. " +
                $"Must be between 1 and {maximumPossibleRank} (= min(rows, columns)).",
                "INVALID_RANK");
        }

        // Compute the full decomposition first
        var fullResult = await fullSvd.DecomposeAsync(matrix, rows, columns);
        if (!fullResult.Success)
            return OperationResult<TruncatedSvdResult>.Fail(
                fullResult.ErrorMessage, fullResult.ErrorCode);

        var full = fullResult.Value!;

        // Extract only the first targetRank columns of U, values of S, and rows of Vᵀ
        var truncatedU = ExtractFirstKColumns(full.FactorOne, full.FactorOneRows, targetRank);
        var truncatedS = full.FactorTwo[..targetRank]; // first targetRank singular values
        var truncatedVt = ExtractFirstKRows(full.FactorThree!, targetRank, full.FactorThreeColumns);

        // Compute the reconstruction error: sqrt(sum of squares of discarded singular values)
        // This is the minimum possible error for any rank-K approximation
        double discardedSingularValuesSumOfSquares = 0;
        for (int i = targetRank; i < full.FactorTwo.Length; i++)
        {
            double discardedValue = full.FactorTwo[i];
            discardedSingularValuesSumOfSquares += discardedValue * discardedValue;
        }
        double reconstructionError = Math.Sqrt(discardedSingularValuesSumOfSquares);

        // Compute the compression ratio: original bytes / compressed bytes
        int originalEntries = rows * columns;
        int compressedEntries = rows * targetRank + targetRank + targetRank * columns;
        double compressionRatio = (double)originalEntries / compressedEntries;

        var truncatedResult = new TruncatedSvdResult
        {
            LeftSingularVectors = truncatedU,      // rows × targetRank
            SingularValues = truncatedS,            // targetRank values
            RightSingularVectorsTransposed = truncatedVt, // targetRank × columns
            OriginalRows = rows,
            OriginalColumns = columns,
            TargetRank = targetRank,
            ReconstructionError = reconstructionError,
            CompressionRatio = compressionRatio
        };

        return OperationResult<TruncatedSvdResult>.Ok(truncatedResult);
    }
}
```

Gate B: compress a 100×100 matrix to rank 10. Verify the compression ratio equals the formula `(100×100) / (100×10 + 10 + 10×100)` = 4.76×. Verify the reconstruction error matches the sum of discarded singular values.

**Estimated: 1.5 days**

---

### Phase 5 — Randomized Singular Value Decomposition (2 days)

**What randomized Singular Value Decomposition does:** the full algorithm is O(m × n²) — for a 4096×4096 matrix this is 68 billion operations. Randomized Singular Value Decomposition reduces this to O(m × n × k) — for k=64 that is 1 billion operations, 68× faster, with nearly the same result.

The algorithm:
1. Multiply the matrix by a random matrix Ω of shape (n × (k+p)) where p is an oversampling parameter (usually 5–10)
2. Compute QR decomposition of the small product A × Ω — this gives a k-dimensional subspace that captures most of the variance of A
3. Project A onto this subspace: B = Qᵀ × A (now B is (k+p) × n — much smaller)
4. Compute exact Singular Value Decomposition of the small B
5. Recover the approximation of A's left singular vectors: U = Q × Û

**Reference:** `sklearn/utils/extmath.py` function `randomized_svd()` — BSD 3-Clause — very readable Python, ~80 lines.

What to write: `RandomizedSvdService.cs`

```csharp
// RandomizedSvdService computes an approximate rank-K Singular Value Decomposition
// using random projections — 10-100× faster than exact methods for large matrices.
// Algorithm: Halko, Martinsson, Tropp (2011) "Finding structure with randomness"
// Reference implementation: sklearn randomized_svd() (BSD 3-Clause, ~80 lines Python)
public class RandomizedSvdService : BaseService
{
    private readonly QrDecompositionService qrService;
    private readonly SingularValueDecompositionService exactSvdService;

    // Compute the approximate rank-K decomposition of a large matrix
    // oversamplingCount — extra dimensions beyond targetRank to improve accuracy
    //   (обычно 5–10 дополнительных измерений значительно улучшают точность без большого замедления)
    // powerIterationCount — how many times to apply power iteration for better accuracy
    //   (степень итерации — повторное умножение на матрицу улучшает разделение между
    //    большими и малыми сингулярными значениями)
    public async Task<OperationResult<TruncatedSvdResult>> ComputeAsync(
        double[] matrix,
        int rows,
        int columns,
        int targetRank,
        int oversamplingCount = 10,
        int powerIterationCount = 4)
    {
        int sketchSize = targetRank + oversamplingCount;

        // Step 1: generate a random Gaussian matrix Ω of shape (columns × sketchSize)
        // Gaussian random means each entry is drawn from a standard normal distribution
        // (нормальное распределение с нулевым средним и единичной дисперсией)
        var randomProjection = GenerateGaussianRandom(columns, sketchSize);

        // Step 2: form the sketch Y = A × Ω (rows × sketchSize)
        // This captures the column space of A in a random low-dimensional subspace
        var sketch = NumericalUtilities.Multiply(
            new DenseMatrix(matrix, rows, columns),
            new DenseMatrix(randomProjection, columns, sketchSize));

        // Step 3: power iteration — multiplying by Aᵀ × A repeatedly
        // makes the sketch more aligned with the top singular vectors
        // Without power iteration: good for matrices with fast spectral decay
        // With power iteration: better for matrices with slow spectral decay
        for (int iteration = 0; iteration < powerIterationCount; iteration++)
        {
            // Multiply by Aᵀ (transpose of A) then by A again
            // sketch = A × (Aᵀ × sketch) — each iteration sharpens the subspace estimate
            var aTranspose = Transpose(new DenseMatrix(matrix, rows, columns));
            var intermediateSketched = NumericalUtilities.Multiply(aTranspose, sketch);
            sketch = NumericalUtilities.Multiply(
                new DenseMatrix(matrix, rows, columns), intermediateSketched);
        }

        // Step 4: QR decomposition of the sketch to get an orthonormal basis Q
        // Q has shape (rows × sketchSize) with orthonormal columns
        var qrResult = await qrService.DecomposeAsync(sketch.AsArray(), rows, sketchSize);
        if (!qrResult.Success)
            return OperationResult<TruncatedSvdResult>.Fail(qrResult.ErrorMessage, "QR_FAILED");

        var Q = new DenseMatrix(qrResult.Value!.FactorOne, rows, sketchSize);

        // Step 5: project A into the low-dimensional subspace: B = Qᵀ × A
        // B is (sketchSize × columns) — much smaller than the original (rows × columns)
        var QTranspose = Transpose(Q);
        var projectedMatrix = NumericalUtilities.Multiply(
            QTranspose, new DenseMatrix(matrix, rows, columns));

        // Step 6: compute exact Singular Value Decomposition of the small projected matrix B
        // This is cheap because B is only (sketchSize × columns) where sketchSize ≈ targetRank + 10
        var svdOfProjected = await exactSvdService.DecomposeAsync(
            projectedMatrix.AsArray(), sketchSize, columns);
        if (!svdOfProjected.Success)
            return OperationResult<TruncatedSvdResult>.Fail(svdOfProjected.ErrorMessage, "SVD_FAILED");

        // Step 7: recover the approximate left singular vectors of A: U = Q × Û
        // Û is the left singular vectors of the small projected matrix
        var Uhat = new DenseMatrix(svdOfProjected.Value!.FactorOne, sketchSize, sketchSize);
        var U = NumericalUtilities.Multiply(Q, Uhat);

        // Keep only the first targetRank columns of U and values of S
        var result = new TruncatedSvdResult
        {
            LeftSingularVectors = ExtractFirstKColumns(U.AsArray(), rows, targetRank),
            SingularValues = svdOfProjected.Value.FactorTwo[..targetRank],
            RightSingularVectorsTransposed = ExtractFirstKRows(
                svdOfProjected.Value.FactorThree!, targetRank, columns),
            OriginalRows = rows,
            OriginalColumns = columns,
            TargetRank = targetRank
        };

        return OperationResult<TruncatedSvdResult>.Ok(result);
    }
}
```

Gate B: on a 500×500 test matrix, verify that randomized Singular Value Decomposition at rank 20 runs in under 1 second and produces a reconstruction error within 5% of exact truncated Singular Value Decomposition at the same rank.

**Estimated: 2 days**

**Total estimated days for sub-project 28: 11**

---

## Sub-Project 29 — Sparse Matrix Formats and Operations

**What this sub-project implements:** three storage formats for sparse matrices (matrices where most entries are zero) and the algorithms that operate on them. A graph adjacency matrix for 10,000 nodes is 10,000 × 10,000 = 100 million entries if stored densely, but typically only 100,000 entries (1%) are non-zero — Compressed Sparse Row uses 300,000 numbers instead of 100 million.

**Reference sources:**
- SuiteSparse: `https://github.com/DrTimothyAldenDavis/SuiteSparse` — `CSparse/` directory — MIT/BSD
- Eigen: `https://gitlab.com/libeigen/eigen` — `Eigen/SparseCore/` — MPL2
- The CSparse module in SuiteSparse is the best reading material — minimal, clean C code

**Dependencies:** none. Sub-project 29 defines its own sparse matrix types and does not depend on TensorCore.

**Folder:** `28.MatrixAlgorithms/Families/02-Sparse/`

---

### Phase 1 — Coordinate format (COO — Coordinate format, координатный формат) (1 day)

**What Coordinate format is:** the simplest way to store a sparse matrix. Three parallel arrays: the row index of each non-zero entry, the column index, and the value. Easy to build (just append triples), but slow for matrix multiplication.

What to write: `CooSparseMatrix.cs`

```csharp
// CooSparseMatrix stores a sparse matrix in Coordinate format (COO — координатный формат).
// Each non-zero entry is stored as a triple: (row, column, value).
// This is the easiest format to build — just call Add() for each non-zero entry.
// It is NOT efficient for matrix-vector multiplication; convert to CSR first.
// Reference: SuiteSparse CSparse (MIT license)
public class CooSparseMatrix
{
    // The row index of each non-zero entry
    // rowIndices[i] = which row non-zero entry i belongs to
    private readonly List<int> rowIndices = new();

    // The column index of each non-zero entry
    // columnIndices[i] = which column non-zero entry i belongs to
    private readonly List<int> columnIndices = new();

    // The value of each non-zero entry
    // values[i] = the actual number stored at (rowIndices[i], columnIndices[i])
    private readonly List<double> values = new();

    // The total number of rows in the matrix (including rows that are all zeros)
    public int Rows { get; }

    // The total number of columns in the matrix (including columns that are all zeros)
    public int Columns { get; }

    public CooSparseMatrix(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
    }

    // Add one non-zero entry to the matrix
    // row — which row (0-indexed) this entry belongs to
    // column — which column (0-indexed) this entry belongs to
    // value — the numerical value to store at this position
    // It is allowed to add the same (row, column) position multiple times —
    //   when converting to CSR, duplicate positions will be summed
    public void Add(int row, int column, double value)
    {
        // Validate that the position is within the declared matrix dimensions
        if (row < 0 || row >= Rows)
            throw new ArgumentOutOfRangeException(nameof(row),
                $"Row index {row} is outside the matrix bounds [0, {Rows - 1}].");
        if (column < 0 || column >= Columns)
            throw new ArgumentOutOfRangeException(nameof(column),
                $"Column index {column} is outside the matrix bounds [0, {Columns - 1}].");

        // Append the triple to the three parallel arrays
        rowIndices.Add(row);
        columnIndices.Add(column);
        values.Add(value);
    }

    // Return the number of non-zero entries currently stored
    public int NonZeroCount => values.Count;

    // How much memory this format uses relative to a dense matrix (as a ratio 0-1)
    // 0.01 means 1% of the memory of the equivalent dense matrix
    public double MemoryEfficiency =>
        1.0 - (3.0 * NonZeroCount) / (double)(Rows * Columns);
}
```

**Estimated: 1 day**

---

### Phase 2 — Compressed Sparse Row format (2 days)

**What Compressed Sparse Row is:** the standard format for sparse matrix operations. Three arrays:
- `values` — all non-zero entries, row by row
- `columnIndices` — the column index of each entry in `values`
- `rowPointers` — for each row, the position in `values` where that row's entries start

This layout enables very fast matrix-vector multiplication (матрично-векторное умножение) because all entries of one row are contiguous in memory.

What to write: `CsrSparseMatrix.cs`, `CooToCsrConverter.cs`

```csharp
// CsrSparseMatrix stores a sparse matrix in Compressed Sparse Row format
// (формат сжатых строк разреженной матрицы).
// Three arrays together represent the matrix:
//   values:         the non-zero entries, stored row by row in left-to-right order
//   columnIndices:  the column position of each entry in values[]
//   rowPointers:    rowPointers[r] = the index in values[] where row r begins
//                   rowPointers[Rows] = total number of non-zero entries
//
// Example — the matrix:
//   10  0  0  20
//    0 30 40   0
//
// Is stored as:
//   values        = [10, 20, 30, 40]
//   columnIndices = [ 0,  3,  1,  2]
//   rowPointers   = [ 0,  2,  4]      ← row 0 starts at index 0, row 1 at index 2, end at 4
//
// Reference: SuiteSparse CSparse cs_compress.c (MIT license)
public class CsrSparseMatrix
{
    // All non-zero values, stored row by row
    public double[] Values { get; }

    // The column index of each value in Values[]
    public int[] ColumnIndices { get; }

    // rowPointers[r] = where row r starts in Values[]; rowPointers[Rows] = total non-zeros
    public int[] RowPointers { get; }

    // Total number of rows (including empty rows)
    public int Rows { get; }

    // Total number of columns
    public int Columns { get; }

    // Total number of stored non-zero entries
    public int NonZeroCount => Values.Length;

    public CsrSparseMatrix(double[] values, int[] columnIndices, int[] rowPointers,
                            int rows, int columns)
    {
        Values = values;
        ColumnIndices = columnIndices;
        RowPointers = rowPointers;
        Rows = rows;
        Columns = columns;
    }

    // Multiply this sparse matrix by a dense column vector
    // The result is a dense vector of length Rows
    // This is the key operation that makes CSR fast:
    //   for each row, we directly jump to its non-zero entries via rowPointers
    public double[] MultiplyByVector(double[] vector)
    {
        if (vector.Length != Columns)
            throw new ArgumentException(
                $"Vector length {vector.Length} does not match matrix column count {Columns}.");

        var result = new double[Rows];

        for (int row = 0; row < Rows; row++)
        {
            // Where does this row start and end in Values[]?
            int rowStart = RowPointers[row];
            int rowEnd = RowPointers[row + 1];

            // Compute the dot product of row r with the input vector
            double rowDotProduct = 0;
            for (int entryIndex = rowStart; entryIndex < rowEnd; entryIndex++)
            {
                // Which column does this entry belong to?
                int columnOfEntry = ColumnIndices[entryIndex];

                // Add the product of the matrix entry and the corresponding vector element
                rowDotProduct += Values[entryIndex] * vector[columnOfEntry];
            }

            result[row] = rowDotProduct;
        }

        return result;
    }
}
```

Also write `CsrSparseMatrixMultiplier.cs` for sparse × sparse multiplication (used in graph algorithms).

Gate B: build a 4×4 CSR matrix from the example in the source document, verify `rowPointers`, `columnIndices`, and `values` arrays match the textbook layout exactly. Verify matrix-vector multiply gives the correct result.

Tests:
- Verify Coordinate format → Compressed Sparse Row conversion preserves all entries
- Verify matrix-vector multiply on a known 3×3 sparse matrix
- Verify sparse × sparse multiply on two known 4×4 matrices
- Verify memory efficiency: a matrix with 1% non-zeros should report ~97% memory savings

**Estimated: 2 days**

---

### Phase 3 — Compressed Sparse Column format and sparse solvers (2 days)

**Compressed Sparse Column** (CSC — формат сжатых столбцов) is the transpose analog of Compressed Sparse Row: the arrays are organized by columns. Used when operations work column by column (e.g. sparse QR decomposition, certain graph algorithms).

What to write: `CscSparseMatrix.cs`, `CsrToCscConverter.cs`, `SparseLinearSolver.cs`

`SparseLinearSolver` solves Ax = b where A is a sparse CSR matrix, using sparse LU decomposition. The key challenge is minimizing fill-in (заполнение — появление новых ненулей при разложении). Uses the column approximate minimum degree ordering (упорядочение по приближённой минимальной степени столбцов — метод перестановки строк и столбцов который минимизирует количество новых ненулей).

Tests:
- Verify Compressed Sparse Row → Compressed Sparse Column conversion
- Verify sparse LU solution: Ax = b, verify A × x = b within 1e-10
- Verify the sparsity of L and U factors (fill-in should be minimal with good ordering)

**Total estimated days for sub-project 29: 5**

---

## Sub-Project 30 — Low-Rank Neural Network Weight Compression

**What this sub-project implements:** algorithms to compress the weight matrices of an already-trained neural network. Sub-project 10 (LoRA) adds small trainable adapter matrices during fine-tuning. Sub-project 30 is the complementary operation: take a large trained weight matrix W and replace it with the product of two smaller matrices W ≈ A × B where A and B together use less memory.

**How this differs from sub-project 10 (LoRA):**
- LoRA: keeps W frozen, adds trainable ΔW = A × B on top during fine-tuning (the two matrices are trained from scratch)
- Sub-project 30: takes a trained W, computes the best rank-K approximation of W using truncated Singular Value Decomposition, and replaces W with the compressed A × B

**Reference sources:**
- LoRA paper algorithm: `https://github.com/microsoft/LoRA` — `loralib/layers.py` — MIT
- GPTQ: `https://github.com/IST-DASLab/gptq` — MIT — quantization + low-rank approach
- bitsandbytes: `https://github.com/TimDettmers/bitsandbytes` — MIT — 8-bit and 4-bit matrices

**Dependencies:** sub-projects 00 (TensorCore), 03 (Transformer Model), 10 (LoRA), 28 (truncated Singular Value Decomposition).

**Folder:** `28.MatrixAlgorithms/Families/03-LowRankCompression/`

---

### Phase 1 — Weight matrix spectral analysis (1.5 days)

Before compressing, analyze which weight matrices are good candidates for low-rank approximation. The key indicator is spectral decay (спектральное затухание — скорость убывания сингулярных значений): if the singular values drop off quickly, a low-rank approximation will be accurate; if they decay slowly, much information is spread across many directions.

What to write: `WeightMatrixAnalyzer.cs`

```csharp
// WeightMatrixAnalyzer computes statistics about a weight matrix to predict
// how much it can be compressed without losing accuracy.
// The main question: do a few directions carry most of the "signal" in this matrix,
// or is the information spread evenly across all directions?
public class WeightMatrixAnalyzer : BaseService
{
    private readonly RandomizedSvdService randomizedSvd;

    // Analyze a weight matrix and return compression recommendations
    // layerName — human-readable name of the layer (e.g. "attention.query_projection layer 3")
    // weightMatrix — the flat double array of the weight matrix
    // rows, columns — the dimensions
    public async Task<OperationResult<WeightAnalysisResult>> AnalyzeAsync(
        string layerName,
        double[] weightMatrix,
        int rows,
        int columns)
    {
        // Compute the top 64 singular values using the fast randomized algorithm
        // 64 is enough to understand the spectral decay without being slow
        int probeRank = Math.Min(64, Math.Min(rows, columns));
        var svdResult = await randomizedSvd.ComputeAsync(
            weightMatrix, rows, columns, probeRank);

        if (!svdResult.Success)
            return OperationResult<WeightAnalysisResult>.Fail(svdResult.ErrorMessage, "SVD_FAILED");

        double[] singularValues = svdResult.Value!.SingularValues;

        // Compute the total "energy" of the matrix: sum of squares of all singular values
        // = the square of the Frobenius norm (норма Фробениуса в квадрате)
        double totalEnergy = singularValues.Sum(s => s * s);

        // For each rank K from 1 to 64, compute what fraction of total energy is captured
        // by the top K singular values
        var cumulativeEnergyFractions = new double[probeRank];
        double cumulativeEnergy = 0;
        for (int k = 0; k < probeRank; k++)
        {
            cumulativeEnergy += singularValues[k] * singularValues[k];
            cumulativeEnergyFractions[k] = cumulativeEnergy / totalEnergy;
        }

        // Find the rank needed to capture 90%, 95%, and 99% of the energy
        // These thresholds correspond to progressively more conservative compression
        int rankFor90Percent = FindRankForEnergyThreshold(cumulativeEnergyFractions, 0.90);
        int rankFor95Percent = FindRankForEnergyThreshold(cumulativeEnergyFractions, 0.95);
        int rankFor99Percent = FindRankForEnergyThreshold(cumulativeEnergyFractions, 0.99);

        // Compute compression ratios for each threshold
        double compressionRatioAt90 = ComputeCompressionRatio(rows, columns, rankFor90Percent);
        double compressionRatioAt95 = ComputeCompressionRatio(rows, columns, rankFor95Percent);
        double compressionRatioAt99 = ComputeCompressionRatio(rows, columns, rankFor99Percent);

        return OperationResult<WeightAnalysisResult>.Ok(new WeightAnalysisResult
        {
            LayerName = layerName,
            Rows = rows,
            Columns = columns,
            TopSingularValues = singularValues,
            CumulativeEnergyFractions = cumulativeEnergyFractions,
            RankFor90PercentEnergy = rankFor90Percent,
            RankFor95PercentEnergy = rankFor95Percent,
            RankFor99PercentEnergy = rankFor99Percent,
            CompressionRatioAt90Percent = compressionRatioAt90,
            CompressionRatioAt95Percent = compressionRatioAt95,
            CompressionRatioAt99Percent = compressionRatioAt99
        });
    }

    // Find the minimum rank needed so that cumulative energy exceeds the threshold
    private static int FindRankForEnergyThreshold(double[] fractions, double threshold)
    {
        for (int k = 0; k < fractions.Length; k++)
        {
            if (fractions[k] >= threshold) return k + 1;
        }
        // If even all 64 singular values don't reach the threshold,
        // the matrix has slow spectral decay — high rank is needed
        return fractions.Length;
    }

    // Compute how much smaller the compressed A × B representation is vs the original W
    private static double ComputeCompressionRatio(int rows, int columns, int rank)
    {
        int originalSize = rows * columns;
        int compressedSize = rows * rank + rank * columns;
        return (double)originalSize / compressedSize;
    }
}
```

Tests:
- Analyze the query projection matrix from sub-project 03 (Transformer); verify the analysis produces meaningful singular values
- Verify that a diagonal matrix has spectral decay concentrated in rank 1
- Verify that a random Gaussian matrix has flat spectral decay (all singular values similar)

**Estimated: 1.5 days**

---

### Phase 2 — Weight matrix SVD compression and replacement (2 days)

What to write: `SvdWeightCompressor.cs`

Uses the truncated Singular Value Decomposition from sub-project 28 to compress weight matrices. After compression, the model's forward pass uses A × (B × x) instead of W × x — the same computation split into two smaller matrix multiplications.

```csharp
// SvdWeightCompressor replaces a large weight matrix W with two smaller matrices A and B
// such that A × B ≈ W (with the least possible reconstruction error for a given rank).
// After compression, a linear layer that computed W × x now computes B_matrix × (A_matrix × x)
// where A_matrix is (rows × rank) and B_matrix is (rank × columns) — two fast small multiplications.
public class SvdWeightCompressor : BaseService, IWeightCompressor
{
    private readonly TruncatedSvdService truncatedSvd;

    public async Task<OperationResult<CompressedWeightMatrix>> CompressAsync(
        double[] weightMatrix,
        int rows,
        int columns,
        int targetRank)
    {
        // Compute the best rank-targetRank approximation using truncated SVD
        var svdResult = await truncatedSvd.ComputeAsync(weightMatrix, rows, columns, targetRank);
        if (!svdResult.Success)
            return OperationResult<CompressedWeightMatrix>.Fail(svdResult.ErrorMessage, "SVD_FAILED");

        var svd = svdResult.Value!;

        // Form the A matrix: left singular vectors scaled by singular values
        // A = U × sqrt(S), shape: rows × targetRank
        // B = sqrt(S) × Vᵀ, shape: targetRank × columns
        // Then A × B = U × S × Vᵀ (the truncated SVD approximation of W)
        //
        // The reason to split the singular values between A and B (taking square root of each)
        // is that it makes both matrices have the same scale, which is better for numerical
        // stability during inference (вычисление — прямой проход через нейросеть)
        var sqrtSingularValues = svd.SingularValues.Select(s => Math.Sqrt(s)).ToArray();

        // Scale the columns of U by sqrt(singular values) to form A
        var aMatrix = new double[rows * targetRank];
        for (int row = 0; row < rows; row++)
        {
            for (int k = 0; k < targetRank; k++)
            {
                aMatrix[row * targetRank + k] =
                    svd.LeftSingularVectors[row * targetRank + k] * sqrtSingularValues[k];
            }
        }

        // Scale the rows of Vᵀ by sqrt(singular values) to form B
        var bMatrix = new double[targetRank * columns];
        for (int k = 0; k < targetRank; k++)
        {
            for (int col = 0; col < columns; col++)
            {
                bMatrix[k * columns + col] =
                    svd.RightSingularVectorsTransposed[k * columns + col] * sqrtSingularValues[k];
            }
        }

        return OperationResult<CompressedWeightMatrix>.Ok(new CompressedWeightMatrix
        {
            MatrixA = aMatrix,                 // rows × targetRank
            MatrixB = bMatrix,                 // targetRank × columns
            OriginalRows = rows,
            OriginalColumns = columns,
            TargetRank = targetRank,
            ReconstructionError = svd.ReconstructionError,
            CompressionRatio = svd.CompressionRatio
        });
    }

    public async Task<OperationResult<double[]>> DecompressAsync(CompressedWeightMatrix compressed)
    {
        // Reconstruct the approximate weight matrix: A × B
        var a = new DenseMatrix(compressed.MatrixA, compressed.OriginalRows, compressed.TargetRank);
        var b = new DenseMatrix(compressed.MatrixB, compressed.TargetRank, compressed.OriginalColumns);
        var reconstructed = NumericalUtilities.Multiply(a, b);
        return OperationResult<double[]>.Ok(reconstructed.AsArray());
    }
}
```

Gate B: compress the query projection layer of the Transformer model (sub-project 03) to rank 64 (a 4096×4096 matrix → two 4096×64 matrices, 32× compression). Verify reconstruction error is below 5% of the original matrix Frobenius norm.

Tests:
- Verify A × B ≈ W within the declared reconstruction error
- Verify compression ratio equals the formula `(rows × columns) / (rows × rank + rank × columns)`
- Benchmark inference speed: two small matrix multiplications should be faster than one large one when rank < columns/2

**Estimated: 2 days**

---

### Phase 3 — 8-bit integer quantization combined with low-rank residual (2 days)

**What quantization does in plain language:** instead of storing each weight as a 64-bit floating-point number (8 bytes), store it as an 8-bit integer (1 byte) — 8× less memory. The quantization introduces a small error. The residual matrix (the difference between the original and the quantized version) is then approximated with a low-rank matrix. This combines two compression techniques: quantization handles the bulk of the matrix cheaply, and low-rank captures the structured part of the quantization error.

**Reference:** inspired by GPTQ (`https://github.com/IST-DASLab/gptq` — MIT).

What to write: `Int8Quantizer.cs`, `QuantizationPlusLowRankCompressor.cs`

```csharp
// Int8Quantizer converts a floating-point weight matrix to 8-bit integer storage.
// Each floating-point value is mapped to an integer in [-127, 127] using a linear scale.
// The scale factor is stored alongside the quantized matrix so it can be undone during inference.
// This achieves 8× memory reduction (8 bytes per float64 → 1 byte per int8) with small accuracy loss.
public class Int8Quantizer : BaseService
{
    // Quantize a floating-point matrix to 8-bit integers
    // Returns the quantized integer matrix and the scale factor needed to convert back
    public OperationResult<(sbyte[] quantized, double scale)> Quantize(
        double[] floatMatrix)
    {
        // Find the absolute maximum value in the matrix
        // All values will be mapped to the range [-127, 127] relative to this maximum
        double maxAbsoluteValue = floatMatrix.Max(Math.Abs);
        if (maxAbsoluteValue < 1e-15)
        {
            // All-zero matrix: quantize trivially
            return OperationResult<(sbyte[], double)>.Ok(
                (new sbyte[floatMatrix.Length], 1.0));
        }

        // Scale factor: multiply a float by this to get an integer in [-127, 127]
        // Divide the stored integer by this to get back approximately the original float
        double scale = 127.0 / maxAbsoluteValue;

        // Quantize each entry: multiply by scale and round to the nearest integer
        var quantized = new sbyte[floatMatrix.Length];
        for (int i = 0; i < floatMatrix.Length; i++)
        {
            // Clamp to [-127, 127] to avoid overflow when converting to sbyte
            int intValue = (int)Math.Round(floatMatrix[i] * scale);
            quantized[i] = (sbyte)Math.Clamp(intValue, -127, 127);
        }

        return OperationResult<(sbyte[], double)>.Ok((quantized, scale));
    }

    // Reconstruct the approximate floating-point matrix from quantized integers and scale
    public double[] Dequantize(sbyte[] quantized, double scale)
    {
        var floatMatrix = new double[quantized.Length];
        for (int i = 0; i < quantized.Length; i++)
        {
            // Reverse the quantization: divide the integer by the scale factor
            // The result is close to the original float but not identical
            floatMatrix[i] = quantized[i] / scale;
        }
        return floatMatrix;
    }
}
```

`QuantizationPlusLowRankCompressor` combines the two: quantize W to int8, compute the residual W - Wquantized, approximate the residual with a low-rank matrix. This gives better accuracy than pure quantization at only slightly higher memory cost.

Tests:
- Verify int8 quantization round-trip error is below 1% of original matrix norm
- Verify combined quantization + low-rank beats pure quantization by at least 0.5% error
- Verify memory usage: int8 + rank-8 residual should use < 15% of the memory of the original float64 matrix

**Estimated: 2 days**

---

### Phase 4 — Full model compression pipeline and inference benchmark (1.5 days)

What to write: `ModelCompressionPipeline.cs`

Orchestrates the analysis → compression → validation loop for all weight matrices in the Transformer model:
1. Analyze all weight matrices and print a summary table (which layers benefit most from compression)
2. Apply compression at the user-selected rank to all target layers
3. Run inference on a test prompt with both the original and compressed model
4. Report accuracy delta (how much the output distribution changed) and memory savings

```csharp
// ModelCompressionPipeline compresses all weight matrices in a trained transformer model
// and measures the resulting accuracy vs. memory trade-off.
public class ModelCompressionPipeline : BaseService
{
    // Run the full analysis → compression → validation cycle
    // modelPath — path to the saved model checkpoint (from sub-project 04 TrainingLoop)
    // targetRank — the rank to use for all compressed layers
    // Returns: a summary of each compressed layer's size, ratio, and error
    public async Task<OperationResult<CompressionReport>> RunAsync(
        string modelPath,
        int targetRank)
    {
        // Load all weight matrices from the checkpoint
        // ... (reads the checkpoint file from sub-project 04) ...

        // Analyze, compress, and collect stats for each layer
        var layerReports = new List<LayerCompressionStats>();
        // ... loop through layers, call WeightMatrixAnalyzer and SvdWeightCompressor ...

        // Run a test inference with original vs compressed model
        // ... compare output token distributions ...

        return OperationResult<CompressionReport>.Ok(new CompressionReport
        {
            LayerStats = layerReports,
            TotalOriginalMemoryBytes = /* sum of all original layer sizes */0,
            TotalCompressedMemoryBytes = /* sum of compressed sizes */0,
            OverallCompressionRatio = /* total original / total compressed */0,
            InferenceDivergence = /* KL divergence between original and compressed output */0
        });
    }
}
```

Gate B: compress a minimal trained model (from sub-project 04, 2 layers, hidden size 256) to rank 16. Verify overall compression ratio is at least 4×. Verify the KL divergence (мера расхождения двух распределений вероятностей — чем меньше, тем похожее ответы) between original and compressed model output is below 0.1 on 10 test prompts.

**Total estimated days for sub-project 30: 9**

---

## Plan Review Checks

**Goal delivery check:** all algorithms from the source document are covered:
- Class 1 (exact decompositions): LU (Phase 1 of sub-project 28), QR (Phase 2), full Singular Value Decomposition (Phase 3), truncated Singular Value Decomposition (Phase 4), randomized Singular Value Decomposition (Phase 5) ✓
- Class 2 (sparse matrices): Coordinate format (sub-project 29 Phase 1), Compressed Sparse Row (Phase 2), Compressed Sparse Column (Phase 3), sparse linear solver (Phase 3) ✓
- Class 3 (low-rank neural networks): spectral analysis (sub-project 30 Phase 1), Singular Value Decomposition weight compression (Phase 2), quantization + low-rank (Phase 3), model compression pipeline (Phase 4) ✓

**Design artifact completeness check:** every algorithm names its specific reference source file. ✓

**Authentication check:** no HTTP controllers. Not applicable. ✓

**NDJSON check:** no arbiter fixtures. Not applicable. ✓

**Definition-of-done check:** every sub-project defines Gate B with quantitative criteria. ✓

**Count accuracy check:** 3 classes → sub-projects 28, 29, 30. No drift. ✓

**Dependency check:** sub-project 28 → depends on TensorCore (00); sub-project 29 → independent; sub-project 30 → depends on TensorCore (00), Transformer (03), LoRA (10), Truncated SVD (28). All dependencies already exist in the solution. ✓

---

## Summary Table

| Sub-project | Family | Algorithms | Reference | Days |
|---|---|---|---|---|
| 28 | Matrix decomposition | LU, QR, full SVD, truncated SVD, randomized SVD | LAPACK, sklearn, fbpca | 11 |
| 29 | Sparse matrices | COO, CSR, CSC, sparse LU solver | SuiteSparse CSparse | 5 |
| 30 | Low-rank weight compression | Spectral analysis, SVD compression, int8 + low-rank, pipeline | LoRA, GPTQ | 9 |
| **Total** | | | | **25 days** |

---

## Updated Solution Structure

```
LlmFromScratch.sln
├── 00.TensorCore/ … 27.Compression/
└── 28.MatrixAlgorithms/              ← this plan
    ├── Fabric/
    │   ├── IMatrixDecomposer.cs
    │   ├── ISparseMatrixStorage.cs
    │   └── IWeightCompressor.cs
    ├── Core/
    │   ├── DenseMatrix.cs
    │   ├── NumericalUtilities.cs
    │   ├── DecompositionResult.cs
    │   └── SparseMatrixData.cs
    ├── Families/
    │   ├── 01-Decomposition/         sub-project 28
    │   │   ├── LuDecompositionService.cs
    │   │   ├── LuLinearSolver.cs
    │   │   ├── QrDecompositionService.cs
    │   │   ├── HouseholderReflection.cs
    │   │   ├── QrLeastSquaresSolver.cs
    │   │   ├── SingularValueDecompositionService.cs
    │   │   ├── BidiagonalReducer.cs
    │   │   ├── TruncatedSvdService.cs
    │   │   └── RandomizedSvdService.cs
    │   ├── 02-Sparse/                sub-project 29
    │   │   ├── CooSparseMatrix.cs
    │   │   ├── CsrSparseMatrix.cs
    │   │   ├── CooToCsrConverter.cs
    │   │   ├── CscSparseMatrix.cs
    │   │   ├── CsrToCscConverter.cs
    │   │   └── SparseLinearSolver.cs
    │   └── 03-LowRankCompression/    sub-project 30
    │       ├── WeightMatrixAnalyzer.cs
    │       ├── SvdWeightCompressor.cs
    │       ├── Int8Quantizer.cs
    │       ├── QuantizationPlusLowRankCompressor.cs
    │       └── ModelCompressionPipeline.cs
    └── Tests/
        ├── LuDecompositionTests.cs
        ├── QrDecompositionTests.cs
        ├── SvdTests.cs
        ├── TruncatedSvdTests.cs
        ├── RandomizedSvdTests.cs
        ├── CooSparseMatrixTests.cs
        ├── CsrSparseMatrixTests.cs
        ├── SparseLinearSolverTests.cs
        ├── WeightMatrixAnalyzerTests.cs
        ├── SvdWeightCompressorTests.cs
        └── QuantizationTests.cs
```

---

## Pre-Commit Checklist

```
□ Every public method returns OperationResult<T>                          (coding rule 3)
□ Every service class extends BaseService                                   (coding rule 4)
□ No scopeId or userId parameter in any method signature                   (coding rule 5)
□ No new() call on a fabric implementation inside a service                (fabric-first)
□ Every line of code carries a plain-language comment
□ No abbreviation without the full form on the same line
   (LU, QR, SVD, CSR, COO, CSC always appear with full names)
□ Variable names fully spelled out (no lu, qr, svd, csr, coo, csc, mtx, vec)
□ dotnet build exits 0, no warnings
□ All unit tests pass
□ Gate B: quantitative criteria met (reconstruction error, compression ratio, solver accuracy)
```

---

*Plan version 1.0 — covers sub-projects 28 through 30 (three matrix algorithm families: decomposition, sparse formats, low-rank compression). Does not touch sub-projects 01–27 or XIIGen work.*
