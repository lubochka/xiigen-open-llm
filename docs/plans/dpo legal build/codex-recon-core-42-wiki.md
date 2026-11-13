# Codex Recon — Bootstrap ILogicalUnit (6 шт.) и flow-адаптеры в 42.ProjectWikiDomain

corpus: core · slug: core-42-wiki · relevance: high

## Что это и где (точные ключевые пути)

- Контракты 6 юнитов: `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationUnitContracts.cs`
  - 6 интерфейсов, каждый наследует `ILogicalUnit<TIn,TOut>`: `IIntentBuilderUnit`, `IProjectScannerUnit`, `IEvidencePlannerUnit`, `IUnitSelectorUnit`, `IRouteComposerUnit`, `IOrchestrationVerifierUnit` (строки 76–107).
  - Агрегатные input-записи: `EvidencePlannerInput`, `UnitSelectorInput`, `RouteComposerInput`, `VerifierInput`; output-запись `VerifierResult` (заменяет голый bool).
- Реализации Bootstrap (6 классов): `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/BootstrapOrchestrationUnits.cs`
  - `BootstrapIntentBuilderUnit`, `BootstrapProjectScannerUnit`, `BootstrapEvidencePlannerUnit`, `BootstrapUnitSelectorUnit`, `BootstrapRouteComposerUnit`, `BootstrapOrchestrationVerifierUnit`.
- Статическая логика + сборка пайплайна: `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectTaskOrchestrationPipeline.cs`
  - `TaskOrchestrationUnit` (оркестратор-секвенсор шагов 1–6), `SubmitUserTaskApi`, и статические классы-первоисточники: `ProjectUnderstandingUnit`, `EvidencePlanner`, `UnitSelector`, `HumanRouteComposer`, `OrchestrationVerifier`.
- Flow-адаптеры (уровень `ILogicalFlow<string>`):
  - `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectDocumentationPipelineFlowAdapter.cs` (FlowId `project-documentation-pipeline-flow`, ScenarioId `project_documentation_pipeline`).
  - `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiTrainingFlowAdapter.cs` (FlowId `project-wiki-training-flow`, ScenarioId `project_wiki_training_orchestrator`).
- Контракты уровней иерархии: `C:/Projects/llm_mvp_core/00.Core/LogicalContracts.cs` — `ILogicalUnit<TInput,TOutput>` (стр. 97), `ILogicalFlow<TInput>` (стр. 166), `ILogicalOrchestrator` (стр. 202).
- Управляющие правила/статус: `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/README.md` (File Map + Integration Points), коммент-шапки обоих файлов ссылаются на `TECHNICAL-DEBT-CLOSURE-PHASE-0.md (TD1)` и `A-LM-03`.

ВАЖНО (пробел регистрации): grep по `ProjectWikiDomainServiceCollectionExtensions.cs` НЕ нашёл регистрации flow-адаптеров. По README §Integration Points регистрация выполняется в `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs` (файл в этой рекон-задаче не открывался — см. Пробелы). `ProjectWikiOrchestrationFlow` (TD2) / id `project-wiki-orchestration-flow` / `BOOTSTRAP_FALLBACK` в области 42 grep'ом не найдены под этими именами; вероятно живут в другом файле/проекте (CLAUDE.md упоминает TD2, но это не доказано локально).

## Как устроено/работает (кратко)

Цепочка одного пользовательского задания (deterministic, без обучения):
`SubmitUserTaskApi.SubmitAsync` → `TaskOrchestrationUnit.OrchestrateAsync` исполняет 6 шагов строго последовательно (fan-in аккумулирующий, без fan-out):
1. `_intentBuilder.ExecuteAsync` (вход `string` промпт) → `TaskOrchestrationIntent`.
2. `_projectScanner.ExecuteAsync` (вход `SubmitUserTaskRequest`) → `ProjectUnderstanding`.
3. `_evidencePlanner.ExecuteAsync` (вход `EvidencePlannerInput`=intent+project) → `TaskEvidencePlan`.
4. `_unitSelector.ExecuteAsync` (вход +evidencePlan) → `TaskUnitSelection`.
5. `_routeComposer.ExecuteAsync` (вход +request +units) → `HumanTaskRoute`.
6. `_verifier.ExecuteAsync` → `VerifierResult{Passes,Verdict}`; статус `ORCHESTRATED`/`NEEDS_MORE_EVIDENCE`.

