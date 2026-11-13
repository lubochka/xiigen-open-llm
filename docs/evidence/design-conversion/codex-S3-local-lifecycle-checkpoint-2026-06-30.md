# Codex S3 local lifecycle/checkpoint boundary evidence - 2026-06-30

claim_scope:
- Implemented a bounded local DesignConversion lifecycle boundary after the DPO dataset count gate.
- The pipeline can represent local checkpoint/export/import evidence after the machine hook is called.
- Bounded P1 fix: local lifecycle training batches now use only dataset `TrainRows`, never `HeldOutRows`.
- Current bounded hardening slice replaces the public replacement `TrainBatchAsync` delegate seam with an observer-only `DesignConversionTrainBatchObservation` seam for train/held-out identity overlap and train-split mapping failure guards.
- The current tests prove refusal before local lifecycle hook side effects by asserting observer notification count is exactly `0` and the machine export version is unchanged after each guard failure.
- This does not prove real trained weights, fresh-load from disk in the pipeline, continue-training, model upload, S3 completion, or product readiness.

evidence_scope:
- Local/in-process DCM domain code and focused tests only.
- Held-out rows remain in the dataset manifest/count gate and are reserved for metrics/evaluation only.
- Production guard codes already existed in `DesignConversionTrainingPipeline.cs`; this hardening slice edited production code only to replace the substituting test seam with an observer-only notification that cannot return training success or bypass `DesignConversionMachine.TrainBatchAsync`.
- No external training, model-server upload, browser/e2e, gh, staging, commit, push, branch switch, or package edits were run.
- Existing worktree was already dirty before this slice, including DCM files and forbidden `artifacts/**`; this worker did not edit `artifacts/**`.

implemented_slice:
- Added `DesignConversionTrainingLifecycleEvidence` to `DesignConversionTrainingPipelineResult`.
- Count-gate failure and scaffold bypass now report lifecycle evidence as not attempted and keep all checkpoint/export/import/fresh-load/continue-training fields `not_proven` or `not_run`.
- Count-gate pass with `CallMachineLifecycleHook=true` calls the local `TrainBatchAsync` hook only for train-split rows and records:
  - checkpoint candidate status: `local_scaffold_checkpoint_candidate_only`;
  - checkpoint import status: `in_process_checkpoint_import_verified`;
  - export status: `metadata_only_export_verified`;
  - import status: `metadata_only_import_verified`;
  - `FreshLoadStatus: "not_proven"`;
  - `ContinueTrainingStatus: "not_proven"`;
  - `MeasurementGateStatus: "not_run"`;
  - promotion status: `PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED`;
  - promotion upload allowed: `false`.
- `RunAsync` maps dataset `TrainRows` back to train-ready records by `RecordIdentityHash`; if train and held-out identities overlap or the train split cannot be mapped exactly, the pipeline fails instead of training a mixed split.
- Exact lifecycle evidence marker represented in code/tests:
  `DESIGN_CONVERSION_LOCAL_LIFECYCLE_HOOK_CALLED_AFTER_COUNT_GATE_PASS`.
- Manifest metadata now includes lifecycle evidence refs from the pipeline result.
- Focused tests prove the local lifecycle hook is not called when the count gate fails, is not called on scaffold bypass, is called after a real synthetic source-disjoint 50+50 count-gate pass, trains 50 train rows instead of all 100 accepted rows, excludes held-out row identities from training batches, and does not permit promotion/upload without a fresh-load/measurement gate pass.

