# E3 Split Provider Harness Tests Evidence Packet - 2026-06-29

## Claim Guard

claim_scope:
- E3 evidence-only split of the timed-out E2 partition into four smaller targeted filters.
- The evidence supports only that the four specified split filters completed separately with exit code 0 under the 180s command timeout in this turn.
- This packet does not claim combined-partition pass, broad solution pass, CI pass, runtime readiness, browser/e2e pass, training readiness, visual fidelity, promotion, deployment, or host readiness.

evidence_scope:
- Repo/worktree: `C:\Projects\llm_mvp_core`
- Branch/status command: `git status --short --branch`
- Non-artifact diff-check command: `git diff --check -- AGENTS.md docs 44.DesignConversionDomain Tests experiments-v2`
- Four split `dotnet test` filters exactly as requested.
- No broad unfiltered `dotnet test`, no combined E2 partition rerun, no browser/e2e/training/gh, no stage/commit/push, and no branch checkout/switch/create.

count_claimed:
- split_filter_test_runs_claimed=4
- precheck_commands_claimed=2
- combined_partition_runs_claimed=0
- broad_unfiltered_test_runs_claimed=0

independent_evidence_count:
- split_filter_test_outputs=4
- precheck_command_outputs=2
- total_prewrite_independent_command_outputs=6

subagent_id_or_NONE: NONE

downgrade_needed: yes - evidence supports split-filter pass only. The E2 combined partition was not rerun and remains not verified by this packet.

## Branch And Status Summary

Command:

```powershell
git status --short --branch
```

exit_code: 0

Branch header observed:

```text
## DesignToCode-clean...origin/DesignToCode-clean
```

Summary:
- Dirty worktree existed before this packet.
- Observed modified paths included `AGENTS.md`, `docs`, `44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs`, and the four targeted test files under `Tests`.
- Observed many artifact modifications/deletions and untracked paths.
- Observed untracked `docs/evidence/` before this packet write.
- No exhaustive file-count claim is made from the large status output.
- No checkout, switch, or branch creation was performed.

## Non-Artifact Diff Check

Command:

```powershell
git diff --check -- AGENTS.md docs 44.DesignConversionDomain Tests experiments-v2
```

exit_code: 0

Result:
- PASS for the requested diff-check scope.
- Console output was empty.

## Split Results

### 1. DesignConversionBrowserQaProviderTests

Exact command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionBrowserQaProviderTests" --no-restore --logger "console;verbosity=minimal"
```

exit_code: 0
elapsed_time_wall: 61.4s
result: PASS
counts_emitted: Failed 0, Passed 4, Skipped 0, Total 4
duration_emitted: 20s
first_failure: none
needs_fix: false

### 2. XiigenExtensionApiContractsTests

Exact command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~XiigenExtensionApiContractsTests" --no-restore --logger "console;verbosity=minimal"
```

exit_code: 0
elapsed_time_wall: 62.4s
result: PASS
counts_emitted: Failed 0, Passed 26, Skipped 0, Total 26
duration_emitted: 23s
first_failure: none
needs_fix: false

### 3. ExtensionTokenQualityRunnerHarnessTests

Exact command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionTokenQualityRunnerHarnessTests" --no-restore --logger "console;verbosity=minimal"
```

exit_code: 0
elapsed_time_wall: 55.2s
result: PASS
counts_emitted: Failed 0, Passed 5, Skipped 0, Total 5
duration_emitted: 16s
first_failure: none
needs_fix: false

### 4. ExtensionQuality03ProofContractRunnerTests

Exact command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests" --no-restore --logger "console;verbosity=minimal"
```

exit_code: 0
elapsed_time_wall: 47.5s
result: PASS
counts_emitted: Failed 0, Passed 15, Skipped 0, Total 15
duration_emitted: 12s
first_failure: none
needs_fix: false

## Boundary False Literals

broad_unfiltered_test_verified=false
combined_partition_verified=false
solution_green_verified=false
ci_green_verified=false
runtime_ready_verified=false
browser_e2e_verified=false
trained_feature_verified=false
visual_fidelity_verified=false
promotion_verified=false
deployment_verified=false
host_readiness_verified=false

Notes:
- All four split filters passed separately.
- The combined E2 partition was not rerun.
- The previous E2 combined timeout is not converted into combined readiness by this packet.

## Validation Results

packet_readback_result=PASS
filter_name_search_result=PASS - all four exact filter names are present:
- FullyQualifiedName~DesignConversionBrowserQaProviderTests
- FullyQualifiedName~XiigenExtensionApiContractsTests
- FullyQualifiedName~ExtensionTokenQualityRunnerHarnessTests
- FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests

boundary_false_literal_search_result=PASS - all required boundary false literals are present.

positive_boundary_true_literal_search_result=PASS_NO_MATCH - no positive boundary verification literal is present.

post_write_git_status_result=PASS - `git status --short` was run after writing the packet; dirty worktree remains, with this packet under `?? docs/evidence/`.

## Next Implementable Blocker

next_implementable_blocker: Investigate why the four-class combined E2 partition timed out even though each split filter passed separately. Do not start from this packet.
