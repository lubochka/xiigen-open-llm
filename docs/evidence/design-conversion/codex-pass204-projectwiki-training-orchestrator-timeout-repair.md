# Pass204 ProjectWiki Training Orchestrator Timeout Repair

claim_scope: Pass204 bounded repair for `ProjectWikiDomainTests.ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToCodeTraceTraining`; not product complete, not 50/50, not full CI, not deployed, not public endpoint, not provider execution, not production checkpoint readiness, not git publication, not PR readiness, not cloud readiness, and not full local suite passed.
evidence_scope: Fresh-read required rule and skill files by command, Pass203 evidence read, current ProjectWiki test and orchestrator/CodeTrace/DPO files inspected, exact focused blame-hang timeout reproduced, one minimal test-local dependency injection fix applied through existing seams, exact focused post-fix run passed, exact focused post-fix blame-hang run passed, direct adjacent CodeTrace pipeline test passed, non-training orchestrator route passed, request-router neighbor recorded as timeout evidence only, DesignConversion regression filter passed, stale test processes cleaned only after exact command-line proof, final diff/process/artifact checks run.
count_claimed: 1 Pass204 bounded pass for repairing the exact ProjectWikiTrainingOrchestrator to CodeTrace timeout and verifying selected gates; 0 full-suite, 0 ProjectWiki class-cleared, 0 product-complete, 0 CI, 0 publication claims.
independent_evidence_count: 31 bounded command/file/diff evidence rows listed below.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
spawned_worker_identity_self_check: spawned_worker=true; used_NONE=false; used_false_no_subagent_claim=false; parent_must_reconcile=true
nested_subagent_usage: No nested sub-agent was used by this worker.
downgrade_needed: Yes. The exact CodeTrace orchestrator route is repaired and verified, but the broader ProjectWiki class gate is not verified. A request-router neighbor timed out under a focused split probe, and the class blame-hang attempt timed out at the tool boundary before returning sequence evidence.

## Skill Load Status

| File | Status | Evidence |
| --- | --- | --- |
| `AGENTS.md` | found/read by command | chars 407799; sha256 `a8f6eb15692f727cd0ec5623e2c13109f3a1ac7c9ea31e4b3dc63c16e56f5d69`; visible transcript for this very large file was partially truncated. |
| `docs/SESSION-START-PROMPT.md` | found/read by command | chars 42267; sha256 `450961e74c8e9db4caa6d59441e8bfc76f2bff973072ed13b8bf6d69df0d9a8c`. |
| `docs/skills/claim-verification-SKILL.md` | found/read by command | chars 65698; sha256 `f6d84c0f3815367c26012a81b6c8c6db37eb4fdd2144263842785a4e694a0d68`; visible transcript for this long file was partially truncated. |
| `docs/skills/audit-protocol-SKILL.md` | found/read by command | chars 35531; sha256 `ff0c6898debd3bf21ec538d6c32f1444044d545dc8996198a24fb3f3876d9d0a`. |
| `docs/skills/output-skepticism-SKILL.md` | found/read by command | chars 21007; sha256 `afe71fa8a2e944d0b65cf6a34acd5ddec9ee6294f89c086c66769086d45d4c8a`. |
| `docs/skills/implementation-integrity-SKILL.md` | found/read by command | chars 38160; sha256 `18c33dd8c4d74770d5daa0a8350bd18fa3f0756f13c05d5e2138e1ab5600dcf1`. |
| `docs/skills/test-integrity-SKILL.md` | found/read by command | chars 41002; sha256 `fd545554bc746e7851119cc5bd9ec59b122f3194cd577a036220a5c17af1805b`. |
| `docs/skills/integration-testing-SKILL.md` | found/read by command | chars 9104; sha256 `6cb1698dc95c6f77e96f54bfc81cd39c8aebd878542cbb0aa8d09127d0996d8a`. |
| `docs/skills/generated-code-review-SKILL.md` | found/read by command | present QA skill; chars 30853; sha256 `ceb6cf6bbfff4121db8fa898a21765afda0cf70e2f9334cd9fa8f171c21edcc4`. |
| `docs/skills/code-examination-SKILL.md` | found/read by command | present QA skill; chars 14801; sha256 `38df19ab6d80a0ef247d538cd3a0d6ebe82c850fc76a099c6c4a412ae38d6aa6`. |
| `docs/skills/test-failure-triage-SKILL.md` | found/read by command | present QA skill; chars 8087; sha256 `151992d53fe19d0d3988882f4474d2d77cc96137f820b3abc9a15a383b507920`. |
| `docs/skills/e2e-test-matrix-SKILL.md` | found/read by command | present QA skill; chars 10011; sha256 `a4c1af58afe0966699b9c7872d05a46a017a6c51d216836f3fee5d18fa86282b`. |

