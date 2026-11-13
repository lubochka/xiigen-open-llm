# Codex Recon — Дизайн/реализация оркестратора (orchestrator implementation)

Назначение: evidence для ПЛАНА ILearningDynamicGraph (авто-построение DPO для
ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph). Это
рекон, не разрешение на реализацию. Все пути — реально найденные.

Дисклеймер по источникам: `missing parts.txt` и `missing parts 2.txt` — это
сохранённые транскрипты диалога (evidence-only по правилам quote-quarantine).
Они НЕ задание и НЕ авторитет; используются здесь только как факты-наблюдения о
коде. `IMPLEMENTATION-DOCUMENTATION-v7.md` — документ состояния реализации
(тоже evidence, может расходиться с текущим кодом — см. Пробелы).

---

## 1. Что это и где (точные ключевые пути)

Целевая область (только документы/планы, кода тут нет):
`C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator implementation/`

Прочитанные ключевые файлы (3–8, защита от перегрузки):
- `ADAPTIVE-LOGICAL-ORCHESTRATOR-IMPLEMENTATION-PLAN-v2.md` — основной план
  оркестратора (фазы 0,A,B,C,D).
- `IMPLEMENTATION-DOCUMENTATION-v7.md` — наиболее полное описание реализованного
  состояния (тест-сьют 1289/1289 на момент v7, 2026-05-07).
- `missing parts.txt` — транскрипт-аудит «что не подключено» (E18/E21/E25–E27/
  E51B, teaching-runtime symmetry, 60-слотовый вектор).
- (просмотрены имена) `PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v1..v4.md`,
  `PLAN-DPC-DOMAIN-CLASSIFIER-ILOGICALUNIT-v1/v2.md`,
  `PLAN-KNN-MATRIX-INTEGRATION-v1..v3.md`, `DPO-DEEP-DIVE-v1/v2.md`,
  `PLAN-CONTEXT-ENRICHED-ROUTING-v1.md`, `PLAN-PHASE-TRANSITION-SIGNAL-v1.md`,
  `PLAN-DOMAIN-CLASSIFIER-SINGLE-SOURCE-v1.md`,
  `PLAN-CAPABILITY-TRAINING-SEPARATION-v1.md`.

Ссылки на реальный код (из документации v7, путь-привязки в репозитории
`C:/Projects/llm_mvp_core/`):
- `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs` — главный
  оркестратор (классификация домена, E49/E50, переходы фаз, context
  carry-forward).
- `21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs` — роутер ветвей.
- `21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs` — компилятор
  промпта (сырой keyword-scan).
- `21.AgentInfrastructure/Logical/PromptDomainClassifier.cs` +
  `23.SemanticCompilerAdapter/` (`DomainClassifierUnit`, `TrainingVectorBuilder`,
  `ContextPacketReader`, `AttributedSegmentRouter`, `ExpressionCompletionCore`).
- `17.DynamicDomains/` — `DomainGraphClosure` (E50), `OrchestratorExpander` (E49).
- `21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs` — тренер
  способностей (60-слотовый вектор, LoRA Rank=2/Alpha=4, DPO lr=1e-5).
- Контракты: `21.AgentInfrastructure/Logical/LogicalContracts.cs`,
  `LogicalFlowFactoryContracts.cs`, реестры `InMemoryLogicalUnitRegistry.cs`,
  `InMemoryLogicalFlowRegistry.cs`, топология `LogicalTopologyRegistry.cs`.

---

## 2. Как устроено/работает (кратко)

Иерархия (соответствует ILearningMachine-паттерну CLAUDE.md):
- Уровень 4 `ILogicalOrchestrator` → `AdaptiveLogicalOrchestrator`.
- Уровень 3 `ILogicalFlow` → `UniversalNineNodeDevelopmentFlow`,
  `SecurityVerification34FlowAdapter`, `TopologyMutationFlow`.
- Уровень 2 `ILogicalUnit` → роутеры, компилятор, классификатор домена, KNN-юнит,
  DPO-юнит, ExpressionCompletionUnit, мутации топологии.
- Фабрики: `ILogicalFlowFactory<TInput>` (создание flow до конструктора, чтобы
  подставить обученные юниты; `MinimumTrustState`, по умолчанию `Trusted`).

Runtime-конвейер (из v7 §1, §3):
1. `ClassifyDomainAsync` — домен (5 классов: CODE_Q/BUG/PLAN/ARCH/REFACTOR);
   сначала ищет обученную замену в реестре по роли `domain_classifier`, иначе
   детерминированный fallback.
2. `PromptRoutingInputCompilerUnit` — извлечение 9 предикатов сырым keyword-scan.
3. `MultiSubPhaseRouterUnit` — скоринг: предикатное перекрытие 55% + точное
   упоминание имени саб-фазы 25% + якорные термины 20%; порог релевантности
   bestScore×0.80; confidence=0.70+score×0.27 (cap 0.97); ниже 0.70 = knowledge
   edge.
