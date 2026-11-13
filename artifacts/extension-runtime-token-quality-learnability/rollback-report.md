# LEARN-07 rollback report

Verdict: FAIL

## Commands
- `in-process Learn07RollbackRunnerTests rollback fixture materialization and public load probes`

## Command results
- `in-process Learn07RollbackRunnerTests rollback fixture materialization and public load probes` -> IN_PROCESS_PROBES_COMPLETED; This report records in-process Learn07 probes, which do not have a process exit code; external dotnet/TRX status is recorded in the caller's test artifacts.

## Track verdicts
- PromptToTaskMachine: PARTIAL_OVERLAY_ROLLBACK_ONLY
  Boundary: `PromptToTaskAdaptiveTrainer.LoadCheckpoint rollback overlay`
  Target: `<test-temp-root>/prompt-to-task/target/.xiigen/prompt-to-task/adaptive-checkpoint.json`
  Rollback: `<test-temp-root>/prompt-to-task/rollback/.xiigen/prompt-to-task/adaptive-checkpoint.json`
  Target-only change: `learn-07-prompt-to-task-target-signal`
  Rollback loaded: True; target-only changes absent: True
  Runtime invocation: False; test-only in-process probe: False
  Observed: rollback_overlay_loaded_without_target_signal_ids; rollbackTaskProfiles=0; rollbackTrainedSignalIds=0; targetTrainedSignalIds=1
  Limitation: PARTIAL_OVERLAY_ROLLBACK_ONLY: PromptToTask target and rollback are adaptive overlay checkpoints without a full PromptToTaskMachine checkpoint plus manifest.
- OrchestrationReasoningMachine: FAIL
  Boundary: `OrchestrationReasoningMachine.LoadCheckpointAsync`
  Target: `<test-temp-root>/orchestration-reasoning/target/checkpoint.json`
  Rollback: `<test-temp-root>/orchestration-reasoning/rollback/checkpoint.json`
  Target-only change: `or-learned-pattern:learn-04-local-runtime-token-quality-boundary`
  Rollback loaded: False; target-only changes absent: False
  Runtime invocation: False; test-only in-process probe: False
  Observed: rollback_public_machine_load_failed; ORCHESTRATION_REASONING_SOURCE_ADMISSION_REJECTED: Could not find a part of the path 'C:\Users\lubao\AppData\Local\Temp\xiigen-learn-07-manifest-bound\d17a2df5e5e840138b04dabc5b51c096\source\payload.jsonl'.
  Limitation: Public machine rollback load failed.
- SkillSelectionMachine: PARTIAL_OVERLAY_ROLLBACK_ONLY
  Boundary: `SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint rollback overlay`
  Target: `<test-temp-root>/skill-selection/target/.xiigen/smart-skills/corrections/adaptive-checkpoint.json`
  Rollback: `<test-temp-root>/skill-selection/rollback/.xiigen/smart-skills/corrections/adaptive-checkpoint.json`
  Target-only change: `skill-correction-adaptive-learn-07-skill-selection-source-learn-07-target-correction`
  Rollback loaded: True; target-only changes absent: True
  Runtime invocation: False; test-only in-process probe: False
  Observed: rollback_overlay_loaded_without_target_signal_ids; rollbackCorrectionProfiles=0; rollbackTrainedSignalIds=0; targetTrainedSignalIds=1
  Limitation: PARTIAL_OVERLAY_ROLLBACK_ONLY: SkillSelection target and rollback are correction adaptive overlay checkpoints without a full SkillSelectionMachine checkpoint plus manifest.

## Safety evidence
- Target promotion performed: False
- Locator updated: False
- Outer fixture root deleted before artifact write: True

## What this proves
- The strict LEARN-07 acceptance predicate failed; no rollback acceptance claim is made.

## What this does not prove
- This does not prove target quality, target promotion, host runtime readiness, or universal learnability.
- PromptToTask and SkillSelection remain PARTIAL_OVERLAY_ROLLBACK_ONLY because their rollback assets are adaptive overlays, not full machine checkpoint plus manifest artifacts.
- The Orchestration rollback manifest checkpointRef differs from the approval packet alias; the verifier used a temp manifest-bound copy for public LoadCheckpointAsync and did not rewrite the named rollback assets.
- RuntimeInvocationSucceeded is false for every track; runtime_allowed and runtime_launched stay false in the state patch.

## Next independent track
- final execution review preparation

Rollback proves recovery only. It does not prove the target should be promoted.
