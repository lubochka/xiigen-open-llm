# P5C parent review

Вердикт: `PASS_WITH_REPAIR_NOT_TRAINING_READY`.

P5C принят как DPO promotion review. Он не создает training set, не запускает eval, не пишет checkpoint/model и не заявляет semantic quality.

## Что проверено

- `promotion-review-ledger.jsonl`: 10 строк.
- Verdict counts:
  - `DPO_READY_CANDIDATE_FOR_NEXT_AUTHORING`: 3;
  - `EVIDENCE_ONLY_NEEDS_SOURCE_ACCEPTANCE`: 2;
  - `MERGE_OR_DROP_SAME_SOURCE_VARIANT`: 2;
  - `QUARANTINE_SHORTCUT_RISK`: 3;
  - `REJECT`: 0.
- `next-dpo-authoring-input.json`: 3 строки.
- `training_ready=true`: 0.
- `training_allowed=false`, `eval_allowed=false`, `semantic_quality_claimed=false`.
- Markdown artifacts readable UTF-8, no mojibake/replacement symbols.

## Что принято

P5C правильно выбрал только три строки как input для следующего DPO authoring:

- `P3-PTT-DRAFT-001` для PromptToTask;
- `P4-ORCH-DPO-DRAFT-001` для OrchestrationReasoning;
- `P5A-CAND-PTT-AUTHORITY-RULES-001` для PromptToTask.

Это DPO authoring input, не training data.

## Что остается слабым

`OrchestrationReasoning` имеет только один safe candidate for next authoring. Остальные orchestration candidates либо named-reference/todo, либо code/test leakage risk, либо near-duplicate concern. Поэтому P6/P8 real training все еще нельзя запускать.

Следующий safe step должен быть одним из двух:

1. `P5D` source-acceptance/anonymized-rewrite repair для evidence-only и quarantined orchestration rows; или
2. узкий DPO authoring только для трех safe candidates, с явным результатом `not enough for full source-disjoint training`.

Предпочтение parent review: сначала `P5D`, чтобы не писать слишком маленький и заведомо неполный DPO batch.
