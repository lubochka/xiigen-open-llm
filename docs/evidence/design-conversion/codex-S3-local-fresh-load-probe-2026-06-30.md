# Codex S3 local fresh-load probe evidence - 2026-06-30

## Scope

- Worker role: bounded local-only worker.
- Repository: `C:\Projects\llm_mvp_core`.
- Implemented slice: local metadata/checkpoint candidate fresh-load proof after DPO count gate pass and local `DesignConversionMachine.TrainBatchAsync` lifecycle hook.
- Not claimed: product readiness, S3 readiness, real trained weights, continue-training, held-out measurement, promotion, upload, external training.

## Context loaded

- `AGENTS.md` targeted rules loaded for honesty, bounded worker execution, no nested agent/codex.exe, no external training/upload, and checkpoint candidate != promotion boundary.
- `docs/skills/dcm-training-GUIDE.md` loaded for `SaveCheckpointAsync` vs `ExportAsync`, `LoadCheckpointAsync`, fresh-load ordering, and continue-training boundaries.
- `docs/skills/dcm-measurement-GUIDE.md` loaded for measurement gate, `PASS_AS_TRAINED_FEATURE` requirements, and promotion-blocked behavior when measurement is absent.
- Existing DesignConversion checkpoint/lifecycle/promotion code and tests inspected before implementation.

## Implemented local-only slice

- Added `DesignConversionLocalFreshLoadProbe` and `DesignConversionLocalFreshLoadProbeReport`.
- The probe saves a checkpoint through existing `DesignConversionMachine.SaveCheckpointAsync`.
- The probe loads the checkpoint through existing `DesignConversionMachine.LoadCheckpointAsync`.
- The probe verifies metadata continuity:
  - `MachineId`
  - `DomainTag`
  - `Version`
  - `ManifestRef`
  - training history count
- The training pipeline now runs the probe only after:
  - dataset count gate passed;
  - local lifecycle hook was attempted and produced batch results;
  - local checkpoint candidate exists.
- Pipeline temp checkpoint path policy:
  - root: `%TEMP%\llm_mvp_core\design-conversion-fresh-load-probe\<run-id>-<guid>\checkpoint.json`;
  - temp directory is deleted after the probe;
  - cleanup refuses paths outside that temp root;
  - no repo `artifacts/**` path is used.
- Promotion publisher now blocks upload when `pipelineResult.LifecycleEvidence.MeasurementGateStatus` is not `passed`, even if a local fresh-load probe passed.

## Narrow status fields

- `FreshLoadStatus`: `local_metadata_checkpoint_fresh_load_verified` only for local metadata/checkpoint continuity.
- `CheckpointContainsRealTrainedWeights`: `false`.
- `LocalFreshLoadProbe.CheckpointContainsRealTrainedWeights`: `false`.
- `ContinueTrainingStatus`: `not_proven`.
- `LocalFreshLoadProbe.ContinueTrainingProven`: `false`.
- `MeasurementGateStatus`: `not_run`.
- `LocalFreshLoadProbe.MeasurementGateRun`: `false`.
- `PromotionStatus`: `PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED`.
- `PromotionUploadAllowed`: `false`.
- `LocalFreshLoadProbe.PromotionUploadAllowed`: `false`.
- `NotProvenFields` keeps `trained_feature_fresh_load_metrics`, `continue_training`, `held_out_measurement_gate`, `promotion_upload`, and `real_trained_weights`.

## Changed files

- `44.DesignConversionDomain/DesignConversionLocalFreshLoadProbe.cs`
- `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs`
- `44.DesignConversionDomain/DesignConversionPromotion.cs`
- `Tests/DesignConversionCheckpointLifecycleTests.cs`
- `Tests/DesignConversionTrainingPipelineTests.cs`
- `Tests/DesignConversionPromotionTests.cs`
- `docs/evidence/design-conversion/codex-S3-local-fresh-load-probe-2026-06-30.md`

## Validation run

Command:

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --no-restore --filter "FullyQualifiedName~DesignConversionCheckpointLifecycleTests|FullyQualifiedName~DesignConversionTrainingPipelineTests|FullyQualifiedName~DesignConversionPromotionTests"
```

Result:

- Passed: 25
- Failed: 0
- Skipped: 0
- Duration reported by test runner: 10 s

Coverage in focused tests:

- local fresh-load probe passes for saved temp checkpoint metadata only;
- local fresh-load probe fails honestly for missing checkpoint with `CHECKPOINT_NOT_FOUND`;
- local fresh-load probe fails honestly for corrupt checkpoint with `CHECKPOINT_READ_FAILED`;
- pipeline probe uses temp path and deletes the temp checkpoint;
- pipeline snapshot verifies no repo `artifacts/**` file changes during the probe test;
- promotion/upload remains blocked when local fresh-load passed but measurement gate is `not_run`;
- existing adjacent count-gate/lifecycle/promotion behavior stays covered by focused DesignConversion tests.

Additional checks:

```powershell
git diff --check -- 44.DesignConversionDomain Tests docs/evidence/design-conversion
```

Result: passed with no output.

```powershell
$files = @('44.DesignConversionDomain/DesignConversionLocalFreshLoadProbe.cs','44.DesignConversionDomain/DesignConversionTrainingPipeline.cs','44.DesignConversionDomain/DesignConversionPromotion.cs','Tests/DesignConversionCheckpointLifecycleTests.cs','Tests/DesignConversionTrainingPipelineTests.cs','Tests/DesignConversionPromotionTests.cs','docs/evidence/design-conversion/codex-S3-local-fresh-load-probe-2026-06-30.md'); ...
```

Result: `slice whitespace check passed`.

```powershell
Test-Path -LiteralPath (Join-Path ([System.IO.Path]::GetTempPath()) 'llm_mvp_core\design-conversion-fresh-load-probe')
```

Result after empty temp-root cleanup: `False`.

Final artifact boundary note:

- `git status --short -- artifacts` shows many dirty `artifacts/**` paths in the worktree.
- This worker did not baseline `artifacts/**` at turn start, so this packet does not claim those repo artifact changes are absent or caused by this worker.
- The implemented probe uses `%TEMP%`, and the focused test `RunAsync_LocalFreshLoadProbeUsesTempPathAndDoesNotWriteRepoArtifacts` compared repo `artifacts/**` before/after during the probe and passed.

## Remaining S3 work

- Real trained weights are not proven.
- Held-out metrics are not run.
- Fresh-load metric reproduction tolerance is not proven.
- Continue-training from loaded checkpoint is not implemented or tested here.
- Measurement gate remains `not_run`.
- Promotion and upload remain blocked.
- External training/model upload was not run.

## Forbidden actions confirmation

- Did not touch `C:\Users\lubao\Documents\xiigen`.
- Did not touch `C:\Projects\xiigen-platform-dot-net-angular`.
- Did not stage, commit, push, branch switch, create PR, run `gh`, run browser/e2e, or run external training/model upload.
- Did not edit `artifacts/**`.
- Did not edit package files.
