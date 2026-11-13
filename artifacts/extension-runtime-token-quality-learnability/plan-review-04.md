# PLAN-REVIEW-04 Evidence Packet

Sub-agent id/name: `PLAN-REVIEW-04` / bounded sub-agent PLAN-REVIEW-04

Target outcome row: Token before/after improvement is proven.

Execution authorized: false. Product implementation, host runtime, eval, training, checkpoint mutation, commit, and push were not authorized or performed.

## Scope

Review focus: token before/after evidence. The pass checked whether the active plan proves token comparison correctly and prevents token savings from passing when prompts/runs are mismatched or adaptive quality regresses.

## Findings

PASS_AFTER_FIX. A narrow plan-text repair was needed.

The active plan already required most of the token proof contract:

- baseline run with adaptive/user-local model disabled and no user checkpoint loaded;
- adaptive run with the exact user-local checkpoint/overlay id named;
- same `scenario_id`, canonical prompt text, prompt hash, host/runtime/build, adapter/core build, method, debug setting, host model settings, tokenizer name/version, and counted accounting spans;
- same real host path classification, so baseline cannot use a helper while adaptive uses host UI;
- repeated-run variance handling with at least 3 runs for variable hosts, median/min/max/stddev, and an inconclusive result when only one run is possible;
- explicit failure outcomes: `INVALID_PAIR`, `FAIL_QUALITY_REGRESSION`, `INCONCLUSIVE_TOKEN_DELTA`, and `FAIL_PRIVACY_TRACE`;
- token delta cannot prove runtime readiness, quality improvement, semantic quality, model readiness, target promotion, rollback, or learnability.

Gap repaired:

- Section 4.2.1 now requires trace rows to save `raw_input_ref_private`, `raw_output_ref_private`, `redacted_input_ref`, and `redacted_output_ref`, alongside input/output/total token counts.
- Section 4.2.1 now requires every token delta report to record `token_delta_threshold_total_tokens`, `token_delta_threshold_percent`, and the selected acceptance method.
- Section 4.2.1 now says missing/changed thresholds or thresholds not tied to the same scenario pack/accounting version produce `INCONCLUSIVE_TOKEN_DELTA`, not PASS.

## Fix Refs

Fresh repair/evidence/bookkeeping created:

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` Section 4.2.1
- `artifacts/extension-runtime-token-quality-learnability/plan-review-04.md`
- `artifacts/extension-runtime-token-quality-learnability/plan-review-04.json`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl`

Plan sections rechecked:

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` Section 0 target matrix
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` Section 4.2 and 4.2.1
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` Section 9 final acceptance matrix
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` Section 9.1 closure contract
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` Section 11 no-overclaim firewall

## Recheck Evidence

Required search terms were checked with `rg`: baseline, adaptive, prompt hash, tokenizer, accounting, token counts, variance, threshold, quality regresses, invalid pair, and token evidence cannot substitute/token evidence. The plan now contains the hard token methodology gate, raw/redacted trace references, explicit threshold/acceptance method, and related no-overclaim rows.

JSON state and JSON evidence parse, the JSONL review ledger parses line by line, and `git diff --check` passes.

Parent self-review does not count. Historical review rows are evidence only. This is fresh pass 4 of 10, with implementation still locked.
