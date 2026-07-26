# Design Conversion Model Lifecycle And Promotion Guide

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
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

This guide describes the lifecycle for a trained DesignConversionMachine: checkpoint,
manifest, export/import, locator/server upload, promote/rollback,
common/adaptive split, and user approval before retrain/upload.

## When to use

Use this guide when a plan/review/work order mentions model asset,
checkpoint, manifest, export, import, backup, server upload, locator update,
promotion, rollback, common model, adaptive user model, retraining, or
continue-training for DesignConversionDomain.

## Inputs

- Validated training run report with held-out metrics and ablation result.
- Current checkpoint and manifest if continuing from an existing model.
- Common/global asset policy and personal/adaptive asset policy.
- User approval record when private or adaptive data is used.
- Server/locator destination for 25.ModelServer or approved model repository.
- Rollback candidate and previous promoted version.

## Outputs / artifacts

Every lifecycle pass must produce:

- `checkpoint_ref`: trained state artifact with version and hash.
- `manifest_ref`: machine id, domain tag, version, source corpus refs, metrics,
  gates, privacy scope, and compatibility.
- `export_package_ref`: portable package for backup/share/import.
- `import_report_ref`: proof that package can be imported.
- `fresh_load_report_ref`: proof that a fresh machine loads without retraining.
- `continue_training_report_ref`: proof that training can resume from the
  imported checkpoint.
- `locator_update_ref`: server/locator entry or explicit not-uploaded reason.
- `promotion_report_ref`: promote/keep/rollback decision.
- `rollback_report_ref`: previous version restore path.
- `approval_ref`: user approval for retrain/upload when needed.

## Required gates

- Checkpoint gate: no trained claim without checkpoint ref and hash.
- Manifest gate: checkpoint must have manifest with domain, version, data refs,
  metrics, gates, privacy scope, and compatibility notes.
- Export/import gate: the model must export and import through project
  infrastructure, not only exist as a local file.
- Fresh-load gate: a new machine instance loads exported state and runs held-out
  inference without retraining.
- Continue-training gate: imported state accepts an approved next batch and
  records metric continuity.
- Locator/server upload gate: promotion must update an approved locator or
  server entry, or record why upload is intentionally blocked.
- Promote/rollback gate: promotion must be reversible to the previous accepted
  checkpoint.
- Common/adaptive split gate:
  - common/global model assets live in `llm_mvp_core` approved model/training
    asset paths;
  - personal/adaptive assets live in the user-approved local state path;
  - personal data cannot silently enter common/global assets.
- User approval gate: retraining on private/user material and uploading an
  adaptive checkpoint require explicit user approval and recorded scope.

## Forbidden substitutes

- A saved checkpoint without manifest is not lifecycle completion.
- A manifest without import/fresh-load/continue-training probes is not enough.
- Uploading a file without locator/server registration is not promotion.
- Promotion without rollback path is incomplete.
- Common/global checkpoint cannot contain private user screenshots, raw design
  payloads, user ids, project paths, or unapproved proprietary data.
- Adaptive user model cannot be overwritten by common model promotion without
  conflict handling and user approval.

## How it feeds the future plan

Details 8a must include lifecycle work orders after training and before any
trained readiness claim. The plan must name checkpoint, manifest, export/import,
fresh-load, continue-training, locator/server upload, promote/rollback,
common/adaptive split, and user approval paths. This guide prevents "saved
model" from being reported as usable project asset before lifecycle proof.

## Pass40/41 Real-S3 Approval-Validation Boundary

For real-S3 lifecycle work, user/source approval is an intake gate, not a
promotion gate. The approval packet must be classified by `approval-validation`
before any private/adaptive source material can feed lifecycle artifacts.

Routes:

- `approval_packet_missing`
- `template_not_approval`
- `approval_validation_failed`
- `approval_valid_but_not_product_ready`

Only `approval_valid_but_not_product_ready` permits continuation into real-data
artifact construction. It still requires `real_data_still_required` because
approval does not create or validate `source-manifest.json`, `dpo-rows.jsonl`,
`heldout-measurements.json`, or `checkpoint-manifest.json`. A template, scaffold,
schema, or focused validator test is not a checkpoint, not a locator update, not
a rollback path, and not a product-ready promotion; `schema/template/scaffold/focused tests do not prove`
lifecycle readiness.
