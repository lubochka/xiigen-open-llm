# Codex Recon — core/26.TrainableRouter (обучаемая маршрутизация)

corpus: core | slug: core-26-router | relevance: high

## Что это и где (точные ключевые пути)

`26.TrainableRouter` — проект-исполняемый раннер (`TrainableRouter.csproj`,
`<OutputType>Exe`, есть `Program`-логика в раннерах), который маршрутизирует
запрос на «юнит» (например V309-оркестратор / перевод / документация .NET) и
готовит DPO-пары и метрики acceptance. Ключевые реально найденные файлы:

- `26.TrainableRouter/Contracts/IUnitScorer.cs` — контракт скоринга кандидатов.
- `26.TrainableRouter/Contracts/ITrainableRoutingFlow.cs` — наследует
  `ILogicalFlow<RoutingRequest>` (Score/Route/RecordDecision).
- `26.TrainableRouter/Contracts/IRoutingDecisionLog.cs` — лог решений.
- `26.TrainableRouter/Training/KeywordUnitScorer.cs` — фактический рантайм-скорер
  (`IUnitScorer`) + record `UnitScoringProfile`.
- `26.TrainableRouter/Training/TrainableRoutingFlow.cs` — флоу маршрутизации
  (`ITrainableRoutingFlow`): ранжирование, порог уверенности, score-gap,
  knowledge-edge.
- `26.TrainableRouter/Training/RoutingProfileTrainer.cs` — «обучение» профилей
  (сбор ключевых слов) + `RoutingUnitMetadataRegistry` / `UnitRoutingMetadata`.
- `26.TrainableRouter/Training/RoutingTrainingRunner.cs` — train→eval раннер
  (accuracy) + сборка DPO-пар.
- `26.TrainableRouter/Training/RoutingDpoPairBuilder.cs` — DPO-пары из решений.
- `26.TrainableRouter/Training/OrchestrationScenarioPhasePlanner.cs` — планировщик
  фаз по ключевым словам (10 правил).
- `26.TrainableRouter/Training/OrchestrationAcceptanceGateEvaluator.cs` — измерение
  качества (routing accuracy + phase coverage).
- `26.TrainableRouter/Training/OrchestrationPromptDpoPairBuilder.cs` — DPO-пары для
  prompt-файла (шаблонные строки chosen/rejected).
- `26.TrainableRouter/Models/RoutingModels.cs` — records (`RoutingRequest`,
  `UnitScoringResult`, `RoutingFlowThresholds`, `RoutingDecision`, `RoutingDpoPair`,
  `RoutingTrainingResult`).
- `26.TrainableRouter/Training/RoutingTrainingModels.cs` — records обучения/eval.
- `26.TrainableRouter/Training/OrchestrationRoutingCurriculum.cs`,
  `DistributedRoutingBootstrap.cs`, `DistributedModelOrchestratorFlow.cs`,
  `OrchestrationTaskDecompositionFlow.cs`, `CodeDocumentationRoutingFlow.cs`,
  `CodeDocumentation*Runner.cs` — curriculum, статические `RemoteUnitManifest`,
  distributed-bootstrap и док-раннеры (читал по grep, не целиком).

## Как устроено / работает (кратко)

Поток (рантайм): `RoutingRequest` (prompt, taskSlice, requiredDirection,
domainHint, candidateUnitIds) → `TrainableRoutingFlow.RouteAsync` →
`IUnitScorer.ScoreAsync` → ранжирование по `Score` (desc) → проверка
`MinimumRoutingConfidence` (0.40) и `MinimumScoreGap` (0.05) →
либо `RoutingDecision` с выбранным юнитом, либо «knowledge-edge»
(отказ маршрутизировать с reason-кодом).

Скоринг (`KeywordUnitScorer.Score`): конкатенирует
prompt+taskSlice+requiredDirection+domainHint в нижний регистр, считает
`hits = profile.Keywords.Count(k => text.Contains(k))`, добавляет
`domainBoost=20` при совпадении `DomainHint==DomainTag`, `directionBoost=2`,
`BaseScore`. `confidence` — арифметическая формула от тех же hits/boost,
зажатая в [ConfidenceFloor, 0.99]. Это чистый подсчёт совпадений ключевых слов.

