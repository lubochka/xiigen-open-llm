# E6 ExtensionQuality03 fact split evidence - 2026-06-29

## Honesty guard

claim_scope: Diagnostic/evidence-only isolation of hanging fact(s) inside `ExtensionQuality03ProofContractRunnerTests`.
evidence_scope: Read-only inspection of `Tests\ExtensionQuality03ProofContractRunnerTests.cs`; repo-associated process checks; direct fact-level `dotnet test` filters; one 90s watchdog rerun of the initially timed-out fact; timeout cleanup; `git diff --check -- Tests docs/evidence`.
count_claimed: 15 methods covered; 16 fact-level command/tool records accepted, consisting of 15 initial direct method-filter attempts plus one focused 90s watchdog rerun of the initially timed-out method.
independent_evidence_count: 21 accepted command/tool records: method-list command, preflight process check, 15 direct fact command/tool records, one focused 90s watchdog rerun, timeout cleanup, post-run process check, and `git diff --check`.
subagent_id_or_NONE: NONE
downgrade_needed: yes - evidence shows one initial direct timeout candidate that did not reproduce under a focused 90s watchdog rerun; persistent fact-level hang is not proven, and class filter, combined partitions, broad tests, CI, runtime, browser, training, visual, promotion, deployment, and host readiness are not proven.

## Method list

Command:

```powershell
rg -n "^\s*\[(Fact|Theory)|^\s*public\s+(async\s+)?(Task|void)\s+[A-Za-z0-9_]+" Tests\ExtensionQuality03ProofContractRunnerTests.cs
```

Result: exit code 0. Fifteen `[Fact]` test methods were listed:

1. `Run_MissingApprovedCorpusRejectsHeldOutAndSourceDisjointPass`
2. `Run_ApprovedSplitAndSourceDisjointHappyPathPassesThoseEighteenRefs`
3. `Run_NearestNeighbourRowsCannotPassWithoutApprovedIndexManifest`
4. `Run_DeterministicAblationRejectsShortcutOnlyAndNotRunEvidence`
5. `Run_ReplayProbesStayRiskWithoutApprovedRawPrivateRefs`
6. `Run_IndependentSecondRaterRejectsDeterministicSelfChecker`
7. `Run_SecondRaterPacketArgumentResolvesOnlyInterRaterCategory`
8. `Run_MalformedMismatchedOrFakedSecondRaterPacketDoesNotPass`
9. `Run_SecondRaterPacketCannotCreateRunnerReadyUnlessAllSixtyThreeProofRefsPass`
10. `Run_SecondRaterPacketRejectsExternalModelTrainingOrMutationFlags`
11. `Run_RunnerReadyEvalInputsAbsentUntilAllSixtyThreeProofRefsPass`
12. `Run_LocalSevenOfSevenCandidateKeepsScorerLockedUntilGlobalSixtyThreePass`
13. `Run_NoExternalModelTrainingOrMutationFlagsRemainFalse`
14. `ResolveOutputPath_BlocksWritesOutsideAllowedOutputRoot`
15. `Run_WritesGeneratedArtifactsWithoutUtf8BomCrLfOrTrailingWhitespace`

## Process preflight

Command:

```powershell
Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('dotnet.exe','testhost.exe','testhost.x86.exe') -and ($_.CommandLine -like '*C:\Projects\llm_mvp_core*' -or $_.CommandLine -like '*LlmFromScratch.Tests*' -or $_.CommandLine -like '*ExtensionQuality03ProofContractRunnerTests*') } | Select-Object ProcessId,Name,CommandLine | Format-List
```

Result: exit code 0, no output. No stale repo-associated `dotnet` or `testhost` processes found before fact-level runs.

## Fact-level results

All completed commands used this shape:

```powershell
$sw = [System.Diagnostics.Stopwatch]::StartNew()
& dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.<MethodName>" --no-restore --logger "console;verbosity=minimal"
$code = $LASTEXITCODE
$sw.Stop()
Write-Output ("E6_EXIT_CODE={0}" -f $code)
Write-Output ("E6_ELAPSED_SECONDS={0:N3}" -f $sw.Elapsed.TotalSeconds)
exit 0
```

1. `Run_MissingApprovedCorpusRejectsHeldOutAndSourceDisjointPass`
   - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_MissingApprovedCorpusRejectsHeldOutAndSourceDisjointPass" --no-restore --logger "console;verbosity=minimal"`
   - Exit code: 0
   - Elapsed: 31.944s
   - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 569 ms
   - First failure/timeout: none

2. `Run_ApprovedSplitAndSourceDisjointHappyPathPassesThoseEighteenRefs`
   - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_ApprovedSplitAndSourceDisjointHappyPathPassesThoseEighteenRefs" --no-restore --logger "console;verbosity=minimal"`
   - Exit code: 0
   - Elapsed: 28.701s
   - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 329 ms
   - First failure/timeout: none

