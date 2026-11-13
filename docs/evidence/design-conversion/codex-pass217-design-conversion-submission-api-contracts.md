# Pass217 DesignConversion Submission/API Contracts Evidence

Date: 2026-07-04

## Scope

Pass217 targeted direct DesignConversion submission, API, model-server caller,
thin-client job, status, and artifact contracts in `C:\Projects\llm_mvp_core`.
The user-facing requirement checked here is: a Figma/Canva-like user can submit
generated HTML and CSS plus original/source tree payloads and screenshot/design
render artifacts to the local server/API contract, then read job status and
artifact references back. This is a bounded local contract and regression slice;
it is not a product-complete, public endpoint, deployment, provider-call, full
CI, or 50/50 claim.

## Claim Guard

claim_scope: Pass217 bounded DesignConversion submission/API/thin-client/model-server caller contract slice only.
evidence_scope: required skill-load read sweep, Pass216 artifact read, requested test file reads, scoped production symbol search, relevant production file reads, baseline segment test, test-only assertion hardening, post-fix segment test, full DesignConversion regression, and whitespace/diff/readback checks.
count_claimed: 1
independent_evidence_count: 11
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
spawned_worker_identity_self_check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true
nested_subagent_usage: none
downgrade_needed: no for this bounded slice; yes if someone tries to restate it as ProductReady, public API, provider execution, full CI, or 50/50 completion.

## Skill Load Status

The successful skill-load sweep read each required file with `Get-Content -Raw`
and reported path, character count, SHA-256 prefix, and first heading. A prior
attempt at the sweep had a PowerShell parser error and is not used as load
evidence.

| Group | Files | Status |
|---|---|---|
| AGENTS/session | `AGENTS.md`, `docs/SESSION-START-PROMPT.md`, `docs/skills/planning-session-startup-SKILL.md` | found; read |
| SK-INTENT | `architect-behavior-classifier-SKILL.md` | found; read |
| SK-PLAN | `authority-requirement-binding-SKILL.md`, `reference-inventory-and-source-map-SKILL.md`, `phase-planning-SKILL.md` | found; read |
| SK-REVIEW | `plan-review-SKILL.md`, `goal-delivery-completeness-SKILL.md`, `claim-verification-SKILL.md`, `audit-protocol-SKILL.md`, `specificity-calibration-SKILL.md`, `output-skepticism-SKILL.md`, `reconnaissance-gate-SKILL.md` | found; read |
| SK-ARCH | `trainable-architecture-decision-SKILL.md`, `no-product-decisions-SKILL.md`, `output-readability-gate-SKILL.md`, `human-readable-architecture-audit-SKILL.md` | found; read |
| QA/code review | `generated-code-review-SKILL.md`, `code-examination-SKILL.md`, `implementation-integrity-SKILL.md`, `test-integrity-SKILL.md`, `integration-testing-SKILL.md`, `e2e-test-matrix-SKILL.md`, `ui-testing-SKILL.md`, `ui-quality-standard-SKILL.md` | found; read |

## Files Read

- `docs/evidence/design-conversion/codex-pass216-projectwiki-coverage-verifier-repair.md`
- `Tests/DesignConversionDomainTests.cs`
- `Tests/ThinClientApiContractTests.cs`
- `Tests/DesignConversionThinClientJobRuntimeTests.cs`
- `Tests/DesignConversionModelServerCallerContractTests.cs`
- `44.DesignConversionDomain/DesignConversionContracts.cs`
- `44.DesignConversionDomain/DesignConversionQaContracts.cs`
- `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs`
- `44.DesignConversionDomain/DesignConversionInternalService.cs`
- `44.DesignConversionDomain/DesignConversionRoutingFlow.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerCallerContract.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerHttpHandler.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerEndpointRegistry.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerStatusHandler.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerLocalCallerAdapter.cs`

The first full-repo `rg` command produced a very large, truncated output because
it also matched generated/evidence text. A narrowed production/test `rg` command
was then used for the inspectable contract surface.

## Contract Coverage Checked

