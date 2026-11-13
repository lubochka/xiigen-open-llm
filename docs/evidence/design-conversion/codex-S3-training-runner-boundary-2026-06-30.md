# Codex S3 Training Runner Boundary Evidence - 2026-06-30

## Scope

Bounded S3 implementation slice only:
- Wire `DesignConversionDpoDatasetBuilder` into the local `DesignConversionTrainingPipeline` / `DesignConversionTrainingRunner` boundary.
- Expose dataset manifest and count-gate decision at the local boundary.
- Refuse the machine lifecycle hook when the required default dataset count gate fails.
- Prove 100 real synthetic accepted source-disjoint records can pass the count gate without calling the machine lifecycle hook.

Not claimed:
- S3 complete.
- External training complete.
- Real model training run complete.
- Checkpoint created.
- Model promotion or upload complete.
- Product readiness.

## Changed Code Scope

- `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs`
  - Added dataset options, required count-gate default, boundary decision record, dataset manifest/count-gate fields, and lifecycle-hook refusal when the count gate fails.
- `44.DesignConversionDomain/DesignConversionTrainingRunner.cs`
  - Propagates dataset manifest, count gate, and boundary decision from the pipeline result.
- `44.DesignConversionDomain/DesignConversionManifestPublisher.cs`
  - Adds dataset count-gate evidence refs into metadata-only manifests.
- `44.DesignConversionDomain/DesignConversionPromotion.cs`
  - Rejects promotion publication when the pipeline dataset count gate failed.
- `Tests/DesignConversionTrainingPipelineTests.cs`
  - Adds count-gate fail test, 100-record pass-without-training test, and runner-level propagation test.
- `Tests/DesignConversionPromotionTests.cs`
  - Updates fixture to use 100 accepted source-disjoint records for promotion-focused tests.
  - Adds focused promotion rejection proof for dataset count-gate failure with zero uploader calls and no upload/promotion claim.

## Commands Run

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionTrainingPipelineTests" --no-restore
```

Result: Passed. Failed: 0, Passed: 6, Skipped: 0, Total: 6.

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionTrainingPipelineTests" --no-restore
```

Result after adding runner-level boundary assertion: Passed. Failed: 0, Passed: 7, Skipped: 0, Total: 7.

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionDpoDatasetBuilderTests" --no-restore
```

Result: Passed. Failed: 0, Passed: 5, Skipped: 0, Total: 5.

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionPromotionTests --no-restore
```

Current final validation after adding the Arendt P1 promotion count-gate rejection test:
`Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 389 ms - LlmFromScratch.Tests.dll (net9.0)`

## Evidence Boundaries

- The passing 100-record test uses real synthetic records generated through `DesignConversionDpoGenerator`, not fake counts.
- The pass test sets `CallMachineLifecycleHook: false`, so it proves count-gate acceptance and boundary metadata only.
- The fail test verifies fewer than 50 train and fewer than 50 held-out records fails the required default count gate and produces no batches, no training items, no batch results, and no machine lifecycle hook attempt.
- The runner-level test verifies `DesignConversionTrainingRunner` returns the same dataset manifest/count-gate boundary decision exposed by the pipeline.
- The promotion update blocks promotion publication when the dataset count gate failed; this does not prove promotion readiness.
- The Arendt P1 promotion test builds a failed dataset count-gate pipeline result, passes otherwise-acceptable promotion gates, verifies promotion rejection evidence `DESIGN_CONVERSION_PROMOTION_REJECTED_BY_DATASET_COUNT_GATE`, and verifies no uploader call, no upload receipt, and no model-server upload/promotion claim.

## Forbidden Actions Confirmation

Not run in this slice:
- external training
- broad unfiltered test suite
- browser or e2e
- `gh`
- staging, commit, push, branch switch, PR
- edits under `artifacts/**`
- edits to package files
- access to `C:\Users\lubao\Documents\xiigen`
- access to `C:\Projects\xiigen-platform-dot-net-angular`

## Remaining S3 Work

Remaining S3 work is outside this bounded count-gate slice:
- real local training execution still needs an explicit authorized work order;
- checkpoint save/fresh-load/continue-training/export/import evidence remains unproven here;
- measurement gates from `dcm-measurement-GUIDE.md` remain unproven here;
- no model-server upload or promotion is proven here.
