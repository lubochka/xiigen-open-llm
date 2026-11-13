# Покрытие карточек после P125 и проверка редакционного ремонта

Дата: 3 ноября 2026. Source pins: core `9d4d79d27f18066ce3a346fac2a5eea156304f62`, planning `bd37b3a653eed19ba07c3c5b7690dd8153c8ed82`. Основание — [аудит142](codex-142-post125-transfer-and-experiment-audit-2026-11-03.md), исполнение — [план143](codex-143-post125-addressed-repair-plan-2026-11-03.md), общий договор — [дополнение codex45 о доказательствах](../experimets/codex-45-post125-evidence-consumer-contract-2026-11-03.md). Продукт, raw, gold, учебные факторы и бюджет этим этапом не менялись.

## Объём

**22 экспериментальные карточки core +14 связанных planning-документов =36 адресных карточных поправок.** Дополнительно уточнены три общих переносных документа (02/03/11), реестр, README и экспериментальная навигация. Это число изменённых карточек, а не число заново принятых опытов или продуктовых исправлений.

В17 core-файлах заменён только прежний current-task блок. В5 будущих карточках и14 planning-документах добавлен новый верхний блок; историческое содержание сохранено. Независимый review проверил это сравнением до/после. Во всех22 core-карточках один anchor `current-executor-task`. Передаваемый prompt находится там же и содержит конкретный check, хороший/плохой случай, предел вывода и запрет незаявленного повторного PRIMARY.

## Core: где действует конкретная проверка

