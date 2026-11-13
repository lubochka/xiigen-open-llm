# Рекон: 28.SemanticNlp — семантические NLP-юниты (для плана ILearningDynamicGraph)

corpus=core · slug=core-28-nlp · relevance=medium

## Что это и где (точные ключевые пути)

Проект `28.SemanticNlp` — «семантическая токенизация поверх BPE»: группировка токенов
по смыслу, а не по морфологии. Реализованы 5 классических NLP-алгоритмов, каждый —
самостоятельный аннотатор предложения, подключаемый к одному диспетчеру.

Реальные исходники (всё, что в `obj/` и `bin/` — артефакты сборки, не код):

- `C:/Projects/llm_mvp_core/28.SemanticNlp/Core/Core.cs` — `OperationResult<T>`, `BaseService`, `StandardEvent`, `IdempotencyStore`.
- `C:/Projects/llm_mvp_core/28.SemanticNlp/Core/Types.cs` — записи результата: `SemanticToken`, `SemanticAnnotation`, `Synset`, `AmrNode/AmrEdge/AmrGraph`, `DependencyToken/DependencyTree`.
- `C:/Projects/llm_mvp_core/28.SemanticNlp/Fabric/Interfaces.cs` — `ITextSource`, `ISemanticLexicon`, `ISemanticStorage`, `ISemanticEventLogger`, `ISemanticAnnotator`.
- `C:/Projects/llm_mvp_core/28.SemanticNlp/Fabric/InMemoryStorage.cs` — in-memory реализации фабрики (не читал полностью; назначение видно из state.json).
- `C:/Projects/llm_mvp_core/28.SemanticNlp/Dispatcher/SemanticNlpDispatcher.cs` — роутер «имя алгоритма → аннотатор».
- `C:/Projects/llm_mvp_core/28.SemanticNlp/Algorithms/FrameNet.cs` — `FrameNetLexicon`, `FrameDetector`, `FrameElementFiller`, `FrameNetAnnotator`, `FrameNetService`.
- `C:/Projects/llm_mvp_core/28.SemanticNlp/Algorithms/Srl.cs` — `PredicateFinder`, `ArgumentLabeler`, `SrlAnnotator`, `SrlService`.
- `C:/Projects/llm_mvp_core/28.SemanticNlp/Algorithms/WordNet.cs` — `SynsetDatabase`, `SynsetNavigator` (Wu-Palmer), `WordNetAnnotator`, `WordNetService` (по state.json; файл не открывал).
- `C:/Projects/llm_mvp_core/28.SemanticNlp/Algorithms/Amr.cs` — `AmrPenmanParser/Serializer`, `SimpleAmrBuilder`, `AmrAnnotator`, `AmrService` (по state.json; файл не открывал).
- `C:/Projects/llm_mvp_core/28.SemanticNlp/Algorithms/UniversalDependencies.cs` — `ConlluParser`, `DependencyTreeNavigator`, `RuleBasedParser`, `UD*Annotator/Service` (по state.json; файл не открывал).
- `C:/Projects/llm_mvp_core/28.SemanticNlp/state.json` — карта компонентов, статус, тесты (Gate A: 450/450 на 2026-04-30; тесты в `Tests/SemanticNlpTests.cs`, 29 тестов).

## Как устроено / работает (кратко)

Единый контракт `ISemanticAnnotator` (`AlgorithmName` + `AnnotateAsync(operationId, sentence)`)
реализуют пять `*Service`-классов. `SemanticNlpDispatcher` хранит `Dictionary<string, ISemanticAnnotator>`
и по имени маршрутизирует вызов (+ пишет событие `semantic.dispatch.requested`). Каждый `*Service`
наследует `BaseService` (фабрика: `ISemanticStorage` + `ISemanticEventLogger` + локальный
`IdempotencyStore`), реализует порядок «idempotency-check → annotate → save → event → record».

Внутри каждого алгоритма — конвейер из 2-3 чистых хелперов:

