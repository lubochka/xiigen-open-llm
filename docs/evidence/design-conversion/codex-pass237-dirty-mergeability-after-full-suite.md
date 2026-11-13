# Pass237 Dirty Worktree Mergeability After Full Suite

timestamp_local: 2026-07-04T12:10:26.5013632+03:00
repo: C:\Projects\llm_mvp_core
branch: DesignToCode-clean
verification_type: no-write dirty-worktree mergeability
publication_scope: none; no stage, commit, push, merge, rebase, checkout, reset, clean, PR, deploy, or external provider/API call

## Honesty Ledger

claim_scope: Pass237 no-write dirty-worktree mergeability for the current C:\Projects\llm_mvp_core worktree on branch DesignToCode-clean against origin/main and origin/codex/chat-gpt-5.5 only.
evidence_scope: Commands run in this turn in C:\Projects\llm_mvp_core. The accepted mergeability evidence is the final read-tree plus status-path temporary-index run. The local full suite pass of 2339/2339 is parent/user-provided context; I did not rerun dotnet test in Pass237.
count_claimed: 2 mergeability checks
independent_evidence_count: 2 accepted git merge-tree command records from the final status-path temporary-index run
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: No downgrade for the bounded local no-write mergeability claim. This does not claim ProductReady, cloud/deploy/public endpoint readiness, external-provider readiness, publication, or CI readiness.

## Skill Load Status

Required local governance files were found and read with a complete `Get-Content -Raw` probe plus targeted key-rule readback before the accepted mergeability check.

| File | status | evidence |
| --- | --- | --- |
| C:\Projects\llm_mvp_core\AGENTS.md | found/read | SHA256 A8F6EB15692F727CD0EC5623E2C13109F3A1AC7C9EA31E4B3DC63C16E56F5D69; targeted readback included no-permission, no-publication, no-branch-switch, mergeability, claim ledger, and QA/code-review stack rules. |
| C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md | found/read | SHA256 C21D82E6CC29995FFBB14318686E5CCF0563DBCB07E9176FB1CD084ABCD9FE90; key readback included startup stack and no-permission/no-sandbox guidance. |
| C:\Projects\llm_mvp_core\docs\SESSION-START-PROMPT.md | found/read | SHA256 450961E74C8E9DB4CAA6D59441E8BFC76F2BFF973072ED13B8BF6D69DF0D9A8C; key readback included claim ledger and mergeability/future-gate language. |
| C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md | found/read | SHA256 F6D84C0F3815367C26012A81B6C8C6DB37EB4FDD2144263842785A4E694A0D68; key readback included evidence-scope narrowing and mergeability/latest-authority requirements. |
| C:\Projects\llm_mvp_core\docs\skills\audit-protocol-SKILL.md | found/read | SHA256 FF0C6898DEBD3BF21EC538D6C32F1444044D545DC8996198A24FB3F3876D9D0A; key readback included audit and claim-ledger guidance. |
| C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md | found/read | SHA256 BF5544598D4E358AA9333D2A0A47D4E010862DC7F58EBB2181908BD45B86B6DD; key readback included QA packet skill-load and evidence-boundary rules. |
| C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md | found/read | SHA256 CEB6CF6BBFFF4121DB8FA898A21765AFDA0CF70E2F9334CD9FA8F171C21EDCC4; key readback included code/branch/ProductReady claim firewall. |
| C:\Projects\llm_mvp_core\docs\skills\phase-git-report-SKILL.md | found/read | SHA256 76FD5A16C6AFF14D7485061B59AB3621CE39759A2B1880E332D78005B75EF4D8; key readback included git/report phase scope. |

## Baseline State

| Field | Value |
| --- | --- |
| `git branch --show-current` | DesignToCode-clean |
| `git rev-parse HEAD` | 0d4a879298772dfad76111ad1301cdfc74047a33 |
| `git fetch origin` | exit 0 |
| `git rev-parse --verify 'origin/main^{commit}'` | exit 0, ded0a6d8946db888f8398b8477c9c66c60d3e37e |
| `git rev-parse --verify 'origin/codex/chat-gpt-5.5^{commit}'` | exit 0, 5084f355e7185e2c2cf7928ae5c972208c226117 |
| real index before | `git diff --cached --name-status` exit 0 with no output |
| `git status --short` before accepted run | exit 0, 696 lines |
| status summary | 304 modified-status lines, 392 untracked-status lines, 0 deleted-status lines |

First 20 status lines:

