# Codex Recon — Расширение оркестратора (orchestrator extension)

Назначение: evidence для ПЛАНА ILearningDynamicGraph (авто-построение DPO для
ILearningUnit / ILearningFlow / ILearningOrchestrator / ILearningDynamicGraph).
Это рекон-сбор, НЕ разрешение на реализацию.

ШАГ 1 (скил-правила, применены при классификации):
- `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md` найден и прочитан (частично, v27.5).
  Ключевые правила для классификации: разрешённые уровни обучаемой архитектуры —
  `ILearningMachine` / `ILogicalOrchestrator` / `ILogicalFlow` / `ILogicalUnit` /
  `ILearningMachinePair`; scorer/selector/classifier/router/KNN/dictionary/regex/
  BM25/lookup/counter — это детерминистические хелперы, а НЕ архитектурный уровень
  (могут быть только деталью реализации внутри разрешённого уровня). Для
  graph/DynamicGraph-планов требуется: roles/hierarchy, nodes/edges, input/output
  контракты, many-to-many, fan-out/fan-in, deterministic-vs-trainable authority
  per edge, проверка скрытых детерминистических шорткатов на рёбрах. Для DPO/teacher
  output обязательны provenance-поля (teacher_access_mode, hosting_provider,
  model_id, model_license, provider_terms_checked и т.д.); Claude/OpenAI/Gemini как
  distillation-teacher запрещены.

## Что это и где (точные ключевые пути)

Область: `C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator extension/` —
набор протоколов/планов (не код .cs; код живёт в `C:/Projects/llm_mvp_core`).
Это архитектура и тренировочные протоколы локального wiki-оркестратора над кодом.

Самые релевантные прочитанные файлы:
- `orchestrator wiki.drawio.xml` — каноническая UML-диаграмма всего пайплайна
  (узлы и рёбра: Main IOrchestrator -> DocumentationOrchestrator -> Documentation
  -> Question Seeds -> Wiki DPO Pairs + Held-out QA -> ProjectLocalWikiOrchestrator;
  ветка вопросов: Question -> Main IOrchestrator -> ProjectWikiRoutingFlow ->
  ProjectLocalWikiOrchestrator -> ProjectWikiQueryPlanningFlow -> WikiContextRetriever
  -> ProjectEvidenceIndex(Mode3 BM25) / GraphRag.HybridRetriever(Mode2) -> EmbeddingModel).
- `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md` — главный архитектурный план (статус
  REFERENCE — DO NOT EXECUTE): уровни L0..L4, паттерн модели как
  `Main IOrchestrator -> DocumentationOrchestrator : ILogicalOrchestrator -> ...
  -> ProjectWikiRoutingFlow : ILogicalFlow -> ProjectLocalWikiOrchestrator :
  ILogicalOrchestrator -> ProjectWikiQueryPlanningFlow : ILogicalFlow ->
  WikiContextRetriever -> ProjectLocalWikiModelUnit -> ProjectAnswerVerifier`.
  DTO документации/relation-графа (documentation-units / call-edges /
  value-flow-edges jsonl), DPO negatives, manifest (domain-manifest.json,
  model-manifest.json), интеграция с 25.ModelServer / 26.TrainableRouter.
- `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md` — контракт обучаемого flow
  `ProjectWikiQueryPlanningFlow : ILogicalFlow<ProjectWikiUserQuestion,
  ProjectWikiQueryPlan>`: интенты, типы query-step, sufficiency rules, DPO-пары
  (chosen = многошаговый план, rejected = single lookup + `[WRONG: ...]`), метрики.
- `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` — каноничный список из 11 обучаемых wiki-
  юнитов с ответственностью, минимальными метриками и remediation; таксономия
  developer-вопросов L0..L5; 50-строчные DPO-батчи; WikiQueryTrace / WikiUnitTrace.
- `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` — обязательная отчётность
  раннера (raw accuracy, reward margin, loss, CPU/RAM/время, per-row/per-50), gate
  на персист JSON-артефакта до отчёта, Source Authority Gate (ACCEPT/QUARANTINE/
  BLOCK по лицензии) и список безопасных внешних источников (MIT/Apache-2.0/CC-BY).
- `DISTRIBUTED-MODEL-ARCHITECTURE-PLAN-v1.md` (статус SUPERSEDED, интегрирован в
  Phase 37 / ORCHESTRATION-MACHINE-TRAINING-PLAN-v20): паттерн `IOrchestrator ->
  ILogicalFlow -> ILogicalUnit`; routing = обучаемый `ITrainableRoutingFlow :
  ILogicalFlow<RoutingRequest>` (НЕ статическая таблица); RemoteUnitManifest,
  IModelRepository, IRemoteUnitLocator; routing-решения как DPO-evidence
  (RoutingDecisionRecord: chosen/rejected + reason + WrongRouteConsequence).

