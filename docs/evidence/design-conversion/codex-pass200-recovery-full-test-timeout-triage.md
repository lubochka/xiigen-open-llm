# Pass200 Recovery - Full Local Test Timeout Triage

claim_scope: Pass200 recovery candidate for bounded full-local-test timeout triage only; not product complete, not full CI, not public/provider/cloud readiness.
evidence_scope: Fresh-read recovery gates, prior Pass200 NEEDS_FIX artifact, Pass199 filtered DesignConversion evidence, test inventory, bounded non-DesignConversion partition with blame-hang, exact single-test hang reproduction, process cleanup check, fresh DesignConversion rerun, and source/sequence readback.
count_claimed: 1 accepted Pass200 recovery candidate, contingent on this artifact's verification checks passing; does not claim the full unfiltered local suite passed.
independent_evidence_count: 8 bounded evidence groups.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
spawned_worker_identity_self_check: spawned_worker=true; used_NONE=false; used_false_no_subagent_claim=false; parent_must_reconcile=true
nested_subagent_usage: No nested sub-agent was used by this worker.
downgrade_needed: Yes. Full unfiltered local tests remain blocked; the supportable claim is timeout triage, not full local/CI/product readiness.

## Skill Load Status

| File | Status | Evidence |
| --- | --- | --- |
| AGENTS.md | found/read | 407799 bytes, 6961 lines, sha16 A8F6EB15692F727C |
| docs/SESSION-START-PROMPT.md | found/read | 42267 bytes, 621 lines, sha16 450961E74C8E9DB4 |
| docs/skills/claim-verification-SKILL.md | found/read | 65698 bytes, 841 lines, sha16 F6D84C0F3815367C |
| docs/skills/audit-protocol-SKILL.md | found/read | 35531 bytes, 511 lines, sha16 FF0C6898DEBD3BF2 |
| docs/skills/output-skepticism-SKILL.md | found/read | 21007 bytes, 371 lines, sha16 AFE71FA8A2E944D0 |
| docs/skills/implementation-integrity-SKILL.md | found/read | 38160 bytes, 794 lines, sha16 18C33DD8C4D74770 |
| docs/skills/test-integrity-SKILL.md | found/read | 41002 bytes, 776 lines, sha16 FD545554BC746E78 |
| docs/skills/integration-testing-SKILL.md | found/read | 9104 bytes, 205 lines, sha16 6CB1698DC95C6F77 |
| docs/skills/generated-code-review-SKILL.md | found/read | 30853 bytes, 672 lines, sha16 CEB6CF6BBFFF4121 |
| docs/skills/code-examination-SKILL.md | found/read | 14801 bytes, 376 lines, sha16 38DF19AB6D80A0EF |
| docs/skills/test-failure-triage-SKILL.md | found/read | 8087 bytes, 215 lines, sha16 151992D53FE19D0D |
| docs/skills/e2e-test-matrix-SKILL.md | found/read | 10011 bytes, 266 lines, sha16 A4C1AF58AFE09666 |

Loaded rules applied: a timeout is a repair queue, not a pass; focused tests cannot be inflated into full CI or product readiness; tests must not be disabled/deleted to make a gate pass; exact command evidence and exit codes are required.

## Prior Evidence Read

| File | Status | Recovery use |
| --- | --- | --- |
| docs/evidence/design-conversion/codex-pass200-current-product-gap-closure.md | found/read | Treated as NEEDS_FIX evidence only: full unfiltered local test timed out after about 20 minutes, and exact stale dotnet cleanup was performed. |
| docs/evidence/design-conversion/codex-pass199-ci-test-readiness-audit.md | found/read | Treated as bounded filtered evidence only: DesignConversion filter previously passed 247/247; no full local or CI claim. |

## Mental Simulation

The full local timeout could have come from seven plausible sources: a DesignConversion resource leak, unrelated slow tests, network/provider tests, training/model tests, a deadlock, stale dotnet/testhost processes, or test-host/runtime behavior. The bounded sequence selected was:

1. Inventory all tests with `--list-tests`.
2. Run the complement partition excluding DesignConversion with blame-hang enabled.
3. Reproduce the active hanging test alone with a shorter blame-hang timeout.
4. Check for stale dotnet processes after aborts.
5. Rerun the DesignConversion filtered gate after the non-DesignConversion abort to test isolation.

This sequence is higher priority than patching because Pass199 already proved the DesignConversion slice locally, while Pass200's failed evidence was the broader verification gate. The recovery target was to isolate whether the gate failure belonged to DesignConversion before changing unrelated product code.

## Triage Partitions

