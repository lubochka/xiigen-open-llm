# R04 — что именно представлено векторами и что доходит до потребителя

**13.10.2026 · R04-REPAIR-1 · предметный результат исследования с адресными пробелами.** R04 не запускал обучение, продуктовые тесты, build или GitHub Actions. Улучшение качества, скорость обучения и пригодность для ПК: **NOT_MEASURED**. Отдельная Astra/CODEX экспертиза: **NOT_PERFORMED**. Авторская документная проверка не названа независимой. Старые F01–F10 и source IDs сохранены; новое чтение/repair отделены ниже.

## Результат для проекта

Первый полезный учебный опыт не требует нового токенизатора, нового kNN или нового DPO-тренера с нуля. Найдены разные, уже существующие представления и настоящий типизированный kNN-потребитель. Их нельзя считать одним общим векторным пространством. **Опыт E01 со сменой представления и опыт E02 с одной новой DPO-парой независимы: E02 не ждёт E01.** Прежнее «сначала изменить encoder, затем подключить урок» было преждевременным при неизвестном ordinary consumer пилота и больше не является порядком исполнения.

Три исходных существенных наблюдения сохраняются: нормализация некоторых каналов отбрасывает важные различия; один 60-мерный TeachingVector строится с локальным словарём конкретного входа; другой, самостоятельный kNN-путь сохраняет обученный BPE и возвращает реальный TOutput. Последний факт запрещает вывод «все векторы несовместимы» и позволяет переиспользовать реализацию.

**Новое основание для ближайшего seam:** R05 CP2 сообщил потерю точного оценённого материала перед optimizer; BuildBatch/Compact непосредственно перечитан при этом review. Raw паспорт сохраняется, но prompt/chosen/rejected проходят whitespace collapse и обрезание каждого до 480 символов перед Encode. Смысловое различие сторон может исчезнуть. Это конкретнее, чем заранее назначать новый encoder первым patch. R05/R12/R18 получают existing exact-pair→tokens→response-objective путь; ordinary consumer по-прежнему требует доказательства, а не предположения.

Пять носителей результата: [источники](SOURCES.csv), этот разбор, [решения и подробные prompts](DECISIONS.md), [передача требований](CHANGE-MAP.csv), [журнал](RUN-LOG.md). Первоначальный research менял только собственные results. **Текущее отдельное поручение review/repair** также исправило R04 prompt и START dispatcher; это не разрешение research worker менять shared files. Core, слайсы исполнения, результаты и prompts R05/R06 не переписаны.

### Снимки и смысл ссылок

P0 = `lubochka/xiigen-planning@de8da48af66094fcdcd803bd5ee97d780c8901ba`.
K0 = `lubochka/llm_mvp_core@dcdded6bb6039c03e80a2786312435350894a4ee`.
P1 = `lubochka/xiigen-planning@76e6f4083cf8327c2db3b173dd62e26cbd5cc8aa` — ранее адресно принята дельта CP6 и прочитан R05 CP1; первоначальное чтение не обнулено.
P2 = `lubochka/xiigen-planning@d02acc4ba83e4c903ea9f23374e891c4b863f70a` — при review принят R05 CP2 и поздний R06 receipt. Это не утверждение, что P2 навсегда HEAD.
D = `llm mvp/design to code session`. Старые локальные source IDs разрешаются через SOURCES.csv; новые repair receipts — также в source appendix RUN-LOG и точных locators ниже. Полное содержательное чтение больших архивов, всех RAG и всех слайсов не заявляется.

## 1. Сохранённые входы и границы S00–S03

