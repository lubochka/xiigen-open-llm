# Pass129 ModelServer Improvement Loop Surface

claim_scope:
- Implemented one bounded local Pass129 surface: an in-process ModelServer improvement-loop route/handler plus local caller adapter envelope for the Pass128 `DesignConversionImprovementLoopCoordinator`.
- This proves only optional in-repo/local ModelServer access to the loop through refs-only response telemetry.
- It does not prove public HTTP hosting, socket hosting, ASP.NET/minimal API startup, external caller repos, real external visual AI/provider calls, production DB, upload/deployment, trained weights, or full product readiness.

evidence_scope:
- Current workspace: `C:\Projects\llm_mvp_core`.
- Direct evidence: code changes in `25.ModelServer/DesignConversion/*.cs`, focused tests in `Tests/DesignConversionModelServerImprovementLoopTests.cs`, and dotnet test output listed below.
- Evidence-only sources read: Pass127 gap review and Pass128 coordinator artifact under `docs/evidence/design-conversion`.
- No external `C:\Users\lubao\Documents\xiigen` writes were made.

count_claimed:
- 1 bounded local implementation pass: Pass129 only.
- 0 public-host passes.
- 0 external-caller passes.
- 0 real-provider passes.
- 0 product-readiness passes.

independent_evidence_count:
- 1 Pass129 evidence artifact: this file.
- 2 successful behavioral test commands after repair: focused Pass129 tests and adjacent ModelServer regression tests.
- Failed/empty earlier commands are recorded as recovery evidence, not as passes.

subagent_id_or_NONE:
- UNKNOWN_IN_SUBAGENT

subagent_identity_self_check:
- spawned_worker=true; used_NONE=false; parent_must_reconcile=true

downgrade_needed:
- No downgrade for the narrow local route/adapter claim after the listed tests passed.
- Downgrade remains required for any public HTTP, external caller, external provider, deployment, trained model, or full product-readiness claim.

skill_load_status:
- AGENTS.md: found/read.
- docs/SESSION-START-PROMPT.md: found/read.
- docs/skills/planning-session-startup-SKILL.md: found/read.
- docs/skills/architect-behavior-classifier-SKILL.md: found/read.
- docs/skills/authority-requirement-binding-SKILL.md: found/read.
- docs/skills/reference-inventory-and-source-map-SKILL.md: found/read.
- docs/skills/phase-planning-SKILL.md: found/read.
- docs/skills/codex-planning-quality-GUIDE.md: found/read.
- docs/skills/plan-review-SKILL.md: found/read.
- docs/skills/goal-delivery-completeness-SKILL.md: found/read.
- docs/skills/claim-verification-SKILL.md: found/read.
- docs/skills/audit-protocol-SKILL.md: found/read.
- docs/skills/specificity-calibration-SKILL.md: found/read.
- docs/skills/output-skepticism-SKILL.md: found/read.
- docs/skills/reconnaissance-gate-SKILL.md: found/read.
- docs/skills/trainable-architecture-decision-SKILL.md: found/read.
- docs/skills/no-product-decisions-SKILL.md: found/read.
- docs/skills/output-readability-gate-SKILL.md: found/read.
- docs/skills/design-conversion-ilearningmachine-architecture-GUIDE.md: found/read.
- docs/skills/qa-session-type-SKILL.md: found/read.
- docs/skills/html-css-render-qa-automation-GUIDE.md: found/read.
- docs/skills/generated-code-review-SKILL.md: found/read.

files_read:
- `AGENTS.md`
- `docs/SESSION-START-PROMPT.md`
- Required `docs/skills/*` stack listed above.
- `docs/evidence/design-conversion/codex-pass127-current-gap-review-next-fix.md`
- `docs/evidence/design-conversion/codex-pass128-improvement-loop-coordinator.md`
- `44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs`
- `44.DesignConversionDomain/DesignConversionContracts.cs`
- `44.DesignConversionDomain/DesignConversionInternalService.cs`
- `44.DesignConversionDomain/DesignConversionVisualQaBoundaryPolicy.cs`
- `44.DesignConversionDomain/DesignConversionVisualJudge.cs`
- `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerHttpHandler.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerEndpointRegistry.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerLocalCallerAdapter.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerCallerContract.cs`
- `Tests/DesignConversionModelServerApiTests.cs`
- `Tests/DesignConversionModelServerLocalCallerAdapterTests.cs`
- `Tests/DesignConversionModelServerCallerContractTests.cs`
- `Tests/DesignConversionImprovementLoopCoordinatorTests.cs`

files_changed:
- `25.ModelServer/DesignConversion/DesignConversionModelServerImprovementLoopHandler.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerEndpointRegistry.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerLocalCallerAdapter.cs`
- `Tests/DesignConversionModelServerImprovementLoopTests.cs`
- `docs/evidence/design-conversion/codex-pass129-modelserver-improvement-loop-surface.md`

