# QUALITY-03-NON-INTER-RATER-ALTERNATIVE-PERMISSIVE-SOURCE-CANDIDATE-REVIEW-10N

## Goal

Independently review the 10M alternative source candidates. The reviewer may accept only source/material classification if safe. The reviewer must not approve common training or user-local training unless exact source/license/provenance/consent/training-use fields and independent acceptance exist.

## Allowed write scope

- A new timestamped folder under `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\quality-scoring\`
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`

## Must load/read

- `AGENTS.md` and `CLAUDE.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260621-102959/quality-03-10m-source-authoring-packet.json`
- `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260621-102959/quality-03-10m-alternative-permissive-source-candidate-matrix.jsonl`
- `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260621-102959/quality-03-10m-row-route-matrix.jsonl`
- 10K and 10L packet refs named by 10M, only as needed for review.

## Forbidden

No git. No external AI/web/model calls unless a future current Luba instruction explicitly authorizes source acquisition. No training, DPO, checkpoint, model, locator, target mutation, proof bundle, producer/runner PASS, raw-private trace as training target, or self-approval.

## Verification

Parse all new JSON/JSONL artifacts. Assert no candidate becomes accepted/common-training-ready without exact source_license_ref, license_hash, provenance, consent, training_use_approval_ref and independent acceptance. Assert common_training_allowed remains false for current proof rows unless legitimately accepted. Assert raw-private rows stay quarantine/evidence-only. Assert no training/model/checkpoint/locator mutation happened. Run Markdown whitespace check for new docs.
