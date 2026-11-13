# Подготовка Row18: обновление локатора и перезагрузка

Статус: `ROW18_PREP_DONE_REVIEW_REQUIRED`.

Сценарий: Row18 `frdp-p01-cursor-locator-update-reload`.

Источник активного задания: текущий незаквоченный work order Luba просит этот bounded sub-agent подготовить только Row18 и пока не выполнять фактическое обновление локатора или reload.

## Использованный объем доказательств

В этом подготовительном проходе были прочитаны:

- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\product-readiness-scenario-board.md`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\product-readiness-scenario-board.json`
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- статус Row17 и run folder под `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-checkpoint-promotion-reload\`
- staged export checkpoint/manifest из Row14, включая хэши и содержимое manifest.

В рамках этого prep packet не выполнялись правки кода, git-действия, host UI действия, training, eval, checkpoint mutation/promotion и фактическое выполнение locator update/reload.

## Что должна доказать Row18

Если эта подготовка будет принята, выполнение Row18 докажет только жизненный цикл temp-run-folder-only locator update/reload:

- baseline temp locator фиксируется внутри evidence Row18;
- temp locator внутри evidence Row18 обновляется так, чтобы указывать на скопированный staged candidate;
- core lifecycle reload находит candidate только через этот temp locator;
- выбранный skill равен `client-adaptive-learning-loop`;
- rollback восстанавливает baseline temp locator;
- исходные хэши checkpoint/manifest из Row14 остаются неизменными;
- все созданные или обновленные файлы находятся внутри temp run folder Row18.

Это только core lifecycle proof. Это не host UI path и не полный joined pass.

## Что Row18 не доказывает

Row18 не доказывает product readiness, token savings, full semantic quality, training readiness, common production locator update, real user locator update, global/common locator mutation, host UI readiness, full extension readiness или full joined product-readiness.

Также Row18 не доказывает реальный Cursor reload из installed production locator. Для host UI evidence потребуется отдельный same-scenario host work order после этого core lifecycle proof, с auth/session preflight и no-blind-spot UI script.

## Безопасная граница

Выбранная граница: temp-run-folder-only locator update.

Единственная допустимая мутация локатора для будущего execution work order ограничена этим файлом:

`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-locator-update-reload\row18-temp-locator-update-run-20260626T131300Z\temp-locator-area\active\temporary-runtime-locator.json`

Baseline copy для rollback:

`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-locator-update-reload\row18-temp-locator-update-run-20260626T131300Z\temp-locator-area\active\temporary-runtime-locator.baseline.json`

Execution не должен писать в real user/global/common locator, installed Cursor extension config, common asset root, production active manifest или source Row14 asset. Если executor обнаружит, что для Row18 необходимо мутировать реальный locator, он обязан остановиться и вернуть `ROW18_EXECUTION_NEEDS_PARENT_DECISION`.

## Исходные checkpoint и manifest

Основной source candidate:

- Source root: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-learnability-training-continue\row14-3-staged-continue-training-export-20260626T103934Z\staged-export`
- Checkpoint: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-learnability-training-continue\row14-3-staged-continue-training-export-20260626T103934Z\staged-export\skill-selection-checkpoint.json`
- Manifest: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-learnability-training-continue\row14-3-staged-continue-training-export-20260626T103934Z\staged-export\skill-selection-manifest.json`

Хэши, вычисленные напрямую в prep turn:

- Checkpoint SHA256: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`
- Manifest SHA256: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`

Факты manifest, прочитанные в prep turn:

- `schemaVersion`: `1`
- `machineId`: `skill-selection-machine`
- `domainTag`: `SmartSkillsDomain.SkillSelection`
- `version`: `1.0.0`
- `checkpointRef`: `skill-selection-checkpoint.json`
- `manifestRef`: `skill-selection-manifest.json`
- `checkpointSchemaVersion`: `1`
- `routeMode`: `candidate_position_scaffold_not_final_semantic_authority`
- `semanticQualityClaimed`: `false`
- `exportedAtUtc`: `2026-06-26T10:45:12.7186568+00:00`

Поддерживающее evidence Row17:

- Execution packet Row17: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-checkpoint-promotion-reload\row17-temp-promotion-run-20260626T124725Z\row17-promotion-reload-execution-packet.json`
- Source/candidate hashes Row17 совпали с теми же Row14 staged-export hashes до reload, после reload и после rollback.
- Row17 выбрала `client-adaptive-learning-loop` через temp locator и откатила temp locator к baseline.

## Точные старые и новые значения temp locator

Будущее выполнение должно создать `temporary-runtime-locator.baseline.json` и `temporary-runtime-locator.json` внутри run folder Row18. Старое baseline value:

```json
{
  "schema": "xiigen.row18.temp_runtime_locator.v1",
  "status": "baseline_no_active_candidate",
  "row": 18,
  "scenarioId": "frdp-p01-cursor-locator-update-reload",
  "activeCandidate": null,
  "candidateRoot": null,
  "checkpointRef": null,
  "manifestRef": null,
  "expectedCheckpointSha256": null,
  "expectedManifestSha256": null,
  "mutationBoundary": "temp-run-folder-only",
  "realUserGlobalCommonLocatorMutation": false,
  "runFolder": "C:\\Projects\\llm_mvp_core\\docs\\plans\\evidence\\extension-full-readiness-dual-path\\scenario-execution\\frdp-p01-cursor-locator-update-reload\\row18-temp-locator-update-run-20260626T131300Z"
}
```

Новое updated value:

```json
{
  "schema": "xiigen.row18.temp_runtime_locator.v1",
  "status": "active_temp_candidate",
  "row": 18,
  "scenarioId": "frdp-p01-cursor-locator-update-reload",
  "activeCandidate": "row14-staged-export-copy",
  "candidateRoot": "C:\\Projects\\llm_mvp_core\\docs\\plans\\evidence\\extension-full-readiness-dual-path\\scenario-execution\\frdp-p01-cursor-locator-update-reload\\row18-temp-locator-update-run-20260626T131300Z\\temp-locator-area\\candidate",
  "checkpointRef": "C:\\Projects\\llm_mvp_core\\docs\\plans\\evidence\\extension-full-readiness-dual-path\\scenario-execution\\frdp-p01-cursor-locator-update-reload\\row18-temp-locator-update-run-20260626T131300Z\\temp-locator-area\\candidate\\skill-selection-checkpoint.json",
  "manifestRef": "C:\\Projects\\llm_mvp_core\\docs\\plans\\evidence\\extension-full-readiness-dual-path\\scenario-execution\\frdp-p01-cursor-locator-update-reload\\row18-temp-locator-update-run-20260626T131300Z\\temp-locator-area\\candidate\\skill-selection-manifest.json",
  "expectedCheckpointSha256": "c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb",
  "expectedManifestSha256": "4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475",
  "expectedSkill": "client-adaptive-learning-loop",
  "mutationBoundary": "temp-run-folder-only",
  "realUserGlobalCommonLocatorMutation": false,
  "sourceRoot": "C:\\Projects\\llm_mvp_core\\docs\\plans\\evidence\\extension-full-readiness-dual-path\\scenario-execution\\frdp-p01-cursor-learnability-training-continue\\row14-3-staged-continue-training-export-20260626T103934Z\\staged-export",
  "runFolder": "C:\\Projects\\llm_mvp_core\\docs\\plans\\evidence\\extension-full-readiness-dual-path\\scenario-execution\\frdp-p01-cursor-locator-update-reload\\row18-temp-locator-update-run-20260626T131300Z"
}
```

Rollback rule: после reload proof нужно перезаписать `temporary-runtime-locator.json` точными baseline bytes из `temporary-runtime-locator.baseline.json`, затем вычислить SHA256 для baseline, after-update и after-rollback locator files. PASS требует, чтобы after-rollback hash равнялся baseline hash, а after-update hash отличался от baseline hash.

## Шаги выполнения и PASS/FAIL

1. Фиксация baseline locator.
   PASS: создать только run folder Row18, создать baseline temp locator value выше, byte-for-byte скопировать его в `temporary-runtime-locator.json` и записать baseline SHA256. FAIL: любая запись вне Row18 run folder, отсутствующий baseline или baseline с real active candidate.

2. Копирование candidate и фиксация хэшей.
   PASS: скопировать staged `skill-selection-checkpoint.json` и `skill-selection-manifest.json` из Row14 в `temp-locator-area\candidate\`; copied checkpoint hash равен `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`; copied manifest hash равен `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`. FAIL: отсутствует copy, hash mismatch, source file mutated или candidate path вне Row18 run folder.

3. Обновление temp locator.
   PASS: обновить только `temporary-runtime-locator.json` до нового active temp candidate value выше и записать after-update SHA256. FAIL: запись в real user/global/common locator, installed extension config, source root или любой файл вне Row18 run folder.

4. Reload через core path.
   PASS: core lifecycle path читает candidate paths, resolved from `temporary-runtime-locator.json`, использует SkillSelection core load/import path и выбирает `client-adaptive-learning-loop`. FAIL: load error, selected skill mismatch, fallback к source Row14 paths вместо copied candidate paths, fallback к production/global assets или любое host UI action.

5. Rollback.
   PASS: rollback восстанавливает temp locator до точных baseline bytes и записывает after-rollback SHA256, равный baseline SHA256. FAIL: after-rollback hash отличается, temp locator все еще указывает на candidate или rollback трогает любой real locator.

6. Проверка, что source и real-state не тронуты.
   PASS: source Row14 checkpoint и manifest hashes после execution все еще равны prep hashes; execution packet перечисляет каждый created/updated path, и каждый такой path находится под Row18 run folder; `realUserGlobalCommonLocatorMutation=false`; ни один host/user/global/common locator path не записан и не заявлен. FAIL: source hash drift, любой changed path вне Row18 run folder, любая production/common/user/global locator mutation или claim шире temp evidence.

7. Создание packet.
   PASS: execution packet записывает source paths, temp paths, old/new locator hashes, copied candidate hashes before/after reload/rollback, selected skill, rollback evidence, changed files и non-claims. FAIL: missing packet, missing hash comparison или Row18 marked accepted by execution.

## Файлы и хэши для сравнения

Execution должен сравнить:

- Source Row14 checkpoint before vs copied temp checkpoint before reload vs copied temp checkpoint after reload vs copied temp checkpoint after rollback.
- Source Row14 manifest before vs copied temp manifest before reload vs copied temp manifest after reload vs copied temp manifest after rollback.
- Source Row14 checkpoint before vs source Row14 checkpoint after rollback.
- Source Row14 manifest before vs source Row14 manifest after rollback.
- `temporary-runtime-locator.baseline.json` hash vs `temporary-runtime-locator.json` after update vs `temporary-runtime-locator.json` after rollback.
- Все execution packet `filesCreated` и `filesUpdated` paths against the Row18 run-folder prefix.

Expected checkpoint hash: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`.

