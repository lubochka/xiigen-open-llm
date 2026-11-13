# Codex Pass235 ProjectWiki Training/Routing Repair Evidence

claim_scope: Bounded local repair for the Pass232 ProjectWiki request-routing, DocumentationGenerator DPO metric, and WikiTraceAggregator DPO metric failures.
evidence_scope: Current-turn file reads/edits, focused dotnet builds/tests, metric artifact readbacks, token-id diagnostics, and the required combined focused filter. Full suite, ProductReady, cloud, deployment, public endpoint, provider, git publication, and mergeability were not verified.
count_claimed: 1 required combined focused verification run; 3 individual focused target reruns after fixes. This is not a 50-pass cycle claim.
independent_evidence_count: 4 target test records used for acceptance: route individual pass, documentation individual pass, trace individual pass, required combined pass.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: no for this bounded three-test ProjectWiki cluster; yes for any broader readiness claim.

verdict: PASS

## skill_load_status

All required repo-local guidance/skill files found by search were read before product repair. Missing exact external skills were not used as blockers because repo-local equivalents were present.

| group | status |
| --- | --- |
| AGENTS.md | found/read |
| SESSION-START-PROMPT and planning-session-startup | found/read |
| SK-INTENT architect behavior classifier | found/read |
| SK-PLAN authority binding, reference inventory/source map, phase planning | found/read |
| SK-REVIEW plan-review, goal delivery completeness, claim verification, audit, specificity calibration, output skepticism, reconnaissance gate | found/read |
| SK-ARCH trainable architecture decision, no product decisions, output readability gate | found/read |
| QA/code-review/test integrity/self-verification guides | found/read |

## focused_failures_before

The required focused filter was run before the repair and failed 3/3:
- `WikiTraceAggregatorTrainingPipeline_RunsBatchGateAndPublishesManifest`: heldOutAccuracy=0, heldOutRewardMargin=-1.300042.
- `ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining`: expected next action `Keep ProjectWikiRequestRouterUnit unpublished...`, actual `Add 50-row remediation batch for ProjectWikiRequestRouterUnit and rerun.`
- `DocumentationGeneratorTrainingPipeline_RunsBatchGateAndPublishesManifest`: heldOutAccuracy=0.32, negative heldOutRewardMargin.

## repair_summary

The failing DPO metric rows used domain-specific preference marker vocabularies that were unstable under the small ProjectWiki DPO harness configurations. Route and documentation were repaired by projecting their DPO metric rows through the same stable preference scaffold already proven by the CodeTrace DPO metric path, while preserving each unit's own `UnitId`, `DEFECT_TYPE`, and domain-specific correction/defect tokens.

The trace aggregator also used `contextLen: 16` in its focused test, leaving only 8 scored answer tokens. Its 12 repeated preference markers crowded out trace details. The repair reduced trace preference marker repetition to 1 and renamed the two retained trace tokens that collided or underperformed in vocab-256 (`missing_remediation`, `ignores_failed_unit`) so the held-out metric scores actual trace correction/defect evidence.

## changed_files

- `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs`
- `42.ProjectWikiDomain/DocumentationGeneratorTrainingPipeline.cs`
- `42.ProjectWikiDomain/WikiTraceAggregatorTrainingPipeline.cs`
- `42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs` remains dirty in the worktree with existing configurable metric-runner constructor support; the diagnostic seed edit made during this pass was restored to unit-specific initialization, so there is no remaining seed behavior change claimed here.
- `docs/evidence/design-conversion/codex-pass235-projectwiki-training-routing-repair.md`

## tests_run

- `dotnet build Tests\LlmFromScratch.Tests.csproj` after repairs: exit 0, with existing ProjectWiki nullable warnings in `OrchestrationPilotLoopRunner.cs` and `ProjectWikiPluginOrchestrationAdapter.cs`.
- Individual route focused rerun after repair: passed 1/1.
- Individual documentation focused rerun after repair: passed 1/1.
- Individual trace focused rerun after repair: passed 1/1.
- Required combined filter:
  `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ProjectWikiDomainTests.WikiTraceAggregatorTrainingPipeline_RunsBatchGateAndPublishesManifest|FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiTrainingOrchestrator_RoutesNaturalPromptToRequestRouterTraining|FullyQualifiedName~ProjectWikiDomainTests.DocumentationGeneratorTrainingPipeline_RunsBatchGateAndPublishesManifest" --logger "console;verbosity=minimal"`
  Result: Failed 0, Passed 3, Skipped 0, Total 3.

## focused_results_after

The required combined focused filter passed all three target tests. The full suite was not run in this pass.

## remaining_risks

- The repair verifies local deterministic DPO metric behavior for the three Pass232 ProjectWiki failures only.
- The shared `CODE_TRACE_DPO` scaffold token is used as a stable local DPO preference vocabulary; this does not claim the route, documentation, or trace units are CodeTrace units.
- No ProductReady, cloud, deployment, public endpoint, external provider, git publication, or full-suite readiness claim is made.