## Context Read

| File or source | Status | Use |
| --- | --- | --- |
| `docs/evidence/design-conversion/codex-pass203-projectwiki-answercomposer-timeout-repair.md` | found/read | Confirmed accepted Pass203 result and exact next blocker: `ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToCodeTraceTraining`. |
| `Tests/ProjectWikiDomainTests.cs` | found/read by command, relevant line windows read back visibly | Confirmed target test used default `new ProjectWikiTrainingOrchestrator()` before this pass. |
| `42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs` | found/read | Confirmed the orchestrator routes `CodeTraceTraining` into `_codeTrace.RunOneBatchAsync(...)` and already has a constructor seam for `CodeTraceBuilderTrainingPipeline`. |
| `42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs` | found/read | Confirmed the pipeline already has a constructor seam for `ProjectWikiDpoMetricRunner` and otherwise defaults to a larger metric runner. |
| `42.ProjectWikiDomain/CodeTraceBuilderTrainingRunner.cs` | found/read | Confirmed deterministic held-out CodeTrace gate still requires 50 training and 50 held-out rows. |
| `42.ProjectWikiDomain/CodeTraceBuilderDpoGenerator.cs` | found/read | Confirmed 50-row train and held-out CodeTrace DPO batch generation. |
| `42.ProjectWikiDomain/CodeTraceBuilderManifestPublisher.cs` | found/read | Confirmed publication remains gated after metrics and promotion-confirmation checks. |
| `42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs` | found/read | Confirmed production-sized DPO metric runner defaults and bounded constructor knobs. |

## Mental Simulation

Natural prompt route:
1. `ProjectWikiRequestRouterUnit.Route(prompt)` classifies the prompt containing "Train code trace builder" as `ProjectWikiRequestRouteKind.CodeTraceTraining`.
2. `ProjectWikiTrainingOrchestrator.RunAsync(...)` calls `_codeTrace.RunOneBatchAsync(...)`.
3. `CodeTraceBuilderTrainingPipeline` builds 50 training rows and 50 held-out rows, runs deterministic CodeTrace recall validation, then runs `ProjectWikiDpoMetricRunner.TrainAndEvaluateAsync(...)`.
4. The publication gate requires a promotion-confirmation batch and policy/reference checkpoint evidence before publishing. The test asserts the publication-skipped warning, so no publication is required.

The pre-fix route was not blocked by classification, deadlock, or publication. It entered the CodeTrace pipeline through the default metric runner path. The direct CodeTrace pipeline test already used a bounded runner; the orchestrator route test did not.

## Root Cause

The selected orchestrator test used `new ProjectWikiTrainingOrchestrator()` and therefore constructed the default `CodeTraceBuilderTrainingPipeline()`. That default pipeline constructs a `ProjectWikiDpoMetricRunner` with larger defaults (`vocabSize` 2048, `contextLen` 192, `hiddenDim` 64, `epochCount` 8 in the CodeTrace default), which is too slow for the focused test/blame-hang budget. The focused pre-fix blame-hang run aborted after 45 seconds of inactivity with this exact test incomplete.