«Обучение» (`RoutingProfileTrainer.Train`): из примеров берёт `ChosenUnitId` и
извлекает токены (split, stop-words, len≥3) из prompt+taskSlice+direction+hint;
строит `UnitScoringProfile` = множество ключевых слов + статическая метадата из
`RoutingUnitMetadataRegistry`. Никаких весов/градиентов/чекпойнтов: «модель» —
это словарь `unitId → set<keyword>`.

Измерение качества: `RoutingTrainingRunner.TrainAndEvaluateAsync` →
accuracy = correct/eval; `OrchestrationAcceptanceGateEvaluator.EvaluateAsync` →
routingAccuracy + phaseCoverage (через `OrchestrationScenarioPhasePlanner`,
который тоже сопоставляет фразы по 10 keyword-правилам).

DPO: `RoutingDpoPairBuilder` и `OrchestrationPromptDpoPairBuilder` собирают
chosen/rejected как форматированные строки (`Route to <unit>...`, rejected с
`[WRONG: ...]`). Это генерация ДАТАСЕТА DPO, а не обучение на нём.

## Релевантность к ILearningDynamicGraph

Высокая — это самый прямой существующий прототип «обучаемой маршрутизации /
оркестрации» в репозитории и естественный источник контрактов и анти-паттернов:

- Граф/рёбра: `RoutingRequest.candidateUnitIds` → выбор одного юнита = ребро
  маршрутизации; `OrchestrationScenarioPhasePlanner` даёт упорядоченные фазы
  (цепочка узлов). Для `ILearningDynamicGraph` это кандидат на ребро
  «orchestrator→unit» и на узлы-фазы, но сейчас authority детерминирована.
- DPO/обучение: уже есть форма пар (`RoutingDpoPair`, `OrchestrationPromptDpoPair`
  с `chosen/rejected/[WRONG]`) и поля `RewardMargin/DpoAccuracy/Promoted`
  (`RoutingTrainingResult`) — но это пустые контрактные поля, реального DPO-цикла
  нет. Это готовый формат для построения настоящих DPO-наборов авто-построителем.
- Измерение: acceptance-gate (routing accuracy + phase coverage) — переиспользуемая
  заготовка метрик качества графа/маршрутизации.
- Teacher-provenance: НЕ найдено никакого teacher/distillation-механизма (нет
  внешней модели-учителя, нет request_id/model_version) — поля provenance из
  Legal-Teacher-гейта в этом проекте отсутствуют; «chosen» в DPO-парах
  генерируется детерминированным шаблоном из ожидаемого юнита/фаз (self-labeling),
  а не учителем. Для плана это пробел: авто-построение DPO нуждается в источнике
  истинности.
- Манифесты/локатор: `RemoteUnitManifest`/`IRemoteUnitLocator` используются как
  СТАТИЧЕСКИЕ дескрипторы (sha из строковых литералов, endpoints) — не чекпойнты
  обученной модели.

## Классификация (по trainable-architecture-decision-SKILL.md)

