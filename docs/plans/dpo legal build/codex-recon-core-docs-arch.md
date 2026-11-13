# Codex Recon — core / docs / ARCHITECTURE.md (§ иерархия ILearningMachine + диаграмма компонентов)

Corpus: core · Slug: core-docs-arch · Дата: 2026-06-23
Роль: рекон-аналитик (только чтение + запись этого файла). Сбор evidence для ПЛАНА ILearningDynamicGraph. Не разрешение на реализацию.

ВАЖНО (несоответствие нумерации в задаче): вопрос ссылается на «§9 иерархия ILearningMachine + диаграмма компонентов». В реальном файле `docs/ARCHITECTURE.md` (486 строк) §9 — это «BaseService Pattern». Иерархия ILearningMachine находится в **§10** (строки 435–486). Диаграмма компонентов/зависимостей — в **§1** (строки 16–63). Иерархическая mermaid-диаграмма уровней — внутри §10 (строки 453–466). Дальше отчёт привязан к реальным секциям, а не к номеру из вопроса.

---

## 1. Что это и где (точные ключевые пути)

- `C:/Projects/llm_mvp_core/docs/ARCHITECTURE.md` — высокоуровневая карта системы. Релевантные секции:
  - §1 «Subproject Dependency Graph» (стр. 16–63) — mermaid `graph TD`: диаграмма компонентов (00.Core, 00.TensorCore, 15.OrchestratorModel, 42.ProjectWikiDomain, 27.TranslationDomain, 43.SmartSkillsDomain и др.) и их связи.
  - §10 «ILearningMachine<TDomain> — Trainable Machine Hierarchy (TD3)» (стр. 435–486) — 5 уровней, правило A-LM-02, mermaid-граф уровней, таблица решений A-LM-01..A-LM-11, таблица Level-5 имплементоров.
  - §4.2 (стр. 178–208) — `OrchestrationPlanMachine : ILearningMachine<OrchestrationPlanDomain>` и `PromptFlowGroundingModel` (3 режима, в т.ч. Mode 1 = learned K-NN).
  - §5 (стр. 241–278) — `TranslationMachine : ILearningMachine<TranslationDomain>`; KNN-каскад `TranslationKnnStore.FindNearestCascade`.
  - §5b (стр. 280–337) — `SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>`; 11 семантических юнитов; границы host-расширений (A-LM-11).
  - §7b/§4.2 (стр. 198–208, 388–399) — удалённые контракты Phase 37 (`PromptFlowIndex`, `_flowIndex`, `RetrievalSource`, `GenerationRouteMode.KnownFlowRetrieval`); `FlowReferenceDetector` = только build-time/audit, запрет в runtime (нарушение D-16).
- `C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs` (262 строки) — **канонический исходник контрактов** уровней 5 и 1. Прочитан полностью; §10 ARCHITECTURE.md ему соответствует (verified, не inferred). Последняя строка файла — закрывающая `}` интерфейса `ILearningMachinePair` (метод `PullGlobalToPersonalAsync`), namespace `AgentInfrastructure.Logical`.
- `C:/Projects/llm_mvp_core/docs/COMPONENT-INTERFACES.md` — существует (Glob подтвердил), НЕ читался в этом реконе (вне бюджета 3–8 файлов; кандидат на доисследование для точных сигнатур ILogicalUnit/Flow/Orchestrator).
- `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md` (782 строки, прочитаны стр. 1–433) — правила классификации уровней vs детерминистических хелперов (использованы ниже).

---

## 2. Как устроено / работает (кратко)

Пять уровней абстракции (ARCHITECTURE.md §10, стр. 439–445; исходник `LearningMachineContracts.cs`):

