# QUALITY-02 Eval-only Runner Code Review Pass 01

Verdict: FAIL

Scope reviewed:
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunnerContracts.cs`
- `43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Quality02EvalOnlyRunner.csproj`
- `43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Program.cs`
- `Tests/ExtensionEvalOnlyQualityRunnerTests.cs`
- `docs/COMPONENT-INTERFACES.md`
- `docs/SUBPROJECT-MAP.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `artifacts/extension-runtime-token-quality-learnability/quality-02-eval-only-runner-implementation/quality-02-eval-only-runner-implementation-20260618-122518/`

Review basis loaded:
- `AGENTS.md`
- `CLAUDE.md`
- `docs/HOW-TO-USE-SKILLS.md`
- `docs/skills/generated-code-review-SKILL.md`
- `docs/skills/implementation-integrity-SKILL.md`
- `docs/skills/trainable-architecture-decision-SKILL.md`
- `docs/skills/test-integrity-SKILL.md`
- `docs/skills/verification-before-completion-SKILL.md`
- `docs/skills/output-readability-gate-SKILL.md`
- Accepted implementation packet under `artifacts/extension-runtime-token-quality-learnability/quality-02-eval-only-runner-implementation-packet/quality-02-eval-only-runner-implementation-packet-20260618-114340/`

## Findings

### HIGH - Public contract names drift from the accepted implementation packet

Exact lines:
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunnerContracts.cs:13`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunnerContracts.cs:38`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunnerContracts.cs:59`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunnerContracts.cs:107`
- `docs/COMPONENT-INTERFACES.md:814`

The accepted packet required runner-specific public records such as `ExtensionEvalOnlyQualityRunnerInputRow`, `ExtensionEvalOnlyQualityRunnerOutputRow`, `ExtensionEvalOnlyQualityRunnerReport`, `ExtensionEvalOnlyQualityRunnerValidation`, and `ExtensionEvalOnlyQualityRunnerClaimLocks`. The implementation exposes generic public names in the shared `SmartSkillsDomain` namespace: `InputRow`, `LayerInput`, `OutputRow`, `PairResult`, `Report`, `Validation`, and `ClaimLocks`; the docs also advertise `OperationResult<Report>` and `IReadOnlyList<InputRow>`.

Why it matters: this is a structured API surface for future runner and documentation consumers. Generic public names make the contract ambiguous, increase collision risk in `SmartSkillsDomain`, and do not match the accepted packet that downstream review was supposed to verify.

Required fix: rename the public records to the packet-approved runner-specific names, update runner methods/tests/docs/evidence schema names accordingly, and rerun the targeted test/build plus JSON/JSONL parse checks.

### HIGH - Required scorer-contract provenance fields are not enforced before scoring

