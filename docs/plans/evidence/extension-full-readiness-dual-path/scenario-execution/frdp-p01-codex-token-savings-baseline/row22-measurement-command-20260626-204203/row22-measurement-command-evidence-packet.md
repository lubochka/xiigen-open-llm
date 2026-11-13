# Row22 Codex measurement command evidence packet

## Summary

An evidence-only Row22/Codex measurement script was created in this fresh folder:

`docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-token-savings-baseline/row22-measurement-command-20260626-204203/run-row22-codex-measurement.ps1`

The script did not produce accepted measurement counts. The first invocation and the one allowed focused rerun both stopped at PowerShell parse time before output JSON was generated.

## Required Status Fields

| Field | Value |
|---|---:|
| row22_measurement_command_created | true |
| row22_measurement_command_run | false |
| before_tokens | null |
| after_tokens | null |
| token_delta | null |
| token_reduction_observed | false |
| quality_guard_available | false |
| quality_guard_passed | false |
| token_savings_claim | false |
| host_visible_candidate_behavior_proven | false |
| host_route_blocker | `ROW21_BLOCKED_UI_INPUT_CONTROL_NOT_FOUND` |
| formal_joined_pass | false |
| product_readiness_claim | false |

## Command Or Blocker

`ROW22_BLOCKED_MEASUREMENT_SCRIPT_PARSE_ERROR_AFTER_ALLOWED_RERUN`

The script exists, but the measurement command did not successfully run. No `row22-measurement-report.json` is present because no counts were produced.

## Mutation Boundaries Kept

- No nested agents.
- No hidden external AI.
- No host UI, screenshots, prompt typing, prompt paste, or prompt send.
- No training, eval, DPO, checkpoint, or locator mutation.
- No product/test/source edits outside this fresh Row22 evidence folder.
- No scenario board, action plan, dashboard, governance, state, model, checkpoint, locator, plugin, or unrelated doc edits.
- No git stage, commit, or push.

## Non-Claims

- I did not produce accepted `before_tokens` or `after_tokens`.
- I did not verify token savings.
- I did not prove a token reduction.
- I did not prove a Row22 same-workload quality guard pass.
- I did not prove Codex host-visible candidate behavior.
- I did not prove Codex host UI readiness.
- I did not create a formal joined pass.
- I did not prove product readiness.

## Exact Next Action

Repair or replace the evidence-only Row22/Codex measurement script inside a new fresh Row22 measurement-command folder, then run it once to emit accepted `before_tokens`, `after_tokens`, `token_delta`, and same-workload quality guard pass/fail. Keep `host_visible_candidate_behavior_proven=false`, `host_route_blocker=ROW21_BLOCKED_UI_INPUT_CONTROL_NOT_FOUND`, `formal_joined_pass=false`, and `product_readiness_claim=false`.
