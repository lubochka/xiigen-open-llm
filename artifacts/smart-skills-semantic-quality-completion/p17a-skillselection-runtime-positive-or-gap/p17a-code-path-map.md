# P17A: карта runtime code path

## SkillSelectionMachine

- `43.SmartSkillsDomain/SkillSelectionMachine.cs:103-107`: `DecideFromTrainedState` сначала вызывает `TryDecideWithSemanticUnitsOnly`. Это правильная runtime точка для будущего положительного доказательства.
- `43.SmartSkillsDomain/SkillSelectionMachine.cs:124-136`: candidate-position fallback возвращает пустой выбор и evidence, что финальная authority еще не закрыта. Это не selector quality.
- `43.SmartSkillsDomain/SkillSelectionMachine.cs:146-158`: `ProbeWithoutCandidatePositionAuthority` проверяет semantic path без позиции кандидатов.
- `43.SmartSkillsDomain/SkillSelectionMachine.cs:169-211`: перечислены typed unit branches: context/result/output/completion/intermediate/selection/candidates/routing/task type/task understanding/prompt understanding.
- `43.SmartSkillsDomain/SkillSelectionMachine.cs:443-456`: `LoadCheckpointAsync` читает checkpoint, создает свежую машину и вызывает `LoadCheckpoint`.
- `43.SmartSkillsDomain/SkillSelectionMachine.cs:2564-2575`: native checkpoint validator требует schema version 1, route mode и native trained-state sections.

## API firewall

- `43.SmartSkillsDomain/XiigenExtensionApiService.cs:560-575`: `ShouldClaimSemanticQuality` требует positive evidence от всех компонентов и запрещает `NO_TRAINED_STATE_AVAILABLE` / `CANDIDATE_POSITION_WEIGHT_SCAFFOLD_ONLY`.

## Вывод

Runtime path не отсутствует. Отсутствует доказательство, что P16D2 candidate прошел именно через этот path и дал source-disjoint positive selector output без diagnostics-only shortcuts.
