# Subproject Completeness Checker

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

A subproject plan that passes internal consistency checks can still fail to
deliver the user's stated goals — if those goals were never mapped to specific
phases. And a plan that maps goals to phases can still fail if the design
artifacts those phases depend on are empty stubs. This skill adds two structural
pre-gates that run before all other checks: goal-to-phase mapping and design
artifact population.

---

## When to Invoke

- "Is this subproject plan ready for implementation?"
- "What's missing from this phase plan?"
- After any phase plan is produced or updated
- Before starting Phase 1 of any multi-phase plan
- When plan-self-validation-SKILL.md returns incomplete results

One completeness run before implementation = zero "phases complete but goals not delivered" failures.

---

## Section 1 — The Pre-Gate Checks (Run First)

These two checks block everything else. A plan that fails either is not ready
for implementation regardless of how well the remaining checks pass.

### Pre-Gate 1 — Goal-to-Phase Mapping

**What it verifies:** every goal element stated by Luba has at least one phase
assigned to it and a verification step specified.

**Protocol:**
1. Load the verbatim goal statement from the session's goal context record
2. Decompose into discrete goal elements
3. For each goal element, identify which phase(s) deliver it
4. Verify each mapped goal has a concrete verification step (dotnet test filter, build count, or observable output)

**Output format:**

```markdown
| Goal Element | Phase(s) | Verification Step | Verdict |
|---|---|---|---|
| GE-1: [text] | Phase 2, Phase 3 | dotnet test --filter → 5 passed | APPROVED |
| GE-2: [text] | — | — | BLOCK — goal not covered |
```

**FAIL** = any goal element has zero phases assigned or no verification step.
Pre-Gate 1 failure blocks the plan regardless of all other checks.

**Resolution:**
- Add phases that deliver the uncovered goal element, OR
- Explicitly defer with Luba's approval (record as DEFERRED with reason in the session)

---

### Pre-Gate 2 — Design Artifact Populated

**What it verifies:** every artifact referenced in the plan passes
design-artifact-completeness-SKILL.md Check 1 (exists) and Check 2 (populated).

**Protocol:**
- For each interface referenced in the plan: Check 1 + Check 2 via design-artifact-completeness-SKILL.md
- For each test class referenced: Check 2 (≥1 [Fact])
- For each service registration referenced: Check 5 (reachable)

**FAIL** = any referenced artifact is MISSING or STUB.
Resolution: implement the stub first, OR scope the plan around the gap with explicit acknowledgment.

---

## Section 2 — Technical Completeness Checks (Run After Pre-Gates Pass)

### T1 — Phase structure declared
Each phase has: goal, output files, Gate B condition, and prerequisite list.

### T2 — Gate B is measurable
Every phase has exactly one Gate B expressed as:
- `dotnet test --filter "[filter]" → [N] passed, 0 failed`, or
- `dotnet build → 0 Error(s)`, or
- a specific count or assertion

"Works correctly" is not a Gate B. "Passes" without a filter is not a Gate B.

### T3 — Prerequisites verified
Every phase that depends on another phase's output has that phase listed as a prerequisite. No phase assumes an artifact that does not exist.

### T4 — OperationResult<T> usage declared
Any new public method in any phase returns `OperationResult<T>`. Phases that add throw-based logic fail T4.

### T5 — Interface registration planned
Any new class that implements an interface has a registration step in its phase plan (or the DI registration phase is explicitly called out).

### T6 — Test categories declared
For any capability with writes, timers, or multi-step operations, the phase plan names which of the 8 e2e-test-matrix-SKILL.md categories apply.

### T7 — Time-based tests use injectable clock
Any capability with a TTL, timer, or scheduled operation has `IDateTimeProvider` injection declared in the plan (never `Thread.Sleep`).

### T8 — Compensation chain order declared
Any capability with multi-step rollback declares LIFO (Last-In First-Out) order explicitly.

### T9 — DNA Rule compliance
All 10 DNA rules satisfied:
- Rule 1: every public method returns OperationResult<T>
- Rule 2: fabric-first (interfaces, not concrete types)
- Rule 3: every production line has a comment
- Rule 10: no external ML frameworks

### T10 — Scope discipline
The plan does not touch subprojects outside its declared scope. Any cross-subproject effect is called out with explicit justification.

---

## Section 3 — Completeness Score Format

```markdown
## Subproject Completeness Check — [Plan Name]

Pre-Gate 1 (Goal-to-Phase Mapping): PASS / FAIL
Pre-Gate 2 (Design Artifact Populated): PASS / FAIL

If Pre-Gate 1 or Pre-Gate 2 FAIL: stop here. Do not evaluate T1–T10.

Technical completeness:
| Check | Status | Note |
|---|---|---|
| T1 — Phase structure | PASS / FAIL | [detail] |
| T2 — Gate B measurable | PASS / FAIL | [detail] |
| T3 — Prerequisites verified | PASS / FAIL | [detail] |
| T4 — OperationResult<T> | PASS / FAIL | [detail] |
| T5 — Interface registration | PASS / FAIL | [detail] |
| T6 — Test categories | PASS / FAIL | [detail] |
| T7 — Injectable clock | PASS / FAIL | [detail if applicable] |
| T8 — Compensation LIFO | PASS / FAIL | [detail if applicable] |
| T9 — DNA compliance | PASS / FAIL | [detail] |
| T10 — Scope discipline | PASS / FAIL | [detail] |

Score: [N]/12 checks pass

Overall verdict: READY / NOT_READY
Blocking items: [list]
Next step: [resolve blocking items / proceed to Phase 1]
```

---

## Anti-Patterns

1. **"The plan covers the goals implicitly"** — implicit coverage is the failure mode.
   Goal-to-phase mapping must be explicit. If it is not written, it does not exist.

2. **"Gate B is 'the tests pass'"** — a Gate B without a filter and count is not checkable.
   Which tests? How many? "The tests pass" could mean 1 or 100.

3. **"We can add the test categories after implementation"** — the test matrix is declared
   before coding so that implementation satisfies the declared behavior. Declaring it after
   means testing what was implemented, not what was required.

4. **"The artifact will be implemented in a later phase"** — if Phase 2 depends on an artifact
   that Phase 1 produces, Phase 1 must be listed as a prerequisite. Silent dependencies
   cause Phase 2 to fail when Phase 1 hasn't run.

---

## Integration

- plan-self-validation-SKILL.md Check 1 is the goal delivery version of Pre-Gate 1
- design-artifact-completeness-SKILL.md provides Checks 1–5 for Pre-Gate 2
- e2e-test-matrix-SKILL.md governs T6 (test category declaration)
- goal-delivery-completeness-SKILL.md provides the arbiter that runs Pre-Gate 1 formally
- claim-as-hypothesis-SKILL.md provides the DEFERRED mechanism for Pre-Gate 1 gaps
