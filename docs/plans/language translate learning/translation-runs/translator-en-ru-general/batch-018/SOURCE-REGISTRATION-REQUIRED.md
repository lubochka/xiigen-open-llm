# Source Registration Required — translator-en-ru-general / batch-018
# Written: 2026-05-11

## Blocker

The source used in all translator-en-ru-general batches (001–018) is synthetic seed,
not a registered parallel corpus. The source_id "original" used in batches 001–017
has no entry in TRANSLATION-SOURCE-REGISTRY-v3.md. It cannot be treated as ACCEPT.

This batch uses source_id: "synthetic-seed-en-ru-v1" with source_license_state: SYNTHETIC_SEED.
All 50 train pairs and 50 held-out pairs carry quality_status: source_registration_required.

## Impact

- Pairs CANNOT be promoted to train_ready until the source is registered as ACCEPT
  or replaced with a registered ACCEPT corpus extract (e.g., Tatoeba TR-S01).
- Chosen sides cannot be treated as parallel corpus authority.
- The pairset is a prepared candidate, not a train-ready batch.

## Required Registration Fields for Tatoeba (TR-S01) Replacement

To replace synthetic seed with a registered source:

| Field | Required value |
|---|---|
| source_id | tatoeba-en-ru |
| registry_entry | TR-S01 |
| source_license_state | ACCEPT |
| license | CC-BY 2.0 FR |
| attribution_required | yes — include sentence id and contributor when available |
| corpus_version | specify Tatoeba release date or snapshot id |
| extraction_date | date the sentences were extracted |
| source_locator | tatoeba sentence id per row |
| chosen_side_authority | yes — human-contributed parallel sentences |

## Required Steps Before Training

1. Extract 100 EN-RU sentence pairs from Tatoeba (TR-S01) for the science/technology
   thematic focus within the general topic domain.
2. Apply Layer 0-A qualification: license=ACCEPT, no mojibake, correct scripts,
   4–20 source tokens per EXAMINATION-METHOD-TRANSLATION-v1.md Phase 1 limits.
3. Replace synthetic source rows in translation-train-50.jsonl and
   translation-heldout-50.jsonl with Tatoeba rows, updating source_id, source_locator,
   and source_license_state fields.
4. Update quality_status from source_registration_required to train_ready.
5. Re-run structural gate before submitting to training runner.

## Affected Files

- source-inventory.jsonl (all 100 rows)
- translation-train-50.jsonl (all 50 rows)
- translation-heldout-50.jsonl (all 50 rows)
- translation-batch-summary.json (source_registration_required flag)

## Prior Batches (001–017)

All prior EN_RU batches (001–017) carry the same source provenance issue.
The NEXT-BATCH-DECISION files for batches 001–017 incorrectly recorded
gate_result: ACCEPTED. Correct status for all is: PAIRSET_PREPARED_TRAINING_PENDING
with source_license_state: SYNTHETIC_SEED and SOURCE_REGISTRATION_REQUIRED.
