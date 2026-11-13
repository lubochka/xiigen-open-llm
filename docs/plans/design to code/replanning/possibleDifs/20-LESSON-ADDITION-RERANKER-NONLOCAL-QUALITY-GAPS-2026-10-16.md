# 20 — Дополнительный независимый аудит: не-локальные регрессии обученной KNN-памяти и ложный структурный сигнал

Дата: 2026-10-16. Статус: **SOURCE-CONFIRMED MECHANISMS + ANALYTICAL COUNTEREXAMPLES; RP01 CAUSALITY NOT PROVED; NATIVE EXPERIMENT NOT RUN**.

## Цель, роль в уже опубликованном possibleDifs

Это **новые адресные гипотезы**, а не переписывание [44 DIF](01-PRIORITIZED-DIFFERENCES.md), [matrix→KNN](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md), [known tier normalization](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md) или [consumer bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md). Мы различаем:
- RP01 **Student**: exact-v2 DPO → q/v LoRA → Transformer GenerateDetailed, без доказанного вызова этой KNN-памяти;
- generic `KnnLogicalUnit<T>`: 64D hashed features, HNSW и first-hit output;
- **`TransformerEmbeddingKnnLogicalUnit<T>`**: обучаемая память примеров, Transformer embeddings + HNSW + собственный E34V2-like reranker. Здесь **не** first-hit: вычисляются `ScoreCandidates`, выбирается его лучший элемент.

Различные маршруты и checkpoint owners нельзя объявлять одним и тем же learner. Старые успешные E51A/E51B/E52 и обученные переводческие Units — положительная историческая улика, но не равны свежей neural generalization. Перед изменением RP01 проверить user-facing call chain.

## NEW-20.01 — не-локальная деградация после добавления нерелевантного урока из-за pool-relative lexical normalization

**Наблюдение в коде (C):**
- [Train 98–165](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L98-L165): Unit строит память `_examples`, encoder embeddings и HNSW; Train — построение Unit из набора примеров, **не доказанный инкрементальный update существующего объекта**.
- [Execute 169–210](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L169-L210): берётся набор ANN-кандидатов, затем `ScoreCandidates`, ответ копируется из выбранного сохранённого example.
- [ScoreCandidates 217–244](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L217-L244):
  `maxLexical = Max(1e-9, max(candidates.lexicalScore))`, затем `normalizedLexical = clamp(lexical/maxLexical)`; score = **0.45 vector + 0.35 normalizedLexical + 0.20 pattern**.
- [BM25-like 246–274](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L246-L274): lexicalScore зависит также от общего `_examples.Count` и частот документов, значит добавление примера способно изменить оценку **старых** кандидатов даже при неизменном query.

**Почему это нетривиально:** новое `C` может иметь высокую лексическую оценку, но низкий fused score и **никогда не быть выбранным**; само присутствие `C` меняет знаменатель **всех** старых lexical scores. Рейтинг `A` и `B` может перевернуться. Поэтому «обучили новый пример, испортилось старое» возможно даже без утраты весов, нарушения DPO, изменения исходных labels или прямого вытеснения `A` кандидатом `C`.

