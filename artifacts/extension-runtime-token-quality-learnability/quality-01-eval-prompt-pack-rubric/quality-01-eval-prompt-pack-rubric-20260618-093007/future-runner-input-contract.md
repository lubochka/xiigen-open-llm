# Future runner input contract

Run: `quality-01-eval-prompt-pack-rubric-20260618-093007`

`runner_may_start_now=false`. Это только input contract для будущего eval-only runner; execution не выполнялся.

## Allowed/excluded routes

- Allowed later: `USER_ADAPTIVE_EVAL_ONLY` only after exact current approval and gates.
- Excluded now: `EVIDENCE_ONLY` and `COMMON_BLOCKED`.

## Required input fields

| Field | Required | Note |
|---|---|---|
| `scenario_id` | True | ['TQ-001-prompt-task-documentation', 'TQ-002-prompt-task-interrupt', 'TQ-003-prompt-task-redaction', 'TQ-006-orchestration-learning-lifecycle', 'TQ-007-skill-architecture-audit', 'TQ-008-skill-debug-runtime', 'TQ-009-skill-learning-safety'] |
| `pair_id` | True | eval-prompt-pack.json#/route_counts/USER_ADAPTIVE_EVAL_ONLY |
| `route` | True | USER_ADAPTIVE_EVAL_ONLY |
| `approval_ref` | True |  |
| `user_local_root_ref` | True |  |
| `raw_private_input_ref` | True | approved user-local/private root only |
| `redacted_shared_evidence_ref` | True | repo artifact ok if redacted |
| `source_ref` | True |  |
| `doc_id` | True |  |
| `privacy_redaction_status` | True | ['PASS_REDACTED', 'PRIVATE_ONLY_NO_SHARED_TEXT'] |
| `baseline_output_ref` | True |  |
| `adaptive_output_ref` | True |  |
| `host_name` | True | ['Claude', 'Cursor', 'Codex'] |
| `method` | True | ['prompt-to-task', 'orchestration-plan', 'skill-selection'] |
| `no_common_mutation_attestation` | True | True |
| `no_training_mutation_attestation` | True | True |
| `no_model_checkpoint_locator_mutation_attestation` | True | True |

## Gates before any later runner

- `G1_CURRENT_APPROVAL`: exact user-local eval-only approval quote, scenario ids and pair ids, approved user-local root, external AI/web allowed=false unless separately authorized, training/model/checkpoint/locator mutation allowed=false
- `G2_PRIVACY_REDACTION`: raw_private_input_ref stored outside common repo training, redacted_shared_evidence_ref, privacy_class, redaction_status, no private text copied into common datasets
- `G3_SOURCE_SEPARATION`: source_ref, doc_id, route=USER_ADAPTIVE_ONLY_EVAL_ONLY_LATER, common_training_allowed=false, source rows not mixed with common train/held-out rows
- `G4_NO_COMMON_MUTATION_LOCK`: common sets unchanged, common checkpoints unchanged, common manifests unchanged, repo fixtures unchanged except redacted evidence artifacts
- `G5_NO_TRAINING_MUTATION_LOCK`: training_started=false, continue_training_started=false, DPO training started=false, model/checkpoint/target/locator mutated=false
- `G6_EVIDENCE_LABELS`: evidence_scope=local_private_eval_only, what_this_proves narrow block, what_this_does_not_prove block, no semantic quality, quality improvement, token savings, learnability, live-host readiness, common readiness, strict QUALITY-03, target promotion, or locator update
- `G7_EVIDENCE_ONLY_EXCLUSION`: 5 EVIDENCE_ONLY scenarios excluded from eval/training inputs, 33 EVIDENCE_ONLY pair rows excluded from eval/training inputs, evidence-only rows may be referenced only as control/no-overclaim context
- `G8_ROLLBACK_SAFETY_IF_TRAINING_LATER_AUTHORIZED`: training remains inactive in eval-only run, if a later separate training approval exists, require base checkpoint sha, target checkpoint sha, rollback checkpoint sha, backup/export/import refs, fresh-load refs, held-out/ablation refs, and locator mutation approval before any promotion, eval-only evidence cannot be reinterpreted as rollback or promotion proof
