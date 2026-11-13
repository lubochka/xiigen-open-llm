# N37 — новые причинные стыки: неизвестные модификаторы в семантическом ключе, преждевременная остановка каскада KNN и асимметрия сходства

**Дата:** 16.10.2026. **Статус:** CODE-SOURCE-VERIFIED; EFFECT_ON_LIVE_QUALITY=HYPOTHESIS; NATIVE_DOTNET=NOT_RUN; INDEPENDENT_REVIEW=PENDING. **Срез:** llm_mvp_core/codex/dpo-implementation @ 793d7b755b23aac90445da3ac6661b1bff7e6672; xiigen-planning/llm_plan3. **Единственное место записи:** этот каталог possibleDifs. Каталог llm_mvp_core/docs/plans/replanning-possible-diffs — READ_ONLY.

## Короткое объяснение

В обычном обучении важно не только запомнить удачный урок. Нужно, чтобы новый запрос получил правильный урок и не потерял значимые условия. В рассмотренном переводческом KNN обнаруживаются **три узкие точки**, каждая из которых способна испортить полезный результат *после* успешного сохранения обучения:

1. **N37-A — неизвестные слова не всегда входят в семантический fingerprint.** Если к технической фразе добавили смысловое условие, которое tokenizer классифицировал как UNKNOWN, полный ключ может не измениться; поиск считает два разных запроса одинаковыми по ключу.
2. **N37-B — «найден сосед» ошибочно приравнивается к «найден пригодный ответ».** Точный tier останавливает cascade до проверки target-language и других downstream-ограничений. Все найденные кандидаты могут быть затем отклонены, хотя на следующем tier доступен допустимый ответ.
3. **N37-C — близость односторонняя, без штрафа за чужие дополнительные условия.** SourceSimilarity делит пересечение на количество содержательных токенов *запроса*, а не на объединение источника и запроса. Если запрос — подмножество старого source, score может быть 1.0 даже при дополнительных смысловых условиях в сохранённом уроке.

Это **гипотезы качества именно TranslationKnnStore/ExecutorUnit**, не доказанные причины свободной генерации RP01 Student DPO/LoRA. Важный отрицательный контроль: user-flow может завершиться на exact-source/phrase/word-map ещё до KNN.

## Первоисточники, читаемые строки

