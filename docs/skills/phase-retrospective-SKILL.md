# Phase Retrospective

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the equivalent in the operator's language), challenges an answer, or says a failure came
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

A set of phases that completes successfully does not automatically produce
calibrated predictions for the next set. Without a retrospective, cycle count
predictions stay wrong, pattern transfer is unverified, and the next wave of
work starts with stale assumptions. This skill defines a hard gate: measure
before proceeding, not after.

---

## When to Invoke

- After the last phase in a sequential batch completes (Gate B passes for all phases)
- Before the next batch of parallel phases begins
- This is a hard gate: the next batch does not start until the retrospective is recorded

```
Sequential batch:  Phase 1 → Phase 2 → Phase 3
                   ↓
                   Retrospective runs here
                   ↓
Next batch begins: Phase 4, Phase 5, Phase 6 (parallel)
```

---

## Section 1 — Four Measurement Dimensions

### Dimension 1 — Cycle Count Accuracy

For each phase in the completed batch, compare predicted cycles to actual cycles:

```
Phase 1: predicted=2 cycles, actual=3 cycles
Phase 2: predicted=2 cycles, actual=1 cycle
Phase 3: predicted=1 cycle, actual=1 cycle
```

**MAE (Mean Absolute Error) calculation:**
```
errors = [|predicted - actual|] for each phase
MAE = sum(errors) / count(phases)
```

**Threshold:** MAE > 1.0 → recalibrate cycle predictions before the next batch.

Recalibration means: identify which capability classes consistently need more
cycles than predicted. Common causes:
- Interfaces that require more infrastructure than anticipated (Axis 3 underestimated)
- Test categories (e2e-test-matrix-SKILL.md) that surface more edge cases than predicted
- ContinuousLearningPipeline wiring that adds unexpected setup work

---

### Dimension 2 — Pattern Transfer Rate

Count how many PROVEN patterns have been added to DomainModelRegistry since
the last retrospective:

```csharp
// Verify DomainModelRegistry entries with status = PROVEN
var provenCount = registry.GetByStatus(CapabilityStatus.Proven).Count;
```

**Expected rate:** approximately 1-2 PROVEN promotions per 3 completed phases.

**If count < expected:** Stage 5 of capability-improvement-cycle-SKILL.md did
not complete for some capabilities. Verify ContinuousLearningPipeline is
collecting Stage 4 observations and confidence is updating.

---

### Dimension 3 — Behavioral Test Coverage

For each phase completed, check that all declared e2e-test-matrix-SKILL.md
categories have at least one passing test:

```bash
# Check test counts per category
dotnet test --filter "Category=HappyPath" → N passed
dotnet test --filter "Category=Idempotency" → N passed
dotnet test --filter "Category=TimeBased" → N passed
```

**Threshold:** Any N/A category without justification in the phase plan is a gap.
Any declared category with 0 passing tests is a Stage 3 completion failure.

---

### Dimension 4 — Gate B Adherence

For each phase, verify the Gate B condition was met as stated (not approximated):

```
Phase 1 Gate B: "dotnet test --filter 'BM25Scorer' → 12 passed, 0 failed"
Actual result: 12 passed, 0 failed → ADHERED
Phase 2 Gate B: "dotnet build → 0 Error(s)"
Actual result: 2 warnings, 0 errors → ADHERED (warnings are not errors)
Phase 3 Gate B: "dotnet test --filter 'HybridRetriever' → 8 passed"
Actual result: 7 passed → NOT_ADHERED
```

**Threshold:** Any NOT_ADHERED gate means the phase is not complete. The retrospective
notes this and the phase is re-opened before the next batch proceeds.

---

## Section 2 — Retrospective Record Format

```markdown
## Phase Retrospective — [SessionId] — Phases [N-M]

Covered phases: [Phase 1, Phase 2, Phase 3]
Date: [date]

### Dimension 1 — Cycle Count Accuracy
| Phase | Predicted Cycles | Actual Cycles | Error |
|---|---|---|---|
| Phase 1 | 2 | 3 | 1 |
| Phase 2 | 2 | 1 | 1 |
| Phase 3 | 1 | 1 | 0 |

MAE: 0.67 — within threshold (≤1.0)
Action: no recalibration needed

### Dimension 2 — Pattern Transfer Rate
PROVEN entries added since last retrospective: 2
Expected: 1-2 — PASS

### Dimension 3 — Behavioral Test Coverage
| Category | Tests | Status |
|---|---|---|
| Happy Path | 8 passed | PASS |
| Idempotency | 3 passed | PASS |
| Time-Based | 2 passed | PASS |
| Compensation | N/A — read-only capability | N/A (justified) |

### Dimension 4 — Gate B Adherence
| Phase | Gate B | Actual | Adhered |
|---|---|---|---|
| Phase 1 | 12 passed | 12 passed | YES |
| Phase 2 | 0 errors | 0 errors | YES |
| Phase 3 | 8 passed | 7 passed | NO |

Gate B NOT_ADHERED: Phase 3 — must be resolved before next batch

### Calibration Adjustments
- Dimension 1: no change
- Dimension 4: Phase 3 gate not met — reopen Phase 3, resolve 1 failing test

### Next Batch Clear to Proceed: NO
Blocking: Phase 3 Gate B not met
```

---

## Section 3 — Next Batch Gate

The next batch does not start until:

1. Retrospective record exists (this document, completed)
2. All Gate B conditions are ADHERED (Dimension 4)
3. MAE ≤ 1.0 OR recalibration adjustments recorded (Dimension 1)
4. No PROVEN-rate blocking issue (Dimension 2)

If `Next Batch Clear to Proceed: NO`:
- Fix blocking items
- Re-run the affected verification
- Update the retrospective record
- Flip the flag to YES
- Do not proceed until flag is YES

---

## Anti-Patterns

1. **"The phases all passed, skip the retrospective"** — passing phases do not produce
   calibration data automatically. The retrospective is the calibration step.

2. **"Gate B was approximately met"** — Gate B is a binary condition. 7 passed when
   8 was declared is not approximate success — it is a failed gate. Re-open the phase.

3. **"We'll do the retrospective at the end of the project"** — retrospectives are
   per-batch, not per-project. Waiting until the end means the next batch runs on
   stale calibration from the very first batch.

4. **"MAE of 1.5 is close enough"** — MAE > 1.0 means predictions are consistently off
   by more than one cycle. Recalibrate now; otherwise the next batch's timeline is
   already wrong before it starts.

---

## Integration

- capability-improvement-cycle-SKILL.md Stage 3 produces the cycle count data for Dimension 1
- e2e-test-matrix-SKILL.md declares the categories verified in Dimension 3
- plan-self-validation-SKILL.md Check 4 (execution order consistency) is a pre-requisite
  that ensures phases have Gate B conditions to check in Dimension 4
- DomainModelRegistry is the data source for Dimension 2 PROVEN counts
