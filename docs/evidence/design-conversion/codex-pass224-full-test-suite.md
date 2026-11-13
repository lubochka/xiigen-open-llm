# Pass224 Full Test Suite Evidence

pass224_verdict: NEEDS_FIX_FULL_SUITE_FAILED
full_test_command: dotnet test Tests\LlmFromScratch.Tests.csproj --logger "console;verbosity=minimal"
full_test_result: Failed - Failed: 21, Passed: 2318, Skipped: 0, Total: 2339, Duration: 27 m 14 s - LlmFromScratch.Tests.dll (net9.0)
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT

## Scope

This is local full-suite evidence only. It is not ProductReady, cloud, deploy,
public endpoint, provider, commit, push, PR, or merge evidence.

## Pre-Work Snapshot

- Branch: DesignToCode-clean.
- Branch relation: ahead 2 versus origin/DesignToCode-clean.
- Status count before work: total 463, staged 0, unstaged 194, untracked 269, conflicted 0.
- Cached index before work: 0 entries.
- Test project read: Tests\LlmFromScratch.Tests.csproj.

## Required Skill Load Status

- AGENTS.md: found/read, sha256 A8F6EB15692F727CD0EC5623E2C13109F3A1AC7C9EA31E4B3DC63C16E56F5D69.
- docs\SESSION-START-PROMPT.md: found/read, sha256 450961E74C8E9DB4CAA6D59441E8BFC76F2BFF973072ED13B8BF6D69DF0D9A8C.
- docs\skills\planning-session-startup-SKILL.md: found/read, sha256 C21D82E6CC29995FFBB14318686E5CCF0563DBCB07E9176FB1CD084ABCD9FE90.
- docs\skills\claim-verification-SKILL.md: found/read, sha256 F6D84C0F3815367C26012A81B6C8C6DB37EB4FDD2144263842785A4E694A0D68.
- docs\skills\audit-protocol-SKILL.md: found/read, sha256 FF0C6898DEBD3BF21EC538D6C32F1444044D545DC8996198A24FB3F3876D9D0A.
- docs\skills\goal-delivery-completeness-SKILL.md: found/read, sha256 2012CCFF230463C0AF2A6FFC3C0DA50230052BD4C1D96AC7F0E64DCE32972755.
- docs\skills\verification-before-completion-SKILL.md: found/read, sha256 BDEC446CFC62417BBE0D9A047307A455815F4849DAF31A3DA405B625FBB936DB.
- docs\skills\test-integrity-SKILL.md: found/read, sha256 FD545554BC746E7851119CC5BD9EC59B122F3194CD577A036220A5C17AF1805B.
- docs\skills\generated-code-review-SKILL.md: found/read, sha256 CEB6CF6BBFFF4121DB8FA898A21765AFDA0CF70E2F9334CD9FA8F171C21EDCC4.
- docs\skills\implementation-integrity-SKILL.md: found/read, sha256 18C33DD8C4D74770D5DAA0A8350BD18FA3F0756F13C05D5E2138E1AB5600DCF1.
- docs\skills\code-examination-SKILL.md: found/read, sha256 38DF19AB6D80A0EF247D538CD3A0D6EBE82C850FC76A099C6C4A412AE38D6AA6.
- docs\skills\qa-session-type-SKILL.md: found/read, sha256 BF5544598D4E358AA9333D2A0A47D4E010862DC7F58EBB2181908BD45B86B6DD.

## Full Suite Failures

The requested full-suite command completed and failed with 21 failing tests:

