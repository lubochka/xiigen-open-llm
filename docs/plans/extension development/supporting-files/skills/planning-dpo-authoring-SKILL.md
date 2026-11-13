# Planning DPO Authoring

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

## Full-Task Readiness Supersession Gate

PLAN_READY for planning/execution requires BOTH zero unresolved design/dependency blockers and full original-task coverage proven by minimum 30 sequential mental implementation passes, one at a time, not parallel. A later stricter full-task simulation, reviewer packet, or attachment that reports NOT_READY, zero FULLY coverage, uncovered original-task requirements, or unresolved design/dependency blockers supersedes any earlier narrow ready verdict.

When this happens, downgrade current readiness to PLAN_NOT_READY before any implementation or final ready claim, and propagate the downgrade into active plan artifacts plus the relevant skills/guides/agent files that allowed the stale claim. A plan-only wording patch is incomplete. Readiness-repair work orders must name skills, source attachments, active files, original-task requirements, good/bad examples, pass-count/evidence rules, verification commands, and explicit propagation targets. Attachment-reported simulations are evidence only; do not claim the worker ran those passes unless the worker actually ran or received independently inspectable records for them. Old PLAN_READY_FOR_LUBA_REVIEW may remain only as historical/pre-attachment/narrower-repair text with current PLAN_NOT_READY nearby.

PLAN_NOT_READY after full-task coverage simulation is not a final reporting boundary. When implementable design/dependency blockers remain, immediately convert the blocker inventory (for active-codex Pass06 and later: B-1..B-22 unresolved blocker inventory is mandatory; Missing blocker numbers are trace defects, not permission to drop them; older B1-B21/B5-absent evidence must become explicit trace-repair backlog, not a dropped row) into sequential bounded sub-agent plan-repair passes and continue until the plan reaches readiness or a true external blocker remains after mitigation/decision-prep. A final answer after only downgrading readiness is a protocol violation unless all implementable blockers have been repaired, delegated to active workers, or proven external/non-actionable. Minimum-pass requirements are evidence-bound: if Luba requires 50 passes with sub-agents one-at-a-time, run or receive 50 inspectable sequential sub-agent packets/records before claiming 50; no parallelism and no ledger-only inflation. Governance/skill repair must precede plan repair when the stop came from a governance loophole, but governance repair is not itself a completion boundary.

Pass06 DPO pair semantics guard: DPO pair semantics must be unit-level unless an explicit Luba-approved plan changes it. A whole-graph blob, graph-vs-graph pair, or hidden runtime shortcut cannot be treated as train-ready DPO merely because a future gate promises to refine it later.

## Core Problem

The AI pipeline generates code — but planning decisions (routing choices, cycle count
predictions, escalation outcomes) also need to be learned. A planning decision that
produces no DPO (Direct Preference Optimization) triple is a missed learning opportunity:
the system will repeat the same routing error in the next session. This skill defines
the three planning signal types, the GENERATED vs REINFORCEMENT distinction, and the
outcome attribution that moves triples from OUTCOME_PENDING to VALIDATED.

---

## When to Invoke

- After an AI pipeline cycle completes (planning decision → outcome known)
- When writing the learning signal for a routing decision, calibration comparison,
  or escalation outcome
- When reviewing DPO triples before they feed IncrementalTrainer
- When a triple is marked OUTCOME_PENDING and Phase F retrospective data is available

---

## Section 1 — Three Planning Signal Types

These extend the code generation signal types (already covered by training-data-gap-audit-SKILL.md)
to capture planning-layer decisions:

### ROUTING_DECISION

Produced when the escalation orchestrator or cycle router chose an action at a score bracket.

```csharp
new DpoTriple
{
    SessionId = currentSessionId,
    CapabilityClass = "RoutingDecision",
    SignalType = "ROUTING_DECISION",
    Chosen = "Routed NEAR_PASS (score=78%) to PATCH cycle with targeted prompt injection",
    Chosen_Source = "escalation-orchestrator-SKILL.md Rule 2 application",
    Rejected = "Escalated NEAR_PASS to Luba without trying algorithmic options",
    Rejected_Source = "early escalation without exhausting algorithmic options",
    TeachingPoint = "NEAR_PASS (70-84%) should attempt PATCH before escalation",
    TrainingCategory = "GENERATED",
    CountsTowardThreshold = true,
    TrainingDataQuality = "OUTCOME_PENDING"  // validated in Phase F
}
```

Outcome known: after Phase F retrospective, when the final score for this capability class is available.

### CALIBRATION_COMPARISON

Produced when a cycle completes and the predicted budget can be compared to actual cycles used.

