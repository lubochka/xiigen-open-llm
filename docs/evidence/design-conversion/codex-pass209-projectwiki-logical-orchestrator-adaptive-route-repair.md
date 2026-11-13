# Pass209 ProjectWiki Logical Orchestrator Adaptive Route Repair

claim_scope: Pass209 bounded local repair for `LogicalOrchestratorAssistant_AdaptivelyRoutesProjectWikiTrainingRequestToLearningFlow`. This pass reproduced the exact focused timeout, inspected the adaptive assistant to flow-adapter to training-orchestrator to CodeTrace path, added one minimal optional assistant seam plus test-local bounded CodeTrace DPO metric runner injection, verified the focused adaptive route, verified the direct flow-adapter route touched by the patch, reran S7 until the next exact blocker, and reran the requested DesignConversion regression filter. This is not product complete, not 50/50, not full CI, not deployed, not a public endpoint, not provider execution, not production checkpoint readiness, not git publication, not PR readiness, not cloud readiness, not a full ProjectWiki class pass, and not a full local suite pass.
evidence_scope: Fresh-read required local governance and QA guidance by file-read command with hashes; read Pass208 artifact; read current `Tests/ProjectWikiDomainTests.cs`; read `ProjectWikiTrainingFlowAdapter`, `ProjectWikiTrainingOrchestrator`, `CodeTraceBuilderTrainingPipeline`, `ProjectWikiDpoMetricRunner`, and `LogicalOrchestratorAssistant`; ran pre-fix focused adaptive route reproduction with blame-hang; patched only `LogicalOrchestratorAssistant.cs` and `Tests/ProjectWikiDomainTests.cs`; ran post-fix focused adaptive route, focused flow-adapter route, S7 training-flow/orchestrator segment, and `FullyQualifiedName~DesignConversion`.
count_claimed: 1 Pass209 bounded pass for the exact adaptive route timeout repaired and the next S7 blocker identified; 0 product-complete, 0 50/50, 0 full-suite, 0 full ProjectWiki class-cleared, 0 CI, 0 publication claims.
independent_evidence_count: 22 grouped command/tool records including artifact creation, artifact verification-record update, final artifact marker verification, and changed-file diff check.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
spawned_worker_identity_self_check: spawned_worker=true; used_NONE=false; used_false_no_subagent_claim=false; parent_must_reconcile=true
nested_subagent_usage: No nested sub-agent was used by this worker.
downgrade_needed: Yes for any broad ProjectWiki class, full local suite, product completion, CI, publication, cloud, provider, public endpoint, production checkpoint readiness, or 50/50 claim. No downgrade for the narrow Pass209 repair-and-next-blocker claim above.

## Skill Load Status

| File | Status | Evidence |
| --- | --- | --- |
| `AGENTS.md` | found/read | `Get-Content -Raw`; SHA256 `A8F6EB15692F727CD0EC5623E2C13109F3A1AC7C9EA31E4B3DC63C16E56F5D69`; 407799 bytes. |
| `docs/SESSION-START-PROMPT.md` | found/read | `Get-Content -Raw`; SHA256 `450961E74C8E9DB4CAA6D59441E8BFC76F2BFF973072ED13B8BF6D69DF0D9A8C`; 42337 bytes. |
| `docs/skills/claim-verification-SKILL.md` | found/read | `Get-Content -Raw`; SHA256 `F6D84C0F3815367C26012A81B6C8C6DB37EB4FDD2144263842785A4E694A0D68`; 65698 bytes. |
| `docs/skills/audit-protocol-SKILL.md` | found/read | `Get-Content -Raw`; SHA256 `FF0C6898DEBD3BF21EC538D6C32F1444044D545DC8996198A24FB3F3876D9D0A`; 35636 bytes. |
| `docs/skills/output-skepticism-SKILL.md` | found/read | `Get-Content -Raw`; SHA256 `AFE71FA8A2E944D0B65CF6A34ACD5DDEC9EE6294F89C086C66769086D45D4C8A`; 21108 bytes. |
| `docs/skills/implementation-integrity-SKILL.md` | found/read | `Get-Content -Raw`; SHA256 `18C33DD8C4D74770D5DAA0A8350BD18FA3F0756F13C05D5E2138E1AB5600DCF1`; 38187 bytes. |
| `docs/skills/test-integrity-SKILL.md` | found/read | `Get-Content -Raw`; SHA256 `FD545554BC746E7851119CC5BD9EC59B122F3194CD577A036220A5C17AF1805B`; 41213 bytes. |
| `docs/skills/integration-testing-SKILL.md` | found/read | `Get-Content -Raw`; SHA256 `6CB1698DC95C6F77E96F54BFC81CD39C8AEBD878542CBB0AA8D09127D0996D8A`; 9104 bytes. |
| `docs/skills/generated-code-review-SKILL.md` | found/read | Optional QA/code-review skill present; SHA256 `CEB6CF6BBFFF4121DB8FA898A21765AFDA0CF70E2F9334CD9FA8F171C21EDCC4`. |
| `docs/skills/code-examination-SKILL.md` | found/read | Optional QA/code-review skill present; SHA256 `38DF19AB6D80A0EF247D538CD3A0D6EBE82C850FC76A099C6C4A412AE38D6AA6`. |
| `docs/skills/test-failure-triage-SKILL.md` | found/read | Optional QA/code-review skill present; SHA256 `151992D53FE19D0D3988882F4474D2D77CC96137F820B3ABC9A15A383B507920`. |
| `docs/skills/e2e-test-matrix-SKILL.md` | found/read | Optional QA/code-review skill present; SHA256 `A4C1AF58AFE0966699B9C7872D05A46A017A6C51D216836F3FEE5D18FA86282B`. |

