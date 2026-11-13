# Codex Recon — 07.GraphEngine (основа для ILearningDynamicGraph)

corpus: core · slug: core-07-graph · relevance: medium

## Что это и где (точные ключевые пути)

Проект `07.GraphEngine` — это маленький, чисто детерминированный графовый
движок над кодовыми сущностями (классы/методы/файлы/интерфейсы). Никакого
обучения, DPO, оркестрации или иерархии `ILearningMachine` внутри нет.

Реально найденные исходники (5 файлов, без obj/bin):
- `C:/Projects/llm_mvp_core/07.GraphEngine/GraphEngine.csproj` — ссылается на
  `05.Embeddings` и `06.VectorSearch`; net9.0, nullable on. ML-фреймворков нет.
- `C:/Projects/llm_mvp_core/07.GraphEngine/Models/GraphNode.cs` — `record GraphNode`.
- `C:/Projects/llm_mvp_core/07.GraphEngine/Models/GraphEdge.cs` — `record GraphEdge`.
- `C:/Projects/llm_mvp_core/07.GraphEngine/KnowledgeGraph.cs` — контейнер графа + API.
- `C:/Projects/llm_mvp_core/07.GraphEngine/PageRankCalculator.cs` — PageRank + Personalised PageRank.
- `C:/Projects/llm_mvp_core/07.GraphEngine/GraphSerializer.cs` — JSON-персистентность (atomic write).

