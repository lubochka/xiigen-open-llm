# experiment-design-SKILL.md — How to design E-N experiments

## When to load
Load when designing a new experiment or writing an experiment plan.

## Anatomy of an experiment

Every experiment has exactly:
- ID: E[number] (next available)
- Component: what is being tested
- Hypothesis: one sentence predicting the direction of change
- Metric: one measurable number (accuracy, ms, count, ratio)
- Baseline: how the system behaves without the change
- Enhanced: how the system behaves with the change
- Gate B: the exact condition for PASSED (e.g. "enhanced > baseline")
- Tier: 1 / 2 / 3

## Failure classification

    PASSED:      enhanced > baseline, Gate B met
    FAILED:      enhanced <= baseline after Tier 3
    SCALE_UP:    needs more data — re-run at higher Tier
    ARCH:        architectural observation — algorithm not applicable here
    DATA:        test data problem — fix generator, re-run
    TOOL:        wrong tool — bigram can't test gradient hypotheses

A FAILED result is informative. Document why it failed.
Never label a result FAILED without explaining the root cause.

## E28-E50 lessons

- Bigram models cannot show gradient-based signal (E28-E31, E44, E45 → SCALE_UP)
- Synthetic data with fixed seed can collapse class distribution (E40, E42 → use StratifiedSplit)
- Baseline must be non-trivial: alphabetical sort is not a real baseline (E38 → fixed to E38v2)
- ProbeNeuralNetwork (ProbeNN) ≠ LoRA (Low-Rank Adaptation) proxy: K-NN (K-Nearest Neighbours) has no base/adapter separation
- Self-healing distribution fix can flip PASSED→FAILED if both reach 1.00 trivially (E33)

## Three critical locked decisions — check before every experiment plan

**D-07 — StackDetector runs before normalization (E20)**
StackDetector must execute before SemanticNormalizer in the pipeline.
Running it after normalization destroys C#/TypeScript/Python markers before they can be detected.
Any plan that reorders these two components violates D-07.

**D-08 — StratifiedSplit for all experiments (E40, E42)**
All experiments use StratifiedSplit, not simple random split.
A naive split with seed 42 collapses class distribution and produces misleading results.
Any plan that omits StratifiedSplit violates D-08.

**D-09 — max_weight formula (E44)**
Weight formula: max_weight = total_samples / (num_classes × rare_class_count)
Using a fixed constant multiplier (for example ×10) causes catastrophic forgetting.
Any plan that proposes a fixed weight constant without this formula violates D-09.
