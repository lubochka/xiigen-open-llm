namespace AutonomousLearning;

using AgentInfrastructure;
using ContinuousLearning;

// ReplayBuffer — E43 algorithm: stores past training examples and mixes them with new data.
// Training batch = 70% new examples + 30% random sample from the buffer.
// Prevents catastrophic forgetting when new domain data replaces old domain data.
// E43 experiment PASSED with +23% retention on old domain after new domain training.
public sealed class ReplayBuffer : BaseService
{
    // ReplayRatio — fraction of the training batch drawn from the replay buffer (30%).
    // Constant from E43 experiment: 0.3 gives the best forgetting / new-learning trade-off.
    public const float ReplayRatio = 0.3f;

    // _buffer — in-memory list of past examples accumulated across training cycles.
    // Grows with each Add call; bounded only by available memory.
    private readonly List<TrainingExample> _buffer = [];

    // _rng — random number generator for reproducible buffer sampling in tests.
    // Seeded from Environment.TickCount64 in production; set via constructor for tests.
    private readonly Random _rng;

    // Constructor: receives ISessionStorage + optional random seed for deterministic tests.
    public ReplayBuffer(ISessionStorage storage, int? seed = null) : base(storage)
        => _rng = seed.HasValue ? new Random(seed.Value) : Random.Shared;

    // Count — number of examples currently held in the replay buffer.
    public int Count => _buffer.Count;

    // Add — store one past example in the replay buffer for future sampling.
    // Called after each successful training cycle to accumulate history.
    public void Add(TrainingExample example)
        // Append example to the in-memory buffer — no deduplication by design.
        => _buffer.Add(example);

    // Sample — produce a mixed training batch: 70% new + 30% from the buffer.
    // When the buffer is empty, returns newExamples unchanged (100% new, no replay risk).
    // replayCount = round(newExamples.Count × ReplayRatio / (1 − ReplayRatio))
    //             = round(N × 0.3 / 0.7) — gives 70/30 split in the final batch.
    public IReadOnlyList<TrainingExample> Sample(IReadOnlyList<TrainingExample> newExamples)
    {
        // No buffer contents: no catastrophic forgetting risk — return all new examples.
        if (_buffer.Count == 0)
            return newExamples;

        // Compute how many replay examples are needed to achieve the 30% share.
        // Formula: replayCount / (newCount + replayCount) = 0.3 → replayCount = N × 3/7.
        var replayCount = (int)Math.Round(newExamples.Count * ReplayRatio / (1f - ReplayRatio));

        // Cap replay count at available buffer size — cannot sample more than we have.
        replayCount = Math.Min(replayCount, _buffer.Count);

        // Fisher-Yates partial shuffle to draw replayCount examples from the buffer.
        // Copy indices to avoid mutating the buffer itself.
        var indices = Enumerable.Range(0, _buffer.Count).ToArray();
        for (int i = 0; i < replayCount; i++)
        {
            // Swap index i with a random index in the remaining range.
            var j = _rng.Next(i, indices.Length);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        // Collect the selected replay examples using the shuffled indices.
        var replayed = indices.Take(replayCount)
            .Select(idx => _buffer[idx])
            .ToList();

        // Build final batch: all new examples first, then replay examples appended.
        // Caller (IncrementalTrainer) trains on the combined list.
        return newExamples.Concat(replayed).ToList();
    }
}
