# Gap to Proposal

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

## The Resolution Ladder

Before proposing a new subproject, new interface, or new algorithm, climb the ladder.
Start at Level 1. Only move up if the lower level genuinely cannot solve the problem.

```
LEVEL 1 — CONVENTION: Can this be fixed by updating CLAUDE.md, AGENTS.md, or a DNA rule?
  Example: Missing Rule 3 comments found in a subproject
  Fix: Update dna-compliance-guard-SKILL.md with a pre-commit grep check
  No new code needed. 10-minute governance update.

LEVEL 2 — ADAPTATION: Does an existing interface cover this with a new implementation?
  Example: Need a graph-based vector search
  Check: IVectorIndex exists (it does)
  Fix: Add GraphVectorIndex : IVectorIndex
  No new interface needed. One implementation session.

LEVEL 3 — EXTENSION: Does an existing class cover this with a new method?
  Example: ISessionStorage needs a ListAllSessions() method
  Check: ISessionStorage already exists with SaveAsync/LoadAsync
  Fix: Add ListSessionsAsync() to ISessionStorage + implement in SessionStateStore
  Phase A addition, not a new subproject.

LEVEL 4 — NEW COMPONENT: None of the above apply.
  Example: Need BM25 scoring — no existing interface covers this
  No existing interface. No existing class can extend to cover it.
  New interface (IBm25Scorer) + new implementation + new tests + new DI registration.
```

Document why each lower level is insufficient before proposing a higher level.

---

## Step 1 — Classify the Gap

```
Gap: IBm25Scorer is missing — HybridRetriever has no BM25 component

Q1: Can CONVENTION fix it?
    No — BM25 scoring is an algorithm, not a configuration value.

Q2: Can ADAPTATION fix it? (existing interface covers it with new implementation)
    No — IVectorIndex covers vector similarity, not term-frequency scoring.
    Different mathematical model, different contract.

Q3: Can EXTENSION fix it? (add method to existing class)
    Partially — HybridRetriever.SearchAsync() could contain BM25 logic inline.
    But: mixing BM25 into HybridRetriever violates Rule 2 (Fabric First)
    and makes BM25 untestable in isolation.

Q4: New component needed?
    YES — IBm25Scorer with BM25 formula in a dedicated class, injected into HybridRetriever.
```

---

## Step 2 — Produce the Proposal

```markdown
## GAP RESOLUTION PROPOSAL — IBm25Scorer

**Gap detected:** HybridRetriever has no BM25 scoring component
**Resolution level:** LEVEL 4 — new interface + implementation

**What will be built:**
  - IBm25Scorer interface: Score(string query, string document): double
  - BM25Scorer class: Implements BM25 formula (k1=1.5, b=0.75 defaults as FREEDOM values)
  - xUnit tests: unit tests for BM25 formula, integration test for HybridRetriever with BM25
  - DI registration: services.AddSingleton<IBm25Scorer, BM25Scorer>()

**Prerequisite gaps to resolve first:**
  - InvertedIndex must exist (BM25Scorer needs document frequency counts)
  - Idf calculation must handle corpus size = 0 (empty corpus edge case)

**Estimated effort:** 1 code session (interface + implementation + tests)
**Builds on:** Pattern from IVectorIndex + BruteForceIndex

**Stop required before:** Beginning HybridRetriever integration tests

**Needs Luba decision:**
  - Should BM25 k1 and b be read from IConfiguration or hardcoded as FREEDOM defaults?
    (Recommendation: IConfiguration with defaults 1.5 and 0.75)
```

---

## Step 3 — Dependency Order for Multiple Gaps

When multiple gaps exist, order them by dependency:

```
Gap A (InvertedIndex) → blocks Gap B
Gap B (IBm25Scorer) → blocks Gap C
Gap C (HybridRetriever integration) → blocks user search feature

Order: A → B → C
Rationale: A is the prerequisite, B uses A, C uses both.
```

---

## Stop Gate

Every proposal requires Luba's approval before work begins.
The proposal is the INPUT to the approval, not the permission to start.

No new component session begins without explicit "yes" from Luba.

---

## Anti-Patterns

1. **Jumping to LEVEL 4 without checking lower levels** — most gaps are Level 2 or 3
2. **"We'll need this eventually so let's build it now"** — build when the gap blocks current work, not speculatively
3. **Proposal without a prerequisite chain** — if Gap A depends on Gap B, the proposal must say so
4. **Starting work on the proposal before Luba's approval** — the proposal gate is mandatory
