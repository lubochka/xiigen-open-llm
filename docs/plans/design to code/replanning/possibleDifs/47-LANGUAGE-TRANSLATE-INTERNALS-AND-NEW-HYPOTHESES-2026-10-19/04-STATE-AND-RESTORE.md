# 04. Сохранение обученного состояния и восстановление Units

Этот раздел отделяет **изменение самого обученного состояния** от **выбора и загрузки другого состояния**. На текущем pin уже есть положительные native результаты доставки частей урока в переводческую память и восстановления этой памяти в новом процессе. Они должны быть исходной точкой следующего эксперимента. Новая гипотеза LT-N04 касается следующего шага: повторный export способен переписать файлы опубликованной модели раньше проверки старого registry index.

**Версии:** текущий core — `a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6`; успешная ветка `language_translate` — `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`. Ссылки ниже закреплены за commits. Новый native эксперимент в рамках этого аудита **не запускался — NOT_RUN**. Исторические измерения обозначены отдельно от выводов по коду.

## 4.1. LT-N04 — повторный export меняет опубликованные файлы до проверки старого index

**Статус:** новая конкретная гипотеза о публикации и восстановлении; причинная цепочка подтверждена кодом. Частота этого сценария в рабочем процессе и его влияние на доступные обученные ответы ещё не измерены.

**Условие:** asset A уже опубликован, а следующий `export-translation-model` получает тот же `--output`. Export переписывает файлы A, но `models/index.json` всё ещё содержит их старые SHA. Последующий publish проверяет старую запись до её замены и отклоняет обновление. При этом существующий CLI load-by-model-id не выполняет эту проверку SHA и может открыть уже переписанный B.

Это не доказательство повреждения LoRA, ухудшения градиента или потери всех весов. Это конкретное расхождение между опубликованной идентичностью модели и фактическими bytes, из которых загружаются Units.

### Подтверждённая цепочка

| Этап | Что делает текущий код | Следствие при повторном использовании каталога |
|---|---|---|
| CLI export | `ExportTranslationModelAsync` берёт пользовательский `--output`, вызывает AssetStore и задаёт новое `CreatedAtUtc: DateTimeOffset.UtcNow`. | Новый каталог или generation identifier не требуются. Даже тот же checkpoint может получить другой manifest SHA. |
| Запись asset | `ExportAsync` выполняет `File.Copy(..., overwrite: true)` для checkpoint и report, затем записывает `manifest.json`. | Файлы, на которые уже ссылается index A, могут измениться до publish B. |
| Publish | Вычисляет новые hashes, вызывает `LoadRegistry`, и лишь после успешной проверки выполняет clone → upsert → запись index. | Проверяется ещё старый index A против уже новых файлов. |
| Проверка index | `LoadRegistry` передаёт прежние bytes index в `ValidateLoadedIndex`. Guard читает manifest/checkpoint по сохранённым путям и сравнивает SHA. | Изменённый manifest или checkpoint вызывает hash mismatch; upsert B не выполняется. |

