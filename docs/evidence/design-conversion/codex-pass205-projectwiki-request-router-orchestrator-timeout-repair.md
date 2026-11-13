# Pass205 ProjectWiki RequestRouter Orchestrator Timeout Repair

claim_scope: Pass205 bounded RequestRouter verification-gate repair for `ProjectWikiDomainTests.ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining`; also repaired the directly related `ProjectWikiRequestRoutingTrainingPipeline_RunsBatchGateAndPublishesManifest` timeout found by the request-router cluster. This is not product complete, not 50/50, not full CI, not deployed, not a public endpoint, not provider execution, not production checkpoint readiness, not git publication, not PR readiness, not cloud readiness, and not full local suite passed.
evidence_scope: Fresh-read required rules/skills by command, Pass204 artifact read, current ProjectWiki tests and RequestRouter orchestrator/pipeline/metric files inspected, exact focused pre-fix RequestRouter orchestrator route blame-hang reproduced, two minimal test-local dependency injection fixes applied through existing seams, exact focused route reruns passed, direct RequestRouting pipeline rerun passed, request-router cluster passed 9/9, ProjectWiki class blame-hang moved past RequestRouter and later timed out at existing-project validation, DesignConversion regression passed 247/247, final diff/process/artifact checks run.
count_claimed: 1 Pass205 bounded pass for repairing the RequestRouter orchestrator timeout and the same-slice direct RequestRouting pipeline timeout, with selected gates verified; 0 full-suite, 0 ProjectWiki class-cleared, 0 product-complete, 0 CI, 0 publication claims.
independent_evidence_count: 31 bounded command/file/diff/process/test evidence rows listed below.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
spawned_worker_identity_self_check: spawned_worker=true; used_NONE=false; used_false_no_subagent_claim=false; parent_must_reconcile=true
nested_subagent_usage: No nested sub-agent was used by this worker.
downgrade_needed: Yes. The RequestRouter slice is repaired and verified, but the broader ProjectWiki class gate is not clear. The next observed class blocker is `ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent`.

## Skill Load Status

| File | Status | Evidence |
| --- | --- | --- |
| `AGENTS.md` | found/read by command | chars 407799; sha256 `a8f6eb15692f727cd0ec5623e2c13109f3a1ac7c9ea31e4b3dc63c16e56f5d69`; visible transcript for this very large file was partially truncated. |
| `docs/SESSION-START-PROMPT.md` | found/read by command | chars 42337; sha256 `450961e74c8e9db4caa6d59441e8bfc76f2bff973072ed13b8bf6d69df0d9a8c`. |
| `docs/skills/claim-verification-SKILL.md` | found/read by command | chars 65698; sha256 `f6d84c0f3815367c26012a81b6c8c6db37eb4fdd2144263842785a4e694a0d68`; visible transcript for this long file was partially truncated. |
| `docs/skills/audit-protocol-SKILL.md` | found/read by command | chars 35636; sha256 `ff0c6898debd3bf21ec538d6c32f1444044d545dc8996198a24fb3f3876d9d0a`. |
| `docs/skills/output-skepticism-SKILL.md` | found/read by command | chars 21108; sha256 `afe71fa8a2e944d0b65cf6a34acd5ddec9ee6294f89c086c66769086d45d4c8a`. |
| `docs/skills/implementation-integrity-SKILL.md` | found/read by command | chars 38187; sha256 `18c33dd8c4d74770d5daa0a8350bd18fa3f0756f13c05d5e2138e1ab5600dcf1`. |
| `docs/skills/test-integrity-SKILL.md` | found/read by command | chars 41213; sha256 `fd545554bc746e7851119cc5bd9ec59b122f3194cd577a036220a5c17af1805b`. |
| `docs/skills/integration-testing-SKILL.md` | found/read by command | chars 9104; sha256 `6cb1698dc95c6f77e96f54bfc81cd39c8aebd878542cbb0aa8d09127d0996d8a`. |
| `docs/skills/generated-code-review-SKILL.md` | found/read by command | present QA skill; chars 30983; sha256 `ceb6cf6bbfff4121db8fa898a21765afda0cf70e2f9334cd9fa8f171c21edcc4`. |
| `docs/skills/code-examination-SKILL.md` | found/read by command | present QA skill; chars 14801; sha256 `38df19ab6d80a0ef247d538cd3a0d6ebe82c850fc76a099c6c4a412ae38d6aa6`. |
| `docs/skills/test-failure-triage-SKILL.md` | found/read by command | present QA skill; chars 8136; sha256 `151992d53fe19d0d3988882f4474d2d77cc96137f820b3abc9a15a383b507920`. |
| `docs/skills/e2e-test-matrix-SKILL.md` | found/read by command | present QA skill; chars 10092; sha256 `a4c1af58afe0966699b9c7872d05a46a017a6c51d216836f3fee5d18fa86282b`. |

