# QUALITY-02 Eval-only Runner Code Review Pass 03 After Fix 2

Verdict: FAIL

Scope reviewed:
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunnerContracts.cs`
- `43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Program.cs`
- `43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Quality02EvalOnlyRunner.csproj`
- `Tests/ExtensionEvalOnlyQualityRunnerTests.cs`
- `docs/COMPONENT-INTERFACES.md`
- `docs/SUBPROJECT-MAP.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `artifacts/extension-runtime-token-quality-learnability/quality-02-eval-only-runner-implementation/quality-02-eval-only-runner-implementation-20260618-122518/`

## Summary

Fix #2 closes the two pass-02 runnable-row blockers: `layer_inputs:null` now returns an `OperationResult` validation failure with no report writes, and row/layer `failure_codes` private markers are blocked before report construction.

The review still fails because excluded rows (`EVIDENCE_ONLY` and by the same path `COMMON_BLOCKED`) skip copied-text privacy validation while their `scenario_id`, `pair_id`, and `evidence_scope` are copied into shared report JSON and Markdown. A raw private sentinel in an `EVIDENCE_ONLY` row exited successfully, wrote reports, and copied the sentinel into shared output.

No product readiness claim is made by this review.

## Finding

### HIGH - Raw private text can leak through excluded-row identifiers into shared report artifacts

Exact lines:
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:169`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:170`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:269`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:270`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:275`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:296`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:297`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:299`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:434`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:440`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:441`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:445`

What I found:
- `ValidateRows` calls `ValidateRunnableRow` only for `USER_ADAPTIVE_EVAL_ONLY`.
- The copied-text/private-marker validators are inside `ValidateRunnableRow`.
- `BuildExcludedPair` still copies excluded-row `ScenarioId`, `PairId`, and non-empty `EvidenceScope` into `pair_results`.
- `pair_results` are written to report JSON and Markdown.

Probe result:
- `EVIDENCE_ONLY` input with redacted raw-marker values in `scenario_id` and `pair_id`: FAIL.
- Runner exit code: `0`.
- Report JSON written: yes.
- Score rows JSONL written: yes, empty because no rows were scored.
- Markdown report written: yes.
- Sentinel found in generated shared outputs: yes, in report JSON and Markdown.

Why it matters:
- Review question 2 asks whether raw private sentinel text is blocked/redacted before JSON, JSONL, Markdown, and shared evidence.
- Excluded rows are shared evidence context. They are not scored, but they are still emitted.
- The current fix blocks the specific runnable `failure_codes` path but does not close all shared-output sentinel paths.

Required fix:
- Validate all input-derived strings that can be emitted for every route before `BuildReport`, not only `USER_ADAPTIVE_EVAL_ONLY`.
- At minimum, apply `ValidateCopiedText` to `scenario_id`, `pair_id`, and any non-empty emitted `evidence_scope` for all rows.
- Apply the same rule to any future emitted excluded-row fields, or avoid copying unvalidated excluded-row input into shared reports.
- Add targeted tests for `EVIDENCE_ONLY` and `COMMON_BLOCKED` rows with raw private markers in emitted identifiers/scope, asserting `EVAL_ONLY_INPUT_VALIDATION_FAILED` and no JSON, JSONL, or Markdown report files.
- Add a negative CLI/shared-artifact scan for excluded-row sentinel text.

## Review Questions

1. Does semantically malformed JSONL with `layer_inputs:null` now return `OperationResult` validation failure with no scoring/report writes? PASS. CLI probe returned `EVAL_ONLY_INPUT_VALIDATION_FAILED`, exit code `3`, and wrote no report JSON, score rows JSONL, or Markdown.
2. Is raw private sentinel text blocked/redacted before `failure_codes`, reviewer notes, JSON, JSONL, Markdown/shared evidence? FAIL. Runnable row/layer `failure_codes` and reviewer-note paths are covered, but excluded-row identifiers can still leak raw private markers into shared report JSON and Markdown.
3. Did fix preserve previous accepted fixes: prefixed public contracts, `prompt_sha256`/`mode`/`evidence_scope` enforcement, synthetic evidence first-class? PASS.
4. Do targeted tests/build/JSON/JSONL/git diff checks pass? PASS for the bounded checks run in this review.
5. Does no hidden deterministic shortcut become semantic/trainable/product authority? PASS. The deterministic scorer remains labelled eval-only/helper/internal baseline and is not used as semantic, trainable, runtime, or product authority.
6. Are core boundary and thin adapter boundaries still respected? PASS. Core validation/scoring/reporting remains in `43.SmartSkillsDomain`; `Program.cs` remains a thin CLI adapter.
7. Do product claims remain closed? PASS. Product/readiness claims remain false in generated evidence.

## Checks Run

- `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionEvalOnlyQualityRunner"`: PASS, 13 passed, 0 failed.
- `dotnet build 43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Quality02EvalOnlyRunner.csproj`: PASS, 0 warnings, 0 errors.
- JSON parse for pass-02 review/fix evidence, current runner report, checks report, self-check, and execution state: PASS.
- JSONL parse for synthetic input and generated score rows: PASS, 4 total non-empty rows parsed.
- Current evidence raw-private marker scan: PASS, no configured raw-marker values matched in the active evidence folder.
- Product-claim scan of current report/check evidence: PASS, no forbidden true readiness/product/runtime/training claim found.
- `git diff --check`: PASS.
- `layer_inputs:null` CLI probe: PASS, validation failure and no reports.
- Row-level `failure_codes` sentinel CLI probe: PASS, validation failure and no reports.
- Layer-level `failure_codes` sentinel CLI probe: PASS, validation failure and no reports.
- `EVIDENCE_ONLY` identifier sentinel CLI probe: FAIL, reports written and sentinel copied into shared JSON/Markdown.

## Boundary Review

- Project standards: FAIL because raw private text can still enter shared report artifacts through excluded rows.
- Tests/Gate B: FAIL because current tests do not cover sentinel leakage from excluded-row emitted identifiers/scope.
- Core boundary: PASS.
- Thin adapter boundary: PASS.
- Hidden deterministic shortcut sweep: PASS.
- Synthetic fixture boundary: PASS.
- Product claims: PASS.

## Next Action

Send a bounded fix worker for the excluded-row privacy gap only. Keep git authority forbidden for the worker. After the fix, rerun an independent review that includes excluded-row sentinel probes for `EVIDENCE_ONLY` and `COMMON_BLOCKED`.
