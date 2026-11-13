# 50 · Дополнительные точечные расхождения E25 → нынешний переводческий Unit

**Дата:** 19.10.2026. **Тип:** source-level comparative audit; реальные новые native .NET learning/quality runs **NOT_RUN**. Донор `external_domains@3036a01acec938f0b83fa4ffbda26202cc35d639`; текущий код `codex/dpo-implementation@649b9343cfda5b81cc54396a3840ad5db5e0b179`. В protected каталоге `llm_mvp_core/docs/plans/replanning-possible-diffs` изменений **нет**.

## 1. Главная граница: это не потерянный Git-файл

Прочитаны две полные версии шести исходников: `12.SemanticCompiler/Translation/PatternClassifier.cs`, `PatternReorderer.cs`, четыре runner-а `E25PatternClassifier`, `E26ContextualDictionary`, `E09StructuralKnn`, `E43ReplayBuffer`. Во всех шести парах нынешний файл *полностью совпадает* с донорским после удаления пяти строк лицензионного заголовка, появившегося в current. Следовательно, **не доказано, что реализация именно этих классов регрессировала при Git-переносе**.

Но [исторический E25 learner](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/12.SemanticCompiler/Translation/PatternClassifier.cs#L29-L86) обучается на POS-биграммах и хранит соседей, а [нынешний `TranslationSemanticTokenizer.ClassifyPattern`](https://github.com/lubochka/llm_mvp_core/blob/649b9343cfda5b81cc54396a3840ad5db5e0b179/27.TranslationDomain/TranslationSemanticTokenizer.cs#L351-L362) пользуется фиксированными условиями. [Нынешний `SemanticTokenizerUnit`](https://github.com/lubochka/llm_mvp_core/blob/649b9343cfda5b81cc54396a3840ad5db5e0b179/27.TranslationDomain/TranslationTrainedUnits.cs#L88-L147) хранит точные/сигнатурные исправления, а не расстояния до POS-последовательностей. **Отсутствие подключения learned E25 к настоящему consumer уже отмечали другие исследователи.** Новыми здесь считаются узкие source-level уязвимости и точные различающие проверки ниже — не весь класс несоответствия.

Исторический [результат E25](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json) = **34,38% → 87,5% top1, validation=32, 160 синтетических POS примеров, k=3**. Это не neural/LoRA и не доказательство улучшения полного перевода.

## 2. N50-01 · Схлопывание verb-first структур + несовместимая онтология классов

### Доказательство A — конкретное поведение нового правила

В [current lines 351–362](https://github.com/lubochka/llm_mvp_core/blob/649b9343cfda5b81cc54396a3840ad5db5e0b179/27.TranslationDomain/TranslationSemanticTokenizer.cs#L351-L362) условие `firstVerb == 0 && !hasPronBefore` возвращает `IMPERATIVE_V`. Но `hasPronBefore` определяется как `firstVerb > 0 && ...`: при `firstVerb == 0` его отрицание всегда истинно.

Отсюда проверяемый аналитический контрпример на **POS уровня правила**, не на сырых строках:
- `[VERB, PRON, NOUN]`, которое по исторической классификации представляет VSO, классифицируется нынешним условием как **IMPERATIVE_V**.
- Контроль `[VERB, NOUN]` действительно остаётся IMPERATIVE_V.
- `[PRON, NOUN, VERB]` может попасть в SOV, контроль для существующей поддержки структуры.

Это **SOURCE_CONFIRMED для ветвления C#** и **SOURCE_EQUIVALENT_PROXY для получаемых ответов на ручной POS-последовательности**; реальный tokenizer может по-разному разметить исходные русский/английский/иврит, поэтому без native raw-input вызова **не утверждать measured domain regression**.

### Доказательство B — скрытый schema/label seam

[Donor PatternClassifier, labels 0–7](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/12.SemanticCompiler/Translation/PatternClassifier.cs#L5-L26):
`SVO(0), SOV(1), VSO(2), IMPERATIVE_V(3), IMPERATIVE_VS(4), QUESTION_SV(5), QUESTION_VS(6), TOPIC_OV(7)`.

[Current Unit enum](https://github.com/lubochka/llm_mvp_core/blob/649b9343cfda5b81cc54396a3840ad5db5e0b179/27.TranslationDomain/TranslationUnitContracts.cs#L22-L43):
`SVO(0), SOV(1), VSO(2), IMPERATIVE_V(3), IMPERATIVE_VS(4), SVO_INF(5), UNKNOWN(6)`.

Из 8 старых и 7 новых меток **только пять совпадают по смыслу и числу**. При копировании ordinal labels без явного преобразования старое `QUESTION_SV=5` стало бы `SVO_INF`, `QUESTION_VS=6` стало бы `UNKNOWN`, а `TOPIC_OV=7` вообще не представимо. **Это конкретная несовместимость, не доказательство того, что текущий мигратор действительно делает такой cast:** старого checkpoint E25 у `external_domains` в этой сессии не извлекали. Нельзя молча переносить `int Pattern` в новый `enum SentencePattern`. Понадобится явная таблица поддержанных трансформаций и отчёт об утраченной семантике.

**Гипотеза качества:** если обученные E25 структуры реально подать в новый Unit, wrong/unsupported labels либо отбрасывают уроки, либо подменяют тип структуры. **Ограничение:** относится к синтаксису языка, не к C# AST retrieval или общему DPO. Нужен тест нового Unit + фактического consumer, а не только enum reflection.

## 3. N50-02 · Обнуление kNN-дистанции на коротких POS и потеря кратности

[Donor `SequenceDistance`](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/12.SemanticCompiler/Translation/PatternClassifier.cs#L69-L86) строит **множества** POS-биграмм и возвращает `union==0 ? 0 : 1-intersection/union`.

Два аналитических примера (ручной source-equivalent расчёт, **не** нативный .NET run):
- `Distance([VERB], [NOUN]) = 0`: оба множества пустые; совершенно разные однословные последовательности ошибочно неразличимы. При `k=3` выбор класса зависит от порядка вставки соседей/голосов.
- `Distance([NOUN,VERB,NOUN], [NOUN,VERB,NOUN,VERB,NOUN]) = 0`: обе последовательности имеют один и тот же **set** двух биграмм; отличие кратности/повтора исчезает.

У донора в E25 синтетические обучающие шаблоны имеют длину от двух POS, поэтому положительные **87,5%** не обязаны покрывать single-token edge. Влияние неодинаково: отдельное короткое повеление RU/EN может требовать unigram fallback; повторение структур может быть существенным для сложных/слитных предложений, но вторую функцию нельзя включать во все домены без проверок. Для C# используются отдельные AST features, а не POS.

**Новый точный риск:** даже после восстановления настоящего E25 Unit в RP01, слепое копирование `SequenceDistance` создаёт ложные tie/соседей на незамеченных в прежнем успешном опыте входах.

## 4. N50-03 · Последующее обучение E25 удаляет ранее выученную память

В [`PatternClassifier.Train` lines 37–44](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/12.SemanticCompiler/Translation/PatternClassifier.cs#L37-L44) сначала выполняется `_train.Clear()`, затем `_train.AddRange(examples)`. На одном вызове это корректное batch fit и объясняет успех E25; **при повторном Train для очередного урока старые neighbors исчезнут**.

**Важно:** это **унаследованное ограничение**, а не подтверждённая текущая regression или баг `TranslationMachine` (эта машина обучает другие memory maps и Units). Если планируемая интеграция вызывает только один `Train` на *полном накопленном* корпусе, стирание памяти не происходит. Если вызывается `Train(newBatch)` без накопления — появится forgetting; нельзя лечить его автоматическим replay без указания, к какому learner относится replay.

## 5. Как исключить дублирование и неверные выводы

| Утверждение | Статус |
|---|---|
| E25 POS-kNN когда-то улучшил синтетическую pattern classification | **HISTORICAL_RUN** с указанным corpus/32 heldout |
| Шесть donor/current файлов сохранены функционально | **SOURCE_CONFIRMED** по полному тексту |
| Current tokenizer не использует обученную E25 POS-kNN память | **SOURCE_CONFIRMED для указанного вызова** |
| Verb-first example попадает в wrong branch условия | **SOURCE_EQUIVALENT_PROXY**, native raw text не проверен |
| Межветочное ordinal отображение выполнялось и ломало checkpoint | **HYPOTHESIS, NOT_ESTABLISHED** |
| Пустые биграммы дают false zero distance | **SOURCE_CONFIRMED** для формулы; влияние на качество ещё NOT_RUN |
| `Train(newBatch)` стирает старые E25 записи | **SOURCE_CONFIRMED** для API, фактическая интеграция ещё не проверена |
| От этого падает качество отдельного RP01 DPO-LoRA Student | **NOT_ESTABLISHED**: отдельный learner/consumer |

Доказанные прежними агентами N26-E27, E26 unknown-context/order, shared KNN, incomplete vector transfer и E09 unfair heldout не заявляются новыми находками. Настоящая **научная новизна здесь локальна и условна**: искать overlap по алгоритму + source + consumer, не по заголовку документа. [Исполнимый малый A/B протокол для этих трёх находок](50-EXTERNAL-DOMAINS-CAUSAL-MICRO-EXPERIMENTS-2026-10-19.md) определяет граничные случаи и falsifier.

## 6. Что не делали

Нет native .NET build, replay, обучения, генерации, восстановления модели, анализа фактических production-запросов; не менялись исходники/Unit и protected каталог. Этот пакет предлагает **причинные проверки**, а не сообщает об уже достигнутом повышении качества.