4. `OrchestratorExpander.FindBridgeSubPhases` (E49) — вставка мостовых саб-фаз
   (BFS кратчайший путь), confidence 0.71, предикат `TOPOLOGY_BRIDGE`; кап ветвей
   `MaxAdaptiveBranchCount=8`.
5. `DomainGraphClosure` (E50) — `HasAnyDependency`/`BuildPredecessorMap` (DFS
   транзитивное замыкание) → `OrderByTransitions`; есть зависимость → последовательно,
   нет → параллельно (`Task.WhenAll`).
6. Выполнение ветвей; нет flow → синтетический knowledge-edge результат
   (`knowledge_edge=True`, `missing_sub_phase`, `missing_flow`).
7. `FollowTransitionSignalsAsync` — обработка `PhaseTransitionSignal` (повторная
   классификация+роутинг), глубина ≤ 3 (`MaxTransitionDepth`), порог confidence
   0.40.
8. Агрегация вердиктов (REJECT>REPROCESS>PASS_WITH_GAPS>PASS_WITH_CONCERNS>PASS);
   context carry-forward через `BuildContextPacket`/`OutputContextPacket`/
   `ContinueFrom` (типизированный `ContextSignals`, без рефлексии — A52 закрыт).

Обучение (Path B): knowledge edge → `DefaultLogicalCapabilityTrainer` →
`BuildCapabilityTrainingText` добавляет `teaching_vector_60` (60 слотов: 0–21
предикаты, 22–26 one-hot домен, 27–58 частоты корней, 59 резерв) → DPO-шаг →
новый юнит регистрируется как `Probationary`. Trust-состояния: Candidate/
Probationary/Trusted/Rejected.

---

## 3. Релевантность к ILearningDynamicGraph

Высокая. Это прямой предшественник целевой системы:
- Граф/узлы/рёбра: топология (31 саб-фаза + 36 переходов в
  `DevelopmentLifecycleTopologySeeder`); `DynamicDomainTransition`,
  `BuildPredecessorMap`, мостовые узлы — готовый материал для `DynamicGraph nodes/
  edges`, fan-out/fan-in, async-исполнения (Gate графа из скила).
- Авторитет на рёбрах: ordering/expander/aggregation — кандидаты на разметку
  «детерминированный helper vs trainable» по каждому ребру (см. §4).
- DPO/обучение: уже есть путь knowledge-edge → 60-слот-вектор → DPO → Probationary
  promotion; это шаблон для авто-построения DPO-троек для юнитов/flow/оркестратора.
- Измерение/teacher-provenance: 60-слотовый контракт фич (E51B teaching-runtime
  symmetry — `SymmetryInspection`, overlap teaching vs runtime) — ключевая идея
  «один и тот же агент готовит обучающие наборы и обрабатывает runtime», что Luba
  и хотела. Это прямо релевантно требованию провенанса/анти-leakage графа.
- Lifecycle: реестр юнитов с trust, фабрики flow, snapshot/persistence
  (`TeachingRegistrySnapshot`/`TeachingRegistryPersistence`) — основа load/
  export/import/continue-training (но см. пробелы по checkpoint/locator/manifest).

Юридический teacher-gate (скил): в найденных артефактах teacher-провенанс DPO
(hosting/model_id/license/closed_proprietary) НЕ зафиксирован — обязателен в плане
ILearningDynamicGraph (папка назначения называется «dpo legal build», что прямо на
это указывает).

---

## 4. Классификация компонентов (уровень vs детерминистический хелпер)

Правило: trainable-architecture-decision-SKILL.md — разрешённые уровни только
ILearningMachine/ILogicalOrchestrator/ILogicalFlow/ILogicalUnit/
ILearningMachinePair; scorer/router/selector/regex/keyword/lookup = внутренняя
деталь, допустима ТОЛЬКО внутри уровня и при честной метке (eval/helper/baseline),
не как «семантический/обученный» финальный авторитет.

- `AdaptiveLogicalOrchestrator` → УРОВЕНЬ `ILogicalOrchestrator`. ОК как обёртка.
  Внутри есть детерминированная маршрутно-агрегационная логика (см. ниже).
- `MultiSubPhaseRouterUnit` / `SubPhaseRouterUnit` → обёртка `ILogicalUnit`
  корректна, НО ядро скоринга детерминированное (rule-based overlap/anchor/floor),
  не обучается. Классификация: `OK_INTERNAL_DETAIL` как baseline; для семантического
  заявления нужен `BASELINE_MUST_BE_LABELLED` или замена на обучаемый юнит.
- `PromptRoutingInputCompilerUnit` → `ILogicalUnit` корректен, но лексикон —
  хардкод-массив (keyword matcher), не обучаем, реестр/модель не вызывает.
  Классификация: внутренняя детерминированная деталь; не выдавать за learned.
