# Адресный план исправлений после P101 — 2 ноября 2026

<!-- POSTMERGE-HARDENING-2026-11-02:START -->
## Уточнение A101-R2: полный Unicode вместо singleton фильтра

M02 плана127 конкретизирует существующую границу: tokenizer bytes+state у GenerateCore, admission до существующего sampler, terminal completeness, raw FirstTokenDecision отдельно от mask. Запрещать все byte-token≥128 нельзя; ж=[208,182] остаётся positive. Новые active byte tests должны согласоваться с этим договором. DESIGN_BOUNDARY/HOLD общего default сохраняется до native multilingual/ordinary/resource приёмки.

Основание и выбранный ремонт: [127](codex-127-postmerge-addressed-repair-plan-2026-11-02.md). Исходные результаты и учебные дозы сохраняются. Контроли реализации не добавляют скрытые Train/Read к научному паспорту; их стоимость и scope указываются отдельно.
<!-- POSTMERGE-HARDENING-2026-11-02:END -->

<!-- POST102-AUDIT-2026-11-02:START -->
## Граница этого исторического среза

Этот документ сохраняет результаты аудита после P101. Позднее опубликованы P102, MethodKnn distance tie fix и Q127–Q130; их новая проверка и действующие статусы — [120](120-post102-transfer-and-executor-audit-2026-11-02.md), уточнённый остаток — [121](121-post102-addressed-repair-plan-2026-11-02.md). Здесь не пересчитываются задним числом прежние101/25/126. Текущая очередь103recorded/27designed из130, NEXTQ104; ниже сохранены исходные доказательства и план своей даты.
<!-- POST102-AUDIT-2026-11-02:END -->


Основание: [аудит117](117-post101-transfer-and-experiment-audit-2026-11-02.md), core `3194310941c428ef9a5e643c4dc9d7ad7d15447e`, planning `6ed289b26ce198e16890fe4c7dd6b0ae55f885d9`. Этот план обновляет текущий статус114/116, сохраняя RF01–RF11. Выполнение аудита и правка карточек не равны выполнению перечисленных ниже продуктовых ремонтов. Научный NEXT принадлежит [индексу](../experimets/cards/00-execution-index.md).

## 1. Порядок работ и решения

Сначала устранить пути тихой потери данных и ложного допуска, затем закрыть приёмку уже внесённых переносов и наблюдаемость. Независимые владельцы могут работать параллельно, но один файл/state имеет одного писателя. Не повторять завершённые P93–P101 и не увеличивать их учебные бюджеты ради оформления.

| Приоритет / ID | Владелец и предметный результат | Статус на срезе | Зависимость и завершение |
|---|---|---|---|
| P0 / A101-R1 | Tokenizer + обычный TextGenerator: неизвестный ID виден вызывающему | OPEN, source + P99 witness | Общая decode/ID граница; native ordinary/detailed положительные и отрицательные controls |
| P0 / A101-R3 | Wiki metric runner + два publication gates: решение по заявленному состоянию | OPEN, source call trace | Сначала контракт времени метрики и совместимость старых receipts, затем producer и все consumers |
| P0 / RF01 | TranslationMachine: партия принимается целиком | OPEN, актуальный source witness | Staged state→полная validation/evaluation→одна публикация; поздний отказ сохраняет S0 |
| P0 / RF06 | Translation retrieval: пригодный перевод достигает executor | OPEN, два разных source witnesses | Полный admission до cap/завершения tier; logical dedup отдельно |
| P1 / RF02d-residual | Run binding: child читает именно допущенные bytes | PARTIAL: rehash уже реализован | Immutable snapshot/owned handle; управляемый late mutation witness без модели |
| P1 / RF02a–c | Build source identity, terminal admission, полный ledger | OPEN по указанным границам | Effective project instances; настоящий поддерживаемый entry; не объединять три обязанности одним PASS |
| P1 / RF05a | RouteClassifier: точный урок не проигрывает постороннему большинству | PARTIAL: zero-overlap уже исправлен | Явное правило точного совпадения/конфликтов + ordinary wrapper; learned architecture остаётся отдельной |
| P1 / A101-R4 | BPE outcome проходит к обычному обучающему отчёту | PRODUCER_IMPLEMENTED, consumer OPEN | Необязательный target size не превращать в безусловный fatal; сохранить явный reached/shortfall/reason |
| P1 / A101-R5 | Измерители: одинаковые величины, scope, строки, полный расход | Документные errata исполнены; writer repairs OPEN | Saved-raw correction с target hash; 0 новых updates |
| P1 / A101-R6 | Приёмка сделанных P89/HNSW/EOS/confidence fixes | SOURCE_IMPLEMENTED; часть stored component evidence есть | Адресный ordinary/native test на актуальной assembly; без ритуального полного прогона |
| P2 / A101-R2 | Полноценный UTF-8 вместо однотокенного ASCII-кандидата | DESIGN_BOUNDARY, общий перенос HOLD | Сначала назначенный потоковый контракт; multilingual controls; R1 сам по себе не решает UTF-8 |
| P2 / A101-R7 | Синхронная KNN-работа не выдаётся за возможность полезного async-overlap | Кандидат P96, перенос не принят | Назвать execution mode реальной единицы; сохранить настоящий I/O async и cancellation semantics |
| P2 / A101-R8 | Strict-K при равенствах: отсутствует зависимость допуска от номера токена | Кандидат, текущий default не меняется | Сначала доказать необходимость ровноK; равновероятная граница требует RNG/state договора |
| После зависимостей / прежние RF03/04/07/08/11 и P83–P88 | Адресные незакрытые работы114 | CARRY_FORWARD, не новая полная аттестация | Точный остаток в§9; не назначать уже внесённые изменения повторно |

