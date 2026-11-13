# Plan Review Cycle 08 - QA Automation Coverage

## Human-readable packet

Stage: cycle 8 of 10 before any runtime/code/training/host continuation.

Reviewer/sub-agent id: `PLAN-REVIEW-CYCLE-08-QA-AUTOMATION-COVERAGE`.

Model/reasoning evidence: `model=gpt-5.5`, `reasoning_effort=xhigh`, `downgrade_warning_or_NONE=NONE`.

Current Luba work-order authority: bounded execution-plan review/fix sub-agent, cycle 8 of 10, focused on tests/e2e/QA automation coverage for the three full extensions. No agents, no commit/push, no branch change, no runtime/source code edits.

## Reviewed material

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `plan-review-cycle-01..07.md/json`
- QSG/runtime/token/quality/local-learning reports as supporting context, especially QSG-17..19, token pairing reports, backup/export/import/share, fresh-load, continue-training and held-out/ablation artifacts.

## Findings

1. `PASS_WITH_PLAN_FIX`: the plan had strong separate gates for runtime, token pairing, quality, local learnability, source/license, deterministic-boundary and no-overclaim, but it did not yet force one complete QA automation coverage matrix per host and evidence layer.
2. `PASS_WITH_SCOPE_HONESTY_FIX`: the missing matrix could let a later report say "tests passed" without stating whether those tests were unit, API, dashboard, handler, package smoke or live host UI/chat. Section 4.1.2 now requires exact evidence labels.
3. `PASS_WITH_FULL_EXTENSION_QA_FIX`: the plan now requires QA coverage for Claude, Cursor, Codex and cross-host paths, including unit tests, integration/API tests, dashboard QA where relevant, handler-level tests, live host UI/chat/Extension Host probes where possible, negative paths, regression tests and UX/debug trace checks.
4. `PASS_WITH_TOKEN_LEARNABILITY_QA_FIX`: the QA matrix now includes token/quality pairing checks and local adaptive model backup/export/import/restore/load-without-retrain/continue-training/fresh-load/held-out/ablation/rollback checks.
5. `PASS_WITH_ARTIFACT_FIX`: Section 6 now adds `QA-01`; Section 9.1 now requires QA automation coverage closure artifacts.
6. `PASS_WITH_STATE_FIX`: execution state advanced to `PLAN-REVIEW-CYCLE-08_OF_10`, records cycle 08 artifacts, keeps `implementation_authorized=false`, and sets next action to cycle 9.

## Fixes made

- Added plan Section `4.1.2. QA automation coverage matrix`.
- Added `QA-01` to the execution phase table.
- Added a final acceptance row for QA automation coverage.
- Updated execution state cycle counters and cycle 08 gate metadata.
- Added this Markdown report and the matching JSON report.

No runtime/source code was edited. No runtime, host probes, training, `dotnet test`, commit or push was run.

## QA matrix added to the plan

The plan now requires every QA report to name:

- `qa_evidence_scope`
- `host`
- `evidence_claim_label`
- `claim_allowed`
- `claim_not_allowed`
- `required_artifacts`

The explicit labels include narrow passes such as `PASS_UNIT_CONTRACT_ONLY`, `PASS_INTEGRATION_API_ONLY`, `PASS_DASHBOARD_QA_ONLY`, `PASS_HANDLER_LEVEL_ONLY`, live labels such as `PASS_LIVE_HOST_RUNTIME` and `PASS_LIVE_EXTENSION_HOST`, lifecycle label `PASS_LOCAL_MODEL_LIFECYCLE`, and failure labels such as `FAIL_SCOPE_OVERCLAIM`, `FAIL_NEGATIVE_PATH_MISSING`, `FAIL_DEBUG_TRACE_MISSING`, `FAIL_TOKEN_QUALITY_PAIRING` and `FAIL_EXPORT_IMPORT_LIFECYCLE`.

## Recheck evidence

Required verification for this bounded cycle:

- JSON parse for cycle 08 JSON artifact and execution state.
- `git diff --check`.
- Targeted `rg` for unit/integration/e2e/QA/dashboard/handler/live host/debug/token/export/import/backup/continue-training.

Expected gate status after verification: cycle 08 counts toward the 10 Review Cycle Gate, with runtime/code/training/host execution still locked until cycles 09 and 10 also pass and the parent presents the required Russian visible plan/status packet.

