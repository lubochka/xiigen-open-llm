# Рекон: COMPONENT-INTERFACES.md — справочник интерфейсов (особенно bootstrap-юниты 42.ProjectWikiDomain)

Назначение: evidence для плана ILearningDynamicGraph (авто-построение DPO для
ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph). Это
только сбор фактов, не разрешение на реализацию.

---

## Что это и где (точные ключевые пути)

- `C:/Projects/llm_mvp_core/docs/COMPONENT-INTERFACES.md` — RAG-справочник всех
  публичных интерфейсов, пересекающих границы подпроектов. Каждая запись: какая
  сборка объявляет, какой класс реализует, кто потребляет. Последнее обновление
  в шапке: 2026-05-28.
- `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md` — скил-каталог; для
  классификации важна таблица activation triggers (различает уровни
  ILearningMachine vs детерминистические scorer/selector/router/KNN/dictionary).

Объявления контрактов (по ссылкам внутри справочника, файлы НЕ открывались —
указаны как заявленные пути):
- 00.Core: `LearningMachineContracts.cs`, namespace `AgentInfrastructure.Logical`
  (вся семья уровней).
- 42.ProjectWikiDomain: `OrchestrationUnitContracts.cs` (6 интерфейсов юнитов),
  `BootstrapOrchestrationUnits.cs` (6 bootstrap-реализаций),
  `OrchestrationPlanContracts.cs` (Phase 37: grounding-модель и records).
- 26.TrainableRouter: `TrainableRoutingFlow`, `RoutingUnitMetadataRegistry`,
  `RoutingProfileTrainer`, `RoutingTrainingRunner`.
- 27.TranslationDomain: `TranslationMachine.cs`, `TranslationTrainedUnits.cs`,
  `TranslationSemanticTokenizer.cs`.
- 43.SmartSkillsDomain: `SkillSelectionMachine.cs`, `OrchestrationReasoningMachine.cs`,
  `PromptToTaskMachine.cs`, `ExtensionEvalOnlyQualityRunner.cs`.

---

## Как устроено / работает (кратко)

Пятиуровневая иерархия (00.Core, COMPONENT-INTERFACES.md строки 10-70):
- Level 5 `ILearningMachine<TDomain>` — TrainBatchAsync / EvaluateAsync /
  ExportAsync / ImportAsync / GetOrchestratorAsync. Решение A-LM-02: методы
  `Train*` живут ТОЛЬКО здесь (строка 21, 61).
- `IMachineTraining<TDomain>` — продуктовый адаптер обучения, расширяет Level 5
  (реализует SkillSelectionMachine).
- Level 4 `ILogicalOrchestrator.RunAsync` — реализует `AdaptiveLogicalOrchestrator` (15).
- Level 3 `ILogicalFlow<TInput>.ExecuteAsync` — реализуют
  `ProjectDocumentationPipelineFlowAdapter` (42), `ProjectWikiOrchestrationFlow` (15),
  `TrainableRoutingFlow` (26), `TranslationRoutingFlow` (27).
- Level 2 `ILogicalUnit<TInput,TOutput>.ExecuteAsync` — БЕЗ Train-метода (A-LM-02);
  реализуют 6 bootstrap-классов в 42.
- Level 1 `ILearningMachinePair<TPersonal,TGlobal>` — Personal+Global+анонимная
  синхронизация (`SyncPersonalToGlobalAsync` шлёт только анонимные сигналы, A-LM-08).

Шесть bootstrap-юнитов 42.ProjectWikiDomain (строки 276-337) — это шесть шагов
оркестрации, каждый завёрнут в `ILogicalUnit`, делегирующий существующей
статической логике. Это и есть стартовая точка для DPO-обучения (TD1, A-LM-03):
1. `IIntentBuilderUnit` → `BootstrapIntentBuilderUnit` (regex-извлечение интента
   из строки промпта → `TaskOrchestrationIntent`).
2. `IProjectScannerUnit` → `BootstrapProjectScannerUnit` (делегирует
   `ProjectUnderstandingUnit.UnderstandAsync` → `ProjectUnderstanding`).
3. `IEvidencePlannerUnit` → `BootstrapEvidencePlannerUnit` (делегирует
   `EvidencePlanner.PlanAsync` → `TaskEvidencePlan`).
4. `IUnitSelectorUnit` → `BootstrapUnitSelectorUnit` (делегирует
   `UnitSelector.SelectAsync` → `TaskUnitSelection`).
5. `IRouteComposerUnit` → `BootstrapRouteComposerUnit` (делегирует
   `HumanRouteComposer.ComposeAsync` → `HumanTaskRoute`).