## Root Cause

The adaptive assistant route pre-routes the natural CodeTrace training prompt to `ProjectWikiTrainingFlowAdapter`. The default adapter constructs a default `ProjectWikiTrainingOrchestrator`, and that default orchestrator constructs a default `CodeTraceBuilderTrainingPipeline`. That pipeline uses a default `ProjectWikiDpoMetricRunner` with a heavier DPO metric path. In the focused pre-fix run, the exact adaptive assistant test stayed active until the 90-second blame-hang timeout.

The timeout was not an assistant deadlock in the adaptive router. It was the default heavy CodeTrace DPO metric path reached through the assistant/flow-adapter route.

## Mental Simulation

1. `LogicalOrchestratorAssistant.RunAsync` receives the natural CodeTrace training prompt without an explicit scenario.
2. The assistant pre-routes the prompt through `ProjectWikiRequestRouterUnit`, sees a ProjectWiki training route, and selects `ProjectWikiTrainingFlowAdapter.ScenarioId`.
3. The logical orchestrator invokes `ProjectWikiTrainingFlowAdapter`.
4. The adapter calls `ProjectWikiTrainingOrchestrator.RunAsync`.
5. The orchestrator routes the prompt to `CodeTraceTraining`.
6. The CodeTrace pipeline builds 50 training rows and 50 held-out rows, validates the training gate, and then runs DPO metrics.
7. Without injection, that final DPO metric step uses the heavier default runner and times out under the test gate.
8. With the bounded runner injected through constructors, the same route, flow response, metrics, runner report, and assertions remain intact while the metric path completes inside the bounded test gate.

## Repair Summary

`15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs`

- Added an optional `ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null` parameter to `LogicalOrchestratorAssistant.CreateDefault`.
- Registered the supplied adapter when provided; otherwise production defaults still construct `new ProjectWikiTrainingFlowAdapter()`.

`Tests/ProjectWikiDomainTests.cs`

- In `ProjectWikiTrainingFlowAdapter_RunsTrainingFromLogicalFlowRequest`, injected a `ProjectWikiTrainingFlowAdapter` backed by a `ProjectWikiTrainingOrchestrator` with a bounded `CodeTraceBuilderTrainingPipeline`.
- In `LogicalOrchestratorAssistant_AdaptivelyRoutesProjectWikiTrainingRequestToLearningFlow`, used the new assistant seam to register the same bounded CodeTrace training adapter.
- Preserved the route and response assertions: selected flow, CodeTrace route kind, target unit id, 50 training rows, primary accuracy, routed sub-phase id, training status, runner report path, report JSON fields, and DPO runner report path.

The bounded runner configuration was `epochCount: 4`, `learningRate: 5e-2f`, `vocabSize: 256`, `contextLen: 48`, `hiddenDim: 16`, `numLayers: 1`, `numHeads: 4`, `numKvHeads: 1`, `ffnHiddenDim: 32`, `deterministicFreshInitialization: true`.

## Segment Results

