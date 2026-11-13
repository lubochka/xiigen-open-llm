# N35-C — обучаемый sentence embedding обрывается в трёх местах до gradient

**Дата:** 16.10.2026 · core@216a1f3d6af3f40ceef081dc168b07e4ce3934f9 · SOURCE_CONFIRMED_AUTOGRAD_DISCONNECT / CURRENT_CONTRASTIVE_CALLER_NOT_FOUND / CAUSAL_QUALITY_NOT_RUN. Отдельная гипотеза для обучаемого Vector/KNN encoder, НЕ доказанное объяснение RP01 DPO Student. Protected core docs/plans/replanning-possible-diffs: READ ONLY.

## Что нового относительно N33-A Embedding

Уже известное исследование [N33 autograd и supervised warmup](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md) правильно обнаружило, что Embedding.Forward отключает градиент таблицы токенов. **Это исследование рассматривает независимую проблему в совершенно другом месте:** даже если исправить Embedding.Forward и разрешить LoRA/Transformer learning, путь от sentence-embedding loss через pooling к encoder всё равно останется прерванным. Здесь **три** разрыва, а не один.

Почему это потенциально важно: если гипотеза «обучаем векторы так, чтобы правильные законы KNN находились чаще» проверяется через существующий EmbeddingModel.EmbedBatch, то значимый contrastive/metric loss просто не сможет изменить encoder. Вектор сохранится, HNSW будет искать — но корректировка представления по сложным DPO/Unit случаям не произойдёт. В обычном **inference-only** retrieval это не баг: возвращение float[] вместо Tensor намеренно удобно.

## Точные доказательства

