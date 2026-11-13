# Grouped DesignConversion Tests - 2026-07-06

claim_scope: grouped DesignConversion test-slice evidence only.
evidence_scope: captured `dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --filter "FullyQualifiedName~DesignConversion" --verbosity minimal` runs in the current slice; no full-suite, mergeability, ProductReady, deployment, commit, push, or PR evidence.
count_claimed: 1 green grouped DesignConversion run.
independent_evidence_count: 7 captured command results after the initial timed-out wrapper attempt: 5 grouped command results, 1 specific Pass164R reproduction/fix result pair, and 1 targeted regression check.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
requested_model: gpt-5.5
requested_reasoning_effort: xhigh
downgrade_needed: no for the grouped DesignConversion test slice. Broader full-suite, mergeability, ProductReady, deployment, commit, push, PR, and end-to-end readiness remain not claimed here.

## Command

`dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --filter "FullyQualifiedName~DesignConversion" --verbosity minimal`

## Result Timeline

- Initial exact command wrapper attempt timed out with shell exit code 124 after about 124 seconds. A later process check saw the exact dotnet test process running as PID 25436 and then exiting, but no exit code or test summary was captured for that process.
- Captured rerun before fixes: exit code 1; Failed 8, Passed 243, Skipped 0, Total 251.
- Captured rerun after first measurement-gate patch: exit code 1; Failed 4, Passed 247, Skipped 0, Total 251.
- Captured rerun after second measurement-gate patch: exit code 1; Failed 1, Passed 250, Skipped 0, Total 251.
- 2026-07-07 current specific Pass164R reproduction before fix: exit code 1; Failed 1, Passed 0, Skipped 0, Total 1. Failure remained `LocalLifecycleHookAttempted` false at `Tests/DesignConversionModelServerImprovementLoopTests.cs:630`.
- 2026-07-07 after approved-runtime local training-candidate policy patch, exact specific Pass164R command: exit code 0; Failed 0, Passed 1, Skipped 0, Total 1, Duration 57 s.
- 2026-07-07 grouped rerun after the first approved-runtime policy patch was too narrow for non-runtime local fixture rows: exit code 1; Failed 3, Passed 248, Skipped 0, Total 251. Regressions were `DesignConversionTrainingMeasurementGateTests.EvaluateLocalArtifactDbAsync_AllowsApprovedFixtureRowsOnlyAsTrainingCandidateArtifact`, `DesignConversionLocalFullLoopE2eTests.LocalNonProviderLoop_SavesConversionDisplaysUsefulnessAndGatesTrainingRows`, and `DesignConversionDpoStorageTests.LocalArtifactDb_RoundTripsConversionRenderJudgeAndTrainingCandidateWithoutPrivateFields`.
- 2026-07-07 targeted Pass164R plus three-regression rerun after non-runtime local fixture guard repair: exit code 0; Failed 0, Passed 4, Skipped 0, Total 4, Duration 54 s.
- Final 2026-07-07 exact grouped rerun: exit code 0; Failed 0, Passed 251, Skipped 0, Total 251, Duration 2 m 31 s.

## Fixes Applied

- `44.DesignConversionDomain/DesignConversionTrainingMeasurementGate.cs`
  - Adjusted the measurement gate so explicit local `training_candidate_only` plus product-not-ready rows are not blocked solely by `training_grade=false` or `not_training_grade=true`.
  - Added a bounded approved-runtime local training-candidate-only path so runtime-created rows with `approval_status=approved`, `approved_for_training=true`, `training_candidate_only=true`, and product-not-ready boundary metadata are not incorrectly excluded before lifecycle batch construction solely because review-only metadata still says `training_grade=false`, `not_training_grade=true`, `synthetic_negative=true`, `scaffold_negative=true`, or `high_quality_negative_pair_not_proven=true`.
  - Preserved honest blockers for pending/unapproved status, `approved_for_training=false`, provider missing, quarantine, user-private data, source manifest gaps, negative-quality gaps, leakage, and non-approved runtime rows.
  - Repaired the guard so non-runtime local fixture training-candidate-only rows with no pending/unapproved or `approved_for_training=false` markers keep the existing training-candidate artifact path.

## Previous Remaining Failure

- `DesignConversionModelServerImprovementLoopTests.Pass164R_LoopCreatedRuntimeCandidatesPassDefaultCountGatePromoteAndModelServerUsesCheckpoint`
  - Previous failure message: `Assert.True() Failure; Expected: True; Actual: False`.
  - Failing assertion: `promoted.Value.TrainingRun.LocalLifecycleHookAttempted` at `Tests/DesignConversionModelServerImprovementLoopTests.cs:630`.
  - Current specific rerun after fix passed: exit code 0; Failed 0, Passed 1, Skipped 0, Total 1.

## 2026-07-07 Interruption Update

Exact current specific-test command captured before interruption:

`dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --filter "FullyQualifiedName~DesignConversionModelServerImprovementLoopTests.Pass164R_LoopCreatedRuntimeCandidatesPassDefaultCountGatePromoteAndModelServerUsesCheckpoint" --verbosity minimal`

Captured result: exit code 1; Failed 1, Passed 0, Skipped 0, Total 1, Duration 1 m 8 s.

Captured failure:

- `DesignConversionModelServerImprovementLoopTests.Pass164R_LoopCreatedRuntimeCandidatesPassDefaultCountGatePromoteAndModelServerUsesCheckpoint`
- `Assert.True() Failure`
- `Expected: True`
- `Actual: False`
- Stack trace points to `C:\Projects\llm_mvp_core\Tests\DesignConversionModelServerImprovementLoopTests.cs:line 630`.

Diagnosis captured before interruption: `LocalLifecycleHookAttempted` is still false. The likely implementation cause is that runtime-approved local `training_candidate_only` rows pass handoff/preflight, but the shared training filter still excludes them before lifecycle batch construction because approval keeps review-only metadata such as `training_grade=false`, `not_training_grade=true`, `synthetic_negative=true`, `scaffold_negative=true`, and `high_quality_negative_pair_not_proven=true`.

No code fix was applied by this worker before interruption. The exact grouped DesignConversion command was not rerun after this specific-test reproduction, so no final grouped counts beyond the prior 1/251 grouped evidence are claimed here.

## Current Status

Result: passed for grouped DesignConversion slice.

Latest counts: Failed 0, Passed 251, Skipped 0, Total 251.

No broader completion claim. This is grouped DesignConversion slice evidence only.

## Next Slice

Next bounded slice, if requested: broader full-suite or mergeability verification. This file does not claim full-suite, ProductReady, deployment, commit, push, PR, or end-to-end readiness.
