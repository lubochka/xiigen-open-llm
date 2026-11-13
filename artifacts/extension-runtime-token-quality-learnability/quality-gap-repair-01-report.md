# QUALITY-GAP-REPAIR-01 report

## What changed

This bounded slice repaired three concrete quality blockers without promoting common checkpoints or claiming semantic quality.

1. **PromptToTask no-matching-pattern coverage**

   The API now sends the user prompt text into `PromptToTaskRequest.RawPrompt`, and `PromptToTaskTaskPatternUnit` now derives normalized prompt-token signals for its transparent baseline nearest-pattern match.

   Claim status: baseline only. This is not semantic quality.

2. **OrchestrationReasoning manifest/checkpoint mismatch**

   The API now resolves the orchestration checkpoint from `manifest.checkpointRef` before using the legacy fallback path.

   Claim status: asset path repair only. This is not common model readiness.

3. **SkillSelection empty/second-pass-only output**

   The API now sends the user prompt text into `SkillSelectionRequest.Prompt`. The test uses a local PromptContext baseline trained through `SkillSelectionMachine.TrainMachineBatchAsync` and verifies non-empty final skills without candidate-position evidence as final authority.

   Claim status: test-local baseline only. This is not full semantic SkillSelection.

## What remains

The real user-local adaptive model effect is still not proven. No adaptive checkpoint was promoted, no locator was updated, and no token savings or quality improvement claim is allowed from this slice.

## Verification

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQualityGapRepairTests|FullyQualifiedName~ExtensionToken04QualityScoringArtifactsTests" --no-restore --nologo
```

Result: PASS, 5 passed, 0 failed.

## Next action

Rerun TOKEN-04 scoring after this repair. Keep token savings, quality improvement, semantic quality, learnability, common model readiness, target promotion, and locator update claims false unless the rerun proves them.
