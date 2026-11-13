# Score output schema examples

Run: `quality-02-scorer-eval-only-runner-prep-20260618-095502`

`score-output-schema.jsonl` contains 12 scenario-level examples. They are schema examples only, not scoring results.

## Coverage

| Route | Scenario examples | Pair rows represented | Runnable now | Runnable later |
|---|---:|---:|---|---|
| `USER_ADAPTIVE_EVAL_ONLY` | 7 | 21 | no | only after exact approval and gates |
| `EVIDENCE_ONLY` | 5 | 33 | no | no, control/no-overclaim only |
| `COMMON_BLOCKED` | 12 | 54 | no | no, until permissive/common source gate exists |

## User-adaptive examples

| Scenario | Pair rows | Score status |
|---|---:|---|
| `TQ-001-prompt-task-documentation` | 3 | `NOT_SCORED_DRY_RUN_ONLY` |
| `TQ-002-prompt-task-interrupt` | 3 | `NOT_SCORED_DRY_RUN_ONLY` |
| `TQ-003-prompt-task-redaction` | 3 | `NOT_SCORED_DRY_RUN_ONLY` |
| `TQ-006-orchestration-learning-lifecycle` | 3 | `NOT_SCORED_DRY_RUN_ONLY` |
| `TQ-007-skill-architecture-audit` | 3 | `NOT_SCORED_DRY_RUN_ONLY` |
| `TQ-008-skill-debug-runtime` | 3 | `NOT_SCORED_DRY_RUN_ONLY` |
| `TQ-009-skill-learning-safety` | 3 | `NOT_SCORED_DRY_RUN_ONLY` |

## Evidence-only blocked examples

| Scenario | Pair rows | Score status |
|---|---:|---|
| `TQ-004-orchestration-runtime-proof` | 3 | `NON_RUNNABLE_EVIDENCE_ONLY` |
| `TQ-005-orchestration-token-pairing` | 3 | `NON_RUNNABLE_EVIDENCE_ONLY` |
| `TQ-010-mixed-no-overclaim` | 9 | `NON_RUNNABLE_EVIDENCE_ONLY` |
| `TQ-011-mixed-shortcut-audit` | 9 | `NON_RUNNABLE_EVIDENCE_ONLY` |
| `TQ-012-mixed-final-readiness` | 9 | `NON_RUNNABLE_EVIDENCE_ONLY` |

## Required layer slots for future scored rows

- `prompt_to_task`
- `orchestration_reasoning`
- `skill_selection`
- `final_usefulness`

For this dry-run prep, all user-adaptive layer slots are `NOT_SCORED` and all evidence-only rows are non-runnable. No PASS, no quality delta, no semantic claim.
