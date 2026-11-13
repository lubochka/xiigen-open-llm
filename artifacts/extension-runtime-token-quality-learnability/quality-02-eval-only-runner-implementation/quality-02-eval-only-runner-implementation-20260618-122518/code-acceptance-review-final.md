# QUALITY-02 Eval-only Runner Final Code Acceptance Review

Generated at local time: 2026-06-18T14:12:24.8024680+03:00

Boundary: bounded CODE_ACCEPTANCE_REVIEW sub-agent. Review only; artifact writes only. Git authority forbidden. No git add, commit, push, branch switch, code edit, docs edit, state edit, host runtime, external AI/model call, actual approved eval, training, DPO creation, checkpoint mutation, target promotion, locator update, or product readiness claim was performed.

Verdict: FAIL

## Scope Reviewed

- Current branch: `codex/chat-gpt-5.5`
- Current changed code/contracts/tool/tests/docs/state/evidence folder.
- Prior review and fix artifacts: `code-review-pass-01`, `code-review-pass-01-fix-report`, `code-review-pass-02-after-fix`, `code-review-pass-02-fix-report`, `code-review-pass-03-after-fix2`, `code-review-pass-03-fix-report`, and `code-review-pass-04-after-fix3`.
- Accepted implementation packet and scorer contracts, including `future-runner-input-contract.json`, `scorer-contract.json`, and `score-output-schema.json`.

## Findings

### HIGH - Per-layer score range contract is not enforced by the runner before shared reports can be produced

Evidence:

- `artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/scorer-contract.json:129`, `:146`, `:163`, and `:180` define each scored layer as `integer_0_to_5_future_only`.
- `artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/scorer-contract.json:231` defines per-layer `score_0_to_5`; `:237` requires adaptive to be compared against baseline per layer.
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:1009`, `:1011`, and `:1031` require per-layer scores plus `baseline_score` and `adaptive_score`.
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:235` starts runnable-row validation, but no validation rejects out-of-range `BaselineScore` or `AdaptiveScore`.
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:461` maps only `AdaptiveScore` into `ExtensionQualityScoring`.
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:468-475` can label an out-of-range adaptive value above the pass threshold as per-layer `PASS`; `:483-485` then emits the raw baseline/adaptive values.
- `Tests/ExtensionEvalOnlyQualityRunnerTests.cs` has no runner test for baseline/adaptive values below 0 or above 5.

Impact:

The runner can accept and emit invalid score values in JSON, JSONL, and Markdown. A baseline score outside 0..5 is not rejected or surfaced by the core scorer at all, and an adaptive score above 5 can be emitted with a per-layer `PASS` label even though the row-level scorer may add an out-of-range reason. This violates the accepted score contract and leaves a shared-evidence integrity gap before parent commit/push.

## Checks Run

- `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionEvalOnlyQualityRunner"`: PASS, 15 passed, 0 failed.
- `dotnet build 43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Quality02EvalOnlyRunner.csproj`: PASS, 0 warnings, 0 errors.
- JSON parse: PASS, 14 JSON files parsed, including active evidence JSON and execution state.
- JSONL parse: PASS, 2 JSONL files and 4 non-empty rows parsed.
- Runner report claim-lock scan: PASS, no true claim-lock booleans found.
- Runner report scope: PASS, `synthetic_fixture_only=true`, `actual_approved_eval_executed=false`, `host_runtime_started=false`, `training_started=false`, and `model_checkpoint_locator_mutated=false`.
- Runner report privacy scan over generated report JSON, score-row JSONL, and Markdown: PASS, 0 raw-private probe matches.
- `git diff --check`: PASS.
- `git status --porcelain`: read-only check completed; dirty set is the expected implementation/docs/state/evidence set plus these final review artifacts.

## Passing Boundary Areas

- OperationResult behavior is mostly aligned for public null input, malformed JSONL, unreadable input, validation failure, and report-write failure paths covered by current tests.
- Core logic remains in `43.SmartSkillsDomain`; `Program.cs` remains a thin CLI adapter.
- No Claude/Cursor/Codex host adapter logic was added.
- Deterministic scorer and privacy helper labels remain helper/eval-only/internal only, not semantic, trainable, runtime, product, or final authority.
- Scored/excluded route separation works for the synthetic fixture: 3 input rows, 1 scored row, 1 evidence-only excluded row, and 1 common-blocked excluded row.
- No approved private eval was executed; current report remains synthetic-fixture-only.
- DPO, training, checkpoint, locator, target promotion, live-host, common-readiness, token-savings, semantic-readiness, and final-readiness claims remain closed.
- Dashboard N/A is justified because no dashboard/API catalog surface changed.

## Non-blocking Observation

The top-level execution state still contains wording that says "after code-review-pass-02 fix" while the active evidence folder contains the pass-03 fix and pass-04 PASS review. This is documentation drift, not the blocking reason for this verdict, because the phase, scope, next action, and claim locks still point to parent review/commit with product claims closed.

## Exact Fix Work Order

Bounded fix worker, git authority forbidden:

- Allowed write scope:
  - `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs`
  - `Tests/ExtensionEvalOnlyQualityRunnerTests.cs`
  - regenerated evidence files in `artifacts/extension-runtime-token-quality-learnability/quality-02-eval-only-runner-implementation/quality-02-eval-only-runner-implementation-20260618-122518/`
  - state/docs only if needed to accurately record the fix evidence, with no product readiness claim
- Required code fix:
  - Validate every `ExtensionEvalOnlyQualityRunnerLayerInput.BaselineScore` and `AdaptiveScore` is an integer from 0 through 5 before scoring or report construction.
  - Return `OperationResult` validation failure with `EVAL_ONLY_INPUT_VALIDATION_FAILED` for invalid values.
  - Ensure invalid score inputs write no report JSON, score JSONL, or Markdown.
  - Keep deterministic labels helper/eval-only only and keep claim locks false.
- Required tests:
  - Add focused tests for baseline below 0, baseline above 5, adaptive below 0, adaptive above 5.
  - Assert validation failure, useful error text naming the invalid score field/layer, and no generated reports.
  - Assert valid 0 and 5 boundary values still pass.
- Required verification:
  - `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionEvalOnlyQualityRunner"`
  - `dotnet build 43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Quality02EvalOnlyRunner.csproj`
  - JSON/JSONL parse of regenerated evidence and execution state if state is touched
  - raw-private probe scan of generated runner report JSON, score-row JSONL, and Markdown
  - product/readiness claim-lock scan
  - `git diff --check`
  - `git status --porcelain` read-only

Parent should not commit/push this QUALITY-02 code slice until that fix is independently reviewed and accepted.
