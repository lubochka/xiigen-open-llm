# QUALITY-03-NON-INTER-RATER-ALTERNATIVE-PERMISSIVE-SOURCE-AUTHORING-10M

## Goal

Find or author a local alternative permissive-source candidate packet for blocked QUALITY-03 common rows using only locally available permissive/open material with exact license/provenance fields, or keep rows no-pass with a concrete source acquisition/approval queue.

## Allowed write scope

- A new timestamped folder under `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\quality-scoring\`
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`

## Must load/read

- `AGENTS.md` and `CLAUDE.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260621-101036/quality-03-10l-review-packet.json`
- `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260621-101036/quality-03-10l-candidate-source-review-matrix.jsonl`
- `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260621-101036/quality-03-10l-row-route-matrix.jsonl`
- `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260621-101036/quality-03-10l-no-pass-deferred-packet.json`
- The 10K candidate matrix and checklist refs named in the 10L review packet.

## Required outputs

- Russian human-readable report.
- Machine-readable source-authoring packet or no-pass packet.
- Per-row candidate mapping with exact `source_license_ref`, `license_hash`, `provenance`, `consent`, `training_use_approval_ref` fields where present.
- Updated row route matrix.
- Exact next bounded work order.
- Updated execution state with human board total/done/current/remaining.

## Forbidden

No git. No external AI/web/model calls. No training, DPO, checkpoint, model, locator, target mutation, proof bundle, producer/runner PASS, raw-private trace as training target, or self-approval.

## Verification

Parse all new JSON/JSONL artifacts. Assert no row becomes approved unless exact source/license/provenance/consent/training-use fields and independent acceptance are present. Assert raw-private rows stay quarantine/evidence-only. Assert no training/model/checkpoint/locator mutation happened. Run whitespace check for new Markdown artifacts.