Соседние (не читались детально, но релевантны для доисследования): множество
`USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v1..v8.md`,
`REAL-CHAT-ORCHESTRATION-CHAINS-v1/v2.md`, `orchestrator prompts.txt`,
`PROJECT-LOCAL-WIKI-MODEL-STATE.json`, сессии SESSION-01..06,
`base-wiki-model-manifest.json`, `reports/ORCHESTRATOR-EXACT-PROMPTS-ACTUAL-RESULTS-v1.md`.

## Как устроено / работает (кратко)

Двухфазный пайплайн «документация -> локальная wiki-модель проекта»:
- Phase 1 (зелёный блок диаграммы): запрос «сделать документацию» -> Main
  IOrchestrator -> DocumentationOrchestrator читает код -> bottom-up документация
  + relation-граф (call-edges, value-flow-edges) -> NegativeDtoDocumentationOrchestrator
  генерирует/верифицирует negative DPO -> обучение локального doc-адаптера.
- Phase 2 (красный блок): «вопрос про код» -> Main IOrchestrator ->
  ProjectWikiRoutingFlow (L0 — нужен ли этот проектный wiki) -> ProjectLocalWikiOrchestrator
  -> ProjectWikiQueryPlanningFlow строит многошаговый план evidence-запросов ->
  WikiContextRetriever исполняет план (Mode 3 BM25 через SearchEngineFacade /
  Mode 2 семантика через GraphRag.HybridRetriever + EmbeddingModel) ->
  ProjectLocalWikiModelUnit отвечает -> ProjectAnswerVerifier проверяет цитаты/границы.
Обучение: DPO-батчи строго по 50 строк (50 train / 50 held-out), chosen != rejected,
обязательная `[WRONG: ...]` аннотация; метрики только из раннера, с персистом JSON.
Модели — отдельные загружаемые юниты с манифестом, публикация через IModelRepository,
локация через IRemoteUnitLocator (паттерн 25.ModelServer / 26.TrainableRouter).

## Релевантность к ILearningDynamicGraph

ОЧЕНЬ ВЫСОКАЯ — это фактически референс-реализация графа обучаемых юнитов/флоу/
оркестраторов, который ILearningDynamicGraph должен авто-обучать через DPO:

- Граф (nodes/edges): диаграмма drawio + раздел «Design Pattern» дают готовые узлы
  (оркестраторы/флоу/юниты) и рёбра (route, plan, execute, retrieve, answer, verify);
  relation-граф (documentation-units / call-edges / value-flow-edges) — пример
  графа данных с многошаговой propagation (callee meaning + params + return ->
  caller). Это прямой источник many-to-many / fan-out / fan-in рёбер.
- DPO / обучение: и routing (RoutingDecisionRecord -> DPO), и query-planning
  (chosen=план, rejected=single lookup), и documentation negatives дают готовые
  схемы авто-построения DPO-троек по рёбрам графа. Формат: A-side / chosen /
  rejected + `[WRONG: consequence]`; типизированные negative-таксономии.
- Оркестрация: явная иерархия `IOrchestrator -> ILogicalFlow -> ILogicalUnit`
  (и вложенность TopOrchestrator -> RouterFlow -> Unit) — совпадает с целевыми
  уровнями ILearning* плана.
- Измерение: жёсткие per-unit метрики (11 юнитов) + WikiQueryTrace / WikiUnitTrace
  (routed_units[], unit_results[], failure_type, remediation_action) — готовый
  шаблон measurement/learning-signal per node/edge для DynamicGraph.
- Teacher-provenance: Source Authority Gate (ACCEPT/QUARANTINE/BLOCK по лицензии),
  список MIT/Apache/CC-BY источников, и (через скил) запрет Claude/OpenAI/Gemini как
  distillation-teacher — прямой источник правил teacher_access_mode / model_license
  для авто-DPO.

## Классификация уровней (по правилам скила)

Замечание: в этой области нет .cs-кода — это контракты/планы. Классификация дана по
ОБЪЯВЛЕННЫМ контрактам и по тому, что правило скила требует/запрещает.

Разрешённые уровни (объявлены явно как обучаемые):
- `DocumentationOrchestrator`, `NegativeDtoDocumentationOrchestrator`,
  `ProjectLocalWikiOrchestrator` -> уровень ILogicalOrchestrator. ОК (учат адаптер,
  имеют DPO/манифест/lifecycle).
- `ProjectWikiRoutingFlow`, `ProjectWikiQueryPlanningFlow`, `ITrainableRoutingFlow`,
  `TrainableQuestionBankFlow` -> уровень ILogicalFlow<TInput>. ОК — план явно
  требует, чтобы routing/planning были ОБУЧАЕМЫМИ флоу с DPO, чек-поинтом и
  метриками, а НЕ статической таблицей (DISTRIBUTED-...: «RouteAsync is not a
  lookup table», static routing — только bootstrap fallback).