This was a test-path sizing problem in the training metric runner, not a change to route semantics and not a DesignConversion feature issue.

## Repair Summary

Patched only `Tests/ProjectWikiDomainTests.cs` in this pass. The CodeTrace orchestrator route test now:

1. Creates a bounded deterministic `ProjectWikiDpoMetricRunner`.
2. Wraps it in `new CodeTraceBuilderTrainingPipeline(dpoMetricRunner: dpoMetricRunner)`.
3. Injects that pipeline into `new ProjectWikiTrainingOrchestrator(codeTrace: codeTrace)`.

Bounded runner settings used by this pass:

| Setting | Value |
| --- | ---: |
| epochCount | 4 |
| learningRate | 0.05 |
| vocabSize | 256 |
| contextLen | 48 |
| hiddenDim | 16 |
| numLayers | 1 |
| numHeads | 4 |
| numKvHeads | 1 |
| ffnHiddenDim | 32 |
| deterministicFreshInitialization | true |

Production defaults are unchanged. The test still routes through `ProjectWikiTrainingOrchestrator`, still calls the real CodeTrace pipeline, still builds 50 training rows and 50 held-out rows, still checks route kind, target unit, gate verdict, row counts, primary accuracy, null publication manifest, and the promotion-confirmation publication warning.

## Files Changed

| File | Change |
| --- | --- |
| `Tests/ProjectWikiDomainTests.cs` | Pass204 added the test-local bounded CodeTrace pipeline injection to `ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToCodeTraceTraining`. The same file already had accepted Pass201, Pass202, and Pass203 hunks; full-file diff includes those prior hunks but they were not authored by this pass. |
| `docs/evidence/design-conversion/codex-pass204-projectwiki-training-orchestrator-timeout-repair.md` | This bounded evidence artifact. |

## Commands Run With Exit Codes

