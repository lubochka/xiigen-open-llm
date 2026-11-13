# N30-B/C — два дополнительных источника скрытого ухудшения KNN: изменяемые векторы и NaN

**16.10.2026 · DRAFT / DIRECT CODE REVIEW / NATIVE NOT_RUN / ORDINARY QUALITY NOT_PROVEN.** Независимый новый сценарий относится к владельцам пространства векторной памяти, не к алгоритму DPO. Pinned core: [codex/dpo-implementation@79082340](https://github.com/lubochka/llm_mvp_core/tree/79082340f01b744cdf08185616f2bd818dbd06b8). Эта заметка сравнивалась с локальными N14/N16/N20/N22/N23/N27/N28/N29 и core 00–40; **не заявляет, что никто в остальных ветках не сформулировал похожий риск**.

## Новое N30-B: изменили массив ПОСЛЕ обучения — а память поменялась без Train

**Смысл.** В индекс записывается *ссылка* на переданный float[] вместо снимка значения. Если вызывающий код позже переиспользует буфер, нормализует его in-place или изменит координаты, ранее «обученный» пример поменяет положение в пространстве. У HNSW топология графа останется от прежних координат, а cosine вычисляется уже по новым. В BruteForceIndex могут измениться score и выбранный ответ без нового урока. Это невидимая мутация обученной памяти, а не «неправильный коэффициент LoRA».

### Прямое свидетельство

1. [HnswIndex.Add:49–59](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/HnswIndex.cs#L49-L59): _vectors.Add(vector) сохраняет сам объект float[], не clone. [GreedySearch/SearchLayer:164–221](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/HnswIndex.cs#L164-L221): поиск читает _vectors по мере выполнения; [Save:115–132](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/HnswIndex.cs#L115-L132) сериализует их *текущее* содержимое.
2. [BruteForceIndex.Add:20–29](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/BruteForceIndex.cs#L20-L29): создаёт IndexEntry(id, vector, metadata) без defensive copy; [IndexEntry:8](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/IndexEntry.cs#L8) — record с полем float[] Vector.
3. [IVectorIndex.Add:9–14](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/IVectorIndex.cs#L9-L14) принимает mutable float[] и Dictionary metadata без правила передачи владения.
4. **Ограничение фактической причинности:** [KnnLogicalUnit.Train](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136-L155) передаёт freshly computed Vectorize(...), а [TransformerEmbeddingKnnLogicalUnit.Train](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L114-L136) передаёт свежий EmbeddingModel.Embed. Для этих двух **прочитанных** callers поздняя мутация массива не найдена. Пока это **подтверждённая уязвимая семантика API / условный дефект при mutable caller**, не установленная причина C#500/ENRU500.

### Короткий проверяемый контрпример

BruteForce получает good=[1,0]→«GOOD» и bad=[0,1]→«BAD». Query=[1,0] выбирает GOOD (cos 1 > 0). Без повторного Add вызывающая сторона меняет свой массив good на [-1,0]. По source тот же query теперь выбирает BAD (cos 0 > -1). Контрольная ветка, копирующая массив при Add, продолжает выбирать GOOD. Это **логический прогноз вычислений по исходникам**, не выполненное native измерение.

Для HNSW отдельно сравнить Save(graph+vectors) до/после такой мутации: frozen graph, changed coordinate bytes — два несогласованных аспекта одного состояния. Чтобы доказать качество, нужен actual learned consumer, а не только числовая перестановка nearest IDs.

**Отличие от [N27 mutable TranslationCheckpointCache](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md):** там alias на живую TranslationMachine между checkpoint/cache/train; здесь aliased vector memory внутри **KNN index** и несовпадение geometry ↔ graph. От [N16 encoder drift](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md) отличие в том, что модель/encoder вовсе не меняется: изменяется старый массив через ссылку.

## Новое N30-C: нечисловой вектор может получить score=NaN и оказаться в learned memory

**Смысл.** Код принимает float.NaN / ±Infinity как координаты и не проверяет, что similarity конечна. На таком сигнале ранжирование становится неинтерпретируемым; факт «положительный confidence / correct top-1» не должен приниматься без проверки конечности score и исходного embedding.

### Источники

- [Distances.CosineSimilarity:10–21](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/Distances.cs#L10-L21) использует умножения, сумму, sqrt и деление без float.IsFinite. С хотя бы одной NaN coordinate score становится NaN.
- [HnswIndex.Add/Search:49–111](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/HnswIndex.cs#L49-L111) и [BruteForceIndex.Add/Search:20–29](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/06.VectorSearch/BruteForceIndex.cs#L20-L29) не отказывают во входном NaN/Infinity.
- [EmbeddingModel.EnsureNonZeroUnitVector:49–74](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/05.Embeddings/EmbeddingModel.cs#L49-L74) проверяет только norm > 1e-6f и norm <= 1e-6f. При NaN обе проверки false, после чего деление на NaN оставляет nonfinite значения. Этот метод сам не создаёт NaN из нормального embedding; нужна upstream numerical failure, до сих пор **не наблюдавшаяся** в наших данных.

**Контрпример (не исполнен):** BruteForceIndex.Add([NaN,0], "poison"); Search([1,0], topK:1) возвращает запись со score=NaN, потому что нет требования конечности. Для HNSW от NaN могут зависеть SortedSet comparisons и выбор кандидатов; конкретный порядок при наличии других точек **нельзя назначать без выполнения**, но отсутствие fail-closed явное.

**Не путать:** [N23-A](23-INDEPENDENT-AUDIT-SHAPE-DECODER-AND-LEARNING-2026-10-16.md) — несовпадающая **длина**; N30-C — одинаковая длина, но **нечисловые координаты**. [N28-H1](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md) — графовое pruning после успешного Add.

## Применимость

| Область | N30-B mutable vector alias | N30-C nonfinite |
| --- | --- | --- |
| Generic KNN / semantic memory | P2 до доказанной записи вызывающим кодом по ссылке; P1 при shared mutable buffer | P1 если upstream действительно выдаёт nonfinite и index используется |
| TransformerEmbeddingKnn / DTC / Wiki | P2: текущий изученный caller создаёт отдельный Embed; не найдено reused buffer | P1 conditional upstream numerical failure; из source не доказана частота |
| Translation Unit own stores, без IVectorIndex | Не переносить автоматически | Не переносить автоматически |
| RP01 direct exact DPO / LoRA / free generation | N/A непосредственной причины | N/A непосредственной причины |

C#, SQL/AST, программные символы, RU/EN/HE могут быть семантическими доменами *потребителя индекса*; эти два дефекта **не требуют разного синтаксического дерева**. Для AST/order/POS нужны отдельные domain-specific функции из [N29 register](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md); нельзя глобально навязать одну similarity.

## Малый эксперимент и границы

[Нативный протокол N30](30-MICRO-VECTOR-INDEX-STATE-AND-QUALITY-2026-10-16.md), Arm B — двумерный exact cosine с одним mutable array; Arm C — один nonfinite vector, одинаковый baseline/guard. **Одна переменная на опыт**, не менять одновременно ANN connectivity, BPE, LoRA или закон домена. Критерий механики B: после изменения внешнего массива нынешний индекс меняет score/nearest, snapshot-index нет; C: NaN admitted/current vs fail-closed/refuse guarded. Показатель learned quality — paired, source-disjoint correct@1/critical-error и confirmed actual consumer receipt после честного lesson, отдельно от механического дефекта.

**Remediation только при достижимости:** Add в индекс копирует массив+метаданные; schema фиксирует immutable dimension/encoder/version; finite guard при Add и query, приёмочный error с запретом частичного изменения; cold Save/Load hash и rerun ordinary consumer. Ни один такой source-only guard сам по себе не лечит случайную языковую B0 в прямом RP01.

**Статус:** direct source checked; no native C#, no real model outputs, no CODEX/Astra independent quality review, no claim that a real RP01 index was poisoned.
