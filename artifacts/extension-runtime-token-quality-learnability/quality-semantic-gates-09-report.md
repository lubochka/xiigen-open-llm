# QUALITY-SEMANTIC-GATES-09

QSG-09 preserves baseline/common PromptToTask runtime repair and reports the OrchestrationReasoning source-admission provenance rebuild blocker. It does not claim semantic quality, common readiness, token savings, target promotion, or final extension readiness.

## Gate Summary

| Component | Baseline/common runtime | Fresh load | Source-disjoint eval | Ablation | Leakage | Continue-training | Semantic quality |
| --- | --- | --- | --- | --- | --- | --- | --- |
| prompt_to_task | PASS | PASS | PASS | PASS | PASS | PASS | BLOCKED |
| orchestration_reasoning | BLOCKED | PASS | PASS | PASS | PASS | PASS | BLOCKED |
| skill_selection | PASS | PASS | PASS | PASS | PASS | PASS | BLOCKED |

## Counts

```json
{
  "qsg08": {
    "row_count": 108,
    "pair_count": 54,
    "adaptive_runtime_pass_rows": 54,
    "baseline_common_needs_repair_rows": 54,
    "prompt_to_task_baseline_common_needs_repair_rows": 54,
    "orchestration_reasoning_baseline_common_needs_repair_rows": 54,
    "failed_rows": 0,
    "quality_gate_pairs": 0,
    "adaptive_effect_proven_pairs": 54,
    "token_savings_claim_allowed_pairs": 0,
    "semantic_framework_component_rows": 3,
    "fresh_load_component_gates_passed": 3,
    "leakage_authority_control_rows_accepted": 108,
    "leakage_component_gates_passed": 3,
    "source_disjoint_eval_rows": 108,
    "source_disjoint_eval_rows_accepted": 108,
    "source_disjoint_eval_input_marker_rows": 0,
    "source_disjoint_component_gates_passed": 3,
    "ablation_control_rows_accepted": 108,
    "ablation_component_gates_passed": 3,
    "continue_training_proof_rows": 108,
    "continue_training_rows_accepted": 108,
    "continue_training_input_marker_rows": 0,
    "continue_training_state_hash_changed_rows": 108,
    "continue_training_component_gates_passed": 3
  },
  "qsg09": {
    "row_count": 108,
    "pair_count": 54,
    "adaptive_runtime_pass_rows": 54,
    "baseline_common_runtime_pass_rows": 0,
    "baseline_common_needs_repair_rows": 54,
    "prompt_to_task_baseline_common_needs_repair_rows": 0,
    "orchestration_reasoning_baseline_common_needs_repair_rows": 54,
    "failed_rows": 0,
    "runtime_quality_passed_rows": 54,
    "quality_gate_pairs": 0,
    "adaptive_effect_proven_pairs": 54,
    "token_savings_claim_allowed_pairs": 0,
    "semantic_framework_component_rows": 3,
    "fresh_load_component_gates_passed": 3,
    "leakage_authority_control_rows_accepted": 108,
    "leakage_component_gates_passed": 3,
    "source_disjoint_eval_rows": 108,
    "source_disjoint_eval_rows_accepted": 108,
    "source_disjoint_component_gates_passed": 3,
    "ablation_control_rows_accepted": 108,
    "ablation_component_gates_passed": 3,
    "continue_training_rows_accepted": 108,
    "continue_training_component_gates_passed": 3,
    "baseline_internal_prompt_to_task_rows": 12,
    "learned_state_prompt_to_task_rows": 42,
    "baseline_internal_orchestration_reasoning_rows": 0,
    "public_plan_orchestration_reasoning_rows": 0,
    "no_matching_orchestration_reasoning_rows": 0,
    "not_trained_orchestration_reasoning_rows": 54,
    "provenance_rebuild_blocked_orchestration_reasoning_rows": 54
  }
}
```

Remaining blockers: OrchestrationReasoning common checkpoint source-admission provenance rebuild, semantic quality, common readiness, token savings, promotion, and final readiness claims remain closed.