## Context Read

| File or source | Status | Use |
| --- | --- | --- |
| `docs/evidence/design-conversion/codex-pass204-projectwiki-training-orchestrator-timeout-repair.md` | found/read | Confirmed accepted Pass204 repaired CodeTrace and named this exact RequestRouter route as the next blocker. |
| `Tests/ProjectWikiDomainTests.cs` | found/read by command, relevant line windows read back visibly | Confirmed the RequestRouter orchestrator route test and direct RequestRouting pipeline test both used default production-sized RequestRouting pipeline/metric runner before Pass205 patches. |
| `42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs` | found/read | Confirmed natural prompt routing to `_requestRouting.RunOneBatchAsync(...)` and existing constructor seam `requestRouting`. |
| `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs` | found/read | Confirmed existing constructor seam `ProjectWikiDpoMetricRunner? dpoMetricRunner = null` and default production-sized DPO metric runner. |
| `42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs` | found/read | Confirmed defaults and bounded constructor knobs for epoch count, vocab size, context length, hidden size, heads, FFN size, and deterministic initialization. |

## Mental Simulation

Natural prompt route:

1. `ProjectWikiRequestRouterUnit.Route(prompt)` classifies "train project wiki request router" as `ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining`.
2. `ProjectWikiTrainingOrchestrator.RunAsync(...)` calls `_requestRouting.RunOneBatchAsync(...)`.
3. `ProjectWikiRequestRoutingTrainingPipeline` builds 50 train rows and 50 held-out rows, runs the structural request-routing runner, builds metric train/held-out rows, then calls `ProjectWikiDpoMetricRunner.TrainAndEvaluateAsync(...)`.
4. Publication is skipped because the trained snapshot has `SemanticQualityClaimed=false`; the tests preserve the route, unit, 50-row, DPO metric, publication-warning, and next-action assertions.

The pre-fix focused route did not fail because classification, async cancellation, or publication deadlocked. It reached the RequestRouting pipeline and stayed silent long enough for blame-hang while the default production-sized DPO metric runner executed. The direct pipeline cluster later showed the same slow default path one level lower.

## Root Cause

The selected orchestrator test used `new ProjectWikiTrainingOrchestrator()`, so it constructed the default `ProjectWikiRequestRoutingTrainingPipeline()`. That pipeline constructed a default `ProjectWikiDpoMetricRunner(epochCount: 8, learningRate: 1e-3f, deterministicFreshInitialization: true)` with the production-sized runner defaults (`vocabSize` 2048, `contextLen` 192, `hiddenDim` 64, `ffnHiddenDim` 128). The exact focused pre-fix blame-hang run aborted after 45 seconds of inactivity with `ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining` incomplete.

The first post-route cluster then exposed the same default production-sized path in `ProjectWikiRequestRoutingTrainingPipeline_RunsBatchGateAndPublishesManifest`, which timed out after three other request-router tests completed. Both symptoms were bounded test-path sizing issues, not route semantics changes and not DesignConversion feature work.

