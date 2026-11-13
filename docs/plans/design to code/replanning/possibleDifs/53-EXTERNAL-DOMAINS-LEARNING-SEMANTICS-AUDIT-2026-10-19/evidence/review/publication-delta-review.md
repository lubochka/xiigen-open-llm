# Узкая независимая сверка двух последних публикационных дельт

**Вердикт: ACCEPTED_LATE_PUBLICATION_DELTA.** После проверки двух параллельных пакетов число наших новых конкретных source hypotheses остаётся **6**: KU-01, KU-02, PT-01, PT-02, AUT-WEIGHT-01 и AUT-REPLAY-02. Переклассификация новизны и изменение основных гипотез не требуются. Приняты точные подготовленные байты главы 07, примечаний README/06, индекса и delta ledger; хэши приведены ниже.

## Эпохи и охват

Основной reviewed этап: `02532653562a5f1e27fd75b1b03d4ae23e930e2f`. Первая поздняя дельта: `4cb6a92662b9cb1667c158800ea36d65724bedc5`, вторая и финальный подготовленный parent: `b0308fbaa783758104987e6544d057062ccf6fc3`.

| Дельта | Прочитанные/поисковые версии Markdown | Новые уникальные пути | Замена README | Корпус после дельты |
|---|---:|---:|---:|---:|
| Sparse KNN, 4cb | 8 | 7 | 1 | 678 |
| C# lexical state, b030 | 10 | 9 | 1 | 687 |

Итого проверены **18 версий Markdown**, содержащие **16 новых путей и две последовательные версии того же README**. Финальный корпус — **687 уникальных путей = 415 planning + 272 protected core**. Уникальность путей пересчитана локально по пяти сохранённым слоям корпуса. Это дополнительная адресная проверка новизны; полное повторное чтение всех 687 документов или первичное воспроизведение исходного C# кода соседнего пакета не заявляются.

