# Design Conversion Tier Budget Policy Guide

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

Этот guide определяет policy inputs for free/paid/pro design conversion tiers:
quality thresholds, visual loop limits, cost budgets, and stop conditions.
Policy is an input to orchestration and promotion; it is not hardcoded model
logic and not a substitute for trained quality.

## When to use / Когда использовать

Используй этот guide when a plan mentions user tier, free/paid/pro mode,
quality threshold, visual loop count, cost budget, render budget, external
provider approval, stop condition, or promotion criteria for DesignConversion.

## Inputs / Входы

- User tier: `free`, `paid`, `pro`, or future approved tier.
- Render QA (Quality Assurance) cost and timing metrics.
- Visual quality thresholds per taxonomy category.
- Inference time budget.
- Training or review loop budget.
- External visual AI (Artificial Intelligence) approval state when applicable.
- User override or approval for extra loops/cost when allowed.

## Outputs/artifacts / Выходы и артефакты

Tier policy must emit:

- `tier_policy_id` and version.
- `quality_thresholds`: minimum visual quality per tier.
- `loop_limits`: maximum render/retry/review loops per tier.
- `cost_budget`: maximum cost per request, batch, or promotion run.
- `time_budget`: maximum inference/render/review duration.
- `stop_condition`: when to return best effort, ask approval, quarantine, or
  escalate.
- `external_provider_policy`: whether external visual AI evidence is forbidden,
  approval-required, or allowed for a scoped run.
- `policy_decision_ref`: artifact recording which policy input stopped or
  allowed the loop.

## Required gates / Обязательные gates

- Free tier gate: lower loop/cost budget, conservative output, no silent
  external provider calls, clear stop condition when quality is insufficient.
- Paid tier gate: higher loop budget and quality threshold than free, but still
  bounded by cost and explicit external-provider approval.
- Pro tier gate: highest configured quality threshold and loop budget, with
  optional approved extended review/render loops.
- Policy-as-input gate: thresholds, limits, and stop conditions are policy
  inputs passed to orchestration/eval, not hardcoded inside model weights or
  hidden deterministic shortcuts.
- Quality floor gate: no tier may promote or claim trained readiness below its
  configured quality floor.
- Cost visibility gate: render time, review time, inference time, training time,
  and external evidence cost must be visible in reports.
- User approval gate: exceeding tier budget or using approval-bound external
  visual AI requires explicit approval.

## Forbidden substitutes / Запрещенные подмены

- A tier label cannot change model behavior through hidden hardcoded branches.
- A free-tier stop is not model failure unless the quality gate says so.
- A pro-tier extended loop cannot hide missing DPO/training evidence.
- Cost budget cannot justify using private data in common/global training.
- External visual AI cannot run silently because a tier is paid/pro.
- Tier policy cannot replace held-out eval, ablation, lifecycle, or promotion
  gates.

## How it feeds the future plan / Как это питает будущий план

Детали 8a должны define tier policy before visual loops and promotion gates.
The plan must state per-tier thresholds, loop limits, cost/time budgets, stop
conditions, reporting, and approval paths. This guide keeps economics and user
experience as explicit policy, not hidden model logic.