## 2. A101-R1 — неизвестный token ID

**Файлы и владельцы:** `01.Tokenizer/BpeTokenizer.cs::Decode`, `09.Inference/TextGenerator.cs::GenerateCore` и все обычные перегрузки `Generate`; error-adapters реальных callers находятся полным reference search перед правкой. Источник — P99 legal/unknown panels и source117/A101-01.

**Выбранное решение:** сохранить строковые сигнатуры и добавить в `01.Tokenizer` один `TokenDecodingException : ArgumentException` с `ErrorCode`, `TokenId`, `TokenPosition` и `SequenceKind` (direct/prompt/generated). Это новый явный контракт, а не уже существующий тип. Unknown ID в словаре получает код `TOKENIZER_UNKNOWN_TOKEN_ID`; отдельный выход за model range сохраняет отличимый код `TEXT_GENERATOR_TOKEN_ID_INVALID`. Единый checked decode проверяет все ID до выдачи строки; GenerateCore использует ту же проверку для ordinary/detailed до следующего Forward/Decode. Позиция нулевая внутри названной последовательности. База ArgumentException сохраняет совместимость существующего catch; [официальная сигнатура](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception?view=net-10.0) допускает наследование. Новый result API в этом slice не вводится. Error-adapters и tests всех найденных callers обновляются вместе с типом исключения; нынешний detailed ArgumentException не выдаётся за готовую типизированную диагностику. Ошибка не подменяется пустым фрагментом или replacement-символом. Проверка model/tokenizer identity лёгкая и не сериализует словарь на каждом токене.

**Один минимальный набор приёмки:** неизвестный отрицательный ID, ID за словарём в начале/середине/конце; законный смешанный поток, пустой поток в прямом Decode, последний законный token ID; искусственный sampler возвращает недопустимый ID в ordinary/detailed/first-decision. До отказа не публикуется частичная строка; код/позиция одинаковы; законные bytes/ответ/порядок остаются прежними. Сохраняется новый EOS-договор: последний элемент vocab не является EOS, explicit EOS останавливает в прежнем допустимом месте. Валидный byte-fragment не обязан отдельно образовывать символ.

**Необходимый receipt:** точные assembly/source hashes, caller, sampled stream, ошибочная позиция, published bytes, stop reason и forward count. Использовать сохранённый обученный BPE/P99 corpus; не обучать замену существующему состоянию. Новые fixtures не являются продолжением исторического P99 и не меняют его raw. Для этого контракта модельное обучение не требуется.

### A101-R2 — UTF-8-поток и однотокенный ответ: отдельный договор

