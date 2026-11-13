# Redacted Source License Packet Template

Статус: `TEMPLATE_UNFILLED_NOT_APPROVED`.

Этот пакет ничего не разблокирует сам по себе. Его надо заполнить точными локальными `source_ref`, `doc_id`, `source_sha256`, `source_license_ref`, `source_license_hash`, redaction status и approval/ref hash.

## Common Route Rule

`common_training_allowed=true` допустим только для универсального материала с доказанной MIT, Apache-2.0 или другой reviewed permissive/open лицензией. Private/project/stack-specific rows не становятся common training через этот template.

## Rows To Fill

| Scenario | Default non-common route | Source | Doc id |
|---|---|---|---|
| `TQ-001-prompt-task-documentation` | `USER_ADAPTIVE_ONLY` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#documentation-no-overclaim-evidence-boundary` |
| `TQ-002-prompt-task-interrupt` | `USER_ADAPTIVE_ONLY` | `C:\Users\lubao\Documents\xiigen\extensions  - orchestration based on our data\problem with authority.txt` | `problem-with-authority.txt#human-interrupt-hard-pause-lines-43279-43328` |
| `TQ-003-prompt-task-redaction` | `USER_ADAPTIVE_ONLY` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#evidence-separation-redaction` |
| `TQ-004-orchestration-runtime-proof` | `EVIDENCE_ONLY` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#runtime-proof-depth-gate` |
| `TQ-005-orchestration-token-pairing` | `EVIDENCE_ONLY` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#token-before-after-pairing` |
| `TQ-006-orchestration-learning-lifecycle` | `USER_ADAPTIVE_ONLY` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#learnability-dpo-lifecycle` |
| `TQ-007-skill-architecture-audit` | `USER_ADAPTIVE_ONLY` | `C:/Users/lubao/Documents/xiigen/llm mvp/skills loading as an ai extension/skills-inventory-universal-vs-project-specific.md` | `skills-inventory-universal-vs-project-specific.md#universal-core-vs-project-adapter-skill-registry` |
| `TQ-008-skill-debug-runtime` | `USER_ADAPTIVE_ONLY` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#runtime-debug-visibility` |
| `TQ-009-skill-learning-safety` | `USER_ADAPTIVE_ONLY` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#dpo-source-quality-and-rollback-safety` |
| `TQ-010-mixed-no-overclaim` | `EVIDENCE_ONLY` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#no-overclaim-evidence-separation` |
| `TQ-011-mixed-shortcut-audit` | `EVIDENCE_ONLY` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#deterministic-shortcut-no-overclaim` |
| `TQ-012-mixed-final-readiness` | `EVIDENCE_ONLY` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#final-readiness-matrix` |
