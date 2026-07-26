// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace CodeOptimizer;

// ─── Result envelope ────────────────────────────────────────────────────────

public sealed class OperationResult<T>
{
    public bool   IsSuccess    { get; }
    public T?     Value        { get; }
    public string ErrorMessage { get; }

    private OperationResult(bool ok, T? value, string error)
        => (IsSuccess, Value, ErrorMessage) = (ok, value, error);

    public static OperationResult<T> Success(T value)        => new(true,  value, "");
    public static OperationResult<T> Failure(string message) => new(false, default, message);

    public OperationResult<TNew> MapError<TNew>() =>
        OperationResult<TNew>.Failure(ErrorMessage);
}

// ─── Domain records ──────────────────────────────────────────────────────────

public sealed record OptimizationFinding
{
    public required string OptimizationType          { get; init; }
    public required string FilePath                  { get; init; }
    public required int    LineNumber                { get; init; }
    public required string OriginalCode              { get; init; }
    public          string? SuggestedReplacement     { get; init; }
    public required string HumanReadableExplanation  { get; init; }
    public required double ConfidenceScore           { get; init; }
}

public sealed record OptimizationReport
{
    public required string                          FilePath                     { get; init; }
    public required IReadOnlyList<OptimizationFinding> Findings                 { get; init; }
    public required long                            AnalysisDurationMilliseconds { get; init; }
    public required bool                            ParsedSuccessfully           { get; init; }
    public          IReadOnlyList<string>           ParseErrors                  { get; init; } = [];
}

public sealed record OptimizationContext
{
    public bool   EnableDeadCodeDetection              { get; init; } = true;
    public bool   EnableConstantFolding                { get; init; } = true;
    public bool   EnableStrengthReduction              { get; init; } = true;
    public bool   EnableCommonSubexpressionElimination { get; init; } = true;
    public bool   EnableLoopOptimization               { get; init; } = true;
    public double MinimumConfidenceThreshold           { get; init; } = 0.7;
}