```csharp
new DpoTriple
{
    SessionId = currentSessionId,
    CapabilityClass = "CalibrationComparison",
    SignalType = "CALIBRATION_COMPARISON",
    Chosen = "Predicted budget=3 for WeightedScoring (novelty=IVectorIndex, first occurrence)",
    Chosen_Source = "cycle-budget-prediction-SKILL.md formula: 1+1+1=3",
    Rejected = "Predicted budget=1 (archetype baseline only, no novelty or occurrence penalty)",
    Rejected_Source = "underestimate ignoring first_occurrence and novelty_sum",
    TeachingPoint = "IVectorIndex injection = +1 novelty; first SCORING occurrence = +1 penalty",
    TrainingCategory = "GENERATED",
    CountsTowardThreshold = true,
    TrainingDataQuality = "VALIDATED"  // outcome known immediately at phase completion
}
```

Outcome known: immediately — actual cycle count is known at phase completion.

### ESCALATION_DECISION

Produced when the escalation orchestrator chose a terminal outcome (ESCALATE_TO_UPPER_JUDGE,
ESCALATE_TO_HUMAN, or UNDECIDED).

```csharp
new DpoTriple
{
    SessionId = currentSessionId,
    CapabilityClass = "EscalationDecision",
    SignalType = "ESCALATION_DECISION",
    Chosen = "ESCALATE_TO_HUMAN with gap analysis: 4 cycles, score plateau at 71%, Rule 1 BLOCK root cause identified",
    Chosen_Source = "meta-escalation-router-SKILL.md: PLATEAU trajectory, gap analysis complete",
    Rejected = "ESCALATE_TO_HUMAN with 'it failed' — no trajectory, no root cause, no options",
    Rejected_Source = "incomplete briefing that forces Luba to reconstruct history",
    TeachingPoint = "Every escalation briefing must include trajectory + gap analysis + 3 options",
    TrainingCategory = "GENERATED",
    CountsTowardThreshold = true,
    TrainingDataQuality = "OUTCOME_PENDING"
}
```

Outcome known: after Phase F, when the escalation path's effect on final score is observable.

---

## Section 2 — GENERATED vs REINFORCEMENT

```
GENERATED:
  - Produced from a real AI pipeline cycle (multi-implementation, different sources)
  - Chosen.Source ≠ Rejected.Source (cross-implementation requirement)
  - CountsTowardThreshold = true
  - Used for both fine-tuning corpus AND DomainModelRegistry edge updates
  - DerivedFrom = null

REINFORCEMENT:
  - Derived from a GENERATED triple (pattern extraction, not a new AI cycle)
  - Exempt from cross-source requirement (no multi-implementation needed)
  - CountsTowardThreshold = false
  - Used for DomainModelRegistry edge updates ONLY (not fine-tuning corpus size)
  - DerivedFrom = [source GENERATED triple's SessionId or identifier]
```

**Why the distinction matters:** REINFORCEMENT triples that set CountsTowardThreshold=true
inflate the graduation threshold count without genuine cross-implementation validation.
The graduation threshold (minimum evidence for confidence promotion) must be earned
from GENERATED triples, not amplified by derived ones.

**Example REINFORCEMENT triple (derived from a GENERATED ROUTING_DECISION):**

```csharp
new DpoTriple
{
    SessionId = currentSessionId,
    CapabilityClass = "RoutingDecision",
    SignalType = "REINFORCEMENT",
    Chosen = "NEAR_PASS bracket (70-84%) routes to PATCH before escalation",
    Chosen_Source = "archetype-failure-map-SKILL.md score bracket table",
    Rejected = "NEAR_PASS routes directly to escalation",
    Rejected_Source = "omitted PATCH bracket from routing logic",
    TeachingPoint = "PATCH bracket reduces escalation rate for 70-84% range by ~40%",
    TrainingCategory = "REINFORCEMENT",
    CountsTowardThreshold = false,
    DerivedFrom = "[source GENERATED triple identifier]"
}
```

---

## Section 3 — Outcome Attribution

Every OUTCOME_PENDING triple must be resolved to VALIDATED before IncrementalTrainer runs.

**Outcome values:**

| Outcome | Definition | Triple direction |
|---|---|---|
| SUCCESS_WITHIN_BUDGET | Phase reached Gate B at or before predicted budget | Chosen reinforced, Rejected penalized |
| WASTED_CYCLE | A cycle ran but score delta was < 5% | Chosen penalized if it led to wasted cycle |
| ESCALATION_REQUIRED | Budget exhausted without Gate B | Both penalized — prediction was wrong |

