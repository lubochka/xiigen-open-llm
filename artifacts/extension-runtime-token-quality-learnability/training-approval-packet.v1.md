# Training Approval Packet v1

Phase: `LEARN-03`.

Verdict: **packet ready for local-track parent review; no training started**.

## What This Packet Does

This packet turns the broad execution permission into exact per-machine fields for the next local-training track. It names the machine, source rows, user-local store, load order, backup path, staged target path, rollback path, held-out/ablation requirements, and promotion blockers.

It does not train a model. It does not mutate checkpoints. It does not update common model assets. It does not claim learnability or semantic quality.

## Shared Inputs

| Input | Path | SHA-256 |
|---|---|---|
| Local DPO candidates | `artifacts/extension-runtime-token-quality-learnability/local-dpo-candidates-v1.jsonl` | `704EDF7A718B60C80585D73ED9E291991736BD53615F17D10EB78FD3129974B1` |
| Scenario pack | `artifacts/extension-runtime-token-quality-learnability/scenario-pack-token-quality-v1.jsonl` | `5BECBBE93D4B32C4CB30A598D35E0A348BDDB86EE91C9ADEDA84C51CEEC3C1A6` |
| User-local store manifest | `artifacts/extension-runtime-token-quality-learnability/user-local-adaptive-store-manifest.v1.json` | `D2F75B89BFF89B6734892E4961E62213A98B7BC566FB67E900FE6D7756188A42` |
| Source/license review | `artifacts/extension-runtime-token-quality-learnability/source-license-redaction-boundary-review.v1.json` | `BAC9CE1C31449BDE360C2E027AE1E17E9C00214B01F359E2AAB41DA581DBCC03` |

## Track 1: PromptToTaskMachine

Rows: `LDPO-003`, `LDPO-011`, `LDPO-012`.

The next phase may only continue-training from the existing user-local checkpoint or a verified copied common base. It must back up `%USERPROFILE%\.xiigen\adaptive-models\prompt-to-task\checkpoints\current` to `%USERPROFILE%\.xiigen\adaptive-models\prompt-to-task\backups\pre-learn-04` before any mutation.

Target path: `%USERPROFILE%\.xiigen\adaptive-models\prompt-to-task\checkpoints\targets\learn-04-prompt-to-task-local-v1`.

Rollback path: `%USERPROFILE%\.xiigen\adaptive-models\prompt-to-task\backups\pre-learn-04`.

Held-out/ablation must prove the model did not merely memorize exact prompts, source ids, paths, or titles.

## Track 2: OrchestrationReasoningMachine

Rows: `LDPO-002`, `LDPO-005`, `LDPO-006`, `LDPO-010`.

The next phase may only continue-training from the existing user-local checkpoint or a verified copied common base. Common training is blocked. `LDPO-006` and `LDPO-010` remain common-quarantined because they need universal-rule extraction, source/license proof, and target/rollback binding before common use.

Target path: `%USERPROFILE%\.xiigen\adaptive-models\orchestration-reasoning\checkpoints\targets\learn-04-orchestration-reasoning-local-v1`.

Rollback path: `%USERPROFILE%\.xiigen\adaptive-models\orchestration-reasoning\backups\pre-learn-04`.

Held-out/ablation must prove the model learned universal orchestration boundaries, not project-stack-specific implementation terms.

## Track 3: SkillSelectionMachine

Rows: `LDPO-001`, `LDPO-007`, `LDPO-009`.

The next phase may only continue-training from the existing user-local checkpoint or a verified copied common base. It must not use candidate order, source id, exact filename, path, prompt hash, or empty `AvailableSkills` as final authority.

Target path: `%USERPROFILE%\.xiigen\adaptive-models\skill-selection\checkpoints\targets\learn-04-skill-selection-local-v1`.

Rollback path: `%USERPROFILE%\.xiigen\adaptive-models\skill-selection\backups\pre-learn-04`.

Held-out/ablation must include candidate-order permutations, no-trained-state control, shuffled chosen/rejected control, and partial/empty available-skills controls.

## Deferred Track: ArchitectBehavior

`ArchitectBehavior` rows stay user-local and deferred. They are not part of the next extension-readiness training run unless a separate behavior-model approval packet is created.

## Next Phase Guard

`LEARN-04` may only do local continue-training for `PromptToTaskMachine`, `OrchestrationReasoningMachine`, and `SkillSelectionMachine`. It must not update common checkpoints, use external AI, claim semantic quality, promote target checkpoints, skip backup, or retrain from zero without a separate cold-start approval.
