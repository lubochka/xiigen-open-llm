// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using System.Diagnostics;

namespace DPO;

/// <summary>Completed helper work only; a started helper that throws has no completed duration.</summary>
public sealed record DpoPhaseTimingSnapshot(long StartedCalls, long CompletedCalls, double? CompletedMilliseconds)
{
    public long IncompleteCalls => StartedCalls - CompletedCalls;
}

/// <summary>
/// Contained wall-clock intervals, never additive with ElapsedMilliseconds.
/// Forward helpers include likelihood reduction; Backward includes logit gradients and backpropagation.
/// ApplyOptimizer includes recipient validation and parameter selection, excludes the following ZeroGrad,
/// and is not a measurement of the AdamW kernel alone. THREW does not infer commit or rollback state.
/// </summary>
public sealed record DpoStepVectorTimingSnapshot(int? Step, string Outcome, double? ElapsedMilliseconds,
    long VectorBatchesCompleted, DpoPhaseTimingSnapshot ReferenceForward,
    DpoPhaseTimingSnapshot PolicyForward, DpoPhaseTimingSnapshot Backward, DpoPhaseTimingSnapshot ApplyOptimizer);

/// <summary>
/// Opt-in, one-call observation for StepVector. Read Snapshot after the call returns or throws.
/// The trainer performs only monotonic clock reads and in-memory scalar updates: no callbacks or I/O.
/// This object is not a mutation authority or a transaction receipt and cannot be reused across calls.
/// </summary>
public sealed class DpoStepVectorTiming
{
    private int _begun;
    private int? _step;
    private long _start, _elapsedTicks, _vectorBatchesCompleted;
    private string _outcome = "NOT_STARTED";
    internal Phase ReferenceForward { get; } = new();
    internal Phase PolicyForward { get; } = new();
    internal Phase Backward { get; } = new();
    internal Phase ApplyOptimizer { get; } = new();
    internal bool Threw => _outcome == "THREW";

    public DpoStepVectorTimingSnapshot Snapshot()
        => new(_step, _outcome, _outcome is "RETURNED" or "THREW" ? Milliseconds(_elapsedTicks) : null,
            _vectorBatchesCompleted, ReferenceForward.Snapshot(), PolicyForward.Snapshot(),
            Backward.Snapshot(), ApplyOptimizer.Snapshot());

    internal void Begin(int step)
    {
        // Reject reuse before the trainer enters admission or any mutation/publication path.
        if (Interlocked.CompareExchange(ref _begun, 1, 0) != 0)
            throw new InvalidOperationException("DPO_STEP_VECTOR_TIMING_ALREADY_USED");
        _step = step;
        _start = Stopwatch.GetTimestamp();
        _outcome = "RUNNING";
    }

    internal void End(bool returned)
    {
        _elapsedTicks = Stopwatch.GetTimestamp() - _start;
        _outcome = returned ? "RETURNED" : "THREW";
    }

    internal void CompleteVectorBatch() => _vectorBatchesCompleted++;
    private static double Milliseconds(long ticks) => ticks * (1000d / Stopwatch.Frequency);

    internal sealed class Phase
    {
        private long _started, _completed, _completedTicks;
        internal long Begin()
        {
            _started++;
            return Stopwatch.GetTimestamp();
        }
        // Call only after the measured helper returns. A thrown helper remains started-only.
        internal void Complete(long start)
        {
            _completedTicks += Stopwatch.GetTimestamp() - start;
            _completed++;
        }
        internal DpoPhaseTimingSnapshot Snapshot()
            => new(_started, _completed, _completed == 0 ? null : Milliseconds(_completedTicks));
    }
}
