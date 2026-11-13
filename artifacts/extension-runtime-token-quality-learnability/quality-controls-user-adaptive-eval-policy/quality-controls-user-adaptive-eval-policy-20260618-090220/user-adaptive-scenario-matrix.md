# User-adaptive scenario matrix

Run: `quality-controls-user-adaptive-eval-policy-20260618-090220`

Покрытие: **7/12 scenarios** и **21/54 pair rows**. Все строки имеют route `USER_ADAPTIVE_ONLY_EVAL_ONLY_LATER`.

Privacy route: raw/private inputs остаются только в approved user-local root. В repo допустимы только redacted, non-training, evidence-only refs.

Expected evidence для любой будущей eval-only строки:

- current explicit user-local eval-only approval;
- user-local root ref;
- raw private input ref;
- redacted shared evidence ref;
- privacy/redaction status;
- no-common-mutation attestation;
- no-training-mutation attestation;
- no model/checkpoint/locator mutation attestation;
- evidence scope label: `local_private_eval_only`.

Non-common limits: no common training, no common checkpoint/set mutation, no target promotion, no locator update, no token savings, no quality improvement, no semantic readiness, no learnability, no strict `QUALITY-03` claim.

## Scenario rows

| Row | Scenario | Source/doc |
|---:|---|---|
| 1 | `TQ-001-prompt-task-documentation` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#documentation-no-overclaim-evidence-boundary` |
| 2 | `TQ-002-prompt-task-interrupt` | `problem-with-authority.txt#human-interrupt-hard-pause-lines-43279-43328` |
| 3 | `TQ-003-prompt-task-redaction` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#evidence-separation-redaction` |
| 6 | `TQ-006-orchestration-learning-lifecycle` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#learnability-dpo-lifecycle` |
| 7 | `TQ-007-skill-architecture-audit` | `skills-inventory-universal-vs-project-specific.md#universal-core-vs-project-adapter-skill-registry` |
| 8 | `TQ-008-skill-debug-runtime` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#runtime-debug-visibility` |
| 9 | `TQ-009-skill-learning-safety` | `EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md#dpo-source-quality-and-rollback-safety` |

## Pair rows

| Row | Pair id |
|---:|---|
| 13 | `TQ-001-prompt-task-documentation\|Claude\|prompt-to-task\|debug-false` |
| 14 | `TQ-001-prompt-task-documentation\|Codex\|prompt-to-task\|debug-false` |
| 15 | `TQ-001-prompt-task-documentation\|Cursor\|prompt-to-task\|debug-false` |
| 16 | `TQ-002-prompt-task-interrupt\|Claude\|prompt-to-task\|debug-false` |
| 17 | `TQ-002-prompt-task-interrupt\|Codex\|prompt-to-task\|debug-false` |
| 18 | `TQ-002-prompt-task-interrupt\|Cursor\|prompt-to-task\|debug-false` |
| 19 | `TQ-003-prompt-task-redaction\|Claude\|prompt-to-task\|debug-false` |
| 20 | `TQ-003-prompt-task-redaction\|Codex\|prompt-to-task\|debug-false` |
| 21 | `TQ-003-prompt-task-redaction\|Cursor\|prompt-to-task\|debug-false` |
| 28 | `TQ-006-orchestration-learning-lifecycle\|Claude\|orchestration-plan\|debug-false` |
| 29 | `TQ-006-orchestration-learning-lifecycle\|Codex\|orchestration-plan\|debug-false` |
| 30 | `TQ-006-orchestration-learning-lifecycle\|Cursor\|orchestration-plan\|debug-false` |
| 31 | `TQ-007-skill-architecture-audit\|Claude\|skill-selection\|debug-false` |
| 32 | `TQ-007-skill-architecture-audit\|Codex\|skill-selection\|debug-false` |
| 33 | `TQ-007-skill-architecture-audit\|Cursor\|skill-selection\|debug-false` |
| 34 | `TQ-008-skill-debug-runtime\|Claude\|skill-selection\|debug-false` |
| 35 | `TQ-008-skill-debug-runtime\|Codex\|skill-selection\|debug-false` |
| 36 | `TQ-008-skill-debug-runtime\|Cursor\|skill-selection\|debug-false` |
| 37 | `TQ-009-skill-learning-safety\|Claude\|skill-selection\|debug-false` |
| 38 | `TQ-009-skill-learning-safety\|Codex\|skill-selection\|debug-false` |
| 39 | `TQ-009-skill-learning-safety\|Cursor\|skill-selection\|debug-false` |
