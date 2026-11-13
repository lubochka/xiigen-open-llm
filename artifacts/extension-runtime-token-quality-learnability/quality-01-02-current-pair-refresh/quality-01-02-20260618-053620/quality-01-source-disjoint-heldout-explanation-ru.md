# QUALITY-01 source-disjoint / held-out explanation

Run: `quality-01-02-20260618-053620`

Текущий scenario pack `artifacts/extension-runtime-token-quality-learnability/scenario-pack-token-quality-v1.jsonl` содержит `source_basis` и `universalization_rule`, но не содержит строгих provenance fields: `source_ref`, `doc_id`, `license_class`, `train_split_id`, `held_out_split_id`, source hash / nearest-neighbour group и approved train-vs-eval source partition.

Поэтому текущий pack нельзя честно назвать source-disjoint held-out eval. Scoring возможен только как limited observed handler-level scoring по существующим TOKEN-03/TOKEN-04 outputs/traces. QUALITY-03 при rerun должен видеть эти ограничения и не открывать semantic/full readiness claims.

## Что можно использовать сейчас

- 12 scenario prompts, expected_outputs, positive_examples, negative_examples и scoring_rubric из scenario pack.
- 54 TOKEN-03 baseline handler-level rows.
- 54 TOKEN-04 adaptive handler-level rows.
- 54 QUALITY-03 pair refs from `quality-03-20260618-051740`.

## Чего не хватает для строгого PASS

- Независимый source ledger с source_ref/doc_id/license.
- Явный train/held-out split по source, а не только по prompt hash.
- No-leakage / no-memorization nearest-neighbour или source-overlap report.
- Ablation rerun для текущих 54 pairs.

## Вывод

Verdict: `INCONCLUSIVE_SOURCE_DISJOINT_NOT_PROVABLE_FROM_CURRENT_SCENARIO_PACK`.