| Механизм (файл/класс/метод) | Заявка | Что реально делает | Уровень / классификация | Вердикт |
|---|---|---|---|---|
| `KeywordUnitScorer.Score` | trainable scorer | `text.Contains(keyword)` подсчёт + domain/direction boosts → score/confidence | внутренний детерминистический хелпер (keyword counter / scorer), НЕ allowed level; рантайм-authority выбора юнита | `FORBIDDEN_RUNTIME_SHORTCUT` (детерминизм как финальная authority там, где заявлен trainable router) |
| `RoutingProfileTrainer.Train` | обучение профилей | сбор множества ключевых слов из chosen-примеров; нет весов/loss/чекпойнта | детерминистический сборщик словаря; не обучение | `REPORT_CLAIM_TOO_STRONG` (это keyword-harvest, не learning) |
| `TrainableRoutingFlow` (`ITrainableRoutingFlow : ILogicalFlow`) | trainable routing flow | оборачивает скорер, пороги, score-gap, knowledge-edge | контракт уровня `ILogicalFlow` присутствует (структурно корректный уровень), НО внутренняя authority — детерминированный скорер | уровень флоу OK как обёртка; нужен обучаемый `IUnitScorer`/`ILogicalUnit` внутри |
| `OrchestrationScenarioPhasePlanner.PlanPhases` | планирование фаз | keyword-правила (10 шт.) → набор фаз | детерминистический keyword-matcher; фактически решает структуру графа фаз | `NEEDS_ILOGICALFLOW_OR_ORCHESTRATOR_WRAPPER` / `FORBIDDEN_RUNTIME_SHORTCUT` если станет рантайм-authority графа |
| `OrchestrationAcceptanceGateEvaluator.EvaluateAsync` | измерение качества | accuracy + phase coverage над флоу | eval-helper (корректно как измерение) | `OK_INTERNAL_DETAIL` (eval-only) |
| `RoutingDpoPairBuilder` / `OrchestrationPromptDpoPairBuilder` | DPO | строит chosen/rejected строки шаблоном из ожидаемого юнита/фаз | генератор датасета; chosen = self-label по ожиданию (без teacher) | dataset-build OK как заготовка; источник «правильного» — детерминированный, риск teaching-shortcut → проверить на `DATASET_REBUILD_REQUIRED` |
| `RoutingTrainingResult.RewardMargin/DpoAccuracy/Promoted` | метрики DPO-тренировки | поля record; нигде не вычисляются реальным DPO-циклом | контрактные плейсхолдеры | `REPORT_CLAIM_TOO_STRONG` (нельзя выдавать за обученную модель) |
| `RemoteUnitManifest` (curriculum/bootstrap/runners) | манифест модели | статические дескрипторы (литеральные sha/endpoint) | provenance/descriptor, НЕ чекпойнт | `OK_INTERNAL_DETAIL` при честной маркировке |

Скрытый детерминизм: ДА, и он системный. Весь рантайм-путь маршрутизации
(`KeywordUnitScorer` + score-gap/confidence пороги) и планирования фаз
(`OrchestrationScenarioPhasePlanner`) — детерминированный подсчёт ключевых слов,
поданный под именами «Trainable…/Training/DPO». В терминах скила: «обучаемой
архитектуры» (allowed level c lifecycle) тут нет — есть честная заготовка
контрактов `ILogicalFlow` поверх детерминированного хелпера, плюс генератор
DPO-датасета и eval-гейт. Это `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` ТОЛЬКО если
отчёты явно говорят «детерминированный baseline / scaffold»; любое утверждение
«обученный роутер/модель» = masquerade.

## Lifecycle-доказательства (по чек-листу скила) — отсутствуют

Подтверждено grep'ом по всему проекту: НЕТ `checkpoint`, `ExportAsync`,
`ImportAsync`, `IModelRepository`, `ILearningMachine`, реального DPO-обучения,
`TensorCore`/градиентов в исходниках (только в `bin/obj` как транзитивные DLL).
Нет fresh-load, continue-training, ablation/no-shortcut тестов в самом проекте.
Значит по `trainable-architecture-decision-SKILL.md` (раздел lifecycle) ни один
механизм не может claim «trained feature».

## Пробелы / что доисследовать

1. Где (вне 26) реально живёт DPO-обучение/`ILearningMachine`/чекпойнты —
   проверить `DPO/`, `TrainingLoop/`, `25.ModelServer`, `15.OrchestratorModel`
   (DLL подтягиваются, но обучающий цикл здесь не вызывается).
2. Регистрируется ли `TrainableRoutingFlow`/`ITrainableRoutingFlow` в
   `15.OrchestratorModel/Logical/` как flow-adapter (требование Trainable Unit
   pattern) — не проверял реестр.
3. Источник истинности `chosen` в DPO-парах: сейчас self-label по ожидаемому
   юниту/фазам (детерминированно). Для ILearningDynamicGraph нужен легальный
   teacher/provenance (gate из скила) — сейчас не покрыт.
4. Тесты проекта (где `RoutingTrainingRunner`/acceptance-gate реально
   прогоняются и какие пороги/числа считаются «качеством») — не открывал
   тестовые файлы.
5. `RewardMargin/DpoAccuracy/Promoted` — найти, есть ли вообще код, который их
   заполняет, или они навсегда контрактные плейсхолдеры.
6. `IRoutingDecisionLog`/`InMemoryRoutingDecisionLog` — персистентность решений
   для онлайн-дообучения графа (читал контракт, не реализацию полностью).
