# После P89: проверка исполнения и опубликованных границ переноса

<!-- POST102-AUDIT-2026-11-02:START -->
## Граница исторического среза

Новые commits P102, MethodKnn distance tiebreak и публикация Q127–Q130 проверены в [POST102-аудите](2026-11-02-POST102-TRANSFER-AND-EXECUTOR-AUDIT.md). Текущие NEXTQ104 и103/27/130 находятся там; прежние показатели и выводы ниже сохраняют свою историческую область. Это дополнение не запускает опыты и не подменяет отсутствующую native приёмку.
<!-- POST102-AUDIT-2026-11-02:END -->


<!-- POST101-AUDIT-2026-11-02:START -->
## Исторический срез и новое продолжение

Этот аудит сохраняется как свидетельство01.11. Его актуальные на тот день92recorded/34design иQ093 не являются текущим NEXT. Проверка02.11 и статус уже исполненных частей —[2026-11-02-POST101-TRANSFER-AND-EXPERIMENT-AUDIT.md](2026-11-02-POST101-TRANSFER-AND-EXPERIMENT-AUDIT.md); актуальный адресный repair —[core118](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/118-post101-addressed-repair-plan-2026-11-02.md). Новые результатыP93–P101 и42 усиленные карточки не переписывают первичные данные этого документа.
<!-- POST101-AUDIT-2026-11-02:END -->


## Поздняя сверка: учтены P90–P92, текущий NEXT — Q093

Публикационная база core630b2e797aedddd960d8b614386f57cc950a8526 включает поправку P89, empty/null и zero-overlap отказы RouteClassifier29, проверку EfConstruction HNSW и P90–P92. Planning1384e815 с дополнением прежнего POST88 сохранён целиком. Теперь исправлены 45 строк Q по 59 свидетельствам;92 recorded-family,34 DESIGNED,126 Q; NEXT Q093/P93-PRIMARY-01. Исходные 89/37 ниже относятся кc4 этапу, не к актуальной навигации.

Адресно усилены 20 карточек: прежние 18 этого прохода плюс P37 и P90. УP90 устранено смешение PRIMARY R-CODE и исторического D07JWTgold; данные 6/12,9/12,8/12 прочитаны поarms/gold. Extra router requests должны быть 18=42−24, не 14; rawне переписывается. comparison.json —LFS pointer, payloadне прочитан. У RouteClassifier различены directAPIиordinary21→23→12→29, fixed7seedlessonsиархитектурная learnedrouting, пустой и непустой unsupportedпрофиль. Непустой zero-overlap теперь отвергается source guard; точный CODE_Q ещё проигрывает далёкому большинству. Обычный native вызов здесь не запускался. Подробности — актуальные 116/114 иevidenceпакет.


Дата: 1 ноября 2026. Источники: core `c4d05a27963fc3edb59f5f75bb097a1fc9936654`, planning `66c3f370623cdc4a356f206717b21b6f1641ea55`. Это дополнение к предыдущему POST88 и четырём новым локальным исследованиям. Продуктовые исходники и сохранённые научные результаты не изменялись; повторного обучения нет.

**Канонический результат:** [аудит 116](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/116-post89-execution-boundary-audit-2026-11-01.md), [уточнённый план 114](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/114-post88-addressed-repair-plan-2026-11-01.md), [20 дополненных карточек по 115](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/115-post88-card-hardening-and-coverage-2026-11-01.md), [пакет свидетельств](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/experiments-v2/_audits/2026-11-01-post89-execution-boundary-audit).

## Что исправлено документами

Опубликованный core index содержал 126 Q,47recorded-family и 79DESIGNED. Q048–Q089 оставались NOT_RUN, хотя их результаты опубликованы. Вход индекса направлял к Q047, README/continuation prompt — к Q032. Итоговые 45 строк Q048–Q092 и три входа согласованы с59 сохранёнными status/completion/run-receipt/exit файлами, включая P90–P92. Теперь **92 recorded-family, 34 DESIGNED, 126 Q; следующий Q093/P93-PRIMARY-01**. Промежуточные 42/54 и 89/37 относятся к этапу до публикацииP90.

Это счёт имеющихся результатов, включая partial, invalid и negative. Первые 47 статусов не переаттестовывались; новый проход уточнил 45 строк. P48 остаётся INVALID_ZERO_TO_ZERO; P52 имеетexit9; P82 сохраняет четыре attempts, P86 — ранний failed и последующий completed. Исправления расхода P85/P87 из POST88 остаются отдельными correction receipts. Старые scientific raw сохранены.

Q125 — **P25-LEARNED-RESTORE-02**, Q126 — **SAVED-STATE-PANEL-LEARNED-02**. Исторический aliasP125 не означает Q125. Научный NEXT не разрешает запускать опыт в ходе аудита.

P91 подтверждает exact pair identity A5/B6 против baseline6, но2/2 при k1 равно постоянному потолку; при k5 обе стороны0/2. P92 подтверждает selectedUnitId0/12→12/12 через нужный factory argument, при сохранности5/8 и экзамене3/4. Его20 Embed — вывод по коду, не инструментальный счётчик; причинность writer/reader mismatch отдельно не измерена. Новых запусков нет.

## Что добавила проверка реального кода

