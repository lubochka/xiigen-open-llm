# Phase 12 fix analysis

Result: PHASE_12_TOKEN_QUALITY_READY_FOR_REVIEW
Source run provenance path: not_provided_to_runner

## Measurement vs product
- MEASUREMENT-12-01: measurement_report_issue_fixed_in_this_runner - The earlier headline said orchestration-plan and skill-selection remained partial/not proven, but adaptive raw scores only fail universal_orchestration_plan_quality. Fresh reports now use the failing criteria map as the source of truth.
- MEASUREMENT-12-02: measurement_coverage_issue_fixed_in_this_runner - Ablation responses are now quality-scored and included in raw_scores/failing-quality-criteria-map; adaptive-vs-baseline remains the Phase 12 pass gate.
- PRODUCT-12-01: actual_product_model_failure - Adaptive universal_orchestration_plan_quality fails in 0 of 9 scenario/host pairs with loaded_guide_state_no_matching_learned_pattern.

## Quality
- Adaptive failed rows:
- Adaptive failed criteria:
- All adaptive pairs passed: True
- Quality regressed anywhere: False

## Leakage and shortcuts
- User-visible project-specific leakage failures: 0
- Audit metadata refs present: True
- Full training corpus disjoint proven: False
- Hidden deterministic shortcut failures: 0

## Token decision
- Token reduction observed: /
- Token success accepted: True
- Decision: Token reduction is internally consistent enough for this harness if quality passes, but remains blocked because adaptive quality does not pass.
