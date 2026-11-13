# RU→EN: три уточнения потери признаков до KNN

## Результат и границы

Найдены три **source-confirmed механизма**, для которых выполнена ограниченная source-equivalent Python-проба. Это не .NET, не обучение DPO, не доказательство улучшения обычного ответа. `NATIVE_NOT_RUN`, `QUALITY_GAIN_NOT_ESTABLISHED`.

Важная граница относительно успешной ветки: **все три механизма унаследованы из `DesignToCode-clean`, а не появились после неё**. В `e346b88a3cc1fcce48aefa626bfc7b00509eefd6` и `358af351e22a92a3f7938d1f9221d5aeb7ddb053` у `27.TranslationDomain/TranslationSemanticTokenizer.cs` один Git blob `ee6d81c7933fa33a9c57a2111990019ec55e5dab` ([независимая tree-квитанция](evidence/knn-independent-tree-proof.json)); у `TranslationTrainedUnits.cs` — `1742abfc9fb617dfc66e4d618e0c169da01a6ccc`. Поэтому их следует ставить после проверки восстановления прежнего trained state и consumer. Нельзя писать, что именно они объясняют clean→RP01 падение.

Относительная новизна: recovery_catalog завершил поиск по **415 Markdown possibleDifs + 92 основным Markdown защищённого каталога**, дополнительно сверялся общий индекс карточек. Для `TrimVerbSuffix`, `RU:TENSE:PAST`, `ResolveRussianSubjectGenderUtf8`, `ResolveRussianPastVerbGenderUtf8` точных прежних механизмов в этом прочитанном наборе не найдено. `BaseConceptsRu` встречается в уже известном N23 про предыдущий concept вместо двустороннего POS-контекста. Статус этих трёх уточнений — **BOUNDED_NEW относительно названного набора**, не глобальное утверждение о всех работах других агентов. Позднее также дочитаны 33 новых/изменённых planning Markdown и следующая редакция master; ещё 15 версий следующего пакета также проверены; KU-01…03 совпадений не получили ([квитанция](evidence/planning-late-dedup-receipt.json)). Расширение чтения защищённого каталога было заблокировано автоматической проверкой; новые запросы туда не выполнялись.

## Карта исполнения и два важных ограничения

Фактическая цепочка:

`TranslationStructuredBatchBuilder.BuildGeneralBatch` → базовый `TranslationSemanticTokenizer.Tokenize` → typed semantic/executor/verifier rows → `TranslationMachine.TrainStructuredBatchAsync` → `ExecutorUnit.TrainOnExample` → signature/prefix/structure в `TranslationKnnStore`.

Обычный ответ: `TrainedTranslationAdapter.TranslateAsync` → `SemanticTokenizerUnit.Tokenize` → `ExecutorUnit.Execute` → exact-source / phrase-map / word-map / KNN → `QualityVerifierUnit.Verify` → ответ либо Bootstrap.

Источники текущего pinned commit:

