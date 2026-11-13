namespace Experiments;

// ─── ResourceMonitor: per-thread allocation and wall-clock timer ────────────────
//
// Uses GC.GetAllocatedBytesForCurrentThread() — shows only what the current
// run allocated, without accumulation from prior runs in the same process.
// Usage:
//   var m = new ResourceMonitor();
//   m.Start();
//   RunAlgorithm();
//   ResourceSnapshot s = m.Stop();
//   // s.ElapsedMs, s.PeakMemoryMb

public sealed class ResourceMonitor : IDisposable
{
    private readonly System.Diagnostics.Stopwatch _stopwatch = new();
    private long _memAtStart;
    private long _peakMem;
    private bool _running;

    // Snapshot the per-thread allocated bytes, then start the clock.
    public void Start()
    {
        _memAtStart = GC.GetAllocatedBytesForCurrentThread();
        _peakMem    = _memAtStart;
        _running    = true;
        _stopwatch.Restart();
    }

    // Optional mid-run sample — updates peak without stopping the clock.
    public void SampleMemory()
    {
        if (!_running) return;
        long cur = GC.GetAllocatedBytesForCurrentThread();
        if (cur > _peakMem) _peakMem = cur;
    }

    // Stop the clock and return the snapshot.
    public ResourceSnapshot Stop()
    {
        _stopwatch.Stop();

        long memAtEnd = GC.GetAllocatedBytesForCurrentThread();
        if (memAtEnd > _peakMem) _peakMem = memAtEnd;

        _running = false;

        return new ResourceSnapshot
        {
            ElapsedMs    = _stopwatch.Elapsed.TotalMilliseconds,
            PeakMemoryMb = Math.Max(0, _peakMem - _memAtStart) / 1_048_576.0,
        };
    }

    public void Dispose() { }
}

// ─── ResourceSnapshot ──────────────────────────────────────────────────────────

public sealed record ResourceSnapshot
{
    public double ElapsedMs    { get; init; }
    // Additional memory allocated during the monitored region, in MB.
    public double PeakMemoryMb { get; init; }
}
