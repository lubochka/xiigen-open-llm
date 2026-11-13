# Quality Improvement Cycle

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

## Core Problem

A generated or trained capability that scores below its threshold on first evaluation
must be improved iteratively. Repeating the same evaluation without changing anything
produces the same score. This skill defines the exact procedure: identify what failed,
make a targeted change, re-evaluate, and either promote or escalate.

---

## When to Invoke

- When `ContinuousLearningPipeline` evaluation scores a capability below the promotion threshold
- When `IncrementalTrainer` produces a result that fails its Gate B assertions
- When a generated output is incorrect and a retraining cycle is needed
- When `dotnet test` passes but behavioral output does not meet the expected standard

Do NOT invoke when the evaluation score is already at or above the promotion threshold.
Do NOT invoke when systematic-debugging-SKILL has identified an architectural root cause
(architectural fixes come before iteration cycles).

---

## Section 1 — Score Band Protocol

Before starting a cycle, classify the score into a band.

| Score | Band | Action |
|-------|------|--------|
| ≥ promotion threshold | PASS | Promote to ACTIVE. No cycle needed. |
| 70–84% of threshold | NEAR-PASS | Run one targeted improvement. Re-evaluate. |
| 60–69% of threshold | PATCH | Run up to 2 targeted improvements. Escalate after 2. |
| < 60% of threshold | STOP | Do NOT run improvement cycles. Escalate. Root cause is architectural. |

Record: `cycle_[N]_score = [value]` before each cycle.

The maximum cycle count per capability is 3. After 3 cycles without reaching
the promotion threshold: escalate. Three cycles without improvement = the problem
is not in the training data — it is in the capability design.

---

## Section 2 — The 9-Step Cycle

### Step 1 — Confirm the Score Band

Read the evaluation output. Extract the score value.
Classify into the band table above. If STOP or PASS, do not continue.

### Step 2 — Read Failing Assertions from Evaluation Output

From `dotnet test` output or `ContinuousLearningPipeline` evaluation report:
```
[FAIL] CapabilityName.Evaluates_Correctly
  Expected: OperationResult.IsSuccess == true, Value == [expected]
  Actual:   OperationResult.IsSuccess == true, Value == [actual — wrong value]

[FAIL] CapabilityName.Produces_No_Violations
  Violation: "IronRule-3 violated — hardcoded threshold found"
```

Extract:
- Failing assertion IDs
- Failure messages (the exact symptom)
- Which capability component is failing

### Step 3 — Classify the Failure Type

| Failure type | Symptom | Fix direction |
|---|---|---|
| Missing rule in training data | Same violation every cycle | Add a negative example to training data |
| Missing positive example | Capability doesn't learn correct pattern | Add a positive example to training data |
| Wrong configuration value | Output has wrong constant, not wrong structure | Check freedom-machine-SKILL — this may be FREEDOM config, not training |
| Architectural issue | Same score after 2 cycles | Escalate — not a training data issue |

### Step 4 — Read Current Capability State

```bash
dotnet test --filter "FullyQualifiedName~[CapabilityName]Tests" --no-build
# Read current test output to verify which assertions pass and which fail
grep -n "NegativeExample\|PositiveExample\|TrainingPair" src/[Project]/[CapabilityName].cs
# Verify which training examples exist
```

### Step 5 — Generate the Fix

For a **missing negative example** (capability learns wrong pattern):
```csharp
// TrainingPair: chosen=correct behavior, rejected=wrong behavior that was observed
var trainingPair = new DpoTrainingPair(
    chosen: [correct OperationResult value],
    rejected: [incorrect OperationResult value that was produced],
    context: "[what was input that triggered the wrong output]"
);
```

For a **missing positive example** (capability doesn't know the right pattern):
```csharp
// Add a test that asserts the correct output explicitly
[Fact]
public void CapabilityName_Produces_Correct_Output_For_[Scenario]()
{
    // Arrange: [specific input]
    // Act: invoke the capability
    // Assert: specific output value, not just IsSuccess
}
```

For a **wrong configuration value** (freedom-machine boundary error):
- Do NOT add training data
- Fix the `appsettings.json` FREEDOM value or the `const` MACHINE value
- Re-run evaluation — if it fails again, the issue is in capability logic, not config

### Step 6 — Apply the Fix

Write the fix to the appropriate file. DNA compliance required:
- Rule 1: OperationResult<T> return type preserved
- Rule 3: comment on every new line explaining what it contributes

### Step 7 — Verify Version Bump

If the training data changed, increment the capability version in any tracking file or STATE-Pn.json:
```
cycle_[N]_version: "1.[N].0"
cycle_[N]_fix: "[short description of what was added]"
```

Do not re-evaluate without recording what changed. Same fix + same evaluation = same result.

### Step 8 — Re-Run Evaluation

```bash
dotnet test --filter "FullyQualifiedName~[CapabilityName]" → record new count
```

Read the output. Apply the score band classification again.

### Step 9 — Compare and Decide

| Result | Action |
|--------|--------|
| Score ≥ promotion threshold | PASS — promote to ACTIVE. Record DPO triple if ContinuousLearningPipeline is active. |
| Score improved but below threshold, cycle count < 3 | Record cycle. Return to Step 2. |
| Score same (±2%) | PLATEAU — not a training data issue. Escalate. |
| Score worse | REGRESSION — escalate immediately. Note which change may have caused it. |
| Cycle count = 3, still below threshold | ESCALATE — 3 cycles is the maximum. |

---

## Section 3 — Cycle Log

Maintain per capability:

```
[CapabilityName] Cycle Log:
  Cycle 1: score=68%, violations=[IronRule-3, IronRule-6], fix=added negative example for IronRule-3
  Cycle 2: score=74%, violations=[IronRule-6], fix=added positive+negative example for IronRule-6
  Cycle 3: score=88%, violations=[], PASSED → promoted to ACTIVE
```

---

## Section 4 — Escalation Report Format

When escalation is required (score < 60%, or plateau, or 3 cycles without promotion):

```
ESCALATION REQUIRED
Capability:     [CapabilityName]
Final score:    [value] (cycle [N])
Lowest component: [which assertion category has lowest pass rate]
Still failing:  [IronRule or assertion] — [N] cycles did not fix this
Root cause assessment: [architectural / training data design / capability interface]
Fixes tried:    [list of changes made]
Last test output: [attach dotnet test output]
Recommended action: [specific next step — redesign interface / rewrite capability / change architecture]
```

---

## Anti-Patterns

1. **"Run another cycle without changing anything"** — same inputs produce same outputs. A cycle without a fix is waste.

2. **"Fix the output file directly"** — fix the capability source, not the output. Fixing output without fixing the source means the next evaluation will fail again.

3. **"Continue past 3 cycles"** — 3 cycles without promotion means the training data strategy is wrong, not the specific examples. The capability needs architectural review.

4. **"Score below 60% → still run cycles"** — below 60% is an architectural issue. No number of training example additions will fix a broken capability design.

---

## Integration

- improvement-measurement-SKILL.md requires before-and-after score values from this skill's cycle log
- systematic-debugging-SKILL.md handles the escalation path (architectural root cause)
- training-data-gap-audit-SKILL.md classifies whether the training pairs being added are RECOVERABLE or CONTAMINATED
- coverage-completeness-gate-SKILL.md checks that promotion claims come with cycle log evidence
