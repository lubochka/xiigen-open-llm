# LEARN-06 held-out/ablation report

Verdict: PASS_WITH_LIMITATIONS

## Commands
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn06HeldOutAblationRunnerTests --no-restore --logger "console;verbosity=normal"`
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn06HeldOutAblationRunnerTests --no-restore --logger "console;verbosity=normal"`
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn06HeldOutAblationRunnerTests --no-restore --logger "console;verbosity=normal"`

## Command results
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn06HeldOutAblationRunnerTests --no-restore --logger "console;verbosity=normal"` -> FAIL_COMPILE; Initial LEARN-06 test compile failed because OperationResult<> needed the AgentInfrastructure namespace.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn06HeldOutAblationRunnerTests --no-restore --logger "console;verbosity=normal"` -> FAIL_COMPILE; Second LEARN-06 test compile failed because IReadOnlyDictionary<string,string> constructor arguments used collection expressions; patched to explicit dictionaries.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~Learn06HeldOutAblationRunnerTests --no-restore --logger "console;verbosity=normal"` -> PASS; Test Run Successful. Total tests: 1. Passed: 1. Failed: 0.

## Track verdicts
- PromptToTaskMachine: PARTIAL_NOT_FULL_MACHINE_EVAL
  Boundary: `PromptToTaskAdaptiveTrainer.LoadCheckpoint + overlay metadata/source-disjoint controls`
  Target: `<test-temp-root>/source-assets/prompt-to-task/.xiigen/prompt-to-task/adaptive-checkpoint.json`
  Held-out dataset: `learn-06-prompt-to-task-overlay-held-out-source-disjoint-v1` / `c34a6d8d2743329e8e73daf0db87e7c16d34edbb810cd5953c4eed05a56750b2`
  Source-disjoint: True; held-out passed: True; not full machine eval: True
  Runtime invocation succeeded: False; test-only in-process probe succeeded: False
  Limitation: PARTIAL/NOT_FULL_MACHINE_EVAL: staged target is an adaptive overlay checkpoint without a PromptToTaskMachine checkpoint plus manifest. Checks are overlay-level source-disjoint metadata and loader checks only.
  - exact_prompt_replay: PARTIAL_STATIC_REF_ONLY; safeRefs=True; refCount=2
  - path_title_source_id_leakage: PARTIAL_STATIC_REF_ONLY; safeRefs=True
  - nearest_neighbor_memorization: PARTIAL_NOT_APPLICABLE_NO_RUNTIME_NEAREST; not run; overlay checkpoint has no public semantic runtime
  - no_trained_state_baseline: PARTIAL_NOT_APPLICABLE_NO_FULL_MACHINE; not run; marked limitation
  - shuffled_chosen_rejected: PARTIAL_STATIC_REF_ONLY; distinctRefs=2
- OrchestrationReasoningMachine: PASS_NAMED_CONTROLS_NOT_LEAKAGE
  Boundary: `OrchestrationReasoningMachine.LoadCheckpointAsync + OrchestrationReasoningMachine.Plan`
  Target: `<test-temp-root>/source-assets/orchestration-reasoning/checkpoint.json`
  Held-out dataset: `learn-06-orchestration-held-out-source-disjoint-v1` / `780ab93961b0e05c98f467261b6dfca17e568593010103bd965ed8eeb88eef0b`
  Source-disjoint: True; held-out passed: True; not full machine eval: False
  Runtime invocation succeeded: False; test-only in-process probe succeeded: True
  Limitation: Scope: test-owned deterministic fixture using LEARN-04 pattern id/schema; real LEARN-04 target not exercised. The public loader plus in-process Machine.Plan prove only the named held-out/ablation checks; no external/host runtime was launched and no runtime success is claimed.
  - exact_prompt_replay: OBSERVED_MATCH_REFERENCE_ONLY; loaded_guide_state_semantic_plan_produced; selected=or-learned-pattern:learn-04-local-runtime-token-quality-boundary
  - path_title_source_id_leakage: PASS_BLOCKED; loaded_guide_state_no_matching_learned_pattern; selected=
  - nearest_neighbor_memorization: PASS_BLOCKED; loaded_guide_state_no_matching_learned_pattern; selected=
  - no_trained_state_baseline: PASS_BLOCKED; not_trained_no_learned_guide_state; selected=
  - shuffled_chosen_rejected: PASS_BLOCKED; loaded_guide_state_no_matching_learned_pattern; selected=
- SkillSelectionMachine: PARTIAL_NOT_FULL_MACHINE_EVAL
  Boundary: `SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint + overlay metadata/source-disjoint controls`
  Target: `<test-temp-root>/source-assets/skill-selection/.xiigen/smart-skills/corrections/adaptive-checkpoint.json`
  Held-out dataset: `learn-06-skill-selection-overlay-held-out-source-disjoint-v1` / `392dc0a719ad0e5a794a5ec46a61272e8d3921d5c7d217ff7c24788ad03be1fb`
  Source-disjoint: True; held-out passed: True; not full machine eval: True
  Runtime invocation succeeded: False; test-only in-process probe succeeded: False
  Limitation: PARTIAL/NOT_FULL_MACHINE_EVAL: staged target is a correction adaptive overlay checkpoint without a SkillSelectionMachine checkpoint plus manifest. Checks are overlay-level source-disjoint metadata and loader checks only.
  - exact_prompt_replay: PARTIAL_STATIC_REF_ONLY; safeRefs=True; refCount=2
  - path_title_source_id_leakage: PARTIAL_STATIC_REF_ONLY; safeRefs=True
  - nearest_neighbor_memorization: PARTIAL_NOT_APPLICABLE_NO_RUNTIME_NEAREST; not run; overlay checkpoint has no public semantic runtime
  - no_trained_state_baseline: PARTIAL_NOT_APPLICABLE_NO_FULL_MACHINE; not run; marked limitation
  - shuffled_chosen_rejected: PARTIAL_STATIC_REF_ONLY; distinctRefs=2

## What this proves
- This proves the named held-out/source-disjoint and ablation controls for a test-owned deterministic fixture using LEARN-04 pattern id/schema; real LEARN-04 target not exercised, through OrchestrationReasoningMachine.LoadCheckpointAsync plus in-process Machine.Plan.
- This proves only overlay-level checkpoint loading and source-disjoint metadata checks for PromptToTask and SkillSelection, because their staged targets are adaptive overlay checkpoints.
- This proves common model assets were not mutated and no external AI, promotion, rollback, runtime host probe, or semantic-quality claim was used.

## What this does not prove
- This does not exercise the real LEARN-04 target: the orchestration evidence is a test-owned deterministic fixture using LEARN-04 pattern id/schema; real LEARN-04 target not exercised.
- This does not prove universal semantic quality or future unseen correctness.
- This does not prove full PromptToTaskMachine or SkillSelectionMachine held-out/runtime readiness.
- This does not prove host runtime readiness, token savings, common training license safety, target promotion, or rollback safety.
- This does not activate or promote any checkpoint.

Runtime allowed: False; runtime launched: False

## Next independent track
- LEARN-07 rollback verifier

This proves the named held-out/ablation controls for the target. It is not runtime or license evidence.