| Scope | Command evidence | Exit | Result |
| --- | --- | ---: | --- |
| Pre-fix focused adaptive route | `dotnet test .\Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.LogicalOrchestratorAssistant_AdaptivelyRoutesProjectWikiTrainingRequestToLearningFlow" --blame-hang --blame-hang-timeout 90s` | 1 | Aborted after 90 seconds of inactivity; active test was the exact adaptive route test. |
| Pre-fix sequence readback | `Tests/TestResults/85a6669e-d691-468c-af05-3db3dfcb616e/Sequence_59a7e1cd5b5c4bc2937eee7db7f6c16f.xml` | 0 | The exact adaptive route test had `Completed="False"`. |
| Post-fix focused adaptive route | `dotnet test .\Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ProjectWikiDomainTests.LogicalOrchestratorAssistant_AdaptivelyRoutesProjectWikiTrainingRequestToLearningFlow" --blame-hang --blame-hang-timeout 120s` | 0 | Passed 1/1 after rebuild. |
| S7 training-flow/orchestrator segment | `dotnet test .\Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectWikiTrainingOrchestrator|FullyQualifiedName~ProjectWikiTrainingFlowAdapter|FullyQualifiedName~ProjectWikiTrainingRequestTo" --blame-hang --blame-hang-timeout 120s` | 1 | Advanced to 3 passed tests, then aborted on the next exact blocker. |
| S7 sequence readback | `Tests/TestResults/f7579a92-2e76-41c3-ba27-e83357175ba0/Sequence_6c6a54e19a2049d9b006e42538a90b96.xml` | 0 | CodeTrace orchestrator, non-training route, and adaptive route completed true; explicit assistant route completed false. |
| Focused direct flow adapter | `dotnet test .\Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiTrainingFlowAdapter_RunsTrainingFromLogicalFlowRequest" --blame-hang --blame-hang-timeout 120s` | 0 | Passed 1/1. |
| DesignConversion regression | `dotnet test .\Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~DesignConversion"` | 0 | Passed 247/247. |

## Commands Run With Exit Codes

| # | Command or action | Exit | Evidence summary |
| ---: | --- | ---: | --- |
| 1 | Workspace, skills, and evidence directory listing | 0 | Confirmed required paths and Pass208 artifact exist. |
| 2 | Initial skill-load status script | 1 | PowerShell parse error before usable skill evidence; retried with corrected script. |
| 3 | Corrected skill-load status script | 0 | Required guidance files found/read with byte counts and SHA256 hashes. |
| 4 | Read Pass208, current ProjectWiki test file, and broad symbol search | 0 | Confirmed accepted Pass208 and target test; broad search output was tool-truncated and not used for exact file claims. |
| 5 | Target method locations, noisy implementation search, and `git status --short` | 0 | Found target methods and confirmed dirty worktree with pre-existing unrelated changes. |
| 6 | Target test line-range readback and current test diff | 0 | Confirmed S7 tests and pre-existing bounded runner patterns. |
| 7 | Exact source-file discovery | 0 | Found `ProjectWikiTrainingFlowAdapter`, `ProjectWikiTrainingOrchestrator`, `CodeTraceBuilderTrainingPipeline`, `ProjectWikiDpoMetricRunner`, and `LogicalOrchestratorAssistant`. |
| 8 | Source file reads | 0 | Read the assistant, flow adapter, training orchestrator, CodeTrace pipeline, and DPO metric runner. |
| 9 | Pre-fix focused adaptive route reproduction | 1 | 90-second blame-hang aborted on the exact adaptive route test. |
| 10 | Pre-fix sequence and process checks | mixed | Sequence confirmed the exact incomplete test; no stale testhost or vstest process remained. |
| 11 | `apply_patch` assistant seam | 0 | Added optional ProjectWiki training adapter parameter to `CreateDefault`. |
| 12 | `apply_patch` test bounded injection | 0 | Injected bounded CodeTrace runner into flow adapter and adaptive assistant tests. |
| 13 | Post-fix focused adaptive route | 0 | Passed 1/1. |
| 14 | S7 training-flow/orchestrator segment | 1 | Passed 3 tests, then aborted on explicit assistant route. |
| 15 | S7 sequence and process checks | mixed | Sequence confirmed adaptive route completed true and explicit route completed false; only Roslyn compiler `dotnet.exe` remained. |
| 16 | DesignConversion regression | 0 | Passed 247/247. |
| 17 | Focused direct flow adapter | 0 | Passed 1/1. |
| 18 | Diff and changed-line readback | 0 | Read back the assistant seam and test injections. |
| 19 | `apply_patch` evidence artifact create | 0 | Created the single requested Pass209 artifact. |
| 20 | `apply_patch` evidence artifact verification-record update | 0 | Added post-write verification records to the same artifact. |
| 21 | Final artifact marker verification | 0 | Verdict-field count 1; identity-line count 1; bad identity count 0; conflict marker count 0; mojibake marker count 0. |
| 22 | `git diff --check -- 15.OrchestratorModel\Logical\LogicalOrchestratorAssistant.cs Tests\ProjectWikiDomainTests.cs docs\evidence\design-conversion\codex-pass209-projectwiki-logical-orchestrator-adaptive-route-repair.md` | 0 | No whitespace errors reported for changed files. |

