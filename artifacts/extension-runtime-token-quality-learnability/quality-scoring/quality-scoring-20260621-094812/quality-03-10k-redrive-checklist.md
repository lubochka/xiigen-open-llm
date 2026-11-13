# QUALITY-03 10K redrive checklist

## 10K-CHECKLIST-001: Select a candidate rule, not a raw trace
Use the matrix route. Raw-private and diagnostic traces stay evidence-only/quarantine; do not copy raw output into chosen/rejected text.

## 10K-CHECKLIST-002: Bind an independent permissive source locally
Provide source_ref, doc_id, source_sha256, source_file_or_upstream_ref, source_license_ref, license_text_ref, license_hash, license_class/SPDX, and provenance_ref. No web calls unless a future work order explicitly authorizes them.

## 10K-CHECKLIST-003: Rewrite as universal orchestration/skill behavior
Author new universal_candidate_text in neutral language. Remove project paths, host/vendor names, implementation-stack details, branch names, private people/session details, and exact transcript replay.

## 10K-CHECKLIST-004: Separate common, user-local, evidence-only, and quarantine
Common candidate may become common only after accepted permissive packet and source-disjoint controls. User-local candidate needs explicit user-local approval and store manifest. Evidence-only and quarantine never become training targets.

## 10K-CHECKLIST-005: Require independent acceptance
Do not self-approve. Fill reviewer_id, accepted_packet_ref, created_utc, redaction_ref/status, privacy_class, source_disjoint_group_id, and train_or_heldout_after_eligibility only after review.

## 10K-CHECKLIST-006: Keep proof bundle and runner locked
Until all 9 rows have accepted source/license/provenance/training-use or approved replay-only refs, do not create proof bundle, producer outputs, runner PASS, DPO, training, checkpoint, locator, or target mutation.
