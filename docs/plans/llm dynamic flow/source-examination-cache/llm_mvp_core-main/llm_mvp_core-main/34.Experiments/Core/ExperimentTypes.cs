namespace Experiments;

// ─── Shared result envelope used by every experiment ───────────────────────────

// Wraps a value of type T together with a success/failure flag and an optional
// error message.  Every public method returns this type so callers never need
// to catch exceptions for expected failure cases.
public sealed class OperationResult<T>
{
    public bool   IsSuccess    { get; }
    public T?     Value        { get; }
    public string ErrorMessage { get; }

    private OperationResult(bool ok, T? value, string error)
        => (IsSuccess, Value, ErrorMessage) = (ok, value, error);

    public static OperationResult<T> Ok(T value)           => new(true,  value, "");
    public static OperationResult<T> Fail(string message)  => new(false, default, message);
}

// ─── Per-metric snapshot for one run (baseline or enhanced) ───────────────────

// Holds the numbers measured during a single experiment run:
// accuracy as a fraction 0–1, wall-clock time in milliseconds,
// peak allocated memory in megabytes, and token count of the dataset used.
public sealed record RunMetrics
{
    // Fraction of next-token predictions that were correct (0.0 to 1.0).
    public double AccuracyFraction  { get; init; }

    // Wall-clock duration of the run in milliseconds.
    public double TimeMs            { get; init; }

    // Approximate peak memory used during this run in megabytes.
    public double MemoryMb          { get; init; }

    // Number of tokens in the dataset that was processed.
    public int    DatasetTokenCount { get; init; }
}

// ─── Full result of one experiment at one scale level ─────────────────────────

// Combines baseline metrics, enhanced metrics, computed deltas, and a pass/fail
// verdict based on the ≥5% accuracy improvement promotion rule.
public sealed record ExperimentResult
{
    // E.g. "E01", "E02", …, "E06".
    public required string ExperimentId { get; init; }

    // 0 = Markov-micro (~1 000 tokens), 1 = Markov-small (~9 000 tokens), etc.
    public required int    Scale        { get; init; }

    // Metrics for the original approach (BPE flat Markov, etc.).
    public required RunMetrics Baseline  { get; init; }

    // Metrics for the improved approach (cluster IDs, trigram, PMI, etc.).
    public required RunMetrics Enhanced  { get; init; }

    // Absolute difference: Enhanced.AccuracyFraction – Baseline.AccuracyFraction.
    public double AccuracyDelta => Enhanced.AccuracyFraction - Baseline.AccuracyFraction;

    // Percentage-point difference (×100).
    public double AccuracyDeltaPct => AccuracyDelta * 100.0;

    // True when AccuracyDeltaPct ≥ 5 (the promotion threshold).
    public bool Passed => AccuracyDeltaPct >= 5.0;

    // Human-readable verdict string.
    public string NextAction => Passed ? "PROMOTE_TO_NEXT_SCALE" : "ABANDON";

    // Optional free-text notes added by the experiment runner.
    public string Notes { get; init; } = "";
}