- Level 5 `ILearningMachine<TDomain>` — `TrainBatchAsync`, `EvaluateAsync`, `ExportAsync`, `ImportAsync`, `GetOrchestratorAsync` (исходник стр. 208–234). Единственный уровень с обучением.
- Level 4 `ILogicalOrchestrator` — `RunAsync(LogicalOrchestratorRequest) → LogicalOrchestratorResult`. Маршрутизация/исполнение, без обучения.
- Level 3 `ILogicalFlow<TInput>` — `ExecuteAsync(LogicalFlowRequest<TInput>) → LogicalFlowResult`. Без обучения.
- Level 2 `ILogicalUnit<TInput, TOutput>` — `ExecuteAsync(LogicalUnitRequest<TInput>) → LogicalUnitResult<TOutput>`. Наименьшая исполняемая ячейка, не знает про обучение.
- Level 1 `ILearningMachinePair<TPersonal, TGlobal>` — два плеча: `Personal` + `Global` + анонимная синхронизация (`SyncPersonalToGlobalAsync`, `PullGlobalToPersonalAsync`) (исходник стр. 245–261).

Связь уровней (§10 mermaid, стр. 453–466; комментарий-иерархия в исходнике стр. 197–207):
`ILearningMachinePair.Personal/Global` → каждый есть `ILearningMachine<TDomain>` → `GetOrchestratorAsync()` отдаёт `ILogicalOrchestrator` → `ILogicalFlow<T>` → `ILogicalUnit<TIn,TOut>`. То есть Pair оборачивает две машины; машина оборачивает оркестратор; оркестратор → флоу → юнит. Обучение «висит» только над оркестратором (A-LM-02).

Разделение train/inference (locked A-LM-02, исходник стр. 8, 204–207, 447–451): `Train*` есть ТОЛЬКО на `ILearningMachine`. `ILogicalUnit` и `ILogicalFlow` — чистые inference-контракты; добавлять туда обучение запрещено.

Диаграмма компонентов (§1): показывает зависимости подпроектов. Ключевое для графа/оркестрации: `Core → OrchestratorModel`, `Core → ProjectWikiDomain`, `Core → TranslationDomain`, `Core → SmartSkillsDomain`; `ProjectWikiDomain → OrchestratorModel`; `SmartSkillsDomain → ProjectWikiDomain → OrchestratorModel`. 00.Core объявлен как носитель контрактов `ILogicalUnit · ILogicalFlow · ILogicalOrchestrator · ILearningMachine · OperationResult<T>` (стр. 20).

Level-5 имплементоры сегодня (§10 таблица, стр. 480–486; плюс §4.2/§5/§5b): `OrchestrationPlanMachine` (42), `TranslationMachine` (27), `SkillSelectionMachine` (43). Скелеты без обучения: `OrchestrationReasoningMachine`, `PromptToTaskMachine` (§5b, статус `not_trained`, отдельный приостановленный план).

---

## 3. Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

