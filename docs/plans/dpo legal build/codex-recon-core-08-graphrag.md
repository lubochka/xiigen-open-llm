# Codex Recon — core/08.GraphRag: Graph RAG и графовые паттерны (извлечение/композиция)

Рекон для плана ILearningDynamicGraph. Только evidence, не разрешение на реализацию.
Прочитаны все исходники области (6 .cs + .csproj), артефакты obj/bin пропущены. Скилы
trainable-architecture-decision и generated-code-review просмотрены ради правил классификации.

## Что это и где (точные ключевые пути)

- `C:/Projects/llm_mvp_core/08.GraphRag/GraphRag.csproj` — проект ссылается на
  `05.Embeddings`, `06.VectorSearch`, `07.GraphEngine` (граф + PageRank + векторный индекс
  живут в соседних проектах, 08 их только оркеструет под RAG).
- `C:/Projects/llm_mvp_core/08.GraphRag/Indexer.cs` — построение индекса: обходит узлы
  `KnowledgeGraph`, эмбедит текст узла, кладёт вектор+метаданные в `IVectorIndex`.
- `C:/Projects/llm_mvp_core/08.GraphRag/LocalSearch.cs` — локальный (семантический) поиск:
  эмбед запроса -> ближайшие векторы -> узлы графа.
- `C:/Projects/llm_mvp_core/08.GraphRag/GlobalSearch.cs` — глобальный поиск: seed-узлы по
  векторной близости -> personalised PageRank -> структурно важные связанные узлы.
- `C:/Projects/llm_mvp_core/08.GraphRag/HybridRetriever.cs` — слияние local+global,
  дедуп по `NodeId`, взвешенный ранг, сборка контекстного текста под бюджет токенов.
- `C:/Projects/llm_mvp_core/08.GraphRag/Models/SearchResult.cs` — record результата
  (NodeId, Name, Type, RawText, VectorScore, GraphScore, CombinedScore, Source).
- `C:/Projects/llm_mvp_core/08.GraphRag/Models/RetrievalContext.cs` — record выдачи
  (Query, Results[], ContextText, TokenCount).

## Как устроено / работает (кратко)

Конвейер из двух стадий: индексация и извлечение.

1. Индексация (`Indexer.IndexGraphAsync`): для каждого узла графа берётся текст
   (`RawText` -> иначе `Signature` -> иначе `Name`), `EmbeddingModel.Embed` даёт вектор,
   вектор+метаданные (name/type/filePath) пишутся в `IVectorIndex.Add`. Граф строится
   полностью до индексации (DNA rule 8, комментарий в файле).

2. Извлечение — два независимых пути по тому же графу/индексу:
   - Local (`LocalSearch.Search`): `Embed(query)` -> `IVectorIndex.Search(vec, topK)` ->
     отбор узлов, существующих в графе -> `CombinedScore = vectorScore + node.PageRankScore`,
     сортировка по убыванию. Это «семантическая близость + статическая важность узла».
   - Global (`GlobalSearch.Search`): seed-узлы из `IVectorIndex.Search(vec, seedK=5)` ->
     `PageRankCalculator.PersonalisedPageRank(graph, seeds)` -> top-K по PPR-скору. Это
     «обход графа от семён по рёбрам» (структурная экспансия за пределы прямого совпадения).

3. Композиция (`HybridRetriever`): local и global сливаются в `Dictionary<NodeId,...>`;
   local-скор умножается на `LocalWeight=0.6`, global-скор на `GlobalWeight=0.4`; при
   совпадении узла скоры складываются и `Source="hybrid"`. Финальный ранг —
   `OrderByDescending(CombinedScore).Take(topK)`. `BuildContext` склеивает
   `[Type] Name\nRawText` сниппеты, пока не упрётся в `MaxTokens=3000` (~*4 символа).
   `TokenCount` = длина/4 (грубая оценка). Итог — `RetrievalContext`.

Все веса/пороги — именованные константы (`LocalWeight`, `GlobalWeight`, `MaxTokens`,
`seedK`, `topK`); ни одного обучаемого параметра, состояния, чекпойнта или импорта/экспорта
в этой области нет.

## Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

- Графовый паттерн как таковой: это «retrieval-граф» (KnowledgeGraph + PageRank), а НЕ
  граф обучаемых решений/рёбер уровня ILearningDynamicGraph. Узлы = сущности кода/доков,
  рёбра = структурные связи; авторитет ранжирования — детерминированная арифметика скоров.
  Это удобная аналогия/донор паттерна «fan-out стадий (local/global) -> fan-in (merge)»,
  но без edge-level trainable authority, без async-графа решений, без контрактов
  input/output между обучаемыми узлами (см. Graph-Style/DynamicGraph Reconstruction Gate
  в trainable-architecture-decision-SKILL.md, строки 41-66).
- Как кандидат-генератор/feature-source для будущих обучаемых юнитов — пригоден: KNN/
  retrieval-компонент допустим внутри разрешённого уровня (skill, строки 112-117). То есть
  `LocalSearch`/`GlobalSearch`/`HybridRetriever` могли бы стать retrieval/memory-слоем ВНУТРИ
  `ILogicalUnit`/`ILearningMachine`, поставляя контекст/кандидатов.
