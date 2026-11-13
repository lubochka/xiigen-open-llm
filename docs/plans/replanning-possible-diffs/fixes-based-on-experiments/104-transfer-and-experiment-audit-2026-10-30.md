# 104. Аудит переноса и достоверности экспериментов — 30 октября 2026

<!-- EFFECTIVE-AUDIT-2026-11-01:START -->
## Действующая сверка — 1 ноября 2026

Core `790228c256168a336b01abae4387e28391048c0c`: **6 DONE / 2 PARTIAL / 1 OPEN / 1 CONDITIONAL** в очереди102. R2 принят; R6 не закрывает reference-only imports и legacy terminal без нового claim; R7 сохраняет constant-tie defect. Поэтому ни старый5/3/1/1, ни поздний8/0/1/1 не являются текущей приёмкой.

- [107 — доказательства, принятые переносы и точность P55–P81](107-transfer-and-experiment-audit-2026-11-01.md).
- [108 — конкретные owners, решения и закрывающие проверки](108-addressed-repair-plan-2026-11-01.md).
- [109 — адресные дополнения89 карточек и проверка сохранности](109-card-hardening-and-coverage-2026-11-01.md).

Все более ранние срезы ниже остаются историей на своих датах. Этот блок не назначает новый scientific NEXT, запуск или повтор обучения.
<!-- EFFECTIVE-AUDIT-2026-11-01:END -->

## Дополнение при публикации: новые коммиты после основного аудита

Документный пакет согласован с core `aab5e5143f2516b92b42ce82606759bb6b4ef0b2` и planning `9b58737da5183da37af5685e86f9e89d3eabe895`. Основной аудит ниже относится к `cbf3b1d...`; его исходные16наблюдений сохранены. За время подготовки появились три новых core commits, которые учтены отдельно:

- `2afbfd7d43c0c5048c0e1454ff26593cb7bdfd4f`: evaluators P30/P40 реализованы, сохранённые controls проходят. R7 теперь **PARTIAL**: новый независимый no-Train контрпример показал, что постоянная ничья `(BM25,KNN)=(1,1)` у обоих кандидатов ошибочно считается `INFORMATIVE_FACTOR_PANEL`. Отсутствие доказательства strict winner не доказывает смену победителя.
- `aab5e5143f2516b92b42ce82606759bb6b4ef0b2`: R8 **DONE в назначенном объёме** — P31 effective reader с hash-bound correction, явным выбором совместимой версии и отказом несовместимых поправок. Проверены source и пять сохранённых PASS controls; повторное исполнение самого R8 здесь не заявляется.
- `d190722004b0bc45e4ef343e3824d84e343c0374`: появился P53. Сохранённый report показывает частичную запись испорченной партии в native live state (+5записей), а экспериментальный clone-then-commit оставляет0; valid control даёт3/3 в обоих плечах. Это новый экспериментальный результат, не production fix. Указанный в report фактический расход14попыток обучения/24flow queries сохраняется, исходный бюджет9/18 не выдаётся за фактический.

**Текущий статус102 на публикационном срезе: 5 DONE / 3 PARTIAL / 1 OPEN / 1 CONDITIONAL.** Историческое4/2/3/1 ниже относится строго к исходному аудиту. R7tie-case и positive controls исполнены отдельно с `Train=0`; они не меняют исходный бюджет научных опытов. Доказательства и воспроизводители находятся в [папке аудита](../../../../experiments-v2/_audits/2026-10-30-transfer-execution-audit/README.md).

