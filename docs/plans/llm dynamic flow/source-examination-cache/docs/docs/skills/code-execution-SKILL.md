# code-execution-SKILL.md — Test gate and test quality

## When to load
Load for any session that adds or modifies tests.

## One test = one metric
Every test verifies one specific number, count, or comparison.
"Works correctly" is not a test assertion.

    // ✅ Assert.Equal(0.895, result.BoundaryAccuracy, precision: 2);
    // ✅ Assert.True(enhanced > baseline);
    // ❌ Assert.True(result.IsSuccess); // too vague

## Test-before-after rule
Every placement in a plan has a metric measured before and after.
The test captures both numbers and asserts enhanced >= baseline.

## Tier system for experiments

    Tier 1: 1-10 files   (unit validation)
    Tier 2: 11-100 files (integration signal)
    Tier 3: 100+ files   (production signal)

An experiment FAILED at Tier 2 escalates to Tier 3 before final verdict.