## Repair Summary

Patched only `Tests/ProjectWikiDomainTests.cs` in this pass.

1. `ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining` now creates a bounded deterministic `ProjectWikiDpoMetricRunner`, wraps it in `new ProjectWikiRequestRoutingTrainingPipeline(dpoMetricRunner: dpoMetricRunner)`, and injects that pipeline into `new ProjectWikiTrainingOrchestrator(requestRouting: requestRouting)`.
2. `ProjectWikiRequestRoutingTrainingPipeline_RunsBatchGateAndPublishesManifest` now injects the same bounded deterministic runner through the existing pipeline constructor seam.

Bounded runner settings used by this pass:

| Setting | Value |
| --- | ---: |
| epochCount | 4 |
| learningRate | 0.05 |
| vocabSize | 256 |
| contextLen | 96 |
| hiddenDim | 16 |
| numLayers | 1 |
| numHeads | 4 |
| numKvHeads | 1 |
| ffnHiddenDim | 32 |
| deterministicFreshInitialization | true |

Production defaults are unchanged. The tests still route through the real RequestRouter pipeline, still build 50 training rows and 50 held-out rows, still run the DPO metric runner, still preserve publication gating, and still assert route kind, target unit, verdict, row counts, primary accuracy, CPU/memory/elapsed metrics, null publication manifest, next action, and publication warning semantics.

## Files Changed

| File | Change |
| --- | --- |
| `Tests/ProjectWikiDomainTests.cs` | Pass205 added bounded RequestRouting DPO metric runner injection to the direct RequestRouting pipeline test and the RequestRouter orchestrator route test. The same file already contained accepted Pass201-Pass204 hunks; full-file diff includes those prior hunks but they were not authored by this pass. |
| `docs/evidence/design-conversion/codex-pass205-projectwiki-request-router-orchestrator-timeout-repair.md` | This bounded evidence artifact. |

## Commands Run With Exit Codes

