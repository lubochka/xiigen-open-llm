# kNN, юниты и предметные границы: независимая проверка

**20 октября 2026. Только документация.** Источники core закреплены на `0b76ab950d82534f5904005dda096545505ae307`, planning — на `4b950599a04388941f56b41e634233ebb7dc17c2`. Код, защищённые карточки, старые результаты и production не изменялись. Обучение, inference, build и тесты в этом аудите не запускались.

## 1. Подтверждённые результаты уже есть; старый статус BLOCKED недостаточен

### BPE-NO-MATCH-COPY-01: успешная экономия выделений внутри настоящего обучения tokenizer

[Свежий отчёт](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/report.md) и четыре исходных JSON [A/code-doc](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/code-doc.json), [B/code-doc](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/code-doc.json), [A/en-ru](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/en-ru.json), [B/en-ru](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/en-ru.json) сверены независимо.

Меняли только `BpeTokenizer.MergeChunk`: исходный вариант создаёт новый список и когда нужной пары нет; экспериментальный откладывает выделение до первого реального merge и возвращает прежний список при отсутствии замены. Вызывался настоящий `BpeTokenizer.Train`. Каждый профиль содержит 5 строк, предъявленных 8 раз; vocabulary 256 → 320, 64 выученных merge. Это обучение tokenizer; neural optimizer steps равны нулю.

| Профиль материала | Allocated bytes A | Allocated bytes B | B−A | Состояние после Train |
|---|---:|---:|---:|---|
| C# документация | 3 904 664 | 1 126 744 | −2 777 920, около −71,1% | Один и тот же конечный state hash у A/B |
| Перевод, EN→RU | 2 892 824 | 893 656 | −1 999 168, около −69,1% | Один и тот же конечный state hash у A/B |

На контрольных строках A/B дают одинаковые token IDs и decoded text; decode exact 4/4 у обоих. Число tokens изменяется от обучения и одинаково между плечами: code-doc 63/75/17/17 → 36/36/5/10; en-ru 60/71/30/31 → 23/25/17/22. Это подтверждает одинаковый выученный tokenizer на данной панели. Семантическое качество документации или перевода не измерено.

**Польза признана по allocated bytes**, рост accuracy для этого не нужен. Однако это не peak RAM и не retained heap. Процессные peaks содержат bootstrap и exam; их нельзя переименовывать в память Train. CPU code-doc ухудшился с 31,25 до 46,875 мс; en-ru остался 46,875 мс. Wall-time A→B 54,3095→34,3645 мс и 44,9896→38,1428 мс — одиночные наблюдения фиксированного порядка, без установленного причинного ускорения. Не назначать новый B→A ради переоформления уже законченного результата.

**Связь с обычным обучением юнита существует:** [KnnLogicalUnit.Train, 136–153](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136-L153) обучает BPE на textSelector всего corpus, затем строит vectors/index. Сам BPE опыт этот downstream юнит не обучал и его ответы не проверял. Поэтому кандидат переносится на точный `01.Tokenizer/BpeTokenizer.cs::MergeChunk`, а эффект полного юнита остаётся не измеренным.

Историческая Windows path failure из пользовательского протокола остаётся важным правилом коротких путей и сохранения partial results. Она не отменяет существующий завершённый результат более позднего запуска. Составление нового отчёта не является основанием повторить этот опыт.

### N-KU-01: полезный выигрыш и потерянный контроль должны идти рядом

Сверены [report.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md), [arm-A.json](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json) и [arm-B.json](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json). Один фактор — добавление `DoStatement` в `ActionLabelGenerator.Label.hasLoop`; 30 признаков и k=3 одинаковы.

| Показатель | A до | A после | B до | B после |
|---|---:|---:|---:|---:|
| Верный совет по назначенной панели | 0/4 | 1/4 | 0/4 | 3/4 |
| Уже имеющийся цикл: нет лишнего ADD_LOOP | 0/3 | 0/3 | 0/3 | 3/3 |
| Цикл действительно нужен: ADD_LOOP | 0/1 | 1/1 | 0/1 | 0/1 |

До Train портированный guard возвращает отсутствие совета. A сохраняет labels LOOP/LOOP/METHOD/LOOP; B — METHOD/METHOD/METHOD/LOOP. После Train A отвечает ADD_LOOP на всё, B — ADD_METHOD на всё. При четырёх примерах и k=3 большинство почти полностью определяет результат. **Это успех по устранению ложного совета о лишнем цикле и по общей панели, одновременно регрессия на необходимом цикле.** «Не предложен лишний цикл» не доказывает, что ADD_METHOD — содержательно хороший совет, или что модель приобрела общее понимание циклов.

