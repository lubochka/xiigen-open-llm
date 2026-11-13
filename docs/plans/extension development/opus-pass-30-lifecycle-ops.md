# Pass P30 — Реальная операция жизненного цикла (backup / export / import / share / continue / rollback)

Дата: 2026-07-08. Оценщик: Opus 4.8 (REAL-STATE ASSESSOR). Срез P30/50.
Фокус задачи: РЕАЛЬНЫЙ пакет backup/export/import/share/continue/rollback (Board Phase 15 = NOT_STARTED
«No real backup/export/import/share/continue/rollback packet»). Определить, что нужно для настоящего
доказательства; добавить ТОЛЬКО новые дыры (dedupe против P05/P09/P11/P19/P24/P25/P26).

## 1. Что реально проверено (bounded reads, живое ядро `C:\Projects\llm_mvp_core`)

- **Код сервиса жизненного цикла** `43.SmartSkillsDomain/Phase08CommonUserTwoLegLifecycle.cs` целиком по срезам:
  записи `Phase08*` (строки 11-250), сервис `Phase08CommonUserTwoLegLifecycleService`
  (`BuildTwoLegLifecycleRows` :267, `BuildCoverageMatrix` :338, `WriteUserLocalStoreManifest` :424,
  `SeedPersonalCheckpoint` :463, `ExportUserModel` :492, `RestoreUserModel` :562,
  `CreateApprovedShareCopy` :623, `ContinueTrainingFromCheckpoint` :668, `ApplyCommonUpgrade` :728,
  `RollbackCommonUpgrade` :789, `WriteEvidenceLeaf` :821, `BuildRow` :838).
- **Кто вызывает сервис**: `git grep` по всему репо → ЕДИНСТВЕННЫЙ вызыватель
  `Tests/Phase08CommonUserTwoLegLifecycleTests.cs` (:12/:70/:85/:96/:127/:146/:192/:220). Ни один
  продовый/host/adapter/dashboard/orchestrator файл не вызывает эти методы.
- **Экспонирование операции на границе расширения**: grep в `XiigenExtensionApiService.cs`,
  `XiigenExtensionApiContracts.cs`, `43.SmartSkillsDomain.Dashboard/*.cs`, `15.OrchestratorModel` →
  НИ ОДНОГО lifecycle-типа запроса (backup/export/import/share/rollback/continue) в контракте хоста;
  «lifecycle»-хиты в 15 = `DevelopmentLifecycleTopologySeeder` (несвязанный dev-topology seed).
- **Артефакт-пакет на диске**:
  `artifacts/extension-runtime-token-quality-learnability/phase-08-common-user-two-leg-lifecycle/`
  (`phase-08-lifecycle-evidence.json`, `-checks.jsonl`, `-report.ru.md`, `backup-export-package/*`,
  `approved-anonymized-share/*`, `phase-08-continue-training-report.json`,
  `phase-08-common-upgrade-rollback-record.json`, каталог `test-work/user-profile-a|b/.xiigen/...`).
- **Реальная машина**: `SkillSelectionMachine.ExportAsync` :397 / `ImportAsync` :412 / `SaveCheckpointAsync`
  :450 — умеет сериализовать/читать НАСТОЯЩИЙ checkpoint+manifest.
- **Второй lifecycle-family** для той же машины:
  `artifacts/smart-skills-semantic-quality-completion/p16f-skill-selection-lifecycle-evidence/*`
  (`p16f-export-import-evidence.json`, `p16f-continue-training-evidence.json`,
  `p16f-fresh-load-evidence.json`).

## 2. Центральный вывод

Реальной **операции** жизненного цикла у трёх расширений нет. То, что на диске выглядит как
«executable lifecycle probe», — это **юнит-тестовый прогон доменного сервиса над синтетическими
данными**, который:
1. round-trip'ит **555-байтный metadata-огрызок** (список signal-id), а не обученную модель;
2. запускается **только из xUnit**, ни один host/extension не может его вызвать (в контракте нет
   операции);
3. называет «continue-training» **инкремент счётчика строк** (2→3), а «share» — **манифест без
   реального переноса/анонимизации файла**;
4. закрыт лишь для **1 из 4** сфер, а собственная матрица честно ставит `passed:false`.

