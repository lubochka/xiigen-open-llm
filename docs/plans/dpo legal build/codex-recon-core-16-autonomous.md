# Codex Recon — 16.AutonomousLearning как прецедент авто-DPO

Корпус: `core` · slug: `core-16-autonomous` · relevance: **medium**
Вопрос: автономный цикл обучения как прямой прецедент авто-DPO (модель сама находит слабые места и учится).

---

## 1. Что это и где (точные ключевые пути)

`16.AutonomousLearning` — компилируемый subproject (`AutonomousLearning.csproj`),
реализующий **8-шаговый ночной цикл само-обучения** доменной LoRA-модели, когда
машина простаивает. Это ближайший в репозитории работающий прецедент
«модель улучшает себя по циклу с измерением до/после», но **на DPO он сегодня не
опирается** (см. §4, §6).

Ключевые файлы (все реально найдены):
- `16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs` — оркестратор всего цикла (8 шагов), `RunAsync(domain, ct) -> OperationResult<CycleResult>`.
- `16.AutonomousLearning/Core/Interfaces.cs` — контракты: `ISystemMonitor`, `IDataSourceFinder`, `ITrainingDataProvider`, `IAccuracyEvaluator`, `IAuLearningStorage`; рекорды `SystemStatus`, `SourceEntry`, `CycleResult`, enum `TrustLevel`.
- `16.AutonomousLearning/IdleDetector/IdleDetector.cs` — E41 4-условный гейт простоя (CPU/ввод/батарея/диск).
- `16.AutonomousLearning/DataSourceFinder/DataSourceFinder.cs` — список .NET-источников по тирам (захардкоженные Tier-1 dotnet/* и MS Docs).
- `16.AutonomousLearning/TrustScorer/TrustScorer.cs` — взвешенный детерминированный скоринг доверия источника + реализация `AgentInfrastructure.Logical.ITrustScorer`.
- `16.AutonomousLearning/Preprocessing/DatasetWeighter.cs` — формула D-09 (class-imbalance), `DatasetPreprocessor.cs` (E20 StackDetector gate, не читался построчно).
- `16.AutonomousLearning/Training/AutonomousTrainer.cs` — обёртка над `IIncrementalTrainer` (из 14) + replay; **сигнатура уже принимает `IReadOnlyList<DpoPair> dpoPairs`**.
- `16.AutonomousLearning/Training/ReplayBuffer.cs` — E43 70/30 new/old anti-forgetting.

Опора на чужие сборки (по bin/obj и `using`): `14.ContinuousLearning`
(`IIncrementalTrainer`, `IDomainModelRegistry`, `TrainingExample`, `DpoPair`,
`ModelAdapter`), `21.AgentInfrastructure` (`ISessionStorage`, `BaseService`,
`OperationResult<T>`, `AgentInfrastructure.Logical.ITrustScorer`).

---

## 2. Как устроено/работает (кратко)

`AutonomousLearningPipeline.RunAsync` (Pipeline/AutonomousLearningPipeline.cs:67):
1. **Шаг 1 — гейт простоя:** `_systemMonitor.Sample()` -> `_idleDetector.Check(status)`; не `SafeToTrain` => `Fail("SYSTEM_ACTIVE")`.
2. **Шаг 2 — baseline:** `_evaluator.Evaluate(domain)` (реальный LoRA-скоринг; cold start = 0.0).
3. **Шаг 3 — сбор данных:** `_dataProvider.CollectAsync` (обёртка DataSourceFinder -> TrustScorer -> DatasetPreprocessor).
4. **Шаг 4 — взвешивание:** D-09 через `DatasetWeighter.Weight`.
5. **Шаг 5 — тренировка:** `_trainer.TrainAsync(domain, weighted, [])` — replay-mix 70/30, затем `IIncrementalTrainer` (LoRA rank=8/alpha=16).
6. **Шаг 6 — enhanced:** повторный `_evaluator.Evaluate(domain)`.
7. **Шаг 7 — промоушен по гейту:** если `enhanced <= baseline` => `Fail("ACCURACY_NOT_IMPROVED")`, иначе `_registry.Register(adapter)`.
8. **Шаг 8 — `CycleResult`** (baseline/enhanced/adapterPath/examples).

Замкнутый контур «собрать -> взвесить -> дообучить -> измерить -> повысить
только при улучшении» с anti-forgetting и аппаратным гейтом. Это и есть
прецедентная механика для авто-улучшения.

---

## 3. Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

**Обучение/измерение (сильно релевантно):** готовый паттерн контролируемого
self-improvement: before/after-метрика + промоушен только при дельте > 0 (шаги
2/6/7). Для ILearningDynamicGraph это прямой образец «как доказывать, что
авто-построенные DPO реально улучшают узел», а не просто сгенерированы. Гейт
`ACCURACY_NOT_IMPROVED` — кандидат на ablation/«не-shortcut» доказательство.

**DPO (частично, но это главный пробел-возможность):** канал DPO **физически
проложен** — `AutonomousTrainer.TrainAsync(..., IReadOnlyList<DpoPair> dpoPairs)`
и проброс в `_trainer.TrainAsync(domain, mixedBatch, dpoPairs)`
(Training/AutonomousTrainer.cs:44,61). Но pipeline вызывает его с **пустым `[]`**
(Pipeline/AutonomousLearningPipeline.cs:120) — т.е. **авто-генерации DPO-пар тут
нет**. Это «розетка без вилки»: место для авто-DPO определено, но «модель сама
находит слабые места» здесь не реализовано (нет анализа ошибок -> chosen/rejected).

**Граф/оркестрация (слабо):** это линейный конвейер (`BaseService`, прямые
ссылки на конкретные классы `IdleDetector`/`DatasetWeighter`/`AutonomousTrainer`),
**не** узел/ребро ILearningDynamicGraph и **не** один из разрешённых уровней
ILogicalUnit/Flow/Orchestrator/Machine. Полезен как **референс данных-потока для
одного узла обучения**, а не как граф.

**Teacher-provenance (отсутствует — важно для legal DPO):** «учитель»
качества — `IAccuracyEvaluator` против фикс. reference-набора и
детерминированный `TrustScorer` источника. Внешнего teacher-LLM нет, провенанс
лицензий источников **не фиксируется** (есть только `TrustLevel`/`Tier`).
Источники захардкожены как dotnet/* и docs.microsoft.com (DataSourceFinder.cs:15-27)
— это кандидаты Apache-2.0/MIT/док-лицензий, но без полей лицензии/URL-провенанса,
которых требует Legal Teacher/Provenance Gate скила.

---

## 4. Классификация кода (по `trainable-architecture-decision-SKILL.md`)

| Компонент | Файл | Механизм | Уровень или внутр. хелпер | Вердикт |
|---|---|---|---|---|
| `AutonomousLearningPipeline` | Pipeline/...:19 | линейная оркестрация 8 шагов через `BaseService` + конкретные классы | **НЕ разрешённый уровень**; по роли это оркестрация, но реализована как `BaseService`-сервис, а не `ILogicalOrchestrator`/`ILogicalFlow` | `NEEDS_ILOGICALFLOW_OR_ORCHESTRATOR_WRAPPER` (если переносить в граф/иерархию) |
| `AutonomousTrainer` | Training/...:10 | обёртка над `IIncrementalTrainer` + replay; **есть DPO-параметр** | внутренняя деталь шага обучения; обучение делегировано в 14 | `OK_INTERNAL_DETAIL` (тренировочный delegат, не самостоятельный trainable-уровень) |
| `IAccuracyEvaluator` | Core/Interfaces.cs:66 | оценка LoRA против reference set, `double [0,1]` | измерительный контракт; **eval-only** authority | `KEEP_FOR_EVAL_ONLY` по роли; для графа — образец оценочного узла |
| `TrustScorer` | TrustScorer/...:22 | **детерминированный взвешенный скоринг** (фикс. веса 0.30/0.20/0.15/0.35/0.10 + пороги 0.70/0.30) | реализует `AgentInfrastructure.Logical.ITrustScorer`, но внутри — фикс. формула, **не обучается** | `BASELINE_MUST_BE_LABELLED` / `OK_INTERNAL_DETAIL`: «scorer» по имени, но это детерминированный хелпер; как trainable-authority НЕ годится без обучаемого состояния |
| `DatasetWeighter` | Preprocessing/...:11 | формула D-09 (арифметика) | детерминированный хелпер | `OK_INTERNAL_DETAIL` |
| `IdleDetector` | IdleDetector/...:20 | 4 пороговых if (CPU/ввод/батарея/диск) | детерминированный гейт ресурсов | `OK_INTERNAL_DETAIL` (boundary-логика, не семантика) |
| `DataSourceFinder` | DataSourceFinder/...:10 | возврат статического списка источников | детерминированный хелпер | `OK_INTERNAL_DETAIL`; для legal-DPO — добавить поля лицензии/провенанса |
| `ReplayBuffer` | Training/...:8 | случайная подвыборка 70/30 | детерминированный/стохастический хелпер (anti-forgetting) | `OK_INTERNAL_DETAIL` |

**Главное по правилу скила:** имя `TrustScorer` подпадает под триггер
«scorer/selector», но фактический механизм — **фиксированная формула без
обученного состояния** (TrustScorer.cs:25-33,82-95). По §«Decision Procedure» это
`internal detail / baseline`, а не разрешённый trainable-уровень. Если в
ILearningDynamicGraph доверие/отбор источника станет обучаемой authority на
ребре, потребуется обёртка в `ILogicalUnit` с типизированным обучаемым
состоянием и lifecycle (load user->repo->fresh, export/import, continue-training)
— иначе это `FORBIDDEN_RUNTIME_SHORTCUT` на ребре.

---

## 5. Прямой прецедент авто-DPO: что переиспользуемо, чего нет

Переиспользуемо как образец:
- **Контур self-improvement с гейтом дельты** (шаги 2/6/7) — модель проверяемого «учится и доказывает улучшение».
- **DPO-канал в обучении уже есть** (`DpoPair` проброшен до `IIncrementalTrainer`).
- **Anti-forgetting (ReplayBuffer 70/30)** и **аппаратный гейт простоя** — готовые операционные кирпичи для фонового авто-обучения.

Чего нет (это и есть дельта до «авто-DPO» в смысле задачи):
- **Нет авто-обнаружения слабых мест** и генерации `chosen/rejected` — `dpoPairs` всегда `[]` (Pipeline/...:120). «Модель сама находит ошибки» здесь не реализована.
- **Нет teacher/provenance-полей** (лицензия/URL/model_id) — Legal Teacher Gate не покрыт.
- **Нет графовой структуры** (узлы/рёбра, fan-in/out, multi-output edge contracts) и нет привязки к разрешённым уровням иерархии.

---

## 6. Пробелы / что доисследовать

1. `Preprocessing/DatasetPreprocessor.cs` и `TrustScorer/TrustScorer.cs` соседи — не прочитан **DatasetPreprocessor** построчно (E20 StackDetector gate, фактическая разметка `TrainingExample`). Нужно для оценки качества входа в DPO.
2. **`14.ContinuousLearning`** — где реально живут `DpoPair`, `IIncrementalTrainer`, `IDomainModelRegistry`, как DPO-пары применяются в LoRA и как считается reward margin. Это центральный вопрос для авто-DPO; здесь только проброс.
3. Реальные реализации `ISystemMonitor`/`ITrainingDataProvider`/`IAccuracyEvaluator` (в этом проекте только интерфейсы + предполагаемые тест-стабы) — есть ли production-реализация, или цикл существует только под тесты.
4. Регистрация в `15.OrchestratorModel`/локаторе: subproject 16 **не** найден как `ILogicalFlow`-адаптер; подтвердить, что он вне иерархии ILearningMachine и не вызывается из оркестратора (для решения, переносить ли его логику в граф).
5. Провенанс/лицензии источников (`SourceEntry` не хранит лицензию) — обязательная доработка, если выход 16 пойдёт в общий DPO-корпус.