# Parallel Execution Coordinator

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

Sequential and parallel subproject execution look identical from the outside — both
have Phase A through Phase N, both produce Gate B completions. The difference is
entirely in three behaviors that must switch when parallel execution is active:
(1) artifact numbering must use pre-allocated ranges rather than live queries,
(2) gate checks must measure deltas rather than absolute baselines, and
(3) lifecycle writes must use compare-and-swap (CAS) semantics rather than blind writes.
Without this skill, a parallel executor will race for artifact numbers, apply stale
baseline checks, and overwrite concurrent lifecycle state.

---

## When to Invoke

- When a session begins with `parallelWave` set in STATE.json (or equivalent)
- Before Phase A of any parallel subproject execution
- When multiple subproject phases are planned to run concurrently
- When any gate check runs in a parallel execution session

---

## Section 1 — Three Questions at Session Start

Read STATE.json. Answer all three before any Phase A action:

### Question 1: Am I in parallel mode?

```
parallelWave: null → SEQUENTIAL mode
  Use absolute gate, live artifact boundaries, blind lifecycle write.
  This skill is dormant.

parallelWave: N (any number) → PARALLEL mode
  Switch all three behaviors in Section 2.
  This skill is active.
```

### Question 2: What are my artifact ranges?

```
PARALLEL mode only:

1. Read: docs/SUBPROJECT-MAP.md → parallel execution ranges for this subproject

2. If subproject has no pre-allocated range entry:
   ⛔ STOP — pre-allocation planning has not run for this wave.
   Do NOT query live docs. Do NOT estimate ranges.
   Report: "[SubprojectName] has no pre-allocated artifact ranges.
            Run parallel pre-allocation planning session first."

3. If entry exists: use those test count, capability count, phase count ranges exclusively.
   Do NOT verify against live docs. The ranges are immutable once allocated.

4. If a collision is detected during Phase A (artifact ID already claimed):
   ⛔ STOP — report conflict. Do not overwrite. Await instruction.
```

### Question 3: Have I verified the prerequisite subproject?

```
PARALLEL mode only (Phase A, before any other action):

Read prerequisiteSubproject from STATE.json.
If present: verify prerequisite subproject Gate B shows PASS before proceeding.
If prerequisite Gate B has not passed:
   ⛔ STOP — prerequisite subproject phase is incomplete.
   Report which subproject and phase is blocking.
   Do not proceed.
```

---

## Section 2 — Three Behavior Switches in Parallel Mode

### Switch 1: Gate model (absolute → delta)

```
SEQUENTIAL (parallelWave: null):
  dotnet test
  Assert: total passing ≥ {absolute_baseline}

PARALLEL (parallelWave: N):
  dotnet test --filter "[SubprojectName]"
  Assert: {subproject_tests} shows {expected_delta} new tests passing
  Total baseline is NOT checked here — CI verifies it post-merge.
```

Why: in parallel mode, other subproject phases may not yet be merged. The absolute
total is unknowable until all parallel phases complete and merge.

### Switch 2: Artifact number assignment (live query → pre-allocated)

```
SEQUENTIAL (parallelWave: null):
  Read DECISIONS-LOCKED.md → last D-NN + 1 (live query, safe)

PARALLEL (parallelWave: N):
  Read pre-allocated range for this subproject from docs/SUBPROJECT-MAP.md
  Use next number from range only — do NOT query live docs for "last used"
  (another parallel instance may have claimed numbers from the live range)
```

### Switch 3: Lifecycle status write (blind write → CAS)

```
SEQUENTIAL (parallelWave: null):
  Write subproject status → ACTIVE  (blind write — no concurrent writers possible)

PARALLEL (parallelWave: N):
  Write subproject status → ACTIVE, expected current status = NOT_STARTED (CAS)
  If CAS fails (another instance already claimed):
   ⛔ STOP — conflict detected. Report and await instruction.
  If CAS succeeds: continue.
```

---

## Section 3 — Execution Log in Parallel Mode

In parallel mode, the execution log must include the delta, not the absolute:

```json
{
  "parallelWave": 2,
  "waveBaselineEntry": 916,
  "testDelta": 12,
  "testBaselineExit": null
}
```

`testBaselineExit` is null in parallel mode — the final total is unknown until
all parallel phases complete. `testDelta` is the count this phase added.

---

## Section 4 — Worked Examples

**Correct parallel execution sequence:**
```
1. Read STATE.json → parallelWave = 2 → PARALLEL mode active
2. Check prerequisite → 21.AgentInfrastructure Phase 3 → Gate B = PASS → proceed
3. Read pre-allocated ranges → use decision numbers D-15..D-18 for this subproject
4. CAS write: subproject status NOT_STARTED → ACTIVE → { success: true } → continue
5. Implement phase, run tests
6. Gate check: dotnet test --filter "AgentAlgorithms" → +12 new tests pass ✅
7. Record testDelta = 12, testBaselineExit = null
```

**Wrong — parallel mode using sequential behaviors:**
```
1. Read STATE.json → parallelWave = 2
2. Query live DECISIONS-LOCKED.md → returns D-14 (last used)
   But another parallel instance already claimed D-15 and D-16
3. Register D-15 → COLLISION with the other instance
→ Decision log corrupted, duplicate D-15 entries
```

---

## Anti-Patterns

1. **"In parallel mode, running absolute gate check"** — absolute baseline is unknowable
   in parallel mode. Use delta gate only.

2. **"Reading live docs for artifact numbers in parallel mode"** — another parallel instance
   may be reading the same docs simultaneously. Use pre-allocated ranges.

3. **"Blind lifecycle status write in parallel mode"** — CAS is mandatory in parallel mode.
   A blind write overwrites another instance's claim.

4. **"Proceeding when pre-allocated ranges are missing"** — STOP. Missing pre-allocation
   means the planning session that assigns ranges has not run. Do not estimate.

---

## Integration

- docs/SUBPROJECT-MAP.md stores the pre-allocated ranges for parallel execution phases
- Gate B per phase (plan-review-SKILL.md) switches to delta measurement in parallel mode
- planning-session-startup-SKILL.md Q0 (mode + scope) reads STATE.json to detect parallelWave
- phase-retrospective-SKILL.md Dimension 4 (Gate B adherence) notes delta vs absolute gate
