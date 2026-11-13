# Row16 prep: frdp-p01-cursor-asset-lifecycle-roundtrip

Вердикт подготовки: `ROW16_PREP_FIX_DONE_REVIEW_REQUIRED`

Это подготовительный пакет для Row16. Он не запускает lifecycle roundtrip, не принимает Row16, не меняет product board/state, не продвигает checkpoint и не обновляет locator.

## Граница доказательства

Row16 должен проверить жизненный цикл уже подготовленного Cursor-compatible adaptive asset: backup/export, переносимую копию для share/package, импорт в свежий временный корень, fresh-load, сравнение хэшей и cleanup/quarantine.

Эта подготовка описывает только безопасный core asset lifecycle proof. Она говорит исполнителю, что именно делать и где остановиться при ошибке. Она не является выполнением Row16.

Если будущий execution пройдет по этому prep, он докажет только следующее:

1. Row14 staged asset можно скопировать в backup/export папку Row16 evidence без изменения исходника.
2. Из backup/export можно сделать переносимую share/package копию и прочитать ее обратно без изменения checkpoint/manifest хэшей.
3. Asset можно импортировать в свежий временный user asset root, который находится только внутри Row16 evidence.
4. Fresh-load probe может загрузить импортированный manifest/checkpoint через тот же core API путь, который Row14.4 использовал для fresh-load.
5. Cleanup/quarantine оставляет все временное состояние внутри Row16 evidence и не трогает production/common/user locator root.

Эта подготовка и будущий core roundtrip не доказывают:

- product readiness;
- token savings;
- full semantic quality;
- broad training readiness;
- checkpoint mutation или checkpoint promotion;
- locator update;
- host UI readiness;
- full joined pass;
- full extension readiness;
- board acceptance для Row16.

## Исходный asset

Использовать только этот Row14 staged export root:

`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-learnability-training-continue\row14-3-staged-continue-training-export-20260626T103934Z\staged-export`

Файлы и ожидаемые хэши:

1. `skill-selection-checkpoint.json`
   SHA256: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`
2. `skill-selection-manifest.json`
   SHA256: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`

Почему выбран именно этот asset:

1. Row14.3 staged export сообщает staged continue-training/export от base 8 pairs к staged 10 pairs.
2. Row14.4 fresh-load evidence сообщает `Fresh import=True`, `Fresh checkpoint load=True`, selected skill `client-adaptive-learning-loop`, expected skill `client-adaptive-learning-loop`.
3. Row14.4 fresh-load использовал core API методы `SkillSelectionMachine.ImportAsync` и `SkillSelectionMachine.LoadCheckpointAsync`.
4. Во время этого fix два source файла были заново проверены через SHA256; полученные хэши совпали с хэшами выше.

Если source root, checkpoint или manifest отсутствуют по точному пути выше, execution обязан остановиться с `ROW16_CORE_LIFECYCLE_EXECUTION_NEEDS_PARENT_DECISION`, назвать отсутствующий путь и не подставлять другой asset root.

## Fresh-load probe: строгий метод

Готового Row16 entrypoint в этом prep не задано. Поэтому исполнитель не имеет права оставлять условную формулировку, выбирать любой похожий скрипт или решать путь на глаз.

Обязательный fallback для Row16:

1. Перед fresh-load execution sub-agent создает маленький test-only .NET harness/evidence script внутри текущего Row16 run folder, например:

   `run-<UTC>\row16-fresh-load-probe\Row16FreshLoadProbe.csproj`
   `run-<UTC>\row16-fresh-load-probe\Program.cs`

2. Harness должен принимать ровно эти аргументы:

   ```powershell
   dotnet run --project "<ROW16_RUN_FOLDER>\row16-fresh-load-probe\Row16FreshLoadProbe.csproj" -- `
     --asset-root "<ROW16_RUN_FOLDER>\fresh-user-asset-root" `
     --evidence-dir "<ROW16_RUN_FOLDER>\fresh-load-probe" `
     --expected-checkpoint-sha256 "c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb" `
     --expected-manifest-sha256 "4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475"
   ```

