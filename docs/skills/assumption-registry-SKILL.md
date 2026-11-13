# Assumption Registry

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

## When to Invoke

After a plan is produced, before session or EXECUTION files are written.
The assumption registry feeds into phase-preflight-SKILL.md, which executes
the verification commands at the start of the Claude Code session.

---

## Why This Exists

Three blocking issues appeared in planning reviews:
1. A method assumed to exist in a class — it didn't (renamed last phase)
2. A DI registration assumed to be in `Program.cs` — it wasn't
3. A field assumed to be a property — it was a method (wrong access pattern)

All three were discoverable in 30 seconds with a grep. None were checked at
plan authoring time. Each caused a blocked session. The cost of a wrong
assumption discovered in review: 1 review cycle. Discovered at execution:
blocked session + re-plan + re-implementation.

---

## The Three Assumption Classes

### Class 1 — INFRASTRUCTURE (verifiable by grep or file existence)

Something must exist in the codebase that the plan depends on.

```
Example:  "ISessionStorage.ListSessionsAsync() method exists"
Verify:   grep -n "ListSessionsAsync" Src/21.AgentInfrastructure/ISessionStorage.cs
Expected: 1+ hits
Fallback: If 0 hits — BLOCKING: add ListSessionsAsync to ISessionStorage before this phase
```

### Class 2 — BEHAVIORAL (verifiable by reading the code)

Something must behave a certain way — a method signature, a field name, return type.

```
Example:  "IVectorIndex.Search() returns OperationResult<List<SearchResult>>"
Verify:   grep -n "OperationResult.*Search\|Search.*OperationResult" Src/09.VectorSearch/IVectorIndex.cs
Expected: OperationResult<List<SearchResult>> wrapping present
Fallback: If unwrapped — must update callers to unwrap or method must be changed
```

### Class 3 — OPERATIONAL (verifiable by running a command)

Something must build or compile for the plan to be executable.

```
Example:  "All existing tests still pass before adding new tests"
Verify:   dotnet test LlmFromScratch.sln --no-build 2>&1 | tail -3
Expected: Failed: 0
Fallback: If failures exist — BLOCKING: fix pre-existing failures before this phase
```

---

## The Assumption Registry Format

Produce an `ASSUMPTIONS` block for every EXECUTION file. It belongs in
the STEP 0 (PRE-FLIGHT VERIFICATION) section:

```markdown
## STEP 0 — PRE-FLIGHT: ASSUMPTION REGISTRY

Before executing any step, verify all assumptions. A failed assumption is BLOCKING
unless a fallback is specified. If BLOCKING and no fallback: STOP. Escalate to Luba.

| # | Assumption | Class | Verification | Expected | Fallback |
|---|-----------|-------|--------------|----------|---------|
| A1 | ISessionStorage.ListSessionsAsync exists | INFRA | `grep -n "ListSessionsAsync" Src/21.AgentInfrastructure/ISessionStorage.cs` | 1+ hits | BLOCKING — add method before proceeding |
| A2 | BM25Scorer implements IBm25Scorer | INFRA | `grep -n "class BM25Scorer.*IBm25Scorer" Src/ -r --include="*.cs"` | 1 hit | BLOCKING — implement interface first |
| A3 | IVectorIndex.Search() returns OperationResult<T> | BEHAVIORAL | `grep -n "OperationResult" Src/09.VectorSearch/IVectorIndex.cs` | 1+ hits | BLOCKING — update all callers |
| A4 | Test baseline is 0 failures | OPERATIONAL | `dotnet test --no-build 2>&1 \| tail -3` | Failed: 0 | BLOCKING — fix pre-existing failures |
```

**Each row must have:**
- A single testable claim (not "everything is set up")
- A literal shell command
- The expected output
- A fallback OR "BLOCKING"

---

## How to Identify Assumptions From a Plan

Read each step in the plan. For every step, ask:

```
1. What file does this step touch?
   → Does that file exist? (Class 1: INFRASTRUCTURE)

2. What method or field does this step call or read?
   → Does that method/field have the expected signature? (Class 2: BEHAVIORAL)
   → Will it still be there when this session runs?

3. What state must the codebase be in for this step to succeed?
   → Are all prior phases complete? (Class 1: INFRASTRUCTURE)
   → Does the build and test gate pass? (Class 3: OPERATIONAL)
```

For every YES answer: the thing in question is an assumption. Register it.
For every assumption: write the verification command.

---

## Assumption Registry vs. Simple File Check

Current approach (weak):
```bash
ls Src/21.AgentInfrastructure/SessionStateStore.cs
# Expected: file exists
```

Assumption registry approach (strong):
```bash
# A1: SessionStateStore.SaveAsync() exists and accepts correct parameters
grep -n "public.*SaveAsync.*string.*object\|SaveAsync.*id.*state" Src/21.AgentInfrastructure/SessionStateStore.cs
# Expected: 1+ hits — method signature present with correct parameters
# Fallback: If 0 hits — check if signature changed; update callers before Phase 2
```

The `ls` check confirms the file exists. The assumption registry confirms
the thing the plan depends on exists INSIDE the file and has the right shape.

---

## Anti-Patterns

1. **Assumptions stated in prose** ("we assume the session store works") — every
   assumption must have a verification command. Prose assumptions are exactly the
   failure mode that caused the three blocking issues.

2. **Registering only obvious assumptions** — the non-obvious ones (signature
   changed, field renamed, namespace conflict) are the ones that cause blocked sessions.

3. **Fallback of "check manually"** — "check manually" is not a fallback. Write
   the specific command, or mark BLOCKING and escalate at planning time.

4. **Assumption registry at the end of the plan** — it is produced BEFORE session
   files are written. If an assumption fails, the session file content may need to change.

---

## Integration With Phase-Preflight

This skill produces the assumption registry at planning time.
phase-preflight-SKILL.md executes the assumption registry at session start.

```
Plan authoring:
  assumption-registry-SKILL → ASSUMPTIONS registered → EXECUTION file STEP 0 populated

Session start:
  phase-preflight-SKILL → reads STEP 0 → runs each verification command → PASS/FAIL/FALLBACK
  → If any BLOCKING fails: STOP, escalate to Luba
  → If fallback available: apply fallback, note in session, continue
```

The two skills are a pair. Without assumption-registry-SKILL, the preflight
check may not cover the right assumptions. Without phase-preflight-SKILL, the
registered assumptions are never actually verified at execution time.
