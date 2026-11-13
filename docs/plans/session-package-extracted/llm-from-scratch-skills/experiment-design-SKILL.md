# Experiment Design — LlmFromScratch
## For: Designing E-N experiments

---

## When to Load

Load when designing a new experiment or writing an experiment plan.
Also load when reviewing the EXPERIMENT-RESULTS-MAP.md before starting work.

---

## Anatomy of an Experiment

Every experiment has exactly:
- **ID:** E[number] (next available from EXPERIMENT-RESULTS-MAP.md)
- **Component:** what is being tested (class name, algorithm family)
- **Hypothesis:** one sentence predicting the direction of change
- **Metric:** one measurable number (accuracy, ms, count, ratio)
- **Baseline:** how the system behaves WITHOUT the change
- **Enhanced:** how the system behaves WITH the change
- **Gate B:** the exact condition for PASSED (e.g., `enhanced > baseline`)
- **Tier:** 1 / 2 / 3

---

## Tier System

| Tier | Scope | When to use |
|------|-------|-------------|
| 1 | 1–10 files, synthetic data | Initial validation — does the algorithm work at all? |
| 2 | 11–100 files, semi-real data | Integration signal — does it work with realistic input? |
| 3 | 100+ files, real codebase data | Production signal — final verdict |

**An experiment FAILED at Tier 2 escalates to Tier 3 before final verdict.**
Do not mark FAILED after Tier 2 alone unless the hypothesis itself is contradicted.

---

## Failure Classification

| Result | Meaning |
|--------|---------|
| `PASSED` | enhanced > baseline, Gate B met |
| `FAILED` | enhanced ≤ baseline after Tier 3 |
| `SCALE_UP` | needs more data — re-run at higher Tier |
| `ARCH` | architectural observation — algorithm not applicable in this position |
| `DATA` | test data problem — fix generator, re-run |
| `TOOL` | wrong tool — e.g., bigram cannot test gradient hypotheses |

A FAILED result is informative. Document WHY it failed.
Never label a result FAILED without explaining the root cause.

---

## Lessons from E28–E50 (from EXPERIMENT-RESULTS-MAP.md)

These lessons must be read before designing any new experiment:

- **Bigram models cannot show gradient-based signal** (E28–E31, E44, E45 → SCALE_UP)
  Reason: bigram has no internal gradient. Any experiment that expects
  gradient-based improvement to show up in bigram accuracy will SCALE_UP forever.

- **Synthetic data with fixed seed can collapse class distribution** (E40, E42 → use StratifiedSplit)
  Reason: a fixed seed can produce a dataset where one class has near-zero representation.
  Always verify class distribution before running.

- **Baseline must be non-trivial** (E38 → fixed to E38v2)
  Alphabetical sort is not a real baseline. Use a meaningful default.

- **ProbeNN ≠ LoRA proxy** (documented in E-N results)
  K-NN has no base/adapter separation. Do not use K-NN tests as proxy for LoRA results.

- **Self-healing distribution fix can flip PASSED → FAILED** (E33)
  If both enhanced and baseline reach the ceiling (1.00), the improvement
  disappears. Gate B must check `enhanced > baseline AND baseline < 1.00`.

---

## Experiment Template

```csharp
// E[N]: [ComponentName] — [one-sentence hypothesis]
// Metric: [specific field or count]
// Baseline: [behavior without change] = [expected value range]
// Enhanced: [behavior with change] = [expected value range]
// Gate B: enhanced > baseline
// Tier: 1 (scale to 2 if SCALE_UP)

[Fact]
public void EN_ComponentName_HypothesisDescription()
{
    // Arrange — Tier 1: minimal realistic input set
    var inputs = new[] { "validate null check", "implement retry loop", "fix null pointer" };
    var baseline = MeasureBaseline(inputs);   // WITHOUT the change
    var enhanced = MeasureEnhanced(inputs);   // WITH the change

    // Gate B: enhanced strictly better
    Assert.True(enhanced > baseline,
        $"E[N] Gate B: enhanced={enhanced:F3} must be > baseline={baseline:F3}");
}
```

---

## Writing Gate B

Gate B must be:
- A specific comparison: `enhanced > baseline`
- Or a specific count: `result.Value!.Length == 22` (E17 VocabSize = 22)
- Or a specific metric: `accuracy >= 0.895`

Gate B must NOT be:
- "Works correctly"
- "Returns a result"
- "No errors"
- `Assert.True(result.IsSuccess)` alone (too vague — any branch passes this)

---

## D-09 Max-Weight Formula (DECISIONS-LOCKED)

The weight calculation for class balancing is locked:

```csharp
// CORRECT — dynamic calculation
double max_weight = (double)total_examples / (num_classes * rare_examples);
// Example: 1000 / (5 * 50) = 4.0

// VIOLATION — hardcoded multiplier
double max_weight = rare_examples * 10.0;  // NEVER do this
```

This formula is DECISIONS-LOCKED D-09. Do not change the calculation.
Do fix: divide-by-zero guard, zero rare_examples edge case.

---

## Experiment Plan Format

```
Experiment: E[N]
Component: [class or module name]
Subproject: [NN.SubprojectName]

Hypothesis: [one sentence — predicts direction, not magnitude]
  "Adding family collapse before predicate extraction reduces semantic noise
   and increases domain classification accuracy."

Metric: [specific field]
  Accuracy of DomainClassifier.ClassifyDomain() on the test set

Baseline: [current behavior]
  DomainClassifier without FamilyCollapser: accuracy ≈ 0.62

Enhanced: [expected behavior with change]
  DomainClassifier with FamilyCollapser in pipeline: accuracy > 0.62

Gate B: enhanced_accuracy > baseline_accuracy

Tier: 1 (files: 10 synthetic prompts, class distribution verified)

Test file: Tests/LlmFromScratch.Tests/ExperimentsV2Tests.cs
Test name: EN_DomainClassifier_FamilyCollapse_IncreasesAccuracy
```