Это отдельная проектная граница после проверки ID, не обязательное свойство R1. Для будущего строгого режима обычного вывода выбран договор по **байтам накопленного ответа**, а не по повторной токенизации каждого ID: состояния VALID_COMPLETE, VALID_PREFIX и INVALID. Внутри ответа допустим корректный незавершённый префикс; EOS и финальная позиция maxTokens разрешают только завершённый UTF-8. При отсутствии допустимого продолжения — явный отказ `NO_ADMISSIBLE_UTF8_CONTINUATION` без лишних Forward/Draw/updates и без скрытого увеличения бюджета. Изолированный однотокенный контракт обязан закончиться в VALID_COMPLETE; общий режим не запрещает byte fragments, составляющие русский/ивритский/emoji-символ вместе.

Контроль допуска расположен до фактического greedy/stochastic выбора и учитывает весь prefix, конкретный tokenizer byte mapping, EOS и оставшийся бюджет. Exact retention требуется для прежних14 greedy readouts, законного greedy winner и режима, где ни один кандидат не исключён. Для stochastic сохраняется заявленный RNG protocol/число Draw, но тот же seed не обязан давать прежний законный token после перераспределения вероятности. До реализации фиксируются порядок admission относительно topK/topP и ожидаемое условное распределение; его изменение, качество и стоимость измеряются отдельно. Это одна из причин статуса DESIGN_BOUNDARY для общего default. Панель приёмки: Я/א/🙂 целиком и по byte fragments; malformed continuation, overlong/surrogate sequences, lone continuation byte, EOS/length на неполном prefix, законный ASCII и whole learned-BPE Cyrillic token. Новый общий default остаётся HOLD до этой multilingual/ordinary приёмки и измерения качества/цены; имеющиеся14 ASCII positives его не квалифицируют. Сама смена API и связывание reader — будущий product slice, здесь задан проверяемый договор.

**Позднее подтверждение03714d8:** ограничения должны применяться в фактической точке выбора ответа, включая `temperature <= 0`/argmax, а не только в TopK/TopP mask. Проверить ordinary `TextGenerator` delegate type, constructor method-group binding и invocation: неиспользуемая новая перегрузка может успешно собраться и ничего не исправить. Однотокенный roundtrip через learned BPE допускает законные не-ASCII tokens, но для общего потока всё равно нужен prefix/terminal contract. Negative controls: mask-only оставляет6/6 greedy дефектов; unused-overload оставляет6/6; positive — прежние14 lawful readouts. Это сохранённое component evidence нового commit, не новая native приёмка здесь.

## 3. A101-R3 — честный финальный показатель и его читатели

**Владельцы:** `42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs`, `ProjectWikiDomainModels.cs`, `ProjectWikiTrainingOrchestrator.cs`, `ProjectWikiTrainingFlowAdapter.cs`, `CrossStackBusinessFlowGraphTrainingPipeline.cs`, `CrossStackBusinessFlowGraphManifestPublisher.cs`; reference search всех FinalLoss/FinalAccuracy/FinalRewardMargin обязателен. Не назначать замену тем отдельным pipelines, которые уже решают по HeldOutAccuracy.

**Решение о времени метрик:** новый receipt различает `TrajectoryMean*`, `LastStepPreUpdate*`, `FinalStateTrain*`, `FinalStateHeldOut*` и происхождение/число наблюдений. Последние две группы получают только чтение одного окончательного checkpoint, одинакового gold и неизменного reference; отсутствие измерения остаётся null/unknown. **Для двух cross-stack gates выбран один явный предикат:** все необходимые значения finite и измерены на одном final checkpoint; `FinalStateTrainAccuracy >= 0.75`, `FinalStateTrainRewardMargin > 0`, `FinalStateHeldOutAccuracy >= 0.75`, плюс уже существующие structural/authority gates. `FinalStateTrainLoss` — отчётная величина, в этот порог не входит. TRAIN evaluation использует зафиксированные фактически обучавшие pair IDs; held-out — прежний независимый batch. Mean и last-step не подставляются при отсутствии final reading. Другие pipelines с уже правильным HeldOut gate не переводятся на TRAIN автоматически.

