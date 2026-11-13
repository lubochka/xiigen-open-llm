# QUALITY-02 scorer contract

Run: `quality-01-02-20260618-053620`

Scorer type: deterministic transparent rubric scoring helper. Он не является trainable semantic authority и не решает task через external AI.

## Inputs per row

- baseline TOKEN-03 row ref, raw/private trace ref, redacted trace ref;
- adaptive TOKEN-04 row ref, raw/private trace ref, redacted trace ref;
- scenario pack row ref;
- QUALITY-03 pair ref.

## Deterministic rules

1. Pair integrity must match `pair_id`, `scenario_id`, host, method, prompt hash, tokenizer/accounting version and redaction policy.
2. Scores use only observable current trace fields: response text, selected skills, check-set statuses, errors/warnings and claim flags.
3. A layer is capped below PASS when the output is generic and does not spell out scenario-specific expected behavior.
4. Forbidden overclaim, external AI, training, model mutation, checkpoint/locator mutation or runtime substitution forces failure.
5. `limited_observed_non_regression=true` is not the same as QUALITY-03 quality PASS.
6. `quality_non_regression_passed=false` in these rows because strict source-disjoint and controls remain missing.

## What this proves

- The current pair set can be scored reproducibly from existing outputs/traces.

## What this does not prove

- Token savings, quality improvement, semantic quality, learnability, live-host savings or final readiness.