current_hardening_slice:
- Replaced the optional `TrainBatchAsync` replacement delegate with an optional observer-only `Action<DesignConversionTrainBatchObservation>` on `DesignConversionTrainingPipeline`; default construction still calls only `DesignConversionMachine.TrainBatchAsync`.
- The observer receives batch id, domain tag, training item count, training item ids, and source ids after the real machine training call succeeds; it cannot return `OperationResult<LearningBatchResult>`, cannot report training success, and cannot substitute batch results.
- Updated `RunAsync_SourceDisjointHundredAcceptedRecordsPassCountGateAndCallsLifecycleHookWithHonestEvidence` to assert observer notifications only cover train-row item ids, exclude held-out ids, match real machine batch result ids, and cannot replace the real machine `RowsTrained` result.
- Updated `RunAsync_RefusesBeforeTrainingWhenTrainAndHeldOutIdentitiesOverlap`, which builds duplicate record identities across disjoint source groups, asserts `DESIGN_CONVERSION_DPO_TRAIN_HELDOUT_IDENTITY_OVERLAP`, and asserts observer notification count is `0`.
- Updated `RunAsync_RefusesBeforeTrainingWhenTrainSplitCannotMapBackToRecords`, which uses a test-only `IReadOnlyList` helper to simulate a record source changing after dataset build, asserts `DESIGN_CONVERSION_TRAIN_SPLIT_MAPPING_FAILED`, and asserts observer notification count is `0`.
- Both negative tests also assert the `DesignConversionMachine` export version remains unchanged, keeping side-effect evidence as a secondary check rather than the only proof.

changed_files_current_slice:
- `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs`
- `Tests/DesignConversionTrainingPipelineTests.cs`
- `docs/evidence/design-conversion/codex-S3-local-lifecycle-checkpoint-2026-06-30.md`

commands_run:
1. `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionTrainingPipelineTests|FullyQualifiedName~DesignConversionCheckpointLifecycleTests|FullyQualifiedName~DesignConversionPromotionTests" --no-restore --logger "console;verbosity=normal"`
   - Result: `Test Run Successful. Total tests: 21. Passed: 21.`
2. Direct content/trailing-whitespace check over changed files.
   - Result: `content_whitespace_check=PASS files=3 markers=7`.
3. `git diff --check`
   - Result: `FAIL` due pre-existing dirty `artifacts/**` trailing-whitespace entries outside this bounded worker's allowed write scope.
4. `git diff --check -- 44.DesignConversionDomain/DesignConversionTrainingPipeline.cs Tests/DesignConversionTrainingPipelineTests.cs docs/evidence/design-conversion/codex-S3-local-lifecycle-checkpoint-2026-06-30.md`
   - Result: `PASS`.

test_results:
- Focused lifecycle/promotion/checkpoint tests: 21 passed, 0 failed.
- Adjacent DPO dataset/count-gate generator/builder tests: not rerun in current hardening slice.
- Broad unfiltered tests: not run.
- Browser/e2e/runtime host probes: not run.
- External training/model upload: not run.
- Full-worktree `git diff --check`: failed on unrelated pre-existing forbidden `artifacts/**` changes; this slice did not edit those files.

remaining_S3_work:
- Real trained weights are still not proven. The checkpoint candidate is scaffold/local evidence only.
- Pipeline-level disk `SaveCheckpointAsync` plus `LoadCheckpointAsync` fresh-load is not executed in this slice.
- Continue-training from a fresh-loaded checkpoint is not executed in this slice.
- Held-out quality metrics, ablation, leakage, speed, source/license full measurement gate, and promotion thresholds are not closed by this slice.
- Model-server upload remains blocked unless a separate measurement/promotion gate passes.

next_bounded_work_order:
- Add a local-only fresh-load probe option or separate bounded runner that saves a checkpoint to a temp local path, loads it through `DesignConversionMachine.LoadCheckpointAsync`, records the fresh-load delta as evidence, and still keeps promotion blocked unless held-out measurement/ablation/source/license gates pass. Do not upload, do not run external training, and do not edit `artifacts/**`.

forbidden_actions_confirmed:
- Did not stage, commit, push, branch switch, create PR, run gh, browser, e2e, external training, or model upload.
- Did not edit `artifacts/**` or package files.
- Did not touch `C:\Users\lubao\Documents\xiigen`.
- Did not touch `C:\Projects\xiigen-platform-dot-net-angular`.
