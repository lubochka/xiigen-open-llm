# Codex Pass121 - Visual AI Provider Boundary

claim_scope:
Pass121 implemented and verified one bounded DesignConversion product slice after accepted Pass120: a visual-AI/provider comparison boundary that can feed the current local render QA probe and in-process ModelServer metadata without faking real external AI or network execution.

evidence_scope:
Current-turn partial/full reads of the required skill stack, Pass119/Pass120 evidence, targeted DesignConversion source/tests, focused code edits, focused dotnet test runs, and this evidence packet. No real external visual AI provider, public HTTP endpoint, cross-repo caller, upload/deployment, git action, or xiigen worktree action was run.

count_claimed:
1 bounded Pass121 implementation/evidence slice.

independent_evidence_count:
10 evidence groups after final post-write verification: required skill inventory/readbacks; Pass119/Pass120 evidence reads; targeted source/test reads; comparison-artifact metadata extension; new visual-AI comparison-provider adapter; local render QA probe metadata propagation; ModelServer API metadata propagation; new Pass121 tests plus API assertion; focused dotnet test outputs; this evidence file plus post-write readback/scans/diff-check.

subagent_id_or_NONE:
UNKNOWN_IN_SUBAGENT

subagent_identity_self_check:
spawned_worker=true; used_NONE=false; parent_must_reconcile=true

downgrade_needed:
No for the narrow Pass121 provider-boundary/no-key/fake-client mapping proof. Yes for real external AI execution, real network/provider calls, public HTTP, cross-repo caller wiring, upload/deployment, full product readiness, broad CI, and trained-model promotion.

skill_load_status:
Large governance files were found and partially read through current-turn inventory, headers, targeted gate readback, and task-relevant sections. Focused QA/DesignConversion guides and no-product-decisions were read directly. Impact of partial reads: enough for this bounded implementation slice's no-permission, claim-honesty, no-overclaim, render-QA, visual-provider, and deterministic-boundary gates; not claimed as a complete semantic review of every large governance file.

| File | Status |
| --- | --- |
| `AGENTS.md` | found/partial_read; large file; targeted claim, sub-agent, no-permission, readiness, and human-override sections read. |
| `docs/SESSION-START-PROMPT.md` | found/partial_read; targeted startup and evidence-honesty gates read. |
| `docs/skills/planning-session-startup-SKILL.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/architect-behavior-classifier-SKILL.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/authority-requirement-binding-SKILL.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/reference-inventory-and-source-map-SKILL.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/phase-planning-SKILL.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/codex-planning-quality-GUIDE.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/plan-review-SKILL.md` | found/partial_read; very large file; targeted review/claim gates read. |
| `docs/skills/goal-delivery-completeness-SKILL.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/claim-verification-SKILL.md` | found/partial_read; targeted claim-verification gates read. |
| `docs/skills/audit-protocol-SKILL.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/specificity-calibration-SKILL.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/output-skepticism-SKILL.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/reconnaissance-gate-SKILL.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/trainable-architecture-decision-SKILL.md` | found/partial_read; header/inventory plus targeted architecture-boundary readback. |
| `docs/skills/no-product-decisions-SKILL.md` | found/read; full file read. |
| `docs/skills/output-readability-gate-SKILL.md` | found/partial_read; header/inventory plus targeted gate readback. |
| `docs/skills/qa-session-type-SKILL.md` | found/read; full file read. |
| `docs/skills/generated-code-review-SKILL.md` | found/partial_read; header/inventory plus targeted generated-code-review readback. |
| `docs/skills/html-css-render-qa-automation-GUIDE.md` | found/read; full file read. |
| `docs/skills/design-conversion-ilearningmachine-architecture-GUIDE.md` | found/read; full file read. |

files_read:
- `docs/evidence/design-conversion/codex-pass119-remaining-gap-inventory.md`
- `docs/evidence/design-conversion/codex-pass120-browser-backed-render-qa-through-probe.md`
- `44.DesignConversionDomain/DesignConversionQaProviders.cs`
- `44.DesignConversionDomain/DesignConversionQaContracts.cs`
- `44.DesignConversionDomain/DesignConversionVisualJudge.cs`
- `44.DesignConversionDomain/DesignConversionVisualQaBoundaryPolicy.cs`
- `44.DesignConversionDomain/DesignConversionLocalRenderQaProbe.cs`
- `44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs`
- `44.DesignConversionDomain/DesignConversionInternalService.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs`
- `Tests/DesignConversionLocalRenderQaProbeTests.cs`
- `Tests/DesignConversionBrowserRenderQaProbeIntegrationTests.cs`
- `Tests/DesignConversionVisualJudgeTests.cs`
- `Tests/DesignConversionModelServerApiTests.cs`

