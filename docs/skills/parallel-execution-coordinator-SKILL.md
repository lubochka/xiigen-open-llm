# Parallel Execution Coordinator

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

A parallelWave value selects wave-accounting conventions only; it never authorizes concurrent agent execution. Permanent OP-65 (AGENTS.md) caps delegated agents at one total across all roles, tools, workflows, sessions and workspaces; spawn/resume share the slot, and pending, canceling, unknown or unverified termination blocks another launch. No task/state/wave exception applies; separate reviews remain sequential.
This skill keeps artifact numbering, gate accounting and lifecycle state safe when
a multi-phase wave plan is processed (without agent fan-out):
(1) artifact numbering must use pre-allocated ranges rather than live queries,
(2) gate checks must measure deltas rather than absolute baselines, and
(3) lifecycle writes must use compare-and-swap (CAS) semantics rather than blind writes.
Without this skill, a phase may race for artifact numbers, use stale
baseline checks, or overwrite shared lifecycle state, including during non-agent CI concurrency.

---

## When to Invoke

- When `parallelWave` is set in STATE.json, to select accounting conventions only (never agent concurrency)
- Before Phase A of a phase assigned a pre-allocated wave range
- When multiple subproject phases are grouped in a wave plan; agent phases still run sequentially
- When a phase gate needs delta accounting before the grouped phases are integrated

---

## Section 1 — Three Questions at Session Start

Read STATE.json. Answer all three before any Phase A action:

### Question 1: Does `parallelWave` select multi-phase accounting?

```
parallelWave: null → SEQUENTIAL ACCOUNTING
  Use the absolute phase gate and live artifact boundaries; use CAS for lifecycle writes whenever another writer could race.
  Multi-phase accounting conventions are dormant.

parallelWave: N (any number) → MULTI-PHASE ACCOUNTING
  Use phase-delta gates, pre-allocated artifact ranges and CAS where shared writes may race.
  This value is planning/accounting metadata only; it never opens another agent slot. Ordinary CI/product concurrency remains governed by its own controls.
```

### Question 2: What are my artifact ranges?

```
MULTI-PHASE ACCOUNTING only (`parallelWave` does not authorize agent fan-out):

1. Read docs/SUBPROJECT-MAP.md → the pre-allocated artifact range assigned to this phase

2. If subproject has no pre-allocated range entry:
   ⛔ STOP — pre-allocation planning has not run for this wave.
   Do NOT query live docs. Do NOT estimate ranges.
   Report: "[SubprojectName] has no pre-allocated artifact ranges.
            Use the wave's pre-allocation planning step first; delegated agents still run sequentially under OP-65."

3. If entry exists: use those test count, capability count, phase count ranges exclusively.
   Do NOT verify against live docs. The ranges are immutable once allocated.

4. If a collision is detected during Phase A (artifact ID already claimed):
   ⛔ STOP — report conflict. Do not overwrite. Await instruction.
```

### Question 3: Have I verified the prerequisite subproject?

```
MULTI-PHASE ACCOUNTING only (Phase A, before any other action):

Read prerequisiteSubproject from STATE.json.
If present: verify prerequisite subproject Gate B shows PASS before proceeding.
If prerequisite Gate B has not passed:
   ⛔ STOP — prerequisite subproject phase is incomplete.
   Report which subproject and phase is blocking.
   Do not proceed.
```

---

## Section 2 — Three Accounting Conventions Selected by `parallelWave`

### Switch 1: Gate model (absolute → delta)

```
SEQUENTIAL (parallelWave: null):
  dotnet test
  Assert: total passing ≥ {absolute_baseline}

MULTI-PHASE ACCOUNTING (`parallelWave: N`; agent execution remains sequential):
  dotnet test --filter "[SubprojectName]"
  Assert: {subproject_tests} shows {expected_delta} new tests passing
  Total baseline is NOT checked here — CI verifies it post-merge.
```

Why: in a multi-phase wave plan, other subproject phases may not yet be integrated. The absolute
total is unknowable until all grouped phases are integrated; this does not require concurrent agents.

