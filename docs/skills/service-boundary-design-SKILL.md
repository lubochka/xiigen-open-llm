# Service Boundary Design

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

## What This Skill Prevents

**Over-splitting:** one algorithm per class where a cohesive service is correct.
Tests are shallow. Each class is trivially simple with no meaningful behavior to test.

**Under-splitting:** one class that does everything. Untestable in isolation.
Changes to one concern break unrelated concerns.

---

## The Four Boundary Tests

Apply all four. The boundary that passes the most tests is correct.

### Test 1 — Transaction Boundary Test

Classes that must succeed or fail atomically belong in the same class.

```
BruteForceIndex.Add() + BruteForceIndex.BuildIndex()
→ If Build fails after Add, the index is in an inconsistent state
→ They share a consistency boundary
→ MERGE into one class

BruteForceIndex.Search() + SessionStorage.SaveAsync()
→ Search result saved independently of whether the search succeeds
→ They do NOT share a transaction boundary
→ SEPARATE classes (separate fabric interfaces)
```

### Test 2 — Lifecycle Test

Classes with different lifetimes, caching policies, or update frequencies belong
in different classes.

```
InvertedIndex — rebuilt once at startup, never updated during session → one class
BM25Scorer — uses the index but is stateless per-query → separate class
BruteForceIndex — holds vectors that change as documents are added → separate class
```

### Test 3 — Consumer Independence Test

If component A's output is consumed by code that doesn't need component B's output,
A and B belong in different classes.

```
IVectorIndex.Search() → consumed by HybridRetriever
IBm25Scorer.Score() → consumed by HybridRetriever
ISessionStorage.LoadAsync() → consumed by AgentOrchestrator, not HybridRetriever

Different consumers → different interfaces → separate classes.
```

### Test 4 — Domain Ownership Test

Classes in different conceptual domains belong in different classes even if they
run sequentially.

```
VectorMath (geometric operations) → owns: distances, dot products, magnitude
BruteForceIndex (data structure) → owns: vectors, storage, search

Different domains → different classes? YES — but may be in the same subproject (09.VectorSearch).
Subproject boundary = domain; class boundary = single coherent responsibility.
```

---

## The Decision Table

| Transaction shared? | Lifecycle shared? | Consumers shared? | Decision |
|--------------------|-------------------|-------------------|---------|
| YES | YES | YES | MERGE — one class |
| YES | YES | NO | MERGE — same class, callers call it separately |
| YES | NO | any | SPLIT — different classes, same subproject |
| NO | YES | YES | MERGE if domains are compatible |
| NO | YES | NO | SPLIT |
| NO | NO | any | SPLIT — definitely separate |

---

## Method vs Class Boundary

Not everything needs its own class. Some things are methods within a class.

**Becomes its own CLASS when:**
- It owns state (fields) that must be managed independently
- It has its own lifecycle (constructed once vs per-call)
- It can be swapped at runtime (Fabric First — behind an interface)
- It has 3+ callers across different subprojects

**Stays a METHOD within a class when:**
- It is stateless and called synchronously
- Its failure doesn't change the calling class's state
- No other class needs it independently

```
VectorMath.CosineSimilarity(float[], float[]) → METHOD within BruteForceIndex
  - Stateless, called only by BruteForceIndex
  - No other class needs it independently
  - No state to manage

BruteForceIndex.Search() → METHOD within BruteForceIndex (not its own class)
HnswIndex (implements IVectorIndex) → SEPARATE CLASS
  - Different algorithm
  - Callers choose which to use at DI registration time
  - Swappable via IVectorIndex interface
```

---

## Subproject Boundary Design

Subprojects (`.csproj` files) have even coarser granularity than classes.

**New subproject when:**
- Genuinely different domain with its own fabric interfaces
- Different team would own this (hypothetically)
- Clear split in the existing SUBPROJECT-MAP.md structure

**Same subproject when:**
- Classes are closely related (e.g., BruteForceIndex and HnswIndex in 09.VectorSearch)
- All classes implement variants of the same interface
- No meaningful reason to separate them

---

## Recording Boundary Decisions

For each significant boundary decision, add to DECISIONS-LOCKED.md:

```
D-NN: HybridRetriever combines BM25 and vector search in one class
Date: YYYY-MM-DD
Decision: HybridRetriever is a single class, not a pipeline of separate classes
Alternatives: Separate BM25Ranker + VectorRanker + CombinationStep
Rationale: The combination logic is the feature — splitting it would produce three
           trivial classes where one meaningful class is correct
Affected: 12.HybridRetriever, any caller of IHybridRetriever
```

---

## Anti-Patterns

1. **One UML box = one class** — boxes describe interactions, not implementation units
2. **Splitting purely on technical lines** — "DatabaseWriter" and "DatabaseReader" as separate classes when they share a connection
3. **Making every stateless helper its own class** — a math utility function is a static method, not a class
4. **Under-splitting for simplicity** — a 500-line class with 8 unrelated responsibilities is not simple
