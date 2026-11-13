# Codex Pass169 - Real Local FLOW to llm_mvp_core ModelServer Harness

## Honesty Header

claim_scope: Pass169 added and verified a local same-machine FLOW intake harness that invokes real llm_mvp_core ModelServer/DesignConversion composition for the explicit browser-backed promoted-local mode.
evidence_scope: Evidence covers safe FLOW worktree test code, a small llm_mvp_core ModelServer metadata seam, focused SDK10/SDK9 test runs, source-repo status checks, changed-file hygiene scans, and non-destructive FLOW patch mergeability checks.
count_claimed: 1
independent_evidence_count: 7
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: Yes. The evidence supports the allowed minimum FLOW-to-real-ModelServer runtime proof. It does not prove a full platform-to-FLOW-to-ModelServer all-up route, cloud deploy, provider calls, public endpoint, production browser farm, ProductReady=true, or actual promoted checkpoint load through FLOW.

## Skill Load Status

- SK-SESSION: found/read `C:\Projects\llm_mvp_core\AGENTS.md`, `C:\Projects\llm_mvp_core\docs\SESSION-START-PROMPT.md`, `C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md`.
- SK-INTENT: found/read `architect-behavior-classifier-SKILL.md`.
- SK-PLAN: found/read `authority-requirement-binding-SKILL.md`, `reference-inventory-and-source-map-SKILL.md`, `phase-planning-SKILL.md`, `codex-planning-quality-GUIDE.md`.
- SK-REVIEW: found/read `plan-review-SKILL.md`, `goal-delivery-completeness-SKILL.md`, `claim-verification-SKILL.md`, `audit-protocol-SKILL.md`, `specificity-calibration-SKILL.md`, `output-skepticism-SKILL.md`, `reconnaissance-gate-SKILL.md`.
- SK-ARCH: found/read `trainable-architecture-decision-SKILL.md`, `no-product-decisions-SKILL.md`, `output-readability-gate-SKILL.md`.
- SK-QA/CODE: found/read `qa-session-type-SKILL.md`, `generated-code-review-SKILL.md`, `test-integrity-SKILL.md`, `e2e-test-matrix-SKILL.md`, `implementation-integrity-SKILL.md`, `verification-before-completion-SKILL.md`, `bug-to-tests-SKILL.md`, `test-failure-triage-SKILL.md`, `self-verification-SKILL.md`, `agent-output-format-SKILL.md`.

## Files Read

- `C:\Projects\llm_mvp_core\AGENTS.md`
- `C:\Projects\llm_mvp_core\docs\SESSION-START-PROMPT.md`
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass168-platform-flow-browser-promoted-composition.md`
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass164R-loop-training-promotion.md`
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass166-browser-backed-render-qa.md`
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass166R-browser-marker-reconcile.md`
- Three Claude DesignToCode files under `C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\`, read-only.
- Current FLOW intake/client/test files under the safe FLOW worktree.
- Current llm_mvp_core ModelServer and DesignConversion local composition files.

## Files Changed

- `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\Xiigen.Flow.DesignConversionIntake.Tests.csproj`
  - Added a safe-worktree test ProjectReference to `C:\Projects\llm_mvp_core\25.ModelServer\ModelServer.csproj`.
- `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\DesignConversionIntakeServiceTests.cs`
  - Added `ExplicitBrowserBackedPromotedLocalCompositionInvokesRealModelServerViaFlow`.
  - Added test-only `RealModelServerHarnessCoreClient`.
- `C:\Projects\llm_mvp_core\25.ModelServer\DesignConversion\DesignConversionModelServerApi.cs`
  - Surfaced browser-backed scaffold markers from the real internal evaluation result into ModelServer boundary metadata.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass169-real-platform-flow-modelserver-harness.md`
  - This evidence packet.

## Real Runtime Trace

