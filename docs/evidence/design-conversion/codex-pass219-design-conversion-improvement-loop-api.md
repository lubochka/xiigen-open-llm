# Codex Pass219 - DesignConversion Improvement Loop API

## Claim Guard

claim_scope: Pass219 bounded DesignConversion improvement-loop API/local-loop behavior in C:\Projects\llm_mvp_core.
evidence_scope: Required guidance load summary, Pass218 readback, target test and implementation readback, scoped improvement-loop symbol search, focused improvement-loop segment test, and full DesignConversion regression.
count_claimed: 1 bounded Pass219 evidence artifact and verification pass.
independent_evidence_count: 7 evidence groups: skill loader summary, Pass218 readback, target tests readback, scoped rg production/test map, targeted production readback, focused segment test, full DesignConversion regression.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
spawned_worker_identity_self_check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true.
nested_subagent_usage: No nested sub-agent was used by this worker.
downgrade_needed: no for the bounded local improvement-loop API slice; yes for product completion, public endpoint, external provider execution, full CI, or 50/50 chain completion claims.

pass219_verdict: PASS_NO_CODE_FIX_LOCAL_IMPROVEMENT_LOOP_API_VERIFIED

## Skill Load Status

Required project guidance was found and read by filesystem command before product/code action:
- AGENTS.md
- docs/SESSION-START-PROMPT.md
- docs/skills/planning-session-startup-SKILL.md
- docs/skills/architect-behavior-classifier-SKILL.md
- docs/skills/authority-requirement-binding-SKILL.md
- docs/skills/reference-inventory-and-source-map-SKILL.md
- docs/skills/phase-planning-SKILL.md
- docs/skills/plan-review-SKILL.md
- docs/skills/goal-delivery-completeness-SKILL.md
- docs/skills/claim-verification-SKILL.md
- docs/skills/audit-protocol-SKILL.md
- docs/skills/specificity-calibration-SKILL.md
- docs/skills/output-skepticism-SKILL.md
- docs/skills/reconnaissance-gate-SKILL.md
- docs/skills/trainable-architecture-decision-SKILL.md
- docs/skills/no-product-decisions-SKILL.md
- docs/skills/output-readability-gate-SKILL.md
- docs/skills/human-readable-architecture-audit-SKILL.md
- docs/skills/generated-code-review-SKILL.md
- docs/skills/code-examination-SKILL.md
- docs/skills/implementation-integrity-SKILL.md
- docs/skills/test-integrity-SKILL.md
- docs/skills/integration-testing-SKILL.md
- docs/skills/e2e-test-matrix-SKILL.md
- docs/skills/ui-testing-SKILL.md
- docs/skills/ui-quality-standard-SKILL.md

Operational gates enforced: no approval prompt, no sandbox permission override, no nested agents, no git publication, no external provider/API call, no ProductReady/public endpoint overclaim, and deterministic QA/render baseline kept separate from trainable or visual-provider authority.

## Context Read

Pass218 evidence read:
- docs/evidence/design-conversion/codex-pass218-design-conversion-status-readback-local-caller.md

Target tests read or symbol-read:
- Tests/DesignConversionModelServerImprovementLoopContractTests.cs
- Tests/DesignConversionModelServerImprovementLoopTests.cs
- Tests/DesignConversionModelServerImprovementLoopCompositionTests.cs
- Tests/DesignConversionModelServerImprovementLoopVisualProviderTests.cs
- Tests/DesignConversionModelServerImprovementLoopBrowserIntegrationTests.cs
- Tests/DesignConversionImprovementLoopCoordinatorTests.cs

Production readback used:
- 44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs
- 44.DesignConversionDomain/DesignConversionInternalService.cs
- 44.DesignConversionDomain/DesignConversionContracts.cs
- 44.DesignConversionDomain/DesignConversionQaContracts.cs
- 44.DesignConversionDomain/DesignConversionVisualJudge.cs
- 44.DesignConversionDomain/DesignConversionVisualQaBoundaryPolicy.cs
- 44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs
- 44.DesignConversionDomain/DesignConversionExternalVisualProviderConfigIntake.cs
- 25.ModelServer/DesignConversion/DesignConversionModelServerImprovementLoopHandler.cs
- 25.ModelServer/DesignConversion/DesignConversionModelServerLocalCallerAdapter.cs
- 25.ModelServer/DesignConversion/DesignConversionModelServerEndpointRegistry.cs
- 25.ModelServer/DesignConversion/DesignConversionModelServerCallerContract.cs
- 25.ModelServer/DesignConversion/DesignConversionModelServerAspNetHostStartup.cs
- 25.ModelServer/DesignConversion/DesignConversionModelServerAspNetEndpointMapper.cs

The initial whole-repo symbol search timed out and is not used as positive evidence. A scoped retry over 25.ModelServer, 44.DesignConversionDomain, and Tests succeeded and identified the implementation/test surface above.

## Behavior Evidence

