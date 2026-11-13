# 23. Какие признаки теряются между языковым входом, памятью слова и арбитром

## Дополнение: адреса после обучения и сохранённые RuleTags

Свежая сверка helper26 language-key/RuleTag на core `d5c2409b24292d361593b8262b0c54997b15f539` подтверждает несовпадение адресов. [Базовый tokenizer](../../../27.TranslationDomain/TranslationSemanticTokenizer.cs:118) добавляет RuleTags в полную, префиксную и структурную signatures (124–150). [SemanticTokenizerUnit](../../../27.TranslationDomain/TranslationTrainedUnits.cs:94) после exact correction или изменения Pattern перестраивает PrefixSignature/StructureSignature методами150–160, которые RuleTags не добавляют. [Executor training](../../../27.TranslationDomain/TranslationTrainedUnits.cs:417) записывает prefix/structure от нового базового tokenizer; inference использует полученную последовательность Unit (333–345). Это действительный шов train/infer адресов.

**Уточнение свежего чтения:** `seq with { ... }` не удаляет само свойство RuleTags; меняются лишь названные signatures и Pattern. Полная Signature может сохранить теги в chosen или в неизменённом суффиксе исходной подписи. Даже [wordmap consumer](../../../27.TranslationDomain/TranslationTrainedUnits.cs:273) продолжает видеть `tokens.RuleTags.Count`. Поэтому «после обучения грамматические признаки полностью исчезают» — чрезмерный вывод. Потеря в двух retrieval-ключах подтверждена, потеря всех признаков — нет.

Exact corrections индексируются только Normalize(rawText) с OrdinalIgnoreCase (85–86,94,140); SourceLang в ключ не входит. Один semanticTokenizer передан всем шести [направлениям](../../../27.TranslationDomain/TranslationMachine.cs:730). Executor-словари при этом отдельные. [Checkpoint export](../../../27.TranslationDomain/TranslationTrainedUnits.cs:196) сохраняет raw key, Pattern и Signature, без языка. Уже утраченный язык нельзя надёжно восстановить простым новым полем при загрузке.

Причинное ограничение остаётся: exact-source/phrase/word maps могут ответить до kNN (264–277); обычная self-label пара может не изменить Pattern у невиданного текста. Прямая разница адресов не равна наблюдённому ухудшению перевода. Будущий различающий опыт должен фиксировать caller, активную ветку и одновременно сравнить все три signatures, RuleTags как отдельное поле, найденную память и итоговый ответ. Исполнение не выполнялось; это дополнение к проекту опыта, не новый runner.

16.10.2026. READ_ONLY_RESEARCH. Проверен код core `259c61769c3c66c9353f70d3dd569da19682067b`. Источники новых гипотез — helper23 language/POS, E26 и verifier; planning `f5188973c9fa`. Ни helper-пилоты, ни tokenizer/Unit здесь не выполнялись. Исторические положительные E25/E26 и другой production29 classifier разобраны отдельно в [17](17-positive-pos-knn-versus-production-rules.md).

## 1. Один и тот же язык может получить разные признаки

[TranslationRoutingFlow.cs](../../../27.TranslationDomain/TranslationRoutingFlow.cs):133–153 нормализует код языка для выбора направления. Но 65–78 передаёт адаптеру первоначальный request. [TranslationDirectionAdapters.cs](../../../27.TranslationDomain/TranslationDirectionAdapters.cs):124 передаёт первоначальный SourceLanguage в tokenizer.

[TranslationSemanticTokenizer.cs](../../../27.TranslationDomain/TranslationSemanticTokenizer.cs):272,306 проверяет точное `lang == "ru"`. Поэтому `ru`, `RU` и `ru ` выбирают одно направление Flow, но не обязательно одни POS/concepts. Builder, [536–545](../../../27.TranslationDomain/TranslationStructuredBatchBuilder.cs), задаёт учебному RU_EN lowercase `ru`. Различие train/query представления возможно без изменения выученных весов или памяти.

**Поправка helper-вывода:** BuildRuleTags в tokenizer153–157 использует OrdinalIgnoreCase. Утверждение, что сама эта проверка отвергает `RU`, неверно; пробел она не удаляет. Но tags зависят и от уже полученных tokens, где ClassifyPos/ResolveConcept case-sensitive. Нельзя объединять эти три проверки в одну. Exact-source lookup или сохранённая tokenizer correction могут скрыть эффект на знакомом запросе — это не доказательство, что остальные signatures одинаковы.

Для EN/HE/ES обычное слово, не распознанное как code identifier/специальный термин, в этом tokenizer становится NOUN, а concept — UNKNOWN (260–323). ClassifyPattern в352–361 возвращает SVO даже без глагола. POS bigrams есть в143–150, но если исходные теги чрезмерно общие, само их наличие не делает представление различающим. Это недостаточная языковая специализация данного пути, а не утверждение обо всех токенизаторах XIIGEN.

## 2. Исторический E26 выбирал по соседям, нынешняя память слова выбирает по другому ключу