Старые serialized Final* сохраняют прежнюю семантику с version/qualification. Не преобразовывать trajectory mean в final-state score, если новых финальных чтений в старом raw нет. Старые Final* остаются отдельными legacy trajectory-mean полями для чтения истории; не создавать alias, меняющий их смысл. Для новой публикации legacy receipt без final-state evaluation даёт `FINAL_STATE_METRICS_NOT_MEASURED` и HOLD; уже опубликованный исторический артефакт от этого не удаляется. Дополнить старый receipt можно новым hash-bound чтением существующего checkpoint с Train0. Писатель/читатели/порог публикации меняются одним связанным slice, без периода смешанной семантики.

**Приёмка без обучения с вычислимым ответом:** при structural/authority=true, final margin=+0.2 и final held-out accuracy=1 receipt A имеет trajectory `[1,1,1,0]`, mean0.75 и final TRAIN accuracy0 — REJECT; receipt B имеет `[0,0,0,1]`, mean0.25 и final TRAIN accuracy1 — ADMIT. Отдельный C имеет last-pre-update accuracy1, final TRAIN accuracy0 — REJECT. Для обеих actual gates проверить границы TRAIN0.75/0.749, held-out0.75/0.749, margin0/+0.001, null/NaN/Infinity. Legacy без final — HOLD; zero-step с явно выполненной final evaluation судится по тем же значениям, без неё — HOLD. Unequal PairCount проверяет отдельное среднее. Отдельно сохранить корректный pair-weighted mean. Для существующих learned checkpoints допустимы чтения с Train0 и state-hash before/after; если payload недоступен, не создавать новый S0. Новый общий quality experiment этим планом не назначен.

## 4. RF01 и RF06 — переводческая память

### RF01: публикация партии

Выбранный путь — **staging**, а не восстановление после уже видимой частичной записи. Typed ingress подготавливает все train/held-out записи; отдельный candidate содержит именно применимые tokenizer/index/output map/exact map/history/readiness/version. Обучение и held-out evaluation работают с candidate; одна проверяемая операция заменяет live state при неизменной исходной revision. Для текущих readonly `_flow/_unitTrainers/_runtime` добавить один owner-level `TranslationRuntimeState`/snapshot provider: все units и их state принадлежат одной revision; staging не разделяет с live mutable tokenizer/index/maps. Publish выполняет compare-and-swap ожидаемой revision. `InlineTranslationOrchestrator`, в том числе полученный до обучения через `GetOrchestratorAsync`, хранит provider и захватывает один целый snapshot в начале каждой операции. Уже выполняющаяся операция заканчивает наS0, следующая операция того же orchestrator берётS1. Не перечитывать snapshot между узлами: смешанная revision запрещена. Конфликт publish даёт явный `TRANSLATION_REVISION_CONFLICT`, не повторный Train.

Приёмка: S0 содержит старый законный урок; первая новая строка корректна, вторая — JSON `42` вместо ExecutorInput. Ещё четыре late witnesses: ошибка trainer, отмена, ошибка typed held-out, конфликт revision на publish. Сразу после отказа и до restore сравнить старые ответы, state digests, count/history/readiness/version. Положительная партия принимает оба новых урока и сохраняет старый. Читатель, выданный до Train, после publish видитS1; операция с barrier, начатая наS0, не смешивает его units сS1. Новая операция видит только целую новую revision. Crash recovery и in-process atomicity — разные требования; отсутствие crash test не подменять словами «полная транзакционность». Объём native fixtures объявляется до исполнения; повтор P5301 не требуется.

### RF06: полный admission до остановки поиска

Фактические владельцы — `TranslationKnnStore.FindNearestCascade` в `TranslationUnitContracts.cs` и ordinary executor в `TranslationTrainedUnits.cs`. **RF06a:** передать в cascade один полный predicate пригодности конкретного caller, включая target language и source threshold; tier успешен по уже пригодным candidates. Cap применяется после admission; существующая tier priority сохраняется. Это самостоятельный ready slice без изменения schema. **RF06b:** logical dedup требует следующего schema slice и не закрывается RF06a. Для новых записей ключ `(UnitId, SourceId, ExampleId)` берётся из реального `TranslationLearningExample`; все три AddPair из одного примера несут один ключ, отдельные examples — разные. Контрольный digest input/chosen content обнаруживает same-key/different-content и даёт конфликт до публикации; автоматического last-write revision правила здесь нет, оно остаётся RF11. Checkpoint v2 сохраняет этот ключ/digest вместе с каждой записью. Legacy v1 не содержит identity: сохранять отдельные rows с явным `identity_unavailable`, не угадывать общий lesson по одинаковому тексту. Полный dedup для legacy требует восстановления mapping из сохранённых Examples по отдельной проверяемой миграции без нового model Fit; при отсутствии Examples оставить эту ось OPEN. Приёмка v2 не объявляет мигрированным весь архив.