| Основание | Что R04 действительно принял | Что не объявлено завершённым |
|---|---|---|
| Условная исходная очередь S00–S10, P14 | S00: свежие источники; S01: продолжение оценки без повторного урока; S02: полное состояние; S03: одинаковая независимая оценка и отдельная certification | Наличие строки в очереди не выполнение слайса. Полный аудит recovery/state остаётся у R05 |
| WorkLeft code/SOLUTIONS/VERIFY/branch, P15–P19 | Уже существующие механизмы переиспользуются; hash, хранение, количество тестов и память маршрута не заменяют качество; диагностика, раскрытая учителю, не является закрытой certification | Сентябрьские снимки и локальные worktree не считаются текущим runtime. Старые команды cleanup/merge не выполнялись |
| R01 F12 и первичный evolution plan, P07/K04 | LearningMachine владеет обучением; Unit/Flow/Orchestrator — runtime-роли; новый внешний слой не требует дублировать интерфейсы | Сохранён отдельный статус R01; подчинение каждой реализации Machine ещё не доказано |
| R02 и CP6, P08/P24 | CODEX Astra 6 ultracode выбирает урок и отдельно оценивает точную пару; внешняя открытая китайская модель пишет ОБЕ окончательные стороны | Старое ограничение final rejected=raw Student не переносится; роли не считаются фактически вызванными |
| R03 CP08, P09/P10 | Raw-word и TaskSignal — разные пространства; semantic order нельзя превращать в set; LoRA не tokenizer; N−1 вариантов одной задачи — одна family | P3-B/P3-C большого IDE-плана приняты как точный sibling receipt, не как новое самостоятельное чтение всего первоисточника |
| R05 CP1 на P1, P25 | Есть отдельная трасса существующего DpoTrainer; TeachingVector в StepVector описан как масштаб сигнала, а не целевой выходной вектор | Это прочитанный результат R05, не повторная проверка его кода в R04 и не подтверждение полного update→consumer |
| R05 CP2 на P2 | Existing coordinator/trainer route; exact-pair/token loss; response-objective и active-count weighting уточнения; S06 multi-lesson возможности | Compact перечитан напрямую, остальные CP2 выводы остаются атрибутированными sibling evidence; R05 OPEN не повышен |
| R06 checkpoint B / RUN-LOG на P2 | Различение сохранённого kNN пространства и TeachingVector принято соседом; его R05 receipt ещё CP1 | Новая исходящая CP2 дельта R04 не считается уже принятой R06; результат соседа не переписан |

WorkLeft сохраняет семь базовых предметов: EN↔RU, orchestration, Figma→HTML/CSS, C#, Angular, SQL documentation, Builder для этих шести. HE↔EN и остальные дополнительные требования не удалены. DPO/DynamicGraph, Design-to-Code и IDE/клиенты остаются тремя линиями программы. Первый узкий опыт не обязан сначала реализовать весь DTC или UI.

## 2. Прослеженная цепочка обычного входа

### R04-F01 — исходный текст, canonical text и TeachingVector не одно и то же

**Наблюдение кода, K05–K09.** `PromptRoutingInputCompilerUnit.ExecuteAsync` получает `LogicalOrchestratorRequest.UserRequest`, вызывает `SemanticTeachingPromptCompiler.Compile`, тот вызывает `DevelopmentPromptCompiler.Compile`. Последний использует `SemanticCompilerPipeline.Compile` и формирует общую compilation и сегменты.

В pipeline `StackDetector.Detect` получает исходный текст ДО `SemanticNormalizer.Normalize`. Это полезный существующий порядок, не новое предложение R04. Самостоятельные тела StackDetector/AstTokenizer в первоначальном проходе не разбирались; прочитан их реальный вызов из pipeline.

`DevelopmentPromptCompilation.Text` и `SemanticTeachingPromptRepresentation.RawText` сохраняют исходный текст. Но `BuildCanonicalPromptText` строит отдельный канал: development predicates + normalized text + pipeline predicates + explicit subphase hints + semantic_domain + semantic_stack. Это не полная копия raw.

Обычный routing compiler передаёт именно этот канал в **`SubPhaseRouterSourceDocument.CompiledText`**. Для нескольких сегментов он дополнительно передаёт raw `segment.Text`. Поэтому утверждение «исходный запрос полностью потерян во всей системе» неверно. Правильный вопрос: какой конкретно consumer читает raw, canonical или vector и какие различия ему ещё доступны.

**Потребитель, K10:** `SubPhaseRouterUnit.ExecuteAsync` читает CompiledText/predicates, строит path-free scoring text, сравнивает с реестром subphase и выбирает результат. В прочитанном коде роль явно `BaselineCandidateGeneratorRole`, не learned edge authority. Это существующий контрольный вариант, а не доказательство обученного выбора.

### R04-F02 — потеря смысла в нормализованном канале является конкретной гипотезой риска

