# Next Batch Decision
# Stream: router-r1-direction
# After: batch-002
# Date: 2026-05-11

## Decision

stream_status: continue_same_level_then_advance

next_allowed_batch: router-r1-direction/batch-003 (harder edge cases), then router-r2-topic/batch-001

## Reason

Batch-002 produced 50 harder R1 routing pairs covering: single-word utterances,
Latin brand names and URLs embedded in Cyrillic text, Latin place names embedded
in Hebrew text, Latin-script transliterations of Hebrew and Russian words used in
English, and English sentences that mention the target language name as an English
adjective. All 50 held-out rows pass the direction_accuracy gate (50/50 = 1.00).

## Batch-003 Shape

- Same stream: router-r1-direction
- Router level: R1
- Focus: edge cases that approach the R1 boundary
  - Code-switched sentences mixing two scripts (mixed-direction ambiguity)
  - Sentences where only punctuation separates script regions
  - Minimal-evidence cases (single letter or punctuation only)
  - Russian or Hebrew sentences embedded within English prose (quotation context)

## After batch-003: advance to R2

Once batch-003 confirms the gate, the R1 direction stream has covered the known
difficulty range. Advance to router-r2-topic/batch-001 for topic domain detection.

## Do Not Proceed To

- Router R2 topic domain until batch-003 confirms direction_accuracy >= 0.99.
- Translator adapter training until routing flow R1 + R2 gates pass.
- EN_HE or HE_EN translator batches until the tokenizer Hebrew coverage test passes.