3. `Run_NearestNeighbourRowsCannotPassWithoutApprovedIndexManifest`
   - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_NearestNeighbourRowsCannotPassWithoutApprovedIndexManifest" --no-restore --logger "console;verbosity=minimal"`
   - Exit code: 0
   - Elapsed: 29.966s
   - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 611 ms
   - First failure/timeout: none

4. `Run_DeterministicAblationRejectsShortcutOnlyAndNotRunEvidence`
   - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_DeterministicAblationRejectsShortcutOnlyAndNotRunEvidence" --no-restore --logger "console;verbosity=minimal"`
   - Exit code: 0
   - Elapsed: 28.954s
   - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 487 ms
   - First failure/timeout: none

5. `Run_ReplayProbesStayRiskWithoutApprovedRawPrivateRefs`
   - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_ReplayProbesStayRiskWithoutApprovedRawPrivateRefs" --no-restore --logger "console;verbosity=minimal"`
   - Exit code: 0
   - Elapsed: 31.559s
   - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 476 ms
   - First failure/timeout: none

6. `Run_IndependentSecondRaterRejectsDeterministicSelfChecker`
   - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_IndependentSecondRaterRejectsDeterministicSelfChecker" --no-restore --logger "console;verbosity=minimal"`
   - Exit code: 0
   - Elapsed: 28.854s
   - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 373 ms
   - First failure/timeout: none

7. `Run_SecondRaterPacketArgumentResolvesOnlyInterRaterCategory`
   - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_SecondRaterPacketArgumentResolvesOnlyInterRaterCategory" --no-restore --logger "console;verbosity=minimal"`
   - Exit code: not emitted because command timed out before completion
   - Elapsed: tool timeout after 99116 ms
   - Counts emitted: none in accepted direct command output
   - First failure/timeout: `command timed out after 99116 milliseconds`
   - Focused 90s watchdog rerun command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_SecondRaterPacketArgumentResolvesOnlyInterRaterCategory" --no-restore --logger "console;verbosity=minimal"`
   - Focused 90s watchdog rerun result: `E6_JOB_EXIT_CODE=0`, `E6_90S_TIMED_OUT=False`, elapsed 38.653s
   - Focused 90s watchdog counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 7 s
   - Focused 90s watchdog cleanup: `E6_90S_CLEANUP_NONE`, `E6_90S_CLEANUP_AFTER_NONE`

8. `Run_MalformedMismatchedOrFakedSecondRaterPacketDoesNotPass`
   - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_MalformedMismatchedOrFakedSecondRaterPacketDoesNotPass" --no-restore --logger "console;verbosity=minimal"`
   - Exit code: 0
   - Elapsed: 30.187s
   - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 1 s
   - First failure/timeout: none

9. `Run_SecondRaterPacketCannotCreateRunnerReadyUnlessAllSixtyThreeProofRefsPass`
   - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_SecondRaterPacketCannotCreateRunnerReadyUnlessAllSixtyThreeProofRefsPass" --no-restore --logger "console;verbosity=minimal"`
   - Exit code: 0
   - Elapsed: 29.477s
   - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 386 ms
   - First failure/timeout: none

10. `Run_SecondRaterPacketRejectsExternalModelTrainingOrMutationFlags`
    - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_SecondRaterPacketRejectsExternalModelTrainingOrMutationFlags" --no-restore --logger "console;verbosity=minimal"`
    - Exit code: 0
    - Elapsed: 28.599s
    - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 955 ms
    - First failure/timeout: none

11. `Run_RunnerReadyEvalInputsAbsentUntilAllSixtyThreeProofRefsPass`
    - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_RunnerReadyEvalInputsAbsentUntilAllSixtyThreeProofRefsPass" --no-restore --logger "console;verbosity=minimal"`
    - Exit code: 0
    - Elapsed: 30.538s
    - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 944 ms
    - First failure/timeout: none

12. `Run_LocalSevenOfSevenCandidateKeepsScorerLockedUntilGlobalSixtyThreePass`
    - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_LocalSevenOfSevenCandidateKeepsScorerLockedUntilGlobalSixtyThreePass" --no-restore --logger "console;verbosity=minimal"`
    - Exit code: 0
    - Elapsed: 31.926s
    - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 455 ms
    - First failure/timeout: none

13. `Run_NoExternalModelTrainingOrMutationFlagsRemainFalse`
    - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_NoExternalModelTrainingOrMutationFlagsRemainFalse" --no-restore --logger "console;verbosity=minimal"`
    - Exit code: 0
    - Elapsed: 30.639s
    - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 546 ms
    - First failure/timeout: none

