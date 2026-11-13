# Row 12 Cursor token trace harness report

Status: MEASUREMENT_NEEDS_FIX

## Current task

Create the smallest row12 Cursor token trace harness and run it once if feasible.

## Commands

- prompt hash verification: PASS
- baseline core/API no-send call: PASS
- adaptive core/API no-send call: PASS

## Token numbers (observation only)

Baseline input/output/total: 151 / 5868 / 6019.
Adaptive input/output/total: 151 / 5868 / 6019.
Observed candidate delta: 0.

This is not a token savings claim.

## Quality guard

Assertions passed: 13 / 14.
Critical failures: 1.

## Proven

- Prompt hash reproduction: True.
- Pair validation errors: 0.
- External AI send: false.
- External AI used: false.
- Token savings claimed: false.

## Not proven

- Token savings.
- Semantic quality.
- Product readiness or full Cursor readiness.
- True Cursor host-visible row12 candidate behavior.

## Main artifacts

- baseline-request.json
- baseline-response.json
- baseline-trace.json
- baseline-core-trace.json
- adaptive-request.json
- adaptive-response.json
- adaptive-trace.json
- adaptive-core-trace.json
- token-traces.jsonl
- pair-validation.json
- quality-guard.json
- execution-report.json