Отрицательные свидетели: (1) первые5 target-invalid, шестой valid приk5; (2) full source similarity0.50 проходит store0.35, не проходит caller0.60, prefix1.00 должен быть найден. Положительные: пригодный full имеет приоритет; all-invalid возвращает честное отсутствие; несколько aliases одного lesson не заполняют k; два разных пригодных lessons не схлопываются. Проверить ordinary executor и исключить обход через exact/phrase/wordmap. Простой component result не закрывает эту строку.

## 5. RF02 — идентичность входов и сборки

### RF02d-residual: от допуска к чтению

Сохранить сделанные prelaunch fingerprints. Для файлов, которые child читает позже, выбрать immutable snapshot в run-owned директории: fingerprint считается по копии, child получает её путь, receipt фиксирует digest фактически открытой копии. Для API, которые нельзя перенаправить, нужен явно документированный owned handle/locking договор; один дополнительный hash после чтения только обнаруживает расхождение и сам не делает чтение атомарным. Не объявлять snapshots всех произвольных внешних ресурсов реализованными этой одной правкой.

Tiny Reader fixture ставит barrier после admission и до чтения, контролируемо меняет оригинал; child либо читает неизменный snapshot, либо получает определённый отказ. Дополнительно unchanged, missing→missing, optional created/removed, extra changed и valid required. Не использовать модель/новое обучение для доказательства файлового договора.

### RF02a: source closure по экземплярам

В `Get-RunBindingProjectEvaluation`/`Get-RunBindingSourceClosure` ключ посещения — canonical project path + полный эффективный набор global properties, влияющих на evaluation; учитывать ProjectReference properties/условия/imports/configuration. Выбранный способ получения графа обязан соответствовать реальной поддерживаемой сборке, а не regex-списку файлов.

Fixture: один child project включён двумя ссылками с разными AdditionalProperties, разные Compile items и conditional import. При изменении каждого реально компилируемого файла соответствующий run identity меняется; неиспользуемый файл не притворяется входом. Запустить полный `New-RunBinding`→`Invoke-BoundRun`→tiny child на поддерживаемой платформе и сохранить actual evaluated closure. Ручная phase6fixture — дополнительное компонентное доказательство.

### RF02b/c: terminal и расход

Legacy terminal receipt сверяется по stable ID, source/config/input identity, состоянию выполнения и consumed evidence, а не только JSON-валидности/равенству копий. Все refused/failed attempts и их фактические операции сохраняются. Новые unit-level tests checker используют отдельные причины отказа вместо исключения: старые8 malformed fixtures остаются regression controls. Старый «0» не становится измерением благодаря новой схеме.

## 6. RF05a, A101-R4, A101-R7, A101-R8

**RF05a.** Зафиксировать rule: единственное неконфликтующее exact match имеет приоритет; конфликт exact labels даёт явный конфликт/abstention, а не выбор по insertion order. Для ненулевых расстояний в этом узком slice сохранить нынешнее невзвешенное голосование без изменений. Его возможная замена — отдельное будущее решение; retention всех семи точных seed-уроков обязателен сейчас. Обычный `PromptDomainClassifier`/`DomainClassifierUnit` показывает neighbors, distances, selected label, UsedKnn, итоговую confidence и fallback. Unknown/no-overlap остаётся честным отказом. Этот repair не закрывает архитектурное подключение нового learned router к semantic compiler.

**A101-R4.** `DefaultLogicalCapabilityTrainer.BuildTokenizer` принимает `BpeTrainingOutcome` и передаёт requested/achieved/merge count/reason в собственный диагностический/обучающий результат. По умолчанию корпусно ограниченный valid словарь остаётся usable с явным shortfall; только caller с заранее заявленным exact-size контрактом отказывает до публикации. Проверки: reached, corpus exhausted, request<256, старый допустимый saved tokenizer и ordinary training result. Увеличение словаря ради подавления предупреждения запрещено.

