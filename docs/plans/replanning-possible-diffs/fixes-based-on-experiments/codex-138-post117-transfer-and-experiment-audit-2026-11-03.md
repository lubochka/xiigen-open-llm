# После P117: аудит переноса, точности опытов и исполнения

**Дата:** 3 ноября 2026. **Проверенный core:** `879a07bcd4dd747b3672f9ab32492bd23d14b5ea`, ветка `codex/dpo-implementation`. **Проверенный planning:** `66c53bbb654f4e297e611767cd10298a6f4567bb`, ветка `llm_plan3`. Это новый проход после [134](codex-134-post113-transfer-and-experiment-audit-2026-11-02.md), а не повторная аттестация всех 130 опытов.

**Итог:** полезные изменения P114–P117 подтверждаются в своей узкой области. Несколько прежних ошибок действительно исправлены в продуктовом исходнике. Но закрывать весь перенос по записи результата, наличию теста или совпадению ожидаемого журнала пока нельзя: остаются потеря доступа к проигравшему обученному кандидату, слабые проверяющие, неверный общий учёт расходов и неполное доказательство восстановления настоящей памяти. Ни один записанный PRIMARY этим аудитом не назначается повторно.

**Публикационная дельта:** пока шёл аудит, появился core c257523. Он отдельно проверен: actual finalPreferences/combinedTrainingText уже связаны с carrier и отрицательной проверкой heldout до mutation permit. Старое утверждение об отсутствии этой связи больше не применяется к текущему HEAD. Границы и два новых узких остатка PUB01/PUB02 описаны в разделе8 и [отдельной сверке](../../../../experiments-v2/_audits/2026-11-03-post117-audit/publication-c257-reconciliation.md); исходный научный pin879a07b сохранён.

**Вторая публикационная дельта918765f:** S03/S06 получили trusted-threshold/finite/import-before-mutation guards; не назначать их заново как отсутствующие. Вместе с ними появились файлы результата P118: наличие status/outcome подтверждено, научная пригодность этим аудитом не проверена. На SHA 918 отчёт ещё отсутствовал; на 84f он уже опубликован, и текущая карточка требует проверки существующих report/raw без нового обучения. Учёт обновлён до 118 recorded/12 designed, Q119 — навигация. Основной научный аудит по-прежнему P114–P117 на879a07b. Подробности — §9.

**Согласование перед публикацией:** изменения 84f и cb1 сохранены в общих документах. Единый реестр по-прежнему содержит 114 самостоятельных работ с очередью Q01–Q66; R117 уточняют существующих владельцев. Параллельный аудит 43 пунктов принадлежит своему автору и не объявляется повторно выполненным этим пакетом. Подробности и границы — в [записи согласования](../../../../experiments-v2/_audits/2026-11-03-post117-audit/publication-84f-merge-note.md).

Рабочее продолжение: [адресный план 139](codex-139-post117-addressed-repair-plan-2026-11-03.md), [покрытие карточек 140](codex-140-post117-card-coverage-2026-11-03.md), [обязательный договор 44](../experimets/codex-44-post117-saved-evidence-and-finalizer-contract-2026-11-03.md). Предыдущие номера N/R сохраняются; находки этого прохода имеют собственные F01–F11.

## 1. Как получен вывод и где кончается доказательство

Прочитаны изменения после прошлого аудита, актуальные продуктовые владельцы, новые тесты, программы, запускатели и текстовые результаты P114–P117; адресно перепроверены открытые ветви P108–P113 и продуктовые admission/import. Исходники сверены по Git blob, проверенные файлы перечислены в [manifest](../../../../experiments-v2/_audits/2026-11-03-post117-audit/source-manifest.json). Более ранние опыты не сертифицируются заново.

Повторный арифметический разбор сохранённых данных выполнен двумя воспроизводимыми скриптами:

- [P115–P117: скрипт](../../../../experiments-v2/_audits/2026-11-03-post117-audit/recount-p115-p117.py) и [результат](../../../../experiments-v2/_audits/2026-11-03-post117-audit/p115-p117-saved-evidence-recount.json).
- [P114 и модели ветвей: скрипт](../../../../experiments-v2/_audits/2026-11-03-post117-audit/recount-p114-and-branch-proofs.py) и [результат](../../../../experiments-v2/_audits/2026-11-03-post117-audit/p114-and-branch-proofs.json).