| Граница | Наблюдение и следующее действие |
|---|---|
| RF02, входы перед запуском | Реальный Invoke-BoundRun на вручную собранном входе фазы 6 пропускает changed extra, changed optional и newly appeared optional; реальный Reader читает новые bytes. Required/source controls отказывают. Исправить общий consumed-input contract; полная цепочка MSBuild/Binding проверяется отдельно. |
| RF09, HNSW Load | Два известных повреждения уровней вновь принимаются и сохраняют 12/12 правильных nearest IDs. Нужны самостоятельный structural refusal и positive quality retention. |
| P83 |10 структурных checks текущего основного extractor проходят. Scoped target использует отдельную Body-only projection; это отдельный кандидат. Full product assembly/Roslyn4.12.0 и обычные Train/Predict/Compile здесь не исполнялись. |
| P89 |Фактический вариант:5 из 60 слотов, Signal13/12, freshDPOstep0, stop3/3; качество не изменилось. Экономии updates не измерено. Ordinary monitor ещё читает weighted Loss; для переноса нужна поэлементная unscaled сумма, в том числе при разных Signalвbatch. |
| Hidden fit |Source path Compile→GrammarMaskFilter.Train подтверждён. Compile нельзя считать чистым чтением без учёта вложенных Fit; исторические 64 вызова из локального исследования не становятся новым измерением этого аудита. |

Свежие маленькие probes имеют отдельный честный расход: component hosts —4builds/4processes,6Load/72Search/32Extract; RF02 —2compiles/8children, из них 4failed из-за первой версии audit deps и 4 успешных. Учебных Fit/Train/Add, optimizer updates и scientific inference —0. Один независимый reviewer проверил источники, наблюдения и итоговую матрицу; reviews не переисполняли пробы.

## Граница четырёх опубликованных локальных исследований

В planning добавлены:

- `2026-10-30-LATE-TRANSFER-AND-ADMISSION-AUDIT.md`;
- `2026-10-30-P42-P52-TRANSFER-EVIDENCE.md`;
- `2026-11-01-POST-P55-TRANSFER-STUDY.md`;
- `2026-11-01-POST-P65-TRANSFER-STUDY.md`.

Они прямо описывают локальные HEAD/рабочие деревья. В их Markdown23 вхождения ссылок на core,22 уникальных exact target paths; **16 целей отсутствуют** в полном проверенном docs subtree remote core. [Машинная матрица](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-01-post89-execution-boundary-audit/cross-repository-references.json) сохраняет все пути, source blob и статус. Это проверка конкретной ветки, а не утверждение, что файлов нет у автора.

Четыре отсутствующие карточки:

| Локальное назначение | Exact filename | Действие по аудиту |
|---|---|---|
| Q127 | KNN-ORDER-PERMUTATION-01.md | Сохранять как непроверенную remote публикацию; получить actual source перед добавлением в canonical queue. |
| Q128 | PHRASE-COVERAGE-CAP-01.md | Не переносить анализ прежнего copied exhaustive composer на нынешний bounded planner без сверки владельца/контракта. |
| Q129 | HELDOUT-ADMISSION-ATOMICITY-01.md | Существенная граница включена в RF01/P53; краткое описание не заменяет полный паспорт и не доказывает исполнение. |
| Q130 | METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md | Источник подтверждает отдельную target projection; основной P83 не объявляется незавершённым из-за не назначенного ему extension. |

Остальные отсутствующие цели — документы 104–114 иreviews с конкретными иными именами. Совпадение номера с опубликованным документом другого названия не разрешает подменять ссылку. Published102,03,74,110,111 ииндекс существуют; полезные наблюдения этих файлов сопоставлены с нынешними owners.

Локальное сообщение о missing type/checkpoint fields в HEAD2004e81030c не переносится наc4d05a2: текущий source уже содержит TranslationExactSourceKey, связанные checkpoint-поля и предварительную проверку версии. Это source-проверка, не новый successful build. P54planner уже внесён; прежнее «ещё greedy» остаётся историей. Во всех четырёх отчётах добавлены верхние границы публикации, прежний текст сохранён.

Позднее дополнение planning1384e815 добавило ещё две локальные ссылки: `116-post88-deep-transfer-audit-and-repair-plan-2026-11-01.md` и `reviews/2026-11-01-post88-rf02-rf09-deep-review.md`. Они отсутствуют в core630b2e7 и учитываются отдельно от23/22/16 четырёх отчётов. Номер116 совпадает, filename и содержание разные; этот текст не является публикацией тех локальных файлов. Их translation/checker находки требуют собственной сверки.

## План и защита следующих карточек

Документный учёт исполнения исправлен в этой поставке. Следующий минимальный продуктовый ремонт — **RF02 consumed inputs и допуск запуска**, затем effective измерители/счётчики, RF09 структурнаязагрузка, RF01 полнаяатомарность и P89 немасштабированныйпоказатель. Приёмка P83/RF10 должна дойти до настоящего caller с явным учётом вложенного обучения. Остальные RF03–RF11 сохраняют конкретные незакрытые обязанности 114; архитектурное решение RF05 не приравнивается к внедрённой learned routing.

Адресно изменены 20 карточек: **P14,P18,P20,P21,P25,P37,P53,P83,P84,P85,P87,P89,P90,P91,P100,P101,P104,P105,P108,P109**. Каждая получила checkID, конкретный ложноположительный сценарий, обязанность owner, наблюдаемую приёмку и границы расхода. Общий договор 39 иcontinuation prompt усилены для exact execution status, actual input bytes, effective setup, независимого оракула и hidden fit. Исходные карточки восстанавливаются побайтно после удаления только нового блока.

Число 20 не суммируется с прежними 53 или 89 из-за пересечений. Удалённые научные результаты не «исправлялись» повторным запуском; код продукта по этому плану ещё предстоит ремонтировать в названных границах.
