# QUALITY-SEMANTIC-GATES-12

QSG-12 re-scores QSG-11 equal pairs using the existing 0-5 human-readable rubric anchors. It advances adaptive quality-improvement evidence for learned-state behavior, but keeps semantic/common/token/final readiness closed.

## What Advanced

- 54/54 pairs show adaptive quality-improvement evidence over baseline/runtime evidence.
- 0/54 pairs regress.
- 0/54 pairs improve user-facing ChatDisplayText; that remains a blocker.

## Counts

```json
{
  "qsg11": {
    "pair_count": 54,
    "human_readable_rubric_scored_pairs": 54,
    "quality_non_regression_passed_pairs": 0,
    "equal_quality_pairs": 0,
    "quality_improvement_observed_pairs": 0,
    "semantic_graduation_accepted_pairs": 0,
    "baseline_total_tokens": 13887,
    "adaptive_total_tokens": 14343,
    "total_token_delta": -456
  },
  "qsg12": {
    "pair_count": 54,
    "adaptive_quality_improvement_evidence_pairs": 54,
    "equal_quality_pairs": 0,
    "quality_regression_observed_pairs": 0,
    "semantic_graduation_accepted_pairs": 0,
    "component_gate_count": 3,
    "components_adaptive_improvement_evidence_advanced": 3,
    "adaptive_chat_text_improved_pairs": 0,
    "token_reduction_observed_pairs": 12,
    "token_increase_observed_pairs": 42,
    "token_equal_pairs": 0,
    "baseline_total_tokens": 13887,
    "adaptive_total_tokens": 14343,
    "total_token_delta": -456
  }
}
```

Remaining blockers: adaptive tokens are still 456 higher in aggregate, semantic graduation remains 0, and the final user-facing output is still generic.