- ExtensionQualitySemanticGates19TargetLocatorPlumbingTests.QualitySemanticGates19_VerifiesTargetLocatorPlumbingAndKeepsReadinessClosed.
- DesignConversionTrainingPipelineTests.RunAsync_LocalFreshLoadProbeUsesTempPathAndDoesNotWriteRepoArtifacts.
- ExtensionQualitySemanticGates15PromotionReadinessTests.QualitySemanticGates15Artifacts_RecordPromotionReviewAndKeepClaimsClosed.
- ExtensionQualitySemanticGates12AdaptiveImprovementTests.QualitySemanticGates12Artifacts_ShowAdaptiveRubricImprovementButKeepReadinessClosed.
- LlmFromScratch.Tests.BackupExportImportSharePortabilityProbeTests.PortabilityProbeExportsImportsAndRedactsSharePackets.
- ExtensionQualitySemanticGates16CandidateProbeTests.QualitySemanticGates16Artifacts_CreateCandidateSetAndExactProbeRecordsWithoutPromotion.
- LlmFromScratch.Tests.Learn07RollbackRunnerTests.Learn07VerifiesRollbackBoundariesWithoutPromotion.
- ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates05Artifacts_BuildSemanticProofFrameworkForExtensionApiOutputs.
- Tests.ExtensionAdaptiveLearnability01Tests.ExtensionApi_LoadsUserAdaptiveAssetsFirst_AndProvesPortableContinueTrainingLifecycle.
- ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates06Artifacts_AddLeakageAuthorityControlsWithoutSemanticOverclaim.
- ExtensionQualitySemanticGates14ClosureTests.QualitySemanticGates14Artifacts_OpenOnlyBoundedSemanticGraduation.
- ExtensionQualitySemanticGates17ProductRuntimeVerificationTests.QualitySemanticGates17_ActivatesQsg16LocatorCandidateInTestOnlyProductApiRuntime.
- LlmFromScratch.Tests.Learn06HeldOutAblationRunnerTests.Learn06RunsHeldOutAblationControlsWithoutOverclaiming.
- ExtensionQualitySemanticGates11RubricTests.QualitySemanticGates11Artifacts_ScoreHumanReadableRubricWithoutReadinessOverclaim.
- Tests.ExtensionAdaptiveEffectGap02Tests.ExtensionApi_UsesScenarioAlignedUserAdaptiveAssets_AndChangesToken01OutputsWithoutClaims.
- ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates09Artifacts_RepairBaselinePromptToTaskAndOrchestrationRuntimeRowsWithoutReadinessOverclaim.
- ExtensionQualitySemanticGates18LiveHostTargetRollbackClosureTests.QualitySemanticGates18_RecordsLiveHostTargetBlockersAndKeepsRollbackClosed.
- ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates07Artifacts_AddSourceDisjointEvalAndAblationControlsWithoutSemanticOverclaim.
- ExtensionQualitySemanticGates13AdaptiveDisplayTests.QualitySemanticGates13Artifacts_ShowAdaptiveChatRepairAndTokenNonIncrease.
- ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates08Artifacts_AddSourceDisjointContinueTrainingProofWithoutCommonReadinessOverclaim.
- ExtensionQualitySemanticGates10NoOverclaimTests.QualitySemanticGates10Artifacts_AddGraduationCriteriaAndNoOverclaimGuard.

## Triage Evidence

- The DesignConversion failing test passed in isolation with command
  `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName=DesignConversionTrainingPipelineTests.RunAsync_LocalFreshLoadProbeUsesTempPathAndDoesNotWriteRepoArtifacts" --logger "console;verbosity=minimal"`.
  Result: Passed: 1, Failed: 0, Skipped: 0, Total: 1, Duration: 9 s.
- A representative OOM failure reproduced in isolation with command
  `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName=ExtensionQualitySemanticGates10NoOverclaimTests.QualitySemanticGates10Artifacts_AddGraduationCriteriaAndNoOverclaimGuard" --logger "console;verbosity=minimal"`.
  Result: Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 27 s. Failure: System.OutOfMemoryException at
  `ExtensionQualitySemanticGates10NoOverclaimTests.UpdateQsg10State`, line 458.
- The shared state file
  `docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
  was measured at 467045572 bytes.
- A representative non-OOM assertion failure reproduced in isolation with command
  `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName=ExtensionQualitySemanticGates19TargetLocatorPlumbingTests.QualitySemanticGates19_VerifiesTargetLocatorPlumbingAndKeepsReadinessClosed" --logger "console;verbosity=minimal"`.
  Result: Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 908 ms. Failure:
  `All proposed_locator_refs entries must reach core CheckSetResults.`

## Bounded Verdict

The requested full-suite gap is not closed. The evidence supports only:

- The full suite was run once and failed with 21 failures.
- The DesignConversion artifact-snapshot failure is not proven to be a deterministic
  DesignConversion regression because the exact failing test passed in isolation.
- At least one extension/quality semantic gate OOM failure is deterministic in
  isolation and is associated with a 467 MB shared execution-state JSON.
- At least one extension target-locator assertion failure is deterministic in
  isolation.

No safe Pass224 source/test repair was applied because the remaining deterministic
failures are in shared extension/adaptive artifact and state-management surfaces,
not in the already-scoped DesignConversion slice.

## Next Bounded Action

Open a focused extension/adaptive test-state repair worker to do one of the
following before rerunning the full suite:

1. Repair the shared extension execution-state update path so QSG/Learn tests do
   not parse and pretty-print a 467 MB JSON file into memory.
2. Repair or refresh the extension/adaptive artifact fixtures that now fail
   deterministic assertions around target locator refs, common asset load flags,
   selected skills, missing state keys, and display leakage.
3. Re-run the exact full-suite command after those repairs.
