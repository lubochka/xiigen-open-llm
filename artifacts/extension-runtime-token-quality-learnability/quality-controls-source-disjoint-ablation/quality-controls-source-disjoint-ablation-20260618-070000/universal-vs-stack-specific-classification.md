# Universal vs Stack-Specific Classification

- Run: `quality-controls-source-disjoint-ablation-20260618-070000`
- Common quality controls are universal task/orchestration/skill semantics.
- Claude/Cursor/Codex labels are matrix coverage labels only; they are not source/license authority and not host implementation mechanics.

| Scenario | Common class | Host labels | Status |
|---|---|---|---|
| `TQ-001-prompt-task-documentation` | `universal_task_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |
| `TQ-002-prompt-task-interrupt` | `universal_task_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |
| `TQ-003-prompt-task-redaction` | `universal_task_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |
| `TQ-004-orchestration-runtime-proof` | `universal_orchestration_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |
| `TQ-005-orchestration-token-pairing` | `universal_orchestration_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |
| `TQ-006-orchestration-learning-lifecycle` | `universal_orchestration_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |
| `TQ-007-skill-architecture-audit` | `universal_skill_selection_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |
| `TQ-008-skill-debug-runtime` | `universal_skill_selection_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |
| `TQ-009-skill-learning-safety` | `universal_skill_selection_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |
| `TQ-010-mixed-no-overclaim` | `universal_mixed_task_orchestration_skill_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |
| `TQ-011-mixed-shortcut-audit` | `universal_mixed_task_orchestration_skill_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |
| `TQ-012-mixed-final-readiness` | `universal_mixed_task_orchestration_skill_semantics` | Claude, Codex, Cursor | `UNIVERSAL_SEMANTICS_SEPARATED_STACK_LABELS_NOT_STRICT_SOURCE_METADATA` |

## Rule

Rows may support eval semantics only. They cannot support common training, semantic quality, token savings, live-host savings or final readiness until approved source/license/partition metadata and controls exist.