3. Harness должен использовать тот же core API путь, что Row14.4:

   - прочитать imported `skill-selection-manifest.json`;
   - создать `LearningMachineExport` из `machineId`, `domainTag`, `version`, checkpoint path, manifest path, `accuracyAtExport`, `exportedAtUtc`;
   - создать новый `SkillSelectionMachine`;
   - вызвать `SkillSelectionMachine.ImportAsync`;
   - вызвать `SkillSelectionMachine.LoadCheckpointAsync`;
   - записать readback: `manifestLoaded`, `checkpointLoaded`, `machineId`, `domainTag`, `version`, `trainedPairCount`;
   - повторно посчитать хэши checkpoint и manifest после load;
   - записать русский markdown report и machine JSON report в `fresh-load-probe`.

4. Harness не имеет права выполнять training, eval, `DecideFromTrainedState`, promotion, locator update, запись в production/common/user locator root или host UI действия.

5. Перед использованием harness должен быть отдельно просмотрен. Если work order не дает отдельного review шага, execution sub-agent обязан остановиться после создания harness с `ROW16_FRESH_LOAD_HARNESS_CREATED_REVIEW_REQUIRED`, перечислить созданные файлы и не запускать fresh-load.

6. Если review уже явно разрешил созданный harness, execution sub-agent запускает ровно команду из пункта 2. Любое изменение аргументов или путей допускается только если оно остается внутри текущего Row16 run folder и явно отражено в evidence.

## Нумерованные шаги execution

1. Preflight и scope lock.

   Создать один timestamped run folder только внутри:

   `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-asset-lifecycle-roundtrip`

   Записать source root, checkpoint path, manifest path, ожидаемые хэши и флаги:

   - `production_mutation=false`
   - `locator_update=false`
   - `promotion=false`
   - `training_performed=false`
   - `host_ui_run=false`
   - `roundtrip_executed=true` только в будущем execution report, не в prep.

   Если Row16 уже помечен accepted, остановиться с `ROW16_CORE_LIFECYCLE_EXECUTION_NEEDS_PARENT_DECISION`.

2. Backup/export copy.

   Скопировать `skill-selection-checkpoint.json` и `skill-selection-manifest.json` из source root в:

   `run-<UTC>\backup-export\`

   Сравнить хэши source before, copied files и source after. Source hashes должны остаться неизменными.

3. Share/package copy.

   Создать test-only share/package artifact внутри Row16 run folder. Это может быть папка или архив.

   Если создан архив, его обязательно распаковать в readback folder перед сравнением. Хэш самого архива не заменяет сравнение распакованных checkpoint/manifest.

4. Import into fresh temporary user asset root.

   Создать свежий временный корень только внутри Row16 run folder:

   `run-<UTC>\fresh-user-asset-root\`

   Импортировать туда checkpoint и manifest из share/package readback или backup/export, по выбранному execution rule. Выбор источника не на усмотрение агента: использовать share/package readback, если он создан; если share/package step завершился FAIL, остановиться и не импортировать.

   Fresh root не должен совпадать с production, common, user locator или Row14 source root.

5. Fresh-load probe.

   Использовать strict fallback из раздела "Fresh-load probe: строгий метод". Нельзя оставлять условную формулировку и нельзя выбирать другой entrypoint без review.

   Fresh-load считается выполненным только если reviewed harness запущен с `--asset-root` равным `run-<UTC>\fresh-user-asset-root`.

6. Hash comparison после fresh-load.

   После fresh-load снова посчитать:

   - source checkpoint;
   - source manifest;
   - backup/export checkpoint;
   - backup/export manifest;
   - share/package readback checkpoint;
   - share/package readback manifest;
   - fresh import checkpoint;
   - fresh import manifest.

   Все checkpoint хэши должны быть равны `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`.

   Все manifest хэши должны быть равны `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`.

7. Cleanup/quarantine.

   Source evidence не удалять. Final evidence не удалять.

   Если все проверки PASS, оставить execution reports и нужные evidence artifacts внутри Row16 run folder.

   Если любая проверка FAIL, не пытаться "починить" asset. Пометить run folder как quarantined, записать причину FAIL, сохранить evidence для review и вернуть `ROW16_CORE_LIFECYCLE_EXECUTION_NEEDS_PARENT_DECISION`.

8. Human-readable execution packet.

   Создать русский markdown report и JSON report внутри Row16 run folder. Report обязан назвать:

   - changed files;
   - source asset root;
   - все сравниваемые хэши;
   - fresh-load command или статус `ROW16_FRESH_LOAD_HARNESS_CREATED_REVIEW_REQUIRED`;
   - PASS/FAIL;
   - узкое доказанное утверждение;
   - non-claims.

## PASS правила

Scoped core lifecycle PASS разрешен только если все пункты истинны:

1. Точный Row14 source root существует.
2. Source checkpoint и manifest существуют.
3. Source checkpoint и manifest перед копированием совпадают с expected SHA256 из этого prep.
4. Backup/export файлы совпадают с source hashes.
5. Share/package readback файлы совпадают с source hashes.
6. Fresh temporary import файлы совпадают с source hashes.
7. Reviewed fresh-load harness успешно загрузил imported asset из `fresh-user-asset-root`.
8. Fresh-load readback подтверждает `manifestLoaded=true`, `checkpointLoaded=true`, `machineId=skill-selection-machine`, `domainTag=SmartSkillsDomain.SkillSelection`, `version=1.0.0`, `trainedPairCount=10`.
9. Source hashes после всех операций равны source hashes до операций.
10. Нет production/common/user locator mutation.
11. Нет training, eval, checkpoint mutation, promotion или locator update.
12. Evidence прямо говорит, что это только scoped core asset lifecycle proof.

Даже при PASS нельзя писать, что Row16 accepted. Row16 acceptance требует отдельного review и, если board требует host-visible restored behavior, отдельного UI path.

## FAIL правила

Execution обязан вернуть FAIL или `ROW16_CORE_LIFECYCLE_EXECUTION_NEEDS_PARENT_DECISION`, если выполнен любой пункт:

1. Source root, checkpoint или manifest отсутствует.
2. Любой source hash отличается от expected hash в этом prep.
3. Любой backup/export, share readback или fresh-import hash отличается от source hash.
4. Fresh-load harness не создан, не reviewed или не запущен точной командой после review.
5. Fresh-load не смог загрузить imported manifest/checkpoint.
6. Fresh-load readback не подтверждает machine/domain/version/trainedPairCount.
7. Любой production/common/user locator, promoted checkpoint или shared product state был затронут.
8. Training, eval, promotion или locator update были выполнены.
9. Cleanup не может доказать, что временное состояние осталось внутри Row16 evidence.
10. Executor не может доказать, что путь временный и находится внутри текущего Row16 run folder.

## UI path

Этот prep не разрешает host UI действия. Он готовит только core asset lifecycle proof.

Board definition для Row16 включает host-visible restored behavior. Поэтому полный Row16 acceptance может потребовать отдельный UI path после core lifecycle proof и review. Этот UI path не входит в данный prep и не должен запускаться сейчас.

## Следующий work order, если prep принят review

```text
Ты bounded sub-agent executor в проекте C:\Projects\llm_mvp_core. Active Luba instruction: continue via sub-agents; no parent execution; no stop gates; human-readable Russian status required.

