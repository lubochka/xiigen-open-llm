# QUALITY-SEMANTIC-GATES-05

QSG-05 adds the semantic proof gate framework for current extension API outputs. It does not claim token savings, semantic quality, common readiness, target promotion, or final extension readiness.

## Gate Summary

| Component | Fresh load | Source-disjoint eval | Ablation | Leakage/no exact authority | Continue-training | Semantic quality |
| --- | --- | --- | --- | --- | --- | --- |
| prompt_to_task | PASS | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED |
| orchestration_reasoning | PASS | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED |
| skill_selection | PASS | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED |

## Counts

```json
{
  "qsg04": {
    "row_count": 108,
    "pair_count": 54,
    "adaptive_runtime_pass_rows": 54,
    "baseline_common_needs_repair_rows": 54,
    "failed_rows": 0,
    "quality_gate_pairs": 0,
    "adaptive_effect_proven_pairs": 54,
    "token_savings_claim_allowed_pairs": 0,
    "semantic_framework_component_rows": 0,
    "fresh_load_component_gates_passed": 0
  },
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
  }
}
```

Remaining blockers: source-disjoint eval, ablation controls, leakage/no exact source authority controls, and continue-training proof for these exact outputs are still missing. Baseline/common PromptToTask and OrchestrationReasoning remain needs-repair.