- [Builder:76–142](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L76-L142).
- [Unit:89–119 — обученная correction меняет Pattern/Signature, но не `seq.Tokens`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationTrainedUnits.cs#L89-L119).
- [Unit:264–298 — ранние ответы и gate перед KNN](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L298).
- [Unit:404–449 — занесение learned target и базовых prefix/structure](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L449).
- [Adapter:124–139 — фактический consumer](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationDirectionAdapters.cs#L124-L139).

Отсюда два ограничения: ранняя точная/фразовая/словная память может скрывать дефект KNN; изменение signature при несогласованном старом индексе само создаёт несовместимость. В A/B все адреса памяти надо получить из одного и того же source-корпуса соответствующим вариантом encoder, сохранив approved chosen/WordMappings/PhraseMappings bytes и разделение train/held-out. Связь encoder с построением targets проходит через [BuildWordMappings:328–400](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L328-L400); повторный builder без такого контроля не является чистым опытом над KNN-адресом. Это не разрешение повторно применить старые DPO к production model.

## KU-01. Обрезание слова разрушает существующий словарь concept и может закрыть сам вход в KNN

### Что происходит

Словарь содержит основы `добавит`, `проверит`, `запустит`, `файл` [Tokenizer:32–51](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L32-L51). Но `ClassifyPos` сначала применяет `TrimVerbSuffix` [260–289](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L260-L289), а `ResolveConcept` затем ищет **полученную lemma** через `StartsWith(key)` [295–334](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L295-L334). Порядок суффиксов — [365–379](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L365-L379).

| Вход | Текущая lemma | Полученный concept | Concept существующего словаря по исходной форме |
|---|---|---|---|
| добавить | добави | UNKNOWN | ADD_OP |
| добавите / добавит | добав | UNKNOWN | ADD_OP |
| запустить | запусти | UNKNOWN | LAUNCH_OP |
| запустите / запустит | запуст | UNKNOWN | LAUNCH_OP |
| проверить | провери | UNKNOWN | VALIDATE_CHECK |
| проверите / проверит | провер | UNKNOWN | VALIDATE_CHECK |
| файл | фай | UNKNOWN | DATA_ENTITY |

Последняя строка — дополнительный контрпример того же семейства. Эвристика `EndsWith("л") && Length > 3` распознаёт существительное «файл» как VERB [543–564](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L543-L564). Это **не повтор известной ошибки `DeriveGenderAlternateForm`**, которая создаёт ложные mappings для «пила/пил»: здесь ещё до обучения слова меняются POS, lemma и concept исходного запроса.

У «запустить поток» и «запустите поток» потеря LAUNCH_OP лишает следующее слово контекста: существующая E26-запись `(поток, LAUNCH_OP) → THREAD_REF` [15–29](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L15-L29) не срабатывает. Проба дала `[UNKNOWN, UNKNOWN]`. Если exact/phrase/word path не вернул ответ и нет `PAIR:` signature, RU→EN не входит в `TrySelectKnnCandidate` вообще: `hasSemanticConcept=false`, а обычная ветвь без concept отдельно разрешена только EN→RU. Диагностический surface-first lookup того же словаря дал `[LAUNCH_OP, THREAD_REF]` и открыл этот gate. **Реальный KNN candidate и правильный ordinary output пробой не измерялись.**

### Почему существующие проверки этого не опровергают

В [checkpoint test:2965–2984](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/Tests/TranslationDomainTests.cs#L2965-L2984) задаются ручная signature и word mappings для «запустить поток», после reload исполняется тот же source. Такое восстановление может быть правильным при сломанном извлечении concept на новом source. Этот тест не запускался здесь; использован только как прочитанный ограниченный контракт. В [word-map test:371–401](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/Tests/TranslationDomainTests.cs#L371-L401) также возвращается обученный source, включая пары mappings одновременно для конкретного concept и UNKNOWN.

### Быстрый опыт KU-01-A

Домен: перевод технических инструкций, профиль RU→EN; получатель — SemanticTokenizerUnit/ExecutorUnit, не neural RP01 decoder. Tier 1: 6–9 существующих разрешённых training rows по ADD/LAUNCH/VALIDATE и 12 source-disjoint запросов, разделённых по семейству исходного примера.

A: current encoder. B: при фиксированных POS/RuleTags проверить исходную очищенную словоформу тем же concept lookup перед потерей суффикса; остальные источники решений не менять. Это диагностический one-factor contrast, не предложение объявить статический словарь полноценным trained classifier. Сначала без обучения вывести `Word/Lemma/POS/Concept`, signatures и KNN-gate. Затем два свежих исследовательских состояния с **одинаковыми teacher-approved chosen, WordMappings и PhraseMappings bytes** и обычный adapter с полной трассой paths. Нельзя просто повторить BuildGeneralBatch с новым encoder и назвать изменившимися только keys: BuildWordMappings использует token.Concept и может изменить саму словную память. Для KNN-адресного contrast меняются только source-derived tokens/addresses при замороженных targets; новый word-map path, если его исследуют, выделяется отдельным опытом. Не обнулять early maps, чтобы вынудить успех; выбирать unseen запросы, а случаи маскировки честно отмечать.

Метрики: token-concept accuracy по независимой разметке; доля запросов, достигших KNN; наличие полезного соседа; semantic correct / wrong accepted / Bootstrap по 12 запросам. Контроли: «проверку», `null`, «метод» сохраняют исходные concepts; EN→RU и запросы, уже отвечающие exact-source, не должны менять ответ. Negative control — одинаковые arbitrary concepts не считаются восстановлением смысла.

Фальсификатор причинности: native tokenizer не воспроизводит потерю; ordinary caller стабильно получает правильный ответ раньше этого gate; либо исправленные concepts не повышают качество при том же наборе и дают новые wrong accepts. Последнее — отсутствие сигнала в малом пилоте, не опровержение метода на всех доменах.

Ремонт после подтверждения: внутри существующего SemanticTokenizerUnit ввести версионированные `TokenConceptCorrection`/`MorphologyFeature` rows с normalized surface, lemma, POS и concept, обучаемые на разрешённом выбранном target; строить все три адреса из этих tokens. Пока API `TokenizerOutput` хранит только Pattern/Signature, он не может обучить отсутствующий concept токена. Изменение только named signature — известный неполный ремонт. Сохранение/импорт должны переносить feature schema и source language; index rebuild обязан использовать те же source rows и не изменять target-память.

## KU-02. Признак PAST назначен по самому факту VERB

В [BuildRuleTags:167–178](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L167-L178) `AddTag("RU:TENSE:PAST")` выполняется до распознавания морфологического прошедшего времени. Gender helper влияет только на дополнительные gender/agreement tags. Поэтому проба получила PAST у «Она работает», «Она будет работать», «работать», «запустите поток». Два прошлых времени «Она ответила»/«Он ответил» сохранили правильный PAST-контроль.

Эта метка входит в full signature, первые три RuleTags в prefix и все tags в structure [118–143](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L118-L143). Следовательно, это реальный адрес retrieval, а не только ошибочный текст отчёта. Возможные эффекты — слияние настоящего/будущего/инфинитивного запросов и ложная специфичность KNN bucket. Знак изменения качества нельзя вывести из имени метки: одинаково ошибочная конвенция могла случайно работать на старом ограниченном корпусе.

### Быстрый опыт KU-02-A

Домен: бытовой перевод RU→EN и отдельная страта технических инструкций; получатель — extractor RuleTags и KNN. Tier 1: 6 текущих допустимых примеров, по два на времена; 12 новых запросов, где независимый gold различает прошлое/настоящее/будущее, и 4 инфинитивных/императивных контроля.

A: нынешний PAST-on-VERB. B: только удалить ошибочное PAST для заранее независимо размеченных nonpast токенов; не добавлять future/present признаки, не менять POS, concept, k, targets или scorer. Для проверки полной product-реализации источник `Tense` должен стать частью обучаемого/проверенного typed morphology state; использование gold flags допустимо только как верхняя диагностическая граница. Проба «если бы признак был правильным» не доказывает, что готовый runtime умеет его извлекать.

Метрики: PAST false-positive rate; число разных source-tense law classes внутри одного bucket; semantic tense correctness и wrong-accepted rate обычного adapter. Past-контроль должен сохранить tags; EN→RU не изменяется. В каждой ветке согласованно строятся train/inference keys, иначе измеряется новый key drift. Фальсификатор: текущая метка не меняет candidate path/ответ ни на одном заранее выбранном случае или убирает полезный fallback, повышая ошибки. Не добавлять одновременно новый classifier и новый KNN metric.

## KU-03. POS очищает пунктуацию, а морфологический род читает исходное Word

`ClassifyPos` вызывает `CleanWord` [260–264](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L260-L264), но `SemanticToken.Word` хранит исходный текст [98–111](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L98-L111). `ResolveRussianPastVerbGenderUtf8` затем делает `EndsWith` прямо на `token.Word` [497–512](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L497-L512). Поэтому у «Она ответила.» POS остаётся VERB, но FEM/AGREEMENT пропадают по сравнению с «Она ответила».

Проба воспроизвела эту разницу для четырёх пар с добавлением единственной конечной точки: «Она ответила», «Она вчера ответила», «Мама пришла», «Он ответил». POS последовательности сохранялись. Full signatures менялись во всех четырёх; диагностическое очищение Word **только при вычислении gender** восстановило равенство всех четырёх. В этой проверке нет model answer. Род существительного имеет аналогичный raw-word доступ [471–491](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationSemanticTokenizer.cs#L471-L491), но не надо превращать синтаксически спорный пример с запятой в gold для перевода.

Это отличается от известных P54/33: там после удаления отдельного punctuation-token расходятся индексы phrase/source arrays. Здесь число и порядок токенов одинаковы; меняется morphology address из-за того, что две стадии нормализуют одно слово по-разному. Также это не известный `prevConcept` через границу предложений.

### Быстрый опыт KU-03-A

Домен: RU→EN, короткие сообщения и описания действий. Tier 1: один замороженный admissible checkpoint/corpus; 8 **новых** source families, каждая в паре без/с terminal period. Training sources не должны быть этими же строками: [`NormalizeMapKey` удаляет пунктуацию](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationTrainedUnits.cs#L1008-L1022) и способен полностью скрыть KNN-различие.

A: raw Word для gender. B: тот же очищенный surface word, который уже получил ClassifyPos; не использовать обрезанную lemma вместо surface, поскольку suffix stripping удаляет сам gender cue. Остальные tags, POS, concepts и targets фиксированы; после change reindex соответствующего варианта. Сначала zero-training feature comparison, затем обычный adapter. Измерять равенство gender tags/prefix, path agreement и независимую semantic correctness обеих половин пары. Отдельно считать wrong accepted, а не только совпавшие ответы.

Контроли: предложения без конечного знака; EN→RU; истинная замена «она→он» должна продолжать менять род. Объединять все формы в один key для получения invariance нельзя. Фальсификатор: нормализация не исправляет native tags; ни один ordinary запрос не читает изменённые keys; либо растёт ложное принятие/теряются реальные различия. Семантически важную пунктуацию, например кавычки или вопросительную модальность, нельзя объявлять универсально несущественной.

## Выполненная проба, происхождение и воспроизведение

Артефакты: [скрипт](scripts/knn-unit-mechanism-probe.py), [итоговый JSON](evidence/knn-unit-mechanism-probe-result.json), [первоначальный результат с опровергнутым контролем](evidence/knn-unit-mechanism-probe-initial-failed-control.json).

Проверка запускает формулы на proper UTF-8 RU fixtures и извлекает C# словарь/порядок suffixes из исходника. Перед исполнением проверяется его Git blob SHA; единственная допустимая поправка локального кэша — один лишний последний LF, причём после его удаления SHA обязан точно совпасть. Source SHA256: `2013c0c066e1d3a0e93384420e73b61f34f00be9f476bc20bb49fabb0be1c1c7`.

Команда в clone, содержащем pinned source:

```sh
python scripts/knn-unit-mechanism-probe.py --source /absolute/path/to/llm_mvp_core/27.TranslationDomain/TranslationSemanticTokenizer.cs
```

Итог: 10 lexical misses на 13 фиксированных rows, три неизменных lexical controls, false PAST на 4/4 nonpast fixtures при сохранении 2/2 past controls, 4/4 punctuation address changes, 2/2 diagnostic E26 gate changes. Это **числа контрпримеров формулы, не accuracy модели**.

Первоначально «файл» был выбран как отрицательный контроль. Результат опроверг предположение: source объявил его VERB и удалил «л». Initial failed-control JSON сохранён; затем «файл» переведён в явный anomaly row, а контроль взят «метод». Native parity не заявляется. Без .NET в текущем окружении не запускались original C# или полный solution. Материалы core/planning не изменялись этим агентом; выполнены только read-only fetch и scratch research artifacts.

Для любого последующего создания настоящих DPO действует заданная пользователем роль: **CODEX на Astra 6 ultracode выбирает случаи и независимо оценивает качество пары/обычного ответа; внешняя открытая китайская модель создаёт chosen и rejected.** Изготовленные здесь формульные fixtures имеют происхождение diagnostic source counterexample и не выдаются за DPO от teacher. Конкретная доступность модели и receipt должны быть записаны при настоящем запуске, а не предположены по имени роли.

## Независимая проверка current consumer

Повторный запуск скрипта другим агентом дал тот же JSON. Отдельно сверены ранние ответы, RU→EN gate и вызов store. Whole KNN chain не объявляется неизменной: current применяет [SourceLaw filter до Take(k)](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationUnitContracts.cs#L268-L292), которого нет в [соответствующем clean-методе](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/27.TranslationDomain/TranslationUnitContracts.cs#L268-L288). Его [узкая английская copular policy](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/27.TranslationDomain/TranslationMemorySourceLaw.cs#L15-L40) для этих русских запросов возвращает Unknown, который фильтр не исключает. Это source review, не native proof успешного ответа. Адресный A/B выше уточнён, чтобы одновременная перестройка WordMappings не подменяла проверку KNN.

## Что не считать новой находкой

Не повторять как NEW: shared direction-free KNN memory; parser enum-order src/ctx; context ignored in verifier; noun augmentation `пила→пил`; phrase/word precedence и omission; POS memory not connected; context k=1; signature correction теряет RuleTags; forced-RU verifier measurement; train-ready self labels; HNSW pruning/RNG/admission; maxLen64; live encoder/index drift; full/prefix alias. Они уже есть в прежних документах. Сравнение subtree также показало отсутствие новой delta в `29.SemanticCompiler`, `05.Embeddings`, `06.VectorSearch` и в первых 300 строках KNN `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`; прочитанные подозрения в этих участках не объявляются отдельной регрессией.