| # | Command or action | Exit | Evidence summary |
| ---: | --- | ---: | --- |
| 1 | `Get-Content -Raw` for `AGENTS.md`, `docs/SESSION-START-PROMPT.md`, required core skills, and present QA/code-review skills | 0 | Files found/read by command; long visible transcripts were partially truncated where noted. |
| 2 | Optional QA skill presence check for generated-code-review, code-examination, test-failure-triage, and e2e-test-matrix | 0 | All four optional QA/code-review skills were present. |
| 3 | `Get-Content -Raw docs/evidence/design-conversion/codex-pass204-projectwiki-training-orchestrator-timeout-repair.md` | 0 | Confirmed accepted Pass204 next blocker was the RequestRouter orchestrator route. |
| 4 | `Get-Content -Raw Tests/ProjectWikiDomainTests.cs` | 0 | Test file read; output display was long/truncated, so targeted windows were read back later. |
| 5 | Broad initial `rg` for ProjectWiki route symbols across repo | 124 | Timed out; no claim is based on this result. |
| 6 | Targeted `rg` under `42.ProjectWikiDomain` and `Tests` for RequestRouter/orchestrator symbols | 0 | Located RequestRouter tests, pipeline, runner, model, publisher, and orchestrator route. |
| 7 | Read `ProjectWikiTrainingOrchestrator.cs`, `ProjectWikiRequestRoutingTrainingPipeline.cs`, and `ProjectWikiDpoMetricRunner.cs` | 0 | Confirmed route path, existing seams, defaults, and bounded knobs. |
| 8 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining" --blame-hang --blame-hang-timeout 45s ...` | 1 | Pre-fix focused reproduction aborted after 45 seconds; active test was the exact named RequestRouter orchestrator route. |
| 9 | Read `Tests/TestResults/143fbef4-7f1e-4ed6-92b5-58a1bc903d1e/Sequence_026cf7901878465784e6b7c45079591c.xml` | 0 | Exact named route test had `Completed="False"`. |
| 10 | Process inventory after pre-fix focused abort | 0 | No stale `testhost.exe` or `vstest.console.exe`; no test-related `dotnet.exe` remained. |
| 11 | Targeted test window readback around RequestRouting tests | 0 | Confirmed the direct pipeline and orchestrator route tests used defaults before this pass. |
| 12 | `apply_patch` on `Tests/ProjectWikiDomainTests.cs` | 0 | Added bounded RequestRouting pipeline injection to the exact orchestrator route test. |
| 13 | `dotnet test ... --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining"` | 0 | Post-fix build-backed focused route run passed 1/1 in 27 seconds; build emitted pre-existing unrelated warnings. |
| 14 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining" --blame-hang --blame-hang-timeout 45s ...` | 0 | Post-fix focused blame-hang route run passed 1/1 in 20 seconds; blame collector reported all tests finished and no sequence file generated. |
| 15 | Request-router test-name inventory via `rg` | 0 | Found 9 request-router-related tests in `ProjectWikiDomainTests.cs`. |
| 16 | Request-router cluster blame-hang before second patch | 1 | Three tests passed, then aborted at `ProjectWikiRequestRoutingTrainingPipeline_RunsBatchGateAndPublishesManifest`. |
| 17 | Read `Tests/TestResults/46c3fae7-8771-40fd-82bf-b101c67fdec6/Sequence_8fc20f14443a4b4a816ad0698b1853d6.xml` | 0 | Three RequestRouting tests completed true; direct pipeline test had `Completed="False"`. |
| 18 | Process inventory after cluster abort | 0 | Only VBCSCompiler `dotnet.exe` remained; no stale `testhost.exe` or `vstest.console.exe`. |
| 19 | `apply_patch` on `Tests/ProjectWikiDomainTests.cs` | 0 | Added bounded DPO metric runner injection to the direct RequestRouting pipeline test. |
| 20 | `dotnet test ... --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiRequestRoutingTrainingPipeline_RunsBatchGateAndPublishesManifest"` | 0 | Direct RequestRouting pipeline run passed 1/1 in 24 seconds after rebuild; build emitted pre-existing unrelated warnings. |
| 21 | Request-router cluster rerun with no-build blame-hang | 0 | Cluster passed 9/9 in 50 seconds; blame collector reported all tests finished and no sequence file generated. |
| 22 | ProjectWiki class no-build blame-hang probe | 1 | Passed 28 tests, then aborted at `ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent`; this is next-blocker evidence only. |
| 23 | Read `Tests/TestResults/6bc7830a-fb87-4acf-9e1b-b648191cd390/Sequence_bdc5f76dfe454cf9812578ee40fea54b.xml` | 0 | RequestRouting runner and CodeTrace route completed true; existing-project validation test completed false. |
| 24 | `dotnet test Tests/LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~DesignConversion"` | 0 | DesignConversion regression filter passed 247/247 in 1 minute 26 seconds. |
| 25 | `git diff -- Tests/ProjectWikiDomainTests.cs` | 0 | Diff reviewed; Pass205 hunks are the two RequestRouting bounded injections, with prior accepted hunks visible in the same file. |
| 26 | Final skill hash/status table | 0 | Recomputed chars and SHA256 for required rules/skills after reads. |
| 27 | Targeted changed-line readback for the two RequestRouting tests | 0 | Confirmed bounded runner injection and existing assertions remain. |
| 28 | Final process inventory for `dotnet|testhost|vstest` | 0 | Only VBCSCompiler `dotnet.exe` was listed; no active testhost/vstest process. |
| 29 | Artifact creation via `apply_patch` | 0 | Created this exact Pass205 evidence artifact. |
| 30 | `git diff --check -- Tests\ProjectWikiDomainTests.cs docs\evidence\design-conversion\codex-pass205-projectwiki-request-router-orchestrator-timeout-repair.md` | 0 | Diff check passed with no whitespace errors. |
| 31 | Artifact marker/readback command plus final process inventory | 0 | Verdict-key count 1; exact spawned identity line count 1; bad identity count 0; conflict marker count 0; configured mojibake marker count 0; top/tail readback clean ASCII; only VBCSCompiler `dotnet.exe` remained, no active testhost/vstest process. |

