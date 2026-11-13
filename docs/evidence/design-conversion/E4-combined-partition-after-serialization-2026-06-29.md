# E4 Combined Partition After Serialization - 2026-06-29

## Claim Guard

claim_scope: E4 serialization fix already existed; current lock-clear task cleared stale PID 11416, then the same exact combined provider/API/harness/proof-contract filter timed out before emitting test counts.
evidence_scope: Current-turn PID 11416 `Get-Process`/CIM inspection; parent PID 18132 vstest command line with the same E4 filter and `LlmFromScratch.Tests.dll`; stop-only attempt for PID 11416 plus post-stop Get-Process/CIM rechecks showing PID 11416 absent; exact filtered `dotnet test` rerun timeout after 604098 ms; post-timeout repo-associated `dotnet.exe` PID 24780 and `testhost.exe` PID 29716 observation; `git diff --check -- Tests docs/evidence` exit 0 before this packet edit.
count_claimed: 0 verified combined partition passes; 1 stale PID 11416 lock-clear action recorded; 1 exact filtered rerun attempt recorded.
independent_evidence_count: 8 current-turn command evidence records recorded in this packet: PID 11416 Get-Process, PID 11416 CIM, parent PID 18132 CIM, stop/recheck Get-Process, stop/recheck CIM, exact filtered dotnet test timeout, post-timeout CIM process check, git diff check.
subagent_id_or_NONE: NONE
downgrade_needed: yes - evidence supports only stale lock clear plus timed-out rerun, not a verified partition.

## Changed Files

Current lock-clear/rerun task changed only this evidence packet:

- `docs/evidence/design-conversion/E4-combined-partition-after-serialization-2026-06-29.md`

Files changed by the prior E4 serialization fix, not edited by the current lock-clear/rerun task:

- `Tests/ProcessSpawningTestCollection.cs`
- `Tests/DesignConversionBrowserQaProviderTests.cs`
- `Tests/XiigenExtensionApiContractsTests.cs`
- `Tests/ExtensionTokenQualityRunnerHarnessTests.cs`
- `Tests/ExtensionQuality03ProofContractRunnerTests.cs`

Pre-existing dirty worktree note: `git status --short` also listed many unrelated modified/deleted/untracked files under product, docs, governance, artifacts, and experiments paths. Those are outside this E4 fix scope and were not touched by this task.

## Exact Fix Made

- Added xUnit nonparallel collection definition:
  - `Tests/ProcessSpawningTestCollection.cs`
  - `[CollectionDefinition("ProcessSpawningTests", DisableParallelization = true)]`
- Added `[Collection("ProcessSpawningTests")]` immediately above these four test classes:
  - `DesignConversionBrowserQaProviderTests`
  - `XiigenExtensionApiContractsTests`
  - `ExtensionTokenQualityRunnerHarnessTests`
  - `ExtensionQuality03ProofContractRunnerTests`
- Test bodies and product code were not edited for this E4 task.

## Validation Command 1

Command:

```powershell
git diff --check -- Tests docs/evidence
```

Result:

- exit_code: 0
- output: empty

## Validation Command 2

Prior blocking run before current lock clear.