Делегирование статике (это и есть «стартовая точка DPO»):
- Конструктор `TaskOrchestrationUnit(...=null)` по умолчанию подставляет 6 Bootstrap-инстансов (`ProjectTaskOrchestrationPipeline.cs:78-93`). Комментарий прямо фиксирует: при передаче обученных реализаций из `25.ModelServer` используются они (A-LM-03) — то есть это слот для замены на trained-юнит.
- `BootstrapIntentBuilderUnit` — единственный, кто несёт логику внутри себя (приватные `BuildIntent`/`ExtractWorkItems`: `Regex.Replace` нормализация, `Regex.Split` по `,|and|then|->`, выбор PascalCase через `\b[A-Z]...\b`). Остальные 5 — тонкие обёртки: создают `new ProjectUnderstandingUnit()/EvidencePlanner()/UnitSelector()/HumanRouteComposer()/OrchestrationVerifier()` и вызывают их методы (`UnderstandAsync`/`PlanEvidence`/`SelectUnits`/`Compose`/`Verify`), затем заворачивают результат в `LogicalUnitResult` с `Confidence: 1.0` и пустыми `PredicateIds/EvidenceIds/Warnings`.
- Природа статики (детерминизм): `ProjectUnderstandingUnit` — обход ФС `Directory.EnumerateFiles` + проверки наличия файлов (`angular.json`, `*.csproj`, `*.sql`...) и чтение строк evidence; `EvidencePlanner` — фиксированный список вопросов + keyword-проверки (`figma/audio/diagram/hosting/platform`); `UnitSelector` — жёсткая if-цепочка имён юнитов (`RAG/context` → `WikiContextRoutingUnit` и т.п.); `HumanRouteComposer` — шаблон `StringBuilder` (русский текст ответа); `OrchestrationVerifier` — булева конъюнкция непустых полей.

Flow-адаптеры: оба реализуют `ILogicalFlow<string>`, читают параметры из `request.Metadata` (`artifact_root`/`project_root` и т.п.), делегируют оркестратору (`ProjectWikiTrainingOrchestrator` / `ProjectDocumentationPipelineOrchestrator`) и собирают `LogicalFlowResult` с метриками. Честный статус «раннер не обучал»: documentation-адаптер при пройденном coverage отдаёт `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING` и `training_status=runner_missing`; training-адаптер ставит `training_status` ∈ {`not_training_route`,`published`,`trained_not_published`} и в registry-режиме помечает все числовые метрики `UNKNOWN` (антимаскарад: не выдумывает loss/accuracy).

## Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

- DPO seed-точка: эти 6 Bootstrap-юнитов и есть канонический «старт обучения» для оркестрации (auto-DPO для ILearningUnit/Flow/Orchestrator). Для ILearningDynamicGraph это готовый набор узлов графа с типизированными I/O-контрактами и местом подмены на trained-веса (A-LM-03), без переписывания статической логики.
- Граф/рёбра: `TaskOrchestrationUnit` даёт линейный DAG из 6 узлов (1→2→3→4→5→6, аккумулирующий fan-in). Это базовый under-trained скелет для Graph-Style Reconstruction Gate скила: узлы = 6 юнитов; рёбра = последовательные передачи записей; many-to-many/fan-out отсутствуют (кандидат на расширение в DynamicGraph). Каждое ребро сейчас несёт детерминистический результат с `Confidence=1.0` — финальная authority на рёбрах сегодня у детерминистических хелперов, а не у обученных юнитов.
- Измерение/teacher-provenance: контракт метрик и «честного untrained» статуса уже задан во flow-адаптерах (rows, primary/secondary accuracy, DPO loss/accuracy/reward_margin, cpu/ram/elapsed, `UNKNOWN` для недоступного). DPO-строки и Section-22 runner-report — `ProjectWikiTrainingFlowAdapter`. Provenance teacher'а в самих юнитах НЕ присутствует (узлы детерминированы); Legal Teacher Gate надо применять на этапе генерации DPO-наборов, а не к Bootstrap-коду.

## Классификация (по правилам trainable-architecture-decision-SKILL.md)

