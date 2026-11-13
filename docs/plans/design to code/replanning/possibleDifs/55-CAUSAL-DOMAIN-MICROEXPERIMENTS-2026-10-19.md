# Быстрые доменные A/B: PrepareToGPL → current без долгих Windows-run

**Дата:** 2026-10-19. Это проект экспериментов, **не отчёт о native результатах**. Goal = actual learned quality restoration per domain, not green CI. Все исходные версии, corpus hash, source family split, model checkpoint и raw answers фиксировать до опыта. Эксперимент с разными факторами не принимается как причинное сравнение.

## G55-A0 — GOLD-first, checkpoint и фактическая сохранённая способность

**Кого обучали:** TranslationMachine: SemanticTokenizerUnit, ExecutorUnit + shared KNN, QualityVerifierUnit; а не DPO Student. **Домен:** RU→EN и EN→RU отдельно; HE→EN/EN→HE если история доказывает эти directions. **Вход:** tracked [gold checkpoint](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/models/translation/translation-v1/translation-v1-checkpoint.json), два frozen known cases + два source-disjoint paraphrases/direction, старый/текущий точные SHAs. **Четыре arms:** A = gold code+gold checkpoint; B = current code+unmodified gold checkpoint; C = current code+new explicit legal migration, preserving 4 prior executor maps and adding 2 empty ES; D = current code+fresh empty state.

**Проверка:** сперва загрузка и count/hash внутренних старых learned maps (без генерации), затем direct Unit, cold load, ordinary Flow с теми же запросами. Запись exact/phrase/kNN/word/verifier источника ответа. **Причинное принятие:** C восстанавливает конкретные прежние ответы A на frozen панель и не теряет другие прежние learned maps; B отказал по документированной причине; D не демонстрирует старое правило. Если A не воспроизводит archive, заявление о recovery = INCONCLUSIVE, не подгонять кейсы. Нельзя просто обходить provenance/manifest schema.

## G55-A1 — phrase/ordered composition, где реально было улучшение

**Домен:** RU→EN commands, отдельно EN→RU. **Вход:** 8 прошедших domain QA lessons, 8 новых композиционных запросов (из других исходных шаблонов) и 4 отрицательных примера: не, обратный порядок действующих лиц, неизвестный noun, quotation. **Arms:** empty; exact-only; learned phrase+word+KNN; learned+verifier; after cold restore. Обучать исходные карты в native Unit, не генерировать заранее правильные ответы в приёмке. **Метрики:** actor/action/object/polarity preserved, coverage/abstention, learned route hit; отдельно learned rule vs deterministic fallback. Existing 11/24→19/24 — только historical comparator. **STOP:** если до обычного Flow не доходят phrase mappings, сначала repair consumer, не менять similarity threshold.

## G55-A2 — новая проверка mixed-direction eval

**Домен:** 6 типов RU_EN/EN_RU/HE_EN/EN_HE/ES_EN/EN_ES по 2 технических synthetic route markers. **Входы:** source_language, target_language, topic_domain explicit per case, fake perfect ITranslationDirectionAdapter различается по direction. **One changed factor:** EvaluateAsync helper forced RU_EN vs исправленный typed metadata caller. **Negative control:** 2/2 RU_EN cases обе версии должны PASS. **Измерение:** requested direction, actual adapter, raw result, per-direction success, group denominator, global score and next AccuracyBefore. **Toy logic** 2/12 против 12/12 — лишь модель-маршрутизатор, а НЕ native learning result. Затем повторить native на двух реальных смысловых проверках каждого направления; отдельно от exact-match текста оценивать negation, actor/object, modalities.

**Falsifier:** production не вызывает общий EvaluateAsync со смешанным eval — данный дефект не объясняет наблюдаемую product regression. Если native typed route не меняет mixed score на подтвержденном наборе, дальнейший эксперимент закрыт как no-impact. Отсутствие per-item language contract означает contract migration prerequisite.