```text
 M 11.DPO/DpoTrainer.cs
 M 15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs
 M 25.ModelServer/ModelServer.csproj
 M 42.ProjectWikiDomain/DocumentationGeneratorTrainingPipeline.cs
 M 42.ProjectWikiDomain/EntityImpactMapperTrainingPipeline.cs
 M 42.ProjectWikiDomain/ProjectAnswerVerifierTrainingPipeline.cs
 M 42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs
 M 42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs
 M 42.ProjectWikiDomain/WikiTraceAggregatorTrainingPipeline.cs
 M 43.SmartSkillsDomain/XiigenExtensionApiService.cs
 M 44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs
 M 44.DesignConversionDomain/DesignConversionCheckpointContracts.cs
 M 44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs
 M 44.DesignConversionDomain/DesignConversionDpoDisplay.cs
 M 44.DesignConversionDomain/DesignConversionDpoGenerator.cs
 M 44.DesignConversionDomain/DesignConversionDpoStorage.cs
 M 44.DesignConversionDomain/DesignConversionInternalService.cs
 M 44.DesignConversionDomain/DesignConversionLocalContinueTrainingProbe.cs
 M 44.DesignConversionDomain/DesignConversionLocalFreshLoadProbe.cs
 M 44.DesignConversionDomain/DesignConversionMachine.cs
```

Last 20 status lines:

```text
?? experiments-v2/T34-PHASE-3-TO-4-TRANSITION-GATE/results/run-2026-07-04-074358.mmd
?? experiments-v2/T34-PHASE-3-TO-4-TRANSITION-GATE/results/run-2026-07-04-082857.json
?? experiments-v2/T34-PHASE-3-TO-4-TRANSITION-GATE/results/run-2026-07-04-082857.md
?? experiments-v2/T34-PHASE-3-TO-4-TRANSITION-GATE/results/run-2026-07-04-082857.mmd
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-021903.json
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-021903.md
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-021903.mmd
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-043152.json
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-043152.md
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-043152.mmd
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-054542.json
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-054542.md
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-054542.mmd
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-074312.json
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-074312.md
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-074312.mmd
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-082852.json
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-082852.md
?? experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-082852.mmd
?? tools/DesignConversionLiveLocalServer/
```

## Accepted Temporary-Index Method

The accepted run used a repo-local temporary index file under:

```text
C:\Projects\llm_mvp_core\.codex-pass237-temp-statuspaths-f079ad642df84e95b4e28984c03822cf\index
```

The temporary directory was removed before return.

Accepted method:

1. Capture real `git status --short`, `git -c core.quotePath=false status --porcelain=v1`, and `git diff --cached --name-status`.
2. Set `GIT_INDEX_FILE` only inside the PowerShell process scope.
3. Run `git read-tree HEAD` into the temporary index.
4. Parse the 696 porcelain status paths from the real worktree.
5. Run `git add -A -- <status paths>` in 14 batches of 50/46 paths. All 14 add batches exited 0.
6. Run `git write-tree`.
7. Run `git commit-tree <dirty_tree> -p HEAD -m "codex pass237 dirty snapshot mergeability probe status paths"`.
8. Run `git merge-tree --write-tree` against both target refs.
9. Restore/unset `GIT_INDEX_FILE`, remove the exact temp directory, and recheck the real index.

This method was chosen over the provisional whole-worktree fresh-index run because a fresh `git read-tree HEAD` plus unrestricted `git add -A -- .` re-added cached clean files with line-ending/stat-cache differences and produced 21,203 staged entries. The accepted status-path run starts from HEAD as requested but limits additions to the 696 paths Git reported dirty/untracked in the real worktree; untracked directories then expand to individual files in the temporary index, giving 12,659 staged entries.

## Accepted Snapshot Evidence

| Field | Value |
| --- | --- |
| accepted temp read-tree | `git read-tree HEAD`, exit 0 |
| status paths parsed | 696 unique paths from `git -c core.quotePath=false status --porcelain=v1`, exit 0 |
| add batches | 14 batches, exit codes all 0 |
| temporary staged diff | 12,659 entries after untracked directories expanded |
| dirty tree | 0ed07c586df0e57b7be7d06f65089222a7d4d10a |
| dirty snapshot commit | a7e6f60b6cdf937b95171173503de8fa0632a8e5 |
| temp directory cleanup | existed before cleanup: true; exists after cleanup: false |
| real index after accepted run | `git diff --cached --name-status` exit 0 with no output |
| real status after accepted run before evidence artifact write | 696 lines, same count as before |

First 20 temporary staged diff entries:

```text
M	11.DPO/DpoTrainer.cs
M	15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs
A	25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs
A	25.ModelServer/DesignConversion/DesignConversionModelServerAspNetEndpointMapper.cs
A	25.ModelServer/DesignConversion/DesignConversionModelServerAspNetHostStartup.cs
A	25.ModelServer/DesignConversion/DesignConversionModelServerCallerContract.cs
A	25.ModelServer/DesignConversion/DesignConversionModelServerDpoUsefulnessHandler.cs
A	25.ModelServer/DesignConversion/DesignConversionModelServerEndpointRegistry.cs
A	25.ModelServer/DesignConversion/DesignConversionModelServerHttpHandler.cs
A	25.ModelServer/DesignConversion/DesignConversionModelServerImprovementLoopHandler.cs
A	25.ModelServer/DesignConversion/DesignConversionModelServerLocalCallerAdapter.cs
A	25.ModelServer/DesignConversion/DesignConversionModelServerLocalHttpHostAdapter.cs
A	25.ModelServer/DesignConversion/DesignConversionModelServerStatusHandler.cs
M	25.ModelServer/ModelServer.csproj
M	42.ProjectWikiDomain/DocumentationGeneratorTrainingPipeline.cs
M	42.ProjectWikiDomain/EntityImpactMapperTrainingPipeline.cs
M	42.ProjectWikiDomain/ProjectAnswerVerifierTrainingPipeline.cs
M	42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs
M	42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs
M	42.ProjectWikiDomain/WikiTraceAggregatorTrainingPipeline.cs
```

