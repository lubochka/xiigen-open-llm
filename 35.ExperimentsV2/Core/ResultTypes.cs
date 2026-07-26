// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// ─── Shared result envelope ────────────────────────────────────────────────────

// Every public method returns OperationResult<T> instead of throwing exceptions,
// so callers handle errors uniformly without try/catch at every call site.
public sealed class OperationResult<T>
{
    public bool   IsSuccess    { get; }
    public T?     Value        { get; }
    public string ErrorMessage { get; }

    private OperationResult(bool ok, T? value, string error)
        => (IsSuccess, Value, ErrorMessage) = (ok, value, error);

    public static OperationResult<T> Ok(T value)          => new(true,  value, "");
    public static OperationResult<T> Fail(string message) => new(false, default, message);
}

// ─── V2 experiment result record ───────────────────────────────────────────────

// Each V2 experiment produces a richer result than V1:
// it carries per-metric snapshots for baseline and enhanced, plus experiment-specific
// scaling signals that track how the experiment behaves as the dataset grows.
public sealed record ExperimentV2Result
{
    // Short identifier like "E01-AST-TOKENIZER".
    public required string ExperimentId { get; init; }

    // 1 = Tier 1 (micro), 2 = Tier 2 (small), etc.
    public required int Tier { get; init; }

    // Primary metric value for the baseline approach (meaning depends on experiment).
    public required double BaselinePrimary { get; init; }

    // Primary metric value for the enhanced approach.
    public required double EnhancedPrimary { get; init; }

    // Name of the primary metric (e.g. "prediction_error", "top1_accuracy", "perplexity").
    public required string PrimaryMetricName { get; init; }

    // True if this is a "lower is better" metric (e.g. prediction_error, perplexity).
    public required bool LowerIsBetter { get; init; }

    // Percentage improvement: positive means the enhanced approach is better.
    // For lower-is-better: (baseline - enhanced) / baseline * 100.
    // For higher-is-better: (enhanced - baseline) / |baseline| * 100.
    public double ImprovementPct =>
        LowerIsBetter
            ? (BaselinePrimary - EnhancedPrimary) / (BaselinePrimary == 0 ? 1 : BaselinePrimary) * 100.0
            : (EnhancedPrimary - BaselinePrimary) / (Math.Abs(BaselinePrimary) < 1e-12 ? 1 : Math.Abs(BaselinePrimary)) * 100.0;

    // True when ImprovementPct ≥ 5 (the promotion threshold).
    public bool Passed => ImprovementPct >= 5.0;

    // True when delta=0 not because the approach is bad, but because there is too little
    // data.  Signal: run at the next tier — do not redesign the experiment.
    public bool ShouldScaleUp { get; init; }

    // Recommended next action string.
    public string NextAction =>
        Passed        ? "PROMOTE_TO_NEXT_TIER" :
        ShouldScaleUp ? "SCALE_UP_DATA_TOO_SMALL" :
                        "TUNE_OR_ABANDON";

    // Scaling-axis-specific signal value (e.g. vocab_saturation_ratio, confidence_ratio).
    public double ScalingSignal { get; init; }

    // Description of what ScalingSignal measures for this experiment.
    public string ScalingSignalNote { get; init; } = "";

    // Free-text additional notes from the experiment runner.
    public string Notes { get; init; } = "";

    // Wall-clock time of baseline run in milliseconds.
    public double BaselineTimeMs { get; init; }

    // Wall-clock time of enhanced run in milliseconds.
    public double EnhancedTimeMs { get; init; }
}

// ─── N-gram result entry — used by E03 ────────────────────────────────────────

// One row in the results_per_n table for E03.
public sealed record NgramResultEntry
{
    // Order of the N-gram model (1=unigram, 2=bigram, 3=trigram, …).
    public required int    N                     { get; init; }

    // Human label like "unigram", "bigram", "trigram".
    public required string Name                  { get; init; }

    // Perplexity = 2^(-avg log2 P(correct_token)).  Lower is better.
    public required double Perplexity            { get; init; }

    // Fraction of predictions where the most-likely token was correct.
    public required double Top1Accuracy          { get; init; }

    // Number of unique N-gram keys seen in training.
    public required int    UniqueNgrams          { get; init; }

    // Average number of training observations per unique N-gram key.
    public required double AvgObservationsPerNgram { get; init; }

    // True when AvgObservationsPerNgram ≥ 5 (reliability threshold).
    public bool IsReliable => AvgObservationsPerNgram >= 5.0;
}
