# Next-Batch Decision — router-r2-topic batch-002

## Decision

PROCEED_TO_BATCH_003

## Rationale

- Batch-002 gate: topic_accuracy = 1.00 (50/50) — ACCEPTED
- Batch-001 gate: topic_accuracy = 1.00 (50/50) — ACCEPTED
- Cumulative train pairs: 100 / 150 target
- No repair required in either batch
- Remaining capacity: 50 pairs needed to complete R2 stream

## Batch-003 Plan

- pairs_to_write: 50 train + 50 heldout
- focus: stress-test edge cases — domain overlap phrases, low-confidence single-keyword, multi-language mixed signals
- direction_target: RU_EN×13 EN_RU×12 HE_EN×10 EN_HE×15
- topic_target: medical×8 legal×8 technical×8 cultural×8 personal×8 general×10
- on_accept: stream_complete = true, cumulative_train_pairs = 150
