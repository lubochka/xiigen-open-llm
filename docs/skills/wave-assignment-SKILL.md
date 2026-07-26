# Wave Assignment

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the same in the operator's language, e.g. "почему"), challenges an answer, or says a failure came
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

A capability that should be built as shared infrastructure gets built as a
per-subproject dependency instead. Later, five subprojects discover they all
need the same capability and each implements it differently. The wave assignment
protocol prevents this: it classifies every capability before planning begins,
identifies infrastructure candidates (N > 2 dependents), and assigns the right
sequential vs parallel execution model for each wave.

---

## When to Invoke

- Before any subproject phase is planned (wave assignment is an input to planning, not an output)
- When a new shared capability is identified that multiple subprojects might need
- When parallel-execution-coordinator-SKILL.md asks which wave a phase belongs to
- Before assigning test gate type (absolute vs delta) for a new phase

---

## Section 1 — Wave Structure

| Wave | Type | Execution | Gate model |
|---|---|---|---|
| FOUNDATION | Shared infrastructure subprojects | Sequential, must complete before Wave 0 | Absolute (dotnet test 0 failures) |
| WAVE 0 | First capability subprojects | Sequential, one at a time | Absolute |
| WAVE 1 | Second-generation capabilities | Sequential, some pattern data available | Absolute |
| WAVE 2+ | Mature capabilities | May run in parallel (up to 5 simultaneous) | Delta (test count must not decrease) |

---

## Section 2 — Rule 1: Infrastructure Test

A subproject belongs at FOUNDATION when:

> **More than 2 other planned subprojects depend on this subproject's output.**

Apply this test literally. Count downstream dependents.

```
Count downstream dependents explicitly:
  If dependents > 2: FOUNDATION (shared infrastructure — build before Wave 0)
  If dependents ≤ 2: Wave 0 or later (not infrastructure)

Examples:
  IAgentMessageBus (event bus):
    Used by: 21, 22, 23, and 3 future subprojects → 6 dependents → FOUNDATION

  ISessionStorage (session cache):
    Used by: all subprojects via BaseService → N > 2 → FOUNDATION

  WeightedScoringService:
    Used by: 22.AgentAlgorithms → 1 dependent → NOT FOUNDATION → Wave 0 or 1
```

**Edge case: expected vs current dependents**
A subproject that currently has 1 dependent but is expected to grow to 6 should
be assigned FOUNDATION based on its intended architecture, not its current state.
Wave reassignment is expensive (requires replanning). Plan for the intended architecture.

**What prevents wrong dependency direction:**
A user-facing subproject should not become FOUNDATION just because another user-facing
subproject shares patterns. The rule is directional: if FLOW-A needs FLOW-B's output
to function, FLOW-B is upstream. If FLOW-A only reuses FLOW-B's patterns without
depending on its runtime output, they are peers at the same wave.

---

## Section 3 — Rule 2: Sequential vs Parallel (Wave 2+ only)

Wave 2+ subprojects may run in parallel. The decision:

**Run in parallel when:**
- At least 3 subprojects of prior waves have completed (pattern data available)
- No cross-subproject dependency between the parallel subprojects
- The delta gate is valid (test baseline is stable — no pre-existing failures)
- Review bandwidth exists for simultaneous completion reviews

**Run sequentially when:**
- Fewer than 3 prior subprojects have completed
- Any cross-subproject dependency exists (A's runtime output feeds B's implementation)
- Pre-existing test failures (delta gate unreliable until absolute gate is clean)

**Why ≥ 3 prior completions are required for parallel:**
Parallel execution compounds benefit when prior phases produced pattern data that reduces
cycle count. Without patterns, running 5 subprojects simultaneously is 5 simultaneous
Wave 0 experiences — no compound benefit justifies the review overhead.

---

## Section 4 — Rule 3: Gate Type Selection