- **Граф/уровни — критический gap имени.** В текущей архитектуре НЕТ контрактов `ILearningUnit` / `ILearningFlow` / `ILearningOrchestrator` / `ILearningDynamicGraph`. Реально существуют только `ILogicalUnit` / `ILogicalFlow` / `ILogicalOrchestrator` (Level 2–4, inference-only) и `ILearningMachine` (Level 5) + `ILearningMachinePair` (Level 1). План ILearningDynamicGraph обязан явно решить: это (а) новый внешний уровень над `ILogicalOrchestrator` (аналогично тому, как `ILearningMachine` — «новое внешнее кольцо», исходник стр. 195), или (б) переименование/расширение существующих `ILogical*`. Молча предполагать существующие имена нельзя.
- **Где живёт граф-оркестрация сейчас:** §4.2 `OrchestrationPlanMachine` (42.ProjectWikiDomain) — ближайший существующий пример «обучаемой машины планирования оркестрации»; через `GetOrchestratorAsync` отдаёт `ILogicalOrchestrator` для wiki-флоу. Это естественный кандидат-прецедент для DynamicGraph (узлы=юниты/флоу, рёбра=маршрут).
- **DPO-обучение — типы готовы:** `LearningBatch<TDomain>` / `LearningTrainingItem` (prompt/chosen/rejected, исходник стр. 18–39) — каноническая DPO-триплета. `TrainBatchAsync` возвращает `LearningBatchResult` с обязательными числовыми метриками (RowsTrained, AccuracyBefore/After, RewardMargin, CpuMs, PeakRamMb, ElapsedMs, PromotedUnitIds; стр. 43–62). Любой обучаемый граф/ребро должен класть DPO сюда, а не во внешний scratch (правило CLAUDE.md «Trainable Unit pattern»).
- **Измерение:** `EvaluateAsync` → `LearningEvaluationResult` (Accuracy, RewardMargin, PassesGate, GateNote; стр. 90–103). Это held-out измерение для гейта. DynamicGraph-план должен переиспользовать эти DTO для «trained-acceptance» доказательств (числа, не static/UNKNOWN).
- **Lifecycle/export-import (для «модель учится, бэкапится, импортируется, продолжает обучение» — R5/CLAUDE.md):** `ExportAsync`/`ImportAsync` + `LearningMachineExport` (CheckpointRef/ManifestRef → 25.ModelServer, AccuracyAtExport обязателен; стр. 112–127, 220–228). Двухплечевая адаптивность: `ILearningMachinePair` + DTO покрытия `LearningMachinePairCoverageRow/Matrix` (стр. 159–187) — уже встроенный «contract-only» аудит наличия common/global + personal/adaptive плеч и портативности; поле `DeterministicShortcutSubstituted` прямо требует доказать, что плечо не подменено regex/BM25/lookup.
- **Teacher-provenance:** в ARCHITECTURE.md и в `LearningMachineContracts.cs` полей провенанса учителя (teacher_access_mode, model_license, и т.п.) НЕТ. Это поля из HOW-TO-USE-SKILLS.md (триггер «DPO/distillation teacher», стр. 330): требуются `teacher_access_mode`, `hosting_provider`, `model_id`, `model_license`, `provider_terms_checked`, `teacher_output_used_for_training`, `closed_proprietary_model`. Claude/OpenAI/Gemini запрещены как distillation-учителя. Для ILearningDynamicGraph эти поля надо добавить в DPO-схему графа (gap, см. §5).

---

## 4. Классификация уровней (ILearningMachine vs детерминистический хелпер)

Правило-источник: CLAUDE.md «Allowed trainable architecture pattern gate» + HOW-TO-USE-SKILLS.md строки 327, 342, 346–347 (scorer/selector/classifier/router/KNN/Jaccard/BM25/dictionary/counter/regex — НЕ архитектурные паттерны; допустимы только как implementation detail внутри разрешённого уровня).

Разрешённые архитектурные уровни (подтверждены исходником `LearningMachineContracts.cs`):
- `ILearningMachine<TDomain>` (Level 5) — обучаемая машина. Имплементоры: `OrchestrationPlanMachine`, `TranslationMachine`, `SkillSelectionMachine`.
- `ILearningMachinePair<TPersonal,TGlobal>` (Level 1) — пара personal+global.
- `ILogicalOrchestrator` (Level 4), `ILogicalFlow<TInput>` (Level 3), `ILogicalUnit<TInput,TOutput>` (Level 2) — inference-only.

Механизмы из ARCHITECTURE.md, которые НЕЛЬЗЯ выдавать за обучаемый уровень (детерминистические хелперы / внутренние детали):
- `TranslationKnnStore.FindNearestCascade` (Full→Prefix→Structure→BootstrapFallback, §5 стр. 271) — **KNN-хелпер**. Допустим только внутри `ExecutorUnit`/машины, не как «архитектура» и не как финальная обучаемая авторитетность сам по себе.
- `PromptFlowGroundingModel` Mode 1 `LearnedFlowGrounding` (§4.2 стр. 186) — заявлен как learned K-NN; в плане DynamicGraph должен оставаться **подкомпонентом** обучаемой `OrchestrationPlanMachine` с lifecycle-доказательствами (checkpoint/manifest/fresh-load/continue-training), иначе это «K-NN как авторитет» = нарушение gate.
- `SearchEngineFacade` / BM25 (`ProjectEvidenceIndex`, §4 стр. 152, 175) — **лексический retrieval-хелпер**, eval/labeling-only по правилам; не обучаемый уровень.
- `StopSignalDetector`, `RouteClassifier`, `StackDetector`, `FamilyCollapser` и пр. из §2/§6 — **detector/classifier-хелперы** конвейера 29.SemanticCompiler; внутренние детали, не уровни иерархии.
- `FlowReferenceDetector` (§4.2/§7b) — build-time/audit-only; **запрещён в runtime** (D-16). Для графа важно: detector рёбер не должен попасть в runtime-маршрут.
- `_flowIndex` / `PromptFlowIndex` (§7b) — **удалены** в Phase 37; повторно вводить как «индекс флоу» нельзя (прямой dictionary/exact-lookup вместо обучаемого grounding — нарушение R4/CLAUDE.md PromptFlowGroundingModel-правила).