- FrameNet: `FrameDetector` голосует словами по фреймам (`frameVotes[frame] += 1`, победитель — макс. голосов, тай-брейк по имени `Ordinal`), затем `FrameElementFiller` раздаёт роли по статической позиционной таблице (до/после триггера: Attacker/Victim и т.п.).
- SRL: `PredicateFinder` ищет глаголы по `HashSet _knownVerbs` + суффиксные эвристики (`ed/ing/ize/...`); `ArgumentLabeler` метит токены позиционно (до предиката → A0, после → A1) плюс статические наборы маркеров `LocationMarkers/TimeMarkers/MannerMarkers`.
- WordNet/AMR/UD (по state.json): обратный индекс синсетов + Wu-Palmer; рекурсивный парсер Penman + «первый глагол = корень, ARG0/ARG1 = существительные до/после»; парсер CoNLL-U + rule-based назначение root/nsubj/obj/det.

Все словари/таблицы — seed-данные из конструктора (`CreateDefault()`), нигде не мутируются
во время работы. `IdempotencyStore` — кэш результата по `operationId`, не обучаемое состояние.

## Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

Средняя. Прямого отношения к авто-построению DPO или к графу обучаемых юнитов нет:
здесь нет обучения, DPO-троек, checkpoint, export/import, метрик reward/accuracy и
teacher-provenance. Чем полезно для плана ILearningDynamicGraph:

- Готовый источник семантических признаков/разметки (фреймы, роли A0/A1, синсеты, AMR-граф, дерево зависимостей), которые могут быть входными фичами для будущего обучаемого юнита — но сами по себе это детерминистические извлекатели, не authority.
- Образец проектных паттернов проекта: единый контракт + диспетчер-роутер + `BaseService` + событие-конверт + идемпотентность. План графа может переиспользовать этот стиль, но обязан заменить роутер/хелперы на разрешённые обучаемые уровни там, где требуется обучение.
- `AmrGraph`/`DependencyTree` — это графовые структуры данных предложения. Это НЕ «граф обучаемых юнитов» (узлы=юниты, рёбра=trainable-маршруты) из ILearningDynamicGraph; терминологию нельзя смешивать.

## Классификация кода (уровень vs детерминистический хелпер)

Все компоненты 28.SemanticNlp — детерминистические хелперы. Ни одного из разрешённых
обучаемых уровней (`ILearningMachine` / `ILogicalOrchestrator` / `ILogicalFlow` /
`ILogicalUnit` / `ILearningMachinePair`) в проекте нет. Это нормально: проект честно
позиционирован как набор rule-based NLP-аннотаторов и нигде не заявляет «trained/semantic
model». Опасность только в том, чтобы план ILearningDynamicGraph случайно не выдал их за
обучаемые узлы графа.

Классификация по правилам скила `trainable-architecture-decision-SKILL.md`
(«scorers/selectors/classifiers/dictionary/counter/regex... не являются архитектурными
паттернами; только internal detail внутри разрешённого уровня») и
`generated-code-review-SKILL.md` Layer 5A:

- `SemanticNlpDispatcher` — роутер по `Dictionary<string, ISemanticAnnotator>`. Internal detail / router, НЕ `ILogicalOrchestrator`. Цитата: `private readonly Dictionary<string, ISemanticAnnotator> _annotators;` + `_annotators.TryGetValue(algorithmName, out var annotator)`. Authority детерминистическая (exact-name lookup). Verdict: `OK_INTERNAL_DETAIL` (роутер честный, обучения не заявляет). Если в плане его захотят сделать оркестратором обучаемых узлов — `NEEDS_ILOGICALFLOW_OR_ORCHESTRATOR_WRAPPER`.
- `ISemanticAnnotator` / `*Service` (`FrameNetService`, `SrlService`, …) — полиморфный контракт инференса + фасад. Похож по форме на «юнит», но это НЕ `ILogicalUnit`: нет обучаемого состояния, нет TrainBatch/Export/Import, метод `AnnotateAsync` зовёт детерминистический конвейер. Verdict: `OK_INTERNAL_DETAIL` как rule-based аннотатор; для семантического/обучаемого заявления был бы `NEEDS_ILOGICALUNIT_WRAPPER`.
- `FrameDetector.DetectFrameAsync` — counter/vote selector. Цитата: `frameVotes[frameName] = frameVotes.GetValueOrDefault(frameName) + 1;` и `OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key)`. Это счётчик-голосование (правило скила: counter/selector = не паттерн). Verdict: `OK_INTERNAL_DETAIL` / при заявке на обучение — `BASELINE_MUST_BE_LABELLED`. В коде честно помечено «vote-based … deterministic».
- `FrameElementFiller.FillRoles` — статическая позиционная таблица ролей (`_frameRoles`, before/after trigger). Pattern-matching по позиции. Verdict: `OK_INTERNAL_DETAIL` (комментарий честно: «simple positional heuristics»).
- `PredicateFinder` — `HashSet _knownVerbs` (dictionary) + суффиксные эвристики (`CommonVerbSuffixes`). Dictionary/regex-like matching. Verdict: `OK_INTERNAL_DETAIL`; код честно: «works without any trained model».
- `ArgumentLabeler` — позиционная логика + статические keyword-наборы (`LocationMarkers/TimeMarkers/MannerMarkers`). Keyword matching + positional default. Verdict: `OK_INTERNAL_DETAIL`.
- `FrameNetLexicon` / `SynsetDatabase` (WordNet) — двусторонние индексы word↔frame / word↔synset из seed-словаря. Dictionary state, конструктор-seeded, без апдейта. Internal detail (lexicon), НЕ обучаемый уровень.
- `SynsetNavigator` Wu-Palmer, `RuleBasedParser` (UD), `SimpleAmrBuilder` (AMR), парсеры Penman/CoNLL-U — детерминистические алгоритмы/парсеры. Internal detail; обучения нет.
- `IdempotencyStore` — кэш результата по operationId. Internal detail, НЕ learned state (важно не спутать с checkpoint при описании lifecycle в плане).

Граф-edge замечание (по Graph-Style Reconstruction Gate скила): если план ILearningDynamicGraph
включит 28.SemanticNlp как узлы, на каждом ребре нужно явно указать владельца authority. Сейчас
вся authority детерминистическая (lookup/vote/position). Использование любого из этих хелперов
как финальной authority в обучаемом ребре без метки eval-only/feature-source — это
`FORBIDDEN_RUNTIME_SHORTCUT`; допустимо только как источник признаков/кандидатов или baseline.

Teacher-provenance: неприменимо (N/A) — компонент не генерирует и не использует teacher-выводы
для обучения; никакого закрытого/проприетарного учителя здесь нет.

## Пробелы / что доисследовать

- НЕ открывал полностью: `WordNet.cs`, `Amr.cs`, `UniversalDependencies.cs`, `Fabric/InMemoryStorage.cs`. Классификация по ним сделана по `state.json` + единому паттерну FrameNet/SRL; перед планом стоит подтвердить чтением, что там тоже нет скрытого обучаемого состояния (ожидается, что нет).
- Тесты `Tests/SemanticNlpTests.cs` не читал — нужны, если план будет опираться на текущее покрытие как на baseline-эвиденс (state.json заявляет 29 тестов, Gate A 450/450 на 2026-04-30; дату/счётчик стоит перепроверить, т.к. baseline в CLAUDE.md уже ≥1488).
- Не проверял, потребляет ли кто-то из `15.OrchestratorModel` / `42.ProjectWikiDomain` выходы `SemanticNlpDispatcher` (нужен grep по `ISemanticAnnotator`/`SemanticNlpDispatcher` за пределами проекта 28), чтобы понять, попадает ли 28 в реальный runtime-путь графа или остаётся изолированной библиотекой признаков.
- Открытый вопрос для плана: должны ли семантические аннотаторы стать источником входных фич для обучаемых узлов ILearningDynamicGraph, или их роль — только eval/корпус-разметка. От этого зависит, нужна ли вокруг них обёртка-`ILogicalUnit` или они остаются internal detail.
