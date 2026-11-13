# Pass201 ProjectWiki Hang Repair

claim_scope: Pass201 bounded repair for `ProjectWikiDomainTests.CodeTraceBuilderTrainingPipeline_SkipsPublicationUntilPromotionConfirmation`; not product complete, not 50/50, not full CI, not deployment, not cloud/provider/public endpoint readiness.
evidence_scope: Fresh-read project rules and requested skills, Pass200 recovery artifact, two blame-hang sequence XML files, ProjectWiki test/pipeline/runner/metric code, focused timeout reproduction, exact stale process cleanup, minimal code/test patch, focused ProjectWiki verification, CodeTraceBuilder cluster verification, DesignConversion filter verification, and post-fix diff/process checks.
count_claimed: 1 Pass201 bounded pass.
independent_evidence_count: 13 bounded evidence groups.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
spawned_worker_identity_self_check: spawned_worker=true; used_NONE=false; used_false_no_subagent_claim=false; parent_must_reconcile=true
nested_subagent_usage: No nested sub-agent was used by this worker.
downgrade_needed: Yes. The repaired gate is the focused ProjectWiki CodeTraceBuilder hang/abort. The whole ProjectWikiDomainTests class filter timed out and the broader suite remains unclaimed.

## Skill Load Status

| File | Status | Evidence |
| --- | --- | --- |
| `AGENTS.md` | found/read | Fresh `Get-Content -Raw` completed. |
| `docs/SESSION-START-PROMPT.md` | found/read | Fresh `Get-Content -Raw` completed. |
| `CLAUDE.md` | found/read | Fresh `Get-Content -Raw` completed because the session prompt requires it. |
| `docs/skills/claim-verification-SKILL.md` | found/read | Fresh `Get-Content -Raw` completed. |
| `docs/skills/audit-protocol-SKILL.md` | found/read | Fresh `Get-Content -Raw` completed. |
| `docs/skills/output-skepticism-SKILL.md` | found/read | Fresh `Get-Content -Raw` completed. |
| `docs/skills/implementation-integrity-SKILL.md` | found/read | Fresh `Get-Content -Raw` completed. |
| `docs/skills/test-integrity-SKILL.md` | found/read | Fresh `Get-Content -Raw` completed. |
| `docs/skills/integration-testing-SKILL.md` | found/read | Fresh `Get-Content -Raw` completed. |
| `docs/skills/generated-code-review-SKILL.md` | found/read | Present QA skill; fresh `Get-Content -Raw` completed. |
| `docs/skills/code-examination-SKILL.md` | found/read | Present QA skill; fresh `Get-Content -Raw` completed. |
| `docs/skills/test-failure-triage-SKILL.md` | found/read | Present QA skill; fresh `Get-Content -Raw` completed. |
| `docs/skills/e2e-test-matrix-SKILL.md` | found/read | Present QA skill; fresh `Get-Content -Raw` completed. |
| `docs/skills/dev-safety-SKILL.md` | found/read | Executor safety skill loaded as session guidance. |
| `docs/skills/code-execution-SKILL.md` | found/read | Executor safety skill loaded as session guidance. |

## Root Cause

The failing test was not an infinite loop in the generator or publication assertion path. The pipeline is bounded: it builds 50 training rows, builds 50 held-out rows, runs `CodeTraceBuilderTrainingRunner`, then runs `ProjectWikiDpoMetricRunner` before checking the publication gate.

The blocker was that this publication-gate test used the production-sized DPO metric runner: context length 192, vocabulary 2048, hidden dimension 64, and 8 epochs. That makes the metric runner perform many transformer forward/backward/evaluation passes with no test output while it works. The focused pre-fix run did not complete before the 304-second tool timeout, and the prior Pass200 blame-hang runs aborted after inactivity.

The safe repair was to keep production defaults unchanged, expose optional metric-runner sizing knobs, and inject a smaller deterministic DPO metric runner only for this test. The test still builds 50 training rows and 50 held-out rows, still runs the actual DPO metric runner, still requires held-out accuracy >= 0.75, still verifies publication is skipped, and still checks the promotion-confirmation warning.

## Mental Simulation

Training batch generation is synchronous and bounded by 50 seeds. `CodeTraceBuilderTrainingRunner` is bounded by 50 held-out analyses and had already passed in the same class. The expensive path starts when the pipeline calls `ProjectWikiDpoMetricRunner.TrainAndEvaluateAsync`; the runner groups 50 rows into 10 mini-batches and repeats them by epoch, then evaluates 50 held-out rows and writes 100 pair diagnostics. With the previous production-scale defaults, the test can stay silent long enough for blame-hang to classify it as hung. With the injected deterministic small runner, the same assertions execute quickly while preserving DPO metric and publication-gate meaning.

## Files Changed

| File | Change |
| --- | --- |
| `42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs` | Added optional constructor parameters for `vocabSize`, `contextLen`, `numHeads`, and `numKvHeads`; defaults preserve the previous production metric config. |
| `Tests/ProjectWikiDomainTests.cs` | Injected a small deterministic `ProjectWikiDpoMetricRunner` into the CodeTraceBuilder publication-gate test and added a metric-rich assertion message. |
| `docs/evidence/design-conversion/codex-pass201-projectwiki-hang-repair.md` | This bounded evidence artifact. |

## Commands Run With Exit Codes