**Наблюдение кода, K06.** `SemanticNormalizer.Normalize` использует разбиение `[^a-z]+`, исключает короткие фрагменты и stopwords, среди которых `not`, `no`, `if`, `then`, `else`, `and`, `or`, `must`, `may`. Числа, операторы и не-латинские символы таким каналом не сохраняются как исходные значения.

**Вывод, не runtime-измерение:** пары с отрицанием, другим количеством, областью действия или языком могут стать неразличимыми на этой стадии. Raw/другие признаки способны сохранить часть различий; наличие потерь в одном transform не доказывает конечную ошибку всех маршрутов.

Различающий пример: «do not delete 10 rows» против «delete 100 rows». Рубрика должна отдельно спрашивать, дошли ли отрицание, действие и количество до нужного consumer. Нельзя засчитать одинаковую длину вектора как правильное понимание. Это разные diagnostic inputs, не chosen/rejected одного frozen prompt. Исходные примеры здесь — иллюстрации экзамена, не допущенный DPO-набор.

### R04-F03 — 60-мерный TeachingVector требует проверки пространства координат, но не является всем kNN

**Наблюдение кода, K08/K09.** `TrainingVectorBuilder.Build(compiled)` строит фиксированные predicate/domain tiers и до 32 root-координат. Root vocabulary создаётся заново для данной compilation; итог возвращается как float[], без самого отображения root→coordinate. В перегрузке `Build(string)` словарь также локален входу. Есть уже существующая перегрузка `Build(prompt, sharedRootVocab)` — сначала исследовать её использование, а не проектировать ещё один общий словарь.

В vectorText этой перегрузки есть semantic_domain, но нет явного semantic_stack, который имеется в canonical text. Пять domain-слотов CODE_Q/BUG/PLAN/ARCH/REFACTOR нельзя без объяснения считать полным предметным каталогом SQL/C#/перевод/дизайн.

**Граница:** вызов Build(compiled) из SemanticTeachingPromptCompiler доказан. Его TeachingVector не используется напрямую прочитанным routing compiler: тот берёт Compilation. Нельзя выдать этот float[] за вход HNSW или выход LoRA без следующего вызова.

**Уточнённый receipt R05-F03/F12@CP2:** StepVector использует TeachingVector для масштаба пары; CP2 сообщает active-coordinate-count heuristic с clamp, а не target semantic encoder. Поэтому перестановку ненулевых координат нельзя автоматически назвать предметно значимым вмешательством в этом consumer. Его полное исследование не повторено R04. Возможное применение координат другим consumer остаётся отдельным адресным вопросом; ни универсальное vector failure, ни learned tokenization не доказаны.

### R04-F04 — существующий kNN использует другой encoder и настоящий TOutput

**Наблюдение кода, K23, `KnnLogicalUnit<TInput,TOutput>`; повторно прочитаны Train/ExecuteAsync/Vectorize на blob `2957816a88aa9cdcd3020bb81be56b5dfff165e0`, 1–320:**

1. Train создаёт BpeTokenizer на тексте примеров, HnswIndex и отображение example ID→typed example; сохраняет tokenizer, index, textSelector и параметры в экземпляре.
2. Vectorize делит пространство на token tier и predicate tier. Token IDs сворачиваются по модулю ширины, predicates — стабильным hash, затем L2 normalization.
3. ExecuteAsync вызывает тот же tokenizer/textSelector/Vectorize для запроса, берёт predicates из metadata, ищет соседей и возвращает **`bestExample.Output` типа TOutput** через LogicalUnitResult.
4. Поиск читает k соседей, но выход выбирается по **первому**. Это не взвешенное голосование k ответов и не генерация нового ответа. TOutput сам может быть составным, однако данная реализация копирует выбранный typed output. k/ANN могут влиять на поиск/evidence; это не доказанная learned adaptive-k функция.

Это положительный reuse-факт: общее пространство внутри сохранённого экземпляра есть; локальный словарь из F03 к нему нельзя приписывать. Одновременно bag/count hashing может терять порядок и давать коллизии. Насколько это вредит реальным задачам, не измерено.

Train принимает predicate IDs примеров, а inference получает их через `Metadata["predicate_ids"]`. Сопоставимость этих двух каналов — обязательное условие опыта. Отсутствие metadata нельзя молча компенсировать эталонной меткой.