```
FOUNDATION:                 Absolute gate (failures === 0)
  Reason: any failure blocks ALL downstream subprojects

WAVE 0, WAVE 1:             Absolute gate (failures === 0)
  Reason: establishing the baseline — no prior history for delta comparison

WAVE 2+ sequential:         Absolute gate
  Reason: sequential is safe; no parallelism conflict

WAVE 2+ parallel:           Delta gate (test count must not decrease)
  Reason: parallel runs may not have merged; absolute total is unknowable mid-wave
```

**Delta gate definition:** `testCount at phase end ≥ testCount at phase start`.
A delta gate does NOT pass if tests were deleted to eliminate failures.

**Delta gate risk:** a delta gate can pass with 3 new failures if 4 new tests were also added.
This is why it is restricted to Wave 2+ parallel where the baseline is stable and
parallel-execution-coordinator-SKILL.md's CAS (compare-and-swap) semantics prevent races.

---

## Section 5 — Wave Assignment Record

For every subproject and every infrastructure capability, record the wave assignment
in DECISIONS-LOCKED.md or a wave assignment log:

```markdown
## Wave Assignment — [SubprojectName or Capability]

**Question:** Should [X] be FOUNDATION or Wave N?
**Downstream count:** [N subprojects depend on this]
**Reasoning:** [why this wave, not another]
**Principle applied:** [N > 2 → FOUNDATION / sequential because no pattern data / etc.]
**Gate type:** [ABSOLUTE / DELTA]
**Outcome:** [FOUNDATION / WAVE 0 / WAVE 1 / WAVE 2 parallel]

**Prerequisites (must be ACTIVE before this subproject starts):**
  - [FOUNDATION subproject A]
  - [FOUNDATION subproject B]
```

---

## Section 6 — Prerequisite Chain Format

Every subproject declares its prerequisites explicitly:

```json
{
  "subprojectId": "22.AgentAlgorithms",
  "wave": 1,
  "hardPrerequisites": ["21.AgentInfrastructure"],
  "softPrerequisites": [],
  "downstream": ["23.SemanticCompiler"]
}
```

A subproject cannot start (become ACTIVE) if any `hardPrerequisite` is not ACTIVE.
`softPrerequisites` are recommended but not blocking — document why the soft dependency
exists but is not hard.

---

## Anti-Patterns

1. **"Assigning a wave without counting downstream dependents"** —
   "this seems like infrastructure" is not the protocol. Count the dependents.
   If N > 2: FOUNDATION. Otherwise: Wave 0+.

2. **"Delaying infrastructure subprojects to keep the critical path shorter"** —
   infrastructure delays become compound blockers. Wave 0 subprojects that
   discover they need a FOUNDATION capability mid-implementation are the most
   expensive rework scenario.

3. **"Assigning Wave 2+ parallel without checking cross-subproject dependencies"** —
   if subproject A's runtime output feeds subproject B's implementation, they are
   sequential. Parallel is only safe when they are genuinely independent at runtime.

4. **"Using delta gate for Wave 0 or Wave 1"** — no baseline exists for delta
   comparison in early waves. Delta gate is undefined when prior test count = 0.
   Always use absolute gate for FOUNDATION, Wave 0, and Wave 1.

5. **"Assigning current wave, not intended wave"** — plan for the intended architecture.
   A capability with 1 dependent today but 8 expected dependents should be FOUNDATION now.
   Reassignment after implementation is expensive.

---

## Integration

- parallel-execution-coordinator-SKILL.md uses the wave assignment to determine gate model (absolute vs delta)
- shared-service-design-SKILL.md uses the appearance count (≥ 3 = FOUNDATION) which is equivalent to this skill's N > 2 rule
- prerequisite-chain-SKILL.md verifies hard prerequisites are ACTIVE before a subproject starts
- docs/SUBPROJECT-MAP.md is the canonical record of current wave assignments
- planning-session-startup-SKILL.md Q2 (baseline) checks gate model against wave assignment