**Воспроизводимый синтетический контрпример** (только алгебра текущего `ScoreCandidates`, НЕ измеренный C# runtime):

| ID | Смысл | normalized cosine→vectorScore | lexicalScore | patternScore | До добавления C | После |
|---|---|---:|---:|---:|---:|---:|
| A | correct old learned output | 0.65 | 0.9 | 1 | **0.8425** | 0.5555 |
| B | wrong old learned output | 0.90 | 0.4 | 1 | 0.76056 | **0.633** |
| C | irrelevant new lesson | 0.30 | 5 | 0 | not present | 0.485 |

Оба старых кандидата, их исходные vector/lexical/pattern scores и labels неизменны. Старый winner A, после добавления C — **B**, хотя C не выигрывает. При фиксированном заранее на DEV lexical reference=0.9 old A остаётся winner. Эти числа доказали **возможность механизма**, не его частоту на реальных данных. Для native проверки нужно установить, что такие lexical значения / ANN pools возникают в реальном corpus, а не выдавать proxy за продемонстрированный RP01 learning regression.

**Условия причинности:** при одинаковом модели/энкодере, frozen исходных embeddings/candidates, попавшем в ANN пул добавленном C и неизменном законном oracle. Прямой RP01 free-generation к этому Unit пока **не привязан**.

## NEW-20.02 — неизвестный structural pattern получает положительный match-вес как будто он установлен

**Наблюдение в коде (C):**
- [Tokenize + ClassifyPattern 276–294](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L276-L294) вызывают `TrainingVectorBuilder.NormalizeRoots`; если нет токенов, `ClassifyPattern` возвращает строку **`"UNKNOWN"`**.
- [NormalizeRoots 104–134](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L104-L134) использует `Regex.Split(..., "[^a-z]+")`. Русские/ивритские строки и чистые операторы/цифры не дают roots в **этом конкретном канале**. Это НЕ означает, что Unicode потерян из BPE Student.
- [ScoreCandidates 219–239](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L219-L239): `patternScore = (queryPattern == candidatePattern) ? 1 : 0`. Поэтому **UNKNOWN == UNKNOWN** даёт полные **+0.20** к fused score без известного структурного соответствия.

**Контрпример условного ранжирования:** `WRONG_UNKNOWN` с vectorScore=0.82 и patternScore=1 выигрывает 0.569 против `CORRECT_KNOWN` с vectorScore=0.93 и patternScore=0 — 0.4185. Если UNKNOWN не даёт бонуса, correct 0.4185 > wrong 0.369. Это **analytic illustration** с замороженным пулом, не измерение конкретного semantic-law violation. Если **все** кандидаты UNKNOWN, +0.20 является общей константой и не меняет их порядок; остаётся риск ложной *уверенности*, если score конвертируется в confidence: [196–205](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L191-L208). Обязательно измерить оба режима.

**Отрицательная проверка:** English/ASCII known-pattern query, где оба pattern names совпадают и независимый law подтверждает структуру; блокировка UNKNOWN-бонуса не должна ухудшить его.

## NEW-20.03 — дополнительные сомнения (не объявлять отдельными новыми root causes без проверки)

- **Глобальная смена IDF при росте learned corpus (C mechanism / H quality):** `ComputeBm25LikeScore` строит documentFrequency / IDF из **всех** `_examples` на каждый запрос. Пополнение коллекции меняет относительную важность слов старых результатов, даже если новые примеры не присутствуют в текущем top-k. Это второй путь не-локальности; отделить от NEW-20.01 в A/B с зафиксированным per-old-example lexicalScore.
- **Hybrid reranker не имеет гарантии увидеть все lexical-relevant примеры (C architecture / H effect):** [ANN Search и topK](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L181-L193) просит `topK = _examples.Count`, но [HNSW SearchLayer traverses graph](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/06.VectorSearch/HnswIndex.cs#L99-L111) и [190–222](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/06.VectorSearch/HnswIndex.cs#L190-L222) не равносилен exhaustive search всех examples. Это отличается от уже задокументированного HNSW adjacency-pruning риска; недостающий кандидат может стать невидимым лексическому BM25 слою. Измерять `candidateRecall@k` против exact exhaustive, не выводить «баг» из одного факта ANN.
- **Фальшивая способность из manifest (C static / H interpretation):** [Train manifest](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L138-L154) жёстко записывает `LoraAdapterCount: 0, DpoStepCount: 0`, даже если в состав Unit передали модель с обученным адаптером. Это descriptor construction, **не доказано**, что consumer реально использует эти поля для routing/adoption.

## Почему существующие документы этого не закрывают

| Ранее опубликованное | Оставшийся новый вопрос |
|---|---|
| [DIF-07/13](01-PRIORITIZED-DIFFERENCES.md): generic KNN first-hit, hashed unordered features | Отдельная **Transformer+E34-like** Unit: winner меняется, даже когда никто не удалил правильный пример и неправильный новый не выбран. |
| [09-historical-fusion](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md): unmatched historical eval | Проверить **non-local rank stability** конкретной формулы scorer, не исторический reported uplift. |
| [16-KNN tier balance](16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md): глобальная L2 против tier-specific | Здесь **unchanged embeddings**; меняется *деноминатор lexical fusion* или unknown-class bonus. |
| [16-encoder index drift](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md): LoRA W0 vs W1 embeddings | Здесь frozen encoder и frozen candidate embeddings, чтобы исключить drift. |
| [18-consumer bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md) | Даже успешный Unit-level causal gain ещё НЕ связан с RP01 ordinary GenerateAsync без traced consumer. |

## Вывод для архитектора и запреты

**Источник подтверждает опасную математическую зависимость, а НЕ установленную причину падения RP01.** Ранжирование, демонстрируемое аналитически, может поменяться только при соответствующих данных/pool. Не объявлять «DPO плохой», «LoRA сломана», «checkpoint не работает» или «новая KNN обязательно даст semantic gain».

Первоочередное решение — не переписывать весь engine. Запустить [MICRO-20.01](20-MICRO-RERANKER-LESSON-ADDITION-CAUSAL-EXPERIMENT.md), сначала cheap algebra/native scorer parity, затем маленький source-disjoint real Unit A/B и только после этого ordinary consumer trace.

Статусы: **source code read: DONE; synthetic algebra run: DONE; C# native model/Unit training: NOT_RUN; blind semantic outcome: NOT_RUN; independent CODEX/Astra reviewer: NOT_RUN; RP01 causal attribution: NOT_PROVEN.**