`ManagedAllocationBytes` в manifest измеряется через GC current-thread allocation delta, а не peak RAM всего процесса. Его нельзя использовать как доказательство PC-fit. Confidence из ближайшего score не является измеренной вероятностью правильного ответа.

**Дополнение review:** low-confidence ветка добавляет warning и всё равно возвращает `Ok(result)` с выбранным output. Это не abstention. В данном ExecuteAsync не доказан предметный eligibility gate; нужно читать реальный внешний caller/pool/arbiter. Не переносить отсутствие gate в этом методе в утверждение о его отсутствии во всём приложении. Копировать допустимый category label может быть правильно; копировать request-specific source spans/IDs без rebinding — отдельный риск, который требует actual output type/consumer.

### R04-F05 — domain, intent, stack и роль нельзя сплющивать

**Наблюдение кода, K11/K12/K14 и план P05.** Есть внешние DomainTag (`whole_development_area` и scope unit), compiler Domain, StackLanguage, registry UnitRole и отдельные InputType/OutputType/Version. Это разные назначения полей. SQL+BUG может означать предмет SQL и intent исправления ошибки, а не противоречащие категории.

Project-23 `DomainClassifierUnit` имеет собственные predicate/kNN и fallback-ветки. Его BuildRequestFromCompilation берёт normalized text. Wrapper `CompiledDomainClassifierUnit` делегирует ему, не доказывая дополнительную обученную функцию.

Предлагаемый будущий domain descriptor обязан отдельно указывать subject domain(s), intent, source/target language, stack/dialect/version, schema, architecture role и applicable constraints. Это смысловой контракт, не требование немедленно добавить ещё один DTO во все проекты.

## 3. Many→many: что существует и где ещё нет полной трассы

### R04-F06 — контракт и материализация уже существуют

**Код, K01/K02/K13.** ILogicalUnit обобщён по входу/выходу; скалярная оболочка не запрещает collection types. `IDynamicGraphNode<TInput,TOutput>` возвращает `IQueryable<GraphNodeCandidate>`. Есть fan-in/fan-out/many-to-many/multi-output/return-loop виды связей.

`LogicalUnitDynamicGraphNode.ExecuteAsync` вызывает wrapped logical unit один раз, передаёт typed результат candidateProjection, затем материализует snapshot в массив и возвращает `AsQueryable()`. Порядок проекции сохраняется на этой границе. Это уже материализованный in-memory queryable, не доказанный потоковый/remote LINQ provider. Есть cancellation checks до/после асинхронного вызова и проекции.

**Не писать новый many→many API только из-за пользовательского alias.** Проверять нужно association входов/выходов, requested type, реальные данные, ownership/lifetime и смысл операций над результатами.

### R04-F07 — OutputRefs и hash кандидатов не доказывают правильный payload

**Код, K15/K16.** GraphNodeCandidate содержит OutputRefs, OutputType, PathOrderRef и provenance. Snapshot копирует эти ссылки. `DeterministicDynamicGraphNodeOutputProjector` проверяет непустой набор уникальных CandidateId, сортирует идентификаторы и вычисляет hash набора.

Этот digest не хеширует значения typed payload. Сортировка order-free множества candidate IDs здесь не равна разрешению сортировать semantic OrderedPatternIds из DPO-плана. Наличие OutputRefs также не доказывает, что ссылка разрешается в правильный объект требуемой версии.

**Точный открытый участок R04-G01:** projection callback→OutputRefs storage/resolution→requested typed payload→дальнейший flow consumer в ordinary graph invocation. Прочитанные LogicalRegistryDynamicGraphProjector и L3SelectedTargetExecutor относятся к topology/selected flow, но не доказывают этот data dereference. Отсутствие полной трассы здесь не означает отсутствие resolver во всём репозитории. R07 получает одну ограниченную подзадачу, не повтор всех Graph contracts.

### R04-F08 — два конкретных риска ассоциации/пустого результата

**Код и ограниченный вывод, K07/K13/K15.**

