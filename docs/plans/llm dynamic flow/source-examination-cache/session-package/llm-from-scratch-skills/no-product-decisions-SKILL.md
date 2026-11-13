# No Product Decisions — LlmFromScratch
## priority: ABSOLUTE — second only to agent-constitution

> Claude executes. Claude does not decide.

---

## What This Skill Does

Prevents Claude from making product decisions that belong to Luba.
Every experiment threshold, scoring formula, algorithm parameter, and
subproject scope is a product decision. Claude implements — it does not choose.

---

## The Core Rule

**Claude may not change behavior that affects algorithm output without
Luba's explicit approval.**

Product decisions in LlmFromScratch include, but are not limited to:
- Experiment Gate B criteria (what score constitutes "PASSED")
- Algorithm weight formulas (the D-09 max_weight calculation)
- DNA rule enforcement thresholds (not changing them without approval)
- Which subprojects are in scope for the current session
- Which experiments to run next
- Which language to port an algorithm to
- Whether a failed experiment is FAILED or needs a Tier 3 re-run

---

## The D-09 Analog (from DECISIONS-LOCKED)

**What happened:** The max_weight formula for class balancing was nearly
changed from the dynamic calculation to a hardcoded constant multiplier
because "the hardcoded version looked cleaner."

The dynamic formula:
```csharp
max_weight = total_examples / (num_classes * rare_examples)
// Example: 1000 / (5 * 50) = 4.0
```

A hardcoded `× 10` would have silently broken class balancing for every
experiment using this formula, with no test catching it because the tests
used synthetic balanced data.

**The rule that emerged:** Algorithm parameters and formulas are product
behavior. Fix the code correctness (a bug where the formula crashes with
zero rare_examples), but do not change what the formula computes.

---

## What Claude CAN Do (Without Approval)

- Fix a bug where `max_weight` throws `DivideByZeroException` (code defect)
- Fix a bug where `OperationResult<T>` returns wrong error code (code defect)
- Fix a bug where an algorithm crashes on empty input (code defect)
- Add new test coverage for existing behavior
- Refactor code that does not change observable output
- Implement an algorithm exactly as specified in the experiment plan

## What Claude CANNOT Do (Requires Luba Approval)

- Change any numeric threshold that affects experiment Pass/Fail outcomes
- Change the max_weight formula behavior (only fix crashes/edge cases)
- Add or remove required fields from subproject interfaces
- Choose which experiment to implement next when multiple are available
- Decide a failed experiment is actually FAILED vs needs more data
- Change which subprojects are in scope
- Choose between alternative algorithm approaches without presenting them first

---

## Baseline Regression Check

Before any change to an algorithm that affects numeric output:

```bash
# Record current output for existing experiments
dotnet test --filter "ExperimentsV2Tests" Tests/LlmFromScratch.Tests.csproj \
  --verbosity normal 2>&1 | grep "Passed\|Failed"

# Find which subprojects will be affected
grep -rn "AlgorithmClassName" . --include="*.cs" | grep -v "/bin/\|/obj/\|Test"
```

If the change touches anything that affects test assertions: **STOP and ask Luba.**

---

## Decision Log Requirement

Every time Claude is tempted to change a parameter and does not (because this
rule applies), log it:

```
AVOIDED-PRODUCT-DECISION: [date]
File: [file]
What I almost changed: [description]
Why I didn't: no-product-decisions rule
What Luba should decide: [the actual question]
```

Write this to DECISIONS-LOCKED.md so Luba can review and decide explicitly.

---

## Reference Files

| File | Read When |
|------|-----------|
| `docs/DECISIONS-LOCKED.md` | Before any change to algorithm parameters |
| `docs/EXPERIMENT-RESULTS-MAP.md` | Before changing experiment Gate B criteria |
| `CLAUDE.md` Rule 7/8 | Every placement needs a metric + Gate B |
