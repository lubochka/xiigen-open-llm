# Узкая независимая сверка последней публикационной дельты

**Вердикт: ACCEPTED_LATE_PUBLICATION_DELTA.** После адресной сверки свежего соседнего пакета число наших новых concrete source hypotheses остаётся **6**. Переклассификация новизны или изменение основных гипотез не требуется. Глава 07, две короткие примечания README/06 и обновлённый индекс приняты в указанных ниже байтах.

## Эпоха и охват

Исходный reviewed этап: `02532653562a5f1e27fd75b1b03d4ae23e930e2f`. Свежий parent: `4cb6a92662b9cb1667c158800ea36d65724bedc5`. Дельта содержит **8 Markdown: 7 новых путей и замену README**; итоговый корпус — **678 уникальных путей = 406 planning + 272 protected core**. Проверка ограничена этой дельтой и её смысловым пересечением с KU/PT/AUT. Полный новый аудит всех 678 документов и первичное воспроизведение чужих гипотез не заявляются.

Параллельный пакет: [53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19](https://github.com/lubochka/xiigen-planning/blob/4cb6a92662b9cb1667c158800ea36d65724bedc5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/README.md). Числовой префикс 53 не является уникальным ID пакета; наши полные каталожные имена различаются. Новый индекс явно подписывает наш пакет Learning Semantics.

## Смысловая дедупликация

| Наш finding | Ближайшая соседняя тема | Почему точная цепочка отличается |
|---|---|---|
| KU-01 | NEW-POSITION / generic many-to-many guidance | Наш owner — MultiSubPhaseRouter aggregation по target с несовместимыми source IDs; peer меняет представимость используемой AST ordinal позиции. Общая many-to-many карта не описывает наш trigger. |
| KU-02 | NEW-SCORE | Peer сохраняет eligible IDs и rank и исследует signed denominator confidence. У нас положительные bounded scores и global relative floor до edge membership удаляют допустимого кандидата; другой owner и отказ. |
| PT-01 | NEW-SCORE / confidence family | У нас semantic duplicate после Continue с другим ID обнуляет top-two margin accepted memory. Peer не добавляет pattern, не меняет membership и не использует PromptToTask. |
| PT-02 | context/skill selection family | У нас substring alias вводит missing_gates из gate/delegated до nearest matching. Peer исследует числовой score contract, не fixed extraction этого сигнала. |
| AUT-WEIGHT-01 | NEW-AGE | У нас single-Domain admission и weighter превращают provider weight>1 в1 до learner. Peer исследует возраст первого update параметра в optimizer; одинаковый поданный вес не устраняет и не создаёт это sparse-age условие. |
| AUT-REPLAY-02 | E43 recovery / replay budget | Peer E43 — уже известный count-model replay и его бюджеты. У нас cohort добавляется после technical training success до последующего candidate quality reject в AutonomousTrainer/Pipeline. |

Основания сопоставления: соседний [01: NEW-AGE, строки 19–78](https://github.com/lubochka/xiigen-planning/blob/4cb6a92662b9cb1667c158800ea36d65724bedc5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/01-NEW-HYPOTHESES.md#L19-L78), [NEW-SCORE, 81–149](https://github.com/lubochka/xiigen-planning/blob/4cb6a92662b9cb1667c158800ea36d65724bedc5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/01-NEW-HYPOTHESES.md#L81-L149), [NEW-POSITION, 151–216](https://github.com/lubochka/xiigen-planning/blob/4cb6a92662b9cb1667c158800ea36d65724bedc5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/01-NEW-HYPOTHESES.md#L151-L216); [карта дедупликации, 47–100](https://github.com/lubochka/xiigen-planning/blob/4cb6a92662b9cb1667c158800ea36d65724bedc5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/03-MECHANISM-COVERAGE-AND-DEDUP.md#L47-L100). Полнотекстовый поиск по всем восьми Markdown не нашёл точных owners/формул KU/PT/AUT. Поиск не заменяет смысловое сравнение: указанные соседние механизмы и контролирующие их IDs/rank/admission прочитаны адресно.

Исторические E09/E25/E26/E43 и общая проверка state→consumer пересекаются по уже известному restoration-first направлению. Это ожидаемое KNOWN пересечение, которое не увеличивает нашу новизну. Соседние E40V2, sparse-age, signed-score и used-position уточнения остаются у авторского пакета; их числа этим review заново не сертифицированы.

## Точная дельта источников

| Файл в свежей дельте | SHA-256 |
|---|---|
| `53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md` | `b556f5ad0777cfce4c9d5d750bc0a8a81abda8993d3912230b9df5b298a75d09` |
| `53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/01-NEW-HYPOTHESES.md` | `7d8effe19f9b33196f3f17a5bd786af25c5ffea9de6446ca463597f12f410771` |
| `53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/02-SMALL-DOMAIN-EXPERIMENTS.md` | `331f5daf797304b8d274b5d6add4685200a51f084a2598ec34dfbd20f7ba3eda` |
| `53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/03-MECHANISM-COVERAGE-AND-DEDUP.md` | `63f3a55b6843e80553358eff41387867b83f822f00d1a0574aecb708e93e21a5` |
| `53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/04-SOURCES-AND-REVIEW.md` | `b69e9331ed000a20073e7c425146f26756bbc9559a64a2f779b26a2d4a43a30d` |
| `53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/README.md` | `4e1e1b08d7da092ba5bf36496a34da7633d65ecc66a164fbc9d5377476fc168f` |
| `53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/evidence/prior-finding-index.md` | `daf15db92f672a5f78d29f73e01367c7560e4f09bb401ac082e181b75e032348` |
| `README.md` | `8ad58079e2e4922de328e87fe3679067ee0717b3b6460d63963624b3cf984bda` |

Все **8/8 локальных байтов** совпали и с записанным Git blob SHA, и с SHA-256. Receipt — `publication-delta-source-byte-checks.json`; точные поисковые выражения и эпоха — `publication-delta-search.json`.

## Принятые байты публикационной сборки

| Артефакт | SHA-256 | Байтов |
|---|---|---|
| `research/deliverable/README.md` | `139474ee1e75d2a234a5e2db33bdc58613b8184ba3b72334e9092005fee931ed` | 7506 |
| `research/deliverable/06-SOURCES-NOVELTY-AND-REVIEW.md` | `5d5d97784ac74a7d0019c3f231e05580926bd7692c229c7859c7f3afb657b278` | 17746 |
| `research/deliverable/07-PUBLICATION-RECONCILIATION.md` | `c8158ae13ed5393500cbdc1dc42d1da3e57866d10897a0b0be2a672520609db6` | 5756 |
| `research/publication/index-prefix.md` | `11d57ee52fcc3be1dc61fcc077d557e3f6207ff233923a96fcb2aa35da90c7c9` | 2441 |
| `research/publication/README.md` | `47a3fed0e2ce194a019d0f11930481602c911034089af1ea2df7e6adc399ecc7` | 163854 |

Подготовленный общий README равен точному concatenation: **наш префикс 2441 байт + свежий исходный README 161413 байтов = 163854 байта**. Все чужие байты сохранены после префикса. Старый README эпохи025 также является неизменённым хвостом свежего README; параллельная публикация добавила собственные 2968 байтов, и они сохранены целиком.

В README и 06 добавлены только объявленные примечания о поздней дельте. Обратное удаление этих примечаний даёт прежние принятые SHA-256. Остальные ранее проверенные content-файлы не изменились. 07 полностью прочитана: она сохраняет источник новизны, различает два каталога 53, не выдаёт параллельную сверку за native learning и не утверждает опубликованный ref до readback.

## Граница принятия

Это финальная проверка актуальных подготовленных файлов, не временный статус старого индекса. Actual parent/tree/ref update и удалённый readback выполняет интегратор. Никаких candidate edits, native runs, training, внешних моделей или GitHub writes этим review не выполнено.