The implementation and tests cover the requested local improvement-loop behavior:
- Model-server route contract documents local-only POST shape, required fields, stable validation failures, refs-only response fields, and no raw private payload policy.
- Local caller maps loopId, initial package, maxAttempts, metadata, budget, design tree, original tree, html/css, screenshot/design-render refs, and viewport refs into the local ModelServer route.
- Internal service renders each configured resolution up to MaxRenderQaRuns, selects expected screenshot/design-render refs by viewport, compares rendered vs expected images, and asks the injected visual judge for a verdict.
- Visual QA policy checks multi-resolution completeness, provider boundary, visual score threshold, estimated cost, max improvement candidates, and budget cap.
- Coordinator records refs-only attempt telemetry, stops on quality threshold, provider block, render-manifest gap, budget cap, max attempts, cancellation, or candidate strategy failure.
- When policy says continue, the coordinator asks the local candidate strategy for an improvement candidate, builds the next html/css request, and re-renders/recompares the candidate.
- Local composition tests wire browser-backed render provider plus comparison/visual-provider fakes without claiming public production readiness.
- Visual provider tests prove config intake and fake local provider composition without executing network or real external provider calls.
- Browser integration test is included in the focused segment and passed in this local environment.
- Artifact DB/DPO path stores sanitized runtime-created candidates as review-pending, training-candidate-only, product-not-ready records when configured.

## Tests Run

Focused improvement-loop segment:

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionModelServerImprovementLoop|FullyQualifiedName~DesignConversionImprovementLoopCoordinator" --logger "console;verbosity=minimal"
```

Result: exit 0. Passed 27, failed 0, skipped 0, total 27.

Full DesignConversion regression:

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversion" --logger "console;verbosity=minimal"
```

Result: exit 0. Passed 247, failed 0, skipped 0, total 247.

## Mental Simulation

1. Caller submits html, css, tree refs, screenshot/design-render refs, viewport information, budget, loop id, and local metadata to the improvement-loop API.
2. The route maps the package to an internal evaluation request with local-only provenance and refs-only boundary metadata.
3. The internal service renders the submitted html/css for each required resolution allowed by render QA budget.
4. Each render is compared against the expected design screenshot/render ref for that viewport, then an injected visual judge returns a verdict.
5. The visual QA policy decides whether the threshold is met, provider evidence is blocked/missing, render coverage is incomplete, or the cost/attempt budget is exhausted.
6. If quality is below threshold and budget allows, the coordinator sends the current evaluation and attempt history to the local improvement-candidate strategy.
7. The next candidate html/css is evaluated again through the same render, compare, and visual-judge chain.
8. The loop stops on quality threshold, budget cap, max attempts, provider/render blocker, cancellation, or candidate failure.
9. The response emits status, attempts, artifact/result refs, evidence refs, public metadata, and warnings without raw html/css/design tree/private payloads.
10. When the local artifact DB is configured, downstream DPO storage receives sanitized runtime-created candidate records that remain review-pending, training-candidate-only, and product-not-ready.

## No-Fix Reason

No code fix was needed for Pass219. The focused segment and full DesignConversion regression both passed, and the inspected tests already assert the local API loop semantics, local provider composition, model-server contract stability, refs-only result/artifact refs, budget and threshold stops, fake/local visual-provider boundary, browser-local integration, and downstream DPO storage behavior requested for this pass.

This evidence does not claim external visual AI execution, public endpoint readiness, deployment readiness, ProductReady status, full CI, or design-to-code chain completion.

## Commands Run With Exit Codes

- Required file-size precheck: exit 0.
- Initial skill hash/read summary attempt: PowerShell emitted hashing API errors; not used as positive load-summary evidence.
- Compatible skill hash/read summary retry: exit 0.
- Pass218 readback and initial target-test readback: exit 0.
- Initial whole-repo symbol search: exit 124 timeout; not used as positive discovery evidence.
- Scoped symbol search over 25.ModelServer, 44.DesignConversionDomain, and Tests: exit 0.
- Targeted production implementation readback: exit 0.
- Focused improvement-loop segment test: exit 0, 27/27 passed.
- Full DesignConversion regression: exit 0, 247/247 passed.
- Tracked scoped diff check for the new artifact path: exit 0, no tracked patch output because the artifact is newly added and untracked.
- No-index diff check against the new artifact: raw exit 1 for file difference, zero check output lines, no whitespace or conflict-marker messages emitted.
- Artifact marker and mojibake scan: worker-id marker count 1, verdict marker count 1, forbidden worker-id NONE line count 0, conflict-marker count 0, mojibake marker count 0.
- Full artifact readback: exit 0, clean readable text displayed.

## Files Changed

- docs/evidence/design-conversion/codex-pass219-design-conversion-improvement-loop-api.md

Production code and test code changed: none.

## Next Recommended Pass

Pass220 can target the next adjacent DesignConversion API/contract surface after improvement-loop local behavior, while preserving the same no-external-provider and no-ProductReady boundaries unless a later unquoted task authorizes broader execution.
