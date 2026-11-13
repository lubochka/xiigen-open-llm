# Pass104 Runtime DPO Candidate Persistence

Current narrow verdict: PASS for the evidence-only Pass104 slice. The supported claim is that completed in-process DesignConversion runtime evaluations can persist sanitized, training-candidate-only DPO rows into the local artifact DB, and the existing local usefulness/readback surface can show those runtime-created rows. The artifact remains `product_not_ready`; this is not product readiness.

## What Pass104 Proves

Pass104 proves local artifact persistence of sanitized runtime-created training candidates. The supported runtime path is:

1. A submitted in-process runtime job completes evaluation.
2. The runtime builds runtime-created DPO candidates from that evaluation.
3. The local artifact DB record stores those candidates in `DpoCandidates`.
4. The local display/readback/usefulness surface can show candidate metadata, usefulness factors, review-pending status, and product-not-ready boundaries.

This maps only the subset of the original requirement where runtime evaluation results are saved to a DB/artifact store for learning, and DPO candidates are visible with usefulness/readback metadata. Larger product requirements remain in the execution queue below.

## Evidence References

Code evidence:

- `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs:224` defines `PersistCompletedEvaluationAsync`.
- `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs:235` to `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs:247` builds runtime-created candidates with `BuildRuntimeCreatedTrainingCandidates`, passes accepted records into `DesignConversionLocalArtifactDbRecord.FromEvaluation`, and attaches candidate-count metadata.
- `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs:418` to `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs:454` marks local artifact DB persistence with runtime-created candidate count, review-required, approval-pending, `training_candidate_only`, `production_dpo=false`, `product_not_ready=true`, held-out not proven, source-disjoint not proven, production DB not ready, and trained model not ready.
- `44.DesignConversionDomain/DesignConversionDpoGenerator.cs:936` to `44.DesignConversionDomain/DesignConversionDpoGenerator.cs:985` creates runtime candidates only after the runtime evidence gate passes, and returns warnings for runtime-created candidate review, approval, training-candidate-only, and not-product-ready boundaries.
- `44.DesignConversionDomain/DesignConversionDpoGenerator.cs:1109` to `44.DesignConversionDomain/DesignConversionDpoGenerator.cs:1264` builds the stored runtime DPO example with hashed chosen/rejected refs, pending review metadata, training-candidate-only metadata, non-production boundaries, held-out not-proven, source-disjoint not-proven, and `raw_payloads_included=false`.
- `44.DesignConversionDomain/DesignConversionDpoGenerator.cs:1280` to `44.DesignConversionDomain/DesignConversionDpoGenerator.cs:1348` builds internal state/source payloads from refs and keeps raw payloads excluded.
- `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:101` to `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:180` wraps `dpoCandidates` from `FromEvaluation` into stored local artifact DB candidates and evidence refs.
- `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:499` and `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:529` provide local append/list persistence for the JSONL artifact DB.
- `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:1046` to `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:1167` exposes `QueryDisplayRowsAsync`, including `TrainingCandidateOnly`, usefulness factors, product-not-ready boundary metadata, and warnings.

Test evidence:

- `Tests/DesignConversionThinClientJobRuntimeTests.cs:139` defines `RunAsync_WithConfiguredLocalArtifactDbPersistsSanitizedCompletedEvaluation`.
- `Tests/DesignConversionThinClientJobRuntimeTests.cs:154` to `Tests/DesignConversionThinClientJobRuntimeTests.cs:158` checks production DB and trained-model boundaries remain false.
- `Tests/DesignConversionThinClientJobRuntimeTests.cs:179` to `Tests/DesignConversionThinClientJobRuntimeTests.cs:183` checks local DB metadata, `training_candidate_only`, one `runtime_created_dpo_candidate`, and pending review status.
- `Tests/DesignConversionThinClientJobRuntimeTests.cs:199` to `Tests/DesignConversionThinClientJobRuntimeTests.cs:225` checks the single stored DPO candidate is training-candidate-only, product-not-ready, not quarantined, review pending, not approved for training, not production DPO, and sanitized away from raw private payloads.
- `Tests/DesignConversionThinClientJobRuntimeTests.cs:227` to `Tests/DesignConversionThinClientJobRuntimeTests.cs:242` checks `QueryDisplayRowsAsync` returns usefulness/readback metadata, review-pending status, `source_disjoint=not_proven`, and runtime-created review-pending warnings.
- `Tests/DesignConversionThinClientJobRuntimeTests.cs:244` to `Tests/DesignConversionThinClientJobRuntimeTests.cs:247` checks approved training candidates remain empty before explicit approval.
- `Tests/DesignConversionThinClientJobRuntimeTests.cs:258` to `Tests/DesignConversionThinClientJobRuntimeTests.cs:266` checks persisted local artifact text contains production DB boundary metadata and excludes raw private payload markers.
- `Tests/DesignConversionThinClientJobRuntimeTests.cs:636` defines `QueryLocalArtifactReadbackAsync_ReturnsRuntimePersistedRecordAndCreatedDpoRows`.
- `Tests/DesignConversionThinClientJobRuntimeTests.cs:648` to `Tests/DesignConversionThinClientJobRuntimeTests.cs:756` checks local readback and usefulness API rows show runtime-created DPO candidate metadata while preserving non-public, non-production, non-trained, sanitized boundaries.

