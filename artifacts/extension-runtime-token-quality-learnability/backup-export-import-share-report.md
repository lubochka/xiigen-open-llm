# PORTABILITY-01 backup/export/import/share report

Verdict: FAIL

## Commands
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~BackupExportImportSharePortabilityProbeTests --no-restore --logger "console;verbosity=normal"`

## Command results
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~BackupExportImportSharePortabilityProbeTests --no-restore --logger "console;verbosity=normal"` -> PASS; Test Run Successful. Total tests: 1. Passed: 1. Failed: 0.

## Track verdicts
- PromptToTaskMachine: PARTIAL_OVERLAY_PORTABILITY_ONLY
  Boundary: `PromptToTaskAdaptiveTrainer.LoadCheckpoint imported overlay`
  Export/import/hash/share: export=True; importLoad=True; hashes=True; redactedShare=True
  Host/production runtime invocation: False
  Test-only machine Plan probe: False
  Observed: taskProfiles=1; trainedSignalIds=1
  Limitation: PromptToTask imported target is an adaptive overlay checkpoint only; no full PromptToTaskMachine checkpoint plus manifest is available in this slice.
- OrchestrationReasoningMachine: FAIL
  Boundary: `OrchestrationReasoningMachine.LoadCheckpointAsync`
  Export/import/hash/share: export=True; importLoad=False; hashes=True; redactedShare=True
  Host/production runtime invocation: False
  Test-only machine Plan probe: False
  Observed: ORCHESTRATION_REASONING_SOURCE_ADMISSION_REJECTED
  Limitation: Imported target checkpoint failed to load.
- SkillSelectionMachine: PARTIAL_OVERLAY_PORTABILITY_ONLY
  Boundary: `SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint imported overlay`
  Export/import/hash/share: export=True; importLoad=True; hashes=True; redactedShare=True
  Host/production runtime invocation: False
  Test-only machine Plan probe: False
  Observed: correctionProfiles=1; trainedSignalIds=1
  Limitation: SkillSelection imported target is a correction adaptive overlay checkpoint only; no full SkillSelectionMachine checkpoint plus manifest is available in this slice.

## Share packet policy
- Raw private prompt text included: false
- Absolute private paths are local-private evidence only and not shareable.

## What this proves
- Each track can be copied from local target/backup assets into an export packet, staged into an import path, hash-verified, and loaded through the available public loader boundary.
- OrchestrationReasoning target import uses OrchestrationReasoningMachine.LoadCheckpointAsync from an imported checkpoint, manifest, and admission proof, plus a separate test-only deterministic machine Plan probe.
- PromptToTask and SkillSelection target imports are overlay-only proofs through public adaptive/correction loaders.
- The redacted share manifests contain hashes, file roles, sizes, and local-private path markers only; no raw private prompt text is included.

## What this does not prove
- This does not prove semantic quality, token savings, universal learnability, promotion readiness, host runtime readiness, common model readiness, or locator update safety.
- The OrchestrationReasoning Plan request is synthetic and pattern-derived from the imported learned pattern Required* signals. It is a deterministic positive portability probe only, not semantic/generalization quality evidence and not host/production runtime invocation.
- PromptToTask and SkillSelection remain PARTIAL_OVERLAY_PORTABILITY_ONLY because only adaptive overlay checkpoint assets are available for this slice.
- Local-private paths are included only as non-shareable evidence markers and must not be published as share packet contents.

## Next phase
- FINAL-REVIEW-05