Потребители в репозитории (не worktree):
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/*` (ProjectDocumentationPipelineOrchestrator,
  DocumentationOrchestrator, ProjectWikiCoverageVerifier и др.) — граф документации.
- `C:/Projects/llm_mvp_core/Tests/GraphEngineTests.cs`, `Tests/GraphEngineEdgeCaseTests.cs`,
  `Tests/IntegrationTests.cs` — тесты движка.
- Документация: `C:/Projects/llm_mvp_core/docs/projects/07-graph-engine.md`,
  `C:/Projects/llm_mvp_core/docs/projects/08-graph-rag.md`.
- ВАЖНО (evidence-only, НЕ мой таргет, не редактировать):
  `C:/Projects/llm_mvp_core/docs/plans/claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md`
  уже существует как claude-* план по теме ILearningDynamicGraph.

## Как устроено / работает (кратко)

Структуры данных:
- `GraphNode` (record, init-свойства): `Id, Name, Type ("Class|Method|File|Interface"),
  FilePath, Signature, RawText, float[] Embedding`, и мутируемое
  `double PageRankScore { get; set; }` (единственное изменяемое поле).
- `GraphEdge` (record): `From, To, RelationType ("Calls|Defines|Inherits|Implements"),
  double Weight = 1.0`. Направленное взвешенное ребро.

`KnowledgeGraph` (ориентированный взвешенный граф):
- Хранение: `Dictionary<string,GraphNode> _nodes`, плюс смежность
  `Dictionary<string,List<GraphEdge>> _outEdges` и `_inEdges`.
- API: `AddNode`, `AddEdge` (бросает `InvalidOperationException`, если конец ребра
  отсутствует), `OutEdges(id)`, `InEdges(id)`, `Neighbours(id)`,
  `TryGetNode(id, out node)`, `Nodes` (read-only), `NodeCount`, `EdgeCount`.
- Замечание по DNA: API возвращает голые типы/`IEnumerable`, а не
  `OperationResult<T>`; `AddEdge` бросает исключение (Rule 1 не соблюдён здесь —
  это структурная библиотека, а не публичный сервис).

`PageRankCalculator` (детерминированная численность):
- `ComputePageRank(graph)`: степенная итерация, damping d=0.85, порог 1e-6,
  ≤100 итераций; учитывает dangling-узлы; пишет результат обратно в
  `node.PageRankScore`.
- `PersonalisedPageRank(graph, seeds)`: телепортация распределяется по seed-узлам;
  возвращает `IReadOnlyList<(string id, double score)>`, отсортировано по убыванию.

`GraphSerializer` (static): `Save`/`Load` графа в JSON через приватный
`SerializedGraph {Nodes, Edges}`; запись атомарная (`.tmp` → `File.Move(overwrite)`).

## Релевантность к ILearningDynamicGraph

Релевантность — СРЕДНЯЯ. Это естественный «скелет графа», но НЕ динамический
обучаемый граф паттернов.

Что переиспользуемо как фундамент:
- Готовые типы узла/ребра и контейнер с двусторонней смежностью (out/in) —
  база для представления узлов-паттернов и рёбер-переходов.
- Personalised PageRank — кандидат на роль ДЕТЕРМИНИРОВАННОГО ретривера/ранкера
  кандидатов (seed → ранжированные соседи) ВНУТРИ обучаемого уровня; не как
  финальная семантическая власть.
- Atomic JSON persistence — образец для сохранения состояния графа/чекпойнта
  (но без manifest/checkpoint/export-import/locator, которых требует обучаемый
  юнит).

Чего НЕТ (ключевые разрывы относительно цели):
- НЕТ обучения/DPO: нет training-loop, learned state, reward/loss,
  chosen/rejected, обновляемых весов от данных. `GraphEdge.Weight` задаётся
  вручную (по умолчанию 1.0) и неизменяем (init) — это не обучаемый вес.
- НЕТ оркестрации/потоков: нет `ILogicalUnit/Flow/Orchestrator/ILearningMachine`,
  нет fan-out/fan-in, async-исполнения по рёбрам, multi-output контрактов,
  владельца финальной власти на ребре.
- НЕТ «динамичности» в смысле обучения: граф строится извне и persist-ится;
  сам движок не наращивает/не переобучает паттерны.
- НЕТ teacher/provenance: телепортация/PageRank — чистая математика, источников
  обучения и легального teacher-gate нет.

## Классификация (по правилам скилов)

Применяю `trainable-architecture-decision-SKILL.md` (5 разрешённых уровней +
KNN/retrieval note + Graph-Style/DynamicGraph Reconstruction Gate) и
`generated-code-review-SKILL.md` Layer 5A.

- `KnowledgeGraph`, `GraphNode`, `GraphEdge`, `GraphSerializer`:
  **внутренний детерминистический хелпер / структура данных** (НЕ архитектурный
  уровень). Это пассивный контейнер графа и его сериализация. Verdict по скилу:
  `OK_INTERNAL_DETAIL` — допустимо как внутренняя деталь данных внутри
  разрешённого уровня; честно подписать как структура/хранилище, не как
  обучаемую сущность.
- `PageRankCalculator` (`ComputePageRank`, `PersonalisedPageRank`):
  **детерминистический скорер/ранкер** (степенная итерация). По скилу скореры/
  ранкеры/ретриверы НЕ являются архитектурным паттерном. Допустим только как
  внутренний компонент/ретривер кандидатов внутри разрешённого уровня и при
  честной подписи. Verdict: `OK_INTERNAL_DETAIL` / `OK_MODEL_COMPONENT` (как
  retrieval-компонент). Если план/отчёт назовёт PageRank «обучаемым ранжированием
  паттернов» или финальной семантической властью графа — это
  `FORBIDDEN_RUNTIME_SHORTCUT` / `REPORT_CLAIM_TOO_STRONG`, и требуется обёртка
  разрешённым уровнем (`ILogicalUnit`/`ILogicalFlow`/`ILearningDynamicGraph`) с
  доказательствами жизненного цикла (training data, checkpoint/manifest,
  export/import, fresh-load, continue-training, ablation/no-shortcut).
- Весь проект `07.GraphEngine` в целом: НЕ соответствует ни одному из пяти
  разрешённых обучаемых уровней (`ILearningMachine`, `ILogicalOrchestrator`,
  `ILogicalFlow`, `ILogicalUnit`, `ILearningMachinePair`). Это
  детерминированная graph+PageRank библиотека.

Граница deterministic-vs-learned для будущего ILearningDynamicGraph:
структура графа и PageRank остаются детерминированными хелперами/ретривером;
финальная власть по выбору/переходу паттерна (chosen vs rejected) ДОЛЖНА
принадлежать обучаемому уровню, а не `Weight`/PageRank-порядку.

## Пробелы / что доисследовать

- `Tests/GraphEngineTests.cs` и `GraphEngineEdgeCaseTests.cs` — НЕ читал;
  проверить фактическое поведение/контракты и краевые случаи (пустой граф,
  dangling, висячие рёбра).
- `42.ProjectWikiDomain/ProjectDocumentationPipelineOrchestrator.cs` и соседние —
  как именно строится граф (узлы/рёбра, веса) и где вызывается PageRank; это
  покажет реальный «исходник» данных для возможного DPO.
- `05.Embeddings` / `06.VectorSearch` — роль `GraphNode.Embedding`: используется ли
  семантическое сходство (кандидат на KNN-ретривер внутри обучаемого уровня).
- `docs/projects/07-graph-engine.md`, `docs/projects/08-graph-rag.md` — заявленный
  замысел Graph-RAG (расхождение «план vs код»).
- `docs/plans/claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md` — прочитать КАК
  EVIDENCE-ONLY (claude-* файл; не редактировать), чтобы не дублировать решения
  и не противоречить уже зафиксированной архитектуре графа паттернов.
- Открытый архитектурный вопрос: где будут жить узлы-ПАТТЕРНЫ и рёбра-ПЕРЕХОДЫ
  обучаемого графа — расширять `GraphNode/GraphEdge` или вводить отдельные
  типизированные обучаемые записи состояния (typed trained state) поверх этого
  контейнера.