| # | Command or action | Exit | Evidence summary |
| ---: | --- | ---: | --- |
| 1 | Fresh `Get-Content -Raw` for `AGENTS.md`, `docs/SESSION-START-PROMPT.md`, required core skills, and present QA/code-review skills | 0 | Files found/read by command; long visible transcripts were partially truncated where noted in skill table. |
| 2 | `Get-ChildItem docs\skills -Filter '*SKILL.md'` | 0 | Confirmed optional QA/code-review skills were present. |
| 3 | `Get-Content -Raw docs\evidence\design-conversion\codex-pass203-projectwiki-answercomposer-timeout-repair.md` | 0 | Confirmed Pass203 named this exact CodeTrace orchestrator route as next blocker. |
| 4 | Broad initial `rg` for ProjectWiki route symbols across repo | 124 | Timed out; no claim is based on this result. |
| 5 | Targeted `rg` under `Tests`, `42.ProjectWikiDomain`, `25.ModelServer`, `44.DesignConversionDomain` | 0 | Located orchestrator, CodeTrace pipeline, DPO runner, target test, prior sequence records. |
| 6 | `rg -C` around target test in `Tests\ProjectWikiDomainTests.cs` | 0 | Confirmed pre-fix target test used default orchestrator. |
| 7 | Read `ProjectWikiTrainingOrchestrator.cs`, `CodeTraceBuilderTrainingPipeline.cs`, `CodeTraceBuilderTrainingRunner.cs`, `CodeTraceBuilderDpoGenerator.cs`, `CodeTraceBuilderManifestPublisher.cs`, `ProjectWikiDpoMetricRunner.cs` | 0 | Confirmed route path, existing seams, production defaults, and publication gate. |
| 8 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToCodeTraceTraining" --blame-hang --blame-hang-timeout 45s --blame-hang-dump-type none ...` | 1 | Pre-fix focused reproduction aborted after 45 seconds; active test was the exact named test. |
| 9 | Read `Tests\TestResults\6813f0cb-7135-4f5d-8519-ec001b5e3f64\Sequence_da119c93ed8f401ca967bf2d03975a22.xml` | 0 | The exact named test had `Completed="False"`. |
| 10 | Process inventory after pre-fix focused abort | 0 | No leftover `dotnet`, `testhost`, or `vstest` processes found. |
| 11 | `apply_patch` on `Tests\ProjectWikiDomainTests.cs` | 0 | Added bounded CodeTrace pipeline injection to the exact target test only. |
| 12 | `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToCodeTraceTraining" ...` | 0 | Post-fix build-backed focused run passed 1/1 in 15 seconds; build emitted pre-existing warnings in unrelated tests. |
| 13 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToCodeTraceTraining" --blame-hang --blame-hang-timeout 45s ...` | 0 | Post-fix focused blame-hang run passed 1/1 in 15 seconds; blame collector reported all tests finished and no sequence file generated. |
| 14 | Broad adjacent OR-cluster filter for `ProjectWikiTrainingOrchestrator|CodeTraceBuilderTrainingPipeline` | 124 | Tool timed out after 120 seconds; no pass claim is based on this. |
| 15 | Command-line process readback for cluster timeout PIDs 5272, 3300, 19248 | 0 | Confirmed the PIDs belonged to the timed-out adjacent OR-cluster test run. |
| 16 | `Stop-Process -Id 5272,3300,19248 -Force` and follow-up `Get-Process` for those IDs | 0 | Exact stale test processes stopped; follow-up found none. |
| 17 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.CodeTraceBuilderTrainingPipeline_SkipsPublicationUntilPromotionConfirmation" ...` | 0 | Direct adjacent CodeTrace pipeline gate passed 1/1 in 14 seconds. |
| 18 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining" ...` | 124 | Focused request-router neighbor timed out after 90 seconds; this is next-pass evidence only. |
| 19 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiTrainingOrchestrator_DoesNotRunTrainingForWikiQuestionRoute" ...` | 0 | Non-training route guard passed 1/1 in 65 ms. |
| 20 | Command-line process readback for request-router timeout PIDs 28280, 17124, 20288 | 0 | Confirmed the PIDs belonged to the timed-out request-router focused run. |
| 21 | `Stop-Process -Id 28280,17124,20288 -Force` and follow-up `Get-Process` for those IDs | 0 | Exact stale request-router test processes stopped; follow-up found none. |
| 22 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests" --blame-hang --blame-hang-timeout 60s ...` | 124 | ProjectWiki class probe hit the 150-second tool timeout before visible sequence/pass output; no class status claim is based on this. |
| 23 | Command-line process readback for class-probe timeout PIDs 20368, 26892, 26088, 8128 | 0 | Confirmed the PIDs belonged to the timed-out class blame-hang probe. |
| 24 | `Stop-Process -Id 20368,26892,26088,8128 -Force` | 0 | Exact stale class-probe process chain stopped. |
| 25 | `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~DesignConversion" ...` | 0 | DesignConversion regression filter passed 247/247 in 1 minute 35 seconds. |
| 26 | `git diff -- Tests\ProjectWikiDomainTests.cs` | 0 | Diff reviewed; Pass204 hunk is the CodeTrace orchestrator bounded injection, with prior accepted Pass201/202/203 hunks visible in the same file. |
| 27 | Final process inventory for `dotnet|testhost|vstest` | 0 | No matching active test processes were returned at the time of final evidence collection. |
| 28 | Skill hash/status table command | 0 | Recomputed chars and SHA256 for required rules/skills after the initial hash command had a PowerShell parser error. |
| 29 | Initial skill hash/status table command | 1 | PowerShell parser error; superseded by row 28 and not used as evidence. |
| 30 | Artifact creation via `apply_patch` | 0 | Created this exact Pass204 evidence artifact. |
| 31 | `git diff --check -- Tests\ProjectWikiDomainTests.cs docs\evidence\design-conversion\codex-pass204-projectwiki-training-orchestrator-timeout-repair.md` plus artifact marker/readback checks | 0 | Diff check passed; verdict-key count 1; exact spawned identity line count 1; bad identity count 0; conflict marker count 0; configured mojibake marker count 0; top and tail readback were clean ASCII. |