Источники: [CLI export, Program.cs:2275–2314](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L2275-L2314); [перезапись asset, TranslationModelAssetStore.cs:84–112](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationModelAssetStore.cs#L84-L112); [порядок publish:220–244](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationModelAssetStore.cs#L220-L244); [LoadRegistry:361–372](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationModelAssetStore.cs#L361-L372); [сравнение SHA, RepositoryModelIndexIntegrityGuard.cs:313–341](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/00.Core/RepositoryModelIndexIntegrityGuard.cs#L313-L341).

На уровне API отказ имеет код `TRANSLATION_MODEL_INDEX_EXISTING_ROW_INTEGRITY_FAILED`; исходная причина guard — `REPOSITORY_MODEL_INDEX_HASH_MISMATCH`. CLI publish бросает `InvalidOperationException` с текстом ошибки, top-level выводит message в stderr и возвращает `RuntimeError = 3`. Нельзя обещать, что оба API error codes будут доступны в машинном JSON CLI: эта ветка выводит текст exception. [AssetStore:367–370](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationModelAssetStore.cs#L367-L370), [guard:63](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/00.Core/RepositoryModelIndexIntegrityGuard.cs#L63), [CLI publish:2578–2601](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L2578-L2601), [CLI constants:25–27](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L25-L27), [exception handling:93–110](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L93-L110).

### Проверяемый index и CLI load ведут себя по-разному

`LoadTranslationModelAsync` вызывает `ResolveModelManifestAsync`. Этот resolver десериализует index, выбирает запись по ModelId и возвращает её manifest path **без проверки registry hashes**. `ProbeTranslationManifestAsync` затем читает текущий manifest, вычисляет путь checkpoint, вызывает `TranslationMachine.LoadCheckpointAsync` и запускает обычный orchestrator. Поэтому после отказа publish B CLI может уже загружать B по старому указателю A. Успешность встроенных fixtures зависит от содержимого B. [Program.cs:2742–2751](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L2742-L2751), [resolver:4796–4807](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L4796-L4807), [fresh load и Flow:4238–4269](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L4238-L4269).

Отсюда два отдельных наблюдения для опыта: **отказ проверки идентичности опубликованного состояния** и **загрузка bytes, не соответствующих этой опубликованной идентичности**. Универсальная недоступность модели из этого кода не следует.

Существующий тест обновления registry это условие не проверяет: новый export направлен в `root/asset`, а старая translation row — в `stale/manifest.json` и `stale/translation-v1-checkpoint.json`. Старые bytes остаются доступны до проверки. Тест подтверждает замену записи при разных каталогах; сценарий old path == new path остаётся отдельным. [TranslationModelAssetTests.cs:543–576](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/Tests/TranslationModelAssetTests.cs#L543-L576), [старые paths/hashes:604–612](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/Tests/TranslationModelAssetTests.cs#L604-L612), [publish assertion:650–656](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/Tests/TranslationModelAssetTests.cs#L650-L656).

**Отличие от прежних гипотез:** это не повтор темы P25 cold restore или edited-corpus identity из [planning40](https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/40-NEW-CORPUS-IDENTITY-AND-INTRABATCH-ADMISSION-2026-10-18.md). Новый фактор — разрешённая последовательная перезапись опубликованных paths до old-index validation и различие проверяющего и непроверяющего consumers. Проверка обоих существующих корпусов possible-diffs не выявила этой конкретной цепочки.

**Связь с language_translate:** поздний проект `27.TranslationDomain` отсутствует в [дереве успешной ветки на закреплённом commit](https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9). LT-N04 — гипотеза о сохранении доступного обученного поведения в текущем asset workflow. Она не представляется доказанной причиной первого RP01 или экспериментом, уже восстановившим историческую алгоритмическую точность.

## 4.2. Эксперимент LT-N04: повторная публикация при неизменном обученном состоянии

**NOT_RUN.** Реализация — один короткий native harness на настоящих `TranslationModelAssetStore`, `RepositoryModelIndexIntegrityGuard`, CLI resolver и `TranslationMachine`. Работать с независимыми временными копиями repository snapshot.

**Материал:** checkpoint, полученный из прежних 8 EN→RU уроков, прежняя замороженная панель 24 заданий, валидные training-data artifact и report. Source checkpoint/report должны находиться вне переиспользуемого output directory. В первом этапе не создавать новый DPO и не менять weights, corpus, mappings, порядок уроков, scorer или thresholds. Если потребуются дополнительные chosen/rejected для второго этапа, их автор — внешний открытый китайский teacher; CODEX с Astra 6 UltraCode выполняет отбор и оценку качества, не подменяя авторство сторон пары.

Сначала опубликовать A в каталог D и проверить hashes, fresh load и все 24 ответа. Из этого состояния сделать три независимые копии:

| Плечо | Единственное различие |
|---|---|
| A — контроль | Повторного export нет. |
| B — тот же каталог | Экспортировать **тот же checkpoint/report** в D с новым временем manifest; затем publish. |
| C — новый каталог | Выполнить тот же повторный export с тем же новым временем в D2; затем publish, сохранив D. |

До export, после export и после publish фиксировать exact index bytes/SHA, manifest bytes/SHA, checkpoint bytes/SHA, corpus identity, version, API error code, CLI stdout/stderr/exit. Отдельно вызвать проверку index и существующий CLI load. Снять фактически прочитанный checkpoint SHA; нельзя считать его равным ожидаемому только по ModelId.

После каждой успешной fresh load выполнить **все 24 обычных Flow-запроса**. Сравнить с исходным A output, выбранный route/adapter, confidence/verdict и strict correctness. Встроенные две CLI fixtures не заменяют эту панель. Отказ валидации учитывать отдельно от ошибки перевода.

**Предсказание по коду:** B export успешно изменяет manifest, guard отклоняет старый index, publish не выполняет upsert; существующий CLI resolver может читать изменённые bytes. C сохраняет старый asset для проверки и позволяет обновить указатель. Это предсказание, не результат выполненного запуска.

**Подтверждение:** смена только стратегии каталога устраняет отказ/расхождение идентичности при неизменных checkpoint и учебном материале; сохранённые правильные ответы A доступны из того же проверяемого состояния. Во втором, необязательном этапе повторить B/C с checkpoint B после одного нового законного урока, чтобы проверить и прежние навыки, и новый. Падение accuracy без подтверждения загруженных bytes не засчитывается как ухудшение обучения.

**Опровержение для рабочего workflow:** он всегда использует новый каталог, старые bytes остаются доступны, hashes и загруженная идентичность совпадают, old/new поведение проходит сравнение. Тогда API-сценарий in-place остаётся риском, но не объясняет наблюдавшийся инцидент.

Причина расположена до языковой обработки, поэтому EN→RU достаточно для первого опыта. Проверка других направлений нужна как отдельная retention-панель при распространении ремонта на общий workflow. На RP01 `ResearchCheckpointEnRuAdapter` или repository-owned neural checkpoints результат автоматически не переносится.

Проверяемый ремонт — immutable generation directory для export, проверка нового checkpoint и поведения, затем обновление index, пока старый A ещё доступен. При отказе прежние bytes и указатель сохраняются. Выключение hashes или удаление старой registry row не является этим ремонтом.

## 4.3. Историческое восстановление generated Units: сравнивать память вместе с весами

В `language_translate` `TeachingRegistryPersistence.LoadGeneratedCapabilityUnit` загружает tokenizer, создаёт LlmModel, применяет LoRA, загружает policy checkpoint и затем строит KNN-примеры из **всех соседних manifests того же domain, созданных не позже выбранного manifest**. Примеры сортируются по времени и UnitId. [Исторический TeachingRegistryPersistence.cs:291–341](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/TeachingRegistryPersistence.cs#L291-L341).

Текущий loader сначала выполняет `ProjectBoundKnnCorpus(manifest)`, проверяет tokenizer/checkpoint hashes, runtime provenance и vocab size. После создания model и применения LoRA загружает checkpoint; KNN rebuild получает именно `knnCorpus.Value.Examples`. Соседние manifests больше не определяют содержимое этой памяти. [Current:1621–1635](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/TeachingRegistryPersistence.cs#L1621-L1635), [проверка identity:1657–1682](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/TeachingRegistryPersistence.cs#L1657-L1682), [model/LoRA/checkpoint/KNN:1693–1715](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/TeachingRegistryPersistence.cs#L1693-L1715).

Это реальное историческое различие источника памяти, уже затронутое прежними исследованиями восстановления и replay; оно не считается новой LT-N04. Старый loader мог получить дополнительные примеры за счёт окружения. Текущий восстанавливает явно привязанный snapshot. Ухудшение качества от этой замены по одному коду не доказано.

Для восстановления исторического результата нужно зафиксировать и сравнить список `ExampleId/TrainingText/OutputText/PredicateIds/DomainTags`, а не только weights SHA. Сначала сравнивать одинаковый явно сохранённый corpus до/после. Затем отдельно испытывать необходимое cumulative replay. Возврат чтения произвольных соседних manifests смешал бы проверку алгоритма с изменением данных.

## 4.4. Какие прежние утверждения о restore уже нельзя повторять

### LoRA-before-load сохранена; optimizer зависит от маршрута

В обоих приведённых generated capability loaders LoRA применяется **до** чтения policy checkpoint. Кроме того, текущий `IncrementalTrainingSession` имеет adapter-aware повтор загрузки при соответствующем topology mismatch: он создаёт model, применяет заданную LoRA topology и повторно читает checkpoint. [IncrementalTrainingSession.cs:363–379](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L363-L379).

Текущий DPO unit вызывает `TryRestoreContinuationState(RandomStateToRestore, OptimizerStateToRestore)`; несовместимый state получает failure. После обучения экспортируется post-step optimizer state. Поэтому «current всегда забывает optimizer при restore» — слишком широкое утверждение. [LlmIngredientLogicalUnits.cs:1312–1322](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1312-L1322), [1393–1402](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1393-L1402).

Другая incremental-session factory создаёт новый AdamW и задаёт per-call warmup=0. Это отдельный путь, который нельзя смешивать с DPO portable-state continuation. [IncrementalTrainingSession.cs:403–407](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L403-L407). Само наличие восстановления не доказывает сохранность обучения во всех конфигурациях; эксперимент обязан назвать конкретный caller.

### Полный TranslationMachine checkpoint восстанавливает discrete state

Generic `ExportAsync/ImportAsync` в `TranslationMachine` — metadata snapshot. Полное сохранение имеет отдельный API. `ImportCheckpoint` сначала проверяет структуру, затем восстанавливает KNNstore, tokenizer corrections, direction Executors, verifier rules, history и structured identities. [Metadata API:204–258](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L204-L258), [полный ImportCheckpoint:334–381](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L334-L381).

[Native P25 cold restore от 18 октября](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md) проверил новый процесс, настоящий `LoadCheckpointAsync/ImportCheckpoint`, ноль новых уроков и 24 ordinary Flow calls. Сохранились **все 24 наблюдаемых ответа/route/confidence/verdict**, включая **19 правильных из 24**. Canonical state совпал после исключения только ExportedAtUtc; сохранились и прежние ошибки смысла. Это измерение было выполнено на указанном в report HEAD `7be28a0d...` и конкретной Domain DLL, а не новый запуск на audit pin.

Этот положительный результат закрывает узкий сценарий same-version cold import исследованной памяти. Он не закрывает перенос старого `language_translate` checkpoint между версиями, registry hash rejection, AssetStore повторную публикацию, все языки или neural continuation. Внешнюю SHA-проверку экспериментального harness нельзя приписывать продуктовой schema checkpoint.

## 4.5. Доставка частей урока уже исправлена: положительный контроль для новых опытов

Ранее реальный CLI corpus reader и builder теряли заданные соответствия частей урока. На текущем pin optional `phrase_mappings` читается из строки, сохраняется в `TranslationContinueCorpusRow` и передаётся в `new ExecutorOutput(row.Target, 0.95, null, row.PhraseMappings)`. [Program.cs:4442–4469](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L4442-L4469), [row:5144–5147](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L5144-L5147), [builder:4682–4699](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L4682-L4699).

[Native translation-memory-ingress-01](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md) сравнил два новых экземпляра на тех же 8 уроках:

| Показатель | До доставки mappings | После |
|---|---:|---:|
| Parts mappings в памяти | 0 | 16 |
| Новые сочетания знакомых частей | 0/8 | 8/8 |
| Все правильные ответы | 11/24 | 19/24 |
| Краевые случаи | 0/5 | 0/5 |
| Принятые ответы, не прошедшие строгую оценку | 1 | 3 |

Нейронных обновлений в опыте не было. Изменился перенос уже заданного материала в существующий Executor; проверялись настоящие private reader/builder через reflection, `TrainStructuredBatchAsync` и обычный Flow. Полный CLI с registry/manifest не запускался. Исправление уже опубликовано в commit `2487d2c`; его нельзя повторно объявлять новым открытым дефектом. Наблюдения об отрицании, вопросе о прошлом и неоднозначном соединении объектов остаются отдельными ограничениями этого результата.

У `TranslationStructuredBatchBuilder` другой producer: одна train-ready pair становится semantic/executor/verifier examples, с собственными heuristic и explicit mapping путями. Исправление CLI reader не доказывает сохранность частей во всех преобразователях DPO → Units. [TranslationStructuredBatchBuilder.cs:77–141](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L77-L141), [BuildWordMappings:328–382](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L328-L382).

Для следующего опыта следует сохранить эти 19 правильных ответов и точную идентичность их state, затем менять один исследуемый механизм. Само увеличение числа принятых ответов или successful load не заменяет проверку того, что загружено ожидаемое состояние и сохранён смысл прежних правильных переводов.

