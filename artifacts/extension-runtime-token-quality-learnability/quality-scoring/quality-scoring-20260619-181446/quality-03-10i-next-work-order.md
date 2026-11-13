# QUALITY-03-NON-INTER-RATER-FRESH-SOURCE-LICENSE-CONSENT-AND-ROW-BINDING-SUPPLY-10I

## Goal
Supply the evidence missing in 10H: explicit source/license/consent bindings for exact QUALITY-03 source_ref/doc_id/source_hash rows and the missing source binding for QUALITY-03-REVIEW-ROW-001.

## Required
1. Fresh current-owner/Luba approval quote or local license/provenance metadata file with source_license_ref/license_hash.
2. Row 001 source_scenario_id/source_ref/doc_id/source_hash binding.
3. Per-row route: common_training_allowed only for permissive/explicit universal material; otherwise USER_ADAPTIVE_ONLY/EVIDENCE_ONLY/QUARANTINE.
4. No raw external AI output as training target.
5. No proof bundle until all approval gates pass.

## Forbidden
No git stage/commit/push/branch/checkout. No external AI/web/model calls. No training/DPO/checkpoint/model/locator mutation.

## Sub-agent Verification
- JSON/JSONL parse.
- Assert all approved rows have source_license_ref/license_hash/consent/provenance before any approval claim.
- Assert exactly 9 approved row assignments only after all gates pass.
- If a proof bundle is created, create it only after gates pass, then run NonInterRaterProofProducer and Quality03ProofContractRunner.
- Do not run git commands; git_authority remains forbidden for the sub-agent.

## Parent Review Verification
- Parent/reviewer may perform whitespace verification for changed artifacts outside the sub-agent work order, including `git diff --check` when parent git authority applies.
