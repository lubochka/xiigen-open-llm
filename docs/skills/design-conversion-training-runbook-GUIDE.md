# Design Conversion Training Runbook Guide

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

Этот guide задает training/evaluation runbook для DesignConversionDomain:
какие метрики скорости, точности и visual quality нужны, какие probes
обязательны, и где training claim остается закрытым.

## When to use / Когда использовать

Используй этот guide при планировании training runner, evaluation runner,
promotion gate, held-out eval, ablation, speed measurement, quality report,
fresh-load probe или continue-training probe для `DesignConversionMachine`.

## Inputs / Входы

- Training-ready DPO (Direct Preference Optimization) chosen/rejected rows.
- Quarantine and rejected-row reports.
- Source-disjoint held-out evaluation set.
- Render QA (Quality Assurance) screenshot artifacts.
- Visual taxonomy metrics and human verdicts.
- Current checkpoint and manifest if training continues from existing state.
- Tier policy inputs: loop limit, cost budget, quality threshold, and stop
  condition.

## Outputs/artifacts / Выходы и артефакты

Training runbook outputs must include:

- `training_run_id` and machine version.
- Input corpus refs and hashes.
- Held-out eval refs and source-disjoint proof.
- Before/after accuracy metrics.
- Before/after visual quality metrics.
- Speed/cost metrics.
- Ablation/no-hidden-determinism report.
- Fresh-load probe result.
- Continue-training probe result.
- Checkpoint ref and manifest ref.
- Promotion recommendation: promote, rollback, keep candidate, or quarantine.
- Human-readable report with remaining gaps.

## Required gates / Обязательные gates

- Speed metrics gate:
  - render time per viewport/state;
  - visual review time per row;
  - train time per batch;
  - inference time per conversion request;
  - cost per user tier and per visual loop.
- Accuracy metrics gate:
  - chosen-vs-rejected preference accuracy;
  - held-out source-disjoint accuracy;
  - taxonomy classification accuracy where applicable;
  - correction acceptance rate;
  - before/after delta after each batch.
- Visual quality metrics gate:
  - layout, typography, color, spacing, responsiveness, image/crop,
    asset-missing, and interaction/state scores;
  - overall visual quality threshold;
  - major/blocking defect rate;
  - responsive pass rate.
- Held-out eval gate: no promotion without source-disjoint held-out set.
- Ablation/no-hidden-determinism gate: rerun eval without source ids, file
  paths, export ids, candidate order, deterministic template shortcuts, and
  fixed visual-diff thresholds as final authority.
- Fresh-load probe: export checkpoint/manifest, load into a fresh machine, and
  rerun held-out inference without retraining.
- Continue-training probe: load existing checkpoint, apply a small approved
  training batch, and prove metrics/report continuity.
- Regression gate: compare against previous promoted checkpoint and rollback
  when quality, speed, or cost gates regress beyond policy.

## Forbidden substitutes / Запрещенные подмены

- Training loss alone is not visual quality.
- A render QA pass alone is not model accuracy.
- A checkpoint file without manifest, fresh-load, and continue-training probes
  is not a completed trained model asset.
- Synthetic fixture pass cannot prove real-source held-out quality.
- Faster inference cannot justify visual quality below tier threshold.
- A deterministic visual-diff score cannot replace trained held-out evaluation.

## How it feeds the future plan / Как это питает будущий план

Детали 8a должны include a training runbook section that names exact metrics,
thresholds as policy inputs, held-out construction, ablation, fresh-load,
continue-training, promotion, and rollback. This guide turns "measure speed,
accuracy, and quality" into concrete artifact rows the future plan can gate.

## Pass40/41 Real-S3 Approval-Validation Boundary

The training runbook may not start real-S3 training, held-out evaluation, upload,
or promotion from an approval template or checklist. It must first classify the
approval packet through `approval-validation`:

- `approval_packet_missing`: blocked before real-data work.
- `template_not_approval`: template/scaffold/checklist only; blocked.
- `approval_validation_failed`: packet supplied but fields, scope, rights, or
  provenance authority failed validation; blocked.
- `approval_valid_but_not_product_ready`: approval validation passed only as an
  intake gate; `real_data_still_required` remains true.

After a valid approval packet, the required proof still comes from
`source-manifest.json`, `dpo-rows.jsonl`, `heldout-measurements.json`, and
`checkpoint-manifest.json`, plus measurement, ablation, fresh-load, and
continue-training evidence. `schema/template/scaffold/focused tests do not prove`
training, checkpoint quality, upload authority, or product readiness.
