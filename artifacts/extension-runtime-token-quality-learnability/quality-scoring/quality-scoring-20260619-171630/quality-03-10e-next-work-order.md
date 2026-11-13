# QUALITY-03-NON-INTER-RATER-SOURCE-DECISIONS-FILES-AND-ROW-ASSIGNMENTS-10E

## Goal
Continue without asking Luba. Resolve exact local source decisions/files needed before any QUALITY-03 non-inter-rater PASS can be opened.

## Starting Counts
- Total proof refs: 63.
- PASS: 9/63.
- Non-inter-rater PASS: 0/54.
- Runner-ready rows: 0/9.
- Quality-pass rows: 0/9.

## First Gap To Close
Approved source authority is still missing: no approved train source corpus ref, no approved held-out split ref, no approved raw/private corpus ref, no row-level approved_assignment for the 9 rows, and no accepted source/license/provenance partition.

## Required Steps
1. Bind each QUALITY-03 row to exact local source refs using the source-license provenance ledger, source-disjoint split ledger, source-license packet routing, user local analysis index, and local xiigen roots.
2. For each source, produce a decision: COMMON_TRAINING_ALLOWED only with accepted license/provenance; USER_LOCAL_ONLY for user/private material; DIAGNOSTIC_ONLY for external AI/vendor/runtime traces; QUARANTINE for unknown/unsafe material.
3. Create these files only if local evidence proves them: `approved-train-source-corpus-manifest.json`, `approved-held-out-split-manifest.json`, `approved-raw-private-corpus-ref.json`, `source-license-provenance-partition.jsonl`, `vendor-trace-exclusion-ledger.jsonl`, `quality-03-row-assignment-approval.jsonl`, and then `approved-held-out-source-corpus-manifest.json`.
4. Only after those files pass, create row-specific NN/ablation/no-leakage/no-memorization artifacts and assemble `quality-03-non-inter-rater-proof-bundle.json` plus 54 per-row proof artifacts.
5. If any decision/file remains missing, produce a no-PASS packet and keep counts unchanged.

## Forbidden
No git stage/commit/push/branch/checkout. No external AI/web/model calls. No training/DPO/checkpoint/model/locator mutation. No raw external AI output as training target. No false PASS.

## Verify
JSON/JSONL parse. Producer + proof runner only if a valid proof bundle is created. `git diff --check`. Whitespace check for new artifacts.