Причинный путь подтверждён: [Facade.TrainFileActionKnn, 194–203](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/SemanticCompilerFacade.cs#L194-L203) → [FeatureVector/Label](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/FileActionKnn.cs#L22-L107) → [native Train/Predict, 122–141](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/FileActionKnn.cs#L122-L141). В опыте использовались source copies, не полный facade. [UseFileActionKnn по умолчанию false](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/SemanticCompilerOptions.cs#L20-L21); обычный reader требует включения, C# и минимум 4 строки. Нельзя объявлять этот локальный успех доказанной причиной всего RP01.

Отсутствие DoStatement в FeatureVector и ForEachVariableStatement уже записано в свежем отчёте. Они не выдаются за новые находки этого аудита. Дополнительный исторический B→A проход отражён отдельно; он не даёт разрешения выполнять новые повторы сейчас.

### NEURAL-FROZEN-TRANSPOSE-01: полезный результат другой оси

[Отчёт](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/report.md) прочитан; независимую проверку neural code/raw выполняет основная часть аудита. В отчёте: 4 optimizer steps, allocated bytes 13 647 840 → 12 140 448, дополнительный retained cache 249 856 bytes, одинаковые learned state/loss/logits. Это разные показатели памяти, их нельзя складывать в «peak RAM уменьшился». Accuracy 0/2 не отменяет ресурсный успех; одинаковая accuracy 0/2 также не доказывает освоение задачи. Отдельный 400-step probe не подменяет результат именно этих четырёх шагов.

## 2. kNN здесь обозначает несколько разных учеников

| Получатель | Что действительно записывает обучение | Что читает результат | Где применим вывод |
|---|---|---|---|
| FileActionKnnPredictor | Пары 30 AST frequencies + deterministic action label | Cosine k3 majority; facade opt-in | C# edit-action supervision; не перевод |
| MethodKnnPredictor | Соседние Current/Next PackedMethod одного файла | Jaccard Current.KindBag, затем CanonicalType Next.KindBag | C# соседние методы; scope и порядок файлов существенны |
| StructuralKnnPredictor | StructuralVector + next-token ID | Weighted structural distance и голосование | C# AST при одном codec; не универсальная семантика предложения |
| Generic KnnLogicalUnit | Собственный BPE, HNSW vectors, ID→typed example | Ближайший hit, его Output и metadata | Разные предметы только при соответствующих TRAIN/EXAM; сохранность IDs/aliases общая |
| TransformerEmbeddingKnnLogicalUnit | Vectors от конкретного encoder/tokenizer + examples | Fusion embedding/BM25/pattern, затем stored Output | Зависит от learned encoder, text distribution и сохранности версии |
| Translation ExecutorUnit | Exact, phrase, word maps и shared TranslationKnnStore | Exact → phrase → word → kNN → fallback | Перевод с проверенным направлением, ролями, отрицанием и словарём |

Основания: [FileActionKnn.cs](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/FileActionKnn.cs), [MethodKnn.cs](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/MethodKnn.cs#L119-L189), [StructuralKnn.cs](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/StructuralKnn.cs#L12-L180), [generic unit](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L102-L299), [transformer unit](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L98-L252), [translation writer/reader](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/27.TranslationDomain/TranslationTrainedUnits.cs#L240-L455).

Следствие для плана: одно и то же слово «kNN», «обучение» или «Unit» не доказывает общий call path. Изменение порядка предложения полезно там, где задача зависит от порядка. Parent/scope/sibling AST features проверяются для конкретного parser/schema. Нельзя переносить C# Roslyn feature на обычный перевод без назначенного соответствующего представления.

## 3. Проверка семейств карточек: source → состояние → чтение

Это семантическая проверка механизмов и доступных исходников, не утверждение, что все справочные P-карточки уже являются готовыми runnable паспортами.

| Карточка | Связь с обучением | Вывод проверки / конкретное ограничение |
|---|---|---|
| P16 | Generic Train vectorizes TRAIN; Execute vectorizes query и читает learned output | Order/roles/scope допустимы. P16.5 честно использует отдельный exact index; его результат не равен HNSW product gain. Gold role feature проверяет expressiveness, не извлечение смысла из raw text. |
| P18 | ID map, tokenizer correction, encoder/index version и hash участвуют в writer/reader | Допустимы как сохранность/чтение learned state. Для shape mismatch нужен реальный owner и caller; абстрактная арифметика сама не даёт такого результата. Generic duplicates видны в Train151–153, уже известны. |
| P19 | Train хранит shared record/Output, Execute выдаёт Output/metadata | Мутация действительно меняет результат выученного урока без нового Train. Для mutable TOutput нужен mutable schema; immutable string не trigger. Отдельно admission copy и outgoing copy. |
| P26 | Structural features строят vectors, записываемые в native kNN | SiblingCount сохраняется Extract57/90, но DistanceTo27–35 его не читает. Это известная P26.4. Parent/scope уже участвуют в production DistanceTo; нельзя объявлять их универсально отсутствующими по старому E07. |
| P27 | Codec создаёт learned next labels и query coordinates | NodeTypeId99–102 делает SyntaxKind%200; P27.6 уже покрывает collision-free кандидат. P27.5 исправляет сопоставимость оценки learned outputs, не создаёт новый Train; нужны именно сравниваемые raw predictions. P27.7 условна до контракта source ownership. |
| P51 | Learned translation keys, tokenizer corrections, restore и lookup | Reader-only разрешение уже исправлено. Для языкового namespace нужен actual caller и same-language controls; направление не равно TopicDomain. |
| P52 | Builder создаёт учебные примеры tokenizer/executor/verifier | Подготовка входит в обучение. Chosen-only Executor не получает прямой neural эффект от исправленного rejected; нужен negative-consuming learner. P52.8/10 уже правильно ограничивают этот вывод. |
| P53 | TranslationMachine.TrainStructuredBatchAsync107–121 вызывает TrainOnExample перед identity ledger | Atomicity/idempotence могут менять learned memory. RowsTrained отдельно и не доказывает actual calls/state. Conditional transaction implementation не объявлять существующей. |
| P54 | Learned maps → Execute → MeasureAccuracy → batch feedback | Семь внутренних OUT_OF_LEARNING_SCOPE запрещают reader вопреки связи. Адресное исправление и требования к snapshot — [06](06-P54-learned-reader-addendum.md). |
| P55 | Word-map keys, gender alias и SourceLaw влияют на сохранение/использование урока | Применимость по направлению. DeriveGenderAlternateForm774–793 — строковое правило -л/-ла; качество POS/role supplier не доказано. Не смешивать сохранение role с использованием в lookup. |
| N-KU-01 | Реальные labels → Train → Predict | Завершённый полезный результат с регрессией контроля. Не запускать заново. |
| METHOD-KNN-TARGET-SCOPE-01 | Target scope готовится из TRAIN AST и сохраняется в Next projection | Карточка не пытается восстановить AST из потерявшего scope KindBag. k1 вместо facade k5 и условный outer-body contract указаны. Это пункт 31, не новая гипотеза этого аудита. |
| KH-COST-01 | Повторный parse в FeatureVector/Label → FileActionKnnPredictor.Train | Реальная подготовка обучения; ресурсный эффект допустим. Ошибочный learner в обязательной вступительной строке — ниже. |
| KH-EXT-01 | Facade admission → destructive MethodKnn.Train → _pairs → Predict | Retention-проверка допустима. A допускает 4 метода, learner пишет только соседние пары одного файла; B считает eligible pairs до Clear. Отказ сохраняет старое знание, но не учит новый материал. |

Источники карточек: [P16](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/P16-semantic-order-roles.md#L129-L162), [P18](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/P18-state-identity.md#L129-L162), [P19](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/P19-mutable-output.md#L129-L157), [P26](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/P26-structural-features.md#L131-L164), [P27](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/P27-structural-codec.md#L129-L172), [P51](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/P51-translation-context.md#L129-L214), [P52](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/P52-semantic-annotation.md#L129-L187), [P53](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/P53-batch-atomicity.md#L129-L172), [P55](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/P55-translation-laws.md#L129-L167), [METHOD](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/METHOD-KNN-TARGET-SCOPE-01.md#L15-L43), [KH-EXT](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/KH-EXT-01.md#L36-L85).

### Конкретный конфликт KH-COST-01

В [KH-COST-01:7](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/KH-COST-01.md#L7) обязательный блок требует ответов после `MethodKnn.Train`. Однако [сам рецепт:34–40](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/KH-COST-01.md#L34-L40), literal данные и source-copy относятся к `FileActionKnnPredictor`. Это разные памяти, labels и readers.

**Точная поправка для исполнителя KH-COST:** «Показать native советы четырёх запросов до/после `FileActionKnnPredictor.Train` из `12.SemanticCompiler/Prediction/FileActionKnn.cs:122–141`. Подготовка — `SemanticCompilerFacade.TrainFileActionKnn:194–203`; reader guard — `PredictFileAction:327–333`. Ссылка на MethodKnn в обязательном вступлении ошибочна; MethodKnn, его категории и проект в этот опыт не включать». Остальной фактор двух parse против одного сохраняется. Не переделывать KH-COST в иной эксперимент и не повторять N-KU.

Дополнительно внутри [KH-COST:9](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/KH-COST-01.md#L9) сохранился номер 23 → 24, а внутри [KH-EXT:11](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/KH-EXT-01.md#L11) — 24 → 25. Закреплённый единый индекс назначает KH-COST 22 → 23, KH-EXT 23 → 24. Для исполнения читать номера из реестра, а не из этих устаревших строк; ID опыта не меняется.

Адресная проверка всех 28 файлов малых карточек, на которые ссылаются 32 строки защищённого индекса, выявила третий конфликт: [GRADB-LOCALITY-01:9](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/GRADB-LOCALITY-01.md#L9) пишет № 25 и «Конец назначенной очереди». Индекс назначает ему № 24 → № 25 RMS-COORD-ALLOC-01. По этой старой строке нельзя останавливать программу и пропускать последующие опыты. Других конфликтов явно записанных внутренних номеров этим поиском не найдено; часть карточек просто отсылает к индексу.

## 4. Проверенные потери и ограничения не объявляются новыми повторно

- Generic Train записывает dictionary по trimmed ID и независимо добавляет vector в index ([151–153](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L150-L153)); P18.2 и прежние исследования уже покрывают duplicate identity.
- Generic metadata принимает только `string[]` ([217–219](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L216-L219)); typed collection mismatch уже известен.
- Generic retrieval запрашивает k hits, но output берёт у top1 ([223–249](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L221-L249)). Это отличается от FileAction majority; перенос «исправления kNN» между ними без отдельного контракта некорректен.
- [BuildKnnTrainingCorpus3467–3507](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L3467-L3507) создаёт по одному примеру из top-level replay manifest и добавляет branch plans; nested KnnExamples не разворачиваются здесь. Этот seam уже P24.7, не новая потеря.
- `LogicalCapabilityTrainingPreferenceProjector` и grouping subphases просмотрены как контекст DPO→units. Generic «Generated capability...» target сам по себе не доказывает ошибку: нужен контракт recipient и ordinary output. Отдельная новая доказанная потеря DPO→unit этим просмотром не заявляется.
- Проверка `if (n is > 0)` подготовлена как [NG01](cards/NG01-null-guard-label.md), **подслучай известного N48-01**, а не новая root-cause family. Native результата пока нет.

## 5. Почему NG01 имеет самостоятельный маленький опыт

В [ActionLabelGenerator.Label32–38](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/FileActionKnn.cs#L32-L38) любой IsPatternExpression внутри if становится признаком защиты от null для любого member access метода. Для заранее nullable `x:string?` выражение `if(n is >0) return x.Length`, где `n:int`, не защищает x. Такой label попадает в настоящую обучаемую таблицу через source→label→Train.

[N48-01:17–95](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md#L17-L95) уже описывает общий false suppression и nested guard, включая отсутствие receiver/scope/dominance. NG01 изолирует ложное evidence от **не-nullable int relational pattern**, сохраняя null-literal, type-pattern и nullable-int controls. Простое удаление всех relational patterns отвергнуто на стадии проектирования: `if(x is >0)` при `x:int?` может законно защищать `x.Value`.

Общий IsPattern тезис также уже имеется в [core65:45](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md#L45) и классифицирован как повтор в [N53 dedup:109](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/03-MECHANISM-COVERAGE-AND-DEDUP.md#L109). Эти первоисточники учтены; новизна ограничена точным маленьким контрастом и рецептом, не семейством дефекта.

Предлагаемый фактор: в копии исключить из hasNullCondition только direct relational pattern на параметре явно типа `int`. Остальной Label, 30 features, k3, память и голосование одинаковы. Это кандидат фильтра учебной метки; не полный null analyzer и не автоматическое production исправление. Результаты **NOT_RUN**.

## 6. Scope/read ledger

| Источник | Что фактически прочитано | Граница |
|---|---|---|
| FileActionKnn.cs, blob ef7a88a3c4c2540569b9ce6f204d8e4750875501 | Полностью, 1–165 | Teacher/state/reader N-KU и NG01 |
| MethodKnn.cs, blob e0ecebb6d513259a0930e8706737254f926197cd | Полностью, 1–189 | METHOD и KH-EXT |
| StructuralKnn.cs, blob b3aa5d376be3f93423997674dc015cea7e9fbf58 | Vector/extractor, train/reader, role mapping | E07/E09 драйвер целиком не проверялся |
| SemanticCompilerFacade.cs, blob f98fc1cc63a01022658e1daff4ac071e3766c239 | Конструктор, Train methods/file/structural, readers | Full Compile и downstream caller не выполнялись |
| SemanticCompilerOptions.cs, blob 9329627b2b25a24c7e3280b2a4162e339338e8c8 | Полностью | FileAction default false учтён |
| KnnLogicalUnit.cs, blob 2957816a88aa9cdcd3020bb81be56b5dfff165e0 | Полностью | Native call не выполнялся |
| LogicalCapabilityTrainingPreferenceProjector.cs, blob c8f54b5835488db07ba3f0836388afa40a58cf8b | Полностью | Новая причинная потеря не объявлена |
| BoundedDpoTrainingContentProjector.cs, blob 31c245b1c129651e4410b2bb1647b42aadba2c95 | Полностью | Bounded digest/schema контекст |
| DefaultLogicalCapabilityTrainer.cs, blob e16657e91d4b0f201aef003f473cd8811bf87893 | Участки 360–750, 900–1040, 3400–3545, 3930–4200 | Большой файл не прочитан целиком |
| LlmIngredientLogicalUnits.cs, blob 2f91fce55c711063aa41780a53ef62b08f337dc8 | Retrieval unit1–320; адресный поиск DPO | Neural часть покрывается отдельно |
| TranslationTrainedUnits.cs, blob 1742abfc9fb617dfc66e4d618e0c169da01a6ccc | Maps, Train, Execute, MeasureAccuracy, checkpoint, composition | Не весь tokenizer/verifier |
| TranslationMachine.cs, blob fd0e736de3739236b4dde2d6b9d000f5ebae8416 | Dispatch, held-out reader, restore, runtime binding | Полный validation contract не ревьюился |
| BPE/N-KU/NEURAL-FROZEN reports | Все три; BPE и N-KU также raw arms | Ни одного повторного запуска |
| P16/18/19/26/27/51–55 | Разделы механизма и всех подслучаев, адресные D-профили/промпты | Не полное чтение каждого повторяющегося профиля |
| Текущие skills | architect-behavior-classifier — адресные разделы; plan-review1–120; output-readability1–120; prompt-patch-authoring1–100; human glossary полностью | Огромный architect skill не прочитан целиком |
| Архивы навыков | Архитектура, планирование, review, code examination, promptops, data integrity, claims, human communication | Старые правила ниже текущего поручения |

Навыки применены для определения recipient/state/consumer, одного фактора, доказательств и ясного русского отчёта. Старые требования accuracy-only success, обязательного merge, другой модели или полной production цепочки не применялись.

## 7. Порядок и рабочие ограничения

Исходная очередь закреплена в [защищённом execution index](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md). № 01–03 завершены; следующий № 04 N62-R1. В исходнике METHOD — № 31, KN consumer — № 32. Для будущего исполнения по этому пакету порядок задаёт [дополнительный реестр 08](08-execution-registry.md): первые 31 позиции сохраняются, NG01 становится № 32, прежний final KN reader — № 33. Защищённый индекс не изменён; прямое назначение ID сильнее номера. P54 остаётся после прямых учебных опытов и не повторяет завершённые результаты.

Для любой карточки и промпта этого пакета: модель текущей сессии по умолчанию; полезный измеренный выигрыш по любой метрике/домену — успех, все ухудшения рядом; экспериментальные throw/try-catch/void и минимальный host допустимы; только русский отчёт и кандидат переноса, без автоматической правки продукта.

**STOP/NEXT:** после прямого «заканчивай и переходи» немедленно перейти к уже назначенному следующему заданию в разрешённой фазе; не возвращаться к report, hashes, review, retry или проверкам прошлого опыта. «Остановить всё» прекращает всю работу, кроме необходимой остановки собственного живого процесса. Старые и partial результаты сохраняются. Runtime текущей документной сессии этим не разрешается.

