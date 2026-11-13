// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MatrixAlgorithms;

// WeightMatrixAnalyzer examines a neural network weight matrix and reports
// how well a low-rank approximation would preserve its information.
// It does this by measuring the spectral decay (спектральный спад) — how quickly
// the singular values drop off. Fast decay means a low-rank approximation is accurate;
// slow decay means the matrix is genuinely high-rank and harder to compress.
public sealed class WeightMatrixAnalyzer : BaseService
{
    // The SVD service used to compute the singular values of the weight matrix.
    private readonly SingularValueDecompositionService _svdService;

    // Create the analyzer with injected storage and logger (fabric-first rule).
    public WeightMatrixAnalyzer(IMatrixStorage storage, IMatrixEventLogger logger)
        : base(storage, logger)
    {
        _svdService = new SingularValueDecompositionService(storage, logger);
    }

    // Analyse the spectral decay of the weight matrix and return a report.
    // weightMatrix — all entries in row-major order
    // rows, columns — matrix dimensions
    // significanceTolerance — singular values below this fraction of the largest are considered noise
    public OperationResult<SpectralDecayReport> Analyse(
        double[] weightMatrix, int rows, int columns,
        double significanceTolerance = 0.01)
    {
        if (weightMatrix.Length < rows * columns)
            return OperationResult<SpectralDecayReport>.Fail(
                "Weight matrix array is too short for the given dimensions.", "BAD_INPUT");

        // Compute all singular values via the full SVD service.
        var svdResult = _svdService.Decompose(weightMatrix, rows, columns);
        if (!svdResult.IsSuccess)
            return OperationResult<SpectralDecayReport>.Fail(
                $"SVD failed: {svdResult.ErrorMessage}", svdResult.ErrorCode);

        // The singular values are already sorted in descending order by the SVD service.
        double[] singularValues = svdResult.Value!.FactorTwo;
        int      totalCount     = singularValues.Length;

        // The total energy of the matrix is the sum of squares of all singular values
        // (this equals the squared Frobenius norm by the Parseval identity).
        double totalEnergy = singularValues.Sum(s => s * s);

        // The significance threshold is: the largest singular value × tolerance.
        double threshold = singularValues[0] * significanceTolerance;

        // Count how many singular values are "significant" (above the threshold).
        int estimatedRank = singularValues.Count(s => s >= threshold);

        // Compute what fraction of the total energy the significant values capture.
        double capturedEnergy = singularValues.Take(estimatedRank).Sum(s => s * s);
        double energyFraction = totalEnergy > 1e-30 ? capturedEnergy / totalEnergy : 1.0;

        // The condition number (число обусловленности) is the ratio of largest to smallest
        // non-zero singular value. A large condition number signals near-singularity.
        double smallestNonZero = singularValues.LastOrDefault(s => s >= threshold * 0.01);
        double conditionNumber = smallestNonZero > 1e-30
            ? singularValues[0] / smallestNonZero
            : double.PositiveInfinity;

        var report = new SpectralDecayReport(
            singularValues,
            conditionNumber,
            estimatedRank,
            energyFraction,
            threshold);

        // Save the analysis result before emitting the event (DNA rule 8).
        Storage.TrySave("weight-analysis-last", new Dictionary<string, object>
        {
            ["rows"] = rows, ["columns"] = columns,
            ["estimatedRank"] = estimatedRank, ["energyFraction"] = energyFraction
        });

        EventLogger.Log(new StandardEvent(
            "matrix.weight-analysis.completed",
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                ["estimatedRank"] = estimatedRank, ["energyFraction"] = energyFraction
            }));

        return OperationResult<SpectralDecayReport>.Ok(report);
    }
}