1. The focused FLOW test submits a platform-shaped Figma payload with explicit local browser-backed promoted-local composition flags and trace id `pass169-real-modelserver-route-trace`.
2. FLOW `DesignConversionIntakeService.SubmitJobAsync` applies the explicit local composition contract and calls its injected core client.
3. The Pass169 test-only core client maps the FLOW request through `RealDesignConversionCoreClient.MapToInternalRequest`.
4. The test-only core client calls real `ModelServer.DesignConversion.DesignConversionModelServerApi.SubmitAndEvaluateAsync`.
5. The real API runs `DesignConversionThinClientJobRuntime` plus `DesignConversionInternalService` with `BrowserBackedDesignConversionRenderProvider`, `LocalChromiumBrowserProcessAdapter`, `DeterministicDesignConversionComparisonProvider`, and `DefaultLocalDesignConversionVisualJudge`.
6. The test asserts real returned markers, including `real_llm_mvp_core_modelserver_invoked=True`, `runtime_surface=in_process_model_server_design_conversion_api`, `browser_render_executed=True`, `browser_renderer_explicit_local_injection_proven=True`, `service_scaffold_browser_renderer_explicit_local_injection_proven=True`, `browser_renderer_injected_provider_kind=browser_process_render_provider`, `promoted_local_model_required_or_used=True`, `cloud_model_upload_attempted=False`, `deployment_verified=False`, and `product_ready=False`.
7. The test also asserts the honest promoted-checkpoint gap: `promotion_status=promotion_not_run`, `local_model_promotion_verified=False`, `local_promoted_checkpoint_loaded=False`, and `training_artifact_readback_not_configured`.

## Commands Run

- Source repo before status:
  - `git -C C:\Projects\xiigen-platform-dot-net-angular status --short --branch` returned `## UpdateUniversalSkills...origin/UpdateUniversalSkills` and pre-existing `?? evidence/`.
  - `git -C C:\Projects\xiigen-flows-dot-net-angular status --short --branch` returned `## UpdateUniversalSkills...origin/UpdateUniversalSkills`.
- Focused FLOW test:
  - `C:\Projects\dotnet-sdk-10\dotnet.exe test ...Xiigen.Flow.DesignConversionIntake.Tests.csproj --filter FullyQualifiedName~ExplicitBrowserBackedPromotedLocalCompositionInvokesRealModelServerViaFlow ... -p:NuGetAudit=false`
  - Result: passed, 1 total, 0 failed.
- Direct llm_mvp_core regression attempt with SDK10:
  - Built, then testhost aborted because the SDK10 folder did not include `Microsoft.NETCore.App` 9.0.0.
- Direct llm_mvp_core regression with machine-wide SDK9:
  - `C:\Program Files\dotnet\dotnet.exe test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionModelServerApiTests.SubmitAndEvaluateAsync_ValidRequest_CallsRuntimeServiceAndReturnsEvaluationEvidence|FullyQualifiedName~DesignConversionModelServerApiTests.SubmitAndEvaluateAsync_BoundariesDoNotClaimDeploymentUploadOrFullReadiness ... -p:NuGetAudit=false`
  - Result: passed, 2 total, 0 failed.
- Source repo after status:
  - `git -C C:\Projects\xiigen-platform-dot-net-angular status --short --branch` returned the same pre-existing `?? evidence/` state.
  - `git -C C:\Projects\xiigen-flows-dot-net-angular status --short --branch` returned the same clean branch status.
- Hygiene:
  - FLOW changed files `git diff --check`: clean.
  - ModelServer untracked-file `git diff --no-index --check` against an empty temp file: no whitespace errors.
  - Changed-file conflict-marker scan: no line-start conflict markers.
  - Changed-file mojibake marker scan: no configured mojibake markers.
- Mergeability:
  - Exported the safe FLOW two-file patch to `C:\Projects\design-to-code-worktrees\pass169-flow-modelserver-harness-20260703-132806.patch`.
  - Temporary clone `C:\Projects\design-to-code-worktrees\pass169-flow-mergecheck-20260703-132806`.
  - Initial temp-clone `origin/main` check was invalid for current main authority: the temp clone resolved `origin/main` to `fd2e3bfd843940d4341a9b86a0503cb382ac8122`, while the safe worktree's actual `origin/main` resolved to `6579ffb12dfb8cd7822fa7e1561fd03980df2fea`.