Пакеты: [Sparse KNN](https://github.com/lubochka/xiigen-planning/blob/4cb6a92662b9cb1667c158800ea36d65724bedc5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/README.md) и [C# lexical state](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-external-domains-lexical-state-audit/README.md). Наше полное имя `53-EXTERNAL-DOMAINS-LEARNING-SEMANTICS-AUDIT-2026-10-19` отличается от соседнего каталога 53; подготовленный индекс подписывает наш пакет **Learning Semantics**. Принятый receipt промежуточной эпохи 4cb сохранён в `publication-delta-review-4cb-accepted.md/json`.

## Смысловая дедупликация: Sparse KNN

| Наш finding | Ближайшая соседняя тема | Почему точная цепочка отличается |
|---|---|---|
| KU-01 | NEW-POSITION / generic many-to-many guidance | Наш owner — MultiSubPhaseRouter aggregation по target с несовместимыми source IDs; peer меняет представимость используемой AST ordinal позиции. Общая many-to-many карта не описывает наш trigger. |
| KU-02 | NEW-SCORE | Peer сохраняет eligible IDs и rank и исследует signed denominator confidence. У нас положительные bounded scores и global relative floor до edge membership удаляют допустимого кандидата; другой owner и отказ. |
| PT-01 | NEW-SCORE / confidence family | У нас semantic duplicate после Continue с другим ID обнуляет top-two margin accepted memory. Peer не добавляет pattern, не меняет membership и не использует PromptToTask. |
| PT-02 | context/skill selection family | У нас substring alias вводит missing_gates из gate/delegated до nearest matching. Peer исследует числовой score contract, не fixed extraction этого сигнала. |
| AUT-WEIGHT-01 | NEW-AGE | У нас single-Domain admission и weighter превращают provider weight>1 в1 до learner. Peer исследует возраст первого update параметра в optimizer; одинаковый поданный вес не устраняет и не создаёт это sparse-age условие. |
| AUT-REPLAY-02 | E43 recovery / replay budget | Peer E43 — уже известный count-model replay и его бюджеты. У нас cohort добавляется после technical training success до последующего candidate quality reject в AutonomousTrainer/Pipeline. |

Основания: [NEW-AGE, строки 19–78](https://github.com/lubochka/xiigen-planning/blob/4cb6a92662b9cb1667c158800ea36d65724bedc5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/01-NEW-HYPOTHESES.md#L19-L78), [NEW-SCORE, 81–149](https://github.com/lubochka/xiigen-planning/blob/4cb6a92662b9cb1667c158800ea36d65724bedc5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/01-NEW-HYPOTHESES.md#L81-L149), [NEW-POSITION, 151–216](https://github.com/lubochka/xiigen-planning/blob/4cb6a92662b9cb1667c158800ea36d65724bedc5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/01-NEW-HYPOTHESES.md#L151-L216) и [dedup, 47–100](https://github.com/lubochka/xiigen-planning/blob/4cb6a92662b9cb1667c158800ea36d65724bedc5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/03-MECHANISM-COVERAGE-AND-DEDUP.md#L47-L100). Exact owners/формулы наших шести механизмов не найдены при полнотекстовом поиске по восьми версиям дельты; адресно сопоставлены описанные owners, triggers, eligibility, scores и admission.

Исторические E09/E25/E26/E43 и state→consumer пересекаются как уже известное recovery-first направление. Это KNOWN пересечение, которое не увеличивает нашу новизну. Соседние E40V2, sparse-age, signed-score и used-position уточнения не присваиваются нашему пакету и не сертифицируются этим review заново.

## Смысловая дедупликация: C# lexical state

| Соседняя находка | Заявленный механизм | Граница с нашими шестью |
|---|---|---|
| [KNN-RX1, 27–62](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-external-domains-lexical-state-audit/02-NEW-CSHARP-INPUT-AND-CALL-LOSS.md#L27-L62) | E10 удаляет //-комментарий раньше строковых литералов и обрезает исходник после // внутри URL. | PT-02 создаёт missing_gates из substring alias над PromptToTask текстом; здесь comment regex удаляет C# данные до Roslyn. Другие producer, representation и observable failure. |
| [KNN-RX2, 64–91](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-external-domains-lexical-state-audit/02-NEW-CSHARP-INPUT-AND-CALL-LOSS.md#L64-L91) | Замена строк стирает interpolation expression при Compile; KNN Train получает raw source. | Наши KU/PT не используют E10 string normalization, raw/Compile mismatch или C# interpolation. Общее слово features не задаёт тот же owner и trigger. |
| [KNN-OBS1, 93–118](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-external-domains-lexical-state-audit/02-NEW-CSHARP-INPUT-AND-CALL-LOSS.md#L93-L118) | ExtractMethodCalls обходит MemberAccessExpressionSyntax и пропускает MemberBindingExpression для условного вызова cache?.Add. | Peer теряет C# наблюдение при producer extraction; KU-02 удаляет уже посчитанный положительный allowed candidate global floor до edge membership. Различны стадия, данные и исправление. |

Главная новая C# глава прочитана целиком, её overview и независимые review сопоставлены адресно; все десять Markdown подвергнуты поиску по шести семействам owners/формул. Все шесть поисковых семейств дали **0 совпадений**. Содержательное сравнение подтверждает различие producer/representation/trigger: lexical preprocessor и C# call extraction не описывают segment-ID equality, whole-registry score floor, PromptToTask accepted-memory/aliases или AutonomousLearning weight/replay admission. [Известный E13 expression-body collapse прямо исключён автором, строка 128](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-external-domains-lexical-state-audit/02-NEW-CSHARP-INPUT-AND-CALL-LOSS.md#L128); его не следует называть новым lexical finding.

Две небольшие публикационные формулировки исправлены интегратором и приняты: **DELTA-META-01** — 07 перечисляет именно границы комментариев внутри строк, стирание interpolation expressions и пропуск conditional calls; **DELTA-META-02** — 06 называет полный b030 SHA свежим parent. Они не добавляют новой гипотезы или измеренного выигрыша качества.

## Точные источники и проверка байтов

Для **18/18 версий** повторно вычислены SHA-256 и Git blob SHA-1 (`blob <byte_length>\0<bytes>`); оба совпали с parent delta ledger. Первая дельта — 8/8, вторая — 10/10. Полный набор source paths, epoch, SHA и byte lengths входит в `publication-delta-review.json`; исходные независимые receipts — `publication-delta-source-byte-checks.json` и `publication-last-delta-source-byte-checks.json`. Поисковые выражения и результаты зафиксированы в `publication-delta-search.json` и `publication-last-delta-search.json`. Это проверка точности Markdown источников, не исполнение соседних training experiments.

## Принятые байты финальной публикационной сборки

| Артефакт | SHA-256 | Байтов |
|---|---|---:|
| `research/deliverable/README.md` | `a496222f735a4b807b9a39a9fc190357177baf3c019ddcceb804275156fedab2` | 7506 |
| `research/deliverable/06-SOURCES-NOVELTY-AND-REVIEW.md` | `74a4bc915308895ce318d2f1fbe35157f57321d25d605c64403c3b8659f575a3` | 17840 |
| `research/deliverable/07-PUBLICATION-RECONCILIATION.md` | `bb9413c4f99eb915601da41cfe9e27f0b8d2de2e841936da9915020a0716c9d3` | 7151 |
| `research/publication/index-prefix.md` | `258a7c4a2cc68a96c60e50f48e41e02ab98324c0927aaabbfd83acbf7d060810` | 2441 |
| `research/publication/README.md` | `2054c268753b545a1333682ac390ff5974209cb33af36f2c6c9e6beb0ae80a39` | 166466 |
| `research/deliverable/evidence/novelty/publication-delta-ledger.json` | `e673802cdb11f1be1ce28106c95bafc2feef16e7afa00bb5b0676316f0e15d7e` | 8160 |

Подготовленный общий README равен точному concatenation: **наш префикс 2441 байт + свежий исходный README 164025 байтов = 166466 байтов**. SHA-256 свежего исходного README: `e38ae407be4eceea15ccf0f7b98a1c8fd42da92059c7dc4949c8d9e1e1e7678c`. Все его байты сохранены после нашего префикса. Свежий исходный README также содержит предыдущий 4cb README как неизменённый хвост; новый C# префикс **2612 байтов** и ранее добавленный Sparse KNN префикс **2968 байтов** сохранены целиком.

В README и 06 добавлены только объявленные примечания о поздней дельте: удаление этих примечаний даёт ранее принятые SHA-256. **Остальные восемь ранее проверенных content-файлов имеют прежние SHA-256**, включая 01/01B/02A/02B/03/04/05 и evidence README. 07 прочитана целиком; она сохраняет ограниченную новизну, различает полные имена каталогов и описывает свежий parent как подготовку публикации, без утверждения уже выполненного remote readback.

## Граница принятия

Это финальная проверка актуальных подготовленных файлов эпохи b030, не временный статус старого индекса. Фактические parent/tree/ref update и удалённый readback выполняет интегратор. Независимый reviewer не менял candidate и не выполнял native runs, обучение, внешние model calls или GitHub writes. Основной accepted content review остаётся действующим в ранее указанных границах source evidence и consumer reachability.
