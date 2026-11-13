# S2-implementation-02 thin-client job runtime evidence

## Return packet

claim_scope: In-process thin-client job adapter works with deterministic/injected core service under focused tests only.

evidence_scope: Implemented and focused-tested `DesignConversionThinClientJobRuntime` against `DesignConversionInternalService` through an injected evaluator wrapper. Evidence is limited to submit/status/explicit-run/failure/idempotent terminal-status behavior plus existing thin-client contract and internal-service tests.

count_claimed: 1 focused final test command, 1 requested diff-check command, 1 requested git status command.

independent_evidence_count: 3 command records: focused test, diff-check, git status. A targeted status readback was also used only to isolate the task-owned lines from the already dirty worktree.

subagent_id_or_NONE: NONE.

downgrade_needed: yes. Evidence supports only the narrow in-process adapter claim. It does not prove public API, FLOW-47, platform integration, persistence, browser/e2e, deployment, trained quality, broad suite green, runtime readiness, product readiness, or S2 completion.

## Changed files

- `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs`
- `Tests/DesignConversionThinClientJobRuntimeTests.cs`
- `docs/evidence/design-conversion/S2-implementation-02-thin-client-job-runtime-2026-06-29.md`

No existing contract/test file was edited for compilation. No csproj/sln, DI/service collection, orchestrator, model server, platform/flows controller, Figma/Canva client, persistent DB, browser/e2e, deployment, AGENTS.md, docs/skills, xiigen, or artifacts files were intentionally modified by this task.

## Implementation summary

- Added `IDesignConversionThinClientJobEvaluator` and `DesignConversionInternalServiceThinClientJobEvaluator` as the injectable in-process bridge to the existing `DesignConversionInternalService`.
- Added `DesignConversionThinClientJobRuntime` with in-memory dictionaries for submitted requests, jobs, and status results.
- `Submit` validates the existing `SubmittedDesignConversionRequest`, creates a deterministic `DesignConversionThinClientJob`, stores submitted status, and does not run the evaluator.
- `GetStatus` returns the stored `DesignConversionThinClientStatusResult` for submitted/running/completed/failed/cancelled jobs.
- `RunAsync` explicitly maps the submitted request to `DesignConversionInternalEvaluateRequest`, derives render resolutions from submitted render/screenshot artifact dimensions, calls the injected evaluator, and stores completed/failed/cancelled status.
- Completed status exposes memory-only artifact refs for improved HTML, improved CSS, and runtime-flow output metadata; it does not write persistent artifacts.
- Runtime status warnings/provenance carry boundary literals that keep the implementation scoped to in-process deterministic/internal service execution only.

## Focused test command/results

Command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionThinClientJobRuntimeTests|FullyQualifiedName~ThinClientApiContractTests|FullyQualifiedName~DesignConversionInternalServiceTests" --no-restore --logger "console;verbosity=minimal"
```

Final result:

```text
Passed!  - Failed:     0, Passed:    13, Skipped:     0, Total:    13, Duration: 164 ms - LlmFromScratch.Tests.dll (net9.0)
```

Notes:

- A prior focused run failed 1 new assertion because the test expected `False` while the implementation intentionally emitted lower-case `false`; the test was corrected and the exact focused command was rerun successfully.
- The passing final command included `DesignConversionThinClientJobRuntimeTests`, `ThinClientApiContractTests`, and `DesignConversionInternalServiceTests` under the requested filter.

## Diff-check/status

Command:

```powershell
git diff --check -- 44.DesignConversionDomain Tests docs/evidence
```

Result:

```text
exit_code=0
output=<no output>
```

Command:

```powershell
git status --short
```

Result:

```text
exit_code=0
```

Status scope note: full `git status --short` output was large and included many pre-existing unrelated modified/deleted/untracked files in forbidden and unrelated areas. Task-owned status lines isolated by targeted readback:

```text
?? 44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs
?? Tests/DesignConversionThinClientJobRuntimeTests.cs
?? docs/evidence/design-conversion/S2-implementation-02-thin-client-job-runtime-2026-06-29.md
```

## Unsupported / not run

- broad_unfiltered_tests_project_verified=false
- public_api_verified=false
- flow47_verified=false
- platform_integration_verified=false
- persistence_verified=false
- product_ready_verified=false
- runtime_ready_verified=false
- trained_feature_verified=false
- model_training_verified=false
- browser_e2e_verified=false
- visual_fidelity_verified=false
- deployment_verified=false
- host_readiness_verified=false

## Next implementable blocker

Do not start here: the next implementable blocker is wiring an approved public/API or platform route only after a separate authorization and boundary-specific plan, because this packet proves only the in-process adapter over the deterministic/internal core service path.
