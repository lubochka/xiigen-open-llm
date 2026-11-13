
# N35 — новые границы GraphRAG: суффикс, метаданные и скрытый fallback embedding

**16.10.2026. SOURCE_CONFIRMED conditions; native .NET NOT_RUN; quality impact HYPOTHESIS; direct RP01 causality NOT_PROVEN.** Source core: [216a1f3d6af3f40ceef081dc168b07e4ce3934f9](https://github.com/lubochka/llm_mvp_core/tree/216a1f3d6af3f40ceef081dc168b07e4ce3934f9), branch codex/dpo-implementation. Запись только в xiigen-planning/llm_plan3/.../possibleDifs; core/docs/plans/replanning-possible-diffs — строго READ ONLY.

## Человеческое объяснение

Можно действительно принять новый урок, сохранить его как GraphNode и даже правильно обновить LoRA, но потерять важное условие еще ДО KNN. GraphRAG строит vector из выбранного текстового поля узла; слишком длинный текст усечется до первых BPE-токенов. Поэтому два правила с противоположными условиями могут иметь одинаковые learned vector features. Это не тот же объект, что offline DPO Student в RP01, и пока не доказан его вызов из RP01.

Проверенная цепь исходников:

1. [GraphRag.Indexer.cs L23–44](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/08.GraphRag/Indexer.cs#L23-L44): node → BuildNodeText → injected EmbeddingModel.Embed → IVectorIndex.Add(node.Id).
2. [EmbeddingModel.cs L22–46, L77–101](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/05.Embeddings/EmbeddingModel.cs#L22-L101): BPE ids → Take(maxLen) → LlmModel.ForwardHidden → masked mean → normalization; maxLen=512 только **значение конструктора по умолчанию**, GraphRAG получает embedder извне и может использовать другую длину.
3. [LocalSearch.cs L24–54](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/08.GraphRag/LocalSearch.cs#L24-L54): query → Embed → index.Search → GraphNode → vector score + PageRank.
4. [GlobalSearch.cs L30–42](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/08.GraphRag/GlobalSearch.cs#L30-L42): тот же vector дает seeds для PersonalisedPageRank; граф **может** компенсировать часть ошибки, но это требуется измерить.
5. [HybridRetriever.cs](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/08.GraphRag/HybridRetriever.cs): соединяет local/global. Не утверждать, что downstream consumer обязан выдавать одинаковые ответы из одинаковых vectors.

## N35-G1 — усечение семантически значимого окончания (P1, если GraphRAG действительно потребляется)

**C — подтверждено:** при фиксированных model/tokenizer/state и одинаковых первых maxLen BPE-IDs в ForwardHidden поступает одинаковый вход. В ОБЫЧНОЙ ветке ненулевой нормы итоговые learned embeddings равны даже для разных финальных условий. Векторный nearest neighbor по ним не различит смысл хвоста. Однако если активируется отдельный fallback G3, vectors могут различаться: нельзя писать «всегда одинаковы».

**H — ожидаемая потеря качества:** запросы и graph nodes с C# nullability/AST-guard, SQL JOIN ON-vs-WHERE/NULL law, EN→RU negation/modal verb/actor, DTC-IDE ограничением после длинного спецификационного контекста попадут в неверный candidate set. Нужен фактический constructor maxLen, законный семантический counterexample с удостоверенным BPE prefix, один фиксированный индекс, затем law-correct recall/ответ.

**Не путать с [N23 64-token Transformer-KNN](23-TRANSFORMER-KNN-64TOKEN-PREFIX-ALIAS-2026-10-16.md):** в том Unit явный maxLen=min(64,ContextLen), после ANN есть lexical reranking, а тут **отдельный GraphRAG.Indexer + Local/GlobalSearch**, другой topK/graph rescue и потенциальный default512. Это не новая измеренная ошибка RP01.

## N35-G2 — RawText исключает Name/Signature из vector (P1 при релевантной метаинформации)

**C — исходник:** [Indexer.BuildNodeText L39–44](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/08.GraphRag/Indexer.cs#L39-L44) выбирает: непустой RawText → его одного, иначе Signature → его одного, иначе Name. В [Indexer L26–37](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/08.GraphRag/Indexer.cs#L26-L37) name/type/filePath хранятся в index metadata, однако не входят в материал EmbeddingModel; Signature вообще не входит в metadata того Add. До ANN ни LocalSearch, ни GlobalSearch не превращают Signature в поисковый вектор.

**Сильный source counterexample:** две разные ноды с одинаковым непустым RawText и различными значимыми Signature или Name получают **одинаковый input text** и обычно идентичный learned embedding. Если отличие в Signature действительно задает C# overloaded method/type, SQL dialect, DTC version/ownership, ANN может не отличать смысл. **Но если RawText уже содержит различение, потери может не быть.**

**Отличие от [N28 GraphRAG evidence packing](28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md):** прежний дефект — потеря УЖЕ найденного result при BuildContext break; этот — потеря нужного признака еще до ANN/index.Search.

## N35-G3 — незаметная подмена learnt embedding на псевдосемантические token buckets (P2 conditional)

**C — условная ветка:** [EmbeddingModel.EnsureNonZeroUnitVector L49–74](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/05.Embeddings/EmbeddingModel.cs#L49-L74). Если после pooling/L2 norm <=1e-6, вектор строится заново: для КАЖДОГО BPE token ID исходного **полного** текста увеличивается coordinate abs(tokenId)%hiddenDimension, затем выполняется нормализация. Для пустого Encode используется StringComparer.Ordinal.GetHashCode(text), но **достижимость пустого пути отдельно проверить**: при пустой строке Tokenize может дать T=0 и упасть до fallback. Код не возвращает признак fallbackUsed.

**H:** нулевые encoder vectors могут казаться полезным ANN по детерминистическому bag-of-hashed-BPE, хотя изменения обученной модели не участвуют в поиске. Более того, эта ветка использует **полный** текст и может различить suffix, который G1 теряет. Без branch-observation нельзя корректно агрегировать результаты G1 и G3 или приписывать улучшение LoRA. Ни частота fallback, ни cross-process поведение хеша в текущем runtime, ни качество не измерены. Это НЕ намеренный token-hash KnnLogicalUnit, исследованный в N21, и НЕ доказанный дефект DPO Student.

## История обучения, область переноса

- [E09 AST-KNN saved run](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json): reported .2413→.3507. [Сверка E07/matrix→KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) обнаружила несовпадающую baseline/enhanced evaluation panel; положительный исторический **сигнал**, НЕ matched causal improvement, и не GraphRAG/LoRA.
- [E26 contextual dictionary](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json): .40→1.00 на 15 случаях; контекстный POS полезен в EN→RU, не универсальная функция C# KNN.
- [RP01 learner recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): C#500 0/3→0/3; ENRU500 0/4→0/4; LoRA изменилась; обычное улучшение не доказано. Прямой путь CodeDocumentationSimulationDpoExperiment → GraphRAG этим аудитом **не установлен**.

**Governance ролей:** человеческий выбор примеров выполняет CODEX на Astra 6 UltraCode; внешняя разрешенная открытая китайская модель создает **обе** окончательные стороны DPO chosen и rejected; их независимую проверку и оценку реальных outputs выполняет CODEX на Astra 6 UltraCode. Astra-simulated teacher, использованный в некоторых RP01 dev/run, не равен external Chinese author. Здесь DPO author/judge/Student NOT_CALLED.

**План [MICRO-N35](35-MICRO-GRAPHRAG-PREFIX-METADATA-AND-FALLBACK-DOMAIN-EXPERIMENTS-2026-10-16.md)** разделяет (1) source-алгоритм, (2) native retrieval, (3) law correctness обычного consumer. Одного доказанного совпадения vector недостаточно, чтобы объявить loss of learning.

**Вердикт:** G1 = C_SOURCE_TRUNCATION + H_QUALITY; G2 = C_SOURCE_FIELD_PRIORITY + H_QUALITY; G3 = C_SOURCE_CONDITIONAL_HASH_FALLBACK + H_RUNTIME_REACHABILITY; direct RP01 cause NOT_PROVEN; native NOT_RUN. Предпочтительно сначала P0 RP01 cold-start/DPO-sanity, параллельно диагностировать настоящий GraphRAG consumer.
