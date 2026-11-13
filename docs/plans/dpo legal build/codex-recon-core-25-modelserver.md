# Рекон: 25.ModelServer — жизненный цикл модели (IModelRepository, манифест, чекпойнт, export/import/locator)

Назначение файла: evidence для ПЛАНА проекта ILearningDynamicGraph. Это сбор фактов, не разрешение на реализацию.
Область прочитана полностью (проект маленький, 8 исходных .cs + .csproj). Worktree-копии (`.claude/worktrees/*`) проигнорированы.

---

## 1. Что это и где (точные ключевые пути)

`25.ModelServer` — тонкий слой публикации/локации обученных юнитов (опубликовать чекпойнт → манифест → найти по unitId). Сам не тренирует и не инференсит модель.

Контракты (`C:/Projects/llm_mvp_core/25.ModelServer/Contracts/`):
- `IModelRepository.cs` — `LoadWeightsAsync(unitId, serverBaseUrl?, localFallbackPath)` и `UploadAsync(GeneratedCapabilityCheckpointManifest, serverBaseUrl)`.
- `IModelServer.cs` — `PublishAsync(RemoteUnitManifest, Stream weights)` и `OpenWeightsAsync(unitId)`. ВАЖНО: интерфейс объявлен, но реализаций `IModelServer` в репозитории нет (см. Пробелы).
- `IRemoteUnitLocator.cs` — `LocateAsync(unitId)` и `RegisterAsync(RemoteUnitManifest)`.

Реализации:
- `Local/LocalModelRepository.cs` — единственная реализация `IModelRepository`.
- `Remote/InMemoryRemoteUnitLocator.cs` — единственная реализация `IRemoteUnitLocator` (in-memory `Dictionary`).
- `Remote/RemoteLogicalUnitProxy.cs` — `ILogicalUnit<string,string>`, прокси: по `unitId` находит манифест и возвращает результат.
- `Remote/RemoteUnitManifest.cs` — записи `RemoteUnitManifest`, `RemoteModelConfig`, `RemoteLoraConfig`.
- `Http/ModelEndpointRegistry.cs` — ещё один in-memory реестр endpoint'ов (Register/Resolve), дублирует логику локатора, НЕ реализует интерфейс.

Тип манифеста/чекпойнта `GeneratedCapabilityCheckpointManifest` определён НЕ здесь, а в:
`C:/Projects/llm_mvp_core/18.CapabilityTraining/TeachingRegistrySnapshot.cs` (стр. 41-83). 25 ссылается на 18 через `.csproj`.

`ModelServer.csproj`: ссылки только на `00.Core` и `18.CapabilityTraining`. Зависимостей от 15.OrchestratorModel нет.

---

## 2. Как устроено/работает (кратко)

Поток публикации (export), на примере реального потребителя
`42.ProjectWikiDomain/ProjectWikiRequestRoutingManifestPublisher.cs`:
1. Гейты ДО публикации: verdict == `..._GATE_PASSED`, `ModelSnapshot.GatePassed`, `SemanticQualityClaimed`, `PublishAllowed` — иначе `Fail` с кодом. Это защита от публикации необученной модели.
2. Пишутся файлы: `policy.bin`, `tokenizer.json`, `generated_capability_manifest.json`.
3. Строится `GeneratedCapabilityCheckpointManifest` (полный конфиг модели + LoRA + provenance: TrainingBranch/TrainingRunId/SourceDatasetSha256).
4. `IModelRepository.UploadAsync(manifest, serverBaseUrl)` → `LocalModelRepository`:
   - проверяет `PolicyCheckpointPath` существует на диске (`POLICY_CHECKPOINT_NOT_FOUND`);
   - считает SHA-256 чекпойнта (`ComputeSha256`);
   - конвертирует `GeneratedCapabilityCheckpointManifest` → `RemoteUnitManifest` (endpoint `/models/{unitId}`), время `UtcNow`.
   - НЕ делает реальной сетевой загрузки — просто возвращает заполненный `RemoteUnitManifest`.
5. `IRemoteUnitLocator.RegisterAsync(remoteManifest)` → `InMemoryRemoteUnitLocator` кладёт манифест в `Dictionary<unitId, manifest>`.