| Владелец и текущий вход | Проверяемый остаток | Вид покрытия |
|---|---|---|
| [P25-LEARNED-RESTORE-02 / Q125](../experimets/cards/P25-cold-restore.md#current-executor-task) | RESTORE-ACTUAL-CHECKPOINT | Адресная сверка исполненного опыта |
| [P56](../experimets/cards/P56-verifier-context.md#current-executor-task) | ONE-CHECKER-HONEST-TO-CORRUPT | Профилактика найденного класса; без новой аттестации |
| [P91](../experimets/cards/P91-method-pair-split.md#current-executor-task) | PAIR-TUPLE-AND-ORIGIN | Профилактика найденного класса; без новой аттестации |
| [P105](../experimets/cards/P105-frozen-reference.md#current-executor-task) | CONTENT-IDENTITY-IS-NOT-OBJECT-IDENTITY | Профилактика найденного класса; без новой аттестации |
| [P111](../experimets/cards/P111-reward-credit.md#current-executor-task) | ACTUAL-RECIPIENT-AT-MUTATION | Профилактика найденного класса; без новой аттестации |
| [P112](../experimets/cards/P112-family-disjoint-evaluation.md#current-executor-task) | FRAMED-BYTES-AND-CONSUMED-MATERIAL | Профилактика найденного класса; без новой аттестации |
| [P114](../experimets/cards/P114-zero-weight-pair.md#current-executor-task) | FAILED-ATTEMPT-DOES-NOT-ERASE-COST | Профилактика найденного класса; без новой аттестации |
| [P115](../experimets/cards/P115-bm25-statistics-cache.md#current-executor-task) | FINALIZER-AND-PHYSICAL-COST | Адресный остаток после предыдущего аудита |
| [P116](../experimets/cards/P116-e43-file-boundary.md#current-executor-task) | ACTUAL-STREAM-PROVENANCE | Адресный остаток после предыдущего аудита |
| [P117](../experimets/cards/P117-translation-direction-memory.md#current-executor-task) | LIVE-KNN-EXPORT-IS-NOT-EXPECTED-JOURNAL | Адресный остаток после предыдущего аудита |
| [P118](../experimets/cards/P118-projectwiki-content.md#current-executor-task) | CONTROL-TO-FINAL-VERDICT | Адресная сверка исполненного опыта |
| [P119](../experimets/cards/P119-learning-dynamics.md#current-executor-task) | LR-FACTOR-CAMPAIGN-ACCOUNTING | Адресная сверка исполненного опыта |
| [P120](../experimets/cards/P120-semantic-compression-laws.md#current-executor-task) | NESTED-PREDICT-AND-COMMON-METRIC | Адресная сверка исполненного опыта |
| [P121](../experimets/cards/P121-project-adapter-selection.md#current-executor-task) | SAME-CHECKPOINT-DIFFERENT-RECIPIENT | Адресная сверка исполненного опыта |
| [P122](../experimets/cards/P122-root-channel-unicode-digits-operators.md#current-executor-task) | RECOGNIZED-ROOTS-REACH-SLOT-32 | Адресная сверка исполненного опыта |
| [P123](../experimets/cards/P123-design-conversion-critic-evidence.md#current-executor-task) | RAW-GEOMETRY-TO-WORKING-ORACLE | Адресная сверка исполненного опыта |
| [P124](../experimets/cards/P124-p3-fixed-discriminator-contract.md#current-executor-task) | ACCEPTED-TO-REFUSED-AND-TRAINED-BASELINE | Адресная сверка исполненного опыта |
| [SAVED-STATE-PANEL-LEARNED-02 / Q126](../experimets/cards/SAVED-STATE-PANEL-0407-01.md#current-executor-task) | SAVED-START-CONTINUATION-COST | Профилактика найденного класса; без новой аттестации |
| [KNN-ORDER-PERMUTATION-01 / Q127](../experimets/cards/KNN-ORDER-PERMUTATION-01.md#current-executor-task) | ORDER-LEARNER-CONSUMER-AND-FINALIZER | Профилактика найденного класса; без новой аттестации |
| [PHRASE-COVERAGE-CAP-01 / Q128](../experimets/cards/PHRASE-COVERAGE-CAP-01.md#current-executor-task) | ACTUAL-PHRASE-CAP-AND-FULL-CHECKPOINT | Профилактика найденного класса; без новой аттестации |
| [HELDOUT-ADMISSION-ATOMICITY-01 / Q129](../experimets/cards/HELDOUT-ADMISSION-ATOMICITY-01.md#current-executor-task) | FAILURE-BEFORE-PUBLICATION | Профилактика найденного класса; без новой аттестации |
| [METHOD-KNN-SCOPED-EXPRESSION-TARGET-01 / Q130](../experimets/cards/METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md#current-executor-task) | SAME-TARGET-CONTENT-REAL-KNN | Профилактика найденного класса; без новой аттестации |

На основном9d4d79d Q126–Q130 были DESIGNED_NOT_RUN. При публикации `3eed42a80a4f` Q126 уже записан: его текущий вход переведён на чтение сохранённыхattempt-0002/0003 без повтора. Q127–Q130 остаются `DESIGNED_NOT_RUN`; их показатели неизвестны. На5a885d6c параллельно добавленаP131, её паспорт сохранён; она не входит в22 наши карточные правки. Итоговая очередь126/131, NEXT127. Новые контроли не являются разрешением добавить Train/Predict к бюджету. Для Q127 физический реестр сохраняет также12 холодных наблюдений; Q129 различает назначенные и фактические уроки; Q130 допускает честное `NOT_EXPOSED` штатного neighbor trace.

## Planning: предметные зеркала

| Проектный документ | Конкретное уточнение |
|---|---|
| [03-DPO-PAIR-AND-GRADIENT.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/03-DPO-PAIR-AND-GRADIENT.md) | SAME-CHECKPOINT-DIFFERENT-RECIPIENT |
| [07-CONTINUAL-REPLAY.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/07-CONTINUAL-REPLAY.md) | ACTUAL-STREAM-PROVENANCE |
| [10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md) | RESTORE-ACTUAL-CHECKPOINT |
| [22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md) | LIVE-KNN-EXPORT-IS-NOT-EXPECTED-JOURNAL |
| [27-MICRO-PROTOCOL-SEQUENCE-AND-NEGATION-LEARNING-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/27-MICRO-PROTOCOL-SEQUENCE-AND-NEGATION-LEARNING-2026-10-16.md) | NESTED-PREDICT-AND-COMMON-METRIC |
| [27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) | NESTED-PREDICT-AND-COMMON-METRIC |
| [34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md) | ACTUAL-STREAM-PROVENANCE |
| [34-MICRO-E43-REPLAY-CAUSAL-PROTOCOL-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/34-MICRO-E43-REPLAY-CAUSAL-PROTOCOL-2026-10-16.md) | PAIR-TUPLE-AND-ORIGIN |
| [35-MICRO-NEGATION-WORDMAP-CONFIDENCE-FACTORIAL-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/35-MICRO-NEGATION-WORDMAP-CONFIDENCE-FACTORIAL-2026-10-16.md) | RESTORE-ACTUAL-CHECKPOINT |
| [35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md) | RESTORE-ACTUAL-CHECKPOINT |
| [39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md) | FRAMED-BYTES-AND-CONSUMED-MATERIAL |
| [54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md) | RAW-GEOMETRY-TO-WORKING-ORACLE |
| [59-02-EXPERIMENT-LEARNING-PATH-COVERAGE-AND-INTEGRITY-2026-10-19.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/59-02-EXPERIMENT-LEARNING-PATH-COVERAGE-AND-INTEGRITY-2026-10-19.md) | ACCEPTED-TO-REFUSED-AND-TRAINED-BASELINE |
| [59-CAUSAL-LEARNING-PASSPORTS-AND-EXECUTOR-PROMPTS-2026-10-19.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/59-CAUSAL-LEARNING-PASSPORTS-AND-EXECUTOR-PROMPTS-2026-10-19.md) | LR-FACTOR-CAMPAIGN-ACCOUNTING |

Planning/10 дополнительно обновляет нынешний диагноз candidate store: механизм уже есть; остатки — BOM подтверждения, ordinary wiring, CAS provenance и поздняя отмена Evaluate. Это предотвращает повтор реализации по старому историческому тексту.

## Переносные документы и навигация

| Файл | Изменение |
|---|---|
| [02-learning-state-and-fixtures.md](02-learning-state-and-fixtures.md) | Actual snapshot, независимый полный comparer, кратность/provenance и UNKNOWN при отсутствующем состоянии |
| [03-test-and-execution-contract.md](03-test-and-execution-contract.md) | Полный честный positive, одна порча, semantic checks→status→exit, saved-only ремонт |
| [11-translation-verifier-admission.md](11-translation-verifier-admission.md) | Q01 candidate bytes/observedCAS/late Evaluate cancel; ограничение FIX-N60 сохранено |
| [01-registry.md](01-registry.md#executor-next) | Существующие Q01, T121 и T120 уточнены по нынешнему коду; одна очередь и переходы сохранены |
| [README.md](README.md) | Новый аудит и историческая область прежних114 строк |
| [Индекс опытов](../experimets/cards/00-execution-index.md#experiment-next), [README опытов](../experimets/00-README.md) | На9d4d79d125/130; publicationdelta3eed42a:126/130; после отдельнойP131 на5a885d6c126/131, Q127 следующий незаписанный без запуска; aliasQ125 указан |
| Planning README | Ссылка на новый отчёт, прежние P117 числа/NEXT явно исторические |

## Замечания независимой проверки и исправления

| Замечание | Внесённое исправление |
|---|---|
| P121 перечислял объекты без точных исходов | В текущем prompt указаны lawful A/A и ownerB/B, отказ A/B и второй capability/B→A, неизменность weights/optimizer/RNG и настоящий issuer |
| P123 называл RawTarget и допускал только битый hash | Используются RawButton/RawOverlay, назначенный canonical/actual bridge, согласованный digest и отказ именно raw→feature/gold |
| P122 называл1/10→10/10 сохранностью | Различение отделено от полной сохранности0/10→9/10 |
| P118 не содержал смысловой контрпример | Добавлены запрет обеих операций и обратный порядок, тот же checker, отдельные payload/semantic знаменатели |
| P25 restore терял multiplicity/scope | Полный состав с кратностью/provenance/order; состоявшийся P12501 отделён от разрешения повторять P2502 |
| Будущие карточки звучали как исполненные | Действия названы будущей приёмкой; actual неизвестен, исходные budgets сохранены |
| Обновлённый top planning/10 не исправлял старый диагноз | Добавлен нынешний byte/store/Evaluate остаток без изменения raw и FIX-N60 |

Конечное доказательство — [validation.json](../../../../experiments-v2/_audits/2026-11-03-post125-audit/validation.json), [разбор закрытия review](../../../../experiments-v2/_audits/2026-11-03-post125-audit/review-resolution.md) и manifest фактически опубликованных путей. Содержательная проверка142 не обнаружила блокирующих ошибок; первоначально отсутствовавшие ссылки должны быть проверены после сборки всего пакета. Native C# остаётся UNRUN; три Python-прохода не подменяют его.
