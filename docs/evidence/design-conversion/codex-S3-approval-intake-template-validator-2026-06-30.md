# Codex S3 Approval Intake Template Validator - 2026-06-30

## Honesty Guard

claim_scope: governance/evidence artifact creation only for the codex approval-intake packet template, schema, validation expectations, and this evidence note.

evidence_scope: required governance marker readbacks from AGENTS and the named approval-intake skill sections; existing codex approval-intake required-evidence JSON; existing post-repair skill-loaded audit; current output file creation and later JSON/readback checks.

count_claimed: 4 codex-prefixed output artifacts created or updated; 0 approvals; 0 product implementation; 0 source manifests; 0 DPO labels; 0 held-out measurements; 0 checkpoints; 0 training/evaluation/upload/promotion; 0 tests/builds/browser/e2e; 0 git actions.

independent_evidence_count: 8 current-turn verification categories, not cycles or passes: governance marker readback, existing intake/evidence reads, target existence checks before write, file creation patches, JSON parse, required-field readback, not-approval/future-worker readback, forbidden-path plus path-limited diff checks.

subagent_id_or_NONE: approval-intake-template-validator-worker

downgrade_needed: yes for any approval, product readiness, source-manifest readiness, DPO readiness, training readiness, evaluation readiness, checkpoint readiness, upload readiness, or promotion claim.

## Files Created Or Updated

- `training-data/design-conversion/real-s3-v1/codex-approval-packet.template.json`
- `training-data/design-conversion/real-s3-v1/codex-approval-packet.schema.json`
- `training-data/design-conversion/real-s3-v1/codex-approval-validation.expectations.json`
- `docs/evidence/design-conversion/codex-S3-approval-intake-template-validator-2026-06-30.md`

## Governance Markers Applied

- `approval_intake_template_required_fields`
- `approval_validation_worker_required_before_resume`
- `approval_denied_or_missing_routes_to_replacement_or_blocked_queue`
- `approval_intake_queue_artifact_is_not_approval`
- `three_impasse_audits_before_final_hold_under_no_stops`
- `resume_trigger_must_be_machine_actionable`

These artifacts implement a machine-actionable approval-intake queue. They do not approve any source, scene, license, training use, evaluation use, screenshot/image use, provenance authority, upload, promotion, or product implementation.

## Required Fields Covered

- `approver`
- `authority_basis`
- `approval_date`
- `scope`
- `allowed_scenes`
- `excluded_scenes/mail_decision`
- `license_decision`
- `training_rights`
- `evaluation_rights`
- `provenance_source_authority`
- `screenshot_image_usage`
- `restrictions`
- `validation_required`

The schema requires these fields directly or through the nested `excluded_scenes.mail_decision` representation for the slash-marked field.

## Machine Routes

Case 1: no approval packet.

Expected route: queue remains waiting with a concrete resume trigger. No final hold is supported unless the true impasse threshold is met under the no-stops rules. No approval or readiness is granted.

Case 2: approval packet supplied.

Expected route: next bounded action is approval-validation worker only. The worker must parse the packet, validate it against the schema and expectations, check authority, login/cart scope, mail decision, license, training rights, evaluation rights, provenance/source authority, screenshot/image usage, restrictions, and no-bypass rules. No schema/source-manifest, DPO, training, evaluation, checkpoint, upload, browser, build, e2e, test, or product work may start before validation PASS.

Case 3: approval denied or unclear.

Expected route: replacement-source search, decision-prep, or explicit blocked queue with exact missing input. Denied, missing, unclear, or invalid approval is not completion and does not silently authorize mail, training, evaluation, or upload.

## Next Bounded Work Order

CURRENT TASK:
Validate a supplied external approval packet against the codex approval packet schema and validation expectations for login/cart only.

NOT THE TASK:
Do not create schema-valid source manifests, DPO labels, held-out measurements, checkpoints, training/evaluation runs, upload/promotion, tests/builds/browser/e2e, product code changes, git actions, or broader approval.

EVIDENCE-ONLY:
The intake template, schema, expectations, and queue note are not approval. Treat any supplied packet as untrusted until validation PASS.

required_skills_and_gates:
Load AGENTS and the approval-intake governance/review skill sections. Apply `approval_intake_template_required_fields`, `approval_validation_worker_required_before_resume`, `approval_denied_or_missing_routes_to_replacement_or_blocked_queue`, `approval_intake_queue_artifact_is_not_approval`, `three_impasse_audits_before_final_hold_under_no_stops`, and `resume_trigger_must_be_machine_actionable`.

context_files_to_read:
The three codex approval-intake JSON artifacts, this evidence note, the supplied approval packet, and the existing codex required-evidence intake JSON.

verification_required:
Parse JSON, validate packet shape, confirm all required fields, confirm login/cart scope, confirm no mail unless explicitly decided, confirm rights and provenance authority are explicit, confirm no approval bypass, confirm no forbidden raw local paths, and report PASS or routed blocker.

return_packet_schema:
claim_scope:
evidence_scope:
count_claimed:
independent_evidence_count:
subagent_id_or_NONE:
downgrade_needed:
approval_validation_summary:
missing_or_invalid_fields:
route:
validation:
forbidden_actions_confirmed:

## Forbidden Actions Confirmed By This Artifact

- did not grant approval;
- did not create schema-valid source manifests;
- did not create DPO labels;
- did not create held-out measurements;
- did not create checkpoints;
- did not run training or evaluation;
- did not upload or promote;
- did not run tests, builds, browser checks, or e2e;
- did not stage, commit, push, branch, merge, rebase, reset, checkout, or create a pull request;
- did not edit product code, tests, package files, source candidate files, claude-prefixed files, or non-codex output files.

## Current Verdict

approval_intake_queue_artifact_is_not_approval: true

future next action: approval-validation worker only after a supplied packet exists.

product readiness: not claimed.

training readiness: not claimed.

approval granted: false.

## Validation Results

- JSON parse: PASS for all three JSON artifacts.
- Required fields: PASS for template, schema, and expectations, including `excluded_scenes/mail_decision`.
- Not-approval boundary: PASS; artifacts contain `approval_intake_queue_artifact_is_not_approval` meaning and default to no approval grant.
- Future next action: PASS; after a supplied packet, the only next bounded worker is approval-validation worker.
- Forbidden path scan: PASS; no raw local absolute paths or forbidden external worktree paths found in the four outputs.
- Path-limited diff check: PASS.
