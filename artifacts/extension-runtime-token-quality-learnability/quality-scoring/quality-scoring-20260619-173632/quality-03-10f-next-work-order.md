# QUALITY-03-NON-INTER-RATER-SOURCE-AUTHORING-APPROVAL-AND-PROOF-BUNDLE-10F

## Goal
Author the missing source/license/consent and row-assignment files that 10E could not locally justify. Build a proof bundle and run producer/runner only if every approval gate is proven by local evidence.

## Required Files To Author

1. `source-license-provenance-partition.jsonl` — one row per candidate source with provenance, license/consent, universal-only extraction, route, and reason.
2. `approved-train-source-corpus-manifest.json` — approved only when included sources have accepted license/consent, source hash, universal extraction, no stack/private leakage, and no raw external AI target text.
3. `approved-held-out-split-manifest.json` — source-disjoint train/held-out split with zero overlap by source_ref, doc_id, source_hash, path, title, prompt hash, correction text, and nearest-neighbour source.
4. `approved-raw-private-corpus-ref.json` — replay-diagnostic-only refs with approved paths/hashes and no-training role.
5. `vendor-trace-exclusion-ledger.jsonl` — explicit exclusion for raw external AI/vendor/runtime traces from all training targets.
6. `quality-03-row-assignment-approval.jsonl` — exactly 9 rows with approved train corpus ref, held-out split ref, raw-private ref, and non-deferred assignment.
7. `approved-held-out-source-corpus-manifest.json` — approved summary manifest only after prior files pass.
8. `row-specific-nearest-neighbour-index-manifest.json` — built only after approved corpus exists.

## Tests

- Parse every JSON/JSONL file.
- Assert no common-training row has UNKNOWN license/consent.
- Assert train/held-out overlap is zero by source_ref/doc_id/source_hash/path/title/prompt/correction/nearest-neighbour source.
- Assert raw-private/vendor traces are diagnostic-only and excluded from training manifests.
- Assert exactly 9 row assignment rows and no assignment is NONE/DEFER.
- Assert universal-only extraction excludes stack-specific/private details.
- Run proof producer and proof runner only after valid approved manifest and proof bundle exist.
- Run `git diff --check` and whitespace checks.
- Run focused tests only if code changes.

## Forbidden

No git stage/commit/push/branch/checkout. No external AI/web/model calls. No training/DPO/checkpoint/model/locator mutation. No raw external AI output as training target. No self-approval from file existence. No false PASS.
