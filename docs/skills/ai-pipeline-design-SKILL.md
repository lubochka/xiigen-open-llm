# AI Pipeline Design

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (in any language), challenges an answer, or says a failure came
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

An AI call that returns both the chosen answer and the rejected alternative from
the same model produces no genuine comparative signal — it is confirmation bias
dressed as evaluation. A pipeline that always runs AI regardless of confidence
never reduces its cost as confidence grows. A decision that uses AI but produces
no learning record wastes every observation. This skill defines when to use the
AI pipeline vs the graph lookup, the four-role structure when AI is used, and
the DPO (Direct Preference Optimization) triple format that records each decision
for learning.

---

## When to Invoke

- When designing any component that calls into DomainModelRegistry for a decision
- When confidence falls below the risk-tier threshold and AI fallback is required
- Before writing any capability that consumes AI-generated decisions
- When wiring ContinuousLearningPipeline to a new decision type

---

## Section 1 — When AI Activates

```
Edge confidence ≥ risk-tier threshold → GRAPH LOOKUP (no AI, no cost, no learning record)
Edge confidence < risk-tier threshold → AI PIPELINE ACTIVATES
```

Risk-tier thresholds (FREEDOM config — never hardcoded):

| Risk tier | Threshold | Applies to |
|---|---|---|
| LOW_RISK | 0.75 | model selection, cycle budget, feature routing |
| MEDIUM | 0.85 | arbiter selection, capability class routing |
| HIGH_RISK | 0.95 | score routing, promotion decisions, escalation |
| NEVER | 1.0, immutable | architectural invariants (DNA rules) |

When the graph is confident enough: read the edge directly. AI adds cost without
adding information when the graph already knows the answer.

---

## Section 2 — The Four Roles

When AI activates, the pipeline uses four roles:

### 1. Implementors

N implementations propose decision candidates independently.

```
Minimum: 2 implementations (can be 2 calls to same model with different context,
         or calls to different models if available)
Protocol: blind — model attribution stripped before arbiters see output
Output:  structured decision in defined schema (not free-text)
Example: Implementation A proposes PATCH for NEAR-PASS on WeightedScoring.
         Implementation B proposes REGENERATE for the same situation.
```

### 2. Arbiters

Challenge each candidate against iron rules from the graph.

```
Roles:    retrieved from DomainModelRegistry (capability REQUIRES_ARBITER edges)
          Not hardcoded in the pipeline
Behavior: ANY_BLOCK_CLASS_REJECTS — one BLOCK verdict removes the candidate
BLOCK:    candidate removed from pool (DNA invariant)
```

### 3. Context

Provides graph traversal results and historical DPO triples.

```
NOT an AI call — this role queries DomainModelRegistry and ContinuousLearningPipeline
Provides: relevant edges, anti-pattern edges, historical outcomes
Injection: into each implementor's call as structured data
```

### 4. Manager (upper judge)

Selects the winner when arbiters disagree.

```
Winner   = chosen (recorded with which implementation produced it)
Runner-up = rejected (recorded with which implementation produced it)
Rule:    chosen.source ≠ rejected.source when two implementations are used
         (cross-implementation comparison is the learning signal)
```

---

## Section 3 — Cross-Implementation Requirement

```
RULE: chosen.source ≠ rejected.source when possible

Implementation:
  1. Implementation A proposes decision DA
  2. Implementation B proposes decision DB
  3. Manager selects winner
  4. Winner   → chosen { decision, reasoning, source: "implementationA" }
  5. Runner-up → rejected { decision, reasoning, source: "implementationB" }

If only one implementation is possible (single-call fallback):
  → Pipeline still runs (single implementation)
  → DPO triple marked: trainingCategory = "SINGLE_IMPLEMENTATION_CALIBRATION"
  → countsTowardThreshold: false (insufficient comparative signal)
```

---

## Section 4 — DPO Triple Structure

Every AI pipeline activation produces a DPO (Direct Preference Optimization) triple:

```csharp
public record DpoTriple
{
    public string Category { get; init; }          // "ROUTING_DECISION" | "CALIBRATION" | "ESCALATION"
    public string TrainingCategory { get; init; }  // "GENERATED" | "SINGLE_IMPLEMENTATION_CALIBRATION"
    public bool CountsTowardThreshold { get; init; } // false for single-implementation runs
    public int CurriculumTier { get; init; }       // 1 = highest strategic value
    public DpoCandidate Chosen { get; init; }
    public DpoCandidate Rejected { get; init; }
    public string TeachingPoint { get; init; }     // what this triple teaches future cycles
    public string TrainingDataQuality { get; init; } // "OUTCOME_PENDING" → "VALIDATED" after verification
}

public record DpoCandidate
{
    public string Decision { get; init; }
    public string Reasoning { get; init; }
    public string Source { get; init; }            // which implementation produced this
}
```

The `TeachingPoint` field is the most important for future cycles — it summarizes
in one sentence what this decision teaches. Make it retrievable and specific:

```
BAD:  "chose option A"
GOOD: "WeightedScoring: PATCH is preferred over REGENERATE when score 70–84% and only one invariant violated"
```

---

## Section 5 — Graduation Path

As confidence grows, AI costs decrease:

```
Cycle 1-4:   AI pipeline runs for every decision of this type (low confidence)
Cycle 5-9:   Graph lookup used when confidence ≥ threshold, AI only for new situations
Cycle 10+:   AI rarely needed — graph covers most decisions at ≥ 0.85 confidence
NEVER:       DNA rule decisions — graph-only at immutable confidence 1.0
```

The goal of every AI pipeline run is to produce a DPO triple that improves
future confidence — eventually making the AI pipeline unnecessary for that decision type.

---

## Anti-Patterns

1. **"Single call returning both chosen and rejected"** — both sides from the same
   call with the same context = confirmation bias. Use two independent implementations.

2. **"Hardcode arbiter roles in the pipeline"** — arbiter roles come from
   DomainModelRegistry edges. The panel grows as edges promote. Hardcoding freezes it.

3. **"AI pipeline that runs for every decision regardless of confidence"** — the
   entire purpose of the graph is to make AI unnecessary for confident decisions.
   Check confidence before running AI.

4. **"Free-text output from implementations"** — every implementation must return
   a structured schema. Free-text cannot be compared or stored as a DPO triple.

5. **"DPO triple with no TeachingPoint"** — a triple that doesn't explain what it
   teaches produces no learning signal for future cycles.

---

## Integration

- DomainModelRegistry stores the edges and their confidence levels (Section 1 thresholds)
- ContinuousLearningPipeline stores DPO triples and updates confidence after outcomes
- confidence-lifecycle-SKILL.md governs the confidence update formulas (Phase 2 learning)
- arbiter-panel-design-SKILL.md governs the arbiter roles in Section 2 Step 2
- learning-loop-closure-SKILL.md governs Event 3 (confidence updated after DPO triple outcome)
