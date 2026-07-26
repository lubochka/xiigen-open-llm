// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MatrixAlgorithms;

// SparseLinearSolver solves the linear system A × x = b for a sparse matrix A.
// For small sparse systems (up to a few thousand entries), the simplest correct approach
// is to convert the sparse matrix to dense and use the LU decomposition already implemented.
// For very large systems, specialised sparse LU (with fill-reducing ordering) would be needed,
// but that is beyond the scope of this sub-project.
// This solver is correct for all sparse matrices whose dense equivalent fits in memory.
public sealed class SparseLinearSolver : BaseService
{
    // The LU decomposition service used to factor the dense form of the sparse matrix.
    private readonly LuDecompositionService _luService;

    // Create the solver by injecting the storage, logger, and LU service (fabric-first rule).
    public SparseLinearSolver(IMatrixStorage storage, IMatrixEventLogger logger)
        : base(storage, logger)
    {
        // The LU service shares the same storage and logger as this solver.
        _luService = new LuDecompositionService(storage, logger);
    }

    // Solve A × x = b where A is provided as a CSR sparse matrix.
    // sparseA — the coefficient matrix in CSR format
    // rhs     — the right-hand side vector b (length = sparseA.Rows)
    // Returns the solution vector x (length = sparseA.Columns).
    public OperationResult<double[]> Solve(CsrSparseMatrix sparseA, double[] rhs)
    {
        // LU decomposition only applies to square matrices.
        if (sparseA.Rows != sparseA.Columns)
            return OperationResult<double[]>.Fail(
                $"Sparse linear solver requires a square matrix. " +
                $"Got {sparseA.Rows} rows × {sparseA.Columns} columns.", "NOT_SQUARE");

        if (rhs.Length != sparseA.Rows)
            return OperationResult<double[]>.Fail(
                $"Right-hand side length {rhs.Length} must equal matrix rows {sparseA.Rows}.", "BAD_INPUT");

        // Convert the sparse matrix to a dense flat array for LU decomposition.
        // This is acceptable because this solver is intended for small-to-medium systems.
        double[] denseMatrix = sparseA.ToDenseArray();
        int      size        = sparseA.Rows;

        // Factor the dense matrix into L and U.
        var luResult = _luService.Decompose(denseMatrix, size, size);
        if (!luResult.IsSuccess)
            return OperationResult<double[]>.Fail(
                $"LU factorisation failed: {luResult.ErrorMessage}", luResult.ErrorCode);

        // Solve using the L and U factors via forward and backward substitution.
        var solveResult = LuLinearSolver.Solve(
            luResult.Value!.FactorOne,
            luResult.Value.FactorTwo,
            size,
            rhs);

        if (!solveResult.IsSuccess)
            return OperationResult<double[]>.Fail(
                $"Triangular solve failed: {solveResult.ErrorMessage}", solveResult.ErrorCode);

        // Save and log the successful solution (Genie DNA rules 8 and 9).
        Storage.TrySave("sparse-solve-last", new Dictionary<string, object>
        {
            ["size"] = size, ["nonZeros"] = sparseA.NonZeroCount
        });

        EventLogger.Log(new StandardEvent(
            "matrix.sparse-solve.completed",
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            new Dictionary<string, object> { ["size"] = size }));

        return solveResult;
    }
}
