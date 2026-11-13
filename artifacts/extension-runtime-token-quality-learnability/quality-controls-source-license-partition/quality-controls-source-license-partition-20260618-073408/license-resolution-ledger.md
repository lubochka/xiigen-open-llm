# License/privacy resolution ledger

Run: `quality-controls-source-license-partition-20260618-073408`

Итог: source/doc candidate есть для 12/12 scenarios и 54/54 pair rows, но `license_class` остается `UNKNOWN` для всех строк. Лицензия MIT/Apache/GPL не выводилась из названия файла или контекста.

## Счетчики

- `scenario_count`: 12
- `pair_row_count`: 54
- `scenarios_with_source_doc_candidate`: 12
- `pair_rows_with_source_doc_candidate`: 54
- `scenarios_with_license_proven`: 0
- `pair_rows_with_license_proven`: 0
- `scenarios_missing_license`: 12
- `pair_rows_missing_license`: 54
- `scenarios_with_privacy_class_resolved`: 12
- `pair_rows_with_privacy_class_resolved`: 54

## Сценарии

- `TQ-001-prompt-task-documentation`: source=`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, doc=`EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#documentation-no-overclaim-evidence-boundary`, license=`UNKNOWN`, privacy=`USER_LOCAL_OR_PRIVATE_REFERENCE_PRESENT`, route=`common_training_allowed:false`.
- `TQ-002-prompt-task-interrupt`: source=`C:\Users\lubao\Documents\xiigen\extensions  - orchestration based on our data\problem with authority.txt`, doc=`problem-with-authority.txt#human-interrupt-hard-pause-lines-43279-43328`, license=`UNKNOWN`, privacy=`USER_LOCAL_OR_PRIVATE_REFERENCE_PRESENT`, route=`common_training_allowed:false`.
- `TQ-003-prompt-task-redaction`: source=`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, doc=`EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#evidence-separation-redaction`, license=`UNKNOWN`, privacy=`USER_LOCAL_OR_PRIVATE_REFERENCE_PRESENT`, route=`common_training_allowed:false`.
- `TQ-004-orchestration-runtime-proof`: source=`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, doc=`EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#runtime-proof-depth-gate`, license=`UNKNOWN`, privacy=`PROJECT_LOCAL_REFERENCE_LICENSE_UNKNOWN`, route=`common_training_allowed:false`.
- `TQ-005-orchestration-token-pairing`: source=`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, doc=`EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#token-before-after-pairing`, license=`UNKNOWN`, privacy=`PROJECT_LOCAL_REFERENCE_LICENSE_UNKNOWN`, route=`common_training_allowed:false`.
- `TQ-006-orchestration-learning-lifecycle`: source=`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, doc=`EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#learnability-dpo-lifecycle`, license=`UNKNOWN`, privacy=`USER_LOCAL_OR_PRIVATE_REFERENCE_PRESENT`, route=`common_training_allowed:false`.
- `TQ-007-skill-architecture-audit`: source=`C:/Users/lubao/Documents/xiigen/llm mvp/skills loading as an ai extension/skills-inventory-universal-vs-project-specific.md`, doc=`skills-inventory-universal-vs-project-specific.md#universal-core-vs-project-adapter-skill-registry`, license=`UNKNOWN`, privacy=`USER_LOCAL_OR_PRIVATE_REFERENCE_PRESENT`, route=`common_training_allowed:false`.
- `TQ-008-skill-debug-runtime`: source=`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, doc=`EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#runtime-debug-visibility`, license=`UNKNOWN`, privacy=`USER_LOCAL_OR_PRIVATE_REFERENCE_PRESENT`, route=`common_training_allowed:false`.
- `TQ-009-skill-learning-safety`: source=`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, doc=`EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#dpo-source-quality-and-rollback-safety`, license=`UNKNOWN`, privacy=`USER_LOCAL_OR_PRIVATE_REFERENCE_PRESENT`, route=`common_training_allowed:false`.
- `TQ-010-mixed-no-overclaim`: source=`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, doc=`EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#no-overclaim-evidence-separation`, license=`UNKNOWN`, privacy=`PROJECT_LOCAL_REFERENCE_LICENSE_UNKNOWN`, route=`common_training_allowed:false`.
- `TQ-011-mixed-shortcut-audit`: source=`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, doc=`EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#deterministic-shortcut-no-overclaim`, license=`UNKNOWN`, privacy=`PROJECT_LOCAL_REFERENCE_LICENSE_UNKNOWN`, route=`common_training_allowed:false`.
- `TQ-012-mixed-final-readiness`: source=`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, doc=`EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#final-readiness-matrix`, license=`UNKNOWN`, privacy=`PROJECT_LOCAL_REFERENCE_LICENSE_UNKNOWN`, route=`common_training_allowed:false`.

## Что нужно для снятия quarantine

Для каждого `source_ref/doc_id` нужен точный `source_license_ref` или SPDX/license metadata с hash/ref; для common training дополнительно нужен permissive/open verdict, redaction/provenance approval и source-disjoint split.
