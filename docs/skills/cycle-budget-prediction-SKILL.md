# Cycle Budget Prediction

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
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

A phase plan that under-estimates cycles runs out of budget mid-implementation
and ships a partial result. A phase plan that over-estimates cycles wastes
allocation and delays downstream phases. Both errors compound across batches
because phase-retrospective-SKILL.md uses the predicted budget to compute MAE
(Mean Absolute Error — the average gap between predicted and actual cycles).
This skill produces a deterministic, auditable cycle budget before any
implementation begins.

---

## When to Invoke

- Before writing any phase plan that includes new capability work
- When plan-self-validation-SKILL.md encounters an unbudgeted phase
- When phase-retrospective-SKILL.md shows MAE > 1.0 for the last batch
- When a capability class appears for the first time in the session

---

## Section 1 — Cycle Budget Formula

```
cycleBudget = archetypeBaseline
            + firstOccurrencePenalty
            + noveltySum
            − clarityDiscount
            + inversionBonus
```

### Component Definitions

| Component | Value | When Applied |
|---|---|---|
| `archetypeBaseline` | 1 | Always — every capability class starts here |
| `firstOccurrencePenalty` | +1 | Capability class has never been built in this project before |
| `noveltySum` | +0.5 per new pattern | Each new fabric interface, behavioral invariant, or DNA constraint introduced for the first time |
| `clarityDiscount` | −1 | The capability can be implemented correctly from iron rules alone — no novel judgment required |
| `inversionBonus` | +1 | Correct behavior inverts the DNA default (rare — e.g., a capability that must NOT use OperationResult<T> at a boundary that normally requires it) |

**Maximum budget: 4** (all penalties, zero discount)
**Minimum budget: 1** (clarity discount, no penalties)

---

## Section 2 — Component Derivation Rules

### archetypeBaseline = 1

Every capability class gets one cycle regardless of complexity.
A cycle is one complete implementation attempt with a Gate B result.

### firstOccurrencePenalty (+1)

Add this when:
- The capability class does not appear in the current `DomainModelRegistry`
  (status STUB or MISSING)
- No prior phase in the current subproject has built this class before

Do NOT add when:
- A prior phase built the same class, even if this phase adds new methods
- The capability is being extended, not created from scratch

### noveltySum (+0.5 per new named pattern)

A "new named pattern" is a specific, named design artifact that does not yet
exist in the codebase. Count each of the following if new:

- New fabric interface (e.g., `INewCapability`) — not yet in `src/`
- New behavioral invariant (an iron rule not in `docs/DECISIONS-LOCKED.md`)
- New DI registration pattern (different from all existing `Program.cs` registrations)
- New OperationResult<T> namespace boundary crossing

Count 0.5 per item. Round total to nearest 0.5.

### clarityDiscount (−1)

Subtract 1 when ALL of the following are true:

1. Every method signature is already defined in a verified interface
2. The algorithm is derivable from existing iron rules without novel judgment
3. No new behavioral invariants are required
4. At least one structurally identical capability already exists in the codebase
   (same pattern, different domain)

If any condition is false: no discount.

### inversionBonus (+1)

Add this only when correct behavior requires explicitly overriding a DNA rule
at a specific boundary. This is rare. Document the override in
`docs/DECISIONS-LOCKED.md` before adding the bonus.

---

## Section 3 — Budget Card Format

Produce this for every capability class before writing its phase plan:

```markdown
## Cycle Budget — [CapabilityClass]

archetypeBaseline:        1
firstOccurrencePenalty:   +N  [reason: "first time INewInterface built" / "already in registry — no penalty"]
noveltySum:               +N  [items: INewFabric (+0.5), new invariant D-13 (+0.5), ...]
clarityDiscount:          −N  [reason: "all methods defined, structurally identical to Bm25Scorer" / "not applicable"]
inversionBonus:           +N  [reason: "requires overriding Rule 1 at test boundary — see ADR in D-13" / "not applicable"]

Predicted cycle budget:   N

Gate B for this budget:
  "Phase [X] completes in ≤ N cycles with ≥ 1 [Fact] test passing per cycle"
```

---

## Section 4 — Budget-to-Phase Wiring

After computing the budget, wire it into the phase plan:

```markdown
Phase [N] — [CapabilityClass]
Cycle budget: [predicted] cycles (cycle-budget-prediction-SKILL.md)
Gate B: [exact passing condition — count or assertion]
Retrospective feed: phase-retrospective-SKILL.md Dimension 1 will record actual vs predicted
```

The predicted budget feeds `taskTypeCycleBudgets` in the session state.
After the phase completes, the actual cycle count is recorded in the
phase-retrospective-SKILL.md output for MAE tracking.

---

## Section 5 — MAE Recalibration Trigger

If phase-retrospective-SKILL.md Dimension 1 shows MAE > 1.0 for the last batch:

1. List all phases where `|predicted − actual| > 1`
2. For each, identify which component was wrong (archetypeBaseline over-estimated?
   noveltySum missed a new pattern?)
3. Adjust component weights for the next batch:
   - If firstOccurrencePenalty was consistently wrong: record in DECISIONS-LOCKED.md
   - If noveltySum items were double-counted: add de-duplication rule to this skill
4. Do NOT adjust baseline below 1

---

## Anti-Patterns

1. **"This is a simple capability, budget = 0"** — archetypeBaseline is always 1.
   Even a trivially simple capability takes one cycle to verify via Gate B.

2. **"I'll set budget = 5 to be safe"** — budget caps at 4. A budget > 4 means the
   capability needs to be split into smaller pieces. Split it before budgeting.

3. **"No Gate B needed for this phase"** — the budget card requires a Gate B. A phase
   with no Gate B cannot feed phase-retrospective-SKILL.md Dimension 1.

4. **"noveltySum = 0 because all interfaces exist"** — check behavioral invariants too.
   A new iron rule is a new named pattern even if all interfaces are pre-existing.

5. **"MAE > 1.0 but the predictions were close enough"** — MAE > 1.0 is a hard
   recalibration trigger (see Section 5). There is no "close enough" for budget accuracy.

---

## Integration

- phase-retrospective-SKILL.md Dimension 1 (cycle count accuracy / MAE) consumes the predicted budget
- plan-self-validation-SKILL.md Phase 0 checks that every phase has a cycle budget declared
- iron-rule-derivation-SKILL.md produces the behavioral invariants counted in noveltySum
- docs/DECISIONS-LOCKED.md is the source for "has this iron rule been recorded before?"
- DomainModelRegistry status (STUB/MISSING) determines firstOccurrencePenalty