Источники: [AlphaPanelEvaluator](https://github.com/lubochka/llm_mvp_core/blob/aab5e5143f2516b92b42ce82606759bb6b4ef0b2/experiments-v2/RP01-MICRO/P3001/verdict/AlphaPanelEvaluator.cs#L154-L165), [P31 controls](https://github.com/lubochka/llm_mvp_core/blob/aab5e5143f2516b92b42ce82606759bb6b4ef0b2/experiments-v2/_runners/R102-R8-EFFECTIVE-RESULTS-RESOLVER-attempt-0001/P3101-acceptance-controls.json), [P53 report](https://github.com/lubochka/llm_mvp_core/blob/aab5e5143f2516b92b42ce82606759bb6b4ef0b2/experiments-v2/RP01-MICRO/P5301/results/attempt-0001/report.md).

## Решение по исходному срезу cbf3b1d

Переносы реально выполнялись; часть исправлений подтверждается поведением текущего кода. Однако наличие коммитов для 14 строк очередей 88/95 не означает завершения всей работы. В очереди102 на проверенном срезе **4 пункта завершены, 2 частичны, 3 открыты и 1 условный**. Дополнительно обнаружены повторения тех же ошибок в соседних владельцах, неполные сохранения состояния и нарушения одноразового исполнения в отдельных runner-скриптах.

Исправления надо продолжить по [адресному плану105](105-addressed-repair-plan-2026-10-30.md). Обновлённые требования находятся непосредственно в карточках P01–P64; [106](106-card-hardening-and-coverage-2026-10-30.md) объясняет покрытие и границы. Новый план ремонта не изменяет научный NEXT в execution index и не разрешает повторное обучение старых попыток.

### Проверенный срез и способ проверки

| Источник | Зафиксированное состояние |
|---|---|
| `llm_mvp_core`, `codex/dpo-implementation` | `cbf3b1d834cbf28baa2bb13baeb1bddc17e21019` |
| `xiigen-planning`, `llm_plan3` | `3b7537c267e45002ebab4f44845fe4ce92f204f9` |
| Дата аудита | 30.10.2026, UTC |
| Последний дополнительный коммит | P52 о пометке времени из rejected; уже включён, хотя отсутствовал в присланном списке |

Проверка сопоставляет исходную гипотезу possibleDifs, актуальную карточку, исполненный host и raw, native owner, обычный consumer и границы сохранённого состояния. Использованы проектные навыки архитектурного разбора, планирования и проверки плана, code examination/review, experiment design/measurement, prompt patch authoring и human-readable architecture audit. Их применение выражено в адресах владельцев, исполнимых контрпримерах и разделении наблюдения от причины.

Инвентаризированы 95 элементов fixes-based-on-experiments (93 плоских файла и два каталога), дерево possibleDifs и последние40 записей истории. Это **не** заявление о семантической проверке всех558 элементов possibleDifs или всех старых опытов. Углублённый разбор касается очередей88/95/102/103, перечисленных пользователем изменений, связанных current owners и экспериментов в таблице ниже.

Выполнены изолированные C#-проверки на исходниках этого SHA: .NET9, без сторонних пакетов, **16 подтверждённых наблюдений**. Из них10 подтверждают работающие инварианты, а6 воспроизводят дефекты. Это не «16 зелёных продуктовых тестов». Воспроизводитель содержит неизменённые native файлы; сокращён только файл общих контрактов до необходимых интерфейсов, что отмечено в manifest. Для публичного SemanticCompilerPipeline используются настоящие router, extractor и projector; подставлены лишь sinks хранения/журнала. В каждом процессе воспроизводителя выполнялся один TrainStep крошечной ProbeNN для численной проверки производной. Всего было три диагностических исполнения: первичные13 наблюдений, расширенные16 и заключительные16 на побайтно сверенных исходниках; суммарно3 ProbeNN updates. Это не повтор научного P48. Большие учебные программы и полный ProductHost/solution test suite не запускались.

Отдельно Python пересчитал сохранённые P25/P49/P50/P52. Пересчёт raw подтверждает арифметику, но не является повторным независимым обучением. Локальные receipts, исходники проверок и манифесты приложены к пакету аудита.

## 1. Что уже работает

| Исправление | Подтверждение на текущем срезе | Граница утверждения |
|---|---|---|
| Дубликаты normalized example ID | `same-id` и его Trim-вариант отклоняются; согласованность счётчиков проверяется | Не доказывает изоляцию mutable answer |
| Явная `FromCopy` для KNN | Мутация входного DTO и полученного DTO не меняет следующий ответ | Только overload с назначенной правильной copy policy |
| `PredicateDatasetRouter` | Свои копии features/labels; null и поздняя ошибка новой партии сохраняют память; valid Train заменяет таблицу | Не распространяется автоматически на другой `RouteClassifier` |
| Фактическое число соседей | Один найденный сосед даёт его голос, а не деление на запрошенное k | Не доказывает обобщение обученного маршрутизатора |
| HNSW/BruteForce vector ownership и shape | Мутация исходного вектора не меняет запись; неверная длина отклоняется; валидный HNSW save/load работает | Отдельная структура графа при load имеет новый пробел F10 |
| RNG по полям | Current helper сравнивает структуру/ключи/seed/cursor, а не ToString коллекции | Source/test review; полный cache parity suite здесь не запускался |
| Native child exit | Общий runner сохраняет ненулевой код прогона перед ошибкой discovery | Не исправляет автоматически каждый отдельный runner |
| P25 checkpoint | В raw cold0/24 → trained19/24 → restored19/24; ответы до/после совпали24/24 | Пять неверных ответов тоже совпали; это checkpoint API, не любой export/import |

Источники: [KNN owner][S1], [snapshot policy][S2], [dataset router][S3], [RNG assertions][S4], [общий runner][S5], [P25 raw/report][S6]. Часть подтверждений получена исполнением; часть — прямым чтением актуального source и сохранённых receipts, как отмечено в таблице.

## 2. Исторический остаток очереди102 на cbf3b1d

| ID | Статус на проверенном SHA | Что осталось |
|---|---|---|
| R1 | DONE | Уникальность normalized ID и четыре счётчика уже реализованы; не назначать заново |
| R2 | PARTIAL | Явная copy policy работает; старый overload по умолчанию принимает неизвестный mutable output с общим владением |
| R3 | DONE | Dataset router владеет входными массивами |
| R4 | DONE | Dataset router сохраняет старую модель при отказе партии |
| R5 | DONE | RNG сравнивается по полям; коммит703e3c5 |
| R6 | PARTIAL | f, приоритет настоящего exit, закрыт5068766; a–e остаются; добавить проверку отдельных P50/P52 entry points |
| R7 | OPEN | Эффективные evaluators P30/P40 по raw observations |
| R8 | OPEN | P31 resolver версионированной числовой поправки |
| R9 | OPEN | Ordinary compiler вызывает keyword overload; learned route не потребляется |
| R10 | CONDITIONAL | Только оставшийся эмпирический вопрос после ремонта постановки; это не обязательный повтор каждого опыта |

Старое `done=0, remaining=10` в102 устарело. Таблица103 исторически корректно отвечает на наличие коммитов и сама ограничивает свой вывод. Её 14 слотов — не14 независимых улучшений: warmup присутствует в обеих очередях, один слот фиксирует HOLD границы RNG, а reachability-проверка завершилась отрицательным результатом. [Исходная очередь102][S7], [карта103][S8].

### Что означает закрытие14 строк88/95

| Очередь / слот | Коммит или результат | Принимаемое утверждение |
|---|---|---|
| 88/1 warmup | `2a8d9a0fbf` | Исправлен первый шаг SFT; общий owner Trainer |
| 88/2 P05 | `8649f0790f` | Восемь receipt controls; не объявлять постоянным регрессионным набором |
| 88/3 P11 | `9e91fdb1a6` | Контроли соответствующего receipt сохранены |
| 88/4 P10 | `8bfede6f24` | Исправлен численный вывод: измеренный фактор3.713, не обещанные≥4 |
| 88/5 DPO | `cfef8a9650`, `1cb9c87ec9` | Постоянные проверки native composed gradient и embedding reachability добавлены; production repair не заявляется |
| 95/1 warmup | `2a8d9a0fbf` | Тот же перенос; не удваивать число улучшений |
| 95/2 sampler fallback | `e9e491d009` | Исправлен invalid top-tail fallback; sampling-policy claim ограничен |
| 95/3 metadata | `51add391e2` | Исправлен другой owner: `TransformerEmbeddingKnnLogicalUnit` |
| 95/4 DPO ref cache | `9c0edc7f80` | После принятого restore очищается reference-score cache |
| 95/5 consumers | `2b804e4e74`, `4b40c25cb8` | Native sampler composition и chunked cache имеют consumer checks |
| 95/6 pruning | `cf8f286e5a` | Reachability-контроль исполнен; protected removals prevented=0, положительного эффекта нет |
| 95/7 shape admission | `fd6054de53` | Есть positive/negative контроли; valid admission8→9 |
| 95/8 RNG boundary | `7d0817d469` | Зафиксировано native/adapted различие, HOLD; не native exact-resume PASS |
| 95/9 source/run binding | `7d0817d469`, продолжение72b | Реальная часть реализована; R6 содержит конкретный незавершённый остаток |

Это source/history audit, а не повтор полного набора тестов каждого перечисленного коммита. Новые дефекты ниже не отменяют автоматически соседние исправления с другим владельцем.

## 3. Открытые находки

### F01 — default KNN всё ещё отдаёт изменяемый ответ общей ссылкой · P1

Старый публичный `Train` вызывает `ResolveDefault`. Для неизвестного mutable типа fallback выбирает `DeclaredShared`, где storage/read возвращают исходный объект. Воспроизводитель принял такой урок, получил тот же output reference и после изменения `Steps` прочитал из памяти `CORRUPTED_BY_READER`. Явная `FromCopy` прошла оба контроля мутации.

Новые тесты на неподдерживаемый mutable тип проверяют явный `KnownImmutable<T>` и null policy, а старому overload разрешают `declared-shared`. Они не закрывают исходное обещание владеть mutable payload по умолчанию. [S1], [S2], [S9].

В bounded inventory94 файлов production областей21/18/23/50 прямой вызов plain `KnnLogicalUnit<TInput,TOutput>.Train` не найден. Там встречается другой `TransformerEmbeddingKnnLogicalUnit<string>`. Mutable plain caller найден в экспериментеP39; P50 использует явную copy policy. Поэтому доказан дефект публичного API и неполная приёмкаR2; порча действующего ProductHost этим конкретным путём **не установлена**. Перед breaking change нужен полный caller inventory. План: для неизвестного mutable типа требовать явную политику до построения памяти, immutable allowlist сохранить; explicit shared режим квалифицировать отдельно.

### F02 — соседний RouteClassifier повторяет alias и очистку до отказа · P1

В `29.SemanticCompiler/Routing/RouteClassifier.Train` выполняются `_train.Clear(); _train.AddRange(examples);`. Внутренние `int[]` остаются собственностью caller. В native probe изменение первого учебного массива меняет запрос2 с PLAN на CODE_Q. `Train(null)` отказывает, но следующее обращение даёт NOT_TRAINED: прежняя память уже удалена. [S10].

Исправление22.PredicateDatasetRouter корректно и не переоткрывается. Тот же контракт владения/атомарной публикации требуется **другому владельцу**: проверить и скопировать всю новую таблицу, затем заменить принятую; valid replacement сохранить. Ремонт обязателен до подключения этого learned пути к facade.

### F03 — ordinary Compile не читает обученное решение · P1, остатокR9

В настоящем `SemanticCompilerPipeline.Compile` вызывается `Classify(string[])`, использующий слова-предикаты. `Train` наполняет таблицу для другого overload, `Classify(int[])`. В probe один и тот же router после смены учебной метки возвращает PLAN→ARCH через обученный overload, но обычный `Compile("fix null")` остаётся BUG→BUG. Это непосредственно воспроизведённый разрыв consumer path. [S10], [S11].

Вместе с этим source review нашёл отдельный работающий learned branch в `23.SemanticCompilerAdapter/DomainClassifierUnit`, через compatibility wrapper12. Вывод о bypass относится именно к29.Compile, а не ко всем product routes. Результат внутреннего learned теста нельзя переносить на другой публичный путь без сквозной трассы. Нужно выбрать существующего владельца extraction/routing, проверить совместимость schema и назначить наблюдаемый fallback; соединение не должно превращать неизвестный input в выдуманный уверенный ответ. Также не смешивать этот owner с keyword-wrapper из22.AgentAlgorithms.

### F04 — ProbeNN использует уже изменённые веса при backprop дважды · P1

`TrainStep` обновляет W2 до вычисления dH и W1 до dEmbedding. Рецепт ремонта P48 иллюстрирует только первое место. Конечные разности на одном pre-step состоянии дали максимальную абсолютную ошибку: embedding **0.02029646**, W1 **0.01132778**, W2 **1.57×10⁻⁷**. Это фактическая проверка native метода с lr0.1, маленькой фиксированной сетью и одним шагом, а не оценка качества обучения. [S12].

Нужна последовательность forward → все градиенты по старым весам → применение всех обновлений. Приёмка охватывает embedding/W1/W2 и весь шаг; фиксированная output loss сама по себе недостаточна. Какая доля NaN в P48 вызвана именно этим дефектом, не установлено; исправление не гарантирует устойчивость исходного lr0.05.

### F05 — защита общего runner не покрывает реальные точки входа · P1

В P50/P52 существующий claim удаляется как якобы оставшийся от умершего процесса, без проверки PID+start/terminal/владения. Код допускает второй запуск в тот же каталог при ещё живом первом. Факт одновременного столкновения по журналу не установлен; **возможность следует из исходного ветвления**. В P50 фактически подтверждены два последовательных основных исполнения под attempt-0001; первое завершилось exit0, затем повтор был разрешён наличием архивного status. [S13], [S14].

Общий R6 также частичен: P20/P21 PS1 указывают attempt-0002, но C# сохраняет в hardcoded attempt-0001; обязательный parent metrics P20 помечен optional; consumed-launch не фиксируется атомарно до process; проверяется entry DLL/runtimeconfig, но не все учтённые dependency DLL/deps; import scope не равен полному evaluated MSBuild graph. Исправленный приоритет настоящего exit сохранить. [S5], [S15], [S16].

Нужен общий контракт с исполнимым тестом каждого entry point. CreateNew terminal после обучения защищает позднюю запись, а не уже потраченный бюджет.

### F06 — P50/P52 не оставляют восстанавливаемое состояние learner · P1

P50 пишет manifest, lessons и hash отдельно обученного tokenizer witness; реальный tokenizer/index/output map KNN не сериализуется. Дальнейшее чтение идёт из того же объекта. Разные формы JSON до/после и timing делают различие SHA недостаточным доказательством изменения сохранённой модели. P52 получает полный `ExportCheckpoint`, но сохраняет hash/counts/signatures вместо полного checkpoint. [S17], [S18].

Узкий наблюдённый эффект обучения сохраняется, а обещание восстановления после падения — нет. Рецепт повторного Train не заменяет сохранённую память. Для KNN нужен собственный state контракт, для нейронного exact continuation — weights/optimizer/RNG/data order. Требовать AdamW у дискретной памяти неверно. P25 показывает хороший новый процесс/Train0 для своего checkpoint API; P42 всё ещё имеет отдельный провал ExportAsync/ImportAsync.

### F07 — сводная цена P50 не включает оба основных исполнения · P1 для отчётности

Текущая таблица8lessons/8graphs/16node calls отражает второй проход; архив первого содержит ещё8/8/16. Полный основной расход — **16/16/32**. С учётом дополнительных probe fits source/raw дают52 предъявления уроков native memory,60 native unit queries,8 cold observations и2 отдельных BPE witness fit. Пять дополнительных probe fits и36 их queries сохраняются отдельными стадиями. [S19].

Дополнительные контроли не объявляются незаконными: проблема в идентичности повторного исполнения и неполной сумме. Исправляется чтением существующих receipts, без Train. Fits, уроки, optimizer updates и queries — разные единицы. Потерянное измерение обозначается unknown, не0.

### F08 — независимость файлов не равна независимости входов learner · P2

В P49 все4 экзаменационные пары `(Input,Gold)` — `(0,1),(2,3),(0,3),(4,5)` — встречаются в TRAIN. Сеть видит Input, но не file/project. File-disjoint разбиение подходит для удержания знакомого отображения, однако не доказывает generalization на незнакомом входе. Такой же контроль нужен связанным P46/P48. [S20].

Результат6/16→11/16 остаётся полезным наблюдением данной панели. Он не доказывает ускорение обучения: есть один seed и одна точка после60 новых updates, а B дополнительно имел600 warmup updates. Цена подготовки и её возможная амортизация должны быть видимы.

### F09 — адресные исправления нельзя путать с продуктовым выигрышем · P1/P2

P47: основной EXAM4/4 в обоих плечах; на контекстных переходах8/12→12/12, destructive collisions4→0. P51: после обученияA2/4,B4/4, ошибочное принятие1→0; topic устраняет затирание двух значений bank **в экспериментальной копии**. Native exact map по-прежнему адресуется normalized source, без TopicDomain. [S21], [S22], [S23].

P52: адресные меткиA0/2,B2/2,mutation0/2; итоговый текст4/4 во всех trained arms, включая copy-off/mutation. Допустимый вывод KEY_SHAPE_ONLY/NO_CONTRAST по тексту. Current builder всё ещё строит rule tags из `pair.Rejected`. Первичное N29 заранее описывало маскировку exact/prefix routes. [S24], [S25], [S26].

При переносе признака надо доказать, что его можно получить из runtime source/context до известного ответа. Teacher annotation и rejected объяснение должны храниться как provenance, а не незаметно становиться недоступным runtime признаком. Нужны трасса выбранного route и контрпример, реально достигающий изменяемой памяти. Нельзя подменять это прямым вызовом внутреннего store.

### F10 — HNSW Load принимает неверный EntryPoint · P2, новый отдельный дефект

Из валидного сохранения с одним узлом меняется только EntryPoint на99. Length/count проверки проходят; Load возвращает объект, а Search падает с ArgumentOutOfRangeException. Валидное сохранение в контрольном probe загружается и ищется. [S27].

Добавить проверку entry point, empty sentinel, уровня и допустимых neighbor IDs до публикации загруженного объекта. Не требовать симметричности всех рёбер без основания: pruning может хранить направленные связи. Эта находка не опровергает уже работающие length/ownership guards.

### F11 — raw результаты должны читаться через эффективный evaluator · P1

P30: одинаковый победитель при крайних alpha линейного score не позволяет различить промежуточный alpha; успешный подбор требует контрпримера с изменением действительного consumer. P40: accuracy1/2 в обоих плечах, spread0; диагностические50п.п. headroom при иной постановке не равны выигрышу основного опыта. P31: существующая поправка должна разрешаться по identity/hash исходного receipt;6уроков,21budget queries и4extras нельзя сливать в27 одинаковых «вызовов». Это остающиеся R7/R8, а не новый повод для обучения. [S7].

### F12 — формулировка результата и текущий статус должны следовать данным · P2

P48 имеет на3 больше неконечных весов в финальном B, но без временного ряда нельзя утверждать «ускоряет разрушение». N33 отделяет исторический неуспех weighted E44/E45 от target-topology confound: это ограничивает причинное объяснение, не отменяя установленную политикуD09. В ряде PRIMARY всё ещё видны DESIGNED/SHALL_IMPLEMENT при наличии исполненных receipts. Карточки обновлены адресными current notes с приоритетом над историческим текстом; старая попытка не превращается в новый разрешённый запуск. [S28], [S29].

## 4. Что в действительности показали проверенные эксперименты

| Опыт | Проверенный результат | Допустимый вывод / открытая часть |
|---|---|---|
| [P23](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P2301/results/attempt-0001/report.md) | Исправлены прежние «7 выигрышей» | Unique vector maximum; не7 улучшенных конечных ответов |
| [P24](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P2401/results/attempt-0001/report.md) | Был SFT; DPO0 в обоих плечах; панель1/4 | Replay-гипотеза не закрыта; exact continuation bundle неполон |
| [P25-02](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P2502/results/attempt-0001/report.md) | 0/24→19/24; restored19/24, equality24/24 | Обученная память пережила новый процесс по проверенному checkpoint API |
| [P28](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P2801/results/attempt-0001/report.md) | A2/4,B4/4; given-family baseline4/4 | Чтение заданной семьи, не доказанное освоение морфологии |
| [P41](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4101/results/attempt-0001/report.md) | A2/4,B4/4; threshold.5 и.7 не различены | Consumer reached; значение выбранного порога не идентифицировано панелью |
| [P42](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4201/results/attempt-0001/report.md) | Feedback достигает урока и меняет ответ | Public export/import обученное состояние не переносит |
| [P43](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4301/results/attempt-0001/report.md) | Rules3/4, learned2/4; train7/8 | На этой панели правило надёжнее; общий вывод о бесполезности learning не следует |
| [P44](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4401/results/attempt-0001/report.md) | Route4/4, selector2/4 в обоих плечах | Представление теряет порядок одинаковых шагов; projection не дала выигрыша |
| [P45](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4501/results/attempt-0001/report.md) | Train8/8, exam0/8; reorder calls0 | Механизм не достигнут обычным путём; не доказана его бесполезность |
| [P46](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4601/results/attempt-0001/report.md) | Оба0→12/16; ответы одинаковы, ties8→0 | Улучшилась различимость, не accuracy; полезная ось должна остаться в отчёте |
| [P47](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4701/results/attempt-0001/metrics.json) | Exam4/4 оба; controls8/12→12/12; collisions4→0 | Подтверждён адресный эффект контекста в копии, не native перенос |
| [P48](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4801/results/attempt-0001/report.md) | По2400updates, обе сетиNaN, exam0/4 | Вопрос weighting остаётся открыт; сначала исправить learner и постановку |
| [P49](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4901/results/attempt-0001/report.md) | A6/16,B11/16; new material6/8→7/8 | Удержание/чтение знакомых отображений; не скорость и не unseen-input generalization |
| [P50](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P5001/results/attempt-0001/report.md) | A2/4,B4/4,mutation0/4; train8/8 | Узкий причинный эффект learned dependencies; есть runner/state/cost дефекты |
| [P51](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P5101/results/attempt-0001/report.md) | A2/4,B4/4; wrong accept1→0 | Topic-aware exact address устраняет collision в копии |
| [P52](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P5201/results/attempt-0001/report.md) | Key labels0/2→2/2, mutation0/2; text4/4 все | KEY_SHAPE_ONLY; translation-quality contrast отсутствует |

Все дроби относятся к своим маленьким панелям; их нельзя усреднять в общую «точность системы». Когда полный raw не пересчитывался собственным скриптом, вывод опирается на прочитанные report/metrics/source, а не на заявленное повторное исполнение.

## 5. Как избежать повторения

1. **Приёмка следует публичному вызову.** Проверить default overload, facade, реальный writer и каждый runner; внутренний положительный тест не переносит доказательство на другой API.
2. **Контрпример обязателен вместе с положительным контролем.** Mutable alias, rejected batch, changed label, missing state, changed dependency и duplicate launch должны ломать именно проверяемый guard. Валидный случай должен продолжать работать.
3. **Состояние имеет тип и потребителя.** Hash описывает проверку байтов; байты модели, loader и fresh-process Train0 доказывают восстановление. Exact continuation требует ещё следующего шага.
4. **Исход решает evaluator.** Derived status получается из versioned raw, а не из строки PASS в report. Контроли constant/rule/given-family и model-visible overlap ограничивают смысл результата.
5. **Ремонт не перезапускает историю.** Старые raw и отрицательные результаты сохраняются. Новое обучение требуется только для конкретного ещё открытого вопроса; отладка report/receipt не требует Train.

Полный контракт и готовый prompt block: [39](../experimets/39-transfer-evidence-contract-2026-10-30.md). Адресные изменения64 карточек перечислены в106. Repository skills использованы как метод работы; их нормативные файлы этим пакетом не переписываются.

## Ссылки на проверенный срез

[S1]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L116-L155
[S2]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/21.AgentInfrastructure/Logical/IKnnAnswerSnapshot.cs#L185-L224
[S3]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs
[S4]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/Tests/RandomStateFieldAssertions.cs
[S5]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/_runners/run-binding/New-RunBinding.ps1
[S6]: https://github.com/lubochka/llm_mvp_core/tree/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P2502/results
[S7]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md
[S8]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/103-transfer-queue-commit-mapping-2026-10-30.md
[S9]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/Tests/ExperimentFixes/KnnAnswerOwnershipTests.cs#L222-L289
[S10]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/29.SemanticCompiler/Routing/RouteClassifier.cs#L52-L99
[S11]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs#L55-L80
[S12]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/35.ExperimentsV2/Core/ProbeNeuralNetwork.cs#L76-L106
[S13]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P5001/Run-Experiment.ps1#L78-L108
[S14]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P5201/Run-Experiment.ps1#L82-L112
[S15]: https://github.com/lubochka/llm_mvp_core/tree/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P2001/reachability-control
[S16]: https://github.com/lubochka/llm_mvp_core/tree/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P2101/shape-preflight-acceptance
[S17]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P5001/Program.cs#L317-L399
[S18]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P5201/Program.cs#L1143-L1176
[S19]: https://github.com/lubochka/llm_mvp_core/tree/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P5001/results/attempt-0001
[S20]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4901/Program.cs#L125-L152
[S21]: https://github.com/lubochka/llm_mvp_core/tree/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4701/results
[S22]: https://github.com/lubochka/llm_mvp_core/tree/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P5101/results
[S23]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/27.TranslationDomain/TranslationTrainedUnits.cs#L251-L268
[S24]: https://github.com/lubochka/llm_mvp_core/tree/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P5201/results
[S25]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L88-L102
[S26]: https://github.com/lubochka/xiigen-planning/blob/3b7537c267e45002ebab4f44845fe4ce92f204f9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md
[S27]: https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/06.VectorSearch/HnswIndex.cs
[S28]: https://github.com/lubochka/llm_mvp_core/tree/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P4801/results
[S29]: https://github.com/lubochka/xiigen-planning/blob/3b7537c267e45002ebab4f44845fe4ce92f204f9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md