| Command | Exit | Evidence summary |
| --- | ---: | --- |
| Read required rules/skills with `Get-Content -Raw` and list skill files | 0 | Required skill load completed; QA/code review skills were present and read. |
| `git branch --show-current; git status --short` | 0 | Current branch is `DesignToCode-clean`; many unrelated dirty/untracked files pre-existed and were not reverted. |
| Read Pass200 recovery artifact | 0 | Confirmed accepted recovery isolated the active hang to the ProjectWiki CodeTraceBuilder pipeline test. |
| Read sequence XML under `d58d0ee6-*` and `f18ed296-*` | 0 | Focused sequence XML showed the ProjectWiki test as `Completed="False"`. |
| `rg "CodeTraceBuilderTrainingPipeline|ProjectWikiDpoMetricRunner|CodeTraceBuilderTrainingRunner|SkipsPublicationUntilPromotionConfirmation|RunOneBatchAsync"` | 0 | Located the test, pipeline, runner, metric runner, and related ProjectWiki callers. |
| Read `Tests/ProjectWikiDomainTests.cs`, `CodeTraceBuilderTrainingPipeline.cs`, `CodeTraceBuilderTrainingRunner.cs`, `ProjectWikiDpoMetricRunner.cs`, `CodeTraceBuilderDpoGenerator.cs`, `DpoTrainer.cs`, `LlmModel.cs`, `ModelConfig.cs`, and `Tests/LlmFromScratch.Tests.csproj` | 0 | Code examination covered the test and affected production metric path before edits. |
| `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.CodeTraceBuilderTrainingPipeline_SkipsPublicationUntilPromotionConfirmation" --logger "console;verbosity=minimal"` | 124 | Pre-fix focused run timed out after about 304 seconds. |
| Process inventory after pre-fix timeout | 0 | Found exact `dotnet`/vstest/testhost PIDs tied to the timed-out focused run. |
| `Stop-Process` for exact focused-run PIDs, then process recheck | 0 | Exact stale focused-run processes were removed; no matching leftovers remained. |
| First patched focused test run | 1 | New small runner reached assertions quickly but failed held-out accuracy; generated summary showed heldOutAccuracy 0 and negative reward margin. |
| Read latest generated metric summary JSON | 0 | Confirmed the first fast config was too small and did not preserve the metric threshold. |
| Final focused test run after tuning | 0 | Passed 1/1; duration 13 seconds after build. |
| Focused test with `--blame-hang --blame-hang-timeout 45s --blame-hang-dump-type none` | 0 | Passed 1/1 in 12 seconds; blame collector reported all tests finished and did not generate a sequence file. |
| Whole `ProjectWikiDomainTests` class filter | 124 | Timed out after about 304 seconds; no containing-class pass claimed. |
| Exact class-filter stale process cleanup | 0 | Stopped only the matching class-filter `dotnet`/vstest/testhost PIDs and verified no matching leftovers remained. |
| `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.CodeTraceBuilder" --logger "console;verbosity=minimal"` | 0 | Passed 3/3 in 12 seconds. |
| `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~DesignConversion" --logger "console;verbosity=minimal"` | 0 | Passed 247/247 in 1 minute 24 seconds. |
| Post-verification metric summary readback | 0 | Passing run recorded epochCount 4, learningRate 0.05, finalAccuracy 0.865, heldOutAccuracy 1.0, heldOutRewardMargin 0.667104, totalElapsedMs 11480.4421. |
| Post-verification process check for ProjectWiki/CodeTraceBuilder leftovers | 0 | No matching `dotnet`, `testhost`, or vstest leftovers remained. |

## Verification

Focused gate:
- Pre-fix exact focused test timed out after about 304 seconds.
- Post-fix exact focused test passed 1/1.
- Post-fix exact focused blame-hang run passed 1/1 in 12 seconds with no sequence file generated.

Bounded containing slice:
- `ProjectWikiDomainTests.CodeTraceBuilder` passed 3/3 in 12 seconds.
- Whole `ProjectWikiDomainTests` class filter timed out after about 304 seconds, so it remains unverified and is not claimed as passing.

DesignConversion regression check:
- `FullyQualifiedName~DesignConversion` passed 247/247 in 1 minute 24 seconds.

Post-write artifact and diff checks:
- Verdict marker count: 1.
- Identity marker count: 1.
- Bad identity marker count: 0.
- Conflict marker count: 0.
- Configured mojibake marker count: 0.
- `git diff --check -- 42.ProjectWikiDomain\ProjectWikiDpoMetricRunner.cs Tests\ProjectWikiDomainTests.cs docs\evidence\design-conversion\codex-pass201-projectwiki-hang-repair.md` exit 0.
- Artifact top/tail readback was clean ASCII.

## Remaining Broad-Suite Blockers

The broad local suite is still not claimed. The whole ProjectWiki class filter timed out independently of the repaired CodeTraceBuilder cluster. Pass200 also recorded non-DesignConversion failures before the hang. Those remain next-pass material; this pass did not broaden into ExtensionQuality or ExtensionToken repair.

## Next Recommended Pass

Run a bounded ProjectWiki class-timeout triage pass or continue Pass200's non-DesignConversion complement partition after the now-repaired CodeTraceBuilder hang. Keep DesignConversion claims limited to the already green filtered gate until the broad local suite has its own evidence.

pass201_verdict: PASS_PROJECTWIKI_HANG_REPAIRED_BOUNDED
