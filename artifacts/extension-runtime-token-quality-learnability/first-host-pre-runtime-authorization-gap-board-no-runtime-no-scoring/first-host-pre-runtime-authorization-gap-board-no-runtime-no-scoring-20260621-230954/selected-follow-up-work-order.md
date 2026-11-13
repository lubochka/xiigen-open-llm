# Selected Follow-Up Work Order

Work order id: $nextWorkOrderId

Artifact type: support-only runtime execution work order builder.

Selected host: Claude.

Purpose: build the actual first-host Claude runtime execution work order with all launch-gate fields ready and an internal proof-runner plan, while keeping runtime/scoring/training flags false in this support step.

Required inputs:
- Prep packet: $prepRef
- Prep review packet: $reviewRef
- Pre-runtime gap board packet: $relativePacketRoot

Required outputs:
- irst-host-runtime-execution-work-order-draft-ru.md
- irst-host-runtime-execution-work-order-draft.json
- internal-proof-runner-plan-ru.md
- internal-proof-runner-plan.json
-
untime-launch-gate-checklist.json
- state-entry.json
- checks-report.json

Required fields to prepare:
- runtime_launch_approval_quote_current
- evidence_output_folder_ref
- prompt_set_ref
- redaction_policy_ref
- cleanup_or_rollback_plan_ref
- no_training_mutation_confirmed
- no_scoring_unless_separate_approval_confirmed
- internal_proof_runner_plan_ref

Authorization flags in this selected follow-up remain false here: runtime_launch_authorized=false, scoring_authorized=false, training_authorized=false. No runtime/scoring/training occurs unless a later bounded work order explicitly carries approved flags.

Git authority: forbidden. Parent will perform commit/push after review.