Это Python над уже записанными JSON/текстом и явные модели сравнения чисел/ветвления. **Новых Train, генераций, model forward, сборок .NET, PowerShell и нативных тестов не было.** Такие проверки отмечаются `NATIVE_UNRUN`; наличие исходника теста не означает PASS. Для P114 доступны Git LFS pointers: совпадение OID подтверждено, сами бинарные веса/моменты заново не прочитаны. Для P115 также не выполнялась новая проверка байтов весового бинарника.

Применены предметные правила навыков из `docs/skills`: классификация поведения и владельца; task-slice и фазовое планирование; проверка плана/кода; целостность тестов; адресные prompt patches; человеческое объяснение проблемы рядом с действием. Использованные роли разделены: основной аудитор проверяет продукт, второй — планирование и P115–P117, затем проводится перекрёстная сверка. Это не заявление о полном повторном чтении всех исторических больших руководств и не выдуманные «30 проходов».

## 2. Уже исправлено: не назначать повторную реализацию

| Изменение на проверенном SHA | Что исходник теперь делает | Что ещё не доказано |
|---|---|---|
| `9c2167f`, `TranslationMachine.TryPublishVersion` | Все пять путей публикации используют сравнение с захваченной основой через `Interlocked.CompareExchange`; оценка связана с измеренным holder | Реальный поздний конфликт и сохранность проигравшего кандидата: F01 |
| `3b4d2b8`, `EvaluateAsync` | Возвращена подпись интерфейса `ILearningMachine`; прежнее несовпадение числа параметров устранено в source | Новая сборка и проверка через интерфейс не выполнялись |
| `1aaa3bc` + `945fe36`, semantic key | Основной ключ и PairId разделены; версия схемы проверяется против формы данных; кандидат в floor-проверке сначала доказывается; схема и чтение после restore проверяются раздельно | Остальные reader/import/RULE случаи и фактический native PASS |
| `5bb7642`, structured batch | Вид JSON проверяется до чтения свойств; non-object получает управляемый отказ; предусмотренный null обработан отдельно | Положительный обучаемый случай и весь native batch contract |
| `e16987c`, material lineage | Guard пересчитывает отпечаток материала и отвергает неоднозначную семью до обучения | Тождество проверенных rows с реально переданными DPO/text/KNN входами; экспериментальный P112 проверяется отдельно |
| `3499063`, DPO | Нулевые пары исключены до loss, градиента и знаменателя; all-zero возвращает `UpdateApplied=false` до оптимизатора/публикации | Нативное исполнение трёх тестов; исправление старой экспериментальной копии не следует из product diff |
| `ff1f9fe`, lexical statistics | Статистика корпуса строится один раз и переиспользуется кандидатами; оценочная формула сохранена | Реальный продуктовый cost/consumer gate и однозначная сериализация identity: F02 |
| `d673add` + `57f24d7`, UTF-8 | Генератор не декодирует обрезанный хвост незавершённого code point; исходник проверок пригоден для публикации | Это не валидация любой внутренней порчи UTF-8, не grapheme policy; неизвестные token ID по-прежнему отдельный остаток |

