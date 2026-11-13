# Next Batch Decision — translator-en-ru-general / batch-018
# Session: 2026-05-11

## Current State

- stream_id: translator-en-ru-general
- batch_index: 18
- status: PAIRSET_READY_TRAINING_RUNNER_MISSING
- structural_gate_pass_rate: 100%
- manual_review_pass_rate: 90% (1 annotation repair needed in heldout row 0036)
- source_authority: TR-S01 ACCEPT for all 50 pairs
- training_runner: MISSING

## Verdict

CONTINUE stream to batch-019.

Batch-018 is a complete, machine-usable pairset. It does not block batch-019
preparation. Batch-019 should not be declared accepted until:

1. The DPO training runner executes batch-018.
2. Dotnet test passes at or above Gate B floor (1425 passed).
3. Heldout evaluation metrics are measured (not invented).

## Batch-019 Specification

- stream_id: translator-en-ru-general
- batch_index: 19
- direction: EN_RU
- topic_domain: general
- topic_label: food_and_eating
- translation_level: 1
- adapter_id: adapter-en-ru-general
- source_id: TR-S01
- source_license_state: ACCEPT
- pair_mix: 35 T01 + 5 T08 + 5 T09 + 5 corruption

Rationale: Food and eating is a major Tatoeba EN-RU category not yet covered
in batches 001-018. It provides rich T01 material (ordering food, cooking,
dining) and named entity candidates (restaurant names, dish names requiring
transliteration).

## Repair Required Before Runner Executes

- Heldout row 0036 annotation text must be updated:
  Replace: nausea and photosensitivity
  With:    sneezing and feverish
  File: translation-heldout-50.jsonl, pair_id translator-en-ru-general-batch-018-heldout-0036

## Blocker Contract

The training runner must implement the fields listed in RUNNER-CONTRACT.md.
Do not rerun without the runner. Do not invent metrics.

NEXT ACTION:
1. Provide or implement the DPO translation runner described in RUNNER-CONTRACT.md.
2. Repair heldout-0036 annotation text (one field update).
3. Rerun the prepared 50-pair batch through the runner.
4. Run dotnet test to verify Gate B (floor: 1425 passed, 0 failed).
5. After runner completes, declare batch-018 accepted or repair.
6. Then prepare batch-019 (food_and_eating topic).
