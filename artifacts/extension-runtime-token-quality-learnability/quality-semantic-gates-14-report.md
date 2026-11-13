# QUALITY-SEMANTIC-GATES-14

QSG-14 evaluates the accumulated QSG-07 through QSG-13 evidence and opens bounded semantic graduation only when every current upstream gate passes. Common readiness and final readiness remain closed.

## What This Proves

- 0/54 evaluated pairs have accepted semantic graduation evidence after source-disjoint, ablation, leakage, fresh-load, continue-training, baseline/common runtime, quality-improvement, ChatDisplayText, token, and no-overclaim gates are considered together.
- Bounded semantic quality claim allowed: false.
- Bounded token savings claim allowed for the QSG-13 paired trace set: false (0/54 pairs).

## What This Does Not Prove

- Runtime responses still do not claim semantic quality.
- Common readiness, target promotion, locator update, product readiness, learnability, rollback, and final readiness remain closed.

## Counts

```json
{
  "qsg13": {
    "pair_count": 54,
    "adaptive_quality_improvement_evidence_pairs": 54,
    "adaptive_chat_text_improved_pairs": 54,
    "semantic_graduation_accepted_pairs": 0,
    "common_readiness_graduated_pairs": 0,
    "token_reduction_observed_pairs": 54,
    "token_increase_observed_pairs": 0,
    "token_equal_pairs": 0,
    "baseline_total_tokens": 13887,
    "adaptive_total_tokens": 12357,
    "total_token_delta": 1530,
    "semantic_quality_gate_passed": false,
    "common_readiness_claim_allowed": false,
    "token_savings_claim_allowed": false,
    "final_readiness_claimed": false
  },
  "qsg14": {
    "pair_count": 54,
    "adaptive_quality_improvement_evidence_pairs": 54,
    "adaptive_chat_text_improved_pairs": 54,
    "semantic_graduation_accepted_pairs": 0,
    "limited_semantic_quality_evidence_accepted_pairs": 0,
    "common_readiness_graduated_pairs": 0,
    "token_savings_claim_allowed_pairs": 0,
    "token_reduction_observed_pairs": 54,
    "token_increase_observed_pairs": 0,
    "token_equal_pairs": 0,
    "baseline_total_tokens": 13887,
    "adaptive_total_tokens": 12357,
    "total_token_delta": 1530,
    "semantic_quality_gate_passed": false,
    "common_readiness_claim_allowed": false,
    "token_savings_claim_allowed": false,
    "final_readiness_claimed": false
  }
}
```

Next independent track: QUALITY-SEMANTIC-GATES-15 should produce common-readiness target/promotion/locator/final-readiness closure evidence or keep common/final readiness closed.