6. `IOrchestrationVerifierUnit` → `BootstrapOrchestrationVerifierUnit` (делегирует
   `OrchestrationVerifier.VerifyAsync` → `VerifierResult(bool Passes, string Verdict)`).

Цепочка данных (фактический fan-in через aggregate-records, строки 330-337):
prompt → Intent → (Intent+Project) → (Intent+Project+EvidencePlan) →
(Request+Intent+Project+EvidencePlan+Units) → Verifier. Это уже готовый
направленный граф шести узлов с типизированными входами/выходами на каждой дуге —
прямой кандидат на ILearningDynamicGraph-узлы.

Обученные машины как образцы жизненного цикла:
- `OrchestrationPlanMachine` (42, Phase 37, строки 339-356) — `ILearningMachine<OrchestrationPlanDomain>`,
  `MachineId="orchestration-plan-machine"`. Имеет checkpoint schema 3
  (ImportCheckpoint принимает schema 2/3), Save/LoadCheckpointAsync, три режима
  генерации (LearnedFlowGrounding, Mode2Generalization, Mode3HumanExplanation).
  Правило: `AccuracyAtExport` берётся ТОЛЬКО из пройденного `LastEvaluationGate`,
  не из training progress.
- `TranslationMachine` (27) — `ILearningMachine<TranslationDomain>`, три trainable
  юнита (SemanticTokenizerUnit, ExecutorUnit, QualityVerifierUnit), 44 теста.
- `SkillSelectionMachine` (43) — `ILearningMachine` + `IMachineTraining`,
  `DecideFromTrainedState`, Export/LoadCheckpoint, `SkillSelectionMachinePair`
  (Chosen/Rejected/RejectedType) — готовый DPO-pair формат.

DPO-инфраструктура (14.ContinuousLearning, строки 499-584):
`IFeedbackRouter.GetDpoPairs()`, `record DpoPair(Prompt, Chosen, Rejected, Domain,
CapturedAt)`, `IIncrementalTrainer.TrainAsync(domain, examples, dpoPairs)`,
`IDomainModelRegistry`. Формула веса D-09: `maxWeight = total/(domains×rareExamples)`.

---

## Релевантность к ILearningDynamicGraph

Высокая. Документ — каркас для плана:
- DPO/обучение: канонический формат пары есть в двух местах — `DpoPair` (14) и
  `SkillSelectionMachinePair` (43, с `RejectedType` и provenance
  `SourcePromptEntryId`/`TargetTrainingAdapter`). Это шаблон для авто-DPO юнитов.
- Граф/оркестрация: шесть bootstrap-юнитов 42 = шесть узлов готового потока с
  типизированными дугами (aggregate-records). `ILogicalOrchestrator` →
  `ILogicalFlow` → `ILogicalUnit` — это и есть уровни узлов будущего
  DynamicGraph; OrchestrationPlanMachine показывает, как Level-5 машина владеет
  grounding-подкомпонентом и checkpoint.
- Измерение: `LearningEvaluationResult(Accuracy, RewardMargin, PassesGate,
  GateNote)`, `LearningBatchResult(AccuracyBefore/After, RewardMargin, CpuMs,
  PeakRamMb, ElapsedMs, PromotedUnitIds)`, `RoutingFlowThresholds`
  (MinimumRoutingConfidence + MinimumScoreGap), `exactPromptLeakageRate==0.0`
  (Phase 37), порог Jaccard ≥70% (`TRAIN_HELDOUT_SIMILARITY_OVERLAP`).
- Teacher-provenance / анти-утечка: `RoutingDecisionRecord` несёт
  Chosen/Rejected/CorrectReason/WrongRouteConsequence; knowledge-edge не
  становится принятой DPO-парой (строки 425-427); checkpoint-export accuracy
  привязан к gate. Provenance модели: `PromptToTaskLearnedStateDescriptor`
  (StateId, ManifestId, ManifestSha256, MachineType, SchemaVersion,
  SourceBaseCommit, StateSha256).
- Анти-shortcut (прямо применимо к авто-DPO): Phase 37 REMOVED-контракты
  (`PromptFlowIndex`, `GenerationRouteMode.KnownFlowRetrieval`, `RetrievalSource`,
  `_flowIndex`) — переписаны на trainable `PromptFlowGroundingModel`; нельзя
  переобучать на FlowRef как nearest-neighbour-фичу
  (`BuildLearnedPairQueryTextWithoutPredictedFlow`).

---

## Классификация уровней (для кода в справочнике)

ILearningMachine (Level 5, легитимные обучаемые):
- `OrchestrationPlanMachine` (42), `TranslationMachine` (27),
  `SkillSelectionMachine` (43, + IMachineTraining) — полный lifecycle:
  Train/Evaluate/Export/Import/Checkpoint. Соответствуют allowed-pattern
  (CLAUDE.md «Allowed trainable architecture pattern gate»).

