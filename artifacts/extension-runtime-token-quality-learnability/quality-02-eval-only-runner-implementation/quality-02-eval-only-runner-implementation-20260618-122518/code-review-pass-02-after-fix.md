# QUALITY-02 Eval-only Runner Code Review Pass 02 After Fix

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

Review basis loaded:
- `docs/skills/generated-code-review-SKILL.md`
- `docs/skills/implementation-integrity-SKILL.md`
- `docs/skills/trainable-architecture-decision-SKILL.md`
- `docs/skills/test-integrity-SKILL.md`
- `code-review-pass-01.md/json`
- `code-review-pass-01-fix-report.md/json`
- accepted implementation packet under `artifacts/extension-runtime-token-quality-learnability/quality-02-eval-only-runner-implementation-packet/quality-02-eval-only-runner-implementation-packet-20260618-114340/`

## Pass-01 Finding Closure

- CR-01 public contract names: PASS. Public runner contract records are now `ExtensionEvalOnlyQualityRunner*`-prefixed and docs use those names.
- CR-02 provenance/scope gates: PASS. `prompt_sha256`, `mode=baseline_vs_adaptive_pair`, and `evidence_scope=local_private_eval_only` are required before scoring and emitted in reports.
- CR-03 OperationResult failures for invalid public input: FAIL. Null public parameters and unreadable/malformed syntax improved, but semantically malformed JSONL can still throw.
- CR-04 synthetic fixture evidence boundary: PASS. Reports and score rows carry `synthetic_fixture_only=true`, `actual_approved_eval_executed=false`, and `evidence_scope=local_private_eval_only`.
- CR-05 privacy coverage: FAIL. Output refs, evidence refs, and reviewer notes are covered, but other emitted/copied fields can still leak raw private text.

## Findings

### HIGH - Semantically malformed JSONL can throw instead of returning `OperationResult` failure

Exact lines:
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:272`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:274`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:281`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:343`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:348`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:459`
- `Tests/ExtensionEvalOnlyQualityRunnerTests.cs:1`

The fix added public null guards, but deserialized JSON can still set non-nullable collection properties to null. A syntactically valid `USER_ADAPTIVE_EVAL_ONLY` row with `"layer_inputs": null` crashes at `row.LayerInputs.Count` instead of returning an `OperationResult` validation failure. The same shape exists for `row.FailureCodes`, `layer.EvidenceRefs`, and `layer.FailureCodes` on later scoring/output paths.

Probe result:
- `dotnet run ... --input <temp-jsonl-with-layer_inputs-null> ...`: FAIL, unhandled `System.NullReferenceException` at `ExtensionEvalOnlyQualityRunner.cs:272`.

Why it matters: review question 4 requires null, unreadable, and malformed input to return `OperationResult` failures for expected input errors. A JSONL row with null arrays is malformed input, not an exceptional crash path.

Required fix: validate deserialized collection properties before any `.Count`, `.Select`, or `.Concat` use. Return `OperationResult` validation errors for null `layer_inputs`, null row-level `failure_codes`, null layer `evidence_refs`, and null layer `failure_codes`; add targeted tests proving these cases fail cleanly without writing reports.

### HIGH - Raw private text can leak through `failure_codes` while `raw_private_text_copied=false`

Exact lines:
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:348`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:359`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:382`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:383`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:390`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:410`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:459`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:470`
- `Tests/ExtensionEvalOnlyQualityRunnerTests.cs:232`
- `Tests/ExtensionEvalOnlyQualityRunnerTests.cs:255`

The privacy fix validates copied output refs, layer evidence refs, and reviewer notes, but does not validate row-level or layer-level `FailureCodes`. Those values are copied into JSON/JSONL `failure_codes`, included in pair results, and can be folded into `HumanReviewerNote` by `ExtensionQualityScoring.Score`. A sentinel probe with `failure_codes:["<sentinel-redacted>"]` succeeded and wrote the sentinel into report JSON, score JSONL, and the human reviewer note while `raw_private_text_copied` remained false.

