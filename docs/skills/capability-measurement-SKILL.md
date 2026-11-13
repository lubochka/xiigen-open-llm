# Capability Measurement

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

## Overview

Measures implementation completeness across the six capability classes.
Used at phase end to report what is complete, what is partial, and what is planned.

---

## The 6 Capability Classes

| Class | Scope | Example in This Project |
|-------|-------|------------------------|
| C1: Core Algorithm | Pure math/logic implementation | BruteForceIndex cosine distance, BM25 IDF calculation |
| C2: Fabric Interface | Interface + at least one implementation | IVectorIndex, ISessionStorage, IAgentMessageBus |
| C3: Integration | Components wired together via DI | HybridRetriever using IVectorIndex + IBm25Scorer |
| C4: Session/State | Persistence and resumption | SessionStateStore load/save round-trip |
| C5: Generated Output | Code or text produced by the engine | SemanticCompilerPipeline predicate output |
| C6: UI/Surface | User-facing interaction | Windows Forms explorer panels, StatusStrip |

---

## Score Scale

| Score | Meaning | Evidence Required |
|-------|---------|------------------|
| 0% | Not started | No files exist |
| 25% | Skeleton | Interface defined, no implementation |
| 50% | Partial | Implementation exists, tests incomplete |
| 75% | Functional | All tests pass, edge cases missing |
| 100% | Complete | All tests pass, edge cases covered, doc synced |

**Evidence citation format:**
```
C1: BruteForceIndex — 100%
  Evidence: dotnet test --filter "BruteForceIndex" → 12 passed, 0 failed (2026-05-01)
  File: Src/09.VectorSearch/BruteForceIndex.cs
```

---

## Capability Manifest

Maintain a manifest of the current capability state. Update at each phase end.

```markdown
## Capability Manifest — [date]

### Tier 1: Active and Complete
| Capability | Class | Score | Evidence |
|-----------|-------|-------|---------|
| BruteForceIndex | C1 | 100% | 12 tests passing |
| IVectorIndex | C2 | 100% | Interface + 2 implementations |
| HybridRetriever | C3 | 75% | Integrated, missing graph-rerank |
| ISessionStorage + SessionStateStore | C4 | 100% | Load/save round-trip tested |

### Tier 2: In Progress
| Capability | Class | Score | Blocking |
|-----------|-------|-------|---------|
| HnswIndex | C1 | 50% | Graph builder edge case failing |
| SemanticCompilerPipeline | C5 | 50% | Predicate output not reviewed |

### Tier 3: Planned
| Capability | Class | Target Phase |
|-----------|-------|-------------|
| 17.DynamicDomains | C5 | P4 |
| Windows Forms Stage Explorer UI | C6 | P5 |
```

---

## Paired Runtime Check

For each capability scored at 75% or higher, pair the score with a runtime command:

```bash
# C1: BruteForceIndex
dotnet test --filter "BruteForceIndexTests" --no-build 2>&1 | tail -3
→ Passed: 12, Failed: 0 → Score: 100% CONFIRMED

# C2: IVectorIndex implementations
grep -rn "class.*: IVectorIndex" Src/ --include="*.cs"
→ BruteForceIndex, HnswIndex → Score: 100% CONFIRMED

# C3: HybridRetriever integration
dotnet test --filter "HybridRetrieverIntegrationTests" --no-build 2>&1 | tail -3
→ Passed: 8, Failed: 0 → Score: 75% (rerank tests pending)
```

**Do not report a score without the runtime command that proves it.**

---

## Phase End Measurement Protocol

At the end of each phase:

```
1. List all deliverables from the phase plan
2. For each deliverable: assign capability class and run measurement command
3. Record score with evidence
4. Update Tier 1/2/3 manifest
5. Report delta: what moved from Tier 3→2, 2→1 this phase
6. Update CLAUDE.md test baseline count
```

---

## Anti-Patterns

1. **Estimating scores without runtime evidence** — scores require dotnet test output
2. **Keeping Tier 2 items in Tier 2 indefinitely** — if blocked, record the blocker explicitly
3. **Scoring C5 (generated output) at 100% without Layer 3 review** — see generated-code-review-SKILL.md
4. **Reporting aggregate % for a class** — score per-component, not per-class
