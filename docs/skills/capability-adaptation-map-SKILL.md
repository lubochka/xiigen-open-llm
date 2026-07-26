# Capability Adaptation Map

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

A capability that scores below threshold on cycle 1 may be failing for predictable,
archetype-specific reasons that are known before the run. Without an adaptation map,
each below-threshold result triggers generic debugging. With an adaptation map, the
session knows exactly what failure to expect, what threshold to set per cycle, and
when to escalate vs. continue iterating.

---

## When to Invoke

- Before designing the quality improvement cycle for any new capability
- During subproject-examination-SKILL (Document 3 — Learning Spec)
- When writing Phase B section of any execution plan
- When a cycle 1 result comes back below threshold and the archetype is known

One adaptation map produced during planning = no per-cycle triage overhead during execution.

---

## Section 1 — Score Band Action Table

Apply this table after every evaluation run:

| Score band | Status | Action |
|---|---|---|
| ≥ promotion threshold | PASS | Promote to ACTIVE. Capture DPO pair if training pipeline is active. |
| 70–84% of threshold | NEAR-PASS | One targeted improvement cycle. Re-evaluate. Escalate if still NEAR-PASS. |
| 60–69% of threshold | PATCH | Up to 2 improvement cycles. Escalate if no improvement after cycle 2. |
| < 60% of threshold | STOP | Do NOT run improvement cycles. Escalate. Root cause is architectural. |

**Exception — inversion test patterns:**
A score of 0% on cycle 1 is CORRECT for capabilities that test MACHINE constant enforcement.
Verify the test correctly rejects the hardcoded value and accepts the config-driven value.
Escalate only if cycle 2 also scores 0%.

---

## Section 2 — Per-Capability Class Expected Failure Modes

For each capability class, the table records: expected cycle 1 score, common cycle 1 failure,
targeted fix, and escalation trigger.

| Capability class | Cycle 1 expected | Common failure | Fix | Escalate trigger |
|---|---|---|---|---|
| Weighted Scoring | 75–90% | Hardcoded weight instead of IConfiguration | Add Rule 1 violation for hardcoded weights | < 65% cycle 1 |
| Temporal Decay | 65–80% | Decay rate hardcoded; decay not idempotent | Add IConfiguration read for decay rate + idempotency test | < 85% after cycle 2 |
| Recommendation (ML) | 60–75% | Model update bypasses IMLService (Rule 10) | Wire through ISemanticCompiler29 / ContinuousLearningPipeline | < 85% after cycle 3 |
| Matching (N-dim) | 70–85% | Asymmetric match scores; no self-match guard | Add symmetry test + self-match guard | < 65% cycle 1 |
| Gamification | 70–85% | Points written directly to DB, not through service | Add fabric-first Rule 2 violation for direct DB write | < 85% after cycle 2 |
| Search Pipeline | 60–75% | IBm25Scorer and IVectorIndex used directly (not HybridRetriever) | Wire through HybridRetriever | < 85% after cycle 2 |
| Classification (4-tier) | 75–90% | FREEDOM value hardcoded as MACHINE constant | Add four-tier-decision-classification test | < 65% cycle 1 |

**Do not escalate before the budget is exhausted.** Two new patterns simultaneously = 3 cycles expected (especially for Recommendation and Matching classes).

---

## Section 3 — Adaptation Map Table Format

Produce one row per capability in the execution plan's Phase B section:

```markdown
## Adaptation Map (capability-adaptation-map-SKILL)

| Capability | Class | Cycle 1 expected | Known failure | Fix action | Escalate trigger |
|---|---|---|---|---|---|
| [CapabilityName] | [class] | [score range] | [failure description] | [fix] | [trigger] |
```

This table belongs in Document 3 (Learning Spec) from subproject-examination-SKILL.
It is not optional — it determines what the EXECUTOR session does per score band
without requiring human diagnosis on each cycle.

---

## Section 4 — Cycle-Specific Escalation Rules

**Always escalate immediately:**
- Score < 60% of threshold on any cycle (STOP band)
- Score decreases between consecutive cycles (regression)
- Same capability class, same failure, after full cycle budget exhausted

**Never escalate (continue cycling):**
- Below threshold on cycle 1 for Recommendation or Matching class (complex patterns need 3 cycles)
- 0% score on cycle 1 for MACHINE constant inversion tests (expected behavior)
- Below threshold on cycle 1 for any capability class being implemented for the first time in this project

**Escalation is not failure** — it means the problem is not in training data or configuration,
and a different intervention (architectural redesign, interface change) is required.

---

## Section 5 — Cycle Budget

| Capability class | Max cycles before escalation |
|---|---|
| Weighted Scoring | 2 |
| Temporal Decay | 2 |
| Recommendation | 3 |
| Matching | 3 |
| Gamification | 2 |
| Search Pipeline | 2 |
| Classification | 2 |
| Any new/unclassified capability | 3 (conservative budget for first encounter) |

---

## Anti-Patterns

1. **"Skip the adaptation map for simple capabilities"** — simple capabilities that hit an unexpected failure pattern produce the same generic debugging overhead. The map costs nothing to write.

2. **"Escalate on cycle 1 below threshold"** — Recommendation and Matching classes are expected to score below threshold on cycle 1. Escalating early wastes the session.

3. **"Keep cycling past the budget"** — exceeding the cycle budget doesn't improve scores; it wastes session context and delays the architectural fix that is actually needed.

4. **"Adaptation map is the same for all capabilities"** — different capability classes have different expected failures. A weighted scoring failure pattern doesn't apply to a gamification failure.

---

## Integration

- subproject-examination-SKILL.md Document 3 (Learning Spec) uses this table
- quality-improvement-cycle-SKILL.md uses the score band action table from Section 1
- pipeline-trace-reader-SKILL.md reads the "common failure" column to direct trace analysis
- improvement-measurement-SKILL.md uses cycle count + final score as the behavioral delta evidence