- `ProjectLocalWikiModelUnit`, 11 wiki-юнитов (CodeTraceBuilder, EntityImpactMapper,
  DocumentationGenerator, NegativeDpoGenerator, WikiAnswerComposer и т.д.) ->
  уровень ILogicalUnit / обучаемый юнит. ОК при наличии train/held-out/DPO/метрик
  (план это требует для каждого).
- `StackAndVersionClassifier`, `WikiContextRoutingUnit`, `ProjectWikiRequestRouter`,
  `ProjectAnswerVerifier` -> ОБЪЯВЛЕНЫ как обучаемые юниты, НО имена (classifier/
  router/verifier) — это именно категории, которые скил относит к детерминистическим
  хелперам. Допустимы как ILogicalUnit ТОЛЬКО при доказанном lifecycle (typed
  trained state, training update, checkpoint/manifest, export/import, fresh-load,
  continue-training, ablation/leakage gate). Иначе это скрытый детерминистический
  шорткат, маскирующийся под обучаемый юнит -> по правилу скила требуется downgrade
  или обёртка + lifecycle-evidence. Для плана ILearningDynamicGraph это рёбра, где
  надо явно зафиксировать deterministic-vs-trainable authority.

Детерминистические хелперы (НЕ архитектурный уровень — только деталь реализации,
eval/helper/internal baseline):
- `SearchEngineFacade` (12.SearchEngine, BM25, Mode 3) — план сам помечает как
  «lexical BM25 only», для audit/citation/evidence-id recovery. Это детерминистический
  лексический lookup. По R4 / trainable-architecture-decision НЕ может быть заменой
  обучаемого юнита; допустим как evidence/validation путь.
- `ProjectEvidenceIndex` — внутренняя зависимость WikiContextRetriever над BM25;
  детерминистический индекс/lookup, не уровень.
- `GraphRag.HybridRetriever` + `EmbeddingModel` (07/09) — retrieval-механизм; сам по
  себе retriever (по списку скила) не архитектурный паттерн. Обучаемость — у
  EmbeddingModel, но в этом пайплайне он используется как зависимость retrieval.
- `WikiTraceAggregator` — агрегатор трейса/метрик и выбор следующего батча: это
  диагностика/provenance/counter, НЕ обучаемая authority. Должен оставаться
  eval/telemetry, не «финальный решатель».

Замечание-риск для будущего плана: query-step-типы и negative-таксономии заданы как
фиксированные перечисления (enums). Если исполнитель реализует выбор шага/негатива
через regex/keyword/dictionary, это будет скрытый детерминизм под видом обучаемого
flow — план ILearningDynamicGraph должен это явно запретить и потребовать обучаемый
выбор с ablation/leakage-gate per edge.

## Пробелы / что доисследовать

1. Реальный код в `C:/Projects/llm_mvp_core` (особенно 42.ProjectWikiDomain,
   15.OrchestratorModel/Logical, 25.ModelServer, 26.TrainableRouter, 00.Core
   LearningMachineContracts): здесь только контракты/планы — нет подтверждения, что
   юниты действительно обучаемые, а не Bootstrap-обёртки над статикой. Нужно проверить
   фактический уровень и наличие checkpoint/manifest/export/import.
2. Нет упоминания именно `ILearningDynamicGraph` / `ILearningUnit` / `ILearningFlow` /
   `ILearningOrchestrator` в прочитанных файлах — здесь термины `ILogicalUnit/Flow/
   Orchestrator` и `ILearningMachine`. Нужно сопоставить нейминг целевого плана с
   существующей иерархией (возможно, DynamicGraph — новый Level над оркестратором).
3. `PROJECT-LOCAL-WIKI-MODEL-STATE.json` и SESSION-01..06 не читались — там фактический
   статус/прогресс обучения и какие юниты уже >=0.75 / promoted.
4. `orchestrator prompts.txt`, `USER-ORCHESTRATION-SCENARIOS-...v1..v8`,
   `REAL-CHAT-ORCHESTRATION-CHAINS-v1/v2` — источник реальных цепочек оркестрации
   (fan-out / many-to-many рёбра) для DPO; не инвентаризированы.
5. Teacher-provenance: в раннер-отчёте есть Source Authority Gate, но не видно
   конкретных полей teacher_access_mode/model_id/model_license в DPO-строках —
   нужно проверить фактический формат DPO-jsonl на наличие provenance.
6. `reports/ORCHESTRATOR-EXACT-PROMPTS-ACTUAL-RESULTS-v1.md` — фактические
   результаты прогона (доказательство runtime vs только контракты) не проверены.
