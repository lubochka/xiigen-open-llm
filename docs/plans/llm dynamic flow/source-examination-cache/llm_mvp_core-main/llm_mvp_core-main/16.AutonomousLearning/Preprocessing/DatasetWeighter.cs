namespace AutonomousLearning;

using AgentInfrastructure;
using ContinuousLearning;

// DatasetWeighter — applies the D-09 class-imbalance weighting formula to training examples.
// D-09 formula: max_weight = total_examples / (num_classes × rare_examples).
// WARNING (E44 failure lesson): NEVER use a ×10 constant — E44 FAILED with -98% accuracy
// because a hardcoded ×10 multiplier destroyed gradient signal on imbalanced datasets.
// Gate B: ComputeMaxWeight(1000, 5, 50) must return exactly 4.0.
public sealed class DatasetWeighter : BaseService
{
    // Constructor: receives ISessionStorage from dependency injection.
    public DatasetWeighter(ISessionStorage storage) : base(storage) { }

    // ComputeMaxWeight — D-09 formula: total_examples / (num_classes × rare_examples).
    // Static for direct testability: Assert.Equal(4.0, DatasetWeighter.ComputeMaxWeight(1000, 5, 50)).
    // NOT a ×10 constant — the ×10 version is the E44 failure. Do not restore it.
    // When rareExamples = 0, returns 1.0 (no weighting needed — all classes balanced).
    // When numClasses = 0, returns 1.0 (guard against divide-by-zero in edge cases).
    public static double ComputeMaxWeight(int totalExamples, int numClasses, int rareExamples)
    {
        // Guard: zero rare examples means no imbalance exists — use weight 1.0 uniformly.
        if (rareExamples <= 0) return 1.0;

        // Guard: zero classes is a configuration error — return 1.0 as safe default.
        if (numClasses <= 0) return 1.0;

        // D-09 formula: max_weight = total / (classes × rare) — calibrates rare-class up-weight.
        // Example: 1000 total, 5 classes, 50 rare → 1000 / (5 × 50) = 4.0.
        return (double)totalExamples / (numClasses * rareExamples);
    }

    // Weight — apply the D-09 max_weight cap to a list of training examples.
    // Returns Fail(ZERO_RARE) when rareExamples ≤ 0 — the instance method guards the pipeline.
    // The static ComputeMaxWeight is used for the formula; Weight enforces the pre-condition.
    public OperationResult<IReadOnlyList<TrainingExample>> Weight(
        IReadOnlyList<TrainingExample> examples,
        int numClasses,
        int rareExamples)
    {
        // Guard: rareExamples = 0 is invalid in a weighting context — fail fast.
        // (ComputeMaxWeight handles 0 gracefully; Weight is stricter for pipeline safety.)
        if (rareExamples <= 0)
            return OperationResult<IReadOnlyList<TrainingExample>>.Fail(
                "rareExamples must be > 0. Use at least 1 to enable class-imbalance weighting.",
                "ZERO_RARE");

        // Compute the D-09 cap: examples with weight above this value are clamped.
        var maxWeight = ComputeMaxWeight(examples.Count, numClasses, rareExamples);

        // Apply cap: each example's weight is multiplied by maxWeight then clamped to [0, maxWeight].
        // This scales rare-class examples up while preventing any single example from dominating.
        var weighted = examples
            .Select(e => e with { Weight = Math.Min(e.Weight * maxWeight, maxWeight) })
            .ToList();

        // Return the re-weighted list — IncrementalTrainer reads the Weight field during LoRA.
        return OperationResult<IReadOnlyList<TrainingExample>>.Ok(weighted);
    }
}
