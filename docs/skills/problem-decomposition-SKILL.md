# Problem Decomposition

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

## When to Invoke

At the start of any session where the problem is complex or ambiguous.
Before reading any file, before proposing any fix, before writing any plan.

---

## The Five Steps

### Step 1 — Symptom or Root Cause?

Ask "why does this problem exist?" three times.

```
First answer  = immediate cause  → fix with a targeted code change
Second answer = structural cause → fix with a pattern or design update
Third answer  = architectural cause → requires a design decision or new component
```

**Rule:** Identify the level before proposing the solution. A Level 1 solution
for a Level 3 problem fixes today and breaks tomorrow.

**Example in this project:**
```
Problem: HybridRetriever returns NaN scores for some queries

Level 1: Add null check in HybridRetriever.CombineScores() → fixes this instance
Level 2: BM25Scorer can return non-normalised scores > 1.0
         Fix: Add normalisation in BM25Scorer before combining
Level 3: Scoring combination assumes both components output [0,1] range
         Architectural: define IScorer interface contract that guarantees normalised output
```

The right level of fix depends on whether this is a one-time issue or a systematic pattern.

### Step 2 — Minimum Information Needed

Do NOT read everything. Answer: which 3–5 files tell the whole story?

```
For test failures:         failing test + class being tested + interface it uses
For algorithm bugs:        the specific method + its callers + test coverage
For interface questions:   COMPONENT-INTERFACES.md + the interface file + one implementation
For phase planning:        STATE-Pn.json + plan file section + test baseline
For "where does X live":   SUBPROJECT-MAP.md + one grep — not reading all projects
```

**Rule:** Start narrow. Widen only when the narrow read is insufficient.
Every extra file read without a specific question is noise.

### Step 3 — Classify Incoming Claims

Before accepting any factual claim from a review, from a prior session summary, or from another model:

```
VERIFIED   = a command that proves it is cited
             → spot-check the command yourself

INFERRED   = reasonable deduction, reasoning shown but command not run
             → run the verification command yourself

ASSUMED    = stated as fact, no command shown
             → treat as hypothesis until confirmed
```

**Rule:** Test counts, interface signatures, file existence, line numbers —
ALWAYS verify before using in a plan. One grep costs 10 seconds.
A wrong number in a plan costs a blocked session.

### Step 4 — Scope the Response

Start at the lowest scope that genuinely solves the problem:

```
METHOD CHANGE → CLASS CHANGE → INTERFACE CHANGE → SUBPROJECT CHANGE → ARCHITECTURE CHANGE
```

Document why each lower scope is insufficient before claiming the higher one needs to change.

Example:
```
Problem: Need to add a search radius filter to vector search

Method change: Add radius parameter to BruteForceIndex.Search() — sufficient? 
  YES — no interface change needed if we add an overload
  Why not interface change: IVectorIndex.Search(float[], int) callers don't need to change
  Decision: Method change only (add SearchRadius overload, don't touch IVectorIndex)
```

### Step 5 — Produce Executable Output

Output is commands and facts, not prose. The next session must be able to run
the plan without interpretation. Include:

- Exact file paths (not descriptions of where to look)
- Exact grep commands with expected output
- Exact test filter commands with expected pass count
- Discoveries and rejected claims (see investigation-handoff-SKILL.md)

---

## Anti-Patterns

1. **Starting to fix before identifying the level** — "the fix is obviously to add a null check" may be correct for Level 1 but wrong if the structural cause is in an upstream class

2. **Reading all files before identifying which 3 answer the question** — information overload causes the actual signal to be missed

3. **Accepting summary numbers without running verification** — test counts, line numbers, class names from summaries must be verified with grep before using in a plan

4. **Proposing infrastructure for a problem that a one-line fix solves** — over-engineering

5. **Treating every problem as architectural** — some problems are just wrong values in the right place

---

## Level Decision Reference

| If the third "why" answer is... | Fix level |
|--------------------------------|-----------|
| A missing null check | Method-level fix |
| A wrong return type | Class-level fix |
| An interface that doesn't guarantee normalised output | Interface-level fix |
| A design decision that was never made explicit | Architecture-level decision + DECISIONS-LOCKED.md entry |
| A DNA rule that wasn't applied | Governance fix (update CLAUDE.md or dna-compliance-guard-SKILL.md) |
