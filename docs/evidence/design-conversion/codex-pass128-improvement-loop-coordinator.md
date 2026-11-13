# Codex Pass128 - Improvement Loop Coordinator

claim_scope:
Pass128 implemented and focused-tested one bounded local DesignConversion improvement-loop coordinator slice for original Requirement 5. The slice coordinates repeated local `DesignConversionInternalService.EvaluateAsync` attempts through an injected local candidate strategy until the current visual QA tier policy says to stop, or until a local stop condition is reached. It does not call real external AI/network providers, start a public/socket host, touch external caller repos, write `C:\Users\lubao\Documents\xiigen`, edit `claude-*` files, perform git actions, or claim full product readiness.

evidence_scope:
Current-turn memory quick pass for repo governance reminders; required stack inventory plus targeted task-relevant gate readback before coding; full reads of Pass127/Pass120/Pass121 local evidence files; targeted read-only external Claude evidence for Requirement 5 semantics; full reads of the local DesignConversion service/policy/contracts/render-probe files and relevant tests; new coordinator code; new focused tests; focused test run; preliminary changed-file scans. Final post-write scans are recorded below after this artifact is written and re-read.

count_claimed:
1 Pass128 local implementation/evidence slice. 1 new coordinator file. 1 new focused test file with 8 passing focused tests. 0 full-readiness claims. 0 external provider executions. 0 public HTTP/socket host executions. 0 cross-repo caller integrations.

independent_evidence_count:
9 current-turn evidence groups: memory quick pass; required stack existence/readback; Pass127/Pass120/Pass121 evidence reads; Requirement 5 targeted external Claude readback; local source reads; relevant existing test/provider reads; implementation edit; focused test run; changed-file hygiene scans.

subagent_id_or_NONE:
UNKNOWN_IN_SUBAGENT

subagent_identity_self_check:
spawned_worker=true; used_NONE=false; parent_must_reconcile=true

downgrade_needed:
No for the narrow Pass128 local coordinator/control-loop slice. Yes for full product readiness, real external visual AI, cross-repo caller wiring, public HTTP/socket host, production DB, upload/deployment, and broad CI.

skill_load_status:
All requested stack files were found before coding. Large governance files were read through inventory plus targeted task-relevant gate readback; smaller DesignConversion QA/domain guides were included in the same readback. This supports this bounded local implementation slice and no-overclaim gates, not a comprehensive semantic audit of every large governance file.

| Group | File | Status |
| --- | --- | --- |
| SK-SESSION | `AGENTS.md` | found/targeted_read; current user-pasted rules plus workspace targeted readback. |
| SK-SESSION | `docs/SESSION-START-PROMPT.md` | found/targeted_read. |
| SK-SESSION | `docs/skills/planning-session-startup-SKILL.md` | found/targeted_read. |
| SK-INTENT | `docs/skills/architect-behavior-classifier-SKILL.md` | found/targeted_read. |
| SK-PLAN | `docs/skills/authority-requirement-binding-SKILL.md` | found/targeted_read. |
| SK-PLAN | `docs/skills/reference-inventory-and-source-map-SKILL.md` | found/targeted_read. |
| SK-PLAN | `docs/skills/phase-planning-SKILL.md` | found/targeted_read. |
| SK-PLAN | `docs/skills/codex-planning-quality-GUIDE.md` | found/targeted_read. |
| SK-REVIEW | `docs/skills/plan-review-SKILL.md` | found/targeted_read. |
| SK-REVIEW | `docs/skills/goal-delivery-completeness-SKILL.md` | found/targeted_read. |
| SK-REVIEW | `docs/skills/claim-verification-SKILL.md` | found/targeted_read. |
| SK-REVIEW | `docs/skills/audit-protocol-SKILL.md` | found/targeted_read. |
| SK-REVIEW | `docs/skills/specificity-calibration-SKILL.md` | found/targeted_read. |
| SK-REVIEW | `docs/skills/output-skepticism-SKILL.md` | found/targeted_read. |
| SK-REVIEW | `docs/skills/reconnaissance-gate-SKILL.md` | found/targeted_read. |
| SK-ARCH | `docs/skills/trainable-architecture-decision-SKILL.md` | found/targeted_read. |
| SK-ARCH | `docs/skills/no-product-decisions-SKILL.md` | found/targeted_read. |
| SK-ARCH | `docs/skills/output-readability-gate-SKILL.md` | found/targeted_read. |
| SK-ARCH | `docs/skills/design-conversion-ilearningmachine-architecture-GUIDE.md` | found/targeted_read. |
| SK-QA/CODE | `docs/skills/qa-session-type-SKILL.md` | found/targeted_read. |
| SK-QA/CODE | `docs/skills/html-css-render-qa-automation-GUIDE.md` | found/targeted_read. |
| SK-QA/CODE | `docs/skills/generated-code-review-SKILL.md` | found/targeted_read. |