Expected manifest hash: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`.

Locator file hashes являются execution-time hashes, потому что locator JSON включает фиксированный Row18 run folder и должен быть записан execution packet. Обязательный invariant: baseline locator hash equals after-rollback locator hash; after-update locator hash differs from baseline.

## Решение по Host UI path

Host UI path не нужен для следующего execution slice. Row18 execution должен быть только core lifecycle proof. Это сохраняет evidence узким и избегает небезопасной production/global locator mutation.

Host UI остается недоказанным после этой подготовки и после предложенного core execution. Для изменения local real-program status потребуется более поздний same-scenario host UI work order, и эта последующая работа должна включать Host Session Auth Preflight and Browser Login Recovery, No-Blind-Spot UI Scenario Script, exact clicks/typing/expected UI state, no secrets captured и clear comparison to this core lifecycle result.

## Non-claims, зафиксированные как false

- Product readiness: false.
- Token savings: false.
- Full semantic quality: false.
- Training readiness: false.
- Training/eval/checkpoint mutation/promotion: false.
- Common production locator update: false.
- Real user/global/common locator mutation: false.
- Host UI readiness: false.
- Full joined pass: false.
- Full extension readiness: false.
- Row18 accepted: false.

## Следующий execution work order, если prep будет принят

Подготовить и выполнить Row18 `frdp-p01-cursor-locator-update-reload` как core-only temp-run-folder-only locator update/reload proof. Разрешенная область записи: создавать файлы только под `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-locator-update-reload\row18-temp-locator-update-run-20260626T131300Z\`; писать execution packet files под Row18 scenario folder только если parent work order явно это разрешает. Разрешенные source reads: Row18 prep files; Row14 staged export checkpoint/manifest, названные в prep; Row17 execution packet для prior temp locator evidence; board/state files, если нужны для non-claim alignment. Запрещено: code edits, git, actual host UI actions, training/eval/checkpoint mutation/promotion, real user/global/common locator mutation, installed extension config mutation, source Row14 mutation, board/status acceptance edits и любой claim, что Row18 accepted. Обязательное execution: создать baseline temp locator, скопировать candidate в temp locator area, обновить только `temporary-runtime-locator.json`, выполнить reload through core path from that temp locator, проверить selected skill `client-adaptive-learning-loop`, откатить temp locator to baseline, сравнить все required hashes, проверить source hashes unchanged и что every changed path stays under Row18 run folder, затем записать execution packet и non-claims. Вернуть `ROW18_EXECUTION_DONE_REVIEW_REQUIRED` или `ROW18_EXECUTION_NEEDS_PARENT_DECISION`.

Узкий claim, поддержанный этим prep: Row18 имеет конкретный temp-only execution plan с source candidate, hashes, temp locator old/new values, rollback rule, PASS/FAIL gates, host-UI non-claim и next work order. Этот prep не выполняет и не принимает Row18.
