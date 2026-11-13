# HD-2 model-server not-trained authority source patch - 2026-07-06

claim_scope: HD-2 focused model-server/API/status qualifier tests only; no ProductReady, git action, pass completion, mergeability, deploy, or overall completion claimed.
evidence_scope: direct source readback, source/evidence edits, and the focused post-edit `dotnet test` command for the two DesignConversion model-server API/status test classes.
count_claimed: 1 focused post-edit test command run; 18 focused tests passed; 0 builds, 0 pass completions.
independent_evidence_count: 1 focused command output.
subagent_id_or_NONE: NONE.
downgrade_needed: no for the HD-2 focused qualifier test slice; broader readiness/product claims remain out of scope.

## Exact Changed Files

- `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerHttpHandler.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerStatusHandler.cs`
- `Tests/DesignConversionModelServerApiTests.cs`
- `Tests/DesignConversionModelServerStatusReadbackTests.cs`
- `docs/evidence/design-conversion/codex-hd2-model-server-not-trained-authority-fix-20260706.md`

## Source Patch Evidence

- `DesignConversionModelServerApiResult` exposes `TrainedModelAuthority` and `NotTrainedModelAuthority` immediately after `LearnedInferenceApplied`.
- `DesignConversionModelServerStatusResponse` exposes `TrainedModelAuthority` and `NotTrainedModelAuthority` immediately after `LearnedInferenceApplied`.
- `DesignConversionModelServerHttpEvaluateResponse` exposes and forwards `TrainedModelAuthority` and `NotTrainedModelAuthority` immediately after `LearnedInferenceApplied`.
- `DesignConversionModelServerStatusReadbackResponse` exposes and forwards `TrainedModelAuthority` and `NotTrainedModelAuthority` immediately after `LearnedInferenceApplied`.
- `DesignConversionModelServerApi.BuildBoundaryMetadata` records `trained_model_authority=false` and `not_trained_model_authority=true` near `learned_inference_applied` and `LearnedInferenceApplied`.
- `DesignConversionModelServerHttpHandler.BuildBoundaryMetadata` records `trained_model_authority=false` and `not_trained_model_authority=true`, and re-forces those keys after copied API metadata.
- `DesignConversionModelServerStatusHandler.BuildBoundaryMetadata` records `trained_model_authority=false` and `not_trained_model_authority=true`, and re-forces those keys after copied API metadata.

## Focused Test Status - interrupted 2026-07-06

Pre-edit focused command run:

```text
dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --filter "FullyQualifiedName~DesignConversionModelServerApiTests|FullyQualifiedName~DesignConversionModelServerStatusReadbackTests" --verbosity minimal
```

Result: passed before test-assertion edits: Failed: 0, Passed: 18, Skipped: 0, Total: 18.

Interrupted post-edit run boundary: the earlier post-edit focused command started with the same command after adding API/HTTP/status authority assertions. The user interrupted before a test result was returned, so no post-edit pass/fail was claimed for that earlier run. Process inspection immediately after interruption showed a still-running `dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --filter ...` process (`PID 6272`) plus MSBuild child `dotnet.exe` processes; a follow-up targeted process check returned no matching `LlmFromScratch.Tests.csproj` or MSBuild `dotnet.exe` processes, but no final test result was captured.

Post-edit focused command run on 2026-07-06:

```text
dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --filter "FullyQualifiedName~DesignConversionModelServerApiTests|FullyQualifiedName~DesignConversionModelServerStatusReadbackTests" --verbosity minimal
```

Result: passed after test-assertion edits: Failed: 0, Passed: 18, Skipped: 0, Total: 18, Duration: 1 s.

Current evidence boundary: the focused HD-2 model-server/API/status qualifier tests pass after the not-trained-authority qualifier patch. This does not verify ProductReady, mergeability, deploy, full test suite, pass 5/50, or 50 passes.

next_review_or_fix: `final review pass 5B-recheck`