- Routing compiler соединяет `compiled.Segments.Zip(attributedResult.Output, ...)`. В прочитанном методе нет отдельного доказательства равной длины и идентичной ассоциации. Zip способен скрыть несовпадение; upstream contract может его запрещать, но должен быть найден. R07 проверяет именно этот producer/consumer, не все коллекции проекта.
- Graph node имеет domain-mismatch путь с пустым результатом; output projector требует непустой набор. Нужно различить законное 1→0, unknown-domain/abstention, cancellation и ошибку. Пустой массив не должен автоматически означать ни успех, ни поломку всей learning-системы.

Будущая examination: для двух входов a,b и трёх допустимых результатов r1,r2,r3 сохранить explicit associations r1→a, r2→a, r3→b. При перестановке delivery order смысловое соответствие не меняется. Потеря одного результата, неверный тип/версия или привязка к другому входу не компенсируются высоким embedding score. Данный пример не объявляет фактически выполненную runtime проверку.

## 4. Что реально считает evaluator

### R04-F09 — числовой gate не является предметным экзаменатором

**Код, K17, DynamicGraphAccuracyMeter.** Meter получает готовые LearningEvaluationResult before/after и split proof, проверяет их связность/диапазоны, вычисляет delta и gate. В прочитанной версии есть Q30-константы after=.95, delta=.02, margin=.025. R04 не меняет их и не объявляет подходящими универсально.

В этом классе нет самостоятельного чтения/понимания решения предметной задачи. Источник чисел, правильность labels, используемый consumer и admissible rubric должны быть прослежены отдельно. Его проверка provenance полезна, но не заменяет модель-экзаменатора.

### R04-F10 — найден предметный scorer, но его метрика поверхностная

**Код, K18, LanguageTranslateHeldOutScorer, строки 1–210.** Scorer действительно сравнивает predicted output с expected label, проверяет held-out origin, unique case IDs и ограничения работы. `ScoreOne` вычисляет `0.5 * exactMatch + 0.5 * characterAccuracy`; затем значения усредняются.

Он полезен как воспроизводимое дополнительное измерение поверхности. Но хорошая перефразировка и смысловой сбой в одном коротком слове требуют разной оценки, которую эта формула сама не обеспечивает. Пример: сохранено ли отрицание/количество/референт; допустимы ли несколько переводов. R04 не утверждает, что другие evaluators проекта отсутствуют.

Для S03 сохранить evaluator/configuration/case identity до/после/resume, но дополнить предметной рубрикой и отдельной подтверждающей выборкой. Раскрытая рабочая диагностика не называется untouched certification. Цель 95% должна иметь определённые task population, rubric и uncertainty; это не 95% cosine/confidence/character score по умолчанию.

## 5. Реальные RAG-переходы, а не только имена файлов

Первоначальный R04 прочитал WorkLeft RAG-04 (P20) как карту Трека C, затем вложенный `RAG-03-language-translate-learning.md` (P21), затем первичный `TRANSLATION-TOKENIZER-GUIDE-v1.md` (P22). Точные корни — `llm mvp/visual studio & visual studio code continue` и `llm mvp/language translate learning`, не D с повторно добавленным префиксом.

Первичный guide требует различать Latin/Cyrillic/Hebrew и отдельно описывает BPE vocabulary. Это подтверждает, что ASCII-only normalization не может автоматически быть общим multilingual tokenizer. Майские размеры vocabulary, корпуса, квоты и инструкции добавить tests не перенесены как текущие стартовые условия. Никаких корпусов не скачано и closed exam не раскрыт. R08 проверяет доступность/права/состав будущих данных.

R03-P12/P3-B/P3-C (raw word→TaskSignal) используется как **sibling-derived** источник решения; собственный новый полный проход по 1.56MB IDE-плану не заявлен. Это сохранённое обязательство с адресом, а не исчезнувшая часть архитектуры.

**Новая primary ветка при repair:** `llm mvp/visual studio & visual studio code continue/RAG-06-llm-dynamic-flow.md`, blob `c09b2db12d3d56c60fdf39e99f67eac65d05793a`, прочитаны 1–148 → `llm mvp/llm dynamic flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md`, blob `0408bddbcd595f2023dec5001b52bac68af7b8c5`, прочитаны 1–125. Domain-scoped pool предшествует ranking, positive/negative/warning memory различаются, final selected arbiters владеют acceptance. Близость, скорость, стоимость, приоритет или красивый output не отменяют rejection. Это восстановленный design contract, не новое наблюдение runtime исполнения; исторический threshold 0.50 не назначен всем доменам.