commands_run:
- `rg` over memory registry for Pass128/Pass129/DesignConversion/ModelServer: no relevant hits used.
- Required skill/context `Get-Content` and `rg` reads before coding.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionModelServerImprovementLoopTests" --no-restore --logger "console;verbosity=minimal"`: first failed to compile due readonly dictionary assignment in the new handler; repaired.
- Same focused command: second failed to compile due test fixture passing raw candidate instead of `OperationResult`; repaired.
- Same focused command: later compiled but returned "No test is available" for that filter, so it was not counted as a test pass.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --list-tests --logger "console;verbosity=minimal"` and `Select-String -Pattern "ImprovementLoop"`: discovered the new Pass129 tests.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "DisplayName~DesignConversionModelServerImprovementLoopTests" --logger "console;verbosity=minimal"`: first behavioral run failed one helper expectation; repaired.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "DisplayName~DesignConversionModelServerImprovementLoopTests" --logger "console;verbosity=minimal"`: passed, 4/4.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "DisplayName~DesignConversionModelServerApiTests|DisplayName~DesignConversionModelServerLocalCallerAdapterTests|DisplayName~DesignConversionModelServerCallerContractTests" --logger "console;verbosity=minimal"`: passed, 26/26.

tests_run:
- Passed: 4/4 `DesignConversionModelServerImprovementLoopTests`.
- Passed: 26/26 adjacent ModelServer API/local-caller/caller-contract tests.
- Earlier compile/filter/helper failures were repaired and are not counted as passing test evidence.

implementation_summary:
- Added `DesignConversionModelServerImprovementLoopHandler` for `POST /api/design-conversion/improvement-loop`.
- The handler maps the local caller package into an internal `DesignConversionImprovementLoopRequest`, invokes an injected optional `DesignConversionImprovementLoopCoordinator`, and returns a refs-only response that excludes `FinalEvaluation`, raw HTML, raw CSS, raw design tree payloads, screenshot bytes, and provider payloads.
- Extended `DesignConversionModelServerEndpointRegistry` with optional improvement-loop route registration while preserving the existing evaluate route.
- Extended `DesignConversionModelServerLocalCallerAdapter` with `TryRunImprovementLoopAsync` and route-specific stable envelope metadata for the improvement-loop path.
- Added focused tests for refs-only success, provider-required bounded non-ready response, missing route, and coordinator-not-configured error envelopes.

mental_simulation_results:
- Local caller invokes coordinator success path: covered by `TryRunImprovementLoopAsync_RegisteredRoute_ReturnsRefsOnlySuccessEnvelope`; response included stop reason, attempt count, refs-only metadata, and no private marker/raw input strings.
- Provider-required blocked path: covered by `TryRunImprovementLoopAsync_ProviderRequired_ReturnsBoundedNonReadyEnvelopeWithoutExternalProviderClaim`; response stayed bounded non-ready with external-provider flags false.
- Route/method missing or coordinator not configured: covered by route-missing and coordinator-not-configured tests; both returned stable error envelopes without crashes or readiness overclaim.
- Boundary metadata: tests assert `socket_opened=false`, `public_http_endpoint_verified=false`, `cross_repo_caller_verified=false`, `external_provider_called=false`, and `product_ready=false`.
- Existing submit/evaluate route behavior: adjacent ModelServer tests passed after registry/adapter edits.

verification_results:
- Focused Pass129 tests passed: 4 passed, 0 failed.
- Adjacent directly touched ModelServer tests passed: 26 passed, 0 failed.
- Conflict-marker scan over changed files: no matches.
- Mojibake marker scan over changed files using the configured marker set from the task: no matches.
- Changed human-readable readback was inspected for the new route/error/boundary strings and this artifact packet fields; no visible mojibake was present in the readback.
- `git diff --check -- <changed files>` exited 0.
- Trailing whitespace scan over changed files: no matches.

remaining_gaps:
- `AspNetHostVerified=false`
- `SocketOpened=false`
- `PublicHttpEndpointVerified=false`
- `CrossRepoCallerVerified=false`
- `ExternalProviderCalled=false`
- `ProductReady=false`
- No public/socket host was started.
- No external caller repo was edited.
- No real external visual AI/network/provider call was made.
- No production DB/upload/deployment was attempted.
- No full product readiness claim is made.

next_single_pass_recommendation:
- Pass130 should either add a local contract descriptor/schema test for the improvement-loop route if Luba wants caller contract documentation, or proceed to the next explicitly authorized design-conversion integration gap. Keep it local-only unless Luba explicitly authorizes host or external caller work.