## Files Changed

| File | Change |
| --- | --- |
| `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs` | Pass209 added a backward-compatible optional ProjectWiki training adapter seam in `CreateDefault`; default production behavior remains unchanged when the parameter is omitted. |
| `Tests/ProjectWikiDomainTests.cs` | Pass209 injected a bounded CodeTrace DPO runner through existing constructors for the direct flow-adapter test and the adaptive assistant CodeTrace training test. This file already contained pre-existing changes from earlier passes. |
| `docs/evidence/design-conversion/codex-pass209-projectwiki-logical-orchestrator-adaptive-route-repair.md` | This single intentional Pass209 evidence artifact. |

Test commands also generated `Tests/TestResults/...` blame-hang attachments. Those are command-generated test outputs, not an intentional docs evidence artifact.

## Next Timeout Or Failure Identified

The next exact S7 blocker is:

`LogicalOrchestratorAssistant_RoutesExplicitProjectWikiTrainingRequestToTrainingFlow`

S7 sequence evidence:

- `ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToCodeTraceTraining` completed true.
- `ProjectWikiTrainingOrchestrator_DoesNotRunTrainingForWikiQuestionRoute` completed true.
- `LogicalOrchestratorAssistant_AdaptivelyRoutesProjectWikiTrainingRequestToLearningFlow` completed true.
- `LogicalOrchestratorAssistant_RoutesExplicitProjectWikiTrainingRequestToTrainingFlow` completed false.

## Verification

Verified:

- Exact focused adaptive route passed 1/1 after the patch.
- Direct flow-adapter route touched by the patch passed 1/1.
- S7 advanced past the repaired adaptive route and identified the next exact blocker.
- DesignConversion regression filter passed 247/247.
- Post-write artifact marker verification passed.
- Changed-file `git diff --check` passed.

Not verified:

- S7 did not pass as a whole.
- Full `ProjectWikiDomainTests` class was not run.
- Full local suite was not run.
- Later ProjectWiki segments were not run because S7 found the next blocker.
- CI, deployment, cloud, provider execution, public endpoint, production checkpoint readiness, git publication, PR readiness, and product completion were not verified.

## Remaining Broad Suite Blockers

| Scope | Current evidence | Status |
| --- | --- | --- |
| Adaptive assistant CodeTrace training route | Focused test passed 1/1; S7 sequence marks it completed true. | repaired in Pass209 |
| Direct flow adapter CodeTrace training route | Focused test passed 1/1. | repaired/verified in Pass209 |
| Explicit assistant AnswerVerifier training route | S7 sequence marks it completed false after 120-second blame-hang. | next exact blocker |
| S7 training-flow/orchestrator segment | Advanced but did not clear. | blocked by explicit assistant route |
| Later ProjectWiki segments | Not run after S7 blocker. | unverified |
| Full ProjectWiki class | Not run. | not claimed |
| DesignConversion regression | Passed 247/247. | selected regression passed |
| Full local suite | Not run. | not claimed |

## Next Recommended Pass

Pass210 should target `LogicalOrchestratorAssistant_RoutesExplicitProjectWikiTrainingRequestToTrainingFlow`: reproduce the exact focused timeout, inspect the explicit scenario path for AnswerVerifier/default DPO metric runner cost, and apply one minimal bounded runner injection or optional seam only if the root cause is the default heavy training/metric path.

pass209_verdict: PASS_PROJECTWIKI_NEXT_BLOCKER_IDENTIFIED_BOUNDED
