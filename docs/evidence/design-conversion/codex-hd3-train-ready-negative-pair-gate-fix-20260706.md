# HD-3 Train-Ready Negative Pair Gate Fix

claim_scope: source-only HD-3 gate patch for `DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons`
evidence_scope: source readback and requested non-test verification only
count_claimed: 0 product/pass completion
subagent_id_or_NONE: NONE
downgrade_needed: no

## Files

- `44.DesignConversionDomain/DesignConversionTrainingMeasurementGate.cs`
- `docs/evidence/design-conversion/codex-hd3-train-ready-negative-pair-gate-fix-20260706.md`

## Implementation Summary

`GetRecordBlockReasons` blocks records with these train-ready negative-pair boundary markers:

- pending/unapproved metadata status values through `approval_status`, `review_status`, and `status`
- `approved_for_training=false`
- `not_training_grade=true`
- `training_grade=false`
- `synthetic_negative=true`
- `scaffold_negative=true`
- `high_quality_negative_pair_not_proven=true`

The source already contained the explicit marker blocks for `approved_for_training`, `not_training_grade`, `training_grade`, `synthetic_negative`, `scaffold_negative`, and `high_quality_negative_pair_not_proven`; this slice added the plain metadata `status` key to the existing pending/unapproved helper path.

## Verification

Tests/builds were intentionally not run in this slice.

- `rg -n "approved_for_training|not_training_grade|training_grade|synthetic_negative|scaffold_negative|high_quality_negative_pair_not_proven|GetRecordBlockReasons" 44.DesignConversionDomain/DesignConversionTrainingMeasurementGate.cs docs/evidence/design-conversion/codex-hd3-train-ready-negative-pair-gate-fix-20260706.md`
  - Result: passed; readback showed `GetRecordBlockReasons` and all requested metadata/block-reason literals in source/evidence.
- `rg -n "HasPendingOrUnapprovedMetadata|approval_status|review_status|status" 44.DesignConversionDomain/DesignConversionTrainingMeasurementGate.cs docs/evidence/design-conversion/codex-hd3-train-ready-negative-pair-gate-fix-20260706.md`
  - Result: passed; readback showed `approval_status`, `review_status`, and `status` metadata checks.
- `git diff --check -- 44.DesignConversionDomain/DesignConversionTrainingMeasurementGate.cs docs/evidence/design-conversion/codex-hd3-train-ready-negative-pair-gate-fix-20260706.md`
  - Result: exit 0, no output. `git status` shows both files as untracked in this checkout, so this command reported no whitespace errors but did not have a tracked baseline for these paths.
- `rg -n "^(<<<<<<<|=======|>>>>>>>)" 44.DesignConversionDomain/DesignConversionTrainingMeasurementGate.cs docs/evidence/design-conversion/codex-hd3-train-ready-negative-pair-gate-fix-20260706.md`
  - Result: exit 1, no matches.
- `rg -n` changed-file mojibake marker scan using the configured marker regex from the work order
  - Result: exit 1, no matches.
- `rg -n "HD-3 focused test patch|claim_scope|evidence_scope|next_review_or_fix" docs/evidence/design-conversion/codex-hd3-train-ready-negative-pair-gate-fix-20260706.md`
  - Result: passed; evidence readback includes next slice `HD-3 focused test patch`.

next_review_or_fix: `HD-3 focused test patch`

## HD-3 Focused Test Patch Interrupted Status

claim_scope: focused HD-3 test update for train-ready measurement gate metadata blockers
evidence_scope: test-file edit/readback plus interrupted focused test command; focused test result not completed
count_claimed: 0 product/pass completion
subagent_id_or_NONE: NONE
downgrade_needed: yes - focused dotnet test result incomplete

### Implementation Update

`Tests/DesignConversionTrainingPipelineTests.cs` now extends the runner-path metadata exclusion test with:

- `status=pending`
- `status=pending` plus `not_training_grade=true`
- `status=pending` plus `high_quality_negative_pair_not_proven=true`

The test still asserts only the clean record reaches `DpoBuild.AcceptedRecords` and `PipelineResult.TrainingItems`, while all blocked records are quarantined by the runner path.

### Interrupted Verification

- Command attempted: `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionTrainingPipelineTests"`
- Result: interrupted by user before a test result was captured in this turn.

next_review_or_fix: `HD-3 smaller focused single-test run: dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionTrainingPipelineTests.RunTrainingRecordsAsync_ExcludesPendingStatusTrainingGradeAndNegativeMetadata"`

## HD-3 Smaller Focused Single-Test Run

claim_scope: exact HD-3 smaller focused single-test run only
evidence_scope: current `dotnet test` output for the exact named test
count_claimed: 1 exact single-test run
subagent_id_or_NONE: NONE
downgrade_needed: no

### Verification

- Command: `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionTrainingPipelineTests.RunTrainingRecordsAsync_ExcludesPendingStatusTrainingGradeAndNegativeMetadata" --verbosity minimal`
- Result: passed; `Failed: 0, Passed: 1, Skipped: 0, Total: 1`.

next_review_or_fix: `final review pass 5C-recheck`
