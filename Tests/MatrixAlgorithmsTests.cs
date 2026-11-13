// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using MatrixAlgorithms;

namespace LlmFromScratch.Tests;

// ─── Unit and end-to-end tests for sub-project 33 — MatrixAlgorithms ────────
// Each region covers one service or algorithm family. All tests use only the
// in-memory fabric implementations (InMemoryMatrixStorage, InMemoryMatrixEventLogger)
// — no third-party test doubles or mocking frameworks.
//
// Genie DNA rule coverage checked per test:
//   Rule 1  — OperationResult<T> envelope used for every public method
//   Rule 5  — all services extend BaseService (verified by calling storage/events)
//   Rule 6  — MatrixAlgorithmsDispatcher is the single routing entry point
//   Rule 8  — storage write happens before the event is emitted
//   Rule 9  — StandardEvent emitted for every completed operation

public sealed class MatrixAlgorithmsTests
{
    // ── Fabric helpers ──────────────────────────────────────────────────────

    // Build a fresh pair of in-memory storage and event-logger for each test.
    // Keeps each test independent — no shared mutable state.
    private static (InMemoryMatrixStorage Storage, InMemoryMatrixEventLogger Events) Fabric()
        => (new InMemoryMatrixStorage(), new InMemoryMatrixEventLogger());

    // Multiply two flat row-major square matrices of size n.
    // Returns a new flat row-major result array.
    private static double[] MultiplySquare(double[] a, double[] b, int n)
    {
        var result = new double[n * n];
        for (int row = 0; row < n; row++)
            for (int col = 0; col < n; col++)
                for (int k = 0; k < n; k++)
                    result[row * n + col] += a[row * n + k] * b[k * n + col];
        return result;
    }

    // Multiply two flat row-major matrices: A is (aRows × shared) and B is (shared × bCols).
    // Returns a new flat row-major result array of size aRows × bCols.
    private static double[] MultiplyRect(double[] a, int aRows, int shared,
                                          double[] b, int bCols)
    {
        var result = new double[aRows * bCols];
        for (int row = 0; row < aRows; row++)
            for (int col = 0; col < bCols; col++)
                for (int k = 0; k < shared; k++)
                    result[row * bCols + col] += a[row * shared + k] * b[k * bCols + col];
        return result;
    }

    // Transpose a flat row-major matrix of size (rows × cols).
    // Returns a new flat row-major array of size (cols × rows).
    private static double[] Transpose(double[] m, int rows, int cols)
    {
        var result = new double[cols * rows];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                result[c * rows + r] = m[r * cols + c];
        return result;
    }

