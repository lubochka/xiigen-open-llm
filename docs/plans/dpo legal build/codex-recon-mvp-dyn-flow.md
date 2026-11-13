# Recon: Концепт динамического потока (предшественник динамического графа)

corpus: mvp · slug: mvp-dyn-flow · relevance: high

## Что это и где (точные ключевые пути)

Это набор **дизайн/план-документов** (markdown + диаграммы), описывающих архитектуру
LlmFromScratch как самомодифицирующейся сети маленьких специализированных
внутренних LLM-юнитов поверх редактируемого графа потока. Кода здесь нет —
только проектные артефакты. Это прямой концептуальный предшественник «динамического графа».

Ключевые файлы (база: `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\`):

- `DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md` — ядро концепта: 7 принципов, каст ролей-юнитов, 4 реестра, 3 пути обратной связи, иерархия доверия источников, sync/async обучение.
- `DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md` — поправка «слой сужения»: intake gate → system topology router → sub-subject routers → evidence pack selector → локальные splitter'ы; +11 новых ролей; locked-правило авторитета.
- `DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md` — каноническая сквозная последовательность из 20 шагов (runtime) + offline dataset-подготовка; правило повторного провала; правило idle-обучения.
- `ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md` — двухуровневая роль арбитров vs KNN (граница детерминизм/обучение на рёбрах).
- `DYNAMIC-FLOW-LOCKED-DECISION-D13-AUTHORITY-v1.md` — D-13: locked-граница авторитета priority/KNN/arbiter/external-AI.
- `dynamic-flow-high-level-topology-v2.mmd` — Mermaid flowchart топологии (узлы/рёбра, fan-out/fan-in, петли возврата).
- `dynamic-flow-v2-node-topology.csv`, `DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1_0.md` — табличная топология узлов и план мини-E2E (не читались детально; см. Пробелы).

## Как устроено/работает (кратко)

- **Граф потока = данные, не код** (Принцип 2). Оркестратор читает топологию (узлы, рёбра, iron rules, quality gates) из реестра в рантайме; правка потока = правка БД; версии append-only, откат = перемещение указателя.
- **Самоподобные юниты** (Принцип 3): единый predicate-in/predicate-out + метаданные; маленький обученный gate над реестром кандидатов — единый примитив (domain splitter, arbiter selector, skill retriever, swappability judge — параметризации одного вида).
- **Канонический рантайм** (из `...CANONICAL-INTEGRATION-SEQUENCE`): source role gate → Logical Structure Compiler (8 шагов: до нормализации обязателен детектор маркеров) → intake gate → topology router → sub-subject router array → evidence pack selector → локальные domain splitter'ы → arbiter-aware KNN → budget/confidence gate → unit registry lookup → локальный поток (domain translator → topology planner → local scheme planner → domain processor → applier → result convergence gate) → knowledge edge detector → (внешний AI path при необходимости) → arbiter selector → arbiter array → **arbiter verdict boundary** → result translator → formatter → style adapter → ответ; параллельно event trail → learning gap packet → idle learning queue → confidence updater.
- **Авторитет (D-13, locked)**: Priority решает что пробовать; KNN использует историю арбитров чтобы выбрать память; **арбитры решают что принято**; external AI — только advisory, не принимающий авторитет. При конфликте побеждает арбитр (если пользователь явно не исправит).
- **3 пути feedback**: (1) операционный реалтайм; (2) обучающий ретроспективный по event-log (граф учится здесь, не в пути 1); (3) gap-driven (детектор провалов → create-or-fetch цикл).
- **4 реестра** в 08.GraphRAG: flow registry, unit registry (рёбра FILLS/SUPERSEDES/SWAPPABLE_WITH/TRAINED_FROM), decision graph (XIIGen-форма маршрутизации), dataset registry. Все с event-sourcing; прямые записи в storage запрещены.
- **Иерархия доверия источников** (Tier 1–5): локально-активный → локально-неактивный → org-shared → внешний marketplace → обучение с web-корпуса; probation до накопления наблюдений.
- **Валидация**: E51B (6/6 трасс), E52 (0% baseline → 100% enhanced); первый имплемент-срез проектов 21–23 = 5 рантайм-компонент (InternalLlmGateway, InternalResultExaminer, ExternalAiRequestPackage, ExternalAiResultAdapter, LearningEventScheduler) — но эти компоненты живут в основном репо `llm_mvp_core`, не здесь.

## Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

- **Граф/оркестрация**: это и есть прямой концептуальный предшественник динамического графа — узлы=роли-юниты, рёбра=маршруты, fan-out (массивы translator/planner/processor/applier/arbiter), fan-in (result convergence gate), асинхронные рёбра (gap path, idle learning), петли возврата (arbiter reject → affected station). Совпадает с разделом скила «Graph-Style / DynamicGraph Reconstruction Gate».
- **DPO/обучение**: датасет-следствия описаны явно — каждый юнит получает примеры в форме своего рантайм-входа; пары хранят natural in/out + logical in/out + source evidence + arbiter status + usage status (positive/negative/warning). Это шаблон построения DPO/триплетов (chosen=approved, rejected=arbiter-failed, warning=отдельный класс).
- **Измерение**: conformance suites на юнит (recorded I/O pairs), invariant/replay/contract тесты, confidence updater, E52 before/after метрики — основа метрик готовности юнита/ребра.
- **Teacher/provenance**: Принцип 5 — внешний AI запрещён в control path; разрешён только как advisory после явного одобрения, результат адаптируется и проходит арбитров (НЕ становится напрямую training target). Это согласуется с Legal Teacher Gate скила: закрытые проприетарные модели (Claude/OpenAI/Gemini) не могут быть DPO/distillation-учителями. Провенанс end-to-end (Принцип 6) встроен по конструкции.

## Классификация уровней (для кода)

Кода в этой области **нет** — артефакты концептуальные. Поэтому классификация дана как **проектное намерение** ролей (mapping на разрешённые уровни иерархии `LearningMachineContracts`), а не как ревизия реализованного кода. Привязка к правилу: trainable-architecture-decision-SKILL — «единственные разрешённые уровни: ILearningMachine / ILogicalOrchestrator / ILogicalFlow / ILogicalUnit / ILearningMachinePair».

- **ILogicalOrchestrator** (намерение): «Orchestrator» роль + topology runtime, читающий граф и обходящий рёбра (предложенный 18.TopologyRuntime). Маршрутизация по графу = оркестрация.
- **ILogicalFlow** (намерение): «локальный динамический поток» (translator→planner→processor→applier→convergence) — последовательность/подграф = уровень flow.
- **ILogicalUnit** (намерение): каждая маленькая роль-юнит (intake gate, topology router, sub-subject router, domain splitter, domain translator, planner, processor, applier, arbiter, arbiter selector, result/style adapter, knowledge edge detector, gap detector, swappability judge). «Маленький обученный gate над реестром кандидатов».
- **ILearningMachinePair** (кандидат): style adapter — обучается преимущественно на per-user истории предпочтений (отдельные storage/cadence/rollback/privacy) → классический случай common-leg + user-adaptive-leg.
- **Внутренние детерминистические хелперы / НЕ-уровни** (по правилу скила «scorer/selector/router/retriever/detector/KNN/Jaccard/BM25/dictionary/counter — не архитектурные паттерны, только детали внутри разрешённого уровня»):
  - **arbiter-aware KNN memory** — компонент памяти/ретривер. Допустим как `OK_MODEL_COMPONENT` ВНУТРИ уровня, но **только guidance**: D-13/ARBITER-KNN явно лишают KNN финального авторитета. KNN как самостоятельный финальный авторитет = `FORBIDDEN_RUNTIME_SHORTCUT`.
  - **budget/confidence gate, priority** — контроллер скорости/стоимости/порядка, НЕ решает приёмку. Детерминистический хелпер; не может переопределять арбитра (риск shortcut-авторитета на ребре).
  - **arbiter verdict boundary** — жёсткий gate приёмки. Сами арбитры — это `ILogicalUnit`-классификаторы (по одному типу риска); «boundary» — детерминистическое применение их вердикта, не отдельный обучаемый уровень.
  - **fidelity & coverage checker** компилятора — проверочный хелпер на входе, не trainable-уровень сам по себе.
  - Соответствие «hidden_deterministic_shortcut_check_tied_to_each_edge»: на КАЖДОМ ребре нужно назвать владельца финального авторитета. Здесь это явно сделано (D-13): авторитет приёмки = арбитр-юнит; priority/KNN/budget = только candidate-generation/guidance.

## Пробелы / что доисследовать

- **Нет кода** — все классификации выше суть проектное намерение; перед планом ILearningDynamicGraph нужно сопоставить с реальными реализациями в `llm_mvp_core` (08.GraphRAG, 14/15/16/17, предложенные 18.TopologyRuntime/19.UnitRegistry; и 5 рантайм-компонент E51B проектов 21–23). Это вне данной области.
- **Не прочитаны детально** (защита от перегрузки; область содержит ~156 971 файл, в основном распакованные zip/кэш под `round-zips`, `source-examination-cache`, `all-documents-method-run`, `flow-01-training-sets-v1`): `dynamic-flow-v2-node-topology.csv` (точные узлы/рёбра топологии v2), `DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1_0.md` (мини-E2E детали), `E51*`/`E52`/`E53*` (валидационные результаты и схемы доказательств), `FLOW-01-TRAINING-DATASET-PLAN-v4.md` (252 KB — детальный план датасета FLOW-01), `EXAMINATION-METHOD-FINAL.md` (68 KB).
- **Точная схема рёбер** четырёх реестров и conformance-suite формат намеренно НЕ зафиксированы в draft (см. §«What This Document Does Not Yet Decide») — нужны для строгой реконструкции графа (input/output contracts, multi-output-edge contracts).
- **Open Items O-1..O-6** (нумерация подпроектов, split storage, sync-escape-hatch, UI, Tier-4, reverse SemanticCompiler) ещё не залочены — влияют на границы ILearningDynamicGraph.
- Связь концепта с реальной иерархией `ILearningMachine`/`ILogicalOrchestrator` (наименования в этих docs — «Orchestrator», «flow», «unit» — концептуальные, не C#-интерфейсы) требует явного маппинга на `00.Core/LearningMachineContracts.cs` при планировании.
