# QUALITY-SEMANTIC-GATES-08

QSG-08 adds source-disjoint continue-training proof rows for the exact extension API outputs accepted by QSG-07. It does not repair baseline/common PromptToTask or OrchestrationReasoning, and it does not claim semantic quality, common readiness, token savings, target promotion, or final extension readiness.

## Gate Summary

| Component | Fresh load | Source-disjoint eval | Ablation | Leakage/no exact authority | Continue-training | Semantic quality |
| --- | --- | --- | --- | --- | --- | --- |
| prompt_to_task | PASS | PASS | PASS | PASS | PASS | BLOCKED |
| orchestration_reasoning | PASS | PASS | PASS | PASS | PASS | BLOCKED |
| skill_selection | PASS | PASS | PASS | PASS | PASS | BLOCKED |

## Counts

```json
{
  "qsg07": {
    "row_count": 108,
    "pair_count": 54,
    "adaptive_runtime_pass_rows": 54,
    "baseline_common_needs_repair_rows": 54,
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
    "ablation_control_rows": 108,
    "ablation_control_rows_accepted": 108,
    "ablation_control_marker_rows": 0,
    "ablation_component_gates_passed": 3,
    "continue_training_rows_accepted": 0
  },
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
  }
}
```

Remaining blockers: baseline/common PromptToTask still has 54 needs-repair rows and baseline/common OrchestrationReasoning still has 54 needs-repair rows. Semantic quality and token/common/full readiness claims remain closed.
