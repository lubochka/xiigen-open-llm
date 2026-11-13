
# N36 — новое: GlobalSearch возвращает «доказательства» при нуле законных vector seeds

**16.10.2026 · SOURCE-CONFIRMED control flow + EXECUTED JS SOURCE-LOGIC PROXY; native .NET NOT_RUN; actual product quality impact HYPOTHESIS; RP01 causality NOT_ESTABLISHED.**

Источники pinned core [216a1f3d6af3f40ceef081dc168b07e4ce3934f9](https://github.com/lubochka/llm_mvp_core/tree/216a1f3d6af3f40ceef081dc168b07e4ce3934f9). Это отдельный механизм от N35 prefix/metadata/fallback, [N28 build-context early break](28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md), HNSW lost-lesion and generic KNN scope. Запись только в xiigen-planning/llm_plan3/.../possibleDifs. core/docs/plans/replanning-possible-diffs НЕ МЕНЯТЬ.

## 1. Человеческое объяснение

В графе могут лежать десятки знаний. Если смысловой поиск не нашёл **ни одного** подходящего узла (или индекс вернул ID, которого в графе уже нет), правильным результатом без другого подтверждённого evidence может быть «не знаю». Нынешняя цепь GlobalSearch создаёт PageRank оценки для **всех** узлов даже при пустом списке реальных seeds, затем отдаёт первые topK в downstream HybridRetriever. Нерелевантный контекст может выглядеть как найденное знание и затем быть принят consumer или evaluator за успешную работу памяти.

**Что доказано исходниками:** при непустом graph и нуле валидных seeds GlobalSearch может вернуть graph nodes, а HybridRetriever — их текст. **Не доказано:** что данный consumer участвует в offline RP01, что реальные индексы попадают в это состояние, либо что final answer изменился/качество ухудшилось.

## 2. Кодовая цепь с точными условиями

1. [GlobalSearch.cs L30–44](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/08.GraphRag/GlobalSearch.cs#L30-L44): запрос кодируется EmbeddingModel, vectorIndex.Search(queryVec, seedK) возвращает IDs, далее _pageRank.PersonalisedPageRank(_graph, seeds) **вызван без проверки непустоты и принадлежности IDs графу**.
2. [PageRankCalculator.cs L53–105](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/07.GraphEngine/PageRankCalculator.cs#L53-L105): n=0 возвращает пустой список, но n>0 делает power iteration и **возвращает ВСЕ graph node IDs по rank**, независимо от seedSet.Count. При seedSet=empty teleport равен нулю, dangling mass распределяется по всем graph nodes. При seedSet, содержащем только отсутствующие IDs, teleport mass для всех graph IDs тоже ноль; это может привести к нулевому rank, но IDs всё равно перечисляются.
3. [GlobalSearch.cs L40–61](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/08.GraphRag/GlobalSearch.cs#L40-L61): сразу Take(topK), TryGetNode и создание SearchResult (включая RawText). Graph nodes существуют даже когда ANN seeds не существуют. Нет проверки GraphScore>epsilon, семантической применимости/отсутствия seeds.
4. [HybridRetriever.cs L30–43, L63–109](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/08.GraphRag/HybridRetriever.cs#L30-L109): MergeAndRank принимает глобальные результаты и BuildContext добавляет их snippets независимо от нулевого/минимального GraphScore; наличие текста не равно grounded retrieval.

**Строгое построение условия:** KnowledgeGraph содержит A,B,C с непустыми RawText; IVectorIndex.Search возвращает empty (например, индекс еще пуст, graph уже построен). GlobalSearch вызывает PPR(empty), получает 3 node IDs, возвращает 3 SearchResults при topK>=3; HybridRetriever добавляет эти snippets в ContextText. Изолированный пустой индекс разрешён интерфейсом и [BruteForceIndex.Search](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/06.VectorSearch/BruteForceIndex.cs#L23-L31). Никакого actual learned lesson для результата не требовалось.

## 3. Два отличающихся отказа

**N36-A · EMPTY_SEED_LEAK.** ANN не вернул кандидатов, но GraphRAG глобальная ветка заполняет context произвольными узлами по residual PageRank. Если ноды из другой domain family, это может ухудшить качество final-answer и создать иллюзию, что GraphRAG «обучен» и на незнакомый вопрос.

**N36-B · STALE_ID_ZERO_SCORE_LEAK.** ANN вернул несуществующий node ID: индекс и knowledge graph были обновлены неатомарно или база восстановилась не той версии. PPR seedSet.Count>0, но ни одному действующему graph node не принадлежит seed; все законные teleport=0, при отсутствии корректного graph link PageRank scores могут затухнуть до 0; GlobalSearch все равно возвращает существующие графовые узлы. Это новая разновидность stale-index эффекта: **не просто низкий KNN recall, а ложный возврат чужих nodes**.

**Доменная область:** любой GraphRAG consumer с empty/stale ANN seeds; особенно опасно C# (неверный метод/токен), SQL (чужая schema/dialect), DTC (другая Angular version), перевод (другой direction). AST/POS/order здесь НЕ первичная правка: сначала остановить ложный path при отсутствии evidence. В прямом RP01 Student-only LoRA этот путь **не установлен**.

## 4. Уже выполненный source-only математический proxy — НЕ native quality

Изолированная JS реконструкция ровно power iteration PPR для трёх **изолированных** graph nodes A/B/C, damping=0.85, delta stop=1e-6, maxIter=100 дала:

| seed case | Векторные seeds в графе | PPR итераций | Сумма рангов при stop | Возвращённые ID |
| --- | --- | --- | --- | --- |
| Empty seedSet | 0 | 75 | 0.0000050865 | A,B,C |
| Stale ID only | 0 (один nonexistent X в seedSet) | 2 | 0 | A,B,C |
| One valid seed B (negative control) | 1 | 2 | 1 | A,B,C (sorted B first) |

**JS proxy status:** исходные численные формулы скопированы смыслово, не выполнялись C# GlobalSearch/HybridRetriever. В реальном коде при пустых seeds список нод по PPR тоже непустой, даже если score близок к нулю; нативный experiment должен проверить final ContextText. Сохранённые [proxy code](36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY.mjs) и [receipt](36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY-RESULT.json). Числа игрушечного графа НЕ модельная точность.

## 5. Быстрый причинный фальсификатор

[MICRO-N36 protocol](36-MICRO-SEEDLESS-PAGERANK-DOMAIN-EXPERIMENT-2026-10-16.md) — 3 native isolated graphs + 6–12 blind domain queries:

- A: existing GlobalSearch/HybridRetriever, empty or stale-only seeds.
- B: только one-factor pre-PPR filter: брать только seeds, которые существуют в graph; если validCount=0, возвращать empty GlobalSearch result (не fabricated graph snippets). Same learned memory, graph, PageRank, weights, tokenizer, oracle, topK.
- C valid-seed control: A/B обязаны давать одинаковые результаты.
- L0: source condition, candidate IDs and actual ContextText; L1 только при достижимом consumer: correct/wrong/abstained/critical after blind independent law judge. При нуле final gains статус NO_QUALITY_GAIN_OBSERVED, несмотря на сильный safety relevance.
- Нельзя считать отсутствие ответа после B автоматическим «качеством»; считать false abstentions/coverage и те случаи, где был доступен верный law.

**Verdict:** C_SOURCE_SEEDLESS_RANKING_LEAK, PROXY_JS_SOURCE_EQUIVALENT; NATIVE_NOT_RUN; RP01_ROOT_CAUSE_NOT_PROVEN. Приоритет P1 только при живом GraphRAG consumer, P2 пока route unknown.

**DPO roles:** человеческую выборку задач выполняет CODEX с Astra 6 UltraCode; обе финальные chosen/rejected — исключительно внешняя допущенная открытая китайская модель; независимо оценивает пары и реальный результат CODEX/Astra 6 UltraCode. Ни teacher, ни CODEX independent judge, ни Student здесь не запускались; не подменять ими source-only proxy.