Поток локации (import/load), на примере `26.TrainableRouter/Training/DistributedRoutingBootstrap.cs`:
- `RouteAndLocateAsync`: после решения роутинга вызывает `runtime.Locator.LocateAsync(chosenUnitId)` → `RemoteUnitManifest`. То есть локатор отдаёт МЕТАДАННЫЕ (endpoint, sha, конфиг), а не веса.

Загрузка весов: `LocalModelRepository.LoadWeightsAsync` открывает локальный файл `localFallbackPath` (`File.OpenRead`) если существует, иначе `MODEL_WEIGHTS_NOT_FOUND`. Параметр `serverBaseUrl` принимается, но в коде НЕ используется для удалённой загрузки (нет HTTP).

`GeneratedCapabilityCheckpointManifest` (чекпойнт-манифест) содержит поля жизненного цикла: `PolicyCheckpointPath`, `TokenizerPath`, конфиг (Vocab/Context/Hidden/Layers/Heads/KvHeads/Ffn), LoRA (Rank/Alpha/Dropout/TargetModules), continue-training поля (`ParentUnitId`, `ParentPolicyCheckpointPath`, `ParentTokenizerPath`, `TrainingStepCount`, `UnitFamilyId`), provenance (`TrainingBranch`, `TrainingRunId`, `SourceDatasetSha256`, `UploadedAtUtc`, `ManifestSha256`), `TrustState`.

---

## 3. Релевантность к ILearningDynamicGraph

Высокая как ИНФРАСТРУКТУРА жизненного цикла, низкая как обучающая/графовая логика.

- DPO/обучение: 25 НЕ участвует в построении DPO и не тренирует. Он — точка ПУБЛИКАЦИИ итогового чекпойнта. CLAUDE.md "Trainable Unit pattern" прямо требует публиковать принятые чекпойнты через `25.ModelServer` (`IModelRepository`) и делать юнит находимым через `IRemoteUnitLocator`. Значит, любой обучаемый узел графа (ILearningUnit/Flow/Orchestrator/DynamicGraph) должен экспортироваться/находиться через эти контракты — это обязательный финал lifecycle, который план должен покрыть.
- Граф/оркестрация (узлы/рёбра): прямой поддержки графа нет. Есть только адресация по `unitId` (плоский словарь). Для DynamicGraph с many-to-many рёбрами потребуется расширение: манифест узла графа, рёбра, версии. Сейчас манифест — на отдельный юнит, без понятия рёбер/связей.
- Измерение/качество: 25 сам метрик не считает, но `UploadAsync` опирается на гейты вызывающего (snapshot gate, semantic_quality_claimed, publish_allowed). Числовые метрики кладутся вызывающим в `TrainingText`/`OutputText` манифеста как строки (см. publisher), а не как типизированные поля — слабое место для проверки качества узла графа.
- Teacher-provenance (Legal Teacher Gate): provenance в манифесте есть на уровне ДАТАСЕТА/обучения (`SourceDatasetSha256`, `TrainingBranch`, `TrainingRunId`), НО полей teacher-провенанса из скила НЕТ: `teacher_access_mode`, `hosting_provider`, `model_id`, `model_license`, `provider_terms_checked`, `teacher_output_used_for_training`, `closed_proprietary_model`. Если DPO для графа строится с участием teacher-модели, манифест 25 эти обязательные поля не несёт — пробел для плана.
- split общий/адаптивный (R5): на уровне 25 разделения нет. Есть `TrustState` и `DomainTag`, но нет явного признака common-vs-user-adaptive и нет двух-ногой модели. `ParentUnitId`/`ParentPolicyCheckpointPath`/`TrainingStepCount` дают механику continue-training (дообучение от существующего чекпойнта), но не разделяют общий и пользовательский слой (требование `ILearningMachinePair`). План split'а должен это вводить выше уровня 25 или расширять манифест.

---

## 4. Классификация по уровням архитектуры (правила trainable-architecture-decision-SKILL.md)