files_read:
- `docs/evidence/design-conversion/codex-pass127-current-gap-review-next-fix.md`
- `docs/evidence/design-conversion/codex-pass120-browser-backed-render-qa-through-probe.md`
- `docs/evidence/design-conversion/codex-pass121-visual-ai-provider-boundary.md`
- `44.DesignConversionDomain/DesignConversionInternalService.cs`
- `44.DesignConversionDomain/DesignConversionVisualQaBoundaryPolicy.cs`
- `44.DesignConversionDomain/DesignConversionQaContracts.cs`
- `44.DesignConversionDomain/DesignConversionLocalRenderQaProbe.cs`
- `44.DesignConversionDomain/DesignConversionContracts.cs`
- `44.DesignConversionDomain/DesignConversionQaProviders.cs`
- `44.DesignConversionDomain/DesignConversionVisualJudge.cs` targeted first-section read
- `44.DesignConversionDomain/DesignConversionDpoGenerator.cs` targeted leakage-guard read
- `Tests/DesignConversionInternalServiceTests.cs`
- `Tests/DesignConversionVisualQaBoundaryPolicyTests.cs`
- `Tests/DesignConversionQaProviderTests.cs`
- `Tests/LlmFromScratch.Tests.csproj`
- Read-only targeted Requirement 5 lines from the three external Claude files under `C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\`.

files_changed:
- `44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs`
- `Tests/DesignConversionImprovementLoopCoordinatorTests.cs`
- `docs/evidence/design-conversion/codex-pass128-improvement-loop-coordinator.md`

commands_run:
- `rg -n "DesignConversion|Pass127|Pass128|Requirement 5|llm_mvp_core|PLAN_NOT_READY|sub-agent" C:\Users\lubao\.codex\memories\MEMORY.md`
- `rg -n "DesignConversion|Pass127|Pass128|Requirement 5" C:\Users\lubao\.codex\memories\rollout_summaries -g "*.md"`
- Required stack/context file inventory using `Test-Path`, `Get-Content`, and line counts.
- Targeted `rg` readback across `AGENTS.md`, session prompt, and requested skill/guide files for DesignConversion, Requirement 5, no-overclaim, provider, budget, render, telemetry, and generated-code QA gates.
- `Get-Content` full reads for Pass127/Pass120/Pass121 evidence and local DesignConversion source/test files listed above.
- Targeted `rg` over the three external Claude files for Requirement 5, quality-loop, threshold, cost, and budget semantics.
- `dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionImprovementLoopCoordinatorTests" --no-restore --logger "console;verbosity=minimal"` first run: failed at compile due a test helper naming collision (`JudgeScript.Score`/`Blocked` static helpers collided with record members); no product claim was made from that run.
- Same focused `dotnet test` command after local test-helper fix: Passed 8, Failed 0, Skipped 0, Total 8, Duration 458 ms.
- Preliminary conflict-marker scan on new code/test files: 0.
- Preliminary mojibake-marker scan on new code/test files: 0.
- Preliminary `git diff --check -- 44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs Tests/DesignConversionImprovementLoopCoordinatorTests.cs`: exit 0/no output.

