# Row 12 measurement execution report

Status: `MEASUREMENT_NEEDS_FIX`

## Current task

Create the row 12 measurement packet for `frdp-p01-cursor-token-savings-baseline` using accepted `row12-workload-spec.md/json`.

## Commands run

- `dotnet run --project MeasurementRunner.csproj -- <measurement-root>`: completed; runner returned non-zero because the quality guard did not pass all assertions.

## Token numbers (observation only)

Baseline input/output/total: `151` / `366` / `517`.
Adaptive input/output/total: `151` / `366` / `517`.
Observed candidate delta: `0`.

This is not a token savings claim.

## Pair validation

Pair valid: `True`.
pair_validation_errors_count: `0`.

## Quality guard

Assertions passed: `3` / `14`.
Critical failures: `11`.

The guard checks actual response JSON only. Request text and report text are not credited as response quality.

## Host-visible candidate

Exact blocker recorded: `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-token-savings-baseline/measurement-20260625T004435/cursor-host-visible-candidate-blocker.json`.

## Proven

- Exact row12 prompt hash reproduced.
- Baseline/adaptive local no-send core API calls completed.
- Pair validation has zero same-field errors.
- External AI send is false.

## Not proven

- Token savings.
- Semantic quality.
- Product readiness or full Cursor readiness.
- Row12 Cursor host-visible candidate behavior.
- The 14-assertion quality guard.

## Counter recommendation for total 28

Do not increment passed scenario count for row 12. Keep row 12 in needs-fix/blocked-for-host-and-quality-guard status until a row12-specific host-visible no-send path emits the required structured response and all 14 quality assertions pass.
