# E5 Sequential Process-Spawning Route - 2026-06-29

## Claim Guard

claim_scope:
- E5 sequential evidence route for four process-spawning test class filters.
- The evidence supports only that three of four requested sequential class filters completed with exit code 0 and emitted passing counts in this turn.
- The fourth requested sequential class filter timed out under the configured 180s tool timeout before emitting test counts.
- This packet does not claim combined filter pass, broad solution pass, CI pass, runtime readiness, browser/e2e pass, training readiness, visual fidelity, promotion, deployment, or host readiness.

evidence_scope:
- Repo/worktree: `C:\Projects\llm_mvp_core`
- Current branch observed: `DesignToCode-clean`
- Metadata inspection of the four focused classes showed existing `[Collection("ProcessSpawningTests")]`; no test metadata change was made.
- Pre-test repo-associated `dotnet.exe`/`testhost.exe` cleanup check for `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\LlmFromScratch.Tests.dll` and `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0`.
- Four requested sequential `dotnet test` class filters exactly as listed below.
- Post-timeout repo-associated `dotnet.exe`/`testhost.exe` cleanup check after the fourth filter timed out.
- No broad unfiltered `dotnet test`, no four-class combined filter rerun, no browser/e2e/training/gh, no stage/commit/push, and no branch checkout/switch/create.

count_claimed:
- sequential_filter_commands_attempted=4
- sequential_filter_passes_claimed=3
- sequential_filter_timeouts_claimed=1
- combined_filter_runs_claimed=0
- broad_unfiltered_test_runs_claimed=0
- metadata_files_edited_claimed=0
- evidence_packets_written_claimed=1

independent_evidence_count:
- branch_command_outputs=1
- metadata_inspection_outputs=1
- process_cleanup_command_outputs=2
- sequential_test_command_outputs=4
- total_current_turn_evidence_records_before_packet_write=8

subagent_id_or_NONE: NONE

downgrade_needed: yes - evidence supports only three passing sequential filters plus one timed-out sequential filter. The combined filter remains blocked_not_proven.

## Changed Files

Changed by this E5 task:

- `docs/evidence/design-conversion/E5-sequential-process-spawning-route-2026-06-29.md`

Not changed by this E5 task:

- No product code files.
- No test metadata files.
- No plan/governance/skill files.
- No scripts.
- No artifacts under `C:\Users\lubao\Documents\xiigen`, `xiigen-flows`, or `artifacts/**`.

## Metadata Decision

Inspection command:

```powershell
rg -n "\[Trait|\[Collection|public sealed class|\[Fact|\[Theory" Tests\DesignConversionBrowserQaProviderTests.cs Tests\XiigenExtensionApiContractsTests.cs Tests\ExtensionTokenQualityRunnerHarnessTests.cs Tests\ExtensionQuality03ProofContractRunnerTests.cs
```

Result:

- exit_code: 0
- Each of the four focused classes already had `[Collection("ProcessSpawningTests")]`.
- No `[Trait("E4", "ProcessSpawning")]` or `[Trait("Category", "ProcessSpawning")]` was added because existing nonparallel process-spawning collection metadata was already present.

## Process Hygiene Before Test Runs

Target DLL:

```text
C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\LlmFromScratch.Tests.dll
```

Target directory:

```text
C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0
```

Result:

- exit_code: 0
- PRE_MATCH_COUNT=0
- POST_MATCH_COUNT=0
- cleanup_action: none - no repo-associated stale `dotnet.exe` or `testhost.exe` processes were found before the sequential test runs.

## Sequential Results

### 1. DesignConversionBrowserQaProviderTests

Exact command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionBrowserQaProviderTests" --no-restore --logger "console;verbosity=minimal"
```

Result:

- exit_code: 0
- elapsed_time_wall: 53.5s
- result: PASS
- counts_emitted: Failed 0, Passed 4, Skipped 0, Total 4
- duration_emitted: 19s
- first_failure: none

### 2. XiigenExtensionApiContractsTests

Exact command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~XiigenExtensionApiContractsTests" --no-restore --logger "console;verbosity=minimal"
```

Result:

- exit_code: 0
- elapsed_time_wall: 49.8s
- result: PASS
- counts_emitted: Failed 0, Passed 26, Skipped 0, Total 26
- duration_emitted: 19s
- first_failure: none

### 3. ExtensionTokenQualityRunnerHarnessTests

Exact command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionTokenQualityRunnerHarnessTests" --no-restore --logger "console;verbosity=minimal"
```

Result:

- exit_code: 0
- elapsed_time_wall: 49.1s
- result: PASS
- counts_emitted: Failed 0, Passed 5, Skipped 0, Total 5
- duration_emitted: 18s
- first_failure: none

### 4. ExtensionQuality03ProofContractRunnerTests

Exact command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests" --no-restore --logger "console;verbosity=minimal"
```

Result:

- exit_code: 124
- elapsed_time_wall: 184.1s
- configured_tool_timeout: 180000ms
- timeout_output: `command timed out after 184112 milliseconds`
- result: TIMEOUT
- counts_emitted: not emitted
- duration_emitted: not emitted
- first_failure: timeout before test counts or first failure were emitted

## Post-Timeout PID Cleanup

Post-timeout repo-associated process check found two matching processes:

- PID 7760, `dotnet.exe`, parent PID 26028
  - command included `vstest.console.dll`
  - command included `--testCaseFilter:"FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests"`
  - command included `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\LlmFromScratch.Tests.dll`
- PID 27160, `testhost.exe`, parent PID 7760
  - executable path: `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\testhost.exe`
  - command included `LlmFromScratch.Tests.runtimeconfig.json` and `LlmFromScratch.Tests.deps.json`

Cleanup actions:

- `Stop-Process -Id 7760 -Force` result: stop_issued
- `Stop-Process -Id 27160 -Force` result: stop_issued
- post_cleanup_match_count=0

classification: only verified repo-associated test processes tied to the E5 timed-out test run were stopped.

## Combined Filter Boundary

combined_filter_run_in_E5=false
combined_filter_status=blocked_not_proven
combined_partition_verified=false

The four-class combined filter was not run in this E5 task. The combined partition remains blocked_not_proven.

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

## Next Implementable Blocker

next_implementable_blocker_do_not_start: Isolate and repair the timeout in `ExtensionQuality03ProofContractRunnerTests` under the same process-spawning cleanup boundary, likely by splitting the class into narrower fact-level filters or adding hang diagnostics before retrying the full proof-contract class filter. Do not start from this packet.