Важно (честная граница): пакет `phase-08-lifecycle-evidence.json` сам НЕ заявляет продуктовой
готовности — `explicitNonClaims` перечисляет «не заявлена full extension readiness … не запускался host
live proof». То есть это добросовестный boundary/scaffold-артефакт. Дыры P30 — про то, что до
**реального** lifecycle-пакета (Board Phase 15) остаётся дистанция, и про **overclaim-и в именах полей**
(`SourceHashesVerified`, `covered_by_executable_probe`, `continue-training`), которые могут ввести в
заблуждение при беглом чтении.

## 3. Новые дыры P30 (dedupe отмечен)

| # | Компонент | Реальный статус | Доказательство file:line | Дыра | Шаги закрытия |
|---|---|---|---|---|---|
| G30-1 | Операция lifecycle на границе хоста/расширения | MISSING OPERATION SURFACE | вызыватели `Phase08CommonUserTwoLegLifecycleService` = только `Tests/Phase08CommonUserTwoLegLifecycleTests.cs`; grep lifecycle-op в `XiigenExtensionApiService.cs`/`XiigenExtensionApiContracts.cs`/`43.SmartSkillsDomain.Dashboard`/`15.OrchestratorModel`→0 | НИ ОДИН из Claude/Cursor/Codex (и общий boundary) не имеет операции backup/export/import/share/continue/rollback: в контракте `XiigenExtensionRequest` нет lifecycle-типа. Пользователь/расширение физически не может запустить ни один шаг цикла — весь сервис доступен только из юнит-тестов. Отдельно от P26 (там parenthetical «Phase08 rollback callers only Tests» про одну операцию) — здесь отсутствует ВСЯ операционная поверхность цикла у хоста. | Ввести lifecycle-операции в контракт расширения (`backup/export/import/share/continue/rollback`), маршрутизировать в `Phase08...Service`; доказать Path A (boundary-вызов) + Path B (локальный desktop: пользователь жмёт export/import, видит результат). |
| G30-2 | Что именно переносит export/import/share (переносимый «checkpoint») | STUB PAYLOAD (metadata, не модель) | `Phase08CommonUserTwoLegLifecycle.cs:56-67` `Phase08PersonalCheckpoint`= SphereId/CheckpointId/**TrainedSignalIds(list<string>)**/TrainingGeneration; пакет `backup-export-package/personal-adaptive-checkpoint.json` = **555 bytes** sha `99c8809f…`; рантайм грузит ДРУГОЙ файл `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json` (P26) | «Backup/export/import обученной пользовательской модели» round-trip'ит 555-байтный JSON со списком signal-id и generation-счётчиком — это НЕ веса/состояние обученной модели и НЕ тот checkpoint, который грузит рантайм расширения. Переносимость доказана для метаданных, а не для модели. Отдельно от P05/P22 (содержимое shipped-checkpoint) — здесь предмет самого lifecycle-переноса. | Export/import должен round-trip'ить НАСТОЯЩИЙ checkpoint модели (тот, что грузит рантайм) через `SkillSelectionMachine.ExportAsync/ImportAsync`; доказать не только byte-identity, но и функциональную эквивалентность (импортированная модель даёт те же выходы на фикс-eval). |
| G30-3 | «Continue-training» в реально исполнившейся (не-no_op) ветке | STUB METRIC (счётчик строк, не обучение) | `Phase08CommonUserTwoLegLifecycle.cs:695-720`: `trained = checkpoint.TrainedSignalIds.Concat(NewTrainingSignalIds).Distinct()`; `MetricBefore=beforeCount`,`MetricAfter=trained.Length`,`MetricImproved=trained.Length>beforeCount`; пакет `phase-08-continue-training-report.json` `metricBefore:2→metricAfter:3` добавлением строки `phase08-signal-3` | Единственный **не**-no_op continue-training меняет ДЛИНУ множества signal-id (union строк), без весов/loss/held-out; `MetricImproved` тривиально true при добавлении любой новой строки-id. «Обучение улучшило модель» = «в списке стало на 1 id больше». Противоположный угол к P25 gap-6 (там continue-probe = no_op before==after): здесь ветка исполнилась, но «метрика» — счётчик. | Continue-training: загрузить checkpoint → подать НОВЫЕ accepted-строки → реальный градиентный апдейт → held-out ПОСЛЕ > ДО (loss↓/acc↑); `MetricBefore/After` = held-out метрика, не длина списка id; count-of-ids не засчитывать как training-evidence. |
| G30-4 | Поле результата экспорта `SourceHashesVerified` | METRIC-HONESTY / имя не соответствует логике | `Phase08CommonUserTwoLegLifecycle.cs:559` `SourceHashesVerified: !files.Any(file => file.ContainsAbsoluteUserPath)` | Поле с именем «source hashes verified» НЕ сверяет ни одного хеша — оно лишь проверяет, что ни в одном файле нет утёкшего абсолютного пути. Имя перезаявляет сделанное (path-leak-check выдаётся за hash-verification). Ново. | Либо реально пересчитывать sha исходных файлов и сверять с манифестом (тогда имя честно), либо переименовать поле в `NoAbsoluteUserPathLeaked`; добавить отдельное поле реальной hash-verification. |
| G30-5 | «Approved anonymized share/copy» | STUB / манифест без переноса и без анонимизации | `Phase08CommonUserTwoLegLifecycle.cs:623-665`: `CreateApprovedShareCopy` строит `entries` с `ShareablePath:false` на всех файлах, `Anonymized:true`/`ContainsRawPrivateText:false` — **захардкоженные булевы**, без трансформации контента; никакой файл не копируется в `ShareDirectory`, пишется только `phase08-anonymized-share-manifest.json`; пакет `approved-anonymized-share/*` = только манифест | «Share/copy» не производит переносимого артефакта и не выполняет анонимизацию: он пишет манифест, перечисляющий файлы как `ShareablePath:false`, и утверждает `Anonymized:true` флагом, а не вычисленной трансформацией. Реальная приватность/перенос не доказаны. Ново. | Реальный share: применить transform, вычищающий приватный текст/пути, ПРОВЕРИТЬ сканом полученного артефакта; фактически произвести shareable-файл (redacted copy); `Anonymized` — результат проверки, а не константа. |
| G30-6 | Common-upgrade + rollback выполнены над sandbox-манифестом, не над shipped-моделью | DISCONNECTED TARGET / sandbox-only | пакет `commonUpgradeResult.commonManifestPortableRef` = `…/phase-08-common-user-two-leg-lifecycle/test-work/common-leg-root/common-upgrade-sandbox/skill-selection-common-manifest.json`; `rollbackResult` восстанавливает тот же sandbox sha `2e4fdb58…`; реальный манифест модели = `models/skill-selection/skill-selection-v1/manifest.json` (не тронут) | Upgrade/rollback прогнаны над одноразовым sandbox-манифестом внутри `test-work/`, а не над реальным promoted-манифестом shipped-модели. Даже корректное byte-restore ничего не доказывает про активную модель. Отдельно от P26 gap-6 (там «нет post-rollback load-probe»): здесь предмет rollback — throwaway sandbox, а не боевой ассет. | Прогнать upgrade→rollback над РЕАЛЬНЫМ `models/skill-selection/skill-selection-v1/manifest.json` (+checkpoint), с post-rollback load-probe (рантайм грузит restored-версию), изоляцией personal-хеша; sandbox-прогон не засчитывать как lifecycle-proof. |
| G30-7 | Personal-нога 3 из 4 сфер | MISSING ARTIFACT (checkpoint не создан) | `phase-08-lifecycle-evidence.json:132,142,162` `sha256:"not-created-yet"` для prompt-to-task / orchestration-reasoning / result-gap-correction; `:85` только skill-selection `covered_by_executable_probe`; `:117` матрица `passed:false` | У 3 из 4 обучаемых сфер personal adaptive checkpoint физически НЕ материализован (`not-created-yet`), executable-probe есть лишь у skill-selection, и матрица сама ставит `passed:false`. «Real lifecycle» закрыт максимум на 1/4, причём для 3 сфер нет даже входного артефакта цикла. Конкретизирует P01 phase-15 (фазовый NOT_STARTED) точным диск-доказательством отсутствия 3/4 checkpoint'ов. | Материализовать personal checkpoint каждой сферы (после реального personal-обучения), прогнать полный lifecycle-цикл на КАЖДОЙ; `passed=true` только когда все 4 строки `covered_by_executable_probe` c реальной моделью. |
| G30-8 | Два расходящихся lifecycle-«доказательства» skill-selection; ни одно не трогает shipped-runtime checkpoint | PROVENANCE DIVERGENCE (lifecycle-evidence) | Phase08: metadata-огрызок sha `99c8809f…`, continue = 2→3 счётчик; p16f: `p16f-export-import-evidence.json` real candidate sha `2507AB03…` (schema v2), `p16f-continue-training-evidence.json` `delta_applied:false`/no-op, `p16f-fresh-load-evidence.json` «не production runtime … не common model»; shipped рантайм = `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json` (ни один пакет его не использует) | Для одной машины два несведённых lifecycle-family с РАЗНЫМИ checkpoint'ами и ПРОТИВОРЕЧИВЫМ continue-training (Phase08: не-no_op счётчик; p16f: явный no-op), и НИ ОДИН не round-trip'ит боевой runtime-checkpoint (оба помечают «не common/final model»). Неоднозначно, какой lifecycle-proof истинный, и оба оторваны от shipped-ассета. Отдельно от P24 gap-6 (два корня ДАТАСЕТА) — здесь два корня lifecycle-EVIDENCE + разрыв с рантаймом. | Свести к ОДНОМУ lifecycle-family на сферу, привязанному к shipped-runtime checkpoint; удалить/пометить расходящийся; e2e: экспорт боевого checkpoint → import в свежий профиль → рантайм даёт те же выходы. |