## 6. Смысл механизмов и допустимые вмешательства

| Механизм | Deterministic часть | Learned state/authority | Consumer и ограничение | Различающее вмешательство / что опровергает пользу |
|---|---|---|---|---|
| Компиляция raw→canonical | Parse/normalization/lexicon/stack order | В прочитанном pipeline обучаемые параметры не установлены | SubPhaseRouter; raw и canonical не равны | Замена только представления при frozen router; если downstream не читает изменённый канал — INVALID_EVIDENCE |
| TeachingVector | Фиксированные tiers и локальный/переданный словарь | Семантическая проекция этим builder не установлена; R05 CP2 сообщает active-count scaling | Compiler representation; DPO consumer по receipt R05 | Изменение координат не назначать до конкретного consumer; изменение scalar weighting не называть улучшением embeddings |
| KNN logical unit | BPE/count-hash/normalization/HNSW/top-1 | Tokenizer, examples и index — сохранённое обученное/производное состояние; final semantic authority отдельно | Настоящий bestExample.Output, low-confidence warning не abstention | Frozen examples/BPE/k/search + другой encoder; отсутствие lawful downstream gain или большая полная цена опровергает кандидат |
| Unit→Graph candidates | Typed call, snapshot/materialization, refs | Wrapped unit может быть learned, wrapper сам не доказывает learning | GraphNodeCandidate; resolver остаётся G01 | Один и тот же payload через прямой typed путь и graph seam; сравнить associations, версии, cardinality |
| Flow/Orchestrator/Graph | Совместимость портов, асинхронная дисциплина, допустимость | Выбор связей/маршрутов/композиций должен иметь свой owner | R07/R06, не считать DTO обучением | Fixed units + changed relation и обратное; одинаковые caps, цена всех веток; не отключать guards |
| LoRA | Линейные операции/shape/decoder contract | Конкретные A/B и freeze/masks по R05 | Target typed output/vector decoder ещё требует consumer trace | No-adapter/static/adapted при неизменном encoder; выигрыш только в loss/shape не достаточен |
| Оценка | Identity/split/арифметика/вспомогательные exact metrics | CODEX/Astra экспертный judgment не Student weights | Реальные решения по предметной рубрике | Замороженная рубрика и независимые families; смена критерия после ответа делает доказательство непригодным |
| Exact pair→tokens | Serialization/tokenization/masks/bounded input | Existing DPO policy/reference update, не новый learner | Raw passport и Compact-различие ниже; R05 objective owner | Сохраняется ли смысл оценённого ответа в фактически оптимизируемом response; raw hash не заменяет этот переход |

Роли не образуют обязательную линейную цепь для каждого запроса. Здесь не назначены новые trainer четырём интерфейсам и не переписано распределение Machine/Graph=L5. Для higher-order Graph будущий contrast должен различать primitive/same-order baseline и новые compositions; один общий end-to-end gain не доказывает вклад каждого блока.

## 7. Охват доменов и будущие наблюдения

| Предмет | Что сохраняет representation | Meaningful near-miss / допустимая эквивалентность | Что измерять и владелец |
|---|---|---|---|
| EN↔RU; дополнительно HE↔EN | Script, direction, negation, entities, quantities, referents | Правильный парафраз против похожего текста с инверсией смысла | Meaning rubric + вспомогательный surface score; R08/R13/R21 |
| Orchestration | TaskSignal, role, ports, ordered dependencies, applicable domain | Те же слова при другой зависимости; разные формулировки одной задачи | Mapping, lawful route completion, cost всех branches; условный representation pilot R13, не обязательный первый урок |
| Figma→HTML/CSS | Hierarchy, constraints, units, assets, responsive context | Похожий один screenshot при неверной структуре/другом разрешении | Constraint/visual/function quality + full conversion cost; R08/R21/R24 |
| C# documentation | Symbol/type/scope/nullability/version and requested purpose | Rename bound variable при сохранении binding; изменение binding как near-miss | Верность описания конкретного кода и applicable version; R08/R21 |
| Angular documentation | Component/template/data flow/lifecycle/version | Совпавшие имена при иной связи события и состояния | Source-grounded correctness и relation preservation; R08/R21 |
| SQL documentation | Schema/dialect, joins, NULL, order/cardinality as required | Похожий запрос, нарушающий заданное сохранение строк; допустимые rewrite только при том же контракте | Meaning of rows/constraints, не строковое совпадение; R08/R21 |
| Builder для шести | Same task/context, pair versions, lawful preference and recipient | Случайный плохой текст вместо сильного near-miss; TIE/BOTH_BAD вместо выдуманного winner | Pair validity отдельно от Student utility; R05/R08/R22 |

