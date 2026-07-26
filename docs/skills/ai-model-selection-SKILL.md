# AI Model Selection

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

Every AI pipeline cycle has a cost. The same capability class can often be
served by a cheaper model once fitness data accumulates. Using an expensive
model on every cycle without checking whether a faster alternative is adequate
wastes budget. Dropping a model after one bad cycle loses the variation signal
needed for DPO triple generation. This skill defines the value ratio formula,
the three-consecutive signal for model replacement, the phase progression from
broad pool to archetype-optimized pool, and the manual fix threshold.

Project override - Model/Reasoning Configuration Gate:

This skill does not authorize downgrading project governance, planning,
plan-review, execution, code-review, execution-review, or completion-review
sub-agents. For this project, every such work order must set explicit
`requested_model=gpt-5.5` and `requested_reasoning_effort=xhigh` fields unless
Luba explicitly authorizes a
downgrade in the current unquoted instruction. If visible runtime/request
metadata or explicit work-order metadata differs from those requested values,
including medium/high/low/default reasoning instead of xhigh, emit a
user-visible warning, record the downgrade/unavailable model in state/log,
classify it as a governance violation, and stop before execution continues.
Generic system/developer identity text such as "GPT-5" is not downgrade evidence
by itself; when exact runtime metadata is unavailable but the work order
explicitly requests `gpt-5.5` / `xhigh`, continue with
`mode_verified_from_work_order=true` unless a concrete contradictory value is
visible. The tier-selection rules below apply only to explicitly authorized
AI-pipeline experiments or product model-pool design, not to bounded project
sub-agent authority. A false downgrade stop is a premature stop loophole.

---

## When to Invoke

- Planning the model pool for a new capability's AI pipeline
- When a model produces below-threshold scores on 3+ consecutive cycles
- When the cycle budget is nearing 80% and a decision to reduce the pool is needed
- When asking "is the cheaper model good enough to replace the current one?"

---

## Section 1 — Model Tier Hierarchy

In this project's AI pipeline, all models are Claude models. Tier selection
governs cost vs. capability trade-off per capability class.

| Tier | Model | Use case |
|---|---|---|
| TIER 1 — PREMIUM | claude-opus-4-7 | Complex orchestration, architectural decisions, ambiguous constraints |
| TIER 2 — STANDARD | claude-sonnet-4-6 | Default implementor and arbiter role |
| TIER 3 — FAST | claude-haiku-4-5 | High-volume scoring, mechanical checks, DPO triple generation |

**Starting pool for new capability classes:** [claude-sonnet, claude-opus] (1 TIER 2 + 1 TIER 1)
Mix ensures quality signal accumulates from cycle 1 and TIER 1 contributes
only where TIER 2 genuinely falls short.

---

## Section 2 — Value Ratio Formula

The value ratio is the primary signal for model fitness per capability class:

```
valueRatio = compositeScore / max(relativeCost, 0.001)
```

Where `relativeCost` is the normalized cost per cycle (TIER 3 = 1.0, TIER 2 = 2.5, TIER 1 = 8.0).

| Value ratio | Interpretation |
|---|---|
| > 500 | Highly cost-efficient — keep |
| 200–500 | Acceptable — monitor |
| 100–200 | Low value — drop unless unique contribution |
| < 100 | Definitely drop |

**Unique contribution exception:** If a model is the only one passing a specific
arbiter (e.g., only TIER 1 catches a principles violation that TIER 2 misses),
keep it regardless of value ratio until another model demonstrates the same coverage.

---

## Section 3 — Model Replacement Signal (Three-Consecutive Rule)

A cheaper model is ready to replace a more expensive one when:

```
Cycle N:   cheaper model score gap = 4.8% below expensive model
Cycle N+1: gap = 3.2% → cheaper close (2nd consecutive)
Cycle N+2: gap = 4.1% → cheaper close (3rd consecutive) → SIGNAL

Three consecutive cycles at < 5% gap for the same capability class:
  → Run one validation cycle with cheaper-only pool
  → If validation confirms: replace expensive model for this capability class
  → Record in DECISIONS-LOCKED.md as a model graduation event

One cycle at < 5%: noise. Do not act.
Two cycles: promising. Continue monitoring.
Three cycles: sufficient evidence. Run validation.
```

---

## Section 4 — When to Drop a Model

**Drop immediately (no cycles needed):**
- Model produces Rule 1-10 violation (principles-arbiter BLOCK) on 2 consecutive cycles
  for the same capability class — the model cannot implement this class correctly