## 4. Что нужно для НАСТОЯЩЕГО lifecycle-пакета (определение доказательства для Board Phase 15)

1. **Реальные байты модели.** Export/import round-trip'ит боевой checkpoint (тот, что грузит рантайм),
   а не metadata-огрызок. Доказать byte-identity И функциональную эквивалентность (импорт даёт те же
   выходы на фикс-eval).
2. **Триггер с host/extension-поверхности.** Хотя бы один из Claude/Cursor/Codex (или общий boundary)
   экспонирует lifecycle-операцию; Path A (boundary-вызов) + Path B (локальный desktop: пользователь
   реально делает backup/export/import/share и видит результат/скриншот).
3. **Continue-training = реальное обучение.** Загрузить checkpoint → новые accepted-строки → реальный
   апдейт → held-out ПОСЛЕ > ДО; before==after допустимо только при честном отсутствии новых данных;
   счётчик signal-id — не метрика.
4. **Fresh-load improvement измерен.** loaded-checkpoint vs cold-модель на source-disjoint held-out,
   before/after числа, а не parse/loadability.
5. **Rollback над боевым ассетом.** Upgrade реального promoted-манифеста/checkpoint, доказать загрузку
   новой версии рантаймом, rollback, доказать загрузку restored-версии (post-rollback load-probe),
   personal-состояние хеш-изолировано.
6. **Share = реальная анонимизация.** Трансформация, вычищающая приватный текст/пути, проверенная
   сканом полученного артефакта; фактически произведённый shareable-файл, а не только манифест.
7. **Integrity на import/load.** Пересчитать sha файла и сверить с манифестом И индексом; повреждённый
   импорт обязан падать.
8. **Все 4 сферы**, каждая с материализованным personal checkpoint; `passed=true` только когда
   исполнены пробы всех сфер.
9. **Единый источник истины.** Один lifecycle-family на сферу, ссылающийся на shipped-runtime
   checkpoint; без форков metadata-stub / candidate / runtime.
10. **Cross-device.** Импорт в по-настоящему свежий профиль (в идеале на другой машине), запуск
    inference, доказательство работы без переобучения.

## 5. Явные не-претензии этого прохода

- Я не запускал тесты/сборку/рантайм; выводы из чтения кода и артефактов на диске.
- Я не объявляю Board Phase 15 закрытой и не переопределяю честные не-claim'ы пакета.
- Проверенный срез — сервис `Phase08...`, его вызыватели, контракт хоста и два lifecycle-family
  skill-selection; DesignConversion promotion-gate (P26) и wiki-домен DPO-числа (P25) не перепроверялись.