Вывод для ILearningDynamicGraph: «узлы/рёбра» графа должны мапиться на разрешённые уровни (юнит/флоу/оркестратор внутри обучаемой машины). Авторитет выбора ребра обязан исходить из обучаемого семантического компонента, а KNN/BM25/detector/counter — только вспомогательные сигналы внутри, с явной меткой eval/helper/internal и lifecycle-доказательством, если претендуют на обучаемость.

---

## 5. Пробелы / что доисследовать

1. **Несоответствие нумерации в задаче:** иерархия = §10 (не §9), диаграмма компонентов = §1; иерархическая mermaid = внутри §10. Закрыто в этом отчёте.
2. **Нет контрактов `ILearning(Unit|Flow|Orchestrator|DynamicGraph)`** ни в ARCHITECTURE.md, ни в `LearningMachineContracts.cs`. Это центральный архитектурный вопрос плана: новый уровень над `ILogicalOrchestrator` против переименования `ILogical*`. Доисследовать: `00.Core` (есть ли `ILogicalOrchestrator/Flow/Unit` определения и где именно) — кандидат `docs/COMPONENT-INTERFACES.md` (§ перечислены в Quick Reference CLAUDE.md) и файлы `00.Core/*.cs` (Interfaces.cs, LearningMachineContracts.cs прочитан).
3. **Точные сигнатуры Level 2–4** (LogicalUnitRequest/Result, LogicalFlowRequest/Result, LogicalOrchestratorRequest/Result, метаданные `active_repo_path`/`linked_repo_paths`) НЕ прочитаны из исходника в этом реконе — только из таблиц §10/§4.1. Доисследовать в `00.Core` для точного контракта «многие-ко-многим / fan-out/fan-in» рёбер графа (требование триггера «graph-style DynamicGraph», HOW-TO-USE-SKILLS стр. 329).
4. **Teacher-provenance поля отсутствуют** в DPO-типах (`LearningTrainingItem`/`LearningBatch`). План DynamicGraph должен ввести поля провенанса учителя (teacher_access_mode, hosting_provider, model_id, model_license, provider_terms_checked, teacher_output_used_for_training, closed_proprietary_model) — иначе DPO-строки графа не пройдут gate. Источник правила: HOW-TO-USE-SKILLS стр. 330.
5. **`COMPONENT-INTERFACES.md` не прочитан** (вне бюджета). Содержит, по Quick Reference CLAUDE.md, «Bootstrap unit interfaces §42.ProjectWikiDomain» и интерфейсный RAG — нужен для классификации узлов графа на разрешённые уровни.
6. **HOW-TO-USE-SKILLS прочитан частично** (стр. 1–433 из 782). Прочитанной части хватило для правил классификации уровней vs хелперов; остаток (434–782) — кандидат на доисследование, если плану нужны точные DPO/quarantine-гейты.
7. **Async-исполнение графа и authority-per-edge** (deterministic-vs-trainable на каждом ребре) — требование триггера DynamicGraph; в текущих контрактах явного «edge authority» нет. Доисследовать прецедент в 15.OrchestratorModel (`LogicalOrchestratorAssistant.CreateDefault`, регистрация флоу) — упомянут в §4/§4.1, но код не читался.