Это маршруты будущей предметной оценки, не закрытый новый список мировых доменов и не готовые корпуса. Для каждого конкретного закона R08 должен указать первоисточник, область применимости, версию и права; иллюстрация не становится законом только по уверенности автора.

## 8. Согласованный итог, который можно потребить сейчас

R13 получает в DECISIONS каркас E01 и отдельный E02, способы различить mapping/representation/solver и правила bulk/uncertainty/cost. **R18 больше не получает обязательное “сначала новый encoder”.** Он получает source-backed выбор ближайшего seam: R05 CP2 exact-pair→смыслосохраняющие tokens→response objective в existing owner; encoder insert/query остаётся условным E01-кандидатом после подтверждения его caller/consumer. Ни один из этих patch здесь не реализован и не разрешает сам по себе training.

R05 продолжает текущую CP2 трассу DPO/LoRA, не повторяет CP1. R06 получает различие vector paths, actual warning/abstention boundary, frozen-k baseline и source-backed memory/arbiter contract. R07 получает только G01 и Zip association; R08 — семь предметных паспортов. R10 интегрирует receipts шести workers без изменения чужих статусов. Исходящие новые receipts остаются PENDING_CONSUMER до фактического принятия адресатом.

Открыто: точный graph payload dereference, конфигурация выбранного ordinary consumer, actual checkpoint/hardware/budget и соответствующие semantic labels. Они блокируют зависимый эксперимент/готовность конкретного implementation packet, а не публикацию исследования и не независимую работу других раундов. **Ни улучшение обучения, ни отсутствие всей реализации, ни ненужность всех Windows проверок этим R04 не доказаны.** E02 не должен ждать unrelated graph/encoder/UI работ, но его действительные data/owner/consumer prerequisites не скрываются.

## 9. R04-F11 — оценённое различие может исчезнуть перед обучением

**Новый прямой code read при repair:** K0 `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`, blob `cb8744fcbbcff99821852e789dc92163976c0ad7`, 2250–EOF, `BuildBatch`, `Compact`, `ToMask`. Первичный caller найден через R05 CP2/SOURCES, а не повторную выдумку пути.

BuildBatch создаёт admitted passport по raw prompt/chosen/rejected. Затем Encode получает интерполяцию `Compact(prompt)` и `Compact(answer)`. Compact делит по space/tab/CR/LF, соединяет фрагменты одним пробелом и возвращает не более первых 480 символов каждой части. ToMask помечает непустые позиции полученной последовательности; отдельную response-objective оценку сохраняет R05.

**Контрпример из кода:** для одного frozen x у y+ и y− совпадают первые 480 символов непустого нормализованного префикса, а противоположное решающее действие расположено дальше. Обе финальные строки Compact совпадут, хотя raw стороны и их паспорта различаются. Другой риск — meaningful newline/indentation или пробел в literal. Это доказывает возможность semantic-loss transformation в прочитанном пути, а не частоту таких пар в реальных данных.

**Нужный контракт:** оценённый смысл должен дойти до действительной tokenized response, с известными tokenizer/version/budget/mask. Невместимый материал требует explicit bounded admission либо разрешённого re-authoring и exact re-assessment; молчаливое обрезание не считается той же оценённой парой. Генератор остаётся китайским автором обеих сторон, CODEX выбирает урок/оценивает пару; technical passport не подменяет semantic approval. Разные diagnostic inputs не DPO pair.

**Граница:** R05 CP2-F11 о mean prompt+answer objective и F12 об active-count signal приняты как sibling evidence, не собственная независимая проверка их математики в этом repair. Новый полнофункциональный learner не нужен только из-за этого seam. Пользу будущего исправления проверять по Student outcomes/retention/cost, не по изменению хеша, loss или числу строк.