Exact lines:
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:195`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:205`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:218`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunnerContracts.cs:24`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunnerContracts.cs:27`

`ValidateRunnableRow` requires approval, local root, raw/private and redacted refs, source/doc refs, host, method, baseline/adaptive refs, redaction status, and mutation attestations. It does not require `prompt_sha256` or `mode`, even though those fields exist on `InputRow` and the accepted scorer contract lists them as required future-runner input fields. The runner also has no first-class `evidence_scope=local_private_eval_only` field, despite the future-runner contract requiring that evidence label.

Why it matters: prompt hash and mode are part of provenance and leakage/no-memorization review. Without requiring them, the runner can score a row that cannot be tied back to the intended paired eval mode or prompt-provenance boundary. Missing `local_private_eval_only` scope also weakens the claim lock between synthetic/local eval evidence and product-readiness claims.

Required fix: add required validation for `prompt_sha256`, `mode`, and an explicit evidence-scope field, require `local_private_eval_only` for scored rows, update tests for missing values, and include the scope in JSON/JSONL/Markdown outputs.

### MEDIUM - Invalid public inputs can still throw instead of returning OperationResult failures

Exact lines:
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:54`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:60`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:96`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:109`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:113`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:121`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:145`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:149`

The public runner methods return `OperationResult<T>`, but they assume non-null `options`, `rows`, and `report`. `Run(null)`, `ValidateRows(null)`, or `WriteReportFiles(null, report)` will dereference before returning a failure result. `ReadInputRows` wraps malformed JSON and `IOException`, but not `UnauthorizedAccessException` on the read path, so unreadable input can escape as an exception even though the write path explicitly maps unauthorized access to a failure result.

Why it matters: project standards require business/input failures to be expressed as `OperationResult` failures, not exceptions or null paths. This is especially important for a CLI adapter that turns `OperationResult` failures into stable exit codes.

Required fix: add null guards for every public method parameter, map unreadable input paths to a failure result, and add targeted tests for null options/rows/report and unauthorized read behavior where practical.

### MEDIUM - Synthetic fixture evidence is not first-class in the generated score report

Exact lines:
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunnerContracts.cs:85`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:250`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:253`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:321`
- `artifacts/extension-runtime-token-quality-learnability/quality-02-eval-only-runner-implementation/quality-02-eval-only-runner-implementation-20260618-122518/quality-02-eval-only-runner-report.json:131`
- `artifacts/extension-runtime-token-quality-learnability/quality-02-eval-only-runner-implementation/quality-02-eval-only-runner-implementation-20260618-122518/quality-02-eval-only-runner-report.json:293`
- `artifacts/extension-runtime-token-quality-learnability/quality-02-eval-only-runner-implementation/quality-02-eval-only-runner-implementation-20260618-122518/quality-02-eval-only-runner-report.json:329`

The implementation evidence correctly says the run used a synthetic fixture and did not execute approved private eval, but the runner-generated report itself sets `no_eval_execution=false` on score rows and `eval_runner_executed=true` at report level without a matching `synthetic_fixture_only`, `actual_approved_eval_executed=false`, or `evidence_scope=local_private_eval_only` machine field. Its `what_this_proves` text says the runner can validate approved eval-only rows, but the emitted evidence file was produced from synthetic rows.

Why it matters: downstream automation reading only the runner report can confuse synthetic fixture scoring with an approved eval execution. The surrounding implementation report is clearer, but the source-of-truth runner output should carry its own evidence boundary.

Required fix: replace or supplement `NoEvalExecution` with explicit fields such as `SyntheticFixtureOnly`, `ActualApprovedEvalExecuted`, and `EvidenceScope`; set them from runner options/input; update Markdown and JSON outputs; add tests that synthetic fixture reports cannot be mistaken for approved eval execution.

### MEDIUM - Privacy coverage only proves one non-emitted field, not all copied fields

Exact lines:
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:309`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:310`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:311`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:313`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:390`
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:391`
- `Tests/ExtensionEvalOnlyQualityRunnerTests.cs:172`
- `Tests/ExtensionEvalOnlyQualityRunnerTests.cs:184`

The privacy test places a secret only in `RawPrivateInputRef`, which the runner does not emit. It does not cover copied fields such as `BaselineOutputRef`, `AdaptiveOutputRef`, layer `EvidenceRefs`, layer `ReviewerNote`, or output `HumanReviewerNote`. Those fields are copied directly into JSON/JSONL/Markdown outputs.

Why it matters: the work order requires no raw private text copied into shared/common evidence. A test that only checks a non-emitted field can pass while emitted refs or reviewer notes contain raw private text.

Required fix: validate copied output fields as redacted refs or approved non-private notes where possible, add negative tests with a sentinel secret in copied fields, and keep `RawPrivateTextCopied=false` only when the emitted artifact scan proves the sentinel is absent from all generated outputs.

## Checks Run

- `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionEvalOnlyQualityRunner"`: PASS, 8 passed, 0 failed.
- `dotnet build 43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Quality02EvalOnlyRunner.csproj`: PASS, 0 warnings, 0 errors.

No broad test run was performed.

## Boundary Review

Project standards: FAIL because `OperationResult<T>` is present but invalid public inputs and unreadable input can still throw instead of returning failure results.

Tests/Gate B: FAIL because targeted tests pass but miss required provenance validation, synthetic-vs-approved eval scope, null/error paths, and copied-field privacy coverage.

Core boundary: PASS. Scoring/validation logic lives in `43.SmartSkillsDomain`; `Program.cs` is a thin argument/exit-code adapter.

Thin adapters: PASS. No Claude, Cursor, or Codex host adapter logic or runtime claim was added.

Hidden deterministic shortcut sweep: PASS. Found deterministic rubric scoring via `ExtensionQualityScoring.Score`, allowed host/method/privacy sets, and score thresholds; no hidden trainable/semantic authority shortcut found.

Deterministic helper labels: PASS. Rubric scoring is labelled eval-only/helper/internal-detail/baseline and not semantic/trainable/runtime/product authority.

Docs/QA/dashboard impact: PARTIAL. Dashboard N/A is justified, and docs describe the runner boundary, but docs currently publish the generic public record names from the drift finding.

Claim locks: PARTIAL. Product/readiness booleans remain false, but the runner-generated report needs first-class synthetic-vs-approved eval scope fields.

Evidence artifact quality: PARTIAL. Existing JSON/JSONL/Markdown artifacts parse and are readable, but the runner report has the scope ambiguity above.

Source/privacy: FAIL until copied-field privacy validation/tests cover all emitted fields, not only `RawPrivateInputRef`.

## Required Fix Sub-agent

Required. The next bounded fix sub-agent should update only the runner contracts, runner implementation, focused tests, affected docs, and regenerated evidence artifacts for the findings above. Git authority should remain forbidden for the fix worker; parent should own commit/push after review.
