# 16 — Несогласованность версии encoder и KNN-индекса после обновления LoRA

**Дата:** 16.10.2026. **Статус:** подтверждённая конструкция кода (C), условная причинная гипотеза (H), **NOT RUN**. Дополнение к [DIF-07/08/31](01-PRIORITIZED-DIFFERENCES.md), [проверке HNSW](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) и [исходному fusion-аудиту](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md). **Не утверждаем**, что эта проблема уже возникла в конкретном RP01.

## Непосредственное доказательство и цепочка

1. [TransformerEmbeddingKnnLogicalUnit.Train, 98–161](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L98-L161): создаёт `EmbeddingModel(model,tokenizer,...)`, вычисляет `embeddingModel.Embed(example.Text)`, записывает в `HnswIndex`. Это **снимок векторов** в момент Train.
2. [Конструктор и ExecuteAsync, 68–95, 166–206](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L68-L206): unit сохраняет ссылки на **тот же** `LlmModel` и `EmbeddingModel`, а поиск использует `_embeddingModel.Embed(request.Input)` при каждом запросе. При этом показанный ExecuteAsync не перестраивает старый `_index`.
3. [EmbeddingModel, 16–41](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/05.Embeddings/EmbeddingModel.cs#L16-L41) вызывает `_encoder.ForwardHidden(ids)`, то есть изменение hidden-state модели потенциально изменяет query embedding; это **не независимый фиксированный текстовый encoder**.
4. [GetServingRuntime, 94–95](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L94-L95) отдаёт объект того же model/tokenizer, [ProductHost 664–682](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/50.ProductHost/CapabilityServing/CapabilityServingCompositionRoot.cs#L664-L682) его использует. Но данный ProductHost заявляет **no-training boundary**; из этого нельзя заключать, что он изменяет model после построения индекса.
5. [EmbeddingModel.Tokenize, 77–83](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/05.Embeddings/EmbeddingModel.cs#L77-L83) берёт только первые `_maxLen` токенов; этот Unit выставляет `maxLen = min(64, ContextLen)`. Разные инструкции с одинаковым 64-токенным префиксом могут получить одинаковую векторную часть запроса. Лексическая часть дальнейшего fusion всё ещё может их разделять: **полной идентичности финального ответа не утверждаем**.

**Инвариант, которого не видно в данном API:** `encoder_fingerprint(index_vectors)==encoder_fingerprint(query_vector)` и `tokenizer_sha/index_build_sha/adapter_sha` должны явно совпадать, если unit предъявляет себя как обученная память. Одних `TrainingExampleCount`, `VectorDimensions`, `LoraAdapterCount: 0`, `DpoStepCount: 0` в [manifest, 139–156](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L139-L156) для контроля версии векторов недостаточно.

## N16-A — stale-index drift (P1, conditional)

Если после `Train` меняются q/v LoRA модели, используемой **тем же** `EmbeddingModel`, поисковый запрос рассчитывается в пространстве весов W1, а учебные векторы остаются в пространстве W0. Cosine-кандидаты и их последующий fusion могут переставиться, даже если DPO математически повышает preference margin. Нужен **actual caller-to-writer trace**; не считать предполагаемое обновление реально вызванным в RP01.

### Микроэксперимент INDEX-VERSION-01

**Вход:** один уже разрешённо обученный W1-checkpoint и соответствующий W0 до его обновления; одинаковый tokenizer SHA, 16 обучающих source-disjoint записей и 12 blind heldout из 2–3 семейств. Не обучать 500 DPO, не менять law/scorer/seed. В первую очередь сравнить **exact cosine top-k по тем же сохранённым vectors** без HNSW, чтобы не смешивать этот фактор с random graph topology.

| Arm | Encoder query | Encoder stored vectors | Смысл |
| --- | --- | --- | --- |
| A | W0 | W0 | исходное согласованное состояние |
| B | W1 | W0 | единственный фактор — рассогласование версий |
| C | W1 | W1 | восстановление согласованности; **B↔C** изолирует эффект refresh |
| D | W1 | W1 cold-loaded | контроль export/import версии, параметры сопоставляются по SHA |

**Метрики:** cosine-rank@1/@3 относительно независимой разметки, правильность доменного закона на конечном `LogicalUnitResult.Output`, critical wrong, abstain, latency и среднее изменение `cos(E0(text),E1(text))`. Для каждого heldout сохранить ranked hit ID/score W0 и W1, контрольный input SHA, frozen label; заранее выбрать семантическую оценку blind. **Основной контраст** B↔C; A↔C смешивает эффект обучения весов с refresh индекса и не доказывает, что refresh улучшил качество.

**Фальсификация:** если query embeddings W0/W1 одинаковы либо B и C дают одинаковые hit sets и domain correctness, гипотеза на этой панели `NO_OBSERVED_EFFECT`. Если B хуже C на заранее выбранных blind задачах и при этом оба используют W1, гипотеза о влиянии index-version mismatch **поддержана на данных задачах**, но RP01 причинная связь остаётся непроверенной до доказанного вызова данной Unit в плохом route. Малые абсолютные изменения на n=12 считать пилотным сигналом, не статистической гарантией.

**Бюджет:** read-only 12×3×2 embedding/probe + один cold restore, ноль новых optimizer steps; без Windows Actions и полного CI.

## N16-B — обрезание смысла после 64 токенов (P1, independent)

Два примера с **буквально одинаковыми первыми 64 BPE-ID**, но противоположным нормативным хвостом (`must NOT delete` vs `must delete`) дают одну и ту же векторную часть при фиксированном encoder, хотя разный downstream law. Это отличается от [DIF-13 — потеря порядка в hashed bag](01-PRIORITIZED-DIFFERENCES.md): здесь source кодирует порядок, но вообще **не видит хвост**.

**Проверка PREFIX-TAIL-01:** на 8 попарно инвертированных сценариях сохранить полный BPE-ID массив, prefix64 SHA, vector cosine, lexical score и финальную law-правильность. A = 64-token encoder; B = 128-token encoder **на том же замороженном W1**, каждый раз перестроить индекс и query в одном пространстве. **Мерить** изменение final answer accuracy на том же heldout; если lexical слой уже правильно различает их, недополученного пользовательского качества здесь нет. Не менять одновременно fusion веса.

## Review / claim boundary

Код/алгоритм: **C**. Возможный эффект на качество: **H**. Конкретная реализация RP01 действительно обновляла *этот экземпляр* embedding model после обучения индекса: **UNKNOWN**. Модель, dotnet-компилятор, эксперименты и независимая CODEX/Astra-проверка здесь **не запускались**. Начальное действие: read-only trace actual Unit train → model update → index consumer, затем `INDEX-VERSION-01` только при достижимом состоянии W1/W0; при недостижимом случае перенести риск в continual-learning RP05, не чинить RP01 вслепую.
