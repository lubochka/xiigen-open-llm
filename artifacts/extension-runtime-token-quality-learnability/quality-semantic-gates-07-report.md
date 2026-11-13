# QUALITY-SEMANTIC-GATES-07

QSG-07 adds source-disjoint row-level eval ledgers and ablation/control rows for current extension API outputs. It does not claim continue-training, semantic quality, common readiness, token savings, target promotion, or final extension readiness.

## Gate Summary

| Component | Fresh load | Source-disjoint eval | Ablation | Leakage/no exact authority | Continue-training | Semantic quality |
| --- | --- | --- | --- | --- | --- | --- |
| prompt_to_task | PASS | PASS | PASS | PASS | BLOCKED | BLOCKED |
| orchestration_reasoning | PASS | PASS | PASS | PASS | BLOCKED | BLOCKED |
| skill_selection | PASS | PASS | PASS | PASS | BLOCKED | BLOCKED |

## Counts

```json
{
  "qsg06": {
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
    "leakage_authority_control_rows": 108,
    "leakage_authority_control_rows_accepted": 108,
    "leakage_component_gates_passed": 3,
    "source_disjoint_eval_rows_accepted": 0,
    "ablation_control_rows_accepted": 0,
    "continue_training_rows_accepted": 0
  },
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
  }
}
```

Remaining blockers: continue-training proof for these exact outputs is still missing. Baseline/common PromptToTask and OrchestrationReasoning remain needs-repair, so semantic quality and token/common readiness claims remain closed.