Exact command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionBrowserQaProviderTests|FullyQualifiedName~XiigenExtensionApiContractsTests|FullyQualifiedName~ExtensionTokenQualityRunnerHarnessTests|FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests" --no-restore --logger "console;verbosity=minimal"
```

Combined filter string:

```text
FullyQualifiedName~DesignConversionBrowserQaProviderTests|FullyQualifiedName~XiigenExtensionApiContractsTests|FullyQualifiedName~ExtensionTokenQualityRunnerHarnessTests|FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests
```

Result:

- exit_code: 1
- elapsed_time: 71.5 seconds
- pass_count: not emitted
- fail_count: not emitted
- skip_count: not emitted
- total_count: not emitted
- first_failure: build output copy lock before test result summary

First blocking output:

```text
C:\Program Files\dotnet\sdk\9.0.310\Microsoft.Common.CurrentVersion.targets(4916,5): warning MSB3026: Could not copy "obj\Debug\net9.0\LlmFromScratch.Tests.dll" to "bin\Debug\net9.0\LlmFromScratch.Tests.dll". Beginning retry 1 in 1000ms. The process cannot access the file 'C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\LlmFromScratch.Tests.dll' because it is being used by another process. The file is locked by: "testhost (11416)" [C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj]
```

Terminal blocking errors:

```text
C:\Program Files\dotnet\sdk\9.0.310\Microsoft.Common.CurrentVersion.targets(4916,5): error MSB3027: Could not copy "obj\Debug\net9.0\LlmFromScratch.Tests.dll" to "bin\Debug\net9.0\LlmFromScratch.Tests.dll". Exceeded retry count of 10. Failed. The file is locked by: "testhost (11416)" [C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj]
C:\Program Files\dotnet\sdk\9.0.310\Microsoft.Common.CurrentVersion.targets(4916,5): error MSB3021: Unable to copy file "obj\Debug\net9.0\LlmFromScratch.Tests.dll" to "bin\Debug\net9.0\LlmFromScratch.Tests.dll". The process cannot access the file 'C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\LlmFromScratch.Tests.dll' because it is being used by another process. [C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj]
```

Classification:

- combined provider/API/harness/proof-contract partition verified after serialization: false
- blocker_classification: stale or concurrent `testhost (11416)` locks `Tests\bin\Debug\net9.0\LlmFromScratch.Tests.dll`, preventing rebuild/copy before filtered tests can run.
- current_status_after_lock_clear_rerun: superseded by current-turn lock-clear and timeout evidence below.

## Current Lock Inspection And Action

PID 11416 inspection:

- `Get-Process -Id 11416` found `ProcessName: testhost`.
- path: `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\testhost.exe`
- start_time: `6/29/2026 10:29:09 PM`

PID 11416 CIM inspection:

- name: `testhost.exe`
- executable_path: `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\testhost.exe`
- command_line included:
  - `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\testhost.exe`
  - `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\LlmFromScratch.Tests.runtimeconfig.json`
  - `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\LlmFromScratch.Tests.deps.json`
- parent_process_id: `18132`

Parent PID 18132 CIM inspection:

- name: `dotnet.exe`
- command_line included:
  - `vstest.console.dll`
  - `--testCaseFilter:"FullyQualifiedName~DesignConversionBrowserQaProviderTests|FullyQualifiedName~XiigenExtensionApiContractsTests|FullyQualifiedName~ExtensionTokenQualityRunnerHarnessTests|FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests"`
  - `--logger:"console;verbosity=minimal"`
  - `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\LlmFromScratch.Tests.dll`

Action:

- stop command targeted only PID 11416: `Stop-Process -Id 11416 -Force -ErrorAction Stop`
- stop command wrapper returned exit_code: 1
- stop command wrapper printed output: empty
- follow-up `Get-Process -Id 11416` result: `PID_11416_NOT_RUNNING`
- follow-up CIM result: `CIM_PID_11416_NOT_RUNNING`
- classification: PID 11416 was a worktree-associated `testhost` and was absent after the stop-only attempt.

## Current Validation Command 3

Exact rerun command after lock clear:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionBrowserQaProviderTests|FullyQualifiedName~XiigenExtensionApiContractsTests|FullyQualifiedName~ExtensionTokenQualityRunnerHarnessTests|FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests" --no-restore --logger "console;verbosity=minimal"
```

Result:

- exit_code: 124
- elapsed_time_observed_by_tool: 604098 milliseconds
- pass_count: not emitted
- fail_count: not emitted
- skip_count: not emitted
- total_count: not emitted
- first_failure: not emitted
- timeout_output:

```text
command timed out after 604098 milliseconds
```

Post-timeout read-only process observation:

- repo-associated `dotnet.exe` PID 24780 remained visible by CIM with the same E4 `--testCaseFilter` and `LlmFromScratch.Tests.dll`.
- repo-associated `testhost.exe` PID 29716 remained visible by CIM under `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\testhost.exe`.
- These new post-timeout PIDs were not stopped by this task because the allowed lock-clear action was scoped to stale PID 11416 and the rerun had already timed out.

Classification:

- combined provider/API/harness/proof-contract partition verified after serialization and lock clear: false
- blocker_classification: exact filtered rerun timed out after 604098 ms; post-timeout process observation found repo-associated `dotnet.exe` PID 24780 and `testhost.exe` PID 29716 still visible.
- next_implementable_blocker_do_not_start: if those post-timeout PIDs are still running, safely clear/isolate the timed-out rerun processes, then rerun the same exact filtered command with timeout/hang evidence capture.

## Current Validation Command 4

Command:

```powershell
git diff --check -- Tests docs/evidence
```

Result before this packet edit:

- exit_code: 0
- output: empty

## Prior Validation Command 5

Prior command before current lock-clear task:

```powershell
git status --short
```

Result:

- exit_code: 0
- relevant E4 entries included the four modified allowed test classes and the new untracked collection file.
- full status also contained many pre-existing unrelated dirty entries outside this task scope.

## Boundary False Literals

combined_provider_api_harness_proof_contract_partition_verified_after_serialization_and_lock_clear=false
broad_unfiltered_test_verified=false
solution_green_verified=false
ci_green_verified=false
runtime_ready_verified=false
browser_e2e_verified=false
trained_feature_verified=false
visual_fidelity_verified=false
promotion_verified=false
deployment_verified=false
host_readiness_verified=false
s2_completion_verified=false

## Post-Packet Validation

- current_task_packet_readback: performed after packet edit and reported in the return packet.
- current_task_boundary_search: performed after packet edit and reported in the return packet.
- current_task_status_check: performed after packet edit and reported in the return packet.
