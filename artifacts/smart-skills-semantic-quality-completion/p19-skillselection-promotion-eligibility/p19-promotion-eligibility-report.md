# P19: SkillSelection promotion eligibility gate

## Короткий ответ

P17E SkillSelection production-export candidate **нельзя** безопасно продвигать напрямую в `models/skill-selection/skill-selection-v1`. Он остаётся artifact-only candidate.

## Почему

1. Candidate manifest сам говорит `accuracyAtExport=0` и `semanticQualityClaimed=false`.
2. P17E locator сам говорит `locator_scope=artifact_only`, `runtime_wired=false`, `common_model_path_updated=false`, `final_model_path_updated=false`.
3. Existing common checkpoint больше и содержит больше trained/profile state: 900 pair ids против 19 у candidate.
4. Direct copy перетрёт runtime/common asset без explicit promotion gate, rollback и non-regression evidence.
5. P19 ничего не меняет в runtime: это eligibility/comparison gate, не promotion.

## Что должно случиться до promote

- Отдельная phase, которой явно разрешено менять common/runtime model path.
- Existing-vs-candidate compatibility/non-regression gate.
- Rollback/load proof.
- Fresh-load через `XiigenExtensionApiService` после обновления path.
- API/firewall rerun без overclaim.
- Manifest/locator rewrite с truthful evidence fields.

## Кодовые точки

- `43.SmartSkillsDomain/XiigenExtensionApiService.cs:637-639`: runtime loader ожидает `models/skill-selection/skill-selection-v1/manifest.json` и `skill-selection-v1-checkpoint.json`.
- `43.SmartSkillsDomain/XiigenExtensionApiService.cs:717-739`: loader читает common checkpoint и вызывает `SkillSelectionMachine.LoadCheckpoint`.
- `43.SmartSkillsDomain/SkillSelectionMachine.cs:55`: route mode остаётся `candidate_position_scaffold_not_final_semantic_authority`.
- `43.SmartSkillsDomain/SkillSelectionMachine.cs:334-383`: export/import lifecycle существует, но lifecycle не равен runtime promotion.

## Focused test

- Command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SmartSkillsSemanticQualityP19PromotionEligibilityTests" --logger "console;verbosity=minimal" --no-restore /p:UseSharedCompilation=false`
- Result: passed, Failed=0, Passed=1, Skipped=0, Total=1.
- Note: первый focused run завис на VBCSCompiler shared compiler и был перезапущен с shared compilation disabled.
- UTF-8 note: focused test читает файлы через `File.ReadAllText(..., Encoding.UTF8)`; parent patch сохранён.

## Parent readability repair

Parent review нашёл mojibake в P19 human-facing artifacts/state. Этот repair перезаписал P19 Markdown/JSON/JSONL в читаемом UTF-8 русском тексте, LF, final newline.

## Вердикт

`PASS_PROMOTION_ELIGIBILITY_ACCEPTED_PENDING_COMMIT`, но eligibility decision = `DIRECT_PROMOTION_BLOCKED_ARTIFACT_ONLY`.
