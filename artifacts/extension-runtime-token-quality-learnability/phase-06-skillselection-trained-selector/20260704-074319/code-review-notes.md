# Code review notes, 4 cycles

1. Generic `ILearningMachine` training/evaluation was scaffold-only. Fixed by routing generic rows into `SkillSelectionMachinePair` and scoring held-out requests through `DecideFromTrainedState`.
2. `GetOrchestratorAsync` returned legacy contract-only orchestration. Fixed by returning `SkillSelectionLogicalOrchestrator` over `SkillSelectionLogicalFlow` for the current machine.
3. Shortcut audit needed ablation evidence. Fixed by writing empty-machine, candidate-order reversal, and source-id-only controls.
4. Lifecycle proof needed fresh load without retrain. Fixed by checkpoint/manifest save, import into a fresh machine, direct checkpoint load, and held-out rerun.