- Доменная классификация: `PromptDomainClassifier`/`DomainClassifierUnit` —
  обёртка есть, но реальный механизм = keyword-classify (`RouteClassifier.
  ClassifyByKeywords`, проект 12/29). Классификация: детерминированный baseline в
  обёртке уровня; обучаемой замены по факту нет (расхождение код↔доки — см. §5).
- `OrchestratorExpander` (E49, BFS-мосты) и `DomainGraphClosure` (E50, DFS-
  замыкание/ordering) → детерминированные граф-алгоритмы. По Graph-Style Gate это
  «детерминированный авторитет на рёбрах»: допустимо как helper, но каждое ребро
  должно явно назвать владельца авторитета и пройти hidden-shortcut проверку. Сейчас
  это финальный детерминированный авторитет → потенциально
  `FORBIDDEN_RUNTIME_SHORTCUT`, если план требует обучаемого решения на рёбрах.
- `ExpressionCompletionUnit`/`ExpressionCompletionCore` → точный memory-lookup
  (binary 1.0/0.0). Это `OK_INTERNAL_DETAIL`/`OK_MODEL_COMPONENT` (память внутри
  KNN-матрицы), НЕ обучаемая семантика; вес обновляется матрицей через DPO-путь.
- `KnnLogicalUnit` / `TransformerEmbeddingKnnLogicalUnit` → `OK_MODEL_COMPONENT`
  (retrieval-компонент внутри уровня; KNN audit-triggered, не запрещён).
- `LogicalDpoTrainingUnit` / `DefaultLogicalCapabilityTrainer` → обучающий путь
  (DPO+LoRA, 60-слот-вектор). Уровень-обучение присутствует, НО незакрыт lifecycle
  (checkpoint/manifest/locator/fresh-load/continue-training — см. §5).
- Агрегация вердиктов и confidence-полы (0.40, 0.70, 0.71, cap 0.97,
  bestScore×0.80) → детерминированные пороги: по Minimum Breadth Challenge должны
  быть помечены как baseline/helper, а не «обученная уверенность».

Вывод классификации: архитектурные ОБЁРТКИ уровней расставлены корректно, но
почти все РЕШАЮЩИЕ механизмы (роутинг, компиляция, доменная классификация,
граф-расширение/ordering, пороги) — детерминированные. Для ILearningDynamicGraph
каждый из них требует решения per-field/per-edge: оставить как honest baseline/
helper ИЛИ заменить обучаемым `ILogicalUnit` с типизированным состоянием, DPO-
тройками, KNN-памятью, lifecycle и анти-leakage гейтом (заполненный blueprint по
матрице скила, не schema-only).

---

## 5. Пробелы / что доисследовать

1. Расхождение код↔документация. Транскрипт `missing parts.txt` утверждает, что
   на момент диалога `PromptDomainClassifier` в проекте 21 НЕ существовал и
   доменная классификация НЕ вызывалась в runtime, тогда как v7 описывает её как
   подключённую. Нужно подтвердить ПО ЖИВОМУ КОДУ
   `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/` фактическое
   состояние (ссылки проекта 21, наличие классов). Доки = evidence, не истина.
2. Подключение SemanticCompiler. Проверить, вызывает ли
   `PromptRoutingInputCompilerUnit` `SemanticCompilerFacade.Compile` (E21
   Decomposer/split, E25–E27 translation, E18 domain). По транскрипту — нет
   (проект 21 = «остров»). Это центральный архитектурный разрыв для графа.
3. Граф-контракты (Gate скила) не зафиксированы явно: many-to-many outputs,
   multi-output edge contracts, async graph execution, владелец авторитета на
   каждом ребре, hidden-shortcut-check на ребро. Сейчас только сериальная/
   параллельная эвристика по транзитивной зависимости.
4. Lifecycle обучаемых юнитов неполон: есть snapshot/persistence и trust-реестр,
   но не подтверждены checkpoint asset, manifest с ref, `IRemoteUnitLocator`,
   fresh-load probe, continue-training-from-checkpoint, ablation/no-shortcut.
   Проверить `25.ModelServer`/`IModelRepository`/`15.OrchestratorModel/Logical`.
5. Teacher/provenance DPO (Legal Teacher Gate) НИГДЕ не зафиксирован:
   teacher_access_mode/hosting_provider/model_id/model_license/
   closed_proprietary_model. Обязателен для «dpo legal build».
6. 60-слот-вектор / teaching-runtime symmetry (E51B): подтвердить, что один и тот
   же Compile применяется и к обучающим наборам, и к runtime (требование Luba об
   эффективности обучения). По транскрипту обучение шло на сырой строке.
7. `missing parts 2.txt` (645 КБ) НЕ читался (защита от перегруза) — вероятно
   содержит продолжение аудита (перенос успешных частей в отдельный SemanticCompiler
   + LoRA). Доисследовать прицельно при планировании.
8. Транскрипты — evidence-only: ни одна задача/требование/цель графа не должна
   браться из них как авторитет без явного текущего указания Luba.