Last 20 temporary staged diff entries:

```text
A	experiments-v2/T34-PHASE-3-TO-4-TRANSITION-GATE/results/run-2026-07-04-082857.json
A	experiments-v2/T34-PHASE-3-TO-4-TRANSITION-GATE/results/run-2026-07-04-082857.md
A	experiments-v2/T34-PHASE-3-TO-4-TRANSITION-GATE/results/run-2026-07-04-082857.mmd
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-021903.json
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-021903.md
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-021903.mmd
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-043152.json
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-043152.md
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-043152.mmd
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-054542.json
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-054542.md
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-054542.mmd
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-074312.json
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-074312.md
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-074312.mmd
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-082852.json
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-082852.md
A	experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-07-04-082852.mmd
A	tools/DesignConversionLiveLocalServer/DesignConversionLiveLocalServer.csproj
A	tools/DesignConversionLiveLocalServer/Program.cs
```

## Mergeability Verdict

| Target | ref commit | command | exit code | first output | verdict |
| --- | --- | --- | ---: | --- | --- |
| origin/main | ded0a6d8946db888f8398b8477c9c66c60d3e37e | `git merge-tree --write-tree origin/main a7e6f60b6cdf937b95171173503de8fa0632a8e5` | 0 | 58b88e15c06f38ae966debedc3e72e8bf4407534 | PASS |
| origin/codex/chat-gpt-5.5 | 5084f355e7185e2c2cf7928ae5c972208c226117 | `git merge-tree --write-tree origin/codex/chat-gpt-5.5 a7e6f60b6cdf937b95171173503de8fa0632a8e5` | 0 | 58b88e15c06f38ae966debedc3e72e8bf4407534 | PASS |

mergeability_origin_main: PASS
mergeability_origin_original: PASS

## Failed Or Provisional Attempts

These records are included to avoid overclaiming:

| Attempt | Evidence | Status |
| --- | --- | --- |
| Initial wrapper attempt | PowerShell helper used a parameter name that resulted in internal `git` calls receiving an empty argument array. Git printed usage and internal records had exit 1. | Downgraded; no snapshot and no mergeability evidence. |
| Unquoted ref verification | `git rev-parse --verify origin/main^{commit}` and `git rev-parse --verify origin/codex/chat-gpt-5.5^{commit}` exited 1 in PowerShell because the revision arguments were not quoted. | Downgraded; quoted rerun succeeded and is the accepted ref evidence. |
| Whole-worktree fresh-index run | Created commit ffeaa3cc297e6306e8a4c85065f0311c6f8f7f34 and both merge-tree commands exited 0, but temp staged diff contained 21,203 entries due clean-file line-ending/stat-cache re-adds. | Provisional only; not the accepted verdict. |
| Copied-index run | Created commit 971f9409fa74725686d0ef10bac6fc52eb19dc70 and both merge-tree commands exited 0. | Corroborating only; final accepted run used read-tree plus status-paths to match the work order more closely. |

## Real Index Preservation

real_index_preserved: yes

Evidence:

```text
before accepted run: git diff --cached --name-status -> exit 0, no output
after accepted run:  git diff --cached --name-status -> exit 0, no output
temp directory after cleanup: false
```

## Verification After Artifact Write

All checks below were run after this evidence artifact was written.

```text
git diff --check -- docs/evidence/design-conversion/codex-pass237-dirty-mergeability-after-full-suite.md -> exit 0, no output
Select-String key-line readback -> exit 0; claim_scope, evidence_scope, count_claimed, independent_evidence_count, subagent_id_or_NONE, downgrade_needed, dirty snapshot commit, mergeability verdicts, and real_index_preserved lines were present
Select-String conflict-marker scan for line-anchored conflict markers -> exit 0, no output
Select-String mojibake-marker scan -> exit 0, no output
git diff --cached --name-status -> exit 0, no output
git status --short -- docs/evidence/design-conversion/codex-pass237-dirty-mergeability-after-full-suite.md -> exit 0, output: ?? docs/evidence/design-conversion/codex-pass237-dirty-mergeability-after-full-suite.md
git status --short after artifact write -> exit 0, 697 lines
post-artifact status difference from accepted-run status count -> +1 line for this evidence artifact
Get-ChildItem -Force -Directory -Filter '.codex-pass237-temp*' -> exit 0, no output
```