- Pass169 recovery verification:
  - Reran exact FLOW test name `ExplicitBrowserBackedPromotedLocalCompositionInvokesRealModelServerViaFlow` with SDK10 and command-scoped `-p:NuGetAudit=false`: passed, 1 total, 0 failed.
  - Reran full `DesignConversionIntakeServiceTests` class with SDK10 and command-scoped `-p:NuGetAudit=false`: passed, 25 total, 0 failed.
  - Verified safe worktree tree state: `origin/main`, `origin/UpdateUniversalSkills`, and `origin/DesignToCode` all contain `flows/FLOW-47-design-conversion-intake/server/Xiigen.Flow.DesignConversionIntake.Tests/DesignConversionIntakeServiceTests.cs`.
  - Exported corrected safe FLOW two-file patch to `C:\Projects\design-to-code-worktrees\pass169-recovery-flow-modelserver-harness-20260703-133929.patch`.
  - Temporary clone `C:\Projects\design-to-code-worktrees\pass169-recovery-flow-mergecheck-20260703-133929`.
  - Corrected exact-hash `git apply --3way --check`: OK on `origin/main` at `6579ffb12dfb8cd7822fa7e1561fd03980df2fea`, OK on `origin/UpdateUniversalSkills` at `3fbc2b09c3406fd5368aa464f2036f4ce33dc47a`, and OK on `origin/DesignToCode` at `5e6836e7328cd896c686ae083e8014b8b94db09b`.

## Source Repo Write Status

- `C:\Projects\xiigen-platform-dot-net-angular`: not written by Pass169. Before and after status both showed the same branch and pre-existing untracked `evidence/`.
- `C:\Projects\xiigen-flows-dot-net-angular`: not written by Pass169. Before and after status both showed the same branch with no short-status changes.

## Verification Results

- PASS: FLOW safe-worktree harness invokes real llm_mvp_core ModelServer/DesignConversion API through the FLOW intake path.
- PASS: Real local browser-backed render markers are returned and asserted.
- PASS: No provider/cloud/deploy/ProductReady=true claim is made by the harness.
- PASS: SDK9 direct ModelServer regression tests still pass.
- PASS: Changed-code whitespace, conflict-marker, and mojibake scans are clean.
- PASS: Non-destructive mergeability is clean for the actual safe-worktree refs `origin/main`, `origin/UpdateUniversalSkills`, and `origin/DesignToCode` when checked by exact commit hash.
- PASS: Parent verification issue 1 repaired. The exact FLOW test name is `ExplicitBrowserBackedPromotedLocalCompositionInvokesRealModelServerViaFlow`, and it was rerun directly.
- PASS: Parent verification issue 2 repaired. The prior main failure came from checking a temp-clone `origin/main` alias that resolved to stale local-main commit `fd2e3bfd843940d4341a9b86a0503cb382ac8122`, not the safe worktree's actual `origin/main` commit `6579ffb12dfb8cd7822fa7e1561fd03980df2fea`.
- NOT PROVEN: Full platform-to-FLOW-to-ModelServer all-up execution.
- NOT PROVEN: Actual promoted checkpoint loading through FLOW.

## Mental Simulation Result

Figma/Canva-style package data can enter the FLOW intake path with the promoted-local/browser-backed contract, carry the trace id, map into a real llm_mvp_core ModelServer internal request, execute the real local DesignConversion composition with an injected local browser render provider, return browser-backed render markers, and preserve no-cloud/no-deploy/ProductReady=false boundaries. The simulation stops before full platform all-up and before actual promoted checkpoint artifact loading.

pass169_verdict: PASS

## Remaining Gaps After Pass169

- Wire the safe platform worktree into the same real FLOW-to-ModelServer harness, preferably as a local-host two-process or in-memory platform gateway test.
- Replace the explicit promoted-local request marker with actual Pass164R promoted-checkpoint readback through FLOW when a safe local artifact path is available.
- Keep all production/cloud/provider/public endpoint/ProductReady=true claims blocked until separately proven.

## Next Recommended Pass

Pass170 should create the platform safe-worktree local-host or in-memory all-up harness that calls the Pass169 real FLOW client, then add promoted-checkpoint artifact readback if the local Pass164R artifact location can be safely supplied without provider/cloud/deploy behavior.