Источники: [TranslationMachine](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/27.TranslationDomain/TranslationMachine.cs), [семантический ключ](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/27.TranslationDomain/TranslationSemanticKey.cs), [DpoTrainer](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/11.DPO/DpoTrainer.cs), [snapshot](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/18.CapabilityTraining/CorpusStatisticsSnapshot.cs), [lineage guard](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/18.CapabilityTraining/CapabilityMaterialLineageGuard.cs), [TextGenerator](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/09.Inference/TextGenerator.cs), [BpeTokenizer](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/01.Tokenizer/BpeTokenizer.cs), [новые проверки](https://github.com/lubochka/llm_mvp_core/tree/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/Tests/ExperimentFixes).

## 3. Что действительно показали P114–P117

| Опыт | Подтверждено сохранёнными данными | Предметный ответ | Ограничение вывода |
|---|---|---|---|
| P114 | B с исключённой нулевой парой и C без неё совпадают по сохранённым loss/градиенту/моментам; OID бинарников B/C одинаковы. A/C имеют максимальное записанное расхождение веса `3.203749656677246e-7` | Каждое плечо до/после 2/4; все 24 проверки дают 12 точных | Это устранение нежелательного вклада, а не показанный рост точности. Свежего сравнения backing LFS bytes нет |
| P115 | 4 вопроса × 4 кандидата: оценки, порядок, выбранный документ, полный ответ и confidence совпадают. На четырёх warm-запросах лексическая токенизация 156→4; прочитанные символы 1988→88; allocation 159344→28336 байт. Отдельная разовая постройка snapshot: ещё4 токенизации/6800 байт | 2/4 у обоих плеч | 39-кратное снижение числа вызовов и −82.22% allocations относятся к этому пути/корпусу. Малое время одного запуска не переносится на весь продукт. Все 4 exam-вопроса видело обучение tokenizer |
| P116 | Уникальные пары 15→12, переходы через границы файлов 3→0; записанное текстовое состояние 58→50 байт, hash пересчитан | Экзамен 3/4→3/4; controls 8/12→9/12 | Replay законных пар также изменился; эффект controls не изолирован от него. Потолок project-blind reader: 3/4 exam и 9/12 controls |
| P117 | Обычный RU→EN запрос достигает KNN: 0→1 из пяти строк. Реальные карты executor восстановлены, 3 сохранённых reload-ответа совпадают | 3/5→3/5; экзаменационный ответ теряет «сейчас»; reload exact 2/3 | Открытый путь не равен правильному переводу. KNN восстановлен из ожидаемого журнала, а не экспорта live-store |

У P114 исходная калибровка сравнивала разные величины. Коррекция по прежним числам законна: product Loss = copy UnscaledLoss = 0.6931472; после множителя 1.0833334 расхождение с copy Loss 0.75090945 составляет `6.287647991154444e-8`, меньше `1e-5`. Это не оправдывает старый безусловный terminal и не требует повторного обучения.

Источники: сохранённые [P114](https://github.com/lubochka/llm_mvp_core/tree/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11401/results/attempt-0001), [P115](https://github.com/lubochka/llm_mvp_core/tree/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11501/results/attempt-0001), [P116](https://github.com/lubochka/llm_mvp_core/tree/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11601/results/attempt-0001), [P117](https://github.com/lubochka/llm_mvp_core/tree/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11701/results/attempt-0001); независимые пересчёты выше. Все абсолютные значения относятся к этому срезу.

## 4. Находки и адресное действие

### F01 — отказ CAS защищает текущую машину, но обученный кандидат становится недоступен

**Приоритет P1.** В [TranslationMachine](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/27.TranslationDomain/TranslationMachine.cs), `DescribeLostUpdate` обещает «обученный кандидат сохранён». Однако после обучения stagedRuntime неудачный `TryPublishVersion` в `TrainStructuredBatchAsync` возвращает обычный `Fail`. Нет durable checkpoint, handle или возвращаемого результата, позволяющего восстановить уже оплаченный кандидат. Приватный объект выходит из области видимости. Сам CAS полезен и остаётся.

[TranslationStateVersionLostUpdateTests](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/Tests/ExperimentFixes/TranslationStateVersionLostUpdateTests.cs) вызывают второго писателя уже после первого, с заранее устаревшим expected version. Это достигает раннего отказа **до** обучения, но не проигрыша CAS после двух захватов одной основы. Тест «кандидат сохранён» не создаёт такого кандидата. Аналогично не доказан порядок «Evaluate захватил S0 → writer опубликовал S1 → Evaluate закончил».

**Исправление:** R117-03 сохраняет отвергнутый реальный checkpoint до возврата конфликта, возвращает его identity и основу, не применяет его к новой версии автоматически. Детерминированные барьеры проверяют поздний CAS и stale evaluation; удаление CAS должно ломать тест. Старый R01 в 135 читается с этим уточнением, а не как приказ заменить уже существующий CAS новым lock.

### F02 — отпечаток корпуса неоднозначен до вычисления SHA

**Приоритет P2.** В [CorpusStatisticsSnapshot.ComputeContentHash](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/18.CapabilityTraining/CorpusStatisticsSnapshot.cs#L310-L323) поля сцеплены неэкранированными `U+001F` и `U+001E`. Два разных документа дают одинаковые входные bytes: `Text="alpha\u001fbeta", Output="gamma"` и `Text="alpha", Output="beta\u001fgamma"` при одном ID. Это воспроизводимая неоднозначность кодирования, не коллизия SHA-256.

В текущем runtime snapshot строится рядом с неизменными строками; **подмена живого cache в этом аудите не воспроизводилась**. Ошибка относится к силе `RequireBelongsTo` как доказательства тождества, особенно при reuse. Исправление R117-04: versioned length-prefix/canonical encoding, определённые null/empty/order rules и отрицательные примеры с разделителями; не заменять это просто более длинным hash.

### F03 — P115 проверяет часть происхождения по словам самой расписки

**Приоритет P1.** [SnapshotLineageVerifier](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11501/src/BlindEvaluator.cs#L117-L205) использует `DeclaredSnapshotRevision` из receipt как revision нового пересчёта; независимой ожидаемой версии нет. `Math.Abs(declaredAverage-recomputed)>tolerance` не отвергает NaN; NaN/Infinity tolerance тоже ослабляет отказ. Цикл длин проверяет actual IDs в declared, но не лишний declared ID. Словарные DF уже проверяются в обе стороны — эту ошибку им не приписываем.

**Исправление:** R117-01 принимает доверенную ожидаемую revision отдельно, проверяет конечность/диапазон и точные keysets. Honest case сохраняет PASS; revision-only, NaN, ghost/missing doc и same-count content replacement получают отказ. Модели IEEE-754 в evidence подтверждают ветвь сравнения, не заменяют C# execution.

### F04 — P115 объявляет экзамен невиданным, хотя его вопросы участвуют в tokenizer.Train

**Приоритет P1 для научного заявления.** [Program.cs](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11501/Program.cs#L278-L360), массив TrainingAlias и сбор `trainCorpusText`: все четыре alias равны `exam_v1.Question`, затем идут в `tokenizer.Train`; записанное объяснение говорит обратное.

**Исправление без обучения:** отметить preprocessing exposure и сузить вывод до A/B parity/cost при общей подготовке. Будущая проверка независимости должна охватить tokenizer, нормировку, vocabulary, encoder fit, подбор параметров и cache, а не только итоговую KNN-таблицу. Исторический gold/сплит не переопределяется постфактум. R117-01/06.

### F05 — обязательные контроли записаны, но не управляют общей пригодностью

**Приоритет P1.** В [P115 Program](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11501/Program.cs#L640-L751) изменение frozen weights только предупреждение, пригодность задана константой. [Compare](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11501/Program.cs#L854-L910) обходит `Math.Min` двух длин; не проверяет точный QueryId/полный output, сравнивает selected ID и confidence без finite guard. Обрезанная панель или другой output того же ID способны не разрушить паритет. На реально сохранённых данных полный паритет отдельно подтверждён.

В [P117 Program](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11701/Program.cs) copyFaithful, cold, journal, reload и budget записаны, но общий статус снова константен; явно бросает только leakCheck. Runner ориентируется на process exit. Старый P114 `verifierOk` имеет тот же незакрытый остаток.

**Исправление R117-01:** список обязательных check IDs задан доверенным назначением; missing/false/unknown запрещает научный VALID и успешный итог. Отрицательный образец считается пройденным контролем лишь когда наблюдаемый отказ совпал с заранее заданным ожиданием. Каждый флаг ломается отдельно, без модели. Процесс, пригодность измерения, бюджет, гипотеза и продуктовая приёмка — отдельные поля, а не один PASS.

### F06 — общий расход не совпадает с научной панелью; P116 обучался для диагностики и после позднего сбоя

**Приоритет P1.** Фактический расход сохраняется, а не вычитается из-за названия control/calibration:

| Опыт | Уточнение по источнику и сохранённым данным |
|---|---|
| P114 | `budget-audit.json` явно пишет 16 extra forward вне назначения, но сохраняет `within_assigned_budget=true` |
| P115 | 18 основных чтений + 6 retention = 24 при назначенных 18; neural updates 0, записи уроков памяти имеют собственный счёт |
| P116 | [RunArm](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11601/Program.cs#L647-L653) сначала обучает `Train(uniquePairs)` для before-fingerprint, затем `Train(presented)`. A:15+21=36; B:12+17=29; одна физическая попытка 65 Learn вместо учтённых 38. Две сохранённые попытки дают **130 source-derived Learn**, 72 записанных Ask |
| P117 | 18 scientific + 5 implementation-control = 23 Execute; 6 + 3 = 9 TrainOnExample. Два ранних build-failure не обучались; не выдавать их за три учебных прогона |

P116 впервые упал уже после обучения из-за оформления свидетельства; повтор обучил снова. Исправление R117-02: ledger на реальных входах в Learn/Train/forward/Execute, суммирование всех physical attempts; отдельный режим saved-only, в котором недоступны учебные и модельные callback. Исторический cap не увеличивать задним числом. Утверждение «130» — восстановление по source и сохранённым картам двух попыток, не новый инструментированный замер.

### F07 — запускатели допускают ложную терминальность и неполное доказательство завершения процессов

**Приоритет P1.** В [P116 Run-Experiment.ps1](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11601/Run-Experiment.ps1#L325-L367) JSON `null` успешно разбирается; object checks пропускаются, список ошибок пуст. Итог `COMPLETED_AND_PARSED` может сосуществовать с `receipt_parsed=false` и exit0. Это точная модель ветви, PowerShell не запускался.

В [P117 Run-Experiment.ps1](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11701/Run-Experiment.ps1#L72-L129) `[bool]$o.terminal` трактует непустую строку `"false"` как true; произвольный state не связан строго с ожидаемым stage/attempt. Оба runner не дают полного наблюдения дерева: P116 перечисляет непосредственных детей, ошибка наблюдения может стать пустым списком; P117 `HasExited` доказывает только завершение root. Аудит не утверждает, что в историческом запуске реально остался живой потомок.

**Исправление R117-02:** строгая схема root object и обязательных типов, исходный expected experiment/attempt/stage/source, boolean terminal и enum state, связь exit/scientific outcome. Для дерева различать отсутствует/жив/не наблюдался; рекурсивно учитывать PID+start-time. Проверять dummy child+grandchild и неудачу наблюдения отдельно от модели. Прежние R02/P108–P113 остаются необходимыми.

### F08 — проверка швов P116 не связывает заявленный проект с исходным потоком

**Приоритет P1 для provenance.** В [BoundaryChecker](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11601/src/BoundaryChecker.cs#L24-L123) trusted `BlindStreamRecord` содержит FileId/Tokens/hash, но не project identity. `CurrentProjectId/NextProjectId` пары не доказываются потоком: изменение только проекта сохраняет вердикт. Существующие шесть файловых controls действительно работают; они не покрывают этот случай. Короткий hash со срезом `[..12]` и повтор FileId через `ToDictionary` могут дать исключение вместо отказа.

**Исправление R117-05:** ключ потока `(projectId,fileId)`, оба конца/позиции/токены и hash проверяются против bytes этого ключа. Честный поток проходит, проектная перестановка с прежними токенами/количеством и malformed/duplicate input отвергаются объяснимо. Не выдавать isolated CountMemory за воспроизведение всего native E43: у исходного E43 replay старого корпуса, у P116 — ownPairs.

### F09 — P117 сохраняет ожидаемый журнал KNN вместо состояния настоящего хранилища

**Приоритет P1.** [ExpectedKnnWrites](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11701/Program.cs#L725-L750) в [P117 Program](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11701/Program.cs) конструирует ожидаемые девять записей из уроков. С живым KNN сравнивается только Count; потом импортируется этот журнал. Его одинаковый hash доказывает журнал, а не фактические source/target/signature, порядок и кратности live-store по его существующей схеме.

Реальные executor maps действительно экспортированы, их hash и сохранённые ответы после import совпадают; этот полезный факт не отбрасывается. Объяснение «internal export недоступен из отдельной сборки» неверно для окончательного [Experiment.csproj](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/experiments-v2/RP01-MICRO/P11701/Experiment.csproj#L33-L43): native `.cs` включены `Compile Include` в ту же сборку. Существующие internal `ExportCheckpoint/ImportCheckpoint` в [contracts](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/27.TranslationDomain/TranslationUnitContracts.cs#L410-L424) и [trained units](https://github.com/lubochka/llm_mvp_core/blob/879a07bcd4dd747b3672f9ab32492bd23d14b5ea/27.TranslationDomain/TranslationTrainedUnits.cs#L549-L590) доступны такому host по структуре source. Новую успешную сборку этим не заявляем.

**Исправление R117-06:** использовать actual export/import, journal оставить oracle; same-count corrupt target/signature должен ломать сверку. Если actual snapshot исторически не сохранён, честно `NOT_OBSERVED`; его нельзя выдумать из уроков или «восстановить доказательство» новым Train. Проверка полного TranslationMachine через project reference остаётся отдельной будущей product acceptance.

### F10 — локальный выигрыш смешивается с более сильным причинным или смысловым выводом

**Приоритет P2.** P115 report называет IDF cedar нулевым при df=N=4, хотя формула даёт `log(10/9)=0.10536051565782635`. Общий нормализованный вклад не различает кандидатов, но численно не нулевой. P116 один seed на списках длиной15/12 выбирает разные законные replay, поэтому +1 control нельзя приписать только швам. P117 перевод `the violet lantern glows` не передаёт `now`; fallback копией русского текста не является typed отказом. Правильный кандидат стоит первым; одно попадание не доказывает защиту от FIFO, всех коллизий или всех направлений.

**Исправление R117-05/06:** внести датированное объяснение рядом с историческим отчётом, сохранить raw. Для будущего причинного назначения зафиксировать replay по стабильному ID пары; качество полного ответа, lawful refusal, FIFO и same-cell distractor проверять отдельными собственными controls. Не повышать старую оценку и не менять gold, чтобы назвать полезный механизм предметным успехом.

### F11 — очередь закрыта, а карточки P116/P117 всё ещё предлагают первоначальный запуск

**Приоритет P1 для следующего исполнителя.** На исходном SHA Q117 уже `RESULT_RECORDED`; 130 строк, 117 записанных и 13 спроектированных, первая незаписанная Q118. Однако паспорта [P116](../experimets/cards/P116-e43-file-boundary.md) и [P117](../experimets/cards/P117-translation-direction-memory.md) ещё имеют `DESIGNED_NOT_RUN / SHALL_IMPLEMENT` и активный первичный work order. Верхнее предупреждение не устраняет противоречие внутри копируемого задания.

**Документное исправление выполнено этим пакетом:** карточки переводятся в recorded/saved-evidence repair; действующие prompt и index уточнены. Очерёдность, budgets, gold и номера сохранены. Q118 — навигация по уже порученной программе, не разрешение нового запуска из аудита. Подробный адресный охват — 140.

## 5. Незакрытые прежние находки, проверенные адресно

| Прежний владелец | Что осталось | Что нельзя ошибочно объявить отсутствующим |
|---|---|---|
| 135/R02, P108–P113 runners | P108 build может оставить общий successful terminal; P109–P113 повторный вход по существованию exit.json возвращает0 без строгой проверки прошлого результата | У P114 repeat-exit4; не распространять чужой дефект на него |
| 135/R03, P110 NumericOutputVerifier | Не-конечные actual/expected/tolerance и границы допуска | Нужна исполняемая численная проверка, а не один список значений |
| 135/R05, P111 AddressingVerifier | After owner keyset/лишние владельцы не доказаны одним Before/count | Before owner checks уже есть |
| 135/R04, P112 verifier и trainer | Эксперимент доверяет claimed hash; product validated material ещё должен быть связан с фактически потреблёнными DPO/text/KNN рядами | Product guard e16987c существует; legacy material без lineage не превращать в blanket запрет Train, а помечать соответствующую научную ось NOT_EVALUATED |
| 135/R06, P113 channels | Полный вектор/поканальная энергия и точные fractions не восстанавливаются только из объявленных scalar | Проверка конечности в P113 уже есть |
| 135/R08, P114 copy | All-zero copy всё ещё доходит до optimizer; verifier и общий terminal требуют ремонта; extra forward и hard deadline | Product DpoTrainer уже исключает all-zero; старая копия и продукт — разные владельцы |
| 124/S03, ProjectWiki routing | `HasPassedEvaluationGate` доверяет порогам evidence и не защищён полным finite/range/local-policy контрактом | Не отменять уже реализованные lawful positive пути |
| 124/S06, OrchestrationPlanMachine | Import переносит PassesGate и частично мутирует поля до окончания импорта; восстановленный gate должен быть привязан к локальной политике и состоянию | В `CreateImportedEvaluationGate` finite checks уже есть; проблема не в их отсутствии |
| 124/S14, BpeTokenizer | `DecodeToBytes` молча пропускает неизвестные token IDs | Новый UTF-8 suffix guard решает другую, более узкую задачу |

Этот перечень ограничен просмотренными ветвями. Он не закрывает остальные исторические R/S и не создаёт новый приоритет выше прямого задания Любы.

## 6. Решение о переносах

**P114:** механизм исключения нуля в продукте написан; принять его через собственные active/all-zero/optimizer/ordinary-consumer проверки, не повторяя PRIMARY. **P115:** snapshot уже написан; исправить identity framing и принять реальную invalidation/cost границу, сохранив скалярную формулу. **P116:** научно обоснован запрет ложных межфайловых пар, но перенос обязан использовать фактические project/file bytes в native E43 и собственный receipt. **P117:** перенос расширения RU→EN имеет доказанную пользу для достижимости, но ещё не для точного перевода, всех направлений и настоящего KNN restore.

Полная цепочка приёмки для каждого: вход обычного caller → изменяемая единица → опубликованное обученное состояние → обычный reader → восстановленная новая машина. Контроль, который обходит хотя бы одно названное звено, закрывает только свою локальную часть.

## 7. Открытие следующей работы

Для приёмки соответствующих экспериментов нужны R117-01/02: ремонт чистых проверяющих, схем и saved-only финализации. Общая очередь сохраняет первым предметным владельцемQ01; разделыR117 не создают конкурирующий порядок. Он не требует новой научной траектории. R117-03/04 закрепляют продуктовые остатки CAS/cache. R117-05/06 отделяют фактические свидетельства от недоказанных частей P116/P117 и готовят отдельную приёмку. Нативный запуск закрывается только реальным результатом на exact source; данный документ и наличие будущих тестов его не заменяют.


## 8. Публикационная дельта c257523: не назначать уже выполненную часть R04

Дельта проверена по пяти исходным файлам, без native run. В DefaultLogicalCapabilityTrainer actual finalPreferences и combinedTrainingText проходят через AdmittedTrainingMaterial.Admit **до первого AcquireMutationPermit**; оба потребителя получают один carrier. Новый binding ищет Content замороженных non-Train строк в фактических Prompt/Chosen/Rejected/общем тексте; receipt действительно читается trace. Null provider/material сохраняет прежний путь NOT_EVALUATED_NO_LINEAGE. Это реальное усиление и частичное исполнение R04.

Открыты: KNN corpus вне этого admission, положительное покрытие всех consumed bytes разрешённым TRAIN, перенос receipt в durable training result/checkpoint, старый экспериментальный P112 checker и native интеграционная проверка. Шесть новых tests вызывают Admit напрямую и остаются UNRUN; helper соединяет combined text переводом строки, а product — пробелом, поэтому helper не доказывает точный production materializer.

**PUB01, P2 — та же ошибка encoding в новом digest.** ComputeConsumedDigest соединяет поля newline. При одинаковых combined/rejected пары Prompt="alpha\nbeta", Chosen="gamma" и Prompt="alpha", Chosen="beta\ngamma" имеют разные поля, но одинаковые serialized bytes и hash. Новый heldout-filter этим примером не обойдён: он отдельно читает реальные strings. Исправление включено в R117-04 вместе с F02, с явной областью text-only fingerprint.

**PUB02, P2 — обещание полной независимости carrier сильнее его shallow-copy.** В preferences копируется внешний массив, но TeachingVector — mutable float[]. Caller и выданная copy сохраняют ссылку на этот лист; текущий тест меняет только array slot. Upstream SnapshotPreferences настоящего trainer уже глубоко копирует vector, поэтому подмена текущего runtime этим аудитом не доказана. Исправление R117-01: carrier владеет mutable leaves и выдаёт отдельные копии либо честно сужает свой контракт; для выбранного в139 полного владения — два leaf-mutation negative и прежний positive.

Кроме того, поиск всех non-Train Content строже пользовательской политики с разрешённым Dev-overlap. У штатных профилей оба forbid-флага true, наблюдённой регрессии штатного профиля нет. До широкого применения следует согласовать binding с объявленными флагами; это не основание запретить legacy обучение.

Первоисточники, границы и точные строки: [публикационная сверка](../../../../experiments-v2/_audits/2026-11-03-post117-audit/publication-c257-reconciliation.md). [Python witness](../../../../experiments-v2/_audits/2026-11-03-post117-audit/recount-consumed-encoding.py) и [его результат](../../../../experiments-v2/_audits/2026-11-03-post117-audit/consumed-encoding-witness.json) показывают encoding и модель alias; это не C# execution.


## 9. Последняя сверенная дельта918765f и защита от повторного P118

Проверены две изменённые продуктовые ветви, девять новых source tests и только status/outcome P118. **Не выполнен полный аудит Program/runner/verifier/raw P118.** Source/results основного879a07b не перезаписаны. Точные первоисточники и пределы — [публикационная сверка918](../../../../experiments-v2/_audits/2026-11-03-post117-audit/publication-918-reconciliation.md).

**S03 уже исправлен в части source:** обычный routing helper проверяет finite трёх наблюдений, совпадение сохранённых порогов с доверенными0.95/1/.85, сравнивает с этими константами; false persisted flag запрещает проход. **Остаток:** верхние границы долей и связь evidence с фактически оценённым snapshot/version. Finite значения2/2/2 проходят именно gate при правильных thresholds: это source-контрпример, не выполненный плохой запрос. Результат обычного Selected/CanStartTargetRunner и полная матрица чисел/порогов ещё требуют приёмки.

**S06 уже исправлен в части source:** общий валидатор до mutation на обеих import-дорогах проверяет finite/[0,1]/.75 и отвергает противоречащий passed checkpoint. Честный низкий результат разрешён как непроверенное состояние; не требовать его полного запрета. **Остаток:** legacy unknown count отличается от измеренного нуля явно; file bytes → LoadCheckpointAsync и сохранность обученных maps/обычных ответов при отказе должны быть проверены собственным native срезом. Девять новых tests UNRUN; сравнение холодного export summary не доказывает эту последнюю цепь.

**P118 уже имеет записанную внешнюю попытку.** Status заявляет COMPLETED/VALID_FOR_NAMED_UNIT и8 presentations/10 scientific GeneratePlanAsync; outcome имеет terminal=true и build/run/exit0. Это наблюдаемые заявления файлов, не независимо подтверждённые научная пригодность/расход/польза. На этом pin report.md не найден. Q118 получает RESULT_RECORDED_EXTERNAL_UNREVIEWED и current saved-evidence prompt. Исходные уроки, gold, budget и команды остаются в историческом паспорте; новый PRIMARY ради отчёта не назначается. Первая незаписанная строка теперьQ119, только навигация. Исторические117/13 на879a07b остаются верны для своего среза.

## 10. Согласование публикации84f

Публикационное согласование84f: внешний реестр переписан владельцем в114самостоятельных работ с единой очередьюQ01–Q66; эта структура сохранена, R117 нанесены как уточнения существующих владельцев. Внешняя строка результатаP118 сохранена; report уже опубликован. Из84f проверены diff путей, пересекающиеся документы и наличие report, не новый ClaimCorrectionReader и не научная реализацияP118. Основные проверенные владельцыF01–F10 не изменены дельтой918→84f. [Отчёт P118](https://github.com/lubochka/llm_mvp_core/blob/84fcd8e43550d4fc23ed83c41f304c4d4b7a761d/experiments-v2/RP01-MICRO/P11801/results/attempt-0001/report.md) имеет Git blob7977da387f591cfc23f06dbc3ef707ebc3cff46c; прочитано начало для подтверждения наличия, полный scientific review не заявлен. Историческое отсутствие на918 не означает отсутствия в текущей ветке.
