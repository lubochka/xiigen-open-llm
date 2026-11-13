# N30-A — HNSW: неудачная вставка может оставить ложный обученный пример в индексе

**16.10.2026. Новое исследование, DRAFT / SOURCE-CONFIRMED CONTROL-FLOW / NATIVE NOT_RUN / LEARNING QUALITY NOT_PROVEN.** Срез: [core codex/dpo-implementation@79082340](https://github.com/lubochka/llm_mvp_core/tree/79082340f01b744cdf08185616f2bd818dbd06b8). Только каталог planning/possibleDifs. Репозиторий core и защищённый каталог core/docs/plans/replanning-possible-diffs не менялись.

## Человеческое объяснение

Векторная память может сообщать «изучено два примера», хотя второй пример **не был добавлен успешно**. Метод Add сначала добавляет вектор и ID в массивы индекса, а только потом строит связи HNSW. Если при построении связи обнаруживается несовместимая размерность, исключение прерывает Add, но уже добавленные данные не откатываются. В памяти остаётся запись, которую нельзя считать успешно интегрированным уроком. При Save ошибочное промежуточное состояние может попасть в checkpoint.

Это не означает, что текущий RP01 neural Student теряет пример этим способом: direct RP01 TextGenerator не вызывает HNSW. Это **самостоятельный дефект целостности возможного KNN-получателя**, применимый только при наличии неуспешного Add и реального потребителя этой памяти.

## Первичное свидетельство: точная цепочка и версия

1. [HnswIndex.Add:49–65](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/HnswIndex.cs#L49-L65): до валидации vector dimensions и расчёта соседей выполняются _vectors.Add(vector), _ids.Add(id), _meta.Add(...), _graph.Add(nodeLevels); Count возвращает _vectors.Count, строки 38 и 49–59.
2. [HnswIndex.Add:68–89](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/HnswIndex.cs#L68-L89): после преждевременного добавления метод выполняет GreedySearch / SearchLayer и двусторонние записи соседей. В методе нет try/catch с rollback созданных элементов и изменённых рёбер.
3. [Distances.CosineSimilarity:10–21](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/Distances.cs#L10-L21): цикл до a.Length, доступ к b[i] без проверки равной размерности. Для второго вектора из трёх координат и существующего из двух возникает IndexOutOfRangeException. Сам дефект размерности уже документирован в [N23-A](23-INDEPENDENT-AUDIT-SHAPE-DECODER-AND-LEARNING-2026-10-16.md); **новая здесь отдельная причина — неатомарное изменение состояния при ошибке**.
4. [HnswIndex.Save:115–132 / Load:134–151](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/HnswIndex.cs#L115-L151): сериализуются Vectors, Ids, Meta, Graph, EntryPoint, MaxLevel. Проверки целостности всех рёбер/векторов при Save/Load не видно; поэтому условно допускается долговременное сохранение остатка после неудачной вставки.
5. [KnnLogicalUnit.Train:136–155](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136-L155) и [TransformerEmbeddingKnnLogicalUnit.Train:114–137](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L114-L137) создают HNSW. В прочитанных train-path оба создают векторы штатной фиксированной длины; **фактический плохой Add в них НЕ установлен**.

**Механический source-предикт:** создать индекс; Add вектор [1,0]; выполнить Add вектор [1,0,0]. Когда текущий Add сравнит 3-компонентный query с прежним 2-компонентным vector, он выбросит исключение *после* увеличения Count. Проверяемая разница: CountBefore = 1, CountAfterFailedAdd = 2. Это рассуждение по коду, **не результат исполнения .NET**.

## В чём отличие от существующих находок

- [N23 shape mismatch](23-INDEPENDENT-AUDIT-SHAPE-DECODER-AND-LEARNING-2026-10-16.md) описывает ошибку *расчёта cosine*; здесь — **неоткаченную мутацию HNSW index при выброшенном исключении**, включая возможный плохой cold restore.
- [N28 reverse-prune](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md) описывает достижимость после **успешного** Add при pruning; здесь Add завершается **ошибкой** ещё до завершения графа.
- [N23 non-atomic Unit training](23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md) относится к частично применённым lesson records/Units; здесь первичный владелец ошибки — конкретный vector index.
- [N22 index identity](22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md) просит hashes; новый риск нельзя устранить одними SHA уже испорченного состояния.

Проверен существующий реестр N29, N23, N28 и core 00–40 по относящимся к HNSW/состоянию материалам; **абсолютная уникальность относительно всех чужих рабочих веток не утверждается**.

## Доменная применимость и уровень уверенности

| Получатель | Вероятность эффекта | Что точно пока неизвестно |
| --- | --- | --- |
| Generic KNN Unit для C#/SQL/правил | Условная P1, **если** источник вызывает неуспешный Add и затем использует сохранённый индекс | Реальные mismatch/failed Add receipts и downstream выбранный ID |
| Transformer embedding KNN: DTC, wiki, domain capabilities | Условная P1/P2 при неоднородных vector schemas, смене encoder или failed index build | В штатном Train векторы одной ширины, route to ordinary user |
| Translation structured Unit без данного HNSW-класса | N/A для данного конкретного класса | Свой отдельный контракт атомарности |
| RP01 exact-v2 DPO→LoRA→TextGenerator | N/A как непосредственная причина C#500/ENRU500 | Нет вызова HNSW на этом neural пути |

## Минимальная причинная проверка

[Полный малый протокол N30](30-MICRO-VECTOR-INDEX-STATE-AND-QUALITY-2026-10-16.md), Arm A. Tier 1 (до 10 файлов), один native C# probe, **не очередной 500-парный запуск**.

1. A = текущий HnswIndex.Add, B = copy-on-write/validate-before-mutate с тем же параметризованным индексом, данными, query и seed/replayed graph. Сначала механическая проверка Count, графа, сохранённого JSON и restored Count.
2. Frozen пример: A(id=good, vec=[1,0]); затем заведомо некорректный B(id=bad_dim, vec=[1,0,0]). Заведомо некорректный пример **не должен** учиться. Зафиксировать type exception, CountBefore/After, persisted IDs/edges.
3. Добавить C — валидный новый урок; после ошибочного B сравнить A-current и B-atomic на одних запросах. Если разница ранга есть, проверить correct@1/abstain и was_good_lesson_reachable, отдельно от факта corrupted Count.
4. Фальсификаторы: native Add не оставляет мутацию; real callers валидируют все vectors до Add и не сохраняют незавершённый index; пользовательский consumer никогда не использует такой индекс. Даже при механическом дефекте **RP01 quality regression остаётся UNKNOWN**, пока нет конкретной трассы успешный/неуспешный урок → real request → chosen answer.

**Предпочтительный будущий guard после подтверждения:** валидировать shape, finiteness и ID до первой мутации; стадировать новые рёбра/обратные pruning edits; commit index только после полного построения; сохранить canonical dimension+encoder/tokenizer identity. Не менять HNSW архитектуру по одному сконструированному exception case без caller evidence.

**Review status:** source-level logical inference; no .NET execution, no user-domain trial, no CODEX Astra 6 UltraCode independent verdict, no product modifications.