14. `ResolveOutputPath_BlocksWritesOutsideAllowedOutputRoot`
    - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.ResolveOutputPath_BlocksWritesOutsideAllowedOutputRoot" --no-restore --logger "console;verbosity=minimal"`
    - Exit code: 0
    - Elapsed: 30.507s
    - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 15 ms
    - First failure/timeout: none

15. `Run_WritesGeneratedArtifactsWithoutUtf8BomCrLfOrTrailingWhitespace`
    - Exact command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_WritesGeneratedArtifactsWithoutUtf8BomCrLfOrTrailingWhitespace" --no-restore --logger "console;verbosity=minimal"`
    - Exit code: 0
    - Elapsed: 31.651s
    - Counts emitted: Failed 0, Passed 1, Skipped 0, Total 1, test duration 685 ms
    - First failure/timeout: none

## Process cleanup actions

After the timeout in `Run_SecondRaterPacketArgumentResolvesOnlyInterRaterCategory`, repo-associated process cleanup command:

```powershell
$targets = Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('dotnet.exe','testhost.exe','testhost.x86.exe') -and ($_.CommandLine -like '*C:\Projects\llm_mvp_core*' -or $_.CommandLine -like '*LlmFromScratch.Tests*' -or $_.CommandLine -like '*ExtensionQuality03ProofContractRunnerTests*') }
```

Stopped verified repo-associated processes:

- PID 20564, `dotnet.exe`, command: `"C:\Program Files\dotnet\dotnet.exe" test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_SecondRaterPacketArgumentResolvesOnlyInterRaterCategory --no-restore --logger console;verbosity=minimal`
- PID 23580, `dotnet.exe`, command: `"dotnet" exec "C:\Program Files\dotnet\sdk\9.0.310\vstest.console.dll" --testAdapterPath:C:\Users\lubao\.nuget\packages\coverlet.collector\6.0.2\build\netstandard2.0\ --framework:".NETCoreApp,Version=v9.0" --testCaseFilter:"FullyQualifiedName~ExtensionQuality03ProofContractRunnerTests.Run_SecondRaterPacketArgumentResolvesOnlyInterRaterCategory" --logger:"console;verbosity:minimal" C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\LlmFromScratch.Tests.dll --artifactsProcessingMode-collect --testSessionCorrelationId:20564_183d0588-5d4d-4453-9b5e-f34193f9b6e1`
- PID 18308, `testhost.exe`, command: `"C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\testhost.exe" --runtimeconfig "C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\LlmFromScratch.Tests.runtimeconfig.json" --depsfile "C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\LlmFromScratch.Tests.deps.json" --port 50544 --endpoint 127.0.0.1:050544 --role client --parentprocessid 23580 --telemetryoptedin false`

Post-cleanup result: `E6_TIMEOUT_CLEANUP_AFTER_NONE`.

Focused 90s watchdog rerun of the same method completed without timeout and required no cleanup: `E6_90S_CLEANUP_NONE`, `E6_90S_CLEANUP_AFTER_NONE`.

Post-run repo-associated process check after all remaining fact runs:

```powershell
Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('dotnet.exe','testhost.exe','testhost.x86.exe') -and ($_.CommandLine -like '*C:\Projects\llm_mvp_core*' -or $_.CommandLine -like '*LlmFromScratch.Tests*' -or $_.CommandLine -like '*ExtensionQuality03ProofContractRunnerTests*') } | Select-Object ProcessId,Name,CommandLine | Format-List
```

Result: exit code 0, no output. No repo-associated `dotnet` or `testhost` processes remained.

## Hygiene check

Command:

```powershell
git diff --check -- Tests docs/evidence
```

Result: exit code 0, no output.

## Excluded diagnostic harness attempts

Two wrapper styles were attempted before accepting direct fact-level command evidence:

- `ProcessStartInfo.ArgumentList` wrapper: failed before `dotnet` started because `ArgumentList` was null in this PowerShell/.NET surface. Not counted as a fact result.
- Synchronous redirected-output batch wrapper: produced pass summaries but timeout labels were discarded because output was not drained during the build phase, which can make the wrapper block independently of the test. Not counted as fact results.

The accepted fact evidence is only the direct `dotnet test` command/tool records listed above.

## Narrow finding

The evidence supports only: the initial direct method filter for `Run_SecondRaterPacketArgumentResolvesOnlyInterRaterCategory` timed out once and left repo-associated `dotnet`/`testhost` processes that required cleanup, but a focused 90s watchdog rerun of the same method completed with exit code 0 and one passed test. The other 14 fact-level direct filters completed with exit code 0 and one passed test each. Persistent fact-level hang is not proven by this split.

## Required boundary literals

class_filter_verified=false
combined_partition_verified=false
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