## G55-A3 — неверный вывод «matrix failed → функция бесполезна»

**Домены и функции:** C# AST: enclosing method/syntax tree, class, переменный порядок операторов; EN↔RU: POS bigrams, subject/object order, negation, грамматический род; SQL: NULL/NOT/COUNT semantics только при наличии реального SQL learner/consumer. **Дизайн 2×2:** carrier matrix vs kNN × structural feature off vs on; при этом training/test split, embeddings/tokenizer identity, k, weights, source IDs и semantic gold неизменны. 8–12 разных семейств, не просто 12 переименований. Gold old E09 кNN comparator читать вместе с actual heldout: ранее E09 baseline/eval population были неидентичны. **Успех:** feature-on даёт source-disjoint advantage в *своём* kNN domain, необоснованного harm/coverage penalty нет; отрицательный контроль feature-shuffle. Нельзя отождествлять C# AST с SQL семантикой и навязывать одинаковый порог неизвестным доменам.

## G55-A4 — DPO gradient и достаточность LoRA vs слабая B0

**Домен:** C# объяснение true program behavior; ENRU meaning-preserving translation — раздельные панели. **Вход:** 8–16 verified legal preferred/rejected pairs, unrelated source-disjoint heldout. **Arms:** frozen B0; B0 DPO only; B0 warmed SFT→DPO при равном вычислительном бюджете; old trained hybrid Unit; warmed+Unit. **Source code:** [trainer StepVector](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L832-L945), [LoRA attachments](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/10.LoRA/LoraApplicator.cs#L432-L493). Сначала tiny train-set overfit sanity: correct sign for chosen/rejected, correct masking, finite difference, grad norm, nonzero adapter logits, frozen base/reference. Затем independent free-answer with semantically changing contrasts. **STOP:** если маленький TRAIN набор не освоен, новые 500 pairs запрещены как диагностически бесполезное масштабирование; root cause isolate before bulk.

## G55-A5 — DPO → multi-Unit → DynamicGraph → ordinary consumer

**Домены:** C# execution trace, orchestration/dependency flow и отдельно design-to-code visual corrections. **Вход:** 6–8 plan pairs, у двух случаев минимум 2 Unit owner и 2 output dependency. **One changed factor:** original chosen unit-level lesson vs current subunit materialization, при одинаковом legal input/corpus. **Trace:** pair_id → authoring and polarity → source-id → UnitId/FlowId → persisted checkpoint → cold restore → chosen graph edges → ordinary typed output. Negative: swap rejected/chosen, break one edge, unknown Unit. **Результат:** correct dependencies and actual consumer; не считать сам факт materialized plan «обучением». Existing E43 replay success и E09 AST — разные метрики/получатели.

## Общий шаблон короткого результата

| Поле | Обязательный факт |
|---|---|
| identity | task, domain, specific objective, learner/Unit/Flow/consumer, branch SHA, data/checkpoint digest |
| isolation | one changed factor, negative/positive controls, fixed split and source families |
| before/after | raw direct Unit AND ordinary product outputs, semantic checklist and critical violations |
| stability | retained old lessons, new lesson gain, cold restore, abstention/coverage, failures |
| cost | CPU wall, RAM/VRAM if relevant, steps/tokens, no unrequested Windows CI |
| scientific verdict | matched causal A/B vs source-only hypothesis, exact falsifier |
| disposition | repair owner + next smallest experiment, never forced PASS |

**Ни один опыт здесь не выдает историческую метрику за выполненный сегодня native результат.** Ref. [Gold-first queue](00-55-PREPARETOGPL-GOLD-FIRST-2026-10-19.md), [new N55](55-NOVEL-DIRECTION-EVAL-AND-METRIC-STATE-2026-10-19.md), [crossbranch register](55-CROSSBRANCH-GAP-REGISTER-2026-10-19.md).
