# Source metadata fill manifest

Run: quality-controls-source-metadata-fill-20260618-065930

Strict QUALITY-03 controls can run now: **no**. Source/doc candidates were partially filled where local proof was available, but license and train/held-out partition are still not proven.

## Counts

| Metric | Count |
|---|---:|
| scenario_count | 12 |
| pair_row_count | 54 |
| scenarios_with_source_doc_candidate | 11 |
| scenarios_missing_source_doc_candidate | 1 |
| scenarios_with_license_proven | 0 |
| scenarios_missing_license | 12 |
| scenarios_with_train_heldout_partition_proven | 0 |
| scenarios_missing_train_heldout_partition | 12 |
| scenarios_full_strict_metadata_resolved | 0 |
| scenarios_full_strict_metadata_missing | 12 |
| pair_rows_with_source_doc_candidate | 51 |
| pair_rows_missing_source_doc_candidate | 3 |
| pair_rows_with_license_proven | 0 |
| pair_rows_missing_license | 54 |
| pair_rows_with_train_heldout_partition_proven | 0 |
| pair_rows_missing_train_heldout_partition | 54 |
| pair_rows_full_strict_metadata_resolved | 0 |
| pair_rows_blocked_for_strict_controls | 54 |
| controls_runnable_now | false |

## Scenarios

| Scenario | Source ref | Doc id | License | Train/held-out | Status | Confidence |
|---|---|---|---|---|---|---|
| TQ-001-prompt-task-documentation | docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md | EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#documentation-no-overclaim-evidence-boundary | UNKNOWN | quarantine_license_unclear | PARTIAL_SOURCE_DOC_MATCH_LICENSE_AND_PARTITION_MISSING | MEDIUM_PROVABLE_THEMATIC_SOURCE_DOC_MATCH_LICENSE_PARTITION_MISSING |
| TQ-002-prompt-task-interrupt | UNKNOWN/MISSING_SOURCE_REF | UNKNOWN/MISSING_DOC_ID | UNKNOWN | quarantine_missing_metadata | MISSING_REQUIRED_SOURCE_REF_DOC_ID_LICENSE_PARTITION | LOW_APPROVED_SOURCE_CONTENT_NOT_FOUND_FOR_HUMAN_INTERRUPT_RULE |
| TQ-003-prompt-task-redaction | docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md | EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#evidence-separation-redaction | UNKNOWN | quarantine_license_unclear | PARTIAL_SOURCE_DOC_MATCH_LICENSE_AND_PARTITION_MISSING | MEDIUM_PROVABLE_THEMATIC_SOURCE_DOC_MATCH_LICENSE_PARTITION_MISSING |
| TQ-004-orchestration-runtime-proof | docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md | EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#runtime-proof-depth-gate | UNKNOWN | quarantine_license_unclear | PARTIAL_SOURCE_DOC_MATCH_LICENSE_AND_PARTITION_MISSING | MEDIUM_PROVABLE_THEMATIC_SOURCE_DOC_MATCH_LICENSE_PARTITION_MISSING |
| TQ-005-orchestration-token-pairing | docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md | EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#token-before-after-pairing | UNKNOWN | quarantine_license_unclear | PARTIAL_SOURCE_DOC_MATCH_LICENSE_AND_PARTITION_MISSING | MEDIUM_PROVABLE_THEMATIC_SOURCE_DOC_MATCH_LICENSE_PARTITION_MISSING |
| TQ-006-orchestration-learning-lifecycle | docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md | EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#learnability-dpo-lifecycle | UNKNOWN | quarantine_license_unclear | PARTIAL_SOURCE_DOC_MATCH_LICENSE_AND_PARTITION_MISSING | MEDIUM_PROVABLE_THEMATIC_SOURCE_DOC_MATCH_LICENSE_PARTITION_MISSING |
| TQ-007-skill-architecture-audit | C:/Users/lubao/Documents/xiigen/llm mvp/skills loading as an ai extension/skills-inventory-universal-vs-project-specific.md | skills-inventory-universal-vs-project-specific.md#universal-core-vs-project-adapter-skill-registry | UNKNOWN | quarantine_license_unclear | PARTIAL_SOURCE_DOC_MATCH_LICENSE_AND_PARTITION_MISSING | MEDIUM_PROVABLE_THEMATIC_SOURCE_DOC_MATCH_LICENSE_PARTITION_MISSING |
| TQ-008-skill-debug-runtime | docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md | EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#runtime-debug-visibility | UNKNOWN | quarantine_license_unclear | PARTIAL_SOURCE_DOC_MATCH_LICENSE_AND_PARTITION_MISSING | MEDIUM_PROVABLE_THEMATIC_SOURCE_DOC_MATCH_LICENSE_PARTITION_MISSING |
| TQ-009-skill-learning-safety | docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md | EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#dpo-source-quality-and-rollback-safety | UNKNOWN | quarantine_license_unclear | PARTIAL_SOURCE_DOC_MATCH_LICENSE_AND_PARTITION_MISSING | MEDIUM_PROVABLE_THEMATIC_SOURCE_DOC_MATCH_LICENSE_PARTITION_MISSING |
| TQ-010-mixed-no-overclaim | docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md | EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#no-overclaim-evidence-separation | UNKNOWN | quarantine_license_unclear | PARTIAL_SOURCE_DOC_MATCH_LICENSE_AND_PARTITION_MISSING | MEDIUM_PROVABLE_THEMATIC_SOURCE_DOC_MATCH_LICENSE_PARTITION_MISSING |
| TQ-011-mixed-shortcut-audit | docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md | EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#deterministic-shortcut-no-overclaim | UNKNOWN | quarantine_license_unclear | PARTIAL_SOURCE_DOC_MATCH_LICENSE_AND_PARTITION_MISSING | MEDIUM_PROVABLE_THEMATIC_SOURCE_DOC_MATCH_LICENSE_PARTITION_MISSING |
| TQ-012-mixed-final-readiness | docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md | EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#final-readiness-matrix | UNKNOWN | quarantine_license_unclear | PARTIAL_SOURCE_DOC_MATCH_LICENSE_AND_PARTITION_MISSING | MEDIUM_PROVABLE_THEMATIC_SOURCE_DOC_MATCH_LICENSE_PARTITION_MISSING |

## Pair Row Coverage

54/54 pair rows are present in the JSON/JSONL manifest. 0/54 are ready for strict controls because license and partition proof are missing for every row.