- Model fails to compile (dotnet build error in generated code)

**Drop after 3 cycles:**
- valueRatio < 100 consistently across 3 cycles
- No unique contribution (every arbiter the model passes, the other model also passes)
- Score trending downward across 3 cycles (regression pattern for this model)

**Never drop based on 1 bad cycle:** Models have variance. One bad cycle is noise.

---

## Section 5 — Phase Progression

As fitness data accumulates, the model pool optimizes per capability class:

**Phase 1 (first 3 capability classes implemented):**
Use full pool [claude-sonnet, claude-opus].
Accumulate fitness data. Track: which capability class is each tier best at?

**Phase 2 (after 3+ capability classes per archetype type):**
Switch to archetype-optimized pools based on fitness data:
```
ROUTING capabilities:      [claude-sonnet] (haiku if data shows < 5% gap)
SCORING capabilities:      [claude-sonnet, claude-opus] (opus needed for edge cases)
TRAINING capabilities:     [claude-opus] (IncrementalTrainer needs highest quality)
ORCHESTRATION capabilities:[claude-sonnet, claude-opus]
```

**Phase 3 (after DPO triple accumulation and fine-tuning):**
A model fine-tuned on this project's DPO triples may replace TIER 2 for
most capability classes. Run validation sessions before replacement.
Track: does fine-tuned model maintain quality at 0 TIER 2 cost?

---

## Section 6 — Budget Management

```
Warning at 80% of cycle budget for this capability class:
  → Drop to single dominant model (highest score from prior cycles)
  → Inject best candidate as context seed for next cycle
  → One more cycle maximum

At 100% of cycle budget:
  → HALT. Take best available candidate.
  → Emit: escalation-orchestrator-SKILL.md Rule 4 (ESCALATE_TO_UPPER_JUDGE)
  → Manual fix is almost always faster than an additional cycle at this point
```

---

## Section 7 — Manual Fix Threshold

When should Luba fix the output manually instead of running another cycle?

**Manual fix is better when:**
- Score ≥ 85% with specific, localized violations (1-2 iron rule violations)
- Violation is mechanical: wrong field name, missing null guard, wrong error code
- 3+ cycles with no improvement above cycle 2 baseline
- The violation is at a single known line (e.g., "line 47: throw statement")

**Manual fix is NOT better when:**
- The violation is architectural (wrong pattern, not a localized change)
- Multiple interconnected violations (fixing one introduces another)
- Score < 70% (structural problems, not surface ones)
- The violation involves the public interface shape (changing interface = blast radius)

---

## Section 8 — Worked Example

```
Cycle 3 results for WeightedScoringService:
  claude-sonnet:  score=88, cost=2.5 (relative), valueRatio=35.2
  claude-opus:    score=91, cost=8.0 (relative), valueRatio=11.4

Assessment:
  claude-sonnet: higher value ratio — dominant cost-efficient model
  claude-opus:   higher score but 3× cost; check unique contribution
    Does opus pass any arbiter that sonnet does not?
    If NO unique contribution: drop opus after 3 cycles at this gap
    If YES (e.g., opus catches a principles violation sonnet misses): keep

  OSS signal check: gap = (91-88)/91 = 3.3% — close!
    If this persists for 2 more cycles: run claude-haiku validation round
```

---

## Anti-Patterns

1. **"Drop the cheaper model after one bad cycle"** — one bad cycle is noise.
   Accumulate 3 cycles before the drop decision.

2. **"Keep the expensive model because it's 'reliable'"** — if the expensive model
   has no unique contribution over 3 cycles, it is consuming budget for no value.
   Drop it for this capability class.

3. **"Run another cycle because the score is at 85% with one mechanical violation"**
   — at 85% with a mechanical violation, manual fix is faster and free.
   Apply the three-consecutive rule for model selection; use the manual fix threshold
   for cycle-vs-fix decisions.

4. **"Replace with cheaper model after 2 cycles at < 5% gap"** — the rule is three
   consecutive cycles. Two is promising, not sufficient.

---

## Integration

- ai-pipeline-design-SKILL.md defines the pipeline roles this skill selects models for
- meta-escalation-router-SKILL.md Section 3 uses budget data from this skill for cost estimates
- meta-arbiter-SKILL.md produces proposals that may trigger model pool changes
- cycle-budget-prediction-SKILL.md defines the cycle budget this skill manages against
- escalation-orchestrator-SKILL.md Rule 4 (ESCALATE_TO_UPPER_JUDGE) fires when budget is exhausted
