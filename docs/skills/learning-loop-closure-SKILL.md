# Learning Loop Closure

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

A capability that records observations but never updates its confidence has a broken
learning loop. A capability that updates confidence but never checks for promotion
produces training data that never improves the system. The four-event learning loop
requires all four events to complete — record, observe, update, promote — in every
capability lifecycle. This skill prevents any event from silently dropping.

---

## When to Invoke

- When designing any capability that enters ContinuousLearningPipeline
- After capability-improvement-cycle-SKILL.md Stage 2 (design) to verify the learning plan exists
- When a capability has been running for 10+ invocations but confidence has not moved
- Before any Phase A design is finalized for a capability with IConfiguration thresholds

---

## Section 1 — The Four-Event Learning Loop

Every capability managed by ContinuousLearningPipeline must complete all four events.
A capability that only completes events 1–2 (records and classifies) but skips 3–4
(update and promote) produces observation data that never improves the system.

```
Event 1: DECISION MADE
  When:   Capability is invoked; OperationResult produced
  What:   ContinuousLearningPipeline records initial confidence + invocation context
  Status: OutcomeClass = PENDING

Event 2: OUTCOME OBSERVED
  When:   Downstream verification completes (test passes, user confirms, or evaluation runs)
  What:   Outcome classified as:
            SUCCESS — capability produced correct result
            NEAR_MISS — result was correct but inefficient or incomplete
            FAILURE — result was wrong or threw exception
  Status: OutcomeClass updated; DPO (Direct Preference Optimization) triple marked OUTCOME_KNOWN

Event 3: CONFIDENCE UPDATED
  When:   Immediately after Event 2
  What:   confidence += delta (positive for SUCCESS, negative for FAILURE)
          observationCount += 1
          delta read from IConfiguration key "ContinuousLearning:ConfidenceDelta" (default 0.05)
  Status: confidence value updated in DomainModelRegistry

Event 4: PROMOTION CHECK
  When:   After Event 3
  What:   If confidence ≥ threshold AND observationCount ≥ minimum:
            DomainModelRegistry.Promote(capability, OPTIONAL → ACTIVE → PROVEN)
          If confidence < decay_floor for N consecutive runs:
            DomainModelRegistry.Demote(capability) and return to design review
  Status: capability status updated
```

---

## Section 2 — Learning Plan Template

Required for every capability that enters ContinuousLearningPipeline.
Add to the Phase A design artifacts:

```markdown
## Learning Plan — [CapabilityName]

Edge updates:
| From | Relationship | To | OutcomeField | PositiveIf | ConfidenceDelta |
|---|---|---|---|---|---|
| [capability] | INVOKES | [consumer] | ResultIsSuccess | == true | 0.05 |
| [capability] | IMPROVES | [metric] | MetricDelta | > 0.0 | 0.05 |

Promotion candidates:
| From | Current Status | Promotion Condition |
|---|---|---|
| [capability] | OPTIONAL | confidence ≥ 0.85 AND observationCount ≥ 10 |

Drift checks:
| Metric | DriftThreshold | Action if exceeded |
|---|---|---|
| AverageSuccessRate | < 0.70 over 5 runs | Demote to OPTIONAL; investigate |
| ConsecutiveFailures | ≥ 3 | Pause capability; return to Stage 2 |
```

Without this plan, the capability accumulates observations with no promotion mechanism.
The plan must exist at design time (Stage 2), not at evaluation time (Stage 4).

---

## Section 3 — Pre-Graph Discovery Rule

For capabilities added before DomainModelRegistry contains the promotion edge,
the learning loop must include a discovery step:

Before checking promotion candidates, verify the capability's entry exists in
DomainModelRegistry. If absent:
```csharp
// Register the edge at base confidence so observations accumulate
_registry.RegisterCapability(capability, initialConfidence: 0.60);
```

Without this, promotion checks for new capabilities return `UNCHANGED` silently
because the registry has no entry to promote. The confidence updates are lost.

---

## Section 4 — Feedback Timing

Decision type → When recorded → When outcome is known:

```
Capability invocation     → Event 1 (at invocation)  → Event 2 (after verification)
Configuration change      → Event 1 (at change)       → Event 2 (after first invocation)
Threshold tuning          → Event 1 (at tuning)       → Event 2 (after 5 invocations)
Phase A design decision   → Event 1 (at design time)  → Event 2 (after Stage 3 score)
```

The gap between Event 1 and Event 2 is the critical window. During this window,
the DPO triple status is `OUTCOME_PENDING`. Event 2 flips it to `VALIDATED`.

**Never record Event 1 and Event 2 at the same time.** If the outcome is known
at decision time, the loop has no learning signal — the decision was already correct.
The learning signal comes from real invocations with real downstream results.

---

## Section 5 — Loop Verification

After 5+ invocations, verify all four events completed for each invocation:

```bash
# Verify observationCount is incrementing
# grep DomainModelRegistry for capability entry
grep -n "[CapabilityName]" src/**/DomainModelRegistry.cs

# Verify confidence is not stuck at initial value
# If confidence == initial after 10 runs: Event 3 is not firing
```

If any event is silently missing:

| Symptom | Broken event | Fix |
|---|---|---|
| observationCount never increments | Event 3 | Verify ContinuousLearningPipeline.RecordOutcome() is called after Event 2 |
| confidence stuck at initial | Event 3 | Verify confidence delta is applied, not just observationCount |
| capability never promoted | Event 4 | Verify promotion check is called after Event 3 |
| promotion called but status unchanged | Event 4 | Verify DomainModelRegistry entry exists (pre-graph discovery rule) |
| DPO triple status stays OUTCOME_PENDING | Event 2 | Verify downstream verification flow reaches ContinuousLearningPipeline |

---

## Anti-Patterns

1. **"Recording decision and outcome at the same time"** — if outcome is known at
   decision time, there is no learning signal. The loop is closed before data
   is collected. Record Event 1 at invocation; record Event 2 after real use.

2. **"Missing the learning plan entirely"** — "Phase F will figure it out" means
   Phase F has nothing to measure. The plan must exist at design time (Stage 2).

3. **"Promotion check with wrong current status"** — the promotion check reads
   the CURRENT status from DomainModelRegistry. An entry already at ACTIVE
   must be passed as ACTIVE for the check to promote to PROVEN. Passing OPTIONAL
   when the entry is already ACTIVE skips the promotion.

4. **"5 invocations, confidence never moved — maybe it needs more runs"** — if
   confidence does not move after 5+ runs, the loop has a broken event, not
   insufficient data. Diagnose which event is missing before adding more runs.

5. **"Skip pre-graph discovery for new capabilities"** — a new capability with no
   DomainModelRegistry entry silently drops all promotion attempts. Register first.

---

## Integration

- confidence-lifecycle-SKILL.md governs Event 3 (delta calculation) and Event 4 (promotion thresholds)
- capability-improvement-cycle-SKILL.md Stage 4 (runs and records outcomes) is the implementation of Events 1–3
- ContinuousLearningPipeline is the concrete implementation that runs Events 1–4
- DomainModelRegistry is the persistence layer that stores status between Events 3 and 4
- IncrementalTrainer (Phase 4 subproject) produces the DPO triples referenced by Event 2 outcome classification