| Requirement | Evidence |
|---|---|
| Submitted request carries generated HTML and CSS | `SubmittedDesignConversionRequest` has `Html` and `Css`; domain, thin-client, runtime, and model-server tests now assert those values. |
| Request carries current/source tree and original tree | `SubmittedDesignConversionRequest` has `DesignTreePayload` and `OriginalTreePayload`; API request has `designTree` and `originalTree`; validation rejects missing source/original tree. |
| Request carries screenshot/design render artifacts | `Screenshots` and `DesignRenders` exist in domain and API contracts; validation requires at least one screenshot or design render with a positive viewport. |
| Request carries budget and tier | `DesignConversionBudgetMetadata` and `DesignConversionModelServerBudget` carry tier and caps; thin-client round-trip now asserts `Budget.Tier` and `MaxRenderQaRuns`. |
| Thin client stores job and exposes status/artifacts | `DesignConversionThinClientJobRuntime.Submit`, `GetStatus`, and `RunAsync` store submitted jobs and return submitted/completed/failed statuses plus artifact refs. |
| Model-server caller contract documents submit shape | `DesignConversionModelServerCallerContract.Describe()` documents method, endpoint, request fields, validation failures, response fields, and boundary metadata. |
| Server/API returns status and artifact readback | `DesignConversionModelServerStatusHandler` exposes local GET status readback with refs-only artifact response and no rerun. |
| No hidden trainable/provider/public endpoint claim | Boundary metadata and warnings stay local-only: public HTTP, deployment, product readiness, cross-repo caller, external provider, and raw response echo flags remain false. |

## Fix Applied

Root cause was a test-evidence gap, not a production contract gap. The runtime
and API contracts already carried the required fields. The narrow fix was to add
missing explicit assertions:

- `Tests/ThinClientApiContractTests.cs`: the thin-client submission round-trip
  now asserts deserialized `Html`, `Css`, `Budget.Tier`, and `Budget.MaxRenderQaRuns`.
- `Tests/DesignConversionModelServerCallerContractTests.cs`: the model-server
  caller descriptor test now explicitly asserts `css` and optional `budget`
  request field descriptors.

No production file was changed.

## Verification

Baseline bounded segment before the test hardening:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionDomainTests|FullyQualifiedName~ThinClientApiContractTests|FullyQualifiedName~DesignConversionThinClientJobRuntimeTests|FullyQualifiedName~DesignConversionModelServerCallerContractTests" --logger "console;verbosity=minimal"
```

Result: exit code 0. Passed 23/23, failed 0, skipped 0.

Post-fix bounded segment:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionDomainTests|FullyQualifiedName~ThinClientApiContractTests|FullyQualifiedName~DesignConversionThinClientJobRuntimeTests|FullyQualifiedName~DesignConversionModelServerCallerContractTests" --logger "console;verbosity=minimal"
```

Result: exit code 0. Passed 23/23, failed 0, skipped 0. The build printed
existing analyzer/nullability warnings in unrelated test files; no test failed.

Full DesignConversion regression:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~DesignConversion" --logger "console;verbosity=minimal"
```

Result: exit code 0. Passed 247/247, failed 0, skipped 0.

Whitespace check over changed Pass217 files:

```powershell
git diff --check -- Tests/ThinClientApiContractTests.cs Tests/DesignConversionModelServerCallerContractTests.cs docs/evidence/design-conversion/codex-pass217-design-conversion-submission-api-contracts.md
```

Result: exit code 0.

Readback confirmed the new test assertions:

- `Tests/ThinClientApiContractTests.cs`: `roundTrip.Html`, `roundTrip.Css`,
  `roundTrip.Budget.Tier`, and `roundTrip.Budget.MaxRenderQaRuns`.
- `Tests/DesignConversionModelServerCallerContractTests.cs`: `JsonName: "css"`
  and `JsonName: "budget"` descriptor assertions.

## Mental Simulation

A Figma or Canva-like caller prepares a generated HTML/CSS candidate, a current
design/source tree, the original tree, screenshot or design-render refs with
positive viewports, and budget/tier metadata. The local model-server caller
contract serializes that request to `POST /api/design-conversion/evaluate`.
The API validates source tree, original tree, HTML, CSS, and render evidence,
maps it into `SubmittedDesignConversionRequest`, and the thin-client runtime
stores a submitted job. `RunAsync` maps the stored submission into the internal
service with HTML, CSS, both tree payloads, render artifacts, budget, and
provenance, then returns completed/failed status with refs-only artifacts. The
status route reads the job result back without rerunning it and returns artifact
refs plus boundary metadata. The path supports later QA/render/DPO loops through
refs, hashes, status, warnings, and local artifacts, while preserving the
honesty boundary that no public deployment, external provider, production
readiness, or hidden trainable authority was verified by this pass.

## Files Changed

- `Tests/ThinClientApiContractTests.cs`
- `Tests/DesignConversionModelServerCallerContractTests.cs`
- `docs/evidence/design-conversion/codex-pass217-design-conversion-submission-api-contracts.md`

## Stale Process Handling

No stale process cleanup was needed.

## Verdict

pass217_verdict: PASS_TEST_EVIDENCE_HARDENED_NO_PRODUCTION_FIX

## Next Recommended Pass

Continue to the next adjacent design-to-code completion slice selected by the
parent chain. A useful next slice would be another direct contract edge around
status/readback, local caller envelopes, or improvement-loop API behavior.
