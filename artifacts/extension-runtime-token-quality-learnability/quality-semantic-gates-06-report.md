# QUALITY-SEMANTIC-GATES-06

QSG-06 adds leakage/no-source-marker authority controls for current extension API outputs. It does not claim source-disjoint eval, ablation, continue-training, semantic quality, common readiness, token savings, target promotion, or final extension readiness.

## Gate Summary

| Component | Fresh load | Source-disjoint eval | Ablation | Leakage/no exact authority | Continue-training | Semantic quality |
| --- | --- | --- | --- | --- | --- | --- |
| prompt_to_task | PASS | BLOCKED | BLOCKED | PASS | BLOCKED | BLOCKED |
| orchestration_reasoning | PASS | BLOCKED | BLOCKED | PASS | BLOCKED | BLOCKED |
| skill_selection | PASS | BLOCKED | BLOCKED | PASS | BLOCKED | BLOCKED |

## Counts

```json
{
  "qsg05": {
    "row_count": 108,
    "pair_count": 54,
    "adaptive_runtime_pass_rows": 54,
    "baseline_common_needs_repair_rows": 54,
    "failed_rows": 0,
    "quality_gate_pairs": 0,
    "adaptive_effect_proven_pairs": 54,
    "token_savings_claim_allowed_pairs": 0,
    "semantic_framework_component_rows": 3,
    "fresh_load_component_gates_passed": 3
  },
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
  }
}
```

Remaining blockers: source-disjoint eval, ablation controls, and continue-training proof for these exact outputs are still missing. Baseline/common PromptToTask and OrchestrationReasoning remain needs-repair.
