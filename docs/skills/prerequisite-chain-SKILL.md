# Prerequisite Chain

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

## Purpose

Before starting any phase or session, verify that all prerequisites are met.
A phase with unmet prerequisites will block mid-session and waste the setup time.

---

## The Prerequisite Check Protocol

### Step 1 — List All Prerequisites

For each planned deliverable, ask:
- What class or interface does it depend on?
- Does that class/interface currently exist?
- Does it have the methods the deliverable needs?

```
Deliverable: HybridRetriever integration test

Prerequisites:
  1. IBm25Scorer interface must exist with Score() method
  2. BM25Scorer must implement IBm25Scorer
  3. IVectorIndex must exist with Search() method
  4. HybridRetriever must exist with SearchAsync() method
  5. All prerequisites must be registered in DI
```

### Step 2 — Write Verification Commands

For each prerequisite, write the verification command:

```bash
# Prereq 1: IBm25Scorer interface exists
grep -n "interface IBm25Scorer" Src/ -r --include="*.cs"
# Expected: 1 match

# Prereq 2: BM25Scorer implements IBm25Scorer
grep -n "class BM25Scorer.*IBm25Scorer\|class BM25Scorer : IBm25Scorer" Src/ -r --include="*.cs"
# Expected: 1 match

# Prereq 3: IVectorIndex.Search() exists
grep -n "Search.*float\[\]\|OperationResult.*Search" Src/09.VectorSearch/IVectorIndex.cs
# Expected: 1+ matches

# Prereq 5: DI registrations
grep -rn "IBm25Scorer\|IVectorIndex" Src/ --include="*.cs" | grep "AddSingleton\|AddTransient"
# Expected: 2+ matches
```

### Step 3 — Classify Each Prerequisite

| Prerequisite | Verification | Status | Action if Unmet |
|-------------|--------------|--------|-----------------|
| IBm25Scorer interface | grep returns 1 match | ✅ PASS / ❌ FAIL | BLOCKING — create before this phase |
| BM25Scorer class | grep returns 1 match | ✅ PASS / ❌ FAIL | BLOCKING — implement IBm25Scorer first |
| IVectorIndex.Search() | grep returns match | ✅ PASS / ❌ FAIL | BLOCKING — check 09.VectorSearch |
| DI registrations | grep returns 2+ | ✅ PASS / ❌ FAIL | BLOCKING — update Program.cs |

---

## Chain Ordering

When multiple phases have prerequisites, map the full chain:

```
Phase 3 (HybridRetriever integration)
  ← requires Phase 2 (BM25Scorer implementation)
     ← requires Phase 1 (IBm25Scorer interface + InvertedIndex)
        ← requires: NONE (starts here)

Execution order: Phase 1 → Phase 2 → Phase 3
```

Do not start Phase 3 until Phase 2 tests show Failed: 0.

---

## Prerequisite Block Protocol

If any BLOCKING prerequisite fails at session start:

```
PREREQUISITE BLOCK: [what failed]

Verification run: [command that showed the gap]
Output: [actual output vs expected]

Cannot proceed with: [which deliverable is blocked]

Options:
A. Build the missing prerequisite now (estimated: N steps)
B. Stop and wait for Luba to assign the prerequisite phase

Recommendation: [A or B with reasoning]
```

Do not try to work around a missing prerequisite by mocking it in tests
or adding `TODO` comments. Either fix the prerequisite or stop.

---

## Session Start Prerequisite Checklist

Before every code session:

```
□ Build: dotnet build LlmFromScratch.sln → 0 Error(s)
□ Test baseline: dotnet test --no-build → record N passed, 0 failed
□ Read STATE-Pn.json → confirm "Can start: YES"
□ Verify top-3 prerequisites for this phase with grep commands
□ No unmet prerequisites → begin work
□ Any unmet prerequisite → apply block protocol above
```

---

## Anti-Patterns

1. **"I'll discover prerequisites as I go"** — blocked sessions have high overhead; verify prerequisites first
2. **Mocking a missing prerequisite to unblock a test** — the mock hides the gap; the real integration will fail
3. **Assuming a prior phase is complete without verifying** — check STATE-Pn.json and run the prior phase's Gate B verification
4. **Not listing prerequisites at all** — implicit prerequisites are the ones that cause blocked sessions