**A101-R7.** У `KnnLogicalUnit.ExecuteAsync` работа выполняется до `Task.FromResult`. Поэтому планировщик не должен выводить способность к полезному overlap из одного Task-return. Сначала описать фактический execution mode/ресурсную границу и назначить последовательное исполнение этой CPU-единицы, сохранив независимые настоящие asynchronous I/O units. Не заменять весь общий интерфейс на sync по одному P96. Приёмка: тот же конечный ответ, started=awaited, нет потерянной ветви и лишней копии состояния; cancellation/quota/late-result требуют собственных назначенных подслучаев. Измерение latency не выводить из количества Task.

**A101-R8.** Текущий sampler допускает все ties на границе; это не дефект, пока контракт не требует ровноK. Если продукту действительно нужен strict-K, выбирать из boundary tie без предпочтения низких token IDs, с отдельно переданным RNG и воспроизводимым сохранением его состояния. 70 комбинаций4-of-8 и перестановка token-ID проверяют support; один seed с ненулевыми counts ещё не доказывает равномерность. Изменение потребления RNG требует собственного compatibility contract: нельзя молча сдвинуть последующую последовательность sampling. Learned readouts P100 имели0boundary ties, поэтому они не служат доказательством пользы этого нового default.

## 7. A101-R5 — исправление отчётов без повторного обучения

| Область | Конкретная правка будущего writer | Отрицательный / положительный контроль |
|---|---|---|
| P101 parity | Сравнивать product.Loss с copy.UnscaledLoss при данном objective; отдельные поля objective_parity, gradient_parity, update_parity и native admission | Разные шкалы не объявлять algebra failure; равные5 metrics не закрывают непроверенные gradients/updates |
| P101 строки | Сохранять RowId/Kind/gold/output до/после и gained/lost/unchanged по TRAIN/EXAM | T2 gained и E2 lost при2/4→2/4 обязаны быть видимы; меняющийся gold/дубликат ID отклоняет сравнение |
| P99 scope | Correction связывает исходный H64 recipe с записанным новым BPE/P98 назначением | COMPLETED_BPE_DIAGNOSTIC не равен executed4SFT; historical neural_segment=false сохраняется |
| P93 calibration | Разделить empirical mean, пригодность DEV для обнаружения ошибок и actual fit count | all-correct mean1/Brier0 допустимы; fit0 не превращается в trained calibrator |
| Стоимость | Typed ledger: APITrain, effective updates, nested BPETrain, дополнительный twin BPETrain, refused attempts, queries/forwards и источник числа | MemoryFits не переименовывать во «все learners»; вложенный Train не считать отдельной независимой программой; unknown не0 |
| Причинность | Endpoint displacement назвать точно; probability отдельно от observed frequency; JIT как непроверенная причина | `+1,-1`: endpoint-rate0 и mean-absolute-step1; фиксированный порядок не доказывает причину timing |
| DPO arithmetic | Исправить словесные40%→20% и добавить вычисляемые absolute/relative error в будущий oracle | Weighted0.9, unscaled0.5, divided0.6 дают0.1 и20%; формула product остаётся прежней |

Correction хранится рядом как новый документ/receipt с SHA256 цели и основанием. Старые JSON/report/stdout остаются доказательствами прежнего исполнения. Для live project docs20% уже исправлены этим пакетом; старый oracle-output не ретушируется. Реализация writer и выпуск versioned correction — отдельный product/tooling slice, не фиктивный новый scientific run.

## 8. A101-R6 — адресная приёмка уже сделанного

Цель — проверить остаточный риск, не создать новый полный quality эксперимент. Перед запуском сверить актуальный SHA и уже появившиеся receipts: если тот же boundary уже принят, использовать его доказательство вместо повторного запуска.