| Partition or probe | Command/result | Evidence |
| --- | --- | --- |
| Full inventory | `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build --list-tests --logger "console;verbosity=minimal"` exit 0 | 2339 listed tests; 247 matched DesignConversion; 2092 did not match DesignConversion. |
| Static inventory attempt | `rg --files Tests | rg "\.cs$"` exit 1 | No useful output from this pattern in the current shell; replaced by `--list-tests` inventory. |
| Grouping attempt | First PowerShell grouping script exit 1 | Parse error from an empty pipeline element; fixed by rerunning the list-tests based grouping. |
| Non-DesignConversion complement | `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName!~DesignConversion" --blame-hang --blame-hang-timeout 90s --blame-hang-dump-type none --logger "console;verbosity=minimal"` exit 1 | Test run reached `Failed: 26, Passed: 1949, Total: 1975, Duration: 3 m 19 s`, then blame-hang aborted after 90 seconds of inactivity. Active test: `LlmFromScratch.Tests.ProjectWikiDomainTests.CodeTraceBuilderTrainingPipeline_SkipsPublicationUntilPromotionConfirmation`. |
| Non-DesignConversion sequence file | `Tests\TestResults\d58d0ee6-9a64-4c7a-8d76-bc07df7ec53c\Sequence_1a79e383f62f4187b33d75a1b5706ff1.xml` readback exit 0 | Sequence evidence showed the run was inside non-DesignConversion tests when blame-hang fired. |
| Focused active test | `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.CodeTraceBuilderTrainingPipeline_SkipsPublicationUntilPromotionConfirmation" --blame-hang --blame-hang-timeout 45s --blame-hang-dump-type none --logger "console;verbosity=minimal"` exit 1 | Single test reproduced the blame-hang abort after 45 seconds of inactivity/testhost crash. |
| Focused sequence file | `Tests\TestResults\f18ed296-5286-493a-a495-e8ccc745011a\Sequence_b2d88e7e2555408bbe54a09bce26aeb1.xml` readback exit 0 | Sequence XML contains the ProjectWiki test with `Completed="False"`. |
| Stale-process checks | `Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" ...` exit 0 after each abort | No stale dotnet processes remained after either blame-hang run. |
| Fresh DesignConversion rerun | `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~DesignConversion" --logger "console;verbosity=minimal"` exit 0 | 247 passed, 0 failed, 0 skipped, duration 1 m 30 s after the non-DesignConversion aborts. |

## Root Cause Or Current Best Evidence

The current best evidence is that the full local gate is blocked outside the DesignConversion product slice. The non-DesignConversion complement produced multiple unrelated failures and then blame-hang identified `ProjectWikiDomainTests.CodeTraceBuilderTrainingPipeline_SkipsPublicationUntilPromotionConfirmation` as the active test at inactivity timeout. Running that exact ProjectWiki test alone reproduced the hang/abort and wrote a sequence file with the test incomplete. A fresh DesignConversion filtered run still passed 247/247 after that, so this recovery did not find evidence that the Pass200 timeout was caused by DesignConversion product changes.

The complement failures also included ExtensionQuality/ExtensionToken artifact and memory-related failures, so the broad local suite has at least two non-DesignConversion problem classes: ordinary failures before the hang and a ProjectWiki training-pipeline hang/testhost abort.

## Source Inspection

Inspected `Tests\ProjectWikiDomainTests.cs` around the focused test. The test calls `new CodeTraceBuilderTrainingPipeline().RunOneBatchAsync(...)` and asserts a 50-row training batch, 50-row held-out batch, a passed training verdict, no publication without promotion confirmation, and DPO metrics at or above 0.75.

Inspected `42.ProjectWikiDomain\CodeTraceBuilderTrainingPipeline.cs`. The pipeline creates batches, runs `CodeTraceBuilderTrainingRunner`, then calls `ProjectWikiDpoMetricRunner(epochCount: 8, learningRate: 1e-3f, deterministicFreshInitialization: true)` before validating the publication gate. The bounded evidence points to this ProjectWiki training/metric path as the next target, not to DesignConversion render/visual/DPO/server behavior.

## Repair Or No-Fix Reason

No code or test patch was applied in this recovery. The reproduced hang is in ProjectWiki training/metric tests and the complement contains additional ExtensionQuality/ExtensionToken failures. Patching that area inside this DesignToCode Pass200 recovery would broaden into unrelated product/test ownership without enough root-cause proof for a safe one-line fix. Disabling, deleting, or weakening the hanging test would violate the recovery rules and would hide a verification problem rather than repair it.

The concrete closure action for this recovery is the bounded hang-aware gate: the full-suite timeout was isolated to a reproducible non-DesignConversion ProjectWiki test path while the DesignConversion filtered gate remained green.

## Deterministic Vs Trainable Boundary

This recovery did not move deterministic validation behind training. The DesignConversion deterministic/product slice was rerun directly and passed 247/247. The hanging path is a separate ProjectWiki trainable metric/training pipeline test; it must be repaired or bounded by its own worker before any broader full-local/CI claim can be made.

## Commands Run With Exit Codes