### Switch 2: Artifact number assignment (live query → pre-allocated)

```
SEQUENTIAL (parallelWave: null):
  Read DECISIONS-LOCKED.md → last D-NN + 1 (live query, safe)

MULTI-PHASE ACCOUNTING (`parallelWave: N`; agent execution remains sequential):
  Read pre-allocated range for this subproject from docs/SUBPROJECT-MAP.md
  Use next number from range only — do NOT query live docs for "last used"
  (a prior phase or concurrent non-agent process may already have claimed numbers in the shared range)
```

### Switch 3: Lifecycle status write (blind write → CAS)

```
SEQUENTIAL (parallelWave: null):
  Write subproject status → ACTIVE using CAS if another shared-state writer could race; blind write only with verified exclusive ownership

MULTI-PHASE ACCOUNTING (`parallelWave: N`; agent execution remains sequential):
  Write subproject status → ACTIVE, expected current status = NOT_STARTED (CAS)
  If CAS fails because a shared-state writer/process changed the status since the expected value:
   ⛔ STOP — conflict detected. Report and await instruction.
  If CAS succeeds: continue.
```

---

## Section 3 — Multi-Phase Wave Accounting Log

When `parallelWave` selects multi-phase accounting, record the phase delta as well as the wave identity:

```json
{
  "parallelWave": 2,
  "waveBaselineEntry": 916,
  "testDelta": 12,
  "testBaselineExit": null
}
```

`testBaselineExit` may be null while phases are not yet integrated — the final total is unknown until
all grouped phases are integrated. `testDelta` is the count this phase added; agent execution remains sequential.

---

## Section 4 — Worked Examples

**Correct multi-phase accounting sequence (agent phases still run one at a time under OP-65):**
```
1. Read STATE.json → parallelWave = 2 → multi-phase accounting conventions active, not agent fan-out
2. Check prerequisite → 21.AgentInfrastructure Phase 3 → Gate B = PASS → proceed
3. Read pre-allocated ranges → use decision numbers D-15..D-18 for this subproject
4. CAS write: subproject status NOT_STARTED → ACTIVE → { success: true } → continue
5. Implement phase, run tests
6. Gate check: dotnet test --filter "AgentAlgorithms" → +12 new tests pass ✅
7. Record testDelta = 12, testBaselineExit = null
```

**Collision example — live allocation despite a pre-allocated wave range:**
```
1. Read STATE.json → parallelWave = 2
2. Query live DECISIONS-LOCKED.md → returns D-14 (last used)
   But a prior phase or concurrent CI/non-agent process already claimed D-15 and D-16
3. Register D-15 → COLLISION with the other instance
→ Decision log corrupted, duplicate D-15 entries
```

---

## Anti-Patterns

1. **"Using only the absolute baseline for each phase in multi-phase accounting"** — an unintegrated phase total is not the wave total
   Use a delta gate per phase, then retain the required aggregate absolute gate after integration.

2. **"Reading live docs for a pre-allocated wave phase"** — another prior phase or concurrent non-agent process
   may claim the same identifiers. Use the assigned immutable ranges.

3. **"Blind lifecycle status write when a shared-state race is possible"** — use CAS whenever another writer could race.
   A blind write can overwrite another process's claim.

4. **"Proceeding when pre-allocated ranges are missing"** — STOP. Missing pre-allocation
   means the planning session that assigns ranges has not run. Do not estimate.

---

## Integration

- docs/SUBPROJECT-MAP.md stores pre-allocated ranges for multi-phase wave planning; these do not authorize concurrent agents
- Gate B per phase (plan-review-SKILL.md) uses delta measurement for unintegrated phases; the aggregate absolute gate remains
- planning-session-startup-SKILL.md Q0 reads STATE.json for accounting metadata only; `parallelWave` never changes OP-65
- phase-retrospective-SKILL.md Dimension 4 (Gate B adherence) notes delta vs absolute gate