## Verification

Focused selected gate:
- Pre-fix focused blame-hang run aborted after 45 seconds; sequence file showed the exact named test incomplete.
- Post-fix build-backed focused run passed 1/1.
- Post-fix focused blame-hang run passed 1/1 in 15 seconds and generated no sequence file.

Relevant adjacent gates:
- Direct `CodeTraceBuilderTrainingPipeline_SkipsPublicationUntilPromotionConfirmation` passed 1/1 in 14 seconds.
- Non-training orchestrator route passed 1/1 in 65 ms.
- Request-router orchestrator neighbor timed out under a focused split probe and is not claimed as passed.
- The broader adjacent OR-cluster timed out and is not claimed as passed.

Broader allowed regression:
- `FullyQualifiedName~DesignConversion` passed 247/247 in 1 minute 35 seconds.

ProjectWiki class gate:
- The ProjectWiki class blame-hang command was attempted, but the tool timed out at 150 seconds before returning a usable sequence or pass/fail count.
- Therefore this pass does not claim `ProjectWikiDomainTests` class passed or that the next class blocker was conclusively identified by this class probe.

Diff and process checks:
- Final diff readback showed the Pass204 hunk plus prior accepted ProjectWiki hunks in the same file.
- Exact stale test processes from timed-out probes were cleaned only after command-line proof.
- Final process inventory returned no matching active test processes.

Artifact checks:
- `git diff --check` over the changed test file and this artifact exited 0.
- Verdict-key count was 1.
- Exact spawned identity line count was 1.
- Bad identity count was 0.
- Conflict marker count was 0.
- Configured mojibake marker count was 0.
- Top and tail readback were clean ASCII.

## Remaining Broad-Suite Blockers

The exact CodeTrace orchestrator timeout is repaired and verified. The broader ProjectWiki class gate remains open. Additional timeout evidence observed during this pass:

| Scope | Evidence | Status |
| --- | --- | --- |
| `ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining` | Focused split probe timed out at the tool boundary after 90 seconds. | next-pass evidence only |
| `FullyQualifiedName~ProjectWikiDomainTests` class blame-hang | Attempted after the fix, but the tool timed out at 150 seconds before sequence/pass output returned. | inconclusive |

This pass did not broaden into ExtensionQuality, ExtensionToken, cloud, provider, publication, PR, deployment, or full-suite repair.

## Code Review / Integrity Notes

| Check | Result |
| --- | --- |
| Route semantics preserved | Yes. Assertions still require `CodeTraceTraining`, `CodeTraceBuilderUnit.UnitId`, `CODE_TRACE_BUILDER_GATE_PASSED`, 50 train rows, 50 held-out rows, primary accuracy 1.0, null publication path, and promotion-confirmation warning. |
| Test weakened/skipped/deleted | No. No test was skipped, deleted, or assertion-removed. |
| Production defaults changed | No. The bounded runner is test-local via existing constructor seams. |
| Deterministic validation hidden behind training | No. The deterministic CodeTrace gate still runs before DPO metrics and publication checks. |
| Trainable quality overclaimed | No. This artifact claims only bounded local timeout repair and selected filter verification. |

## Next Recommended Pass

Run the next bounded ProjectWiki local verification repair on `ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining`. Start with focused blame-hang reproduction and inspect `ProjectWikiRequestRoutingTrainingPipeline` plus its default DPO metric runner path. Prefer the same safe pattern only if evidence confirms another production-sized test metric runner path: inject a bounded runner through an existing test seam while preserving route and publication assertions.

pass204_verdict: PASS_PROJECTWIKI_TRAINING_ORCHESTRATOR_TIMEOUT_REPAIRED_BOUNDED
