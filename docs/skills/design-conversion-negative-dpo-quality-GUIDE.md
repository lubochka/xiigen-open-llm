# Design Conversion Negative DPO Quality Guide

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

Этот guide определяет, каким должен быть rejected candidate в DPO (Direct
Preference Optimization) pair для DesignConversionDomain. Цель negative pair
не в том, чтобы показать мусор, а в том, чтобы обучить модель различать
правдоподобные near-miss ошибки дизайна.

## When to use / Когда использовать

Используй этот guide при создании, проверке или ревью negative DPO rows для
design-to-code conversion, especially when rejected HTML/CSS candidate will
train layout, typography, spacing, color, responsiveness, asset, crop, or state
decisions.

## Inputs / Входы

- Source design tree and source screenshots.
- Chosen candidate with rendered screenshots.
- Rejected candidate with rendered screenshots.
- Visual taxonomy verdict for the rejected side.
- Human correction explaining the dominant problem.
- Provenance and source-disjoint group.

## Outputs/artifacts / Выходы и артефакты

Each accepted negative pair must record:

- `negative_quality_class`: `near_miss`, `single_dominant_error`,
  `multi_error_quarantine`, `garbage_quarantine`, or `leakage_quarantine`.
- `dominant_error_category`: one taxonomy category that explains the rejection.
- `why_plausible`: why the rejected candidate is close enough to teach.
- `why_wrong`: the specific visual/layout error.
- `no_shortcut_leakage_evidence`: statement that source ids, paths, candidate
  order, tool ids, and reviewer ids do not decide the label.
- `source_disjoint_group`: group used for held-out construction.
- `quarantine_reason` when the pair is not trainable.

## Required gates / Обязательные gates

- Plausible near-miss gate: rejected output should look like a serious but
  wrong attempt at the same design, not random broken output.
- One dominant error gate: the rejected side should teach one main visual or
  layout error. Multiple unrelated errors go to quarantine or separate rows.
- Exact review phrase: high-quality negative pairs must keep one dominant error.
- No garbage negative gate: invalid HTML/CSS, blank renders, missing root, or
  arbitrary nonsense cannot become a high-quality negative pair.
- Same-source gate: chosen and rejected must be compared against the same
  original tree, screenshots, viewport matrix, and state contract.
- No shortcut leakage gate: labels must not be inferable from file path, source
  id, export id, candidate order, generator name, timestamp, reviewer id, or
  artificial naming.
- Source-disjoint held-out gate: held-out negatives must be built from source
  groups that do not overlap training groups.
- Human correction gate: the pair must include a correction that tells the
  model what visual decision should have been different.

## High-quality negative examples / Примеры хороших negative pairs

- Layout near-miss: same component hierarchy, but primary call-to-action is one
  grid column too far right.
- Typography near-miss: correct layout and text, but headline weight and line
  height are wrong.
- Color near-miss: correct structure, but semantic background and accent colors
  are swapped.
- Spacing near-miss: correct elements, but card padding and vertical rhythm are
  too dense.
- Responsiveness near-miss: desktop matches, mobile breakpoint stacks in the
  wrong order.
- Image/crop near-miss: correct asset, wrong object-fit and focal crop.
- State near-miss: base state matches, hover/focus/selected state is wrong.

## Forbidden substitutes / Запрещенные подмены

- Garbage negative: blank page, invalid markup, random colors, missing body, or
  unrelated page.
- Multi-error negative presented as a single teaching signal.
- Rejected candidate produced from a different source tree than chosen.
- Negative pair where the row label leaks through filename, folder, source id,
  candidate index, or tool name.
- Held-out pair from the same template/export/project family as training.
- Human preference without taxonomy finding and correction.

## How it feeds the future plan / Как это питает будущий план

Детали 8a должны add negative DPO quality gates before training batches are
accepted. The plan must define near-miss construction, dominant-error taxonomy,
garbage/multi-error quarantine, source-disjoint held-out rules, and shortcut
leakage checks. This guide prevents the future model from learning artifacts of
bad data construction instead of visual quality.
