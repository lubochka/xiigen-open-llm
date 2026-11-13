namespace AutonomousLearning;

using AgentInfrastructure;
using ContinuousLearning;

// AutonomousTrainer — wraps IncrementalTrainer with E43 replay-buffer mixing.
// Implements the core training step of the 8-step autonomous learning cycle:
//   Step 7: IncrementalTrainer → LoRA rank=8, alpha=16 (from 14.ContinuousLearning).
// IncrementalTrainer is injected — never reimplemented here (Principle 4).
public sealed class AutonomousTrainer : BaseService
{
    // MinExamples — minimum number of new examples required to start a training cycle.
    // Fewer than this means the domain dataset is not yet statistically meaningful.
    public const int MinExamples = 5;

    // _trainer — injected IIncrementalTrainer from 14.ContinuousLearning.
    // Performs the actual LoRA gradient steps — not reimplemented here.
    private readonly IIncrementalTrainer _trainer;

    // _buffer — E43 ReplayBuffer that maintains the 70/30 new/old mix.
    // Shared across training cycles to accumulate domain history.
    private readonly ReplayBuffer _buffer;

    // Constructor: fabric-first injection of trainer and replay buffer.
    public AutonomousTrainer(
        ISessionStorage    storage,
        IIncrementalTrainer trainer,
        ReplayBuffer        buffer)
        : base(storage)
    {
        // Store trainer reference — called during TrainAsync after replay mixing.
        _trainer = trainer;
        // Store buffer reference — sampled before training and updated after.
        _buffer  = buffer;
    }

    // TrainAsync — run one autonomous training cycle for a single domain.
    // Pipeline:
    //   1. Guard: minimum example count (MinExamples) — fail fast if insufficient data.
    //   2. ReplayBuffer.Sample — mix 70% new + 30% old (E43 algorithm).
    //   3. IIncrementalTrainer.TrainAsync — run LoRA fine-tuning on the mixed batch.
    //   4. ReplayBuffer.Add — store new examples for future replay.
    // Returns the path to the saved LoRA adapter file on success.
    public async Task<OperationResult<string>> TrainAsync(
        string                         domain,
        IReadOnlyList<TrainingExample>  newExamples,
        IReadOnlyList<DpoPair>          dpoPairs)
    {
        // Guard: reject calls with too few examples — gradient signal is unreliable below MinExamples.
        if (newExamples.Count < MinExamples)
            return OperationResult<string>.Fail(
                $"At least {MinExamples} new examples required. Got {newExamples.Count}.",
                "INSUFFICIENT_EXAMPLES");

        // Step 2: mix new examples with replay buffer — E43 70/30 ratio.
        // When buffer is empty, mixedBatch = newExamples (100% new — no forgetting risk yet).
        var mixedBatch = _buffer.Sample(newExamples);

        // Step 3: delegate training to IncrementalTrainer from 14.ContinuousLearning.
        // IncrementalTrainer runs LoRA with rank=8, alpha=16 — no gradient logic here.
        var trainResult = await _trainer.TrainAsync(domain, mixedBatch, dpoPairs);

        // If training failed, return the error immediately — do not update replay buffer.
        if (!trainResult.IsSuccess)
            return trainResult;

        // Step 4: add new examples to the replay buffer ONLY on successful training.
        // This prevents polluting the buffer with examples that produced a failed adapter.
        foreach (var ex in newExamples)
            _buffer.Add(ex);

        // Return the adapter path reported by IncrementalTrainer.
        return trainResult;
    }
}