| Компонент | Класс | Обоснование (ссылка на правило) |
|---|---|---|
| `RemoteLogicalUnitProxy : ILogicalUnit<string,string>` | Уровень `ILogicalUnit` (но НЕ обучаемый) | Реализует разрешённый контракт `ILogicalUnit`. Но lifecycle-evidence обучения нет: возвращает `request.Input` с confidence=1.0, `["REMOTE_UNIT"]`, без trained state. По скилу это `OK_INTERNAL_DETAIL`/транспорт-прокси, НЕ semantic/trained unit. Подавать как обученный узел графа нельзя (REPORT_CLAIM_TOO_STRONG риск). |
| `IModelRepository` / `LocalModelRepository` | Внутренний детерминистический хелпер (lifecycle I/O) | Это persistence/serialization (файл, SHA-256, маппинг записей). Не scorer/router/selector. По скилу — не trainable level, а инфраструктура lifecycle (export/import). Допустимо как helper. |
| `IRemoteUnitLocator` / `InMemoryRemoteUnitLocator` | Внутренний детерминистический хелпер (lookup-словарь) | Чистый `Dictionary` lookup по `unitId`. Скил: dictionary/lookup сам по себе НЕ архитектурный уровень. Это `OK_INTERNAL_DETAIL` (locator/registry), пока он только адресует, а не принимает обучаемых решений. |
| `ModelEndpointRegistry` | Внутренний детерминистический хелпер (lookup-словарь) | Дублирует логику локатора (Register/Resolve по словарю), не реализует интерфейс. `OK_INTERNAL_DETAIL`, но дубль — кандидат на унификацию. |
| `RemoteUnitManifest` / `GeneratedCapabilityCheckpointManifest` | Данные (record), не уровень | Манифест/чекпойнт-метаданные. Не механизм решения. Носитель lifecycle-evidence (checkpoint/manifest/provenance). |
| `IModelServer` | Контракт без реализации | Объявлен `PublishAsync`/`OpenWeightsAsync`, реализаций в репо нет. Не классифицируется как рабочий механизм. |

Вывод по детерминизму-vs-обучению: в 25.ModelServer НЕТ скрытого детерминистического scorer/router, выдающего себя за обучаемый. Locator/registry честно являются адресацией. Единственный риск маскарада — `RemoteLogicalUnitProxy`: он носит тип `ILogicalUnit`, но семантики/обучения не несёт; в плане ILearningDynamicGraph его нельзя считать узлом-моделью без реального инференса весов.

---

## 5. Пробелы / что доисследовать

1. `IModelServer` без реализации: `PublishAsync(manifest, Stream weights)` и `OpenWeightsAsync` нигде не реализованы. Реальной передачи весов (Stream) по сети нет; `serverBaseUrl` в `LoadWeightsAsync` игнорируется. Для графа с удалёнными узлами это незакрытый lifecycle (import весов реально только из локального файла).
2. Teacher-provenance отсутствует в манифесте: нет полей `teacher_access_mode`/`hosting_provider`/`model_id`/`model_license`/`provider_terms_checked`/`teacher_output_used_for_training`/`closed_proprietary_model` (требование Legal Teacher / Provenance Gate). Доисследовать: где они хранятся (датасет/раннер?) — кандидаты в `18.CapabilityTraining`, `43.LearningLocalWikiData/tools/ProjectWikiTrainingRunner`.
3. Нет графовых контрактов: манифест на отдельный `unitId`, без рёбер/fan-out/fan-in/multi-output. Для ILearningDynamicGraph (правило Graph-Style Reconstruction Gate) нужно решить, расширять ли 25 манифестом графа/рёбер или вводить отдельный слой.
4. split общий/адаптивный (R5) на уровне 25 не выражен: нет common-vs-user-adaptive флага и двух-ногой модели (`ILearningMachinePair`). Continue-training есть (`ParentUnitId`/`TrainingStepCount`), split — нет. Доисследовать `00.Core/LearningMachineContracts.cs` и `26.TrainableRouter`.
5. Метрики качества узла хранятся строками в `TrainingText`/`OutputText` манифеста (см. publisher), не типизированы — слабо для машинной проверки гейтов качества графа.
6. Два параллельных реестра: `InMemoryRemoteUnitLocator` и `ModelEndpointRegistry` дублируют friend-логику; уточнить, кто канонический для графа.
7. Реальные потребители 25 (вне worktrees): `42.ProjectWikiDomain/*ManifestPublisher.cs` (несколько publisher'ов), `26.TrainableRouter`, тесты `Tests/DistributedModelArchitectureTests.cs` и `Tests/TrainableRouterOrchestrationTests.cs`, документация `docs/projects/25-model-server.md`. Их стоит прочитать при детализации плана (не читались полностью в этом реконе — защита от перегруза).
