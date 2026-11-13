# Train/held-out partition ledger

Run: `quality-controls-source-license-partition-20260618-073408`

Итог: concrete decision для каждой строки — `quarantine_license_unclear`. Ни одна строка не назначена в train или held-out, потому что license/source approval не доказан. Это осознанное закрытие дырки, а не PASS.

## Счетчики

- `scenario_count`: 12
- `pair_row_count`: 54
- `scenarios_quarantined`: 12
- `pair_rows_quarantined`: 54
- `scenarios_with_train_heldout_partition_proven`: 0
- `pair_rows_with_train_heldout_partition_proven`: 0

## Основные группы source overlap

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`: 10 scenarios / 48 pair rows.
- `C:/Users/lubao/Documents/xiigen/llm mvp/skills loading as an ai extension/skills-inventory-universal-vs-project-specific.md`: 1 scenarios / 3 pair rows.
- `C:\Users\lubao\Documents\xiigen\extensions  - orchestration based on our data\problem with authority.txt`: 1 scenarios / 3 pair rows.

Strict split будет возможен только после license approval и назначения train/held-out без общего `source_ref`, `doc_id`, `source_disjoint_group`, prompt hash, path/title и nearest-neighbour source.
