# N23 · Transformer-KNN теряет предметное условие после 64 BPE-токенов

<!-- EFFECTIVE-AUDIT-2026-11-01:START -->
## Актуализация доказательств 01.11.2026 — P69

P6901 добавил actual retrieval evidence: длинные документы с общим64-token prefix схлопываются, short controls различаются, наблюдались уверенные wrong-source answers. Старое native-NOT_RUN ниже относится к16 октября. Приёмка требует raw/consumed span pairing, tail-law contrast и actual reached fusion/fallback; одинаковый embedding сам по себе всё ещё не гарантирует одинаковый final answer. Новый pooling/encoder не считается выбранным без отдельной постановки.

Основание: [2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md](2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md), закреплённый core `790228c256168a336b01abae4387e28391048c0c`. Старый текст и его ограничения относятся к указанной там дате; новый блок уточняет текущий статус, не переисполняет исторический опыт и не меняет научную очередь.
<!-- EFFECTIVE-AUDIT-2026-11-01:END -->

**Дата:** 2026-10-16. **Статус:** source-confirmed truncation and conditional indistinguishability; native retrieval/quality experiment **не запускался**. Проверенный снимок [core@0eccbdc3](https://github.com/lubochka/llm_mvp_core/tree/0eccbdc3c4efb90adf1d699806cbfa9f5610998d). Это НЕ generic KnnLogicalUnit из проекта 21 и НЕ neural TextGenerator из RP01.

## Для человека

В гибридном Transformer+KNN поиск обученных знаний использует вектор запроса. Хотя у модели context может быть больше, именно здесь embedding читает не более 64 BPE-токенов. Если два длинных запроса различаются только тем, что находится за этой границей — например, «сохранить файл» против «НЕ сохранять файл» или «до оплаты» против «после оплаты» — их **embedding в данном Unit будет одинаковым**. Новое различие от уже известных BPE-/hash-/scope-рисков: **потеря правой части запроса ДО семантического поиска**, даже если BPE словарь, веса и HNSW индекс не менялись.

Важно: последующий BM25-like lexical scorer смотрит полный query и candidate texts. Поэтому **одинаковый embedding не гарантирует одинаковый final answer**: BM25/pattern fusion *может* различить строки и восстановить правильный ответ. Только paired native experiment определит наличие реального вреда.

## Непосредственные кодовые доказательства

1. [TransformerEmbeddingKnnLogicalUnit.Train, 98–110](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L98-L110): new EmbeddingModel(model, tokenizer, maxLen: Math.Min(64, model.Config.ContextLen)).
2. [EmbeddingModel.Tokenize, 62–89](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/05.Embeddings/EmbeddingModel.cs#L62-L89): BPE Encode(text), затем Take(_maxLen), padding и mask. Нет windowing, chunk pooling, tail inclusion, domain-law segment preservation или span aggregation.
3. [EmbeddingModel.EmbedBatch, 22–42](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/05.Embeddings/EmbeddingModel.cs#L22-L42): именно эти токены передаются ForwardHidden и mean pooling/L2 normalization. При одинаковых 64 prefix token IDs и одинаковом model state получаются одинаковые embedding vectors (кроме не относящегося сюда случая иной runtime/nondeterminism; фиксировать runtime).
4. [TransformerEmbeddingKnnLogicalUnit.ExecuteAsync, 169–210](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L169-L210): queryVector = _embeddingModel.Embed(request.Input), затем HNSW, далее ScoreCandidates по полному request.Input. Семантический ANN работает на усеченном векторе; BM25 может спасти финальный rank, но лишь среди доступных retrieved candidates.
5. [ScoreCandidates, 217–243](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L217-L243): lexical/pattern fusion после retrieval, не исправляет omitted vector coordinates автоматически. В частности, если корректный смысловой сосед вообще не вошёл в candidate set, rerank не сможет его выбрать.

**Граница вывода:** source подтверждает truncation и условную embedding-equivalence. Не подтверждены частота длинных реальных запросов, ранги retrieved hits в RP01 и изменение law correctness. Обычный RP01 Student-only DPO/LoRA не использует этот Unit напрямую в показанном training runner. Квалификация для RP01: P1 conditional / causal candidate for hybrid serving, NOT_CONFIRMED_RP01_ROOT_CAUSE.

## Отличие от существующих находок

- [N21 BPE order drift](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md): там меняется encoder (BPE merge/token IDs) при retrain generic KNN. Здесь encoder неизменен, но его **вход обрезан до 64 токенов**.
- [N16 live encoder/index drift](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md): mismatch train/query model versions; здесь версии строго одинаковые.
- [N20 fusion nonlocal normalization](20-NEW-GAP-NONLOCAL-FUSION-AND-BM25-SCALING-2026-10-16.md): reranker changes scores when candidate set grows; здесь именно вход semantic retrieval становится алиасом ещё до rerank.
- [DIF-13] generic bag-of-token order: не тот Unit и не тот механизм.

## Археология веток

[language_translate@f6032aff:EmbeddingModel](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/05.Embeddings/EmbeddingModel.cs#L72-L80), [external_domains@3036a:EmbeddingModel](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/05.Embeddings/EmbeddingModel.cs#L72-L80), [DesignToCode@8f20:EmbeddingModel](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/05.Embeddings/EmbeddingModel.cs#L72-L80) уже обрезают по переданному maxLen. Следовательно это старое поведение базового класса. Не установлено, что каждая историческая ветка создавала EmbeddingModel именно с maxLen=64 или использовала этот Unit; нужно проверить конкретный caller и commit старого успешного опыта.

## MICRO-PREFIX64-01 · краткий реальный причинный эксперимент

**Tier 1:** 4–8 learning-memory rows и 8–16 frozen blind queries из двух доменов. **Primary metric:** law_correct@served_query/N и pairwise disagreement/flips при unchanged learned memory. Историческую RP01 free generation не переписывать. Выполнить два логически раздельных этапа:

**L0 — проверка потери признака.** Реальным BpeTokenizer.Encode построить пары query А/B с *одинаковыми первыми 64 token IDs* и одним законно значимым отличием после позиции 64 (отрицание, условие, адресат, порядок вызовов, исключение). Negative control: то же отличие внутри первых 64 IDs. Заморозить weights, tokenizer SHA, hidden dim, runtime seed, training examples, exact vectors. Проверить Embed(A)==Embed(B) или требовать численной паритетности для реального runtime. Никакой новой LoRA/Student DPO.

**L1 — эффект на кNN и final consumer.** Построить 2 индекса с одними и теми же memory rows и outputs: Production64 и Shadow128 (либо evidence-bearing chunked variant), re-embed **все** memory texts и queries в каждом варианте соответствующим encoder version. Не менять одновременно BM25, thresholds, formulas или oracle. Для каждого запроса зафиксировать full text, truncation IDs, candidate IDs, cosine scores, BM25 scores, fused ranks, returned output, law correctness, warnings/abstains. Отдельно сравнить fixed-candidate full lexical rescoring vs real HNSW retrieval, чтобы понять, компенсирует ли BM25 потерю suffix.

**Фальсификаторы:** (а) нет реальных queries с meaningful tail — проблема не активна на данном корпусе; (б) embedding aliased, но BM25 различает без ошибок — only representation risk; (в) correct candidate missing under 64 but present under 128 и semantic correctness improves — causal gain; (г) обе arms не улучшаются — гипотеза влияния на качество этой панели не подтверждена. Не увеличивать maxLen глобально без проверки PC memory/latency и source precision.

**Ожидаемый output artifact:** one JSON table for each paired case (IDs, first64 identity, suffix law, candidate sets, vector scores, ranked outputs, independent correctness, resources), same-input A/B+negative controls, one page reviewer verdict. CODEX/Astra validates subject law independently; не приписывать этому reviewer авторство DPO. Без независимого native измерения документ остаётся DRAFT.

## Практическое решение пока запрещено выдавать за fix

Возможные варианты после подтверждения: semantic tail-aware selection, domain-law span extraction с отдельным vector field, multi-chunk embeddings, larger maxLen с бюджетом. Не выбирать по интуиции; сравнить один механизм за раз, с реальным потребительским quality gain и учётом CPU/GPU на PC. Изменение tokenizer/LoRA одновременно уничтожит причинный контраст.
