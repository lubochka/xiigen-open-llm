# Исполнение переносов и точность P93–P101 — 2 ноября 2026

<!-- POST102-AUDIT-2026-11-02:START -->
## Граница исторического среза

Новые commits P102, MethodKnn distance tiebreak и публикация Q127–Q130 проверены в [POST102-аудите](2026-11-02-POST102-TRANSFER-AND-EXECUTOR-AUDIT.md). Текущие NEXTQ104 и103/27/130 находятся там; прежние показатели и выводы ниже сохраняют свою историческую область. Это дополнение не запускает опыты и не подменяет отсутствующую native приёмку.
<!-- POST102-AUDIT-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход перед новым аудитом или заданием

Обязателен [договор120](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

До действий прочитать выбранную карточку целиком и её локальную секцию120, source/raw и фактический маршрут producer→ordinary caller→learner→consumer; local HEAD, published SHA и dirty bytes различаются. Списки навыков ниже — дополнительные, не освобождение от полного свежего стека120. Исторические Tier3/vocab/split/UI/Git/model правила не расширяют нынешний паспорт и авторизацию.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Missing API/filter/данные вернуть автору как точный HOLD, не выбирать замену. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.

**Сохранение постановки:** exact recorded result не повторять, old NOT_RUN не выше текущей Q-строки. Полный outer/nested бюджет и независимый gold остаются у карточки; пересчёт raw/структурный guard сTrain0 не заменяет обязательный учебный цикл и не получает фиктивный Train. Команды, API и фильтры не додумывать исполнителю. Вход обязателен также в передаваемом промпте выбранного ID.
<!-- EXECUTOR-CONTEXT-120:END -->

**Связь с прежним аудитом:** опубликованный119 уже усилил42 карточки. [Матрица120](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-coverage-2026-11-02.md) фиксирует новый охват живых инструкций, не научную аттестацию и не новый результат исполнения.


## Текущий результат

На core `3194310941c428ef9a5e643c4dc9d7ad7d15447e` часть назначенных переносов уже исполнена: P89 UnscaledLoss имеет два обычных читателя, HNSW проверяет межуровневые связи, run-binding сверяет optional/extra, BPE сообщает фактическую ёмкость, confidence flag назван по назначению, а EOS больше не выдумывается. Их нужно принимать в точной границе, а не повторно реализовывать.

Открытые задачи: обычный Decode скрывает неизвестный ID; Wiki Final содержит среднее по траектории и участвует в двух publication gates; RF01 допускает поздний отказ после записи части партии; RF06 фильтрует/ограничивает поиск до полного допуска читателя; RF02 ещё не связывает immutable consumption и evaluated project instances. Кандидат ASCII-фильтра P101 не переносится глобально: он исключает многобайтовые русские/ивритские/emoji-ответы. Общий default strict-K по P100 тоже не принят.

Полные владельцы, source citations, отрицательные/положительные witnesses, решения и зависимости:

- [117 — аудит переносов и девяти опытов](https://github.com/lubochka/llm_mvp_core/blob/ecd0642d20f726adbc50606680621662df5dc93a/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/117-post101-transfer-and-experiment-audit-2026-11-02.md).
- [118 — конкретный план ремонта](https://github.com/lubochka/llm_mvp_core/blob/ecd0642d20f726adbc50606680621662df5dc93a/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/118-post101-addressed-repair-plan-2026-11-02.md).
- [119 —42 адресно усиленные карточки](https://github.com/lubochka/llm_mvp_core/blob/ecd0642d20f726adbc50606680621662df5dc93a/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/119-post101-card-hardening-and-coverage-2026-11-02.md).
- [Исходники, пересчёт и фактические проверки](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-02-post101-transfer-audit/README.md).

## Что показала сверка экспериментов

| Опыт | Подтверждённый результат | Ограничение |
|---|---|---|
| P93 |12/12 правильных,4 low-confidence warnings | PAV fits0, калибровка ошибок не измерена; empirical success rate определён и равен1 |
| P94 | Полные множества3/5→5/5 | Experimental first-ref adapter, а не доказанный текущий product defect; simple rule тоже5/5 |
| P95 | Исполненный Retry/Stop2/4→4/4 | Exam structural keys совпадают сTrain; simple rule равенB |
| P96 | Все схемы12/12,24started=24awaited, lost0 | Синхронный KNN workload; async I/O, cancellation/quota и JIT-причинность не проверены |
| P97 | Sink5/10→10/10; illegal graph2refusals | Память порядка10/10 уже вA; simple rule равенB; restore подслучай не исполнен |
| P98 |629/2048requested→одинаковые336actual,80merges | Corpus upper bound393; фактическое сравнение ёмкостей не состоялось |
| P99 |16legal сохранены,8unknown ID скрытыA/отклоненыB | Выполнен изменённый BPE scope; исходный neural segment не исполнялся, позднее основание записано |
| P100 |4updates;70tie-controls подтверждают ID bias | На4learned readouts boundary ties0; вероятность около46% не наблюдаемая частота |
| P101 |100copied updates400→500; TRAIN1/2→2/2, EXAM1/2→0/2 |2/4→2/4 скрываетT2 gain/E2 loss; native Step отказал; objective parity не update parity |

В P101 product.Loss совпадает с copy.UnscaledLoss во всех5 checkpoint; старый false-verdict возник при сравнении с weighted loss. Причина optimizer dynamics из endpoint distance не доказана. В объяснении mixed-signal P89 исправлено40%→20%; сама новая формула product корректна. Historical raw не изменён, новый вывод связан с его источником.

## Изменения карточек и продолжения

Изменены42 core experiment cards, из них33 помимо P93–P101. Общие правила внесены в02/03/04/90 и экспериментальный39: RowId/gold и gained/lost, typed nested costs, scope revision, одинаковые шкалы/временные точки, реальный consumer, simple baseline, UTF-8 prefix/terminal. Каждый адресный блок имеет WRONG/CORRECT, check ID и приёмку; прежний текст сохранён.

Девять строкQ093–Q101 больше не DESIGNED_NOT_RUN. На закреплённом срезе **101 RESULT_RECORDED /25 DESIGNED из126 Q**, следующий **Q102/P102-PRIMARY-01**. Recorded означает наличие результата со своей квалификацией, не101 научный успех. Первые92 статуса унаследованы из116, не заново полностью аттестованы. Единственная очередь — [core execution index](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md). Этот planning-документ не создаёт отдельного запуска, дозы или повторного обучения.

## Что действительно проверено

Current planning source — `6ed289b26ce198e16890fe4c7dd6b0ae55f885d9`. Прочитаны текущие navigation/audit boundaries этой ветки и связаны с actual core code/results. Старые локальные Q127–Q130 и исторические отдельные reports не считаются опубликованными новыми core assignments. Аудит не объявляет все568 элементов possibleDifs или все271 карточку независимо научно проверенными.

Новые работы этой сессии: source review, Python-пересчёт saved JSON/арифметики,42 адресных правки и согласование документов. Native .NET/PowerShell runs, inference и Train в аудите —0. Сохранённые авторские runs и написанные tests обозначены отдельно. Бинарные checkpoints существуют удалённо, но payload не десериализован здесь; отсутствие в локальном text snapshot не означает потерю модели. Независимый участник `/root/audit_method` вернул отдельную проверку, а не фиктивное число циклов.

Исторический planning README и предыдущие аудиты сохраняют свои даты/источники; их прежние «следующий» и OPEN/DONE не конкурируют с этим дополнением. Не копировать новую очередь в ещё один самостоятельный executor prompt: читать текущий canonical index и прямые указания владельца.

## Поздняя публикационная сверка

Core продвинулся на`03714d8f8d220d8653396d0c976fd0d076759afd` с двумя oracle-файлами и append реестра, без product changes/новогоP102. Оба oracle и весь новый текст сохранены при публикации аудита. Подтверждён дополнительный risk: mask не достигает greedy argmax; отдельная overload может не быть связана с ordinary generator. Single-token roundtrip learned BPE тоже не заменяет общий UTF-8-prefix contract. [Публикационное дополнение](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-02-post101-transfer-audit/publication-reconciliation.md) описывает exact source и ограничения;101/25/NEXTQ102 не меняются.
