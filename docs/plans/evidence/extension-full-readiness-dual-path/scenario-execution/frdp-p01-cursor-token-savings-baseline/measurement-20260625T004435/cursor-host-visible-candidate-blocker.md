# Cursor host-visible candidate blocker

Status: `BLOCKED_WITH_NEXT_ACTION`

Scenario: `frdp-p01-cursor-token-savings-baseline`
Marker: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`

No existing row12-specific no-send Cursor host-visible harness path was found in this slice.

Existing row11 Cursor extension-host/panel harness files are hard-coded to another scenario and marker. Changing or parameterizing that harness is outside this measurement write scope.

Next fix: create or parameterize a row12 no-send Cursor extension-host/panel harness that accepts scenario_id, marker, prompt, method=prompt-to-task, and evidenceRoot under the measurement folder; then rerun the same marker and attach host-visible candidate evidence.

Non-claims: no token savings, no semantic quality, no readiness, no training, no checkpoint mutation, no locator update.
