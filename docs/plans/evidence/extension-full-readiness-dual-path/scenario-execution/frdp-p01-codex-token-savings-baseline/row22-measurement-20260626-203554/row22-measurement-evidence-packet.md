# Row22 Measurement Evidence Packet

claim_scope:
Row22 `frdp-p01-codex-token-savings-baseline` smaller measurement attempt and exact blocker for missing accepted before/after token counts plus same-workload quality guard evidence.

evidence_scope:
Read-only inspection of required Row22/Row21/Phase 03/board/action-plan artifacts, targeted read of the prior Cursor Row12 measurement pattern, and generated blocker/status files under this fresh Row22 folder.

count_claimed:
0 measurement command runs. 1 exact Row22 blocker packet created.

independent_evidence_count:
10 read records plus this generated status/packet. No command-output evidence record exists for a new measurement run because no safe existing Row22/Codex measurement command was identified.

subagent_id_or_NONE:
row22-measurement-worker

downgrade_needed:
Yes. The evidence supports only an exact blocker. It does not support token savings, host-visible candidate behavior, formal joined pass, product readiness, or full extension readiness.

## Row

- row: 22
- scenario_id: `frdp-p01-codex-token-savings-baseline`
- host/extension target: Codex token baseline

## Files Read

- `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-token-savings-baseline/row22-token-baseline-20260626-202908/row22-token-baseline-status.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-token-savings-baseline/row22-token-baseline-20260626-202908/row22-token-baseline-evidence-packet.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-codex-chat-ui-marker/row21-status-update-20260626-202023/row21-status-update.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-03-command-output/03-dotnet-token-trace.txt`
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-action-plan.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-token-savings-baseline/run-scenario.ps1`
- `Tests/ExtensionTokenTraceAccountingTests.cs`
- `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-token-savings-baseline/measurement-rerun-20260625-023928/pair-validation.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-token-savings-baseline/measurement-rerun-20260625-023928/execution-report.json`

## Command Run Or Blocker

No measurement command was run.

Exact blocker:

`ROW22_BLOCKED_NO_SAFE_EXISTING_CODEX_MEASUREMENT_COMMAND`

Reason:

- Existing Row22 status already says `before_tokens=null`, `after_tokens=null`, `token_savings_claim=false`, and the prior dotnet guard command did not expose accepted before/after counts or a same-workload quality guard for Row22.
- The prior Cursor Row12 pattern was found, but `run-scenario.ps1` is hardcoded for `frdp-p01-cursor-token-savings-baseline`, Cursor host fields, and `$PSScriptRoot` output paths.
- Running that Cursor script would write outside the allowed fresh Row22 measurement folder and would not create accepted Codex Row22 evidence.
- Creating a new ad hoc Row22 measurement harness would be inventing code, which this bounded work order forbids when no safe existing command exposes the required values.

## Required Measurement Decision

- row22_measurement_attempted: `true`
- measurement_command_run: `false`
- before_tokens: `null`
- after_tokens: `null`
- token_delta: `null`
- token_reduction_observed: `false`
- quality_guard_available: `false`
- quality_guard_passed: `false`
- token_savings_claim: `false`

The evidence supports only the blocker above. Counts are missing, and there is no accepted Row22 same-workload quality guard pass.

## Host And Product Readiness Locks

- host_visible_candidate_behavior_proven: `false`
- host_route_blocker: `ROW21_BLOCKED_UI_INPUT_CONTROL_NOT_FOUND`
- formal_joined_pass: `false`
- product_readiness_claim: `false`

Row21 remains the controlling Codex host-route blocker. This packet did not attempt host UI and does not prove host-visible candidate behavior.

## Exact Missing Artifact Or Command

A non-mutating Row22/Codex measurement command or documented script parameterization that:

- accepts an output directory under a fresh Row22 measurement folder;
- writes all generated artifacts only inside that folder;
- uses the same workload/tokenizer for baseline and adaptive runs;
- emits accepted `before_tokens`, `after_tokens`, `token_delta`, `token_reduction_observed`, and same-workload quality guard pass/fail;
- preserves `token_savings_claim=false` unless reduction and quality guard pass are both accepted;
- does not run training, eval, DPO, checkpoint mutation, locator mutation, hidden external AI, or git actions.

## Mutation Boundaries Kept

- training_mutation: `false`
- eval_run: `false`
- dpo_run: `false`
- checkpoint_mutation: `false`
- locator_mutation: `false`
- hidden_external_ai: `false`
- host_ui_screenshot_or_readback: `false`
- source_patch: `false`
- git_stage: `false`
- git_commit: `false`
- git_push: `false`

## Non-Claims

- I did not run a measurement command.
- I did not verify token savings.
- I did not find accepted Row22 `before_tokens` or `after_tokens`.
- I did not prove a token reduction.
- I did not prove a Row22 same-workload quality guard pass.
- I did not prove Codex host-visible candidate behavior.
- I did not prove product readiness.
- I did not create a formal joined pass.
- I did not run training, eval, DPO, checkpoint mutation, locator mutation, hidden external AI, or git actions.

## Exact Next Action

Create or identify a bounded non-mutating Row22/Codex measurement script or command that accepts an output directory under a fresh Row22 measurement folder, writes no artifacts elsewhere, and emits accepted `before_tokens`, `after_tokens`, `token_delta`, and same-workload quality guard pass/fail. Keep host-visible behavior blocked by `ROW21_BLOCKED_UI_INPUT_CONTROL_NOT_FOUND` until a separate clean route is proven.