tests_run:
Focused command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionImprovementLoopCoordinatorTests" --no-restore --logger "console;verbosity=minimal"
```

Result after the local helper fix: Passed 8, Failed 0, Skipped 0, Total 8.

implementation_summary:
- Added `DesignConversionImprovementLoopCoordinator`, `DesignConversionImprovementLoopRequest`, `DesignConversionCandidateUpdateRequest`, `IDesignConversionImprovementCandidateStrategy`, attempt telemetry, and loop result records.
- The coordinator runs `DesignConversionInternalService.EvaluateAsync` for the current local candidate, reads `DesignConversionVisualQaBoundaryTrace.TierPolicy`, and calls the injected candidate strategy only when `ShouldContinue` is true and coordinator `MaxAttempts` has not been reached.
- Stop reasons are explicit: `quality_reached`, `provider_required`, `render_manifest_incomplete`, `budget_cap_reached`, `max_attempts_reached`, `cancelled`, and `candidate_strategy_failed`.
- Public loop/attempt telemetry includes refs/hashes/scores/status fields and explicit false markers for raw HTML, raw CSS, raw design tree, screenshot bytes, product readiness, trained feature claim, and external AI execution.
- The implementation reuses the existing internal service/policy boundary and does not call external providers, start a host, or integrate external repos.

mental_simulation_results:
| Flow | Test coverage | Result |
| --- | --- | --- |
| Attempt 1 below threshold, strategy updates candidate, attempt 2 reaches quality | `RunAsync_UpdatesCandidateAndStopsWhenSecondAttemptReachesQuality` | 2 attempts, 1 strategy call, stop `quality_reached`, completed true for the narrow loop. |
| Cost/budget cap prevents extra attempt | `RunAsync_StopsOnBudgetCapWithoutRequestingAnotherCandidate` | 1 attempt, 0 strategy calls, stop `budget_cap_reached`. |
| Provider required/provider blocked | `RunAsync_StopsOnProviderRequiredWithSanitizedBlockedTelemetry` | 1 attempt, 0 strategy calls, stop `provider_required`, external AI execution false. |
| Render manifest incomplete | `RunAsync_StopsOnRenderManifestIncomplete` | 1 rendered viewport from a two-viewport request, stop `render_manifest_incomplete`. |
| Max attempts reached while policy would continue | `RunAsync_StopsAtCoordinatorMaxAttemptsEvenWhenPolicyWouldContinue` | 1 attempt, 0 strategy calls, stop `max_attempts_reached`. |
| Cancellation observed without completion claim | `RunAsync_ObservesCancellationWithoutCompletionClaim` | 0 attempts, stop `cancelled`, completed false. |
| Public telemetry contains refs/hashes/scores/status only | `RunAsync_PublicTelemetryContainsOnlyRefsHashesScoresAndStatus` | Public metadata/evidence/attempt refs do not contain private HTML/CSS/tree markers or data-image bytes. |
| Candidate strategy failure | `RunAsync_StopsCleanlyOnCandidateStrategyFailure` | 1 attempt, 1 strategy call, stop `candidate_strategy_failed`, completed false. |

verification_results:
- Focused tests passed: 8 passed, 0 failed, 0 skipped.
- Preliminary conflict-marker scan on new code/test files: 0.
- Preliminary mojibake-marker scan on new code/test files: 0.
- Preliminary code/test `git diff --check`: exit 0/no output.
- Final post-write artifact readback completed and displayed the required packet sections.
- Final section-header readback found `claim_scope`, `evidence_scope`, `count_claimed`, `independent_evidence_count`, `subagent_id_or_NONE`, `subagent_identity_self_check`, `downgrade_needed`, `skill_load_status`, `files_read`, `files_changed`, `commands_run`, `tests_run`, `implementation_summary`, `mental_simulation_results`, `verification_results`, `remaining_gaps`, and `next_single_pass_recommendation`.
- Final conflict-marker scan over all changed files: 0.
- Final mojibake-marker scan over all changed files: 0.
- Final `git diff --check -- 44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs Tests/DesignConversionImprovementLoopCoordinatorTests.cs docs/evidence/design-conversion/codex-pass128-improvement-loop-coordinator.md`: exit 0/no output.

remaining_gaps:
- This pass proves only the local coordinator/control-loop slice. It does not prove real external visual AI, real network/provider execution, public HTTP/socket host, cross-repo FigmaToCode/CanvaToCode caller integration, production DB, upload/deployment, broad CI, or full product readiness.
- The loop depends on injected local candidate strategy behavior; it does not prove a trained external improver or real visual-AI-generated fix.
- Browser-backed render and visual-provider boundaries remain local slices from prior evidence; this pass did not rerun browser integration.
- ModelServer/caller status/readback envelope and broader default composition remain safe follow-up gaps.

next_single_pass_recommendation:
Run one bounded follow-up pass to connect this local coordinator to the in-process ModelServer/local caller surface as a refs-only optional route or status/readback envelope, still without public host, external repo edits, external AI calls, upload/deployment, or full readiness claims.
