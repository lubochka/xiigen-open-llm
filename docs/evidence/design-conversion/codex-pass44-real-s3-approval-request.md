# Codex Pass44 real-S3 approval/evidence request

claim_scope: approval/evidence request for `training-data/design-conversion/real-s3-v1`; this artifact does not grant approval, readiness, training rights, evaluation rights, screenshot/image usage, source authority, DPO data, held-out measurements, checkpoint evidence, upload permission, or promotion permission.

evidence_scope: derived from current workspace reads of `AGENTS.md`, `docs/SESSION-START-PROMPT.md`, current real-S3 schema/template/intake files, current evidence docs, and readable prior isolated clone file listings/comparisons. Prior Pass40/41/42/43 text is evidence-only until directly verified.

count_claimed: 1 codex-prefixed request artifact created; 0 approvals; 0 real `codex-approval-packet.json`; 0 real `source-manifest.json`; 0 real `dpo-rows.jsonl`; 0 real held-out measurement files; 0 real `checkpoint-manifest.json`; 0 tests/builds/training/uploads/promotions.

independent_evidence_count: current-turn direct read/inspection records only; this file does not convert schema, template, README, or ledger evidence into product readiness.

subagent_id_or_NONE: NONE - no callable sub-agent launcher was exposed in this session; parent created this allowed evidence artifact only.

downgrade_needed: yes for any approval, data, readiness, training, evaluation, checkpoint, upload, promotion, or external platform integration claim.

## Status

This is not approval.

This is not readiness.

This is not `codex-approval-packet.json`.

This is not `source-manifest.json`.

This is not `dpo-rows.jsonl`.

This is not `heldout-measurements.json`, `heldout-measurements.jsonl`, or any other heldout-measurements evidence file.

This is not `checkpoint-manifest.json`.

The current real-S3 directory contains schemas, templates, expectations, candidate/source review files, and decision-prep/blocker files. It does not contain the required real approval/data artifacts listed below.

## Missing approval fields

The approval packet must provide these fields before any approval-dependent route can resume:

- `approver`: who approved, role/source, contact or non-secret reference id, authority basis, and authority reference.
- `authority_basis`: explicit owner, reviewer, legal, maintainer, delegated authority, or other checkable basis.
- `approval_date`: concrete date or timestamp of the approval/denial.
- `scope`: exact source bundle, covered material types, covered scenes, and no silent scope expansion beyond login/cart.
- `allowed_scenes`: explicit login/cart inclusion and covered materials for each scene.
- `excluded_scenes/mail_decision`: explicit mail decision; absence means mail stays excluded.
- `license_decision`: explicit license or custom decision, decision reference, allowed use summary, and restrictions.
- `training_rights`: explicit allowed/denied training rights, covered source materials, model/dataset use, restrictions, and review/expiry if any.
- `evaluation_rights`: explicit allowed/denied evaluation rights, covered source materials, metrics/held-out use, restrictions, and review/expiry if any.
- `provenance_source_authority`: source owner/authority, source bundle identity, origin reference, chain to the login/cart route, reviewer/owner authority ref, and restrictions.
- `screenshot_image_usage`: explicit allowed/denied/not-applicable screenshot/image usage, covered image types, allowed use, redaction/exclusion rules, and restrictions.
- `restrictions`: global, scene, privacy, redistribution, upload/promotion, attribution, expiry, or explicit none statement.
- `validation_required`: must be true; approval text alone is not enough.

The schema/control fields also need to preserve: `approval_validation_status`, `validation_required_before_any_resume`, `approval_bypass_allowed=false`, `validation`, and `blocked_until_validation_pass`.

## Required artifact files

- `codex-approval-packet.json`: owner/Luba-provided approval packet matching `codex-approval-packet.schema.json`; needed before validation can check authority, scope, rights, restrictions, provenance, screenshot/image usage, and mail decision.
- `source-manifest.json`: real, schema-valid source manifest; needed only after approval validation passes, to bind source refs, hashes, license/authority, privacy guard, and source-disjoint identity.
- `dpo-rows.jsonl`: real DPO rows; needed only after approval/source validation, with chosen/rejected safe refs, teacher labels, curriculum tier, validation outcome, provenance, and leakage guard.
- `heldout-measurements`: real held-out measurement evidence, for example `heldout-measurements.json` or `heldout-measurements.jsonl`; needed to prove source-disjoint metrics, anti-leakage, ablation, thresholds, runtime/cost, and promotion status.
- `checkpoint-manifest.json`: real checkpoint manifest; needed to prove checkpoint refs/hashes, learned weights, fresh-load probe, continue-training probe, row counts, measurement refs, and upload/promotion status.

## What Luba or the owner can provide

- A completed `codex-approval-packet.json` using the current template and schema.
- A checkable approver authority basis and non-secret authority reference.
- An explicit login/cart scope decision and explicit mail decision.
- Explicit license decision and restrictions.
- Explicit training rights and evaluation rights.
- Explicit provenance/source authority tying the source bundle to the login/cart candidate route.
- Explicit screenshot/image usage decision, including redaction or exclusion rules.
- A statement of all restrictions, or an explicit "none" only if the authority truly says none.

If approval is denied, unclear, or incomplete, the next safe route is replacement-source decision prep or a blocked queue item with exact missing input. It is not permission to create DPO rows, held-out results, checkpoints, training runs, uploads, or promotions.

## Next workers

1. Approval validation worker: validate supplied `codex-approval-packet.json` against schema and expectations; check authority, scope, license, training rights, evaluation rights, provenance/source authority, screenshot/image usage, restrictions, mail decision, and no-bypass rules.
2. Source-manifest worker: only after approval validation PASS, create/validate `source-manifest.json` with source refs, hashes, privacy guard, license/authority, and source-disjoint identity.
3. DPO rows worker: only after source-manifest validation PASS, create/validate `dpo-rows.jsonl` with chosen/rejected safe refs, teacher labels, provenance, validation outcome, and leakage guard.
4. Heldout worker: only after DPO/source gates, create/validate heldout-measurements evidence with source-disjoint metrics, anti-leakage, ablation, thresholds, runtime/cost, and promotion status.
5. Checkpoint worker: only after held-out gate and training authority, create/validate `checkpoint-manifest.json` with checkpoint refs/hashes, learned weights evidence, fresh-load and continue-training probes, row counts, measurement refs, and upload status.
6. External platform/caller integration worker: only after approval, source, DPO, held-out, and checkpoint gates are valid; integrate caller/platform route without expanding scope, using secrets, uploading, or promoting unless separately authorized.

## Pass40/41 presence note

Readable old isolated clone evidence shows Pass41-like modified files in `44.DesignConversionDomain/DesignConversionRealS3EvidenceBundleValidator.cs` and `Tests/DesignConversionRealS3EvidenceBundleValidatorTests.cs`, plus Pass40-like docs/skills changes. Current workspace contains corresponding real-S3 validator/tests and docs/skills files, but direct no-index name-only comparisons showed differences from the old isolated clone. Therefore this pass can claim only that related files/markers are present in the current workspace; it cannot claim exact old-clone Pass40/41 patch parity.

## Boundary

Do not treat this request as approval. Do not treat this request as readiness. Do not create real `codex-approval-packet.json`, `source-manifest.json`, `dpo-rows.jsonl`, heldout-measurements files, or `checkpoint-manifest.json` from this artifact alone.
