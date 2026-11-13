# LEARN-05 fresh-load report

Verdict: PASS_WITH_LIMITATIONS

## Commands

- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn05FreshLoadRunnerTests --no-restore --logger "console;verbosity=normal"`

## Command results

- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn05FreshLoadRunnerTests --no-restore --logger "console;verbosity=normal"` -> IN_PROCESS_EVIDENCE_ONLY; This Learn05 artifact is written by the in-process fixture. The external dotnet test exit code and pass/fail status are recorded by the executor TRX/log, not self-authored here.

## Track verdicts

- PromptToTaskMachine: PARTIAL_ADAPTIVE_LOADER_ONLY
  Boundary: `PromptToTaskAdaptiveTrainer.LoadCheckpoint`
  Fixture path: `<test-temp-root>/prompt-to-task/.xiigen/prompt-to-task/adaptive-checkpoint.json`
  Runtime invocation succeeded: false
  Test-only in-process probe succeeded: false
  Observed: adaptive_checkpoint_loaded; taskProfiles=1; trainedSignalIds=1
  Limitation: Narrow public adaptive loader only. The test-owned deterministic fixture is an adaptive overlay checkpoint without a PromptToTaskMachine manifest, so this does not prove PromptToTaskMachine.LoadCheckpointAsync or Translate.
- OrchestrationReasoningMachine: PASS_PUBLIC_MACHINE_BOUNDARY
  Boundary: `OrchestrationReasoningMachine.LoadCheckpointAsync + OrchestrationReasoningMachine.Plan`
  Fixture path: `<test-temp-root>/orchestration-reasoning/checkpoint.json`
  Runtime invocation succeeded: false
  Test-only in-process probe succeeded: true
  Observed: loaded_guide_state_semantic_plan_produced; or-learned-pattern:learn-04-local-runtime-token-quality-boundary
  Limitation: Fresh-load proves loadability and public machine invocation for the test-owned deterministic fixture only. It does not prove semantic quality, held-out/source-disjoint behavior, ablation, rollback, or promotion readiness.
- SkillSelectionMachine: PARTIAL_ADAPTIVE_LOADER_ONLY
  Boundary: `SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint`
  Fixture path: `<test-temp-root>/skill-selection/.xiigen/smart-skills/corrections/adaptive-checkpoint.json`
  Runtime invocation succeeded: false
  Test-only in-process probe succeeded: false
  Observed: correction_adaptive_checkpoint_loaded; correctionProfiles=1; trainedSignalIds=1
  Limitation: Narrow public correction-adaptive loader only. The test-owned deterministic fixture is not a full SkillSelectionMachine checkpoint/manifest, so this does not prove SkillSelectionMachine.LoadCheckpointAsync or DecideFromTrainedState.

## What this proves
- The OrchestrationReasoning test-owned deterministic fixture using the LEARN-04 pattern id/schema fresh-loads through OrchestrationReasoningMachine.LoadCheckpointAsync and produces a Plan response through OrchestrationReasoningMachine.Plan.
- The PromptToTask test-owned deterministic adaptive fixture loads through the public PromptToTaskAdaptiveTrainer.LoadCheckpoint adaptive boundary only.
- The SkillSelection test-owned deterministic correction fixture loads through the public SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint adaptive boundary only.
- The source checkpoint files are generated under a unique test temp root, reported only as <test-temp-root>/... paths, and removed before tracked Learn05 artifacts can record temp_fixture_removed=true.

## What this does not prove
- This does not prove semantic quality.
- This does not prove held-out/source-disjoint quality.
- This does not prove ablation/no-memorization controls.
- This does not prove rollback safety.
- This does not promote target checkpoints.
- This does not exercise the real LEARN-04 target checkpoint files; it exercises test-owned deterministic fixtures matching the LEARN-04 pattern id/schema.
- PromptToTask and SkillSelection are not full machine fresh-load proofs because these test-owned deterministic fixtures are adaptive overlay checkpoints without the full machine checkpoint/manifest shape.

## Next independent track

- LEARN-06 held-out/ablation runner

Fixture fresh-load proves only loadability. It requires held-out, ablation, and rollback evidence before readiness.
