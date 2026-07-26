# Archetype Failure Map

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

A plan that says "implement BM25 scorer and run tests" with no prediction of
where it will fail leaves the executor making ad-hoc decisions per score bracket.
When cycle 1 scores 0.68, should the response be a targeted patch, a full
regeneration, or an escalation? Without a per-archetype failure map, the answer
varies by whoever is running the cycle — not by the evidence. This skill produces
a failure map for each capability class before any implementation begins.

---

## When to Invoke

- After cycle-budget-prediction-SKILL.md declares the cycle budget
- Before writing any phase plan for a new capability class
- When capability-improvement-cycle-SKILL.md Stage 3 requires score interpretation
- When phase-retrospective-SKILL.md MAE > 1.0 for a cycle — recalibrate the map

---

## Section 1 — Score Bracket Action Table

Every capability class must declare its score bracket actions before implementation:

| Score | Classification | Action |
|---|---|---|
| ≥ 85% | PASS | Gate B satisfied. Proceed to next phase. Store result. |
| 70–84% | NEAR-PASS | Add targeted patch for the specific gap. One re-run. Escalate if still NEAR-PASS after patch. |
| 60–69% | PATCH | Identify failing iron rule or missing behavioral invariant. Add correction. Re-run. |
| < 60% | STRUCTURAL_FAILURE | Stop. Root cause required before cycling again. Do not retry blindly. |

**Exception — inversion patterns:**
Score 0 on cycle 1 is CORRECT when the capability's iron rules require inverting
the DNA default (e.g., a validation capability that must reject what the default
accepts). Verify the [Fact] test asserts the inversion. Proceed to cycle 2.
Escalate only if cycle 2 also fails.

---

## Section 2 — Per-Capability-Class Failure Mode Templates

For each capability class built in this project, declare its failure map entry:

### WeightedScoring (CapabilityClass)

```
Expected cycle 1 score: 70–85%
Common failure: weights summed incorrectly across algorithm results
Fix: add iron rule — "sum of weights must equal 1.0 before applying to scores"
Escalate if: < 60% on cycle 1 (STRUCTURAL_FAILURE)
```

### HybridRetriever (CapabilityClass)

```
Expected cycle 1 score: 60–75%
Common failure: BM25 and vector results not normalized to same scale before merging
Fix cycle 1: add iron rule — "BM25 scores and vector scores must be normalized to [0,1] before merge"
Fix cycle 2: per-remaining-gap patch
Escalate if: < 85% after cycle 3
Note: two new patterns simultaneously (BM25 normalization + vector normalization) = 3 cycles expected
```

### IncrementalTrainer (CapabilityClass)

```
Expected cycle 1 score: 50–70%
Common failure: training data not isolated per session — bleeds across sessions
Fix: add iron rule — "training data must be scoped by SessionId before applying LoRA update"
Escalate if: < 85% after cycle 3
```

### ContinuousLearningPipeline (CapabilityClass)

```
Expected cycle 1 score: 65–80%
Common failure: observationCount not incremented atomically — race between Event 3 and Event 4
Fix: add iron rule — "confidence update and observationCount increment must complete in one DomainModelRegistry call"
Escalate if: < 85% after cycle 2
```

---

## Section 3 — Cycle-Specific Escalation Triggers

**Always escalate:**
- STRUCTURAL_FAILURE on any cycle
- Score decreases between consecutive cycles (regression)
- Same failure mode recurs after a targeted patch was applied
- Expected-failure capability class still failing after its full cycle budget

**Never escalate before budget exhausted for these:**
- Multi-pattern capabilities (HybridRetriever, ContinuousLearningPipeline): do not escalate before cycle 3
- First instance of any capability class in this project: cycle 1 < 85% is expected
- NEAR-PASS on cycle 1 for an established capability class: add one targeted patch, do not escalate

---

## Section 4 — Failure Map Table Format

Produce this for every capability class before any implementation phase:

```markdown
## Failure Map — [CapabilityClass]

| Capability class | Archetype | Cycle 1 expected | Known failure | Fix action | Escalate trigger |
|---|---|---|---|---|---|
| WeightedScoring | SCORING | 70–85% | Weight sum ≠ 1.0 | Add iron rule: weight normalization | < 60% cycle 1 |
| HybridRetriever | HYBRID_SEARCH | 60–75% | Score scale mismatch | Add iron rule: normalize before merge | < 85% cycle 3 |
| IncrementalTrainer | TRAINING | 50–70% | Session bleed | Add iron rule: scope by SessionId | < 85% cycle 3 |
```

This table belongs in the phase plan's adaptation map section.
It is not optional even when cycle counts are low — it determines what happens
per score bracket without ad-hoc diagnosis each cycle.

---

## Anti-Patterns

1. **"Score 68% is fine, it's close to NEAR-PASS"** — 60–69% is PATCH, not NEAR-PASS.
   PATCH requires identifying a specific failing invariant before re-running. Not retrying.

2. **"I'll escalate as soon as cycle 1 misses 85%"** — NEAR-PASS and PATCH are not
   escalation triggers. Only STRUCTURAL_FAILURE or budget exhaustion triggers escalation.

3. **"The failure map is for simple capabilities only"** — the failure map is most valuable
   for complex capabilities. A simple capability might not fail; a complex one will.
   Pre-declaring where it fails gives the executor a response plan, not just a diagnosis task.

4. **"Score 0 = failure"** — for inversion-pattern capabilities, score 0 on cycle 1
   verifies that the inversion is enforced. Treat as PASS-CONDITIONAL.

---

## Integration

- cycle-budget-prediction-SKILL.md determines the cycle budget that bounds this map
- capability-improvement-cycle-SKILL.md Stage 3 uses the score bracket from this map
- phase-retrospective-SKILL.md Dimension 1 (cycle count accuracy) measures whether cycle budgets were right
- iron-rule-derivation-SKILL.md produces the iron rules that fix PATCH-class failures
- diagnostic-escalation-SKILL.md governs the escalation briefing when escalation is triggered