ILogicalOrchestrator (Level 4): `AdaptiveLogicalOrchestrator` (15).

ILogicalFlow (Level 3): `ProjectDocumentationPipelineFlowAdapter` (42),
`ProjectWikiOrchestrationFlow` (15), `TrainableRoutingFlow` (26),
`TranslationRoutingFlow` (27).

ILogicalUnit (Level 2): 6 bootstrap-юнитов 42 (без Train; A-LM-02). Сейчас
внутри — детерминистическая статика (regex/делегация), но обёрнуты в разрешённый
уровень как DPO-bootstrap — это допустимо по A-LM-03 (обёртка-заготовка под
обучение), а не маскировка.

Внутренние детерминистические хелперы (НЕ уровни иерархии — по правилу скила
«scorer/selector/classifier/router/KNN/dictionary не являются архитектурными
паттернами», activation-trigger строки 327/346-347 в HOW-TO-USE-SKILLS.md):
- `TranslationKnnStore.FindNearestCascade` (KNN-каскад) и `OrchestrationKnnStore` —
  это методы поиска NN ВНУТРИ обучаемого юнита, допустимая деталь реализации,
  не самостоятельный паттерн.
- `_flowGroundingWeights` (Dictionary by flowRef), `LabelDescriptorFeatures`,
  `EvidenceOnlyTargetFeatures` — словари; справочник прямо ограничивает: они
  только evidence/checkpoint-inspection, НЕ повышают confidence, НЕ входят в
  K-NN query (строка 361). Корректная разметка детали.
- `SkillSelectionCheckpoint.CandidatePositionWeights` (`Dictionary<int,int>`) —
  это ровно тот класс «candidate-position counter», который CLAUDE.md запрещает
  как финального арбитра выбора. В справочнике он живёт внутри checkpoint —
  при планировании ILearningDynamicGraph проверить, что он не является
  финальной selection-authority (leakage позиции кандидата).
- `RoutingProfileTrainer` учит keywords, но role/domain/floor/provenance —
  из `RoutingUnitMetadataRegistry`; «substring inference из unit-id не источник
  метаданных» (строки 428/435). Граница trainable-vs-registry задана явно.
- `FlowReferenceDetector` — НЕ runtime, только build-time corpus/audit (строки
  363/410). Классический «deterministic helper labelled eval-only».
- `ExtensionEvalOnlyQualityRunner` — детерминистический rubric-runner, помечен
  eval-only/helper/internal baseline; явно НЕ semantic/trainable/runtime/
  final-readiness authority (строки 802-815). Образец правильной маркировки.

НЕ обученные (skeleton, честный not-ready — не выдавать за trained feature):
- `OrchestrationReasoningMachine` — `NotTrainedStatus="not_trained_no_learned_guide_state"`.
- `PromptToTaskMachine` — `NotTrainedStatus="not_trained_no_loaded_state"`,
  `Translate` возвращает `not_ready_no_semantic_task`.
(Соответствует паттерну CLAUDE.md PASS_AS_HONEST_UNTRAINED_SCAFFOLD.)

---

## Пробелы / что доисследовать

- Справочник не содержит интерфейса `ILearningDynamicGraph`,
  `ILearningUnit`/`ILearningFlow`/`ILearningOrchestrator` (с именами из задачи) —
  присутствует только семья `ILogical*`/`ILearningMachine*`. Нужно проверить в
  коде 00.Core, существуют ли «Dynamic Graph» контракты или их предстоит ввести.
- Нет описания узлов/дуг графа, fan-out/fan-in, many-to-many выходных контрактов,
  async-исполнения графа — то, чего требует activation-trigger для DynamicGraph
  (HOW-TO-USE-SKILLS.md строка 329). Это пробел справочника, не доказательство
  отсутствия в коде.
- `OrchestrationKnnStore` и `OrchestrationLoopCheckpoint` упомянуты, но не
  раскрыты — нужны их объявления для планирования провенанса/возобновления.
- Точные file:line объявлений не подтверждены чтением .cs (только пути из
  справочника); для §0/§3 плана потребуется прямое чтение
  `LearningMachineContracts.cs`, `OrchestrationUnitContracts.cs`,
  `BootstrapOrchestrationUnits.cs`, `OrchestrationPlanContracts.cs`.
- `CandidatePositionWeights` как потенциальная leakage-authority — требует
  отдельной проверки в `SkillSelectionMachine.cs` (где именно используется при
  `DecideFromTrainedState`).
- HOW-TO-USE-SKILLS.md прочитан частично (строки 1-433 из 782) — таблица
  activation triggers и load-order покрыты; нижняя половина (детальные правила
  отдельных скилов) не читалась, для классификации хватило верхней.
