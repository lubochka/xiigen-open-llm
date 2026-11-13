# Eval prompt pack: QUALITY-01-EVAL-PROMPT-PACK-AND-RUBRIC-PREP

Run: `quality-01-eval-prompt-pack-rubric-20260618-093007`

Этот pack только подготавливает prompts/routes. Eval runner не запускался, scoring не выполнялся, качество не заявлено.

| Route | Scenarios | Pair rows | Use now |
|---|---:|---:|---|
| USER_ADAPTIVE_EVAL_ONLY | 7 | 21 | Prepare only for later local/private eval-only runner after approval; no execution now. |
| EVIDENCE_ONLY | 5 | 33 | Reference/control context only; excluded from eval/training inputs now. |
| COMMON_BLOCKED | 12 | 54 | Blocked until permissive/common source/license exists. |

## Scenario routes

| Scenario | Route | Common strict | Pairs | Source doc |
|---|---|---|---:|---|
| `TQ-001-prompt-task-documentation` | `USER_ADAPTIVE_EVAL_ONLY` | `COMMON_BLOCKED` | 3 | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#documentation-no-overclaim-evidence-boundary` |
| `TQ-002-prompt-task-interrupt` | `USER_ADAPTIVE_EVAL_ONLY` | `COMMON_BLOCKED` | 3 | `problem-with-authority.txt#human-interrupt-hard-pause-lines-43279-43328` |
| `TQ-003-prompt-task-redaction` | `USER_ADAPTIVE_EVAL_ONLY` | `COMMON_BLOCKED` | 3 | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#evidence-separation-redaction` |
| `TQ-004-orchestration-runtime-proof` | `EVIDENCE_ONLY` | `COMMON_BLOCKED` | 3 | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#runtime-proof-depth-gate` |
| `TQ-005-orchestration-token-pairing` | `EVIDENCE_ONLY` | `COMMON_BLOCKED` | 3 | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#token-before-after-pairing` |
| `TQ-006-orchestration-learning-lifecycle` | `USER_ADAPTIVE_EVAL_ONLY` | `COMMON_BLOCKED` | 3 | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#learnability-dpo-lifecycle` |
| `TQ-007-skill-architecture-audit` | `USER_ADAPTIVE_EVAL_ONLY` | `COMMON_BLOCKED` | 3 | `skills-inventory-universal-vs-project-specific.md#universal-core-vs-project-adapter-skill-registry` |
| `TQ-008-skill-debug-runtime` | `USER_ADAPTIVE_EVAL_ONLY` | `COMMON_BLOCKED` | 3 | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#runtime-debug-visibility` |
| `TQ-009-skill-learning-safety` | `USER_ADAPTIVE_EVAL_ONLY` | `COMMON_BLOCKED` | 3 | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#dpo-source-quality-and-rollback-safety` |
| `TQ-010-mixed-no-overclaim` | `EVIDENCE_ONLY` | `COMMON_BLOCKED` | 9 | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#no-overclaim-evidence-separation` |
| `TQ-011-mixed-shortcut-audit` | `EVIDENCE_ONLY` | `COMMON_BLOCKED` | 9 | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#deterministic-shortcut-no-overclaim` |
| `TQ-012-mixed-final-readiness` | `EVIDENCE_ONLY` | `COMMON_BLOCKED` | 9 | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#final-readiness-matrix` |

## Machine targets

| Layer | Machine target | Minimum later PASS | Forbidden shortcuts |
|---|---|---|---|
| prompt -> task | `PromptToTaskMachine` | score >= 4 and no critical failure | exact prompt replay, old state next_action as approval, private path/source id as semantic authority |
| task -> plan | `OrchestrationReasoningMachine` | score >= 4 and phase order preserved | dashboard/API/helper as host runtime proof, quality before pairing/source gates, common strict route without license packet |
| task/plan -> skills | `SkillSelectionMachine` | score >= 4 and no leakage signal | candidate order, filename/path/title/source-id signal, host package detail as semantic selector |
| final usefulness | `FinalOutputUsefulness` | score >= 4 and no overclaim | PASS label without evidence refs, averaging away failed layer, token delta as quality proof |

## Common strict route

Все 12 scenarios и 54 pair rows имеют отдельный `COMMON_BLOCKED` route. Причина: `COMMON_ALLOWED` = 0/12 и 0/54; accepted permissive/common source packet = 0.