| Command | Exit | Evidence summary |
| --- | ---: | --- |
| Refreshed required recovery gates and QA/code skills with PowerShell file reads and hashes | 0 | All required refreshed files found/read; details in skill table. |
| Read previous Pass200 artifact | 0 | Confirmed NEEDS_FIX-only full local timeout evidence. |
| Read Pass199 artifact | 0 | Confirmed bounded DesignConversion 247/247 evidence only. |
| `dotnet test Tests\LlmFromScratch.Tests.csproj --no-build --list-tests --logger "console;verbosity=minimal"` | 0 | 2339 listed tests. |
| `rg --files Tests | rg "\.cs$"` | 1 | Pattern produced no useful output in this shell. |
| First PowerShell grouping script | 1 | Parse error; superseded by fixed grouping over list-tests output. |
| Fixed list-tests grouping | 0 | 247 DesignConversion, 2092 non-DesignConversion; selected partition counts recorded. |
| Static source inventory via `rg --files` plus `Select-String` | 1 | Produced source inventory output despite nonzero exit from the filter. |
| Dotnet help probe for blame-hang/no-dump flags | 0 | Confirmed `--blame-hang`, `--blame-hang-timeout`, and dump type `none`. |
| Non-DesignConversion complement with 90s blame-hang | 1 | 1949 passed, 26 failed, then inactivity abort with ProjectWiki active test. |
| Dotnet process check after complement abort | 0 | No stale dotnet process entries. |
| Non-DesignConversion sequence XML readback | 0 | Sequence file created under `Tests\TestResults\d58d0ee6-9a64-4c7a-8d76-bc07df7ec53c`. |
| Broad source search for ProjectWiki pipeline | 124 | Timed out; superseded by narrower source inspection. |
| Focused ProjectWiki single-test blame-hang run | 1 | Single test reproduced inactivity abort after 45 seconds. |
| Dotnet process check after focused abort | 0 | No stale dotnet process entries. |
| Focused ProjectWiki sequence XML readback | 0 | Sequence file showed the exact test incomplete. |
| Narrow source inspections for ProjectWiki test/pipeline/runner/generator | 0 | Confirmed trainable metric/training path. |
| Fresh DesignConversion filtered rerun | 0 | 247/247 passed in 1 m 30 s. |
| TestResults inventory for recovery-created sequence files | 0 | Two sequence XML files found; no dump files were generated by these blame runs. |
| `git diff --check -- docs/evidence/design-conversion/codex-pass200-recovery-full-test-timeout-triage.md` | 0 | Scoped artifact diff check passed. |
| First nested post-write marker script | 0 shell exit, invalid evidence | Inner PowerShell printed a parser error, so this check was not counted. |
| Correct direct post-write marker scan | 0 | Verdict marker 1, identity marker 1, forbidden identity markers 0, conflict markers 0, configured mojibake markers 0. |
| Post-write human-readable artifact readback | 0 | Full artifact read back cleanly. |
| Post-write trailing-whitespace scan | 0 | TrailingWhitespaceLineCount=0. |
| Post-write status for recovery files | 0 | Recovery artifact and two blame-hang sequence directories are untracked. |

## Files Changed

Intentional edit:

- `docs/evidence/design-conversion/codex-pass200-recovery-full-test-timeout-triage.md`

Command-generated evidence files from blame-hang runs:

- `Tests\TestResults\d58d0ee6-9a64-4c7a-8d76-bc07df7ec53c\Sequence_1a79e383f62f4187b33d75a1b5706ff1.xml`
- `Tests\TestResults\f18ed296-5286-493a-a495-e8ccc745011a\Sequence_b2d88e7e2555408bbe54a09bce26aeb1.xml`

Existing dirty/untracked files from the broader worktree were not reverted or cleaned.

## Verification

Post-write checks completed:

- Marker count for this artifact's verdict line: 1.
- Identity marker count: 1.
- Forbidden identity marker count: 0.
- Conflict-marker count: 0.
- Configured mojibake-marker count: 0.
- `git diff --check -- docs/evidence/design-conversion/codex-pass200-recovery-full-test-timeout-triage.md` exit 0.
- Changed human-readable artifact lines read back cleanly.
- Trailing whitespace scan exit 0.

pass200_recovery_verdict: PASS_FULL_TEST_TIMEOUT_TRIAGED_BOUNDED

## Next Recommended Pass

Run a dedicated non-DesignConversion full-local repair worker for `ProjectWikiDomainTests.CodeTraceBuilderTrainingPipeline_SkipsPublicationUntilPromotionConfirmation`. It should inspect `ProjectWikiDpoMetricRunner` and the CodeTraceBuilder pipeline with bounded per-test timeouts, then separately triage the ExtensionQuality/ExtensionToken artifact and memory failures reported before the hang. Do not use the current DesignConversion recovery as a full local test pass.