Исторический ContextualDictionary использует ContextBefore/After и приоритет. Текущий [ExecutorUnit](../../../27.TranslationDomain/TranslationTrainedUnits.cs):744–753 хранит `(normalized word, concept) → target` и запасной `normalized surface → target`; 860–899 применяет эти ключи. POS слева/справа там не участвуют. Запись того же key присваиванием заменяет прежнее слово. Фильтр ShouldAcceptStandaloneWordMapping существует; он не восстанавливает недостающий контекст ключа.

В нынешнем tokenizer15–29 есть статическая E26Mappings, но это `(stem, previous known concept) → concept`, а не тот же старый POS-neighbor поиск. Например, две записи для «поток» обе дают THREAD_REF; будущий POS в lookup не участвует. Переменная prevConcept в98–114 обновляется только при известном concept и не сбрасывается при точке. Следовательно, это **последний известный concept**, а не обязательно ближайшее слово или тот же clause. Явного установления связи с конкретным объектом здесь не видно. Сброс на каждой точке не объявляется универсальным исправлением: он может удалить полезную межфразовую связь; нужен доменный контраст.

Текущий TranslationPos enum ([contracts](../../../27.TranslationDomain/TranslationUnitContracts.cs):42–55) отличается от исторических числовых тегов E26. Автоматически переносить номер старого тега в новый enum было бы неверно. Фактический такой cast не найден; это ограничение будущего переноса, не существующая доказанная ошибка cast.

Реальный consumer не выбирает этот word map первым: Executor264–293 сначала exact-source, потом phrase composition, потом word composition, затем отдельные kNN пути. Поэтому отсутствие POS в word key не доказывает ошибочный конечный ответ любого неоднозначного слова. Ранний exact/phrase успех способен скрыть проблему. Текст комментария «KNN lookup → exact» в263 не соответствует прочитанной очереди; оценивать нужно тело метода.

## 3. Накопление требований может отвергнуть ранее правильный ответ

QualityVerifier в [TranslationTrainedUnits.cs](../../../27.TranslationDomain/TranslationTrainedUnits.cs):1045–1088 хранит правила нескольких видов. Они **не одинаковы**:

- Для Signature перебирает все required terms и требует каждое: AND.
- Для SourceMarker принимает любое из сохранённых допустимых слов: OR.
- Для `(src,ctx)` фактически проверяет только наличие src-concept; ctx выводится в issue, но не проверяется как условие применения.

В1095–1144 FAIL-примеры могут дописывать требования к общей Signature. При одинаковой Signature разных законных случаев старый и новый target способны стать одновременными обязанностями. В другом branch generic required term записывается в signature только при SignatureHasSpecificConcept и если это не quality-marker issue. Эти ограничения надо сохранять: не каждый FAIL-урок загрязняет общий signature list.

В [TranslationMachine.cs](../../../27.TranslationDomain/TranslationMachine.cs):717–737 создан один verifier, а executors отдельные по направлениям. В показанных Verify keys и параметрах нет target language. Поэтому общие signature/concept правила могут требовать слово другого целевого языка, если такие уроки пришли в общий verifier. Это условная архитектурная связь; частота конфликтов сохранённой памяти не подсчитана.

Путь до ответа прослежен: adapter124–139 вызывает verifier и при обычном FAIL отдаёт fallback. У phrase-map с confidence≥.85 есть исключение, способное скрыть отказ verifier. Поэтому в будущем опыте нужны итоговый путь и сам verification result; одна правильная фраза не опровергает конфликт внутренних правил.

## Небольшие будущие различающие проверки — не запуск

**Language code.** Одна сохранённая модель и один русский вход, `ru/RU/ru `; сравнить POS/concepts/signatures, выбранный tier и предметный ответ. Отдельно знакомый exact input и новая комбинация тех же известных слов. Положительный результат только на exact-пути не проверяет kNN generalization.

**Контекст слова.** Два допустимых значения одного слова, контраст только в соседнем POS/референте; две записи памяти и новые фразы. Сравнить текущий key с отдельным вариантом key, включающим нужный контекст. Не менять одновременно учебные ответы, порядок правил и tokenizer. Наблюдать фактически использованный map. Отдельная панель с межфразовым referent нужна прежде автоматического сброса context.

**Арбитр после нового урока.** Сохранить правильный старый ответ; добавить один законный, другой по смыслу урок с той же вычисленной Signature. На тех же ответах проверить до/после список issues, Pass и конечный tier. Затем отдельно языковые направления. Если signature отличается или правило попадает только в marker-OR, гипотеза signature-AND для этого примера не активировалась. Если внутренний отказ не меняет потребительский ответ из-за phrase exception, нельзя объявлять потерю пользовательского качества.

Для всех трёх: собственная проверка источников устанавливает условия, а не величину ущерба. Ни измерения качества, ни training/inference не запускались. Отличия относятся к structured translation consumer; связи с RP01 neural free generation не установлено.
