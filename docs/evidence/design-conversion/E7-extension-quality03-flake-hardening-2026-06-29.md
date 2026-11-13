# E7 ExtensionQuality03 Flake Hardening Evidence

Date: 2026-06-29

## Claim Guard

claim_scope: Stabilized and instrumented only the single flaky CLI method path in `ExtensionQuality03ProofContractRunnerTests.Run_SecondRaterPacketArgumentResolvesOnlyInterRaterCategory`; reran the requested filtered evidence only.
evidence_scope: Read and edited `Tests/ExtensionQuality03ProofContractRunnerTests.cs`; checked repo-associated `dotnet.exe`/`testhost.exe`/`vstest.console.exe` processes before filtered runs; ran one targeted method filter and one class filter; ran `git diff --check -- Tests docs/evidence`.
count_claimed: 2 filtered dotnet test runs plus 1 diff hygiene check.
independent_evidence_count: 4 command records: pre-test process sweep, targeted method filter, class filter, diff check.
subagent_id_or_NONE: NONE
downgrade_needed: yes; evidence supports only the requested filtered method/class pass and helper diagnostics hardening, not broad solution readiness.

## Changed Files

- `Tests/ExtensionQuality03ProofContractRunnerTests.cs`
- `docs/evidence/design-conversion/E7-extension-quality03-flake-hardening-2026-06-29.md`

No product code was edited in this task.

## Process Cleanup

Pre-test command:

```powershell
$repo = (Resolve-Path .).Path; $names = @('dotnet.exe','testhost.exe','vstest.console.exe'); $matches = Get-CimInstance Win32_Process | Where-Object { $names -contains $_.Name -and $_.CommandLine -and $_.CommandLine.IndexOf($repo, [StringComparison]::OrdinalIgnoreCase) -ge 0 } | Select-Object ProcessId, Name, CreationDate, CommandLine; if ($matches) { $matches | Format-List } else { 'NO_REPO_ASSOCIATED_DOTNET_OR_TESTHOST_PROCESSES_FOUND' }
```

Result: `NO_REPO_ASSOCIATED_DOTNET_OR_TESTHOST_PROCESSES_FOUND`

Stopped processes: none.

Post-target spot check before the class filter returned the same `NO_REPO_ASSOCIATED_DOTNET_OR_TESTHOST_PROCESSES_FOUND`; stopped processes: none.

## Helper Hardening

Pre-existing in the working tree before this E7 edit:

- The class was already in `[Collection("ProcessSpawningTests")]`.
- `RunCli` already redirected stdout/stderr, used `UseShellExecute = false`, drained stdout/stderr asynchronously, used `WaitForExitAsync` with an explicit timeout, killed the process tree on timeout, and included command plus stdout/stderr tails in timeout exceptions.

E7 minimal hardening added in this task:

- Increased the CLI helper timeout from 60 seconds to 120 seconds so the inner process wrapper matches the requested single-method watchdog and avoids false timeout pressure on the flaky CLI path.
- Replaced the bare `Assert.Equal(0, cli.ExitCode)` with `AssertCliSuccess(cli)`.
- Added `CliProcessResult` carrying `ExitCode`, `StdOut`, `StdErr`, `Command`, and `Timeout`.
- Added nonzero-exit diagnostics that include the exact command, timeout, stdout tail, and stderr tail.

Semantics preserved: the test still requires CLI exit code 0, the same stdout marker, and the same JSON report assertions.

## Targeted Method Evidence

Command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_SecondRaterPacketArgumentResolvesOnlyInterRaterCategory" --no-restore --logger "console;verbosity=minimal"
```

Timeout ceiling: 120 seconds.

Exit code: 0.

Elapsed wall time from command runner: 75.1 seconds.

Emitted result summary:

```text
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 8 s - LlmFromScratch.Tests.dll (net9.0)
```

First failure: none emitted.

## Class Filter Evidence

Command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests" --no-restore --logger "console;verbosity=minimal"
```

Timeout ceiling: 240 seconds.

Exit code: 0.

Elapsed wall time from command runner: 40.4 seconds.

Emitted result summary:

```text
Passed!  - Failed:     0, Passed:    15, Skipped:     0, Total:    15, Duration: 9 s - LlmFromScratch.Tests.dll (net9.0)
```

First failure: none emitted.

## Diff Hygiene

Command:

```powershell
git diff --check -- Tests docs/evidence
```

Exit code: 0.

Output: no output.

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

## Narrow Non-Claims

I did not run broad unfiltered tests.
I did not run the combined four-class filter.
I did not verify solution-wide green status.
I did not verify CI.
I did not run browser/e2e/training/GitHub workflows.
I did not stage, commit, push, checkout, switch, or create a branch.