| Capability | Файл/класс | Заявленный паттерн | Фактический механизм | Уровень / классификация | Runtime authority | Семантическая претензия | Verdict |
|---|---|---|---|---|---|---|---|
| Парсинг промпта в intent | `BootstrapIntentBuilderUnit` (BuildIntent/ExtractWorkItems) | `ILogicalUnit` (Bootstrap) | Regex split/normalize, PascalCase извлечение | `ILogicalUnit` (контракт), внутри — детерминистический regex-хелпер | Да (на ребре 1) | Нет (Confidence=1.0, untrained scaffold) | `OK_INTERNAL_DETAIL` как scaffold; честно помечен Bootstrap/стартом DPO |
| Скан проекта | `BootstrapProjectScannerUnit`→`ProjectUnderstandingUnit` | `ILogicalUnit` | Обход ФС + наличие файлов/чтение строк | `ILogicalUnit` обёртка над детерминистическим сканером | Да | Нет | `OK_INTERNAL_DETAIL` (baseline, помечать eval/scaffold) |
| План evidence | `BootstrapEvidencePlannerUnit`→`EvidencePlanner` | `ILogicalUnit` | Фикс-список вопросов + keyword match | `ILogicalUnit` обёртка над keyword-хелпером | Да | Нет | `OK_INTERNAL_DETAIL` / `BASELINE_MUST_BE_LABELLED` |
| Выбор юнитов | `BootstrapUnitSelectorUnit`→`UnitSelector` | `ILogicalUnit` (selector) | Жёсткая if-цепочка по подстрокам (`RAG/context`) | `ILogicalUnit` обёртка; selector — внутренняя деталь, НЕ отдельный уровень | Да | Нет | `OK_INTERNAL_DETAIL`; при будущей claim «умного выбора» → нужен trained-юнит + ablation |
| Сборка ответа | `BootstrapRouteComposerUnit`→`HumanRouteComposer` | `ILogicalUnit` | `StringBuilder` шаблон | `ILogicalUnit` обёртка над шаблоном | Да | Нет | `OK_INTERNAL_DETAIL` |
| Верификация маршрута | `BootstrapOrchestrationVerifierUnit`→`OrchestrationVerifier` | `ILogicalUnit` | Булева конъюнкция непустых полей | `ILogicalUnit` обёртка | Да | Нет | `OK_INTERNAL_DETAIL` |
| Секвенс 6 шагов | `TaskOrchestrationUnit.OrchestrateAsync` | — | Прямой последовательный вызов 6 юнитов | по поведению — orchestration/flow-логика (НЕ реализует интерфейс) | Да | Нет | примечание: оркестрация шагов сейчас живёт в обычном классе, а не в `ILogicalOrchestrator`/`ILogicalFlow` (см. Пробелы) |
| Documentation pipeline flow | `ProjectDocumentationPipelineFlowAdapter` | `ILogicalFlow<string>` | Делегирование оркестратору + сбор метрик | `ILogicalFlow` (allowed level) | Да | Честный «runner_missing» | `OK_ALLOWED_LEVEL` (как scaffold; trained-вес не заявлен) |
| Training flow | `ProjectWikiTrainingFlowAdapter` | `ILogicalFlow<string>` | Делегирование training-оркестратору, Section-22 отчёт, `UNKNOWN` метрики | `ILogicalFlow` (allowed level) | Да | Нет ложной (антимаскарад соблюдён) | `OK_ALLOWED_LEVEL` |

Обоснование (правило скила): allowed-уровни — только `ILearningMachine/ILogicalOrchestrator/ILogicalFlow/ILogicalUnit/ILearningMachinePair`; regex/keyword/if-selector/шаблон/булев чек — «implementation detail unless wrapped by one of those levels». Здесь они корректно завёрнуты в `ILogicalUnit` и честно объявлены Bootstrap/untrained (A-LM-03), без претензии на semantic quality (`Confidence=1.0` фиксирован, не обучен). Это соответствует двум-PASS логике CLAUDE.md: сейчас это `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`, а НЕ `PASS_AS_TRAINED_FEATURE`. Скрытого детерминизма «под видом trained» нет — нет ни checkpoint/manifest, ни заявленных метрик в самих юнитах.

## Пробелы / что доисследовать

- НЕ открыт `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs` — точная регистрация flow-адаптеров (route id `project-documentation-pipeline`, `project_wiki_training_orchestrator`) и заявленный TD2 `project-wiki-orchestration-flow` + `BOOTSTRAP_FALLBACK` не подтверждены кодом (вне области 42; вне scope этой рекон-задачи).
- `ProjectWikiOrchestrationFlow` (TD2) и `ProjectWikiPluginOrchestrationAdapter.cs` присутствуют в Glob по 42, но в этой задаче не читались — стоит проверить, как plugin-оркестрация маршрутизирует через `ILogicalOrchestrator` и где fallback на `SubmitUserTaskApi`.
- `TaskOrchestrationUnit` как секвенсор 6 шагов не реализует `ILogicalOrchestrator`/`ILogicalFlow`. Для плана ILearningDynamicGraph это кандидат на явный orchestrator/flow-уровень (сейчас оркестрация — обычный класс; не нарушение, т.к. семантической claim нет, но для графа authority на рёбрах надо назвать явно).
- Лайфцикл trained-замены (load user/cache → repo/common fallback → export/import/continue-training) для этих 6 юнитов в коде НЕ виден; A-LM-03 описывает только слот подмены. Нужны DPO-генераторы именно для оркестрационных юнитов (в 42 уже есть генераторы для wiki-юнитов: `*DpoGenerator.cs`, `*TrainingRunner.cs`, `*ManifestPublisher.cs` — но для 6 Bootstrap-оркестрационных юнитов отдельных DPO/раннеров grep'ом не обнаружено — проверить).
- `OrchestrationKnnStore.cs`, `OrchestrationCorpusBuilder.cs`, `FlowReferenceDetector.cs`, `OrchestrationPlanOrchestrator.cs` (есть в Glob, не читались) — вероятные смежные узлы графа/ретривер-память; для DynamicGraph их роль (model component vs deterministic helper) нужно классифицировать отдельно (KNN — audit-triggered, не запрещён).