| Перенос | Существующий тест/свидетель | Минимально недостающее доказательство |
|---|---|---|
| P89 UnscaledLoss | `Tests/ExperimentFixes/DpoConvergenceGuardReadsUnscaledLossTests.cs` проверяет source usage; producer oracle проверяет arithmetic | Реальные разнородные Signal в одном batch и оба ordinary consumer; old weighted loss/gradient не меняются; missing metric не является0; disabled/short/invalid configs |
| HNSW | `NativeHnswShapePreflightTests.cs`, сохранённые shape probes | Актуальная native сборка выполняет malformed-target-level/global-depth controls и valid directed graph/load→add; не требовать reciprocity |
| BPE outcome | `BpeTrainingReportsWhetherRequestedVocabSizeWasReachedTests.cs`, сохранённый compiled oracle | Доходящий outcome в ordinary caller поR4; прежние vocabulary bytes совпадают |
| Confidence rename | `LowConfidenceLessonSignalContractTests.cs`, сохранённый alias oracle | Actual caller меняет signal flag; warning сохраняет Ok+answer; alias/new-name одна state cell; прочие ConfidenceFloor механизмы неизменны |
| EOS | Сохранённый product oracle, текущие Generate overloads | Ordinary last-vocab token продолжает ответ; explicit EOS иmaxTokens различимы; decoder-unknown проверяется поR1 отдельно |
| Run binding | `Test-DeclaredInputIdentityContract.ps1`, `Test-ConsumedInputDigestContract.ps1`, `Test-AuditFixtureClaimContract.ps1` | Полный поддерживаемый entry и late-mutation/RF02a fixtures; реальные paths/digests и child receipt |
| P83 | Основной Extract исправлен; старые10 structural component checks | Actual assembly/Roslyn4.12.0 + ordinary caller; target-side expression cases как отдельное расширение; учитывать скрытый GrammarTrain в Compile |

Точные фильтры команд берутся из реально существующего test project/runner на платформе исполнителя; выдуманная успешная команда не включается в receipt. Результат записывает command, source/assembly hash, exit code, elapsed и границу. Авторский текст «тест добавлен» не заменяет выполнение. После этой адресной приёмки новые gates добавлять только под конкретный остаточный риск.

## 9. Остатки прежнего плана, не закрытые текущими коммитами

- **RF03/P30:** constant ties не дают informative alpha contrast; evaluator ремонтируется по saved scores, Train0.
- **RF04/E28:** file-local split, exposure quarantine и реально достижимый adapted donor; composite key отдельно от правильного split.
- **RF07:** два ранее красных translation gates не исправлять ослаблением assertions/ожидаемого count; сначала привязать старую неудачу к её source/assembly, затем проверить актуальный случай.
- **RF08:**0observations→UNEXERCISED,0denominator→UNMEASURED; corrections bound к исходному digest; P89 фактическиеoptimizer0 иSignal13/12 сохраняются.
- **RF11:** authority/revision одного lesson key, old/new reverse order и equal-revision conflict; exact key сам по себе не задаёт authority.
- **P84/P85/P88:** lambda family/arity, obligation-set consumer, actual context integration остаются отдельными кандидатами114. P94/P97 подтверждают важность конечного consumer, но не закрывают эти разные contracts.

Это перенос актуального остатка114, не заявление о новом полном повторном аудите каждой исторической ветви. Если новый commit меняет owner, сначала адресно сверить его, сохранив исходные raw и отрицательные witnesses.

## 10. Готовое задание исполнителю одного slice

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/118-post101-addressed-repair-plan-2026-11-02.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Возьми одну готовую подработу этого плана и зафиксируй текущий owner SHA. DESIGN_BOUNDARY/условный кандидат и legacy-migration без Examples сначала требуют указанного проектного решения/источника и не выбираются как готовый code slice. Назови предметный отказ, producer, ordinary consumer и изменяемое состояние. Проверь, что требуемый repair ещё отсутствует. Сохрани negative witness и valid retention до правки; выбери указанную границу публикации/ошибки. Измени только владельцев этого контракта и зависимых читателей. Не переписывай frozen snapshots/raw/checkpoints и не обучай новый S0 вместо доступного. Проверь назначенный обычный путь, посчитай фактический расход и отдели source-derived от measured. Опубликуй связанные code/tests/docs с подтверждением branch head и readback. Если native boundary не исполнен, оставь его открытым, даже когда component oracle зелёный.

Завершение проверяется по конкретному отказу/ответу, состоянию после вмешательства и достигнутому читателю. Ни количество карточек, ни commit, ни написанный тест сами по себе не означают закрытия продуктовой строки.
