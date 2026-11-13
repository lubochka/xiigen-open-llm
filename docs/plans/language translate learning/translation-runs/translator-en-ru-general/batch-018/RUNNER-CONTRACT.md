# Runner Contract — translator-en-ru-general / batch-018
# Session: 2026-05-11
# Purpose: defines what the training/evaluation runner must produce for this batch

## Input Files

- translation-train-50.jsonl — 50 DPO pairs for training
- translation-heldout-50.jsonl — 50 DPO pairs for held-out evaluation

## Required Input Fields Per Row

- pair_id (string)
- source_text (string)
- chosen (string)
- rejected (string)
- defect_type (one of: literal_translation, grammar_error, wrong_register,
  missing_idiom, false_friend, omission, wrong_word_choice)
- direction (EN_RU)
- adapter_id (adapter-en-ru-general)
- translation_level (1)

## Required Output Metrics

The runner must produce all of the following. Do not invent values.

### Per-Batch Training Metrics

- rows_trained (int)
- rows_held_out (int)
- elapsed_time_seconds (float)
- train_time_seconds (float)
- evaluation_time_seconds (float)
- peak_ram_mb (float)
- average_ram_mb (float)
- average_cpu_percent (float)
- peak_cpu_percent (float)
- disk_written_mb (float)
- checkpoint_size_mb (float)
- training_loss (float)
- dpo_accuracy (float, 0.0-1.0)
- dpo_reward_margin (float, positive = chosen > rejected)

### Per-Batch Translation Quality Metrics

- bleu (float) or chrf (float) — at least one required; name the primary metric
- target_script_accuracy (float) — fraction of output rows in Cyrillic
- copied_source_rate (float) — fraction of outputs that match source verbatim
- wrong_direction_rate (float) — fraction of outputs in English instead of Russian
- named_entity_preservation (float) — fraction of T09 pairs with entity rendered correctly
- completeness_score (float) — fraction of T08 pairs where chosen content is fully present

### Per-Row Held-Out Output

For each held-out row, the runner must produce:
- pair_id (string)
- source_text (string)
- reference (chosen string)
- model_output (string)
- bleu_score (float)
- chrf_score (float)
- script_correct (bool)
- direction_correct (bool)
- verdict (pass | fail)
- error_category (string or null)

## Promotion Gate

The batch may be declared accepted only when:

- dpo_reward_margin > 0
- dpo_accuracy >= 0.75 (non-regressive)
- target_script_accuracy >= 0.95 (Cyrillic coverage target)
- copied_source_rate <= 0.02
- wrong_direction_rate == 0.0
- dotnet test passes with >= 1425 tests, 0 failures (Gate B)

## Output Files Expected From Runner

- translation-batch-summary.json (update metric_mode from not_run to measured)
- translation-heldout-evaluation.json (populate all metric fields)
- LOOK-LIKE-RUN-REPORT.md (update all not_run fields)

## Notes

- Do not merge batch-019 into batch-018 runner execution.
- The batch-018 C# gate test (EnRuGeneral_Batch018_StructuralGatePasses) must pass
  before training is started; run dotnet test first.
- TOKENIZER_CHECK_PENDING: before executing training, verify Cyrillic tokenizer
  coverage >= 95% using the tokenizer coverage tool.