## Focused Test Evidence From Darwin

Darwin sub-agent id as seen by parent: `019f408d-cc5d-7291-b6a8-1507554f1db0`.

Darwin reported this focused test command, with no edits and no git writes:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionThinClientJobRuntimeTests" --no-restore --logger "console;verbosity=minimal"
```

Reported result: Total 9, Passed 9, Failed 0, Skipped 0.

I did not rerun the dotnet test command while writing this packaging artifact. This artifact uses Darwin's focused 9/9 test result as sub-agent evidence and my local code/test/artifact readback as packaging verification.

## Mental Simulation

The supported chain is complete for the narrow slice:

1. `RunAsync` receives a submitted job and completes evaluation through the injected in-process evaluator.
2. `PersistCompletedEvaluationAsync` calls `BuildRuntimeCreatedTrainingCandidates` when a local artifact DB is configured.
3. The generator creates a review-pending runtime DPO candidate only after runtime evidence is sufficient.
4. `FromEvaluation` stores those candidates in the local DB record.
5. Local display/readback APIs expose candidate count, usefulness score/factors, review status, approval status, and boundary metadata.

No link in this simulation supports product readiness, public API readiness, production storage, external visual AI execution, real training, real trained weights, upload, broad-suite health, or mergeability.

## Explicit Boundaries

- Not product ready.
- Not public/server API.
- Not production DB or production service.
- Not external visual AI judge.
- Not real training and not real trained weights.
- Not upload, promotion, or deployment.
- Not a broad suite result.
- Not mergeability.
- Not source-disjoint proof.
- Not held-out proof.
- Not proof of high-quality negative pairs.
- Not approval to train from these runtime-created candidates.

## Remaining Execution Queue

Remaining work, derived from the Pass103 residual queue and current Pass104 boundaries:

1. Public/server API adapter for submit, run/status, artifact readback, and DPO row readback.
2. Browser-backed multi-resolution QA as the default runtime path, not only a bounded/local provider path.
3. Iterative improvement loop that continues until target quality or budget exhaustion.
4. External visual AI/provider integration with cost and tier boundaries.
5. Production DB/service adapter and migration boundary.
6. Real DPO quality gates, including high-quality negative pairs, source-disjoint proof, and held-out proof.
7. Measured retraining with fresh-load and continue-training evidence.
8. User retrain/upload surface plus upload/promotion path after measurement passes.
9. Mergeability proof.

## Narrow Acceptance Checklist

- [x] Runtime evaluation can be persisted to the local artifact DB when a DB is configured.
- [x] Runtime-created DPO candidates are passed into the local artifact DB record.
- [x] Stored candidates are marked `training_candidate_only`.
- [x] Stored candidates and readback rows preserve `product_not_ready`.
- [x] Review and approval stay pending; approved training candidates remain empty before explicit approval.
- [x] The display/usefulness/readback surface can show runtime-created rows with usefulness metadata.
- [x] Sanitization boundaries exclude raw HTML, CSS, private paths, exact prompts, private file refs, and external provider payloads in the tested persistence/readback path.
- [x] Darwin's focused sub-agent test evidence reports 9/9 passing.
- [x] Broader product/server/training/upload/mergeability claims remain explicitly unproven.