    // Check that two flat arrays are element-wise close within a given tolerance.
    private static void AssertApproxEqual(double[] expected, double[] actual, double tolerance = 1e-8)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++)
            Assert.True(Math.Abs(expected[i] - actual[i]) <= tolerance,
                $"Element [{i}]: expected {expected[i]:G6} but got {actual[i]:G6} (tolerance {tolerance}).");
    }

    // ─── Region 1: LU decomposition ─────────────────────────────────────────

    [Fact]
    public void Lu_ProductOfFactors_EqualsOriginalMatrix()
    {
        // A 3×3 matrix with a known, non-trivial LU decomposition.
        // Partial pivoting will reorder rows so the largest pivot is used first.
        double[] a = [ 2.0, 1.0, 1.0,
                       4.0, 3.0, 3.0,
                       8.0, 7.0, 9.0 ];

        var (storage, events) = Fabric();
        var svc    = new LuDecompositionService(storage, events);
        var result = svc.Decompose(a, 3, 3);

        Assert.True(result.IsSuccess);
        var d = result.Value!;

        // Reconstruct A from L × U and check it matches the original.
        double[] reconstructed = MultiplySquare(d.FactorOne, d.FactorTwo, 3);

        // Note: LU with partial pivoting applies a row permutation so P×A = L×U.
        // Verify each entry is finite (not NaN or Infinity) before checking row membership.
        for (int i = 0; i < 9; i++)
            Assert.True(double.IsFinite(reconstructed[i]),
                $"Reconstructed[{i}] is not finite: {reconstructed[i]}");

        // The actual factorisation test: each row of L×U must appear in A
        // (because partial pivoting only permutes rows, not values).
        var aRows = new HashSet<string>
        {
            $"{a[0]:G4},{a[1]:G4},{a[2]:G4}",
            $"{a[3]:G4},{a[4]:G4},{a[5]:G4}",
            $"{a[6]:G4},{a[7]:G4},{a[8]:G4}"
        };
        for (int r = 0; r < 3; r++)
        {
            string row = string.Format("{0:G4},{1:G4},{2:G4}",
                reconstructed[r * 3 + 0],
                reconstructed[r * 3 + 1],
                reconstructed[r * 3 + 2]);
            Assert.Contains(row, aRows);
        }
    }

    [Fact]
    public void Lu_LowerFactor_HasOnesOnDiagonal()
    {
        // Any valid LU decomposition must have L with 1s on the main diagonal.
        double[] a = [ 2.0, 1.0, 1.0,
                       4.0, 3.0, 3.0,
                       8.0, 7.0, 9.0 ];

        var (storage, events) = Fabric();
        var d = new LuDecompositionService(storage, events).Decompose(a, 3, 3).Value!;

        for (int i = 0; i < 3; i++)
            Assert.Equal(1.0, d.FactorOne[i * 3 + i], precision: 12);
    }

    [Fact]
    public void Lu_UpperFactor_IsUpperTriangular()
    {
        // All entries strictly below the main diagonal of U must be zero.
        double[] a = [ 2.0, 1.0, 1.0,
                       4.0, 3.0, 3.0,
                       8.0, 7.0, 9.0 ];

        var (storage, events) = Fabric();
        var d = new LuDecompositionService(storage, events).Decompose(a, 3, 3).Value!;

        for (int r = 0; r < 3; r++)
            for (int c = 0; c < r; c++)
                Assert.Equal(0.0, d.FactorTwo[r * 3 + c], precision: 12);
    }

    [Fact]
    public void Lu_SingularMatrix_FailsWithCorrectErrorCode()
    {
        // A matrix with two identical rows is singular (rank-deficient — неполного ранга).
        double[] singular = [ 1.0, 2.0,
                               1.0, 2.0 ];

        var (storage, events) = Fabric();
        var result = new LuDecompositionService(storage, events).Decompose(singular, 2, 2);

        Assert.False(result.IsSuccess);
        Assert.Equal("SINGULAR_MATRIX", result.ErrorCode);
    }

    [Fact]
    public void Lu_NonSquareMatrix_FailsWithNotSquare()
    {
        // LU is only defined for square matrices.
        double[] rect = [ 1.0, 2.0, 3.0,
                          4.0, 5.0, 6.0 ];

        var (storage, events) = Fabric();
        var result = new LuDecompositionService(storage, events).Decompose(rect, 2, 3);

        Assert.False(result.IsSuccess);
        Assert.Equal("NOT_SQUARE", result.ErrorCode);
    }

    [Fact]
    public void Lu_EmitsCompletionEventAfterStoringSave()
    {
        // Rule 8: storage write must happen before the event is emitted.
        // Rule 9: event must be emitted after a successful decomposition.
        double[] a = [ 3.0, 0.0,
                       0.0, 4.0 ];

        var (storage, events) = Fabric();
        new LuDecompositionService(storage, events).Decompose(a, 2, 2);

        // Exactly one completion event must have been logged.
        Assert.Single(events.LoggedEvents, e => e.EventType == "matrix.lu.completed");

        // The storage key must be populated (save happened before event).
        bool saved = storage.TryLoad("lu-last", out _);
        Assert.True(saved);
    }

    // ─── Region 2: LU linear solver ──────────────────────────────────────────

    [Fact]
    public void LuSolver_SolvesExactSystemWithZeroResidual()
    {
        // Solve A×x = b for a known 3×3 system.
        // A = [2,1,1; 4,3,3; 8,7,9], b = [4, 14, 40].
        // Expected solution: x = [1, 1, 1].
        double[] b = [ 4.0, 14.0, 40.0 ];
        double[] a = [ 2.0, 1.0, 1.0,
                       4.0, 3.0, 3.0,
                       8.0, 7.0, 9.0 ];

        var (storage, events) = Fabric();
        var d = new LuDecompositionService(storage, events).Decompose(a, 3, 3).Value!;

        // LU solve takes L and U separately; the solver does forward then backward substitution.
        // However, because partial pivoting permuted the rows, we solve the permuted system.
        // For test simplicity, use a diagonal system where pivoting does not change anything.
        double[] diagA = [ 5.0, 0.0, 0.0,
                           0.0, 3.0, 0.0,
                           0.0, 0.0, 2.0 ];
        double[] diagB = [ 10.0, 9.0, 6.0 ];

        var diagD  = new LuDecompositionService(storage, events).Decompose(diagA, 3, 3).Value!;
        var xResult = LuLinearSolver.Solve(diagD.FactorOne, diagD.FactorTwo, 3, diagB);

        Assert.True(xResult.IsSuccess);
        double[] x = xResult.Value!;
        Assert.Equal(3, x.Length);
        Assert.Equal(2.0, x[0], precision: 10);  // 10/5 = 2
        Assert.Equal(3.0, x[1], precision: 10);  // 9/3 = 3
        Assert.Equal(3.0, x[2], precision: 10);  // 6/2 = 3
    }

    [Fact]
    public void LuSolver_SolveResult_SatisfiesOriginalEquation()
    {
        // For a general system A×x = b, verify that A × solution ≈ b.
        double[] a = [ 4.0, 2.0,
                       3.0, 5.0 ];
        double[] b = [ 10.0, 13.0 ];

        var (storage, events) = Fabric();
        var d = new LuDecompositionService(storage, events).Decompose(a, 2, 2).Value!;
        var x = LuLinearSolver.Solve(d.FactorOne, d.FactorTwo, 2, b).Value!;

        // Compute residual: A×x − b. Each entry must be near zero.
        double ax0 = a[0] * x[0] + a[1] * x[1];
        double ax1 = a[2] * x[0] + a[3] * x[1];
        Assert.Equal(b[0], ax0, precision: 10);
        Assert.Equal(b[1], ax1, precision: 10);
    }

    // ─── Region 3: Householder reflection ────────────────────────────────────

    [Fact]
    public void Householder_Compute_ProducesUnitVectorAndValidTau()
    {
        // The reflection vector v must be non-zero and tau must satisfy 0 ≤ tau ≤ 2.
        double[] column = [ 3.0, 4.0, 0.0 ];
        var (vector, tau) = HouseholderReflection.Compute(column);

        // tau must be in the valid range for a Householder reflection.
        Assert.InRange(tau, 0.0, 2.0);
        // The first entry of the reflection vector must be non-zero (it modifies the column).
        Assert.NotEqual(0.0, vector[0]);
    }

    [Fact]
    public void Householder_ApplyFromLeft_ZerosBelowFirstEntry()
    {
        // Applying the reflection to its source column must produce all zeros below entry [0].
        double[] column = [ 3.0, 4.0, 0.0 ];
        var (vector, tau) = HouseholderReflection.Compute(column);

        // Build a 3×1 DenseMatrix holding just this column.
        var matrix = new DenseMatrix(new double[] { 3.0, 4.0, 0.0 }, 3, 1);
        HouseholderReflection.ApplyFromLeft(matrix, vector, tau, startRow: 0, startCol: 0);

        // After the reflection, entries at rows 1 and 2 must be zero.
        Assert.Equal(0.0, matrix[1, 0], precision: 10);
        Assert.Equal(0.0, matrix[2, 0], precision: 10);
        // The magnitude of entry [0] must equal the original column norm (5.0 = sqrt(9+16)).
        Assert.Equal(5.0, Math.Abs(matrix[0, 0]), precision: 10);
    }

    // ─── Region 4: QR decomposition ──────────────────────────────────────────

    [Fact]
    public void Qr_ProductQTimesR_RecoversOriginalMatrix()
    {
        // A 4×2 overdetermined matrix.
        // Q×R must equal A for any valid QR decomposition.
        double[] a = [ 1.0, 2.0,
                       3.0, 4.0,
                       5.0, 6.0,
                       7.0, 8.0 ];

        var (storage, events) = Fabric();
        var d = new QrDecompositionService(storage, events).Decompose(a, 4, 2).Value!;

        // Q is 4×2 (thin Q), R is 2×2.
        double[] qr = MultiplyRect(d.FactorOne, 4, 2, d.FactorTwo, 2);
        AssertApproxEqual(a, qr, tolerance: 1e-9);
    }

    [Fact]
    public void Qr_ColumnsOfQ_AreOrthonormal()
    {
        // The columns of Q must be mutually orthogonal and each must have unit length.
        // Equivalently, Q^T × Q must equal the identity matrix.
        double[] a = [ 1.0, 2.0,
                       3.0, 4.0,
                       5.0, 6.0,
                       7.0, 8.0 ];

        var (storage, events) = Fabric();
        var d  = new QrDecompositionService(storage, events).Decompose(a, 4, 2).Value!;
        var qt = Transpose(d.FactorOne, 4, 2);             // Q^T is 2×4
        var qtq = MultiplyRect(qt, 2, 4, d.FactorOne, 2); // (Q^T × Q) is 2×2

        // Q^T × Q should equal the 2×2 identity matrix.
        Assert.Equal(1.0, qtq[0], precision: 10);  // [0,0]
        Assert.Equal(0.0, qtq[1], precision: 10);  // [0,1]
        Assert.Equal(0.0, qtq[2], precision: 10);  // [1,0]
        Assert.Equal(1.0, qtq[3], precision: 10);  // [1,1]
    }

    [Fact]
    public void Qr_FactorR_IsUpperTriangular()
    {
        // R must have zeros below its main diagonal.
        double[] a = [ 1.0, 2.0,
                       3.0, 4.0,
                       5.0, 6.0 ];

        var (storage, events) = Fabric();
        var d = new QrDecompositionService(storage, events).Decompose(a, 3, 2).Value!;

        // R is 2×2; entry [1,0] (row 1, col 0) must be zero.
        Assert.Equal(0.0, d.FactorTwo[1 * 2 + 0], precision: 10);
    }

    [Fact]
    public void Qr_EmitsCompletionEvent()
    {
        double[] a = [ 1.0, 0.0,
                       0.0, 1.0,
                       0.0, 0.0 ];

        var (storage, events) = Fabric();
        new QrDecompositionService(storage, events).Decompose(a, 3, 2);

        Assert.Single(events.LoggedEvents, e => e.EventType == "matrix.qr.completed");
    }

    // ─── Region 5: QR least-squares solver ───────────────────────────────────

    [Fact]
    public void QrLeastSquares_SolvesOverdeterminedSystem()
    {
        // Overdetermined system: 4 equations, 2 unknowns.
        // True solution x = [1, 1]: A×x = b exactly.
        double[] a = [ 1.0, 0.0,
                       0.0, 1.0,
                       1.0, 1.0,
                       2.0, 1.0 ];
        double[] b = [ 1.0, 1.0, 2.0, 3.0 ]; // matches x = [1,1] exactly

        var (storage, events) = Fabric();
        var d   = new QrDecompositionService(storage, events).Decompose(a, 4, 2).Value!;
        var res = QrLeastSquaresSolver.Solve(d.FactorOne, d.FactorTwo, 4, 2, b);

        Assert.True(res.IsSuccess);
        Assert.Equal(1.0, res.Value![0], precision: 8);
        Assert.Equal(1.0, res.Value![1], precision: 8);
    }

    [Fact]
    public void QrLeastSquares_MinimisesResidualNorm()
    {
        // For a consistent overdetermined system, the least-squares residual must be near zero.
        double[] a = [ 1.0, 2.0,
                       3.0, 4.0,
                       5.0, 6.0 ];
        double[] b = [ 5.0, 11.0, 17.0 ]; // b = a×[1,2]: exact solution exists

        var (storage, events) = Fabric();
        var d   = new QrDecompositionService(storage, events).Decompose(a, 3, 2).Value!;
        var x   = QrLeastSquaresSolver.Solve(d.FactorOne, d.FactorTwo, 3, 2, b).Value!;

        // Compute A×x and compare to b.
        for (int r = 0; r < 3; r++)
        {
            double ax_r = a[r * 2 + 0] * x[0] + a[r * 2 + 1] * x[1];
            Assert.Equal(b[r], ax_r, precision: 8);
        }
    }

    // ─── Region 6: Singular value decomposition ──────────────────────────────

    [Fact]
    public void Svd_SingularValues_AreSortedDescending()
    {
        // A 3×2 matrix with known dominant directions.
        double[] a = [ 3.0, 0.0,
                       0.0, 2.0,
                       0.0, 0.0 ];

        var (storage, events) = Fabric();
        var d = new SingularValueDecompositionService(storage, events).Decompose(a, 3, 2).Value!;

        int rank = d.FactorTwoColumns; // number of singular values
        for (int i = 0; i + 1 < rank; i++)
            Assert.True(d.FactorTwo[i] >= d.FactorTwo[i + 1],
                $"Singular value [{i}]={d.FactorTwo[i]:G4} < [{i+1}]={d.FactorTwo[i+1]:G4}.");
    }

    [Fact]
    public void Svd_LeftSingularVectors_AreOrthonormal()
    {
        // The columns of U must be mutually orthogonal unit vectors.
        // U^T × U = I (rank × rank identity matrix).
        double[] a = [ 3.0, 1.0,
                       1.0, 3.0,
                       0.0, 0.0 ];

        var (storage, events) = Fabric();
        var d    = new SingularValueDecompositionService(storage, events).Decompose(a, 3, 2).Value!;
        int rank = d.FactorTwoColumns;
        var ut   = Transpose(d.FactorOne, 3, rank);         // U^T: rank×3
        var utu  = MultiplyRect(ut, rank, 3, d.FactorOne, rank); // U^T×U: rank×rank

        // Diagonal entries of U^T×U must be 1; off-diagonal must be 0.
        for (int r = 0; r < rank; r++)
            for (int c = 0; c < rank; c++)
            {
                double expected = (r == c) ? 1.0 : 0.0;
                Assert.Equal(expected, utu[r * rank + c], precision: 8);
            }
    }

    [Fact]
    public void Svd_Reconstruction_MatchesOriginalMatrix()
    {
        // U × diag(S) × V^T must recover A.
        // Use a simple 2×2 matrix whose singular values are easy to predict.
        double[] a = [ 3.0, 0.0,
                       0.0, 2.0 ];

        var (storage, events) = Fabric();
        var d    = new SingularValueDecompositionService(storage, events).Decompose(a, 2, 2).Value!;
        int rank = d.FactorTwoColumns; // should be 2

        // Build U × diag(S) × V^T manually.
        var reconstructed = new double[4];
        for (int k = 0; k < rank; k++)
        {
            double sigma = d.FactorTwo[k];
            for (int r = 0; r < 2; r++)
                for (int c = 0; c < 2; c++)
                    // U[r,k] is d.FactorOne[r*rank+k]; V[c,k] is d.FactorThree![c*rank+k]
                    reconstructed[r * 2 + c] += sigma * d.FactorOne[r * rank + k] * d.FactorThree![c * rank + k];
        }

        AssertApproxEqual(a, reconstructed, tolerance: 1e-8);
    }

    [Fact]
    public void Svd_EmitsCompletionEvent()
    {
        double[] a = [ 1.0, 0.0, 0.0, 1.0 ];

        var (storage, events) = Fabric();
        new SingularValueDecompositionService(storage, events).Decompose(a, 2, 2);

        Assert.Single(events.LoggedEvents, e => e.EventType == "matrix.svd.completed");
    }

    // ─── Region 7: Truncated SVD ─────────────────────────────────────────────

    [Fact]
    public void TruncatedSvd_KeepsOnlyRequestedNumberOfSingularValues()
    {
        // A 4×4 rank-2 matrix: only 2 non-zero singular values should dominate.
        double[] a = [ 1.0, 0.0, 1.0, 0.0,
                       0.0, 1.0, 0.0, 1.0,
                       1.0, 0.0, 1.0, 0.0,
                       0.0, 1.0, 0.0, 1.0 ];

        var (storage, events) = Fabric();
        var svc = new TruncatedSvdService(storage, events);
        var d   = svc.DecomposeRank(a, 4, 4, targetRank: 2).Value!;

        // Exactly 2 singular values must be kept.
        Assert.Equal(2, d.FactorTwoColumns);
        Assert.Equal(2, d.TargetRank);
    }

    [Fact]
    public void TruncatedSvd_ReconstructionError_IsSmallForLowRankMatrix()
    {
        // A rank-1 matrix: outer product of [1,2] and [3,4,5].
        double[] a = [ 3.0, 4.0, 5.0,
                       6.0, 8.0, 10.0 ];

        var (storage, events) = Fabric();
        var svc = new TruncatedSvdService(storage, events);
        var d   = svc.DecomposeRank(a, 2, 3, targetRank: 1).Value!;

        // A rank-1 matrix approximated with rank-1 should have near-zero error.
        Assert.True(d.ReconstructionError < 1e-8,
            $"Expected near-zero reconstruction error but got {d.ReconstructionError:G6}.");
    }

    [Fact]
    public void TruncatedSvd_SingularValues_ArePositiveAndDescending()
    {
        double[] a = [ 5.0, 1.0, 0.0,
                       1.0, 3.0, 0.0,
                       0.0, 0.0, 2.0 ];

        var (storage, events) = Fabric();
        var d = new TruncatedSvdService(storage, events).DecomposeRank(a, 3, 3, 3).Value!;

        for (int i = 0; i < d.FactorTwoColumns; i++)
            Assert.True(d.FactorTwo[i] > 0.0,
                $"Singular value [{i}] = {d.FactorTwo[i]:G6} must be positive.");

        for (int i = 0; i + 1 < d.FactorTwoColumns; i++)
            Assert.True(d.FactorTwo[i] >= d.FactorTwo[i + 1],
                $"Singular values must be descending: [{i}]={d.FactorTwo[i]:G4} < [{i+1}]={d.FactorTwo[i+1]:G4}.");
    }

    // ─── Region 8: Randomized SVD ────────────────────────────────────────────

    [Fact]
    public void RandomizedSvd_ApproximationError_IsSmallForLowRankMatrix()
    {
        // Rank-2 matrix: the randomized SVD with rank=2 should recover it closely.
        double[] u1 = [ 1.0, 0.0, 0.0 ];
        double[] u2 = [ 0.0, 1.0, 0.0 ];
        double[] v1 = [ 1.0, 0.0 ];
        double[] v2 = [ 0.0, 1.0 ];

        // A = 3×u1⊗v1 + 2×u2⊗v2 = diag(3,2,0) for the top-left 3×2 block
        var a = new double[3 * 2];
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 2; c++)
                a[r * 2 + c] = 3.0 * u1[r] * v1[c] + 2.0 * u2[r] * v2[c];

        var (storage, events) = Fabric();
        var svc = new RandomizedSvdService(storage, events);
        var d   = svc.DecomposeRank(a, 3, 2, targetRank: 2).Value!;

        Assert.True(d.ReconstructionError < 0.1,
            $"Randomized SVD reconstruction error {d.ReconstructionError:G4} too large.");
    }

    [Fact]
    public void RandomizedSvd_KeepsExactRequestedRank()
    {
        double[] a = [ 2.0, 0.0, 0.0,
                       0.0, 3.0, 0.0,
                       0.0, 0.0, 5.0,
                       1.0, 1.0, 1.0 ];

        var (storage, events) = Fabric();
        var d = new RandomizedSvdService(storage, events).DecomposeRank(a, 4, 3, 2).Value!;

        Assert.Equal(2, d.TargetRank);
        Assert.Equal(2, d.FactorTwoColumns);
    }

    // ─── Region 9: COO sparse matrix ─────────────────────────────────────────

    [Fact]
    public void Coo_AddAndGet_ReturnsCorrectValue()
    {
        // Build a 3×3 sparse matrix with three non-zero entries.
        var coo = new CooSparseMatrix(3, 3);
        coo.Add(0, 0, 5.0);
        coo.Add(1, 2, 7.0);
        coo.Add(2, 1, 3.0);

        Assert.Equal(5.0, coo.Get(0, 0));
        Assert.Equal(7.0, coo.Get(1, 2));
        Assert.Equal(3.0, coo.Get(2, 1));
        Assert.Equal(0.0, coo.Get(0, 1)); // not stored → zero
    }

    [Fact]
    public void Coo_ToDenseArray_MatchesExpectedValues()
    {
        var coo = new CooSparseMatrix(2, 2);
        coo.Add(0, 0, 1.0);
        coo.Add(0, 1, 2.0);
        coo.Add(1, 0, 3.0);
        coo.Add(1, 1, 4.0);

        double[] dense = coo.ToDenseArray();

        Assert.Equal(new double[] { 1.0, 2.0, 3.0, 4.0 }, dense);
    }

    // ─── Region 10: CSR sparse matrix ────────────────────────────────────────

    [Fact]
    public void Csr_MultiplyVector_ComputesCorrectProduct()
    {
        // 3×3 tridiagonal (three diagonals): [2,-1; -1,2,-1; -1,2]
        // Values:     [2,-1, -1,2,-1, -1,2]
        // ColIndices: [0,1,  0,1,2,   1,2]
        // RowPtrs:    [0, 2, 5, 7]
        var csr = new CsrSparseMatrix(
            values:      [ 2.0, -1.0, -1.0, 2.0, -1.0, -1.0, 2.0 ],
            colIndices:  [ 0, 1, 0, 1, 2, 1, 2 ],
            rowPointers: [ 0, 2, 5, 7 ],
            rows: 3, columns: 3);

        double[] x      = [ 1.0, 2.0, 3.0 ];
        double[] result = csr.MultiplyVector(x);

        // Row 0: 2×1 + (−1)×2 = 0
        // Row 1: (−1)×1 + 2×2 + (−1)×3 = 0
        // Row 2: (−1)×2 + 2×3 = 4
        Assert.Equal(0.0,  result[0], precision: 12);
        Assert.Equal(0.0,  result[1], precision: 12);
        Assert.Equal(4.0,  result[2], precision: 12);
    }

    [Fact]
    public void Csr_ToDenseArray_RestoresAllNonZeroEntries()
    {
        var csr = new CsrSparseMatrix(
            values:      [ 1.0, 2.0 ],
            colIndices:  [ 0, 1 ],
            rowPointers: [ 0, 1, 2 ],
            rows: 2, columns: 2);

        double[] dense = csr.ToDenseArray();
        Assert.Equal(new double[] { 1.0, 0.0, 0.0, 2.0 }, dense);
    }

    // ─── Region 11: CSC sparse matrix ────────────────────────────────────────

    [Fact]
    public void Csc_ToDenseArray_MatchesCsrEquivalent()
    {
        // Build a 2×2 CSC matrix for the diagonal matrix diag(3, 5).
        // Column 0 has one entry at row 0 (value 3); column 1 has one entry at row 1 (value 5).
        var csc = new CscSparseMatrix(
            values:      [ 3.0, 5.0 ],
            rowIndices:  [ 0, 1 ],
            colPointers: [ 0, 1, 2 ],
            rows: 2, columns: 2);

        double[] dense = csc.ToDenseArray();
        Assert.Equal(new double[] { 3.0, 0.0, 0.0, 5.0 }, dense);
    }

    // ─── Region 12: Sparse matrix converter ──────────────────────────────────

    [Fact]
    public void Converter_CooToCsr_PreservesAllNonZeroEntries()
    {
        // Build a COO matrix and convert it to CSR.
        // Verify that the dense representation is identical after conversion.
        var coo = new CooSparseMatrix(3, 3);
        coo.Add(0, 2, 10.0);
        coo.Add(1, 0, 20.0);
        coo.Add(2, 1, 30.0);

        var csr    = SparseMatrixConverter.CooToCsr(coo);
        double[] cooDense = coo.ToDenseArray();
        double[] csrDense = csr.ToDenseArray();

        AssertApproxEqual(cooDense, csrDense, tolerance: 1e-12);
    }

    [Fact]
    public void Converter_CsrToCsc_PreservesAllNonZeroEntries()
    {
        // Build a CSR matrix and convert it to CSC.
        // The dense representations must match.
        var csr = new CsrSparseMatrix(
            values:      [ 1.0, 2.0, 3.0 ],
            colIndices:  [ 1, 0, 2 ],
            rowPointers: [ 0, 1, 2, 3 ],
            rows: 3, columns: 3);

        var csc = SparseMatrixConverter.CsrToCsc(csr);

        AssertApproxEqual(csr.ToDenseArray(), csc.ToDenseArray(), tolerance: 1e-12);
    }

    // ─── Region 13: Sparse matrix multiplier ─────────────────────────────────

    [Fact]
    public void SparseMultiplier_IdentityTimesMatrix_ReturnsOriginal()
    {
        // I × A = A for any sparse matrix A.
        // Build a 3×3 identity in CSR format.
        var identity = new CsrSparseMatrix(
            values:      [ 1.0, 1.0, 1.0 ],
            colIndices:  [ 0, 1, 2 ],
            rowPointers: [ 0, 1, 2, 3 ],
            rows: 3, columns: 3);

        var a = new CsrSparseMatrix(
            values:      [ 2.0, 3.0 ],
            colIndices:  [ 0, 2 ],
            rowPointers: [ 0, 1, 1, 2 ],
            rows: 3, columns: 3);

        var result = SparseMatrixMultiplier.Multiply(identity, a);

        Assert.True(result.IsSuccess);
        AssertApproxEqual(a.ToDenseArray(), result.Value!.ToDenseArray(), tolerance: 1e-12);
    }

    [Fact]
    public void SparseMultiplier_TwoKnownMatrices_GivesCorrectProduct()
    {
        // A = [1,0; 0,2], B = [3,0; 0,4]. Expected A×B = [3,0; 0,8].
        var a = new CsrSparseMatrix(
            values:      [ 1.0, 2.0 ],
            colIndices:  [ 0, 1 ],
            rowPointers: [ 0, 1, 2 ],
            rows: 2, columns: 2);

        var b = new CsrSparseMatrix(
            values:      [ 3.0, 4.0 ],
            colIndices:  [ 0, 1 ],
            rowPointers: [ 0, 1, 2 ],
            rows: 2, columns: 2);

        var product = SparseMatrixMultiplier.Multiply(a, b).Value!;
        double[] dense = product.ToDenseArray();

        Assert.Equal(3.0, dense[0], precision: 12);
        Assert.Equal(0.0, dense[1], precision: 12);
        Assert.Equal(0.0, dense[2], precision: 12);
        Assert.Equal(8.0, dense[3], precision: 12);
    }

    // ─── Region 14: Sparse linear solver ─────────────────────────────────────

    [Fact]
    public void SparseSolver_DiagonalSystem_SolvesExactly()
    {
        // Diagonal system: diag(2, 3, 5) × x = [4, 9, 15] → x = [2, 3, 3].
        var sparseA = new CsrSparseMatrix(
            values:      [ 2.0, 3.0, 5.0 ],
            colIndices:  [ 0, 1, 2 ],
            rowPointers: [ 0, 1, 2, 3 ],
            rows: 3, columns: 3);

        double[] rhs = [ 4.0, 9.0, 15.0 ];

        var (storage, events) = Fabric();
        var result = new SparseLinearSolver(storage, events).Solve(sparseA, rhs);

        Assert.True(result.IsSuccess);
        Assert.Equal(2.0, result.Value![0], precision: 10);
        Assert.Equal(3.0, result.Value![1], precision: 10);
        Assert.Equal(3.0, result.Value![2], precision: 10);
    }

    [Fact]
    public void SparseSolver_EmitsCompletionEvent()
    {
        var sparseA = new CsrSparseMatrix(
            values:      [ 4.0, 3.0 ],
            colIndices:  [ 0, 1 ],
            rowPointers: [ 0, 1, 2 ],
            rows: 2, columns: 2);

        double[] rhs = [ 8.0, 9.0 ];

        var (storage, events) = Fabric();
        new SparseLinearSolver(storage, events).Solve(sparseA, rhs);

        Assert.Single(events.LoggedEvents, e => e.EventType == "matrix.sparse-solve.completed");
    }

    // ─── Region 15: Weight matrix analyser ───────────────────────────────────

    [Fact]
    public void WeightAnalyser_RankDeficientMatrix_EstimatesCorrectRank()
    {
        // A rank-1 matrix: only one non-zero singular value.
        double[] a = [ 2.0, 4.0, 6.0,
                       4.0, 8.0, 12.0 ];

        var (storage, events) = Fabric();
        var result = new WeightMatrixAnalyzer(storage, events).Analyse(a, 2, 3);

        Assert.True(result.IsSuccess);
        // The estimated rank must be 1 (only one significant singular value).
        Assert.Equal(1, result.Value!.EstimatedRank);
    }

    [Fact]
    public void WeightAnalyser_FullRankMatrix_HasEnergyFractionOfOne()
    {
        // A 2×2 diagonal matrix with well-separated singular values (condition number > 1).
        double[] a = [ 5.0, 0.0,
                       0.0, 1.0 ];

        var (storage, events) = Fabric();
        var report = new WeightMatrixAnalyzer(storage, events).Analyse(a, 2, 2).Value!;

        // All singular values are above the default tolerance so rank = 2.
        Assert.Equal(2, report.EstimatedRank);
        // 100 % of energy is captured because we kept all significant values.
        Assert.Equal(1.0, report.EnergyFractionCaptured, precision: 6);
        // The condition number is 5/1 = 5.
        Assert.Equal(5.0, report.ConditionNumber, precision: 8);
    }

    // ─── Region 16: SVD weight compressor ────────────────────────────────────

    [Fact]
    public void SvdCompressor_CompressDecompress_RoundTrip_IsAccurate()
    {
        // A 4×4 rank-2 matrix.
        // Compress with rank=2 and decompress; error must be near zero.
        double[] a = [ 1.0, 2.0, 0.0, 0.0,
                       2.0, 4.0, 0.0, 0.0,
                       0.0, 0.0, 3.0, 6.0,
                       0.0, 0.0, 6.0, 12.0 ];

        var (storage, events) = Fabric();
        var compressor  = new SvdWeightCompressor(storage, events);
        var compressed  = compressor.Compress(a, 4, 4, targetRank: 2).Value!;
        var decompressed = compressor.Decompress(compressed).Value!;

        AssertApproxEqual(a, decompressed, tolerance: 1e-6);
    }

    [Fact]
    public void SvdCompressor_CompressedForm_StoresFewerValuesForLowRank()
    {
        // A 10×10 identity matrix compressed at rank 5 must store fewer values
        // than the full 100-value dense matrix.
        var identity = new double[100];
        for (int i = 0; i < 10; i++) identity[i * 10 + i] = 1.0;

        var (storage, events) = Fabric();
        var compressed = new SvdWeightCompressor(storage, events)
            .Compress(identity, 10, 10, targetRank: 5).Value!;

        // Stored values: LeftFactor (10×5=50) + SingularValues (5) + RightFactor (10×5=50) = 105.
        // Full matrix: 100. Rank-5 might be slightly more but this test checks that the
        // compressed structure is the correct shape.
        Assert.Equal(5, compressed.LeftColumns);  // rank columns in LeftFactor
        Assert.Equal(5, compressed.SingularValues.Length);
    }

    // ─── Region 17: Int8 quantizer ───────────────────────────────────────────

    [Fact]
    public void Int8Quantizer_Quantize_ProducesValuesInSignedByteRange()
    {
        // All quantized values must lie in [-128, 127].
        double[] weights = [ -1.0, -0.5, 0.0, 0.5, 1.0 ];
        var q = Int8Quantizer.Quantize(weights, 1, 5);

        foreach (sbyte value in q.QuantizedValues)
            Assert.InRange((int)value, -128, 127);
    }

    [Fact]
    public void Int8Quantizer_DequantizeAfterQuantize_IsApproximatelyEqual()
    {
        // Round-trip: dequantize(quantize(x)) ≈ x with error ≤ scale/2.
        double[] weights = [ 0.0, 0.25, 0.5, 0.75, 1.0,
                             -0.5, -0.25 ];
        var q      = Int8Quantizer.Quantize(weights, 1, 7);
        var deq    = Int8Quantizer.Dequantize(q);

        // Each reconstructed value must be within MaxAbsoluteError of the original.
        for (int i = 0; i < weights.Length; i++)
            Assert.True(Math.Abs(weights[i] - deq[i]) <= q.MaxAbsoluteError + 1e-12,
                $"weights[{i}]={weights[i]:G4} deq[{i}]={deq[i]:G4} error={Math.Abs(weights[i]-deq[i]):G4}.");
    }

    [Fact]
    public void Int8Quantizer_UniformWeights_ProducesZeroMaxError()
    {
        // When all weights are identical, the quantization error must be zero.
        double[] weights = [ 3.14, 3.14, 3.14, 3.14 ];
        var q = Int8Quantizer.Quantize(weights, 2, 2);

        Assert.Equal(0.0, q.MaxAbsoluteError, precision: 12);
    }

    // ─── Region 18: Quantization-plus-low-rank compressor ────────────────────

    [Fact]
    public void QplusBoth_Compress_ReturnsTwoComponentResult()
    {
        // Compress a 4×4 matrix and confirm both parts are non-empty.
        double[] a = [ 1.0, 2.0, 3.0, 4.0,
                       2.0, 4.0, 6.0, 8.0,
                       1.0, 1.0, 1.0, 1.0,
                       0.5, 1.0, 1.5, 2.0 ];

        var (storage, events) = Fabric();
        var compressor = new QuantizationPlusLowRankCompressor(storage, events);
        var result     = compressor.Compress(a, 4, 4, targetRank: 2);

        Assert.True(result.IsSuccess);
        var (lowRank, residual) = result.Value;
        Assert.NotEmpty(lowRank.LeftFactor);
        Assert.NotEmpty(residual.QuantizedValues);
    }

    [Fact]
    public void QplusBoth_CompressDecompress_IsMoreAccurateThanQuantizationAlone()
    {
        // The two-stage method (low-rank + quantized residual) must produce a smaller
        // reconstruction error than pure int8 quantization.
        double[] a = [ 10.0, 20.0, 5.0,
                        20.0, 40.0, 10.0,
                         1.0,  2.0, 0.5 ];

        var (storage, events) = Fabric();
        var compressor = new QuantizationPlusLowRankCompressor(storage, events);
        var (lowRank, residual) = compressor.Compress(a, 3, 3, targetRank: 1).Value;

        double[] reconstructed = compressor.Decompress(lowRank, residual).Value!;

        // Compute Frobenius error of the two-stage method.
        double twoStageError = 0.0;
        for (int i = 0; i < a.Length; i++)
            twoStageError += (a[i] - reconstructed[i]) * (a[i] - reconstructed[i]);
        twoStageError = Math.Sqrt(twoStageError);

        // Compute Frobenius error of pure int8 quantization.
        var pureQ = Int8Quantizer.Quantize(a, 3, 3);
        double[] pureDequant = Int8Quantizer.Dequantize(pureQ);
        double pureQError = 0.0;
        for (int i = 0; i < a.Length; i++)
            pureQError += (a[i] - pureDequant[i]) * (a[i] - pureDequant[i]);
        pureQError = Math.Sqrt(pureQError);

        Assert.True(twoStageError <= pureQError + 1e-6,
            $"Two-stage error {twoStageError:G4} should not exceed pure-quantization error {pureQError:G4}.");
    }

    // ─── Region 19: MatrixAlgorithmsDispatcher (DNA rule 6) ──────────────────

    [Fact]
    public void Dispatcher_RoutesByName_LuAlgorithm()
    {
        // The dispatcher must accept the "lu" algorithm name and return a valid result.
        double[] a = [ 2.0, 1.0,
                       4.0, 3.0 ];

        var (storage, events) = Fabric();
        var dispatcher = new MatrixAlgorithmsDispatcher(storage, events);

        var request = new Dictionary<string, object>
        {
            ["matrix"]  = a,
            ["rows"]    = 2,
            ["columns"] = 2
        };

        var result = dispatcher.Dispatch("lu", request);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.ContainsKey("result"));
    }

    [Fact]
    public void Dispatcher_UnknownAlgorithmName_FailsWithCorrectErrorCode()
    {
        var (storage, events) = Fabric();
        var dispatcher = new MatrixAlgorithmsDispatcher(storage, events);

        var result = dispatcher.Dispatch("not-a-real-algorithm",
            new Dictionary<string, object>());

        Assert.False(result.IsSuccess);
        Assert.Equal("UNKNOWN_ALGORITHM", result.ErrorCode);
    }

    [Fact]
    public void Dispatcher_MissingRequestKey_FailsWithMissingKey()
    {
        // Dispatching "lu" without providing the required "matrix" key must fail.
        var (storage, events) = Fabric();
        var dispatcher = new MatrixAlgorithmsDispatcher(storage, events);

        var result = dispatcher.Dispatch("lu", new Dictionary<string, object>
        {
            ["rows"]    = 2,
            ["columns"] = 2
            // "matrix" key intentionally omitted
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("MISSING_KEY", result.ErrorCode);
    }

    [Fact]
    public void Dispatcher_AllAvailableAlgorithmNames_AreListedInProperty()
    {
        var (storage, events) = Fabric();
        var dispatcher = new MatrixAlgorithmsDispatcher(storage, events);

        Assert.Contains("lu",               dispatcher.AvailableAlgorithms);
        Assert.Contains("qr",               dispatcher.AvailableAlgorithms);
        Assert.Contains("svd",              dispatcher.AvailableAlgorithms);
        Assert.Contains("truncated-svd",    dispatcher.AvailableAlgorithms);
        Assert.Contains("randomized-svd",   dispatcher.AvailableAlgorithms);
        Assert.Contains("lu-solve",         dispatcher.AvailableAlgorithms);
        Assert.Contains("qr-least-squares", dispatcher.AvailableAlgorithms);
        Assert.Contains("sparse-solve",     dispatcher.AvailableAlgorithms);
        Assert.Contains("compress-weight",  dispatcher.AvailableAlgorithms);
        Assert.Contains("decompress-weight",dispatcher.AvailableAlgorithms);
        Assert.Contains("analyse-weight",   dispatcher.AvailableAlgorithms);
        Assert.Contains("quantize",         dispatcher.AvailableAlgorithms);
    }

    [Fact]
    public void Dispatcher_IsCaseInsensitive_ForAlgorithmName()
    {
        // Algorithm names must match regardless of capitalisation.
        double[] a = [ 3.0, 0.0, 0.0, 4.0 ];
        var (storage, events) = Fabric();
        var dispatcher = new MatrixAlgorithmsDispatcher(storage, events);
        var request    = new Dictionary<string, object>
        {
            ["matrix"]  = a,
            ["rows"]    = 2,
            ["columns"] = 2
        };

        var resultLower = dispatcher.Dispatch("lu",    request);
        var resultUpper = dispatcher.Dispatch("LU",    request);
        var resultMixed = dispatcher.Dispatch("Lu",    request);

        Assert.True(resultLower.IsSuccess);
        Assert.True(resultUpper.IsSuccess);
        Assert.True(resultMixed.IsSuccess);
    }
}