**Validation procedure:**

```bash
# At Phase F retrospective: find all OUTCOME_PENDING triples for this phase
grep -rn "OUTCOME_PENDING" tests/TrainingData/

# For each: compare predicted budget to actual cycles
# Update TrainingDataQuality to VALIDATED
# Record actual outcome (SUCCESS_WITHIN_BUDGET / WASTED_CYCLE / ESCALATION_REQUIRED)
```

---

## Section 4 — Cross-Layer Learning

**Planning → Code generation:** An ESCALATION_DECISION triple that identifies a
gap in a specific iron rule produces a REINFORCEMENT triple for the code generation layer:

```
Source: GENERATED planning triple (ESCALATION_DECISION — iron rule gap found)
Derived: REINFORCEMENT code-gen triple (NAMED_CHECK_REINFORCEMENT)
DerivedFrom: source triple's identifier
```

**Code generation → Planning graph:** When a principles arbiter BLOCK is produced,
the DomainModelRegistry AVOID edge for that violation is strengthened:

```
Event: principles-arbiter BLOCK on Rule 1 (throw statement)
Action: DomainModelRegistry.UpdateEdge(capabilityClass, Rule1ViolationPattern, direction=AVOID)
Side effect: REINFORCEMENT triple capturing the cross-layer signal
```

---

## Section 5 — Source Material Curation for Trainable Planning Models

Before authoring DPO rows for a trainable planning, orchestration,
prompt-to-task, skill-selection, or adaptive user model, classify source
material. Do not turn every available document into DPO.

Each source ref must be classified:

```text
source_ref:
source_sha256:
source_version_id:
machine_target:
source_quality: gold | silver | rejected
training_route: trainable_common | adaptive_only | evidence_only | reject
classification_reason:
teaches_boundary:
raw_private_text_copied=false
technical_execution_refs_excluded=true
invented_approval=false
```

Good material:
- comes from a real prompt, approved analysis, accepted plan, skill doc, or
  explicit Luba correction;
- has a known approved outcome or correction;
- teaches one decision boundary;
- can be cited by ref/hash/provenance without copying private text into common
  training rows.

Bad material:
- is a raw unresolved transcript;
- is a filename/path clue rather than reasoning;
- is a technical command or patch recipe for common semantic training;
- has no approved outcome;
- creates a rejected example that is generic, absurd, refusal-only, or
  checklist-only.

Near-miss DPO quality:
- rejected must be plausible and close to chosen;
- rejected must map to a concrete taxonomy code;
- chosen/rejected must differ on one main decision boundary;
- reviewer must be able to explain what the model should learn from the pair.

Required real-reference seed row before training:

```text
seed_id:
machine_target:
source_ref:
source_sha256:
source_version_id:
strategy_doc_ref:
chosen_ref:
rejected_ref:
error_code:
teaches_boundary:
source_quality:
common_or_adaptive:
raw_private_text_copied=false
technical_execution_refs_excluded=true
```

Training must not start from abstract example families alone. Families explain
coverage; seed rows prove that actual source material exists.

---

## Anti-Patterns

1. **"Both chosen and rejected from the same session run"** — a GENERATED triple requires
   Chosen.Source ≠ Rejected.Source. Single-source DPO does not count toward threshold.

2. **"REINFORCEMENT triple with CountsTowardThreshold=true"** — derived triples inflate
   the graduation count. Set CountsTowardThreshold=false for all REINFORCEMENT triples.

3. **"REINFORCEMENT triple without DerivedFrom"** — provenance chain broken. The triple
   cannot be traced back to the source GENERATED triple. DerivedFrom is required.

4. **"GENERATED triple created with TrainingDataQuality=VALIDATED"** — the outcome
   is not yet known at creation time. Always start OUTCOME_PENDING. Phase F validates.

5. **"Skipping CALIBRATION_COMPARISON signal"** — cycle budget predictions that are
   never compared to actuals produce no calibration feedback. The MAE recalibration
   trigger (MAE > 1.0 after 3+ phases) requires this data.

---

## Integration

- training-data-gap-audit-SKILL.md classifies RECOVERED/CONTAMINATED/CALIBRATION before training
- IncrementalTrainer runs after all OUTCOME_PENDING triples for the phase are VALIDATED
- DomainModelRegistry.UpdateEdge() is called after each VALIDATED triple
- cycle-budget-prediction-SKILL.md Dimension 4 retrospective MAE uses CALIBRATION_COMPARISON signals
- phase-retrospective-SKILL.md Dimension 2 (pattern transfer) checks if ROUTING_DECISION triples improve future routing
