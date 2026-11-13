# После local merge: реальный перенос, пригодность приёмки и сохранённые доказательства

**2 ноября 2026.** Первоначально проверенный core: `3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e`; исходный planning: `568758eaa5f86c4df5a4fb787c221b96e0eaa7ca`. Этот проход продолжает [POST107](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) и проверяет новый объединённый срез. Он не объявляет ранее выполненное усиление новым результатом и не запускает обучение.

**Главный вывод:** в объединённой ветке есть полезный рабочий код и сохранённые проверки, но сам merge не прошёл целостную приёмку. В действующих инструкциях остались конфликт-маркеры. Добавленные тесты местами проверяют ещё не реализованный ремонт, а два новых критерия способны закрепить неверный договор: отдельный байт обязан быть целым символом; поведение заново обученной третьей единицы якобы доказывает восстановление импортированной памяти.

Подробный core-пакет: [126 — аудит](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-126-postmerge-transfer-and-experiment-audit-2026-11-02.md), [127 — адресный ремонт](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md), [128 — охват карточек](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-128-postmerge-card-coverage-2026-11-02.md).

## Дополнение при изменении ветки: S01 проверен на735fa919

Пока выполнялся аудит, core продвинулся до `735fa919d953e3eae636ac3c40ac5ff7c0c8d9da`. Его ремонт документов сохранён и согласован с новыми карточками; первоначальные179/795 не выдаются за единолично сделанную позднюю правку. Отдельно проверен [новый оценщикP104](https://github.com/lubochka/llm_mvp_core/blob/735fa919d953e3eae636ac3c40ac5ff7c0c8d9da/experiments-v2/RP01-MICRO/P10401/NegativeControls.cs). Четыре прежних ложных negative исправлены, пять pure условий дали шесть различающих пар и два красных контроля в saved replay. Выполненная частьS01 зачтена.

Остаток конкретен: при неизменном expected проходят пять копийE1 вместоE1–E5, подменаrank1/ответаE3 даннымиE1 и winner`NOT-IN-POOL` при правильной заявленной схеме. Нужны уникальные RowId и заранее закреплённая связь с ожидаемым уроком, а также членство и порядок победителя. Contract hash не заменяет implementation hash; остальные16 условий не аттестуются вычитанием. Повторяемый Python-пересчёт дал58 наблюдений без неожиданных расхождений,14 source blobs; нативного запуска или обучения этим аудитом не было.

Автор нового ремонта отдельно сообщил случайное обучение при назначенномTrain0:1Train,10чтений,5холодных наблюдений,12вызовов узлов. Полный расход по raw независимо не пересчитан. План127 требует отдельной поправки к отчёту и отказа неизвестногоCLI ключа доTrain; исходная история не переписывается. Полное доказательство, ограничения и адресный остаток — в [126](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-126-postmerge-transfer-and-experiment-audit-2026-11-02.md) и [127](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md).

## 1. Сначала — однозначные действующие инструкции

В [40-effective-executor-context](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md#L3-L48) опубликованы вложенные маркеры конфликтов и два несовместимых абзаца о полноте чтения. Это новый дефект объединённого документа. Предыдущая проверка родительского коммита не подтверждает качество результата merge.

Ремонт сохраняет полезные уточнения обеих сторон, исторические данные, фактические критерии и текущую пользовательскую границу. Удаляются маркеры, дублирование и противоречащая действующая инструкция. Приёмка проверяет конечные байты всех изменённых договоров/индексов и их ссылок. Контрольная порча, возвращающая конфликт-маркер в копию входа, должна давать отказ проверки документа; это не требует модели или Train.

Оба прежде недоступных файла 120 теперь опубликованы: [договор](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md) и [матрица охвата](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-coverage-2026-11-02.md). В planning README заменены две ссылки `C:/Projects/...` на эти доступные repository targets. Прежнее сообщение об отсутствии файлов остаётся верным для своего исторического среза.

## 2. Приёмка декодера должна сохранять возможность русского ответа

Новый [AnswerReadingRejectsIncompleteCharacterIdentifiersTests](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/Tests/ExperimentFixes/AnswerReadingRejectsIncompleteCharacterIdentifiersTests.cs#L144-L217) требует обратимости каждого отдельного ID: `Encode(Decode([id])) == [id]`. На байтовом словаре 256 это оставляет 128 ASCII-байтов. Проверка иных BPE-токенов за 127 не решает проблему: русский символ может законно состоять из нескольких отдельных byte-token.

Простой контрпример: «ж» имеет UTF-8 bytes `[208,182]`. Первый байт является началом символа, второй — его продолжением; оба по отдельности незавершённы, последовательность целиком законна. Отбрасывание обоих уничтожает допустимый ответ.

Перенос должен различать допустимый префикс, незаконную последовательность и неполный terminal result. Положительные проверки включают кириллицу в byte-token и BPE-merge; отрицательные — continuation без начала, unknown ID, остановку после неполного символа. Чтение проходит через обычный generator с настоящим tokenizer и накопленным префиксом. Узкий односимвольный результат P101 сохраняется; он не даёт основания запретить фрагменты символов во всех генеративных путях.

## 3. Восстановленный target должен сам ответить на запрос

В [новом PAIR-тесте](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/Tests/ExperimentFixes/TranslationPairTailSignatureLearningTests.cs#L253-L283) есть импорт в target и проверка его повторного экспорта. Но окончательный `Tokenize` принадлежит третьему `SemanticTokenizerUnit`, которому снова дали урок. Этот ответ не подтверждает использование памяти target.

Приёмка сохраняет отдельно проверку данных и поведения. После импорта target получает запрос через свой обычный reader без нового Train. Контроль, сохраняющий re-export, но отключающий использование импортированной correction, обязан ломать behavioral gate. Проверяется полный ответ с verdict, а не только отсутствие хвоста в Signature.

В том же [тестовом файле](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/Tests/ExperimentFixes/TranslationPairTailSignatureLearningTests.cs#L29-L43) предложено сохранять PAIR в рабочих ключах и срезать только на выходе. Это противоречит уже принятому направлению S04 плана 124: provenance отдельно, единая версия writer/tokenizer/executor/verifier/checkpoint. Ожидание теста нужно согласовать с этим решением. RULE после PAIR и конфликты миграции остаются обязательными случаями.

## 4. P66: наблюдаемый источник и полный счёт измерений

Основной результат P66 о видимости контрастов не отменяется. Два уточнения относятся к свидетельствам его исполнения.

**Финальный адаптер не показывает, вызывался ли поиск.** [OrdinaryFlow](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/experiments-v2/RP01-MICRO/P6601/Program.cs#L841-L855) ставит `KNN_NOT_REACHED`, когда итоговый AdapterId не содержит `-knn`. Но [ExecutorUnit](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/27.TranslationDomain/TranslationTrainedUnits.cs#L288-L303) после неуспешного поиска может продолжиться запасным путём. Корректное наблюдение — «KNN не выбран». Был ли он вызван на каждой из шести исторических строк, без отдельной трассы не восстановлено.

Будущие receipts разделяют `search_attempted`, число найденных/допущенных кандидатов, выбранный lesson ID и окончательный источник. Два разных контроля — ранний exact ответ и достигнутый, но отклонивший всех соседей каскад — не должны иметь одинаковое описание.

**Счёт 12 не охватывает все прямые вызовы меры похожести.** По исходнику и сохранённым размерам:

| Участок | Прямые вызовы `ComputeTokenOverlap` |
|---|---:|
| Допуск 6 строк против 2 уроков |12|
| Таблица 12 пар, два обращения на каждую |24|
| Ручной контроль |2|
| Граница 0.70 |1|
| Итого прямых вызовов |39|

Основания: [admission seam](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/experiments-v2/RP01-MICRO/P6601/src/ExamAdmissionSeam.cs#L87-L92), [таблица и контроли](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/experiments-v2/RP01-MICRO/P6601/Program.cs#L153-L207), [записанный расход](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/experiments-v2/RP01-MICRO/P6601/results/attempt-0001/metrics.json#L682-L713). Это статический счёт прямых обращений, **не новый runtime measurement и не весь вложенный расход обучения/Flow**. Уникальные пары, применения политик и физические вызовы указываются раздельно. Будущий budget gate читает счётчик на callable; лишний вызов должен изменить его. Исправление старого отчёта выполняется без повторного обучения.

## 5. Реализация, подготовленный тест и широкая приёмка

Файлы тестов для PAIR, отката Translation и чтения байтов опубликованы. Их наличие не означает, что соответствующий ремонт production source завершён. Для каждой строки переноса сохраняются раздельно `source_implemented`, `component_verified` и `native_acceptance`, с точными версиями и журналами.

В частности, исторические журналы от 30 октября 2026 для защиты памяти `RouteClassifier` содержат узкий 90/90, широкий consumer прогон 225/226 и повтор одного неуспешного child readiness test. Это сохранённые свидетельства, подробно разобранные в core 126, а не новые запуски на merge от 2 ноября. Они не суммируются и не заменяются узким зелёным результатом. Причина consumer failure должна быть установлена; одного неуспеха недостаточно, чтобы объявить его регрессией нового алгоритма.

## 6. Какие исходные карточки усилены

| Карточка | Конкретный критерий следующего исполнения |
|---|---|
|[04 — гибридный поиск](04-KNN-HYBRID-RETRIEVAL.md)|Отдельные попытка, допуск, выбор и финальный источник; reached-but-rejected контроль|
|[05 — семантика/vector/KNN](05-SEMANTIC-VECTOR-KNN.md)|Единая версия ключа вместо одностороннего PAIR strip; RULE и происхождение сохранены|
|[06 — checkpoint](06-CHECKPOINT-RESTORE-ADOPTION.md)|После импорта читает target без Train; отказ обнаруживает подмену reader|
|[10 — inference](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md)|Unicode всей последовательности, законный префикс и terminal completeness|
|[12 — provenance переноса](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md)|Счёт callable отдельно от числа пар; test addition отдельно от source repair|
|[17 — structured dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md)|Поздняя ошибка известного Unit сохраняет прежнее состояние до внешнего restore|
|[28 — tokenizer→executor](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md)|Совместимые writer/reader/verifier/loader; полный перевод после миграции|
|[30 — routing/threshold](30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md)|Точный test selection и видимый незакрытый ordinary consumer|

Это восемь адресных уточнений, один навигационный файл и данный новый аудит. Исходные тела карточек сохранены; README отдельно меняет два недоступных локальных href. Исторические raw, научные выводы, TRAIN/EXAM, бюджет и порядок очереди этим проходом не переписаны. Нативные .NET/PowerShell тесты и новое обучение в этом аудите не запускались. Полная научная проверка всех карточек программы не заявляется.

## Дополнение066725d8: записанный P108 и повторение ошибки итоговой приёмки

К моменту завершения аудита опубликован [P108](https://github.com/lubochka/llm_mvp_core/blob/066725d8f3e92d692a95f5566f353ce9053f4095/experiments-v2/RP01-MICRO/P10801/results/attempt-0001/report.md). Доставка опознаний в признаки улучшилась4/12→12/12, но выбор маршрута и полные Steps остались12/12 уA/B: прирост точности не измерен. Результат относится к одной живой KnnLogicalUnit; новый ordinary Flow consumer и fresh Load не доказаны. Q108 теперь RESULT_RECORDED с ограничениями, текущая очередь108результатов/22будущих,NEXT109; это наличие результата, а не общий PASS.

В текущих raw контроли порчи и независимости от порядкаtrue. Однако ихfalse не закрывает финальныеexit0/SUPPORTED: повторяется семейство дефектаP107, несмотря на уже записанное требование обязательной технической приёмки. M06-P108 в127 назначает один действующий finalizer и saved-outcome отрицательные проверки. Запускатель должен исполнять deadline; одного сравнения elapsed после завершения недостаточно.

Успешный физический запуск содержит40учтённыхнаблюдений при назначенных38, включая4холодныхNO_UNIT: это не40nativeExecute. За две физические попытки сохранены2Train/16предъявлений и64наблюдения, а не один оплаченный запуск. Именаattempt-0001 и фраза «доза на попытку не менялась» не отменяют общего расхода. Счёт nativeExecute отдельно от достиженияSearch:21из22в успешном запуске,41из42заоба — вывод из исходника и сохранённого отказа доSearch, не инструментированный runtime counter. Исторические данные сохранены; новые Python проверки не вызывают обучение.