Probe result:
- `dotnet run ... --input <temp-jsonl-with-failure_codes-sentinel> ...`: exited 0 and emitted `<sentinel-redacted>` in shared artifacts.

Why it matters: review question 6 asks for no raw private text in shared artifacts. The current tests prove raw private input refs are not emitted and reject sentinel text in output refs/evidence refs/reviewer notes, but they do not cover all emitted copied fields.

Required fix: apply raw-private marker validation to row-level and layer-level `failure_codes`, and preferably to every emitted input-derived string field. Add negative tests with sentinel text in row `FailureCodes` and layer `FailureCodes`, plus an output artifact scan proving the sentinel is absent before `RawPrivateTextCopied=false` is accepted.

## Review Questions

1. Are all 5 findings from code-review-pass-01 genuinely fixed? No. CR-01, CR-02, and CR-04 pass; CR-03 and CR-05 remain partially open.
2. Are public contract type names now runner-prefixed and consistent with accepted packet? Yes.
3. Are `prompt_sha256`, `mode`, and `local_private_eval_only/evidence_scope` enforced before scoring and represented in reports? Yes for scored `USER_ADAPTIVE_EVAL_ONLY` rows.
4. Do public methods return `OperationResult` failures for null/unreadable/malformed input rather than throwing expected errors? No. Semantically malformed JSONL with null collection fields can throw.
5. Is synthetic fixture evidence first-class and not blurred with approved eval execution? Yes.
6. Do privacy tests cover raw private input and copied output refs/evidence refs/reviewer notes? Partially. Those named paths are covered, but other emitted copied fields still leak.
7. Core boundary: validation/scoring/reporting in `43.SmartSkillsDomain`; `Program.cs` thin. Pass.
8. Hidden deterministic shortcut sweep: no deterministic helper is presented as semantic/trainable/product readiness authority. Pass.
9. Tests/checks: targeted tests/build pass; JSON/JSONL parse; `git diff --check`. Pass for existing tests/checks; additional review probes fail as above.
10. Product claims remain closed. Pass.

## Checks Run

- `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionEvalOnlyQualityRunner"`: PASS, 11 passed, 0 failed.
- `dotnet build 43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Quality02EvalOnlyRunner.csproj`: PASS, 0 warnings, 0 errors.
- JSON parse for relevant evidence JSON plus execution state: PASS.
- JSONL parse for synthetic input and generated score rows: PASS, 4 non-empty rows parsed.
- `git diff --check`: PASS.
- Malformed null-collection JSONL probe: FAIL, unhandled `NullReferenceException`.
- Failure-code privacy sentinel probe: FAIL, sentinel copied into shared artifacts.

## Boundary Review

Project standards: FAIL because expected malformed input can still throw instead of returning `OperationResult` failure.

Tests/Gate B: FAIL because targeted tests pass but miss semantic-malformed collection inputs and failure-code privacy leakage.

Core boundary: PASS. Validation, scoring, and reporting live in `43.SmartSkillsDomain`; `Program.cs` only parses CLI args and invokes the runner.

Thin adapters: PASS. No Claude, Cursor, or Codex host adapter logic was added.

Hidden deterministic shortcut sweep: PASS. `ExtensionQualityScoring.Score` remains explicitly labelled eval-only/helper/internal-detail/baseline and is not claimed as semantic, trainable, runtime, or product authority.

Synthetic fixture boundary: PASS. Current generated reports keep `synthetic_fixture_only=true` and `actual_approved_eval_executed=false`.

Product claims: PASS. Token savings, quality improvement, semantic quality/readiness, learnability, live host readiness, common readiness, strict QUALITY-03 readiness, training readiness, target promotion, locator update, and final readiness remain closed.

## Required Next Action

Send a bounded fix worker for the two findings above. It should update only the runner contracts/implementation, focused tests, regenerated evidence as needed, and any directly affected docs/state. Git authority should remain forbidden for the fix worker; parent should review, then handle commit/push only after a passing independent re-review.