- DPO/обучение: точек обучения нет (нет chosen/rejected, нет training update, нет reward).
  `CombinedScore`-слияние с весами 0.6/0.4 — это место, где плохой план мог бы заявить
  «обучаемый скоринг»; на самом деле это фиксированная линейная комбинация (кандидат на
  REPORT_CLAIM_TOO_STRONG, если кто-то назовёт это learned ranking).
- Измерение/teacher-provenance: отсутствуют полностью (нет метрик, нет teacher, нет
  provenance-полей). Для плана это пробел-«поставщик», а не нарушение здесь.

## Классификация (по уровням ILearningMachine vs детерминированный хелпер)

Правило: разрешённые trainable-уровни — только `ILearningMachine`/`ILogicalOrchestrator`/
`ILogicalFlow`/`ILogicalUnit`/`ILearningMachinePair`; всё остальное — internal detail,
если не обёрнуто в уровень и не имеет lifecycle-evidence (skill строки 95-117, 141-147).
Ни один из найденных классов НЕ реализует ни один из этих интерфейсов (проверено: в файлах
нет `: ILogical*`/`: ILearning*`, конструкторы принимают конкретные `EmbeddingModel`,
`PageRankCalculator` и интерфейс `IVectorIndex`).

- `Indexer` — внутренний детерминированный хелпер (ETL индексации). Уровень: нет.
  Verdict: OK_INTERNAL_DETAIL. Чисто построение индекса, обучаемой авторитетности нет.
- `LocalSearch` — детерминированный retrieval-хелпер (эмбеддинг-KNN + статический PageRank
  как фича). Verdict: OK_INTERNAL_DETAIL / потенциально OK_MODEL_COMPONENT, если будет
  вложен в разрешённый уровень и его роль (candidate retriever) названа честно (skill 112-117,
  199). Сам по себе авторитетом trainable-решения быть не должен.
- `GlobalSearch` — детерминированный графовый retrieval-хелпер (PPR-экспансия). Verdict:
  OK_INTERNAL_DETAIL / OK_MODEL_COMPONENT при вложении в уровень. Honest label = graph
  candidate retriever.
- `HybridRetriever` — детерминированный merge/rank/context-builder с фиксированными весами
  `0.6/0.4`. Это похоже на «оркестрацию» (две стадии -> слияние), НО без обучаемой
  маршрутизации это НЕ `ILogicalFlow`/`ILogicalOrchestrator`, а internal detail.
  Verdict: OK_INTERNAL_DETAIL сейчас; BASELINE_MUST_BE_LABELLED, если в плане его назовут
  «умным/семантическим ранжированием». Если бы такая фиксированная линейная комбинация
  стала финальной авторитетностью там, где требуется обучаемый юнит — это
  FORBIDDEN_RUNTIME_SHORTCUT (skill 204-205). Сейчас претензии на semantic quality в коде
  нет, поэтому нарушения нет; это риск формулировок плана.
- `SearchResult`, `RetrievalContext` — пассивные record-DTO. Уровень: нет, не применимо.

deterministic-vs-learned boundary: вся область — детерминированная (эмбеддинг-близость +
PageRank + фиксированные веса). Обучаемого слоя нет; граница «learned» = N/A в текущем коде,
обучаемость появляется только если эти хелперы станут компонентами внутри разрешённого
уровня ILearningDynamicGraph/ILogicalUnit.

## Пробелы / что доисследовать

- Кто вызывает 08.GraphRag в проекте? В прочитанных файлах нет потребителя — надо
  grep по решению (`new HybridRetriever`, `LocalSearch`, `GlobalSearch`, `Indexer`,
  `RetrievalContext`), чтобы понять, подключён ли GraphRag к оркестрации/42.ProjectWikiDomain
  или это изолированная библиотека. Без этого нельзя утверждать роль в runtime.
- `07.GraphEngine` (`KnowledgeGraph`, `GraphNode`, `PageRankCalculator`,
  `PersonalisedPageRank`) и `06.VectorSearch` (`IVectorIndex`) не прочитаны (вне области
  задания) — структура узлов/рёбер и контракт PPR там; для DynamicGraph-реконструкции
  (edges/contracts/authority) их надо отдельно отрекогносцировать.
- `Rule 1 — OperationResult<T>`: методы здесь возвращают `RetrievalContext`/`List<...>`/
  `Task`, а не `OperationResult<T>` (нарушение проектного правила Rule 1). Это для плана —
  тех-долг области, не относится к графовому вопросу, но стоит зафиксировать.
- Нет тестов в самой папке 08.GraphRag — где покрытие (unit/e2e) для local/global/hybrid?
  Доисследовать в тестовых проектах решения.
- Реальный размер «токенов» считается как `length/4` — это грубая эвристика, не токенайзер;
  для плана измерения/бюджета это слабое место, если потребуется точность.
