# P17A: runtime-positive SkillSelection или точный gap

## Вердикт

PASS_EXACT_RUNTIME_GAP_READY_FOR_PARENT_REVIEW.

P17A не нашла основание для положительного runtime claim. В коде уже есть настоящий путь загрузки и выбора: `SkillSelectionMachine.LoadCheckpointAsync`, `ImportAsync`, `DecideFromTrainedState`, `TryDecideWithSemanticUnitsOnly`. Но принятые P16D2/P16F artifacts пока доказывают artifact-level schema/readback, а не то, что P16D2 candidate загружается через native runtime path и выбирает skills на source-disjoint примерах.

## Что уже есть

- Runtime selector path существует: semantic units пробуются до candidate-position fallback.
- Candidate-position guard в P16D2 остается diagnostics-only: `can_select_skill=false`, `authority=false`. Это правильная защита, но не положительное качество выбора.
- P16E2 показал schema-level improvement: 10 positive pass, 2 diagnostics-only gaps, 0 shortcut leaks. Но там `runtime_evaluation=false`.
- P16F сохранил sha/export-import identity, но boundary говорит: это чтение artifact bytes, не production runtime SkillSelection.

## Точный gap

Native loader валидирует `SkillSelectionCheckpoint` schema version 1 и полный набор native sections. P16D2 candidate имеет artifact schema `schema_version=2` и diagnostics-only guard. Без bounded repair, который создаст native-loadable checkpoint или approved binder/converter, нельзя считать P16D2 runtime-positive evidence.

## Что должно быть следующим

Следующий безопасный gate: `P17B_SKILLSELECTION_NATIVE_CHECKPOINT_BINDING_OR_REBUILD`. Он должен создать или связать native-loadable SkillSelection checkpoint из P16D2 material, свежо загрузить его через `LoadCheckpointAsync`/`ImportAsync`, затем прогнать `DecideFromTrainedState` на eval/holdout rows и ablation controls. Candidate-position diagnostics-only rows не должны попадать в positive selector counts.

## Что не заявляется

Не заявляется semantic quality, SkillSelection readiness, full API readiness, trained model readiness, host/dashboard readiness или final model readiness. Commit/push не выполнялись.