Task: Execute Row16 core asset lifecycle roundtrip only, using the reviewed Row16 prep md/json. Do not mark Row16 accepted.

Allowed read:
- Row16 prep md/json.
- Exact Row14 staged-export source root from prep.
- Row14.4 Program.cs/csproj/report only as API-method reference for fresh-load harness.

Allowed write:
- Only under C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-asset-lifecycle-roundtrip\

Forbidden:
- No production code edits.
- No git.
- No training/eval/checkpoint mutation/promotion/locator update.
- No host UI actions.
- No broad repo audit.
- Do not mark Row16 accepted.

Fresh-load rule:
- There is no pre-approved Row16 entrypoint in the prep.
- Create a small test-only Row16 fresh-load harness under the Row16 run folder.
- The harness must use SkillSelectionMachine.ImportAsync and SkillSelectionMachine.LoadCheckpointAsync.
- Stop for review with ROW16_FRESH_LOAD_HARNESS_CREATED_REVIEW_REQUIRED before using the harness unless a current work order explicitly includes reviewed-harness approval.

Return:
- ROW16_CORE_LIFECYCLE_EXECUTION_DONE_REVIEW_REQUIRED only for completed scoped execution with evidence.
- ROW16_CORE_LIFECYCLE_EXECUTION_NEEDS_PARENT_DECISION for real blocker.
```

## Узкое утверждение этой подготовки

Этот пакет доказывает только то, что Row16 prep исправлен: он русскоязычный, человекочитаемый, задает точный source asset root и хэши, описывает нумерованные execution steps, фиксирует PASS/FAIL правила и заменяет расплывчатый fresh-load шаг на строгий Row16 fresh-load harness fallback через Row14.4 core API path.

Он не доказывает, что Row16 был выполнен, прошел проверку или принят.
