# Design Conversion DPO Authoring Skill

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

This skill describes how to author DPO (Direct Preference Optimization) rows
for DesignConversionDomain. It does not run training and does not write product code;
it defines the data shape, quality gates, and quarantine rules for a future plan.

## When to use

Use this skill when you need to plan or review DPO rows for
Figma/Canva/design-to-code conversion: chosen/rejected HTML/CSS candidates,
human corrections, visual verdicts, negative pairs, held-out split, training
batch schema, or provenance fields.

## Inputs

- `target_machine_or_internal_unit`: `DesignConversionMachine` or the exact
  internal flow/unit being trained/evaluated under that machine.
- `source_structure`: Figma, Canva, HTML/CSS snapshot, image reference, manual
  correction, approved fixture, or other approved source type.
- `original_tree`: normalized design tree artifact.
- `original_screenshots`: source screenshots with viewport/state metadata.
- `html_css_candidate`: candidate HTML/CSS bundle.
- `rendered_screenshots`: server-rendered candidate screenshots.
- `visual_verdict`: taxonomy-based verdict and metrics.
- `correction`: human or approved correction explaining the intended fix.
- `decision`: chosen, rejected, or quarantine.
- `provenance_hash_fields`: source payload hash, normalized tree hash,
  screenshot hash, candidate hash, render hash, verdict hash, correction hash,
  row hash.

## Outputs / artifacts

Each DPO row must contain this context vector:

```text
row_id
target_machine_or_internal_unit
source_structure
source_license_or_user_approval
source_disjoint_group
privacy_scope
original_tree_ref
original_screenshot_refs
html_css_candidate_ref
rendered_screenshot_refs
visual_verdict_ref
correction_ref
chosen_candidate_ref
rejected_candidate_ref
quarantine_reason
dominant_error_category
viewport_state_coverage
provenance_hash_fields
created_at_utc
reviewer_or_approval_ref
```

The row can be one of three artifact classes:

- `chosen_rejected_pair`: training-ready after all gates pass.
- `quarantine_row`: preserved as evidence but not trainable.
- `held_out_eval_row`: source-disjoint row used for evaluation, not training.

## Required gates

- Context-vector completeness gate: no row enters training without every
  context vector field that applies to the row class.
- Chosen/rejected clarity gate: chosen and rejected candidates must answer the
  same source structure and viewport/state contract.
- Visual verdict gate: the rejected side must be tied to taxonomy evidence, not
  only to a vague preference.
- Correction gate: a correction must say what should change visually or
  structurally, in human-readable language.
- Provenance/hash gate: every referenced artifact must be hash-addressable.
- Quarantine gate: ambiguous source, missing asset, unstable render, multi-error
  negative, source leakage, or unclear correction must quarantine the row.
- Held-out split gate: held-out rows must be source-disjoint from training.
- No-shortcut-leakage gate: source id, file path, export id, candidate order,
  reviewer id, or tool id cannot be predictive features.

## Forbidden substitutes

- A prompt plus chosen/rejected text is not enough for design conversion DPO.
- A screenshot-only preference without original tree and rendered candidate is
  not a complete DPO row.
- A rejected candidate that is random garbage is not a useful negative.
- A row whose answer can be inferred from file path, source id, or candidate
  order is leakage, not learning.
- A quarantine row cannot be counted as training data.
- A held-out eval row cannot be reused for training promotion.

## How it feeds the future plan

Details 8a must include a DPO authoring section that imports this skill as the
schema and gate definition. The plan must state how rows are built from intake,
render QA, visual taxonomy, human correction, and provenance; how rows split
into chosen/rejected/quarantine/held-out; and how training batches are blocked
when context vector, negative quality, or leakage gates fail.

## Pass40/41 Real-S3 Approval-Validation Boundary

For real-S3 source material, DPO row authoring must first pass the
`approval-validation` route classifier. Missing, template, or failed approval
routes block authoring:

- `approval_packet_missing`
- `template_not_approval`
- `approval_validation_failed`

The only non-blocking approval route is `approval_valid_but_not_product_ready`.
That route means the worker may continue constructing or validating real-data
artifacts; it does not create `source-manifest.json`, `dpo-rows.jsonl`,
`heldout-measurements.json`, or `checkpoint-manifest.json`, and it does not prove
training, promotion, upload, or product readiness. `real_data_still_required`
remains true. `schema/template/scaffold/focused tests do not prove` a DPO row is
train-ready.