files_changed:
- `44.DesignConversionDomain/DesignConversionQaContracts.cs`
- `44.DesignConversionDomain/DesignConversionVisualAiComparisonProvider.cs`
- `44.DesignConversionDomain/DesignConversionLocalRenderQaProbe.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs`
- `Tests/DesignConversionVisualAiComparisonProviderTests.cs`
- `Tests/DesignConversionModelServerApiTests.cs`
- `docs/evidence/design-conversion/codex-pass121-visual-ai-provider-boundary.md`

commands_run:
- `rg -n "DesignToCode|DesignConversion|Pass119|Pass120|Pass121|PASS27_ORIGINAL_TASK_COVERAGE|llm_mvp_core" C:\Users\lubao\.codex\memories\MEMORY.md`
- Required skill file inventory/readback commands with `Get-Item`, `Get-Content`, and `Select-String`; one broad `Select-String` timed out after partial output, and one inventory loop had a PowerShell parser slip before rerun succeeded.
- `rg`/`Get-Content` readbacks for Pass119/Pass120 evidence and targeted DesignConversion source/tests.
- `dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionVisualAiComparisonProviderTests" --no-restore --logger "console;verbosity=minimal"` -> Passed: 2, Failed: 0, Skipped: 0.
- `dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionLocalRenderQaProbeTests|FullyQualifiedName~DesignConversionModelServerApiTests|FullyQualifiedName~DesignConversionVisualJudgeTests" --no-restore --logger "console;verbosity=minimal"` -> Passed: 19, Failed: 0, Skipped: 0.
- `dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionLocalRenderQaProbeTests|FullyQualifiedName~DesignConversionModelServerApiTests|FullyQualifiedName~DesignConversionVisualJudgeTests|FullyQualifiedName~DesignConversionVisualAiComparisonProviderTests" --no-restore --logger "console;verbosity=minimal"` -> Passed: 21, Failed: 0, Skipped: 0.
- Post-write conflict marker scan on touched files -> `conflict_marker_count=0`.
- Post-write mojibake marker scan on touched files -> `mojibake_marker_count=0`.
- Post-write key evidence readback with `Select-String` confirmed `claim_scope`, `count_claimed`, no-key path, fake-client path, and boundaries-not-proven lines.
- `git diff --check -- 44.DesignConversionDomain\DesignConversionQaContracts.cs 44.DesignConversionDomain\DesignConversionVisualAiComparisonProvider.cs 44.DesignConversionDomain\DesignConversionLocalRenderQaProbe.cs 25.ModelServer\DesignConversion\DesignConversionModelServerApi.cs Tests\DesignConversionVisualAiComparisonProviderTests.cs Tests\DesignConversionModelServerApiTests.cs docs\evidence\design-conversion\codex-pass121-visual-ai-provider-boundary.md` -> exit 0, no output.

implementation_summary:
- Added optional `Metadata` to `DesignConversionQaComparisonArtifact` without changing existing constructor call sites.
- Added `DesignConversionExternalVisualAiComparisonProvider`, an adapter from `IDesignConversionVisualJudge` to `IDesignConversionComparisonProvider`.
- The adapter defaults to `NoConfiguredVisualJudge`, returns a boundary artifact instead of calling network, and records `external_visual_ai_configured`, `external_visual_ai_executed`, `visual_ai_provider_contract_executed`, `real_external_provider_executed`, `network_call_executed`, `fake_client`, `blocked_reason`, provider/model, score, defect count, estimated cost, and refs-only/no-raw-payload markers.
- Extended `DesignConversionLocalRenderQaProbe` to propagate comparison-artifact metadata into viewport comparison refs and aggregate visual-AI provider/config/cost/fake/network state into `BoundaryMetadata`.
- Extended `DesignConversionModelServerApi` boundary metadata with visual-judge status, provider boundary, configured/executed/blocked state, blocked reason, and estimated visual-judge cost.
- Added focused Pass121 tests for no-key blocked state and configured fake-client mapping, plus a ModelServer API assertion for provider metadata propagation.