| ID | Факт исходного кода | Точный источник |
| --- | --- | --- |
| S37-01 | Tokenize строит Signature из Pattern, **только** tokens с Concept != UNKNOWN и rule tags; PrefixSignature — первые 2 concept и первые 3 тега. Обычные слова без известного concept могут не попасть в full semantic key. | [TranslationSemanticTokenizer.cs L98–144](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationSemanticTokenizer.cs#L98-L144) |
| S37-02 | Для non-Russian ClassifyPos, кроме ASCII code identifiers и null/true/false, возвращает NOUN; при отсутствии VERB ClassifyPattern возвращает SVO. Это может сохранить coarse pattern при добавлении модификаторов. | [TranslationSemanticTokenizer.cs L260–295](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationSemanticTokenizer.cs#L260-L295), [L351–362](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationSemanticTokenizer.cs#L351-L362) |
| S37-03 | Full tier получает minimumSourceSimilarity=0 при «информативной» сигнатуре; IsLowInformationSignature смотрит на количество компонентов и совпадение остальных с именами POS; наличие concept-компонента может снять source guard. | [TranslationUnitContracts.cs L246–306](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationUnitContracts.cs#L246-L306) |
| S37-04 | SourceLaw.Compare возвращает Unknown за пределами узкого английского шаблона article + copular clause. Unknown не является veto. | [TranslationMemorySourceLaw.cs L11–57](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationMemorySourceLaw.cs#L11-L57) |
| S37-05 | FindNearestCascade делает return для любого непустого full, затем prefix, затем structure. Он не знает, будет ли target принят downstream. | [TranslationUnitContracts.cs L246–267](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationUnitContracts.cs#L246-L267) |
| S37-06 | Execute сначала пробует exact-source, phrase, word-map; KNN вызывается условно. В TrySelectKnnCandidate FindNearestCascade(k:5) предшествует фильтрам target compatibility и опционального дополнительного сходства; если все 5 отвергнуты, другой tier не запрашивается. | [TranslationTrainedUnits.cs L262–365](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationTrainedUnits.cs#L262-L365) |
| S37-07 | SourceTokens удаляет англоязычные function words (среди них «not»), SourceSimilarity = overlap/queryTokens.Count. Это **не** симметричный Jaccard; query-source containment и source-query containment могут различаться. | [TranslationUnitContracts.cs L186–200](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationUnitContracts.cs#L186-L200), [L308–325](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationUnitContracts.cs#L308-L325) |
| S37-08 | TrainedTranslationAdapter принимает кандидата только после Verifier/Confidence, а TranslationRoutingFlow возвращает конечный текст; подтвердить конечную полезность нужно здесь, не по одному ANN hit. | [TranslationDirectionAdapters.cs L117–142](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationDirectionAdapters.cs#L117-L142), [TranslationRoutingFlow.cs L44–132](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationRoutingFlow.cs#L44-L132) |

## H37-A — структурный fingerprint не кодирует некоторые существенные условия

**Условный механизм.** Например, технический source с известным концептом null и сохранённым идентификатором может получить тот же full semantic key после добавления неизвестных слов «without consent», хотя ответ обязан учитывать добавленное условие. Если после native Tokenize действительно совпали Signature и RuleTags, IsLowInformationSignature может объявить ключ информативным и отключить lexical similarity для full tier; narrow SourceLaw не распознаёт такой технический текст. Сосед оказывается допустимым к выбору **по механике поиска**, не по смыслу.

**Не утверждаем до измерения:** что пример с конкретным идентификатором даёт одинаковые подписи на этой ревизии; что unknown-tail всегда меняет смысл; что stored chosen пригоден; что обычный запрос достигает KNN. Для тренирующегося Executor пример.SemanticSignature может дополнительно нести PAIR/правило rejected, поэтому native experiment должен сначала вывести три training/query keys, а не предполагать их равными.

**Существенная область:** технические RU↔EN/EN↔HE/EN↔ES с условием, отрицанием, ограничением или указанием роли. C# AST — только концептуальная аналогия: у C# другой tokenizer/consumer; этот класс не является его прямым дефектом. Простое сохранение количества слов в индексе не гарантирует правильной семантики: после проверки источника требуется domain-specific representation и learned relevance.

### Дополнительный N37-A/BOOL: true и false — POS есть, предметного Concept нет

**Более строгая source-backed проверка, найденная при code review:** [ClassifyPos L260–274](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationSemanticTokenizer.cs#L260-L274) явно называет null/true/false техническими токенами (TranslationPos.TECH_TERM). Но [ResolveConcept L295–326](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationSemanticTokenizer.cs#L295-L326) выдаёт из этих технических литералов отдельный SemanticConcept только для **null**, тогда как true/false в non-Russian source попадают в UNKNOWN. В английском BuildRuleTags возвращает пустой список: [L153–160](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationSemanticTokenizer.cs#L153-L160).

Потому входы **«null customerId42 true»** и **«null customerId42 false»** при описанных правилах сохраняют одинаковые known concepts NULL_LITERAL + CODE_IDENTIFIER, coarse POS и Pattern=SVO; следовательно, их full semantic Signature совпадает **согласно самому коду** (без запуска live Unit). В полном tier с этим ключом SourceLaw вне своего copular-шаблона не различит true/false, а source threshold может быть нулём. Это сильнее общей гипотезы UNKNOWN-tail: здесь предметная противоположность явная и проверяемая. Но финальный пользовательский неправильный перевод ещё не наблюдался, а ранние mappings, source-family, builder signature и Verifier могут менять маршрут. **Native falsifier:** вывести actual keys, добавить один законный lesson true, запросить false с независимым gold; сравнить current vs boolean-sensitive key/guard на одинаковом Unit state, затем проверка cold restore.

Неверный первый JS прототип использовал отсутствующие в enum TRUE_LITERAL/FALSE_LITERAL; **он исправлен до публикации данного уточнения**: [актуальный proxy](37-MICRO-CASCADE-UNKNOWN-PROXY.mjs) использует действительные NULL_LITERAL/CODE_IDENTIFIER. JSON receipt также обновлён после повторного локального исполнения. Этот факт ремонта не является выполнением нативного обучения.

## H37-B — downstream filter не может запросить следующий tier

На точном tier найдены X1,X2. По source они были допустимы, но оба перевода не соответствуют целевому языку. Prefix содержит Y, соответствующий смыслу и target direction. Baseline FindNearestCascade возвращает X1,X2 и прекращает поиск; Executor отвергает их, но prefix не вызывается. Если A/B разрешит **продолжить cascade только когда все кандидаты точного tier действительно неприемлемы по тем же текущим downstream laws**, Y может быть возвращён.

Это отдельный дефект от уже известных **N16 FIFO** (старые записи до новых), **N34 alias-дубли** (две записи одного урока съедают top-k) и **N30 direction collision** (чужое направление может быть принято). Здесь исход может быть хуже из-за **неправильного порядка принятия решения между tier и target validation** даже при k=5, одной записи на ключ и корректных раздельных языковых метках.

**Защита от ложного исправления:** не позволять слабому tier обходить SourceLaw/Verifier; не падать на next tier при одном допустимом, но низкоранжированном кандидате без отдельной гипотезы reranking; при неизвестном/непроверяемом target сохранять abstain, а не «первый попавшийся».

## H37-C — containment-асимметрия меняет допустимость урока при развороте source/query

Берём два содержательных множества Q={null,CustomerId42} и L={null,CustomerId42,without,consent}, предполагая, что токены реально так выделены. При текущей формуле:

- SourceSimilarity(Q,L) = 2/2 = **1.00**.
- SourceSimilarity(L,Q) = 2/4 = **0.50**.
- Jaccard(Q,L) = 2/4 = **0.50**.

В широком fallback source-aware порог 0.35, в prefix 0.25: конкретно этот пример может проходить обе стороны; сама асимметрия уже SOURCE-CONFIRMED, а **отказ на пороге** зависит от набора токенов/ключа. Для более длинных добавлений и выбранного frozen порога асимметрия может решать судьбу урока. Для изменения смысла с отрицанием особая проблема: «not» удаляется как stopword, но специализированная SourceLaw распознаёт лишь узкую форму «the box is not empty». Для остальных доменов семантический эффект должен определять предметный oracle, а не только Jaccard.

## Что уже выясняли другие агенты: граница новизны

- [N16 FIFO и TopicDomain](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md) — проблема временного порядка и области хранения, не потери UNKNOWN-модификатора в full signature.
- [N30 шесть направлений](30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md) — проблема возможности попасть в KNN и cross-direction contamination. N37-B проверяет **post-filter fallback starvation**.
- [N34 alias и top-k](34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md) — дубли ключей, не условие full-return-before-eligibility.
- [N27 порядок и отрицание](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) и [N35 wordmap confidence](35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md) — другие слои; признаём пересечение по риску семантического закона.
- [N28 tokenizer/index key drift](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md) — расхождение между producer и consumer ключа, не совпадение **двух семантически разных** source внутри одного корректно вычисленного ключа.

«Новое» значит новая **проверяемая причинная связка** после адресной сверки с перечисленными evidence owners. Полный независимый поиск по содержимому каждого из сотен файлов НЕ проведён; право на абсолютную уникальность не заявляется.

## Разграничение с RP01 и первая развилка

RP01 C#500 и ENRU500 имели изменившиеся q/v-LoRA weights, но 0/3 и 0/4 free-gain; первоочередная neural-гипотеза — **random non-pretrained B0 + DPO preference objective без предварительного умения генерировать**. Ни один N37 KNN-патч **не обучит neural Student писать текст**, если его consumer вообще не читает TranslationKnnStore. Сначала source→recipient→actual ordinary consumer и domain routing; только потом KNN-native A/B. См. [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).

Полный протокол микроопытов: [37-MICRO-THREE-FACTOR-KNN-CAUSAL-PROTOCOL-2026-10-16.md](37-MICRO-THREE-FACTOR-KNN-CAUSAL-PROTOCOL-2026-10-16.md).

**Review gate:** отдельный reviewer проверяет соответствие ссылок этой конкретной ревизии source, не дублирует найденное, требует native key/neighbor/final-response traces, и понижает любой «quality improved» до HYPOTHESIS, если нет одинакового слепого heldout. Доказательство от исходников ≠ воспроизведённый .NET эксперимент ≠ успешное обучение.