| Этап | Source | Механизм разрыва |
| --- | --- | --- |
| ForwardHidden → Pool | [EmbeddingModel.cs L29–46](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/05.Embeddings/EmbeddingModel.cs#L29-L46) | Вызов encoder.ForwardHidden(ids), затем MeanPooling.Pool(hidden,mask). |
| Pool → new Tensor | [MeanPooling.cs L19–49](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/05.Embeddings/MeanPooling.cs#L19-L49) | Weighted sum вычисляется на сырых float[]; return new Tensor(result, shape) без requiresGrad/Children/BackwardFunc. Градиент к hidden не проходит. |
| Normalize → new Tensor | [TensorOps.cs L1566–1596](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/00.TensorCore/Core/TensorOps.cs#L1566-L1596) | L2Normalize снова создаёт detached Tensor после деления на норму; нет производной ни для 1D, ни для 2D. |
| Return float[] | [EmbeddingModel.cs L38–46](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/05.Embeddings/EmbeddingModel.cs#L38-L46) | Array.Copy(normed.Data → float[]) исключает Tensor autograd и в public EmbedBatch API. |
| Реальный inference-only consumer | [LlmIngredientLogicalUnits.cs L105–146, L175–210](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L105-L210) | EmbeddingModel(...maxLen=min(64,ctx)).Embed(example.Text) строит ANN memory; Embed(request.Input) строит query. Обучение encoder через этот loss **в этом call site не найдено**, следовательно claim для текущего Student остаётся условным. |

**Доказано исходником:** нет differentiable connection от output sentence vector к hidden encoder в данном API; это нельзя исправить только правильным Embedding.Weight backward. **Не доказано:** что реальная RP01 цепь когда-либо пыталась делать contrastive gradient, что именно этот gap объясняет какой-либо historical loss или что новая обучаемая версия автоматически повысит correct retrieval. По наблюдаемому caller — это инференсный encoder.

## Быстрые native градиентные falsifiers — три независимые стадии

**M0-POOL — forward/gradient:** зафиксировать tokenVectors Tensor(shape [1,2,2], requiresGrad=true), значения токенов [[1,0],[3,0]], mask=[1,1]. MeanPooling.Pool даёт [[2,0]]. Вызвать Backward с upstream ones. Аналитически для каждого из 2 исходных токенов и обеих координат dL/dTokenVector=1/2. Current source: tokenVectors.Grad остаётся null, потому что Pool output не хранит Children; finite-difference по входным tokenVectors даёт примерно 0.5. **A** original, **B** test-only differentiable weighted mean; выбирать epsilon=1e-3 и проверять error <1e-2. Пропуск нормы на этом шаге преднамеренный: одно место обрыва за один тест.

**M0-NORM — отдельная функция:** x=[3,4], L=L2Normalize(x)[0]; аналитические производные dL/dx0=0.128, dL/dx1=-0.096 при eps, не задевающем norm=5. В A исходный L2Normalize output detached; в B differentiable L2Normalize + finite differences. Не менять pooling/encoder. Если A native уже накопил обе производные — source-hypothesis DISCONFIRMED.

**M0-API — detached output:** после исправлений M0-POOL и M0-NORM проверить, что public EmbedBatch по-прежнему отдаёт float[] и не позволяет вызвать Backward. Для neural training требуется **новый отдельный differentiable training API**, возвращающий Tensor и поддерживающий mask, а inference float[] сохранять. Нельзя «починить» публичный инференсный контракт скрытой неявной autograd-ссылкой.

**L1 semantic pilot, только если M0 подтвердит:** заморозить 16 пар текстов (8 C# code AST-context + 8 EN→RU negation/word order), 8–16 source-family-disjoint holdout queries и 4 критических contrastive hard negatives. На одинаковом исходном encoder/LoRA SHA и tokenizer SHA сравнивать **непрерывно обучаемое** test-only Tensor API (B) против frozen encoder index (A), равные training steps / optimizer / rank / source corpus; при B заново пересчитать **и memory vectors, и query vectors** одной версии encoder и атомарно перестроить ANN. Иначе наблюдается уже известный [encoder/index mismatch N16](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md), а не чистое влияние gradient. Как отдельную ось сравнить B с stopped-grad training API, чтобы именно восстановление derivative было причиной.

**Primary metrics:** finite-difference gradient agreement; LoRA/encoder weight SHA before-after; recall@1/@5 против exact brute-force oracle на том же encoder snapshot; law correctness при том же KNN/reranker/consumer, critical wrong accepted, abstentions, encoder/index version equality после cold restore; negative transfer by language, C# syntactic-role reversal и word-order reversal. Если обновился encoder, но heldout correct retrieval не изменился — MECHANISM_ONLY, не QUALITY_GAIN.

## Доменные ограничения и проектное решение

- **C# / Design-to-Code:** Roslyn AST labels/role-aware vectors должны входить в репрезентацию или candidate scorer только при доказанной приростной ценности. Plain sentence encoder сам по себе не гарантирует сохранение порядка, operator и null law.
- **EN↔RU/HE↔EN:** проверять отрицание, именованные сущности, POS, порядок, морфологию; кодовый AST-context нельзя объявлять глобальным улучшением.
- **Domain routing / Graph:** нужно доказать, что именно обновлённый encoder указанного Unit и именно обновлённый ANN индекс достигли реального продукта, а не только тестового EmbedBatch.
- **RP01 exact DPO:** LoRA q/v DPO trainer делает модельный Forward/log-prob, не вызывает sentence-embedding pooling; данный gap **NOT_CAUSE_ESTABLISHED** для текущего 500+500 free-text failure.

**Decision:** P2 / CONDITIONAL_BLOCKER прежде чем планировать новое differentiable metric learning; P3 для текущего read-only retrieval без обучаемого encoder. Никакой production fix, DPO generation или native probe в рамках этой публикации не исполнялись.

**Роли:** CODEX (Astra 6 UltraCode) выбирает cases как человек; независимый CODEX/Astra оценивает обе стороны DPO и качество blind learned output; внешняя открытая китайская модель генерирует **обе** финальные DPO стороны chosen/rejected. Эти полномочия нельзя заменять статическими эвристиками.