verification_results:
- New Pass121 test filter: 2 passed, 0 failed.
- Related regression/API filter before API assertion: 19 passed, 0 failed.
- Related regression/API/new-test filter after API assertion: 21 passed, 0 failed.
- Existing warning output remained from unrelated tests/analyzers, e.g. nullable and xUnit analyzer warnings in other test files; no Pass121 test failure.
- Post-write conflict marker scan: `conflict_marker_count=0`.
- Post-write mojibake marker scan: `mojibake_marker_count=0`.
- Post-write key evidence readback: claim/count/no-key/fake-client/boundaries lines present.
- Touched-file `git diff --check`: exit 0, no output.

coverage_update_for_original_requirements:
- Requirement 4 visual checking by available AI: narrow boundary proof improved. The product now has a comparison-provider seam that can carry no-key blocked state or fake-client mapping through the current render QA probe without presenting local/fake code as real external AI.
- Requirement 5 cost/provider threshold loop support: metadata now carries provider name/model, configured/executed/blocked flags, fake/network flags, and estimated cost so later tier/cost policy can consume it.
- Requirement 3 browser-backed render QA from Pass120 remains preserved; Pass121 did not rerun the browser-backed integration test and does not add new browser evidence.

visual_ai_boundary_evidence:
- No-key path: `DesignConversionExternalVisualAiComparisonProvider` defaulted to `NoConfiguredVisualJudge`; probe result was success with failed QA verdict and metadata `external_visual_ai_configured=False`, `external_visual_ai_executed=False`, `external_visual_ai_blocked=True`, `blocked_reason=not_configured`, `network_call_executed=False`, `fake_client=False`, `real_external_provider_executed=False`, `visual_ai_executed=False`, `visual_ai_not_executed=True`, `estimated_cost_usd=0`.
- Fake-client path: fake judge mapped score `0.88`, one `spacing` defect, provider `fake-vision-provider`, model `fake-vision-model-v1`, cost `0.0042`, and token metrics into the comparison artifact and probe metadata. It explicitly reported `external_visual_ai_executed=False`, `network_call_executed=False`, `fake_client=True`, and `real_external_provider_executed=False`.
- API propagation: `DesignConversionModelServerApiTests.SubmitAndEvaluateAsync_ValidRequest_CallsRuntimeServiceAndReturnsEvaluationEvidence` now asserts `visual_judge_status`, `visual_judge_provider_boundary`, `external_visual_ai_executed=False`, and `estimated_visual_judge_cost_usd=0`.
- Raw payload boundary: tests serialize probe/comparison results and fake judge requests and assert private HTML/CSS markers are absent.

boundaries_not_proven:
- Real external AI execution is not proven.
- Real network/provider calls are not proven and were not attempted.
- Browser-backed Pass120 integration was not rerun in Pass121.
- Public HTTP endpoint, cross-repo caller wiring, upload/deployment, product readiness, broad CI, trained-model promotion, and real provider credentials are not proven.
- The fake-client path proves mapping only; it is not a claim that external AI judged screenshots.

next_single_pass_recommendation:
Run a single bounded pass for either a local HTTP host seam around the already in-process ModelServer surface, or a provider-configuration validation/env-binding pass that remains no-network unless Luba explicitly authorizes a real provider call. The next pass should not claim real external AI unless credentials, provider terms, network execution, and returned artifacts are actually verified.

packet_self_check:
- `subagent_id_or_NONE` is `UNKNOWN_IN_SUBAGENT`, not `NONE`.
- `subagent_identity_self_check` is present and says spawned_worker=true; used_NONE=false; parent_must_reconcile=true.
- `count_claimed` is 1 bounded Pass121 slice, not full product completion and not 121 passes.
- The evidence does not claim real external AI, network provider calls, public HTTP, cross-repo caller wiring, upload/deployment, broad CI, or product readiness.
- A ledger/checklist row is not counted as a pass; the claimed pass is this implemented and tested bounded product slice.