## Verification

Focused selected gate:

- Pre-fix focused blame-hang run aborted after 45 seconds; sequence file showed the exact named RequestRouter orchestrator route incomplete.
- Post-fix build-backed focused route run passed 1/1.
- Post-fix focused route blame-hang run passed 1/1 in 20 seconds and generated no sequence file.

Relevant RequestRouter cluster:

- First cluster run showed the direct RequestRouting pipeline test still had the same production-sized metric runner timeout.
- After the direct pipeline test-local injection, the RequestRouter cluster passed 9/9 in 50 seconds with no sequence file generated.

Broader allowed regression:

- `FullyQualifiedName~DesignConversion` passed 247/247 in 1 minute 26 seconds.

ProjectWiki class gate:

- The ProjectWiki class blame-hang probe was attempted after the RequestRouter repairs.
- It passed 28 tests and moved past the RequestRouting runner and CodeTrace orchestrator route.
- It then aborted at `ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent`.
- Therefore this pass does not claim `ProjectWikiDomainTests` class passed.

## Next Timeout Or Failure Identified

`ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent` is the next observed broad ProjectWiki class blocker from the post-repair class probe. This pass does not repair it.

## Code Review / Integrity Notes

| Check | Result |
| --- | --- |
| Route semantics preserved | Yes. The orchestrator test still requires `ProjectWikiRequestRouterTraining`, `ProjectWikiRequestRouterUnit.UnitId`, `PROJECT_WIKI_REQUEST_ROUTING_GATE_PASSED`, 50 train rows, 50 held-out rows, primary accuracy 1.0, metric timing/resource fields, null publication manifest, exact next action, and the semantic-quality publication warning. |
| Pipeline semantics preserved | Yes. The direct pipeline test still requires 50 train rows, 50 held-out rows, gate-passed verdict, non-null DPO metrics, and publication or publication-skipped evidence. |
| Test weakened/skipped/deleted | No. No test was skipped, deleted, or assertion-removed. |
| Production defaults changed | No. The bounded runner is test-local via existing constructor seams. |
| Deterministic validation hidden behind training | No. RequestRouting structural evaluation still runs before DPO metric and publication gate checks. |
| Trainable quality overclaimed | No. This artifact claims only bounded local timeout repair and selected filter verification. |

## Remaining Broad-Suite Blockers

The RequestRouter orchestrator route and related RequestRouter cluster are repaired and verified. The broader ProjectWiki class gate remains open.

| Scope | Evidence | Status |
| --- | --- | --- |
| RequestRouter focused route | Post-fix focused build and blame-hang runs passed 1/1. | repaired in Pass205 |
| RequestRouter cluster | Post-fix no-build blame-hang cluster passed 9/9. | repaired in Pass205 |
| `ProjectWikiDomainTests` class | Post-fix class probe passed 28 tests, then aborted at existing-project validation. | next blocker |
| DesignConversion regression | `FullyQualifiedName~DesignConversion` passed 247/247. | selected regression passed |

## Next Recommended Pass

Run the next bounded ProjectWiki local verification repair on `ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent`. Start with focused blame-hang reproduction and inspect whether the timeout depends on external existing wiki directories, production-sized validation loops, or optional real-project data. Keep DesignConversion out of scope except for the selected regression filter after any safe patch.

pass205_verdict: PASS_PROJECTWIKI_REQUEST_ROUTER_ORCHESTRATOR_TIMEOUT_REPAIRED_BOUNDED
